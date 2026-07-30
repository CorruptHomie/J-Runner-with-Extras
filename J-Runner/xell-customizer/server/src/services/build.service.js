import { createId } from "@paralleldrive/cuid2";
import { HTTPException } from "hono/http-exception";
import { Octokit } from "@octokit/rest";
import fs from "node:fs";
import zlib from "node:zlib";
import path from "node:path";
import { fileURLToPath } from "node:url";

// Ported from xell-customizer-api/src/services/build.service.ts. The actual XeLL image is
// still built by GitHub Actions, exactly as on the Cloudflare-hosted version - this
// dispatches that workflow with the chosen colours/ASCII art.
//
// Two things differ from the original:
//  1. The target repo is configurable. The upstream default (xell-worker/xell-builder)
//     only accepts dispatches from tokens that have write access to it, which a normal
//     personal access token does not - that's the "Resource not accessible by personal
//     access token" 403. Forking the builder and pointing GITHUB_OWNER/GITHUB_REPO at your
//     own fork is what makes this work with your own token.
//  2. After a successful dispatch it polls the run, downloads the resulting artifact and
//     unpacks it into J-Runner's XeLL folder, so a finished build lands where J-Runner
//     actually reads XeLL from instead of in your browser's downloads.

const OWNER = process.env.GITHUB_OWNER || "xell-worker";
const REPO = process.env.GITHUB_REPO || "xell-builder";
const WORKFLOW = process.env.GITHUB_WORKFLOW || "build.yml";
const REF = process.env.GITHUB_REF || "main";

export function xellOutputDir() {
  if (process.env.XELL_OUTPUT_DIR) return path.resolve(process.env.XELL_OUTPUT_DIR);
  // "Custom XeLL images" beside the J-Runner executable. server/src/services is four
  // levels down from the app root. fileURLToPath, not URL.pathname - on Windows the latter
  // yields "/C:/..." with a leading slash, which isn't a valid path.
  return path.resolve(fileURLToPath(new URL("../../../../Custom XeLL images", import.meta.url)));
}

// A bare 404 from the dispatch endpoint is ambiguous - it covers "repo not visible to this
// token", "Actions disabled", "workflow missing" and "branch missing" alike, and GitHub
// deliberately returns 404 rather than 403 for repos a fine-grained token isn't scoped to,
// so it doesn't leak whether they exist. These checks tell them apart and say which it is.
async function preflight(octokit) {
  try {
    await octokit.repos.get({ owner: OWNER, repo: REPO });
  } catch (e) {
    throw new HTTPException(404, {
      message:
        `Can't see ${OWNER}/${REPO} with this token.\n\n` +
        `A fine-grained personal access token only reaches the repositories picked under ` +
        `"Repository access" - granting permissions is a separate step from selecting which ` +
        `repos they apply to, and GitHub returns 404 (not 403) for anything outside that ` +
        `list. Check ${REPO} is in the token's selected repositories, and that GITHUB_OWNER ` +
        `and GITHUB_REPO match your fork.`,
    });
  }

  let workflow;
  try {
    const r = await octokit.actions.getWorkflow({ owner: OWNER, repo: REPO, workflow_id: WORKFLOW });
    workflow = r.data;
  } catch (e) {
    let available = [];
    try {
      const l = await octokit.actions.listRepoWorkflows({ owner: OWNER, repo: REPO });
      available = (l.data.workflows || []).map((w) => w.path.replace(/^.*\//, ""));
    } catch { /* listing is only for a better message */ }

    throw new HTTPException(404, {
      message:
        `${OWNER}/${REPO} is reachable, but workflow "${WORKFLOW}" isn't.\n\n` +
        (available.length
          ? `Workflows there: ${available.join(", ")}. Set GITHUB_WORKFLOW to one of those.`
          : `No workflows are visible at all, which is what a fork looks like before its ` +
            `Actions are enabled - GitHub disables them on new forks. Open the Actions tab ` +
            `on ${OWNER}/${REPO} and enable workflows, then try again.`),
    });
  }

  if (workflow.state !== "active") {
    throw new HTTPException(409, {
      message:
        `Workflow "${WORKFLOW}" exists but its state is "${workflow.state}".\n\n` +
        `Forks start with Actions disabled. Open the Actions tab on ${OWNER}/${REPO}, enable ` +
        `workflows, and enable this one specifically if it's listed as disabled.`,
    });
  }

  try {
    await octokit.repos.getBranch({ owner: OWNER, repo: REPO, branch: REF });
  } catch (e) {
    throw new HTTPException(404, {
      message: `Branch "${REF}" doesn't exist in ${OWNER}/${REPO}. Set GITHUB_REF to the branch the workflow lives on.`,
    });
  }
}

export async function generateBuild(input, ghToken) {
  const octokit = new Octokit({ auth: ghToken });

  const id = createId();
  const date = new Date().toISOString().slice(0, 10).replace(/-/g, "");

  await preflight(octokit);

  // background_color and foreground_color are declared `required: true` by the workflow, so
  // a dispatch omitting either is rejected outright (422). The frontend treats them as
  // optional, so fall back to the workflow's own declared defaults.
  const inputs = {
    id,
    date,
    background_color: "0xD8444E00",
    foreground_color: "0xFFFFFF00",
    ...Object.fromEntries(Object.entries(input).filter(([, v]) => v !== undefined && v !== "")),
  };

  try {
    await octokit.actions.createWorkflowDispatch({
      owner: OWNER,
      repo: REPO,
      ref: REF,
      workflow_id: WORKFLOW,
      inputs,
    });
  } catch (error) {
    const status = error?.status;
    let message = error?.message || "Failed to dispatch workflow";
    if (status === 403 || status === 404) {
      message =
        `${message}\n\nGitHub refused the dispatch for ${OWNER}/${REPO}. A personal access ` +
        `token can only dispatch workflows on a repo you have write access to. Fork the ` +
        `builder repo, then set GITHUB_OWNER and GITHUB_REPO in server/.env to your fork ` +
        `and use a token with the "workflow" scope.`;
    }
    throw new HTTPException(status === 403 ? 403 : 418, { message });
  }

  return { id, date };
}

// The build's second job commits its output into the repo at {year}/{mmdd}/{id}/ and pushes,
// which is what the frontend was polling for - except it polled the hardcoded upstream repo,
// so once the build moved to a fork nothing ever showed up and it timed out. Reported here
// instead, against whichever repo is configured.
function rawBase(date, id) {
  const year = date.slice(0, 4);
  const mmdd = date.slice(4, 8);
  return `https://raw.githubusercontent.com/${OWNER}/${REPO}/refs/heads/${REF}/${year}/${mmdd}/${id}`;
}

// The build workflow fetches its own job log with
//   curl -sL -H "Authorization: Bearer $TOKEN" .../actions/jobs/$ID/logs -o log.txt
// That endpoint 302s to Azure Blob Storage, and -L re-sends the Authorization header to the
// redirect target, which Azure rejects - so what gets committed as log.txt is an Azure
// <Error><Code>BlobNotFound</Code> document rather than the log. It's harmless (the build
// itself is fine, and log.txt existing is only used as the "published" signal) but opening
// it shows XML instead of a log, so detect it rather than hand it to the user.
function looksLikeStorageError(text) {
  const head = (text || "").slice(0, 500);
  return /<Error>/i.test(head) && /BlobNotFound|AuthenticationFailed|ResourceNotFound|InvalidQueryParameterValue/i.test(head);
}

export async function fetchLog(date, id) {
  const res = await fetch(`${rawBase(date, id)}/log.txt`, { cache: "no-store" });
  if (!res.ok) return "No log has been published for this build yet.";

  const text = await res.text();
  if (!looksLikeStorageError(text)) return text;

  return [
    "The build workflow didn't capture its log, so there's nothing to show here.",
    "",
    "What it committed instead is an Azure Blob Storage error. The workflow fetches its own",
    "log with:",
    "",
    '    curl -sL -H "Authorization: Bearer $TOKEN" .../actions/jobs/$JOB_ID/logs -o log.txt',
    "",
    "That endpoint redirects to Azure, and -L re-sends the Authorization header to the",
    "redirect target, which Azure rejects - so the error document lands in log.txt.",
    "",
    "The build itself is unaffected: this only concerns the log. To fix it in your fork,",
    "resolve the redirect first and fetch the blob without the auth header, e.g.",
    "",
    '    URL=$(curl -s -o /dev/null -w \'%{redirect_url}\' \\',
    '            -H "Authorization: Bearer $TOKEN" .../actions/jobs/$JOB_ID/logs)',
    '    curl -s "$URL" -o /tmp/log.txt',
    "",
    "----- what the workflow actually committed -----",
    "",
    text.trim(),
  ].join("\n");
}

// Locates the Actions run this build id was dispatched as, and its artifact, so the UI can
// link straight to them. The dispatch API doesn't hand back a run id, but the workflow's
// run-name embeds the id ("XeLL Build (<id>)"), so the run can be matched on that.
async function findRunLinks(octokit, id) {
  if (!octokit) return null;
  try {
    const { data } = await octokit.actions.listWorkflowRuns({
      owner: OWNER,
      repo: REPO,
      workflow_id: WORKFLOW,
      per_page: 30,
    });

    const run = (data.workflow_runs || []).find(
      (r) => `${r.name || ""} ${r.display_title || ""}`.includes(id),
    );
    if (!run) return null;

    const links = {
      runId: run.id,
      runUrl: run.html_url,
      runStatus: run.status,
      runConclusion: run.conclusion,
    };

    const arts = await octokit.actions.listWorkflowRunArtifacts({
      owner: OWNER,
      repo: REPO,
      run_id: run.id,
    });
    const artifact = (arts.data.artifacts || [])[0];
    if (artifact) {
      links.artifactName = artifact.name;
      links.artifactSize = artifact.size_in_bytes;
      links.artifactExpired = artifact.expired;
      // The browser-facing URL. GitHub requires you to be signed in for it, which is why the
      // API URL is offered alongside rather than instead.
      links.artifactUrl = `https://github.com/${OWNER}/${REPO}/actions/runs/${run.id}/artifacts/${artifact.id}`;
      links.artifactApiUrl = artifact.archive_download_url;
    }
    return links;
  } catch (e) {
    console.error(`Couldn't look up the workflow run: ${e?.message || e}`);
    return null;
  }
}

export async function buildStatus(date, id, ghToken) {
  const base = rawBase(date, id);

  // Resolved first and returned even while the build is still running, so the run can be
  // watched from the moment it starts rather than only once it has published something.
  const octokit = ghToken ? new Octokit({ auth: ghToken }) : null;
  // actionsUrl is unconditional: the error message tells people to open the workflow, so
  // there has to be somewhere to send them even when the specific run couldn't be resolved
  // (no token, or it hasn't appeared in the run list yet).
  const links = {
    actionsUrl: `https://github.com/${OWNER}/${REPO}/actions`,
    ...((await findRunLinks(octokit, id)) || {}),
  };

  const log = await fetch(`${base}/log.txt`, { cache: "no-store" });
  if (!log.ok) return { ready: false, ...links };

  // Served through this server rather than linking raw.githubusercontent directly, so an
  // unusable log gets explained instead of dumping Azure's XML in a browser tab.
  const logUrl = `/log?id=${encodeURIComponent(id)}&date=${encodeURIComponent(date)}`;

  const nameRes = await fetch(`${base}/original-filename.txt`, { cache: "no-store" });
  if (!nameRes.ok) return { ready: true, failed: true, logUrl, ...links };

  const filename = (await nameRes.text()).trim();
  const downloadUrl = `${base}/${id}.tar.gz`;

  let savedTo = null;
  try {
    savedTo = await saveBuild(downloadUrl, filename, id);
  } catch (e) {
    console.error(`Couldn't save the build locally: ${e?.message || e}`);
  }

  if (links.artifactUrl) {
    console.log(`Workflow run:      ${links.runUrl}`);
    console.log(`Artifact download: ${links.artifactUrl}`);
  }

  return { ready: true, failed: false, filename, downloadUrl, logUrl, savedTo, ...links };
}

async function saveBuild(downloadUrl, filename, id) {
  const outDir = path.join(xellOutputDir(), id);
  const archivePath = path.join(outDir, filename || `${id}.tar.gz`);
  if (fs.existsSync(archivePath)) return outDir;   // already fetched

  const res = await fetch(downloadUrl);
  if (!res.ok) throw new Error(`download failed with ${res.status}`);
  const buf = Buffer.from(await res.arrayBuffer());

  fs.mkdirSync(outDir, { recursive: true });
  fs.writeFileSync(archivePath, buf);

  // Unpacked as well as kept, so the XeLL binaries are usable straight away without
  // needing a tar tool on Windows. Done with the built-in zlib plus a minimal tar reader
  // rather than another dependency.
  let extracted = [];
  try {
    extracted = extractTarGz(buf, outDir);
  } catch (e) {
    console.error(`Archive saved but couldn't be unpacked: ${e?.message || e}`);
  }

  console.log(`Custom XeLL saved to ${outDir}${extracted.length ? ` (${extracted.join(", ")})` : ""}`);
  return outDir;
}

// Minimal POSIX tar reader - 512-byte headers, octal size at offset 124, type flag at 156.
// Names are flattened to their basename, which both suits the flat output folder and makes
// path traversal out of it impossible.
function extractTarGz(gz, destDir) {
  const tar = zlib.gunzipSync(gz);
  const written = [];
  let off = 0;

  while (off + 512 <= tar.length) {
    const header = tar.subarray(off, off + 512);
    const name = header.subarray(0, 100).toString("utf8").replace(/\0.*$/, "");
    if (!name) break;   // trailing zero blocks mark the end

    const size = parseInt(header.subarray(124, 136).toString("utf8").replace(/\0.*$/, "").trim(), 8) || 0;
    const type = String.fromCharCode(header[156]);
    off += 512;

    if (type === "0" || type === "\0") {
      const base = path.basename(name);
      if (base && base !== "." && base !== "..") {
        fs.writeFileSync(path.join(destDir, base), tar.subarray(off, off + size));
        written.push(base);
      }
    }
    off += Math.ceil(size / 512) * 512;
  }
  return written;
}

function sleep(ms) {
  return new Promise((r) => setTimeout(r, ms));
}

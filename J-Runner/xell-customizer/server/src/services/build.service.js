import { createId } from "@paralleldrive/cuid2";
import { HTTPException } from "hono/http-exception";
import { Octokit } from "@octokit/rest";
import AdmZip from "adm-zip";
import fs from "node:fs";
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
  // Defaults to <app>/common/xell relative to this file (server/src/services ->
  // ../../../../common/xell), matching where MainForm/xebuild read XeLL templates from.
  if (process.env.XELL_OUTPUT_DIR) return path.resolve(process.env.XELL_OUTPUT_DIR);
  // fileURLToPath, not URL.pathname - on Windows the latter yields "/C:/..." with a
  // leading slash, which is not a valid path.
  return path.resolve(fileURLToPath(new URL("../../../../common/xell", import.meta.url)));
}

export async function generateBuild(input, ghToken) {
  const octokit = new Octokit({ auth: ghToken });

  const id = createId();
  const date = new Date().toISOString().slice(0, 10).replace(/-/g, "");

  try {
    await octokit.actions.createWorkflowDispatch({
      owner: OWNER,
      repo: REPO,
      ref: REF,
      workflow_id: WORKFLOW,
      inputs: { id, date, ...input },
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

  // Fire-and-forget: the frontend keeps its own progress UI, and this logs to stdout,
  // which J-Runner pipes into its console window.
  collectArtifact(octokit, id).catch((e) =>
    console.error(`Artifact collection failed: ${e?.message || e}`),
  );

  return { id, date };
}

async function collectArtifact(octokit, id) {
  const runId = await waitForRun(octokit, id);
  if (!runId) {
    console.log("Could not identify the workflow run; skipping automatic download.");
    return;
  }

  const conclusion = await waitForCompletion(octokit, runId);
  if (conclusion !== "success") {
    console.log(`Build finished with conclusion "${conclusion}"; nothing to download.`);
    return;
  }

  const { data } = await octokit.actions.listWorkflowRunArtifacts({
    owner: OWNER,
    repo: REPO,
    run_id: runId,
  });
  const artifact = data.artifacts?.[0];
  if (!artifact) {
    console.log("Build succeeded but produced no artifact.");
    return;
  }

  const zip = await octokit.actions.downloadArtifact({
    owner: OWNER,
    repo: REPO,
    artifact_id: artifact.id,
    archive_format: "zip",
  });

  const outDir = xellOutputDir();
  fs.mkdirSync(outDir, { recursive: true });
  new AdmZip(Buffer.from(zip.data)).extractAllTo(outDir, true);
  console.log(`Custom XeLL written to ${outDir}`);
}

async function waitForRun(octokit, id, attempts = 20) {
  // The dispatch API doesn't return a run id, so match on the run whose name/head matches
  // and that started after the dispatch. Poll briefly - the run takes a moment to appear.
  for (let i = 0; i < attempts; i++) {
    await sleep(3000);
    const { data } = await octokit.actions.listWorkflowRuns({
      owner: OWNER,
      repo: REPO,
      workflow_id: WORKFLOW,
      per_page: 20,
    });
    const match = data.workflow_runs?.find((r) => JSON.stringify(r).includes(id));
    if (match) return match.id;
    if (i === 0 && data.workflow_runs?.length) {
      // Fall back to the newest run if the id isn't echoed anywhere in the run payload.
      const newest = data.workflow_runs[0];
      if (Date.now() - new Date(newest.created_at).getTime() < 120000) return newest.id;
    }
  }
  return null;
}

async function waitForCompletion(octokit, runId, attempts = 200) {
  for (let i = 0; i < attempts; i++) {
    const { data } = await octokit.actions.getWorkflowRun({
      owner: OWNER,
      repo: REPO,
      run_id: runId,
    });
    if (data.status === "completed") return data.conclusion;
    if (i % 10 === 0) console.log(`Build ${runId} is ${data.status}...`);
    await sleep(5000);
  }
  return "timed_out";
}

function sleep(ms) {
  return new Promise((r) => setTimeout(r, ms));
}

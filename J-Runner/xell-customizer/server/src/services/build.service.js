import { createId } from "@paralleldrive/cuid2";
import { HTTPException } from "hono/http-exception";
import { Octokit } from "@octokit/rest";

// Ported from xell-customizer-api/src/services/build.service.ts. The actual XeLL image is
// still built by GitHub Actions in xell-worker/xell-builder, exactly as it was on the
// Cloudflare-hosted version - this function's only job is to kick off that workflow with
// the chosen colors/ASCII art and hand back the id/date the frontend needs to find the
// result. Nothing about *how* XeLL gets compiled changes; only where this dispatcher runs
// (a local Node process launched by J-Runner, instead of a Cloudflare Worker) changes.
export async function generateBuild(input, ghToken) {
  const octokit = new Octokit({ auth: ghToken });

  const id = createId();
  const date = new Date().toISOString().slice(0, 10).replace(/-/g, "");

  try {
    await octokit.actions.createWorkflowDispatch({
      owner: "xell-worker",
      repo: "xell-builder",
      ref: "main",
      workflow_id: "build.yml",
      inputs: {
        id,
        date,
        ...input,
      },
    });
  } catch (error) {
    throw new HTTPException(418, {
      message: error?.message || "Failed to dispatch workflow",
    });
  }

  return { id, date };
}

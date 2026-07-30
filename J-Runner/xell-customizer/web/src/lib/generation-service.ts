// Was: "https://xell-customizer-api.barrenechea.workers.dev" (the Cloudflare Workers
// deployment). Ported to run locally under J-Runner: the API now serves this same
// frontend from the same origin (see xell-customizer/server), so a relative path is both
// correct and simpler - no origin to keep in sync with whatever port it's launched on.
const API_BASE_URL = "";

export interface GenerationParams {
  /** LibXenon-formatted BGR color */
  background_color: string;
  /** LibXenon-formatted BGR color */
  foreground_color: string;
  ascii_art?: string;
}

interface GenerationResponse {
  id: string;
  /** Date in YYYYMMDD format */
  date: string;
}

export interface BuildStatus {
  ready: boolean;
  failed?: boolean;
  filename?: string;
  downloadUrl?: string;
  logUrl?: string;
  /** Local folder the finished build was saved to, when the server managed to fetch it. */
  savedTo?: string | null;
  /** The Actions run this build was dispatched as. */
  runUrl?: string;
  runId?: number;
  runStatus?: string;
  runConclusion?: string | null;
  /** Direct link to the run's artifact (requires being signed in to GitHub). */
  artifactUrl?: string;
  artifactApiUrl?: string;
  artifactName?: string;
}

/**
 * Starts the generation process by calling the API
 */
export const startGeneration = async (
  params: GenerationParams,
): Promise<GenerationResponse> => {
  const filteredParams = Object.fromEntries(
    Object.entries(params).filter(([, value]) => value),
  );

  const response = await fetch(`${API_BASE_URL}/generate`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(filteredParams),
  });

  if (!response.ok) {
    const error = (await response.json()) as { error: string };
    throw new Error(error.error ?? "Failed to start generation");
  }

  return response.json() as Promise<GenerationResponse>;
};

/**
 * Polls the local server for the build status.
 *
 * This used to fetch raw.githubusercontent.com for xell-worker/xell-builder directly, with
 * the repo hardcoded. That breaks as soon as the build runs anywhere else - a dispatch to
 * your own fork commits the result to *your* repo, so polling upstream finds nothing and the
 * generation just times out even though the build succeeded. The server knows which repo is
 * configured, so it answers this instead - and saves the finished build locally while it's
 * at it.
 */
export const checkBuildStatus = async (
  id: string,
  date: string,
): Promise<BuildStatus> => {
  const response = await fetch(
    `${API_BASE_URL}/status?id=${encodeURIComponent(id)}&date=${encodeURIComponent(date)}`,
    { cache: "no-store" },
  );

  if (!response.ok) {
    return { ready: false };
  }

  // A server left running from an older build serves this frontend straight off disk but
  // doesn't have /status, so the request falls through to the SPA fallback and returns
  // index.html. Parsing that as JSON is where "Unexpected token '<'" came from - say what's
  // actually wrong instead.
  const body = await response.text();
  try {
    return JSON.parse(body) as BuildStatus;
  } catch {
    throw new Error(
      "The local XeLL Customizer server is out of date - it returned a page instead of JSON. " +
        "Close J-Runner completely (so the old server exits) and reopen it.",
    );
  }
};

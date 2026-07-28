import "dotenv/config";
import { serve } from "@hono/node-server";
import { serveStatic } from "@hono/node-server/serve-static";
import { Hono } from "hono";
import { cors } from "hono/cors";
import { zValidator } from "@hono/zod-validator";
import { HTTPException } from "hono/http-exception";
import { fileURLToPath } from "node:url";
import path from "node:path";

import { generateSchema } from "./schemas/index.js";
import { generateBuild } from "./services/build.service.js";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const PORT = Number(process.env.PORT) || 2222;
const WEB_ROOT = path.resolve(__dirname, "..", "..", "web", "dist");

const app = new Hono();

// The Cloudflare-hosted version locked CORS to https://xell.barrenechea.cl. Here the
// frontend is served from this same process (see serveStatic below), so requests to
// /generate are same-origin and CORS barely matters - left open rather than pointed at a
// specific origin since this only ever listens on localhost, launched and torn down by
// J-Runner itself, never exposed to the network.
app.use(
  "*",
  cors({
    origin: "*",
    allowMethods: ["GET", "POST"],
  }),
);

app.get("/health", (c) => c.json({ ok: true }));

app.post(
  "/generate",
  zValidator("json", generateSchema),
  async function handleGenerate(c) {
    try {
      const body = c.req.valid("json");
      const token = process.env.GITHUB_TOKEN;
      if (!token) {
        return c.json(
          { error: "GITHUB_TOKEN is not set. Copy server/.env.example to server/.env and fill it in." },
          500,
        );
      }
      const result = await generateBuild(body, token);
      return c.json(result);
    } catch (error) {
      if (error instanceof HTTPException) {
        return c.json({ error: error.message }, error.status);
      }
      console.error(error);
      return c.json({ error: "Internal server error" }, 500);
    }
  },
);

// Built frontend (npm run build inside web/, see xell-customizer/README.md) lives at
// web/dist. Falls back to index.html for client-side routing.
app.use("/*", serveStatic({ root: path.relative(process.cwd(), WEB_ROOT) }));
app.get("*", serveStatic({ path: path.relative(process.cwd(), path.join(WEB_ROOT, "index.html")) }));

console.log(`XeLL Customizer listening on http://localhost:${PORT}`);
serve({ fetch: app.fetch, port: PORT });

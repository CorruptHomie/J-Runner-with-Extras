# XeLL Customizer (ported)

This is [barrenechea/xell-customizer](https://github.com/barrenechea/xell-customizer) +
[barrenechea/xell-customizer-api](https://github.com/barrenechea/xell-customizer-api),
ported from their original Cloudflare Pages / Cloudflare Workers hosting to a local Node
process, so it can be launched from inside J-Runner (Advanced/XeLL -> "XeLL Customizer
(Web)") instead of requiring a separate deployed web service.

## What actually changed

**Nothing about how a themed XeLL image gets built.** Both versions dispatch a GitHub
Actions workflow (`xell-worker/xell-builder`, `build.yml`) with your chosen colors/ASCII
art, and that workflow does the real compiling. Only the hosting changed:

| | Original | This port |
|---|---|---|
| Frontend hosting | Cloudflare Pages | Served as static files by the local server |
| API hosting | Cloudflare Workers | Node, via `@hono/node-server` |
| Access | `https://xell.barrenechea.cl` | `http://localhost:2222` |

`server/` is the API (`src/index.js`, `src/services/build.service.js`,
`src/schemas/index.js`) ported line-for-line from the original `src/index.ts` /
`build.service.ts` / `schemas/index.ts` - Hono, Zod, and Octokit all run identically under
plain Node, so this isn't a rewrite, just a different entry point
(`serve()` from `@hono/node-server` instead of a Workers `export default`).

`web/` is the original frontend source, unmodified except for `src/lib/generation-service.ts`,
where `API_BASE_URL` now points at the same origin instead of the Workers deployment. It's
included as source *and* pre-built to `web/dist/` so it runs out of the box; rebuild it any
time with `npm run build` inside `web/`.

## Setup

You need your own GitHub personal access token with permission to dispatch workflows on
`xell-worker/xell-builder` (the same requirement the original had via `wrangler secret put
GITHUB_TOKEN`) - this project doesn't provide one.

```
cd server
npm install
cp .env.example .env    # then fill in GITHUB_TOKEN
```

J-Runner's "XeLL Customizer (Web)" menu item runs `node server/src/index.js` for you and
opens `http://localhost:2222`. To run it standalone: `npm start` inside `server/`.

To modify the frontend: edit under `web/src/`, then `cd web && npm install && npm run build`
to refresh `web/dist/`.

## Credits

Both original projects are by **barrenechea** ([barrenechea.cl](https://www.barrenechea.cl)) -
this is the credit added to J-Runner's About screen. Full license: [LICENSE](LICENSE)
(GPL-3.0, carried over from the original repos).

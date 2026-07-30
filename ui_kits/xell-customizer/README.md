# UI kit — XeLL Theme Customizer (web)

J-Runner Premium bundles a second, completely separate surface: a React/Vite web app the
desktop app starts on `localhost:2222` (you can watch it boot in the session log:
"XeLL Customizer: Starting local server on port 2222..."). It lets you recolour the XeLL
bootloader screen and swap its ASCII banner, then generate a custom XeLL build.

Built from:

| Piece | Source |
| --- | --- |
| Page composition, cards, tabs, presets | `xell-customizer/web/src/components/xell-customizer.tsx` |
| Colour palette + presets | `xell-customizer/web/src/lib/theme-colors.ts` |
| Copy (all strings verbatim) | `xell-customizer/web/src/locales/en/translation.json` |
| Theme scale | `xell-customizer/web/src/index.css` (shadcn "slate", dark) |
| Console font | `xell-customizer/web/src/assets/Web437_IBM_VGA_8x16.woff` (copied to `assets/fonts/`) |

**This surface does not share the desktop app's design language** and should not be made to.
It is stock shadcn/ui on the slate dark scale with a blue→purple gradient heading and Lucide
icons; the desktop app is a flat grey WinForms reskin with a green accent and no icon library.
Keep them apart — tokens for this surface are namespaced `--xc-*` and `--xell-*`.

Interactive: preset buttons, the background and text colour grids and the ASCII textarea all
drive the live console preview. "Generate Custom XeLL Build" is inert (it dispatches a GitHub
Actions workflow in the real app).

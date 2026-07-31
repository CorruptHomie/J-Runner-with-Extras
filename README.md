# J-Runner Premium — Design System

**J-Runner Premium** is a Windows desktop tool for modding Xbox 360 consoles: reading and
writing NAND flash, building XeBuild images, programming RGH glitch chips, decrypting
keyvaults and driving a shelf of third-party flashing hardware (PicoFlasher, xFlasher,
Matrix, DemoN, USB tools). It bills itself as *"The Ultimate RGH/JTAG App"*. It is a
community/homebrew project — .NET Framework 4.8 WinForms, distributed as a GitHub release —
not a commercial product, and its design language reflects that: dense, hand-positioned,
information-first, with no marketing surface at all.

## Sources this system was built from

| Source | Detail |
| --- | --- |
| Codebase | `J-Runner-Premium-release/` (attached locally, read-only) |
| GitHub | https://github.com/ScallywagDude/J-Runner-Premium |
| Bundled web app | `J-Runner/xell-customizer/` — upstream: https://github.com/barrenechea/xell-customizer |
| Screenshots | `uploads/4.0.0pre4png.png`, `custom dialogues.png`, `flashing nand.png`, `picorgh builtin.png`, `xellcustomizerbuiltin.png`, `JRLogo.png` |

Related repositories referenced by the project's own README, worth exploring if you want to
build more deeply against this product:
https://github.com/X360Tools/PicoFlasher · https://github.com/Agustin8123/Pico2Flasher ·
https://github.com/prodeveloper0/picoflasher · https://github.com/X360Tools ·
https://github.com/J-Runner-With-Extras

The single most important file in the codebase for design purposes is
**`J-Runner/UI/Theme.cs`** — a central palette plus a recursive "reskin whatever's already
there" pass over the legacy WinForms controls. Every colour token here is lifted from it
verbatim. `UI/DarkTabControl.cs`, `UI/XboxFillProgressBar.cs`,
`UI/JRunnerToolStripRenderer.cs`, `UI/MessageDialog.cs`, `UI/FlashProgressOverlay.cs` and
`MainForm.Chrome.cs` supply the rest of the drawing rules.

## The two products

**1. J-Runner Premium (Windows desktop app).** One fixed-size 832 × 661 window plus a 36px
custom title bar, ~65 secondary forms, everything absolutely positioned. Flat dark grey with
one green accent. This is the brand.

**2. XeLL Theme Customizer (web).** A React/Vite app the desktop app starts on
`localhost:2222` for recolouring the XeLL bootloader screen. It is stock shadcn/ui on the
slate dark scale with a blue→purple gradient heading and Lucide icons — **a deliberately
different design language**. Do not blend the two. Its tokens are namespaced `--xc-*` (web
shell) and `--xell-*` (LibXenon console colours).

---

# CONTENT FUNDAMENTALS

**Voice: terse operator instructions, second person, no personality.** The app talks the way
a technician talks to another technician. Labels are noun phrases — "Nand Reads", "Console
Type", "CPU Key", "Bad Blocks". Buttons are bare imperatives — "Read Nand", "Write ECC",
"Create Donor", "Extract Files", "Program", "Reload", "Scan IP", "Exit". Nothing is softened
and nothing is explained twice.

**Casing:** Title Case for buttons, labels and window titles. Domain nouns keep their
community spelling exactly, and it is inconsistent on purpose — **Nand** (not NAND) in every
control label, but **"Writing NAND"** in the flash overlay; **XeBuild**, **XeLL**,
**Dashlaunch**, **Keyvault**, **RGH**, **JTAG**, **PicoRGH**, **RPicoRGH**, **BOOTSEL**,
**CB**, **LDV**, **KV**. Copy them character-for-character; do not normalise.

**"Ok", not "OK".** The acknowledgement button in `MessageDialog` is literally `"Ok"`.

**Sentence copy is used only where a mistake is expensive.** Three examples, all verbatim:

> Are you sure? Did you make sure to select the correct options and patches?
> Remember to disconnect the flasher from the computer before booting!
> Flashes a Raspberry Pi Pico set to BOOTSEL mode with RGH 1.2 glitcher firmware. Hold the
> BOOTSEL button while plugging the Pico into this PC, then click Program.

Note the pattern: a plain statement of what will happen, then the physical action the user
must take. Exclamation marks appear exactly once, on the "don't brick your console" reminder.

**The log is the product's narrator.** Everything reports into the session console in flat
timestamped lines — `J-Runner Premium` / `Session: 07/30/2026 3:27:15` / `Version: 4.0.0devpre4`
/ `Status: Up to date`. Raw upstream messages, deprecation warnings and stack context are
shown, not hidden. Write log copy as machine output, not as prose.

**No emoji anywhere.** No exclamation-heavy encouragement, no "Oops!", no empty-state
illustrations with a friendly line. The empty state for hardware is four words:
"No flasher detected".

**Credit is given generously and by name.** The project README thanks contributors with links;
the customizer scrolls a "Thanks to:" contributor list. If you write an About or credits
surface, name people and link their profiles.

---

# VISUAL FOUNDATIONS

**Overall vibe:** a legacy WinForms utility that has been carefully re-skinned dark — not a
modern app. Density is high, everything is aligned to a hand-built grid of group boxes, and
the chrome gets out of the way. It should feel like a workbench instrument.

**Colour.** Five neutral surfaces stacked by value do all the structural work:
`--jr-window-bg #161619` → `--jr-panel-bg #1e1e22` → `--jr-raised-bg #2d2d33` →
`--jr-hover-bg #3c3c43`, with fields *recessed* below the window at `--jr-field-bg #111113`.
That downward step is what reads as "editable". Exactly one brand colour exists:
**`--jr-accent #74c757`**, a slightly desaturated green, used for the selected-tab underline
(2px), the checked checkbox/radio, the progress fill, primary dialog buttons, and success
lines. Selection in lists and grids uses the *dimmed* `--jr-accent-dim #468037`, never the
bright accent — the theme explicitly overrides the OS highlight colour to stop it looking
neon. `--jr-danger #d65a50` appears only on the close button's hover fill and on failure
text. The raw saturated console colours (`#00ff00`, `#ffa500`, `#ff0000`) live **only inside
the log well**.

**Type.** Segoe UI at 9pt (12px) for essentially everything; the theme deliberately refuses to
restyle designer fonts because the layout is sized around them. Bold exists only for dialog
titles (11pt), the flash caption (15pt), the Glitch Chip title (10pt), and accent button
labels. **There is no semibold.** Mono is `Web437 IBM VGA 8x16` — a genuine DOS bitmap face
shipped with the customizer — used for console output, CPU keys and hex.

**Spacing & layout.** WinForms absolute positioning: the window is a **fixed 832 × 661**, has
no maximize button, and nothing reflows. Numbers are whatever the designer typed — 6px, 9px,
10px, 11px, 20px — not a 4/8 grid. Copy exact values. Layout is grouped exclusively by
`GroupBox`: a 6px-rounded 1px hairline with the caption punched into its top edge.

**Backgrounds.** Flat colour, always. No gradients, no imagery, no patterns, no texture.
(There *is* a `SnowfallBackground` easter egg attached to the main form — a seasonal particle
effect drawn in the gaps between controls. Treat it as an easter egg, not a motif.) The one
exception is the customizer's blue→purple *text* gradient, which belongs to that surface only.

**Cards.** There are none. What looks like a card is a group box: no shadow, no fill contrast,
no accent left border. If you find yourself adding a rounded elevated card with a coloured
edge, you have left the brand.

**Borders & radii.** 1px hairlines only, in two values — `--jr-border-subtle #2e2e34` at rest,
`--jr-border #3e3e45` on hover or around dialogs. Radii are small and specific: 3px checkbox,
4px menu row, 5px button (derived as `clamp(3, height/5, 8)` — a taller button is rounder),
6px group box, 10px small dialog, 12px message dialog. **Text fields and dropdowns are square** —
`BorderStyle.FixedSingle`, no radius at all.

**Shadows.** Effectively none. Panels are flat; the only real shadow in the product is the OS
drop shadow a borderless window gets back through `CS_DROPSHADOW`. Dialogs get a soft
`--jr-shadow-dialog` here purely so HTML mocks read as floating.

**Transparency & blur.** No blur anywhere. Two transparencies exist: the 45% black scrim
behind a modal (`--jr-scrim`) and the flash overlay's 97% opacity sheet. That's it.

**Hover / press states.** Hover = a step *up* the neutral ladder (RaisedBg → HoverBg) plus the
border brightening from subtle to full. Press = a step *down* (→ PressedBg). Nothing scales,
nothing shifts, nothing fades. Accent buttons hover lighter (`--jr-accent-light`) and press
to `--jr-accent-dim`. The close button is the exception: it hovers to solid `--jr-danger` with
a white glyph. Menu items round to 10px on hover in the title bar, 4px in dropdowns.

**Disabled.** Fill drops to PanelBg, border to BorderSubtle, text to `--jr-text-disabled`.
No opacity trick.

**Animation.** Almost none, and it is user-disableable (Settings → "Enable animations").
Hover, press, tab switching and selection have **zero** transition — WinForms repaints
instantly and the recreation must too. The only motion in the product is during a NAND write:
the progress fraction eases toward its target (12% of the remaining distance every 30ms — an
ease-out, ~600ms to settle), and an indeterminate marquee band sweeps top-to-bottom on a
~1.5s loop. Build everything so it still works with motion off.

**The one hero moment.** A NAND write blocks the whole window with a 97%-opaque sheet: the
caption "Writing NAND" in 15pt bold, the console mark up to 280px square rendered in
**greyscale with a full-colour copy clipped to the bottom N%** — colour literally rises up the
mark as the write progresses — then the percentage and "Do not disconnect the flasher or close
J-Runner." This is the product's only piece of drama and it exists because the operation can
brick hardware. Do not reuse the treatment for anything else.

**Imagery.** Real product photography of flashing hardware on white backgrounds, shown small
and centred in the device panel. Cool, literal, catalogue-like — no styling, no grading, no
illustration. Everything else is UI.

---

# ICONOGRAPHY

**The desktop app has no icon system.** There is no icon font, no SVG sprite, no Lucide/Feather
dependency. What exists is a bag of small raster PNGs in `J-Runner/Resources/` accumulated
over years, copied here into `assets/`:

- **Status:** `icon-greencheck.png`, `icon-rederror.png`, `icon-red-exclamation.png`
- **Actions:** `icon-settings.png`, `icon-update.png`, `icon-clipboard.png`, `icon-key.png`,
  `icon-download.png`, `icon-menu.png`, `icon-hourglass.png`
- **Hardware photos** (not icons — real photographs): `device-picoflasher.png`,
  `device-dirtypico.png`, `device-demon.png`, `device-matrix.png`, `device-usbtool.png`,
  `device-xflash-spi.png`, `device-xflash-emmc.png`, `device-openxenium.png`, `device-usboff.png`
- **Quality raters** (`good/great/average/bad.png`) and a `legend.png` for timing results
- **Controller glyphs** (`key_A`, `key_LB`, `key_RT`, …) used by the POST/DemoN surfaces

**Window chrome glyphs are drawn, not iconised** — minimise and close are 1.3px antialiased
strokes painted in `DrawChromeGlyph`, and the checkbox tick is two 2px round-capped strokes.
A handful of **unicode characters** stand in for the rest: `✕` (U+2715) closes the Glitch Chip
dialog, `▼`/`▶` are dropdown and submenu arrows.

**No emoji, ever.**

For new desktop-app surfaces: reuse an existing PNG, draw a stroke glyph, or use a unicode
arrow. Do not introduce an icon library — it would immediately read as foreign.

**The XeLL Customizer web app is the exception** and uses **Lucide** (`Terminal`, `Monitor`,
`Layout`, `Check`, `Palette`) at 20–24px with the default 2px stroke, tinted
`--xc-icon` (`#a855f7`). The UI kit loads Lucide from CDN — this is the real dependency, not a
substitution.

---

# BRAND MARK

The only mark is a hand-drawn calligraphic "JR" monogram, shipped in two versions:

- **`assets/logo-jr2.png`** — black JR. Use on white or light surfaces.
- **`assets/logo-jr.png`** — white outline JR on transparency. Use on the dark app chrome
  (this is the one the title bar loads).

There is no wordmark, no lockup, no clear-space rule. The mark appears at 16px in the title
bar and nowhere else. Where a larger brand presence is needed, set the product name
**"J-Runner Premium"** in the UI face — do not scale the monogram up or invent a lockup.

**`assets/icon-app.png` is not the brand mark.** It is `Resources/Project3.png`, a **NAND-X
wordmark** the app uses as its window icon. Do not use it as a logo or a favicon.

`assets/xbox-sphere.png` is the console-manufacturer mark the flash overlay fills with colour.
It is a **third-party trademark** carried over from the app's own resources for fidelity — use
it only in recreations of that overlay, never as a brand element of this design system.

---

# FONT SUBSTITUTION — ACTION NEEDED

**Segoe UI is not shipped here.** It is a Microsoft system font, not redistributable, so
`tokens/fonts.css` declares no `@font-face` for it; `--jr-font-ui` falls back to the host UI
font and then to **Open Sans** as the nearest freely-available match. On Windows this renders
correctly. On macOS/Linux screenshots the metrics will differ slightly. If you have a licence
that permits it, drop the Segoe UI files into `assets/fonts/` and add the `@font-face` rules.

`Web437 IBM VGA 8x16` **is** included (it ships with the customizer) and is the real console face.

---

# Index

**Root**
- `styles.css` — the single entry point; `@import`s only.
- `readme.md` (this file), `SKILL.md`, `github.md`, `thumbnail.html`

**`tokens/`** — `colors.css`, `typography.css`, `spacing.css`, `radius.css`, `elevation.css`,
`motion.css`, `fonts.css`, `semantic.css` (role aliases: `--surface-*`, `--text-*`, `--action-*`)

**`components/`** — 17 primitives, one `.jsx` + `.d.ts` + `.prompt.md` each, plus a card per folder.

| Group | Components |
| --- | --- |
| `controls/` | `Button`, `SplitButton`, `TextField`, `Select`, `NumberField`, `Checkbox`, `Radio` |
| `layout/` | `TitleBar`, `GroupBox`, `Tabs`, `MenuDropdown`, `StatusBar` |
| `feedback/` | `ProgressBar`, `LogConsole`, `MessageDialog`, `FlashOverlay`, `DeviceCard` |
| `data/` | `DataTable` |

Every one has a counterpart in `UI/Theme.cs` or a hand-built form. **Intentional additions:**
none — no Toast, Avatar, Tooltip or Accordion has been invented, because the product has none.

**`ui_kits/`**
- `jrunner-app/` — the full 832 × 661 main window, live menus, the Glitch Chip dialog, and the
  complete Confirm Flash → flash overlay → Flash Complete sequence.
- `xell-customizer/` — the bundled web app, with working presets, colour grids and ASCII editor.

**`guidelines/`** — 21 specimen cards (Colors, Type, Spacing, Brand) rendered in the Design
System tab.

**`assets/`** — logos, app icon, status icons, hardware photographs, and `fonts/`.

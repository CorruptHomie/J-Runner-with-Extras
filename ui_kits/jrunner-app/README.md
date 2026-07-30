# UI kit — J-Runner Premium (Windows desktop app)

Recreation of `MainForm` and the two hand-built dialogs, at the app's real fixed client size
(832 × 661 plus the 36px custom title bar).

Source files this was built from:

| Surface | Source |
| --- | --- |
| Window chrome, menu strip, fixed sizing | `J-Runner/MainForm.Chrome.cs` |
| Layout and control inventory | `J-Runner/MainForm.Designer.cs` |
| All colours, glyph painting, tab painting | `J-Runner/UI/Theme.cs`, `UI/DarkTabControl.cs` |
| Menus and context menus | `J-Runner/UI/JRunnerToolStripRenderer.cs` |
| Confirm / complete dialogs | `J-Runner/UI/MessageDialog.cs` |
| Flash overlay and progress fill | `J-Runner/UI/FlashProgressOverlay.cs`, `UI/XboxFillProgressBar.cs` |
| Glitch-chip dialog | `J-Runner/Forms/GlitchChipProgrammer.cs` |
| Side panels | `J-Runner/Panels/XeBuildPanel*`, `Panels/NandInfo*`, `Panels/NandTools*` |

## What is interactive

- Title-bar menus open real dropdowns; **Advanced → Program Glitch Chip** and **Nand → Write Nand** are wired.
- **Write Nand** runs the whole real sequence: Confirm Flash → blocking flash overlay with the
  mark filling from greyscale to colour → Flash Complete.
- **Program Glitch Chip** opens the 420 × 300 dialog and fakes a BOOTSEL programming run.
- Tabs, radios, the Nand Reads stepper and the CPU Key field all work; a flasher "appears" ~1s
  after load the way the real device poll does.

## Deliberately blank

Fields the real app only fills after reading a NAND (console type, CB versions, KV info, bad
blocks) are left empty — the same state J-Runner shows on a cold start. Panels behind
`XB Settings`, `Patches` and `Dashlaunch` were not recreated; they are large data forms whose
contents are out of scope for a visual kit.

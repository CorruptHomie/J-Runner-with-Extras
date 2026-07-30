repo: ScallywagDude/J-Runner-Premium
branch: master
path: J-Runner

## Last sync
date: 2026-07-30T00:00:00Z

### Updated in this project
- Extracted the full `UI/Theme.cs` palette into `tokens/colors.css` (exact ARGB values).
- Authored 17 React components mirroring the app's owner-drawn WinForms controls.
- Recreated `MainForm` (832x661 + 36px custom chrome) as an interactive UI kit.
- Recreated the bundled XeLL Theme Customizer web surface as a second UI kit.

## Screen map
| Project screen | Repo files |
| --- | --- |
| ui_kits/jrunner-app/index.html | J-Runner/MainForm.Chrome.cs, J-Runner/MainForm.Designer.cs, J-Runner/Panels/* |
| ui_kits/jrunner-app/panels.jsx | J-Runner/Panels/NandTools*, J-Runner/Panels/XeBuildPanel*, J-Runner/Panels/NandInfo* |
| ui_kits/jrunner-app/dialogs.jsx | J-Runner/Forms/GlitchChipProgrammer.cs |
| ui_kits/jrunner-app/MainWindow.jsx | J-Runner/UI/MessageDialog.cs, J-Runner/UI/FlashProgressOverlay.cs |
| ui_kits/xell-customizer/index.html | J-Runner/xell-customizer/web/src/components/xell-customizer.tsx |
| ui_kits/xell-customizer/XellCustomizer.jsx | J-Runner/xell-customizer/web/src/lib/theme-colors.ts, .../locales/en/translation.json |
| components/** | J-Runner/UI/Theme.cs, UI/DarkTabControl.cs, UI/XboxFillProgressBar.cs, UI/JRunnerToolStripRenderer.cs |
| tokens/** | J-Runner/UI/Theme.cs, J-Runner/xell-customizer/web/src/index.css |

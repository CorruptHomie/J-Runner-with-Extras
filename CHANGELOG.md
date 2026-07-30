\# J-Runner Premium v4.0.0 developer preview 4 Changelog



\### UI \& Theming

&#x20;AeroWizard Replacement Removed the third-party `AeroWizard` dependency entirely. Replaced all 10 wizard forms (including Update UI, Create Donor Nand, and Restore Files) with a custom, native `ThemedWizard` control for consistent dark theming.

&#x20;Native Dark Mode Title Bars Implemented `DWMWA\_USE\_IMMERSIVE\_DARK\_MODE` to force native Windows title bars into dark mode across all \~65 application windows.

&#x20;Stock OS Controls Expanded dark theme coverage to previously un-themed stock OS controls, including `ListView` (with owner-drawn column headers), `TreeView`, `TrackBar`, scrollbars, and splitters.

&#x20;Menu Logo Visibility Re-rendered the 16px menu bar logo with alpha boosting to prevent thin strokes from antialiasing away, ensuring a solid, bright white appearance.



\### Features \& Visuals

&#x20;Background Snowfall Effect Introduced a background animation featuring Xbox X glyphs drifting in three depth tiers across the main application window. 

&#x20;Dynamic Flashing Visuals The snowfall effect dynamically shifts to lime green while an active NAND flashing operation is in progress.

&#x20;Snowfall Toggle Added a dedicated Background Snow toggle directly in the main JR menu (under Restore Files...) to easily enable or disable the visual effect.

&#x20;Console Rendering Extended the snowfall visual effect to render smoothly over the native console text box without causing text flickering.



\### XeLL Customizer (Web Integration)

&#x20;Stale Process Handling The local server's `health` endpoint now utilizes API versioning. The launcher will automatically detect stale background Node processes and prompt the user to restart if a version mismatch occurs.

&#x20;Direct Workflow Links The interface now provides direct links to View workflow run and Download artifact via the GitHub Actions API while a build is in progress or completed.

&#x20;Log Fetching Improved log fetching to gracefully handle Azure Blob Storage redirects and prevent raw XML error documents from being displayed.

&#x20;Failure Messaging Updated the build failure message to guide users to manually download their artifacts via the workflow link. This message has been localized across English, Spanish, French, and Portuguese.



\### Bug Fixes \& Internal Changes

&#x20;Version \& Build Stamps Version bumped to `4.0.0devpre4`. The internal build stamp (previously hardcoded) now dynamically tracks the version string and linker timestamp (e.g., `400.YYMMDD.HHMM`).

&#x20;Wizard Crash Fix Fixed an `InvalidCastException` that crashed wizard forms on load by properly implementing `ISupportInitialize` for compatibility with designer-generated code.

&#x20;Compilation Fix Resolved a build break caused by a method accidentally placed inside an `enum` block, and added a project-wide check to prevent recurrence.

&#x20;Code Cleanup Removed dormant code references (e.g., `UI.MenuButton`) and stripped out legacy `ThirdPartyBackground` painting workarounds that were previously required for AeroWizard.

&#x20;Polling Fix Fixed a JavaScript reference error in the XeLL Customizer's build status polling logic.


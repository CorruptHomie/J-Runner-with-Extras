## J-Runner Premium 4.0.0dev -

# Changelog

 Fixed a pathing bug to ensure patched NAND images correctly save to the output folder
instead of the parent directory.
 Renamed all folder and variable references from 'Updated NaNDs' to 'Updated flash' for
consistency.
 Consolidated duplicate and incorrect developer credits for DirtyPico into a single
accurate entry.
 Added a new dedicated dialog to support programming RPicoRGH glitch-chips via USB
mass-storage file copy.
 Integrated a bundled, locally hosted Node.js server to run the XeLL Customizer web
interface.s
 Updated the build process to automatically copy necessary web server files and self-
install Node dependencies on first launch.



\# Does not work or does not work properly!

 Replaced the broken 0% progress bar with a standard themed bar and introduced a full-
window animated Xbox logo overlay during actual NAND flashes.

 Expanded the custom UI theme engine to include owner-drawn tab headers, group box
borders, and numeric inputs.

 Implemented a recursive custom dark theme covering menus, borderless window
chrome, and custom dialog boxes.

 Added explicit confirmation and post-write completion dialogs to the hardware NAND
writing process.

 Fixed theming application logic to ensure menus, status strips, and dynamically loaded
controls are properly styled.


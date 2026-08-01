# J-Runner Premium v4.0.0 Pre-Release 1

### Core Refinements \& Fixes

* **ECC Algorithm Optimization:** Rewrote `Nand.addecc\\\_v2()` from a time complexity of `O(n^2)` to `O(n)`. This change reduces array allocations from \~675,000 down to 2, drastically increasing speed while generating byte-identical outputs across all edge cases.
* **Streamlined I/O and Conversions:**

  * Replaced byte-by-byte file loading in `Oper.openfile()` and `Oper.openfilefromoffset()` with direct bulk reads, cutting 69+ million function calls down to a single pass for a 64MB image, while retaining exact error handling behavior.
  * Refactored `ByteArrayToString` across 300 call sites to write hexadecimal strings in one pass with a single allocation, removing significant memory overhead.
* **Logo Consistency:** Regenerated the main interface and menu logos. The white monogram is now properly aspect-corrected, and alpha-boosted downsampling ensures the 16px versions remain bright and clearly defined without aliasing out.

### UI Enhancements

* **Smooth Animation Framework:** Overhauled the background snowfall rendering logic to fix noticeable visual stuttering.

  * Motion is now time-scaled using a `Stopwatch` instead of relying on unpredictable UI tick counts.
  * Increased the render rate to \~64fps by aligning timer intervals to native system clock ticks (15ms).
  * Shifted invalidation logic to only redraw the precise bounding box of each moving flake rather than the entire background, drastically reducing computational load.
* **Console Overlay Compositing:** The console snowfall overlay now utilizes a single-pass `WM\\\_PRINTCLIENT` bitmap blit instead of drawing over the window post-render. This eliminates the persistent visual flicker over the `TextBox` when running at 64fps.
* **Snowfall Toggle Details:** Added a "Background Snow" item in the JR menu under "Restore Files". This toggle is distinct from the "Enable animations" setting, instantly clearing the snow upon click instead of just pausing the frames.
* **Menu Styling:** Implemented an owner-drawn, accent-colored rounded checkmark for menu items to ensure they respect the application's dark theme, rather than defaulting to system-blue boxes.


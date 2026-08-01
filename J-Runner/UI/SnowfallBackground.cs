using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace UI
{
    /// <summary>
    /// Drifting "X" glyphs across the window background, a shade lighter than it.
    ///
    /// WinForms controls are separate windows drawn over the form, so a form-level effect is
    /// only ever visible where nothing sits on top - on MainForm that's roughly a tenth of
    /// the surface: the title strip, the gutters between panels and the outer margins. This
    /// therefore paints deliberately subtly; it's texture, not a focal point.
    ///
    /// Only the uncovered region is painted and invalidated, so child controls are never
    /// asked to repaint. That's what keeps it cheap and flicker-free - invalidating the whole
    /// form 30 times a second would drag ~150 controls into every frame.
    /// </summary>
    internal sealed class SnowfallBackground : IDisposable
    {
        private sealed class Flake
        {
            public float X, Y;
            public float Speed;     // px per tick
            public float Size;      // arm length
            public float Drift;     // horizontal sway amplitude
            public float Phase;     // sway offset
            public int Tier;        // 0 = furthest, 2 = nearest; colour is derived per frame
            public float PrevX, PrevY;   // last drawn position, for dirty-rect invalidation

        }

        // 15ms, not 33. System.Windows.Forms.Timer is a WM_TIMER wrapper and can only fire
        // on a system clock tick, which defaults to 15.6ms. 33 sits just above two ticks
        // (31.2ms), so every frame waited for the third at 46.8ms - and any small delay
        // pushed the odd frame to 62.4ms, so the gap alternated. That uneven schedule is the
        // stutter; making the motion time-based (last round) smoothed the speed but couldn't
        // fix the delivery. Anything <= 15 lands on the next tick, giving a steady ~64fps.
        private const int IntervalMs = 15;
        private const float MaxStepSeconds = 0.10f;     // clamp after a stall

        private readonly Form _form;
        private readonly Timer _timer;
        private readonly List<Flake> _flakes = new List<Flake>();
        private readonly Random _rng = new Random();

        private readonly System.Diagnostics.Stopwatch _clock = System.Diagnostics.Stopwatch.StartNew();
        private double _lastSeconds;
        private Region _gaps;
        private bool _disposed;
        private readonly List<OverlayPainter> _overlays = new List<OverlayPainter>();

        public static SnowfallBackground Attach(Form form)
        {
            return form == null ? null : new SnowfallBackground(form);
        }

        private SnowfallBackground(Form form)
        {
            _form = form;

            int area = Math.Max(1, form.ClientSize.Width * form.ClientSize.Height);
            // Denser than the raw area suggests: only about a tenth of the window is bare
            // background, so most flakes are behind a control at any moment and a sparser
            // field reads as almost nothing.
            int count = Math.Max(30, Math.Min(80, area / 7000));
            for (int i = 0; i < count; i++) _flakes.Add(NewFlake(seeded: true));

            _form.Paint += OnPaint;
            _form.Resize += OnResize;
            _form.ControlAdded += OnLayoutChanged;
            _form.ControlRemoved += OnLayoutChanged;

            _timer = new Timer { Interval = IntervalMs };
            _timer.Tick += OnTick;
            _timer.Start();
        }

        /// <summary>
        /// Extends the field onto a control that would otherwise cover it. Needed for the
        /// console, which is a native TextBox: it's opaque and drawn over the form, so the
        /// only way onto it is to intercept its own painting. Flake positions stay
        /// form-relative and are translated, so a glyph crossing the console's edge carries
        /// straight on rather than restarting.
        /// </summary>
        public void PaintOnto(Control target)
        {
            if (target == null) return;
            _overlays.Add(new OverlayPainter(this, target));
        }

        private sealed class OverlayPainter : NativeWindow
        {
            private const int WM_PAINT = 0x000F;
            private const int WM_ERASEBKGND = 0x0014;
            private const int WM_PRINTCLIENT = 0x0318;
            private const int PRF_CLIENT = 0x00000004;

            [StructLayout(LayoutKind.Sequential)]
            private struct RECT { public int Left, Top, Right, Bottom; }

            [StructLayout(LayoutKind.Sequential)]
            private struct PAINTSTRUCT
            {
                public IntPtr hdc;
                public bool fErase;
                public RECT rcPaint;
                public bool fRestore;
                public bool fIncUpdate;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
                public byte[] rgbReserved;
            }

            [DllImport("user32.dll")] private static extern IntPtr BeginPaint(IntPtr hWnd, out PAINTSTRUCT ps);
            [DllImport("user32.dll")] private static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT ps);
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

            private readonly SnowfallBackground _owner;
            private readonly Control _target;
            private Bitmap _buffer;

            public OverlayPainter(SnowfallBackground owner, Control target)
            {
                _owner = owner;
                _target = target;
                if (target.IsHandleCreated) AssignHandle(target.Handle);
                else target.HandleCreated += (s, e) => { try { AssignHandle(target.Handle); } catch { } };
                target.HandleDestroyed += (s, e) => { try { ReleaseHandle(); } catch { } };
                target.Resize += (s, e) => { if (_buffer != null) { _buffer.Dispose(); _buffer = null; } };
            }

            public Control Target { get { return _target; } }

            protected override void WndProc(ref Message m)
            {
                // Swallowed entirely - the composited paint below repaints the whole dirty
                // area, so an erase would only add a blank flash before it.
                if (m.Msg == WM_ERASEBKGND) { m.Result = (IntPtr)1; return; }

                if (m.Msg == WM_PAINT && TryPaintComposited()) return;

                base.WndProc(ref m);
            }

            /// <summary>
            /// Renders the control and the flakes into one off-screen bitmap, then blits it
            /// in a single operation.
            ///
            /// The previous version let the control paint itself and then drew flakes on top
            /// afterwards. That's two visible passes - erase, text, then glyphs - and at
            /// 64fps the intermediate state is on screen long enough to read as flicker.
            /// WM_PRINTCLIENT asks the control to render into a DC we supply, so the text and
            /// the glyphs reach the screen together in one blit and there is no intermediate
            /// state to see.
            /// </summary>
            private bool TryPaintComposited()
            {
                PAINTSTRUCT ps;
                IntPtr hdc = BeginPaint(_target.Handle, out ps);
                if (hdc == IntPtr.Zero) return false;

                try
                {
                    Rectangle dirty = Rectangle.FromLTRB(ps.rcPaint.Left, ps.rcPaint.Top,
                                                         ps.rcPaint.Right, ps.rcPaint.Bottom);
                    if (dirty.Width <= 0 || dirty.Height <= 0) return true;

                    int w = Math.Max(1, _target.ClientSize.Width);
                    int h = Math.Max(1, _target.ClientSize.Height);
                    if (_buffer == null || _buffer.Width != w || _buffer.Height != h)
                    {
                        if (_buffer != null) _buffer.Dispose();
                        _buffer = new Bitmap(w, h);
                    }

                    using (Graphics mg = Graphics.FromImage(_buffer))
                    {
                        // Clipped to the dirty rectangle so the control only re-renders the
                        // strip that actually changed, not its whole surface every frame.
                        mg.SetClip(dirty);
                        using (SolidBrush bg = new SolidBrush(_target.BackColor))
                            mg.FillRectangle(bg, dirty);

                        IntPtr mdc = mg.GetHdc();
                        try { SendMessage(_target.Handle, WM_PRINTCLIENT, mdc, (IntPtr)PRF_CLIENT); }
                        finally { mg.ReleaseHdc(mdc); }

                        mg.SmoothingMode = SmoothingMode.AntiAlias;
                        _owner.DrawFlakes(mg, -_target.Left, -_target.Top);
                    }

                    using (Graphics wg = Graphics.FromHdc(hdc))
                    {
                        wg.SetClip(dirty);
                        wg.DrawImageUnscaled(_buffer, 0, 0);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    // The frame is lost rather than the control - BeginPaint has already
                    // validated the region, so returning true keeps the paint cycle intact.
                    if (JRunner.variables.debugme) Console.WriteLine("Snowfall overlay: " + ex.Message);
                    return true;
                }
                finally
                {
                    EndPaint(_target.Handle, ref ps);
                }
            }

            public void DisposeBuffer()
            {
                if (_buffer != null) { _buffer.Dispose(); _buffer = null; }
            }
        }

        /// <summary>
        /// Union of where each flake was last drawn and where it is now, invalidated on the
        /// form and on every overlay. Padded by a couple of pixels to cover the antialiased
        /// edge of the strokes.
        /// </summary>
        private void InvalidateFlakeAreas()
        {
            foreach (Flake f in _flakes)
            {
                float curX = f.X + (float)Math.Sin(f.Phase) * f.Drift;
                float curY = f.Y;
                float pad = f.Size + 3f;

                // A flake that hasn't been drawn yet has no previous position. NaN would
                // propagate through Min/Max and make the cast to int undefined, so fall back
                // to the current position for the first frame.
                float prevX = float.IsNaN(f.PrevX) ? curX : f.PrevX;
                float prevY = float.IsNaN(f.PrevY) ? curY : f.PrevY;

                float minX = Math.Min(curX, prevX) - pad;
                float minY = Math.Min(curY, prevY) - pad;
                float maxX = Math.Max(curX, prevX) + pad;
                float maxY = Math.Max(curY, prevY) + pad;

                Rectangle r = Rectangle.FromLTRB(
                    (int)Math.Floor(minX), (int)Math.Floor(minY),
                    (int)Math.Ceiling(maxX), (int)Math.Ceiling(maxY));
                if (r.Width <= 0 || r.Height <= 0) continue;

                _form.Invalidate(r);

                foreach (OverlayPainter o in _overlays)
                {
                    Control t = o.Target;
                    if (!t.IsHandleCreated || !t.Visible) continue;
                    Rectangle local = new Rectangle(r.X - t.Left, r.Y - t.Top, r.Width, r.Height);
                    if (local.IntersectsWith(t.ClientRectangle)) t.Invalidate(local);
                }
            }
        }

        private void DrawFlakes(Graphics g, float offsetX, float offsetY)
        {
            foreach (Flake f in _flakes)
            {
                float baseX = f.X + (float)Math.Sin(f.Phase) * f.Drift;
                float x = baseX + offsetX;
                float y = f.Y + offsetY;
                float sz = f.Size;

                // Recorded from the form pass only (offset 0,0); overlays draw the same
                // frame and must not advance the record before the next invalidate.
                if (offsetX == 0f && offsetY == 0f) { f.PrevX = baseX; f.PrevY = f.Y; }

                using (Pen p = new Pen(ColourFor(f.Tier), sz <= 3.5f ? 1f : 1.4f))
                {
                    p.StartCap = LineCap.Round;
                    p.EndCap = LineCap.Round;
                    g.DrawLine(p, x - sz, y - sz, x + sz, y + sz);
                    g.DrawLine(p, x + sz, y - sz, x - sz, y + sz);
                }
            }
        }

        private Flake NewFlake(bool seeded)
        {
            // Three depth tiers: smaller and dimmer reads as further away, and the variation
            // stops the field looking like a regular grid.
            int tier = _rng.Next(3);
            float size = tier == 0 ? 3.5f : tier == 1 ? 5f : 7f;

            return new Flake
            {
                X = (float)_rng.NextDouble() * Math.Max(1, _form.ClientSize.Width),
                // Seeded flakes start scattered; recycled ones re-enter just above the top.
                Y = seeded
                    ? (float)_rng.NextDouble() * Math.Max(1, _form.ClientSize.Height)
                    : -size * 2f,
                // Pixels per SECOND, not per tick. Tying movement to the tick count made
                // the fall visibly stutter: a WinForms Timer fires whenever the UI thread is
                // free, so a late tick moved a flake the same distance as an on-time one and
                // the motion jerked. Distance is now elapsed-time based, so an irregular
                // tick rate changes only the smoothness, never the speed.
                Speed = 18f + (tier * 10.5f) + (float)_rng.NextDouble() * 12f,
                Size = size,
                Drift = 4f + (float)_rng.NextDouble() * 10f,
                Phase = (float)(_rng.NextDouble() * Math.PI * 2),
                Tier = tier,
                PrevX = float.NaN,   // set on first draw; NaN keeps the first rect tight
                PrevY = float.NaN,
            };
        }

        // Normally a shade lighter than whatever it's drawn on; bright lime while a NAND
        // write is running, so the state reads at a glance from anywhere in the room.
        private static Color ColourFor(int tier)
        {
            if (Theme.FlashActive)
            {
                return tier == 0 ? Color.FromArgb(70, 170, 45)
                     : tier == 1 ? Color.FromArgb(110, 225, 65)
                                 : Color.FromArgb(150, 255, 95);
            }
            return tier == 0 ? Color.FromArgb(32, 32, 37)
                 : tier == 1 ? Color.FromArgb(38, 38, 44)
                             : Color.FromArgb(45, 45, 52);
        }

        /// <summary>Forces one repaint, so toggling the effect off clears it at once.</summary>
        public void Refresh()
        {
            if (_disposed) return;
            RefreshGaps();
            if (_gaps != null) _form.Invalidate(_gaps);
            foreach (OverlayPainter o in _overlays)
                if (o.Target.IsHandleCreated) o.Target.Invalidate();
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!Theme.SnowfallEnabled) return;
            // The "Enable animations" setting stops the motion but leaves the glyphs on
            // screen - the brief was lighter X's in the background *and* a way to turn the
            // animation off, so switching it off shouldn't empty the window.
            if (!Theme.AnimationsEnabled) return;
            if (_form.WindowState == FormWindowState.Minimized || !_form.Visible) return;

            double now = _clock.Elapsed.TotalSeconds;
            float dt = (float)(now - _lastSeconds);
            _lastSeconds = now;
            // A stall (dialog, heavy NAND work) would otherwise teleport every flake at once.
            if (dt > MaxStepSeconds) dt = MaxStepSeconds;
            if (dt <= 0f) return;

            int h = _form.ClientSize.Height;
            for (int i = 0; i < _flakes.Count; i++)
            {
                Flake f = _flakes[i];
                f.Y += f.Speed * dt;
                f.Phase += 0.6f * dt;
                if (f.Y - f.Size > h) _flakes[i] = NewFlake(seeded: false);
            }

            // Invalidating the whole gap region repainted every uncovered pixel of the window
            // each frame - at 64fps that's far too much work and frames started dropping,
            // which looks like stutter even with correct timing. Only the small area each
            // flake actually moved through is invalidated now: ~80 tiny rectangles instead of
            // the entire background.
            InvalidateFlakeAreas();

            // Rebuilt only when the layout can actually have changed. Doing it on a timer
            // meant excluding ~150 rectangles mid-animation every couple of seconds, which
            // showed up as a periodic hitch. The window is fixed-size, so the gaps only move
            // when something is shown or hidden - Resize/ControlAdded/ControlRemoved cover
            // that, and Refresh() forces it when a caller knows better.
            if (_gaps == null) RefreshGaps();

            // Only the uncovered area is invalidated, so no child control repaints.
            // Overlays get the same dirty rectangles, translated - so the console repaints a
            // few small strips rather than its whole surface. That's cheap enough to run at
            // full rate now, which it has to: refreshing it at half rate made the flakes over
            // the console visibly steppier than the ones beside it.
        }

        private void OnResize(object sender, EventArgs e)
        {
            InvalidateGaps();
        }

        private void OnLayoutChanged(object sender, ControlEventArgs e)
        {
            InvalidateGaps();
        }

        // Dropping the cached region makes the next tick rebuild it, rather than doing the
        // work inside the event itself.
        private void InvalidateGaps()
        {
            Region old = _gaps;
            _gaps = null;
            if (old != null) old.Dispose();
        }

        /// <summary>
        /// Recomputes which parts of the form aren't covered by a control. Cached rather than
        /// rebuilt per frame - excluding ~150 rectangles from a region 30 times a second is
        /// real work, and on a fixed-size window the answer rarely changes.
        /// </summary>
        private void RefreshGaps()
        {
            Region updated = new Region(_form.ClientRectangle);
            try
            {
                foreach (Control c in _form.Controls)
                    if (c.Visible) updated.Exclude(c.Bounds);
            }
            catch (Exception ex)
            {
                if (JRunner.variables.debugme) Console.WriteLine("Snowfall: " + ex.Message);
            }

            Region old = _gaps;
            _gaps = updated;
            if (old != null) old.Dispose();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            if (_disposed) return;
            if (_gaps == null) RefreshGaps();

            if (!Theme.SnowfallEnabled) return;

            Graphics g = e.Graphics;
            Region previousClip = g.Clip;
            try
            {
                // Clipped to the gaps so nothing is drawn underneath a control, where it
                // would only cost time and never be seen.
                g.Clip = _gaps;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                DrawFlakes(g, 0f, 0f);
            }
            catch (Exception ex)
            {
                if (JRunner.variables.debugme) Console.WriteLine("Snowfall: " + ex.Message);
            }
            finally
            {
                g.Clip = previousClip;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_timer != null) { _timer.Stop(); _timer.Dispose(); }
            if (_form != null)
            {
                _form.Paint -= OnPaint;
                _form.Resize -= OnResize;
                _form.ControlAdded -= OnLayoutChanged;
                _form.ControlRemoved -= OnLayoutChanged;
            }
            foreach (OverlayPainter o in _overlays)
            {
                try { o.ReleaseHandle(); } catch { }
                try { o.DisposeBuffer(); } catch { }
            }
            _overlays.Clear();
            if (_gaps != null) { _gaps.Dispose(); _gaps = null; }
        }
    }
}

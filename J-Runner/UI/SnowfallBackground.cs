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

        }

        private const int IntervalMs = 33;              // ~30fps
        private const int RegionRefreshTicks = 60;      // ~2s

        private readonly Form _form;
        private readonly Timer _timer;
        private readonly List<Flake> _flakes = new List<Flake>();
        private readonly Random _rng = new Random();

        private Region _gaps;
        private int _sinceRegionRefresh = int.MaxValue;
        private bool _disposed;
        private readonly List<OverlayPainter> _overlays = new List<OverlayPainter>();
        private int _tick;

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

            private readonly SnowfallBackground _owner;
            private readonly Control _target;

            public OverlayPainter(SnowfallBackground owner, Control target)
            {
                _owner = owner;
                _target = target;
                if (target.IsHandleCreated) AssignHandle(target.Handle);
                else target.HandleCreated += (s, e) => { try { AssignHandle(target.Handle); } catch { } };
                target.HandleDestroyed += (s, e) => { try { ReleaseHandle(); } catch { } };
            }

            public Control Target { get { return _target; } }

            protected override void WndProc(ref Message m)
            {
                // Erasing is answered here rather than left to the control, because the
                // default erase-then-paint on a native EDIT flickers badly once something is
                // being drawn over it 15 times a second.
                if (m.Msg == WM_ERASEBKGND)
                {
                    try
                    {
                        using (Graphics g = Graphics.FromHdc(m.WParam))
                        using (SolidBrush b = new SolidBrush(_target.BackColor))
                            g.FillRectangle(b, _target.ClientRectangle);
                        m.Result = (IntPtr)1;
                        return;
                    }
                    catch { /* fall through to the default erase */ }
                }

                base.WndProc(ref m);
                if (m.Msg == WM_PAINT) _owner.PaintOverlay(_target);
            }
        }

        private void PaintOverlay(Control target)
        {
            if (_disposed || !Theme.SnowfallEnabled || !target.IsHandleCreated) return;
            try
            {
                using (Graphics g = Graphics.FromHwnd(target.Handle))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    DrawFlakes(g, -target.Left, -target.Top);
                }
            }
            catch (Exception ex)
            {
                if (JRunner.variables.debugme) Console.WriteLine("Snowfall: " + ex.Message);
            }
        }

        private void DrawFlakes(Graphics g, float offsetX, float offsetY)
        {
            foreach (Flake f in _flakes)
            {
                float x = f.X + (float)Math.Sin(f.Phase) * f.Drift + offsetX;
                float y = f.Y + offsetY;
                float sz = f.Size;

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
                // ~18-50 px/s at 30fps, so a flake crosses the window in roughly 13-35
                // seconds - drifting rather than falling, but visibly moving.
                Speed = 0.6f + (tier * 0.35f) + (float)_rng.NextDouble() * 0.4f,
                Size = size,
                Drift = 4f + (float)_rng.NextDouble() * 10f,
                Phase = (float)(_rng.NextDouble() * Math.PI * 2),
                Tier = tier,
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

            int h = _form.ClientSize.Height;
            for (int i = 0; i < _flakes.Count; i++)
            {
                Flake f = _flakes[i];
                f.Y += f.Speed;
                f.Phase += 0.02f;
                if (f.Y - f.Size > h) _flakes[i] = NewFlake(seeded: false);
            }

            if (++_sinceRegionRefresh >= RegionRefreshTicks) RefreshGaps();

            // Only the uncovered area is invalidated, so no child control repaints.
            if (_gaps != null) _form.Invalidate(_gaps);

            // Overlaid controls are refreshed at half rate. A native TextBox repaints its
            // text on every invalidate, and doing that at the full frame rate is visibly
            // busy while the console is scrolling; half is still smooth for something moving
            // this slowly.
            if ((++_tick & 1) == 0)
            {
                foreach (OverlayPainter o in _overlays)
                    if (o.Target.IsHandleCreated && o.Target.Visible) o.Target.Invalidate();
            }
        }

        private void OnResize(object sender, EventArgs e)
        {
            _sinceRegionRefresh = int.MaxValue;
        }

        /// <summary>
        /// Recomputes which parts of the form aren't covered by a control. Cached rather than
        /// rebuilt per frame - excluding ~150 rectangles from a region 30 times a second is
        /// real work, and on a fixed-size window the answer rarely changes.
        /// </summary>
        private void RefreshGaps()
        {
            _sinceRegionRefresh = 0;
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
            }
            foreach (OverlayPainter o in _overlays) { try { o.ReleaseHandle(); } catch { } }
            _overlays.Clear();
            if (_gaps != null) { _gaps.Dispose(); _gaps = null; }
        }
    }
}

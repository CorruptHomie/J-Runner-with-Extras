
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace JRunner
{
    /// <summary>
    /// Routes Console output into the log TextBox.
    ///
    /// The previous version overrode only Write(char). TextWriter's base implementation
    /// decomposes every string into individual characters, so a 45-character log line cost
    /// 45 cross-thread BeginInvoke posts and 45 AppendText calls - and AppendText re-lays
    /// out the control each time. With 2,605 Console.Write call sites and hundreds of lines
    /// logged during a flash, that was tens of thousands of marshalled UI updates competing
    /// with the operation actually doing the work.
    ///
    /// Now: string-level overrides so a line is one post instead of one per character, and
    /// a short coalescing buffer so a burst of lines becomes a single append rather than one
    /// per line.
    /// </summary>
    public class TextBoxStreamWriter : TextWriter
    {
        private readonly TextBox _output;
        private readonly StringBuilder _pending = new StringBuilder();
        private readonly object _lock = new object();
        private readonly Timer _flushTimer;
        private bool _flushQueued;

        // Long enough to batch a burst, short enough that output still feels immediate.
        private const int FlushDelayMs = 50;
        // Guards against a runaway producer holding text indefinitely.
        private const int MaxPendingChars = 16384;

        public TextBoxStreamWriter(TextBox output)
        {
            _output = output;

            _flushTimer = new Timer { Interval = FlushDelayMs };
            _flushTimer.Tick += (s, e) => { _flushTimer.Stop(); Flush(); };
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        public override void Write(char value)
        {
            Append(value.ToString());
        }

        // Without these, TextWriter falls back to per-character Write - which is the whole
        // problem this class had.
        public override void Write(string value)
        {
            if (!string.IsNullOrEmpty(value)) Append(value);
        }

        public override void WriteLine(string value)
        {
            Append((value ?? string.Empty) + Environment.NewLine);
        }

        public override void WriteLine()
        {
            Append(Environment.NewLine);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            if (buffer != null && count > 0) Append(new string(buffer, index, count));
        }

        private void Append(string text)
        {
            bool flushNow;
            lock (_lock)
            {
                _pending.Append(text);
                flushNow = _pending.Length >= MaxPendingChars;
                if (!flushNow && _flushQueued) return;
                _flushQueued = true;
            }

            try
            {
                if (_output.InvokeRequired) _output.BeginInvoke(new Action(ScheduleFlush));
                else ScheduleFlush();
            }
            catch (InvalidOperationException)
            {
                // Handle destroyed while output was in flight - nothing useful to do.
                lock (_lock) { _flushQueued = false; }
            }
        }

        private void ScheduleFlush()
        {
            // Runs on the UI thread; the timer coalesces a burst into one append.
            if (!_flushTimer.Enabled) _flushTimer.Start();
        }

        public override void Flush()
        {
            string text;
            lock (_lock)
            {
                if (_pending.Length == 0) { _flushQueued = false; return; }
                text = _pending.ToString();
                _pending.Length = 0;
                _flushQueued = false;
            }

            try
            {
                if (_output.InvokeRequired) _output.BeginInvoke(new Action<string>(AppendToBox), text);
                else AppendToBox(text);
            }
            catch (InvalidOperationException) { }
        }

        private void AppendToBox(string text)
        {
            // InvalidOperationException alone: ObjectDisposedException derives from it, so
            // listing both made the second clause unreachable and failed to compile (CS0160).
            // Both mean the same thing here - the target control is gone, drop the text.
            try { _output.AppendText(text); }
            catch (InvalidOperationException) { }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try { Flush(); } catch { }
                if (_flushTimer != null) _flushTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

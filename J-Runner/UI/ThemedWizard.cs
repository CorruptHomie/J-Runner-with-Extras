using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace UI
{
    /// <summary>
    /// Same shape as AeroWizard's WizardPageConfirmEventArgs - a cancellable event arg - so
    /// existing Commit handlers keep working after the type swap.
    /// </summary>
    public class WizardPageConfirmEventArgs : CancelEventArgs
    {
    }

    /// <summary>
    /// Drop-in replacement for AeroWizard.WizardPage.
    /// </summary>
    [DesignerCategory("")]
    public class ThemedWizardPage : Panel
    {
        /// <summary>Page shown when Next is pressed. Null means "the one after this".</summary>
        public ThemedWizardPage NextPage { get; set; }

        /// <summary>Forces the button to read Finish regardless of position.</summary>
        public bool IsFinishPage { get; set; }

        // Navigation flags, matching AeroWizard's. Set from designers and at runtime - a
        // page that's mid-operation turns Next and Cancel off and back on again - so the
        // setters have to tell the owning wizard to re-evaluate its buttons rather than
        // being read only when the page is first shown.
        private bool _allowNext = true;
        private bool _allowBack = true;
        private bool _allowCancel = true;
        private bool _showNext = true;
        private bool _showCancel = true;

        /// <summary>Whether Next/Finish is enabled on this page.</summary>
        public bool AllowNext
        {
            get { return _allowNext; }
            set { _allowNext = value; NotifyOwner(); }
        }

        /// <summary>Whether going back from this page is permitted.</summary>
        public bool AllowBack
        {
            get { return _allowBack; }
            set { _allowBack = value; NotifyOwner(); }
        }

        /// <summary>Whether Cancel is enabled on this page.</summary>
        public bool AllowCancel
        {
            get { return _allowCancel; }
            set { _allowCancel = value; NotifyOwner(); }
        }

        /// <summary>Whether Next/Finish is shown at all on this page.</summary>
        public bool ShowNext
        {
            get { return _showNext; }
            set { _showNext = value; NotifyOwner(); }
        }

        /// <summary>Whether Cancel is shown at all on this page.</summary>
        public bool ShowCancel
        {
            get { return _showCancel; }
            set { _showCancel = value; NotifyOwner(); }
        }

        /// <summary>Set by the wizard when the page is added to its Pages collection.</summary>
        internal ThemedWizard Owner { get; set; }

        private void NotifyOwner()
        {
            if (Owner != null && Owner.SelectedPage == this) Owner.RefreshButtons();
        }

        /// <summary>Raised when leaving the page forwards; set e.Cancel to stay.</summary>
        public event EventHandler<WizardPageConfirmEventArgs> Commit;

        /// <summary>Raised when the page becomes visible.</summary>
        public event EventHandler Initialize;

        /// <summary>Raised when leaving the page backwards; set e.Cancel to stay.</summary>
        public event EventHandler<WizardPageConfirmEventArgs> Rollback;

        public ThemedWizardPage()
        {
            BackColor = Theme.PanelBg;
            ForeColor = Theme.TextPrimary;
        }

        internal bool RaiseCommit()
        {
            EventHandler<WizardPageConfirmEventArgs> handler = Commit;
            if (handler == null) return true;
            WizardPageConfirmEventArgs args = new WizardPageConfirmEventArgs();
            handler(this, args);
            return !args.Cancel;
        }

        internal void RaiseInitialize()
        {
            EventHandler handler = Initialize;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        internal bool RaiseRollback()
        {
            EventHandler<WizardPageConfirmEventArgs> handler = Rollback;
            if (handler == null) return true;
            WizardPageConfirmEventArgs args = new WizardPageConfirmEventArgs();
            handler(this, args);
            return !args.Cancel;
        }

        // AeroWizard rendered WizardPage.Text as a heading at the top of the page, and the
        // designers positioned page content beneath it accordingly. Panel draws no Text at
        // all, so drawing it here keeps every existing page layout lining up.
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (string.IsNullOrEmpty(Text)) return;

            Rectangle r = new Rectangle(2, 2, Math.Max(0, Width - 4), 24);
            using (Font f = new Font("Segoe UI", 11F, FontStyle.Regular))
                TextRenderer.DrawText(e.Graphics, Text, f, r, Theme.Accent,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }
    }

    /// <summary>
    /// Drop-in replacement for AeroWizard.WizardControl.
    ///
    /// AeroWizard is a compiled NuGet control that paints its own header, page surround and
    /// command area from system colours, with no properties to change them. Its header is a
    /// child control too, so painting over the parent's uncovered regions couldn't reach it
    /// either - every window using it kept a white strip no matter what the theme did. Since
    /// these forms only ever used it as a titled page host with Back/Next/Cancel buttons,
    /// this reimplements exactly that, themed, with the same member names so the forms swap
    /// over by type alone.
    /// </summary>
    [DesignerCategory("")]
    public class ThemedWizard : Panel, ISupportInitialize
    {
        private const int HeaderHeight = 52;
        private const int FooterHeight = 56;
        private const int Pad = 18;

        private readonly Panel _content;
        private readonly Panel _footer;
        private readonly Panel _footerEdge;
        private readonly Button _btnBack;
        private readonly Button _btnNext;
        private readonly Button _btnCancel;
        private readonly List<ThemedWizardPage> _history = new List<ThemedWizardPage>();

        private ThemedWizardPage _selected;
        private string _title = "";
        private Icon _titleIcon;
        private bool _initializing;

        public ThemedWizard()
        {
            Pages = new PageCollection(this);
            BackColor = Theme.PanelBg;
            ForeColor = Theme.TextPrimary;
            DoubleBuffered = true;

            _content = new Panel
            {
                BackColor = Theme.PanelBg,
                Location = new Point(0, HeaderHeight),
            };

            _footer = new Panel { BackColor = Theme.PanelBg };
            // Drawn as a child rather than in OnPaint: the footer panel sits on top of the
            // wizard's own surface, so a line painted there would be covered by it.
            _footerEdge = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = Theme.BorderSubtle,
                Tag = Theme.SkipTag,
            };
            _footer.Controls.Add(_footerEdge);

            _btnCancel = MakeButton("Cancel");
            _btnNext = MakeButton("&Next", primary: true);
            _btnBack = MakeButton("&Back");

            _btnCancel.Click += (s, e) => DoCancel();
            _btnNext.Click += (s, e) => NextPage();
            _btnBack.Click += (s, e) => PreviousPage();

            _footer.Controls.Add(_btnCancel);
            _footer.Controls.Add(_btnNext);
            _footer.Controls.Add(_btnBack);

            Controls.Add(_content);
            Controls.Add(_footer);

            Resize += (s, e) => LayoutChildren();
        }

        // ---- ISupportInitialize ------------------------------------------------------
        // The generated designer code wraps its property assignments in
        // ((ISupportInitialize)(this.SomeWizard)).BeginInit() / .EndInit(), because
        // AeroWizard's control implemented this. Not implementing it threw an
        // InvalidCastException the moment any of these forms was constructed - a runtime
        // failure a clean compile can't catch. It also gives somewhere sensible to defer
        // layout to, rather than recomputing on every property the designer sets.

        public void BeginInit()
        {
            _initializing = true;
        }

        public void EndInit()
        {
            _initializing = false;
            // Deferred until now: raising it mid-construction would fire a page's Initialize
            // handler before the form's own fields were assigned.
            if (_selected != null) _selected.RaiseInitialize();
            UpdateButtons();
            LayoutChildren();
        }

        // ---- AeroWizard-compatible surface ------------------------------------------

        public PageCollection Pages { get; private set; }

        public string Title
        {
            get { return _title; }
            set { _title = value ?? ""; Invalidate(); }
        }

        public Icon TitleIcon
        {
            get { return _titleIcon; }
            set { _titleIcon = value; Invalidate(); }
        }

        public string FinishButtonText { get; set; } = "&Finish";
        public string NextButtonText { get; set; } = "&Next";
        public string CancelButtonText { get; set; } = "Cancel";
        public string BackButtonText { get; set; } = "&Back";

        /// <summary>Raised once the last page is committed.</summary>
        public event EventHandler Finished;

        /// <summary>Raised when Cancel is pressed; set e.Cancel to stay open.</summary>
        public event CancelEventHandler Cancelling;

        public event EventHandler SelectedPageChanged;

        public ThemedWizardPage SelectedPage
        {
            get { return _selected; }
            set { ShowPage(value, resetHistory: false); }
        }

        /// <summary>Advances, honouring the current page's Commit handler and NextPage.</summary>
        public void NextPage()
        {
            NextPage(null);
        }

        public void NextPage(ThemedWizardPage page)
        {
            if (_selected != null && !_selected.RaiseCommit()) return;

            ThemedWizardPage next = page ?? _selected?.NextPage ?? PageAfter(_selected);
            if (next == null)
            {
                EventHandler handler = Finished;
                if (handler != null) handler(this, EventArgs.Empty);
                return;
            }

            if (_selected != null) _history.Add(_selected);
            ShowPage(next, resetHistory: false);
        }

        public void PreviousPage()
        {
            if (_history.Count == 0) return;
            if (_selected != null && !_selected.RaiseRollback()) return;
            ThemedWizardPage prev = _history[_history.Count - 1];
            _history.RemoveAt(_history.Count - 1);
            ShowPage(prev, resetHistory: false);
        }

        /// <summary>Places a caller-supplied control in the command area, on the left.</summary>
        public void AddCommandControl(Control control)
        {
            if (control == null) return;
            control.Parent = _footer;
            control.Location = new Point(Pad, 1 + (FooterHeight - 1 - control.Height) / 2);
            control.BringToFront();
        }

        // ---- internals ---------------------------------------------------------------

        public class PageCollection : Collection<ThemedWizardPage>
        {
            private readonly ThemedWizard _owner;
            internal PageCollection(ThemedWizard owner) { _owner = owner; }

            protected override void InsertItem(int index, ThemedWizardPage item)
            {
                base.InsertItem(index, item);
                if (item == null) return;
                // Pages are added to Pages, not Controls, in the designers - so parenting
                // has to happen here or nothing would ever be displayed.
                item.Owner = _owner;
                item.Visible = false;
                item.Parent = _owner._content;
                item.Dock = DockStyle.Fill;
                if (_owner._selected == null) _owner.ShowPage(item, resetHistory: true);
                _owner.UpdateButtons();
            }

            public void AddRange(IEnumerable<ThemedWizardPage> pages)
            {
                if (pages == null) return;
                foreach (ThemedWizardPage p in pages) Add(p);
            }
        }

        private ThemedWizardPage PageAfter(ThemedWizardPage page)
        {
            if (page == null) return Pages.Count > 0 ? Pages[0] : null;
            int i = Pages.IndexOf(page);
            return (i >= 0 && i + 1 < Pages.Count) ? Pages[i + 1] : null;
        }

        private void ShowPage(ThemedWizardPage page, bool resetHistory)
        {
            if (page == null) return;
            if (resetHistory) _history.Clear();

            foreach (ThemedWizardPage p in Pages) if (p != page) p.Visible = false;
            _selected = page;
            page.Visible = true;
            page.BringToFront();
            if (!_initializing) page.RaiseInitialize();

            UpdateButtons();
            Invalidate();

            EventHandler handler = SelectedPageChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        internal void RefreshButtons()
        {
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            if (_initializing) return;   // applied once in EndInit
            bool last = _selected == null
                     || _selected.IsFinishPage
                     || (_selected.NextPage == null && PageAfter(_selected) == null);

            _btnNext.Text = last ? FinishButtonText : NextButtonText;
            _btnCancel.Text = CancelButtonText;
            _btnBack.Text = BackButtonText;

            _btnNext.Visible = _selected == null || _selected.ShowNext;
            _btnNext.Enabled = _selected == null || _selected.AllowNext;
            _btnCancel.Visible = _selected == null || _selected.ShowCancel;
            _btnCancel.Enabled = _selected == null || _selected.AllowCancel;
            _btnBack.Visible = _history.Count > 0 && (_selected == null || _selected.AllowBack);

            LayoutChildren();
        }

        private void DoCancel()
        {
            CancelEventHandler handler = Cancelling;
            if (handler != null)
            {
                CancelEventArgs args = new CancelEventArgs();
                handler(this, args);
                if (args.Cancel) return;
            }
            Form form = FindForm();
            if (form != null) form.Close();
        }

        private Button MakeButton(string text, bool primary = false)
        {
            Button b = new Button
            {
                Text = text,
                Size = new Size(92, 30),
                FlatStyle = FlatStyle.Flat,
                Font = Theme.UiFont,
                BackColor = primary ? Theme.Accent : Theme.RaisedBg,
                ForeColor = primary ? Color.FromArgb(20, 24, 18) : Theme.TextPrimary,
                Cursor = Cursors.Hand,
                Tag = Theme.SkipTag,
            };
            b.FlatAppearance.BorderColor = primary ? Theme.Accent : Theme.Border;
            b.FlatAppearance.MouseOverBackColor = primary ? ControlPaint.Light(Theme.Accent) : Theme.HoverBg;
            b.FlatAppearance.MouseDownBackColor = primary ? Theme.AccentDim : Theme.PressedBg;
            return b;
        }

        private void LayoutChildren()
        {
            if (_initializing) return;   // applied once in EndInit
            if (_content == null || _footer == null) return;

            _content.Bounds = new Rectangle(0, HeaderHeight, ClientSize.Width,
                                            Math.Max(0, ClientSize.Height - HeaderHeight - FooterHeight));
            _footer.Bounds = new Rectangle(0, Math.Max(0, ClientSize.Height - FooterHeight),
                                           ClientSize.Width, FooterHeight);

            // Laid out right to left, skipping hidden buttons so a page with ShowNext or
            // ShowCancel off doesn't leave a hole in the row.
            int y = (FooterHeight - _btnNext.Height) / 2;
            int x = ClientSize.Width - Pad;
            foreach (Button b in new[] { _btnCancel, _btnNext, _btnBack })
            {
                if (!b.Visible) continue;
                x -= b.Width;
                b.Location = new Point(x, y);
                x -= 8;
            }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            UpdateButtons();
            LayoutChildren();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Header: title (and icon if one was set), then a hairline separating it from
            // the page area.
            int textLeft = Pad;
            if (_titleIcon != null)
            {
                try
                {
                    g.DrawIcon(_titleIcon, new Rectangle(Pad, (HeaderHeight - 22) / 2, 22, 22));
                    textLeft = Pad + 32;
                }
                catch { /* a bad icon must not stop the header drawing */ }
            }

            if (!string.IsNullOrEmpty(_title))
            {
                Rectangle r = new Rectangle(textLeft, 0, Math.Max(0, ClientSize.Width - textLeft - Pad), HeaderHeight);
                using (Font f = new Font("Segoe UI", 11.5F, FontStyle.Bold))
                    TextRenderer.DrawText(g, _title, f, r, Theme.TextPrimary,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }

            // Footer edge is a child panel (see the constructor); only the header rule is
            // painted here, since nothing covers it.
            using (Pen p = new Pen(Theme.BorderSubtle))
                g.DrawLine(p, 0, HeaderHeight - 1, ClientSize.Width, HeaderHeight - 1);
        }
    }
}

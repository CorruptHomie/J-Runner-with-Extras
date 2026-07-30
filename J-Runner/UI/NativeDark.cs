using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace UI
{
    /// <summary>
    /// Scrollbars, the tab strip's scroll arrows and similar bits are drawn by Windows
    /// itself, not by WinForms, so no managed property recolours them - they stay light on
    /// a dark form. The only real fix is to opt the window into Windows' own dark mode,
    /// which is what this does.
    ///
    /// SetWindowTheme with "DarkMode_Explorer" is documented; the uxtheme entry points that
    /// enable dark mode process-wide are not - they're exported by ordinal only and moved
    /// between Windows 10 builds. Everything here is therefore best-effort and wrapped:
    /// on an older Windows (or if Microsoft moves them again) the app simply keeps the
    /// light scrollbars it has today rather than failing.
    /// </summary>
    internal static class NativeDark
    {
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string subAppName, string subIdList);

        [DllImport("uxtheme.dll", EntryPoint = "#135", SetLastError = true)]
        private static extern int SetPreferredAppMode(int appMode);

        [DllImport("uxtheme.dll", EntryPoint = "#136")]
        private static extern void FlushMenuThemes();

        [DllImport("user32.dll")]
        private static extern bool EnumChildWindows(IntPtr hWnd, EnumWindowsProc callback, IntPtr lParam);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hWnd, int attribute, ref int value, int size);

        // 20 on Windows 10 2004+ and Windows 11; 19 on the 1809-1909 builds where it was
        // still undocumented. Try the current one, fall back to the old one.
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_LEGACY = 19;

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private const int APPMODE_ALLOW_DARK = 1;

        private static bool _appModeSet;

        /// <summary>
        /// Called once at startup, before any window exists. Without this the per-window
        /// SetWindowTheme calls below have no dark theme to switch to.
        /// </summary>
        public static void EnableForApp()
        {
            if (_appModeSet) return;
            _appModeSet = true;
            try
            {
                // Windows 10 1809 and older don't export this; nothing to do there.
                if (Environment.OSVersion.Version.Major < 10) return;
                SetPreferredAppMode(APPMODE_ALLOW_DARK);
                FlushMenuThemes();
            }
            catch (Exception ex)
            {
                if (JRunner.variables.debugme) Console.WriteLine("NativeDark: " + ex.Message);
            }
        }

        /// <summary>
        /// Darkens a window's title bar. The caption is drawn by the window manager, not by
        /// WinForms, so no BackColor reaches it - a form with a native border keeps a white
        /// title bar on an otherwise dark window until DWM is told otherwise. No-op on a
        /// borderless form, and on Windows versions predating the attribute.
        /// </summary>
        public static void EnableDarkTitleBar(Form form)
        {
            if (form == null) return;
            try
            {
                if (!form.IsHandleCreated)
                {
                    form.HandleCreated -= OnFormHandleCreated;
                    form.HandleCreated += OnFormHandleCreated;
                    return;
                }
                ApplyDarkTitleBar(form.Handle);
            }
            catch (Exception ex)
            {
                if (JRunner.variables.debugme) Console.WriteLine("NativeDark: " + ex.Message);
            }
        }

        private static void OnFormHandleCreated(object sender, EventArgs e)
        {
            Form f = sender as Form;
            if (f != null) { try { ApplyDarkTitleBar(f.Handle); } catch { } }
        }

        private static void ApplyDarkTitleBar(IntPtr hWnd)
        {
            int on = 1;
            if (DwmSetWindowAttribute(hWnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref on, sizeof(int)) != 0)
                DwmSetWindowAttribute(hWnd, DWMWA_USE_IMMERSIVE_DARK_MODE_LEGACY, ref on, sizeof(int));
        }

        /// <summary>
        /// Switches a control (and anything Windows created inside it, like a tab strip's
        /// scroll-arrow buttons) to the dark visual style.
        /// </summary>
        public static void Apply(Control c)
        {
            if (c == null) return;
            try
            {
                if (!c.IsHandleCreated)
                {
                    // Applying to a control that has no window yet does nothing, so wait.
                    c.HandleCreated -= OnHandleCreated;
                    c.HandleCreated += OnHandleCreated;
                    return;
                }
                ApplyToHandle(c.Handle);
            }
            catch (Exception ex)
            {
                if (JRunner.variables.debugme) Console.WriteLine("NativeDark: " + ex.Message);
            }
        }

        private static void OnHandleCreated(object sender, EventArgs e)
        {
            Control c = sender as Control;
            if (c != null) { try { ApplyToHandle(c.Handle); } catch { } }
        }

        private static void ApplyToHandle(IntPtr hWnd)
        {
            SetWindowTheme(hWnd, "DarkMode_Explorer", null);
            // Child windows Windows makes for us - the tab strip's up-down scroll buttons
            // being the visible one here - are separate HWNDs and need it individually.
            EnumChildWindows(hWnd, (child, _) =>
            {
                try { SetWindowTheme(child, "DarkMode_Explorer", null); } catch { }
                return true;
            }, IntPtr.Zero);
        }
    }
}

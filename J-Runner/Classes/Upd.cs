using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace JRunner
{
    // Pings ScallywagDude/J-Runner-Premium's GitHub releases for updates, on whichever channel
    // (Release or Pre-Release) matches the currently running build, and can download + install
    // the result automatically. Wired up from Program.cs / MainForm.cs and the Update* wizard
    // forms (UpdateAvailable, UpdChangelog, UpdUI, RestoreFiles).
    public static class Upd
    {
        public const string RepoUrl = "https://github.com/ScallywagDude/J-Runner-Premium";
        private const string RepoApiUrl = "https://api.github.com/repos/ScallywagDude/J-Runner-Premium/releases";
        private const string UserAgent = "J-Runner-with-Extras-Updater";

        // ---- State read by the update wizard forms ----
        public static bool checkSuccess = false;
        public static bool upToDate = true;
        public static bool allowUpdate = false;
        public static bool noUpdateChk = false;
        public static bool runFullUpdate = false;
        public static bool deleteFolders = false;
        public static string changelog = "";
        public static string pendingVersion = "";
        public static string failedReason = "";
        public static CancellationTokenSource cancelSource = new CancellationTokenSource();

        private static string _pendingAssetUrl;
        private static WebClient _activeDownloadClient;

        private class GhAsset
        {
            [JsonProperty("name")] public string Name;
            [JsonProperty("browser_download_url")] public string BrowserDownloadUrl;
        }

        private class GhRelease
        {
            [JsonProperty("tag_name")] public string TagName;
            [JsonProperty("prerelease")] public bool Prerelease;
            [JsonProperty("body")] public string Body;
            [JsonProperty("assets")] public List<GhAsset> Assets;
        }

        // Returns the dotted numeric core of a version string, with the first standalone
        // trailing number (if any) folded in as a synthetic 4th component so that two
        // prereleases sharing a base version - "3.3.0-beta1" vs "3.3.0-beta2" - still compare
        // correctly instead of looking identical once the qualifier text is stripped.
        internal static string ExtractVersionCore(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return null;
            Match m = Regex.Match(raw, @"\d+(\.\d+)+");
            if (!m.Success) return null;

            string remainder = raw.Substring(m.Index + m.Length);
            Match iter = Regex.Match(remainder, @"\d+");
            string core = m.Value;
            int dots = core.Count(c => c == '.');
            if (iter.Success)
            {
                while (dots < 2) { core += ".0"; dots++; }
                core += "." + iter.Value;
            }
            return core;
        }

        // The pre-release channel is used if the running build IS a pre-release, or the user
        // has explicitly opted into pre-release updates from a stable build (Settings).
        public static bool WantsPrereleaseChannel()
        {
            return variables.checkPrereleaseUpdates
                || variables.version.IndexOf("Pre-Release", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool TryFindAssetUrl(List<GhAsset> assets, out string url)
        {
            url = null;
            if (assets == null) return false;
            // Prefer the known release asset name; fall back to any .zip so a rename upstream
            // doesn't quietly break auto-update.
            GhAsset asset = assets.FirstOrDefault(a => a.Name != null && a.Name.Equals("J-Runner.Pro.zip", StringComparison.OrdinalIgnoreCase))
                          ?? assets.FirstOrDefault(a => a.Name != null && a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));
            if (asset == null || string.IsNullOrEmpty(asset.BrowserDownloadUrl)) return false;
            url = asset.BrowserDownloadUrl;
            return true;
        }

        // Finds the latest release matching wantPrerelease, if it's newer than currentVersion.
        // GitHub returns releases newest-first, so the first channel match is that channel's
        // latest - if it isn't newer, nothing further down the list will be either.
        private static bool TryGetNewerRelease(List<GhRelease> releases, string currentVersion, bool wantPrerelease, out GhRelease match)
        {
            match = null;
            string curCore = ExtractVersionCore(currentVersion);
            Version curVer;
            if (curCore == null || !Version.TryParse(curCore, out curVer)) return false;

            foreach (GhRelease r in releases)
            {
                if (r.Prerelease != wantPrerelease) continue;

                string core = ExtractVersionCore(r.TagName);
                Version relVer;
                if (core == null || !Version.TryParse(core, out relVer)) continue;

                if (relVer > curVer)
                {
                    match = r;
                    return true;
                }
                return false;
            }
            return false;
        }

        private static List<GhRelease> FetchReleases()
        {
            string json;
            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add("User-Agent", UserAgent);
                json = wc.DownloadString(RepoApiUrl);
            }
            return JsonConvert.DeserializeObject<List<GhRelease>>(json) ?? new List<GhRelease>();
        }

        // Pings GitHub for the latest Release or Pre-Release - whichever channel the currently
        // running build belongs to - and records the result in checkSuccess/upToDate/changelog.
        public static void check()
        {
            checkSuccess = false;
            upToDate = true;
            changelog = "";
            pendingVersion = "";
            _pendingAssetUrl = null;

            try { cancelSource.Dispose(); } catch { }
            cancelSource = new CancellationTokenSource();

            try
            {
                List<GhRelease> releases = FetchReleases();
                checkSuccess = true;

                GhRelease newer;
                if (TryGetNewerRelease(releases, variables.version, WantsPrereleaseChannel(), out newer))
                {
                    string assetUrl;
                    if (TryFindAssetUrl(newer.Assets, out assetUrl))
                    {
                        upToDate = false;
                        changelog = newer.Body ?? "";
                        pendingVersion = newer.TagName;
                        _pendingAssetUrl = assetUrl;
                    }
                    // else: a newer tag exists but has no usable asset yet (still publishing) -
                    // stay "up to date" for now rather than offering an update with nothing to install.
                }
            }
            catch (Exception ex)
            {
                checkSuccess = false;
                failedReason = ex.Message;
            }
        }

        // Cancels an in-progress download (if any) and closes the app - mirrors what every
        // Cancel button in the update wizard chain expects.
        public static void cancel()
        {
            try { cancelSource.Cancel(); } catch { }
            try { if (_activeDownloadClient != null) _activeDownloadClient.CancelAsync(); } catch { }
            Application.ExitThread();
            Application.Exit();
        }

        // Used by the "Restore Files" flow: wipes common/xeBuild, then reuses startFull() to
        // fetch and re-extract a clean copy.
        public static void restoreFiles()
        {
            deleteFolders = true;
            startFull();
        }

        // Downloads and installs whatever check() found (or, if called directly - e.g. /fullupdate
        // or /restorefiles - resolves the right release itself first), showing progress via UpdUI.
        public static void startFull()
        {
            UpdUI ui = new UpdUI();
            Thread worker = new Thread(() => RunFullUpdate(ui));
            worker.IsBackground = true;
            worker.Start();
            ui.ShowDialog();
        }

        private static void RunFullUpdate(UpdUI ui)
        {
            try
            {
                if (deleteFolders)
                {
                    SafeDeleteDirectory(Path.Combine(variables.pathforit, "common"));
                    SafeDeleteDirectory(Path.Combine(variables.pathforit, "xeBuild"));
                }

                string assetUrl = _pendingAssetUrl;
                if (string.IsNullOrEmpty(assetUrl))
                {
                    bool wantPrerelease = WantsPrereleaseChannel();
                    List<GhRelease> releases = FetchReleases();
                    GhRelease target = releases.FirstOrDefault(r => r.Prerelease == wantPrerelease)
                                     ?? releases.FirstOrDefault(r => !r.Prerelease);
                    if (target == null || !TryFindAssetUrl(target.Assets, out assetUrl))
                    {
                        failedReason = "Could not find a downloadable release for this update channel.";
                        ui.showFailed();
                        return;
                    }
                }

                string zipPath = Path.Combine(Path.GetTempPath(), "JRunnerUpdate.zip");
                using (WebClient wc = new WebClient())
                {
                    _activeDownloadClient = wc;
                    wc.Headers.Add("User-Agent", UserAgent);
                    wc.DownloadProgressChanged += ui.updateProgress;

                    ManualResetEvent done = new ManualResetEvent(false);
                    Exception downloadError = null;
                    wc.DownloadFileCompleted += (s, e) =>
                    {
                        if (e.Error != null && !e.Cancelled) downloadError = e.Error;
                        done.Set();
                    };
                    wc.DownloadFileAsync(new Uri(assetUrl), zipPath);
                    done.WaitOne();
                    _activeDownloadClient = null;
                    if (downloadError != null) throw downloadError;
                }

                if (cancelSource.IsCancellationRequested) return;

                ui.installMode();

                string exePath = Assembly.GetExecutingAssembly().Location;
                string oldPath = exePath + ".old";
                try
                {
                    if (File.Exists(oldPath)) File.Delete(oldPath);
                    File.Move(exePath, oldPath);
                }
                catch { /* best-effort backup; extraction below still proceeds either way */ }

                using (Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(zipPath))
                {
                    zip.ExtractAll(variables.pathforit, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                }

                try { File.Delete(zipPath); } catch { }

                ui.showSuccess();
            }
            catch (Exception ex)
            {
                failedReason = ex.Message;
                ui.showFailed();
            }
        }

        private static void SafeDeleteDirectory(string path)
        {
            try { if (Directory.Exists(path)) Directory.Delete(path, true); }
            catch { /* best-effort; a locked file inside shouldn't block the rest of the restore */ }
        }
    }
}

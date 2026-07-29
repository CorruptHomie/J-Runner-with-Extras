using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace JRunner
{
    // Launches the ported xell-customizer (see /xell-customizer next to the .exe) as a
    // local Node process and opens it in the system browser at http://localhost:2222.
    // This is the "make it a Node.js script you access via http://localhost:2222" option -
    // chosen over a native C# rewrite because the real work the web app does (dispatching
    // and polling a GitHub Actions build in xell-worker/xell-builder) is already written,
    // tested, and working there; re-deriving it in C# would mean re-implementing something
    // that already works rather than porting it.
    public static class XellCustomizerWeb
    {
        private const int Port = 2222;
        // Must match API_VERSION in xell-customizer/server/src/index.js.
        private const int RequiredApiVersion = 3;
        private static Process _serverProcess;

        private static string ServerDir => Path.Combine(variables.pathforit, "xell-customizer", "server");
        private static string EntryPoint => Path.Combine(ServerDir, "src", "index.js");

        public static bool inUse => _serverProcess != null && !_serverProcess.HasExited;

        public static async void LaunchOrFocus()
        {
            try
            {
                ServerState state = await ProbeServer();
                if (state == ServerState.Current)
                {
                    OpenBrowser();
                    return;
                }
                if (state == ServerState.Outdated)
                {
                    // Reusing it would serve the current frontend against an older API, and
                    // the frontend would get HTML back where it expects JSON.
                    if (_serverProcess != null && !_serverProcess.HasExited)
                    {
                        Console.WriteLine("XeLL Customizer: restarting an out-of-date server...");
                        Shutdown();
                        await Task.Delay(500);
                    }
                    else
                    {
                        Console.WriteLine("XeLL Customizer: an older XeLL Customizer server is already running on port {0}.", Port);
                        Console.WriteLine("XeLL Customizer: it was started by a previous session - close J-Runner completely (or end the stray node.exe) and reopen.");
                        return;
                    }
                }

                if (!File.Exists(EntryPoint))
                {
                    Console.WriteLine("XeLL Customizer: {0} not found.", EntryPoint);
                    return;
                }

                string node = FindNode();
                if (node == null)
                {
                    Console.WriteLine("XeLL Customizer: Node.js was not found on PATH.");
                    Console.WriteLine("XeLL Customizer: Install Node.js (nodejs.org), then run \"npm install\" once inside {0}.", ServerDir);
                    return;
                }

                // node_modules is deliberately not copied into the build output (it's
                // ~20MB / thousands of files, and bin\ gets wiped on a clean rebuild), so
                // install it here on first launch instead of making the user do it by hand
                // in a folder that won't survive the next build.
                if (!DependenciesInstalled())
                {
                    Console.WriteLine("XeLL Customizer: First run - installing dependencies, this takes a few seconds...");
                    bool installed = await RunNpmInstall();
                    if (!installed)
                    {
                        Console.WriteLine("XeLL Customizer: npm install failed. You can run it manually in {0}.", ServerDir);
                        return;
                    }
                    Console.WriteLine("XeLL Customizer: Dependencies installed.");
                }

                Console.WriteLine("XeLL Customizer: Starting local server on port {0}...", Port);

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = node,
                    Arguments = "\"" + EntryPoint + "\"",
                    WorkingDirectory = ServerDir,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                _serverProcess = new Process { StartInfo = psi, EnableRaisingEvents = true };
                _serverProcess.OutputDataReceived += (s, e) => { if (variables.debugme && e.Data != null) Console.WriteLine("XeLL Customizer: {0}", e.Data); };
                _serverProcess.ErrorDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine("XeLL Customizer: {0}", e.Data); };
                _serverProcess.Exited += (s, e) => _serverProcess = null;
                _serverProcess.Start();
                _serverProcess.BeginOutputReadLine();
                _serverProcess.BeginErrorReadLine();

                // Give it a few seconds to finish booting before opening the browser -
                // polling /health rather than a fixed sleep so this doesn't race a slow
                // first start.
                for (int i = 0; i < 40; i++)
                {
                    if (await ProbeServer() == ServerState.Current) break;
                    await Task.Delay(250);
                }

                OpenBrowser();
            }
            catch (Exception ex)
            {
                Console.WriteLine("XeLL Customizer: Failed to start - {0}", ex.Message);
                if (variables.debugMode) Console.WriteLine(ex.ToString());
            }
        }

        public static void Shutdown()
        {
            try
            {
                if (_serverProcess != null && !_serverProcess.HasExited) _serverProcess.Kill();
            }
            catch (Exception ex) { if (variables.debugme) Console.WriteLine(ex.ToString()); }
            _serverProcess = null;
        }

        private enum ServerState { Absent, Outdated, Current }

        private static async Task<ServerState> ProbeServer()
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("User-Agent", "J-Runner");
                    string result = await wc.DownloadStringTaskAsync("http://localhost:" + Port + "/health");
                    if (!result.Contains("\"ok\":true")) return ServerState.Absent;
                    return result.Contains("\"api\":" + RequiredApiVersion)
                        ? ServerState.Current
                        : ServerState.Outdated;
                }
            }
            catch { return ServerState.Absent; }
        }

        private static void OpenBrowser()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "http://localhost:" + Port + "/",
                UseShellExecute = true
            });
        }

        // Checking only that node_modules exists isn't enough: if the dependency list grows
        // after someone has already installed once, the folder is there but the new package
        // isn't, and the server dies at import with ERR_MODULE_NOT_FOUND. Verify each
        // dependency named in package.json actually has a folder.
        private static bool DependenciesInstalled()
        {
            string modules = Path.Combine(ServerDir, "node_modules");
            try
            {
                if (!Directory.Exists(modules)) return false;

                string pkgPath = Path.Combine(ServerDir, "package.json");
                if (!File.Exists(pkgPath)) return true;

                string json = File.ReadAllText(pkgPath);
                int at = json.IndexOf("\"dependencies\"", StringComparison.Ordinal);
                if (at < 0) return true;
                int open = json.IndexOf('{', at);
                int close = json.IndexOf('}', open);
                if (open < 0 || close < 0) return true;

                foreach (Match m in Regex.Matches(json.Substring(open, close - open), "\"([^\"]+)\"\\s*:"))
                {
                    string name = m.Groups[1].Value;
                    string dir = Path.Combine(modules, name.Replace('/', Path.DirectorySeparatorChar));
                    if (!Directory.Exists(dir))
                    {
                        Console.WriteLine("XeLL Customizer: dependency \"{0}\" is missing; reinstalling.", name);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                if (variables.debugme) Console.WriteLine(ex.ToString());
                return Directory.Exists(modules);
            }
        }

        private static Task<bool> RunNpmInstall()
        {
            return Task.Run(() =>
            {
                try
                {
                    string npm = FindNpm();
                    if (npm == null)
                    {
                        Console.WriteLine("XeLL Customizer: npm was not found on PATH (it ships with Node.js).");
                        return false;
                    }

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = npm,
                        Arguments = "install --no-audit --no-fund",
                        WorkingDirectory = ServerDir,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    };

                    using (Process p = new Process { StartInfo = psi })
                    {
                        p.OutputDataReceived += (s, e) => { if (variables.debugme && e.Data != null) Console.WriteLine("npm: {0}", e.Data); };
                        p.ErrorDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine("npm: {0}", e.Data); };
                        p.Start();
                        p.BeginOutputReadLine();
                        p.BeginErrorReadLine();
                        if (!p.WaitForExit(240000))
                        {
                            try { p.Kill(); } catch { }
                            Console.WriteLine("XeLL Customizer: npm install timed out.");
                            return false;
                        }
                        return p.ExitCode == 0 && Directory.Exists(Path.Combine(ServerDir, "node_modules"));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("XeLL Customizer: npm install error - {0}", ex.Message);
                    return false;
                }
            });
        }

        private static string FindNpm()
        {
            // npm is a .cmd shim on Windows, so it has to be looked up separately from node.
            return FindOnPath(new[] { "npm.cmd", "npm.exe", "npm" })
                ?? FirstExisting(new[]
                {
                    Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles") ?? "", "nodejs", "npm.cmd"),
                    Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles(x86)") ?? "", "nodejs", "npm.cmd"),
                });
        }

        private static string FindOnPath(string[] candidates)
        {
            string[] pathDirs = (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator);
            foreach (string dir in pathDirs)
            {
                foreach (string exe in candidates)
                {
                    try
                    {
                        string full = Path.Combine(dir, exe);
                        if (File.Exists(full)) return full;
                    }
                    catch { /* malformed PATH entry - skip it */ }
                }
            }
            return null;
        }

        private static string FirstExisting(string[] paths)
        {
            foreach (string p in paths) { if (!string.IsNullOrEmpty(p) && File.Exists(p)) return p; }
            return null;
        }

        private static string FindNode()
        {
            string[] candidates = { "node.exe", "node" };
            string[] pathDirs = (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator);

            foreach (string dir in pathDirs)
            {
                foreach (string exe in candidates)
                {
                    try
                    {
                        string full = Path.Combine(dir, exe);
                        if (File.Exists(full)) return full;
                    }
                    catch { /* malformed PATH entry - skip it */ }
                }
            }

            // Common install locations PATH sometimes misses (e.g. a shell profile that
            // hasn't been reloaded since Node was installed).
            string[] fallbacks =
            {
                Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles") ?? "", "nodejs", "node.exe"),
                Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles(x86)") ?? "", "nodejs", "node.exe"),
            };
            foreach (string f in fallbacks) { if (!string.IsNullOrEmpty(f) && File.Exists(f)) return f; }

            return null;
        }
    }
}

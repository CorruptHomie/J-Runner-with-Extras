using System;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Windows.Forms;

namespace JRunner
{
    // Integrates RPicoRGH (github.com/user/RPicoRGH - RGH 1.2 for Xbox 360 on a Raspberry
    // Pi Pico, credited to Balázs Triszka in About) as a Glitch Chip programming option
    // alongside the existing CPLD/JTAG-based DirtyPico class. Unlike DirtyPico, there's no
    // custom protocol to speak: a Pico in BOOTSEL mode enumerates as a plain USB mass
    // storage drive (volume label "RPI-RP2" on RP2040 boards, "RP2350" on some RP2350
    // boards), and flashing firmware is just copying the .uf2 onto it - the RP2040/RP2350
    // bootrom does the rest and reboots into the new firmware on its own.
    public class RPicoRGH
    {
        public enum Variant
        {
            // The upstream/original build this project was forked from.
            PicoRGH,
            // The fork bundled with J-Runner - the recommended default.
            RPicoRGH
        }

        public bool inUse = false;

        // Fired once when a Program() attempt finishes, success or not, with a short
        // human-readable message. Always raised on a background thread - subscribers
        // touching UI must marshal back with Invoke/BeginInvoke themselves.
        public event Action<bool, string> Completed;

        // RP2040 boards report "RPI-RP2"; some RP2350 boards (Pico 2 and clones) report
        // "RP2350" instead depending on bootrom version. Check both rather than assuming.
        private static readonly string[] BootselVolumeLabels = { "RPI-RP2", "RP2350" };

        private string FirmwarePath(Variant variant)
        {
            string file = variant == Variant.RPicoRGH ? "RPicoRGH.uf2" : "PicoRGH.uf2";
            return Path.Combine(variables.pathforit, @"common\rpicorgh", file);
        }

        // Looks for a Pico currently sitting in BOOTSEL mode. Returns null if none is found
        // (either not plugged in, not in BOOTSEL mode, or Windows hasn't finished mounting
        // the drive yet).
        public static DriveInfo FindBootselDrive()
        {
            try
            {
                return DriveInfo.GetDrives().FirstOrDefault(d =>
                    d.IsReady
                    && d.DriveType == DriveType.Removable
                    && BootselVolumeLabels.Contains(d.VolumeLabel, StringComparer.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                if (variables.debugme) Console.WriteLine(ex.ToString());
                return null;
            }
        }

        // Copies the chosen firmware onto a Pico in BOOTSEL mode. Mirrors DirtyPico's
        // flashSvf() shape (background thread, inUse guard, Console-logged progress) so the
        // two Glitch Chip flashing paths behave consistently from the caller's side.
        public void Program(Variant variant)
        {
            if (inUse) return;

            string firmware = FirmwarePath(variant);
            if (!File.Exists(firmware))
            {
                Console.WriteLine("RPicoRGH: Firmware file not found: {0}", firmware);
                Completed?.Invoke(false, "Firmware file not found: " + firmware);
                return;
            }

            Thread flashThread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    inUse = true;

                    Console.WriteLine("RPicoRGH: Looking for a Pico in BOOTSEL mode...");
                    DriveInfo drive = FindBootselDrive();

                    if (drive == null)
                    {
                        Console.WriteLine("RPicoRGH: No Pico found in BOOTSEL mode.");
                        Console.WriteLine("RPicoRGH: Hold BOOTSEL while plugging the Pico in, then try again.");
                        Completed?.Invoke(false, "No Pico found in BOOTSEL mode. Hold BOOTSEL while plugging it in, then try again.");
                        return;
                    }

                    string destination = Path.Combine(drive.RootDirectory.FullName, Path.GetFileName(firmware));
                    Console.WriteLine("RPicoRGH: Found {0} at {1}", drive.VolumeLabel, drive.Name);
                    Console.WriteLine("RPicoRGH: Copying {0}...", Path.GetFileName(firmware));

                    File.Copy(firmware, destination, true);

                    // The Pico's bootrom unmounts the drive and reboots into the new
                    // firmware itself as soon as the .uf2 write completes - there's nothing
                    // further for J-Runner to do or wait on.
                    Console.WriteLine("RPicoRGH: Flash Successful! The Pico will reboot on its own.");
                    Console.WriteLine("");
                    Completed?.Invoke(true, "Flashed " + Path.GetFileName(firmware) + " successfully. The Pico will reboot on its own.");

                    if (variables.playSuccess)
                    {
                        try
                        {
                            SoundPlayer success = new SoundPlayer(Properties.Resources.chime);
                            if (variables.soundsuccess != "") success.SoundLocation = variables.soundsuccess;
                            success.Play();
                        }
                        catch (Exception ex) { if (variables.debugme) Console.WriteLine(ex.ToString()); }
                    }
                }
                catch (IOException ex)
                {
                    // Most commonly: the bootrom already unmounted the drive mid-copy
                    // (which usually just means the write finished and it rebooted early).
                    Console.WriteLine("RPicoRGH: {0}", ex.Message);
                    Completed?.Invoke(false, ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("RPicoRGH: Flash Failed - {0}", ex.Message);
                    if (variables.debugMode) Console.WriteLine(ex.ToString());
                    Completed?.Invoke(false, "Flash failed - " + ex.Message);
                }
                finally
                {
                    inUse = false;
                }
            }));
            flashThread.Start();
        }
    }
}

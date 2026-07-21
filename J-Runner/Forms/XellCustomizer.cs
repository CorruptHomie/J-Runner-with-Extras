using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net.Configuration;

namespace JRunner
{
    public partial class XellCustomizer : Form
    {
        // Every path here gets the same theme applied when the user hits Save. In
        // practice this is the pair of XeLL templates JRunner actually bakes into every
        // build (common\xell\xell-2f.bin and xell-gggggg.bin) - see moveXell() in
        // Classes/xebuild.cs and creatergh2eccinit() in Nand/ECC.cs, which always copy
        // those files in FRESH on every build/create. Customizing anything else (like a
        // loaded source NAND) has no effect on what actually gets flashed, since the
        // build pipeline never reads XeLL back out of the source image - it always
        // pulls a clean copy from these templates.
        string[] _flashFilePaths;
        bool flashHasEcc;
        byte[] flashData;

        int pagesz = 0x200;
        int pagesz_phys = 0x210;
        int blockType = 0;

        // This is the default "White on Blue" XeLL theme
        Color bgcolor = Color.FromArgb(78, 68, 216), fgcolor = Color.White;

        private void setColor()
        {
            try
            {
                txtXellPreview.BackColor = bgcolor;
                txtXellPreview.ForeColor = fgcolor;
            }
            catch
            {
                txtXellPreview.BackColor = bgcolor = Color.FromArgb(78, 68, 216);
                txtXellPreview.ForeColor = fgcolor = Color.White;
            }
        }

        public XellCustomizer()
        {
            InitializeComponent();
        }

        // Works out whether a given XeLL image has ECC/spare data wrapping it (and, if
        // so, its block type). Recognized full-NAND-dump sizes are handled exactly as
        // before; anything else at least one page long is treated as a bare/standalone
        // XeLL binary (no ECC) rather than being rejected outright, so the two on-disk
        // templates - which are plain binaries, not NAND dumps - can be customized too.
        private bool DetectLayout(byte[] data, out bool hasEcc, out int physPageSize, out int detectedBlockType)
        {
            detectedBlockType = 0;

            if (data.Length == 17301504 || data.Length == 69206016 || data.Length == 1351680)
            {
                hasEcc = true;
                physPageSize = 0x210;
            }
            else if (data.Length == 50331648 || data.Length == 1310720 || data.Length >= pagesz)
            {
                hasEcc = false;
                physPageSize = pagesz;
            }
            else
            {
                hasEcc = false;
                physPageSize = 0;
                return false;
            }

            if (hasEcc)
            {
                byte[] sparedata = data.Skip(0x4400).Take(0x10).ToArray();

                // Block Types
                // 0 = Small block NAND (XSB)
                // 1 = Small block NAND on BB controller (PSB/KSB)
                // 2 = Big block NAND on BB controller (PSB/KSB)
                detectedBlockType = Nand.Nand.identifylayout(sparedata);
            }

            return true;
        }

        public DialogResult InitializeAndShowDialog(string[] flashFilePaths)
        {
            _flashFilePaths = (flashFilePaths ?? new string[0]).Where(File.Exists).Distinct().ToArray();

            if (_flashFilePaths.Length == 0)
            {
                MessageBox.Show("Couldn't find the XeLL template files to customize (common\\xell\\xell-2f.bin / xell-gggggg.bin). Try updating your support files.", "Can't", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return DialogResult.Cancel;
            }

            try
            {
                // Read the first available template just to populate the preview -
                // Save (below) applies the same theme to every path in _flashFilePaths
                // independently, recalculating each one's own ECC as needed.
                flashData = File.ReadAllBytes(_flashFilePaths[0]);
            }
            catch
            {
                Console.WriteLine("Customize XeLL theme error: unable to read input file.");
                return DialogResult.Cancel;
            }

            if (!DetectLayout(flashData, out flashHasEcc, out int physSize, out blockType))
            {
                Console.WriteLine("Customize XeLL theme error: invalid image size.");
                return DialogResult.Cancel;
            }
            pagesz_phys = physSize;

            if (0 != flashData[0x5F])
            {
                bgcolor = Color.FromArgb(BitConverter.ToInt32(flashData.Skip(0x50).Take(0x4).ToArray(), 0));
                fgcolor = Color.FromArgb(BitConverter.ToInt32(flashData.Skip(0x54).Take(0x4).ToArray(), 0));
                chkEnableColours.Checked = true;
            }

            txtCopyrightString.Text = Encoding.ASCII.GetString(flashData.Skip(0x12).Take(0x36).ToArray());

            return this.ShowDialog();
        }

        private void btnSelectBgcolor_Click(object sender, EventArgs e)
        {
            xellColorDialog.ShowDialog();
            bgcolor = xellColorDialog.Color;
            setColor();
        }

        private void btnSelectFgcolor_Click(object sender, EventArgs e)
        {
            xellColorDialog.ShowDialog();
            fgcolor = xellColorDialog.Color;
            setColor();
        }

        // Applies the currently selected theme to a single XeLL image's bytes,
        // recalculating ECC if that particular image needs it, and returns the full,
        // modified file contents ready to write back out.
        private byte[] ApplyThemeToImage(byte[] data, bool hasEcc, int physPageSize, int imgBlockType)
        {
            byte[] flashFirstPage = data.Take(physPageSize).ToArray();

            // If the flash has ECC data, determine the block type so ECC data can be recalculated
            if (hasEcc)
            {
                flashFirstPage = Nand.Nand.unecc(flashFirstPage);
            }

            // Set the colour
            if (chkEnableColours.Checked)
            {
                flashFirstPage[0x5F] = 0x1;

                byte[] bgbytes = BitConverter.GetBytes(bgcolor.ToArgb());
                byte[] fgbytes = BitConverter.GetBytes(fgcolor.ToArgb());

                Buffer.BlockCopy(bgbytes, 0, flashFirstPage, 0x50, 0x4);
                Buffer.BlockCopy(fgbytes, 0, flashFirstPage, 0x54, 0x4);
            }
            else
            {
                flashFirstPage[0x5F] = 0x0;
            }

            // Set the copyright string, we're going to clip the string at 0x47
            // so it isn't any longer than the stock NAND copyright string
            byte[] copyrightStringBytes = Encoding.ASCII.GetBytes(txtCopyrightString.Text);
            Array.Resize(ref copyrightStringBytes, 0x36);
            Buffer.BlockCopy(copyrightStringBytes, 0, flashFirstPage, 0x12, 0x36);
            flashFirstPage[0x47] = 0x0;

            if (hasEcc)
            {
                flashFirstPage = Nand.Nand.addecc_v2(flashFirstPage, true, 0, imgBlockType);
            }

            byte[] result = (byte[])data.Clone();
            Buffer.BlockCopy(flashFirstPage, 0, result, 0, physPageSize);
            return result;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            List<string> succeeded = new List<string>();
            List<string> failed = new List<string>();

            foreach (string path in _flashFilePaths)
            {
                try
                {
                    byte[] data = File.ReadAllBytes(path);

                    if (!DetectLayout(data, out bool hasEcc, out int physPageSize, out int imgBlockType))
                    {
                        failed.Add(Path.GetFileName(path) + " (unrecognized image size)");
                        continue;
                    }

                    byte[] modified = ApplyThemeToImage(data, hasEcc, physPageSize, imgBlockType);
                    File.WriteAllBytes(path, modified);
                    succeeded.Add(Path.GetFileName(path));
                }
                catch (Exception ex)
                {
                    if (variables.debugme) Console.WriteLine(ex.ToString());
                    failed.Add(Path.GetFileName(path) + " (" + ex.Message + ")");
                }
            }

            if (succeeded.Count == 0)
            {
                MessageBox.Show("Failed to save the XeLL theme:\n\n" + string.Join("\n", failed), "Can't", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // btnSave has DialogResult=OK set in the designer, which closes this
                // dialog as soon as this handler returns. On a total failure, cancel
                // that so the user can see the error and try again instead of the
                // dialog silently closing on them with nothing actually saved.
                this.DialogResult = DialogResult.None;
                return;
            }

            string message = "Theme saved to: " + string.Join(", ", succeeded);
            if (failed.Count > 0) message += "\n\nFailed: " + string.Join(", ", failed);
            MessageBox.Show(message, "XeLL Theme", MessageBoxButtons.OK, failed.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        private void chkEnableColours_CheckedChanged(object sender, EventArgs e)
        {
            btnSelectBgcolor.Enabled = chkEnableColours.Checked;
            btnSelectFgcolor.Enabled = chkEnableColours.Checked;

            if (chkEnableColours.Checked)
            {
                setColor();
            }
            else
            {
                txtXellPreview.ForeColor = Color.White;
                txtXellPreview.BackColor = Color.FromArgb(78, 68, 216);
            }
        }
    }
}

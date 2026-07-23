using System;
using System.IO;
using System.Threading;

namespace JRunner
{
    public class rgh3build
    {
        private string filename;

        public void injectECC(string eccpath, string cpuKey, bool patchSMC = true)
        {
            if (patchSMC)
            {
                Console.WriteLine("Injecting glitch3 ECC...");
            }
            else
            {
                Console.WriteLine("Injecting glitch3 CB_X...");
            }

            Thread.Sleep(1000); // Important

            Classes.RGH_CONVERT_ERROR result;
            try
            {
                result = Classes.RGH2to3.ConvertRgh2ToRgh3(eccpath, variables.filename1, cpuKey, variables.filename1, patchSMC);
            }
            catch (Exception ex)
            {
                if (variables.debugMode) Console.WriteLine(ex.ToString());
                Console.WriteLine("Failed: An unexpected error occurred while converting to RGH3 (" + ex.Message + ")");
                Console.WriteLine("");
                return;
            }

            if (result != Classes.RGH_CONVERT_ERROR.ERROR_NONE)
            {
                Console.WriteLine("Failed: " + DescribeRgh3Error(result));
                Console.WriteLine("");
                return;
            }

            MainForm.mainForm.nand_init();
        }

        // Maps the specific RGH_CONVERT_ERROR reason to an accurate message. Previously the
        // conversion's return value was discarded entirely and every failure - wrong CPU key,
        // wrong image size, missing bootloaders, etc. - showed the same generic "already RGH3,
        // or an unsupported image type" message, which is only actually true for one of these.
        private static string DescribeRgh3Error(Classes.RGH_CONVERT_ERROR error)
        {
            switch (error)
            {
                case Classes.RGH_CONVERT_ERROR.ERROR_INVALID_ECC_SIZE:
                    return "The RGH3 ECC file is not a recognized size";
                case Classes.RGH_CONVERT_ERROR.ERROR_INVALID_ECC_BOOTLOADERS:
                    return "Could not find valid CB_A/CB_B bootloaders in the RGH3 ECC file";
                case Classes.RGH_CONVERT_ERROR.ERROR_INVALID_FLASH_LENGTH:
                    return "The NAND image is not a recognized size";
                case Classes.RGH_CONVERT_ERROR.ERROR_INVALID_FLASH_BLOCK_TYPE:
                    return "Could not determine the NAND's ECC block type";
                case Classes.RGH_CONVERT_ERROR.ERROR_XELL_NOT_FOUND:
                    return "The image is either already RGH3, or an unsupported image type";
                case Classes.RGH_CONVERT_ERROR.ERROR_FLASH_CBB_DECRYPT_FAILED:
                    return "Failed to decrypt CB_B - double check the CPU key";
                case Classes.RGH_CONVERT_ERROR.ERROR_INVALID_FLASH_BOOTLOADERS:
                    return "Could not find valid CB_A/CB_B bootloaders in the built image - this dashboard build's CB layout isn't supported for RGH3 conversion";
                default:
                    return "Unknown error (" + error + ")";
            }
        }

        public void create(string board, string cpuKey, bool sequenced = true)
        {
            Console.WriteLine("Converting Image to RGH3...");
            Thread.Sleep(1000); // Important

            string ecc;
            string mhz = "";
            if (MainForm.mainForm.xPanel.getRgh3Mhz() != "27") mhz = "_" + MainForm.mainForm.xPanel.getRgh3Mhz();

            if (board == "Corona 16MB") ecc = variables.RGH3_corona;
            else if (board == "Corona 4GB") ecc = variables.RGH3_corona4GB;
            else if (board == "Trinity") ecc = variables.RGH3_trinity;
            else if (board == "Jasper 16MB" || board == "Jasper SB") ecc = variables.RGH3_jasper + mhz;
            else if (board == "Jasper 256MB" || board == "Jasper 512MB") ecc = variables.RGH3_jasperBB + mhz;
            else if (board == "Falcon") ecc = variables.RGH3_falcon + mhz;
            else
            {
                Console.WriteLine("RGH3 Failed: Unsupported Console Type");
                if (sequenced)
                {
                    variables.xefinished = true;
                    MainForm.mainForm.xPanel.xeExitActual();
                }
                return;
            }

            if (sequenced) filename = Path.Combine(variables.xefolder, variables.nandflash);
            else filename = variables.filename1;

            Classes.RGH_CONVERT_ERROR result;
            try
            {
                result = Classes.RGH2to3.ConvertRgh2ToRgh3(Path.Combine(variables.pathforit, "common", "ECC", ecc + ".ecc"), filename, cpuKey, filename);
            }
            catch (Exception ex)
            {
                if (variables.debugMode) Console.WriteLine(ex.ToString());
                Console.WriteLine("Failed: An unexpected error occurred while converting to RGH3 (" + ex.Message + ")");
                Console.WriteLine("");
                return;
            }

            if (result != Classes.RGH_CONVERT_ERROR.ERROR_NONE)
            {
                Console.WriteLine("Failed: " + DescribeRgh3Error(result));
                Console.WriteLine("");
                return;
            }

            Console.WriteLine("RGH3 Conversion Finished!");

            if (sequenced)
            {
                variables.xefinished = true;
                MainForm.mainForm.xPanel.xeExitActual();
            }
            else
            {
                MainForm.mainForm.nand_init();
            }
        }
    }
}

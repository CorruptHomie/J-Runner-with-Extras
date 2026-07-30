using System;
using System.Diagnostics;
using System.Net;
using System.Windows.Forms;

namespace JRunner
{
    public partial class UpdUI : Form
    {
        public UpdUI()
        {
            InitializeComponent();
            UI.Theme.ApplyTheme(this);
            UpdateWizard.Cancelling += WizardCancelled;
            UpdateWizard.Finished += WizardFinished;
        }

        private void WizardCancelled(object sender, EventArgs e)
        {
            Upd.cancel();
        }

        private void WizardFinished(object sender, EventArgs e)
        {
            if (UpdateWizard.SelectedPage == FailedPage)
            {
                Application.ExitThread();
                Application.Exit();
            }
            else
            {
                Process.Start("JRunner.exe");
                Application.ExitThread();
                Application.Exit();
            }
        }

        public void updateProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            updateProgressBar.BeginInvoke((Action)(() => updateProgressBar.Value = e.ProgressPercentage));
        }

        public void installMode()
        {
            updateProgressBar.BeginInvoke((Action)(() =>
            {
                updateProgressBar.Style = ProgressBarStyle.Marquee;
                UpdatePage.Text = "Installing Update...";
                UpdatePage.AllowCancel = false;
            }));
        }

        public void showSuccess()
        {
            updateProgressBar.BeginInvoke((Action)(() => UpdateWizard.NextPage(SuccessPage)));
        }

        public void showFailed()
        {
            updateProgressBar.BeginInvoke((Action)(() =>
            {
                FailedReason.Text = Upd.failedReason;
                UpdateWizard.NextPage(FailedPage);
            }));
        }

        private void DownloadButton_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/ScallywagDude/J-Runner-Premium/releases/latest");
        }
    }
}

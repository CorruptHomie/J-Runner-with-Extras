using System;
using System.Windows.Forms;

namespace JRunner
{
    public partial class UpdateAvailable : Form
    {
        public UpdateAvailable()
        {
            InitializeComponent();
            UI.Theme.ApplyTheme(this);
            lblMessage.Text = string.IsNullOrEmpty(Upd.pendingVersion)
                ? "A new version is available.\n\nWould you like to view what's new and install it?"
                : "Version " + Upd.pendingVersion + " is available.\n\nWould you like to view what's new and install it?";
            SuccessWizard.Cancelling += WizardCancelled;
            SuccessWizard.Finished += WizardFinished;
        }

        private void UpdateSuccess_Load(object sender, EventArgs e)
        {
            // Make sure we're on top
            bool top = TopMost;
            TopMost = true; // Bring to front
            TopMost = top; // Set it back
            Activate();
        }

        private void WizardCancelled(object sender, EventArgs e) // No
        {
            Upd.allowUpdate = false;
            Upd.cancelSource.Cancel();
        }

        private void WizardFinished(object sender, EventArgs e) // Yes
        {
            Upd.allowUpdate = true;
            Upd.cancelSource.Cancel();
            this.Close();
        }
    }
}

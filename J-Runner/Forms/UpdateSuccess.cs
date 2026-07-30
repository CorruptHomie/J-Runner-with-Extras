using System;
using System.Windows.Forms;

namespace JRunner
{
    public partial class UpdateSuccess : Form
    {
        public UpdateSuccess()
        {
            InitializeComponent();
            UI.Theme.ApplyTheme(this);
            SuccessWizard.Cancelling += WizardCancelled;
            SuccessWizard.Finished += WizardFinished;
        }

        private void WizardCancelled(object sender, EventArgs e)
        {
            Program.restart();
        }

        private void WizardFinished(object sender, EventArgs e)
        {
            Program.restart();
        }
    }
}

using System.Windows.Forms;

namespace JRunner
{
    public partial class UpdateCheck : Form
    {
        public UpdateCheck()
        {
            InitializeComponent();
            UI.Theme.ApplyTheme(this);
        }
    }
}

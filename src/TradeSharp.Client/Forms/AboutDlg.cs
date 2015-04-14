using System.Windows.Forms;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class AboutDlg : Form
    {
        public string NameProgram
        {
            get; set;
        }

        public AboutDlg()
        {
            InitializeComponent();
            Localizer.LocalizeControl(this);
        }

        private void AboutDlg_Load(object sender, System.EventArgs e)
        {
            aboutCtrl1.NameProgram = NameProgram;
        }
    }
}

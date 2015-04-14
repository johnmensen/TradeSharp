using System.Windows.Forms;

namespace TradeSharp.Client.Forms
{
    public partial class ResultDisplaySettingsForm : Form
    {
        public bool ShowEnters { get { return cbShowEnters.Checked; } }

        public bool ShowExits { get { return cbShowExits.Checked; } }

        public bool ShowDetailedEnters { get { return cbDetailedEnters.Checked; } }
        
        public bool ShowDetailedExits { get { return cbDetailedExits.Checked; } }

        public bool RemoveOldMarkers { get { return cbRemoveOld.Checked; } }
        
        public bool ShowMarkers
        {
            get { return cbShowEnters.Checked || cbShowExits.Checked; }
        }

        public ResultDisplaySettingsForm()
        {
            InitializeComponent();
        }
    }
}

using System.Windows.Forms;

namespace TradeSharp.Util.Forms
{
    public partial class TickerUpdateDialog : Form
    {
        public bool acceptAll = false;

        private string customVolume;
        private int[] customVolumeInt;

        public string CustomVolume
        {
            get
            {
                customVolume = txtbxCurentCustomValues.Text;
                return customVolume;
            }
            set
            {
                customVolume = value;
                customVolumeInt = value.ToIntArrayUniform();
            }
        }


        public TickerUpdateDialog()
        {
            InitializeComponent();
        }


        protected override void OnShown(System.EventArgs e)
        {
            base.OnShown(e);
            txtbxCurentCustomValues.Text = customVolume;
        }

        private void BtnAcceptClick(object sender, System.EventArgs e)
        {
            acceptAll = false;
        }

        private void BtnAcceptAllClick(object sender, System.EventArgs e)
        {
            acceptAll = true;
        }
    }
}

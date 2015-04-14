using System.Windows.Forms;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class CloseTargetDlg : Form
    {
        public enum UnitType { Points = 0, Counter = 1, Price = 2 }

        public float TargetNumber { get; private set; }

        public UnitType Units
        {
            get { return (UnitType) cbUnit.SelectedIndex; }
        }

        public CloseTargetDlg()
        {
            InitializeComponent();
        }

        public CloseTargetDlg(string currency, float parityPrice) : this()
        {
            cbUnit.Items.Add("Пункты");
            cbUnit.Items.Add(currency);
            cbUnit.Items.Add("Цена");
            cbUnit.SelectedIndex = 0;
            lblPrice.Text = parityPrice.ToStringUniformPriceFormat(true);
        }

        private void BtnAcceptClick(object sender, System.EventArgs e)
        {
            var val = tbTarget.Text.Replace(",", ".").ToFloatUniformSafe();
            if (!val.HasValue)
            {
                MessageBox.Show(
                    Localizer.GetString("MessageInputIncorrect") + 
                        ". " + Localizer.GetString("MessageCorrectFormatExamples") + ": 0, 50, -125.30",
                        Localizer.GetString("TitleError"), 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            TargetNumber = val.Value;
            DialogResult = DialogResult.OK;
        }

        private void LblPriceLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var val = lblPrice.Text.Trim().ToDecimalUniformSafe();
            if (val.HasValue)
                tbTarget.Text = lblPrice.Text;
        }
    }
}

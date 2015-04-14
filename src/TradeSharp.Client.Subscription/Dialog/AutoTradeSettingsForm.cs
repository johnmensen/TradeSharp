using System.Windows.Forms;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.Subscription.Dialog
{
    public partial class AutoTradeSettingsForm : Form
    {
        public AutoTradeSettings sets;

        public AutoTradeSettingsForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
            tradeSettings.ButtonCancelClicked += () =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };
        }

        public AutoTradeSettingsForm(AutoTradeSettings sets)
            : this()
        {
            this.sets = sets;
            tradeSettings.DataContext = sets.MakeCopy();
        }

        private void TradeSettingsButtonOkClicked()
        {
            var controlSets = tradeSettings.DataContext;
            if (controlSets == null) return;
            sets = controlSets;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void AutoTradeSettingsFormHelpRequested(object sender, HelpEventArgs hlpevent)
        {
            // !!
            //hlpevent.Handled = true;
        }
    }
}

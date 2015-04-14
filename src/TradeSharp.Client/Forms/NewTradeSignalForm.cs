using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Entity;
using TradeSharp.Client.BL;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class NewTradeSignalForm : Form
    {
        private const bool RequireRealAccountForTradeSignals = false;

        public NewTradeSignalForm()
        {
            InitializeComponent();
        }

        private void TbFeeSignalMonthTextChanged(object sender, EventArgs e)
        {
            var srcTextBox = (TextBox)sender;
            var val = srcTextBox.Text.ToDecimalUniformSafe();
            if (val == null) return;

            var targetVal = PaidService.GetMonthFeeFromDailyFee(val.Value, srcTextBox == tbFeeSignalMonth);
            var targetBox = srcTextBox == tbFeeSignalMonth ? tbFeeSignalDay : tbFeeSignalMonth;
            targetBox.TextChanged -= TbFeeSignalMonthTextChanged;
            targetBox.Text = targetVal.ToStringUniformMoneyFormat(true);
            targetBox.TextChanged += TbFeeSignalMonthTextChanged;
        }

        private void BtnMakeSignalServiceClick(object sender, EventArgs e)
        {
            // проверить введенные данные
            var listError = new List<string>();
            var feeDay = tbFeeSignalDay.Text.ToDecimalUniformSafe();
            if (!feeDay.HasValue)
                listError.Add(Localizer.GetString("MessageCannotRecognizeSignalComissionString") + ": 0 0.25 5 12.50");
            if (listError.Count > 0)
            {
                MessageBox.Show(string.Join(Environment.NewLine, listError), 
                    Localizer.GetString("TitleError"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // проверить наличие реального счета, если их несколько - дать выбор
            var accountSelected = UserServiceRegistrator.SelectTerminalUserAccount(RequireRealAccountForTradeSignals);
            if (accountSelected == null) return;
            
            var service = new PaidService
            {
                FixedPrice = feeDay.Value,
                Comment = nameTextBox.Text,
                ServiceType = PaidServiceType.Signals,
                AccountId = accountSelected.ID
            };
            if (UserServiceRegistrator.RegisterOrUpdateService(service))
                DialogResult = DialogResult.OK;
        }

        private void CbEnableTradeSignalsCheckStateChanged(object sender, EventArgs e)
        {
            panelSignalsDetail.Enabled = cbEnableTradeSignals.Checked;
        }

        private void CbPaidSignalsCheckedChanged(object sender, EventArgs e)
        {
            tbFeeSignalDay.Enabled = cbPaidSignals.Checked;
            tbFeeSignalMonth.Enabled = cbPaidSignals.Checked;
            if (!cbPaidSignals.Checked)
            {
                tbFeeSignalDay.Text = "0";
                tbFeeSignalMonth.Text = "0";
            }
        }
    }
}

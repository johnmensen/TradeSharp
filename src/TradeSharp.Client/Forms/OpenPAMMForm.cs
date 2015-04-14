using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class OpenPAMMForm : Form
    {
        public bool EnablePAMM
        {
            get { return cbEnablePAMM.Checked; }
        }

        public List<PaidServiceRate> ServiceRates { get; private set; }

        public OpenPAMMForm()
        {
            InitializeComponent();
        }

        public OpenPAMMForm(string accountCurx) : this()
        {
            lblDepoCurrency.Text = accountCurx;
        }

        private void cbMoreRate_CheckedChanged(object sender, EventArgs e)
        {
            tbLargeDepo.Enabled = cbMoreRate.Checked;
            tbPercentMore.Enabled = cbMoreRate.Checked;
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            if (EnablePAMM)
            {
                // посчитать процент вознаграждения и заполнить список рейтов
                var percents = new List<decimal?>
                    {
                        tbPercent.Text.Replace(" ", "").ToDecimalUniformSafe(),
                    };
                int? depoSize = null;
                if (cbMoreRate.Checked)
                {
                    percents.Add(tbPercentMore.Text.Replace(" ", "").ToDecimalUniformSafe());
                    depoSize = tbLargeDepo.Text.Replace(" ", "").ToIntSafe();
                }

                var errorList = new List<string>();

                if (percents.Any(p => p == null || p.Value <= 0 || p.Value >= 100))
                    errorList.Add(Localizer.GetString("MessagePAMMFeeComissionWrong") +
                        ":\n5 10 7.50 22.25");

                if (cbMoreRate.Checked && (depoSize == null || depoSize < 1))
                    errorList.Add(Localizer.GetString("MessagePAMMFeeDepositWrong"));

                if (errorList.Count > 0)
                {
                    MessageBox.Show(string.Join(Environment.NewLine, errorList),
                                    Localizer.GetString("TitleError"), 
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var rates = new List<PaidServiceRate>
                    {
                        new PaidServiceRate
                            {
                                Amount = percents[0].Value,
                                RateType = PaidServiceRate.ServiceRateType.Percent,
                                UserBalance = 0
                            }
                    };
                if (cbMoreRate.Checked)
                    rates.Add(new PaidServiceRate
                                    {
                                        Amount = percents[1].Value,
                                        RateType = PaidServiceRate.ServiceRateType.Percent,
                                        UserBalance = depoSize.Value
                                    });
                ServiceRates = rates;
            }
            DialogResult = DialogResult.OK;
        }
    }
}

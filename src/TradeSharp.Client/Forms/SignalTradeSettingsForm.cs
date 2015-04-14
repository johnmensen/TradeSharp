using System;
using System.Windows.Forms;
using TradeSharp.Client.BL;
using TradeSharp.Client.Subscription.Model;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class SignalTradeSettingsForm : Form
    {
        private readonly TradeSignalSubscriptionSettings sets;

        public SignalTradeSettingsForm()
        {
            InitializeComponent();
        }

        public SignalTradeSettingsForm(TradeSignalSubscriptionSettings sets)
        {
            InitializeComponent();
            this.sets = sets;
            // заполнить поля
            cbTradeAuto.Checked = sets.AutoTrade;
            tbMaxLeverage.Text = sets.MaxLeverage.ToStringUniform();
            tbLeveragePercent.Text = sets.PercentLeverage.ToStringUniform();
            cbLockOrders.Checked = sets.HedgingOrdersEnabled;
            tbMagic.Text = sets.Magic.ToString();
            tbFixVolume.Text = sets.FixedVolume.HasValue
                                   ? sets.FixedVolume.Value.ToString()
                                   : "10 000";
            cbFixVolume.Checked = sets.FixedVolume.HasValue && sets.FixedVolume.Value > 0;
            cbRoundType.SelectedIndex =
                sets.VolumeRound == VolumeRoundType.Ближайшее
                    ? 0 : sets.VolumeRound == VolumeRoundType.Вниз ? 1 : 2;
            tbMinVolume.Text = sets.MinVolume.ToString();
            tbStepVolume.Text = sets.StepVolume.ToString();
        }

        private void CbFixVolumeCheckedChanged(object sender, EventArgs e)
        {
            tbFixVolume.Enabled = cbFixVolume.Checked;
        }

        private void BtnAcceptClick(object sender, EventArgs e)
        {
            // обновить поля объекта настроек
            sets.AutoTrade = cbTradeAuto.Checked;
            sets.MaxLeverage = tbMaxLeverage.Text.ToDecimalUniformSafe() ?? 5;
            sets.PercentLeverage = tbLeveragePercent.Text.ToDecimalUniformSafe() ?? 100;
            sets.HedgingOrdersEnabled = cbLockOrders.Checked;
            sets.Magic = tbMagic.Text.ToIntSafe() ?? 0;
            sets.FixedVolume = cbFixVolume.Checked ? tbFixVolume.Text.ToIntSafe() : null;
            sets.VolumeRound = cbRoundType.SelectedIndex == 0
                                   ? VolumeRoundType.Ближайшее
                                   : cbRoundType.SelectedIndex == 1
                                         ? VolumeRoundType.Вниз
                                         : VolumeRoundType.Вверх;
            sets.MinVolume = tbMinVolume.Text.ToIntSafe() ?? 10000;
            sets.StepVolume = tbStepVolume.Text.ToIntSafe() ?? 10000;
            DialogResult = DialogResult.OK;
        }

        private void LabelMaxLevClick(object sender, EventArgs e)
        {
            // показать раздел справки по авто-торговле
            HelpManager.Instance.ShowHelp(HelpManager.KeySignalAutoTrade);
        }
    }
}

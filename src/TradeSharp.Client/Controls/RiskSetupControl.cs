using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Entity;
using TradeSharp.Client.BL;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Client.Controls
{
    public partial class RiskSetupControl : UserControl
    {
        private bool initializingCompleted;

        private bool balanceChangedByUser = false;

        public Action settingsChanged;

        public decimal? LeveragePerOrder
        {
            get 
            { 
                var numDeals = tbDealByTickerCount.Text.ToIntSafe();
                if (!numDeals.HasValue || numDeals <= 0) return null;
                var numTickers = tbTickerCount.Text.ToIntSafe();
                if (!numTickers.HasValue || numTickers <= 0) return null;
                var levSum = tbLeverage.Text.ToDecimalUniformSafe();
                if (!levSum.HasValue || levSum <= 0) return null;
                return levSum.Value/numDeals.Value/numTickers.Value;
            }

            set 
            { 
                if (value == null) return;
                tbOrderLeverage.Text = value.Value.ToStringUniform(3);
            }
        }

        public string Ticker
        {
            get { return cbTicker.Text; }
            set { cbTicker.Text = value; }
        }

        public RiskSetupControl()
        {
            Logger.Debug("Start");
            InitializeComponent();
        }

        private void RiskSetupControlSizeChanged(object sender, EventArgs e)
        {
            panelLeverage.Left = panelMiddle.Width / 2 - panelLeverage.Width / 2;
        }

        private void BtnCalcLeverageClick(object sender, EventArgs e)
        {
            RecalcSingleDealLeverage();
        }

        private void RecalcSingleDealLeverage()
        {
            var tickersCount = tbTickerCount.Text.ToIntSafe() ?? 1;
            var dealsCount = tbDealByTickerCount.Text.ToIntSafe() ?? 1;
            var leverageSum = tbLeverage.Text.ToDecimalUniformSafe() ?? 10;

            tbOrderLeverage.Text = (leverageSum/tickersCount/dealsCount).ToStringUniform(3);
            CalcDealVolume();
        }

        private void CalcDealVolume()
        {
            var account = AccountStatus.Instance.AccountData ?? new Account
                {
                    Equity = 10000,
                    Balance = 10000,
                    Currency = "USD",
                    Group = "Demo"
                };

            var balance = tbBalance.Text.Replace(" ", "").ToFloatUniformSafe() ?? 10000;
            var leverage = tbOrderLeverage.Text.ToFloatUniformSafe() ?? 1;

            var volumeDepo = balance * leverage;
            // пересчитать в базовую валюту
            string errorStr;
            var volmBase = DalSpot.Instance.ConvertToTargetCurrency(cbTicker.Text,
                true, account.Currency, volumeDepo, QuoteStorage.Instance.ReceiveAllData(), out errorStr);
            if (!string.IsNullOrEmpty(errorStr))
                Logger.Error("CalcDealVolume() - ошибка перевода в валюту депо " + cbTicker.Text + " " + errorStr);
            
            tbResultedVolume.Text = volmBase.HasValue ? volmBase.Value.ToStringUniformMoneyFormat(false) : "-";
            // округлить объем
            var lotSize = DalSpot.Instance.GetMinStepLot(cbTicker.Text, account.Group);
            tbResultRounded.Text = volmBase.HasValue
                                       ? MarketOrder.RoundDealVolume((int) Math.Round(volmBase.Value),
                                                                     ((EnumItem<VolumeRoundType>) cbRoundVolume.SelectedItem).Value,
                                                                     lotSize.a, lotSize.b).ToStringUniformMoneyFormat()
                                       : "-";
        }

        private void RiskSetupControlLoad(object sender, EventArgs e)
        {
            if (DesignMode) // может падла валиться в дизайнере
                return;
            cbTicker.Initialize();
            if(cbTicker.Items.Count > 0)
               cbTicker.SelectedIndex = 0;

            cbRoundVolume.DataSource = EnumItem<VolumeRoundType>.items;
            cbRoundVolume.SelectedIndex = 0;

            var account = AccountStatus.Instance.AccountData ?? new Account
                              {
                                  Balance = 10000,
                                  Equity = 10000,
                                  Currency = "USD",
                                  Group = "Demo"
                              };
            tbBalance.Text = account.Equity.ToStringUniformMoneyFormat(false);
            lblCurrency.Text = account.Currency;

            initializingCompleted = true;
            balanceUpdateTimer.Enabled = true;
        }

        public void ShowUserSettings()
        {
            // плечо сделки
            LeveragePerOrder = UserSettings.Instance.DealLeverage;
            // тип округления
            cbRoundVolume.SelectedIndex =
                cbRoundVolume.Items.Cast<EnumItem<VolumeRoundType>>()
                             .Select(i => i.Value)
                             .IndexOf(UserSettings.Instance.VolumeRoundType);
            tbDealByTickerCount.Text = UserSettings.Instance.DealByTickerCount.ToString();
            tbTickerCount.Text = UserSettings.Instance.TickerTradedCount.ToString();
            tbLeverage.Text = UserSettings.Instance.MaxLeverageTotal.ToStringUniform(3);
            cbUseLeverageAsDefault.Checked = UserSettings.Instance.UseLeverageByDefault;
        }

        public void ApplyToUserSettings()
        {
            UserSettings.Instance.DealLeverage = LeveragePerOrder;
            UserSettings.Instance.VolumeRoundType = ((EnumItem<VolumeRoundType>) cbRoundVolume.SelectedItem).Value;
            UserSettings.Instance.DealByTickerCount = tbDealByTickerCount.Text.ToIntSafe() ?? 5;
            UserSettings.Instance.TickerTradedCount = tbTickerCount.Text.ToIntSafe() ?? 4;
            UserSettings.Instance.MaxLeverageTotal = tbLeverage.Text.ToDecimalUniformSafe() ?? 20;
            UserSettings.Instance.UseLeverageByDefault = cbUseLeverageAsDefault.Checked;
        }
        
        /// <summary>
        /// при изменении значений сказать родительской форме, что настройки изменились
        /// </summary>
        private void NotifyParentOnChange()
        {
            if (!initializingCompleted) return;
            var isValid = LeveragePerOrder.HasValue;
            if (!isValid) return;

            if (settingsChanged != null) settingsChanged();
            CalcDealVolume();
        }

        private void TbTickerCountTextChanged(object sender, EventArgs e)
        {
            //NotifyParentOnChange();
            var isValid = LeveragePerOrder.HasValue;
            if (!isValid) return;
            RecalcSingleDealLeverage();
        }

        private void CbTickerSelectedIndexChanged(object sender, EventArgs e)
        {
            if (initializingCompleted)
                CalcDealVolume();
            if (sender == cbRoundVolume)
                NotifyParentOnChange();
        }

        private void TbOrderLeverageTextChanged(object sender, EventArgs e)
        {
            NotifyParentOnChange();
        }

        private void CbUseLeverageAsDefaultCheckedChanged(object sender, EventArgs e)
        {
            NotifyParentOnChange();
        }

        private void TbBalanceKeyDown(object sender, KeyEventArgs e)
        {
            if (balanceChangedByUser) return;
            balanceChangedByUser = true;
            tbBalance.ForeColor = SystemColors.WindowText;
        }

        private void BalanceUpdateTimerTick(object sender, EventArgs e)
        {
            Invoke(new Action(() =>
                {
                    if (balanceChangedByUser) return;
                    var acData = AccountStatus.Instance.AccountData;
                    if (acData == null) return;
                    tbBalance.Text = acData.Equity.ToStringUniformMoneyFormat(false);
                }));
        }
    }
}

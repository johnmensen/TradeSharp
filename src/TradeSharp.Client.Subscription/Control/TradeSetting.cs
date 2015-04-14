using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TradeSharp.Client.Util;
using TradeSharp.Util;
using System.Windows.Forms;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Client.Subscription.Control
{
    public partial class TradeSetting : UserControl
    {
        public event Action ButtonOkClicked;

        public event Action ButtonCancelClicked;

        private AutoTradeSettings dataContext;

        [Browsable(false)]
        public AutoTradeSettings DataContext
        {
            get
            {
                var result = dataContext ?? new AutoTradeSettings();
                if (DesignMode) return result;
                var errorList = new List<string>();

                var fixedVolume = tbFixedVolume.Text.Replace(" ", "").ToIntSafe();
                if (fixedVolume <= 0)
                    errorList.Add("\"Фиксированный объем\" должно быть целым числом, большим нуля");

                var maxLeverage = tbMaxLeverage.Text.Replace(" ", "").ToDecimalUniformSafe();
                if (/*maxLeverage == null || */maxLeverage <= 0) 
                    errorList.Add("\"Макс. плечо\" должно быть дробным числом, большим нуля");
                
                var minVolume = tbMinVolume.Text.Replace(" ", "").ToIntSafe();
                if (/*minVolume == null || */minVolume <= 0) 
                    errorList.Add("\"Мин. объем\" должно быть целым числом, большим нуля");

                var maxVolume = tbMaxVolume.Text.Replace(" ", "").ToIntSafe();
                if (/*maxVolume == null || */maxVolume <= 0)
                    errorList.Add("\"Макс. объем\" должно быть целым числом, большим нуля");

                var percentLeverage = tbPercentageLeverage.Text.Replace(" ", "").ToDecimalUniformSafe();
                if (percentLeverage == null || percentLeverage <= 0) 
                    errorList.Add("\"Процент плеча\" должно быть дробным числом, большим нуля");

                var stepVolume = tbStepVolume.Text.Replace(" ", "").ToIntSafe();
                if (/*stepVolume == null || */stepVolume <= 0) 
                    errorList.Add("\"Шаг объема\" должно быть целым числом, большим нуля");
                
                if (errorList.Count > 0)
                {
                    MessageBox.Show(string.Format("Некорректно введённые данные:\n{0}", String.Join("\n", errorList.ToArray())));
                    return null;
                }

                result.FixedVolume = fixedVolume;
                result.MaxLeverage = (float?)maxLeverage;
                result.MinVolume = minVolume;
                result.MaxVolume = maxVolume;
                result.PercentLeverage = (float)(percentLeverage ?? 100);
                result.StepVolume = stepVolume;

                result.TradeAuto = tbAutoTrade.Checked;
                result.HedgingOrdersEnabled = cbHedgeOrders.Checked;
                result.VolumeRound = ((EnumItem<VolumeRoundType>) cbRoundingVolume.SelectedItem).Value;

                if (cbAccount.SelectedIndex >= 0)
                    result.TargetAccount = ((Account) cbAccount.SelectedItem).ID;

                return result;
            }
            set
            {
                dataContext = value;
                tbFixedVolume.Text = dataContext.FixedVolume == null ? "" 
                    : dataContext.FixedVolume.Value.ToStringUniformMoneyFormat();
                tbMaxLeverage.Text = 
                    dataContext.MaxLeverage.HasValue ? dataContext.MaxLeverage.Value.ToStringUniform() : "";

                tbMinVolume.Text = dataContext.MinVolume.HasValue 
                    ? dataContext.MinVolume.Value.ToStringUniformMoneyFormat() : "";
                tbMaxVolume.Text = dataContext.MaxVolume.HasValue
                    ? dataContext.MaxVolume.Value.ToStringUniformMoneyFormat() : "";

                tbPercentageLeverage.Text = dataContext.PercentLeverage.ToStringUniform();
                tbStepVolume.Text = dataContext.StepVolume.HasValue
                                        ? dataContext.StepVolume.Value.ToStringUniformMoneyFormat()
                                        : "";

                tbAutoTrade.Checked = dataContext.TradeAuto;
                cbHedgeOrders.Checked = dataContext.HedgingOrdersEnabled ?? true;
                cbRoundingVolume.SelectedItem = dataContext.VolumeRound ??
                                                (object) EnumItem<VolumeRoundType>.items.FirstOrDefault(i => i.Value == VolumeRoundType.Ближайшее);
            }
        }

        public TradeSetting()
        {
            InitializeComponent();
            cbRoundingVolume.DataSource = EnumItem<VolumeRoundType>.items;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            if (ButtonOkClicked != null)
                ButtonOkClicked();
        }

        private void ButtonCancelClick(object sender, EventArgs e)
        {
            if (ButtonCancelClicked != null)
                ButtonCancelClicked();
        }

        private void TradeSetting_Load(object sender, EventArgs e)
        {
            // заполнить список выбора счетов
            var accounts = SubscriptionModel.Instance.GetUserOwnedAccounts();
            if (accounts != null && accounts.Length > 0)
            {
                cbAccount.DataSource = accounts;
                cbAccount.SelectedIndex = 0;
            }
        }
    }
}

using System;
using System.Linq;
using System.Windows.Forms;
using TradeSharp.Client.Subscription.Dialog;
using TradeSharp.Client.Subscription.Model;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace TradeSharp.Client.Subscription.Control
{
    // краткая однострочная информация о топе
    public partial class TopSubscriptionControl : UserControl
    {
        public Action<SubscriptionControl.ActivePage> PageTargeted;

        private TopPortfolio portfolio;
        public TopPortfolio Portfolio
        {
            get { return portfolio; }
            set
            {
                portfolio = value;
                if (portfolio == null)
                {
                    gotoStrategiesButton.Enabled = false;
                    setupButton.Enabled = false;
                    unsubscribeButton.Enabled = false;
                    return;
                }
                nameLabel.Text = string.Format("{0} {1}", Localizer.GetString("TitleTOP"), portfolio.ParticipantCount);
                titleLabel.Text = portfolio.Name;
                gotoStrategiesButton.Enabled = true;
                setupButton.Enabled = true;
                unsubscribeButton.Enabled = true;

                // copied from TopPortfolioControl
                // подсказка в виде формулы и расшифровки
                var expressionToolTip = portfolio.Criteria + "\n";
                var resolver = new ExpressionResolver(portfolio.Criteria);
                foreach (var var in resolver.GetVariableNames())
                {
                    var varName = var;
                    var field = PerformerStatField.fields.FirstOrDefault(f =>
                        !string.IsNullOrEmpty(f.ExpressionParamName) && f.ExpressionParamName.Equals(
                        varName, StringComparison.OrdinalIgnoreCase));
                    if (field == null)
                        continue;
                    expressionToolTip += "\n" + field.ExpressionParamName + " - " + field.ExpressionParamTitle;
                }
                toolTip.SetToolTip(nameLabel, expressionToolTip);
            }
        }

        public TopSubscriptionControl()
        {
            InitializeComponent();

            gotoStrategiesButton.Click += (o, args) =>
                {
                    if (PageTargeted != null)
                        PageTargeted(SubscriptionControl.ActivePage.Signals);
                };
        }

        private void UnsubscribeButtonClick(object sender, EventArgs e)
        {
            var result = MessageBox.Show(this, "Отписаться от всех участников портфеля", "Вопрос",
                                         MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                                         MessageBoxDefaultButton.Button3);
            if (result == DialogResult.Cancel)
                return;
            RequestStatus status;
            try
            {
                status = AccountModel.Instance.ServerProxy.UnsubscribePortfolio(
                    CurrentProtectedContext.Instance.MakeProtectedContext(), AccountModel.Instance.GetUserLogin(),
                    false,
                    result == DialogResult.Yes);
            }
            catch (Exception ex)
            {
                //4 debug
                MessageBox.Show(this, "Операция выполнена с ошибкой:" + Environment.NewLine + ex.Message, "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Logger.Info("TopSubscriptionControl.UnsubscribeButtonClick: error calling UnsubscribePortfolio", ex);
                return;
            }
            if (status != RequestStatus.OK)
            {
                MessageBox.Show(this,
                                "Операция выполнена с ошибкой:" + Environment.NewLine +
                                EnumFriendlyName<RequestStatus>.GetString(status), "Ошибка", MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
                return;
            }
            MessageBox.Show(this, "Операция выполнена успешно", "Информация", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
            SubscriptionModel.Instance.LoadSubscribedCategories();
        }

        // copied from TopPortfolioControl
        private void SetupButtonClick(object sender, EventArgs e)
        {
            try
            {
                var subscribedPortfolio = TradeSharpAccountStatistics.Instance.proxy.GetSubscribedTopPortfolio(
                    AccountModel.Instance.GetUserLogin());
                var form = new AutoTradeSettingsForm(subscribedPortfolio.TradeSettings);
                if (form.ShowDialog(this) == DialogResult.Cancel)
                    return;
                var status = AccountModel.Instance.ServerProxy.ApplyPortfolioTradeSettings(
                    CurrentProtectedContext.Instance.MakeProtectedContext(),
                    AccountModel.Instance.GetUserLogin(), form.sets);
                if (status == RequestStatus.OK)
                    MessageBox.Show(this, "Операция выполнена успешно", "Информация", MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                else
                    MessageBox.Show(this,
                                    "Операция выполнена с ошибкой:" + Environment.NewLine +
                                    EnumFriendlyName<RequestStatus>.GetString(status), "Ошибка",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Exclamation);
            }
            catch (Exception ex)
            {
                //4 debug
                MessageBox.Show(this, "Операция выполнена с ошибкой:" + Environment.NewLine + ex.Message, "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Logger.Info("TopSubscriptionControl.SetupButtonClick: error calling ApplyPortfolioTradeSettings", ex);
                return;
            }
        }
    }
}

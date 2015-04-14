using System;
using System.Windows.Forms;
using TradeSharp.Client.Subscription.Control;
using TradeSharp.Client.Subscription.Dialog;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class ServiceDetailForm : Form
    {
        private readonly PaidService service;
        private readonly PlatformUser user;

        public ServiceDetailForm()
        {
            InitializeComponent();
        }

        public ServiceDetailForm(int serviceId)
            : this()
        {
            try
            {
                service = TradeSharpWalletManager.Instance.proxy.GetPaidServiceDetail(serviceId, out user);
            }
            catch (Exception ex)
            {
                Logger.Error("ServiceDetailForm - GetPaidServiceDetail(" + serviceId + ")", ex);
            }
        }

        private void ServiceDetailFormLoad(object sender, EventArgs e)
        {
            if (service == null) return;
            Text = service.Comment;
            lblAccount.Text = service.AccountId.HasValue ? service.AccountId.ToString() : "-";
            lblFee.Text = service.FixedPrice.ToStringUniformMoneyFormat(true);
            lblOwner.Text = user.FullName;
            lblRegisterDate.Text = user.RegistrationDate.ToStringUniform();
        }

        /// <summary>
        /// перейти на страничку сигнальщика
        /// </summary>
        private void LblAccountClick(object sender, EventArgs e)
        {
            if (service == null || service.AccountId == null) return;


            PerformerStat performer;
            try
            {
                performer = TradeSharpAccountStatistics.Instance.proxy.GetPerformerByAccountId(service.AccountId.Value);
            }
            catch (Exception ex)
            {
                Logger.Error("LblAccountClick.GetPerformerByAccountId(" + service.AccountId.Value + ")", ex);
                return;
            }

            MainForm.Instance.EnsureSubscriptionForm(SubscriptionControl.ActivePage.Subscription);

            var form = new SubscriberStatisticsForm(performer);
            form.EnterRoomRequested += SubscriptionForm.EnterRoom;
            form.Show(this);
        }
    }
}

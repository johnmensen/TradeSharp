using System;
using System.Windows.Forms;
using TradeSharp.Chat.Client.BL;
using TradeSharp.Client.Subscription.Control;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.Subscription.Dialog
{
    public partial class SubscriberStatisticsForm : Form
    {
        public event ChatControlBackEnd.EnterRoomDel EnterRoomRequested;

        public Action<SubscriptionControl.ActivePage> pageTargeted;

        public SubscriberStatisticsForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
            closeButton.Click += (sender, args) => Close();
        }

        public SubscriberStatisticsForm(PerformerStat performer) : this()
        {
            performerStatistic.DataBindCompleted += PerformerStatisticDataBindCompleted;
            performerStatistic.EnterRoomRequested += OnEnterRoomRequested;
            performerStatistic.DataBindAsynch(performer);
            performerStatistic.pageTargeted += OnPageTargeted;
        }

        private void PerformerStatisticDataBindCompleted()
        {
            standByControl.IsShown = false;
            standByControl.Visible = false;
            performerStatistic.Visible = true;
        }

        private void OnEnterRoomRequested(string name, string password = "")
        {
            if (EnterRoomRequested != null)
                EnterRoomRequested(name, password);
        }

        private void OnPageTargeted(SubscriptionControl.ActivePage page)
        {
            if (pageTargeted != null)
                pageTargeted(page);
            Close();
        }
    }
}

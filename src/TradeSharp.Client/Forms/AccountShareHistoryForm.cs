using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FastMultiChart;
using TradeSharp.Client.Subscription.Control;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class AccountShareHistoryForm : Form
    {
        private readonly AccountShared account;

        public AccountShareHistoryForm()
        {
            InitializeComponent();
            SetupChart();
        }

        public AccountShareHistoryForm(AccountShared account) : this()
        {
            this.account = account;
            BuildChart();
        }

        private void SetupChart()
        {
            chartShareTrack.GetXScaleValue = FastMultiChartUtils.GetDateTimeScaleValue;
            chartShareTrack.GetXValue = FastMultiChartUtils.GetDateTimeValue;
            chartShareTrack.GetXDivisionValue = FastMultiChartUtils.GetDateTimeDivisionValue;
            chartShareTrack.GetMinXScaleDivision = FastMultiChartUtils.GetDateTimeMinScaleDivision;
            chartShareTrack.GetMinYScaleDivision = FastMultiChartUtils.GetDoubleMinScaleDivision;
            chartShareTrack.GetXStringValue = FastMultiChartUtils.GetDateTimeStringValue;
            chartShareTrack.GetXStringScaleValue = FastMultiChartUtils.GetDateTimeStringScaleValue;
            chartShareTrack.Graphs[0].Series.Add(new Series("Time", "Balans", new Pen(Color.Blue, 2f))
            {
                XMemberTitle = "дата",
                YMemberTitle = "баланс"
            });
            chartShareTrack.Graphs[0].Series.Add(new Series("Time", "Balans", new Pen(Color.LightGreen, 2f))
            {
                XMemberTitle = "дата",
                YMemberTitle = "HWM"
            });
        }

        private void BuildChart()
        {
            // запросить на сервере историю стоимости пая
            List<AccountShareOnDate> shares;
            try
            {
                shares = TradeSharpAccount.Instance.proxy.GetAccountShareHistory(account.AccountId, AccountStatus.Instance.Login);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка получения истории пая", ex);
                MessageBox.Show(
                    Localizer.GetString("MessageErrorGettingShareHistory"), 
                    Localizer.GetString("TitleError"), 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            // построить график
            chartShareTrack.Graphs[0].Series[0].Clear();
            foreach (var share in shares)
            {
                chartShareTrack.Graphs[0].Series[0].Add(new TimeBalans(share.date, (float)share.shareAmount));
                chartShareTrack.Graphs[0].Series[1].Add(new TimeBalans(share.date, (float)share.newHWM));
            }
            chartShareTrack.Initialize();
        }
    }
}

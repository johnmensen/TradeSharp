using System;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.UI.Util.Forms
{
    public partial class PositionForm : Form
    {
        private readonly MarketOrder order;

        public PositionForm()
        {
            InitializeComponent();
        }

        public PositionForm(MarketOrder order)
        {
            InitializeComponent();
            
            this.order = order;
            SetupForm();
            ShowOrderParams();
        }

        private void SetupForm()
        {
            foreach (PositionState state in Enum.GetValues(typeof(PositionState)))
                cbStatus.Items.Add(EnumFriendlyName<PositionState>.GetString(state));
            
            foreach (PositionExitReason reason in Enum.GetValues(typeof(PositionExitReason)))
                cbExitReason.Items.Add(EnumFriendlyName<PositionExitReason>.GetString(reason));
        }

        private void ShowOrderParams()
        {
            // заголовок окна
            Text = order.ToString();
            
            // параметры сделки
            cbStatus.SelectedIndex = cbStatus.Items.IndexOf(EnumFriendlyName<PositionState>.GetString(order.State));
            if (order.State == PositionState.Closed)
                cbExitReason.SelectedIndex = cbExitReason.Items.IndexOf(EnumFriendlyName<PositionExitReason>.GetString(order.ExitReason));
            else 
                cbExitReason.Visible = false;

            tbPriceEnter.Text = FormatPrice(order.PriceEnter);
            dpEnter.Value = order.TimeEnter;
            if (order.PriceExit.HasValue)
                tbPriceExit.Text = FormatPrice(order.PriceExit.Value);
            if (order.TimeExit.HasValue)
                dpExit.Value = order.TimeExit.Value;
            if (order.StopLoss.HasValue && order.StopLoss.Value > 0)
                tbSL.Text = FormatPrice(order.StopLoss.Value);
            if (order.TakeProfit.HasValue && order.TakeProfit.Value > 0)
                tbTP.Text = FormatPrice(order.TakeProfit.Value);
            if (order.Magic.HasValue)
                tbMagic.Text = order.Magic.Value.ToString();
            tbVolume.Text = order.Volume.ToStringUniformMoneyFormat();

            var sbTrailing = new StringBuilder();
            for (var i = 0; i < order.trailingLevels.Length; i++)
            {
                if ((!order.trailingLevels[i].HasValue) || order.trailingLevels[i] == 0 ||
                    (!order.trailingTargets[i].HasValue)) break;
                sbTrailing.AppendFormat("[{0} - {1}]",
                                        FormatPrice(order.trailingLevels[i].Value),
                                        FormatPrice(order.trailingTargets[i].Value));
            }
            tbTrailing.Text = sbTrailing.ToString();
            tbComment.Text = 
                  string.IsNullOrEmpty(order.Comment) && string.IsNullOrEmpty(order.ExpertComment) ? ""
                : string.IsNullOrEmpty(order.Comment) ? "[R] " + order.ExpertComment
                : string.IsNullOrEmpty(order.ExpertComment) ? order.Comment
                : order.Comment + ", [R] " + order.ExpertComment;
        }

        private static string FormatPrice(float price)
        {
            return price < 8 ? price.ToString("f4", CultureInfo.InvariantCulture) 
                : price.ToString(price < 60 ? "f3" : "f2", CultureInfo.InvariantCulture);
        }
    }
}

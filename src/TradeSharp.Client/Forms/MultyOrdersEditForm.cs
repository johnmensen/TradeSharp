using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Entity;
using FastGrid;
using TradeSharp.Client.BL;
using TradeSharp.Client.Controls;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;
using System.Linq;

namespace TradeSharp.Client.Forms
{
    public partial class MultyOrdersEditForm : Form
    {
        private readonly List<int> orderIds = new List<int>();

        public delegate string FloatFormatterDel(float? val);

        private readonly List<PreValuedTextBox> editFields = new List<PreValuedTextBox>();

        private readonly Dictionary<string, PropertyInfo> fieldAccessor = new Dictionary<string, PropertyInfo>();

        public MultyOrdersEditForm()
        {
            InitializeComponent();
        }

        public MultyOrdersEditForm(List<int> orderIds) : this()
        {
            this.orderIds = orderIds;
            SetupGrid();
            SetupEditFields();
            timerUpdate.Enabled = true;
        }

        private void SetupEditFields()
        {
            tbStoploss.BoundPropertyName = "StopLoss";
            tbTakeprofit.BoundPropertyName = "TakeProfit";
            tbMagic.BoundPropertyName = "Magic";
            tbMagic.formatter = text => text.ToIntSafe();
            tbComment.BoundPropertyName = "Comment";
            tbComment.formatter = text => text;
            tbCommentRobot.BoundPropertyName = "ExpertComment";
            tbCommentRobot.formatter = text => text;
            
            editFields.Add(tbStoploss);
            editFields.Add(tbTakeprofit);
            editFields.Add(tbMagic);
            editFields.Add(tbComment);
            editFields.Add(tbCommentRobot);

            foreach (var pi in typeof (MarketOrder).GetProperties())
            {
                var field = editFields.FirstOrDefault(f => f.BoundPropertyName == pi.Name);
                if (field == null) continue;
                
                var title = pi.Name;
                var dispNameAttrs = pi.GetCustomAttributes(typeof (DisplayNameAttribute), true);
                if (dispNameAttrs.Length > 0)
                    title = ((DisplayNameAttribute) dispNameAttrs[0]).DisplayName;

                field.BoundPropertyTitle = title;
                fieldAccessor.Add(pi.Name, pi);
            }
        }

        private void SetupGrid()
        {
            var fontBold = new Font(Font, FontStyle.Bold);
            gridOrders.Columns.Add(new FastColumn("ID", "№")
            {
                ColumnWidth = 40,
                SortOrder = FastColumnSort.Ascending
            });
            gridOrders.Columns.Add(new FastColumn("Side", "Тип")
            {
                ColumnWidth = 40,
                formatter = value => (int)value == -1 ? "SELL" : "BUY",
                IsHyperlinkStyleColumn = true,
                HyperlinkActiveCursor = Cursors.Hand,
                ColorHyperlinkTextActive = Color.DodgerBlue,
                HyperlinkFontActive = fontBold
            });
            gridOrders.Columns.Add(new FastColumn("Volume", "Объем")
            {
                ColumnWidth = 74,
                formatter = value => ((int)value).ToStringUniformMoneyFormat()
            });
            gridOrders.Columns.Add(new FastColumn("PriceEnter", "Вход")
            {
                ColumnWidth = 66,
                formatter = value => ((float)value).ToStringUniformPriceFormat()
            });
            gridOrders.Columns.Add(new FastColumn("ResultDepo", "Прибыль")
            {
                ColumnWidth = 73,
                formatter = value => ((float)value).ToStringUniformMoneyFormat(),
                SortOrder = FastColumnSort.Ascending,
                colorColumnFormatter = (object value, out Color? color, out Color? fontColor) =>
                {
                    color = null;
                    fontColor = null;
                    if ((float)value < 0) fontColor = Color.DarkRed;
                }
            });
            gridOrders.Columns.Add(new FastColumn("ResultPoints", "Пункты")
            {
                ColumnWidth = 68,
                formatter = value => ((float)value).ToStringUniformMoneyFormat(),
                colorColumnFormatter = (object value, out Color? color, out Color? fontColor) =>
                {
                    color = null;
                    fontColor = null;
                    if ((float)value < 0) fontColor = Color.DarkRed;
                }
            });
            gridOrders.Columns.Add(new FastColumn("StopLoss", "SL")
            {
                ColumnWidth = 66,
                formatter = value => value == null ? "" : ((float)value).ToStringUniformPriceFormat()
            });
            gridOrders.Columns.Add(new FastColumn("TakeProfit", "TP")
            {
                ColumnWidth = 66,
                formatter = value => value == null ? "" : ((float)value).ToStringUniformPriceFormat()
            });

            gridOrders.ColorAnchorCellBackground = Color.FromArgb(220, 255, 200);
            gridOrders.StickLast = true;
            gridOrders.CalcSetTableMinWidth();
        }
    
        private void UpdateOrdersSafe()
        {
            try
            {
                Invoke(new Action(UpdateOrders));
            }
            catch (InvalidOperationException)
            {
                return;
            }
        }

        private void UpdateOrders()
        {
            var orders = MarketOrdersStorage.Instance.MarketOrders;
            orders = orders.Where(o => orderIds.Contains(o.ID)).ToList();
            if (orders.Count == 0)
                gridOrders.rows.Clear();
            else
            {
                if (orders.Count > 0)
                {
                    var quote = QuoteStorage.Instance.ReceiveValue(orders[0].Symbol);
                    var avgOrder = DalSpot.Instance.CalculateSummaryOrder(orders, quote, null);
                    if (avgOrder != null)
                        orders.Add(avgOrder);
                }

                gridOrders.DataBind(orders);
            }

            // обновить поля SL, TP и прочие
            if (tbStoploss.HasDefaultValue)
                tbStoploss.UpdateDefaultValue(GetDefaultSample(
                    orders.Select(o => o.StopLoss),
                    v => v == null ? "" : v.Value.ToStringUniformPriceFormat(true), true, 0.0001f));
            if (tbTakeprofit.HasDefaultValue)
                tbTakeprofit.UpdateDefaultValue(GetDefaultSample(
                    orders.Select(o => o.TakeProfit), 
                    v => v == null ? "" : v.Value.ToStringUniformPriceFormat(true), true, 0.0001f));

            if (tbMagic.HasDefaultValue)
                tbMagic.UpdateDefaultValue(GetDefaultSample(orders.Select(o => o.Magic), true));
            if (tbComment.HasDefaultValue)
                tbComment.UpdateDefaultValue(GetDefaultSample(orders.Select(o => o.Comment)));
            if (tbCommentRobot.HasDefaultValue)
                tbCommentRobot.UpdateDefaultValue(GetDefaultSample(orders.Select(o => o.ExpertComment)));
        }

        public string GetDefaultSample(IEnumerable<float?> values, FloatFormatterDel floatFormatter,
            bool nilAsNull, float maxDeltaToCountSame)
        {
            float? sample = null;
            foreach (var val in values)
            {
                if (val == null || (val == 0 && nilAsNull)) continue;
                if (sample.HasValue)
                {
                    var delta = Math.Abs(sample.Value - val.Value);
                    if (delta > maxDeltaToCountSame)
                        return floatFormatter(null);
                    continue;
                }
                sample = val;
            }

            return floatFormatter(sample);
        }

        public string GetDefaultSample(IEnumerable<int?> values,
            bool nilAsNull)
        {
            int? sample = null;
            foreach (var val in values)
            {
                if (val == null || (val == 0 && nilAsNull)) continue;
                if (sample.HasValue)
                {
                    if (sample.Value != val.Value)
                        return "";
                    continue;
                }
                sample = val;
            }

            return sample == null ? "" : sample.ToString();
        }

        public string GetDefaultSample(IEnumerable<string> values)
        {
            string sample = null;
            foreach (var val in values)
            {
                if (string.IsNullOrEmpty(val)) continue;
                if (sample != null)
                {
                    if (sample != val)
                        return "";
                    continue;
                }
                sample = val;
            }

            return sample == null ? "" : sample;
        }

        private void TimerUpdateTick(object sender, EventArgs e)
        {
            UpdateOrdersSafe();
        }

        private void BtnAcceptClick(object sender, EventArgs e)
        {
            // если все поля содержат значения по-умолчанию - ничего не делать
            if (editFields.All(f => f.HasDefaultValue))
                return;
            if (MessageBox.Show(Localizer.GetString("MessageFieldsWillBeUpdated") + ": " +
                                string.Join(", ",
                                            editFields.Where(f => !f.HasDefaultValue).Select(f => f.BoundPropertyTitle)) +
                                ". " + Localizer.GetString("TitleProceed") + "?",
                                Localizer.GetString("TitleConfirmation"), 
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            // получить ордера..
            var orders = MarketOrdersStorage.Instance.MarketOrders;
            orders = orders.Where(o => orderIds.Contains(o.ID)).ToList();

            // и обновить их поля
            foreach (var order in orders)
            {
                foreach (var field in editFields.Where(f => !f.HasDefaultValue))
                {
                    var fieldVal = field.GetFormattedValue();
                    fieldAccessor[field.BoundPropertyName].SetValue(order, fieldVal, null);
                }

                // отправить запрос на изменение ордера
                MainForm.Instance.SendEditMarketRequestSafe(order);
            }
        }

        private void BtnWizzardSlClick(object sender, EventArgs e)
        {
            CalculateSlOrTpTarget(true);
        }

        private void CalculateSlOrTpTarget(bool slChosen = true)
        {
            var orders = gridOrders.GetRowValues<MarketOrder>(false).Where(o => o.ID != 0).ToList();
            if (orders.Count == 0) return;
            if (orders.Select(o => o.Symbol).Distinct().Count() != 1)
            {
                MessageBox.Show(Localizer.GetString("MessageCannotSetupCommonSlTp"));
                return;
            }

            var symbol = orders[0].Symbol;
            var active = DalSpot.Instance.GetActiveFromPair(symbol, false);

            // посчитать цену паритета
            var priceParity = 0f;
            var priceParityDenom = orders.Sum(o => o.Side*o.Volume);
            if (priceParityDenom != 0)
                priceParity = orders.Sum(o => o.Side*o.Volume*o.PriceEnter)/priceParityDenom;

            // выбрать цель - пункты или контрвалюта, указать смещение от паритета
            var dlg = new CloseTargetDlg(active, priceParity);
            if (dlg.ShowDialog() != DialogResult.OK) return;

            // для ордеров вычислить TP - SL, соответствующий указанной цене / пунктам
            var target = dlg.TargetNumber;
            float price = 0;
            if (dlg.Units == CloseTargetDlg.UnitType.Points) // пунктам...
            {
                var sumSign = orders.Sum(o => o.Side);
                if (sumSign == 0)
                {
                    MessageBox.Show(Localizer.GetString("MessageSumPointsBuysAndSellsAreNil"));
                    return;
                }
                var sideTotal = Math.Sign(sumSign);
                target = DalSpot.Instance.GetAbsValue(symbol, target);
                price = sideTotal * target + orders.Sum(o => o.Side * o.PriceEnter) / sumSign;
            }
            else if (dlg.Units == CloseTargetDlg.UnitType.Counter)
            {
                // прибыль в контрвалюте?
                var sumVolume = orders.Sum(o => o.Side * o.Volume);
                if (sumVolume == 0)
                {
                    MessageBox.Show(Localizer.GetString("MessageSumVolmBuysAndSellsAreNil"));
                    return;
                }
                //var sideTotal = Math.Sign(sumVolume);
                price = (target + orders.Sum(o => o.Side * o.PriceEnter * o.Volume)) / sumVolume;
            }
            else
            {
                // цена
                price = dlg.TargetNumber;
            }

            // получить текущую цену, для сравнения
            var quote = QuoteStorage.Instance.ReceiveValue(symbol);
            if (quote == null)
            {
                MessageBox.Show(Localizer.GetString("MessageNoQuote") + " " + symbol);
                return;
            }

            // предупредить пользователя
            int countTp = 0, countSl = 0;
            foreach (var order in orders)
            {
                var isSl = (order.Side > 0 && price < quote.bid) ||
                           (order.Side < 0 && price > quote.bid);
                if (isSl) countSl++;
                else countTp++;
            }

            var priceStr = price.ToStringUniformPriceFormat();
            var msgParts = new List<string>();
            if (countSl > 0)
                msgParts.Add(
                    string.Format(Localizer.GetString("MessagePtrWillBeSetForNPosFmt"), countSl) +
                    " SL = " + priceStr);
            if (countTp > 0)
                msgParts.Add(string.Format(Localizer.GetString("MessagePtrWillBeSetForNPosFmt"), countTp) +
                    " TP = " + priceStr);

            if (MessageBox.Show(string.Join(Environment.NewLine, msgParts) +
                ". " + Localizer.GetString("TitleProceed") + "?",
                Localizer.GetString("TitleConfirmation"), 
                MessageBoxButtons.YesNo) == DialogResult.No) return;

            // таки выставить SL - TP
            foreach (var order in orders)
            {
                var isSl = (order.Side > 0 && price < quote.bid) ||
                           (order.Side < 0 && price > quote.bid);
                if (isSl) order.StopLoss = price;
                else order.TakeProfit = price;
                // отправить запрос на изменение ордера
                MainForm.Instance.SendEditMarketRequestSafe(order);
            }
        }

        private void BtnWizzardTpClick(object sender, EventArgs e)
        {
            CalculateSlOrTpTarget(false);
        }
    }
}

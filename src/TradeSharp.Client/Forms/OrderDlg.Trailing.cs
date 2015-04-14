using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Linq;
using Entity;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class OrderDlg
    {
        private int maxTrailLevels = 4;

        private readonly BindingList<TrailingTag> trailLevels = new BindingList<TrailingTag>();

        private void GridTrailingCellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (gridTrailing.EditingControl == null) return;

            string str;
            if (e.ColumnIndex == 0)
            {
                var num = gridTrailing.EditingControl.Text.Replace(",", ".").ToDecimalUniformSafe() ?? 0;
                str = num.ToString();
                // пересчитать цену в пункты
                if (num > 0 && IsEditMode)
                {
                    var price = tbPrice.Text.ToDecimalUniformSafe();
                    if (price.HasValue)
                    {
                        var points = (int)Math.Round(DalSpot.Instance.GetPointsValue(Ticker, (num - price.Value) * orderSide));
                        ((TrailingTag)gridTrailing.Rows[e.RowIndex].DataBoundItem).Points = points;
                    }
                }
            }
            else
            {
                str = (gridTrailing.EditingControl.Text.ToIntSafe() ?? 0).ToString();
                // пересчитать пункты в цену
                if (e.ColumnIndex == 1 && IsEditMode)
                {
                    var num = gridTrailing.EditingControl.Text.ToIntSafe();
                    var price = tbPrice.Text.ToFloatUniformSafe();
                    if (price.HasValue && num.HasValue)
                    {
                        var abs = DalSpot.Instance.GetAbsValue(Ticker, (float)num.Value);
                        price += abs*orderSide;
                        ((TrailingTag)gridTrailing.Rows[e.RowIndex].DataBoundItem).Price = price.Value;
                        gridTrailing.Invalidate();
                    }
                }
            }
            gridTrailing.EditingControl.Text = str;
        }

        private void GridTrailingCellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                Invoke(new Action(() => { gridTrailing.AllowUserToAddRows = gridTrailing.Rows.Count < maxTrailLevels; }));
            }
            catch (InvalidOperationException ex)
            {                
                throw;
            }
        }

        private void GridTrailingRowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            if (!gridTrailing.AllowUserToAddRows)
                gridTrailing.AllowUserToAddRows = gridTrailing.Rows.Count < maxTrailLevels;
        }

        private void TabControlExtraSelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlExtra.SelectedTab != tabPagePending) return;
            if (!IsPending) return;
            if (AccountStatus.Instance.connectionStatus != AccountConnectionStatus.Connected) return;
            var accountId = AccountStatus.Instance.accountID;

            // заполнить список OCO
            // пустой элемент
            cbOrderOCO.Items.Clear();
            cbOrderOCO.Items.Add(new PendingOrder());
            cbOrderOCO.SelectedIndex = 0;

            try
            {
                List<PendingOrder> orders;
                TradeSharpAccount.Instance.proxy.GetPendingOrders(accountId, out orders);
                if (orders == null || orders.Count == 0) return;
                
                // добавить полученные ордера в список
                foreach (var order in orders)
                {
                    if (order.Symbol != ticker) continue;
                    if (pendingOrder != null && order.ID == pendingOrder.ID) continue;
                    cbOrderOCO.Items.Add(order);                    
                }

                // выбрать и показать парный ордер
                if (pendingOrder != null && pendingOrder.PairOCO.HasValue)
                {
                    var orderPair = orders.FirstOrDefault(o => o.ID == pendingOrder.PairOCO.Value);
                    if (orderPair != null)
                        cbOrderOCO.SelectedIndex = cbOrderOCO.Items.IndexOf(orderPair);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка загрузки списка ордеров OCO", ex);
            }
        }

        private void GetTrailingLevels(int side,
            out float?[] trailingLevels, out float?[] trailingTargets)
        {
            trailingLevels = new float?[4];
            trailingTargets = new float?[4];
            var trails = new List<Cortege2<float, float>>();

            if (trailLevels.Count > 0)
            {
                foreach (var level in trailLevels)
                {
                    if (level.Points == 0 && level.Price <= 0) continue;
                    var points = level.Points;
                    if (points == 0)
                    {
                        // получить пункты трейлинга из цены
                        var priceEnter = tbPrice.Text.ToFloatUniformSafe();
                        if (!priceEnter.HasValue && lastQuote != null)
                            priceEnter = side > 0 ? lastQuote.ask : lastQuote.bid;
                        if (!priceEnter.HasValue) continue;
                        points = (int) Math.Round(
                            DalSpot.Instance.GetPointsValue(Ticker, side*(level.Price - priceEnter.Value)));
                    }
                    trails.Add(new Cortege2<float, float>(points, level.Target));                    
                }
            }
            trails = trails.OrderBy(t => t.a).ToList();
            var index = 0;
            foreach (var trail in trails)
            {
                trailingLevels[index] = trail.a;
                trailingTargets[index] = trail.b;
                index++;
                if (index == 4) break;
            }
        }

        private void LoadTrailingLevels(float?[] trailingLevels, float?[] trailingTargets)
        {
            for (var i = 0; i < trailingLevels.Length; i++)
            {
                var tag = MakeTrailingTag(trailingLevels[i], trailingTargets[i]);
                if (tag == null) continue;
                if (trailLevels.Count <= i) trailLevels.Add(tag);
                else trailLevels[i] = tag;
            }
            // обновить привязку
        }

        private TrailingTag MakeTrailingTag(float? levelPips, float? target)
        {
            if (!levelPips.HasValue || !target.HasValue) return null;
            var tag = new TrailingTag
                          {
                              Points = (int) Math.Round(levelPips.Value),
                              Target = (int) Math.Round(target.Value)
                          };
            // пункты в цену
            var price = tbPrice.Text.ToFloatUniformSafe();
            if (price.HasValue)
            {
                tag.Price = price.Value + DalSpot.Instance.GetAbsValue(Ticker, (float) tag.Points) * orderSide;
            }
            return tag;
        }
    }    

    class TrailingTag
    {
        public float Price { get; set; }

        public int Points { get; set; }

        public int Target { get; set; }
    }
}

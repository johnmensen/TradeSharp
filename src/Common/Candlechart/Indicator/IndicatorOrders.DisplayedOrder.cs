using System.Text;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    static class DisplayedOrder
    {
        public static bool AreSame(MarketOrder a, MarketOrder b)
        {
            return a.ID == b.ID && a.State == b.State &&
                   a.StopLoss == b.StopLoss && a.TakeProfit == b.TakeProfit;            
        }

        public static string MakeOrderTitle(MarketOrder order)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("#{0} {1} {2} по {3}, {4:dd.MM HH:mm:ss}{5}",
                                            order.ID, order.Side > 0 ? "BUY" : "SELL",
                                            order.Volume.ToStringUniformMoneyFormat(),
                                            DalSpot.Instance.FormatPrice(
                                                order.Symbol, order.PriceEnter), 
                                                order.TimeEnter, order.PriceExit.HasValue 
                                                ? "," : ""));
            if (order.PriceExit.HasValue)
            {
                sb.AppendLine(string.Format("закрыта по {0} {1:dd.MM HH:mm:ss}\n" +
                    "Результат (пп - депо.): {2} / {3}",
                // ReSharper disable PossibleInvalidOperationException
                                            order.PriceExit.Value, order.TimeExit.Value,
                                            (int)order.ResultPoints,
                                            (int)order.ResultDepo));
                // ReSharper restore PossibleInvalidOperationException
            }
            return sb.ToString();
        }

        public static string MakeOrderTitle(PendingOrder order)
        {
            return string.Empty;
        }
    }        
}

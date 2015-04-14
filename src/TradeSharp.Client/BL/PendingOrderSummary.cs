using System.Collections.Generic;
using System.Linq;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Client.BL
{    
    struct PendingOrderSummary
    {
        public List<PendingOrder> orders;
        /// <summary>
        /// валютная пара
        /// </summary>
        public string symbol;
        /// <summary>
        /// суммарная поза (< 0 - продажа) 
        /// </summary>
        public int exposition;
        /// <summary>
        /// средняя взвешенная цена
        /// (Vb*Pb - Vs*Ps) / (Vb - Vs)
        /// </summary>

        public float averagePrice;

        public float hashCode;

        public static List<PendingOrderSummary> GetPendingOrderSummary(List<PendingOrder> orders)
        {
            var summary = new List<PendingOrderSummary>();
            var symbols = orders.Select(o => o.Symbol).Distinct();

            foreach (var symbol in symbols)
            {
                float sumBuys = 0, sumSell = 0;
                var sum = new PendingOrderSummary { symbol = symbol, orders = new List<PendingOrder>() };
                var curSymbol = symbol;
                var sumDeals = orders.FindAll(o => o.Symbol == curSymbol);
                foreach (var sumDeal in sumDeals)
                {
                    sum.orders.Add(sumDeal);
                    sum.exposition += (int)(sumDeal.Side * sumDeal.Volume);

                    if (sumDeal.Side > 0)
                        sumBuys += sumDeal.Volume * sumDeal.PriceFrom;
                    else
                        sumSell += sumDeal.Volume * sumDeal.PriceFrom;
                }
                sum.averagePrice = sum.exposition == 0 ? 0 : (sumBuys - sumSell) / sum.exposition;
                // вычисляю хеш
                sum.hashCode = 0;
                foreach (var order in sumDeals)
                    sum.hashCode += order.PriceFrom * 1000 + (order.TakeProfit ?? 0) * 10 + (order.StopLoss ?? 0) * 10;
                summary.Add(sum);
            }
            return summary;
        }

        public override string ToString()
        {
            return string.Format("{0:f0} {1}{2}", exposition, symbol, averagePrice == 0
                      ? ""
                      : string.Format(" [{0:f4}]", averagePrice));
        }
    }    
}

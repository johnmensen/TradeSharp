using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.BL
{
    class AccountStatistics : AccountEfficiency
    {
        /// <summary>
        /// суммарный результат по закрытым сделкам
        /// </summary>
        public float sumClosedResult;

        /// <summary>
        /// суммарный результат по открытым сделкам
        /// </summary>
        public float sumOpenResult;

        /// <summary>
        /// сумма всех пополнений/списаний за период
        /// </summary>
        public float sumDeltaBalance;

        /// <summary>
        /// среднегеом. доход за день
        /// </summary>
        public float ProfitGeomDay { get; set; }

        public void Calculate(
            List<BalanceChange> balanceChanges, 
            List<MarketOrder> marketOrders, DateTime startDate)
        {
            // первый трансфер считаем за начальный баланс
            InitialBalance = (float)balanceChanges[0].SignedAmountDepo;
            balanceChanges.RemoveAt(0);
            if (listEquity.Count < 2)
            {
                return;
            }
            
            // считаем результат по открытым позициям
            var openPosList = PositionSummary.GetPositionSummary(marketOrders.Where(o => o.State == PositionState.Opened).ToList(),
                AccountStatus.Instance.AccountData.Currency, (float)AccountStatus.Instance.AccountData.Balance);
            sumOpenResult = openPosList[openPosList.Count - 1].Profit;
            // посчитать суммарный результат по сделкам (дельта эквити за вычетом пополнений/списаний)
            // все закрытые позиции
            var dueBalance = balanceChanges.Where(bc => bc.ValueDate >= listEquity[0].time &&
                (bc.ChangeType == BalanceChangeType.Deposit ||
                 bc.ChangeType == BalanceChangeType.Withdrawal)).ToList();

            var closedList = balanceChanges.Where(bc => bc.ValueDate >= listEquity[0].time &&
                (bc.ChangeType == BalanceChangeType.Profit || 
                bc.ChangeType == BalanceChangeType.Loss)).ToList();

            // результат по всем закрытым сделкам
            sumClosedResult = (float)closedList.Sum(bc => bc.SignedAmountDepo);

            // сумма всех неторговых операций
            sumDeltaBalance = (float)dueBalance.Sum(bc => bc.SignedAmountDepo);
            //CurrentBalance = InitialBalance + sumClosedResult + sumDeltaBalance + sumOpenResult;
            
            // получить список ROR
            var listROR = new List<Cortege2<DateTime, float>>();

            for (var i = 1; i < listEquity.Count; i++)
            {
                var endEquity = listEquity[i];
                var startEquity = listEquity[i - 1];
                if (startEquity.equity == 0) break;
                var deltaBalance = 0f;
                for (var j = 0; j < dueBalance.Count; j++)
                {
                    if (dueBalance[j].ValueDate > listEquity[i].time) break;
                    deltaBalance += (float)dueBalance[j].SignedAmountDepo;
                    dueBalance.RemoveAt(j);
                    j--;
                }
                var rateOfReturn = ((endEquity.equity - startEquity.equity - deltaBalance) / startEquity.equity);
                listROR.Add(new Cortege2<DateTime, float>(listEquity[i].time, rateOfReturn));
            }
            // убрать все 0-е значения от начала отсчета
            for (var i = 0; i < listROR.Count; i++)
                if (listROR[i].b == 0)
                {
                    listROR.RemoveAt(i);
                    i--;
                }
                else break;

            if (listROR.Count == 0) return;

            // получить кривую доходности на виртуальную 1000
            listProfit1000 = new List<EquityOnTime>();
            var startBalance1000 = 1000f;
            listProfit1000.Add(new EquityOnTime(startBalance1000, startDate));
            foreach (var ret in listROR)
            {
                startBalance1000 += startBalance1000 * ret.b;
                listProfit1000.Add(new EquityOnTime(startBalance1000, ret.a));
            }
            // посчитать макс. проседание
            CalculateDrawdown();
            // посчитать среднегеометрическую дневную, месячную и годовую доходность
            var avgROR = listROR.Average(ret => ret.b);
            ProfitGeomMonth = (float)Math.Pow(1 + avgROR, 20f) - 1;
            ProfitGeomYear = (float)Math.Pow(1 + avgROR, 250f) - 1;
            ProfitGeomDay = avgROR;
        }

        private void CalculateDrawdown()
        {
            Statistics.MaxRelDrawDown = 0;
            if (listProfit1000.Count == 0) return;
            
            for (var i = 0; i < listProfit1000.Count - 1;)
            {
                var tempDrawDown = 0f;
                var startBalance = listProfit1000[i].equity;
                if (startBalance == 0) continue;
                
                var j = i + 1;
                for (; j < listProfit1000.Count; j++)
                {
                    var curBal = listProfit1000[j].equity;
                    if (curBal >= startBalance) break;
                    var curDd = startBalance - curBal;
                    if (curDd > tempDrawDown) tempDrawDown = curDd;
                }
                i = j;

                tempDrawDown /= startBalance;
                if (tempDrawDown > Statistics.MaxRelDrawDown)
                    Statistics.MaxRelDrawDown = tempDrawDown;
            }
            Statistics.MaxRelDrawDown *= 100;
        }
    }    
}

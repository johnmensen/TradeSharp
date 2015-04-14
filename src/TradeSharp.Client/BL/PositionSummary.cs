using System;
using System.Collections.Generic;
using System.Linq;
using Entity;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Client.BL
{
    class PositionSummary
    {
        public List<MarketOrder> orders;

        /// <summary>
        /// валютная пара
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// суммарная поза (less than 0 - продажа) 
        /// </summary>
        public int Exposition { get; set; }


        /// <summary>
        /// суммарный объем (less than 0 - продажа) 
        /// </summary>
        public int Volume { get; set; }
        
        public int VolumeProtected { get; set; }

        /// <summary>
        /// средняя взвешенная цена суммарной позы
        /// (Vb*Pb - Vs*Ps) / (Vb - Vs)
        /// </summary>
        public float AveragePrice { get; set; }

        public int Side { get; set; }

        public float Profit { get; set; }

        public int ProfitInPoints { get; set; }

        public float ProfitInPercent { get; set; }

        public float Leverage { get; set; }

        public float LeverageProtected { get; set; }

        public float hashCode;

        public string Equity { get; set; }

        public string TakeProfit { get; set; }

        public string StopLoss { get; set; }

        public string Command { get; set; }

        private static readonly FloodSafeLogger logNoFlood = new FloodSafeLogger(1000 * 60);

        private const int MsgNoQuote = 1;
        
        public float ConvertExpositionToDepo(string dealSymbol, string depoCurrency,
            Dictionary<string, QuoteData> curPrices, QuoteType quoteType, float value)
        {
            bool inverse, pairsEqual;
            var pair = DalSpot.Instance.FindSymbol(dealSymbol, true, depoCurrency, out inverse, out pairsEqual);
            if (pairsEqual) return value;
            if (string.IsNullOrEmpty(pair))
            {
                logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, MsgNoQuote,
                    1000 * 60 * 15, "Ошибка в ConvertBaseOrCounterDepo: {0}, {1} - не найдена",
                    dealSymbol, depoCurrency);
                return value;
            }
            if (!curPrices.ContainsKey(pair))
            {
                logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, MsgNoQuote,
                    1000 * 60 * 15, "Ошибка в ConvertBaseOrCounterDepo: {0} - не найдена", pair);
                return value;
            }
            var price = curPrices[pair].GetPrice(quoteType);
            if (price == 0)
            {
                logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, MsgNoQuote,
                    1000 * 60 * 15, "Ошибка в ConvertBaseOrCounterDepo: {0} - цена = 0", pair);
                return value;
            }

            value = inverse ? value / price : value * price;
            return value;
        }

        public static List<PositionSummary> GetPositionSummary(List<MarketOrder> orders, 
            string accountCurrency, float accountBalance)
        {
            var quotes = QuoteStorage.Instance.ReceiveAllData();
            return GetPositionSummary(quotes, orders, accountCurrency, accountBalance);
        }

        public static List<PositionSummary> GetPositionSummary(Dictionary<string, QuoteData> quotes,
            List<MarketOrder> orders, string accountCurrency)
        {
            return GetPositionSummary(quotes, orders, accountCurrency,
                                      (float) AccountStatus.Instance.AccountData.Balance);
        }

        public static List<PositionSummary> GetPositionSummary(Dictionary<string, QuoteData> quotes,
            List<MarketOrder> orders, string accountCurrency, float accountBalance)
        {
            var summary = new List<PositionSummary>();
            if (orders == null) orders = new List<MarketOrder>();
            
            var symbols = orders.Select(o => o.Symbol).Distinct();

            foreach (var symbol in symbols)
            {
                float sumBuys = 0, sumSell = 0;
                var sum = new PositionSummary { Symbol = symbol, orders = new List<MarketOrder>() };
                var curSymbol = symbol;
                var sumDeals = orders.FindAll(o => o.Symbol == curSymbol);
                sum.ProfitInPercent = 0;
                sum.Profit = 0;
                sum.Volume = 0;

                //var rate = 
                var tpflag = true;
                var slflag = true;
                float? tp = null;
                float? sl = null;
                foreach (var sumDeal in sumDeals)
                {
                    if (tpflag)
                    {
                        if (sumDeal.TakeProfit != null)
                        {
                            if (tp == null)
                                tp = sumDeal.TakeProfit;
                            else
                            {
                                if (tp != null && tp != sumDeal.TakeProfit)
                                    tpflag = false;
                            }
                        }
                        else
                            tpflag = false;
                    }
                    if (slflag)
                    {
                        if (sumDeal.StopLoss != null)
                        {
                            if (sl == null)
                                sl = sumDeal.StopLoss;
                            else
                            {
                                if (sl != null && sl != sumDeal.StopLoss)
                                    slflag = false;
                            }
                        }
                        else
                            slflag = false;
                    }

                    var profitDepo = DalSpot.Instance.CalculateProfitInDepoCurrency(sumDeal, quotes, accountCurrency);
                    //var profitStr = profitDepo.HasValue ? profitDepo.Value.ToString("f2") : "-";
                    if (AccountStatus.Instance.AccountData != null)
                    {
                        var profitPercent = (profitDepo ?? 0) / accountBalance * 100;
                        sum.ProfitInPercent += profitPercent;
                        sum.Profit += profitDepo ?? 0;
                    }
                    sum.orders.Add(sumDeal);
                    sum.Volume += sumDeal.Side * sumDeal.Volume;

                    // в админском режиме - посчитать объем и сумму "Free"
                    if (HiddenModes.ManagerMode)
                        if (sumDeal.StopLoss.HasValue)
                        {
                            // проверяем позиция поджата или нет
                            if ((sumDeal.Side > 0 && sumDeal.PriceEnter < sumDeal.StopLoss) || (sumDeal.Side < 0 && sumDeal.PriceEnter > sumDeal.StopLoss))
                            {
                                // позиция поджата (защищена)
                                sum.VolumeProtected += sumDeal.Side * sumDeal.Volume;
                            }
                        }
                    if (sumDeal.Side > 0)
                        sumBuys += sumDeal.Volume * sumDeal.PriceEnter;
                    else
                        sumSell += sumDeal.Volume * sumDeal.PriceEnter;
                }

                sum.AveragePrice = (float)
                    Math.Round(sum.Volume == 0 ? 0 : (sumBuys - sumSell) / sum.Volume, DalSpot.Instance.GetPrecision(sum.Symbol));

                sum.Exposition = (int)sum.ConvertExpositionToDepo(sum.Symbol, accountCurrency, quotes,
                                                             sum.Volume > 0 ? QuoteType.Bid : QuoteType.Ask,
                                                             sum.Volume);
                sum.Side = sum.Exposition > 0 ? 1 : -1;

                // в менеджерском режиме - считаем параметры "free"
                // в остальных режимах эти данные не нужны
                if (HiddenModes.ManagerMode)
                    if (sum.VolumeProtected != 0 && AccountStatus.Instance.AccountData != null)
                    {
                        // вычисляем плечо защищенных позиций
                        sum.LeverageProtected = (float)Math.Round(Math.Abs(sum.ConvertExpositionToDepo(sum.Symbol, 
                            accountCurrency, quotes, sum.VolumeProtected > 0 
                            ? QuoteType.Bid : QuoteType.Ask, sum.VolumeProtected)) / (float)AccountStatus.Instance.AccountData.Equity, 2);
                    }

                var quote = QuoteStorage.Instance.ReceiveValue(sum.Symbol);
                var currPrice = quote == null ? 0 : (sum.Side == 1 ? quote.bid : quote.ask);
                sum.ProfitInPoints = sum.Volume == 0 ? 0 : 
                    (int)Math.Round(DalSpot.Instance.GetPointsValue(sum.Symbol, sum.Side == 1 ? (currPrice - sum.AveragePrice) : sum.AveragePrice - currPrice));

                var acData = AccountStatus.Instance.AccountData;
                if (acData != null && acData.Equity != 0)
                    sum.Leverage = (float) Math.Round(Math.Abs(sum.Exposition) / AccountStatus.Instance.AccountData.Equity, 2);
                // вычисляю хеш
                sum.hashCode = 0;
                foreach (var order in sumDeals)
                    sum.hashCode += order.PriceEnter * 1000 + (order.TakeProfit ?? 0) * 50 + 
                        (order.StopLoss ?? 0) * 50 + (order.Swap ?? 0) * 10;

                sum.Equity = string.Format("Позиций:{0}", sumDeals.Count);
                sum.StopLoss = slflag ? sl.ToString() : "Установить";
                sum.TakeProfit = tpflag ? tp.ToString() : "Установить";
                sum.Command = "Закрыть всё";
                summary.Add(sum);
            }
            
            // записываем итоговую информацию
            var overallSum = new PositionSummary { Symbol = "", orders = new List<MarketOrder>() };
            foreach (var item in summary)
            {
                overallSum.Leverage += item.Leverage;
                overallSum.Exposition += Math.Abs(item.Exposition);
                overallSum.Volume += Math.Abs(item.Volume);
                overallSum.Profit += item.Profit;
                overallSum.ProfitInPercent += item.ProfitInPercent;
                overallSum.ProfitInPoints += item.ProfitInPoints;
                if (HiddenModes.ManagerMode)
                {
                    overallSum.VolumeProtected += Math.Abs(item.VolumeProtected);
                    overallSum.LeverageProtected += item.LeverageProtected;
                }
                overallSum.Side = 0;
            }

            var accountData = AccountStatus.Instance.AccountData;
            if (accountData != null && accountData.Equity != 0)
                overallSum.Equity = "Средства = " + accountData.Equity.ToStringUniform(2);
            summary.Add(overallSum);
            if (orders.Count > 0 && orders[0].State == PositionState.Opened)
            {
                AccountStatus.Instance.Leverage = overallSum.Leverage;
                AccountStatus.Instance.Trades = orders.Count;
                AccountStatus.Instance.Points = overallSum.ProfitInPoints;
                AccountStatus.Instance.ProfitInPercents = overallSum.ProfitInPercent;
            }
            else
                AccountStatus.Instance.Trades = 0;
            return summary;
        }

        public override string ToString()
        {
            return string.Format("{0:f0} {1}{2}", Exposition, Symbol, AveragePrice == 0
                      ? "" : string.Format(" [{0:f4}]", AveragePrice));
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Entity;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.BL;

namespace TradeSharp.SiteAdmin.Models.Report
{
    public partial class ReportBroker
    {
        /// <summary>
        /// первая пятерка трейдеров по критерию...
        /// </summary>
        public const int TopRecordsCount = 5;

        /// <summary>
        /// посчитать прибыль по счетам
        /// </summary>
        private void CalculateAccountProfit(TradeSharpConnection ctx,
            List<ACCOUNT> accounts)
        {
            var performers = new List<AccountPerformance>();
            PerformersTopTotalIncome = new List<AccountPerformance>();
            PerformersTopProfitBroker = new List<AccountPerformance>();
            PerformersTopProfitIncome = new List<AccountPerformance>();
            PerformersTopWorstProfitBroker = new List<AccountPerformance>();

            // получить текущие котировки
            var candles = QuoteDatabase.GetCandlesOnTime(null);
            if (candles.Count == 0)
            {
                Errors.Add("CalculateAccountProfit - " + Resource.ErrorMessageQuotNotReceived);
                return;
            }

            // посчитать доходность по каждому счету
            // в валюте брокера            
            foreach (var account in accounts)
            {
                var openProfit = CalculateAccountOpenProfit(ctx, account, candles);
                var acId = account.ID;
                
                // ввод-вывод и профиль по закрытым сделкам
                var profitClosed = 0M;
                var investedTotal = 0M;
                var withdrawnTotal = 0M;
                foreach (var bc in ctx.BALANCE_CHANGE.Where(b => b.AccountID == acId))
                {
                    if (bc.ChangeType == (int)BalanceChangeType.Profit)
                        profitClosed += bc.Amount;
                    else if (bc.ChangeType == (int)BalanceChangeType.Loss)
                        profitClosed -= bc.Amount;
                    else if (bc.ChangeType == (int) BalanceChangeType.Deposit)
                        investedTotal += bc.Amount;
                    else if (bc.ChangeType == (int)BalanceChangeType.Withdrawal)
                        withdrawnTotal += bc.Amount;
                }

                // профит по сделкам...
                var profit = profitClosed + (decimal)openProfit;

                // сводка по счету
                var performer = new AccountPerformance
                    {
                        Account = account,
                        TotalIncome = investedTotal,
                        TotalWithdraw = withdrawnTotal,
                        TotalProfit = profit,
                        TotalProfitPercent = investedTotal == 0 ? 0 : 100*profit/investedTotal
                    };
                // пересчитать в валюту брокера
                string errorStr;
                performer.CalculateResultsInBrokerCurrency(candles, BrokerCurrency, out errorStr);
                if (!string.IsNullOrEmpty(errorStr))
                    Errors.Add(errorStr);
                
                performers.Add(performer);
            }

            // вывести краткую сводку по доходности
            if (performers.Count > 0)
            {
                // средние показатели
                AvgTotalIncome = performers.Average(p => p.TotalIncomeBroker);
                AvgProfitBroker = performers.Average(p => p.TotalProfitBroker);
                AvgProfitIncome = performers.Average(p => p.TotalProfitPercent);

                // топы лучших - худших
                PerformersTopTotalIncome =
                    performers.OrderByDescending(p => p.TotalIncome).Take(TopRecordsCount).ToList();
                PerformersTopTotalIncome =
                    performers.OrderByDescending(p => p.TotalIncome).Take(TopRecordsCount).ToList();
                PerformersTopProfitBroker =
                    performers.OrderByDescending(p => p.TotalProfitBroker).Take(TopRecordsCount).ToList();
                PerformersTopWorstProfitBroker =
                    performers.OrderBy(p => p.TotalProfitBroker).Take(TopRecordsCount).ToList();
                PerformersTopProfitIncome =
                    performers.OrderByDescending(p => p.TotalProfitPercent).Take(TopRecordsCount).ToList();
            }
        }

        /// <summary>
        /// посчитать профит по открытым позам в валюте депозита
        /// </summary>
        private float CalculateAccountOpenProfit(TradeSharpConnection ctx,
                                            ACCOUNT account, Dictionary<string, CandleData> candles)
        {
            var profitOpen = 0f;
            foreach (var pos in ctx.POSITION.Where(p => p.AccountID == account.ID))
            {
                // найти последнюю котировку
                CandleData candle;
                if (!candles.TryGetValue(pos.Symbol, out candle))
                {
                    Errors.Add("CalculateAccountOpenProfit - " + Resource.ErrorMessageNoExitPriceFor + " " + pos.Symbol);
                    continue;
                }

                // получить профит по позиции
                var priceExit = pos.Side > 0
                                    ? candle.close
                                    : DalSpot.Instance.GetAskPriceWithDefaultSpread(pos.Symbol, candle.close);
                var profitCounter = pos.Volume * pos.Side * (priceExit - (float)pos.PriceEnter);

                // перевести профит в валюту депо
                if (!ConvertBaseOrCounterDepo(false, pos.Symbol, account.Currency, candles, ref profitCounter))
                    continue;
                profitOpen += profitCounter;
            }
            return profitOpen;
        }

        private bool ConvertBaseOrCounterDepo(bool useBase,
            string curPair, string depoCurrency,
            Dictionary<string, CandleData> curPrices, ref float value)
        {
            bool inverse, pairsEqual;
            var pair = DalSpot.Instance.FindSymbol(curPair, useBase, depoCurrency, out inverse, out pairsEqual);
            if (pairsEqual) return true;
            if (string.IsNullOrEmpty(pair))
            {
                Errors.Add(string.Format("{3} ConvertBaseOrCounterDepo: {0}, {1}, {2} - {4}",
                        curPair, useBase, depoCurrency, Resource.ErrorMessageErrorIn, Resource.ErrorMessageNotFound));
                return false;
            }
            if (!curPrices.ContainsKey(pair))
            {
                Errors.Add(string.Format("{1} ConvertBaseOrCounterDepo: {0} - {2}", pair, Resource.ErrorMessageErrorIn, Resource.ErrorMessageNotFound));
                return false;
            }
            var price = (curPrices[pair].close +
                DalSpot.Instance.GetAskPriceWithDefaultSpread(pair, curPrices[pair].close)) * 0.5f;
            
            if (price == 0)
            {
                Errors.Add(string.Format("{1} ConvertBaseOrCounterDepo: {0} - {2} = 0", pair, Resource.ErrorMessageErrorIn, Resource.TitlePrice));
                return false;
            }

            value = inverse ? value / price : value * price;
            return true;
        }

    }
}
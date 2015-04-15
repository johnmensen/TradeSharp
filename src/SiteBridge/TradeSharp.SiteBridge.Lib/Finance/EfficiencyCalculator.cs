using System;
using System.Collections.Generic;
using System.Linq;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.QuoteHistory;
using TradeSharp.SiteBridge.Lib.Contract;
using TradeSharp.SiteBridge.Lib.Quotes;
using TradeSharp.Util;

namespace TradeSharp.SiteBridge.Lib.Finance
{
    public class EfficiencyCalculator : IEfficiencyCalculator
    {
        private readonly IDailyQuoteStorage dailyQuoteStorage;
        private readonly IEquityCurveCalculator equityCurveCalculator;

        public EfficiencyCalculator()
        {
        }

        public EfficiencyCalculator(IDailyQuoteStorage dailyQuoteStorage, IEquityCurveCalculator equityCurveCalculator)
        {
            this.equityCurveCalculator = equityCurveCalculator;
            this.dailyQuoteStorage = dailyQuoteStorage;
        }

        public bool Calculate(AccountEfficiency ef)
        {
            if (ef == null)
                throw new ArgumentException("EfficiencyCalculator.Calculate - input ptr is NULL");
            if (ef.Statistics.Account == 0)
                throw new ArgumentException("EfficiencyCalculator.Calculate - input_ptr.AccountId is 0");

            // получить сделки
            var deals = DealStorage.Instance.GetDeals(ef.Statistics.Account);
            ef.openedDeals = new List<MarketOrder>();
            ef.closedDeals = new List<MarketOrder>();

            foreach (var deal in deals)
                if (deal.IsOpened) 
                    ef.openedDeals.Add(deal);
                else
                    ef.closedDeals.Add(deal);

            if (deals.Count == 0)
            {
                Logger.Info("AccountEfficiency.Calculate - нет сделок");
                return false;
            }

            ef.Statistics.DealsCount = deals.Count;
            ef.DealsStillOpened = ef.openedDeals.Count;

            // транзакции
            ef.listTransaction = BalanceStorage.Instance.GetBalanceChanges(ef.Statistics.Account);
            if (ef.listTransaction == null || ef.listTransaction.Count == 0)
            {
                Logger.Info("AccountEfficiency.Calculate - нет транзакций");
                return false;
            }

            Logger.Info("AccountEfficiency.Calculate(" + ef.Statistics.Account + ")");

            // время отсчета - время первого заведения средств
            var startDate = ef.listTransaction.Min(t => t.ValueDate);

            // получить список используемых котировок
            var symbolsUsed = ef.closedDeals.Select(d => d.Symbol).Union(ef.openedDeals.Select(o => o.Symbol)).Distinct().ToList();
            // ... в т.ч., котировок для перевода базовой валюты в валюту депо (плечо)
            // и перевода контрвалюты в валюту депо (профит)
            var symbolsMore = new List<string>();
            foreach (var smb in symbolsUsed)
            {
                bool inverse, eq;
                var smbBase = DalSpot.Instance.FindSymbol(smb, true, ef.Statistics.DepoCurrency, out inverse, out eq);
                if (!string.IsNullOrEmpty(smbBase)) symbolsMore.Add(smbBase);
                var smbCounter = DalSpot.Instance.FindSymbol(smb, false, ef.Statistics.DepoCurrency, out inverse, out eq);
                if (!string.IsNullOrEmpty(smbCounter)) symbolsMore.Add(smbCounter);
            }
            symbolsUsed.AddRange(symbolsMore);
            symbolsUsed = symbolsUsed.Distinct().ToList();

            // котировки
            var dicQuote = new Dictionary<string, List<QuoteData>>();
            foreach (var smb in symbolsUsed)
            {
                dicQuote.Add(smb,
                    dailyQuoteStorage.GetQuotes(smb).Select(q => new QuoteData(q.b, q.b, q.a)).ToList());
            }
            //TickerStorage.Instance.GetQuotes(symbolsUsed.ToDictionary(s => s, s => (DateTime?)null));
            if (dicQuote == null || dicQuote.Count == 0)
            {
                Logger.Info("AccountEfficiency.Calculate - нет котировок");
                return false;
            }

            if (ef.openedDeals.Count > 0)
            {
                foreach (var t in ef.openedDeals)
                {
                    List<QuoteData> dicQuoteValue;
                    if (!dicQuote.TryGetValue(t.Symbol, out dicQuoteValue))
                        Logger.Error(String.Format("Symbol {0} was not found in dicQuote", t.Symbol));
                    else
                        if (dicQuoteValue.Count == 0)
                            Logger.Error(String.Format("No quote data for symbol {0}", t.Symbol));
                        else
                            t.PriceExit = dicQuoteValue.Last().GetPrice(t.Side == 1
                                ? QuoteType.Bid : QuoteType.Ask);
                }
            }

            var quoteArc = new QuoteArchive(dicQuote);
            AccountPerformanceRaw performance;
            try
            {
                performance = equityCurveCalculator.CalculateEquityCurve(deals, ef.Statistics.DepoCurrency, quoteArc, ef.listTransaction);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в EfficiencyCalculator.CalculateEquityCurve()", ex);
                return false;
            }

            ef.Statistics.TotalTradedInDepoCurrency = performance.totalTradedVolume;
            var lstEquity = performance.equity;
            if (lstEquity == null) return false;
            ef.listLeverage = performance.leverage;

            // исключить пустые значения с начала отсчета
            ef.listEquity = new List<EquityOnTime>();
            var startCopy = false;
            foreach (var eq in lstEquity)
            {
                if (eq.equity > 0) startCopy = true;
                if (startCopy) ef.listEquity.Add(eq);
            }

            if (ef.listEquity.Count == 0) return false;
            ef.StartDate = startDate;
            ef.InitialBalance = ef.listEquity[0].equity;

            // рассчитать коэффициенты доходности
            CalculateProfitCoeffs(ef);
            CalculateRiskCoeffs(ef);

            // актуальные котировки
            ef.currentQuotes = quoteArc.GetCurrentQuotes();

            ef.Statistics.Chart = ef.listProfit1000 == null ||
                                         ef.listProfit1000.Count == 0
                                             ? new byte[MiniChartPacker.profitChartPointCount]
                                             : MiniChartPacker.PackChartInArray(ef.listProfit1000);

            // дней торгует
            var startDayOpen = DateTime.Now;
            if (ef.openedDeals.Count > 0)
                startDayOpen = ef.openedDeals.Max(d => d.TimeEnter);
            if (ef.closedDeals.Count > 0)
            {
                var dateClosed = ef.closedDeals.Min(d => d.TimeEnter);
                if (dateClosed < startDayOpen)
                    startDayOpen = dateClosed;
            }
            ef.Statistics.DaysTraded = (int)Math.Round((DateTime.Now - startDayOpen).TotalDays);

            // сумма профитных сделок (результат) к сумме убыточных сделок
            var sumProf = ef.closedDeals.Sum(d => d.ResultDepo > 0 ? d.ResultDepo : 0) +
                ef.openedDeals.Sum(d => d.ResultDepo > 0 ? d.ResultDepo : 0);
            var sumLoss = ef.closedDeals.Sum(d => d.ResultDepo > 0 ? d.ResultDepo : 0) +
                ef.openedDeals.Sum(d => d.ResultDepo > 0 ? d.ResultDepo : 0);
            ef.Statistics.AvgWeightedDealProfitToLoss = sumLoss == 0 && sumProf == 0
                                                           ? 0
                                                           : 100 * sumProf / (sumProf + sumLoss);
            var dateFirst = DateTime.Now.Date.AddMonths(-3);
            ef.Statistics.WithdrawalLastMonths = (float)ef.listTransaction.Sum(t =>
                                                                               (t.ChangeType ==
                                                                                BalanceChangeType.Withdrawal &&
                                                                                t.ValueDate >= dateFirst)
                                                                                   ? t.AmountDepo : 0);
            // профит в ПП
            ef.Statistics.SumProfitPoints = ef.closedDeals.Sum(d => d.ResultPoints);

            return true;
        }

        public void CalculateProfitCoeffs(AccountEfficiency ef)
        {
            if (ef.listEquity == null) return;
            if (ef.listEquity.Count == 0) return;

            // посчитать TWR
            var lastEq = ef.listEquity[ef.listEquity.Count - 1].equity;
            ef.Statistics.Equity = lastEq;

            var dueTransactions = ef.listTransaction.Where(t => t.ChangeType == BalanceChangeType.Deposit ||
                                                         t.ChangeType == BalanceChangeType.Withdrawal).ToList();
            // вычесть стартовый баланс - первую транзакцию
            if (dueTransactions.Count > 0)
                dueTransactions.RemoveAt(0);
            var sumTrans = (float)dueTransactions.Sum(t => t.SignedAmountDepo);

            var absTWR = ef.InitialBalance == 0 ? 0 : (lastEq - sumTrans) / ef.InitialBalance;
            ef.Statistics.Profit = (absTWR - 1f) * 100f;

            // Посчитать относительные доходности на дату
            // Pi = 1 + (Di - Di-1 - SUM(T)) / Di
            // P - доходность, D - депозит, SUM(T) - сумма транзакций с момента i-1 по i
            var listTWR = new List<float>();
            var listDatedTWR = new List<EquityOnTime>();

            for (var i = 1; i < ef.listEquity.Count; i++)
            {
                var time = ef.listEquity[i].time;
                var sumT = 0f;
                for (var j = 0; j < dueTransactions.Count(); j++)
                {
                    var trans = dueTransactions[j];
                    if (trans.ValueDate > time) continue;
                    sumT += (float)trans.SignedAmountDepo;
                    dueTransactions.RemoveAt(j);
                    j--;
                }
                var depo = ef.listEquity[i].equity;
                var depoPrev = ef.listEquity[i - 1].equity;
                if (depoPrev == 0) continue;
                var twr = 1 + (depo - depoPrev - sumT) / depoPrev;
                listTWR.Add(twr);
                listDatedTWR.Add(new EquityOnTime(twr, time));
            }
            if (listTWR.Count < 2) return;

            // Шарп
            var sko = 0f;
            var avgTWR = listTWR.Average();
            for (var i = 0; i < listTWR.Count; i++)
            {
                var deltaTWR = listTWR[i] - avgTWR;
                sko += (deltaTWR * deltaTWR);
            }
            ef.Statistics.Sharp = sko == 0 ? 0 : (absTWR - 1) / (float)Math.Sqrt(sko);

            // макс. проседание
            CalcDrawDown(ef);

            // доходность на 1000
            if (ef.listProfit1000 == null)
                CalcProfit1000(ef, listDatedTWR);

            // среднегеометрическая
            var prodProfit = listTWR.Product(t => t);
            if (prodProfit < 0) prodProfit = 0;

            ef.ProfitGeomMonth = prodProfit == 0 ? 0 : 100 * ((float)Math.Pow(prodProfit, 20.5f / listDatedTWR.Count) - 1);
            ef.ProfitGeomYear = prodProfit == 0 ? 0 : 100 * ((float)Math.Pow(prodProfit, 250f / listDatedTWR.Count) - 1);
            ef.Statistics.AvgYearProfit = ef.ProfitGeomYear;

            // доходность за последние N месяцев
            // по кривой доходности на 1 000 долларов
            if (ef.listProfit1000.Count > 1)
            {
                const int months = -3;
                var dateOld = DateTime.Now.Date.AddMonths(months);
                var lastEquityDate = ef.listProfit1000[0];
                var curEquity = ef.listProfit1000[ef.listProfit1000.Count - 1].equity;
                if (lastEquityDate.time.Date < dateOld)
                {
                    lastEquityDate = ef.listProfit1000.FirstOrDefault(p => p.time >= dateOld);
                    if (lastEquityDate.Equals(default(EquityOnTime)))
                        lastEquityDate = ef.listProfit1000[0];
                }
                var delta = curEquity - lastEquityDate.equity;
                ef.Statistics.ProfitLastMonths = lastEquityDate.equity == 0
                                        ? (curEquity == 0 ? 0 : 100)
                                        : 100 * delta / lastEquityDate.equity;

                // доходность за N месяцев, по абс. величине
                ef.Statistics.ProfitLastMonthsAbs = (ef.closedDeals == null || ef.closedDeals.Count == 0) ? 0 :
                    ef.closedDeals.Where(d => d.TimeExit >= dateOld).Sum(d => d.ResultDepo);
            }
        }

        /// <summary>
        /// считаются по кривой плеча
        /// </summary>
        private void CalculateRiskCoeffs(AccountEfficiency ef)
        {
            if (ef.closedDeals.Count == 0 && ef.openedDeals.Count == 0) return;
            if (ef.listLeverage == null || ef.listLeverage.Count == 0) return;

            // наибольшее плечо в момент
            ef.Statistics.MaxLeverage = ef.listLeverage.Max(l => l.equity);

            // среднее плечо (без 0-х значений)
            var countLev = ef.Statistics.MaxLeverage == 0 ? 0 : ef.listLeverage.Count(l => l.equity > 0);
            ef.Statistics.AvgLeverage = countLev == 0 ? 0 : ef.listLeverage.Sum(l => l.equity) / countLev;

            // коэффициент жадности - средний профит по сделке, деленный на средний убыток
            var countProfit = ef.closedDeals.Count(d => d.ResultDepo > 0);
            var countLoss = ef.closedDeals.Count(d => d.ResultDepo > 0);
            if (countProfit > 0 || countLoss > 0)
            {
                var avgProfit = ef.closedDeals.Sum(d => d.ResultDepo > 0 ? d.ResultDepo : 0) / countProfit;
                var avgLoss = ef.closedDeals.Sum(d => d.ResultDepo < 0 ? -d.ResultDepo : 0) / countLoss;
                ef.Statistics.GreedyRatio = avgLoss == 0 ? 0 : avgProfit / avgLoss;
            }
        }

        private void CalcDrawDown(AccountEfficiency ef)
        {
            float drawDown = 0;
            float drawStart = 0;
            for (var i = 0; i < ef.listEquity.Count; i++)
            {
                var level = ef.listEquity[i].equity;
                var curDD = 0f;
                var j = i + 1;
                var probablyDrawStart = level;
                for (; j < ef.listEquity.Count; j++)
                {
                    var cl = ef.listEquity[j].equity;
                    if (cl >= level) break;
                    var delta = cl - level;
                    if (delta < curDD) curDD = delta;
                }
                i = j;
                if (curDD < drawDown)
                {
                    drawDown = curDD;
                    drawStart = level;
                }
            }

            if (drawStart > 0 && drawDown < 0)
            {
                ef.Statistics.MaxRelDrawDown = 100 * (-drawDown) / drawStart;
            }
        }

        private void CalcProfit1000(AccountEfficiency ef, List<EquityOnTime> listDatedTWR)
        {
            const float startBalance = 1000;
            if (listDatedTWR.Count == 0) return;
            ef.listProfit1000 = new List<EquityOnTime> { new EquityOnTime(startBalance, ef.StartDate) };
            var balance = startBalance;
            for (var i = 0; i < listDatedTWR.Count; i++)
            {
                var twr = listDatedTWR[i];
                balance = balance * twr.equity;

                // Хардкод на последнее число - дублировать запись                    
                if (i == listDatedTWR.Count - 1 && i > 0 &&
                    listDatedTWR[i].time.Month != listDatedTWR[i - 1].time.Month) // !!
                {
                    var doubleDate = twr.time;
                    doubleDate = doubleDate.Hour < 12 ? doubleDate.AddHours(1) : doubleDate.AddHours(-1);
                    ef.listProfit1000.Add(new EquityOnTime(balance, doubleDate));
                }
                ef.listProfit1000.Add(new EquityOnTime(balance, twr.time));
            }
        }
    }    
}
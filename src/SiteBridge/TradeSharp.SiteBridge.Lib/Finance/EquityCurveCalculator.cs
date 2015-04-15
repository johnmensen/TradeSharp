using System;
using System.Collections.Generic;
using System.Linq;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.QuoteHistory;
using TradeSharp.Util;

namespace TradeSharp.SiteBridge.Lib.Finance
{
    public class EquityCurveCalculator : IEquityCurveCalculator
    {
        public AccountPerformanceRaw CalculateEquityCurve(List<MarketOrder> deals, string depoCurrency, QuoteArchive quoteArc, List<BalanceChange> transactions)
        {
            var timeStart = transactions.Min(t => t.ValueDate);
            return CalculateEquityCurve(deals, depoCurrency, quoteArc, transactions, timeStart, DateTime.Now);
        }

        public AccountPerformanceRaw CalculateEquityCurve(List<MarketOrder> deals, string depoCurrency, QuoteArchive quoteArc,
            List<BalanceChange> transactions, DateTime timeStart, DateTime timeEnd)
        {
            Logger.InfoFormat("CalculateEquityCurve(closedDeals:{0}, quotes:{1}, transactions:{2})",
                              deals.Count, quoteArc.dicQuote.Count, transactions.Count);

            var performance = new AccountPerformanceRaw();
            if (transactions.Count == 0) return performance;
            // скопировать списки сделок и транзакций, т.к. они будут модифицированы
            var dueDeals = deals.ToList();
            var dueTransactions = transactions.ToList();

            // посчитать результат
            decimal balance = 0;
            

            var lstEq = performance.equity;
            var lstLev = performance.leverage;

            // сформировать список дат - момент входа в рынок, конец дня
            // флаг (второй параметр) - означает конец дня
            var dateList = new List<Cortege2<DateTime, bool>>();
            for (var date = timeStart.Date; date <= timeEnd; date = date.AddDays(1))
            {
                var nextDate = date.AddDays(1);
                // сделки за день
                for (var i = 0; i < dueDeals.Count; i++)
                {
                    if (dueDeals[i].TimeEnter > nextDate) continue;
                    dateList.Add(new Cortege2<DateTime, bool>(dueDeals[i].TimeEnter, false));
                    dueDeals.RemoveAt(i);
                    i--;
                }

                // конец дня
                dateList.Add(new Cortege2<DateTime, bool>(date, true));
            }
            dateList = dateList.OrderBy(d => d.a).ToList();
            dueDeals = deals.ToList();

            foreach (var datePart in dateList)
            {
                var date = datePart.a;
                var isEndOfDay = datePart.b;

                if (DaysOff.Instance.IsDayOff(date)) continue;
                // добавить сумму транзакции
                for (var i = 0; i < dueTransactions.Count; i++)
                {
                    var trans = dueTransactions[i];
                    if (date < trans.ValueDate) continue;
                    balance += trans.SignedAmountDepo;
                    dueTransactions.RemoveAt(i);
                    i--;
                }
                if (balance == 0) continue;

                // посчитать результат по открытым сделкам
                // и суммировать плечи по открытым сделкам
                var openResult = 0f;
                for (var i = 0; i < dueDeals.Count; i++)
                {
                    var deal = dueDeals[i];
                    // удалить закрытую сделку
                    if (deal.TimeExit.HasValue && deal.TimeExit.Value < date)
                    {
                        // учесть объем сделки второй раз - на закрытии
                        performance.totalTradedVolume += (long)Math.Round(deal.VolumeInDepoCurrency);
                        dueDeals.RemoveAt(i);
                        i--;
                        continue;
                    }

                    if (date < deal.TimeEnter || date > deal.TimeExit) continue;
                    // получить текущую котиру
                    if (!quoteArc.dicQuote.ContainsKey(deal.Symbol)) continue;
                    var quote = quoteArc.GetQuoteOnDateSeq(deal.Symbol, date);
                    if (quote == null) continue;

                    if (deal.VolumeInDepoCurrency == 0)
                    {
                        CalculateDealVolumeInDepoCurrency(deal, quote, depoCurrency, quoteArc, date);
                        performance.totalTradedVolume += (long)Math.Round(deal.VolumeInDepoCurrency);
                    }

                    // текущая прибыль по сделке
                    var dealProfit = deal.CalculateProfit(quote);
                    // перевести в валюту депо
                    dealProfit = CalculateProfitInDepoCurx(false, dealProfit, deal.Symbol, depoCurrency, quoteArc, date);
                    openResult += dealProfit;
                }

                // посчитать плечо по сделкам
                var dealByTicker = dueDeals.Where(d => d.TimeEnter < date).GroupBy(x => x.Symbol).ToDictionary(d => d.Key, d => d.ToList());
                var sumVolumeDepo = dealByTicker.Sum(d =>
                    {
                        var totalVolume = Math.Abs(d.Value.Sum(order => order.Side*order.Volume));
                        if (totalVolume == 0) return 0;
                        return CalculateProfitInDepoCurx(true, totalVolume, d.Key, depoCurrency, quoteArc, date);
                    });

                var equity = balance + (decimal)openResult;
                var leverage = equity == 0 ? 0 : (decimal)sumVolumeDepo / equity;

                // добавить точки в кривые 
                if (isEndOfDay)
                    lstEq.Add(new EquityOnTime((float)equity, date));
                lstLev.Add(new EquityOnTime((float)leverage, date));
            }

            var dateNow = DateTime.Now;
            // открытым сделкам посчитать текущую прибыль / убыток
            foreach (var deal in dueDeals)
            {
                var quote = quoteArc.GetLastQuote(deal.Symbol, dateNow);
                if (quote == null) continue;

                // условно - цена выхода
                deal.PriceExit = deal.Side > 0 ? quote.bid : quote.ask;
                var dealProfit = deal.CalculateProfit(quote);
                // перевести в валюту депо
                dealProfit = CalculateProfitInDepoCurx(false, dealProfit, deal.Symbol, depoCurrency, quoteArc, dateNow);
                deal.ResultDepo = dealProfit;
            }

            return performance;
        }

        /// <summary>
        /// Пересчитать указанный профит по сделке в профит в валюте депозита
        /// </summary>
        public float CalculateProfitInDepoCurx(bool useBase, float profit, string dealTicker, string depoCurrency, QuoteArchive arc, DateTime date)
        {
            bool inverse, pairsAreEqual;
            var smb = DalSpot.Instance.FindSymbol(dealTicker, useBase, depoCurrency, out inverse, out pairsAreEqual);
            if (pairsAreEqual) return profit;

            if (string.IsNullOrEmpty(smb)) return 0;
            // найти котиру по символу
            var quote = arc.GetQuoteOnDateSeq(smb, date);
            if (quote == null) return 0;
            return inverse
                       ? profit / quote.GetPrice(QuoteType.Middle)
                       : profit * quote.GetPrice(QuoteType.Middle);
        }

        /// <summary>
        /// Расчитать объем сделки в валюте депозита
        /// </summary>
        public void CalculateDealVolumeInDepoCurrency(MarketOrder deal, QuoteData dealQuote, string depoCurrency, QuoteArchive arc, DateTime date)
        {
            var volumeInCounter = deal.Volume * dealQuote.GetPrice(QuoteType.NonSpecified);

            // перевести из контрвалюты в валюту депо            
            bool inverse, pairsAreEqual;
            var smb = DalSpot.Instance.FindSymbol(deal.Symbol, false, depoCurrency, out inverse, out pairsAreEqual);
            if (pairsAreEqual)
            {
                deal.VolumeInDepoCurrency = volumeInCounter;
                return;
            }

            if (string.IsNullOrEmpty(smb)) return;
            var quoteCtDepo = arc.GetQuoteOnDateSeq(smb, date);
            if (quoteCtDepo == null) return;
            var priceCtDepo = quoteCtDepo.GetPrice(QuoteType.NonSpecified);
            if (priceCtDepo == 0) return;

            deal.VolumeInDepoCurrency = inverse ? volumeInCounter / priceCtDepo : volumeInCounter * priceCtDepo;
        }
    }
}

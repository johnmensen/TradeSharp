using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace Candlechart
{
    /*
     * в модуле собран код по обработке котировок, подгружаемых "на лету"
     */

    public partial class CandleChartControl
    {
        private DateTime timeOfLastUpdate = DateTime.Now;
        private const int MinIntervalBetweenUpdates = 1000;
        
        /// <param name="newQuotes">обновленные котировки</param>
        /// <returns>нужна перерисовка</returns>
        public bool ProcessQuotes(QuoteData[] newQuotes)
        {
            if (newQuotes == null) return false;
            // выдерживание минимального интервала между обновлениями
            var deltaMills = (DateTime.Now - timeOfLastUpdate).TotalMilliseconds;
            if (deltaMills < MinIntervalBetweenUpdates) 
                return false; 

            timeOfLastUpdate = DateTime.Now;
            // доукомплектовать свечки
            if (newQuotes.Length == 0) return false;
            if (candlePacker == null) return false;
            var candles = chart.StockSeries.Data.Candles;
            // обновленная свечка и список добавленных свечек
            CandleData updatedCandle = null;
            var newCandles = new List<CandleData>();

            foreach (var q in newQuotes)
            {
                if (q.bid == 0 || q.ask == 0)
                {
                    Logger.ErrorFormat("Nil quote: {0} at {1}",
                                       Symbol, q.time, q.bid == 0 && q.ask == 0 ? " both" : "");
                    continue;
                }
                var candle = candlePacker.UpdateCandle(q.bid, q.time);
                if (candle == null) continue; 
                
                // либо завершена старая свечка, либо слеплена новая
                if (candles.Count > 0)
                    if (candles[candles.Count - 1].timeOpen == candle.timeOpen)
                    {
                        candles[candles.Count - 1] = new CandleData(candle); // заменить обновленную
                        // запомнить обновленную свечку
                        updatedCandle = candles[candles.Count - 1];
                        continue;
                    }
                candles.Add(new CandleData(candle));
                // запомнить добавленную свечку
                newCandles.Add(candles[candles.Count - 1]);
            }
            // если сформировалась, но не закончена новая свеча...
            if (candlePacker.CurrentCandle != null)
            {
                var candleUpdated = false;
                if (candles.Count > 0)
                    if (candles[candles.Count - 1].timeOpen == candlePacker.CurrentCandle.timeOpen)
                    {
                        candles[candles.Count - 1] = new CandleData(candlePacker.CurrentCandle); // заменить обновленную
                        // запомнить обновленную свечку
                        updatedCandle = candles[candles.Count - 1];
                        candleUpdated = true;
                    }
                if (!candleUpdated)
                {
                    candles.Add(new CandleData(candlePacker.CurrentCandle));
                    // запомнить добавленную свечку
                    newCandles.Add(candles[candles.Count - 1]);
                }
            }

            // обновить границу интервала
            if (newQuotes.Length > 0)
                chart.EndTime = newQuotes[newQuotes.Length - 1].time;

            // обработать котировки в индикаторах
            ProcessQuotesByIndicators(updatedCandle, newCandles);
            
            if (chart.StockSeries.ShowLastQuote)
            {
                float? price = newCandles.Count > 0
                     ? newCandles[newCandles.Count - 1].close
                     : updatedCandle != null
                     ? (float?)updatedCandle.close : null;
                if (price.HasValue)
                    chart.Panes.StockPane.YAxis.CurrentPrice = price;
            }

            if (chart.StockSeries.AutoScroll)
                chart.ScrollToEnd();
            return true;            
        }        

        public bool ProcessNews(News[] news)
        {
            if (onNewsReceived != null)
            {
                onNewsReceived(news);
                return true;
            }
            return false;
        }
        
        public bool ProcessOrders(MarketOrder[] posList)
        {
            if (onPositionsReceived != null)
            {
                onPositionsReceived(posList);
                return true;
            }
            return false;
        }

        public bool ProcessClosedOrders(List<MarketOrder> posList)
        {
            if (onClosedOrdersReceived != null)
            {
                onClosedOrdersReceived(posList);
                return true;
            }
            return false;
        }

        public bool ProcessPendingOrders(PendingOrder[] ordList)
        {
            if (onPendingOrdersReceived != null)
            {
                onPendingOrdersReceived(ordList);
                return true;
            }
            return false;
        }

        public void RedrawChartSafe()
        {            
            if (ParentForm != null)
                if (ParentForm.WindowState != FormWindowState.Minimized)
                {           
                    chart.RepaintAndInvalidate();                    
                }            
        }
    }
}

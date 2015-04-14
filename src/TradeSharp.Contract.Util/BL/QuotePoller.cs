using System;
using System.Collections.Generic;
using System.Threading;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Contract.Util.BL
{
    /// <summary>
    /// опрашивает хеш котировок QuoteStorage с заданным интервалом
    /// прочитав котировки, инициирует ивенты по соответствующим валютным парам
    /// </summary>
    public class QuotePoller
    {
        private readonly int pollInterval;
        private Thread pollThread;
        private volatile bool isStopping;
        private QuoteHashUpdatedDel onQuoteHashUpdated;
        public event QuoteHashUpdatedDel OnQuoteHashUpdated
        {
            add { onQuoteHashUpdated += value; }
            remove { onQuoteHashUpdated -= value; }
        }
        private readonly Dictionary<string, QuoteData> oldQuotes =
            new Dictionary<string, QuoteData>();
        private readonly static ManualResetEvent cellsAreUpdated = new ManualResetEvent(true);

        public QuotePoller(int pollInterval)
        {
            this.pollInterval = pollInterval;
        }

        private void PollRoutine()
        {
            while (!isStopping)
            {
                Thread.Sleep(pollInterval);
                // запросить котировки
                var quotes = QuoteStorage.Instance.ReceiveAllData();
                // сравнить со старыми значениями
                var newNames = new List<string>();
                var newPrices = new List<QuoteData>();

                foreach (var pair in quotes)
                {
                    if (!oldQuotes.ContainsKey(pair.Key))
                    {
                        newNames.Add(pair.Key);
                        newPrices.Add(pair.Value);
                        oldQuotes.Add(pair.Key, pair.Value);
                        continue;
                    }
                    var quote = oldQuotes[pair.Key];
                    if (quote.PricesAreSame(pair.Value)) continue;
                    oldQuotes[pair.Key] = pair.Value;
                    newNames.Add(pair.Key);
                    newPrices.Add(pair.Value);
                }
                // вызвать событие

                if (newNames.Count > 0)
                {                    
                    if (onQuoteHashUpdated != null)
                    {
                        
                        cellsAreUpdated.Reset();
                        try
                        {
                            onQuoteHashUpdated(newNames, newPrices);
                        }
                        finally
                        {
                            cellsAreUpdated.Set();
                        }              
                            
                    }                    
                }
            }
        }

        public void StartPolling()
        {
            isStopping = false;
            pollThread = new Thread(PollRoutine);
            pollThread.Start();
        }

        public void StopPolling()
        {
            if (pollThread == null) return;
            isStopping = true;
            cellsAreUpdated.WaitOne();
            pollThread.Join();
        }
    }

    public delegate void QuoteHashUpdatedDel(List<string> names, List<QuoteData> quotes);
}
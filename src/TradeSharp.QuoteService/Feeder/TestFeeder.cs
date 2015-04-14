using System;
using System.Collections.Generic;
using System.Threading;
using Entity;
using TradeSharp.Contract.Entity;

namespace TradeSharp.QuoteService.Feeder
{
    class TestFeeder : IQuoteFeeder
    {
        private bool stopFlag;
        private Thread listenThread;        
        
        private QuotesReceived onQuotesReceived;
        public event QuotesReceived OnQuotesReceived
        {
            add { onQuotesReceived += value; }
            remove { onQuotesReceived -= value; }
        }

        public void Start()
        {
            stopFlag = false;
            listenThread = new Thread(GenerateLoop);
            listenThread.Start();            
        }

        public void Stop()
        {
            if (listenThread == null) return;
            stopFlag = true;            
            listenThread = null;
        }

        private void GenerateLoop()
        {
            var genPass = new Random();
            var quoteNames = new[] {"EURUSD", "GBPUSD", "USDJPY", "USDCHF"};
            var quoteMO = new[] {1.2858f, 1.4261f, 96.45f, 0.9875f};
            var quoteSig = new[] {0.0003f, 0.0004f, 0.03f, 0.0003f};

            while (!stopFlag)
            {
                var lstNames = new List<string>();
                var lstQuotes = new List<QuoteData>();

                for (var i = 0; i < quoteNames.Length; i++)
                {
                    // случайно пропустить пару
                    if (genPass.Next(100) > 70) continue;
                    // расчитать новое значение
                    quoteMO[i] = quoteMO[i] + quoteSig[i]*(float) Gaussian.BoxMuller(0, 0.00015f);
                    lstNames.Add(quoteNames[i]);
                    lstQuotes.Add(new QuoteData(quoteMO[i], quoteMO[i] + quoteSig[i], DateTime.Now));
                    if (onQuotesReceived != null)
                        onQuotesReceived(lstNames, lstQuotes);
                }

                Thread.Sleep(200);
            }
        }
    }
}

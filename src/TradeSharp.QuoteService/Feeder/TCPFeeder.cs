using System;
using System.Collections.Generic;
using System.Threading;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.QuoteService.Feeder
{
    class TCPFeeder : IQuoteFeeder
    {
        private TcpReceiverClient receiver;
        /// <summary>
        /// таймаут проверяется в ProcessQuotesLoop
        /// </summary>
        private bool reconnectOnTimeout = AppConfig.GetBooleanParam("QuoteStream.ShouldReconnect", true);
        private int timeoutIntervalMils = AppConfig.GetIntParam("QuoteStream.ReconnectTimoutMils", 10 * 1000);
        private readonly ThreadSafeTimeStamp lastMessageReceived = new ThreadSafeTimeStamp();
        private volatile bool isStopping;
        private Thread processQuotesThread;

        public void Start()
        {
            StartTcpReceiver();

            processQuotesThread = new Thread(ProcessQuotesLoop);
            lastMessageReceived.Touch();
            processQuotesThread.Start();
        }

        void OnTcpPacketReceived(string data)
        {
            lastMessageReceived.Touch();
            List<string> names;
            List<QuoteData> quotes;
            //Logger.InfoFormat("TCP Feeder: data is {0}", data);
            QuoteFeederParser.ParseQuotes(data, out names, out quotes);
            //Logger.InfoFormat("TCP Feeder: got {0} quotes", quotes.Count);
            if (quotesReceived != null)
                quotesReceived(names, quotes);
        }

        public void Stop()
        {
            receiver.Stop();
            isStopping = true;
            if (processQuotesThread != null) processQuotesThread.Join();
        }

        private void ProcessQuotesLoop()
        {
            while (!isStopping)
            {
                Thread.Sleep(200);
                if (reconnectOnTimeout)
                    CheckTimeout();
            }
        }

        private QuotesReceived quotesReceived;
        public event QuotesReceived OnQuotesReceived
        {
            add { quotesReceived += value; }
            remove { quotesReceived -= value; }
        }

        private void CheckTimeout()
        {
            // поток котировок может быть не включен в рабочий день
            if (!WorkingDay.Instance.IsWorkingDay(DateTime.Now)) return;
            var delta = (DateTime.Now - lastMessageReceived.GetLastHit()).TotalMilliseconds;
            if (delta < timeoutIntervalMils) return;
            Logger.Error("TCPFeeder: сработал таймаут TCP получателя котировок, перезапускаем");
            // инициировать таймаут
            /*receiver.Stop();
            lastMessageReceived.Touch();

            StartTcpReceiver();*/
        }

        private void StartTcpReceiver()
        {
            var targetSock = AppConfig.GetStringParam("TCP.Source", "127.0.0.1:8600");
            var parts = targetSock.Split(':');
            var host = parts[0];
            var port = parts[1].ToInt();
            receiver = new TcpReceiverClient(host, port);
            receiver.OnTcpPacketReceived += OnTcpPacketReceived;
            receiver.Connect();
        }
    }
}

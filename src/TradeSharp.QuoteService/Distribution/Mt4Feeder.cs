using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.QuoteService.Distribution
{
    class Mt4Quote
    {
        public string Symbol { get; set; }
        public float Bid { get; set; }
        public float Ask { get; set; }
        public int Volume { get; set; }

        public Mt4Quote(string symbol, float bid, float ask)
        {
            Symbol = symbol;
            Bid = bid;
            Ask = ask;
        }

        public Mt4Quote(string symbol, QuoteData quote)
        {
            Symbol = symbol;
            Bid = quote.bid;
            Ask = quote.ask;
        }

        public string ToStringMtFormat()
        {
            return string.Format(CultureProvider.Common, "{0} {1} {2};", Symbol, Bid, Ask);
        }
    }

    class Mt4Feeder
    {
        private static Mt4Feeder instance;
        public static Mt4Feeder Instance
        {
            get
            {
                if (instance != null) return instance;
                instance = new Mt4Feeder();
                return instance;
            }
        }

        private readonly Encoding encoding;
        private readonly FloodSafeLogger logNoFlood = new FloodSafeLogger(1000);
        private const int LogMsgDeliverQuote = 1;

        private Mt4Feeder()
        {
            var encodingString = AppConfig.GetStringParam("MT4.Encoding", "windows-1251");
            encoding = Encoding.GetEncoding(encodingString);
            var listenPort = AppConfig.GetIntParam("Mt4Feeder.TcpPort", 19001);
            try
            {
                server = new TcpServer(listenPort, encoding) { onMessage = OnMessage };
                Logger.InfoFormat("Mt4Feeder - слушает порт {0}", listenPort);
            }
            catch (Exception ex)
            {
                server = null;
                Logger.Error("Ошибка при создании TCP-сервера", ex);
            }            
        }

        private static void OnMessage(string msg, TcpClient client)
        {
            Logger.InfoFormat("Mt4Feeder - клиент подключился ({0})", msg);
            //server.SendToClient("Login: successfull", client);
        }

        /// <summary>
        /// список объектов для отправки - котировки и новости
        /// </summary>
        private readonly List<Mt4Quote> messages = new List<Mt4Quote>();
        /// <summary>
        /// синхро-объект для messages
        /// </summary>
        private readonly object locker = new object();
        private volatile bool stopFlag;
        private const int DistributeTimeout = 1000;
        private Thread threadDistribute;
        private readonly TcpServer server;

        public void Enqueue(List<string> names, List<QuoteData> quotes)
        {
            if (server == null) return;
            var quotesMt4 = new List<Mt4Quote>();
            for (var i = 0; i < names.Count; i++)
            {
                quotesMt4.Add(new Mt4Quote(names[i], quotes[i]));
            }

            logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info,
                LogMsgDeliverQuote, 1000 * 60 * 15,
                "Mt4Feeder - доставка {0} котировок", quotesMt4.Count);

            lock (locker)
            {
                messages.AddRange(quotesMt4);
            }
        }

        public void Stop()
        {
            if (server == null) return;
            server.Stop();
            stopFlag = true;
            threadDistribute.Join();
        }

        public void Start()
        {
            if (server == null) return;
            threadDistribute = new Thread(ThreadFunc);
            threadDistribute.Start();
        }

        private void ThreadFunc()
        {
            while (!stopFlag)
            {
                DistributeMessages();
                Thread.Sleep(DistributeTimeout);
            }
        }

        private void DistributeMessages()
        {
            var messageStr = "";
            lock (locker)
            {
                // отправить все сообщения (котировки)
                foreach (var obj in messages)
                {
                    messageStr += obj.ToStringMtFormat();
                }
                messages.Clear();                
            }
            try
            {
                SendMessages(messageStr);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка рассылки TCP-сообщения", ex);
            }
        }

        public void SendMessages(string quoteString)
        {
            if (string.IsNullOrEmpty(quoteString)) return;
            var parts = quoteString.Split(';');

            var sb = new StringBuilder();
            var curLength = 0;
            foreach (var part in parts)
            {
                sb.AppendFormat("{0}{1}{2}", part, (char)0x0D, (char)0x0A);
                curLength++;
                if (curLength == 32)
                {
                    server.SendToAll(encoding.GetBytes(sb.ToString()));
                    curLength = 0;
                    sb = new StringBuilder();
                }
            }
            if (sb.Length > 0) server.SendToAll(encoding.GetBytes(sb.ToString()));
        }
    }    
}

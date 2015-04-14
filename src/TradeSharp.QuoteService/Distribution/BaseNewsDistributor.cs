using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.QuoteService.Distribution
{

    class BaseNewsDistributor
    {
        private const int PollInterval = 100;
        private const int Timeout = 1000;
        private Thread pollThread;
        private volatile bool isStopping;
        public TcpDistributor distributor;

        static BaseNewsDistributor instance;
        public static BaseNewsDistributor Instance
        {
            get { return instance ?? (instance = new BaseNewsDistributor()); }
        }

        /// <summary>
        /// устанавливается в Insert
        /// входит в GetStatusString
        /// </summary>
        private DateTime lastQuoteTimeUnsafe;

        private DateTime LastQuoteTime
        {
            get
            {
                lock (lastQuoteTimeLocker)
                {
                    return lastQuoteTimeUnsafe;
                }
            }
            set
            {
                lock (lastQuoteTimeLocker)
                {
                    lastQuoteTimeUnsafe = value;
                }
            }
        }

        private readonly object lastQuoteTimeLocker = new object();

        private readonly ThreadSafeQueue<IBaseNews> newsQueue = new ThreadSafeQueue<IBaseNews>();

        private BaseNewsDistributor() {}
        private void PollRoutine()
        {
            while (!isStopping)
            {
                Thread.Sleep(PollInterval);
                // запрашиваем новости
                bool timeoutFlag;
                var news = newsQueue.ExtractAll(Timeout, out timeoutFlag);
                if (timeoutFlag) 
                    Logger.DebugFormat("Таймаут {0} при попытке получить новости на раздачу", Timeout);
                if (news == null || timeoutFlag) continue;
                if (news.Count > 0)
                {
                    //Logger.InfoFormat("Доставка online {0} котировок", news.Count);
                    distributor.DistributeStringData(BaseNewsParser.ToString(news));
                }
            }
        }

        public void Insert(List<IBaseNews> news)
        {
            if (news.Count > 0)            
                LastQuoteTime = DateTime.Now;
            newsQueue.InQueue(news, Timeout);
        }

        /// <summary>
        /// часть данных о состоянии всего сервиса
        /// </summary>
        /// <returns></returns>
        public string GetStatusString()
        {
            // последняя котировка
            var sb = new StringBuilder();
            sb.Append("Последняя котировка: ");
            var lastQuoteTime = LastQuoteTime;
            if (lastQuoteTime == new DateTime()) sb.Append("-");
            else
            {
                var deltaTime = DateTime.Now - lastQuoteTime;
                sb.AppendFormat("{0} м. {1} с.", (int)deltaTime.TotalMinutes, deltaTime.Seconds);
            }
            // подключенные клиенты
            //sb.AppendLine();
            //sb.AppendLine("Подключенные клиенты:");
            //sb.Append(distributor.GetClientsInfo());
            return sb.ToString();
        }

        public void StartPolling()
        {
            isStopping = false;
            // раздача котировок
            distributor = new TcpDistributor(AppConfig.GetIntParam("TCP.DistributorPort", 2771));
            distributor.Start();
            pollThread = new Thread(PollRoutine);
            pollThread.Start();
        }

        public void StopPolling()
        {
            if (pollThread == null) 
                return;
            isStopping = true;
            pollThread.Join();
            distributor.Stop();
        }
    }
}

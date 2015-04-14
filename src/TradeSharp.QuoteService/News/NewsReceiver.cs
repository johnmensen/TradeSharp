using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using TradeSharp.Contract.Contract;
using TradeSharp.Util;

namespace TradeSharp.QuoteService.News
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class NewsReceiver : INewsReceiver
    {
        private static NewsReceiver instance;
        public static NewsReceiver Instance
        {
            get
            {
                if (instance == null) instance = new NewsReceiver();
                return instance;
            }
        }

        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["QuoteBase"].ConnectionString;        

        private readonly ThreadSafeQueue<Contract.Entity.News> newsQueue = 
            new ThreadSafeQueue<Contract.Entity.News>();
        private const int QueueTimeout = 200;

        private readonly FloodSafeLogger loggerNoFlood = new FloodSafeLogger(1000);
        private const int LogMagicTimeoutPut = 1;
        private const int LogMagicTimeoutExtract = 2;

        private volatile bool isStopping;
        private Thread threadCheckQueue;
        private const int ThreadSleepInterval = 200;
        private const int ThreadIterationsPerStore = 5;

        private NewsReceiver()
        {
        }

        public void Start()
        {
            isStopping = false;
            threadCheckQueue = new Thread(StoreRoutine);
            threadCheckQueue.Start();
        }

        public void Stop()
        {
            isStopping = true;
            threadCheckQueue.Join();
        }
        
        public void PutNews(Contract.Entity.News[] news)
        {
            if (!newsQueue.InQueue(news.ToList(), QueueTimeout))
                loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Debug, LogMagicTimeoutPut,
                                                         1000*60*15, "NewsReceiver: таймаут при выполнении PutNews");
        }

        private void StoreRoutine()
        {
            int curInterval = ThreadIterationsPerStore;
            while (!isStopping)
            {
                curInterval--;
                if (curInterval == 0)
                {
                    curInterval = ThreadIterationsPerStore;
                    StoreNews();
                }
                Thread.Sleep(ThreadSleepInterval);
            }
        }

        private void StoreNews()
        {
            bool timeout;
            var news = newsQueue.ExtractAll(QueueTimeout, out timeout);            
            if (timeout)
            {
                loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Debug, LogMagicTimeoutExtract,
                                                         1000*60*15, "NewsReceiver: таймаут при выполнении StoreNews");
                return;
            }
            if (news.Count == 0) return;
            try
            {
                using (var cn = new SqlConnection(connectionString))
                {
                    cn.Open();
                    InsertNewsIntoDb(news, cn);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("DbNewsStorage.PutNews: возникла ошибка ", ex);
            }
        }

        private static void InsertNewsIntoDb(List<Contract.Entity.News> news, SqlConnection cn)
        {
            var cmdBuilder = new StringBuilder();
            var cmdInQueue = 0;
            const int maxCmdInQueue = 50;

            foreach(var n in news)
            {
                cmdBuilder.AppendLine(
                    string.Format(
                        "INSERT INTO NEWS(Channel, DateNews, Title, Body, Minutes) VALUES ({0}, '{1:yyyyMMdd}', '{2}', '{3}', {4})",
                        n.ChannelId, 
                        n.Time.Date, 
                        n.Title, 
                        n.Body.Replace("\'", "\''"),
                        (int)(n.Time - n.Time.Date).TotalMinutes));
                cmdInQueue++;
                if (cmdInQueue == maxCmdInQueue)
                {
                    cmdInQueue = 0;
                    ExecuteSql(cmdBuilder, cn);
                }
            }
            if (cmdInQueue > 0)
                ExecuteSql(cmdBuilder, cn);
        }

        private static void ExecuteSql(StringBuilder cmdBuilder, SqlConnection cn)
        {
            try
            {
                var cmd = cn.CreateCommand();
                cmd.CommandText = cmdBuilder.ToString();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в NewsReceiver: ExecuteSql", ex);
            }            
        }
    }
}

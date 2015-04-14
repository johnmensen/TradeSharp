using System;
using System.Collections.Generic;
using System.Messaging;
using System.Threading;
using TradeSharp.Contract.Entity;
using TradeSharp.DealerInterface;
using TradeSharp.ProviderProxyContract.Entity;
using TradeSharp.Server.Contract;
using TradeSharp.Util;

namespace TradeSharp.Server.BL
{
    class ProviderQueueReader
    {
        private static readonly FloodSafeLogger loggerNoFlood = new FloodSafeLogger(1000 * 60 * 30);
        
        private readonly int messagePollInterval;

        private const int MessageQueueError = 1;
        
        private static ProviderQueueReader instance;

        public IOrderManager orderManager;

        private volatile bool isStopping;

        private Thread threadLoop;

        public delegate void ExecutionReportReceivedDel(Dictionary<BrokerResponse, BrokerOrder> reports);

        private ExecutionReportReceivedDel onExecutionReportReceived;

        public event ExecutionReportReceivedDel OnExecutionReportReceived
        {
            add { onExecutionReportReceived += value; }
            remove { onExecutionReportReceived -= value; }
        }
        
        public static ProviderQueueReader Instance
        {
            get { return instance ?? (instance = new ProviderQueueReader()); }
        }

        private ProviderQueueReader()
        {
            messagePollInterval = AppConfig.GetIntParam("MQ.PollInterval", 200);
        }

        private void LoopRoutine()
        {
            // очередь сообщений от провайдера
            MessageQueue mq;
            try
            {
                var queueName = MessageQueues.FormatQueueName(HostName.MessageQueueHostName, MessageQueues.QueueFromProvider);
                mq = new MessageQueue(queueName)
                         {Formatter = new BinaryMessageFormatter()};
                Logger.InfoFormat("ProviderQueueReader: слушает очередь {0}", queueName);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка при обращении к очереди [{0}]: {1}",
                    MessageQueues.QueueFromProvider, ex);
                throw;
            }

            const int messageRecvTimeout = 50;
            var timeout = new TimeSpan(0, 0, 0, 0, messageRecvTimeout);
            using (mq)
            while (!isStopping)
            {
                Thread.Sleep(messagePollInterval);
                Message msg;
                try
                {
                    msg = mq.Receive(timeout);
                }
                catch (MessageQueueException ex)
                {
                    loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                        1, "Ошибка получения сообщения из очереди (MQE) [{0}]: {1}", MessageQueues.QueueFromProvider, ex.Message);
                    continue;
                }
                catch (Exception ex)
                {
                    loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                        2, "Ошибка получения сообщения из очереди [{0}]: {1}", MessageQueues.QueueFromProvider, ex.Message);
                    continue;
                }
                if (msg == null) continue;
                // состояние сервиса
                ModuleStatusController.Instance.lastProviderMessageTime.Value = DateTime.Now;

                if (msg.Body is Nullable)
                {
                    Logger.Error("В очереди сообщений от провайдера пустое сообщение");
                    continue;
                }
                if (msg.Body is ExecutionReport == false)
                {
                    Logger.ErrorFormat("В очереди сообщений от провайдера сообщение типа {0}",
                        msg.Body.GetType());
                    continue;
                }
                var execReport = (ExecutionReport) msg.Body;
                if (execReport.brokerResponse == null)
                {
                    Logger.ErrorFormat("ProviderQueueReader: тело ExecutionReport пустое");
                    continue;
                }
                // раздать сообщения получателям (FIX-дилерам)                
                ProcessExecutionReports(new List<BrokerResponse>{execReport.brokerResponse});
            }
        }

        public void Start()
        {
            threadLoop = new Thread(LoopRoutine);
            threadLoop.Start();
        }

        public void Stop()
        {
            isStopping = true;
            threadLoop.Join();
        }

        /// <summary>
        /// для каждого отчета брокера найти запрос (MarketOrder),
        /// которому тот соответствует
        /// 
        /// передать отчет в обработку соотв. дилеру
        /// </summary>        
        private void ProcessExecutionReports(List<BrokerResponse> reports)
        {
            var responseDic = orderManager.FindRequestsForExecutionReports(reports);
            onExecutionReportReceived(responseDic);
        }
    }    
}

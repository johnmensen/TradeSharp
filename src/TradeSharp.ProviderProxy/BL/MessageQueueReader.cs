using System;
using System.Messaging;
using System.Threading;
using TradeSharp.ProviderProxyContract.Entity;
using TradeSharp.Util;

namespace TradeSharp.ProviderProxy.BL
{
    class MessageQueueReader
    {
        private readonly int messagePollInterval;
        private readonly int messageRecvTimeout = 50;
        private readonly string queueName;
        private static readonly FloodSafeLogger LoggerNoFlood = new FloodSafeLogger(1000*60*30);

        private const int LogMsgReadQueueError = 1;

        private const int LogMsgReadQueueBodyError = 2;

        public MessageQueueReader(string queueName)
        {
            messagePollInterval = AppConfig.GetIntParam("MQ.PollInterval", 200);
            messageRecvTimeout = AppConfig.GetIntParam("MQ.RecvTimeout", 200);
            this.queueName = queueName;
        }

        private volatile bool isStopping;

        private Thread threadLoop;

        public delegate void MarketOrderReceivedDel(string queueName, MarketOrder[] orders);

        private MarketOrderReceivedDel onMarketOrderReceived;

        public event MarketOrderReceivedDel OnMarketOrderReceived
        {
            add { onMarketOrderReceived += value; }
            remove { onMarketOrderReceived -= value; }
        }

        private void LoopRoutine()
        {
            // очередь сообщений от провайдера
            MessageQueue mq;
            try
            {
                mq = new MessageQueue(MessageQueues.FormatQueueName(
                    HostName.MessageQueueHostName, queueName)) {Formatter = new BinaryMessageFormatter()};
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка при обращении к очереди [{0}]: {1}",
                    MessageQueues.QueueFromProvider, ex);
                throw;
            }
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
                    catch (MessageQueueException)
                    {
                        //LoggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                        //    LogMsgReadQueueError, "Ошибка получения сообщения из очереди (MQE) [{0}]: {1}", queueName, ex);                        
                        continue;
                    }
                    catch (Exception ex)
                    {
                        LoggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                            LogMsgReadQueueError, "Ошибка получения сообщения из очереди [{0}]: {1}", queueName, ex.GetType().Name);
                        continue;
                    }
                    if (msg == null) continue;                    
                    // раздать сообщения получателям (FIX-дилерам)
                    object body;
                    try
                    {
                        body = msg.Body;
                    }
                    catch (Exception ex)
                    {
                        LoggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                            LogMsgReadQueueBodyError, "Ошибка получения чтения тела сообщения в очереди [{0}]: {1}", queueName, ex);
                        continue;
                    }
                    if (body == null)
                    {
                        Logger.ErrorFormat("В очереди сообщений от дилера [{0}] пустое сообщение", 
                            queueName);
                        continue;
                    }
                    if (body is MarketOrder == false)
                    {
                        Logger.ErrorFormat("В очереди сообщений от дилера [{0}] сообщение типа [{1}]",
                            queueName, body.GetType());
                        continue;
                    }
                    var order = (MarketOrder) body;
                    // проверка на устаревание
                    var timeElapsed = DateTime.Now - order.TimeSent;
                    if (timeElapsed.Milliseconds > MarketOrder.MillisecondsToStale)
                    {
                        Logger.InfoFormat("Ордер [{1} {2} {3}] устарел (прошло {0} сек.)",
                            timeElapsed.TotalSeconds,
                            order.brokerOrder.Side > 0 ? "BUY" : "SELL", order.brokerOrder.Volume, order.brokerOrder.Ticker);
                        continue;
                    }
                    onMarketOrderReceived(queueName, new [] { order });
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
    }    
}

using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.ProviderProxyContract.Entity;
using TradeSharp.Util;
using QuickFix;

namespace TradeSharp.ProviderProxy.BL
{
    class MessageQueueReaderPool
    {
        private static MessageQueueReaderPool instance;
        public static MessageQueueReaderPool Instance
        {
            get { return instance ?? (instance = new MessageQueueReaderPool()); }
        }

        /// <summary>
        /// имя очереди - очередь
        /// </summary>
        private readonly Dictionary<string, MessageQueueReader> queues = new Dictionary<string, MessageQueueReader>();

        private MessageQueueReaderPool()
        {
            // прочитать настройки: имя очереди-сессия
            foreach (var sessionInfo in SessionInfo.Instance.dicQueueSession)
            {
                var queue = new MessageQueueReader(sessionInfo.MessageQueue);
                Logger.InfoFormat("Слушается очередь {0}", sessionInfo.MessageQueue);
                queue.OnMarketOrderReceived += ProcessMarketOrder;
                queues.Add(sessionInfo.MessageQueue, queue);
            }
        }

        private static void ProcessMarketOrder(string queueName, MarketOrder[] orders)
        {
            var sessionInfo = SessionInfo.Instance.dicQueueSession.First(s => s.MessageQueue == queueName);
            Logger.InfoFormat("Получено {0} ордеров из очереди {1}", orders.Length, queueName);
            foreach (var order in orders)
            {
                Message msg;
                try
                {
                    msg = FixMessageMaker.Instance.MakeMessage(order, sessionInfo);
                }
                catch (Exception ex)
                {
                    Logger.Error("ProcessMarketOrder: ошибка форматирования сообщения", ex);
                    continue;
                }
                if (msg == null) continue;
                try
                {
                    FixApplication.SendMessage(msg);
                }
                catch (Exception ex)
                {
                    Logger.Error("ProcessMarketOrder: ошибка отправки сообщения", ex);
                }
            }            
        }

        public void Start()
        {
            foreach (var queue in queues.Values)
                queue.Start();
        }

        public void Stop()
        {
            foreach (var queue in queues.Values)
                queue.Stop();
        }
    }
}

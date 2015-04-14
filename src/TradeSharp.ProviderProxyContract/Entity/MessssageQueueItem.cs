using System;
using System.Linq;
using System.Messaging;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.ProviderProxyContract.Entity
{
    [Serializable]
    public abstract class MessssageQueueItem
    {
        public abstract bool SendToQueue(bool isError);        

        public abstract string Title { get; protected set; }

        public string ErrorString { get; set; }

        public int SentCount { get; protected set; }
        
        private const MessagePriority QueueMessagePriority = MessagePriority.Normal;

        /// <summary>
        /// время отправки сообщения - проверяется, не устарело ли в процессе обработки
        /// </summary>
        public DateTime TimeSent { get; set; }

        public void SendToQueue(string queueName, bool sendToError)
        {
            Logger.InfoFormat("Отправка в [{0}]: [{1}]", queueName, ToString());
            if (string.IsNullOrEmpty(Title))
            {
                var stack = new System.Diagnostics.StackTrace();
                Logger.ErrorFormat("Для сообщения не указан обязательный параметр Title: {0}", stack);
                throw new ArgumentException("Для сообщения не указан обязательный параметр Title");
            }
            if (!sendToError) SentCount++;

            var fullQueueName = MessageQueues.FormatQueueName(HostName.MessageQueueHostName, queueName);
            MessageQueue queue;
            try
            {
                queue = new MessageQueue(fullQueueName);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка создания экземпляра очереди [{0}]: {1}",
                    fullQueueName, ex);
                throw;
            }
            using (queue)
            {
                var msgProperties = new DefaultPropertiesToSend
                               {
                                   Priority = QueueMessagePriority,
                                   Label = Title,
                                   Recoverable = true
                               };
                queue.Formatter = new BinaryMessageFormatter();
                queue.DefaultPropertiesToSend = msgProperties;
                TimeSent = DateTime.Now;
                try
                {                    
                    queue.Send(this);
                    Logger.Info("Сообщение отправлено в очередь");
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Ошибка доставки сообщения [{0}] в [{1}]: {2}", Title, fullQueueName, ex);
                    throw;
                }                
                queue.Close();
            }
        }

        protected static string GetQueueNameByGroup(string groupCode, bool isError)
        {
            if (DalAccountGroup.Instance.Groups == null)
            {
                Logger.ErrorFormat("MessssageQueueItem: доставка сообщения группе {0} невозможна - не заполнен словарь",
                    groupCode);
                return string.Empty;
            }
            var groupInfo = DalAccountGroup.Instance.Groups.FirstOrDefault(gr => gr.Code == groupCode);
            if (groupInfo == null)
            {
                Logger.ErrorFormat("MessssageQueueItem: доставка сообщения группе {0} невозможна - нет записи в словаре (всего записей {1})",
                    groupCode, DalAccountGroup.Instance.Groups.Length);
                return string.Empty;
            }
            return !isError ? groupInfo.MessageQueue : string.Format("{0}.Error", groupInfo.MessageQueue);
        }
    }
}

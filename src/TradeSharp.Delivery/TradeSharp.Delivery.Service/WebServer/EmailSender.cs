using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.ServiceModel;
using System.Threading;
using TradeSharp.Contract.Entity;
using TradeSharp.Delivery.Contract;
using TradeSharp.Util;

namespace TradeSharp.Delivery.Service.WebServer
{
    /// <summary>
    /// Класс-диспетчер, реализующий WCF-службу, управляющий потоками отправки электронных сообщений
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class EmailSender : ITradeSharpDelivery
    {
        private const string ErrorCodeBlackListTooLarge = "BLTL";

        private static EmailSender instance;

        public static EmailSender Instance
        {
            get { return instance ?? (instance = new EmailSender()); }
        }

        private EmailSender () {}

        /// <summary>
        /// Остановка текущего цикла отправки
        /// </summary>
        public volatile bool breakCurrentDelivered;

        /// <summary>
        /// Поле для отображения текущего адреса отправки 
        /// </summary>
        public volatile string currentReceiver = String.Empty;

        private bool sendToAddressInFailList;
        private UrgencyFlag minUrgency = UrgencyFlag.Low;
        private volatile bool isStopping;
        private Thread threadDistribution;
        private readonly ThreadSafeList<DeliveryServerError> errorList = new ThreadSafeList<DeliveryServerError>();
        private readonly ThreadSafeList<EmailMessage> messages = new ThreadSafeList<EmailMessage>();

        /// <summary>
        /// Адреса получателей / количество TimeOut-ов для соответствующего адреса
        /// </summary>
        private readonly Dictionary<string, int> timeOutConter = new Dictionary<string, int>();

        /// <summary>
        /// Номер адресата, которому в данный момент отправляется сообщение
        /// </summary>
        public volatile int currentNomberMessagesInCurrentMail;     

        /// <summary>
        /// Количество адресатов текущего сообщения
        /// </summary>
        public volatile int countMessagesInCurrentMail;

        /// <summary>
        /// Количество адресатов во всём массиве 'EmailMessage[]' текущей отправки
        /// </summary>
        public volatile int countMessagesInAllMail;

        private readonly int failTimeOut = AppConfig.GetIntParam("WebServer.FailTimeOut", 1000);
        private readonly int failsToBlack = AppConfig.GetIntParam("WebServer.FailsToBlack", 1);

        /// <summary>
        /// Общее число отправленных сообщений
        /// </summary>
        public long totalDelivered;

        /// <summary>
        /// Адреса пользователей, которым не удалось доставить сообщения (собственно сам чёрный список)
        /// </summary>
        private readonly ThreadSafeList<string> failReceiversList = new ThreadSafeList<string>();

        /// <summary>
        /// для настройки минимального уровня важности отправляемых сообщений
        /// </summary>
        public UrgencyFlag MinUrgency
        {
            get { return minUrgency; }
            set { minUrgency = value; }
        }

        /// <summary>
        /// Отправлять ли сообщения на адреса из списка заблокированных
        /// </summary>
        public bool SendToAddressInFailList
        {
            get { return sendToAddressInFailList; }
            set { sendToAddressInFailList = value; }
        }

        /// <summary>
        /// Общее число отправленных сообщений
        /// </summary>
        public long TotalDelivered
        {
            get { return totalDelivered; }
        }

        /// <summary>
        /// Количество адресов в чёрном списке
        /// </summary>
        public int FailReceiversCount
        {
            get
            {
                return failReceiversList.Count ?? 0;
            }
        }

        public void Start()
        {
            if (threadDistribution != null && threadDistribution.IsAlive)
                return;
            isStopping = false;
            threadDistribution = new Thread(DistributionLoop);
            threadDistribution.Start();
        }

        public void Stop()
        {
            isStopping = true;
            if (threadDistribution != null) threadDistribution.Join();
        }

        /// <summary>
        /// Ставим новое сообщение в очередь на отправку
        /// </summary>
        /// <param name="msg">Сообщение</param>
        public void DeliverEmail(EmailMessage msg)
        {
            if (messages.Add(msg, 5000)) return;

            ModuleStatusController.Instance.Status.AddError(ServiceProcessState.HasCriticalErrors,
                                                            String.Format("Таймаут постановки в очередь на отправку сообщения {0}", msg.Title),
                                                            DateTime.Now);

            Logger.Error(String.Format("не удалось поставить в очередь на отправку сообщение {0}", msg.Title));
            
            var addError = errorList.Add(new DeliveryServerError{
                ReasonMessage = String.Format("не удалось поставить в очередь на отправку сообщение {0}", msg.Title),
                DateException = DateTime.Now,
                Urgency = msg.Urgency,
                TimeSpan = 5000,
                ExceptionLink = new Exception(String.Format("не удалось поставить в очередь на отправку сообщение {0}", msg.Title)),
                Receiver = String.Empty
            }, 1000);

            if (!addError)
                ModuleStatusController.Instance.Status.AddError(ServiceProcessState.HasCriticalErrors, "Таймаут добавления ошибки в список ошибок", DateTime.Now);
        }

        /// <summary>
        /// Ставим новое сообщение в очередь на отправку
        /// </summary>
        /// <param name="addresses">Массив адресов получателей сообщения</param>
        /// <param name="body">Основной текст сообщения</param>
        /// <param name="title">Заголовок сообщения</param>
        /// <param name="htmlFormat">Флаг формата</param>
        public void DeliverEmail(string[] addresses, string body, string title, bool htmlFormat)
        {
            var newMessage = new EmailMessage
                {
                    Receivers = addresses,
                    Body = body,
                    Title = title,
                    HTMLFormat = htmlFormat,
                    Urgency = UrgencyFlag.Normal
                };
            DeliverEmail(newMessage);
        }

        public bool ClearErrorList()
        {
            if (errorList.TryClear(5000)) return true;
            Logger.Error("не удалось очистить список ошибок");
            return false;
        }

        public bool ClearFailReceiversList()
        {
            if (failReceiversList.TryClear(5000)) return true;
            Logger.Error("не удалось очистить список заблокированных адресов");
            return false;
        }

        public List<DeliveryServerError> GetErrorList()
        {
            bool timeOutFlag;
            var result = errorList.GetAll(5000, out timeOutFlag);
            if (timeOutFlag)
            {
                Logger.Error("GetErrorList() : Не удалось получить список ошибок");
                ModuleStatusController.Instance.Status.AddError(ServiceProcessState.HasErrors, "Таймаут получения списока ошибок", DateTime.Now);
                return null;
            }
            return result;
        }

        /// <summary>
        /// метод проверяет очередь сообщений на наличие в ней элементов. 
        /// </summary>
        private void DistributionLoop()
        {
            while (!isStopping)
            {
                Thread.Sleep(100);
                bool timeoutFlag;
                var messagesToDeliver = messages.ExtractAll(1000, out timeoutFlag).OrderBy(a => (int)a.Urgency).ToArray();

                // Сообщаем в Watch службу о возникновении ошибки
                if (timeoutFlag)
                    ModuleStatusController.Instance.Status.AddError(ServiceProcessState.HasCriticalErrors, "Таймаут чтения очереди отправки", DateTime.Now);
                if (messagesToDeliver.Length > 0)
                    SendEmails(messagesToDeliver);                
            }

            // Если служба не была остановлена, выход из цикла произошел, то вероятно где то произошла ошибка
            if (!isStopping)
                ModuleStatusController.Instance.Status.AddError(ServiceProcessState.HasCriticalErrors, 
                    "Выход из цикла рассылки произошёл НЕ в ручном режиме. Метод DistributionLoop() завершился и вернул управление.", DateTime.Now);
        }

        /// <summary>
        /// 
        /// </summary>
        private void SendEmails(EmailMessage[] messagesArray)
        {
            countMessagesInAllMail = messagesArray.Sum(ma => ma.Receivers.Length);           

            Logger.InfoFormat("начинаем отправку {0} сообщений", messagesArray.Length);
            using (var client = new SmtpClient())
            {
                foreach (var message in messagesArray)
                {
                    countMessagesInCurrentMail = message.Receivers.Length;
                    currentNomberMessagesInCurrentMail = 0;
                    
                    if (isStopping) break;
                    if (breakCurrentDelivered) break;

                    Logger.InfoFormat("Начинаем рассылку сообщения '{0}' {1} получателям", message.Title, message.Receivers.Length);
                    if ((int)MinUrgency > (int)message.Urgency) continue; // Если уровень важности сообщения менее важный чем указанный минимальный, то не отсылаем это сообщение

                    foreach (var recv in message.Receivers)
                    {
                        using (var msg = new MailMessage {IsBodyHtml = message.HTMLFormat})
                        {
                            if (isStopping) break;
                            currentNomberMessagesInCurrentMail++;

                            bool timeOutFlaf;
                            // Если этот адрес в fail-списке, то не отправляем по нему сообщение
                            if (failReceiversList.Contains(recv, 1000, out timeOutFlaf) && !sendToAddressInFailList)
                                continue;

                            currentReceiver = recv;
                            msg.Subject = message.Title;
                            msg.Body = message.Body;
                            msg.To.Clear();

                            MailAddress mailAddress;
                            try
                            {
                                mailAddress = new MailAddress(recv);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error("new MailAddress(recv)", ex);
                                continue;
                            }
                            

                            msg.To.Add(mailAddress);

                            var timeStartSend = DateTime.Now;
                            try
                            {
                                client.Send(msg); // Засекаем время и пытаемся отправить сообщение
                                var milsSpared = (DateTime.Now - timeStartSend).TotalMilliseconds;
                                Logger.InfoFormat("сообщения '{0}' отправлено получателю {1}. Время отправки {2}",
                                                  message.Title, recv, milsSpared);
                                CheckTimeOut(milsSpared, recv, message);
                                Interlocked.Increment(ref totalDelivered);

                            }
                                #region catch

                            catch (Exception ex)
                            {
                                var milsSpared = (DateTime.Now - timeStartSend).TotalMilliseconds;
                                Logger.ErrorFormat(
                                    "Сбой доставки сообщения '{0}' получателю {1}. Время отправки составило {2}. " +
                                    "Этот адрес будет занесён в список заблокированных адресов. Ошибка: {3}",
                                    message.Title, recv, milsSpared, ex);

                                errorList.Add(new DeliveryServerError
                                    {
                                        ReasonMessage =
                                            String.Format("Сбой доставки сообщения '{0}' по причине {1}", message.Title,
                                                          ex.Message),
                                        Urgency = message.Urgency,
                                        DateException = DateTime.Now,
                                        ExceptionLink = ex,
                                        TimeSpan = milsSpared,
                                        Receiver = recv
                                    }, 1000);

                                if (!failReceiversList.AddIfNotContains(recv, 1000))
                                    Logger.ErrorFormat(
                                        String.Format("Не удалось добавить адрес {0} в список заблокированных адресов",
                                                      recv));
                                else
                                    Logger.Info(String.Format("Адрес {0} добавлен в список заблокированных адресов",
                                                              recv));
                            }

                            #endregion
                        }
                    }

                    if (failReceiversList.Count >= message.Receivers.Length)
                        ModuleStatusController.Instance.Status.AddError(ServiceProcessState.HasWarnings,
                            "Чёрный список больше, чем количество получателей сообщения.", DateTime.Now, ErrorCodeBlackListTooLarge);
                    //else
                    //    ModuleStatusController.Instance.Status.RemoveError(ServiceProcessState.HasWarnings, ErrorCodeBlackListTooLarge);
                }

                countMessagesInAllMail = 0;
            }
        }

        /// <summary>
        /// Проверка времени отправки сообщения. Если время слишком велико, тогда добавляем адрес получателя в чёрный писок
        /// </summary>
        /// <param name="milsSpared">Время отправки сообщения в миллисекундах</param>
        /// <param name="recv">Адрес получателя</param>
        /// <param name="message">ссылка на объект сообщения</param>
        private void CheckTimeOut(double milsSpared, string recv, EmailMessage message)
        {
            if (milsSpared <= failTimeOut) return; // Если время отправки меньше установленного TimeOut-а, тогда ошибки нет - уходим из этой процедуры

            int failCount;
            if (!timeOutConter.TryGetValue(recv, out failCount))
                timeOutConter.Add(recv, ++failCount);
            else
                timeOutConter[recv] = ++failCount;

            if (failCount <= failsToBlack) return; // Если количество TimeOut-ов не достигла максимального, тогда уходим из процедуры

            // Выводим ощибку в GUI сервера
            errorList.Add(new DeliveryServerError
                {
                    ReasonMessage = String.Format("Превышен timeout доставки сообщения '{0}'. Этот адрес будет занесён в список заблокированных адресов", message.Title),
                    Urgency = message.Urgency,
                    DateException = DateTime.Now,
                    ExceptionLink = new TimeoutException(String.Format("Превышен timeout доставки сообщения '{0}' получателю {1}.", message.Title, recv)),
                    TimeSpan = milsSpared,
                    Receiver = recv
                }, 1000);

            // Выводим ошибку в лог
            Logger.Error(String.Format("Превышен timeout доставки сообщения '{0}' получателю {1}. Время отправки составило {2}. Этот адрес будет занесён в список заблокированных адресов",
                              message.Title, recv, milsSpared));

            if (!failReceiversList.AddIfNotContains(recv, 1000))
                Logger.Error(String.Format("Не удалось добавить адрес {0} в список заблокированных адресов", recv));
            else Logger.Info(String.Format("Адрес {0} добавлен в список заблокированных адресов", recv));
        }
    }
}
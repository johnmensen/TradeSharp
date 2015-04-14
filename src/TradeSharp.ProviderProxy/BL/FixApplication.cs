using System;
using System.Linq;
using System.Threading;
using Entity;
using TradeSharp.Util;
using QuickFix;

namespace TradeSharp.ProviderProxy.BL
{
    class FixApplication : MessageCracker, Application
    {
        public delegate void OnLogonDel(SessionID sessionID);

        private OnLogonDel onSessionLogon;
        public event OnLogonDel OnSessionLogon
        {
            add { onSessionLogon += value; }
            remove { onSessionLogon -= value; }
        }

        private static string senderSubID;
        private static string SenderSubID
        {
            get
            {
                if (!string.IsNullOrEmpty(senderSubID)) return senderSubID;                
                return senderSubID = AppConfig.GetStringParam("FIX.SenderSubID", "");
            }
        }

        private static readonly bool needUserRequestMessage = AppConfig.GetBooleanParam("FIX.UserRequest", true);
        private static readonly bool needCredsInLogon = AppConfig.GetBooleanParam("FIX.NeedCredsInLogon", true);
        private static readonly bool needTargetSubId = AppConfig.GetBooleanParam("FIX.NeedTargetSubId", true);
        private static readonly bool needSenderSubId = AppConfig.GetBooleanParam("FIX.NeedSenderSubId", false);

        // типы сообщений (фильтруемых) - "W;S;0"
        private string[] messageLogFilterTypes;

        protected string[] MessageLogFilterTypes
        {
            get
            {
                if (messageLogFilterTypes == null)
                {
                    var filtered = AppConfig.GetStringParam("Log.FilteredMessages", "W;S;0");
                    messageLogFilterTypes = filtered.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                }
                return messageLogFilterTypes;
            }
        }

        private static TickerNamingStyle? providerNaming;
        // ReSharper disable PossibleInvalidOperationException
        public static TickerNamingStyle ProviderNaming
        {
            get
            {
                if (providerNaming.HasValue) return providerNaming.Value;
                var providerNamingStr = AppConfig.GetStringParam(
                    "Provider.TickerNaming", TickerNamingStyle.Системный.ToString());
                providerNaming = (TickerNamingStyle)Enum.Parse(
                    typeof(TickerNamingStyle), providerNamingStr);
                return providerNaming.Value;
            }
        }
        // ReSharper restore PossibleInvalidOperationException

        private bool ShouldFilterMessage(Message message)
        {
            try
            {
                var field = message.getHeader().getField(new MsgType());
                return MessageLogFilterTypes.Contains(field.getValue());
            }
            catch (Exception ex)
            {
                Logger.Error("Сообщение не содержит поля типа", ex);
                return false;
            }
        }

        public void onCreate(SessionID sessionID)
        {
            Logger.Info("onCreate: " + sessionID);
        }

        public void onLogon(SessionID sessionID)
        {
            if (onSessionLogon != null) onSessionLogon(sessionID);

            if (FixMessage.FixVersion == FixVersion.Fix44 && needUserRequestMessage)
            {
                // послать UserRequest
                var userName = SessionSettingsParser.Instance.GetSessionParam(
                    sessionID.getTargetCompID(), sessionID.getSenderCompID(), "Username", "");
                var password = SessionSettingsParser.Instance.GetSessionParam(
                    sessionID.getTargetCompID(), sessionID.getSenderCompID(), "Password", "");
                QuickFix44.UserRequest msg;
                try
                {
                    msg = new QuickFix44.UserRequest(new UserRequestID("1"), new UserRequestType(1),
                                                         new Username(userName));
                    msg.setField(new SendingTime(DateTime.Now.ToUniversalTime(), FixMessageMaker.Instance.ShowMillisecond));  // !! появляется дважды
                    msg.setField(new Password(password));
                    msg.getHeader().setField(new SenderCompID(sessionID.getSenderCompID()));
                    msg.getHeader().setField(new TargetCompID(sessionID.getTargetCompID()));
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка сборки сообщения UserRequest", ex);
                    throw;
                }
                SendMessage(msg);
            }
        }

        public void onLogout(SessionID sessionID)
        {
            Logger.Info("onLogout: " + sessionID);
        }

        public void toAdmin(Message message, SessionID sessionID)
        {
            if (message is QuickFix44.Logon && needCredsInLogon)
            {
                var logonMsg = (QuickFix44.Logon) message;
                var userName = SessionSettingsParser.Instance.GetSessionParam(
                    sessionID.getTargetCompID(), sessionID.getSenderCompID(), "Username", "");
                var password = SessionSettingsParser.Instance.GetSessionParam(
                    sessionID.getTargetCompID(), sessionID.getSenderCompID(), "Password", "");
                logonMsg.setField(new Password(password));
                logonMsg.setField(new Username(userName));                
            }
            
            var subId = SessionSettingsParser.Instance.GetSessionParam(
                        sessionID.getTargetCompID(), sessionID.getSenderCompID(), "SessionQualifier", "FX");
            var targetSubID = SessionSettingsParser.Instance.GetSessionParam(
                        sessionID.getTargetCompID(), sessionID.getSenderCompID(), "TargetSubID", "");
            try
            {
                if (!string.IsNullOrEmpty(subId) && message is QuickFix43.Logon && needSenderSubId)
                    message.getHeader().setField(new SenderSubID(subId));
                message.getHeader().setField(new SendingTime(DateTime.Now.ToUniversalTime(), FixMessageMaker.Instance.ShowMillisecond));
                if (!string.IsNullOrEmpty(targetSubID) && needTargetSubId)
                    message.getHeader().setField(new TargetSubID(targetSubID));
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка установки данных заголовка админ. сообщения, сессия {0}", sessionID);
                Logger.Error("Исключение", ex);
                return;
            }
            #region Fix43
            if (message is QuickFix43.Logon)
            {
                try
                {
                    var logon = (QuickFix43.Logon)message;
                    logon.setField(new ResetSeqNumFlag(true));
                    var userName = SessionSettingsParser.Instance.GetSessionParam(
                        sessionID.getTargetCompID(), sessionID.getSenderCompID(), "Username", "");
                    var password = SessionSettingsParser.Instance.GetSessionParam(
                        sessionID.getTargetCompID(), sessionID.getSenderCompID(), "Password", "");
                    if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                        Logger.ErrorFormat("Не найдены настройки сессии {0}", sessionID);
                    else
                    {
                        logon.set(new Username(userName));
                        logon.set(new Password(password));
                    }
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Ошибка установки параметров авторизации, сессия {0}", sessionID);
                    Logger.Error("Исключение", ex);
                }
            }
            #endregion
            #region Fix44
            if (message is QuickFix44.Logon)
            {// ничего не делаю - пароль и логин указываются отдельно в последующем UserRequest (BE)
            }
            #endregion

            if (!ShouldFilterMessage(message))
                Logger.Info(string.Format("toAdmin({0}, {1})", message, sessionID));
        }

        public void toApp(Message message, SessionID sessionID)
        {
            var msgText = string.Format("toApp({0}, {1})", message, sessionID);
            try
            {
                message.getHeader().setField(new SendingTime(DateTime.Now.ToUniversalTime(), FixMessageMaker.Instance.ShowMillisecond));
                Logger.Info(msgText);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка установки данных заголовка сообщения, сессия {0}", sessionID);
                Logger.Error("Исключение", ex);
                return;
            }
            try
            {
                if (message is QuickFix41.NewOrderSingle || 
                    message is QuickFix42.NewOrderSingle ||
                    message is QuickFix43.NewOrderSingle ||
                    message is QuickFix44.NewOrderSingle)
                FileMessageLogQueue.Instance.LogMessage(msgText);
            }
            catch (Exception ex)
            {
                Logger.Error("Исключение при сохранении сообщения в файл", ex);
                return;
            }
        }

        public void fromAdmin(Message message, SessionID sessionID)
        {
            ThreadPool.QueueUserWorkItem(data =>
            {
                var msgAndSession = (Cortege2<Message, SessionID>) data;
                if (!ShouldFilterMessage(msgAndSession.a))
                    Logger.Info(string.Format("fromAdmin({0}, {1})", msgAndSession.a, msgAndSession.b));
            }, 
            new Cortege2<Message, SessionID>(message, sessionID)); 
        }

        public void fromApp(Message msg, SessionID sesId)
        {
            ThreadPool.QueueUserWorkItem(data =>
            {
                var msgAndSession = (Cortege2<Message, SessionID>)data;
                var message = msgAndSession.a;
                var sessionId = msgAndSession.b;

                var msgText = string.Format("fromApp({0}, {1})", message, sessionId);
                if (!ShouldFilterMessage(message)) Logger.Info(msgText);

                if (processMessageFromBroker != null)
                    processMessageFromBroker(message, new FixMessage(message.ToString(), sessionId.ToString()));

                try
                {
                    if (message is QuickFix41.ExecutionReport ||
                        message is QuickFix42.ExecutionReport ||
                        message is QuickFix43.ExecutionReport ||
                        message is QuickFix44.ExecutionReport)
                        FileMessageLogQueue.Instance.LogMessage(msgText);
                }
                catch (Exception ex)
                {
                    Logger.Error("Исключение при сохранении сообщения в файл", ex);
                    return;
                }
            },
            new Cortege2<Message, SessionID>(msg, sesId));      
        }

        public ProcessMessageFromBrokerDel processMessageFromBroker;

        public override void onMessage(QuickFix43.Logon message, SessionID sessionID)
        {
            Logger.InfoFormat("Logon command (Sender = {0}, Target = {1}, Encrypt = {2}," +
                              " Password = {3}, Heartbeat int = {4})",
                              sessionID.getSenderCompID(), sessionID.getTargetCompID(),
                              message.getEncryptMethod(), message.getPassword(), message.getHeartBtInt());
        }

        public override void onMessage(QuickFix44.Logon message, SessionID sessionID)
        {
            Logger.InfoFormat("Logon command (Sender = {0}, Target = {1}, Encrypt = {2}," +
                              " Password = {3}, Heartbeat int = {4})",
                              sessionID.getSenderCompID(), sessionID.getTargetCompID(),
                              message.getEncryptMethod(), message.getPassword(), message.getHeartBtInt());
        }

        public override void onMessage(QuickFix43.Logout message, SessionID sessionID)
        {
            Logger.InfoFormat("Logout command (Sender = {0}, Target = {1}, Text = {2}," +
                              " Encoded text = {3})",
                              sessionID.getSenderCompID(), sessionID.getTargetCompID(),
                              message.getText(), message.getEncodedText());
        }

        public override void onMessage(QuickFix44.Logout message, SessionID sessionID)
        {
            Logger.InfoFormat("Logout command (Sender = {0}, Target = {1}, Text = {2}," +
                              " Encoded text = {3})",
                              sessionID.getSenderCompID(), sessionID.getTargetCompID(),
                              message.getText(), message.getEncodedText());
        }

        public override void onMessage(QuickFix43.ExecutionReport message, SessionID sessionID)
        {
            Logger.InfoFormat("Execution report (Sender = {0}, Target = {1})",
                              sessionID.getSenderCompID(), sessionID.getTargetCompID());
        }

        public override void onMessage(QuickFix44.ExecutionReport message, SessionID sessionID)
        {
            Logger.InfoFormat("Execution report (Sender = {0}, Target = {1})",
                              sessionID.getSenderCompID(), sessionID.getTargetCompID());
        }

        public static void SendMessage(Message msg)
        {
            try
            {
                // получить код машины назначения, код машины отправления,
                // SessionQualifier (SenderSubID)...
                var target = msg.getHeader().getField(new TargetCompID()).getValue();
                var sender = msg.getHeader().getField(new SenderCompID()).getValue();
                var subId = SessionSettingsParser.Instance.GetSessionParam(
                    target, sender, "SessionQualifier", "FX");
                if (string.IsNullOrEmpty(subId)) subId = SenderSubID;
                if (!string.IsNullOrEmpty(subId) && needSenderSubId) 
                    msg.getHeader().setField(new SenderSubID(subId));
                
                // отправить сообщение ( будет обработано в ToApp() )
                Logger.DebugFormat("Отправка сообщения: target [{0}], sender [{1}], sender sub Id [{2}]",
                                   target, sender, subId);
                try
                {
                    if (!string.IsNullOrEmpty(subId))
                        Session.sendToTarget(msg, sender, target, subId);
                    else
                        Session.sendToTarget(msg, sender, target);
                }
                catch (SessionNotFound) /* Сессия не найдена */
                {
                    var sessionId = new SessionID(FixVersionTitle.GetVersionTitle(FixMessage.FixVersion),
                                                  sender, target);
                    var exists = Session.doesSessionExist(sessionId);
                    Logger.InfoFormat("SendMessage(target [{0}], sender [{1}], sender sub Id [{2}]) - сессия не найдена ({3})",
                        target, sender, subId, exists ? "но - существует" : "не существует");
                }
            }            
            catch (Exception ex)
            {
                Logger.InfoFormat("Ошибка отправки сообщения ({0}): {1} - {2}",
                    msg, ex.GetType(), ex.Message);
            }
        }
    }
    
    public delegate void ProcessMessageFromBrokerDel(Message msgOrig, FixMessage message);
}

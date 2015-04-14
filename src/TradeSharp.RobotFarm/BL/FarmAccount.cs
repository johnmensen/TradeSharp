using System;
using System.Collections.Generic;
using System.Xml;
using Entity;
using TradeSharp.Client.Util.Storage;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Localisation;
using TradeSharp.Robot.BacktestServerProxy;
using TradeSharp.Robot.Robot;
using TradeSharp.Util;

namespace TradeSharp.RobotFarm.BL
{
    /// <summary>
    /// "поле" в составе "фермерского хозяйства",
    /// на котором "пасутся" роботы
    /// </summary>
    class FarmAccount
    {
        [PropertyXMLTag("AccountId")]
        public int AccountId
        {
            get { return accountData.accountId; }
            set { accountData.accountId = value; }
        }

        [PropertyXMLTag("UserLogin")]
        public string UserLogin { get; set; }

        [PropertyXMLTag("UserPassword")]
        public string UserPassword { get; set; }

        private List<BaseRobot> robots = new List<BaseRobot>();
        public List<BaseRobot> Robots
        {
            get { return robots; }
            set { robots = value; }
        }

        [PropertyXMLTag("TradeEnabled")]
        public bool TradeEnabled { get; set; }

        private RobotContextLiveFarm context;

        public CurrentProtectedContext protectedContext;

        public TradeSharpServerTrade proxyTrade;

        private readonly TradeServerCallbackProcessor callbackProcessor;

        private const string TerminalVersionString = "robot_farm";

        private const int TestTerminalId = 100;

        #region LogNoFlood
        private readonly FloodSafeLogger logNoFlood = new FloodSafeLogger(1000 * 60);

        private const int LogMsgQuoteRecvError = 1;

        private const int LogMsgPingError = 2;
        #endregion

        private long termId;
        [PropertyXMLTag("TerminalId")]
        public long TerminalId
        {
            get
            {
                if (termId == 0)
                    return termId = LongRandom(0, long.MaxValue, new Random());
                return termId;
            }
            set { termId = value; }
        }

        private readonly ActualAccountData accountData;

        public FarmAccount()
        {
            callbackProcessor = new TradeServerCallbackProcessor();
            accountData = new ActualAccountData(0);
        }

        public RobotContextLiveFarm GetContext()
        {
            return context;
        }

        public int LoadRobots(XmlElement node)
        {
            robots.Clear();
            try
            {
                var nodes = node.SelectNodes("robot");
                // ReSharper disable PossibleNullReferenceException
                foreach (XmlElement item in nodes)
                {
                    var inodes = item.SelectNodes("Robot.TypeName");
                    var inode = (XmlElement)inodes[0];
                    // ReSharper restore PossibleNullReferenceException
                    var title = inode.Attributes["value"].Value;
                    var robot = RobotCollection.MakeRobot(title);
                    PropertyXMLTagAttribute.InitObjectProperties(robot, item, false);
                    robots.Add(robot);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка чтения файла настроек портфеля роботов: ", ex);
            }
            return robots.Count;
        }

        public void SaveRobots(XmlElement rootNode)
        {
            foreach (var robot in Robots)
            {
                // ReSharper disable PossibleNullReferenceException
                var robotNode = (XmlElement)rootNode.AppendChild(rootNode.OwnerDocument.CreateElement("robot"));
                // ReSharper restore PossibleNullReferenceException
                try
                {
                    PropertyXMLTagAttribute.SaveObjectProperties(robot, robotNode);
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка сохранения робота " + robot.GetUniqueName() + ": " + ex);
                }
            }
        }

        public static string CheckCredentials(FarmAccountArg e)
        {
            try
            {
                var proxyTrade = new TradeSharpServerTrade(new TradeServerCallbackProcessor());
                var localTime = DateTime.Now.Ticks;
                var hash = CredentialsHash.MakeCredentialsHash(e.Login, e.Password, localTime);
                int sessionTag;
                var response = proxyTrade.proxy.Authenticate(e.Login, hash, "robot_farm",
                    TestTerminalId, localTime, out sessionTag);
                if (response != AuthenticationResponse.OK)
                    return EnumFriendlyName<AuthenticationResponse>.GetString(response);
                Account account;
                var status = TradeSharpAccount.Instance.proxy.GetAccountInfo(e.AccountId, false, out account);
                // !! logout here
                return EnumFriendlyName<RequestStatus>.GetString(status);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public bool SetupLiveContext()
        {
            context = null;
            protectedContext = new CurrentProtectedContext();

            try
            {
                protectedContext.Initialize(TerminalId);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка иницииализации защищенного контекста", ex);
                throw;
            }
            
            // создать объект proxy
            try
            {
                if (proxyTrade == null)
                    proxyTrade = new TradeSharpServerTrade(callbackProcessor);
            }
            catch (Exception ex)
            {
                RobotFarm.Instance.AppendLogMessage("Ошибка создания TradeSharpServerTrade: " + ex);
                return false;
            }

            // залогиниться и
            // получить актуальную информацию по счету
            Account account;
            try
            {
                var localTime = DateTime.Now.Ticks;
                var hash = CredentialsHash.MakeCredentialsHash(UserLogin, UserPassword, localTime);
                int sessionTag;
                var response = proxyTrade.proxy.Authenticate(UserLogin, hash,
                    TerminalVersionString, TerminalId, localTime, out sessionTag);
                if (response != AuthenticationResponse.OK)
                {
                    protectedContext.OnAuthenticateFaulted();
                    RobotFarm.Instance.AppendLogMessage(string.Format("Ошибка аутентификации ({0}-{1}): {2}",
                        UserLogin, UserPassword, response));
                    return false;
                }                
                protectedContext.OnAuthenticated(sessionTag);

                var opRst = TradeSharpAccount.Instance.proxy.GetAccountInfo(AccountId, false, out account);
                if (account == null)
                {
                    RobotFarm.Instance.AppendLogMessage(string.Format("Невозможно получить информацию по счету {0}: {1}",
                        AccountId, opRst));
                    return false;
                }

                accountData.accountId = AccountId;
            }
            catch (Exception ex)
            {
                RobotFarm.Instance.AppendLogMessage(
                    string.Format("Ошибка аутентификации ({0}-{1}): {2}",
                        UserLogin, UserPassword, ex));
                return false;
            }
            
            // инициализировать киборгов
            var countSuccess = robots == null ? 0 : robots.Count;
            if (robots != null && robots.Count > 0)
            {
                // контекст для роботов
                context = new RobotContextLiveFarm(proxyTrade, account, () => UserLogin, accountData)
                {
                    robotContextMode = RobotContext.ContextMode.Realtime
                };
                context.OnRobotMessage += (robot, time, messages) =>
                {
                    foreach (var msg in messages)
                        RobotFarm.Instance.AppendLogMessage(
                            "#" + AccountId + ": [" + 
                            robot.GetUniqueName() + "] said \"" + msg + "\"");
                };

                // включить каждого робота в контекст
                foreach (var robot in robots)
                {
                    try
                    {
                        robot.Initialize(context, protectedContext);
                        context.SubscribeRobot(robot);
                    }
                    catch (Exception ex)
                    {
                        countSuccess--;
                        RobotFarm.Instance.AppendLogMessage("Ошибка инициализации робота " +
                                                            robot.GetUniqueName() + ": " + ex);
                    }
                }
            }

            // создать объект - обработчик торговых сигалов
            callbackProcessor.SignalProcessor = new TradeSignalProcessor(
                () => accountData.GetActualAccount(true),
                s => RobotFarm.Instance.AppendLogMessage(s),
                () => accountData.GetActualOrderList(),
                () => VolumeRoundType.Ближайшее,
                proxyTrade.proxy, protectedContext, this);

            return (robots == null || robots.Count <= 0) || countSuccess > 0;
        }

        public void StopProcessing()
        {
            // деинициализировать роботов
            foreach (var robot in robots)
            {
                try
                {
                    robot.DeInitialize();
                    context.UnsubscribeRobot(robot);
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка останова робота " + robot.GetUniqueName() + ":", ex);
                }                
            }
            // выполнить логаут и закрыть прокси
            try
            {
                context.Logout(protectedContext.MakeProtectedContext());
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка закрытия контекста счета " + AccountId, ex);
            }
            protectedContext = null;
        }

        /// <summary>
        /// дать роботам котировки в работу
        /// </summary>
        public void OnQuotesReceived(string[] names, CandleDataBidAsk[] quotes, bool isHistoryQuotes)
        {
            if (context == null) return;
            try
            {
                context.OnQuotesReceived(names, quotes, isHistoryQuotes);
            }
            catch (Exception ex)
            {
                logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogMsgQuoteRecvError,
                    1000 * 60 * 5, "Ошибка в OnQuotesReceived (счет {0}): {1}",
                        AccountId, ex);
            }
        }
    
        /// <summary>
        /// если счет подписан на "топ" - обновить состав участников топа
        /// </summary>
        public void UpdatePortfolio()
        {
            var sets = RobotFarm.Instance.portfolioTradeSettings;
            if (sets == null) return;
            int? subscribedPortfolioId = 0;
            
            try
            {
                // получить информацию по портфелю
                var portfolioId = TradeSharpAccountStatistics.Instance.proxy.GetSubscribedTopPortfolioId(UserLogin);
                subscribedPortfolioId = portfolioId > 0 ? portfolioId : (int?) null;
            }
            catch (Exception ex)
            {
                RobotFarm.Instance.AppendLogMessage(string.Format("Невозможно получить информацию по портфелю ({0}): {1}",
                    UserLogin, ex.Message));
                Logger.Error("GetSubscribedTopPortfolio() error", ex);
            }

            if (!subscribedPortfolioId.HasValue)
                return;

            var status = RequestStatus.ServerError;
            try
            {
                status = proxyTrade.proxy.SubscribeOnPortfolio(protectedContext.MakeProtectedContext(),
                                                      UserLogin,
                                                      new TopPortfolio
                                                          {
                                                              Id = subscribedPortfolioId.Value
                                                          }, null, sets);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("UpdatePortfolio(\"{0}\", {1}): {2}",
                    UserLogin, subscribedPortfolioId, ex);
            }

            RobotFarm.Instance.AppendLogMessage(string.Format("Обновление портфеля (\"{0}\", {1}) {2}: {3}",
                    UserLogin, subscribedPortfolioId, 
                    status == RequestStatus.OK ? "произведено" : "неуспешно",
                    EnumFriendlyName<RequestStatus>.GetString(status)));
        }
    
        public void ReviveChannel()
        {
            try
            {
                proxyTrade.proxy.ReviveChannel(protectedContext.MakeProtectedContext(),
                    UserLogin, AccountId, TerminalVersionString);
            }
            catch (Exception ex)
            {
                logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                    LogMsgPingError, 1000 * 60 * 30, "Ошибка в proxy.ReviveChannel(): {0}", ex);
                throw;
            }
        }

        public List<MarketOrder> GetAccountOrders()
        {
            if (accountData == null) return new List<MarketOrder>();
            try
            {
                return accountData.GetActualOrderList();
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка получения сделок по счету {0}: {1}", AccountId, ex);
                return new List<MarketOrder>();
            }
        }

        private static long LongRandom(long min, long max, Random rand)
        {
            var buf = new byte[8];
            rand.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);

            return (Math.Abs(longRand % (max - min)) + min);
        }
    }
}

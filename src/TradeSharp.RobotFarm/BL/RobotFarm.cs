using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.QuoteHistory;
using TradeSharp.RobotFarm.Request;
using TradeSharp.Util;

namespace TradeSharp.RobotFarm.BL
{
    class RobotFarm
    {
        #region Singletone

        private static readonly Lazy<RobotFarm> instance = new Lazy<RobotFarm>(() => new RobotFarm());

        public static RobotFarm Instance
        {
            get { return instance.Value; }
        }

        private RobotFarm()
        {
            configFilePath = ExecutablePath.ExecPath + "\\farmsets.xml";
            timerThread = new Thread(TimerThreadFunction);
            portfolioTradeSettings = new AutoTradeSettings();
            packerPool = new CandlePackerPool();
        }

        #endregion

        private int stateOfFarm = (int)FarmState.Stopped;
        /// <summary>
        /// текущее состояние (Запущена, Остановлена, Запускается, Останавливается)
        /// </summary>
        public FarmState State
        {
            get { return (FarmState)stateOfFarm; }
            set { Interlocked.Exchange(ref stateOfFarm, (int)value); }
        }

        private readonly string configFilePath;

        private List<FarmAccount> accounts = new List<FarmAccount>();
        [PropertyXMLTag("Account")]
        public List<FarmAccount> Accounts
        {
            get { return accounts; }
            set { accounts = value; }
        }

        public readonly ThreadSafeList<string> logMessages = new ThreadSafeList<string>();

        private const int MaxMessagesInLog = 1000, ClearLogBy = 100;

        private const string QuoteFolder = "\\quotes";

        private readonly CandlePackerPool packerPool;

        private readonly Thread timerThread;

        private const int TimerThreadInterval = 250;

        public AutoTradeSettings portfolioTradeSettings;

        private const string TradeSettingsNodeName = "tradeSettings";

        #region Загрузка - сохранение
        /// <summary>
        /// загрузить из XML-файла конфигурации настройки "фермы" -
        /// счета
        /// </summary>
        public void LoadSettings()
        {
            if (!File.Exists(configFilePath)) return;
            var doc = new XmlDocument();
            try
            {
                using (var sr = new StreamReader(configFilePath, Encoding.UTF8))
                    doc.Load(sr);
            }
            catch (Exception ex)
            {
                var msgError = "RobotFarm.LoadSettings() - ошибка чтения файла настроек " + configFilePath +
                               ", " + ex;
                Logger.Error(msgError);
                AppendLogMessage(msgError);
                return;
            }
            if (doc.DocumentElement == null) return;

            // настройки авто-торговли
            var nodeTradeSets =
                doc.DocumentElement.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == TradeSettingsNodeName);
            if (nodeTradeSets != null)
            {
                portfolioTradeSettings.LoadTradeSettings((XmlElement)nodeTradeSets);
                Logger.InfoFormat("Настройки портфеля: торговать={0}, %плеча={1:f0}, макс. плечо={2:f1}",
                    portfolioTradeSettings.TradeAuto, portfolioTradeSettings.PercentLeverage,
                    portfolioTradeSettings.MaxLeverage ?? 0);
            }

            try
            {
                PropertyXMLTagAttribute.InitObjectProperties(this, doc.DocumentElement);
                // подгрузить роботов
                foreach (XmlElement nodeAcc in doc.DocumentElement.ChildNodes)
                {
                    var acIdNode = (XmlElement)nodeAcc.SelectSingleNode("AccountId");
                    if (acIdNode == null) continue;
                    var acId = acIdNode.Attributes["value"] == null
                                   ? -1
                                   : (acIdNode.Attributes["value"].Value.ToIntSafe() ?? -1);
                    var account = Accounts.FirstOrDefault(a => a.AccountId == acId);
                    if (account != null)
                        account.LoadRobots(nodeAcc);
                }
            }
            catch (Exception ex)
            {
                var msgError = "RobotFarm.LoadSettings() - ошибка применения настроек " + ex;
                Logger.Error(msgError);
                AppendLogMessage(msgError);
            }
            var accsLoaded = Accounts;
            var msg = string.Format("Настройки успешно применены, загружено {0} счетов, {1} роботов",
                                    accsLoaded.Count, accsLoaded.Sum(a => a.Robots.Count));
            Logger.InfoFormat(msg);
            AppendLogMessage(msg);
        }

        public bool SaveSettings(XmlDocument doc)
        {
            var docNode = (XmlElement)doc.AppendChild(doc.CreateElement("farm"));
            try
            {
                PropertyXMLTagAttribute.SaveObjectProperties(this, docNode);
            }
            catch (Exception ex)
            {
                Logger.Error("RobotFarm.SaveSettings() - ошибка применения настроек к документу " + ex);
                return false;
            }

            try
            {
                foreach (var acc in Accounts)
                {
                    // сохранить роботов
                    var accItem = (XmlElement)doc.SelectSingleNode("farm/Account/AccountId[@value='" + acc.AccountId + "']");
                    if (accItem == null) continue;
                    acc.SaveRobots((XmlElement)accItem.ParentNode);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("RobotFarm.SaveSettings() - ошибка сохранения роботов " + ex);
                return false;
            }

            // настройки авто-торговли
            if (portfolioTradeSettings != null)
                portfolioTradeSettings.SaveTradeSettings(docNode, TradeSettingsNodeName);

            return true;
        }

        public void SaveSettings()
        {
            var doc = new XmlDocument();
            if (!SaveSettings(doc)) return;

            try
            {
                using (var sw = new StreamWriter(configFilePath, false, Encoding.UTF8))
                {
                    using (var xw = new XmlTextWriter(sw) { Formatting = Formatting.Indented })
                    {
                        doc.Save(xw);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("RobotFarm.LoadSettings() - ошибка сохранения файла настроек " + configFilePath +
                    ", " + ex);
            }
        }

        public int LoadRobotsFromXml(XmlDocument doc, int accountId)
        {
            if (doc == null || doc.DocumentElement == null)
            {
                AppendLogMessage("Загрузка портфеля - документ пуст");
                return 0;
            }

            // счет
            var account = Accounts.FirstOrDefault(a => a.AccountId == accountId);
            if (account == null)
            {
                AppendLogMessage("Загрузка портфеля - счет " + accountId + " не найден");
                return 0;
            }

            // прочитать роботню
            var count = account.LoadRobots(doc.DocumentElement);
            AppendLogMessage("Загрузка портфеля - прочитано " + count + " роботов для счета " + accountId);
            return count;
        }
        #endregion

        public FarmAccount GetAccountById(int id)
        {
            return Accounts.FirstOrDefault(a => a.AccountId == id);
        }

        public void StartOrStopFarm()
        {
            if (State == FarmState.Stopped)
                StartFarm();
            else StopFarm();
        }

        private void StartFarm()
        {
            if (State != FarmState.Stopped) return;

            // есть ли счета, разрешенные для торговли?
            if (Accounts.Count == 0 || !Accounts.Any(a => a.TradeEnabled)) return;
            var enabledAccounts = Accounts.Where(a => a.TradeEnabled).ToList();
            var robotsTotal = enabledAccounts.Sum(a => a.Robots.Count);

            // определить интервал истории, необходимый для загрузки
            // сменить состояние - ферма стартует
            State = FarmState.Starting;
            AppendLogMessage("Старт работы в " + DateTime.Now.ToStringUniform());
            var historyByTicker = BuildHistoryByTicker(robotsTotal, enabledAccounts);

            try
            {
                // актуализировать историю и запустить роботов
                ThreadPool.QueueUserWorkItem(ActualizeQuotesAndStartCyborgs,
                    historyByTicker.ToDictionary(h => h.Key, h => (DateTime?)h.Value));
            }
            catch (Exception ex)
            {
                AppendLogMessage("Ошибка запуска фермы роботов: " + ex);
            }
        }

        private Dictionary<string, DateTime> BuildHistoryByTicker(int robotsTotal, List<FarmAccount> enabledAccounts)
        {
            var historyByTicker = new Dictionary<string, DateTime>();

            if (robotsTotal > 0)
            {
                foreach (var acc in enabledAccounts)
                {
                    foreach (var robot in acc.Robots)
                    {
                        try
                        {
                            var robotHistoryRequest = robot.GetRequiredSymbolStartupQuotes(DateTime.Now) 
                                ?? new Dictionary<string, DateTime>();
                            foreach (var pair in robotHistoryRequest)
                            {
                                if (string.IsNullOrEmpty(pair.Key) || pair.Value == default(DateTime))
                                    AppendLogMessage(string.Format("Ошибка запроса истории по роботу '{0}' (счет {1}): " +
                                        " некорректный тикер/время старта",
                                        robot.GetUniqueName(), acc.AccountId));
                                if (historyByTicker.ContainsKey(pair.Key))
                                {
                                    if (historyByTicker[pair.Key] > pair.Value)
                                        historyByTicker[pair.Key] = pair.Value;
                                }
                                else
                                    historyByTicker.Add(pair.Key, pair.Value);
                            }
                        }
                        catch (Exception ex)
                        {
                            AppendLogMessage(string.Format("Ошибка запроса истории по роботу '{0}' (счет {1}): {2}", 
                                robot.GetUniqueName(), acc.AccountId, ex));
                        }                        
                    }
                }

                AppendLogMessage("Следующие котировки будут обновлены: " +
                                 string.Join(", ",
                                     historyByTicker.Select(p => string.Format("{0} с {1:dd.MM.yyyy}", p.Key, p.Value))));
            }
            return historyByTicker;
        }

        public void StopFarm()
        {
            // остановить роботов
            if (State != FarmState.Started) return;

            State = FarmState.Stopping;
            FarmScheduler.Instance.Stop();
            ThreadPool.QueueUserWorkItem(o =>
            {
                var acList = Accounts;
                foreach (var account in acList)
                {
                    try
                    {
                        account.StopProcessing();
                    }
                    catch (Exception ex)
                    {
                        AppendLogMessage("Ошибка останова счета " + account.AccountId + " " + ex);
                    }
                }
                State = FarmState.Stopped;
            });
        }

        private void ActualizeQuotesAndStartCyborgs(object quotesObj)
        {
            var quotes = (Dictionary<string, DateTime?>)quotesObj;
            Dictionary<string, List<CandleDataBidAsk>> dicQuote = null;

            if (quotes.Count > 0)
            {
                // создать директорию котировок, если не существует
                var quotesFolder = ExecutablePath.ExecPath + QuoteFolder;
                try
                {
                    if (!Directory.Exists(quotesFolder))
                        Directory.CreateDirectory(quotesFolder);
                }
                catch (Exception ex)
                {
                    AppendLogMessage("Ошибка создания директории \"" + quotesFolder + "\": " + ex);
                    State = FarmState.Stopped;
                    return;
                }

                // освежить котировки в директории за указанный период
                dicQuote = TickerStorage.Instance.GetQuotes(quotes).Where(p => p.Value.Count > 0).ToDictionary(
                    p => p.Key, p => p.Value);
                AppendLogMessage("Завершена актуализация " + quotes.Count + " котировок");
            }

            // подготовить контекст
            var accountList = Accounts;
            var countOk = accountList.Count;
            foreach (var account in accountList)
            {
                if (!account.SetupLiveContext())
                    countOk--;
            }

            if (countOk == 0)
            {
                State = FarmState.Stopped;
                return;
            }
            AppendLogMessage("Контекст роботов инициализирован");

            // собрать статистику по времени обработки истории котировок
            // дать роботам переварить историю котировок, заботливо подкачанную заранее
            if (quotes.Count > 0)
            {
                var stream = new HistoryTickerStream(dicQuote);
                using (new TimeLogger("Ферма роботов стартовала, обработка истории заняла"))
                    while (true)
                    {
                        string[] names;
                        CandleDataBidAsk[] histQuotes;
                        if (!stream.Step(out names, out histQuotes)) break;                        
                        foreach (var account in accountList)
                        {
                            try
                            {
                                account.OnQuotesReceived(names, histQuotes, true);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error("Ошибка обработки котировок по счету " + account.AccountId + ": " + ex);
                            }
                        }
                    }
            }

            // запланировать обновления портфелей
            try
            {
                FarmScheduler.Instance.ScheduleTopPortfolioUpdates(accounts);
                FarmScheduler.Instance.Start();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка старта обновления портфелей", ex);
                throw;
            }            

            State = FarmState.Started;

            // запустить поток, по-расписанию выполняющий процедуры
            timerThread.Start();

            AppendLogMessage("Запуск осуществлен");
        }

        public void AppendLogMessage(string msg)
        {
            logMessages.Add(DateTime.Now.ToString("HH:mm:ss") + " " + msg, 1000);
            // отрезать линюю историю
            if (logMessages.Count <= MaxMessagesInLog) return;
            bool timeout;
            var subMsgList = logMessages.ExtractAll(1000, out timeout);
            var countTarget = MaxMessagesInLog - ClearLogBy;
            subMsgList = subMsgList.Skip(Math.Max(0, subMsgList.Count() - countTarget)).Take(countTarget).ToList();
            logMessages.AddRange(subMsgList, 1000);
        }

        public void OnQuotesReceived(string[] names, QuoteData[] quotes)
        {
            if (State != FarmState.Started) return;
            // if (RobotFarmOffTime.IsTimeOff()) return;
            var candles = packerPool.MakeCandles(quotes, ref names);
            foreach (var account in Accounts)
            {
                try
                {
                    account.OnQuotesReceived(names, candles, false);
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка обработки котировок по счету " + account.AccountId + ": " + ex.Message);
                }
            }
        }

        private void TimerThreadFunction()
        {
            while (State == FarmState.Started)
            {
                Thread.Sleep(TimerThreadInterval);
            }
        }
    }
}

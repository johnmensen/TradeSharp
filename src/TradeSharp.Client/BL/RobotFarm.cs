using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using Candlechart.Indicator;
using Entity;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Robot.BacktestServerProxy;
using TradeSharp.Robot.Robot;
using TradeSharp.Util;

namespace TradeSharp.Client.BL
{
    /// <summary>
    /// обеспечивает real-time торговлю роботов
    /// хранит контекст, при старте зачитывает историю для
    /// "разгона конвейера" роботов
    /// </summary>
    public class RobotFarm
    {
        public enum RobotFarmState { Stopped = 0, StartingUp, Started, Stopping, Undefined }
        public delegate void StartupFinishedDel();
        public delegate void StateChangedDel(RobotFarmState state);

        private readonly ReaderWriterLock lockerState = new ReaderWriterLock();
        private const int LockerStateTimeout = 300;
        private RobotFarmState state;
        public RobotFarmState State
        {
            get
            {
                try
                {
                    lockerState.AcquireReaderLock(LockerStateTimeout);
                }
                catch
                {
                    return RobotFarmState.Undefined;
                }
                try
                {
                    return state;
                }
                finally
                {
                    lockerState.ReleaseReaderLock();
                }
            }
            set
            {
                try
                {
                    lockerState.AcquireWriterLock(LockerStateTimeout);
                }
                catch
                {
                    return;
                }
                try
                {
                    state = value;
                }
                finally
                {
                    lockerState.ReleaseWriterLock();
                }
                // вызвать событие - state changed
                if (onStateChanged != null) onStateChanged(state);
            }
        }

        private const string RobotFileName = "robots.xml";
        private TerminalLiveRobotContext robotContext;
        private List<BaseRobot> robots = new List<BaseRobot>();
        private readonly BackgroundWorker startRoutine = new BackgroundWorker();
        private readonly BackgroundWorker processQuoteRoutine = new BackgroundWorker();        
        private const int MinMinutesToLoadQuotes = 5;

        private StartupFinishedDel onStartupFinished;
        public event StartupFinishedDel OnStartupFinished
        {
            add { onStartupFinished += value; }
            remove { onStartupFinished -= value; }
        }

        private StateChangedDel onStateChanged;
        public event StateChangedDel OnStateChanged
        {
            add { onStateChanged += value; }
            remove { onStateChanged -= value; }
        }

        private readonly UpdateTickersCacheForRobotsExDel updateTickersCacheForRobots;

        private readonly CandlePackerPool packerPool;

        public RobotFarm(UpdateTickersCacheForRobotsExDel updateTickersCacheForRobots)
        {
            packerPool = new CandlePackerPool();
            this.updateTickersCacheForRobots = updateTickersCacheForRobots;
            startRoutine.RunWorkerCompleted += StartRoutineCompleted;
            startRoutine.DoWork += StartRoutineDoWork;
            processQuoteRoutine.DoWork += ProcessQuoteRoutineDoWork;
            try
            {
                robots = LoadRobots();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка загрузки роботов", ex);
            }
        }

        public bool Start(Account accountInfo)
        {
            if (state != RobotFarmState.Stopped) return false;
            if (accountInfo == null) return false;
            robotContext = new TerminalLiveRobotContext(MainForm.serverProxyTrade, accountInfo, () => AccountStatus.Instance.Login);
            // загрузить роботов
            robots = LoadRobots();
            Logger.InfoFormat("Загружено {0} роботов", robots.Count);
            foreach (var robot in robots)
                robot.Initialize(robotContext, CurrentProtectedContext.Instance);
            if (robots.Count == 0) return false;

            if (MainForm.Instance.ChatEngine != null)
                foreach (Chat.Client.BL.IChatSpamRobot bot in robots.Where(r => r is Chat.Client.BL.IChatSpamRobot))
                {
                    bot.EnterRoom += MainForm.Instance.ChatEngine.EnterRoom;
                    bot.SendMessageInRoom += MainForm.Instance.ChatEngine.SendMessageInRoom;
                }

            State = RobotFarmState.StartingUp;
            // загрузить историю для роботов            
            startRoutine.RunWorkerAsync();
            return true;
        }

        public void Stop()
        {
            State = RobotFarmState.Stopping;
            // ждать завершения processQuoteRoutine
            // ...
            State = RobotFarmState.Stopped;
            foreach (var robot in robots)
                robot.DeInitialize();
            // сохранить настройки роботов, которые, возможно, поменялись 
            SaveRobots(robots);
        }

        public void UpdateRobotContext(Account account)
        {
            if (State != RobotFarmState.Started)
                return;
            if (robots == null || robots.Count == 0)
                return;
            robotContext.AccountInfo = account;
            robots.ForEach(robot => robot.UpdateAccountInfo(account));
        }

        public static List<BaseRobot> LoadRobots()
        {
            var listRobots = new List<BaseRobot>();
            var fileName = string.Format("{0}\\{1}", ExecutablePath.ExecPath, RobotFileName);
            if (!File.Exists(fileName)) return listRobots;

            try
            {
                var doc = new XmlDocument();
                doc.Load(fileName);
                var node = doc.SelectSingleNode("RobotsPortfolio");
                var nodes = node.SelectNodes("robot");

                foreach (XmlElement item in nodes)
                {
                    var inodes = item.SelectNodes("Robot.TypeName");
                    var inode = (XmlElement)inodes[0];
                    var title = inode.Attributes["value"].Value;
                    var robot = RobotCollection.MakeRobot(title);
                    PropertyXMLTagAttribute.InitObjectProperties(robot, item);
                    listRobots.Add(robot);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка чтения файла \"{0}\": {1}", fileName, ex);
            }
            return listRobots;
        }

        public static void SaveRobots(List<BaseRobot> robots)
        {
            var fileName = string.Format("{0}\\{1}", ExecutablePath.ExecPath, RobotFileName);
            SaveRobots(robots, fileName);
        }

        public static void SaveRobots(List<BaseRobot> robots, string path)
        {
            var doc = new XmlDocument();
            var xmlNode = (XmlElement)doc.AppendChild(doc.CreateElement("RobotsPortfolio"));
            foreach (var robot in robots)
            {
                var xmlChild = (XmlElement)xmlNode.AppendChild(doc.CreateElement("robot"));
                PropertyXMLTagAttribute.SaveObjectProperties(robot, xmlChild);
            }
            using (var sw = new StreamWriterLog(path, false))
            {
                using (var xw = new XmlTextWriter(sw) { Formatting = Formatting.Indented })
                {
                    doc.Save(xw);
                }
            }
        }

        public void OnQuotesReceived(string[] names, QuoteData[] quotes)
        {
            if (State != RobotFarmState.Started) return;
            var quoteList = names.Select((t, i) => new Cortege2<string, QuoteData>(t, quotes[i])).ToList();
            if (processQuoteRoutine.IsBusy) return;
            processQuoteRoutine.RunWorkerAsync(quoteList);
        }

        private void StartRoutineDoWork(object sender, DoWorkEventArgs e)
        {
            // загрузить историю котировок и отработать старт роботов
            var quotesToReceive = GetUsedTickersAndStartTime();
            if (quotesToReceive.Count == 0) return;

            var firstDate = quotesToReceive.Min(q => q.Value);
            if ((DateTime.Now - firstDate).TotalMinutes < MinMinutesToLoadQuotes) return;
            // обновить кэш котировок
            updateTickersCacheForRobots(quotesToReceive, 3);
            
            Logger.InfoFormat("Портфель роботов: загрузка {0} котировок c {1:dd.MM.yyyy HH:mm}",
                quotesToReceive.Count, firstDate);
            // закачать котировки и "скормить" их роботам
            var cursor = new BacktestTickerCursor();
            // !! path !!
            var path = ExecutablePath.ExecPath + "\\quotes";
            try
            {
                if (!cursor.SetupCursor(path, quotesToReceive.Keys.ToList(), firstDate))
                {
                    Logger.DebugFormat("Старт портфеля из {0} роботов: не удалось установить курсор, путь \"{1}\"",
                        robots.Count, path);
                    return;
                }
                do
                {
                    var candles = cursor.GetCurrentQuotes();
                    if (candles.Count == 0) break;
                    
                    var names = candles.Select(q => q.a).ToArray();
                    var prices = candles.Select(q => 
                        new CandleDataBidAsk(q.b, DalSpot.Instance.GetDefaultSpread(q.a))).ToArray();
                    // дать роботам котировки в работу
                    foreach (var robot in robots)
                    {
                        var robotEvents = robot.OnQuotesReceived(names, prices, true);
                        if (robotEvents == null || robotEvents.Count == 0) continue;
                        ShowRobotEventsSafe(robot, robotEvents);
                    }
                } while (cursor.MoveNext());
            }
            finally
            {
                cursor.Close();
            }
        }

        private static void ShowRobotEventsSafe(BaseRobot robot, List<string> robotEvents)
        {
            var hints = new List<RobotMark>();
            foreach (var eventStr in robotEvents)
            {
                var hint = RobotMark.ParseString(eventStr);
                if (hint != null) hints.Add(hint);
                else
                    MainForm.Instance.ProcessRobotMessage(DateTime.Now, eventStr);
            }
            if (hints.Count == 0) return;
            // показать хинты на графике
            MainForm.Instance.ShowRobotHintsSafe(robot, hints);
        }

        private void StartRoutineCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // загрузка роботов завершена - разрешить обработку котировок роботами
            if (onStartupFinished != null) onStartupFinished();
            State = RobotFarmState.Started;
            Logger.Info("Портфель роботов: загрузка котировок завершена");
        }

        /// <summary>
        /// отдать "живые" котировки роботам
        /// </summary>        
        private void ProcessQuoteRoutineDoWork(object sender, DoWorkEventArgs e)
        {
            var quoteList = (List<Cortege2<string, QuoteData>>)e.Argument;
            string[] quoteNames;
            CandleDataBidAsk[] candles;
            packerPool.MakeCandles(quoteList, out quoteNames, out candles);
            
            foreach (var robot in robots)
            {
                List<string> robotEvents;
                try
                {
                    robotEvents = robot.OnQuotesReceived(quoteNames, candles, false);
                    if (robotEvents == null || robotEvents.Count == 0) continue;
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Ошибка в ProcessQuoteRoutineDoWork ({0}): {1}",
                        robot.GetUniqueName(), ex);
                    continue;
                }
                
                ShowRobotEventsSafe(robot, robotEvents);
            }
        }

        private Dictionary<string, DateTime> GetUsedTickersAndStartTime()
        {
            var timeNow = DateTime.Now;
            var allTickers = new Dictionary<string, DateTime>();

            foreach (var robot in robots)
            {
                var tickers = robot.GetRequiredSymbolStartupQuotes(timeNow);
                // добавить тикеры, которые затребовал робот
                if (tickers == null) continue;
                foreach (var ticker in tickers)
                {
                    if (allTickers.ContainsKey(ticker.Key))
                    {
                        if (ticker.Value < allTickers[ticker.Key])
                            allTickers[ticker.Key] = ticker.Value;
                    }
                    else
                        allTickers.Add(ticker.Key, ticker.Value);
                }                
                // ! тикеры из Graphics робота не берутся, если не затребованы им отдельно
            }
            return allTickers;
        }
    
        public List<BaseRobot> GetRobotCopies()
        {
            return robots.Select(r => r.MakeCopy()).ToList();
        }

        public List<BaseRobot> GetRobotsAsIs()
        {
            return robots.ToList();
        }

        public void SetRobotSettings(List<BaseRobot> newRobotsPortfolio)
        {
            robots = newRobotsPortfolio;
        }
    }
}

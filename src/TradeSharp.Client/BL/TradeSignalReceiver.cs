using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading;
using Entity;
using TradeSharp.Client.Subscription.Model;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;
using System.Linq;

namespace TradeSharp.Client.BL
{
    /// <summary>
    /// опрашивает сервер на предмет торговых сигналов
    /// </summary>
    class TradeSignalReceiver
    {
        #region URI / User info
        // http://70.38.11.49:8062
        private static string url = AppConfig.GetStringParam("Service.Url", "http://70.38.11.49:8062");
        private static string username = AppConfig.GetStringParam("Service.Username", "termsharp");
        private static string pass = AppConfig.GetStringParam("Service.Pass", "sharpsharp");
        #endregion

        public delegate void TradeSignalsUpdatedDel(List<TradeSignalUpdate> signals);

        private readonly MessageDateFileStorage messageDateFileStorage =
            new MessageDateFileStorage(ExecutablePath.ExecPath + "\\accountevent.filter");

        private static TradeSignalReceiver instance;

        public static TradeSignalReceiver Instance
        {
            get { return instance ?? (instance = new TradeSignalReceiver()); }
        }

        private TradeSignalsUpdatedDel tradeSignalsUpdated;

        public event TradeSignalsUpdatedDel TradeSignalsUpdated
        {
            add { tradeSignalsUpdated += value; }
            remove { tradeSignalsUpdated -= value; }
        }

        private TradeSignalReceiver()
        {
            serverRequest.OnResponce += serverRequest_OnResponce;
        }

        private const int PollIntervalMils = 1000 * 8;

        private const int ThreadSleepInterval = 200;

        private const int ServerRequestTimeooutMils = 5 * 1000;

        private Thread pollThread;

        private volatile bool isStopping = true;

        private readonly AsynchWebRequest serverRequest = new AsynchWebRequest();

        private readonly FloodSafeLogger logNoFlood = new FloodSafeLogger(1000 * 10);

        private const int LogServerRequestError = 1;

        private const int LogServerResponseNil = 2;

        private const int LogServerRequestXmlError = 3;

        private const int LogServerResponseXmlNil = 4;

        private const int LogDoPollServerError = 5;

        private const int LogDoPollServerCommonError = 6;

        private const int LogDoPollAccountEventsError = 7;

        public void Start()
        {
            isStopping = false;
            pollThread = new Thread(ThreadRoutine);
            pollThread.Start();
        }

        public void Stop()
        {
            if (isStopping) return;
            isStopping = true;
            if (!pollThread.Join(1000 * 2))
            {
                try
                {
                    pollThread.Abort();
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка останова TradeSignalReceiver", ex);
                }
            }
        }

        private void ThreadRoutine()
        {
            var countInters = PollIntervalMils / ThreadSleepInterval;
            var numInter = countInters;

            try
            {
                while (!isStopping)
                {
                    Thread.Sleep(ThreadSleepInterval);
                    numInter--;
                    if (numInter > 0) continue;
                    numInter = countInters;

                    // получить XML "прогнозов"
                    try
                    {
                        DoPollServer();
                    }
                    catch (Exception ex)
                    {
                        logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                            LogDoPollServerCommonError, 1000 * 60 * 5, "TradeSignalReceiver.DoPollServer() common error: {0}", ex);
                    }

                    // получить события, связанные со счетом
                    try
                    {
                        PollAccountEvents();
                    }
                    catch (Exception ex)
                    {
                        logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                            LogDoPollAccountEventsError, 1000 * 60 * 5, "TradeSignalReceiver.PollAccountEvents() common error: {0}", ex);
                    }
                }
            }
            catch (ThreadAbortException)
            {
            }
        }

        private void DoPollServer()
        {
            // запрашиваемые категории сигналов
            var signals = GetTradeSignalsSubscribed();
            if (signals == null || signals.Count == 0) return;
            var subscribedSignalCategories = signals.Select(s => s.Service).ToList();

            // запросить на сервере обновления,
            // сверить с уже имеющимися данными,
            if (serverRequest.RequestInProcess) return;

            try
            {
                bool result;
                // указать в параметрах запроса все торговые сигналы, на которые подписан клиент
                var uri = url + "?" + TradeSignalXml.ReqPtrSignalUpdates + "=" +
                          string.Join("_", subscribedSignalCategories);

                if (!string.IsNullOrEmpty(username))
                    result = serverRequest.StartRequest(uri, ServerRequestTimeooutMils,
                        new NetworkCredential(username, pass));
                else
                    result = serverRequest.StartRequest(uri, ServerRequestTimeooutMils);
                if (!result)
                    logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogServerRequestError, 1000 * 60 * 5,
                        "DoPollServer: making request to \"{0}\" is unsuccessfull", url);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("DoPollServer: error creating request to {0}: {1}", url, ex);
                return;
            }
        }

        private void serverRequest_OnResponce(Stream s)
        {
            if (s == null)
            {
                logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogServerResponseNil, 1000 * 60 * 5,
                    "DoPollServer: response from {0} is null", url);
                return;
            }

            // сигналы, которые будут отправлены коду главного окна
            var signals = new List<TradeSignalUpdate>();
            try
            {
                // распарсить ответ - plain text вида
                // 1_EURUSD_0;#240_20.12.2012 11:04:42
                // 1_GBPUSD_0;#240_20.12.2012 11:17:20                
                using (var sr = new StreamReader(s, TradeSignalXml.DefaultEncoding))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (string.IsNullOrEmpty(line)) continue;
                        if (line == "empty") continue;
                        TradeSignalUpdate signal;
                        if (!ProcessUpdateRequestLine(line, out signal))
                            Logger.ErrorFormat("TradeSignalReceiver.ProcessUpdateRequestLine() - " +
                                " ошибка обработки строки \"{0}\"", line);
                        if (signal != null) signals.Add(signal);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("DoPollServer error", ex);
                logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogDoPollServerError, 1000 * 60 * 5,
                    "DoPollServer error", ex);
            }

            // подгрузить XML
            if (signals.Count == 0) return;
            signals = UploadSignalsXml(signals);
            if (signals.Count == 0) return;

            // вызвать событие
            try
            {
                if (tradeSignalsUpdated != null)
                    tradeSignalsUpdated(signals);
            }
            catch (Exception ex)
            {
                Logger.Info("TradeSignalReceiver.DoPollServer - event firing error", ex);
            }
        }

        /// <summary>
        /// обработать строку вида
        /// 1_EURUSD_0;#240_20.12.2012 11:04:42        
        /// </summary>        
        private static bool ProcessUpdateRequestLine(string line, out TradeSignalUpdate signalUpdate)
        {
            signalUpdate = null;
            var parts = line.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4) return false;
            // категория ТС
            var catId = parts[0].ToIntSafe();
            if (!catId.HasValue) return false;
            // тикер
            var ticker = parts[1];
            if (!DalSpot.Instance.GetTickerNames().Contains(ticker)) return false;
            // таймфрейм
            var timeframeStr = parts[2];
            var barSets = BarSettings.TryParseString(timeframeStr);
            if (barSets == null) return false;
            // время обновления
            var timeUpdated = parts[3].ToDateTimeUniformSafe();
            if (!timeUpdated.HasValue) return false;

            // проверить актуальность обновления
            var localUpdateInfo = TradeSignalFileStorage.Instance.FindTradeSignal(catId.Value, ticker,
                barSets.ToString());
            if (localUpdateInfo != null && localUpdateInfo.TimeUpdated == timeUpdated.Value)
                return true; // такое обновление уже имело место
            signalUpdate = new TradeSignalUpdate(catId.Value, ticker, barSets, timeUpdated.Value);
            return true;
        }

        /// <summary>
        /// по указанным сигналам загрузить с сервера XML
        /// вернуть список сигналов, по которым XML успешно прочитан
        /// </summary>        
        private List<TradeSignalUpdate> UploadSignalsXml(List<TradeSignalUpdate> signalsToUpload)
        {
            var processedSignals = new List<TradeSignalUpdate>();
            foreach (var signal in signalsToUpload)
            {
                if (LoadTradeSignalXml(signal))
                    processedSignals.Add(signal);
            }
            return processedSignals;
        }

        private bool LoadTradeSignalXml(TradeSignalUpdate signal)
        {
            // указать в параметрах запроса параметры сигнала
            var reqParams = new Dictionary<string, string>
                                {
                                    { TradeSignalXml.ReqPtrGetForecast, "1" },
                                    { TradeSignalXml.ReqPtrForecastCategory, signal.ServiceId.ToString() },
                                    { TradeSignalXml.ReqPtrForecastTicker, signal.Ticker },
                                    { TradeSignalXml.ReqPtrForecastTimeframe, signal.Timeframe.ToString(TradeSignalXml.TimeframeSeparator) }
                                };
            var queryString = string.Join("&", reqParams.Select(p => string.Format("{0}={1}", p.Key, p.Value)));

            WebRequest serverRequest;
            try
            {
                serverRequest = WebRequest.Create(url + "?" + queryString);
                serverRequest.Timeout = ServerRequestTimeooutMils;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("LoadTradeSignalXml: error creating request to {0}: {1}", url, ex);
                return false;
            }
            if (!string.IsNullOrEmpty(username))
                serverRequest.Credentials = new NetworkCredential(username, pass);

            WebResponse serverResponse;
            try
            {
                serverResponse = serverRequest.GetResponse();
            }
            catch (Exception ex)
            {
                logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogServerRequestXmlError, 1000 * 60 * 5,
                    "LoadTradeSignalXml: error getting response from {0}: {1}", url, ex);
                return false;
            }
            if (serverResponse == null)
            {
                logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogServerResponseXmlNil, 1000 * 60 * 5,
                    "LoadTradeSignalXml: response from {0} is null", url);
                return false;
            }

            try
            {
                // прочитать и сохранить ответ - XML
                using (var s = serverResponse.GetResponseStream())
                {
                    if (s == null) return false;

                    using (var sr = new StreamReader(s, TradeSignalXml.DefaultEncoding))
                    {
                        var signalXml = sr.ReadToEnd();
                        if (string.IsNullOrEmpty(signalXml)) return false;
                        if (signalXml.StartsWith(TradeSignalXml.ResponseErrorPreffix))
                        {
                            Logger.ErrorFormat("LoadTradeSignalXml({0}, {1}, {2}): response error ({3})",
                                signal.ServiceId, signal.Ticker, signal.Timeframe,
                                signalXml.Substring(TradeSignalXml.ResponseErrorPreffix.Length));
                            return false;
                        }
                        if (signalXml == TradeSignalXml.ResponseForecastIsEmpty)
                        {
                            Logger.InfoFormat("LoadTradeSignalXml({0}, {1}, {2}): response is empty",
                                signal.ServiceId, signal.Ticker, signal.Timeframe);
                            return false;
                        }

                        // таки сохранить XML
                        TradeSignalFileStorage.Instance.SaveTradeSignal(signal.ServiceId,
                            signal.Ticker, signal.Timeframe.ToString(), signalXml);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("LoadTradeSignalXml error", ex);
                return false;
            }
            finally
            {
                serverResponse.Close();
            }
        }

        public static List<Contract.Entity.Subscription> GetTradeSignalsSubscribed()
        {
            if (!AccountStatus.Instance.isAuthorized) return null;
            var accountId = AccountStatus.Instance.accountID;

            // получить сигналы от сервера, потом прочитать настройки этих сигналов
            var cats = new List<Contract.Entity.Subscription>();
            try
            {
                cats = TradeSharpAccount.Instance.proxy.GetSubscriptions(AccountStatus.Instance.Login);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("GetTradeSignalsSubscribed({0}) error: {1}", accountId, ex);
            }
            if (cats == null || cats.Count == 0)
                return null;
            return cats;
        }

        public static bool PutForecast(int categoryId, string ticker, BarSettings timeframe, string xmlForecast)
        {
            var wc = new WebClient();
            if (!string.IsNullOrEmpty(username))
                wc.Credentials = new NetworkCredential(username, pass);

            var requestParams = new NameValueCollection
                                    {
                                        { TradeSignalXml.ReqPtrForecastCategory, categoryId.ToString() },
                                        { TradeSignalXml.ReqPtrForecastTicker, ticker },
                                        { TradeSignalXml.ReqPtrForecastTimeframe, timeframe.ToString(TradeSignalXml.TimeframeSeparator) }
                                    };
            wc.QueryString = requestParams;
            try
            {
                var responseBytes = wc.UploadData(url, "PUT", TradeSignalXml.DefaultEncoding.GetBytes(xmlForecast));
                var respString = TradeSignalXml.DefaultEncoding.GetString(responseBytes);
                return respString == "OK";
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в PutForecast()", ex);
                return false;
            }
        }

        /// <summary>
        /// получить события, связанные со счетом (в т.ч. - текстовые "торговые сигналы")
        /// </summary>
        private void PollAccountEvents()
        {
            if (!AccountStatus.Instance.isAuthorized) return;
            // таки получить события
            var acEvents = MainForm.serverProxyTrade.proxy.GetUserEvents(CurrentProtectedContext.Instance.MakeProtectedContext(),
                                                               AccountStatus.Instance.Login,
                                                               UserSettings.Instance.DeleteAccountEventsOnRead);
            if (acEvents.Count == 0) return;

            // из списка полученных событий удалить те, что уже были однажды получены
            FilterOldAccountEvents(ref acEvents);
            if (acEvents.Count == 0) return;

            // таки обработать события
            MainForm.Instance.ProcessAccountEventsSafe(acEvents);
        }

        private void FilterOldAccountEvents(ref List<UserEvent> acEvents)
        {
            var filterDate = messageDateFileStorage.Date;
            acEvents = acEvents.Where(e => e.Time > filterDate).ToList();
            if (acEvents.Count > 0)
                messageDateFileStorage.Date = acEvents.Max(e => e.Time);
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Drawing;
using Entity;
using NewsRobot.UI;
using TradeSharp.Chat.Client.BL;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Robot.BacktestServerProxy;
using TradeSharp.Robot.BL;
using TradeSharp.Robot.Robot;
using TradeSharp.Util;

// ReSharper disable LocalizableElement
namespace NewsRobot
{
    [DisplayName("NewsRobot")]
    public partial class NewsRobot : BaseRobot, IChatSpamRobot
    {
        public enum EnterSignEnum
        {
            Tn = 0,
            NotTn,
            Tba,
            NotTba,
            Tcb,
            NotTcb
        }

        public static readonly Dictionary<string, string> formulaArguments = new Dictionary<string, string>
            {
                {"Tn", "Знак новостного сигнала"},
                {"Tba", "Знак тренда до выхода новости"},
                {"Tcb", "Знак тренда после выхода новости"}
            };

        private int volume = 10000;
        [PropertyXMLTag("Robot.Volume")]
        [DisplayName("Объем сделки")]
        [Category("Торговые")]
        [Description("Объем сделки, ед. базовой валюты")]
        public int Volume
        {
            get { return volume; }
            set { volume = value; }
        }

        private int initialSlPoints = 200;
        [PropertyXMLTag("Robot.InitialSlPoints")]
        [DisplayName("Исходный SL")]
        [Category("Торговые")]
        [Description("Положение исходного SL относительно позиции открытия, пп")]
        public int InitialSlPoints
        {
            get { return initialSlPoints; }
            set { initialSlPoints = value; }
        }

        private  int initialTpPoints = 200;
        [PropertyXMLTag("Robot.InitialTPPoints")]
        [DisplayName("Исходный TP")]
        [Category("Торговые")]
        [Description("Положение исходного TP относительно позиции открытия, пп")]
        public int InitialTpPoints
        {
            get { return initialTpPoints; }
            set { initialTpPoints = value; }
        }

        private int slTpPeriodMinutes = 60;
        [PropertyXMLTag("Robot.SlTpPeriodMinutes")]
        [DisplayName("Период обновления SL/TP, мин")]
        [Category("Торговые")]
        public int SlTpPeriodMinutes
        {
            get { return slTpPeriodMinutes; }
            set { slTpPeriodMinutes = value < 1 ? 60 : value; }
        }

        private int slTpPointsDecrease = 5;
        [PropertyXMLTag("Robot.SlTpPointsDecrease")]
        [DisplayName("Изменение SL/TP")]
        [Category("Торговые")]
        [Description("Величина периодического изменения SL/TP в сторону позиции открытия, пп")]
        public int SlTpPointsDecrease
        {
            get { return slTpPointsDecrease; }
            set { slTpPointsDecrease = value < 1 ? 5 : value; }
        }

        private int newsProcessingDelayMinutes = 5;
        [PropertyXMLTag("Robot.NewsProcessingDelaySeconds")]
        [DisplayName("Ожидание перед обработкой, мин")]
        [Category("Торговые")]
        [Description("Время ожидания перед обработкой новости, мин")]
        public int NewsProcessingDelayMinutes
        {
            get { return newsProcessingDelayMinutes; }
            set { newsProcessingDelayMinutes = value; }
        }

        // ближняя и дальняя отметки времени для аналитики
        private int timeNearMinutes = 5;
        [PropertyXMLTag("Robot.TimeNearMinutes")]
        [DisplayName("Ближняя отметка времени, мин")]
        [Category("Торговые")]
        [Description("Ближняя отметка времени для определения тренда, мин")]
        public int TimeNearMinutes
        {
            get { return timeNearMinutes; }
            set { timeNearMinutes = value; }
        }
        
        private int timeFarMinutes = 60;
        [PropertyXMLTag("Robot.TimeFarMinutes")]
        [DisplayName("Дальняя отметка времени, мин")]
        [Category("Торговые")]
        [Description("Дальняя отметка времени для определения тренда, мин")]
        public int TimeFarMinutes
        {
            get { return timeFarMinutes; }
            set { timeFarMinutes = value; }
        }

        private string formula;
        [PropertyXMLTag("Robot.Formula")]
        [DisplayName("Формула для входа в рынок")]
        [Category("Торговые")]
        [Editor(typeof(FormulaEditor), typeof(UITypeEditor))]
        public string Formula
        {
            get { return formula; }
            set { formula = value; }
        }

        [PropertyXMLTag("Robot.EnterSign")]
        [DisplayName("Знак входа в рынок")]
        [Category("Торговые")]
        public EnterSignEnum EnterSign { get; set; }

        [DisplayName("Обработано новостей")]
        [Category("Новости")]
        [RuntimeAccess(true)]
        public int ProcessedNewsCount
        {
            get { return oldNews.Count; }
        }

        private ThreadSafeTimeStamp lastGrabNewsCall = new ThreadSafeTimeStamp();
        [DisplayName("Время последнего получения новостей")]
        [Category("Новости")]
        [RuntimeAccess(true)]
        public string LastGrabNewsCall
        {
            get { return lastGrabNewsCall.GetLastHit().ToString(); }
        }

        // веса новостей
        private List<NewsSettings> newsSettings;
        // зависимости валют
        private List<CurrencySettings> currencySettings;
        // необработанные, актуальные новости
        private List<RobotNews> freshNews = new List<RobotNews>();
        // обработанные, устаревшие новости
        private List<RobotNews> oldNews = new List<RobotNews>();
        // пауза перед повторным чтением новостей
        private const int PauseForNewsReadingSeconds = 60;
        // время устаревания новости
        private const int NewsObsolescenceTimeMinutes = 10;
        // кэш котировок
        private Dictionary<string, List<QuoteData>> quoteStorage = new Dictionary<string, List<QuoteData>>();
        // ошибки при разборе новостей
        private readonly List<string> logMessages = new List<string>();
        // прочие ошибки
        private readonly List<string> pendingLogMessages = new List<string>();

        private readonly FloodSafeLogger logNoFlood = new FloodSafeLogger(1000 * 60 * 5);

        private const int LogMsgNoNewsParsed = 0;

        private const int LogMsgNewsParsed = 1;
        // комната чата для вывода сообщений
        private const string chatRoom = "NewsRobot";

        private ExpressionResolver expressionResolver;

        private volatile bool isProcessing;

        // вызывается при открытии окна состояния роботов
        public override BaseRobot MakeCopy()
        {
            var robo = new NewsRobot
            {
                Volume = Volume,
                lastGrabNewsCall = lastGrabNewsCall,
                newsSettings = newsSettings,
                currencySettings = currencySettings,
                oldNews = oldNews,
                quoteStorage = quoteStorage
                // возможно, что что-то забыли
            };
            CopyBaseSettings(robo);
            return robo;
        }

        public override void Initialize(RobotContext context, CurrentProtectedContext protectedContext)
        {
            var cfgFileName = Assembly.GetAssembly(GetType()).Location + ".xml";
            string error;
            newsSettings = NewsSettings.LoadNewsSettings(cfgFileName, out error);
            if (error != null)
                pendingLogMessages.Add(error);
            currencySettings = CurrencySettings.LoadCurrencySettings(cfgFileName, out error);
            if (error != null)
                pendingLogMessages.Add(error);
            if(string.IsNullOrEmpty(Formula))
                pendingLogMessages.Add("формула не задана");
            try
            {
                expressionResolver = new ExpressionResolver(Formula.ToLower());
            }
            catch (Exception)
            {
                pendingLogMessages.Add("ошибка в формуле");
            }
            base.Initialize(context, protectedContext);
        }

        public override Dictionary<string, DateTime> GetRequiredSymbolStartupQuotes(DateTime startTrade)
        {
            oldNews.Clear();
            freshNews.Clear();
            lastGrabNewsCall.ResetHit();
            quoteStorage.Clear();
            logMessages.Clear();
            return Graphics.ToDictionary(g => g.a, g => startTrade.AddMinutes(-TimeFarMinutes));
        }

        public override List<string> OnQuotesReceived(string[] names, CandleDataBidAsk[] quotes, bool isHistoryStartOff)
        {
            if (isProcessing)
                return new List<string>();
            try
            {
                isProcessing = true;
                return ProcessQuotes(names, quotes, isHistoryStartOff);
            }
            finally
            {
                isProcessing = false;
            }
        }

        private List<string> ProcessQuotes(string[] names, CandleDataBidAsk[] quotes, bool isHistoryStartOff)
        {
            var result = new List<string>();

            // дописываем лог
            foreach (var m in pendingLogMessages.Where(e => !logMessages.Contains(e)))
            {
                logMessages.Add(m);
                result.Add(MakeLog(m));
            }

            // проверяем инициализацию
            var message = "не инициализирован, остановлен";
            if (newsSettings == null || currencySettings == null)
            {
                if (!logMessages.Contains(message))
                {
                    logMessages.Add(message);
                    result.Add(MakeLog(message));
                }
                return result;
            }

            // all ok inform
            // информируем о начале работы
            message = "запущен";
            if (!logMessages.Contains(message))
            {
                logMessages.Add(message);
                result.Add(MakeLog(message));
            }

            // saving to cache
            // сохраняем кэш котировок
            for (var i = 0; i < names.Length; i++)
            {
                var name = names[i];
                if(!quoteStorage.ContainsKey(name))
                    quoteStorage.Add(name, new List<QuoteData>());
                quoteStorage[name].Add(quotes[i].GetCloseQuote());
            }

            // if this call is used for caching quotes - exiting
            // если мы в режиме наполнения кэша - выходим
            if (isHistoryStartOff)
                return result;

            // detemining model time
            // определение модельного времени
            if (quotes.Length == 0)
                return result;
            var now = quotes[0].timeClose;

            // update orders' SL & TP
            // обновляем сделки
            List<MarketOrder> orders;
            robotContext.GetMarketOrders(robotContext.AccountInfo.ID, out orders);
            result.AddRange(UpdateOrders(now, orders));

            // working with news
            // make GrabNews calls unfrequent
            // сокращаем частоту вызовов GrabNews
            if ((now - lastGrabNewsCall.GetLastHit()).TotalSeconds < PauseForNewsReadingSeconds)
                return result;
            var grab = true; // in present - grab each PauseForNewsReadingSeconds
            if (now.Date < DateTime.Today) // working in the past (testing)
                if (now.Date == lastGrabNewsCall.GetLastHit().Date) // grabbing only once in day
                    grab = false;
            // grabbing
            // извлечение с Alpari
            if (grab)
            {
                List<string> parseErrors;
                var news = GrabNews(new DateTime(now.Year, now.Month, now.Day), out parseErrors);
                foreach (var m in parseErrors.Where(e => !logMessages.Contains(e)))
                {
                    logMessages.Add(m);
                    result.Add(MakeLog(m));
                }
                if (news.Count == 0)
                    logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                                                          LogMsgNoNewsParsed, 1000 * 60 * 5, "Прочитано 0 новостей");
                else
                    logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info,
                                                          LogMsgNewsParsed, 1000 * 60 * 5, "Прочитано {0} новостей",
                                                          news.Count);
                freshNews.AddRange(news);
                lastGrabNewsCall.SetTime(now);
            }
            // processing
            // обработка новостей (внешний цикл)
            foreach (var curNews in freshNews)
            {
                var currentNews = curNews;

                // сверяем время
                if (oldNews.Contains(currentNews)) // news already processed
                    continue;
                // news obsolete
                // новость устарела
                if ((now - currentNews.Time - new TimeSpan(0, NewsProcessingDelayMinutes, 0)).TotalMinutes > NewsObsolescenceTimeMinutes)
                {
                    oldNews.Add(currentNews);
                    continue;
                }
                // news in future; skip processing
                // придержим новость на будущее
                if (currentNews.Time + new TimeSpan(0, NewsProcessingDelayMinutes, 0) > now)
                    continue;

                var timeNear = currentNews.Time.AddMinutes(TimeNearMinutes);
                var timeFar = currentNews.Time.AddMinutes(-TimeFarMinutes);

                result.Add(MakeLog("обрабатывается " + currentNews));
                var chatMessage = "\n"; // здесь формируется сообщение в чат
                chatMessage += string.Format("Обрабатывается новость: [{0}] {1}\nВремя: {2}, прогноз: {3}, фактическое: {4}\n",
                                  currentNews.CountryCode, currentNews.Title, currentNews.Time,
                                  currentNews.ProjectedValue, currentNews.Value);

                // calc weights
                // вычисляем веса, определяем знак
                int valueWeight;
                if(currentNews.ProjectedValue > currentNews.Value)
                    valueWeight = -1;
                else if((currentNews.ProjectedValue < currentNews.Value))
                    valueWeight = 1;
                else
                    valueWeight = 0;
                var delta = currentNews.ProjectedValue == 0
                                ? 100
                                : (int)Math.Abs((currentNews.Value - currentNews.ProjectedValue) / currentNews.ProjectedValue * 100);
                var newsWeight = GetWeight(currentNews.CountryCode, currentNews.Title, delta);
                // 4 debug
                /*if (newsWeight == 0)
                {
                    message = string.Format("Valuable news not processed: [{0}] {1}", currentNews.CountryCode,
                                                currentNews.Title);
                    if (!logMessages.Contains(message))
                    {
                        logMessages.Add(message);
                        result.AddRange(MakeLog(message));
                    }
                }*/
                var sign = valueWeight * newsWeight;
                if(sign == 0)
                {
                    oldNews.Add(currentNews);
                    chatMessage += "Результат: нет входа в рынок\n";
                    chatMessage += "Причина: " +
                                   (newsWeight == 0
                                        ? "вес новости равен 0\n"
                                        : "отклонение экономического показателя равно 0\n");
                    if (SendMessageInRoom != null)
                        SendMessageInRoom(chatMessage, chatRoom);
                    continue;
                }

                // gathering tickers for affected currecncies
                // определяем затронутые новостью валютные пары и их знак новостного сигнала
                var tickersAndSigns = new List<Cortege2<string, int>>();
                var currencies = currencySettings.Where(c => c.CountryCode == currentNews.CountryCode).Select(c => c.CurrencyCode);
                foreach (var currency in currencies)
                {
                    var cur = currency;
                    var tickersWithBaseCur = Graphics.Where(g => g.a.StartsWith(cur)).Select(g => g.a);
                    tickersAndSigns.AddRange(tickersWithBaseCur.Select(t => new Cortege2<string, int>(t, sign)).ToList());
                    var tickersWithQuoteCur = Graphics.Where(g => g.a.EndsWith(cur)).Select(g => g.a);
                    tickersAndSigns.AddRange(
                        tickersWithQuoteCur.Select(t => new Cortege2<string, int>(t, -sign)).ToList());
                }

                // processing tickers
                // работаем с выбранными валютными парами (внутренний цикл)
                foreach (var tickerAndSign in tickersAndSigns)
                {
                    var ticker = tickerAndSign.a;
                    var curSign = tickerAndSign.b;

                    // определение действующей на этот момент котировки
                    var data = GetDataFromStorage(ticker, currentNews.Time, currentNews.Time.AddMinutes(5));
                    if(data.Count == 0)
                    {
                        chatMessage += "Результат: нет входа в рынок с " + ticker + "\n";
                        chatMessage += "Причина: нет котировки для " + ticker + " в диапазоне от " +
                                       currentNews.Time + " до " + currentNews.Time.AddMinutes(5) + "\n";
                        if (SendMessageInRoom != null)
                            SendMessageInRoom(chatMessage, chatRoom);
                        continue;
                    }

                    // опорное значение для определения трендов и величины сделки - цена спроса
                    var value = data[0].bid;

                    // определение тренда
                    int signNear = 0, signFar = 0;
                    data = GetDataFromStorage(ticker, timeNear, timeNear.AddMinutes(5));
                    if (data.Count == 0)
                    {
                        // 4 debug
                        var hint = new RobotHint(ticker, "", "insufficient data for near-trend",
                                                    curSign > 0 ? "BUY no near-data" : "SELL no near-data", "e", value)
                                        {Time = now, ColorFill = Color.Red};
                        result.Add(hint.ToString());
                        chatMessage += "Результат: нет входа в рынок с " + ticker + "\n";
                        chatMessage += "Причина: недостаточно данных для определения тренда после выхода новости\n";
                        if (SendMessageInRoom != null)
                            SendMessageInRoom(chatMessage, chatRoom);
                        continue;
                    }
                    var valueNear = data[0].bid;
                    if (value > valueNear)
                        signNear = 1;
                    else if (value < valueNear)
                        signNear = -1;
                    data = GetDataFromStorage(ticker, timeFar, timeFar.AddMinutes(5));
                    if (data.Count == 0)
                    {
                        // 4 debug
                        var hint = new RobotHint(ticker, "", "insufficient data for far-trend",
                                                    curSign > 0 ? "BUY no far-data" : "SELL no far-data", "e", value)
                                        {Time = now, ColorFill = Color.Red};
                        result.Add(hint.ToString());
                        chatMessage += "Результат: нет входа в рынок с " + ticker + "\n";
                        chatMessage += "Причина: недостаточно данных для определения тренда до выхода новости\n";
                        if (SendMessageInRoom != null)
                            SendMessageInRoom(chatMessage, chatRoom);
                        continue;
                    }
                    var valueFar = data[0].bid;
                    if (value > valueFar)
                        signFar = 1;
                    else if (value < valueFar)
                        signFar = -1;

                    // определяем необходимость входа в рынок
                    var values = new Dictionary<string, double>();
                    values.Add("tn", curSign);
                    values.Add("tba", signFar);
                    values.Add("tcb", signNear);
                    double formulaResult;
                    var resultFlag = expressionResolver.Calculate(values, out formulaResult);
                    if (!resultFlag)
                    {
                        result.Add(MakeLog("Ошибка в расчете по формуле для входа в рынок"));
                        chatMessage += "Результат: нет входа в рынок с " + ticker + "\n";
                        chatMessage += "Причина: ошибка в расчете по формуле для входа в рынок\n";
                        if(SendMessageInRoom != null)
                            SendMessageInRoom(chatMessage, chatRoom);
                        continue;
                    }

                    // вход в рынок
                    var tradeSign = 0;
                    switch (EnterSign)
                    {
                        case EnterSignEnum.Tn:
                            tradeSign = curSign;
                            break;
                        case EnterSignEnum.NotTn:
                            tradeSign = -curSign;
                            break;
                        case EnterSignEnum.Tba:
                            tradeSign = signFar;
                            break;
                        case EnterSignEnum.NotTba:
                            tradeSign = -signFar;
                            break;
                        case EnterSignEnum.Tcb:
                            tradeSign = signNear;
                            break;
                        case EnterSignEnum.NotTcb:
                            tradeSign = -signNear;
                            break;
                    }
                    if (formulaResult != 0)
                    {
                        // если получен сигнал на покупку - купить, закрыв продажи
                        // наоборот, если получен сигнал на продажу - продать, закрыв покупки
                        robotContext.GetMarketOrders(robotContext.AccountInfo.ID, out orders);
                        var ordersToClose = orders.Where(o => o.Symbol == ticker && o.Side != tradeSign).ToList();
                        foreach (var order in ordersToClose)
                        {
                            robotContext.SendCloseRequest(protectedContext.MakeProtectedContext(),
                                robotContext.AccountInfo.ID, order.ID, PositionExitReason.ClosedByRobot);
                        }
                        // создаем ордер
                        var decreaseSl = DalSpot.Instance.GetAbsValue(ticker, (float)InitialSlPoints);
                        var slValue = value - tradeSign * decreaseSl;
                        var decreaseTp = DalSpot.Instance.GetAbsValue(ticker, (float)InitialTpPoints);
                        var tpValue = value + tradeSign * decreaseTp;
                        var newOrder = new MarketOrder
                            {
                                AccountID = robotContext.AccountInfo.ID, 
                                Magic = Magic, 
                                Symbol = ticker, 
                                Volume = Volume,
                                Side = tradeSign,
                                StopLoss = slValue, 
                                TakeProfit = tpValue,
                                ExpertComment = currentNews.Title
                            };

                        var status = robotContext.SendNewOrderRequest(protectedContext.MakeProtectedContext(),
                                                            RequestUniqueId.Next(),
                                                            newOrder,
                                                            OrderType.Market, (decimal)value, 0);
                        if (status != RequestStatus.OK)
                        {
                            var hint = new RobotHint(ticker, "",
                                                     "SendNewOrderRequest error: " +
                                                     EnumFriendlyName<RequestStatus>.GetString(status) + " news: " +
                                                     currentNews,
                                                     tradeSign > 0 ? "BUY error" : "SELL error", "e", value)
                                {
                                    Time = now,
                                    ColorFill = Color.Red
                                };
                            result.Add(hint.ToString());
                        }
                        else
                        {
                            var hint = new RobotHint(ticker, "", "SendNewOrderRequest Ok, news: " + currentNews,
                                                        tradeSign > 0 ? "BUY" : "SELL", "i", value) { Time = now, ColorFill = Color.Red };
                            result.Add(MakeLog("вход в рынок по новости " + currentNews));
                            result.Add(hint.ToString());
                            chatMessage += "Результат: вход в рынок: " + (tradeSign > 0 ? "покупка " : "продажа ") + ticker + "\n";
                        }
                    }
                    else
                    {
                        var hint = new RobotHint(ticker, "", "Market condition fulfill failed, news: " + currentNews,
                                                    tradeSign > 0 ? "BUY no condition" : "SELL no condition", "e", value)
                                        {
                                            Time = now, RobotHintType = RobotHint.HintType.Стоп
                                        };
                        result.Add(hint.ToString());
                        chatMessage += "Результат: нет входа в рынок с " + ticker + "\n";
                        chatMessage += "Причина: не выполнено условие входа\n";
                    }
                    if (SendMessageInRoom != null)
                        SendMessageInRoom(chatMessage, chatRoom);
                }
                oldNews.Add(currentNews);
            }
            return result;
        }

        private List<QuoteData> GetDataFromStorage(string ticker, DateTime start, DateTime end)
        {
            if (!quoteStorage.ContainsKey(ticker))
                return new List<QuoteData>();
            var data = quoteStorage[ticker];
            return data.Where(quoteData => quoteData.time >= start && quoteData.time <= end).ToList();
        }

        private int GetWeight(string countryCode, string title, int delta)
        {
            var foundNews = newsSettings.Where(n => n.CountryCode == countryCode && n.Title == title).ToList();
            if (!foundNews.Any())
            {
                //Console.WriteLine("Unvaluable news: {0}, {1}", countryCode, title);
                return 0;
            }
            var news = foundNews[0];
            if (delta < news.MinDelta)
                return 0;
            return news.Weight;
        }

        private List<string> UpdateOrders(DateTime now, List<MarketOrder> orders)
        {
            var result = new List<string>();
            foreach (var order in orders.Where(o => o.Magic == Magic))
            {
                var sign = order.Side > 0 ? 1 : -1;
                var updateOrder = false;

                // sl
                var slPointsDecrease = InitialSlPoints - (int)((now - order.TimeEnter).TotalMinutes / SlTpPeriodMinutes) * SlTpPointsDecrease;
                var slDecrease = DalSpot.Instance.GetAbsValue(order.Symbol, (float)slPointsDecrease);
                var slValue = order.PriceEnter - sign * slDecrease;
                var slPointsDeviation = GetPointsDeviation(order, slValue, true);
                if (Math.Abs((int)slPointsDeviation) >= 1)
                    updateOrder = true;

                // tp
                var tpPointsDecrease = InitialTpPoints - (int)((now - order.TimeEnter).TotalMinutes / SlTpPeriodMinutes) * SlTpPointsDecrease;
                var tpDecrease = DalSpot.Instance.GetAbsValue(order.Symbol, (float)tpPointsDecrease);
                var tpValue = order.PriceEnter + sign * tpDecrease;
                var tpPointsDeviation = GetPointsDeviation(order, tpValue, false);
                if (Math.Abs((int)tpPointsDeviation) >= 1)
                    updateOrder = true;

                // update
                if (!updateOrder) // sl/tp changes are < 1 points - not updating
                    continue;
                order.StopLoss = slValue;
                order.TakeProfit = tpValue;
                var status = robotContext.SendEditMarketRequest(protectedContext.MakeProtectedContext(), order);
                if (status != RequestStatus.OK)
                {
                    var hint = new RobotHint(order.Symbol, "",
                                                "SendEditMarketRequest error: " +
                                                EnumFriendlyName<RequestStatus>.GetString(status),
                                                order.Side > 0 ? "BUY SL" : "SELL SL", "e", order.StopLoss.Value)
                    {
                        Time = now,
                        RobotHintType = RobotHint.HintType.Поджатие,
                        ColorFill = Color.Red
                    };
                    result.Add(hint.ToString());
                    hint = new RobotHint(order.Symbol, "",
                                         "SendEditMarketRequest error: " +
                                         EnumFriendlyName<RequestStatus>.GetString(status),
                                         order.Side > 0 ? "BUY TP" : "SELL TP", "e", order.TakeProfit.Value)
                    {
                        Time = now,
                        RobotHintType = RobotHint.HintType.Поджатие,
                        ColorFill = Color.Red
                    };
                    result.Add(hint.ToString());
                }
                /*else
                {
                    // 4 debug
                    var hint = new RobotHint(order.Symbol, "", "SendEditMarketRequest Ok",
                                             order.Side > 0 ? "BUY SL" : "SELL SL", "i", order.StopLoss.Value)
                                   {
                                       Time = now,
                                       RobotHintType = RobotHint.HintType.Поджатие,
                                   };
                    result.Add(hint.ToString());
                    hint = new RobotHint(order.Symbol, "", "SendEditMarketRequest Ok",
                                         order.Side > 0 ? "BUY TP" : "SELL TP", "i", order.TakeProfit.Value)
                               {
                                   Time = now,
                                   RobotHintType = RobotHint.HintType.Поджатие,
                               };
                    result.Add(hint.ToString());
                }*/
            }
            return result;
        }

        public event ChatControlBackEnd.EnterRoomDel EnterRoom;
        public event ChatControlBackEnd.SendMessageInRoomDel SendMessageInRoom;
    }
}
// ReSharper restore LocalizableElement

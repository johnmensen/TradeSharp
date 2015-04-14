using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Robot.BL;
using TradeSharp.Util;

namespace TradeSharp.Robot.Robot
{
    // ReSharper disable LocalizableElement
    /// <summary>
    /// отправляет команды:
    /// CLOS_1020_1021 /* закрыть сделки с Magic 1020, 1021 */
    /// BUY_1032_USDCHF_1030 /* купить 1032 USDCHF, присвоить Magic 1030 */
    /// ACTLIST_1011_1021_1043_1055_1072_1073_1094
    /// 
    /// принимает сообщения:
    /// EQT=9983 /* средства в МТ4 = 9983 ед. базовой валюты */
    /// </summary>
    [DisplayName("MT4 транслятор")]
    [Editor(typeof(Mt4RobotEditorForm), typeof(Form))]
    public class Mt4Robot : BaseRobot
    {
        #region Класс спам-сообщений робота
        class SpamMessage
        {
            public enum MessageCategory
            {
                InitialMessage = 0, ErrorMessage, MessageFromMt4, MessageToMt4, OpenOrder, CloseOrder, AccountMismatch
            }

            private readonly int minSecondsBetweenSameMessages;

            private readonly ThreadSafeList<Cortege2<MessageCategory, string>> messages = new ThreadSafeList<Cortege2<MessageCategory, string>>();

            private readonly ThreadSafeStorage<MessageCategory, DateTime?> messagePublished =
                new ThreadSafeStorage<MessageCategory, DateTime?>();

            public SpamMessage(int minSecondsBetweenSameMessages)
            {
                this.minSecondsBetweenSameMessages = minSecondsBetweenSameMessages;
            }

            public void PutMessage(MessageCategory cat, string msg)
            {
                var upTime = messagePublished.ReceiveValue(cat);
                if (upTime.HasValue)
                {
                    var deltaSec = (DateTime.Now - upTime.Value).TotalSeconds;
                    if (deltaSec < minSecondsBetweenSameMessages) return;
                }

                messages.Add(new Cortege2<MessageCategory, string>(cat, msg), 1000);
                messagePublished.UpdateValues(cat, DateTime.Now);
            }

            public List<Cortege2<MessageCategory, string>> GetMessages()
            {
                return messages.ExtractAll(1000);
            }
        }
        #endregion

        #region Неактуальные настройки
        [Browsable(false)]
        public override string NewsChannels { get; set; }
        #endregion

        #region Настройки

        // ReSharper disable InconsistentNaming
        public enum RobotMode { УправлятьМТ4 = 0, КопироватьОрдераМТ4 }
        // ReSharper restore InconsistentNaming

        [Category("Основные")]
        [PropertyXMLTag("Robot.TradeSharpAccount")]
        [DisplayName("Счет T#")]
        [Description("Торговля ведется только для указанного счета, если указан (не 0)")]
        public int TradeSharpAccount { get; set; }

        [Category("MT4")]
        [PropertyXMLTag("Robot.Mode")]
        [DisplayName("Режим")]
        [Description("Транслировать ордера в МТ4 либо дублировать ордера из МТ4")]
        public RobotMode Mode { get; set; }

        [Category("MT4")]
        [PropertyXMLTag("Robot.ControlOrdersDisregardMagic")]
        [DisplayName("Игнорировать Magic")]
        [Description("Сопровождать ордера, вне зависимости от Magic")]
        public bool ControlOrdersDisregardMagic { get; set; }

        private decimal mt4ToTradeSharpRate = 1M;
        [Category("MT4")]
        [PropertyXMLTag("Robot.Mt4ToTradeSharpRate")]
        [DisplayName("Курс МТ4/T#")]
        [Description("Курс валюты депо МТ4 к валюте депо TradeSharp")]
        public decimal Mt4ToTradeSharpRate
        {
            get { return mt4ToTradeSharpRate; }
            set { mt4ToTradeSharpRate = value; }
        }
        
        public int portOwn = 8010;
        [Category("MT4")]
        [PropertyXMLTag("Robot.PortOwn")]
        [DisplayName("Порт (свой)")]
        [Description("Порт, который слушает робот, на предмет сообщений от МТ4")]
        public int PortOwn
        {
            get { return portOwn; }
            set { portOwn = value; }
        }

        private string hostMt4 = "127.0.0.1";
        [Category("MT4")]
        [PropertyXMLTag("Robot.Host4")]
        [DisplayName("Адрес комп. MT4")]
        [Description("Адрес компьютера-терминала МТ4 (127.0.0.1 - локальный адрес)")]
        public string HostMt4
        {
            get { return hostMt4; }
            set { hostMt4 = value; }
        }

        public int portMt4 = 8011;
        [Category("MT4")]
        [PropertyXMLTag("Robot.PortMt4")]
        [DisplayName("Порт (MT4)")]
        [Description("Порт, который слушает советник МТ4, на предмет сообщений от робота")]
        public int PortMt4
        {
            get { return portMt4; }
            set { portMt4 = value; }
        }

        //[Category("MT4")]
        //[PropertyXMLTag("Robot.AccountMt4")]
        //[DisplayName("Счет в MT4")]
        //[Description("Номер счета в MT4 (для контроля исполнения)")]
        //public int AccountMt4 { get; set; }

        public int percentScale = 100;
        [Category("Основные")]
        [PropertyXMLTag("Robot.PercentScale")]
        [DisplayName("Объем сделки, %")]
        [Description("Объем сделки в МТ4 по отношению к объему сделки TradeSharp")]
        public int PercentScale
        {
            get { return percentScale; }
            set { percentScale = value; }
        }

        [Category("MT4")]
        [DisplayName("Связь с MT4")]
        [RuntimeAccess(true)]
        public bool Mt4Connected
        {
            get { return mt4Connected; }
        }

        [Category("MT4")]
        [DisplayName("Суффикс тикер. MT4")]
        [Description("Добавка к названию тикера в MT4")]
        [PropertyXMLTag("Mt4TickerSuffix")]
        public string Mt4TickerSuffix { get; set; }
        #endregion


        #region Члены

        private bool initMessageWasSent;

        private UdpClient client;

        private string errorMessage;

        private readonly Encoding encoding = Encoding.Unicode;

        private readonly ThreadSafeTimeStamp timeSinceUpdate = new ThreadSafeTimeStamp();

        private const int MilsBetweenChecks = 1000 * 1;

        /// <summary>
        /// текущие средства в терминале MT4
        /// </summary>
        private decimal? mt4Equity;

        /// <summary>
        /// текущее состояние открытых ордеров
        /// </summary>
        private List<MarketOrder> currentOrders = new List<MarketOrder>();

        /// <summary>
        /// флаг наличия соедиенения с МТ4
        /// взводится по сообщению от МТ4
        /// снимается по таймауту на неполученное сообщение
        /// </summary>
        private volatile bool mt4Connected;

        private bool ordersAreRead;

        private Thread commandSendThread;

        private const int SendThreadInterval = 100;

        private readonly ThreadSafeList<string> outgoingCommandsList = new ThreadSafeList<string>();

        private const int CommandListTimeout = 1000;

        private volatile bool commandSendThreadIsStopping;

        private readonly ThreadSafeTimeStamp timeSinceActList = new ThreadSafeTimeStamp();

        private const int MilsBetweenSendActualOrdersList = 1000 * 5;

        private readonly ThreadSafeList<MarketOrder> ordersFromMt4 = new ThreadSafeList<MarketOrder>();

        private volatile bool ordersFromMt4Received;

        private readonly ThreadSafeList<int> sentRequests = new ThreadSafeList<int>();

        private readonly SpamMessage spamMessage = new SpamMessage(60*60);
        #endregion

        public override BaseRobot MakeCopy()
        {
            var bot = new Mt4Robot
            {
                HostMt4 = HostMt4,
                //AccountMt4 = AccountMt4,
                PortMt4 = PortMt4,
                PortOwn = PortOwn,
                PercentScale = PercentScale,
                Mt4ToTradeSharpRate = Mt4ToTradeSharpRate,
                Mode = Mode,
                ControlOrdersDisregardMagic = ControlOrdersDisregardMagic,
                FixedVolume = FixedVolume,
                Mt4TickerSuffix = Mt4TickerSuffix,
                TradeSharpAccount = TradeSharpAccount
            };
            CopyBaseSettings(bot);
            return bot;
        }

        public override Dictionary<string, DateTime> GetRequiredSymbolStartupQuotes(DateTime startTrade)
        {
            return new Dictionary<string, DateTime> { { "EURUSD", startTrade } };
        }

        public override void Initialize(BacktestServerProxy.RobotContext grobotContext, CurrentProtectedContext protectedContext)
        {
            base.Initialize(grobotContext, protectedContext);

            initMessageWasSent = false;
            timeSinceUpdate.Touch();
            
            ordersFromMt4.ExtractAll(1000*10);
            ordersFromMt4Received = false;
            sentRequests.ExtractAll(1000*10);
            
            // стартовать прослушку порта
            try
            {
                var e = new IPEndPoint(IPAddress.Parse(hostMt4), portOwn);
                client = new UdpClient(e);
                client.BeginReceive(ReceiveCallback, null);
            }
            catch (ArgumentOutOfRangeException)
            {
                errorMessage = string.Format("MT4 транслятор: порт ({0}) задан неверно. Допустимый диапазон: {1} - {2}",
                                             portOwn, IPEndPoint.MinPort, IPEndPoint.MaxPort);
                client = null;
                return;
            }
            catch (Exception ex)
            {
                client = null;
                errorMessage = string.Format("MT4 транслятор: ошибка прослушивания порта ({0}): {1}",
                                             portOwn, ex.Message);
            }

            // стратовать поток отправки команд
            commandSendThread = new Thread(CommandSendRoutine);
            commandSendThread.Start();
        }

        public override void DeInitialize()
        {
            base.DeInitialize();
            
            // закрыть порт
            if (client == null) return;
            try
            {
                client.Close();
                client = null;
            }
            catch (Exception ex)
            {
                Logger.Error("Mt4Robot - DeInitialize", ex);
            }

            // остановить поток отправки команд
            commandSendThreadIsStopping = true;
            commandSendThread.Join();
        }

        public override List<string> OnQuotesReceived(string[] names, CandleDataBidAsk[] quotes, bool isHistoryStartOff)
        {
            if (isHistoryStartOff) return new List<string>();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                var evts = new List<string> { errorMessage };
                errorMessage = string.Empty;
                return evts;
            }

            var evsList = spamMessage.GetMessages().Select(m => m.b).ToList();
            if (TradeSharpAccount > 0)
            {
                var account = robotContext.AccountInfo;
                if (account != null && account.ID != TradeSharpAccount)
                {
                    spamMessage.PutMessage(SpamMessage.MessageCategory.AccountMismatch,
                        "Открыт счет #" + account.ID + ", ожидается счет #" + TradeSharpAccount);
                    return evsList;
                }                
            }
            
            if (!initMessageWasSent)
            {
                // сообщить - привет, я торговый робот, делаю то-то и то-то
                initMessageWasSent = true;
                evsList.Add(string.Format("MT4 робот, слушает порт {0}, отправляет на порт {1}",
                    portOwn, portMt4));
            }

            // проверить время с последней проверки ордеров
            var lastTime = timeSinceUpdate.GetLastHit();
            var nowTime = DateTime.Now;
            var milsPassed = (nowTime - lastTime).TotalMilliseconds;
            if (milsPassed < MilsBetweenChecks) return evsList;
            timeSinceUpdate.Touch();

            // транслировать сделки в МТ4
            if (Mode == RobotMode.УправлятьМТ4)
            {
                List<MarketOrder> orders;
                // проверить имеющиеся ордера
                GetMarketOrders(out orders, false);
                TranslateOrdersToMt4(orders);
                return evsList;
            }

            // копировать сделки из МТ4
            if (Mode == RobotMode.КопироватьОрдераМТ4)
            {
                CopyOrdersFromMt4(evsList);
                return evsList;
            }

            return evsList;
        }

        private void CopyOrdersFromMt4(List<string> events)
        {
            if (!ordersFromMt4Received) return;

            try
            {
                // ордера из МТ4
                var ordersMt4 = ordersFromMt4.ExtractAll(1000);
                
                // ордера из самого терминала
                List<MarketOrder> ordersTs;
                GetMarketOrders(out ordersTs, !ControlOrdersDisregardMagic);

                // для каждого ордера терминала найти аналогичный ордер в списке от МТ4
                // "лишние" ордера удалить
                foreach (var orderTs in ordersTs)
                {
                    var side = orderTs.Side;
                    var symbol = orderTs.Symbol;

                    var orderTicket = 0;
                    if (orderTs.ExpertComment.StartsWith("Mt4ticket="))
                        orderTicket = orderTs.ExpertComment.Substring("Mt4ticket=".Length).ToIntSafe() ?? 0;
                    var existOrderMt4 = ordersMt4.FirstOrDefault(o => o.Magic == orderTicket && 
                        o.Side == side && o.Symbol == symbol);
                    if (existOrderMt4 == null)
                    {
                        // закрыть ордер
                        events.Add(string.Format("Ордер {0} не найден в МТ4 (всего {1} ордеров) и будет закрыт",
                            orderTs, ordersMt4.Count));
                        robotContext.SendCloseRequest(protectedContext.MakeProtectedContext(),
                                    robotContext.AccountInfo.ID, orderTs.ID, PositionExitReason.ClosedByRobot);
                        continue;
                    }

                    // проверить, совпадают ли цены SL / TP для ордера в МТ4 и T#
                    var deltaStop = DalSpot.Instance.GetPointsValue(symbol, Math.Abs((orderTs.StopLoss ?? 0) -
                                                                                     (existOrderMt4.StopLoss ?? 0)));
                    var deltaTake = DalSpot.Instance.GetPointsValue(symbol, Math.Abs((orderTs.TakeProfit ?? 0) -
                                                                                     (existOrderMt4.TakeProfit ?? 0)));
                    // обновить цены TP / SL
                    if (deltaStop > 1 || deltaTake > 1)
                    {
                        orderTs.StopLoss = existOrderMt4.StopLoss;
                        orderTs.TakeProfit = existOrderMt4.TakeProfit;
                        robotContext.SendEditMarketRequest(protectedContext.MakeProtectedContext(), orderTs);
                    }
                }

                // открыть новые ордера
                var requestsWhatHaveBeenAlreadySent = sentRequests.ExtractAll(1000);
                foreach (var orderMt4 in ordersMt4)
                {
                    var orderTicket = orderMt4.Magic.Value;
                    var side = orderMt4.Side;
                    var symbol = orderMt4.Symbol;
                    var comment = "Mt4ticket=" + orderTicket;

                    if (ordersTs.Any(o => o.ExpertComment == comment &&
                                          o.Side == side && o.Symbol == symbol)) continue;

                    orderMt4.ExpertComment = comment;
                    // пересчитать объем
                    orderMt4.Volume = CalcTradeSharpDealVolume(orderMt4.Volume);
                    if (orderMt4.Volume == 0) continue;

                    orderMt4.AccountID = robotContext.AccountInfo.ID;
                    orderMt4.Magic = Magic;

                    // антифлад
                    if (requestsWhatHaveBeenAlreadySent.Contains(orderTicket)) continue;
                    // таки открыть ордер
                    var response = robotContext.SendNewOrderRequest(protectedContext.MakeProtectedContext(),
                                                     RequestUniqueId.Next(), orderMt4, OrderType.Market,
                                                     (decimal) orderMt4.PriceEnter, 0);
                    requestsWhatHaveBeenAlreadySent.Add(orderTicket);
                }
                sentRequests.ReplaceRange(requestsWhatHaveBeenAlreadySent, 1000);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в CopyOrdersFromMt4()", ex);
            }
            finally
            {
                ordersFromMt4Received = false;
            }
        }

        private void TranslateOrdersToMt4(List<MarketOrder> orders)
        {
            orders = orders.Where(o => o.State == PositionState.Opened).ToList();
            if ((DateTime.Now - timeSinceActList.GetLastHit()).TotalMilliseconds > MilsBetweenSendActualOrdersList)
            {
                SendActualOrdersCommand(orders);
                timeSinceActList.Touch();
            }
            
            if (!ordersAreRead)
            {
                ordersAreRead = true;
                currentOrders = orders;
                return;
            }
            
            var ordersNew = (from order in orders let orderId = order.ID where 
                     !currentOrders.Any(o => o.ID == orderId) select order).ToList();
            var ordersClosed = (from order in currentOrders
                             let orderId = order.ID
                             where
                                 !orders.Any(o => o.ID == orderId)
                             select order).ToList();
            currentOrders = orders;

            // сформировать и отправить команды на закрытие ордеров
            if (ordersClosed.Count > 0)
                SendCloseCommands(ordersClosed);
            if (ordersNew.Count > 0)
                SendOpenCommands(ordersNew);
        }

        private void SendActualOrdersCommand(List<MarketOrder> orders)
        {
            // команда - перечисление открытых еще ордеров
            var cmd = "ACTLIST_" + string.Join("_", orders.Select(o => o.ID));
            outgoingCommandsList.Add(cmd, CommandListTimeout);
        }

        private void SendCloseCommands(List<MarketOrder> ordersClosed)
        {
            // сформировать одну команду на закрытие сделок
            var cmd = "CLOS_" + string.Join("_", ordersClosed.Select(o => o.ID));
            outgoingCommandsList.Add(cmd, CommandListTimeout);

            spamMessage.PutMessage(SpamMessage.MessageCategory.CloseOrder,
                ordersClosed.Count > 1
                ? "Закрытие " + ordersClosed.Count + " ордеров"
                : "Закрытие ордера");
        }

        private void SendOpenCommands(List<MarketOrder> ordersNew)
        {
            if (!mt4Equity.HasValue || mt4Equity == 0)
            {
                errorMessage = string.Format("Открытие {0} сделок(сделки) отменяется: нет данных о средствах в MT4",
                                             ordersNew.Count);
                return;
            }

            var ownEquity = robotContext.AccountInfo.Equity;
            if (ownEquity == 0)
            {
                errorMessage = string.Format("Открытие {0} сделок(сделки) отменяется: нет средств",
                                             ordersNew.Count);
                return;
            }

            var scale = mt4Equity.Value * PercentScale / 100M / (decimal)ownEquity;

            foreach (var order in ordersNew)
            {
                var volumeMt4 = (int)(order.Volume * scale + 0.5M);
                var ticker = order.Symbol;
                if (!string.IsNullOrEmpty(Mt4TickerSuffix))
                    ticker = ticker + Mt4TickerSuffix;
                var cmd = string.Format("{0}_{1}_{2}_{3}",
                                        order.Side > 0 ? "BUY" : "SELL",
                                        volumeMt4, ticker, order.ID);
                outgoingCommandsList.Add(cmd, CommandListTimeout);
            }

            spamMessage.PutMessage(SpamMessage.MessageCategory.OpenOrder, 
                ordersNew.Count > 1 
                ? "Открытие " + ordersNew.Count + " ордеров" 
                : "Открытие ордера");
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                var e = new IPEndPoint(IPAddress.Parse(hostMt4), portOwn);
                if (client == null)
                    return;
                var receiveBytes = client.EndReceive(ar, ref e);
                if (receiveBytes == null || receiveBytes.Length == 0)
                    return;
                var str = encoding.GetString(receiveBytes);
                if (string.IsNullOrEmpty(str)) return;

                // добавить в СПАМ-лист
                spamMessage.PutMessage(SpamMessage.MessageCategory.MessageFromMt4, "от MT4: " + str);
                ProcessMt4Message(str);
                
                client.BeginReceive(ReceiveCallback, null);
            }
            catch (Exception ex)
            {
                Logger.Error("Mt4Robot - ReceiveCallback", ex);
                //return;
            }
        }

        private void ProcessMt4Message(string ms)
        {
            mt4Connected = true;
            // информация о балансе...
            // EQT=499990
            // возможно также - и о сделках
            // EQT=50007;0,EURUSD,10000,-1,1.34000,0.00000,1.33749
            if (ms.StartsWith("EQT=") && ms.Length > "EQT=".Length)
            {
                ms = ms.Substring("EQT=".Length);
                var parts = ms.Split(new [] {';'}, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                {
                    errorMessage = "Не распознано: " + ms;
                    return;
                }
                var newEq = parts[0].ToDecimalUniformSafe();
                if (!newEq.HasValue)
                {
                    errorMessage = "Не распознан параметр (средства): " + ms;
                    return;
                }

                mt4Equity = newEq.Value * mt4ToTradeSharpRate;
                ordersFromMt4Received = true;
                
                // прочитать из строки ордера MT4
                var dealsMt4 = new List<MarketOrder>();
                for (var i = 1; i < parts.Length; i++)
                {
                    var dealParts = parts[i].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (dealParts.Length < 7) continue;

                    var magic = dealParts[0].ToIntSafe() ?? 0;
                    var ticker = dealParts[1];
                    if (!string.IsNullOrEmpty(Mt4TickerSuffix))
                        ticker.Replace(Mt4TickerSuffix, "");

                    if (!DalSpot.Instance.GetTickerNames().Contains(ticker))
                    {
                        errorMessage = "Не найден торгуемый актив: " + ms;
                        continue;
                    }
                    var volume = dealParts[2].ToIntSafe() ?? 0;
                    if (volume == 0)
                    {
                        errorMessage = "Объем сделки не прочитан: " + ms;
                        continue;
                    }
                    var side = dealParts[3].ToIntSafe() ?? 0;
                    if (side == 0)
                    {
                        errorMessage = "Тип сделки не прочитан: " + ms;
                        continue;
                    }
                    var stop = dealParts[4].ToFloatUniformSafe();
                    var take = dealParts[5].ToFloatUniformSafe();
                    var priceEnter = dealParts[6].ToFloatUniformSafe() ?? 0;
                    var order = new MarketOrder
                        {
                            Magic = magic,
                            PriceEnter = priceEnter,
                            Volume = volume,
                            Symbol = ticker,
                            StopLoss = stop,
                            TakeProfit = take,
                            Side = side
                        };
                    dealsMt4.Add(order);
                }

                // обработать список сделок, полученных от МТ4
                ordersFromMt4.ReplaceRange(dealsMt4, 1000);
                return;
            }

            // если формат не распознан
            errorMessage = "Не распознано: " + ms;
        }

        private int CalcTradeSharpDealVolume(int srcVolume)
        {
            if (FixedVolume.HasValue && FixedVolume.Value > 0) return FixedVolume.Value;
            
            // посчитать объем из плеча
            var eqMt4 = mt4Equity ?? 0;
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (eqMt4 == 0) return 0;

            var ownEq = robotContext.AccountInfo.Equity;
            if (ownEq == 0) return 0;
            // ReSharper restore CompareOfFloatsByEqualityOperator

            var koeff = PercentScale / 100M * ownEq / eqMt4;
            srcVolume = (int) Math.Round(srcVolume*koeff);

            // выровнять объем
            return MarketOrder.RoundDealVolume(srcVolume, VolumeRoundType.Ближайшее, 10000, 10000);
        }
    
        private void CommandSendRoutine()
        {
            while (!commandSendThreadIsStopping)
            {
                Thread.Sleep(SendThreadInterval);

                bool timeout;
                var command = outgoingCommandsList.ExtractFirst(CommandListTimeout, out timeout);
                if (timeout) Logger.Error("Mt4Robot::list read timeout");
                if (string.IsNullOrEmpty(command)) continue;

                spamMessage.PutMessage(SpamMessage.MessageCategory.MessageToMt4, ">MT4: " + command);

                // отправить полученные команды на указанный адрес
                try
                {
                    var data = encoding.GetBytes(command);
                    //Array.Resize(ref data, data.Length + 1);
                    new UdpClient().Send(data, data.Length, HostMt4, PortMt4);
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка отправки сообщения в МТ4", ex);
                }
            }
        }
    }
    // ReSharper restore LocalizableElement
}

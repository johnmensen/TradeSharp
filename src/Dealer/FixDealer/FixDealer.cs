using System;
using System.Collections.Generic;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.DealerInterface;
using TradeSharp.Util;
using MarketOrder = TradeSharp.Contract.Entity.MarketOrder;

namespace FixDealer
{
    public class FixDealer : SimpleDealer, IFixDealer, IDealer
    {
        private RequestWatchdog requestWatchdog;
        
        private ErrorStorage errorStorage;
        
        public List<string> GroupCodes { get; private set; }
        
        public string DealerCode { get; private set; }

        private readonly ThreadSafeStorage<int, PositionExitReason> exitReasonByOrderId = new ThreadSafeStorage<int, PositionExitReason>();

        private static uint RequestUniqueId
        {
            get 
            { 
                var date = DateTime.Now;
                return (uint)(date.Millisecond + date.Second * 1000 + date.Minute * 60 * 35 + date.Hour * 60 * 60 * 24);
            }
        }

        public FixDealer(DealerDescription desc, List<string> groupCodes)
        {
            GroupCodes = groupCodes;
            DealerCode = desc.Code;
        }

        public void Initialize()
        {
            errorStorage = new ErrorStorage();
            requestWatchdog = new RequestWatchdog(errorStorage);
            requestWatchdog.Start();
        }

        public void Deinitialize()
        {
            requestWatchdog.Stop();
        }

        public string GetErrorString()
        {
            return errorStorage.GetMessagesString();
        }

        public void ClearError()
        {
            errorStorage.ClearMessages();
        }

        public RequestStatus SendNewOrderRequest(Account account,
            MarketOrder order,
            OrderType orderType,
            decimal requestedPrice,
            decimal slippagePoints)
        {
            TradeSharp.ProviderProxyContract.Entity.MarketOrder request;
            RequestStatus error;
            if (!MakeMarketRequest(account, order.Symbol, order.Magic, order.Volume, order.Side,
                orderType,
                requestedPrice, slippagePoints,
                string.Format("open for acc#{0}", account.ID),
                null, order.Comment, order.ExpertComment, order.trailingLevels[0], order.trailingTargets[0],
                out error, out request)) return error;

            // отправить запрос на вход в рынок в соотв сессию
            if (!request.SendToQueue(false))
            {
                errorStorage.AddMessage(new ErrorMessage(DateTime.Now, ErrorMessageType.ОшибкаОтправки,
                    "Невозможно отправить сообщение (NewOrder) в очередь MQ", null));
                return RequestStatus.DealerError;
            }
            return RequestStatus.OK;
        }

        public RequestStatus SendCloseRequest(MarketOrder order, PositionExitReason reason)
        {
            // получить счет
            RequestStatus error;
            var acDecorated = ServerInterface.GetAccount(order.AccountID);
            if (acDecorated == null) return RequestStatus.ServerError;
            
            // подготовить запрос
            TradeSharp.ProviderProxyContract.Entity.MarketOrder request;
            if (!MakeMarketRequest(acDecorated, order.Symbol, order.Magic, order.Volume, -order.Side, 
                OrderType.Market,
                0, 0,
                string.Format("close order #{0}", order.ID),
                order.ID, order.Comment, order.ExpertComment, order.TrailLevel1, order.TrailTarget1,
                out error, out request)) return error;
            exitReasonByOrderId.UpdateValues(order.ID, reason);

            // отправить запрос на вход в рынок (оно же - закрытие позиции) в соотв сессию
            if (!request.SendToQueue(false))
            {
                errorStorage.AddMessage(new ErrorMessage(DateTime.Now, ErrorMessageType.ОшибкаОтправки,
                    "Невозможно отправить сообщение (CloseOrder) в очередь MQ", null));
                return RequestStatus.DealerError;
            }
            return RequestStatus.OK;
        }

        /// <summary>
        /// преобразовать запрос на вход / выход из рынка в отложенный ордер
        /// </summary>        
        private bool MakeMarketRequest(Account account, string symbol, int? magic,
            int volume, int side,
            OrderType orderPricing,
            decimal requestedPrice, decimal slippagePoints,  
            string description,
            int? closingPositionId, string comment, string expertComment, float? trailingLevel, float? trailingGoal,
            out RequestStatus error,
            out TradeSharp.ProviderProxyContract.Entity.MarketOrder req)
        {
            Logger.InfoFormat("Dealer [{0}, account: {1}]: MakeMarketRequest({2} {3})", 
                DealerCode, account.ID, side > 0 ? "BUY" : "SELL", symbol);
            error = RequestStatus.OK;
            req = new TradeSharp.ProviderProxyContract.Entity.MarketOrder(new BrokerOrder());            

            if (orderPricing == OrderType.Instant && slippagePoints != 0)
            {
                var slipAbs = DalSpot.Instance.GetAbsValue(symbol, slippagePoints);
                if (slipAbs == 0)
                {
                    Logger.ErrorFormat("Ошибка в FixDealer - нет информации по символу {0} (пересчет проскальзывания)", symbol);
                    errorStorage.AddMessage(new ErrorMessage(DateTime.Now, ErrorMessageType.ОшибкаОтправки,
                        string.Format("Ошибка в FixDealer - нет информации по символу {0} (пересчет проскальзывания)", symbol), 
                        null));
                    error = RequestStatus.ServerError;
                    return false;
                }
                req.brokerOrder.Slippage = slipAbs;
            }
            int posId = 0;
            if (closingPositionId == null || closingPositionId == 0)
            {
                var order = new MarketOrder
                                {
                                    AccountID = account.ID,
                                    Side = side,
                                    ID = 0,
                                    Volume = volume,
                                    Symbol = symbol,
                                    Comment = comment,
                                    ExpertComment = expertComment,
                                    Magic = magic,
                                    TimeEnter = DateTime.Now,
                                    TrailLevel1 = trailingLevel,
                                    TrailTarget1 = trailingGoal,
                                    PriceEnter = (float) requestedPrice,
                                    State = PositionState.StartOpened,
                                };

                // создание позы - обработано
                ServerInterface.SaveOrderAndNotifyClient(order, out posId);
                if (posId <= 0)
                {
                    Logger.ErrorFormat("Ошибка в FixDealer - ошибка создания открытой позиции в БД по {0}", symbol);
                    errorStorage.AddMessage(new ErrorMessage(DateTime.Now, ErrorMessageType.ОшибкаОтправки,
                                                             string.Format(
                                                                 "Ошибка в FixDealer - ошибка создания открытой позиции в БД по {0}",
                                                                 symbol),
                                                             null));
                    error = RequestStatus.ServerError;
                    return false;
                }
            }
            else
            {// закрытие позы
                MarketOrder order;
                ServerInterface.GetMarketOrder(closingPositionId.Value, out order);
                if (order == null)
                {
                    error = RequestStatus.ServerError;
                    Logger.ErrorFormat("FixDealer - MakeMarketRequest - order {0} not found", closingPositionId.Value);
                    return false;
                }
                order.State = PositionState.StartClosed;                
                if (!ServerInterface.ModifyMarketOrder(order))
                {
                    error = RequestStatus.ServerError;
                    Logger.ErrorFormat("FixDealer - MakeMarketRequest - cannot modify order {0}", closingPositionId.Value);
                    return false;
                }
            }

            req.AccountGroupCode = account.Group;
            req.brokerOrder.Instrument = Instrument.Spot;
            req.brokerOrder.OrderPricing = orderPricing == OrderType.Instant ? OrderPricing.Instant : OrderPricing.Market;
            if (req.brokerOrder.OrderPricing == OrderPricing.Instant)
                req.brokerOrder.RequestedPrice = requestedPrice == 0 ? (decimal?)null : requestedPrice;
            req.brokerOrder.Side = side;
            req.brokerOrder.Volume = volume;
            req.brokerOrder.Ticker = symbol;
            req.brokerOrder.RequestId = posId != 0 ? posId : (int)RequestUniqueId;
            req.brokerOrder.DealerCode = DealerCode;
            req.brokerOrder.TimeCreated = DateTime.Now;
            req.brokerOrder.ClosingPositionID = closingPositionId;
            req.brokerOrder.AccountID = account.ID;
            req.brokerOrder.Magic = magic;
            req.brokerOrder.Comment = comment;
            req.brokerOrder.ExpertComment = expertComment;
            
            requestWatchdog.AddRequest(new RequestWatchdogItem 
                { requestId = req.brokerOrder.RequestId, requestTime = DateTime.Now, 
                    requestSymbol = symbol, requestType = (DealType)side });
            
            // сохранить сообщение в БД
            try
            {
                ServerInterface.SaveProviderMessage(req.brokerOrder);                
            }
            catch (Exception ex)
            {
                error = RequestStatus.SerializationError;
                Logger.ErrorFormat("Дилер {0}: ошибка сохранения запроса брокеру: {1}",
                    DealerCode, ex);
                errorStorage.AddMessage(new ErrorMessage(DateTime.Now, ErrorMessageType.ОшибкаОтправки,
                        string.Format("Дилер {0}: ошибка сохранения запроса брокеру", DealerCode), ex));
                return false;
            }            
            
            return true;
        }
    
        /// <summary>
        /// отдельный класс читает очередь сообщений от провайдеров
        /// каждый FIX-дилер получает сообщения своей группы        
        /// </summary>        
        public void ProcessExecutionReport(BrokerResponse response, BrokerOrder request)
        {
            Logger.InfoFormat("Запрос ({0}), ответ ({1})", request, response);
            requestWatchdog.OnRequestProcessed(response.RequestId);
            // сообщить клиенту об отказе обработать ордер
            // вынимаем ордер из базы
            MarketOrder order;
            int orderId;
            if (request.ClosingPositionID != null && request.ClosingPositionID > 0)
                orderId = (int)request.ClosingPositionID;
            else
            {
                orderId = request.RequestId;
                Logger.InfoFormat("Запрос GetMarketOrder orderId={0} requestId={1}", orderId, request.RequestId);
            }
            Logger.InfoFormat("Запрос GetMarketOrder accountId={0}, orderId={1}", request.AccountID, orderId);
            var res = ServerInterface.GetMarketOrder(orderId, out order);
            if (res == false)
            {
                errorStorage.AddMessage(new ErrorMessage(DateTime.Now, ErrorMessageType.ОтказСервера,
                        string.Format("Дилер {0}: не найдена позиция ID={0} для обработка запроса [{1}]-[счет {2}, пара {3}]",
                        request.RequestId, request.Id, request.AccountID, request.Instrument), null));
                ServerInterface.NotifyClientOnOrderRejected(order, "не найден ордер");
                return;
            }
            
            if (response.Status == OrderStatus.Отклонен)
            {
                var rejectReasonStr = string.IsNullOrEmpty(response.RejectReasonString)
                                          ? (response.RejectReason ?? OrderRejectReason.None).ToString()
                                          : response.RejectReasonString;
                ServerInterface.NotifyClientOnOrderRejected(order, rejectReasonStr);
                errorStorage.AddMessage(new ErrorMessage(DateTime.Now, ErrorMessageType.ОтказСервера,
                        string.Format("Дилер {0}: отказ сервера [{1}] на запрос [{2}]-[счет {3}, пара {4}]", 
                        DealerCode, rejectReasonStr, request.Id, request.AccountID, request.Instrument), null));
                return;
            }
            if (!response.Price.HasValue)
            {
                errorStorage.AddMessage(new ErrorMessage(DateTime.Now, ErrorMessageType.ОтказСервера,
                        string.Format("Дилер {0}: отказ сервера [нет цены] на запрос [{1}]-[счет {2}, пара {3}]",
                        DealerCode, request.Id, request.AccountID, request.Instrument), null));
                ServerInterface.NotifyClientOnOrderRejected(order, "Нет цены");
                return;
            }
            var deltaMarkup = request.MarkupAbs * order.Side;
            
            // закрытие позиции - ордер обработан
            if (request.ClosingPositionID.HasValue)
            {
                var reason = exitReasonByOrderId.ReceiveValue(order.ID);
                // закрыть ордер немедленно
                ServerInterface.CloseOrder(order.ID, response.Price.Value - (decimal)deltaMarkup, reason);
                return;
            }

            // открытие позы - обработано
            order.TimeEnter = response.ValueDate;
            order.PriceEnter = (float)response.Price.Value + deltaMarkup;
            order.State = PositionState.Opened;
            ServerInterface.ModifyMarketOrder(order);
        }
    }
}

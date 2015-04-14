using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Linq;
using System.ServiceModel;
using Entity;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.DealerInterface;
using TradeSharp.Linq;
using TradeSharp.Server.BL;
using TradeSharp.Server.Repository;
using TradeSharp.TradeLib;
using TradeSharp.Util;
using TradeSharp.Util.Serialization;

namespace TradeSharp.Server.Contract
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public partial class ManagerTrade : ITradeSharpServerTrade, IOrderManager
    {
        public IAccountRepository accountRepository;

        public IOrderRepository orderRepository;

        public IProfitCalculator profitCalculator;
        
        #region Singletone

        private static Lazy<ManagerTrade> instance = new Lazy<ManagerTrade>(() => new ManagerTrade(true));

        public static ManagerTrade Instance
        {
            get { return instance.Value; }
        }

        public static void MakeTestInstance()
        {
            instance = new Lazy<ManagerTrade>(() => new ManagerTrade(false));
        }

        public static void MakeTestInstance(Dictionary<string, IDealer> dealers)
        {
            instance = new Lazy<ManagerTrade>(() => new ManagerTrade(dealers));
        }

        #endregion        

        public TradeManager tradeManager;

        private IWalletRepository walletRepository;

        private IBrokerRepository brokerRepository;

        private ManagerTrade(bool initialize)
        {
            accountRepository = AccountRepository.Instance;
            orderRepository = OrderRepository.Instance;
            profitCalculator = ProfitCalculator.Instance;
            brokerRepository = new BrokerRepository();
            walletRepository = new WalletRepository();

            if (!initialize)
                return;

            InitializeDealers();
            

            // инициализация сериализаторов
            SerializationWriter.TypeSurrogates.Add(new MarketOrderSerializer());
        }

        private ManagerTrade(Dictionary<string, IDealer> dealers)
        {
            accountRepository = AccountRepository.Instance;
            orderRepository = OrderRepository.Instance;
            profitCalculator = ProfitCalculator.Instance;
            walletRepository = new WalletRepository();
            brokerRepository = new BrokerRepository();

            this.dealers = dealers;
            foreach (var pair in dealers)
                pair.Value.ServerInterface = this;
        }

        #region ITradeSharpServer

        public RequestStatus SendNewOrderRequest(
            ProtectedOperationContext secCtx,
            int requestUniqueId,
            MarketOrder order,
            OrderType orderType,
            decimal requestedPrice,
            decimal slippagePoints)
        {
            // уникальность запроса
            if (RequestIdStorage.Instance.RequestsIsDoubled(order.AccountID, requestUniqueId))
                return RequestStatus.DoubledRequest;

            if (UserOperationRightsStorage.IsProtectedOperation(UserOperation.MakeNewOrder))
                if (!UserSessionStorage.Instance.PermitUserOperation(secCtx, 
                    UserOperationRightsStorage.IsTradeOperation(UserOperation.ModifyOrder), true)) 
                    return RequestStatus.Unauthorized;
            
            IDealer dealer;
            Account account;
            decimal equity;

            // объем входа и тикер
            if (order.Volume <= 0)
                return RequestStatus.BadRequest;
            if (!DalSpot.Instance.GetTickerNames().Contains(order.Symbol))
                return RequestStatus.BadRequest;

            try
            {            
                // проверить маржинальные требования и предел плеча
                if (!tradeManager.IsEnterEnabled(order.AccountID, order.Symbol, order.Side, order.Volume, out equity))
                    return RequestStatus.MarginOrLeverageExceeded;
                // по группе счета выбрать дилера и отправить ему запрос
                dealer = GetDealerByAccount(order.AccountID, out account);
                if (dealer == null) return RequestStatus.GroupUnsupported;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в SendNewOrderRequest", ex);
                return RequestStatus.ServerError;
            }

            try
            {
                var orderStatus = dealer.SendNewOrderRequest(account, order, orderType, 
                    requestedPrice, slippagePoints);
                AccountLogger.InfoFormat("Вызов SendNewOrderRequest(acc #{0}, {1} {2}, us. machine \"{3}\")",
                    order.AccountID, order.Symbol, order.Volume, secCtx.userMachineName);
                return orderStatus;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка дилера {0}: {1}", dealer, ex);
                return RequestStatus.DealerError;
            }
        }

        public RequestStatus SendNewPendingOrderRequest(
            ProtectedOperationContext secCtx,
            int requestUniqueId,
            PendingOrder order)
        {
            try
            {
                // уникальность запроса
                if (RequestIdStorage.Instance.RequestsIsDoubled(order.AccountID, requestUniqueId))
                    return RequestStatus.DoubledRequest;

                if (UserOperationRightsStorage.IsProtectedOperation(UserOperation.MakeNewOrder))
                    if (!UserSessionStorage.Instance.PermitUserOperation(secCtx,
                        UserOperationRightsStorage.IsTradeOperation(UserOperation.ModifyOrder), true)) 
                        return RequestStatus.Unauthorized;

                // проверить - 
                // время  не должно быть вне указанных пределов
                if (order.TimeFrom >= order.TimeTo || DateTime.Now >= order.TimeTo) return RequestStatus.WrongTime;
                // определить автоматом тип ордера (STOP - LIMIT)
                var pricePair = QuoteStorage.Instance.ReceiveValue(order.Symbol);
                if (pricePair == null)
                {
                    Logger.ErrorFormat("Невозможно создать отложенный ордер (счет {0}, {1}) - нет цены",
                                       order.AccountID, order.Symbol);
                    return RequestStatus.NoPrice;
                }
                var price = order.Side > 0 ? pricePair.ask : pricePair.bid;
                var delta = DalSpot.Instance.GetPointsValue(order.Symbol, Math.Abs(price - order.PriceFrom));
                if (delta < 1)
                {
                    Logger.ErrorFormat("Отложенный ордер (счет {0}, {1}) - близко к рынку {2}",
                                       order.AccountID, order.Symbol, price);
                    return RequestStatus.WrongPrice;
                }

                // проверить правильность задания интервалов цен
                var priceCorrect = true;
                if (order.PriceSide == PendingOrderType.Stop && order.Side > 0)
                {
                    priceCorrect = order.PriceTo != null
                                       ? price < order.PriceFrom && order.PriceTo.Value > order.PriceFrom
                                       : price < order.PriceFrom;
                }
                else if (order.PriceSide == PendingOrderType.Limit && order.Side > 0)
                {
                    priceCorrect = order.PriceTo != null
                                       ? price > order.PriceFrom && order.PriceTo.Value < order.PriceFrom
                                       : price > order.PriceFrom;
                }
                else if (order.PriceSide == PendingOrderType.Stop && order.Side < 0)
                {
                    priceCorrect = order.PriceTo != null
                                       ? price > order.PriceFrom && order.PriceTo.Value < order.PriceFrom
                                       : price > order.PriceFrom;
                }
                else
                    priceCorrect = order.PriceTo != null
                                       ? price < order.PriceFrom && order.PriceTo.Value > order.PriceFrom
                                       : price < order.PriceFrom;
                if (!priceCorrect)
                {
                    Logger.ErrorFormat("Отложенный ордер (счет {0}, {1}) - неверные цены {2} [{3}-{4}]",
                                       order.AccountID, order.Symbol, price, order.PriceFrom, order.PriceTo ?? 0);
                    return RequestStatus.WrongPrice;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в SendNewPendingOrderRequest", ex);
                return RequestStatus.ServerError;
            }
            
            // по группе счета выбрать дилера и отправить ему запрос
            Account account;
            var dealer = GetDealerByAccount(order.AccountID, out account);
            if (dealer == null) return RequestStatus.GroupUnsupported;
            try
            {
                AccountLogger.InfoFormat("Вызов SendNewPendingOrderRequest(acc #{0}, {1} {2}, us. machine \"{3}\")",
                    order.AccountID, order.Symbol, order.Volume, secCtx.userMachineName);
                return dealer.SendNewPendingOrderRequest(account, order);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка дилера {0}: {1}", dealer, ex);
                return RequestStatus.DealerError;
            }
        }

        /// <summary>
        /// закрыть все сделки по инструменту
        /// маржинальные требования не проверяются
        /// </summary>        
        public RequestStatus SendCloseByTickerRequest(ProtectedOperationContext secCtx,
                                                      int accountId, string ticker, PositionExitReason reason)
        {
            if (UserOperationRightsStorage.IsProtectedOperation(UserOperation.CloseOrder))
                if (!UserSessionStorage.Instance.PermitUserOperation(secCtx,
                                                                     UserOperationRightsStorage.IsTradeOperation(
                                                                         UserOperation.ModifyOrder), true))
                    return RequestStatus.Unauthorized;

            List<MarketOrder> orders;
            using (var ctx = DatabaseContext.Instance.Make())
            {
                orders = ctx.POSITION.Where(p => p.Symbol == ticker &&
                    p.AccountID == accountId).ToList().Select(LinqToEntity.DecorateOrder).ToList();
                if (orders.Count == 0) return RequestStatus.OK;
            }

            // дилер, обрабатывающий запрос
            Account account;
            var dealer = GetDealerByAccount(orders[0].AccountID, out account);
            if (dealer == null) return RequestStatus.GroupUnsupported;

            // сформировать запросы на закрытие для дилера
            var results = new List<RequestStatus>();
            foreach (var order in orders)
            {
                try
                {
                    var res = dealer.SendCloseRequest(order, reason);
                    if (res == RequestStatus.OK)
                        Logger.InfoFormat("Ордер {0} закрыт, счет {1} символ {2}, объем {3}, тип {4}", order.ID,
                                          order.AccountID, order.Symbol, order.Volume, order.Side == 1 ? "buy" : "sell");
                    else
                        Logger.ErrorFormat("Ошибка закрытия ордера {0} - ответ дилера:{1}", order.ID, res);
                    results.Add(res);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Ошибка дилера {0}: {1}", dealer, ex);
                    return RequestStatus.DealerError;
                }                
            }    
            return results.Mode();
        }

        public RequestStatus SendCloseRequest(ProtectedOperationContext secCtx,
            int accountId, int orderId, PositionExitReason reason)
        {
            if (UserOperationRightsStorage.IsProtectedOperation(UserOperation.CloseOrder))
                if (!UserSessionStorage.Instance.PermitUserOperation(secCtx,
                    UserOperationRightsStorage.IsTradeOperation(UserOperation.ModifyOrder), true))
                return RequestStatus.Unauthorized;
            
            // получить позу
            POSITION order;
            using (var ctx = DatabaseContext.Instance.Make())
            {
                order = ctx.POSITION.FirstOrDefault(p => p.ID == orderId && p.AccountID == accountId);
                if (order != null)
                    if (order.State != (int)PositionState.Opened)
                        return RequestStatus.BadRequest;
            }
            if (order == null) return RequestStatus.NotFound;

            AccountLogger.InfoFormat("Вызов SendCloseRequest(acc #{0}, orderId #{1}, us. machine \"{2}\")",
                accountId, orderId, secCtx.userMachineName);

            return SendCloseRequest(order, reason);
        }

        public RequestStatus SendCloseRequest(POSITION order, PositionExitReason reason)
        {
            Account account;
            var dealer = GetDealerByAccount(order.AccountID, out account);
            if (dealer == null) return RequestStatus.GroupUnsupported;

            // проверить маржинальные требования
            decimal equity;
            if (!tradeManager.IsEnterEnabled(order.AccountID, order.Symbol, -order.Side, order.Volume, out equity))
                return RequestStatus.MarginOrLeverageExceeded;

            try
            {
                var res = dealer.SendCloseRequest(LinqToEntity.DecorateOrder(order), reason);
                if (res == RequestStatus.OK)
                    Logger.InfoFormat("Ордер {0} закрыт, счет {1} символ {2}, объем {3}, тип {4}", order.ID, order.AccountID, order.Symbol, order.Volume, order.Side == 1 ? "buy": "sell");
                else
                    Logger.ErrorFormat("Ошибка закрытия ордера {0} - ответ дилера:{1}", order.ID, res);
                return res;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка дилера {0}: {1}", dealer, ex);
                return RequestStatus.DealerError;
            }
        }

        public RequestStatus SendDeletePendingOrderRequest(
            ProtectedOperationContext secCtx, 
            PendingOrder order,
            PendingOrderStatus status, int? positionID, string closeReason)
        {
            if (UserOperationRightsStorage.IsProtectedOperation(UserOperation.CloseOrder))
                if (!UserSessionStorage.Instance.PermitUserOperation(secCtx,
                    UserOperationRightsStorage.IsTradeOperation(UserOperation.ModifyOrder), true))
                    return RequestStatus.Unauthorized;

            Account account;
            var dealer = GetDealerByAccount(order.AccountID, out account);
            if (dealer == null) return RequestStatus.GroupUnsupported;

            try
            {
                var rst = dealer.DeletePendingOrderRequest(order, status, positionID, closeReason);
                AccountLogger.InfoFormat("SendDeletePendingOrderRequest({0}, {1} {2} {3}): {4}",
                    order.ID, (DealType)order.Side, order.PriceSide, order.Symbol, rst);
                return rst;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка дилера {0}: {1}", dealer, ex);
                return RequestStatus.DealerError;
            }
        }

        public RequestStatus SendEditMarketRequest(ProtectedOperationContext secCtx, MarketOrder ord)
        {
            if (UserOperationRightsStorage.IsProtectedOperation(UserOperation.ModifyOrder))
                if (!UserSessionStorage.Instance.PermitUserOperation(secCtx,
                    UserOperationRightsStorage.IsTradeOperation(UserOperation.ModifyOrder), true))
                    return RequestStatus.Unauthorized;

            Account account;
            var dealer = GetDealerByAccount(ord.AccountID, out account);
            if (dealer == null) return RequestStatus.GroupUnsupported;
            try
            {
                var rst = dealer.ModifyMarketOrderRequest(ord);
                AccountLogger.InfoFormat(
                    "SendEditMarketRequest({0}, {1} {2} {3}, SL={4}, TP={5}): {6}, пользователь: \"{7}\"",
                    ord.ID, (DealType)ord.Side, ord.Volume, ord.Symbol, 
                    ord.StopLoss, ord.TakeProfit,
                    rst, secCtx.userMachineName);
                return rst;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка дилера {0}: {1}", dealer, ex);
                return RequestStatus.DealerError;
            }
        }

        public RequestStatus SendEditPendingRequest(ProtectedOperationContext secCtx, PendingOrder ord)
        {
            if (UserOperationRightsStorage.IsProtectedOperation(UserOperation.ModifyOrder))
                if (!UserSessionStorage.Instance.PermitUserOperation(secCtx, true, true))
                    return RequestStatus.Unauthorized;

            Account account;
            var dealer = GetDealerByAccount(ord.AccountID, out account);
            if (dealer == null) return RequestStatus.GroupUnsupported;
            try
            {
                var rst = dealer.ModifyPendingOrderRequest(ord);
                AccountLogger.InfoFormat(
                    "SendEditPendingRequest({0}, {1} {2} {3}, {4} at {5}, SL={6}, TP={7}): {8}, пользователь: {9}",
                    ord.ID, (DealType)ord.Side, ord.PriceSide,
                    ord.Volume, ord.Symbol, ord.PriceFrom.ToStringUniformPriceFormat(),
                    ord.StopLoss, ord.TakeProfit,
                    rst, secCtx.userMachineName);
                return rst;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка дилера {0}: {1}", dealer, ex);
                return RequestStatus.DealerError;
            }
        }
        #endregion        
        
        #region Методы, вызываемые дилерами
        /// <summary>
        /// вызывается дилером - в БД заносится новая поза (открытая либо в процессе открытия)
        /// отправить уведомление клиенту
        /// </summary>
        /// <param name="order">поза</param>
        /// <param name="posID">ID сущности из БД</param>        
        /// <returns>OK?</returns>
        public bool SaveOrderAndNotifyClient(MarketOrder order, out int posID)
        {
            posID = -1;
            using (var ctx = DatabaseContext.Instance.Make())
            {
                var account = ctx.ACCOUNT.FirstOrDefault(ac => ac.ID == order.AccountID);
                if (account == null)
                    return false;

                // провести ордер через биллинг
                OrderBill bill = null;
                if (order.State == PositionState.Opened)
                    bill = BillingManager.ProcessOrderOpening(order, LinqToEntity.DecorateAccount(account));

                var pos = new POSITION
                              {
                                  AccountID = order.AccountID,
                                  Comment = order.Comment,
                                  ExpertComment = order.ExpertComment,
                                  Magic = order.Magic,
                                  PendingOrderID = order.PendingOrderID,
                                  PriceBest = (decimal?)order.PriceBest,
                                  PriceWorst = (decimal?)order.PriceWorst,
                                  PriceEnter = (decimal)order.PriceEnter,
                                  Side = order.Side,
                                  State = ((int) order.State),
                                  Stoploss = (decimal?)order.StopLoss,
                                  Symbol = order.Symbol,
                                  Takeprofit = (decimal?)order.TakeProfit,
                                  TimeEnter = order.TimeEnter,
                                  TrailLevel1 = (decimal?)order.TrailLevel1,
                                  TrailLevel2 = (decimal?)order.TrailLevel2,
                                  TrailLevel3 = (decimal?)order.TrailLevel3,
                                  TrailLevel4 = (decimal?)order.TrailLevel4,
                                  TrailTarget1 = (decimal?)order.TrailTarget1,
                                  TrailTarget2 = (decimal?)order.TrailTarget2,
                                  TrailTarget3 = (decimal?)order.TrailTarget3,
                                  TrailTarget4 = (decimal?)order.TrailTarget4,
                                  Volume = order.Volume,
                                  MasterOrder = order.MasterOrder
                              };
                
                account.POSITION.Add(pos);
                try
                {
                    ctx.SaveChanges();
                    posID = pos.ID;
                    order.ID = posID;
                    if (bill != null)
                        BillingManager.SaveNewOrderBill(bill, pos.ID, ctx);
                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка сохранения сущности POSITION", ex);
                    ServiceManagerClientManagerProxy.Instance.NewOrderResponse(null,
                                                                               RequestStatus.ServerError, "crudsav");
                    return false;
                }
            }
            // отправить уведомление клиенту            
            ServiceManagerClientManagerProxy.Instance.NewOrderResponse(order, RequestStatus.OK, "");

            // разослать торговый сигнал?
            if (order.State == PositionState.Opened)
                MakeSignalNewDeal(order.AccountID, order.Symbol, order.Side, order.Volume,
                                  order.StopLoss.HasValue ? (decimal) order.StopLoss.Value : (decimal?) null,
                                  order.TakeProfit.HasValue ? (decimal) order.TakeProfit.Value : (decimal?) null,
                                  posID, order.PriceEnter);

            return true;
        }

        public void NotifyClientOnOrderRejected(MarketOrder order, string rejectReason)
        {
            ServiceManagerClientManagerProxy.Instance.NewOrderResponse(order, RequestStatus.DealerError,
                rejectReason);
        }
        
        public bool SavePendingOrder(PendingOrder order, out int orderID)
        {
            orderID = -1;
            using (var ctx = DatabaseContext.Instance.Make())
            {
                var account = ctx.ACCOUNT.FirstOrDefault(ac => ac.ID == order.AccountID);
                if (account == null) return false;

                // найти парный ордер, чтобы прописать OCO
                PENDING_ORDER pairOrder = null;

                if (order.PairOCO.HasValue && order.PairOCO.Value > 0)
                {
                    pairOrder = ctx.PENDING_ORDER.FirstOrDefault(o => o.ID == order.PairOCO.Value);
                    if (pairOrder == null) order.PairOCO = null;
                    else
                    {
                        var anotherPair = ctx.PENDING_ORDER.FirstOrDefault(o => o.ID == pairOrder.ID);
                        if (anotherPair != null)
                        {
                            anotherPair.PairOCO = null;
                            ctx.Entry(anotherPair).State = EntityState.Modified;
                        }
                    }
                }

                var pendingOrder = LinqToEntity.UndecorateLiveActiveOrder(order);
                account.PENDING_ORDER.Add(pendingOrder);

                try
                {
                    ctx.SaveChanges();
                    orderID = pendingOrder.ID;

                    // таки прописать ID парному ордеру
                    if (pairOrder != null)
                    {
                        pairOrder.PairOCO = orderID;
                        ctx.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка сохранения сущности PENDING_ORDER", ex);
                    ServiceManagerClientManagerProxy.Instance.NewOrderResponse(null,
                                                                               RequestStatus.ServerError, "crudsav");
                    return false;
                }
            }
            // отправить уведомление клиенту
            ServiceManagerClientManagerProxy.Instance.NewPendingOrderResponse(order, RequestStatus.OK, "");
            return true;
        }

        public bool CloseOrder(int orderId, decimal price, PositionExitReason exitReason)
        {
            MarketOrder orderDecor;
            using (var ctx = DatabaseContext.Instance.Make())
            {
                var orderQuery = ctx.POSITION.Where(p => p.ID == orderId).ToList();
                if (orderQuery.Count == 0)
                {
                    Logger.ErrorFormat("CloseOrder(Id = {0}) - позиция не найдена", orderId);
                    return false;
                }
                orderDecor = LinqToEntity.DecorateOrder(orderQuery[0]);
            }
            return CloseOrder(orderDecor, price, exitReason);
        }

        public bool CloseOrder(MarketOrder order, decimal price, PositionExitReason exitReason)
        {
            return orderRepository.CloseOrder(order, price, exitReason);
        }

        public bool GetMarketOrder(int orderId, out MarketOrder order)
        {
            //Logger.InfoFormat("GetMarketOrder : orderId={0}", orderId);
            order = GetMarketOrder(orderId);
            if (order == null) return false;
            return true;
        }

        public bool DeletePendingOrder(PendingOrder orderDecorated,
            PendingOrderStatus status, int? positionID, string closeReason)
        {
            var order = LinqToEntity.UndecorateLiveActiveOrder(orderDecorated);
            using (var ctx = DatabaseContext.Instance.Make())
            {
                // получить текущую цену
                var pair = QuoteStorage.Instance.ReceiveValue(order.Symbol);
                var priceClosed = pair == null ? 0 : order.Side > 0 ? pair.bid : pair.ask;

                var hist = new PENDING_ORDER_CLOSED
                               {
                                   OrderID = order.ID,
                                   AccountID = order.AccountID,
                                   Comment = order.Comment,
                                   ExpertComment = order.ExpertComment,
                                   Magic = order.Magic,
                                   PairOCO = order.PairOCO,
                                   PriceFrom = order.PriceFrom,
                                   PriceTo = order.PriceTo,
                                   Side = order.Side,
                                   Status = (int) status,
                                   Stoploss = order.Stoploss,
                                   Takeprofit = order.Takeprofit,
                                   Symbol = order.Symbol,
                                   TimeClosed = DateTime.Now,
                                   Volume = order.Volume,
                                   PriceClosed = (decimal)priceClosed,
                                   Position = positionID,
                                   CloseReason = closeReason
                               };
                try
                {
                    ctx.PENDING_ORDER_CLOSED.Add(hist);

                    var ord = ctx.PENDING_ORDER.FirstOrDefault(p => p.ID == order.ID);
                    if (ord == null)
                    {
                        Logger.ErrorFormat("DeletePendingOrder - ордер {0} не найден", order.ID);
                        ServiceManagerClientManagerProxy.Instance.NewOrderResponse(null,
                                                                                   RequestStatus.ServerError, "crudsav");
                        return false;
                    }
                    ctx.PENDING_ORDER.Remove(ord);
                    // сохранить изменения
                    ctx.SaveChanges();

                    return true;
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Ошибка в DeletePendingOrder (#{0}): {1}", order.ID, ex);
                }
            }
            return false;
        }

        public bool DeletePendingOrder(PENDING_ORDER order,
            TradeSharpConnection ctx,
            PendingOrderStatus status,
            int? positionID,
            string closeReason)
        {
            // получить текущую цену
            var pair = QuoteStorage.Instance.ReceiveValue(order.Symbol);
            var priceClosed = pair == null ? 0 : order.Side > 0 ? pair.bid : pair.ask;

            var hist = new PENDING_ORDER_CLOSED
            {
                OrderID = order.ID,
                AccountID = order.AccountID,
                Comment = order.Comment,
                ExpertComment = order.ExpertComment,
                Magic = order.Magic,
                PairOCO = order.PairOCO,
                PriceFrom = order.PriceFrom,
                PriceTo = order.PriceTo,
                Side = order.Side,
                Status = (int)status,
                Stoploss = order.Stoploss,
                Takeprofit = order.Takeprofit,
                Symbol = order.Symbol,
                TimeClosed = DateTime.Now,
                Volume = order.Volume,
                PriceClosed = (decimal)priceClosed,
                Position = positionID,
                CloseReason = closeReason
            };
            try
            {
                ctx.PENDING_ORDER_CLOSED.Add(hist);
                var ord = ctx.PENDING_ORDER.FirstOrDefault(p => p.ID == order.ID);
                if (ord == null)
                {
                    Logger.ErrorFormat("DeletePendingOrder - ордер {0} не найден", order.ID);
                    ServiceManagerClientManagerProxy.Instance.NewOrderResponse(null,
                                                                               RequestStatus.ServerError, "crudsav");
                    return false;
                }
                ctx.PENDING_ORDER.Remove(ord);
                ctx.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка в DeletePendingOrder (#{0}): {1}", order.ID, ex);
            }
            return false;
        }

        /// <summary>
        /// модифицировать ордер - только открытые ордера
        /// </summary>        
        public bool ModifyMarketOrder(MarketOrder order)
        {
            // внести изменения в БД
            POSITION pos;
            PositionState oldPosState;
            decimal? oldStop, oldTake;

            var targetStop = order.StopLoss == 0 ? null : (decimal?)order.StopLoss;
            var targetTake = order.TakeProfit == 0 ? null : (decimal?)order.TakeProfit;

            order.TakeProfit = order.TakeProfit == 0 ? null : order.TakeProfit;

            using (var ctx = DatabaseContext.Instance.Make())
            {
                try
                {
                    pos = (from p in ctx.POSITION where p.ID == order.ID select p).First();
                }
                catch (Exception)
                {
                    Logger.ErrorFormat("ModifyMarketOrder - ордер {0} не найден", order.ID);
                    ServiceManagerClientManagerProxy.Instance.ModifyPendingOrderResponse(null,
                                                                                         RequestStatus.ServerError,
                                                                                         "crudsav");
                    return false;
                }

                oldPosState = (PositionState)pos.State;
                oldStop = pos.Stoploss;
                oldTake = pos.Takeprofit;

                pos.Magic = order.Magic;
                pos.Stoploss = targetStop;
                pos.Takeprofit = targetTake;
                pos.TimeEnter = order.TimeEnter;
                pos.Comment = order.Comment;
                pos.ExpertComment = order.ExpertComment;
                pos.State = (int)order.State;
                pos.PriceEnter = (decimal)order.PriceEnter;
                pos.PriceBest = (decimal?)order.PriceBest;
                pos.PriceWorst = (decimal?)order.PriceWorst;
                pos.TrailLevel1 = (decimal?)order.TrailLevel1;
                pos.TrailLevel2 = (decimal?)order.TrailLevel2;
                pos.TrailLevel3 = (decimal?)order.TrailLevel3;
                pos.TrailLevel4 = (decimal?)order.TrailLevel4;
                pos.TrailTarget1 = (decimal?) order.TrailTarget1;
                pos.TrailTarget2 = (decimal?)order.TrailTarget2;
                pos.TrailTarget3 = (decimal?)order.TrailTarget3;
                pos.TrailTarget4 = (decimal?)order.TrailTarget4;
                try
                {
                    ctx.SaveChanges();
                }
                catch (OptimisticConcurrencyException)
                {
                    // Resolve the concurrency conflict by refreshing the 
                    // object context before re-saving changes. 
                    Logger.Error("ModifyMarketOrder - OptimisticConcurrencyException");
                    ctx.Entry(pos).State = EntityState.Modified;
                    ((IObjectContextAdapter)ctx).ObjectContext.Refresh(RefreshMode.ClientWins, pos);
                    // Save changes.
                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка модификации сущности POSITION", ex);
                    ServiceManagerClientManagerProxy.Instance.ModifyMarketOrderResponse(null,
                                                                                        RequestStatus.ServerError,
                                                                                        "crudsav");
                    return false;
                }
            }

            // уведомить клиента
            ServiceManagerClientManagerProxy.Instance.ModifyMarketOrderResponse(order, RequestStatus.OK, "");

            // отправить торговый сигнал на открытие сделки?
            if (oldPosState != PositionState.Opened && order.State == PositionState.Opened)
                MakeSignalNewDeal(order.AccountID, order.Symbol, order.Side, order.Volume,
                                  order.StopLoss.HasValue ? (decimal) order.StopLoss.Value : (decimal?) null,
                                  order.TakeProfit.HasValue ? (decimal) order.TakeProfit.Value : (decimal?) null,
                                  order.ID, order.PriceEnter);
            else
            {
                if (oldStop != targetStop || oldTake != targetTake)
                    // отправить торговый сигнал на изменение SL - TP?
                    MakeOrderChangedSignal(order.AccountID, order.ID, targetStop, targetTake);
            }

            return true;
        }

        public bool ModifyPendingOrder(PendingOrder modified)
        {
            // внести изменения в БД
            PENDING_ORDER ord;
            using (var ctx = DatabaseContext.Instance.Make())
            {
                try
                {
                    ord = ctx.PENDING_ORDER.FirstOrDefault(o => o.ID == modified.ID);
                    if (ord == null) return false;
                }
                catch (Exception)
                {
                    Logger.ErrorFormat("ModifyPendingOrder - ордер {0} не найден", modified.ID);
                    ServiceManagerClientManagerProxy.Instance.ModifyPendingOrderResponse(null,
                                                                                         RequestStatus.ServerError,
                                                                                         "crudsav");
                    return false;
                }

                ord.PriceFrom = (decimal)modified.PriceFrom;
                ord.PriceTo = (decimal?)modified.PriceTo;
                ord.Stoploss = (decimal?)modified.StopLoss;
                ord.Takeprofit = (decimal?)modified.TakeProfit;
                ord.TimeFrom = modified.TimeFrom;
                ord.TimeTo = modified.TimeTo;
                ord.Volume = modified.Volume;
                ord.Side = modified.Side;
                ord.Symbol = modified.Symbol;
                ord.Magic = modified.Magic;
                ord.Comment = modified.Comment;
                ord.ExpertComment = modified.ExpertComment;
                ord.PriceSide = (int)modified.PriceSide;
                // трейлинг
                ord.TrailLevel1 = (decimal?)modified.TrailLevel1;
                ord.TrailLevel2 = (decimal?)modified.TrailLevel2;
                ord.TrailLevel3 = (decimal?)modified.TrailLevel3;
                ord.TrailLevel4 = (decimal?)modified.TrailLevel4;
                ord.TrailTarget1 = (decimal?)modified.TrailTarget1;
                ord.TrailTarget2 = (decimal?)modified.TrailTarget2;
                ord.TrailTarget3 = (decimal?)modified.TrailTarget3;
                ord.TrailTarget4 = (decimal?)modified.TrailTarget4;

                // обновить ссылки ордеров друг на друга
                if (ord.PairOCO != modified.PairOCO)
                {
                    if (modified.PairOCO.HasValue)
                    {
                        // обновить парный ордер
                        var pairOrder = ctx.PENDING_ORDER.FirstOrDefault(o => o.ID == modified.PairOCO.Value);
                        if (pairOrder != null)
                        {
                            var oldPair = pairOrder.PairOCO.HasValue 
                                ? ctx.PENDING_ORDER.FirstOrDefault(o => o.ID == pairOrder.PairOCO.Value) : null;
                            if (oldPair != null)
                            {
                                oldPair.PairOCO = null;
                                ctx.Entry(oldPair).State = EntityState.Modified;
                            }
                            pairOrder.PairOCO = ord.ID;
                            ctx.Entry(pairOrder).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        // снять ссылку для парного ордера
                        var pairOrder = ctx.PENDING_ORDER.FirstOrDefault(o => o.PairOCO == ord.ID);
                        if (pairOrder != null)
                        {
                            pairOrder.PairOCO = null;
                            ctx.Entry(pairOrder).State = EntityState.Modified;
                        }
                    }
                }
                ord.PairOCO = modified.PairOCO;

                try
                {
                    ctx.SaveChanges();
                }
                catch (OptimisticConcurrencyException)
                {
                    // Resolve the concurrency conflict by refreshing the 
                    // object context before re-saving changes. 
                    Logger.Error("ModifyPendingOrder - OptimisticConcurrencyException");
                    ctx.Entry(ord).State = EntityState.Modified;
                    ((IObjectContextAdapter)ctx).ObjectContext.Refresh(RefreshMode.ClientWins, ord);
                    // Save changes.
                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка модификации сущности PENDING_ORDER", ex);
                    ServiceManagerClientManagerProxy.Instance.ModifyPendingOrderResponse(null,
                                                                                         RequestStatus.ServerError,
                                                                                         "crudsav");
                    return false;
                }
            }

            // уведомить клиента
            ServiceManagerClientManagerProxy.Instance.ModifyPendingOrderResponse(modified, RequestStatus.OK, "");

            return true;
        }
        
        #endregion

        public void Stop()
        {
            foreach (var dealer in dealers.Values)
                dealer.Deinitialize();
        }
    }
}

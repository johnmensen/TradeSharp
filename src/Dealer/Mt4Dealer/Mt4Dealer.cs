using System;
using System.Collections.Generic;
using BrokerService.Contract.Contract;
using BrokerService.Contract.Entity;
using Entity;
using FixDealer;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.DealerInterface;
using TradeSharp.Util;
using MarketOrder = TradeSharp.Contract.Entity.MarketOrder;
using OrderRejectReason = BrokerService.Contract.Entity.OrderRejectReason;
using OrderStatus = BrokerService.Contract.Entity.OrderStatus;
using RequestStatus = TradeSharp.Contract.Entity.RequestStatus;

namespace Mt4Dealer
{
    public class Mt4Dealer : SimpleDealer, IDealer, ITradeRequestCallback
    {
        private readonly string configFileName;

        private static int RequestUniqueId
        {
            get
            {
                var date = DateTime.Now;
                return (int)(date.Millisecond + date.Second * 1000 + date.Minute * 60 * 35 + date.Hour * 60 * 60 * 24);
            }
        }

        private readonly ErrorStorage errorStorage;

        private readonly FloodSafeLogger logNoFlood = new FloodSafeLogger(1000 * 60);

        private const int MsgPingServerError = 1;

        private RabbitServer rabbitServer;

        public List<string> GroupCodes { get; private set; }

        public string DealerCode { get; private set; }

        private static readonly Dictionary<BrokerService.Contract.Entity.RequestStatus, RequestStatus> responseNative =
            new Dictionary<BrokerService.Contract.Entity.RequestStatus, RequestStatus>
            {
                {BrokerService.Contract.Entity.RequestStatus.ErrorOnExecute, RequestStatus.ServerError},
                {BrokerService.Contract.Entity.RequestStatus.Executed, RequestStatus.OK},
                {BrokerService.Contract.Entity.RequestStatus.Rejected, RequestStatus.DealerError}
            };

        public Mt4Dealer(DealerDescription desc, List<string> groupCodes)
        {
            errorStorage = new ErrorStorage();
            GroupCodes = groupCodes;
            DealerCode = desc.Code;
            configFileName = desc.FileName;                        
        }

        public void Initialize()
        {
            var cfg = new DealerConfigParser(typeof(Mt4Dealer), configFileName);
            rabbitServer = new RabbitServer(cfg, RequestProcessed);
            rabbitServer.Start();
        }

        public void Deinitialize()
        {
            rabbitServer.Stop();
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
            var quote = QuoteStorage.Instance.ReceiveValue(order.Symbol);
            if (quote == null)
                return RequestStatus.NoPrice;

            var slippageAbs = slippagePoints;
            if (slippageAbs > 0)
                slippageAbs = DalSpot.Instance.GetAbsValue(order.Symbol, slippageAbs);
            var status = MakeMarketRequest(new TradeTransactionRequest
            {
                Account = account.ID,
                ClosingPositionId = null,
                Comment = order.Comment,
                ExpertComment = order.ExpertComment,
                Id = RequestUniqueId, // !!
                Magic = order.Magic,
                RequestedPrice = (decimal)(order.Side > 0 ? quote.ask : quote.bid),
                SlippageAbs = slippageAbs,
                Side = order.Side,
                Symbol = order.Symbol,
                Volume = order.Volume
            }, order);
            return status;
        }

        public RequestStatus SendCloseRequest(MarketOrder order, PositionExitReason reason)
        {
            var requestedPrice = 0M;
            var quote = QuoteStorage.Instance.ReceiveValue(order.Symbol);
            if (quote != null)
                requestedPrice = (decimal) (order.Side > 0 ? quote.bid : quote.ask);

            if (!order.MasterOrder.HasValue)
            {
                Logger.DebugFormat("{0}: SendCloseRequest - закрытие ордера {1} - MasterOrder не задан",
                    DealerCode, order.ToStringShort());
                return RequestStatus.DealerError;
            }

            var status = MakeMarketRequest(new TradeTransactionRequest
            {
                Account = order.AccountID,
                ClosingPositionId = order.MasterOrder,
                Comment = order.Comment,
                ExpertComment = order.ExpertComment,
                Id = order.ID,
                Magic = order.Magic,
                RequestedPrice = requestedPrice,
                SlippageAbs = 0,
                Side = order.Side,
                Symbol = order.Symbol,
                Volume = order.Volume
            }, order);
            return status;
        }
 
        private RequestStatus MakeMarketRequest(TradeTransactionRequest request, MarketOrder order)
        {
            Logger.InfoFormat("Отправка запроса в MT4 ({0}): {1}", DealerCode, request);

            try
            {
                RequestStorage.Instance.StoreRequest(new RequestedOrder(request, order));
                rabbitServer.SendRequest(request);
                return RequestStatus.OK;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка отправки запроса MT4", ex);
                return responseNative[BrokerService.Contract.Entity.RequestStatus.ErrorOnExecute];
            }
        }

        public void RequestProcessed(BrokerService.Contract.Entity.BrokerResponse response)
        {
            var request = RequestStorage.Instance.FindRequest(response.RequestId);
            if (request == null)
            {
                Logger.Info("MT4 executor - исходный запрос не найден: " + response);
                return;
            }

            Logger.Info("Ответ MT4 executor: " + response);

            if (response.Status != OrderStatus.Executed)
                return;

            // закрытие ордера
            if (request.request.IsClosingRequest)
            {
                if (!response.Price.HasValue)
                {
                    Logger.Error("Ответ MT4 executor - закрытие позиции - нет цены");
                    return;
                }

                ServerInterface.CloseOrder(request.requestedOrder.ID, 
                    response.Price.Value, PositionExitReason.Closed);
                return;
            }

            // открытие ордера
            if (!response.Price.HasValue)
                response.RejectReason = OrderRejectReason.UnknownOrder;
            if (response.RejectReason.HasValue && response.RejectReason.Value != OrderRejectReason.None)
            {
                ServerInterface.NotifyClientOnOrderRejected(request.requestedOrder,
                    response.RejectReasonString);
                Logger.DebugFormat("{0}: order {1} is rejected for reason {2}",
                    DealerCode, request.requestedOrder.ToStringShort(), response.RejectReason);
                return;
            }

            var order = request.requestedOrder;
            order.PriceEnter = (float) response.Price.Value;
            order.AccountID = request.request.Account;
            order.TimeEnter = DateTime.Now;
            order.State = PositionState.Opened;
            order.MasterOrder = response.Mt4OrderId;

            int openedOrderId;
            ServerInterface.SaveOrderAndNotifyClient(order, out openedOrderId);
        }

        public void Ping()
        {
        }

        //private void PingServerRoutine()
        //{
        //    const int secsBetweenPing = 10;
        //    var lastTimePinged = DateTime.Now;
        //    while (!isStopping)
        //    {
        //        Thread.Sleep(200);
        //        var secondsPassed = (DateTime.Now - lastTimePinged).TotalSeconds;
        //        if (secondsPassed < secsBetweenPing) continue;
        //        PingServer();
        //        lastTimePinged = DateTime.Now;
        //    }
        //}

        //private void PingServer()
        //{
        //    try
        //    {
        //        mt4Executor.Ping(sessionId);
        //    }
        //    catch (Exception ex)
        //    {        
        //        logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, MsgPingServerError, 1000 * 60 * 30,
        //            "MT4 dealer - ошибка пинга сервера: {0}", ex);
        //    }
        //}
    }
}

using System;
using System.Collections.Generic;
using Entity;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Robot.Robot;
using TradeSharp.Util;

namespace TradeSharp.Robot.BacktestServerProxy
{
    public abstract partial class RobotContext : IQuoteStorage, INewsStorage
    {
        public delegate void OnRobotMessageDel(BaseRobot robot, DateTime time, List<string> messages);
        
        protected IStorage<string, QuoteData> quotesStorage;

        public IStorage<string, QuoteData> QuotesStorage
        {
            get { return quotesStorage; }
        }
        protected readonly List<BaseRobot> listRobots = new List<BaseRobot>();

        private OnRobotMessageDel onRobotMessage;
        public event OnRobotMessageDel OnRobotMessage
        {
            add { onRobotMessage += value; }
            remove { onRobotMessage -= value; }
        }

        public void SubscribeRobot(BaseRobot robot)
        {
            if (listRobots.IndexOf(robot) == -1)
                listRobots.Add(robot);
        }

        public void UnsubscribeRobot(BaseRobot robot)
        {
            if (listRobots.IndexOf(robot) != -1)
                listRobots.Remove(robot);
        }

        public PackedQuoteStream GetDayQuotes(string symbol, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public PackedCandleStream GetMinuteCandlesPacked(string symbol, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, DateSpan> GetTickersHistoryStarts()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, QuoteData> GetQuoteData()
        {
            throw new NotImplementedException();
        }

        public PackedCandleStream GetMinuteCandlesPackedFast(string symbol, List<Cortege2<DateTime, DateTime>> intervals)
        {
            throw new NotImplementedException();
        }

        public void OnQuotesReceived(string[] names, CandleDataBidAsk[] quotes, bool isHistoryTakeOff)
        {
            var realOrModelTime = quotes[0].timeClose;
            foreach (var robot in listRobots)
            {                
                var robotMessages = robot.OnQuotesReceived(names, quotes, isHistoryTakeOff);
                if (robotMessages != null && robotMessages.Count > 0 && onRobotMessage != null)
                    onRobotMessage(robot, realOrModelTime, robotMessages);
            }
        }

        public void OnNewsReceived(News[] news)
        {
            var realOrModelTime = news[0].Time;
            foreach (var robot in listRobots)
            {
                var robotMessages = robot.OnNewsReceived(news);
                if (robotMessages != null && robotMessages.Count > 0 && onRobotMessage != null)
                    onRobotMessage(robot, realOrModelTime, robotMessages);
            }
        }

        #region Функции для загрузки истории новостей и раздачи их роботам
        private void LoadNews()
        {
            NewsStorage.Instance.LoadNews();
            // и че с ними делать дальше???
        }

        #endregion

        public abstract RequestStatus SendNewOrderRequest(ProtectedOperationContext secCtx,
            int requestUniqueId,
            MarketOrder order,
            OrderType orderType,
            decimal requestedPrice,
            decimal slippagePoints);

        public abstract RequestStatus SendNewPendingOrderRequest(ProtectedOperationContext ctx,
                                                                 int requestUniqueId, PendingOrder order);

        public abstract RequestStatus SendDeletePendingOrderRequest(ProtectedOperationContext ctx,
            PendingOrder order, PendingOrderStatus status, int? positionId, string closeReason);

        public abstract RequestStatus SendEditMarketRequest(ProtectedOperationContext secCtx, MarketOrder pos);

        public abstract RequestStatus SendCloseRequest(ProtectedOperationContext ctx, int accountId, int orderId, PositionExitReason reason);

        public abstract RequestStatus SendCloseByTickerRequest(ProtectedOperationContext ctx, int accountId, string ticker, PositionExitReason reason);

        public abstract RequestStatus SendEditPendingRequest(ProtectedOperationContext secCtx, PendingOrder ord);

        public AuthenticationResponse GetUserDetail(string login, string password, out PlatformUser user)
        {
            user = null;
            return AuthenticationResponse.ServerError;
        }

        public AuthenticationResponse ModifyUserAndAccount(string oldLogin, string oldPassword, 
            PlatformUser user, int? accountId, float accountMaxLeverage, out bool loginIsBusy)
        {
            loginIsBusy = false;
            return AuthenticationResponse.ServerError;
        }

        public List<UserEvent> GetUserEvents(ProtectedOperationContext ctx, string userLogin,
            bool deleteReceivedEvents)
        {
            return new List<UserEvent>();
        }

        public virtual void SendTradeSignalEvent(ProtectedOperationContext ctx, int accountId, int tradeSignalCategory,
            UserEvent acEvent)
        {
            Logger.InfoFormat("Текстовое сообщение робота: [{0:dd.MM.yyyy HH:mm}] {1} {2} {3}",
                acEvent.Time, acEvent.Code, acEvent.Title, acEvent.Text);
        }

        public List<PerformerStat> GetAllPerformers()
        {
            throw new NotImplementedException();
        }

        public CreateReadonlyUserRequestStatus MakeOrDeleteReadonlyUser(ProtectedOperationContext secCtx,
                                                                       int accountId, bool makeNew, string pwrd,
                                                                       out PlatformUser user)
        {
            user = null;
            return CreateReadonlyUserRequestStatus.CommonError;
        }

        public RequestStatus QueryReadonlyUserForAccount(ProtectedOperationContext secCtx, int accountId,
                                                         out PlatformUser user)
        {
            user = null;
            return RequestStatus.CommonError;
        }

        public RequestStatus ApplyPortfolioTradeSettings(ProtectedOperationContext secCtx, string login, AutoTradeSettings sets)
        {
            throw new NotImplementedException();
        }

        public void Ping()
        {            
        }

        public List<News> GetNews(int account, DateTime date, int[] newsChannelIds)
        {
            return null;
        }

        public NewsMap GetNewsMap(int accountId)
        {
            return null;
        }

        public List<PaidService> GetPaidServices(string userLogin)
        {
            throw new NotImplementedException();
        }

        public abstract RequestStatus GetOrdersByFilter(int accountId, bool getClosedOrders, OrderFilterAndSortOrder filter, out List<MarketOrder> orders);

        public List<string> GetTickersTraded(int accountId)
        {
            throw new NotImplementedException();
        }

        public List<AccountShareOnDate> GetAccountShareHistory(int accountId, string userLogin)
        {
            throw new NotImplementedException();
        }

        public List<MarketOrder> GetClosedOrders(int accountId, string optionalSymbolFilter, int startId, int maxCount)
        {
            throw new NotImplementedException();
        }

        public List<Transfer> GetAccountTransfersPartByPart(ProtectedOperationContext secCtx,
            string login, int startId, int countMax)
        {
            throw new NotImplementedException();
        }

        public List<AccountShare> GetAccountShares(ProtectedOperationContext secCtx, int accountId, bool needMoneyShares)
        {
            throw new NotImplementedException();
        }

        public RequestStatus GetUserOwnAndSharedAccounts(string login, ProtectedOperationContext secCtx, out List<AccountShared> accounts)
        {
            throw new NotImplementedException();
        }

        public TransfersByAccountSummary GetTransfersSummary(ProtectedOperationContext secCtx, string login)
        {
            throw new NotImplementedException();
        }

        public List<int> GetFreeMagicsPool(int accountId, int poolSize)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Test.Moq
{
    class MockTradeProxy : ITradeSharpServerTrade, ITradeSharpAccount
    {
        private static MockTradeProxy instance;

        public static MockTradeProxy Instance
        {
            get { return instance ?? (instance = new MockTradeProxy()); }
        }

        public virtual bool BindToTradeSignal(ProtectedOperationContext secCtx, string userLogin, 
            TradeSignalCategory cat, out WalletError error)
        {
            error = WalletError.OK;
            return true;
        }

        public virtual List<TradeSignalCategory> GetTradeSignalsSubscribed(string userLogin)
        {
            return userLogin == "Demo"
                        ? new List<TradeSignalCategory>
                            {
                                new TradeSignalCategory
                                    {
                                        Id = 1,
                                        Title = "Test signals",
                                        PercentLeverage = 50,
                                        MaxLeverage = 10,
                                        MinVolume = 10000,
                                        StepVolume = 5000,
                                        RightsMask = TradeSignalCategory.SubscriberFlag | TradeSignalCategory.TradeAutoFlag
                                    }
                            }
                        : new List<TradeSignalCategory>();
        }

        #region Empty ones
        public void Ping()
        {
            throw new NotImplementedException();
        }

        public AuthenticationResponse GetUserAccounts(string login, ProtectedOperationContext ctx, out List<int> accounts, out List<AccountRights> roles)
        {
            throw new NotImplementedException();
        }

        public AuthenticationResponse GetUserAccountsWithDetail(string login, ProtectedOperationContext secCtx, out List<Account> accounts)
        {
            throw new NotImplementedException();
        }

        public AuthenticationResponse GetUserOwnedAccountsWithActualBalance(string login, ProtectedOperationContext secCtx,
                                                                            bool realOnly, out List<Account> accounts)
        {
            throw new NotImplementedException();
        }

        public bool SelectAccount(ProtectedOperationContext ctx, int accountId)
        {
            throw new NotImplementedException();
        }

        public AuthenticationResponse GetUserDetail(string login, string password, out PlatformUser user)
        {
            throw new NotImplementedException();
        }

        public AuthenticationResponse ModifyUserAndAccount(string oldLogin, string oldPassword, PlatformUser user, int? accountId,
                                                           float accountMaxLeverage, out bool loginIsBusy)
        {
            throw new NotImplementedException();
        }

        public RequestStatus GetAccountInfo(int accountId, bool needEquityInfo, out Account account)
        {
            throw new NotImplementedException();
        }

        public RequestStatus GetMarketOrders(int accountId, out List<MarketOrder> orders)
        {
            throw new NotImplementedException();
        }

        public RequestStatus GetHistoryOrdersCompressed(int? accountId, DateTime? startDate, out byte[] buffer)
        {
            throw new NotImplementedException();
        }

        public RequestStatus GetHistoryOrders(int? accountId, DateTime? startDate, out List<MarketOrder> orders)
        {
            throw new NotImplementedException();
        }

        public RequestStatus GetHistoryOrdersByCloseDate(int? accountId, DateTime? startDate, out List<MarketOrder> orders)
        {
            throw new NotImplementedException();
        }

        public RequestStatus GetPendingOrders(int accountId, out List<PendingOrder> orders)
        {
            throw new NotImplementedException();
        }

        public List<int> GetAccountChannelIDs(int accountId)
        {
            throw new NotImplementedException();
        }

        public RequestStatus GetBalanceChanges(int accountId, DateTime? startTime, out List<BalanceChange> balanceChanges)
        {
            throw new NotImplementedException();
        }

        public RequestStatus ChangeBalance(int accountId, decimal summ, string comment)
        {
            throw new NotImplementedException();
        }

        public AccountRegistrationStatus RegisterAccount(PlatformUser user, string accountCurrency, int startBalance,
                                                         decimal maxLeverage, string completedPassword, bool autoSignIn)
        {
            throw new NotImplementedException();
        }

        public bool RemindPassword(string email, string login)
        {
            throw new NotImplementedException();
        }

        public AuthenticationResponse ChangePassword(ProtectedOperationContext ctx, string login, string newPassword)
        {
            throw new NotImplementedException();
        }

        public List<TradeSignalCategory> GetAllTradeSignals()
        {
            throw new NotImplementedException();
        }        

        public List<UserEvent> GetUserEvents(ProtectedOperationContext ctx, string userLogin, bool deleteReceivedEvents)
        {
            throw new NotImplementedException();
        }

        public void SendTradeSignalEvent(ProtectedOperationContext ctx, int accountId, int tradeSignalCategory, UserEvent acEvent)
        {
            throw new NotImplementedException();
        }

        public CreateReadonlyUserRequestStatus MakeOrDeleteReadonlyUser(ProtectedOperationContext secCtx, int accountId, bool makeNew,
                                                                        string pwrd, out PlatformUser user)
        {
            throw new NotImplementedException();
        }

        public RequestStatus QueryReadonlyUserForAccount(ProtectedOperationContext secCtx, int accountId, out PlatformUser user)
        {
            throw new NotImplementedException();
        }

        public AuthenticationResponse Authenticate(string login, string hash, string terminalVersion, long terminalId, long clientTime,
                                                   out int sessionTag)
        {
            throw new NotImplementedException();
        }

        public void Logout(ProtectedOperationContext ctx)
        {
            throw new NotImplementedException();
        }

        public void ReviveChannel(ProtectedOperationContext ctx, string login, int accountId, string terminalVersion)
        {
            throw new NotImplementedException();
        }

        public RequestStatus SendNewOrderRequest(ProtectedOperationContext ctx, int requestUniqueId, MarketOrder order,
                                                 OrderType orderType, decimal requestedPrice, decimal slippagePoints)
        {
            throw new NotImplementedException();
        }

        public RequestStatus SendNewPendingOrderRequest(ProtectedOperationContext ctx, int requestUniqueId, PendingOrder order)
        {
            throw new NotImplementedException();
        }

        public RequestStatus SendCloseRequest(ProtectedOperationContext ctx, int accountId, int orderId, PositionExitReason reason)
        {
            throw new NotImplementedException();
        }

        public RequestStatus SendDeletePendingOrderRequest(ProtectedOperationContext ctx, PendingOrder order, PendingOrderStatus status,
                                                           int? positionID, string closeReason)
        {
            throw new NotImplementedException();
        }

        public RequestStatus SendEditMarketRequest(ProtectedOperationContext secCtx, MarketOrder ord)
        {
            throw new NotImplementedException();
        }

        public RequestStatus SendEditPendingRequest(ProtectedOperationContext secCtx, PendingOrder ord)
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, List<MarketOrder>> GetSignallersOpenTrades(int accountId)
        {
            throw new NotImplementedException();
        }

        public RequestStatus SubscribeOnPortfolio(ProtectedOperationContext secCtx, string subscriberLogin,
            TopPortfolio portfolio, decimal? maxFee)
        {
            throw new NotImplementedException();
        }

        public RequestStatus UnsubscribePortfolio(ProtectedOperationContext secCtx,
                                                  string subscriberLogin, bool deletePortfolio, bool deleteSubscriptions)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;

namespace TradeSharp.AdminSite.Test.MOQ
{
    class TradeSharpServerTradeMock : ITradeSharpServerTrade
    {
        private List<MarketOrder> allOrders;

        public TradeSharpServerTradeMock()
        {
            MakeOrders();
        }

        /// <summary>
        /// Заполняет список allOrders позициями (сделками) из БД
        /// </summary>
        private void MakeOrders()
        {
            allOrders = new List<MarketOrder>();
            using (var ctx = DatabaseContext.Instance.Make())
            {
                foreach (var pos in ctx.POSITION)
                    allOrders.Add(LinqToEntity.DecorateOrder(pos));
                foreach (var pos in ctx.POSITION_CLOSED)
                    allOrders.Add(LinqToEntity.DecorateOrder(pos));
            }
        }

        #region ITradeSharpServerTrade

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

        public RequestStatus SendCloseByTickerRequest(ProtectedOperationContext ctx, int accountId, string ticker,
                                                      PositionExitReason reason)
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

        public RequestStatus ApplyPortfolioTradeSettings(ProtectedOperationContext secCtx, string login, AutoTradeSettings sets)
        {
            throw new NotImplementedException();
        }

        public RequestStatus UnsubscribePortfolio(ProtectedOperationContext secCtx, string subscriberLogin, bool deletePortfolio,
                                                  bool deleteSubscriptions)
        {
            throw new NotImplementedException();
        }

        public void Ping()
        {
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

        public AuthenticationResponse ChangePassword(ProtectedOperationContext ctx, string login, string newPassword)
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

        public bool SubscribeOnService(ProtectedOperationContext secCtx, string login, int serviceId, bool renewAuto, bool unsubscribe,
                                       AutoTradeSettings tradeSets, out WalletError error)
        {
            throw new NotImplementedException();
        }

        public RequestStatus SubscribeOnPortfolio(ProtectedOperationContext secCtx, string subscriberLogin, TopPortfolio portfolio,
                                                  decimal? maxFee, AutoTradeSettings tradeSets)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

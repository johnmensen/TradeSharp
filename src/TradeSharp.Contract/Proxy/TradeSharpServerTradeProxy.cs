using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;
using TradeSharp.Util.Serialization;

namespace TradeSharp.Contract.Proxy
{
    //non-stable connetivity
    public class TradeSharpServerTradeProxyNs : DuplexClientBase<ITradeSharpServerTrade>, ITradeSharpServerTrade, IDisposable
    {
        public TradeSharpServerTradeProxyNs(string endpointName, ITradeSharpServerCallback callback)
            : base(callback, endpointName)
        {
        }

        public void Dispose()
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                //Logger.Error("Error disposing TradeSharpServerTradeProxyNs", ex);
            }
        }

        #region ITradeSharpServerTrade

        public AuthenticationResponse Authenticate(string login, string hash, string terminalVersion, long terminalId, long clientTime, out int sessionTag)
        {
            try
            {
                return Channel.Authenticate(login, hash, terminalVersion, terminalId, clientTime, out sessionTag);
            }
            catch (System.Xml.XmlException ex)
            {
                Logger.Error("Authenticate() error", ex);
                sessionTag = 0;
                return AuthenticationResponse.ServerError;            
            }
        }

        public void Logout(ProtectedOperationContext ctx)
        {
            Channel.Logout(ctx);
        }

        public void ReviveChannel(ProtectedOperationContext ctx, string login, int accountId, string terminalVersion)
        {
            Channel.ReviveChannel(ctx, login, accountId, terminalVersion);
        }

        public RequestStatus SendNewOrderRequest(ProtectedOperationContext secCtx,
            int requestUniqueId,
            MarketOrder order,
            OrderType orderType,
            decimal requestedPrice,
            decimal slippagePoints)
        {
            return Channel.SendNewOrderRequest(secCtx, requestUniqueId, order,
                orderType, requestedPrice, slippagePoints);
        }

        public RequestStatus SendNewPendingOrderRequest(ProtectedOperationContext ctx,
            int requestUniqueId, PendingOrder order)
        {
            return Channel.SendNewPendingOrderRequest(ctx, requestUniqueId, order);
        }

        public RequestStatus SendCloseRequest(ProtectedOperationContext ctx, int accountId, int orderId, PositionExitReason reason)
        {
            return Channel.SendCloseRequest(ctx, accountId, orderId, reason);
        }

        public RequestStatus SendCloseByTickerRequest(ProtectedOperationContext ctx, int accountId, string ticker,
                                               PositionExitReason reason)
        {
            return Channel.SendCloseByTickerRequest(ctx, accountId, ticker, reason);
        }

        public RequestStatus SendDeletePendingOrderRequest(ProtectedOperationContext secCtx, PendingOrder order, PendingOrderStatus status, int? positionID, string closeReason)
        {
            return Channel.SendDeletePendingOrderRequest(secCtx, order, status, positionID, closeReason);
        }

        public RequestStatus SendEditMarketRequest(ProtectedOperationContext secCtx, MarketOrder ord)
        {
            return Channel.SendEditMarketRequest(secCtx, ord);
        }

        public RequestStatus SendEditPendingRequest(ProtectedOperationContext secCtx, PendingOrder ord)
        {
            return Channel.SendEditPendingRequest(secCtx, ord);
        }

        #endregion

        #region ITradeSharpAccount

        public AuthenticationResponse GetUserAccounts(string login, ProtectedOperationContext ctx, out List<int> accounts, out List<AccountRights> roles)
        {
            return Channel.GetUserAccounts(login, ctx, out accounts, out roles);
        }

        public AuthenticationResponse GetUserAccountsWithDetail(string login, ProtectedOperationContext secCtx, out List<Account> accounts)
        {
            return Channel.GetUserAccountsWithDetail(login, secCtx, out accounts);
        }

        public AuthenticationResponse GetUserOwnedAccountsWithActualBalance(string login,
                                                                            ProtectedOperationContext secCtx,
                                                                            bool realOnly, out List<Account> accounts)
        {
            return Channel.GetUserOwnedAccountsWithActualBalance(login, secCtx, realOnly, out accounts);
        }

        public bool SelectAccount(ProtectedOperationContext ctx, int accountId)
        {
            return Channel.SelectAccount(ctx, accountId);
        }

        public AuthenticationResponse GetUserDetail(string login, string password, out PlatformUser user)
        {
            return Channel.GetUserDetail(login, password, out user);
        }

        public AuthenticationResponse ModifyUserAndAccount(string oldLogin, string oldPassword, PlatformUser user, int? accountId, float accountMaxLeverage, out bool loginIsBusy)
        {
            return Channel.ModifyUserAndAccount(oldLogin, oldPassword, user, accountId, accountMaxLeverage,
                                                     out loginIsBusy);
        }

        public AuthenticationResponse ChangePassword(ProtectedOperationContext ctx, string login, string newPassword)
        {
            return Channel.ChangePassword(ctx, login, newPassword);
        }

        public List<UserEvent> GetUserEvents(ProtectedOperationContext ctx, string userLogin, bool deleteReceivedEvents)
        {
            return Channel.GetUserEvents(ctx, userLogin, deleteReceivedEvents);
        }

        public void SendTradeSignalEvent(ProtectedOperationContext ctx, int accountId, int tradeSignalCategory, UserEvent acEvent)
        {
            Channel.SendTradeSignalEvent(ctx, accountId, tradeSignalCategory, acEvent);
        }

        public void Ping()
        {
            Channel.Ping();
        }

        public CreateReadonlyUserRequestStatus MakeOrDeleteReadonlyUser(ProtectedOperationContext secCtx,
                                                                       int accountId, bool makeNew, string pwrd,
                                                                       out PlatformUser user)
        {
            return Channel.MakeOrDeleteReadonlyUser(secCtx, accountId, makeNew, pwrd, out user);
        }

        public RequestStatus QueryReadonlyUserForAccount(ProtectedOperationContext secCtx, int accountId, out PlatformUser user)
        {
            return Channel.QueryReadonlyUserForAccount(secCtx, accountId, out user);
        }

        public bool SubscribeOnService(ProtectedOperationContext secCtx, string login, int serviceId, bool renewAuto, bool unsubscribe,
                                       AutoTradeSettings tradeSets, out WalletError error)
        {
            return Channel.SubscribeOnService(secCtx, login, serviceId, renewAuto, unsubscribe, tradeSets, out error);
        }

        public RequestStatus SubscribeOnPortfolio(ProtectedOperationContext secCtx, string subscriberLogin,
            TopPortfolio portfolio, decimal? maxFee, AutoTradeSettings tradeAutoSettings)
        {
            return Channel.SubscribeOnPortfolio(secCtx, subscriberLogin, portfolio, maxFee, tradeAutoSettings);
        }

        public RequestStatus ApplyPortfolioTradeSettings(ProtectedOperationContext secCtx, string login, AutoTradeSettings sets)
        {
            return Channel.ApplyPortfolioTradeSettings(secCtx, login, sets);
        }

        public RequestStatus UnsubscribePortfolio(ProtectedOperationContext secCtx,
                                                  string subscriberLogin, bool deletePortfolio, bool deleteSubscriptions)
        {
            return Channel.UnsubscribePortfolio(secCtx, subscriberLogin, deletePortfolio, deleteSubscriptions);
        }

        #endregion
    }

    //stable connetivity
    public class TradeSharpServerTradeProxy : ITradeSharpServerTrade, IDisposable
    {
        private TradeSharpServerTradeProxyNs proxyNs;
        private readonly string endpointName;
        private readonly ITradeSharpServerCallback callback;
        private Action onConnectionReestablished;
        /// <summary>
        /// количество вызовов (в процессе) ReviveChannel(...)
        /// обновляется атомарно
        /// </summary>
        private long revivesCount;

        public static ITradeSharpServerTrade fakeProxy;

        private ITradeSharpServerTrade Proxy
        {
            get { return fakeProxy ?? proxyNs; }
        }

        public event Action OnConnectionReestablished
        {
            add { onConnectionReestablished += value; }
            remove { onConnectionReestablished -= value; }
        }

        public TradeSharpServerTradeProxy(string endpointName, ITradeSharpServerCallback callback)
        {
            this.endpointName = endpointName;
            this.callback = callback;
            RenewChannel();

            // сериализаторы
            SerializationWriter.TypeSurrogates.Add(new MarketOrderSerializer());
        }

        public void Abort()
        {
            try
            {
                if (fakeProxy == null)
                    proxyNs.Abort();

            }
            catch (Exception ex)
            {
                Logger.Error("TradeSharpServerTradeProxy.Abort()", ex);
            }
        }

        private void RenewChannel()
        {
            try
            {
                if (fakeProxy != null) return;
                if (proxyNs != null)
                    proxyNs.Dispose();
                proxyNs = new TradeSharpServerTradeProxyNs(endpointName, callback);
                if (onConnectionReestablished != null)
                    onConnectionReestablished();
            }
            catch (Exception ex)
            {
                Logger.Error("TradeSharpServerTradeProxy: невозможно создать прокси", ex);
            }
        }

        #region ITradeSharpServerTrade

        public AuthenticationResponse Authenticate(string login, string hash, string terminalVersion, long terminalId, long clientTime, out int sessionTag)
        {
            try
            {
                return Proxy.Authenticate(login, hash, terminalVersion, terminalId, clientTime, out sessionTag);
            }
            catch (Exception)
            {
                RenewChannel();
                sessionTag = 0;
                try //no try-catch in original
                {
                    return Proxy.Authenticate(login, hash, terminalVersion, terminalId, clientTime, out sessionTag);
                }
                catch (Exception)
                {
                    return AuthenticationResponse.ServerError; //new result
                }
            }
        }

        public void Logout(ProtectedOperationContext ctx)
        {
            try
            {
                Proxy.Logout(ctx);
            }
            catch (Exception)
            {
                RenewChannel();
                Proxy.Logout(ctx); //wait communication exception
            }
        }

        public void ReviveChannel(ProtectedOperationContext ctx, string login, int accountId, string terminalVersion)
        {
            // уже был сделан вызов?
            if (Interlocked.CompareExchange(ref revivesCount, 1, 0) == 1) return;
            try
            {
                Proxy.ReviveChannel(ctx, login, accountId, terminalVersion);
            }
            catch (Exception)
            {
                RenewChannel();
                try
                {
                    Proxy.ReviveChannel(ctx, login, accountId, terminalVersion); //wait communication exception
                }
                catch
                {
                }
            }
            finally
            {
                Interlocked.CompareExchange(ref revivesCount, 0, 1);
            }
        }

        public RequestStatus SendNewOrderRequest(ProtectedOperationContext secCtx,
            int requestUniqueId,
            MarketOrder order,
            OrderType orderType,
            decimal requestedPrice,
            decimal slippagePoints)
        {
            try
            {
                return Proxy.SendNewOrderRequest(secCtx, requestUniqueId, order, orderType,
                    requestedPrice, slippagePoints);
            }
            catch (Exception)
            {
                RenewChannel();
                try
                {
                    return Proxy.SendNewOrderRequest(secCtx, requestUniqueId, order, orderType,
                        requestedPrice, slippagePoints);
                }
                catch (Exception)
                {
                    return RequestStatus.ServerError;
                }
            }
        }

        public RequestStatus SendNewPendingOrderRequest(ProtectedOperationContext ctx,
            int requestUniqueId, PendingOrder order)
        {
            try
            {
                return Proxy.SendNewPendingOrderRequest(ctx, requestUniqueId, order);
            }
            catch (Exception)
            {
                RenewChannel();
                try
                {
                    return Proxy.SendNewPendingOrderRequest(ctx, requestUniqueId, order);
                }
                catch (Exception)
                {
                    return RequestStatus.ServerError;
                }
            }
        }

        public RequestStatus SendCloseRequest(ProtectedOperationContext ctx,
            int accountId, int orderId, PositionExitReason reason)
        {
            try
            {
                return Proxy.SendCloseRequest(ctx, accountId, orderId, reason);
            }
            catch (Exception ex)
            {
                RenewChannel();
                try
                {
                    return Proxy.SendCloseRequest(ctx, accountId, orderId, reason);
                }
                catch (Exception ex2)
                {
                    return RequestStatus.ServerError;
                }
            }
        }

        public RequestStatus SendCloseByTickerRequest(ProtectedOperationContext ctx, int accountId, string ticker,
                                               PositionExitReason reason)
        {
            try
            {
                return Proxy.SendCloseByTickerRequest(ctx, accountId, ticker, reason);
            }
            catch (Exception ex)
            {
                RenewChannel();
                try
                {
                    return Proxy.SendCloseByTickerRequest(ctx, accountId, ticker, reason);
                }
                catch (Exception ex2)
                {
                    return RequestStatus.ServerError;
                }
            }
        }

        public RequestStatus SendDeletePendingOrderRequest(ProtectedOperationContext secCtx, PendingOrder order, PendingOrderStatus status, int? positionID, string closeReason)
        {
            try
            {
                return Proxy.SendDeletePendingOrderRequest(secCtx, order, status, positionID, closeReason);
            }
            catch (Exception) //no try-catch in original
            {
                RenewChannel();
                try
                {
                    return Proxy.SendDeletePendingOrderRequest(secCtx, order, status, positionID, closeReason);
                }
                catch (Exception)
                {
                    return RequestStatus.ServerError;
                }
            }
        }

        public RequestStatus SendEditMarketRequest(ProtectedOperationContext secCtx, MarketOrder ord)
        {
            try
            {
                return Proxy.SendEditMarketRequest(secCtx, ord);
            }
            catch (Exception) //no try-catch in original
            {
                RenewChannel();
                try
                {
                    return Proxy.SendEditMarketRequest(secCtx, ord);
                }
                catch (Exception)
                {
                    return RequestStatus.ServerError;
                }
            }
        }

        public RequestStatus SendEditPendingRequest(ProtectedOperationContext secCtx, PendingOrder ord)
        {
            try
            {
                return Proxy.SendEditPendingRequest(secCtx, ord);
            }
            catch (Exception) //no try-catch in original
            {
                RenewChannel();
                try
                {
                    return Proxy.SendEditPendingRequest(secCtx, ord);
                }
                catch (Exception)
                {
                    return RequestStatus.ServerError;
                }
            }
        }

        #endregion

        #region ITradeSharpAccount

        public AuthenticationResponse GetUserAccounts(string login, ProtectedOperationContext ctx, out List<int> accounts, out List<AccountRights> roles)
        {
            try
            {
                return Proxy.GetUserAccounts(login, ctx, out accounts, out roles);
            }
            catch (Exception)
            {
                RenewChannel();
                accounts = new List<int>();
                roles = new List<AccountRights>();
                try //no try-catch in original
                {
                    return Proxy.GetUserAccounts(login, ctx, out accounts, out roles);
                }
                catch (Exception)
                {
                    return AuthenticationResponse.ServerError;
                }
            }
        }

        public AuthenticationResponse GetUserAccountsWithDetail(string login, ProtectedOperationContext secCtx, out List<Account> accounts)
        {
            try
            {
                return Proxy.GetUserAccountsWithDetail(login, secCtx, out accounts);
            }
            catch (Exception)
            {
                RenewChannel();
                accounts = new List<Account>();
                try
                {
                    return Proxy.GetUserAccountsWithDetail(login, secCtx, out accounts);
                }
                catch (Exception)
                {
                    return AuthenticationResponse.ServerError;
                }
            }
        }

        public AuthenticationResponse GetUserOwnedAccountsWithActualBalance(string login,
                                                                            ProtectedOperationContext secCtx,
                                                                            bool realOnly, out List<Account> accounts)
        {
            try
            {
                return Proxy.GetUserOwnedAccountsWithActualBalance(login, secCtx, realOnly, out accounts);
            }
            catch (Exception)
            {
                RenewChannel();
                accounts = new List<Account>();
                try
                {
                    return Proxy.GetUserOwnedAccountsWithActualBalance(login, secCtx, realOnly, out accounts);
                }
                catch (Exception)
                {
                    return AuthenticationResponse.ServerError;
                }
            }
        }

        public bool SelectAccount(ProtectedOperationContext ctx, int accountId)
        {
            try
            {
                return Proxy.SelectAccount(ctx, accountId);
            }
            catch (Exception)
            {
                RenewChannel();
                try //no try-catch in original
                {
                    return Proxy.SelectAccount(ctx, accountId);
                }
                catch (Exception ex)
                {
                    Logger.Error("SelectAccount", ex);
                    return false; //new result
                }
            }
        }

        public AuthenticationResponse GetUserDetail(string login, string password, out PlatformUser user)
        {
            user = null;
            try
            {
                return Proxy.GetUserDetail(login, password, out user);
            }
            catch (Exception)
            {
                RenewChannel();
                try //no try-catch in original
                {
                    return Proxy.GetUserDetail(login, password, out user);
                }
                catch (Exception)
                {
                    return AuthenticationResponse.ServerError;
                }
            }
        }

        public AuthenticationResponse ModifyUserAndAccount(string oldLogin, string oldPassword, PlatformUser user, int? accountId, float accountMaxLeverage, out bool loginIsBusy)
        {
            loginIsBusy = false;
            try
            {
                return Proxy.ModifyUserAndAccount(oldLogin, oldPassword, user, accountId, accountMaxLeverage, out loginIsBusy);
            }
            catch (Exception)
            {
                RenewChannel();
                try
                {
                    return Proxy.ModifyUserAndAccount(oldLogin, oldPassword, user, accountId, accountMaxLeverage, out loginIsBusy);
                }
                catch (Exception)
                {
                    return AuthenticationResponse.ServerError;
                }
            }
        }

        public AuthenticationResponse ChangePassword(ProtectedOperationContext ctx, string login, string newPassword)
        {
            try
            {
                return Proxy.ChangePassword(ctx, login, newPassword);
            }
            catch (Exception)
            {
                RenewChannel();
                try //no try-catch in original
                {
                    return Proxy.ChangePassword(ctx, login, newPassword);
                }
                catch (Exception)
                {
                    return AuthenticationResponse.ServerError; //new result
                }
            }
        }

        public List<UserEvent> GetUserEvents(ProtectedOperationContext ctx, string userLogin, bool deleteReceivedEvents)
        {
            try
            {
                return Proxy.GetUserEvents(ctx, userLogin, deleteReceivedEvents);
            }
            catch (Exception)
            {
                RenewChannel();
                try //no try-catch in original
                {
                    return Proxy.GetUserEvents(ctx, userLogin, deleteReceivedEvents);
                }
                catch (Exception)
                {
                    return null; //new result
                }
            }
        }

        public void SendTradeSignalEvent(ProtectedOperationContext ctx, int accountId, int tradeSignalCategory, UserEvent acEvent)
        {
            try
            {
                Proxy.SendTradeSignalEvent(ctx, accountId, tradeSignalCategory, acEvent);
            }
            catch (Exception)
            {
                RenewChannel();
                Proxy.SendTradeSignalEvent(ctx, accountId, tradeSignalCategory, acEvent);
            }
        }        

        public void Ping()
        {
            Proxy.Ping();
        }

        public CreateReadonlyUserRequestStatus MakeOrDeleteReadonlyUser(ProtectedOperationContext secCtx,
                                                                       int accountId, bool makeNew, string pwrd,
                                                                       out PlatformUser user)
        {
            try
            {
                return Proxy.MakeOrDeleteReadonlyUser(secCtx, accountId, makeNew, pwrd, out user);
            }
            catch (Exception)
            {
                RenewChannel();
                return Proxy.MakeOrDeleteReadonlyUser(secCtx, accountId, makeNew, pwrd, out user);
            }
        }

        public RequestStatus QueryReadonlyUserForAccount(ProtectedOperationContext secCtx, int accountId,
                                                         out PlatformUser user)
        {
            try
            {
                return Proxy.QueryReadonlyUserForAccount(secCtx, accountId, out user);
            }
            catch (Exception)
            {
                RenewChannel();
                return Proxy.QueryReadonlyUserForAccount(secCtx, accountId, out user);
            }
        }

        public bool SubscribeOnService(ProtectedOperationContext secCtx, string login, int serviceId, bool renewAuto, bool unsubscribe,
                                       AutoTradeSettings tradeSets, out WalletError error)
        {
            try
            {
                return Proxy.SubscribeOnService(secCtx, login, serviceId, renewAuto, unsubscribe, tradeSets, out error);
            }
            catch (Exception)
            {
                RenewChannel();
                return Proxy.SubscribeOnService(secCtx, login, serviceId, renewAuto, unsubscribe, tradeSets, out error);
            }
        }

        public RequestStatus SubscribeOnPortfolio(ProtectedOperationContext secCtx, string subscriberLogin,
            TopPortfolio portfolio, decimal? maxFee, AutoTradeSettings tradeAutoSettings)
        {
            try
            {
                return Proxy.SubscribeOnPortfolio(secCtx, subscriberLogin, portfolio, maxFee, tradeAutoSettings);
            }
            catch (Exception)
            {
                RenewChannel();
                return Proxy.SubscribeOnPortfolio(secCtx, subscriberLogin, portfolio, maxFee, tradeAutoSettings);
            }
        }

        public RequestStatus ApplyPortfolioTradeSettings(ProtectedOperationContext secCtx, string login, AutoTradeSettings sets)
        {
            try
            {
                return Proxy.ApplyPortfolioTradeSettings(secCtx, login, sets);
            }
            catch (Exception)
            {
                RenewChannel();
                return Proxy.ApplyPortfolioTradeSettings(secCtx, login, sets);
            }
        }

        public RequestStatus UnsubscribePortfolio(ProtectedOperationContext secCtx,
                                                  string subscriberLogin, bool deletePortfolio, bool deleteSubscriptions)
        {
            try
            {
                return Proxy.UnsubscribePortfolio(secCtx, subscriberLogin, deletePortfolio, deleteSubscriptions);
            }
            catch (Exception)
            {
                RenewChannel();
                return Proxy.UnsubscribePortfolio(secCtx, subscriberLogin, deletePortfolio, deleteSubscriptions);
            }
        }

        #endregion
 
        public void Dispose()
        {
            if (fakeProxy == null)
                proxyNs.Close();
        }
    }
}

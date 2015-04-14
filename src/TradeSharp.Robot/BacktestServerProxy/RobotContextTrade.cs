using System;
using System.Collections.Generic;
using System.Linq;
using Entity;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.TradeLib;
using TradeSharp.Util;

namespace TradeSharp.Robot.BacktestServerProxy
{
    // торговые функции
    public partial class RobotContext : ITradeSharpServerTrade, ITradeSharpAccount
    {
        public enum ContextMode
        {
            Testing = 1,
            Realtime = 2
        }

        // режим работы роботов
        public ContextMode robotContextMode = ContextMode.Testing;

        public abstract Account AccountInfo { get; set; }

        #region счетчик ордеров ID
        protected int counterId;
        /// <summary>
        /// счетчик ID
        /// </summary>
        public int NewId
        {
            get { return counterId++; }
        }

        public int CurrentId
        {
            set { if (value >= 0) { counterId = value; } }
            get { return counterId; }
        }
        #endregion

        protected TradeManager tradeManager;

        protected IProfitCalculator profitCalculator;

        public virtual AccountGroup GetAccountGroup(int accountId)
        {
            var accInfo = GetAccountInfo(false);
            if (accInfo == null) return null;
            if (string.IsNullOrEmpty(accInfo.Group)) return null;
            if (DalAccountGroup.Instance == null) return null;
            var groups = DalAccountGroup.Instance.Groups;
            return groups.FirstOrDefault(gr => gr.Code == accInfo.Group);
        }

        public abstract Account GetAccountInfo(bool needEquity);

        public AuthenticationResponse GetUserAccounts(string login, ProtectedOperationContext ctx,
            out List<int> accounts, out List<AccountRights> roles)
        {
            accounts = new List<int> { GetAccountInfo(false).ID };
            roles = new List<AccountRights> { AccountRights.Управление };
            return AuthenticationResponse.OK;
        }

        public AuthenticationResponse GetUserAccountsWithDetail(string login, ProtectedOperationContext secCtx,
                                                         out List<Account> accounts)
        {
            accounts = new List<Account>
                           {
                               new Account { ID = GetAccountInfo(false).ID }
                           };
            return AuthenticationResponse.OK;
        }

        public AuthenticationResponse GetUserOwnedAccountsWithActualBalance(string login, ProtectedOperationContext secCtx,
                                                                            bool realOnly, out List<Account> accounts)
        {
            accounts = new List<Account>
                           {
                               new Account { ID = GetAccountInfo(false).ID }
                           };
            return AuthenticationResponse.OK;
        }

        public bool SelectAccount(ProtectedOperationContext ctx, int accountId)
        {
            return true;
        }

        public List<PaidService> GetUserOwnedPaidServices(string userLogin)
        {
            throw new NotImplementedException();
        }

        public List<Subscription> GetSubscriptions(string userLogin)
        {
            throw new NotImplementedException();
        }

        public RequestStatus GetAccountInfo(int accountId, bool needEquity, out Account account)
        {
            if (accountId == GetAccountInfo(false).ID)
            {
                account = GetAccountInfo(needEquity);
                return RequestStatus.OK;
            }
            Logger.ErrorFormat("GetAccountInfo({0}) - ID missmatch ({1})",
                accountId, GetAccountInfo(false).ID);
            account = null;
            return RequestStatus.NotFound;
        }

        public abstract RequestStatus GetMarketOrders(int accountId, out List<MarketOrder> ordlist);

        public abstract RequestStatus GetHistoryOrdersCompressed(int? accountId,
            DateTime? startDate, out byte[] buffer);

        public abstract RequestStatus GetHistoryOrders(int? accountId, DateTime? startDate,
                                                       out List<MarketOrder> ordlist);

        public abstract RequestStatus GetHistoryOrdersByCloseDate(int? accountId, DateTime? startDate,
                                                       out List<MarketOrder> ordlist);

        public abstract RequestStatus GetPendingOrders(int accountId, out List<PendingOrder> ordlist);

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

        public AccountRegistrationStatus RegisterAccount(PlatformUser user, string accountCurrency,
            int startBalance, decimal maxLeverage, string completedPassword, bool autoSignIn)
        {
            return AccountRegistrationStatus.ServerError;
        }

        public bool RemindPassword(string email, string login)
        {
            return false;
        }

        public Dictionary<int, List<MarketOrder>> GetSignallersOpenTrades(int accountId)
        {
            throw new NotImplementedException();
        }

        public AuthenticationResponse ChangePassword(ProtectedOperationContext ctx, string login, string newPassword)
        {
            return AuthenticationResponse.ServerError;
        }

        public bool RemindPassword(string email)
        {
            return false;
        }

        public AuthenticationResponse Authenticate(
            string login, string hash, string terminalVersion,
            long terminalId, long clientTime,
            out int sessionTag)
        {
            sessionTag = 0;
            return AuthenticationResponse.OK;
        }

        public void Logout(ProtectedOperationContext ctx)
        {
            return;
        }

        public void ReviveChannel(ProtectedOperationContext ctx, string login, int accountId, string terminalVersion)
        {
            return;
        }

        public bool SubscribeOnService(ProtectedOperationContext secCtx, string login, int serviceId, bool renewAuto, bool unsubscribe,
                                       AutoTradeSettings tradeSets, out WalletError error)
        {
            error = WalletError.CommonError;
            return false;
        }

        public RequestStatus SubscribeOnPortfolio(ProtectedOperationContext secCtx, string subscriberLogin,
            TopPortfolio portfolio, decimal? maxFee, AutoTradeSettings tradeAutoSettings)
        {
            return RequestStatus.CommonError;
        }

        public RequestStatus UnsubscribePortfolio(ProtectedOperationContext secCtx,
                                                  string subscriberLogin, bool deletePortfolio, bool deleteSubscriptions)
        {
            return RequestStatus.CommonError;
        }
    }
}

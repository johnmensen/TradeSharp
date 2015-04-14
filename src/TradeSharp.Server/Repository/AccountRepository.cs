using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.Server.Repository
{
    class AccountRepository : IAccountRepository
    {
        private static readonly Lazy<IAccountRepository> instance = new Lazy<IAccountRepository>(() => new AccountRepository());

        public static IAccountRepository Instance
        {
            get { return instance.Value; }
        }

        private readonly ThreadSafeUpdatingList<AccountGroup> accountGroupsList;
        
        private readonly ThreadSafeUpdatingList<Account> accountsList;
        
        private readonly ThreadSafeUpdatingList<CHANNEL> channelsList;

        // ReSharper disable InconsistentNaming
        private static readonly int updateIntervalMils = AppConfig.GetIntParam(
            "Dictionary.UpdateIntervalMils", 1000 * 15);

        private static readonly int updateFastIntervalMils = AppConfig.GetIntParam(
            "Dictionary.UpdateIntervalMils", 1000 * 5);

        private static readonly int lockIntervalMils = AppConfig.GetIntParam(
            "Dictionary.LockIntervalMils", 1000);

        private static readonly int deleteAccountEventsAfterDays = AppConfig.GetIntParam(
            "AccountEvents.DaysToStore", 12);
        // ReSharper restore InconsistentNaming

        private AccountRepository()
        {
            accountGroupsList = new ThreadSafeUpdatingList<AccountGroup>(lockIntervalMils, updateIntervalMils, AccountGroupsUpdateRoutine);
            accountsList = new ThreadSafeUpdatingList<Account>(lockIntervalMils, updateFastIntervalMils, AccountsUpdateRoutine);
            channelsList = new ThreadSafeUpdatingList<CHANNEL>(lockIntervalMils, updateIntervalMils, ChannelsUpdateRoutine);
        }

        public AccountGroup GetAccountGroup(int accountId)
        {
            try
            {
                var accounts = accountsList.GetItems();
                var groups = accountGroupsList.GetItems();
                return (from account in accounts
                        where account.ID == accountId
                        from gr in groups.Where(gr => gr.Code == account.Group)
                        select gr).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка в GetAccountGroup (userLogin={0}): {1}", accountId, ex);
                return null;
            }
        }

        public Account GetAccount(int accountId)
        {
            RequestStatus reqStatus;
            return GetAccount(accountId, out reqStatus);
            //return account == null ? null : LinqToEntity.DecorateAccount(account);
        }

        public Account GetAccount(int accountId, out RequestStatus errorCode)
        {
            var accounts = accountsList.GetItems();
            foreach (var account in accounts)
            {
                if (account.ID != accountId) continue;
                errorCode = RequestStatus.OK;
                return account;
            }
            errorCode = RequestStatus.NotFound;
            return null;
        }

        public List<Account> GetAccounts(out RequestStatus errorCode)
        {
            var accounts = accountsList.GetItems();
            errorCode = RequestStatus.OK;
            return accounts;
        }

        public List<string> GetTickersTradedCheckCredentials(string hash, string userLogin, int accountId,
            long localTime, bool checkCredentials)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    if (checkCredentials)
                    {
                        var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == userLogin);
                        if (user == null) return null;

                        var userHash = CredentialsHash.MakeCredentialsHash(userLogin, user.Password, localTime);
                        if (userHash != hash)
                            return null;

                        if (!ctx.PLATFORM_USER_ACCOUNT.Any(pa => pa.PlatformUser == user.ID && pa.Account == accountId))
                            return null;
                    }

                    return ctx.POSITION.Where(p => p.AccountID == accountId).Select(p => p.Symbol).Distinct().Union(
                        ctx.POSITION_CLOSED.Where(p => p.AccountID == accountId).Select(p => p.Symbol).Distinct()).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в GetTickersTraded()", ex);
                return null;
            }
        }

        public List<MarketOrder> GetClosedOrOpenedOrdersCheckCredentials(string hash, string userLogin, long localTime,
            int accountId, string optionalSymbolFilter, int startId, int maxCount, bool checkCredentials,
            bool needClosedOrders)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    if (checkCredentials)
                    {
                        var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == userLogin);
                        if (user == null) return null;

                        var userHash = CredentialsHash.MakeCredentialsHash(userLogin, user.Password, localTime);
                        if (userHash != hash)
                            return null;

                        if (!ctx.PLATFORM_USER_ACCOUNT.Any(pa => pa.PlatformUser == user.ID && pa.Account == accountId))
                            return null;
                    }

                    return needClosedOrders
                               ? ctx.POSITION_CLOSED.Where(p => p.AccountID == accountId && p.ID > startId &&
                                    (string.IsNullOrEmpty(optionalSymbolFilter) || p.Symbol == optionalSymbolFilter)).OrderBy(p =>
                                        p.ID).Take(maxCount).ToList().Select(LinqToEntity.DecorateOrder).ToList()
                               : ctx.POSITION.Where(p => p.AccountID == accountId && p.ID > startId &&
                                    (string.IsNullOrEmpty(optionalSymbolFilter) || p.Symbol == optionalSymbolFilter)).OrderBy(p =>
                                        p.ID).Take(maxCount).ToList().Select(LinqToEntity.DecorateOrder).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в GetClosedOrOpenedOrdersCheckCredentials()", ex);
                return null;
            }
        }

        public RequestStatus ChangeBalance(int accountId, decimal summ, string comment, DateTime date, BalanceChangeType changeType)
        {
            if (summ == 0) return RequestStatus.BadRequest;
            // корректировать знак
            summ = BalanceChange.CorrectSign(summ, changeType);

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    ACCOUNT acc;
                    try
                    {
                        acc = (from a in ctx.ACCOUNT
                               where a.ID == accountId
                               select a).First();
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorFormat("Ошибка получения счета для редактирования {0}: {1}", accountId, ex);
                        return RequestStatus.ServerError;
                    }

                    var bc = new BALANCE_CHANGE
                    {
                        AccountID = accountId,
                        ValueDate = date,
                        Amount = summ,
                        ChangeType = (int)changeType,
                        Description = comment
                    };
                    try
                    {
                        acc.Balance = acc.Balance + summ;
                        ctx.BALANCE_CHANGE.Add(bc);
                        ctx.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorFormat("Ошибка изменения баланса счета {0}: {1}", accountId, ex);
                        return RequestStatus.ServerError;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в ChangeBalance", ex);
                return RequestStatus.ServerError;
            }
            return RequestStatus.OK;
        }

        private static List<AccountGroup> AccountGroupsUpdateRoutine()
        {
            using (var ctx = DatabaseContext.Instance.Make())
            {
                try
                {
                    var list = new List<AccountGroup>();
                    foreach (var group in ctx.ACCOUNT_GROUP)
                        list.Add(LinqToEntity.DecorateAccountGroup(group));
                    return list;
                }
                catch (Exception ex)
                {
                    Logger.Error("ServerManagerAccount - ошибка в AccountGroupsUpdateRoutine()", ex);
                    return new List<AccountGroup>();
                }
            }
        }

        private static List<CHANNEL> ChannelsUpdateRoutine()
        {
            using (var ctx = DatabaseContext.Instance.Make())
            {
                try
                {
                    return (from c in ctx.CHANNEL select c).ToList();
                }
                catch (Exception ex)
                {
                    Logger.Error("ServerManagerAccount - ошибка в ChannelsUpdateRoutine()", ex);
                    return new List<CHANNEL>();
                }
            }
        }

        private static List<Account> AccountsUpdateRoutine()
        {
            using (var ctx = DatabaseContext.Instance.Make())
            {
                try
                {
                    var list = new List<Account>();
                    foreach (var acc in ctx.ACCOUNT)
                        list.Add(LinqToEntity.DecorateAccount(acc));
                    return list;
                }
                catch (Exception ex)
                {
                    Logger.Error("ServerManagerAccount - ошибка в AccountsUpdateRoutine()", ex);
                    return new List<Account>();
                }
            }
        }        
    }
}

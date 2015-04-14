using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entity;
using TradeSharp.Admin.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Proxy;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Contract.WebContract;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Contract;
using TradeSharp.SiteAdmin.Models;
using TradeSharp.SiteAdmin.Models.Items;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Repository
{
    public class AccountRepository : IAccountRepository
    {
        public List<AccountTag> GetAllAccounts(AccountViewModel model)
        {
            var accounts = GetFilterAccountFromServer(model);
            if (model != null)
            {
                if (model.SortColName != null && model.SortColName.Count > 0)
                {
                    model.SortColName.Reverse();
                    accounts.Sort((a, b) =>
                    {
                        foreach (var field in model.SortColName)
                        {
                            try
                            {
                                var valA = (IComparable)AccountViewModel.sortFieldSelector[field.Key](a);
                                var valB = (IComparable)AccountViewModel.sortFieldSelector[field.Key](b);
                                var order = field.Value * valA.CompareTo(valB);
                                if (order != 0)
                                    return order;
                                //Если элементы равны по первому полю, тогда сравниваем по второму. Если не равны по первому - сравнивать по второму нет смысла
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(
                                    String.Format(
                                        "GetAllAccountsSort() не удалось сравнить элементы типа AccountTag. Возможно тип поля {0} не реализует IComparable",
                                        field.Key), ex);
                            }
                        }
                        return 0;
                    });
                }
            }
            return accounts;
        }

        public List<Account> GetAllAccounts()
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var result = new List<Account>();
                    foreach (var account in ctx.ACCOUNT)
                        result.Add(LinqToEntity.DecorateAccount(account));

                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetAllAccounts()", ex);
                return null;
            }
        }

        public AccountItem GetAccount(int id)
        {
            decimal brokerLeverage;
            AccountItem account;
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var accountDb = ctx.ACCOUNT.FirstOrDefault(x => x.ID == id);
                    account = new AccountItem(accountDb)
                    {
                        CountOpenPosition = ctx.POSITION.Count(x => x.AccountID == id)
                    };

                    brokerLeverage = ctx.ACCOUNT_GROUP.Single(x => x.Code == account.Group).BrokerLeverage;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(String.Format("GetAccount() - Не удалось получить счет {0}", id), ex);
                return null;
            }

            // инициализация "расчётных" параметров счёта таких ка баланс и профит
            try
            {
                List<MarketOrder> marketOrders;

                TradeSharpAccount.Instance.proxy.GetMarketOrders(account.ID, out marketOrders);

                var quoteStorage = new QuoteStorageProxy("IQuoteStorageBinding");
                var currentQuotes = quoteStorage.GetQuoteData(); // актуальные значения катировок на текущий момент (всех катировок, зарегистрированных в терминале)

                account.Equity = account.Balance + (decimal)DalSpot.Instance.CalculateOpenedPositionsCurrentResult(marketOrders,
                    currentQuotes, account.Currency);

                var processingErrors = new List<string>();
                account.Exposure = DalSpot.Instance.CalculateExposure(marketOrders, currentQuotes, account.Currency, processingErrors);
                if (brokerLeverage != 0 && account.Exposure != 0)
                    account.UsedMargin = (account.Exposure / brokerLeverage);
            }
            catch (Exception ex)
            {
                Logger.Error("GetAccount() не удалось расчитать текущий profit по открытым сделкам", ex);
                return null;
            }
            return account;
        }

        public List<Account> GetAccountForUser(int userId)
        {
            var result = new List<Account>();
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var res = ctx.ACCOUNT.Where(x => x.PLATFORM_USER_ACCOUNT.Count(y => y.PlatformUser == userId) > 0);

                    foreach (var account in res)
                        result.Add(LinqToEntity.DecorateAccount(account));
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetAccountForUser", ex);
            }
            return result;
        }

        public bool SaveAccountChanges(Account account)
        {
            Logger.Info(String.Format("попытка сохранить изменения внесённые в счет {0}", account.ID));
            using (var ctx = DatabaseContext.Instance.Make())
            {
                try
                {
                    var undecorateAccount = ctx.ACCOUNT.First(a => a.ID == account.ID);
                    LinqToEntity.UndecorateAccount(undecorateAccount, account);
                    ctx.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Error(String.Format("Не удалось сохранить изменения в счете {0}", account.ID), ex);
                }
                return false;
            }
        }

        public int? GetAccountId(string login)
        {
            using (var ctx = DatabaseContext.Instance.Make())
            {
                var query = from us in ctx.PLATFORM_USER
                            join ac in ctx.PLATFORM_USER_ACCOUNT
                                on us.ID equals ac.PlatformUser
                            where us.Login == login
                            select ac.Account;

                return query.FirstOrDefault();
            }
        }

        public bool AddAccount(AddAccountModel newAccount)
        {
            // В данном методе напрямую с БД не работаем - работаем через прокси
            // Сформированный JSON отправляется на сервер терминала 
            const string queryPtr = "/?formatquery=1&register=1";

            var urlSrv = AppConfig.GetStringParam("Env.TradeSharpServiceUrl", "http://10.5.237.10:8061");
            var userName = AppConfig.GetStringParam("Env.TradeSharpUser", "forexinvest\\asitaev");
            var userPwrd = AppConfig.GetStringParam("Env.TradeSharpPwrd", "AndSit!qa");


            if (newAccount == null)
            {
                Logger.Error("AddAccount. Ошибка добавления аккаунта. Объект модели равен null");
                return false;
            }

            var newAccountParamList = new List<HttpParameter>
                {
                    new RegisterAccountQuery
                        {
                            #region быстрая инициализация свойств
		                    Balance = newAccount.AccountBalance,
                            Group = newAccount.AccountGroup,
                            Currency = newAccount.AccountCurrency,
                            MaxLeverage = newAccount.AccountMaxLeverage,

                            UserLogin = newAccount.UserLogin,
                            UserPassword = newAccount.UserPassword,
                            UserName = newAccount.UserName,
                            UserSurname = newAccount.UserSurname,
                            UserPatronym = newAccount.UserPatronym,         
                            UserDescription = newAccount.UserDescription,
                            UserEmail = newAccount.UserEmail,
                            UserPhone1 = newAccount.UserPhone1,
                            UserPhone2 = newAccount.UserPhone2,
                            UserRightsMask = newAccount.UserRightsMask,
                            UserRoleMask = newAccount.UserRoleMask,
	                        #endregion
                        }
                };

            try
            {
                string rawData;

                var result = HttpParameter.DeserializeServerResponse(urlSrv + queryPtr, newAccountParamList, out rawData, userName, userPwrd);
                var report = (ExecutionReport)result.FirstOrDefault(p => p is ExecutionReport);

                if (report != null)
                {
                    if (!report.IsOk)
                        Logger.Error("Ошибка: " + report.Comment);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("AddAccount(). Ошибка добавления аккаунта", ex);
            }
            try
            {
                if (newAccount.WalletBalance < 0)
                {
                    Logger.Error("AddAccount(). Не удалось зачислить средства на кошелёк пользователя - невозможно зачислить отридцательную сумму.");
                    return false;
                }
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var userId = ctx.PLATFORM_USER.Single(x => x.Login == newAccount.UserLogin);
                    var wallet = ctx.WALLET.Single(w => w.User == userId.ID);
                    wallet.Balance = (decimal)newAccount.AccountBalance;
                    ctx.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("AddAccount() - не удалось зачислить средства на кошелёк пользователя", ex);
            }
            return false;
        }

        public AccountOwnerModel GetAccountOwnerModel(int? accountId = null, int? ownerId = null)
        {
            var resultAccounts = new List<Account>();
            var resultOwners = new List<AccountUserModel>();
            var resultAccountUserRights = new List<Tuple<int, UserAccountRights>>();
            var modeAccountOwner = AccountOwnerPartialViewType.Undefined;

            List<PLATFORM_USER_ACCOUNT> acc = null;
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    if (accountId.HasValue && !ownerId.HasValue) // Если номер счёта указан, а пользователь нет - то формируем модель типа "показать всех подписчиков счёта"
                    {
                        modeAccountOwner = AccountOwnerPartialViewType.ForAccount;
                        acc = new List<PLATFORM_USER_ACCOUNT>(ctx.PLATFORM_USER_ACCOUNT.Where(x => x.ACCOUNT1.ID == accountId));
                        if (acc.Count <= 0) // на данный счёт пока не подписано ни одного пользователя
                        {
                            try
                            {
                                var account = ctx.ACCOUNT.Single(x => x.ID == accountId.Value);
                                resultAccounts.Add(LinqToEntity.DecorateAccount(account));
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(String.Format("GetAccountOwnerModel() - Не соответствие между связанными таблицами! " +
                                             "В таблице ACCOUNT отсутствует записть с ID {0}, " +
                                             "ссылка на которую есть в таблице PLATFORM_USER_ACCOUNT, " +
                                             "или запись с таким ID встречается несколько раз", accountId.Value), ex);
                            }
                        }
                    }
                    if (!accountId.HasValue && ownerId.HasValue) // Если пользователь указан, а номер счёта нет - то формируем модель типа "показать все счета пользователя"
                    {
                        acc = new List<PLATFORM_USER_ACCOUNT>(ctx.PLATFORM_USER_ACCOUNT.Where(x => x.PLATFORM_USER.ID == ownerId));
                        modeAccountOwner = AccountOwnerPartialViewType.ForOwner;
                        if (acc.Count <= 0) // данный пользователь не управляет счетами
                        {
                            try
                            {
                                var owner = ctx.PLATFORM_USER.Single(x => x.ID == ownerId.Value);
                                resultOwners.Add(new AccountUserModel { UserId = owner.ID, Title = owner.Title, UserDescription = owner.Description });
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(String.Format("GetAccountOwnerModel() - Не соответствие между связанными таблицами! " +
                                             "В таблице PLATFORM_USER отсутствует записть с ID {0}, " +
                                             "ссылка на которую есть в таблице PLATFORM_USER_ACCOUNT, " +
                                             "или запись с таким ID встречается несколько раз", ownerId.Value), ex);
                            }
                        }
                    }

                    if (acc == null || acc.Count <= 0) return new AccountOwnerModel(resultAccounts, resultOwners, resultAccountUserRights, modeAccountOwner);

                    resultAccounts = acc.Select(x => LinqToEntity.DecorateAccount(x.ACCOUNT1)).ToList();
                    resultOwners = acc.Select(x => new AccountUserModel { UserId = x.PLATFORM_USER.ID, Title = x.PLATFORM_USER.Title, UserDescription = x.PLATFORM_USER.Description }).ToList();

                    switch (modeAccountOwner)
                    {
                        case AccountOwnerPartialViewType.Undefined:
                            throw new Exception("");
                        case AccountOwnerPartialViewType.ForAccount:
                            resultAccountUserRights.AddRange(acc.Select(x => new Tuple<int, UserAccountRights>(x.PlatformUser, (UserAccountRights)x.RightsMask)));
                            break;
                        case AccountOwnerPartialViewType.ForOwner:
                            resultAccountUserRights.AddRange(acc.Select(x => new Tuple<int, UserAccountRights>(x.Account, (UserAccountRights)x.RightsMask)));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetAccountOwnerModel()", ex);
            }

            return new AccountOwnerModel(resultAccounts, resultOwners, resultAccountUserRights, modeAccountOwner);
        }

        public void EditAccountOwnerModel(int accountId, int ownerId, int right, string action)
        {
            switch (action)
            {
                case "add":
                    try
                    {
                        using (var ctx = DatabaseContext.Instance.Make())
                        {
                            var newItem = new PLATFORM_USER_ACCOUNT
                            {
                                Account = accountId,
                                PlatformUser = ownerId,
                                RightsMask = right
                            };
                            ctx.PLATFORM_USER_ACCOUNT.Add(newItem);
                            ctx.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(String.Format("EditAccountOwnerModel() - ошибка при добавлении пользователя с ID {0} к управляющми счёта {1}", ownerId, accountId), ex);
                    }
                    break;
                case "del":
                    try
                    {
                        using (var ctx = DatabaseContext.Instance.Make())
                        {
                            var itemToDel = ctx.PLATFORM_USER_ACCOUNT.FirstOrDefault(x => x.Account == accountId && x.PlatformUser == ownerId);
                            if (itemToDel == null) return;
                            ctx.PLATFORM_USER_ACCOUNT.Remove(itemToDel);
                            ctx.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(String.Format("EditAccountOwnerModel() - ошибка при удалении пользователя с ID {0} из управляющих счётом {1}", ownerId, accountId), ex);
                    }
                    break;
            }
        }

        /// <summary>
        /// Запрос проще сформировать в виде стоки и отпра
        /// </summary>
        public List<AccountTag> GetFilterAccountFromServer(AccountViewModel model)
        {
            #region Формируем список фильтров
            var filters = new List<string>();
            if (model != null)
            {
                if (model.FilterId.HasValue) filters.Add("ID = " + model.FilterId.Value);
                if (model.FilterBalanceLower.HasValue) filters.Add("Balance >= " + model.FilterBalanceLower.Value);
                if (model.FilterBalanceUpper.HasValue) filters.Add("Balance <= " + model.FilterBalanceUpper.Value);
                if (!String.IsNullOrEmpty(model.FilterBalanceTicker)) filters.Add("Currency = '" + model.FilterBalanceTicker + "'");
                if (!String.IsNullOrEmpty(model.FilterGroup)) filters.Add("AccountGroup = '" + model.FilterGroup + "'");
            }
            #endregion

            var result = new List<AccountTag>();
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var sqlQueryString = new StringBuilder();
                    sqlQueryString.Append(@"select ac.ID, AccountGroup, Currency, Balance, 
	                                        STUFF((select ',' +  dbo.MakeUserNameWithInitials(usr.Login, usr.Name, usr.Surname, usr.Patronym)
	                                                from PLATFORM_USER usr join PLATFORM_USER_ACCOUNT ua on usr.ID = ua.PlatformUser
	                                        where ac.ID = ua.Account for xml path('')), 1, 1, '') UserNames,
	                                        STUFF((select ',' + convert(VARCHAR, usr.ID)
	                                            from PLATFORM_USER usr join PLATFORM_USER_ACCOUNT ua on usr.ID = ua.PlatformUser
	                                        where ac.ID = ua.Account for xml path('')), 1, 1, '') UserId from ACCOUNT ac");
                    // фильтр по хозяевам счёта
                    if (model != null && !String.IsNullOrEmpty(model.FilterOwners))
                        sqlQueryString.Append(@" join PLATFORM_USER_ACCOUNT pac on ac.ID = pac.Account 
                                                join PLATFORM_USER us on us.ID = pac.PlatformUser and dbo.MakeUserNameWithInitials(us.Login, us.Name, us.Surname, us.Patronym) 
                                                like '%" + model.FilterOwners + "%'");

                    if (filters.Count > 0)
                    {
                        sqlQueryString.Append(" where ");
                        sqlQueryString.Append(String.Join(" and ", filters));
                    }

                    var accountsDb = ctx.Database.SqlQuery<GetAllAccountsUserDetail_Result>(sqlQueryString.ToString()).ToList();
                    // ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var ac in accountsDb)
                    // ReSharper restore LoopCanBeConvertedToQuery
                    {
                        result.Add(new AccountTag(ac));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Не удалось получить список всех счетов", ex);
                return new List<AccountTag>();
            }
            return result;
        }

        public bool Deactivate(int accountId)
        {
            Logger.Info(String.Format("Начинаем деактивировать счёт {0}", accountId));
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var account = ctx.ACCOUNT.First(x => x.ID == accountId);
                    account.Status = (int)Account.AccountStatus.Blocked;

                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Deactivate()", ex);
            }
            return false;
        }
        
        public int? GetSignalCount(int id)
        {
            try
            {
                int count;
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var account = ctx.ACCOUNT.First(x => x.ID == id);
                    count = account.SERVICE.Count;
                }
                return count;
            }
            catch (Exception ex)
            {
                Logger.Error("GetSignalCount()", ex);
                return null;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Entity;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Linq;
using TradeSharp.Server.BL;
using TradeSharp.Server.Repository;
using TradeSharp.TradeLib;
using TradeSharp.Util;
using TradeSharp.Util.Serialization;

namespace TradeSharp.Server.Contract
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ManagerAccount : ITradeSharpAccount
    {
        //private readonly int[] autoSignInCategories = AppConfig.GetStringParam("AutoSignIn.Categories", "").ToIntArrayUniform();

        private readonly bool makeNewlyAddedUsersSignallers = AppConfig.GetBooleanParam("MakeNewUsersSignallers", true);

        private IAccountRepository accountRepository;

        private IProfitCalculator profitCalculator;

        private readonly string defaultDemoAccountGroupCode = AppConfig.GetStringParam("DefaultDemoGroup", "Demo");

        #region Singletone

        private static readonly Lazy<ManagerAccount> instance = new Lazy<ManagerAccount>(() => new ManagerAccount());

        public static ManagerAccount Instance
        {
            get { return instance.Value; }
        }

        private ManagerAccount()
        {
            profitCalculator = ProfitCalculator.Instance;
            accountRepository = AccountRepository.Instance;
        }

        #endregion

        public List<int> GetAccountChannelIDs(int accountId)
        {
            try
            {
                var channelIds = new List<int>();

                RequestStatus errorCode;
                var accounts = accountRepository.GetAccounts(out errorCode);
                var account = accounts.FirstOrDefault(a => a.ID == accountId);
                if (account == null) return channelIds;
                var acGroup = account.Group;

                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var channels = ctx.CHANNEL.Where(c => c.ACCOUNT_GROUP.Any(g => g.Code == acGroup));
                    channelIds.AddRange(channels.Select(channel => channel.ID));
                }
                return channelIds;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("GetAccountChannelIDs({0}) - ошибка: {1}", accountId, ex);
            }
            return new List<int>();
        }

        public AccountRegistrationStatus RegisterAccount(PlatformUser user,
            string accountCurrency, int startBalance, decimal maxLeverage, string completedPassword, bool autoSignIn)
        {
            // проверить заполнение логина-почты-баланса-плеча
            if (user.Login.Length < PlatformUser.LoginLenMin ||
                user.Login.Length > PlatformUser.LoginLenMax) return AccountRegistrationStatus.IncorrectLogin;
            if (user.Email.Length < PlatformUser.EmailLenMin ||
                user.Email.Length > PlatformUser.EmailLenMax) return AccountRegistrationStatus.IncorrectEmail;
            if (!PlatformUser.CheckLoginSpelling(user.Login)) return AccountRegistrationStatus.IncorrectLogin;
            if (startBalance < Account.MinimumStartDepo || startBalance > Account.MaximumStartDepo)
                return AccountRegistrationStatus.IncorrectBalance;
            if (maxLeverage < 0) maxLeverage = 0;
            else if (maxLeverage > Account.MaximumDepoLeverage) maxLeverage = Account.MaximumDepoLeverage;
            long hash;
            if (!TradingContractDictionary.Instance.GetTickers(out hash).Any(c => c.ActiveBase == accountCurrency ||
                c.ActiveCounter == accountCurrency)) return AccountRegistrationStatus.WrongCurrency;

            // сгенерировать пароль
            if (string.IsNullOrEmpty(completedPassword))
            {
                string pwrd;
                while (true)
                {
                    pwrd = RandomWordGenerator.Password(new Random().Next(2) + 2);
                    if (pwrd.Length < PlatformUser.PasswordLenMin || pwrd.Length > PlatformUser.PasswordLenMax)
                        continue;
                    break;
                }
                user.Password = pwrd;
            }
            else
                user.Password = completedPassword;
            user.RegistrationDate = DateTime.Now;
            Logger.InfoFormat("RegisterAccount (email={0}, login={1}, pwrd={2}{3})",
                user.Email, user.Login, user.Password, string.IsNullOrEmpty(completedPassword) ? " (auto)" : "");
            if (string.IsNullOrEmpty(user.Title))
                user.Title = string.IsNullOrEmpty(user.Name) ? user.Login : user.Name;
            user.RoleMask = UserRole.Trader;

            // попытка создать пользователя и открыть счет
            using (var ctx = DatabaseContext.Instance.Make())
            {
                try
                {
                    // проверка дублирования
                    var existUser = ctx.PLATFORM_USER.FirstOrDefault(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase));
                    Logger.InfoFormat("Регистрация пользователя: email {0} занят", user.Email);
                    if (existUser != null) return AccountRegistrationStatus.DuplicateEmail;
                    existUser = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == user.Login);
                    if (existUser != null) return AccountRegistrationStatus.DuplicateLogin;
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка в RegisterAccount(checks)", ex);
                    return AccountRegistrationStatus.ServerError;
                }

                // в рамках одной транзакции создать логин и счет
                //using (var transaction = ctx.Connection.BeginTransaction())
                {
                    DbTransaction transaction;
                    try
                    {
                        if (((IObjectContextAdapter)ctx).ObjectContext.Connection.State != ConnectionState.Open)
                            ((IObjectContextAdapter)ctx).ObjectContext.Connection.Open();
                        Logger.Info("Connection's opened");
                        transaction = ((IObjectContextAdapter)ctx).ObjectContext.Connection.BeginTransaction();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("RegisterAccount - ошибка в ctx.Connection.BeginTransaction", ex);
                        return AccountRegistrationStatus.ServerError;
                    }

                    try
                    {
                        // добавить пользователя
                        var userBase = LinqToEntity.UndecoratePlatformUser(user);
                        ctx.PLATFORM_USER.Add(userBase);

                        // добавить счет
                        var account = new ACCOUNT
                        {
                            AccountGroup = defaultDemoAccountGroupCode,
                            MaxLeverage = maxLeverage,
                            Balance = startBalance,
                            UsedMargin = 0,
                            Currency = accountCurrency,
                            Description = string.Format("demo account for {0}", user.Login),
                            Status = (int)Account.AccountStatus.Created,
                            TimeCreated = DateTime.Now,
                        };
                        try
                        {
                            ctx.ACCOUNT.Add(account);
                            // сохранить изменения (добавление пользователя и счета, нужны ID)
                            ctx.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("RegisterAccount - ACCOUNT adding error", ex);
                            return AccountRegistrationStatus.ServerError;
                        }


                        // добавить кошелек
                        try
                        {
                            var wallet = new WALLET
                            {
                                Balance = 0,
                                Currency = accountCurrency,
                                Password = user.Password,
                                User = userBase.ID
                            };
                            ctx.WALLET.Add(wallet);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("RegisterAccount - WALLET adding error", ex);
                            return AccountRegistrationStatus.ServerError;
                        }

                        // пользователь-счет
                        var userAccount = new PLATFORM_USER_ACCOUNT
                        {
                            PlatformUser = userBase.ID,
                            Account = account.ID,
                            RightsMask = (int)AccountRights.Управление
                        };
                        ctx.PLATFORM_USER_ACCOUNT.Add(userAccount);

                        // перевод на счет
                        var trans = new BALANCE_CHANGE
                        {
                            ValueDate = DateTime.Now,
                            AccountID = account.ID,
                            Amount = startBalance,
                            ChangeType = (int)BalanceChangeType.Deposit,
                            Description = "initial deposit"
                        };
                        ctx.BALANCE_CHANGE.Add(trans);

                        // сделать сигнальщиком
                        if (makeNewlyAddedUsersSignallers)
                            MakeNewlyRegisteredAccountSignaller(ctx, userBase, account);

                        // сохранить все изменения
                        ctx.SaveChanges();
                        if (string.IsNullOrEmpty(completedPassword))
                            if (!SendEmailOnNewAccount(user, true))
                            {
                                transaction.Rollback();
                                return AccountRegistrationStatus.EmailDeliveryError;
                            }

                        transaction.Commit();
                        ((IObjectContextAdapter)ctx).ObjectContext.Connection.Close();
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorFormat("Ошибка в RegisterAccount (login={0}, email={1}) : {2}",
                                           user.Login, user.Email, ex);
                        transaction.Rollback();
                        ((IObjectContextAdapter)ctx).ObjectContext.Connection.Close();
                        return AccountRegistrationStatus.ServerError;
                    }
                } // using (transaction ...
            }            
            return AccountRegistrationStatus.OK;
        }

        public void MakeNewlyRegisteredAccountSignaller(TradeSharpConnection ctx,
            PLATFORM_USER user, ACCOUNT account)
        {
            try
            {
                ctx.SERVICE.Add(new SERVICE
                    {
                        AccountId = account.ID,
                        ServiceType = (int) PaidServiceType.PAMM,
                        Currency = account.Currency,
                        FixedPrice = 1,
                        Comment = "Сигналы " + LinqToEntity.DecoratePlatformUser(user).NameWithInitials,
                        User = user.ID
                    });
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка в MakeNewlyRegisteredAccountSignaller(login={0}, account={1}): {2}",
                    user.Login, account.ID, ex);
            }
        }

        public RequestStatus ChangeBalance(int accountId, decimal summ, string comment)
        {
            var changeType = summ < 0 ? BalanceChangeType.Withdrawal : BalanceChangeType.Deposit;
            summ = Math.Abs(summ);
            return accountRepository.ChangeBalance(accountId, summ, comment, DateTime.Now, changeType);
        }

        public bool RemindPassword(string email, string login)
        {
            if (string.IsNullOrEmpty(email)) email = "";
            if (string.IsNullOrEmpty(login)) login = "";

            using (var ctx = DatabaseContext.Instance.Make())
            {
                PLATFORM_USER user;
                try
                {
                    user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Email == email || u.Login == login);
                    if (user == null) return false;
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Ошибка в RemindPassword({0}) -  получение пользователя: {1}", email, ex);
                    return false;
                }
                // отправить письмо
                try
                {
                    return SendEmailOnNewAccount(LinqToEntity.DecoratePlatformUser(user), false);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Ошибка в RemindPassword({0}) - отправка письма: {1}", email, ex);
                    return false;
                }
            }
        }

        public List<PaidService> GetPaidServices(string userLogin)
        {
            using (var ctx = DatabaseContext.Instance.Make())
            try
            {
                return (from usr in ctx.PLATFORM_USER
                        join sub in ctx.SUBSCRIPTION on usr.ID equals sub.User
                        join srv in ctx.SERVICE on sub.Service equals srv.ID
                        where usr.Login == userLogin
                        select srv).ToList().Select(LinqToEntity.DecoratePaidService).ToList();
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка в ServerManagerAccount.GetPaidServices(login={0}): {1}", userLogin, ex);
                return null;
            }
        }

        public List<PaidService> GetUserOwnedPaidServices(string userLogin)
        {
            using (var ctx = DatabaseContext.Instance.Make())
                try
                {
                    return (from usr in ctx.PLATFORM_USER
                            join srv in ctx.SERVICE on usr.ID equals srv.User
                            where usr.Login == userLogin
                            select srv).ToList().Select(LinqToEntity.DecoratePaidService).ToList();
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Ошибка в ServerManagerAccount.GetUserOwnedPaidServices(login={0}): {1}", userLogin, ex);
                    return null;
                }
        }

        public List<Subscription> GetSubscriptions(string userLogin)
        {
            using (var ctx = DatabaseContext.Instance.Make())
            {
                try
                {
                    return (from usr in ctx.PLATFORM_USER
                            join sub in ctx.SUBSCRIPTION_V on usr.ID equals sub.User
                            where usr.Login == userLogin
                            select sub).ToList().Select(LinqToEntity.DecorateSubscription).ToList();

                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Ошибка в ServerManagerAccount.GetSubscriptions(login={0}): {1}",
                        userLogin, ex);
                    return null;
                }
            }
        }
        
        public Dictionary<int, List<MarketOrder>> GetSignallersOpenTrades(int accountId)
        {
            var ordersByService = new Dictionary<int, List<MarketOrder>>();
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var subscribedServices =
                        (from sub in ctx.SUBSCRIPTION
                         join act in ctx.PLATFORM_USER_ACCOUNT on sub.User equals act.PlatformUser
                         join srv in ctx.SERVICE on sub.Service equals srv.ID
                         where act.RightsMask == (int)AccountRights.Управление && act.Account == accountId
                         select srv).ToList();
                    var accountsOfManagers = subscribedServices.Select(s => s.AccountId).ToList();

                    // получить сделки по счетам из списка signallerIds
                    var dealsList = ctx.POSITION.Where(p => p.State == (int)PositionState.Opened &&
                                                            accountsOfManagers.Contains(p.AccountID));
                    foreach (var deal in dealsList)
                    {
                        var dealDec = LinqToEntity.DecorateOrder(deal);
                        var serviceId = subscribedServices.First(a => a.AccountId == dealDec.AccountID).ID;

                        List<MarketOrder> ordersLst;
                        if (ordersByService.TryGetValue(serviceId, out ordersLst))
                        {
                            ordersLst.Add(dealDec);
                            continue;
                        }
                        ordersLst = new List<MarketOrder> { dealDec };
                        ordersByService.Add(serviceId, ordersLst);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка в GetSignallersOpenTrades({0}): {1}", accountId, ex);
            }
            return ordersByService;
        }

        /// <summary>
        /// вернуть информацию по счету
        /// </summary>        
        /// <returns></returns>
        public RequestStatus GetAccountInfo(int accountId, bool needEquityInfo, out Account accountInfo)
        {
            RequestStatus accStatus;
            accountInfo = accountRepository.GetAccount(accountId, out accStatus);
            if (accStatus != RequestStatus.OK) return accStatus;
            //accountInfo = LinqToEntity.DecorateAccount(account);
            // расчет эквити
            if (needEquityInfo)
                accountInfo.Equity = profitCalculator.CalculateAccountEquity(accountId, accountInfo.Balance, accountInfo.Currency,
                    TradeSharp.Contract.Util.BL.QuoteStorage.Instance.ReceiveAllData(), this);
            return RequestStatus.OK;
        }

        /// <summary>
        /// получить список открытых позиций
        /// </summary>        
        public RequestStatus GetMarketOrders(int accountId, out List<MarketOrder> orders)
        {
            using (var ctx = DatabaseContext.Instance.Make())
                try
                {
                    var positions = from pos in ctx.POSITION
                                    where (pos.State == (int)(PositionState.Opened) || pos.State == (int)(PositionState.StartOpened) ||
                                        pos.State == (int)(PositionState.StartClosed)) && pos.AccountID == accountId
                                    select pos;
                    orders = new List<MarketOrder>();
                    // ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var position in positions)
                    // ReSharper restore LoopCanBeConvertedToQuery
                    {
                        orders.Add(LinqToEntity.DecorateOrder(position));
                    }
                    return RequestStatus.OK;
                }
                catch (Exception ex)
                {
                    orders = null;
                    Logger.ErrorFormat("Ошибка в GetMarketOrders({0}): {1}", accountId, ex);
                    return RequestStatus.ServerError;
                }
        }
        
        /// <summary>
        /// то же, что и GetHistoryOrders, но упаковано MarketOrderSerializer
        /// </summary>        
        public RequestStatus GetHistoryOrdersCompressed(int? accountId, DateTime? startDate,
            out byte[] buffer)
        {
            List<MarketOrder> orders;
            buffer = null;
            var retVal = GetHistoryOrders(accountId, startDate, out orders);
            if (orders != null && orders.Count > 0)
            {
                try
                {
                    using (var writer = new SerializationWriter())
                    {
                        writer.Write(orders);
                        writer.Flush();
                        buffer = writer.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("GetHistoryOrdersCompressed() - ошибка сериализации", ex);
                    return RequestStatus.SerializationError;
                }
            }
            return retVal;
        }

        /// <summary>
        /// получить список закрытых позиций
        /// </summary>        
        public RequestStatus GetHistoryOrders(int? accountId, DateTime? startDate,
            out List<MarketOrder> orders)
        {
            using (var ctx = DatabaseContext.Instance.Make())
            {
                try
                {
                    var positions = from pos in ctx.POSITION_CLOSED
                                    where
                                        pos.AccountID == (accountId ?? pos.AccountID) &&
                                        pos.TimeEnter >= (startDate ?? pos.TimeEnter)
                                    select pos;
                    orders = new List<MarketOrder>();
                    // ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var position in positions)
                    // ReSharper restore LoopCanBeConvertedToQuery
                    {
                        orders.Add(LinqToEntity.DecorateOrder(position));
                    }
                    return RequestStatus.OK;
                }
                catch (Exception ex)
                {
                    orders = null;
                    Logger.ErrorFormat("Ошибка в GetHistoryOrders({0}): {1}", accountId, ex);
                    return RequestStatus.ServerError;
                }
            }
        }

        /// <summary>
        /// получить закрытые ордера по дате закрытия
        /// </summary>        
        public RequestStatus GetHistoryOrdersByCloseDate(int? accountId, DateTime? startDate,
            out List<MarketOrder> orders)
        {
            using (var ctx = DatabaseContext.Instance.Make())
            {
                try
                {
                    var positions = from pos in ctx.POSITION_CLOSED
                                    where
                                        pos.AccountID == (accountId ?? pos.AccountID) &&
                                        (pos.TimeExit >= startDate) || (!startDate.HasValue)
                                    select pos;
                    orders = new List<MarketOrder>();
                    // ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var position in positions)
                    // ReSharper restore LoopCanBeConvertedToQuery
                    {
                        orders.Add(LinqToEntity.DecorateOrder(position));
                    }
                    return RequestStatus.OK;
                }
                catch (Exception ex)
                {
                    orders = null;
                    Logger.ErrorFormat("Ошибка в GetHistoryOrdersByCloseDate({0}): {1}", accountId, ex);
                    return RequestStatus.ServerError;
                }
            }
        }

        public RequestStatus GetOrdersByFilter(
            int accountId, 
            bool getClosedOrders,
            OrderFilterAndSortOrder filter,
            out List<MarketOrder> orders)
        {
            using (var ctx = DatabaseContext.Instance.Make())
            {
                try
                {
                    if (getClosedOrders)
                    {
                        var positions = from pos in ctx.POSITION_CLOSED
                                        where
                                            pos.AccountID == accountId &&
                                            (filter.filterMagic == null || filter.filterMagic.Value == pos.Magic) &&
                                            (filter.filterSide == null || filter.filterSide.Value == pos.Side) &&
                                            (string.IsNullOrEmpty(filter.filterComment) ||
                                             pos.Comment.Contains(filter.filterComment)) &&
                                            (string.IsNullOrEmpty(filter.filterExpertComment) ||
                                             pos.ExpertComment.Contains(filter.filterExpertComment)) &&
                                            (string.IsNullOrEmpty(filter.filterTicker) ||
                                             pos.Symbol.Contains(filter.filterTicker)) &&
                                            (filter.filterTimeEnterStartsWith == null ||
                                             pos.TimeEnter >= filter.filterTimeEnterStartsWith.Value) &&
                                            (filter.filterTimeEnterEndsWith == null ||
                                             pos.TimeEnter <= filter.filterTimeEnterEndsWith.Value) &&
                                            (filter.filterTimeExitStartsWith == null ||
                                             pos.TimeExit >= filter.filterTimeExitStartsWith) &&
                                            (filter.filterTimeExitEndsWith == null ||
                                             pos.TimeExit <= filter.filterTimeExitEndsWith)
                                        select pos;
                        positions =
                            (filter.sortAscending
                                 ? positions.OrderBy(p => filter.sortByTimeEnter ? p.TimeEnter : p.TimeExit)
                                 : positions.OrderByDescending(p => filter.sortByTimeEnter ? p.TimeEnter : p.TimeExit))
                                .Take(filter.takeCount);

                        orders = positions.ToList().Select(LinqToEntity.DecorateOrder).ToList();
                    }
                    else
                    {
                        var positions = from pos in ctx.POSITION
                                        where
                                            pos.AccountID == accountId &&
                                            (filter.filterMagic == null || filter.filterMagic.Value == pos.Magic) &&
                                            (filter.filterSide == null || filter.filterSide.Value == pos.Side) &&
                                            (string.IsNullOrEmpty(filter.filterComment) ||
                                                pos.Comment.Contains(filter.filterComment)) &&
                                            (string.IsNullOrEmpty(filter.filterExpertComment) ||
                                                pos.Comment.Contains(filter.filterExpertComment)) &&
                                            (string.IsNullOrEmpty(filter.filterTicker) ||
                                                pos.Comment.Contains(filter.filterTicker)) &&
                                            (filter.filterTimeEnterStartsWith == null ||
                                                pos.TimeEnter >= filter.filterTimeEnterStartsWith.Value) &&
                                            (filter.filterTimeEnterEndsWith == null ||
                                                pos.TimeEnter <= filter.filterTimeEnterEndsWith.Value)
                                        select pos;                        
                        positions =
                            (filter.sortAscending
                                    ? positions.OrderBy(p => p.TimeEnter)
                                    : positions.OrderByDescending(p => p.TimeEnter))
                                .Take(filter.takeCount);

                        orders = positions.ToList().Select(LinqToEntity.DecorateOrder).ToList();
                    }
                    return RequestStatus.OK;
                }
                catch (Exception ex)
                {
                    orders = null;
                    Logger.ErrorFormat("Ошибка в GetOrdersByFilter({0}): {1}", accountId, ex);
                    return RequestStatus.ServerError;
                }
            }
        }

        /// <summary>
        /// получить список отложенных ордеров
        /// </summary>        
        public RequestStatus GetPendingOrders(int accountId, out List<PendingOrder> orders)
        {
            orders = new List<PendingOrder>();
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var pendingOrders = from ord in ctx.PENDING_ORDER
                                        where ord.AccountID == accountId
                                        select ord;
                    // ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var ord in pendingOrders) // нельзя рефакторить
                    // ReSharper restore LoopCanBeConvertedToQuery
                    {
                        orders.Add(LinqToEntity.DecoratePendingOrder(ord));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка при получении списка отложенных ордеров", ex);
                return RequestStatus.ServerError;
            }
            return RequestStatus.OK;
        }

        public RequestStatus GetBalanceChanges(int accountId, DateTime? startTime,
            out List<BalanceChange> balanceChanges)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    balanceChanges = (startTime.HasValue
                                      ? from b in ctx.BALANCE_CHANGE where b.AccountID == accountId && b.ValueDate >= startTime.Value select b
                                      : from b in ctx.BALANCE_CHANGE where b.AccountID == accountId select b).ToList().Select(
                                        LinqToEntity.DecorateBalanceChange).ToList();
                    return RequestStatus.OK;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в GetBalanceChanges", ex);
                balanceChanges = new List<BalanceChange>();
                return RequestStatus.ServerError;
            }
        }
        
        private static bool SendEmailOnNewAccount(PlatformUser user, bool isDemo)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("Уважаемый {0},", user.MakeFullName()));
            sb.AppendLine();
            sb.AppendLine(string.Format("в платформе {0} открыт {1} для пользователя {2}.",
                PlatformConstants.PlatformLongName, isDemo ? "демо-счет" : "счет", user.Login));
            sb.AppendLine(string.Format("Пароль для доступа к счету: {0}.", user.Password));
            sb.AppendLine();
            sb.AppendLine("Спасибо, что выбрали нас!");

            return HttpMailServerProxy.SendEmails(new[] { user.Email },
                                           "Регистрация счета в " + PlatformConstants.PlatformShortName,
                                           sb.ToString(), new string[0]);
        }

        public List<string> GetTickersTraded(int accountId)
        {
            return accountRepository.GetTickersTradedCheckCredentials(string.Empty, string.Empty, accountId, 0, false);
        }

        public List<AccountShareOnDate> GetAccountShareHistory(int accountId, string userLogin)
        {
            using (var db = DatabaseContext.Instance.Make())
            {
                var records = (from hst in db.ACCOUNT_SHARE_HISTORY
                               join usr in db.PLATFORM_USER on hst.ShareOwner equals usr.ID
                               where usr.Login == userLogin && hst.Account == accountId
                               select hst).OrderBy(h => h.Date).ToList().Select(LinqToEntity.DecorateAccountShareHistory).ToList();
                return records;
            }
        }

        public List<MarketOrder> GetClosedOrders(int accountId, string optionalSymbolFilter, int startId, int maxCount)
        {
            return accountRepository.GetClosedOrOpenedOrdersCheckCredentials(string.Empty, string.Empty, 0,
                accountId, optionalSymbolFilter, startId, maxCount, false, true);
        }

        public List<int> GetFreeMagicsPool(int accountId, int poolSize)
        {
            List<int> magics;
            using (var db = DatabaseContext.Instance.Make())
            {
                magics =
                    db.POSITION.Where(p => p.AccountID == accountId && p.Magic.HasValue)
                      .Select(p => p.Magic.Value)
                      .Union(
                          db.POSITION_CLOSED.Where(p => p.AccountID == accountId && p.Magic.HasValue)
                            .Select(p => p.Magic.Value)).ToList().OrderBy(m => m).ToList();
            }

            if (magics.Count == 0)
                return Enumerable.Range(0, poolSize).ToList();

            // найти N незанятых номеров
            var pool = new List<int>();
            var nextIndex = 0;

            for (var i = 1;; i++)
            {
                if (nextIndex >= 0)                
                {
                    while (magics[nextIndex] <= i)
                    {
                        i++;
                        nextIndex++;
                        if (nextIndex == magics.Count)
                        {
                            nextIndex = -1;
                            break;
                        }
                    }
                }

                pool.Add(i);
                if (pool.Count == poolSize)
                    break;
            }

            return pool;
        }

        public List<Transfer> GetAccountTransfersPartByPart(ProtectedOperationContext secCtx, 
            string login, int startId, int countMax)
        {
            if (!UserSessionStorage.Instance.PermitUserOperation(secCtx, false, true))
                return new List<Transfer>();

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    return ctx.TRANSFER.Where(t => t.PLATFORM_USER.Login == login && 
                        t.ID > startId).Take(countMax).ToList().Select(LinqToEntity.DecorateTransfer).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в GetAccountTransfersPartByPart", ex);
            }
            return new List<Transfer>();
        }

        public List<AccountShare> GetAccountShares(ProtectedOperationContext secCtx, int accountId, bool needMoneyShares)
        {
            if (!UserSessionStorage.Instance.PermitUserOperation(secCtx, false, true))
                return new List<AccountShare>();

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var shares = (from sh in ctx.ACCOUNT_SHARE
                                 join usr in ctx.PLATFORM_USER on sh.ShareOwner equals usr.ID
                                 where sh.Account == accountId
                                 select new AccountShare
                                     {
                                         UserId = sh.ShareOwner,
                                         SharePercent = sh.Share,
                                         UserLogin = usr.Login,
                                         HWM = sh.HWM ?? 0
                                     }).ToList();
                    if (shares.Count == 0 || needMoneyShares == false)
                    return shares;

                    // посчитать текущий баланс счета, из него посчитать доли в деньгах (валюте счета)
                    var account = ctx.ACCOUNT.First(a => a.ID == accountId);
                    var equity = profitCalculator.CalculateAccountEquity(accountId, account.Balance, account.Currency,
                        TradeSharp.Contract.Util.BL.QuoteStorage.Instance.ReceiveAllData(), this);
                    shares.ForEach(s => s.ShareMoney = equity * s.SharePercent / 100M);
                    return shares;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в GetAccountShares", ex);
            }
            return new List<AccountShare>();
        }

        public RequestStatus GetUserOwnAndSharedAccounts(string login,
                                                                  ProtectedOperationContext secCtx,
                                                                  out List<AccountShared> accounts)
        {
            accounts = new List<AccountShared>();
            if (!UserSessionStorage.Instance.PermitUserOperation(secCtx, false, true))
                return RequestStatus.Unauthorized;

            try
            {
                Wallet userWallet;
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    // получить собственные реальные! счета пользователя
                    var ownAccounts = (from ac in ctx.ACCOUNT
                                       join gr in ctx.ACCOUNT_GROUP on ac.AccountGroup equals gr.Code
                                       join pa in ctx.PLATFORM_USER_ACCOUNT on ac.ID equals pa.Account
                                       join u in ctx.PLATFORM_USER on pa.PlatformUser equals u.ID
                                       where u.Login == login && gr.IsReal
                                       select ac).ToList()
                                                 .Select(a => new AccountShared(LinqToEntity.DecorateAccount(a), true)
                                                     {
                                                         SharePercent = 100
                                                     })
                                                 .ToList();
                    // получить те счета, в которых у запросившего есть доля
                    var shares = (from sh in ctx.ACCOUNT_SHARE
                                  join u in ctx.PLATFORM_USER on sh.ShareOwner equals u.ID
                                  join ac in ctx.ACCOUNT on sh.Account equals ac.ID
                                  join gr in ctx.ACCOUNT_GROUP on ac.AccountGroup equals gr.Code
                                  where u.Login == login && gr.IsReal
                                  select new AccountShared
                                      {
                                          Account = new Account
                                              {
                                                  ID = ac.ID,
                                                  Group = ac.AccountGroup,
                                                  Currency = ac.Currency,
                                                  Balance = ac.Balance                                                  
                                              },
                                          SharePercent = sh.Share
                                      }).ToList();
                    // склеить оба списка
                    ownAccounts.ForEach(a =>
                        {
                            var sharedAccount = shares.FirstOrDefault(s => s.Account.ID == a.Account.ID);
                            if (sharedAccount != null)
                                a.SharePercent = sharedAccount.SharePercent;
                        });
                    accounts = ownAccounts.Union(shares, AccountShared.ComparerOnId.Instance).ToList();
                    if (accounts.Count == 0) return RequestStatus.OK;

                    userWallet = LinqToEntity.DecorateWallet((from w in ctx.WALLET
                                                              join u in ctx.PLATFORM_USER on w.User equals u.ID
                                                              where u.Login == login
                                                              select w).First());
                }

                // посчитать профит (equity) для счетов
                foreach (var account in accounts)
                {
                    var equity = profitCalculator.CalculateAccountEquity(account.Account.ID, account.Account.Balance,
                        account.Account.Currency, TradeSharp.Contract.Util.BL.QuoteStorage.Instance.ReceiveAllData(), this);
                    account.Account.Equity = equity;
                    account.ShareMoney = account.Account.Equity * account.SharePercent / 100M;
                    account.ShareMoneyWallet = account.ShareMoney;
                    
                    // посчитать долю в валюте кошелька
                    if (account.Currency == userWallet.Currency) continue;
                    string errorString;
                    var shareWallet = DalSpot.Instance.ConvertSourceCurrencyToTargetCurrency(userWallet.Currency, account.Currency,
                        (double) account.ShareMoney,
                        TradeSharp.Contract.Util.BL.QuoteStorage.Instance.ReceiveAllData(), out errorString);
                    account.ShareMoneyWallet = shareWallet ?? account.ShareMoney;
                    if (errorString != null)
                        Logger.ErrorFormat("GetUserOwnAndSharedAccounts({0}) - перевод средств из {1} в {2}: {3}",
                            login, account.Currency, userWallet.Currency, errorString);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в GetUserOwnAndSharedAccounts", ex);
                return RequestStatus.ServerError;
            }
            return RequestStatus.OK;
        }

        public TransfersByAccountSummary GetTransfersSummary(ProtectedOperationContext secCtx, string login)
        {
            var summInfo = new TransfersByAccountSummary(login);
            if (!UserSessionStorage.Instance.PermitUserOperation(secCtx, false, true))
                return summInfo;

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == login);
                    if (user == null)
                        return summInfo;
                    var rawData = ctx.GetTransfersSummaryWrapped(user.ID);
                    if (rawData == null)
                        return summInfo;

                    foreach (var transType in rawData)
                    {
                        summInfo.TransfersByType[(TransfersByAccountSummary.AccountTransferType) transType.TransferType]
                            = new Cortege2<int, decimal>(transType.TotalCount ?? 0, transType.TotalAmount ?? 0);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в GetTransfersSummary", ex);
            }
            return summInfo;
        }
    }
}

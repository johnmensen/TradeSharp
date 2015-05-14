using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Entity;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Linq;
using TradeSharp.Server.BL;
using TradeSharp.Server.Repository;
using TradeSharp.Util;

namespace TradeSharp.Server.Contract
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class PlatformManager : IPlatformManager
    {
        #region Flood Safe Logger
        private readonly FloodSafeLogger logNoFlood = new FloodSafeLogger(1000);

        private const int LogMsgErrorConvertGross = 1;
        #endregion

        private ManagerTrade managerTrade;

        private IUserSettingsStorage userSettingsStorage;

        private IWalletRepository walletRepository;

        private IAccountRepository accountRepository;

        public PlatformManager()
        {
            managerTrade = ManagerTrade.Instance;
            userSettingsStorage = new UserSettingsStorage();
            walletRepository = new WalletRepository();
            accountRepository = AccountRepository.Instance;
        }

        public string AuthoriseUser(string userLogin, string password, long localTime)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == userLogin && u.Password == password);
                    if (user == null)
                    {
                        return string.Empty;
                    }

                    var hash = CredentialsHash.MakeCredentialsHash(userLogin, password, localTime);
                    return hash;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("AuthoriseUser", ex);
                return string.Empty;
            }
        }

        public string AuthoriseUserWithAccountDetail(string userLogin, string password, long localTime,
                                                     out Account[] userAccounts)
        {
            userAccounts = new Account[0];
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == userLogin && u.Password == password);
                    if (user == null)                    
                        return string.Empty;

                    var hash = CredentialsHash.MakeCredentialsHash(userLogin, password, localTime);
                    userAccounts = ctx.PLATFORM_USER_ACCOUNT.Where(pa => pa.PlatformUser == user.ID).Select(pa => 
                        pa.ACCOUNT1).ToArray().Select(LinqToEntity.DecorateAccount).ToArray();
                    return hash;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("AuthoriseUserWithAccountDetail", ex);
                return string.Empty;
            }
        }

        public Wallet GetUserWalletSubscriptionAndLastPayments(string hash, string userLogin,
            long localTime, int maxPaymentsQuery,
            out int paymentsTotalCount, out List<Subscription> subscriptions,
            out List<Transfer> transfers, out WalletError error)
        {
            paymentsTotalCount = 0;
            subscriptions = null;
            transfers = null;

            PLATFORM_USER user;
            using (var ctx = DatabaseContext.Instance.Make())
            {
                user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == userLogin);
                if (user == null)
                {
                    error = WalletError.AuthenticationError;
                    return null;
                }
            }

            var userHash = CredentialsHash.MakeCredentialsHash(userLogin, user.Password, localTime);
            if (hash != userHash)
            {
                error = WalletError.AuthenticationError;
                return null;
            }

            return walletRepository.GetUserWalletSubscriptionAndLastPaymentsInner(userLogin, maxPaymentsQuery,
                                                                 out paymentsTotalCount, out subscriptions,
                                                                 out transfers, out error);
        }

        public List<Account> GetUserAccounts(string hash, string userLogin, long localTime, out RequestStatus error)
        {
            var accounts = new List<Account>();
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == userLogin);
                    if (user == null)
                    {
                        error = RequestStatus.Unauthorized;
                        return accounts;
                    }

                    var userHash = CredentialsHash.MakeCredentialsHash(userLogin, user.Password, localTime);
                    if (userHash != hash)
                    {
                        error = RequestStatus.Unauthorized;
                        return accounts;
                    }

                    // получить счета пользователя
                    var query = from pa in ctx.PLATFORM_USER_ACCOUNT
                                join ac in ctx.ACCOUNT on pa.Account equals ac.ID
                                where pa.RightsMask == (int)AccountRights.Управление && pa.PlatformUser == user.ID
                                select new Account
                                {
                                    ID = ac.ID,
                                    Group = ac.AccountGroup,
                                    Balance = ac.Balance,
                                    Currency = ac.Currency,
                                    MaxLeverage = (float)ac.MaxLeverage,
                                    Status = (Account.AccountStatus)ac.Status
                                };
                    // ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var ac in query)
                        accounts.Add(ac);
                    // ReSharper restore LoopCanBeConvertedToQuery
                    error = RequestStatus.OK;
                    return accounts;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error in GetUserAccounts()", ex);
                error = RequestStatus.ServerError;
                return accounts;
            }
        }

        public List<Subscription> GetUserSubscriptions(string hash, string userLogin, long localTime, out RequestStatus error)
        {
            List<Subscription> cats;
            using (var ctx = DatabaseContext.Instance.Make())
                error = GetUserSubscribedCats(ctx, userLogin, hash, localTime, out cats);
            return cats;
        }

        private RequestStatus GetUserSubscribedCats(TradeSharpConnection ctx,
            string userLogin, string hash, long localTime,
            out List<Subscription> categories)
        {
            categories = new List<Subscription>();
            try
            {
                var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == userLogin);
                if (user == null)
                    return RequestStatus.Unauthorized;

                var userHash = CredentialsHash.MakeCredentialsHash(userLogin, user.Password, localTime);
                if (userHash != hash)
                    return RequestStatus.Unauthorized;

                // получить подписку на торг. сигналы и собственные сигналы пользователя
                categories = (from uc in ctx.SUBSCRIPTION_V
                              where uc.User == user.ID
                              select uc).ToList().Select(LinqToEntity.DecorateSubscription).ToList();
                return RequestStatus.OK;
            }
            catch (Exception ex)
            {
                Logger.Error("Error in GetUserSubscribedCats()", ex);
                return RequestStatus.ServerError;
            }
        }

        public WalletError SubscribeOnTradeSignal(
            string hash, string userLogin, long localTime, int serviceId,
            bool tradeAuto, bool enableHedgingOrders, int percentLeverage,
            int maxVolume, int minVolume,
            int volumeStep, double maxLeverage)
        {
            var tradeSets = new AutoTradeSettings
            {
                TradeAuto = tradeAuto,
                HedgingOrdersEnabled = enableHedgingOrders,
                PercentLeverage = percentLeverage,
                MaxVolume = maxVolume,
                MinVolume = minVolume,
                StepVolume = volumeStep,
                MaxLeverage = maxLeverage
            };

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == userLogin);
                    if (user == null)
                    {
                        Logger.InfoFormat("PlatformManager.SubscribeOnTradeSignal(usr={0} not found)",
                            userLogin);
                        return WalletError.InvalidData;
                    }
                    var userHash = CredentialsHash.MakeCredentialsHash(userLogin, user.Password, localTime);
                    if (userHash != hash)
                        return WalletError.AuthenticationError;

                    WalletError error;
                    walletRepository.SubscribeOnService(ctx, user.ID, serviceId, true,
                                                             false, tradeSets, out error);
                    if (error != WalletError.OK)
                        Logger.InfoFormat("PlatformManager.SubscribeOnTradeSignal(usr={0}, login={1}, srv={2}): {3}",
                            user.ID, userLogin, serviceId, error);
                    return error;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error in SubscribeOnTradeSignal()", ex);
                return WalletError.ServerError;
            }
        }

        public WalletError UnsubscribeFromTradeSignal(string hash, string userLogin, long localTime, int serviceId)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == userLogin);
                    if (user == null)
                        return WalletError.AuthenticationError;

                    var userHash = CredentialsHash.MakeCredentialsHash(userLogin, user.Password, localTime);
                    if (userHash != hash)
                        return WalletError.AuthenticationError;

                    WalletError error;
                    walletRepository.SubscribeOnService(ctx, user.ID, serviceId, false, true,
                                                             new AutoTradeSettings(), out error);
                    return error;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error in UnsubscribeFromTradeSignal()", ex);
                return WalletError.ServerError;
            }
        }

        public PaidServiceDetail GetPaidServiceDetail(int serviceId)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var service = ctx.SERVICE.First(s => s.ID == serviceId);
                    if (service == null) return null;

                    var srvDec = LinqToEntity.DecoratePaidService(service);
                    var detail = new PaidServiceDetail
                    {
                        Currency = service.Currency,
                        DayFee = service.FixedPrice,
                        MonthFee = srvDec.FixedPriceMonth,
                        AccountId = service.AccountId
                    };

                    var signalOwner = LinqToEntity.DecoratePlatformUser(ctx.PLATFORM_USER.First(u => u.ID == service.User));
                    detail.UserName = signalOwner.MakeNameWithInitials();

                    if (srvDec.AccountId.HasValue)
                    {
                        var account = LinqToEntity.DecorateAccount(ctx.ACCOUNT.First(a => a.ID == srvDec.AccountId));
                        detail.AccountGroup = account.Group;
                    }

                    // информация по торговым сигналам
                    var userSignal =
                        ctx.SERVICE.FirstOrDefault(us => us.User == service.User);
                    if (userSignal == null)
                        return detail;
                    detail.SignalTitle =
                        string.IsNullOrEmpty(userSignal.Comment) ? "Сигналы №" + userSignal.ID : userSignal.Comment;
                    var subsCount = ctx.SUBSCRIPTION.Count(us => us.Service == userSignal.ID);
                    detail.SubscribersCount = subsCount;

                    return detail;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error in GetPaidServiceDetail()", ex);
                return null;
            }
        }

        public PlatformUser GetUserFullInfo(string hash, string userLogin, long localTime, out List<Account> accounts)
        {
            using (var ctx = DatabaseContext.Instance.Make())
            {
                var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == userLogin);
                if (user == null)
                {
                    accounts = null;
                    return null;
                }
                var userHash = CredentialsHash.MakeCredentialsHash(userLogin, user.Password, localTime);
                if (hash != userHash)
                {
                    accounts = null;
                    return null;
                }
                accounts =
                    (from pua in ctx.PLATFORM_USER_ACCOUNT where pua.PlatformUser == user.ID select pua.ACCOUNT1).ToList
                        ().Select(LinqToEntity.DecorateAccount).ToList();
                return LinqToEntity.DecoratePlatformUser(user);
            }
        }

        public RequestStatus SubscribeOnPortfolio(string hash, string userLogin, long localTime,
                                                  int portfolioId, AutoTradeSettings tradeAutoSettings)
        {
            var portfolio = new TopPortfolio
            {
                Id = portfolioId
            };
            return SubscribeOnUserOrCustomPortfolio(hash, userLogin, localTime, portfolio, tradeAutoSettings);
        }

        public RequestStatus SubscribeOnCustomPortfolio(string hash, string userLogin, long localTime,
                                                        string formula, float? marginValue, int topCount,
                                                        AutoTradeSettings tradeAutoSettings)
        {
            var portfolio = new TopPortfolio
            {
                Criteria = formula,
                MarginValue = marginValue,
                ParticipantCount = topCount
            };
            return SubscribeOnUserOrCustomPortfolio(hash, userLogin, localTime, portfolio, tradeAutoSettings);
        }

        private RequestStatus SubscribeOnUserOrCustomPortfolio(string hash, string userLogin, long localTime,
            TopPortfolio portfolio,
            AutoTradeSettings tradeAutoSettings)
        {
            if (portfolio == null)
            {
                Logger.Error("SubscribeOnUserOrCustomPortfolio(null)");
                return RequestStatus.BadRequest;
            }

            Logger.InfoFormat("SubscribeOnUserOrCustomPortfolio({0}, портфель {1})", userLogin,
                portfolio.Id > 0 ? "#" + portfolio.Id : portfolio.Criteria);
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == userLogin);
                    if (user == null)
                        return RequestStatus.Unauthorized;

                    var userHash = CredentialsHash.MakeCredentialsHash(userLogin, user.Password, localTime);
                    if (userHash != hash)
                        return RequestStatus.Unauthorized;

                    var status = walletRepository.SubscribeUserOnPortfolio(ctx, userLogin, portfolio, null, tradeAutoSettings);
                    if (status != RequestStatus.OK)
                        Logger.Info("SubscribeOnUserOrCustomPortfolio: status: " + status);
                    return status;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в SubscribeOnUserOrCustomPortfolio()", ex);
                return RequestStatus.ServerError;
            }
        }

        public RequestStatus UnsubscribePortfolio(string hash, string userLogin, long localTime,
                                                  bool deleteSubscriptions)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == userLogin);
                    if (user == null)
                        return RequestStatus.Unauthorized;

                    var userHash = CredentialsHash.MakeCredentialsHash(userLogin, user.Password, localTime);
                    if (userHash != hash)
                        return RequestStatus.Unauthorized;

                    Logger.InfoFormat("UnsubscribePortfolio({0}) in progress", userLogin);

                    return walletRepository.UnsubscribeUserFromPortfolio(ctx, userLogin, true, deleteSubscriptions);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в UnsubscribePortfolio()", ex);
                return RequestStatus.ServerError;
            }
        }

        public RequestStatus GetUserPortfolioAndSubscriptions(string hash, string userLogin, long localTime,
            out List<Subscription> signals, out TopPortfolio portfolio)
        {
            portfolio = null;

            using (var ctx = DatabaseContext.Instance.Make())
            {
                var error = GetUserSubscribedCats(ctx, userLogin, hash, localTime, out signals);
                if (error != RequestStatus.OK)
                    return error;

                try
                {
                    var query = from us in ctx.PLATFORM_USER
                                join up in ctx.USER_TOP_PORTFOLIO on us.ID equals up.User
                                join pf in ctx.TOP_PORTFOLIO on up.Portfolio equals pf.Id
                                where us.Login == userLogin
                                select new TopPortfolio
                                {
                                    Id = pf.Id,
                                    ParticipantCount = pf.ParticipantCount,
                                    Criteria = pf.Criteria,
                                    MarginValue = (float?)pf.MarginValue,
                                    DescendingOrder = pf.DescendingOrder,
                                    ManagedAccount = pf.ManagedAccount,
                                    Name = pf.Name,
                                    OwnerUser = pf.OwnerUser
                                };

                    portfolio = query.FirstOrDefault();
                    return RequestStatus.OK;
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка в GetUserPortfolioAndSubscriptions", ex);
                    return RequestStatus.ServerError;
                }
            }
        }

        public int SendCloseRequests(int[] orderIds, PositionExitReason reason)
        {
            RequestStatus status;
            return SendCloseRequestsInner(orderIds, reason, out status);
        }

        public RequestStatus SendCloseRequest(int orderId, PositionExitReason reason)
        {
            RequestStatus status;
            SendCloseRequestsInner(new[] { orderId }, reason, out status);
            return status;
        }

        private int SendCloseRequestsInner(int[] orderId, PositionExitReason reason, out RequestStatus lastStatus)
        {
            var countOk = 0;
            lastStatus = RequestStatus.OK;
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    foreach (var posId in orderId)
                    {
                        var pos = ctx.POSITION.FirstOrDefault(p => p.ID == posId);
                        lastStatus = pos == null ? RequestStatus.NotFound : managerTrade.SendCloseRequest(pos, reason);
                        if (lastStatus == RequestStatus.OK)
                            countOk++;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в PlatformManager.SendCloseRequest", ex);
                lastStatus = RequestStatus.ServerError;
            }
            return countOk;
        }

        public AccountRegistrationResponse RegisterAccount(
            string login,
            string password,
            string email,
            string firstName,
            string lastName,
            string patronym,
            string phone,
            string currency, int startBalance)
        {
            var usr = new PlatformUser
            {
                Login = login,
                Password = password,
                Email = email,
                Name = firstName,
                Surname = lastName,
                Patronym = patronym,
                Phone1 = phone,
                RegistrationDate = DateTime.Now
            };

            var resp = new AccountRegistrationResponse();
            if (currency != "USD")
                resp.errors.Add("Поддерживаемые валюты: " + "USD");
            //if (!PlatformUser.CheckLoginSpelling(usr.Login))
            //    resp.errors.Add("Логин задан некорректно");
            if (resp.errors.Count > 0)
                return resp;

            var status = ManagerAccount.Instance.RegisterAccount(usr,
                currency, startBalance < 1000 ? 1000 : startBalance, 0, usr.Password, false);
            resp.status = status;
            resp.statusName = EnumFriendlyName<AccountRegistrationStatus>.GetString(status);

            return resp;
        }

        public List<string> GetSubscriberEmails(int signallerId)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var masterSignal = ctx.SERVICE.FirstOrDefault(s => s.User == signallerId);
                    if (masterSignal == null) return new List<string>();

                    var query = from sig in ctx.SUBSCRIPTION
                                join user in ctx.PLATFORM_USER on sig.User equals user.ID
                                where sig.Service == masterSignal.ID
                                select user.Email;
                    return query.ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка в GetSubscriberEmails({0}): {1}", signallerId, ex);
                return new List<string>();
            }
        }

        public List<string> GetSubscriberEmailsByAccount(int accountId)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var masterSignal = (from sig in ctx.SERVICE
                                        join user in ctx.PLATFORM_USER on sig.User equals user.ID
                                        join pa in ctx.PLATFORM_USER_ACCOUNT on user.ID equals pa.PlatformUser
                                        where pa.Account == accountId
                                        select sig.ID).FirstOrDefault();

                    if (masterSignal == default(int)) return new List<string>();

                    var query = from sig in ctx.SUBSCRIPTION
                                join user in ctx.PLATFORM_USER on sig.User equals user.ID
                                where sig.Service == masterSignal
                                select user.Email;
                    return query.ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка в GetSubscriberEmailsByAccount({0}): {1}", accountId, ex);
                return new List<string>();
            }
        }

        public List<MarketOrder> GetAllOpenedOrders(int count)
        {
            if (count <= 0)
                return new List<MarketOrder>();

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    return ctx.POSITION.OrderByDescending(p => p.TimeEnter).Take(count).ToList().Select(LinqToEntity.DecorateOrder).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error in GetAllOpenedOrders()", ex);
                return null;
            }
        }

        public PlatformVolumesSummary GetPairVolumes(string semicolonSeparatedTickersListString)
        {
            var tikersToStore = 
                string.IsNullOrEmpty(semicolonSeparatedTickersListString) 
                ? new string[0]
                : semicolonSeparatedTickersListString.Trim().Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            
            try
            {
                var nowTime = DateTime.Now;
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var volumes = ctx.GetVolumesByTikerWrapped().Select(v => new TotalOpenedVolumesByTicker
                        {
                            Ticker = v.Symbol,
                            TotalBuy = v.Buy ?? 0,
                            TotalSell = v.Sell ?? 0,
                            TimeMeasured = nowTime
                        });

                    var total = new PlatformVolumesSummary(
                        tikersToStore.Length == 0
                               ? volumes.ToArray()
                               : volumes.Where(v => tikersToStore.Any(t => t.Equals(v.Ticker, 
                                   StringComparison.InvariantCultureIgnoreCase))).ToArray());

                    // посчитать TotalGrossInUsd
                    var quotes = QuoteStorage.Instance.ReceiveAllData();
                    var gross = 0L;
                    foreach (var pair in total.TotalVolumesByTicker)
                    {
                        var sumBuySell = pair.TotalBuy + pair.TotalSell;
                        if (sumBuySell == 0) continue;
                        string errorStr;
                        var volumeUsd = DalSpot.Instance.ConvertToTargetCurrency(pair.Ticker, true, "USD", sumBuySell, quotes,
                                                                 out errorStr);
                        if (!string.IsNullOrEmpty(errorStr))
                            logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogMsgErrorConvertGross, 1000 * 60 * 20,
                                "GetPairVolumes - ошибка конвертации {0}: {1}", pair.Ticker, errorStr);
                        gross += (long) (volumeUsd ?? 0);
                    }
                    total.TotalGrossInUsd = gross;
                    return total;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в GetPairVolumes()", ex);
                return new PlatformVolumesSummary(new TotalOpenedVolumesByTicker[0]);
            }
        }
    
        public List<string> GetTickersTraded(string hash, string userLogin, int accountId,
            long localTime)
        {
            return accountRepository.GetTickersTradedCheckCredentials(hash, userLogin, accountId, localTime, true);
        }

        

        public List<MarketOrder> GetClosedOrders(string hash, string userLogin, long localTime,
            int accountId, string optionalSymbolFilter, int startId, int maxCount)
        {
            return accountRepository.GetClosedOrOpenedOrdersCheckCredentials(hash, userLogin, localTime,
                accountId, optionalSymbolFilter, startId, maxCount, true, true);
        }

        public List<MarketOrder> GetOpenOrdersByAccount(string hash, string userLogin, long localTime,
                                                        int accountId, string optionalSymbolFilter, int startId,
                                                        int maxCount)
        {
            return accountRepository.GetClosedOrOpenedOrdersCheckCredentials(hash, userLogin, localTime,
                accountId, optionalSymbolFilter, startId, maxCount, true, false);
        }

        public Account GetAccountDetail(string hash, string userLogin, long localTime, 
            int accountId, bool calculateEquity,
            out decimal brokerLeverage, out decimal exposure)
        {
            brokerLeverage = 0;
            exposure = 0;

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == userLogin);
                    if (user == null) return null;

                    var userHash = CredentialsHash.MakeCredentialsHash(userLogin, user.Password, localTime);
                    if (userHash != hash)
                        return null;

                    // авторизован?
                    if (!PlatformUser.IsManager(user.RoleMask) && !PlatformUser.IsAdmin(user.RoleMask))
                        if (!ctx.PLATFORM_USER_ACCOUNT.Any(pa => pa.PlatformUser == user.ID && pa.Account == accountId))
                            return null;

                    var account = LinqToEntity.DecorateAccount(ctx.ACCOUNT.First(a => a.ID == accountId));
                    if (!calculateEquity) return account;
                    
                    // получить открытые ордера по счету
                    var orders = ctx.POSITION.Where(p => p.AccountID == accountId && p.State ==
                        (int) PositionState.Opened).ToList().Select(LinqToEntity.DecorateOrder).ToList();
                    
                    // посчитать открытый результат и экспозицию в валюте депо
                    var quotes = QuoteStorage.Instance.ReceiveAllData();
                    var errors = new List<string>();
                    exposure = DalSpot.Instance.CalculateExposure(orders, quotes, account.Currency, errors);
                    var group = ctx.ACCOUNT_GROUP.First(g => g.Code == account.Group);
                    brokerLeverage = group.BrokerLeverage;
                    account.UsedMargin = exposure / brokerLeverage;
                    return account;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в GetAccountDetail()", ex);
                return null;
            }
        }

        public RequestStatus OpenPosition(string hash, string userLogin, long localTime, int accountId,
            string symbol, int volume, int side, float stopLoss, float takeProfit, int magic, string comment)
        {
            var status = CheckCredentials(hash, userLogin, localTime, accountId, true);
            if (status != RequestStatus.OK) return status;

            status = managerTrade.SendNewOrderRequest(ProtectedOperationContext.MakeServerSideContext(),
                                             RequestUniqueId.Next(), new MarketOrder
                                                 {
                                                     AccountID = accountId,
                                                     Symbol = symbol,
                                                     Volume = volume,
                                                     Side = side,
                                                     StopLoss = stopLoss == 0 ? (float?) null : stopLoss,
                                                     TakeProfit = takeProfit == 0 ? (float?) null : takeProfit,
                                                     Magic = magic,
                                                     Comment = comment
                                                 },
                                             OrderType.Market,
                                             0, 0);
            return status;
        }

        public RequestStatus ClosePosition(string hash, string userLogin, long localTime, int accountId,
                                           int positionId)
        {
            var status = CheckCredentials(hash, userLogin, localTime, accountId, true);
            if (status != RequestStatus.OK) return status;

            status = managerTrade.SendCloseRequest(ProtectedOperationContext.MakeServerSideContext(),
                                                   accountId, positionId, PositionExitReason.ClosedFromUI);
            return status;
        }

        public RequestStatus EditPosition(string hash, string userLogin, long localTime, int accountId,
            int orderId,
            float stopLoss, float takeProfit, int magic, string comment)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var status = CheckCredentials(hash, userLogin, localTime, accountId, true, ctx);
                    if (status != RequestStatus.OK) return status;

                    var pos = ctx.POSITION.FirstOrDefault(p => p.AccountID == accountId && p.ID == orderId);
                    if (pos == null)
                        return RequestStatus.IncorrectData;

                    var order = LinqToEntity.DecorateOrder(pos);
                    order.StopLoss = stopLoss == 0 ? (float?) null : stopLoss;
                    order.TakeProfit = takeProfit == 0 ? (float?) null : takeProfit;
                    order.Magic = magic;
                    order.Comment = comment;

                    status = managerTrade.SendEditMarketRequest(ProtectedOperationContext.MakeServerSideContext(), order);
                    return status;
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка в EditPosition(user={0}, pos={1}): {2}",
                    userLogin, orderId, ex);
                return RequestStatus.ServerError;
            }
        }

        private RequestStatus CheckCredentials(string hash, string userLogin, long localTime, int accountId, bool checkTradeRights,
            TradeSharpConnection connection = null)
        {
            try
            {
                var ctx = connection ?? DatabaseContext.Instance.Make();
                try
                {
                    var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == userLogin);
                    if (user == null) return RequestStatus.IncorrectData;

                    var userHash = CredentialsHash.MakeCredentialsHash(userLogin, user.Password, localTime);
                    if (userHash != hash)
                        return RequestStatus.IncorrectData;

                    if (!ctx.PLATFORM_USER_ACCOUNT.Any(pa => pa.PlatformUser == user.ID && pa.Account == accountId &&
                                                             (!checkTradeRights ||
                                                              pa.RightsMask == (int) AccountRights.Управление)))
                        return RequestStatus.Unauthorized;
                }
                finally
                {
                    if (connection == null)
                        ctx.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в CheckCredentials()", ex);
                return RequestStatus.ServerError;
            }
            return RequestStatus.OK;
        }

        public void SaveUserSettingsString(string hash, string userLogin, long localTime, string settingsString)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())

                {
                    var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == userLogin);
                    if (user == null) return;

                    var userHash = CredentialsHash.MakeCredentialsHash(userLogin, user.Password, localTime);
                    if (userHash != hash) return;

                    userSettingsStorage.SaveUserSettings(user.ID, settingsString);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в SaveUserSettingsString()", ex);
            }
        }

        public string LoadUserSettingsString(string hash, string userLogin, long localTime)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == userLogin);
                    if (user == null) return string.Empty;

                    var userHash = CredentialsHash.MakeCredentialsHash(userLogin, user.Password, localTime);
                    if (userHash != hash) return string.Empty;

                    return userSettingsStorage.LoadUserSettings(user.ID);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в LoadUserSettingsString()", ex);
            }
            return string.Empty;
        }

        public bool ModifyOrder(MarketOrder order, out string errorString)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var pos = ctx.POSITION.FirstOrDefault(p => p.ID == order.ID);
                    if (pos != null)
                        return ModifyOpenedPosition(order, ctx, pos, out errorString);
                    var closed = ctx.POSITION_CLOSED.FirstOrDefault(p => p.ID == order.ID);
                    if (closed != null)
                        return ModifyClosedPosition(order, ctx, closed, out errorString);
                }
            }
            catch (Exception ex)
            {
                errorString = ex.GetType().Name + ": " + ex.Message;
                return false;
            }
            errorString = "Ордер " + order.ID + " не найден";
            return false;
        }

        public bool SetBalance(string hash, string userLogin, long localTime,
            int accountId, decimal newBalance, string comment, out string errorString)
        {
            errorString = string.Empty;
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == userLogin);
                    if (user == null)
                    {
                        errorString = "Unauthorised (not found)";
                        return false;
                    }

                    var userHash = CredentialsHash.MakeCredentialsHash(userLogin, user.Password, localTime);
                    if (userHash != hash)
                    {
                        errorString = "Unauthorised (wrong credentials)";
                        return false;
                    }

                    if (!PlatformUser.IsAdmin(user.RoleMask) &&
                        !PlatformUser.IsManager(user.RoleMask))
                    {
                        errorString = "Unauthorised (insufficient rights)";
                        return false;
                    }

                    var account = ctx.ACCOUNT.FirstOrDefault(a => a.ID == accountId);
                    if (account == null)
                    {
                        errorString = "Account " + accountId + " was not found";
                        return false;
                    }

                    var delta = newBalance - account.Balance;
                    if (delta == 0)
                        return true;

                    // сформировать транзакцию и поправить баланс
                    var sign = Math.Sign(delta);
                    var amount = Math.Abs(delta);
                    var trans = new BALANCE_CHANGE
                    {
                        AccountID = accountId,
                        ChangeType = sign > 0 ? (int) BalanceChangeType.Deposit : (int) BalanceChangeType.Withdrawal,
                        Amount = amount,
                        ValueDate = DateTime.Now,
                        Description = comment
                    };
                    account.Balance = newBalance;
                    ctx.BALANCE_CHANGE.Add(trans);
                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в SetBalance()", ex);
            }
            return false;
        }

        private bool ModifyOpenedPosition(MarketOrder order, TradeSharpConnection ctx, POSITION pos, 
            out string errorString)
        {
            errorString = string.Empty;
            if (order.IsClosed)
            {
                var closed = new POSITION_CLOSED
                {
                    AccountID = pos.AccountID,
                    ID = pos.ID,
                    Comment = order.Comment,
                    ExitReason = (int) order.ExitReason,
                    Magic = order.Magic,
                    ExpertComment = order.ExpertComment,
                    PendingOrderID = order.PendingOrderID,
                    PriceBest = (decimal?) order.PriceBest,
                    PriceEnter = (decimal) order.PriceEnter,
                    PriceExit = (decimal) (order.PriceExit ?? 0),
                    PriceWorst = (decimal?) order.PriceWorst,
                    ResultBase = (decimal) order.ResultBase,
                    ResultDepo = (decimal) order.ResultDepo,
                    ResultPoints = (decimal) order.ResultPoints,
                    Side = order.Side,
                    Stoploss = (decimal?) order.StopLoss,
                    Swap = (decimal) (order.Swap ?? 0),
                    Symbol = order.Symbol,
                    Takeprofit = (decimal?) order.TakeProfit,
                    TimeEnter = order.TimeEnter,
                    TimeExit = order.TimeExit ?? default(DateTime),
                    Volume = order.Volume
                };
                ctx.POSITION.Remove(pos);
                ctx.POSITION_CLOSED.Add(closed);
                return true;
            }
            pos.Comment = order.Comment;
            pos.Magic = order.Magic;
            pos.ExpertComment = order.ExpertComment;
            pos.PendingOrderID = order.PendingOrderID;
            pos.PriceBest = (decimal?) order.PriceBest;
            pos.PriceEnter = (decimal) order.PriceEnter;
            pos.PriceWorst = (decimal?) order.PriceWorst;
            pos.Side = order.Side;
            pos.Stoploss = (decimal?) order.StopLoss;
            pos.Symbol = order.Symbol;
            pos.Takeprofit = (decimal?) order.TakeProfit;
            pos.TimeEnter = order.TimeEnter;
            pos.Volume = order.Volume;
            pos.State = (int) order.State;
            pos.MasterOrder = order.MasterOrder;
            ctx.SaveChanges();
            return true;
        }

        private bool ModifyClosedPosition(MarketOrder order, TradeSharpConnection ctx, POSITION_CLOSED pos,
            out string errorString)
        {
            errorString = string.Empty;
            if (order.IsClosed)
            {
                var opened = new POSITION
                {
                    AccountID = pos.AccountID,
                    ID = pos.ID,
                    Comment = order.Comment,
                    Magic = order.Magic,
                    ExpertComment = order.ExpertComment,
                    PendingOrderID = order.PendingOrderID,
                    PriceBest = (decimal?)order.PriceBest,
                    PriceEnter = (decimal)order.PriceEnter,
                    PriceWorst = (decimal?)order.PriceWorst,
                    Side = order.Side,
                    Stoploss = (decimal?)order.StopLoss,
                    Symbol = order.Symbol,
                    Takeprofit = (decimal?)order.TakeProfit,
                    TimeEnter = order.TimeEnter,
                    Volume = order.Volume,                  
                    State = (int) order.State,
                    MasterOrder = order.MasterOrder
                };
                ctx.POSITION_CLOSED.Remove(pos);
                ctx.POSITION.Add(opened);
                return true;
            }
            pos.Comment = order.Comment;
            pos.Magic = order.Magic;
            pos.ExpertComment = order.ExpertComment;
            pos.PendingOrderID = order.PendingOrderID;
            pos.PriceBest = (decimal?)order.PriceBest;
            pos.PriceEnter = (decimal)order.PriceEnter;
            pos.PriceWorst = (decimal?)order.PriceWorst;
            pos.Side = order.Side;
            pos.Stoploss = (decimal?)order.StopLoss;
            pos.Symbol = order.Symbol;
            pos.Takeprofit = (decimal?)order.TakeProfit;
            pos.TimeEnter = order.TimeEnter;
            pos.Volume = order.Volume;
            pos.ExitReason = (int) order.ExitReason;
            pos.PriceExit = (decimal) (order.PriceExit ?? 0);
            pos.PriceWorst = (decimal?) order.PriceWorst;
            pos.ResultBase = (decimal)order.ResultBase;
            pos.ResultDepo = (decimal)order.ResultDepo;
            pos.ResultPoints = (decimal)order.ResultPoints;
            pos.TimeExit = order.TimeExit ?? default(DateTime);
            pos.Swap = (decimal) (order.Swap ?? 0);
            ctx.SaveChanges();
            return true;
        }        
    }
}

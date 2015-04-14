using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using Entity;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Linq;
using TradeSharp.Processing.Lib;
using TradeSharp.Server.BL;
using TradeSharp.Server.Repository;
using TradeSharp.TradeLib;
using TradeSharp.Util;

namespace TradeSharp.Server.Contract
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public partial class WalletManager : IWalletManager
    {
        private IWalletRepository walletRepository;

        private IProfitCalculator profitCalculator;

        private IAccountRepository accountRepository;

        private static readonly Lazy<WalletManager> instance = new Lazy<WalletManager>(() => new WalletManager());

        public WalletManager()
        {
            walletRepository = new WalletRepository();
            profitCalculator = ProfitCalculator.Instance;
            accountRepository = AccountRepository.Instance;
        }

        private const int MaxFixedPricePerDayUSD = 200;

        private const string MaxFixedPriceCheckCurrency = "USD";

        public Wallet GetUserWallet(ProtectedOperationContext secCtx, string userLogin)
        {
            // запрос разрешен?
            if (!UserSessionStorage.Instance.PermitUserOperation(secCtx, false, false))
                return null;

            // получить кошелек пользователя и вернуть его
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var queryWallet = from us in ctx.PLATFORM_USER where us.Login == userLogin select us.WALLET;

                    foreach (var walInf in queryWallet)
                    {
                        var wallet = LinqToEntity.DecorateWallet(walInf);
                        return wallet;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("GetUserWallet({0}) - exception: {1}", userLogin, ex);
            }

            return null;
        }

        public Wallet GetUserWalletSubscriptionAndLastPayments(ProtectedOperationContext secCtx,
                                                               string userLogin, int maxPaymentsQuery,
                                                               out int paymentsTotalCount,
                                                               out List<Subscription> subscriptions,
                                                               out List<Transfer> transfers, out WalletError error)
        {
            // запрос разрешен?
            if (!UserSessionStorage.Instance.PermitUserOperation(secCtx, false, false))
            {
                subscriptions = null;
                transfers = null;
                paymentsTotalCount = 0;
                error = WalletError.InsufficientRights;
                return null;
            }

            return walletRepository.GetUserWalletSubscriptionAndLastPaymentsInner(userLogin, maxPaymentsQuery,
                                                                 out paymentsTotalCount, out subscriptions,
                                                                 out transfers, out error);
        }

        public List<Transfer> GetAllUserPayments(ProtectedOperationContext secCtx, PlatformUser user, out WalletError error)
        {
            if (!UserSessionStorage.Instance.PermitUserOperation(secCtx, false, false))
            {
                error = WalletError.InsufficientRights;
                return null;
            }

            error = WalletError.ServerError;
            var transfers = new List<Transfer>();

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var queryTrans = (from trans in ctx.TRANSFER where trans.User == user.ID select trans).ToList().Select(
                        LinqToEntity.DecorateTransfer).ToList();
                }

                return transfers;
            }
            catch (Exception ex)
            {
                Logger.Error("GetAllUserPayments() error", ex);
                error = WalletError.ServerError;
                return null;
            }
        }

        public bool RegisterOrUpdateService(ProtectedOperationContext secCtx, PaidService service, out WalletError error)
        {
            if (!UserSessionStorage.Instance.PermitUserOperation(secCtx, false, false))
            {
                error = WalletError.InsufficientRights;
                return false;
            }

            // проверить сервис - 1 000 000 USD в день, к примеру, перебор
            // одному пользователю недозволительно высталять 10 сервисов
            // счет должен быть реальным

            if (service.ServiceType == PaidServiceType.PAMM ||
                service.ServiceType == PaidServiceType.Signals)
            {
                // проверить  - реальный счет?
                if (!service.AccountId.HasValue)
                {
                    error = WalletError.InvalidData;
                    return false;
                }

                if (service.FixedPrice < 0)
                {
                    error = WalletError.InvalidData;
                    return false;
                }

                try
                {
                    using (var ctx = DatabaseContext.Instance.Make())
                    {
                        var account = ctx.ACCOUNT.FirstOrDefault(a => a.ID == service.AccountId.Value);
                        if (account == null)
                        {
                            error = WalletError.InvalidData;
                            return false;
                        }

                        service.Currency = account.Currency;

                        // перевести сумму в USD - если она велика - вернуть false
                        // перевести стоимость сервиса в USD
                        var fixedPrice = service.FixedPrice;
                        if (account.Currency != MaxFixedPriceCheckCurrency)
                        {
                            string errorStr;
                            var fixedPriceTarget = DalSpot.Instance.ConvertSourceCurrencyToTargetCurrency(MaxFixedPriceCheckCurrency,
                                account.Currency, (double)service.FixedPrice, QuoteStorage.Instance.ReceiveAllData(), out errorStr);
                            if (!fixedPriceTarget.HasValue)
                            {
                                error = WalletError.InvalidData;
                                return false;
                            }
                            fixedPrice = fixedPriceTarget.Value;
                        }

                        if (fixedPrice > MaxFixedPricePerDayUSD)
                        {
                            error = WalletError.InvalidData;
                            return false;
                        }

                        // проверить, солько сервисов зарегистрировал пользователь?
                        // заодно проверить - нет ли сервиса того же типа с указанием того же счета
                        var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.ID == service.User);
                        if (user == null)
                        {
                            error = WalletError.InvalidData;
                            return false;
                        }

                        var existService = ctx.SERVICE.FirstOrDefault(s => s.User == user.ID &&
                            s.ServiceType == (int)service.ServiceType);

                        // обновить сервис
                        if (existService != null)
                        {
                            service.Id = existService.ID;
                            existService.AccountId = service.AccountId;
                            existService.Comment = service.Comment;
                            existService.FixedPrice = service.FixedPrice;
                            existService.Currency = service.Currency;
                            // прогрессивная шкала оплаты
                            if (!CreateOrUpdateServiceFeeRecords(service, ctx, true))
                            {
                                error = WalletError.ServerError;
                                return false;
                            }
                            ctx.SaveChanges();
                            error = WalletError.OK;
                            return true;
                        }

                        // создать новый сервис
                        var srvNew = ctx.SERVICE.Add(new SERVICE
                        {
                            User = service.User,
                            AccountId = service.AccountId,
                            Comment = service.Comment,
                            FixedPrice = service.FixedPrice,
                            Currency = service.Currency,
                            ServiceType = (short)service.ServiceType
                        });
                        Logger.InfoFormat("New service added: user={0}, account={1}, type={2}, price={3} {4}",
                            service.User, service.AccountId, service.ServiceType, service.FixedPrice.ToStringUniformMoneyFormat(), service.Currency);
                        ctx.SaveChanges();
                        service.Id = srvNew.ID;
                        // прогрессивная шкала оплаты
                        if (!CreateOrUpdateServiceFeeRecords(service, ctx, false))
                        {
                            error = WalletError.ServerError;
                            return false;
                        }

                        error = WalletError.OK;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("RegisterOrUpdateService error", ex);
                }
            }

            // не ПАММ и не сигналы...
            error = WalletError.CommonError;
            return false;
        }

        private bool CreateOrUpdateServiceFeeRecords(PaidService service, TradeSharpConnection ctx, bool existedService)
        {
            if (service.serviceRates == null)
                service.serviceRates = new List<PaidServiceRate>();
            try
            {
                if ((service.serviceRates == null || service.serviceRates.Count == 0) && !existedService)
                    return true;

                // сервис уже был зарегистрирован?
                if (existedService)
                {
                    var rates = ctx.SERVICE_RATE.Where(r => r.Service == service.Id).ToList();
                    if (rates.Count == 0 && service.serviceRates.Count == 0)
                        return true;
                    // рейты не изменились?
                    if (rates.Count == service.serviceRates.Count)
                        if (rates.All(r => service.serviceRates.Any(sr =>
                                                                    sr.Amount == r.Amount &&
                                                                    sr.UserBalance == r.UserBalance &&
                                                                    (int)sr.RateType == r.RateType)))
                            return true;
                    // удалить имеющиеся рейты
                    foreach (var rate in rates)
                        ctx.SERVICE_RATE.Remove(rate);
                }

                // добавить рейты
                foreach (var rate in service.serviceRates)
                    ctx.SERVICE_RATE.Add(new SERVICE_RATE
                    {
                        Amount = rate.Amount,
                        Service = service.Id,
                        UserBalance = rate.UserBalance,
                        RateType = (int)rate.RateType
                    });

                ctx.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("CreateOrUpdateServiceFeeRecords() error", ex);
                return false;
            }
        }

        /// <summary>
        /// отключить сервис
        /// отключить клиентам торговые сигналы - вывести их из ПАММ-а и т.п.
        /// </summary>
        public bool DisableService(ProtectedOperationContext secCtx, int serviceId, out WalletError error)
        {
            if (!UserSessionStorage.Instance.PermitUserOperation(secCtx, false, false))
            {
                error = WalletError.InsufficientRights;
                return false;
            }

            // найти сервис - и - удалить
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    // собственно сервис
                    var service = ctx.SERVICE.FirstOrDefault(s => s.ID == serviceId);
                    if (service == null)
                    {
                        error = WalletError.InvalidData;
                        return false;
                    }

                    // отписать всех подписчиков
                    var subs = ctx.SUBSCRIPTION.Where(s => s.Service == serviceId).ToList();
                    foreach (var sub in subs)
                        if (!walletRepository.UnsubscribeSubscriber(ctx, sub))
                        {
                            error = WalletError.ServerError;
                            return false;
                        }

                    // удалить сервис-рейты (прогрессивная шкала)
                    var rates = ctx.SERVICE_RATE.Where(r => r.Service == serviceId).ToList();
                    foreach (var rate in rates)
                        ctx.SERVICE_RATE.Remove(rate);

                    // удалить сам сервис
                    ctx.SERVICE.Remove(service);
                    ctx.SaveChanges();
                    error = WalletError.OK;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка в DisableService({0}): {1}", serviceId, ex);
                error = WalletError.ServerError;
                return false;
            }
        }

        public Wallet TransferToTradingAccount(ProtectedOperationContext secCtx,
            string userLogin, int accountId, decimal amountInWalletCurrency, out WalletError error)
        {
            var rst = DepositOrWithdraw(secCtx, true, userLogin, accountId, amountInWalletCurrency, out error);
            return rst;
        }

        /// <summary>
        /// просто пополнить счет, вывести оттуда долю или пополнить ПАММ-пай
        /// </summary>
        private Wallet DepositOrWithdraw(ProtectedOperationContext secCtx,
            bool putOnAccount,
            string userLogin, int accountId, decimal amountInSrcCurrency, out WalletError error)
        {
            var operationParams = string.Format("DepositOrWithdraw(log={0}, acc={1}, amt={2})",
                                                userLogin, accountId, amountInSrcCurrency.ToStringUniformMoneyFormat());

            if (!UserSessionStorage.Instance.PermitUserOperation(secCtx, false, false))
            {
                Logger.Error(operationParams + ": InsufficientRights");
                error = WalletError.InsufficientRights;
                return null;
            }

            if (amountInSrcCurrency <= 0)
            {
                Logger.Error(operationParams + ": amountInSrcCurrency <= 0");
                error = WalletError.InvalidData;
                return null;
            }

            // перевести деньги на счет / со счета на кошелек
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    // счет
                    var account = ctx.ACCOUNT.FirstOrDefault(a => a.ID == accountId);
                    if (account == null)
                    {
                        Logger.Error(operationParams + ": account is not found");
                        error = WalletError.InvalidData;
                        return null;
                    }

                    // пользователь и кошелек
                    var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == userLogin);
                    if (user == null)
                    {
                        Logger.Error(operationParams + ": user is not found");
                        error = WalletError.InvalidData;
                        return null;
                    }
                    var wallet = ctx.WALLET.FirstOrDefault(w => w.User == user.ID);
                    if (wallet == null)
                    {
                        Logger.Error(operationParams + ": wallet is not found");
                        error = WalletError.InvalidData;
                        return null;
                    }

                    // это - ПАММ-счет или обычный счет?
                    var userShare = GetAccountSharePercent(ctx, accountId, user.ID);
                    if (userShare < 100M)
                    {
                        var status = new WalletManager4Pamm(secCtx, userLogin, accountId).InvestOrWithdrawFromPamm(
                                amountInSrcCurrency,
                                !putOnAccount, false, ctx);
                        error = status == RequestStatus.OK ? WalletError.OK : WalletError.InvalidData;
                        if (error != WalletError.OK)
                            Logger.Error(operationParams + ": InvestOrWithdrawFromPamm - " + status);

                        return LinqToEntity.DecorateWallet(wallet);
                    }
                    
                    // деньги - в кошелек
                    if (putOnAccount)
                        return PutMoneyOnUserOwnedAccount(accountId, amountInSrcCurrency, out error, wallet, account, ctx, user);
                     
                    // перевести со счета в кошелек
                    return GetMoneyFromUserOwnedAccount(accountId, amountInSrcCurrency, out error, account, wallet, ctx, user);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка в TransferToTradingAccount({0}): {1}", accountId, ex);
                error = WalletError.ServerError;
                return null;
            }
        }

        private Wallet GetMoneyFromUserOwnedAccount(int accountId, decimal amountInSrcCurrency, out WalletError error,
                                                    ACCOUNT account, WALLET wallet, TradeSharpConnection ctx, PLATFORM_USER user)
        {
            var amountWallet = amountInSrcCurrency;
            if (account.Currency != wallet.Currency)
            {
                // найти котировку и перевести
                string errorString;
                var amountTarget = DalSpot.Instance.ConvertSourceCurrencyToTargetCurrency(wallet.Currency,
                                                                                          account.Currency,
                                                                                          (double) amountWallet,
                                                                                          QuoteStorage.Instance.ReceiveAllData(),
                                                                                          out errorString);
                if (!amountTarget.HasValue)
                {
                    Logger.ErrorFormat("DepositOrWithdraw({0} {1}): {2} (withdraw)",
                                       amountInSrcCurrency, account.Currency + "/" + wallet.Currency,
                                       errorString);
                    error = WalletError.CurrencyExchangeFailed;
                    return null;
                }
                amountWallet = amountTarget.Value;
            }

            // достаточно ли средств на счете?
            // проверить средства / зарезервированное марж. обеспечение
            decimal equity, usedMargin;
            if (!GetAccountEquityAndUsedMargin(account, out equity, out usedMargin))
            {
                error = WalletError.ServerError;
                return null;
            }
            if (equity - usedMargin < amountInSrcCurrency)
            {
                error = WalletError.ServerError;
                return null;
            }

            // списать со счета в пользу кошелька
            wallet.Balance += amountInSrcCurrency;
            account.Balance -= amountInSrcCurrency;
            var date = DateTime.Now;
            var bc = ctx.BALANCE_CHANGE.Add(new BALANCE_CHANGE
                {
                    AccountID = accountId,
                    Amount = amountInSrcCurrency,
                    ChangeType = (int) BalanceChangeType.Withdrawal,
                    ValueDate = date,
                    Description = "Списание на кошелек №" + wallet.User
                });
            ctx.SaveChanges();
            ctx.TRANSFER.Add(new TRANSFER
                {
                    Amount = amountWallet,
                    TargetAmount = amountWallet,
                    User = user.ID,
                    Comment = "Вывод средств со счета №" + account.ID,
                    ValueDate = date,
                    BalanceChange = bc.ID,
                });

            ctx.SaveChanges();
            error = WalletError.OK;
            return LinqToEntity.DecorateWallet(wallet);
        }

        private static Wallet PutMoneyOnUserOwnedAccount(int accountId, decimal amountInSrcCurrency, out WalletError error,
                                                         WALLET wallet, ACCOUNT account, TradeSharpConnection ctx,
                                                         PLATFORM_USER user)
        {
            // достаточно ли денег в кошельке?    
            if (amountInSrcCurrency > wallet.Balance)
            {
                error = WalletError.InsufficientFunds;
                return null;
            }

            // перевести объем в валюту счета
            var amount = amountInSrcCurrency;
            if (account.Currency != wallet.Currency)
            {
                // найти котировку и перевести
                string errorString;
                var amountTarget = DalSpot.Instance.ConvertSourceCurrencyToTargetCurrency(account.Currency,
                                                                                          wallet.Currency, (double) amount,
                                                                                          QuoteStorage.Instance.ReceiveAllData(),
                                                                                          out errorString);
                if (!amountTarget.HasValue)
                {
                    Logger.ErrorFormat("DepositOrWithdraw({0} {1}): {2}",
                                       amountInSrcCurrency, account.Currency + "/" + wallet.Currency,
                                       errorString);
                    error = WalletError.CurrencyExchangeFailed;
                    return null;
                }
                amount = amountTarget.Value;
            }

            // списать с кошелька и пополнить счет
            wallet.Balance -= amountInSrcCurrency;
            account.Balance += amount;
            var dateOper = DateTime.Now;
            var balanceChange = ctx.BALANCE_CHANGE.Add(new BALANCE_CHANGE
                {
                    AccountID = accountId,
                    Amount = amount,
                    ChangeType = (int) BalanceChangeType.Deposit,
                    ValueDate = dateOper,
                    Description = "Пополнение с кошелька №" + wallet.User
                });
            try
            {
                ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error("DepositOrWithdraw() - error saving balance change", ex);
                error = WalletError.ServerError;
                return null;
            }

            var balanceChangeId = balanceChange.ID;
            ctx.TRANSFER.Add(new TRANSFER
                {
                    Amount = -amountInSrcCurrency,
                    TargetAmount = -amountInSrcCurrency,
                    User = user.ID,
                    Comment = "Т. счет №" + account.ID,
                    ValueDate = dateOper,
                    BalanceChange = balanceChangeId,
                });

            try
            {
                ctx.SaveChanges();
            }
            //catch (System.Data.Entity.Infrastructure.DbUpdateConcurrencyException)
            //{
            //}
            catch (Exception ex)
            {
                Logger.Error("DepositOrWithdraw() - error saving transfer for user " + user.ID +
                             ", balance change Id: " + balanceChangeId + ", user: " + user.ID, ex);
                error = WalletError.ServerError;
                return null;
            }
            error = WalletError.OK;
            return LinqToEntity.DecorateWallet(wallet);
        }

        public decimal GetAccountSharePercent(TradeSharpConnection ctx, int accountId, int userId)
        {
            var share = ctx.ACCOUNT_SHARE.FirstOrDefault(s => s.Account == accountId && s.ShareOwner == userId);
            return share == null ? 100 : share.Share;
        }

        private bool GetAccountEquityAndUsedMargin(ACCOUNT account, out decimal equity, out decimal usedMargin)
        {
            try
            {
                decimal exposure;
                profitCalculator.CalculateAccountExposure(account.ID, out equity, out usedMargin, out exposure,
                    QuoteStorage.Instance.ReceiveAllData(),
                    ManagerAccount.Instance, accountRepository.GetAccountGroup);
                return true;
            }
            catch (Exception ex)
            {
                equity = 0;
                usedMargin = 0;
                Logger.ErrorFormat("GetAccountEquityAndUsedMargin({0}) error: {1}", account.ID, ex);
                return false;
            }
        }

        public Wallet TransferToWallet(ProtectedOperationContext secCtx,
            string userLogin, int accountId,
            decimal amountInAccountCurrency, out WalletError error)
        {
            return DepositOrWithdraw(secCtx, false, userLogin, accountId, amountInAccountCurrency, out error);
        }

        public PaidService GetPaidServiceDetail(int serviceId, out PlatformUser serviceOwner)
        {
            // вернуть сервис и прогрессивную шкалу расценок
            var list = GetServicesByUserOrId(serviceId, false, out serviceOwner);
            return list.Count == 0 ? null : list[0];
        }

        public List<PaidService> GetUserOwnerPaidServices(int userId)
        {
            PlatformUser serviceOwner;
            return GetServicesByUserOrId(userId, true, out serviceOwner);
        }

        private List<PaidService> GetServicesByUserOrId(int id, bool byUser, out PlatformUser serviceOwner)
        {
            serviceOwner = null;
            var srvList = new List<PaidService>();
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var query = byUser
                                    ? ctx.SERVICE.Where(s => s.User == id)
                                    : ctx.SERVICE.Where(s => s.ID == id);
                    foreach (var srv in query)
                    {
                        var paidServ = LinqToEntity.DecoratePaidService(srv);
                        var srvId = srv.ID;
                        // добавить рейты
                        foreach (var rate in ctx.SERVICE_RATE.Where(r => r.Service == srvId))
                            paidServ.serviceRates.Add(new PaidServiceRate
                            {
                                Amount = rate.Amount,
                                RateType = (PaidServiceRate.ServiceRateType)rate.RateType,
                                UserBalance = rate.UserBalance
                            });
                        srvList.Add(paidServ);
                    }

                    if (byUser)
                        serviceOwner = LinqToEntity.DecoratePlatformUser(ctx.PLATFORM_USER.First(u => u.ID == id));
                    else
                    {
                        var srv = srvList.FirstOrDefault();
                        if (srv != null)
                            serviceOwner = LinqToEntity.DecoratePlatformUser(ctx.PLATFORM_USER.First(u => u.ID == srv.User));
                    }
                }
                return srvList;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("GetServicesByUserOrId({0}, {1}) error: {2}", id, byUser, ex);
                return srvList;
            }
        }

        public WalletError GetTransferExtendedInfo(ProtectedOperationContext secCtx,
                                                   int transferId, out BalanceChange balanceChange, out PlatformUser user)
        {
            balanceChange = null;
            user = null;

            if (!UserSessionStorage.Instance.PermitUserOperation(secCtx, false, false))
                return WalletError.InsufficientRights;

            var error = WalletError.ServerError;
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var trans = ctx.TRANSFER.FirstOrDefault(t => t.ID == transferId);
                    if (trans == null) return WalletError.InvalidData;
                    if (trans.BalanceChange.HasValue)
                    {
                        balanceChange = LinqToEntity.DecorateBalanceChange(trans.BALANCE_CHANGE);
                        return WalletError.OK;
                    }
                    if (trans.RefWallet.HasValue)
                    {
                        var us = ctx.PLATFORM_USER.FirstOrDefault(u => u.ID == trans.RefWallet.Value);
                        if (us != null)
                        {
                            user = LinqToEntity.DecoratePlatformUser(us);
                            user.Password = string.Empty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("GetTransferExtendedInfo({0}) error: {1}", transferId, ex);
                error = WalletError.ServerError;
            }
            return error;
        }

        public Wallet UpdateBalance(int walletId, decimal transferVolume, bool deposit, out WalletError error)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var wallet = ctx.WALLET.FirstOrDefault(x => x.User == walletId);
                    if (wallet == null)
                    {
                        Logger.InfoFormat("не найден кошек {0}", walletId);
                        error = WalletError.InvalidData;
                        return null;
                    }

                    if (transferVolume <= 0)
                    {
                        Logger.Info("объём зачисляемых средств должен быть больше нуля}");
                        error = WalletError.InvalidData;
                        return LinqToEntity.DecorateWallet(wallet);
                    }
                    var currentAmount = deposit ? transferVolume : -transferVolume;
                    var oldWalletBalance = wallet.Balance;
                    wallet.Balance += currentAmount;


                    Logger.InfoFormat("средства на кошеке {0} изменены с {1} на {2}",
                                      walletId, oldWalletBalance, wallet.Balance);

                    var newTransfer = new TRANSFER
                    {
                        Amount = currentAmount,
                        TargetAmount = currentAmount,
                        User = walletId,
                        Comment = "Изменение администратором средств кошелька № " + walletId,
                        ValueDate = DateTime.Now,
                        BalanceChange = null,
                    };
                    ctx.TRANSFER.Add(newTransfer);

                    Logger.Info("Сохраняем изменения...");
                    ctx.SaveChanges();
                    error = WalletError.OK;
                    return LinqToEntity.DecorateWallet(wallet);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("UpdateUserWallet()", ex);
                error = WalletError.CommonError;
                return null;
            }
        }

        public Wallet ChangeCurrency(int walletId, string walletCurrency, bool recalculationBalance,
                                     out WalletError error)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var wallet = ctx.WALLET.FirstOrDefault(x => x.User == walletId);
                    if (wallet == null)
                    {
                        Logger.InfoFormat("не найден кошек для пользователя с Id = {0}", walletId);
                        error = WalletError.InvalidData;
                        return null;
                    }

                    var oldWalletCurrency = wallet.Currency;
                    if (oldWalletCurrency != walletCurrency)
                    {
                        wallet.Currency = walletCurrency;
                        Logger.InfoFormat("валюта кошека {0} изменена с {1} на {2}",
                                          walletId, oldWalletCurrency, walletCurrency);
                    }

                    if (recalculationBalance)
                    {
                        #region 

                        Logger.InfoFormat("пересчёт баланса кошелька {0} {1} в новую валюту {2}...",
                                          walletId, oldWalletCurrency, walletCurrency);

                        var oldBalans = wallet.Balance;
                        var newValue = PaymentProcessor.ConvertPaySysCurrencyToWalletCurrency(walletCurrency,
                                                                                              oldWalletCurrency,
                                                                                              (double) oldBalans);

                        if (newValue != null)
                        {
                            newValue = Math.Round(newValue.Value, 2);
                            wallet.Balance = (decimal) newValue;
                            Logger.InfoFormat("баланса кошелька {0} изменён с {1} на {2}", walletId, oldBalans,
                                              wallet.Balance);
                        }
                        else
                        {
                            Logger.ErrorFormat("не удалось пересчитать балланс кошелька {0} в новой валюте {1}",
                                               walletId, walletCurrency);
                            error = WalletError.CurrencyExchangeFailed;
                            return LinqToEntity.DecorateWallet(wallet);
                        }

                        #endregion
                    }

                    Logger.Info("Сохраняем изменения...");
                    ctx.SaveChanges();
                    error = WalletError.OK;
                    Logger.Info("изменения сохранены");
                    return LinqToEntity.DecorateWallet(wallet);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ChangeCurrency()", ex);
                error = WalletError.CommonError;
                return null;
            }
        }

        #region Payment System

        public List<UserPaymentSystem> GetUserRegistredPaymentSystemWallets(ProtectedOperationContext secCtx,
                                                                            string userLogin,
                                                                            string walletPwrd,
                                                                            out WalletError error)
        {
            if (!UserSessionStorage.Instance.PermitUserOperation(secCtx, false, false))
            {
                error = WalletError.InsufficientRights;
                return null;
            }

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == userLogin);
                    if (user == null)
                    {
                        error = WalletError.AuthenticationError;
                        return null;
                    }

                    var systems = new List<UserPaymentSystem>();
                    var query = ctx.USER_PAYMENT_SYSTEM.Where(s => s.UserId == user.ID);
                    foreach (var payment in query)
                        systems.Add(LinqToEntity.DecorateUserPaymentSystem(payment));
                    error = WalletError.OK;
                    return systems;
                }
            }
            catch (Exception ex)
            {
                error = WalletError.ServerError;
                Logger.Error("GetUserRegistredPaymentSystemWallets() error", ex);
                return null;
            }
        }

        public WalletError ChangePaymentSystemWallets(ProtectedOperationContext secCtx, UserPaymentSystem paySys, bool remove,
                                               string userLogin, string walletPwrd)
        {
            if (!UserSessionStorage.Instance.PermitUserOperation(secCtx, false, false))
            {
                return WalletError.InsufficientRights;
            }

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == userLogin);
                    if (user == null)
                    {
                        return WalletError.AuthenticationError;
                    }
                    var existSyst = ctx.USER_PAYMENT_SYSTEM.FirstOrDefault(s => s.Id == paySys.Id);

                    // удалить запись
                    if (remove)
                    {
                        if (existSyst == null)
                            return WalletError.ServerError;
                        ctx.USER_PAYMENT_SYSTEM.Remove(existSyst);
                        ctx.SaveChanges();
                        return WalletError.OK;
                    }

                    // создать новую запись
                    if (existSyst == null)
                    {

                    }

                    return WalletError.OK;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ChangePaymentSystemWallets() error", ex);
                return WalletError.ServerError;
            }
        }

        public WalletError SetPaymentWalletsBySystem(ProtectedOperationContext secCtx,
            PaymentSystem syst,
            List<UserPaymentSystem> actualPaySys,
            string userLogin, string walletPwrd)
        {
            if (!UserSessionStorage.Instance.PermitUserOperation(secCtx, false, false))
                return WalletError.InsufficientRights;

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == userLogin);
                    if (user == null)
                        return WalletError.AuthenticationError;

                    // уничтожить старые записи того же типа
                    var existRecs =
                        ctx.USER_PAYMENT_SYSTEM.Where(r => r.UserId == user.ID && r.SystemPayment == (int)syst).ToList();
                    foreach (var rec in existRecs)
                        ctx.USER_PAYMENT_SYSTEM.Remove(rec);
                    ctx.SaveChanges();

                    // добавить новые записи
                    foreach (var paySyst in actualPaySys)
                    {
                        paySyst.UserId = user.ID;
                        paySyst.SystemPayment = syst;
                        ctx.USER_PAYMENT_SYSTEM.Add(LinqToEntity.UndecorateUserPaymentSystem(paySyst));
                    }
                    ctx.SaveChanges();
                    return WalletError.OK;
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException.InnerException is SqlException)
                {
                    Logger.Error("SetPaymentWalletsBySystem() error: возможно, запись с таким RootId и PurseId уже есть в таблице USER_PAYMENT_SYSTEM", ex);
                    return WalletError.ServerSqlError;
                }
                Logger.Error("SetPaymentWalletsBySystem() error", ex);
                return WalletError.ServerError;
            }
            catch (Exception ex)
            {
                Logger.Error("SetPaymentWalletsBySystem() error", ex);
                return WalletError.ServerError;
            }
        }

        #endregion
    }
}

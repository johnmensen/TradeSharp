using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.Objects;
using System.Diagnostics;
using System.Linq;
using Entity;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Linq;
using TradeSharp.Util;
using QuoteStorage = TradeSharp.Contract.Util.BL.QuoteStorage;

namespace TradeSharp.Server.Repository
{
    class WalletRepository : IWalletRepository
    {
        public WalletError ChargeFeeOnSubscription(TradeSharpConnection ctx, int serviceId, int usr, bool renewSubscription)
        {
            SERVICE service;
            try
            {
                service = ctx.SERVICE.First(s => s.ID == serviceId);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в ChargeFeeOnSubscription() - сервис не найден", ex);
                return WalletError.InvalidData;
            }
            return ChargeFeeOnSubscription(ctx, service, usr, renewSubscription);
        }

        public WalletError ChargeFeeOnSubscription(TradeSharpConnection ctx, SERVICE service, int usr, bool renewSubscription)
        {
            try
            {
                // создать платеж с кошелька usr на кошелек владельца сервиса (service)
                if (service.FixedPrice == 0)
                    return WalletError.OK; // все на шарку!
                var price = service.FixedPrice > 0
                                ? service.FixedPrice
                                : GetFeeByUserBalance(ctx, service);
                if (price == 0) return WalletError.OK;

                var usrWallet = ctx.WALLET.FirstOrDefault(w => w.User == usr);
                if (usrWallet == null)
                {
                    Logger.Error("ChargeFeeOnSubscription(usrId=" + usr + ") - wallet is not found");
                    return WalletError.ServerError;
                }

                if (HasUserPaidTheService(ctx, service, usr))
                    return WalletError.OK;

                // посчитать в валюте пользователя
                var usrAmount = price;
                if (usrWallet.Currency != service.Currency)
                {
                    string strError;
                    var targetAmount = DalSpot.Instance.ConvertSourceCurrencyToTargetCurrency(usrWallet.Currency, service.Currency,
                                                                           (double)usrAmount,
                                                                           QuoteStorage.Instance.ReceiveAllData(),
                                                                           out strError);
                    if (!targetAmount.HasValue)
                    {
                        Logger.ErrorFormat("ChargeFeeOnSubscription(usrId={0}) - currency rate is not found ({1}/{2})",
                            usr, usrWallet.Currency, service.Currency);
                        return WalletError.CurrencyExchangeFailed;
                    }
                    usrAmount = targetAmount.Value;
                }

                // если объем превышает возможности пользователя...
                if (usrWallet.Balance < usrAmount)
                {
                    Logger.InfoFormat("ChargeFeeOnSubscription(usrId={0}) - not enough money ({1}, needed {2})",
                            usr, usrAmount, usrWallet.Balance);
                    return WalletError.InsufficientFunds;
                }

                usrWallet.Balance -= usrAmount;
                var trans = ctx.TRANSFER.Add(new TRANSFER
                {
                    Amount = usrAmount,
                    TargetAmount = price,
                    Comment = "Fee on srv " + service.ID,
                    RefWallet = service.User,
                    User = usr,
                    ValueDate = DateTime.Now,
                    Subscription = service.ID
                });
                Logger.InfoFormat("Добавляется трансфер: usr={0}, comment={1}",
                    usr, trans.Comment);

                // добавить денег владельцу
                ctx.TRANSFER.Add(new TRANSFER
                {
                    Amount = price,
                    TargetAmount = usrAmount,
                    Comment = "Paid for srv " + service.ID,
                    RefWallet = usr,
                    User = service.User,
                    ValueDate = DateTime.Now,
                    Subscription = service.ID
                });
                var ownerWallet = ctx.WALLET.First(w => w.User == service.User);
                ownerWallet.Balance += price;

                // обновить подписку
                if (renewSubscription)
                {
                    var sub = ctx.SUBSCRIPTION.FirstOrDefault(s => s.Service == service.ID);
                    if (sub != null && sub.RenewAuto)
                    {
                        sub.TimeStarted = DateTime.Now.Date;
                        sub.TimeEnd = sub.TimeStarted.AddDays(1);
                    }
                }

                Logger.InfoFormat("ChargeFeeOnSubscription() - сохранение");
                ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error("Error in ChargeFeeOnSubscription()", ex);
                return WalletError.ServerError;
            }
            return WalletError.OK;
        }

        /// <summary>
        /// отключить подписчика
        /// заодно отрубить ему торговые сигналы
        /// </summary>
        public bool UnsubscribeSubscriber(TradeSharpConnection ctx, SUBSCRIPTION sub)
        {
            // отписать подписчика и сформировать для него уведомление
            try
            {
                var service = ctx.SERVICE.First(s => s.ID == sub.Service);
                Logger.InfoFormat("UnsubscribeSubscriber(srv={0}, user={1}, srvAccount={2})",
                    sub.Service, sub.User, service.AccountId);

                // отписать от торг. сигналов?
                if (service.ServiceType == (int)PaidServiceType.Signals)
                {
                    var signalService = (from srv in ctx.SERVICE
                                         where srv.ID == sub.Service
                                         select srv).FirstOrDefault();

                    if (signalService != null)
                    {
                        var signalId = signalService.ID;
                        var query = (from us in ctx.SUBSCRIPTION
                                     where us.Service == signalId
                                        && us.User == sub.User
                                     select us);

                        var userSignalToRemove = new List<SUBSCRIPTION>();
                        foreach (var userSubscribed in query)
                            userSignalToRemove.Add(userSubscribed);
                        Logger.InfoFormat("UnsubscribeSubscriber(): remove {0} signals", userSignalToRemove.Count);

                        foreach (var userSubscribed in userSignalToRemove)
                            ctx.SUBSCRIPTION.Remove(userSubscribed);
                    }
                    else
                    {
                        Logger.ErrorFormat("UnsubscribeSubscriber(#{0}) - владелец сигнала #{1} не найден",
                            sub.User, sub.Service);
                    }
                }

                // убрать саму подписку
                ctx.SUBSCRIPTION.Remove(sub);
                // !! с уведомлением пока что вопрос 

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Error in UnsubscribeSubscriber()", ex);
                return false;
            }
        }

        public Wallet GetUserWalletSubscriptionAndLastPaymentsInner(
            string userLogin, int maxPaymentsQuery,
            out int paymentsTotalCount,
            out List<Subscription> subscriptions,
            out List<Transfer> transfers, out WalletError error)
        {
            subscriptions = null;
            transfers = null;
            paymentsTotalCount = 0;
            error = WalletError.ServerError;

            // получить кошелек пользователя и вернуть его
            Wallet wallet = null;
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    // получить пользователя
                    var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == userLogin);
                    if (user == null)
                    {
                        error = WalletError.AuthenticationError;
                        return null;
                    }

                    // получить кошелек
                    var walletBase = ctx.WALLET.FirstOrDefault(w => w.User == user.ID);
                    if (walletBase == null)
                    {
                        error = WalletError.ServerError;
                        return null;
                    }
                    wallet = LinqToEntity.DecorateWallet(walletBase);
                    subscriptions = new List<Subscription>();
                    transfers = new List<Transfer>();

                    // получить подписки пользователя
                    subscriptions =
                        (from subs in ctx.SUBSCRIPTION
                         join srv in ctx.SERVICE on subs.Service equals srv.ID
                         where subs.User == user.ID
                         select new TradeSharp.Contract.Entity.Subscription
                         {
                             User = subs.User,
                             RenewAuto = subs.RenewAuto,
                             Service = subs.Service,
                             TimeEnd = subs.TimeEnd,
                             TimeStarted = subs.TimeStarted,
                             PaidService = new PaidService
                             {
                                 User = srv.User,
                                 AccountId = srv.AccountId,
                                 Currency = srv.Currency,
                                 Comment = srv.Comment,
                                 ServiceType = (PaidServiceType)srv.ServiceType,
                                 Id = srv.ID,
                                 FixedPrice = srv.FixedPrice
                             }
                         }).ToList();

                    // получить последние платежи по кошельку
                    paymentsTotalCount = ctx.TRANSFER.Count(t => t.User == user.ID);
                    transfers = (from trans in ctx.TRANSFER where trans.User == user.ID select trans).Take(maxPaymentsQuery).ToList().Select(
                        LinqToEntity.DecorateTransfer).ToList();
                    error = WalletError.OK;
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("GetUserWalletSubscriptionAndLastPayments({0}) - exception: {1}", userLogin, ex);
            }

            return wallet;
        }

        public bool HasUserPaidTheService(TradeSharpConnection ctx, SERVICE service, int usr)
        {
            var nowDate = DateTime.Now.Date;
            // проверить - совершался ли платеж за подключение к подписке в ближайшие N минут?
            // если да - не снимать денег
            var hasTodayTransfers = ctx.TRANSFER.Any(t => t.User == usr &&
                EntityFunctions.TruncateTime(t.ValueDate) == nowDate && t.Subscription == service.ID);
            return hasTodayTransfers;
        }

        /// <summary>
        /// по средствам клиента определить сумму по прогрессивной шкале
        /// </summary>
        private decimal GetFeeByUserBalance(TradeSharpConnection ctx, SERVICE srv)//, PLATFORM_USER usr)
        {
            var rate = ctx.SERVICE_RATE.FirstOrDefault(r => r.Service == srv.ID);
            return rate == null ? 0 : rate.Amount;
        }

        public bool SubscribeOnService(TradeSharpConnection ctx,
            int userId, int serviceId, bool renewAuto, bool unsubscribe,
            AutoTradeSettings tradeSets, out WalletError error)
        {
            // имеющаяся подписка
            var subs = ctx.SUBSCRIPTION.FirstOrDefault(s => s.Service == serviceId && s.User == userId);

            // просто отписаться от сервиса
            if (unsubscribe)
            {
                error = WalletError.OK;
                if (subs == null)
                    return true;
                ctx.SUBSCRIPTION.Remove(subs);
                try
                {
                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка удаления подписки (SubscribeOnService)", ex);
                    error = WalletError.ServerError;
                    return false;
                }

                return true;
            }

            var paidService = ctx.SERVICE.FirstOrDefault(s => s.ID == serviceId);
            if (paidService == null)
            {
                error = WalletError.InvalidData;
                return false;
            }

            // проверить - не подписывается ли пользователь сам на себя?
            if (paidService.User == userId)
            {
                error = WalletError.InvalidData;
                return false;
            }

            // провести списание денежных средств
            // содрать денежку
            var feeError = ChargeFeeOnSubscription(ctx, serviceId, userId, false);
            if (feeError != WalletError.OK)
            {
                error = feeError;
                return false;
            }

            // продлить или обновить подписку
            var subExists = subs != null;
            if (subs == null)
                subs = new SUBSCRIPTION();
            subs.RenewAuto = renewAuto;
            subs.TimeEnd = DateTime.Now.Date.AddDays(1);
            subs.TimeStarted = DateTime.Now.Date;
            subs.User = userId;
            subs.Service = serviceId;
            if (!subExists)
                ctx.SUBSCRIPTION.Add(subs);

            // обновить или создать настройки торговли
            var signalTradeSets = ctx.SUBSCRIPTION_SIGNAL.FirstOrDefault(s => s.Service == serviceId && s.User == userId);
            var setsExists = signalTradeSets != null;
            if (signalTradeSets == null)
                signalTradeSets = new SUBSCRIPTION_SIGNAL();
            signalTradeSets.AutoTrade = tradeSets.TradeAuto;
            signalTradeSets.FixedVolume = tradeSets.FixedVolume;
            signalTradeSets.HedgingOrdersEnabled = tradeSets.HedgingOrdersEnabled;
            signalTradeSets.MaxLeverage = tradeSets.MaxLeverage;
            signalTradeSets.MaxVolume = tradeSets.MaxVolume;
            signalTradeSets.MinVolume = tradeSets.MinVolume;
            signalTradeSets.PercentLeverage = tradeSets.PercentLeverage;
            signalTradeSets.Service = serviceId;
            signalTradeSets.StepVolume = tradeSets.StepVolume;
            signalTradeSets.User = userId;
            signalTradeSets.TargetAccount = tradeSets.TargetAccount;
            signalTradeSets.VolumeRound = (int?)tradeSets.VolumeRound;
            if (!setsExists)
                ctx.SUBSCRIPTION_SIGNAL.Add(signalTradeSets);

            try
            {
                ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка сохранения подписки (SubscribeOnService)", ex);
                error = WalletError.ServerError;
                return false;
            }

            error = WalletError.OK;
            return true;
        }

        public RequestStatus SubscribeUserOnPortfolio(
            TradeSharpConnection ctx,
            string subscriberLogin,
            TopPortfolio portfolio,
            decimal? maxFee,
            AutoTradeSettings tradeAutoSettings)
        {
            var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == subscriberLogin);
            if (user == null)
                return RequestStatus.Unauthorized;

            TOP_PORTFOLIO targetPortfolio = null;
            if (portfolio.Id > 0)
                targetPortfolio = ctx.TOP_PORTFOLIO.FirstOrDefault(p => p.Id == portfolio.Id);
            if (targetPortfolio != null)
                portfolio.ManagedAccount = targetPortfolio.ManagedAccount;

            // если портфель - пользовательский - сохранить его
            // для пользователя или обновить его имеющийся портфель
            if (!portfolio.IsCompanyPortfolio)
            {
                Logger.Info("SubscribeOnPortfolio() - user portfolio");
                try
                {
                    var existPortfolio = ctx.TOP_PORTFOLIO.FirstOrDefault(p => p.OwnerUser == user.ID);
                    if (existPortfolio != null)
                    {
                        if (!LinqToEntity.DecoratePortfolio(existPortfolio).AreSame(portfolio))
                        {
                            // удалить старый портфель пользователя
                            ctx.TOP_PORTFOLIO.Remove(existPortfolio);
                            existPortfolio = null;
                        }
                        else
                            targetPortfolio = existPortfolio;
                    }
                    // создать портфель пользователя
                    if (existPortfolio == null)
                    {
                        targetPortfolio = LinqToEntity.UndecoratePortfolio(portfolio);
                        targetPortfolio.OwnerUser = user.ID;
                        targetPortfolio.ManagedAccount = null;
                        ctx.TOP_PORTFOLIO.Add(targetPortfolio);
                        ctx.SaveChanges();
                    }
                }
                catch (DbEntityValidationException dbEx)
                {
                    Logger.Error("SubscribeUserOnPortfolio - ошибка сохранения портфеля");
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            Logger.ErrorFormat("Свойство: {0}, ошибка: {1}", 
                                validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка в SubscribeOnPortfolio() - обновление портфеля", ex);
                    return RequestStatus.ServerError;
                }
            }
            else
            {// портфель компании
                Logger.Info("SubscribeOnPortfolio() - company portfolio");
                if (targetPortfolio == null)
                {
                    Logger.Error("Пользователь запросил несуществующий портфель компании " + portfolio.Id);
                    return RequestStatus.ServerError;
                }
            }

            // очистить подписки пользователя на портфели
            // и привязать его к целевому портфелю
            //Logger.Info("SubscribeOnPortfolio() - removing bindings");
            var oldBinding = ctx.USER_TOP_PORTFOLIO.FirstOrDefault(u => u.User == user.ID);
            // "посмотреть" настройки портфельной торговли в имеющейся подписке
            if (tradeAutoSettings == null)
            {
                if (oldBinding == null)
                {
                    Logger.ErrorFormat("Подписка пользователя {0} на портфель {1} - нет данных по автоматической торговле",
                        user.Login, portfolio.Id > 0 ? portfolio.Id.ToString() : portfolio.Name);
                    return RequestStatus.BadRequest;
                }
                tradeAutoSettings = new AutoTradeSettings
                {
                    FixedVolume = oldBinding.FixedVolume,
                    HedgingOrdersEnabled = oldBinding.HedgingOrdersEnabled,
                    MaxLeverage = oldBinding.MaxLeverage,
                    MaxVolume = oldBinding.MaxVolume,
                    MinVolume = oldBinding.MinVolume,
                    PercentLeverage = oldBinding.PercentLeverage ?? 100,
                    StepVolume = oldBinding.StepVolume,
                    TargetAccount = oldBinding.TargetAccount,
                    TradeAuto = oldBinding.AutoTrade ?? false,
                    VolumeRound = (VolumeRoundType?)oldBinding.VolumeRound
                };
            }

            if (oldBinding != null)
                ctx.USER_TOP_PORTFOLIO.Remove(oldBinding);

            //Logger.Info("SubscribeOnPortfolio() - adding binding");
            ctx.USER_TOP_PORTFOLIO.Add(new USER_TOP_PORTFOLIO
            {
                User = user.ID,
                Portfolio = targetPortfolio.Id,
                MaxFee = maxFee,
                AutoTrade = tradeAutoSettings.TradeAuto,
                MaxLeverage = tradeAutoSettings.MaxLeverage,
                PercentLeverage = tradeAutoSettings.PercentLeverage,
                HedgingOrdersEnabled = tradeAutoSettings.HedgingOrdersEnabled,
                FixedVolume = tradeAutoSettings.FixedVolume,
                MinVolume = tradeAutoSettings.MinVolume,
                MaxVolume = tradeAutoSettings.MaxVolume,
                VolumeRound = (int?)tradeAutoSettings.VolumeRound,
                StepVolume = tradeAutoSettings.StepVolume,
                TargetAccount = tradeAutoSettings.TargetAccount
            });
            ctx.SaveChanges();
            //Logger.Info("SubscribeOnPortfolio() - changes are saved");

            // найти трейдеров, удовлетворяющих критерию
            List<PerformerStat> performers;
            try
            {
                try
                {
                    performers = TradeSharpAccountStatistics.Instance.proxy.GetAllPerformersWithCriteria(true,
                        targetPortfolio.Criteria, targetPortfolio.ParticipantCount,
                        !targetPortfolio.DescendingOrder, (float?)targetPortfolio.MarginValue, 0);
                }
                catch (Exception ex)
                {
                    Logger.Error(
                        "Ошибка в SubscribeOnPortfolio() - получение перформеров (" + targetPortfolio.Criteria + ")",
                        ex);
                    return RequestStatus.ServerError;
                }

                if (performers == null)
                {
                    Logger.Error("Ошибка в SubscribeOnPortfolio() - список перформеров не получен (" + targetPortfolio.Criteria + ")");
                    return RequestStatus.ServerError;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в SubscribeOnPortfolio() - подписка", ex);
                return RequestStatus.ServerError;
            }

            // сравнить полученный список с текущими подписками заказчика ("инвестора")
            // сформировать список для "отписки" и список для подписки
            var performerAcs = performers.Select(p => p.Account).ToList();
            var subsToRemove = ctx.SUBSCRIPTION.Where(s => s.User == user.ID &&
                s.SERVICE1.ServiceType == (int)PaidServiceType.Signals &&
                !performerAcs.Contains(s.SERVICE1.AccountId ?? 0)).ToList();

            foreach (var sub in subsToRemove)
            {
                WalletError error;
                SubscribeOnService(ctx, user.ID, sub.Service, false, true, tradeAutoSettings, out error);
                if (error != WalletError.OK)
                {
                    Logger.ErrorFormat("Portfolio - unsubscribe user {0} from srv {1}: error {2}",
                        user.ID, sub.Service, error);
                }
            }

            // новоподписавшиеся
            foreach (var pf in performers)
            {
                WalletError error;
                SubscribeOnService(ctx, user.ID, pf.Service, true, false, tradeAutoSettings, out error);
                if (error != WalletError.OK)
                {
                    Logger.DebugFormat("Подписка SubscribeOnPortfolio({0}), сигн. {1}: {2}",
                        subscriberLogin, pf.Service, error);
                }
            }
            return RequestStatus.OK;
        }

        public RequestStatus UnsubscribeUserFromPortfolio(TradeSharpConnection ctx, string subscriberLogin,
            bool deletePortfolio, bool deleteSubscriptions)
        {
            try
            {
                var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == subscriberLogin);
                if (user == null)
                    return RequestStatus.Unauthorized;

                var subscriptions = ctx.USER_TOP_PORTFOLIO.Where(u => u.User == user.ID).ToList();
                foreach (var portfolioSub in subscriptions)
                {
                    // удалить подписку
                    ctx.USER_TOP_PORTFOLIO.Remove(portfolioSub);
                    Logger.InfoFormat("UnsubscribeUserFromPortfolio({0}) - отписан от портфеля", subscriberLogin);

                    // если подписка была на пользовательский портфель - удалить пользовательский портфель
                    if (deletePortfolio)
                    {
                        var portfolio = ctx.TOP_PORTFOLIO.Single(p => p.Id == portfolioSub.Portfolio);
                        if (portfolio.OwnerUser == user.ID)
                            ctx.TOP_PORTFOLIO.Remove(portfolio);
                    }

                    if (!deleteSubscriptions) continue;

                    // удалить подписки на сервисы
                    var subs = ctx.SUBSCRIPTION.Where(s => s.User == user.ID).ToList();
                    foreach (var sub in subs)
                        ctx.SUBSCRIPTION.Remove(sub);
                }
                ctx.SaveChanges();

                return RequestStatus.OK;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в UnsubscribePortfolio()", ex);
                return RequestStatus.ServerError;
            }
        }
    }
}

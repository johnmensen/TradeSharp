using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Entity;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Server.Contract;
using TradeSharp.Server.Repository;
using TradeSharp.TradeLib;
using TradeSharp.Util;
using QuoteStorage = TradeSharp.Contract.Util.BL.QuoteStorage;

namespace TradeSharp.Server.BL
{
    /// <summary>
    /// проверка ордеров SL-TP, трейлинга, отложенных ордеров,
    /// средств (М.К. - стопаут)
    /// </summary>
    class AccountCheckStream : Scheduler
    {
        private static readonly Lazy<AccountCheckStream> instance = new Lazy<AccountCheckStream>(() => new AccountCheckStream());
        public static AccountCheckStream Instance
        {
            get { return instance.Value; }
        }

        private readonly FloodSafeLogger loggerNoFlood = new FloodSafeLogger(5000);
        private const int LogMagicExposureCheck = 1;
        private const int LogMagicStopout = 2;
        private const int LogMagicMarginCall = 3;
        
        private readonly ThreadSafeTimeStamp swapCheckedTime = new ThreadSafeTimeStamp();
        private readonly int swapCheckHourGmt;
        private readonly int minutesToCheckSwap;
        private DateTime? lastRenewedSubscriptions;
        
        private readonly Dictionary<int, DateTime> lastTimeStopoutLogged = new Dictionary<int, DateTime>();

        /// <summary>
        /// проверка отложенного ордера отключается на PendingFloodTimoutMils, если ордер не может 
        /// быть активирован после первой попытки
        /// </summary>
        //private readonly Dictionary<int, DateTime> pendingFloodTimes = new Dictionary<int, DateTime>();

        private readonly TradeManager tradeManager;

        private IWalletRepository walletRepository;

        private IAccountRepository accountRepository;

        private IOrderRepository orderRepository;

        private IProfitCalculator profitCalculator;

        private IBrokerRepository brokerRepository;

        private AccountCheckStream()
        {
            walletRepository = new WalletRepository();
            accountRepository = AccountRepository.Instance;
            orderRepository = OrderRepository.Instance;
            profitCalculator = ProfitCalculator.Instance;
            tradeManager = ManagerTrade.Instance.tradeManager;
            brokerRepository = new BrokerRepository();

            threadIntervalMils = AppConfig.GetIntParam("CheckLoop.Interval", 100);

            schedules = new[]
                {
                    new Schedule(CheckOrders, AppConfig.GetIntParam("CheckLoop.IntervalOrders", 300)),
                    new Schedule(CheckMargin, AppConfig.GetIntParam("CheckLoop.IntervalMargin", 15000)),
                    new Schedule(CheckSwap, AppConfig.GetIntParam("CheckLoop.IntervalSwap", 1000)),
                    new Schedule(RenewSubscriptions, AppConfig.GetIntParam("CheckLoop.UpdateSubscriptions", 1000))
                };

            // параметры начисления свопов
            var dicMetadata = brokerRepository.GetMetadataByCategory("SWAP");
            object swapHourGmtObj, minutesToCheckSwapObj;
            if (!dicMetadata.TryGetValue("Hour.GMT", out swapHourGmtObj))
                swapHourGmtObj = 21;
            swapCheckHourGmt = (int) swapHourGmtObj;

            if (!dicMetadata.TryGetValue("MinutesToCheck", out minutesToCheckSwapObj))
                minutesToCheckSwapObj = 0;
            minutesToCheckSwap = (int)minutesToCheckSwapObj;
        }

        /// <summary>
        /// проверить SL, TP, трейлинги
        /// </summary>
        private void CheckOrders()
        {
            if (!WorkingDay.Instance.IsWorkingDay(DateTime.Now)) return;

            var curPrices = QuoteStorage.Instance.ReceiveAllData();
            try
            {            
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    // позиции
                    foreach (var order in ctx.POSITION)
                    {
                        if ((PositionState)order.State != PositionState.Opened) continue;
                        try
                        {
                            tradeManager.CheckOrder(LinqToEntity.DecorateOrder(order), curPrices);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("AccountCheckStream.CheckOrder() error", ex);
                        }
                    }

                    // отложенные ордера
                    foreach (var order in ctx.PENDING_ORDER)
                    {
                        try
                        {
                            tradeManager.CheckPendingOrder(LinqToEntity.DecoratePendingOrder(order), curPrices);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("AccountCheckStream.CheckPendingOrder() error", ex);
                        }                    
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в CheckOrders()", ex);
            }
        }

        /// <summary>
        /// проверить маржин колл - стопаут
        /// </summary>
        private void CheckMargin()
        {
            if (!WorkingDay.Instance.IsWorkingDay(DateTime.Now)) return;
            var curPrices = QuoteStorage.Instance.ReceiveAllData();
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    foreach (var group in ctx.ACCOUNT_GROUP)
                    {
                        // проверка каждого счета
                        foreach (var account in group.ACCOUNT)
                        {
                            decimal reservedMargin, equity;
                            try
                            {
                                decimal exposure;
                                profitCalculator.CalculateAccountExposure(account.ID, 
                                    out equity, out reservedMargin, out exposure, curPrices,
                                    ManagerAccount.Instance, accountRepository.GetAccountGroup);
                            }
                            catch (Exception ex)
                            {
                                loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                                                                         LogMagicExposureCheck, 60*1000, 
                                                                         "Ошибка при расчете маржи/средств счета {0}: {1}", 
                                                                         account.ID, ex);
                                break;
                            }                        
                            // посчитать отношение reservedMargin / equity, %
                            var expLevel = equity == 0 ? 0 : 
                                (equity < 0 && reservedMargin != 0) ? 100 
                                : 100 * reservedMargin / equity;
                            var isMc = expLevel >= group.MarginCallPercentLevel;
                            var isSo = expLevel >= group.StopoutPercentLevel;
                            // стопаут!
                            if (isSo)
                            {
                                if (ShouldLogStopout(account.ID))
                                    Logger.InfoFormat("Стопаут для счета {0}: нач. маржа={1:f0}, средства={2:f0}, предел={3:f1}%",
                                        account.ID, reservedMargin, equity, group.StopoutPercentLevel);
                                PerformStopout(account, curPrices, ctx);
                                continue;
                            }
                            if (!isMc) continue;
                            loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                                                                     LogMagicMarginCall,
                                                                     30 * 60 * 1000,
                                                                     "МК для счета {0}: нач. маржа={1:f0}, средства={2:f0}, предел={3:f1}%",
                                                                     account.ID, reservedMargin, equity, group.MarginCallPercentLevel);
                            PerformMarginCall(account);
                            //continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в CheckMargin", ex);
            }
        }

        private void PerformStopout(ACCOUNT ac, Dictionary<string, QuoteData> quotes,
            TradeSharpConnection ctx)
        {
            try
            {
                var sb = new StringBuilder();
                var posReq = from pos in ctx.POSITION where pos.AccountID == ac.ID 
                                 && pos.State == (int)PositionState.Opened select pos;
                var totalCount = 0;
                var closedCount = 0;
                foreach (var pos in posReq)
                {
                    totalCount++;
                    // закрыть ордер
                    QuoteData quote;
                    quotes.TryGetValue(pos.Symbol, out quote);
                    var price = quote == null ? 0 : quote.GetPrice(pos.Side > 0 ? QuoteType.Bid : QuoteType.Ask);
                    if (price == 0)
                    {
                        sb.AppendFormat("Невозможно закрыть ордер {0} ({1}): нет цены", pos.ID, pos.Symbol);
                        continue;
                    }
                    if (orderRepository.CloseOrder(LinqToEntity.DecorateOrder(pos),
                                                               (decimal) price, PositionExitReason.Stopout))
                        closedCount++;
                    sb.AppendFormat("позиция {0} закрыта по цене {1:f4} - стопаут. ", pos.ID, price);
                }
                if (ShouldLogStopout(ac.ID))
                    Logger.Info(string.Format("Счет {0}. Закрыто {1} из {2} позиций. ", ac.ID, closedCount, totalCount) + sb);
            }
            catch (Exception ex)
            {
                loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogMagicStopout, 60 * 1000, 
                    "Ошибка при выполнении стопаута {0}: {1}", ac.ID, ex);
            }
        }

        private bool ShouldLogStopout(int accountId)
        {
            DateTime lastTime;
            if (!lastTimeStopoutLogged.TryGetValue(accountId, out lastTime))
            {
                lastTimeStopoutLogged.Add(accountId, DateTime.Now);
                return true;
            }
            var timeNow = DateTime.Now;
            var deltaMinuts = (timeNow - lastTime).TotalMinutes;
            if (deltaMinuts < 5) return false;

            lastTimeStopoutLogged[accountId] = timeNow;
            return true;
        }

        private void PerformMarginCall(ACCOUNT ac)
        {
            // !! предусмотреть нотификацию?
        }
    
        /// <summary>
        /// проверить - не пора ли начислить своп?
        /// </summary>
        private void CheckSwap()
        {
            // проверить час
            var timeNow = DateTime.Now.ToUniversalTime();
            var hourGmt = timeNow.Hour;
            if (swapCheckHourGmt != hourGmt) return;
            
            // проверить день
            if (timeNow.DayOfWeek == DayOfWeek.Sunday) return;

            // проверить, не было ли уже начисления свопа
            var lastTimeChecked = swapCheckedTime.GetLastHitIfHitted();
            if (lastTimeChecked.HasValue && lastTimeChecked.Value.Date == timeNow.Date)
                return;

            // сколько минут прошло с часа Х
            if (timeNow.Minute > minutesToCheckSwap) return;

            swapCheckedTime.SetTime(timeNow);
            // таки насчитать своп
            DoMakeSwap();            
        }

        public void DoMakeSwap()
        {
            // ticker - swap buy, swap sell
            var dicSwapByTicker = new Dictionary<string, Cortege2<decimal, decimal>>();
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    foreach (var spot in ctx.SPOT)
                    {
                        var swapBuy = spot.SwapBuy ?? 0;
                        var swapSell = spot.SwapSell ?? 0;

                        if (swapBuy != 0 || swapSell != 0)
                            dicSwapByTicker.Add(spot.Title,
                                            new Cortege2<decimal, decimal>(swapBuy, swapSell));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в DoMakeSwap(dicSwapByTicker)", ex);
                return;
            }

            var quotes = QuoteStorage.Instance.ReceiveAllData();
            var nowTime = DateTime.Now;
            
            // подготовить список трансферов
            var transfers = new List<BALANCE_CHANGE>();
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var resx = ctx.GetPositionsToSwapWrapped().ToList();
                    Logger.InfoFormat("DoMakeSwap() - process {0} positions", resx.Count);
                    foreach (var pos in resx)
                    {
                        Cortege2<decimal, decimal> swap;
                        if (!dicSwapByTicker.TryGetValue(pos.Symbol, out swap)) continue;
                        var swapPoints = pos.Side > 0 ? swap.a : swap.b;
                        if (swapPoints == 0) return;
                        // абс. значение свопа
                        var swapAbs = DalSpot.Instance.GetAbsValue(pos.Symbol, swapPoints);
                        var swapMoneyCounter = swapAbs * pos.Volume;
                        // посчитать в валюте депозита и сформировать трансфер
                        var trans = MakeSwapTransfer(swapMoneyCounter, pos.Symbol, pos.Currency, quotes, pos.ID,
                                         pos.AccountID, nowTime);
                        if (trans != null)
                            transfers.Add(trans);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в DoMakeSwap(makeSwap)", ex);
            }

            if (transfers.Count == 0) return;

            // сохранить трансферы в БД
            var accountTrans = (from trans in transfers
                                group trans by trans.AccountID
                                    into groupedTransfers
                                    select groupedTransfers).ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());
            // таки пополнить / списать
            try
            {
                var countModified = 0;
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    foreach (var account in accountTrans)
                    {
                        try
                        {
                            var sumBalance = 0M;
                            foreach (var bc in account.Value)
                            {
                                ctx.BALANCE_CHANGE.Add(bc);
                                sumBalance += bc.Amount;
                            }
                            if (sumBalance != 0)
                            {
                                var accountItem = ctx.ACCOUNT.First(a => a.ID == account.Key);
                                accountItem.Balance += sumBalance;
                            }
                            countModified++;
                        }
                        catch (Exception ex)
                        {
                            Logger.ErrorFormat("Ошибка сохранения {0} начислений свопов: {1}",
                                               account.Value.Count, ex);
                        }
                    }
                    Logger.InfoFormat("Своп начислен по {0} счетам", countModified);
                    if (countModified > 0)
                        ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка начисления свопа (сохранение в БД)", ex);
            }
        }

        private BALANCE_CHANGE MakeSwapTransfer(decimal amountCounter, string ticker, string curxDepo, 
            Dictionary<string, QuoteData> quotes,
            int posId, int accountId, DateTime nowTime)
        {
            var amountDepo = amountCounter;
            bool inverse, equalPairs;
            var symbol = DalSpot.Instance.FindSymbol(ticker, false, curxDepo, out inverse, out equalPairs);
            if (!equalPairs)
            {
                if (string.IsNullOrEmpty(symbol))
                {
                    Logger.Error("MakeSwapTransfer - не найден тикер пересчета " + ticker + " в " + curxDepo);
                    return null;
                }

                QuoteData quote;
                if (!quotes.TryGetValue(symbol, out quote))
                {
                    Logger.ErrorFormat("Начисление свопа поз. {0} ({1}) - нет котировки {2}",
                        posId, ticker, symbol);
                    return null;
                }
                var price = inverse
                                    ? (amountDepo < 0 ? 1 / quote.bid : 1 / quote.ask)
                                    : (amountDepo < 0 ? quote.ask : quote.bid);
                amountDepo *= (decimal)price;
            }

            var transfer = new BALANCE_CHANGE
                {
                    Amount = amountDepo,
                    AccountID = accountId,
                    ChangeType = (int)BalanceChangeType.Swap,
                    ValueDate = nowTime,
                    Description = "swap " + posId,
                    Position = posId
                };
            return transfer;
        }
    
        /// <summary>
        /// обновить подписки на платные сервисы
        /// снять денег за подписку 
        /// если денег недостаточно - отписать
        /// </summary>
        private void RenewSubscriptions()
        {
            // пришло время проверить подписки?
            var nowTime = DateTime.Now;
            if (lastRenewedSubscriptions.HasValue)
            {
                if (lastRenewedSubscriptions.Value.Date == nowTime.Date) return;
            }
            lastRenewedSubscriptions = nowTime;

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var subList = ctx.SUBSCRIPTION.ToList();
                    foreach (var sub in subList)
                    {
                        if (sub.TimeEnd > nowTime) continue;
                        var shouldUnsubscribe = true;
                        if (sub.RenewAuto)
                        {
                            var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.ID == sub.User);
                            if (user == null)
                            {
                                Logger.ErrorFormat("User not found for a SUBSCRIPTION srv={0}, user={1}",
                                    sub.Service, sub.User);
                                continue;
                            }

                            // продлить подписку?
                            var feeChargedError = walletRepository.ChargeFeeOnSubscription(ctx, sub.Service, user.ID, true);
                            if (feeChargedError == WalletError.OK)
                                shouldUnsubscribe = false;
                        }

                        // таки отписать пользователя
                        if (shouldUnsubscribe)
                        {
                            Logger.InfoFormat(
                                "Unsubscribing user {0} from srv {1} (renew auto is {2}, time end is {3})",
                                sub.User, sub.Service, sub.RenewAuto, sub.TimeEnd);
                            try
                            {
                                walletRepository.UnsubscribeSubscriber(ctx, sub);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error("RenewSubscriptions.UnsubscribeSubscriber() error", ex);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error in RenewSubscriptions()", ex);
            }
        }
    }
}

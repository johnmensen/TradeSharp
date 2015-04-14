using System;
using System.Linq;
using Entity;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.TradeSignalExecutor.BL
{
    /// <summary>
    /// отправляет запросы на открытие / закрытие ордеров, редактирует ордера
    /// </summary>
    static class Dealer
    {
        #region Logger
        private static readonly FloodSafeLogger logNoFlood = new FloodSafeLogger(1000);

        private const int LogMsgContraOrders = 1;

        private const int LogMsgVolmCalcNoQuote = 2;

        private const int LogMsgVolmToBase = 3;

        private const int LogMsgVolmRoundToZero = 4;

        private const int LogMsgExposureNoQuote = 5;

        private const int LogMsgEquityIsNil = 6;

        private const int LogMsgMaxLevExceeded = 7;
        #endregion

        public static readonly FixedSizedQueue<ProcessedSignal> processedSignals =
            new FixedSizedQueue<ProcessedSignal>(50);

        public static ITradeSharpServerTrade fakeProxyTrade;

        private static ITradeSharpServerTrade proxyTrade;

        public static ITradeSharpServerTrade ProxyTrade
        {
            get
            {
                if (fakeProxyTrade != null)
                    return fakeProxyTrade;
                if (proxyTrade != null) return proxyTrade;
                proxyTrade = new TradeSharpServerTrade(EmptyTradeSharpCallbackReceiver.Instance).proxy;
                return proxyTrade;
            }
        }

        public static void ProcessSignal(TradeSignalAction action)
        {
            if (action is TradeSignalActionMoveStopTake)
                ProcessSignalEditOrder((TradeSignalActionMoveStopTake)action);
            else if (action is TradeSignalActionClose)
                ProcessSignalCloseOrder((TradeSignalActionClose)action);
            else if (action is TradeSignalActionTrade)
                ProcessSignalOpenOrder((TradeSignalActionTrade)action);
        }

        private static void ProcessSignalEditOrder(TradeSignalActionMoveStopTake action)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var targetOrders = ctx.POSITION.Where(p => p.MasterOrder == action.OrderId).ToList();
                    foreach (var order in targetOrders.Select(LinqToEntity.DecorateOrder))
                    {
                        // исполнить сигнал - просто модифицировать ордер
                        order.TakeProfit = (float?)action.NewTakeProfit;
                        order.StopLoss = (float?)action.NewStopLoss;

                        try
                        {
                            var status = ProxyTrade.SendEditMarketRequest(
                                ProtectedOperationContext.MakeServerSideContext(), order);
                            if (status != RequestStatus.OK)
                                Logger.DebugFormat("ProcessSignalEditOrder(#{0}) - ошибка: {1}", action.OrderId, status);
                        }
                        catch (Exception ex)
                        {
                            Logger.DebugFormat("ProcessSignalEditOrder(#{0}) - исключение при отправке запроса: {1}",
                                               action.OrderId, ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.DebugFormat("ProcessSignalEditOrder(#{0}) - исключение: {1}", action.OrderId, ex);
            }
        }

        private static void ProcessSignalCloseOrder(TradeSignalActionClose action)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var targetPositions = ctx.POSITION.Where(p => p.MasterOrder ==
                        action.OrderId).ToList().Select(LinqToEntity.DecorateOrder);

                    foreach (var order in targetPositions)
                    {
                        // исполнить сигнал - закрыть ордер
                        try
                        {
                            var status = ProxyTrade.SendCloseRequest(ProtectedOperationContext.MakeServerSideContext(),
                                                                     order.AccountID, order.ID,
                                                                     PositionExitReason.ClosedBySignal);
                            if (status != RequestStatus.OK)
                                Logger.DebugFormat("ProcessSignalCloseOrder(#{0}) - ошибка: {1}", action.OrderId, status);
                        }
                        catch (Exception ex)
                        {
                            Logger.DebugFormat("ProcessSignalCloseOrder(#{0}) - исключение при отправке запроса: {1}",
                                               action.OrderId, ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.DebugFormat("ProcessSignalCloseOrder(#{0}) - исключение: {1}", action.OrderId, ex);
            }
        }

        private static void ProcessSignalOpenOrder(TradeSignalActionTrade action)
        {
            try
            {
                var signalRecord = EnqueueProcessedSignalRecord(action);

                using (var ctx = DatabaseContext.Instance.Make())
                {
                    // получить информацию по настройкам торговых сигналов
                    var subscriptions = (from subsSig in ctx.SUBSCRIPTION_SIGNAL
                                         where (subsSig.AutoTrade ?? false) && subsSig.Service == action.ServiceId && subsSig.TargetAccount.HasValue
                                         select subsSig).ToList();

                    foreach (var sub in subscriptions)
                    {

                        var volm = sub.FixedVolume ?? 0;
                        if (volm == 0)
                            volm = GetNewOrderVolume(action, ctx, sub);

                        // объем входа недопустимо мал либо ошибка в расчете
                        if (volm == 0)
                            return;

                        var order = new MarketOrder
                        {
                            // ReSharper disable PossibleInvalidOperationException
                            AccountID = sub.TargetAccount.Value,
                            // ReSharper restore PossibleInvalidOperationException
                            Volume = volm,
                            Side = action.Side,
                            Symbol = action.Ticker,
                            StopLoss = (float?)action.StopLoss,
                            TakeProfit = (float?)action.TakeProfit,
                            Magic = action.OrderId,
                            ExpertComment =
                                MarketOrder.MakeSignalComment(
                                    action.ServiceId),
                            MasterOrder = action.OrderId
                        };

                        try
                        {
                            var status =
                                ProxyTrade.SendNewOrderRequest(ProtectedOperationContext.MakeServerSideContext(),
                                                               action.OrderId, order, OrderType.Market,
                                                               (decimal)action.Price, 0);

                            if (signalRecord.subscriberAccountProcessingStatus.ContainsKey(order.AccountID))                            
                                Logger.ErrorFormat("Сигнал {0} - повторная отправка на счет {1}",
                                    action, order.AccountID);
                            else
                                signalRecord.subscriberAccountProcessingStatus[order.AccountID] = status;

                            if (status != RequestStatus.OK)
                                Logger.ErrorFormat("ProcessSignalOpenOrder - ошибка обработки сервером" +
                                                   " запроса SendNewOrderRequest, master order: {0}, account: {1}, {2}",
                                                   action.OrderId, sub.TargetAccount, status);
                        }
                        catch (Exception ex)
                        {
                            Logger.ErrorFormat(
                                "ProcessSignalOpenOrder - ошибка отправки запроса SendNewOrderRequest, master order: {0}, account: {1}, {2}",
                                action.OrderId, sub.TargetAccount, ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.DebugFormat("ProcessSignalOpenOrder(#{0} - {1}) - исключение: {2}",
                    action.OrderId, action.Ticker, ex);
            }
        }

        public static int GetNewOrderVolume(TradeSignalActionTrade action,
            TradeSharpConnection ctx, SUBSCRIPTION_SIGNAL tradeSets)
        {
            // получить ордера по счету, посчитать актуальный баланс (средства) счета
            try
            {
                var account = ctx.ACCOUNT.FirstOrDefault(a => a.ID == tradeSets.TargetAccount.Value);
                if (account == null)
                {
                    Logger.ErrorFormat("Ошибка при расчете объема по счету {0}, ордер {1}: счет не найден",
                        tradeSets.TargetAccount, action.OrderId);
                    return 0;
                }

                var equity = (double)account.Balance;

                // ордера
                var orders = ctx.POSITION.Where(p => p.AccountID == tradeSets.TargetAccount &&
                    p.State == (int)PositionState.Opened).ToList().Select(o => LinqToEntity.DecorateOrder(o)).ToList();

                // проверить - нет ли противоположного ордера?
                var quotes = Contract.Util.BL.QuoteStorage.Instance.ReceiveAllData();

                if (orders.Count > 0)
                {
                    if (!(tradeSets.HedgingOrdersEnabled ?? false))
                    {
                        var hasOrdersCounter = orders.Any(p => p.Symbol == action.Ticker && p.Side != action.Side);
                        if (hasOrdersCounter)
                        {
                            logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info, LogMsgContraOrders,
                                1000 * 60 * 90, "Ордер {0} {1}, счет {2} - есть встречные ордера",
                                action.Side > 0 ? "BUY" : "SELL", action.Ticker, tradeSets.TargetAccount);
                            return 0;
                        }
                    }

                    // посчитать профит по позам
                    bool noQuoteError;
                    var openResult = DalSpot.Instance.CalculateOpenedPositionsCurrentResult(
                        orders, quotes, account.Currency, out noQuoteError);
                    if (noQuoteError)
                    {
                        logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogMsgVolmCalcNoQuote,
                            1000 * 60 * 90, "GetNewOrderVolume - нет котировки");
                        return 0;
                    }
                    equity += openResult;
                }

                if (equity <= 0)
                {
                    logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogMsgEquityIsNil,
                            1000 * 60 * 90, "Баланс счета {0}: {1}", account.ID, equity);
                    return 0;
                }

                // посчитать объем
                var volumeDepo = (tradeSets.PercentLeverage ?? 100) / 100f * equity * (double)action.Leverage;
                // пересчитать объем в базовую валюту
                string errorStr;
                var volumeBase = DalSpot.Instance.ConvertToTargetCurrency(action.Ticker, true, account.Currency, volumeDepo,
                                                         quotes, out errorStr);
                if (!volumeBase.HasValue)
                {
                    logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogMsgVolmToBase,
                            1000 * 60 * 90, "GetNewOrderVolume - невозможно посчитать объем по сделке {0}, валюта счета {1}: {2}",
                            action.Ticker, account.Currency, errorStr);
                    return 0;
                }

                // округлить объем
                var volume = MarketOrder.RoundDealVolume((int)volumeBase,
                    (VolumeRoundType)(tradeSets.VolumeRound ?? (int)VolumeRoundType.Ближайшее),
                    tradeSets.MinVolume ?? 10000, tradeSets.StepVolume ?? 10000);
                if (volume == 0)
                {
                    var msg = string.Format("Полученный объем входа ({0}) округлен до 0", (int)volumeBase);
                    logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                        LogMsgVolmRoundToZero, 1000 * 60 * 90, msg);
                    return 0;
                }

                // проверить на превышение плеча
                if ((tradeSets.MaxLeverage ?? 0) > 0)
                {
                    var totalExposure = 0M;
                    var ordersBySignaller = orders.Where(position =>
                    {
                        int orderSignalCatId, parentDealId;
                        if (MarketOrder.GetTradeSignalFromDeal(position, out orderSignalCatId, out parentDealId))
                            return orderSignalCatId == action.ServiceId;
                        return false;
                    }).GroupBy(o => o.Symbol).ToDictionary(o => o.Key, o => o.ToList());

                    foreach (var orderByTicker in ordersBySignaller)
                    {
                        // суммарная экспозиция
                        var exp = orderByTicker.Value.Sum(o => o.Side * o.Volume);
                        if (orderByTicker.Key == action.Ticker)
                            exp += action.Side * volume;
                        if (exp == 0) continue;

                        // перевести экспозицию в валюту депозита
                        var expBase = DalSpot.Instance.ConvertToTargetCurrency(action.Ticker, true, account.Currency,
                                                                               volumeDepo,
                                                                               quotes, out errorStr);
                        if (!expBase.HasValue)
                        {
                            logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogMsgExposureNoQuote,
                                                                  1000 * 60 * 90,
                                                                  "GetNewOrderVolume - невозможно посчитать экспозицию по сделке " +
                                                                  "{0}, тикер {1}: {2}",
                                                                  action.Ticker, orderByTicker.Key, errorStr);
                            return 0;
                        }

                        totalExposure += Math.Abs(expBase.Value);
                    }
                    var totalLeverage = (double)totalExposure / equity;
                    // ReSharper disable PossibleInvalidOperationException
                    if (totalLeverage > tradeSets.MaxLeverage.Value)
                    // ReSharper restore PossibleInvalidOperationException
                    {
                        logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogMsgMaxLevExceeded,
                                                                  1000 * 60 * 90,
                                                                  "GetNewOrderVolume - макс плечо ({0}) превышено, " +
                                                                  "тикер {1}: плечо составит {2}",
                                                                  tradeSets.MaxLeverage.Value,
                                                                  action.Ticker, totalLeverage);
                        return 0;
                    }
                }

                return volume;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка при расчете объема по счету {0}, ордер {1}: {2}",
                    tradeSets.TargetAccount, action.OrderId, ex);
            }

            return 0;
        }
    
        private static ProcessedSignal EnqueueProcessedSignalRecord(TradeSignalAction action)
        {
            var record = new ProcessedSignal {signal = action};
            processedSignals.Enqueue(record);
            return record;
        }
    }
}

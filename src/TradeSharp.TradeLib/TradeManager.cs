using System;
using System.Collections.Generic;
using System.Linq;
using Entity;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.TradeLib
{
    public class TradeManager
    {        
        private readonly ITradeSharpServerTrade proxyTrade;
        
        private readonly ITradeSharpAccount proxyAccount;

        private IProfitCalculator profitCalculator;

        private readonly IStorage<string, QuoteData> quoteStorage;

        public Func<int, AccountGroup> getAccountGroup;

        private readonly FloodSafeLogger floodSafeLogger = new FloodSafeLogger(2000);
        
        private const int MsgNoQuoteMagic = 1;
        
        private const int MsgMoveStopMagic = 3;
        
        private readonly DateTime timeStarted;
        
        private const int MinMillsToReportError = 1000 * 30;

        /// <summary>
        /// проверка ордера отключается на OrderFloodTimoutMils, если ордер не может 
        /// быть закрыт после первой попытки
        /// </summary>
        public readonly Dictionary<int, DateTime> orderFloodTimes = new Dictionary<int, DateTime>();

        /// <summary>
        /// проверка отложенного ордера отключается на PendingFloodTimoutMils, если ордер не может 
        /// быть активирован после первой попытки
        /// </summary>
        public readonly Dictionary<int, DateTime> pendingFloodTimes = new Dictionary<int, DateTime>();

        public const int OrderFloodTimeoutMils = 2000;
        
        public const int PendingFloodTimeoutMils = 5000;

        public TradeManager(ITradeSharpServerTrade proxyTrade, 
            ITradeSharpAccount proxyAccount,
            IStorage<string, QuoteData> quoteStorage, Func<int, AccountGroup> getAccountGroup)
        {
            profitCalculator = ProfitCalculator.Instance;
            if (getAccountGroup == null) throw new ArgumentException("TradeManager ctor()", "getAccountGroup");
            this.proxyTrade = proxyTrade;
            this.proxyAccount = proxyAccount;
            this.quoteStorage = quoteStorage;
            this.getAccountGroup = getAccountGroup;
            timeStarted = DateTime.Now;
        }

        /// <summary>
        /// Проверяет срабатываение отложенных ордеров
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public List<PendingOrder> CheckPendingOrders(int accountId)
        {
            var execOrders = new List<PendingOrder>();
            List<PendingOrder> orders;
            var status = proxyAccount.GetPendingOrders(accountId, out orders);
            if (status != RequestStatus.OK)
                return execOrders;
            
            foreach (var ord in orders)
            {
                var quote = quoteStorage.ReceiveValue(ord.Symbol);
                if (ord.Side == 1 && ord.PriceSide == PendingOrderType.Stop && quote.ask >= ord.PriceFrom
                    && (!ord.PriceTo.HasValue || quote.ask <= ord.PriceTo.Value))
                    execOrders.Add(ord);
                else
                    if (ord.Side == 1 && ord.PriceSide == PendingOrderType.Limit && quote.ask <= ord.PriceFrom
                        && (!ord.PriceTo.HasValue || quote.ask >= ord.PriceTo.Value))
                        execOrders.Add(ord);
                    else
                        if (ord.Side == -1 && ord.PriceSide == PendingOrderType.Stop && quote.bid <= ord.PriceFrom
                            && (!ord.PriceTo.HasValue || quote.ask >= ord.PriceTo.Value))
                            execOrders.Add(ord);
                        else
                            if (ord.Side == -1 && ord.PriceSide == PendingOrderType.Limit && quote.bid >= ord.PriceFrom
                                && (!ord.PriceTo.HasValue || quote.ask <= ord.PriceTo.Value))
                                execOrders.Add(ord);
            }
            return execOrders;
        }
           
        /// <summary>
        /// возвращает позиции, которые надо закрыть
        /// </summary>
        public List<MarketOrder> CheckMarketOrders(int accountId)
        {
            var execOrders = new List<MarketOrder>();
            List<MarketOrder> orders;
            var status = proxyAccount.GetMarketOrders(accountId, out orders);
            if (status != RequestStatus.OK)
                return execOrders;
            foreach (var pos in orders)
            {
                var quote = quoteStorage.ReceiveValue(pos.Symbol);
                if (quote == null)
                    continue;

                CheckOrderTrailing(pos, quote);

                if (pos.Side == 1)
                {
                    if (pos.StopLoss != null && pos.StopLoss >= quote.bid)
                    {
                        execOrders.Add(pos);
                        pos.ExitReason = PositionExitReason.SL;
                    }
                    else
                        if (pos.TakeProfit != null && pos.TakeProfit <= quote.bid)
                        {
                            execOrders.Add(pos);
                            pos.ExitReason = PositionExitReason.TP;
                        }
                }
                else
                {
                    // sell позиция
                    if (pos.StopLoss != null && pos.StopLoss <= quote.ask)
                    {
                        execOrders.Add(pos);
                        pos.ExitReason = PositionExitReason.SL;
                    }
                    else
                        if (pos.TakeProfit != null && pos.TakeProfit >= quote.ask)
                        {
                            execOrders.Add(pos);
                            pos.ExitReason = PositionExitReason.TP;
                        }
                }
            }
            return execOrders;
        }

        public bool IsEnterEnabled(int accountId, string symbol, int side, int volume, out decimal equity)
        {
            Account account;
            equity = 0;

            var status = proxyAccount.GetAccountInfo(accountId, false, out account);
            if (status != RequestStatus.OK || account == null)
            {
                Logger.ErrorFormat("IsEnterEnabled - не удалось получить счет ({0})", accountId);
                return false;
            }

            var curPrices = quoteStorage.ReceiveAllData();
            decimal reservedMargin, exposure;
            var accountExposure = profitCalculator.CalculateAccountExposure(
                accountId, out equity, out reservedMargin,
                out exposure, curPrices, proxyAccount, getAccountGroup);
            account.Equity = equity;
            // посчитать маржу с учетом новой позы
            var smbExp = accountExposure.ContainsKey(symbol) ? accountExposure[symbol] : 0;
            var smbExpNew = Math.Abs(smbExp + side * volume);
            smbExp = Math.Abs(smbExp);
            if (smbExpNew <= smbExp) // новая поза не увеличивает экспозицию
                return true;
            // посчитать новую экспозицию
            var deltaExp = (smbExpNew - smbExp);
            // перевести в валюту депо
            string errorStr;
            var deltaExpDepo = DalSpot.Instance.ConvertToTargetCurrency(symbol, true, account.Currency,
                (double)deltaExp, curPrices, out errorStr, false);
            if (!deltaExpDepo.HasValue)
            {
                Logger.ErrorFormat("IsEnterEnabled - не удалось конвертировать базовую {0} в депо {1}",
                    symbol, account.Currency);
                return false;
            }

            var newExp = deltaExpDepo.Value + exposure;
            // проверить маржинальные требования
            var group = getAccountGroup(account.ID);
            var deltaMargin = deltaExp / (decimal)group.BrokerLeverage;
            var newMargin = deltaMargin + reservedMargin;
            var margPercent = equity == 0 ? 100 : 100 * newMargin / equity;
            if (margPercent >= (decimal)group.MarginCallPercentLevel)
            {
                Logger.InfoFormat("Счет {0}: запрос на {1} {2} {3} превышает марж. требования ({4:f2} из {5:f2})",
                    accountId, side > 0 ? "BUY" : "SELL", volume, symbol, margPercent, group.MarginCallPercentLevel);
                Logger.InfoFormat("{0:f1} {1:f1} {2:f1} {3:f1} {4:f1} {5} {6:f1} {7:f1}",
                                  smbExp, smbExpNew, deltaExp, exposure,
                                  equity, accountExposure.Count, deltaMargin, account.Balance);                
                return false;
            }
            // окончательно - проверить макс. плечо
            if (equity == 0 || account.MaxLeverage == 0) return true;
            var newLeverage = newExp / equity;
            if (newLeverage < (decimal)account.MaxLeverage) return true;
            Logger.InfoFormat("Превышение плеча ({0} при допустимом {1})", newLeverage, account.MaxLeverage);
            return false;
        }
        
        /// <summary>
        /// true - наступление события стоп-аута по открытым позициям на счете
        /// </summary>        
        public bool CheckStopOut(int accountId)
        {
            decimal reservedMargin, exposure, equity;
            profitCalculator.CalculateAccountExposure(
                accountId, out equity, out reservedMargin,
                out exposure, quoteStorage.ReceiveAllData(), proxyAccount, getAccountGroup);
            if (exposure == 0) return false;
            Account account;
            var status = proxyAccount.GetAccountInfo(accountId, true, out account);
            if (status != RequestStatus.OK)
                return false;
            var group = getAccountGroup(account.ID);
            if (group == null) return false;
            return reservedMargin >= (100 - (decimal)group.StopoutPercentLevel)/100M * equity;
        }

        /// <returns>символ - экспозиция</returns>
        

        public void CheckOrder(MarketOrder pos, Dictionary<string, QuoteData> curPrices)
        {
            if (!curPrices.ContainsKey(pos.Symbol))
            {
                if ((DateTime.Now - timeStarted).TotalMilliseconds > MinMillsToReportError)
                    floodSafeLogger.LogMessageFormatCheckFlood(LogEntryType.Debug, MsgNoQuoteMagic, 60 * 1000 * 5,
                                           "CheckOrder: нет котировки по {0} для позиции #{1} ({2} {3})",
                                           pos.Symbol,
                                           pos.ID,
                                           pos.Side > 0 ? "BUY" : "SELL",
                                           pos.Volume);
                return;
            }
            var price = curPrices[pos.Symbol];
            // стоп
            var isSl = false;
            if (pos.StopLoss > 0) isSl = pos.Side > 0 ? price.bid <= pos.StopLoss : price.ask >= pos.StopLoss;
            if (isSl)
            {
                Logger.InfoFormat("Закрытие ордера #{0} ({1} {2} {3}) по SL {4}",
                    pos.ID, pos.Side > 0 ? "BUY" : "SELL", pos.Volume.ToStringUniformMoneyFormat(), pos.Symbol,
                    (pos.StopLoss ?? 0).ToStringUniformPriceFormat());
                TryCloseOrder(pos, PositionExitReason.SL);
                return;
            }
            // тейк
            var isTp = false;
            if (pos.TakeProfit > 0) isTp = pos.Side > 0 ? price.bid >= pos.TakeProfit : price.ask <= pos.TakeProfit;
            if (isTp)
            {
                Logger.InfoFormat("Закрытие ордера #{0} ({1} {2} {3}) по TP {4}",
                    pos.ID, pos.Side > 0 ? "BUY" : "SELL", pos.Volume.ToStringUniformMoneyFormat(), pos.Symbol,
                    (pos.TakeProfit ?? 0).ToStringUniformPriceFormat());
                TryCloseOrder(pos, PositionExitReason.TP);
                return;
            }
            // трейлинг
            CheckOrderTrailing(pos, price);
        }

        public void CheckOrderTrailing(MarketOrder pos, QuoteData price)
        {
            const int minPipsToMoveStop = 1;

            if (!pos.TrailLevel1.HasValue) return;
            var resultAbs = pos.Side > 0 ? price.bid - pos.PriceEnter : pos.PriceEnter - price.ask;
            var resultPoints = DalSpot.Instance.GetPointsValue(pos.Symbol, resultAbs);
            for (var i = pos.trailingLevels.Length - 1; i >= 0; i--)
            {
                if ((pos.trailingLevels[i] == null) || (pos.trailingTargets[i] == null)) continue;
                if (resultPoints >= pos.trailingLevels[i].Value)
                {
                    // перетащить стоп
                    var targetStop = pos.PriceEnter + pos.Side *
                                     DalSpot.Instance.GetAbsValue(pos.Symbol, pos.trailingTargets[i].Value);
                    if ((pos.StopLoss == null || (targetStop > pos.StopLoss && pos.Side > 0) ||
                         (targetStop < pos.StopLoss && pos.Side < 0)))
                    {
                        // проверить расстояние между целевым стопом и текущим стопом
                        var deltaSlPips = Math.Abs(DalSpot.Instance.GetPointsValue(pos.Symbol,
                                                                                   targetStop - (pos.StopLoss ?? 0)));
                        if (deltaSlPips >= minPipsToMoveStop)
                        {
                            // вот тут уж точно перетащить стоп
                            Logger.InfoFormat("Установка SL для позиции #{0} с {1:f5} на {2:f5}",
                                              pos.ID, pos.StopLoss ?? 0, targetStop);
                            MoveStop(pos, targetStop);
                            break;
                        }
                    }
                }
            }
        }

        public void MoveStop(MarketOrder pos, float targetSl)
        {
            pos.StopLoss = targetSl;
            if (proxyTrade.SendEditMarketRequest(ProtectedOperationContext.MakeServerSideContext(), pos) != RequestStatus.OK)
                floodSafeLogger.LogMessageFormatCheckFlood(LogEntryType.Error, MsgMoveStopMagic, 60 * 1000,
                                           "Ошибка установки SL для позиции #{0} на {1:f5}: {2}", pos.ID, targetSl);
        }

        private void TryCloseOrder(MarketOrder pos, PositionExitReason reason)
        {
            // проверка - не вызывается ли метод раз за разом для одной и той же позиции,
            // которую невозможно закрыть, например, из-за недостатка свободной маржи
            DateTime lastTime;
            if (orderFloodTimes.TryGetValue(pos.ID, out lastTime))            
                if (DateTime.Now < lastTime) return;
            orderFloodTimes.Remove(pos.ID);

            // отправить запрос
            proxyTrade.SendCloseRequest(ProtectedOperationContext.MakeServerSideContext(), pos.AccountID, pos.ID, reason);
            
            // не допустить забивание потока попытками закрыть позу...            
            orderFloodTimes.Add(pos.ID, DateTime.Now.AddMilliseconds(OrderFloodTimeoutMils));
        }

        public void CheckPendingOrder(PendingOrder order, Dictionary<string, QuoteData> curPrices)
        {
            // проверка на зафлаживание
            if (pendingFloodTimes.ContainsKey(order.ID))
            {
                var time = pendingFloodTimes[order.ID];
                if (DateTime.Now < time) return;
                    pendingFloodTimes.Remove(order.ID);
            }
            // отменить устаревший ордер)
            if (order.TimeTo.HasValue)
                if (DateTime.Now > order.TimeTo)
                {
                    //var pairCancel = curPrices.ContainsKey(order.Symbol) ? curPrices[order.Symbol] : null;
                    //var priceCancel = pairCancel == null ? 0 : order.Side > 0 ? pairCancel.Bid : pairCancel.Ask;
                    proxyTrade.SendDeletePendingOrderRequest(ProtectedOperationContext.MakeServerSideContext(), 
                        order, PendingOrderStatus.Отменен, null, "Экспирация");
                    return;
                }
            // получить текущую котировку
            if (!curPrices.ContainsKey(order.Symbol))
            {
                if ((DateTime.Now - timeStarted).TotalMilliseconds > MinMillsToReportError)
                    floodSafeLogger.LogMessageFormatCheckFlood(LogEntryType.Debug, MsgNoQuoteMagic, 5 * 60 * 1000,
                                           "CheckOrder: нет котировки по {0} для отлож. ордера #{1}",
                                           order.Symbol, order.ID);
                return;
            }
            var pricePair = curPrices[order.Symbol];
            var price = order.Side > 0 ? pricePair.ask : pricePair.bid;

            // проверить нахождение в рамках цены и времени
            if (order.TimeFrom.HasValue)
                if (DateTime.Now < order.TimeFrom) return;
            // цена...
            var orderFires = false;
            if (order.Side > 0 && ((PendingOrderPriceSide)order.PriceSide) == PendingOrderPriceSide.Limit)
            {
                if (price <= order.PriceFrom && (order.PriceTo == null ? true : price >= order.PriceTo)) orderFires = true;
            }
            else
                if (order.Side < 0 && ((PendingOrderPriceSide)order.PriceSide) == PendingOrderPriceSide.Limit)
                {
                    if (price >= order.PriceFrom && (order.PriceTo == null ? true : price <= order.PriceTo)) orderFires = true;
                }
                else
                    if (order.Side > 0 && ((PendingOrderPriceSide)order.PriceSide) == PendingOrderPriceSide.Stop)
                    {
                        if (price >= order.PriceFrom && (order.PriceTo == null ? true : price <= order.PriceTo)) orderFires = true;
                    }
                    else
                    {
                        if (price <= order.PriceFrom && (order.PriceTo == null ? true : price >= order.PriceTo)) orderFires = true;
                    }
            if (!orderFires) return;
            Logger.InfoFormat("Активация отлож. ордера #{0} ({1} {2} at {3}), цена {4}", 
                order.ID, (DealType)order.Side, order.PriceSide, 
                order.PriceFrom.ToStringUniformPriceFormat(true),
                price.ToStringUniformPriceFormat(true));
            
            // активировать ордер
            var orderMarket = new MarketOrder
                {
                    AccountID = order.AccountID,
                    Magic = order.Magic,
                    Symbol = order.Symbol,
                    Volume = order.Volume,
                    Side = order.Side,
                    StopLoss = order.StopLoss,
                    TakeProfit = order.TakeProfit,
                    Comment = string.Format("по ордеру #{0}", order.ID)
                };

            var reqStatus = proxyTrade.SendNewOrderRequest(
                    ProtectedOperationContext.MakeServerSideContext(),
                    RequestUniqueId.Next(),
                    orderMarket,
                    OrderType.Market,
                    0, 0); // проскальзывание
            if (reqStatus == RequestStatus.OK)
            {
                // удалить парный ордер
                if (order.PairOCO.HasValue && order.PairOCO > 0)
                {
                    List<PendingOrder> orders;
                    try
                    {
                        proxyAccount.GetPendingOrders(order.AccountID, out orders);
                        var orderPair = orders == null ? null : orders.FirstOrDefault(o => o.ID == order.PairOCO.Value);
                        if (orderPair != null)
                            proxyTrade.SendDeletePendingOrderRequest(ProtectedOperationContext.MakeServerSideContext(), 
                                orderPair, PendingOrderStatus.Отменен, null, "OCO");
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorFormat("Ошибка удаления парного ордера {0} для ордера {1}: {2}",
                            order.PairOCO, order, ex);
                    }
                }
                
                // удалить ордер
                proxyTrade.SendDeletePendingOrderRequest(ProtectedOperationContext.MakeServerSideContext(), 
                    order, PendingOrderStatus.Отменен, null, "Активирован");
            }
            else
            {
                // зафлаживание
                Logger.ErrorFormat("Ошибка активации отлож. ордера #{0}: {1}", order.ID, reqStatus);
                var nextTime = DateTime.Now.AddMilliseconds(PendingFloodTimeoutMils);
                if (pendingFloodTimes.ContainsKey(order.ID))
                    pendingFloodTimes[order.ID] = nextTime;
                else
                    pendingFloodTimes.Add(order.ID, nextTime);
            }
        }        
    }
}

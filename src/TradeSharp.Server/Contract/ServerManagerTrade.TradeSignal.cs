using System;
using Entity;
using System.Linq;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.Server.Contract
{
    public partial class ManagerTrade
    {
        private void MakeSignalNewDeal(
            int accountId, 
            string ticker,
            int side,
            int volume,
            decimal? stopLoss, 
            decimal? takeProfit,
            int orderId,
            float priceEnter)
        {
            // разослать сигнал подписчикам?            
            var signalCat = ManagementTraderList.Instance.IsSignaller(accountId);
            if (!signalCat.HasValue) return;
            Logger.InfoFormat("MakeSignalNewDeal (cat={0})", signalCat);

            // посчитать плечо сделки
            var leverage = CalculateDealLeverage(accountId, ticker, volume);
            if (leverage == null)
            {
                Logger.InfoFormat("MakeSignalNewDeal({0}, {1}, {2}, {3}) - leverage is null",
                    accountId, ticker, side, volume);
                return;
            }

            // таки разослать
            var action = new TradeSignalActionTrade
            {
                ServiceId = signalCat.Value,
                Price = priceEnter,                
                Side = side,
                Ticker = ticker,
                StopLoss = stopLoss ?? 0,
                TakeProfit = takeProfit ?? 0,
                OrderId = orderId,
                Leverage = leverage.Value
            };

            Logger.InfoFormat("MakeSignalNewDeal({0}, {1}, {2}, {3}) - sending signal (leverage is {4:f2})",
                    accountId, ticker, side, volume, leverage.Value);
            ServiceManagerClientManagerProxy.Instance.ProcessTradeSignalAction(action);
        }

        private static void MakeOrderChangedSignal(int accountId, int orderId, decimal? newStop, decimal? newTake)
        {
            var signalCat = ManagementTraderList.Instance.IsSignaller(accountId);
            if (!signalCat.HasValue) return;
            Logger.Info("MakeOrderChangedSignal"); // !!
            var action = new TradeSignalActionMoveStopTake
                             {
                                 OrderId = orderId,
                                 ServiceId = signalCat.Value,
                                 NewStopLoss = newStop == 0 ? null : newStop,
                                 NewTakeProfit = newTake == 0 ? null : newTake                                 
                             };
            ServiceManagerClientManagerProxy.Instance.ProcessTradeSignalAction(action);
        }

        public MarketOrder GetMarketOrder(int orderId)
        {
            using (var ctx = DatabaseContext.Instance.Make())
                try
                {
                    var ps = from pos in ctx.POSITION
                             where pos.ID == orderId
                             select pos;
                    // ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var position in ps)
                        // ReSharper restore LoopCanBeConvertedToQuery                    
                        return LinqToEntity.DecorateOrder(position);
                    return null;
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Ошибка в GetMarketOrder({0}): {1}", orderId, ex);
                    return null;
                }
        }

        private decimal? CalculateDealLeverage(int accountId, string ticker, int dealVolume)
        {
            var curPrices = QuoteStorage.Instance.ReceiveAllData();
            if (curPrices == null || curPrices.Count == 0) return null;
            
            decimal reservedMargin, exposure, equity;
            profitCalculator.CalculateAccountExposure(accountId, out equity, out reservedMargin,
                out exposure, curPrices, ManagerAccount.Instance, accountRepository.GetAccountGroup);
            if (equity == 0)
            {
                Logger.Error("ManagerTrade.CalculateDealLeverage - equity is 0");
                return null;
            }
            // получить валюту счета
            string acCurx;
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var account = ctx.ACCOUNT.FirstOrDefault(a => a.ID == accountId);
                    if (account == null)
                    {
                        Logger.ErrorFormat("CalculateDealLeverage - can not get account {0}", accountId);
                        return null;
                    }
                    acCurx = account.Currency;
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("CalculateDealLeverage - error receiving account {0}: {1}", accountId, ex);
                return null;
            }

            // перевести объем сделки в валюту депозита
            string strError;
            var volume = DalSpot.Instance.ConvertToTargetCurrency(ticker, true, acCurx, dealVolume,
                                                                  curPrices, out strError);
            if (!string.IsNullOrEmpty(strError))
                Logger.ErrorFormat("ManagerTrade.CalculateDealLeverage - {0}", strError);
            return volume == null ? (decimal?)null : volume.Value / equity;
        }
    }
}

        
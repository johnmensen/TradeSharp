using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Robot.BL
{
    class MarketOrderSafeStorage
    {
        private readonly ReaderWriterLock lockOrders = new ReaderWriterLock();
        private readonly int ordersTimeout;
        private readonly int intervalToUpdateMils;
        private DateTime? lastUpdateTime;
        private List<MarketOrder> currentOrders;
        private readonly BacktestServerProxy.RobotContext robotContext;

        public MarketOrderSafeStorage(int ordersTimeout, int intervalToUpdateMils,
            BacktestServerProxy.RobotContext robotContext)
        {
            this.ordersTimeout = ordersTimeout;
            this.intervalToUpdateMils = intervalToUpdateMils;
            this.robotContext = robotContext;
        }

        public List<MarketOrder> CurrentOrders
        {
            get
            {
                // если недавно обновлялись...
                var needUpdate = !lastUpdateTime.HasValue;
                if (!needUpdate)
                    needUpdate = (DateTime.Now - lastUpdateTime.Value).TotalMilliseconds >= intervalToUpdateMils;

                if (needUpdate)
                {
                    // обновить ордера
                    List<MarketOrder> orders;
                    var result = robotContext.GetMarketOrders(robotContext.AccountInfo.ID, out orders);
                    if (result == RequestStatus.OK)
                    {
                        lastUpdateTime = DateTime.Now;
                        try
                        {
                            lockOrders.AcquireWriterLock(ordersTimeout);
                        }
                        catch (ApplicationException)
                        {
                            Logger.Error("LevelBreakRobot - таймаут обновления ордеров");
                            return null;
                        }
                        try
                        {
                            currentOrders = orders;
                            return orders.ToList();
                        }
                        finally
                        {
                            lockOrders.ReleaseWriterLock();
                        }
                    }
                    Logger.ErrorFormat("Запрос ордеров (CheckProtectCondition) вернул {0}", result);
                }                                
                
                try
                {
                    lockOrders.AcquireReaderLock(ordersTimeout);
                }
                catch (ApplicationException)
                {
                    Logger.Error("LevelBreakRobot - таймаут чтения ордеров");
                    return null;
                }
                try
                {
                    return currentOrders.ToList();
                }
                finally
                {
                    lockOrders.ReleaseReaderLock();
                }
            }
        }
    }
}

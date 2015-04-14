using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.TradeSignalExecutor.BL
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, 
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class SignalExecutor : Scheduler, ITradeSignalExecutor
    {
        #region Signletone
        
        private static readonly Lazy<SignalExecutor> instance = new Lazy<SignalExecutor>(() => new SignalExecutor());

        public static SignalExecutor Instance
        {
            get { return instance.Value; }
        }

        private SignalExecutor()
        {
            serverProxy = ProxyBuilder.Instance.MakeImplementer<IPlatformManager>(true);
            schedules = new[]
                {
                    new Schedule(CheckStaleOrders, AppConfig.GetIntParam("StaleOrdersCheck.Interval", 1000*5)),
                    new Schedule(ProcessTradeSignalsPending, AppConfig.GetIntParam("TradeSignalsProcessing.Interval", 1000)),
                    new DailySchedule(passedValue => PortfolioBalancer.Instance.UpdatePortfolios(),
                        1000 * 10, null, 0, 0, 0, 1, "Обновление портфелей")
                };
        }
        
        #endregion

        private readonly IPlatformManager serverProxy;

        private readonly ThreadSafeList<TradeSignalAction> actionsPending = new ThreadSafeList<TradeSignalAction>();

        private long totalActionsProcessed;

        private readonly ThreadSafeTimeStamp lastTimePortfoliosBalanced = new ThreadSafeTimeStamp();

        #region FloodSafeLogger
        private readonly FloodSafeLogger logNoFlood = new FloodSafeLogger(1000);

        private const int LogCheckStaleOrdersError = 1;        
        private const int LogStaleOrdersFound = 2;
        private const int LogCheckStaleOrdersExecuteError = 3;
        private const int LogOrdersAreProcessed = 4;
        #endregion

        public void ProcessTradeSignals(List<TradeSignalAction> actions)
        {
            actionsPending.AddRange(actions, 2000);
        }

        private void CheckStaleOrders()
        {
            List<int> stalePositions = null;
            try
            {
                // позиции, мастер - ордера по которым закрыты
                stalePositions = GetStaleOrders();                
            }
            catch (Exception ex)
            {
                logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogCheckStaleOrdersError, 1000 * 60 * 15,
                    "Ошибка в CheckStaleOrders(): " + ex);
            }

            if (stalePositions == null || stalePositions.Count == 0)
                return;

            logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogStaleOrdersFound, 1000 * 60 * 10,
                        "CheckStaleOrders({0})", stalePositions.Count);

            // отправить пачку запросов на закрытие позиций
            const int maxRequestsInPack = 15;
            for (var i = 0; i < stalePositions.Count; i += maxRequestsInPack)
            {
                var count = (i + maxRequestsInPack) < stalePositions.Count
                                ? maxRequestsInPack
                                : stalePositions.Count - i;
                var subQuery = stalePositions.GetRange(i, count).ToArray();

                try
                {
                    serverProxy.SendCloseRequests(subQuery, PositionExitReason.ClosedBySignal);
                }
                catch (Exception ex)
                {
                    logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogCheckStaleOrdersExecuteError, 1000 * 60 * 15,
                        "Ошибка в CheckStaleOrders() - закрытие ордеров: " + ex);
                }
            }
        }

        public static List<int> GetStaleOrders()
        {
            using (var ctx = DatabaseContext.Instance.Make())
            {
                // позиции, мастер - ордера по которым закрыты
                return ctx.POSITION.Where(p => p.MasterOrder.HasValue).Where(p => ctx.POSITION.All(pos => pos.ID != p.MasterOrder)).Select(p => p.ID).ToList();
            }
        }
    
        private void ProcessTradeSignalsPending()
        {
            var acts = actionsPending.ExtractAll(2000);
            if (acts == null || acts.Count == 0) return;
            foreach (var act in acts)
            {
                Dealer.ProcessSignal(act);
            }
            totalActionsProcessed += acts.Count;
            logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info, LogOrdersAreProcessed,
                                                  1000*60*90, "Обработано " + acts.Count +
                                                  " сигналов, всего обработано " + totalActionsProcessed);
        }
    }
}

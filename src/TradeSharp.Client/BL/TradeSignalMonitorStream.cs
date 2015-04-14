using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace TradeSharp.Client.BL
{
    /// <summary>
    /// в потоке опрашивается состояние сделок по торговым сигналам, на которые подписан акаунт
    /// делается это с тем, чтобы сделки, закрытие которых было пропущено
    /// (нет сети, закрыт терминал, сбой в работе) таки закрылись
    /// </summary>
    class TradeSignalMonitorStream
    {
        private static readonly int wcfTimeout = AppConfig.GetIntParam("WCF.Timeout", 1000);
        private static TradeSignalMonitorStream instance;

        public static TradeSignalMonitorStream Instance
        {
            get { return instance ?? (instance = new TradeSignalMonitorStream()); }
        }

        private TradeSignalMonitorStream()
        {
            
        }

        private volatile bool isStopping;
        private Thread thread;

        private volatile bool dealsRequestInProcess;

        /// <summary>
        /// сделки, которые поток пытался / пытается закрыть
        /// дабы не рассылать сообщение о попытке закрытия сделок каждые N секунд
        /// </summary>
        private readonly Dictionary<int, int> spareDealIds = new Dictionary<int, int>();

        public Action<DateTime, string> showMessageInUI;
        
        public void Start()
        {
            thread = new Thread(ThreadRoutine);
            thread.Start();
        }

        public void Stop()
        {
            isStopping = true;
            if (thread == null) return;            
            if (!thread.Join(wcfTimeout))
            {
                // остановить соединение WCF
                Logger.Info("TradeSignalMonitorStream - таймаут, принудительное завершение");
                MainForm.serverProxyTrade.AbortWCFCall();
                
                // завершить поток
                Logger.Info("TradeSignalMonitorStream - ожидание завершения");
                thread.Join();
            }
        }

        private void ThreadRoutine()
        {
            const int sleepInterval = 200;
            const int pollInterval = 8 * 1000;
            const int numInters = pollInterval/sleepInterval;
            var curInter = numInters;

            while (!isStopping)
            {
                Thread.Sleep(sleepInterval);
                curInter--;
                if (curInter > 0) continue;
                curInter = numInters;
                CheckOrders();
            }
        }

        /// <summary>
        /// таки запросить ордера
        /// </summary>
        private void CheckOrders()
        {
            Dictionary<int, List<MarketOrder>> orderDic;
            // получить ордера
            try
            {
                if (!AccountStatus.Instance.isAuthorized) return;
                var acId = AccountStatus.Instance.accountID;
                dealsRequestInProcess = true;
                try
                {
                    orderDic = TradeSharpAccount.Instance.proxy.GetSignallersOpenTrades(acId);
                }
                finally
                {
                    dealsRequestInProcess = false;
                }                
                if (orderDic == null || orderDic.Count == 0) return;
            }
            catch (Exception ex)
            {
                Logger.Error("TradeSignalMonitorStream.CheckOrders() - ошибка получения ордеров", ex);
                return;
            }

            if (isStopping) return;
            // сравнить ордера с открытыми ордерами по счету - найти "лишние" открытые
            var dealsToClose = new List<MarketOrder>();
            try
            {
                var orders = MarketOrdersStorage.Instance.MarketOrders;
                if (orders == null || orders.Count == 0) return;
                // найти в списке "лишние" ордера
                foreach (var order in orders)
                {
                    int serviceId, parentDealId;
                    if (!MarketOrder.GetTradeSignalFromDeal(order, out serviceId, out parentDealId)) continue;
                    
                    // найти для данного ордера "родительский" ордер
                    List<MarketOrder> parentOrders;
                    orderDic.TryGetValue(serviceId, out parentOrders);
                    if (parentOrders == null) continue; // возможно, на данныю категорию более не подписаны
                    
                    if (!parentOrders.Any(o => o.ID == parentDealId))
                    { 
                        // у сигнальщика не найдена сделка - попытка закрыть ее
                        if (!spareDealIds.ContainsKey(order.ID))
                        {
                            // сообщить о левой сделке
                            showMessageInUI(DateTime.Now,
                                string.Format("Сделка {0} ({1} {2} {3}) закрыта у управляющего (сигнал #{4}) - производится закрытие",
                                              order.ID, order.Side > 0 ? "BUY" : "SELL", order.Volume, order.Symbol,
                                              serviceId));
                            spareDealIds.Add(order.ID, order.ID);
                        }
                        dealsToClose.Add(order);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("TradeSignalMonitorStream.CheckOrders() - ошибка сравнения ордеров", ex);
            }

            try
            {
                foreach (var deal in dealsToClose)
                    TryCloseDeal(deal);
            }
            catch (Exception ex)
            {
                Logger.Error("TradeSignalMonitorStream.CheckOrders() - ошибка закрытия сделки", ex);
            }
        }

        private static void TryCloseDeal(MarketOrder deal)
        {
            try
            {
                var res = MainForm.serverProxyTrade.proxy.SendCloseRequest(
                    CurrentProtectedContext.Instance.MakeProtectedContext(),
                    deal.AccountID, deal.ID, PositionExitReason.ClosedBySignal);
                if (res != RequestStatus.OK)
                {
                    Logger.ErrorFormat("TradeSignalMonitorStream.CheckOrders() - невозможно закрыть сделку #{0} ({1} {2} {3}) - {4}",
                    deal.ID, deal.Side > 0 ? "BUY" : "SELL", deal.Volume, deal.Symbol, res);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("TradeSignalMonitorStream.CheckOrders() - ошибка закрытия сделки #{0} ({1} {2} {3}) - {4}", 
                    deal.ID, deal.Side > 0 ? "BUY" : "SELL", deal.Volume, deal.Symbol, ex);
            }
        }
    }
}

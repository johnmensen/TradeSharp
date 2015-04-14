using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Server.BL;
using TradeSharp.Util;

namespace TradeSharp.Server.Contract
{
    /// <summary>
    /// перебирает всех клиентов и доставляет нужному сообщение
    /// потокобезопасный
    /// </summary>
    class ServiceManagerClientManagerProxy
    {
        private static ServiceManagerClientManagerProxy instance;
        public static ServiceManagerClientManagerProxy Instance
        {
            get { return instance ?? (instance = new ServiceManagerClientManagerProxy()); }
        }

        private readonly List<TradeSharpClientResponse> responses = new List<TradeSharpClientResponse>();
        private readonly ReaderWriterLock responseLocker = new ReaderWriterLock();
        private const int LockTimeout = 1500;

        private Thread threadDistribute;
        private bool stopFlag;
        private const int DistributeIntervalMils = 100;

        private ServiceManagerClientManagerProxy()
        {
        }

        public void StartDistribution()
        {
            if (threadDistribute != null) return;
            stopFlag = false;
            threadDistribute = new Thread(DeliveryLoop);
            threadDistribute.Start();
        }

        public void StopDistribution()
        {
            stopFlag = true;
            threadDistribute.Join();
        }

        public void NewOrderResponse(MarketOrder order, RequestStatus status, string detailMessage)
        {
            PutResponse(status, detailMessage, order, string.Empty, order.AccountID, TradeSharpClientResponseType.NewOrder);
        }

        public void NewPendingOrderResponse(PendingOrder order, RequestStatus status, string detailMessage)
        {
            PutResponse(status, detailMessage, order, string.Empty, order.AccountID, TradeSharpClientResponseType.NewPendingOrder);
        }

        public void CloseOrderResponse(MarketOrder order, RequestStatus status, string detailMessage)
        {
            PutResponse(status, detailMessage, order, string.Empty, order.AccountID, TradeSharpClientResponseType.CloseOrder);
        }

        public void DeletePendingOrderResponse(PendingOrder order, RequestStatus status, string detailMessage)
        {
            PutResponse(status, detailMessage, order, string.Empty, order.AccountID, TradeSharpClientResponseType.CancelPendingOrder);
        }

        public void ModifyPendingOrderResponse(PendingOrder order, RequestStatus status, string detailMessage)
        {
            PutResponse(status, detailMessage, order, string.Empty, order.AccountID, TradeSharpClientResponseType.EditPendingOrder);
        }

        public void ModifyMarketOrderResponse(MarketOrder order, RequestStatus status, string detailMessage)
        {
            PutResponse(status, detailMessage, order, string.Empty, order.AccountID, TradeSharpClientResponseType.EditOrder);
        }

        public void ProcessTradeSignalAction(TradeSignalAction action)
        {
            // передать на исполнение службе обработки сигналов
            TradeSignalExecutor.Instance.proxy.ProcessTradeSignals(new List<TradeSignalAction> { action });
        }

        private void DeliveryLoop()
        {
            const int sleepInterval = 50;
            var nSteps = (int)Math.Ceiling(DistributeIntervalMils / (double)sleepInterval);

            var step = nSteps;
            while (!stopFlag)
            {
                step--;
                if (step == 0)
                {
                    step = nSteps;
                    // раздача сообщений
                    DistributeRoutine();
                }
                Thread.Sleep(sleepInterval);
            }
        }

        private void DistributeRoutine()
        {
            var respCpy = ReadResponsesSafely();
            if (respCpy.Count == 0) return;
            var sessions = UserSessionStorage.Instance.GetSessions();
            var staleSessions = new List<long>();

            foreach (var resp in respCpy)
            {
                try
                {
                    // адресная доставка
                    if (resp.broadcast) continue; // !! потом доделаю

                    var targetLogin = resp.targetUserLogin;
                    var targetAccount = resp.accountId;

                    var sessionAcc = targetAccount.HasValue
                                         ? sessions.Where(s => s.accountId == targetAccount.Value)
                                         : sessions.Where(s => s.login == targetLogin);
                    if (!targetAccount.HasValue)
                        Logger.InfoFormat("Доставка сообщения в сессию - поиск по логину: login={0}", targetLogin);

                    // попытка доставить сообщение
                    var deliveredCount = 0;
                    foreach (var session in sessionAcc)
                    {
                        if (targetAccount.HasValue)
                            Logger.InfoFormat("Доставка сообщения в сессию: account={0}, ip={1}, terminalId={2}",
                                              session.accountId, session.ip, session.terminalId);
                        else
                            Logger.InfoFormat("Доставка сообщения в сессию: login={0}, ip={1}, terminalId={2}",
                                              session.login, session.ip, session.terminalId);
                        // попытка доставить сообщение
                        Exception deliveryEx;
                        if (DeliverResponse(session.callback, resp, out deliveryEx))
                        {
                            deliveredCount++;
                            continue;
                        }
                        // удалить отвалившегося клиента
                        staleSessions.Add(session.terminalId);
                        Logger.InfoFormat("Отвалился клиент ip={0} {1} {2} account={3}",
                                          session.ip, session.login, session.lastRequestClientTime, session.accountId);
                    }
                    if (deliveredCount > 0)
                        Logger.InfoFormat("Доставлено {0} сигналов получателям", deliveredCount);
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка в DistributeRoutine", ex);
                }
            }
            if (staleSessions.Count > 0)
            {
                staleSessions = staleSessions.Distinct().ToList();
                try
                {
                    UserSessionStorage.Instance.ExcludeStaleSessions(staleSessions);
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка в DistributeRoutine (удаление сессий)", ex);
                }
            }
        }

        private static bool DeliverResponse(ITradeSharpServerCallback callback, TradeSharpClientResponse resp, out Exception exception)
        {
            exception = null;
            try
            {
                switch (resp.responseType)
                {
                    case TradeSharpClientResponseType.NewOrder:
                        callback.NewOrderResponse((MarketOrder)resp.ticket, resp.status, resp.detailMessage);
                        break;

                    case TradeSharpClientResponseType.NewPendingOrder:
                        callback.NewPendingOrderResponse((PendingOrder)resp.ticket, resp.status, resp.detailMessage);
                        break;

                    case TradeSharpClientResponseType.CloseOrder:
                        callback.NewCloseOrderResponse((MarketOrder)resp.ticket, resp.status, resp.detailMessage);
                        break;
                    case TradeSharpClientResponseType.EditOrder:
                        callback.EditOrderResponse((MarketOrder)resp.ticket, resp.status, resp.detailMessage);
                        break;
                    case TradeSharpClientResponseType.EditPendingOrder:
                        callback.EditPendingOrderResponse((PendingOrder)resp.ticket, resp.status, resp.detailMessage);
                        break;
                    //case TradeSharpClientResponseType.TradeSignalAction :
                    //    callback.ProcessTradeSignalAction((TradeSignalAction)resp.ticket);
                    //    break;
                    default: break;
                }
            }
            catch (TimeoutException ex)
            {
                Logger.Error("ServiceManagerClientManagerProxy.DeliverResponse TimeoutException", ex);
                exception = ex;
                return false;
            }
            catch (CommunicationObjectAbortedException ex)
            {
                Logger.Error("ServiceManagerClientManagerProxy.DeliverResponse CommunicationObjectAbortedException", ex);
                exception = ex;
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("ServiceManagerClientManagerProxy.DeliverResponse exception: " + ex.Message);
                exception = ex;
                return false;
            }
            return true;
        }

        private void PutResponse(RequestStatus status, string detailMessage, object ticket,
            string login, int? accountId, TradeSharpClientResponseType respType)
        {
            responseLocker.AcquireWriterLock(LockTimeout);
            try
            {
                responses.Add(new TradeSharpClientResponse
                {
                    status = status,
                    detailMessage = detailMessage,
                    ticket = ticket,
                    targetUserLogin = login,
                    responseType = respType,
                    accountId = accountId
                });
            }
            finally
            {
                responseLocker.ReleaseWriterLock();
            }
        }

        private void PutResponse(TradeSharpClientResponse resp)
        {
            responseLocker.AcquireWriterLock(LockTimeout);
            try
            {
                responses.Add(resp);
            }
            finally
            {
                responseLocker.ReleaseWriterLock();
            }
        }

        private List<TradeSharpClientResponse> ReadResponsesSafely()
        {
            var respCopy = new List<TradeSharpClientResponse>();
            responseLocker.AcquireReaderLock(LockTimeout);
            try
            {
                if (responses.Count == 0) return respCopy;
                respCopy.AddRange(responses);
                responseLocker.UpgradeToWriterLock(LockTimeout);
                responses.Clear();
                return respCopy;
            }
            finally
            {
                responseLocker.ReleaseLock();
            }
        }
    }

    class TradeSharpClientResponse
    {
        public TradeSharpClientResponseType responseType;
        public RequestStatus status;
        public string detailMessage;
        public object ticket;
        public bool broadcast;
        public string targetUserLogin;
        public int? accountId;
    }

    enum TradeSharpClientResponseType
    {
        NewOrder, NewPendingOrder, CloseOrder, CancelOrder, CancelPendingOrder, EditOrder, EditPendingOrder, TradeSignalAction
    }
}

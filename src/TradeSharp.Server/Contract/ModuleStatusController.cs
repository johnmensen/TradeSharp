using System;
using System.ServiceModel;
using System.Text;
using System.Threading;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Server.Contract
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ModuleStatusController : IModuleStatus
    {
        private static ModuleStatusController instance;

        public static ModuleStatusController Instance
        {
            get { return instance ?? (instance = new ModuleStatusController()); }
        }

        #region Настройки
        private readonly int minutesToReportErroroNoQuote = AppConfig.GetIntParam("Report.MinutesOfQuoteGap", 10);
        #endregion
        #region Переменные состояния
        private ServiceStateInfo status = new ServiceStateInfo(ServiceProcessState.OK);
        private readonly ReaderWriterLock statusLocker = new ReaderWriterLock();
        public ServiceStateInfo Status
        {
            get
            {
                try
                {
                    statusLocker.AcquireReaderLock(200);
                }
                catch
                {
                    return null;
                }
                try
                {
                    return status;
                }
                finally
                {
                    statusLocker.ReleaseReaderLock();
                }
            }
            set
            {
                try
                {
                    statusLocker.AcquireWriterLock(200);
                }
                catch
                {
                    return;
                }
                try
                {
                    status = value;
                }
                finally
                {
                    statusLocker.ReleaseWriterLock();
                }
            }
        }
        public ThreadSafeDateTime lastLoginTime = new ThreadSafeDateTime();
        public ThreadSafeDateTime lastQuoteTime = new ThreadSafeDateTime();
        public ThreadSafeDateTime lastProviderMessageTime = new ThreadSafeDateTime();
        private string lastError;
        private readonly ReaderWriterLock lastErrorLocker = new ReaderWriterLock();
        public string LastError
        {
            get
            {
                try
                {
                    lastErrorLocker.AcquireReaderLock(200);
                }
                catch
                {
                    return string.Empty;
                }
                try
                {
                    return lastError;
                }
                finally
                {
                    lastErrorLocker.ReleaseReaderLock();
                }                
            }
            set
            {
                try
                {
                    lastErrorLocker.AcquireWriterLock(200);
                }
                catch
                {
                    return;
                }
                try
                {
                    lastError = value;
                }
                finally
                {
                    lastErrorLocker.ReleaseWriterLock();
                }
            }
        }
        #endregion

        public ServiceStateInfo GetModuleStatus()
        {
            UpdateStatusAuto();
            return Status;
        }

        public string GetModuleExtendedStatusString()
        {
            try
            {
                // вернуть данные о потоке котировок, подключенных клиентах
                // текущих предупреждениях, стоп-аутах
                var sb = new StringBuilder();
                var errStr = LastError;
                sb.AppendLine(!string.IsNullOrEmpty(errStr) ? string.Format("Ошибка: {0}", errStr) : 
                    status.State.ToString());

                var timeLog = lastLoginTime.Value;
                if (timeLog.HasValue)
                    sb.AppendLine(string.Format("Послед. логин: {0:ddd HH:mm:ss}", timeLog.Value));

                var timeQuote = lastQuoteTime.Value;
                if (timeQuote.HasValue)
                    sb.AppendLine(string.Format("Послед. котировка: {0:ddd HH:mm:ss}", timeQuote.Value));
                var dealersError = ManagerTrade.Instance.GetDealersErrorString();
                if (!string.IsNullOrEmpty(dealersError))
                {
                    sb.AppendLine("Ошибки дилеров:");
                    sb.AppendLine(dealersError);
                }

                var timeMsg = lastProviderMessageTime.Value;
                if (timeMsg.HasValue)
                    sb.AppendLine(string.Format("Послед. сообщение провайдера: {0:ddd HH:mm:ss}", timeMsg.Value));

                return sb.ToString();
            }
            catch (Exception ex)
            {
                Status.SetState(ServiceProcessState.HasErrors);
                return ex.ToString();
            }
        }

        public void ResetStatus()
        {
            Logger.Info("ModluleStatusController.ResetStatus()");
            Status.SetState(ServiceProcessState.OK);
            ManagerTrade.Instance.ClearDealersError();
            LastError = string.Empty;
        }
    
        private void UpdateStatusAuto()
        {
            var minsSinceQuote = lastQuoteTime.Value.HasValue
                                     ? (DateTime.Now - lastQuoteTime.Value.Value).TotalMinutes
                                     : 0; // мб сервис только что запущен
            var errorNoQuote = minsSinceQuote > minutesToReportErroroNoQuote;
            if (errorNoQuote)
                Status.AddError(ServiceProcessState.HasErrors, "Нет котировки", DateTime.Now, "QUOTE");
            else
                Status.RemoveError(ServiceProcessState.HasErrors, "QUOTE");

            var errorDealer = ManagerTrade.Instance.GetDealersErrorString();
            if (!string.IsNullOrEmpty(errorDealer))
                Status.AddError(ServiceProcessState.HasCriticalErrors, errorDealer, DateTime.Now, "DEALER");
        }
    }
}

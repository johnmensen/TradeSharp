using System.ServiceModel;
using System.Threading;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Chat.Service.BL
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
            return Status;
        }

        public string GetModuleExtendedStatusString()
        {
            return "OK";
        }

        public void ResetStatus()
        {
            Logger.Info("ModluleStatusController.ResetStatus()");
            Status.SetState(ServiceProcessState.OK);
            LastError = string.Empty;
        }        
    }
}

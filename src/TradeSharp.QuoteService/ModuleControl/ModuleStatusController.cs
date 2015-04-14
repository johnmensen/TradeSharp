using System;
using System.ServiceModel;
using System.Text;
using System.Threading;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.QuoteService.Distribution;
using TradeSharp.Util;

namespace TradeSharp.QuoteService.ModuleControl
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ModuleStatusController : IModuleStatus
    {
        public const int MaxMillsBetweenQuotes = 1000 * 10;

        private readonly FloodSafeLogger loggerNoFlood = new FloodSafeLogger(1000);
        private const int LogMagicReadTimeout = 1;
        private const int LogMagicWriteTimeout = 2;
        
        private static ModuleStatusController instance;

        public static ModuleStatusController Instance
        {
            get { return instance ?? (instance = new ModuleStatusController()); }
        }

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
                    loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Debug, LogMagicReadTimeout,
                        1000 * 60 * 15, "ModuleStatusController: таймаут сохранения статуса");
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
                    loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Debug, LogMagicWriteTimeout,
                        1000 * 60 * 15, "ModuleStatusController: таймаут чтения статуса");
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

        public ServiceStateInfo GetModuleStatus()
        {
            return Status;
        }

        public string GetModuleExtendedStatusString()
        {
            try
            {
                // вернуть данные о потоке котировок, подключенных клиентах
                var clientsInfo = BaseNewsDistributor.Instance.GetStatusString();
                /*var sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine("Distribution events:");
                bool isTimeout;
                var events = BaseNewsDistributor.Instance.distributor.distributorEvents.GetAll(200, out isTimeout);
                if (events != null && events.Count > 0)
                    foreach (var evtStr in events) sb.AppendLine(evtStr);
                sb.AppendLine("Delivery events:");
                events = BaseNewsDistributor.Instance.distributor.deliveryEvents.GetAll(200, out isTimeout);
                if (events != null && events.Count > 0)
                    foreach (var evtStr in events) sb.AppendLine(evtStr);*/
                return clientsInfo;// +sb;
            }
            catch (Exception ex)
            {
                Status.SetState(ServiceProcessState.HasErrors);
                return ex.ToString();
            }            
        }

        public void ResetStatus()
        {
            Status.SetState(ServiceProcessState.OK);
        }
    }
}

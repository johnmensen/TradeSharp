using System;
using System.Threading;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Contract.Util.BL
{
    public abstract class DefaultModuleStatusController : IModuleStatus
    {
        protected virtual string ModuleName
        {
            get { return "module"; }
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
                catch (Exception ex)
                {
                    Logger.Error(ModuleName + " Status get : ", ex);
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
                catch (Exception ex)
                {
                    Logger.Error(ModuleName + " Status set : ", ex);
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

                return String.Empty;
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

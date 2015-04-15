using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.Reports.Lib.Contract
{
    public class IndexGrabberProxy : IIndexGrabber, IDisposable
    {
        private ChannelFactory<IIndexGrabber> factory;
        private IIndexGrabber channel;
        private readonly string endpointName;
        private readonly bool useLock;
        private readonly object locker;
        private readonly int lockTimeout;

        public IndexGrabberProxy(string endpointName)
        {
            this.endpointName = endpointName;
            RenewFactory();
        }

        public IndexGrabberProxy(string endpointName, bool useLock, int timeout)
        {
            this.endpointName = endpointName;
            RenewFactory();
            this.useLock = useLock;
            locker = new object();
            lockTimeout = timeout;
        }

        private void RenewFactory()
        {
            try
            {
                if (factory != null) factory.Abort();
                factory = new ChannelFactory<IIndexGrabber>(endpointName);
                channel = factory.CreateChannel();
            }
            catch (Exception)
            {
                Logger.Error("IndexGrabberProxy: невозможно создать прокси");
                channel = null;
            }
        }

        public void Dispose()
        {
            factory.Close();
        }

        #region IIndexGrabber

        public Dictionary<string, List<Cortege2<DateTime, float>>> GetIndexData()
        {
            if (channel == null) throw new Exception("IndexGrabberProxy: связь не установлена");
            if (useLock)
                if (!Monitor.TryEnter(locker, lockTimeout))
                    throw new TimeoutException(string.Format("ServerStatisticsContractProxy.GetAccountProfit1000: timeout({0})", lockTimeout));
            try
            {
                return channel.GetIndexData();
            }
            catch (CommunicationException)
            {
                RenewFactory();

                return channel.GetIndexData();
            }
            finally
            {
                if (useLock) Monitor.Exit(locker);
            }
        }

        #endregion
    }
}

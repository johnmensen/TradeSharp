using System;
using System.Collections.Generic;
using System.ServiceModel;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Contract.Proxy
{
    public class TradeSignalExecutorProxy : ITradeSignalExecutor
    {
        private static ITradeSignalExecutor fakeChannel;
        private ChannelFactory<ITradeSignalExecutor> factory;
        private ITradeSignalExecutor channel;
        private readonly string endpointName;

        private readonly FloodSafeLogger logNoFlood = new FloodSafeLogger(1000);

        private const int LogMsgProcessTradeSignals = 1;

        private ITradeSignalExecutor Channel
        {
            get
            {
                if (fakeChannel != null)
                    return fakeChannel;
                if (channel == null)
                    RenewFactory();
                return channel;
            }
        }

        public static void InitializeFake(ITradeSignalExecutor chan)
        {
            fakeChannel = chan;
        }

        public TradeSignalExecutorProxy(string endpointName)
        {
            this.endpointName = endpointName;
            RenewFactory();
        }

        private void RenewFactory()
        {
            try
            {
                if (factory != null) factory.Abort();
                factory = new ChannelFactory<ITradeSignalExecutor>(endpointName);
                channel = factory.CreateChannel();
            }
            catch (Exception ex)
            {
                Logger.Error("TradeSignalExecutorProxy: невозможно создать прокси", ex);
                channel = null;
            }
        }

        #region ITradeSignalExecutor
        public void ProcessTradeSignals(List<TradeSignalAction> actions)
        {
            if (Channel == null)
                throw new Exception("TradeSignalExecutorProxy: связь не установлена");
            try
            {
                Channel.ProcessTradeSignals(actions);
            }
            catch (Exception)
            {
                RenewFactory();
                try
                {
                    Channel.ProcessTradeSignals(actions);
                }
                catch (Exception ex)
                {
                    logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, LogMsgProcessTradeSignals, 1000 * 60 * 10,
                        "Ошибка в ProcessTradeSignals({0}): {1}", actions.Count, ex);
                }
            }
        }
        #endregion
    }
}

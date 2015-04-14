using System;
using System.Collections.Generic;
using System.ServiceModel;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Contract.Proxy
{
    public class QuoteStorageProxy : IQuoteStorage
    {
        private static IQuoteStorage fakeChannel;
        private ChannelFactory<IQuoteStorage> factory;
        private IQuoteStorage channel;
        private readonly string endpointName;
        
        private IQuoteStorage Channel
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

        public static void InitializeFake(IQuoteStorage chan)
        {
            fakeChannel = chan;
        }

        public QuoteStorageProxy(string endpointName)
        {
            this.endpointName = endpointName;
            RenewFactory();
        }

        private void RenewFactory()
        {
            try
            {
                if (factory != null) factory.Abort();
                factory = new ChannelFactory<IQuoteStorage>(endpointName);
                channel = factory.CreateChannel();
            }
            catch (Exception ex)
            {
                Logger.Error("QuoteStorageProxy: невозможно создать прокси", ex);
                channel = null;
            }
        }

        #region IQuoteStorage

        public PackedCandleStream GetMinuteCandlesPacked(string symbol, DateTime start, DateTime end)
        {
            if (CommonChannelSettings.workOffline) return null;
            if (Channel == null)
                throw new Exception("QuoteStorageProxy: связь не установлена");
            try
            {
                return Channel.GetMinuteCandlesPacked(symbol, start, end);
            }
            catch (Exception)
            {
                RenewFactory();
                return Channel.GetMinuteCandlesPacked(symbol, start, end);
            }
        }

        public Dictionary<string, DateSpan> GetTickersHistoryStarts()
        {
            if (CommonChannelSettings.workOffline) return null;
            if (Channel == null)
                throw new Exception("QuoteStorageProxy: связь не установлена");
            try
            {
                return Channel.GetTickersHistoryStarts();
            }
            catch (Exception)
            {
                RenewFactory();
                return Channel.GetTickersHistoryStarts();
            }
        }

        public Dictionary<string, QuoteData> GetQuoteData()
        {
            if (CommonChannelSettings.workOffline) return null;
            if (Channel == null)
                throw new Exception("QuoteStorageProxy: связь не установлена");
            try
            {
                return Channel.GetQuoteData();
            }
            catch (Exception)
            {
                RenewFactory();
                return Channel.GetQuoteData();
            }
        }

        public PackedCandleStream GetMinuteCandlesPackedFast(string symbol, List<Cortege2<DateTime, DateTime>> intervals)
        {
            if (CommonChannelSettings.workOffline)
                return null;
            if (Channel == null)
                throw new Exception("QuoteStorageFast: связь не установлена");
            try
            {
                return Channel.GetMinuteCandlesPackedFast(symbol, intervals);
            }
            catch (Exception)
            {
                RenewFactory();
                return Channel.GetMinuteCandlesPackedFast(symbol, intervals);
            }
        }

        #endregion
    }
}

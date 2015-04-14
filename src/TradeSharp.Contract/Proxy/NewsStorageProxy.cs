using System;
using System.Collections.Generic;
using System.ServiceModel;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Contract.Proxy
{
    public class NewsStorageProxy : INewsStorage, IDisposable
    {
        private ChannelFactory<INewsStorage> factory;
        private readonly string endpointName;
        private INewsStorage channel;
        public static INewsStorage fakeChannel;
        private INewsStorage Channel
        {
            get
            {
                return fakeChannel ?? channel;
            }
        }

        public NewsStorageProxy(string endpointName)
        {
            this.endpointName = endpointName;
            RenewFactory();
        }

        private void RenewFactory()
        {
            if (fakeChannel != null) return;
            try
            {
                if (factory != null) factory.Abort();
                factory = new ChannelFactory<INewsStorage>(endpointName);
                channel = factory.CreateChannel();
            }
            catch (Exception)
            {
                Logger.Error("NewsStorageProxy: невозможно создать прокси");
                channel = null;
            }
        }

        public void Dispose()
        {
            if (fakeChannel != null) return;
            factory.Close();
        }

        public List<News> GetNews(int account, DateTime date, int[] newsChannelIds)
        {
            if (Channel == null)
                throw new Exception("NewsStorageProxy: связь не установлена");
            try
            {
                return Channel.GetNews(account, date, newsChannelIds);
            }
            catch
            {
                RenewFactory();
                try
                {
                    return Channel.GetNews(account, date, newsChannelIds);
                }
                catch (Exception ex2)
                {
                    Logger.Error("GetNews() error: ", ex2);
                    return new List<News>();
                }
            }
        }

        public NewsMap GetNewsMap(int accountId)
        {
            if (Channel == null)
                throw new Exception("NewsStorageProxy: связь не установлена");
            try
            {
                return Channel.GetNewsMap(accountId);
            }
            catch
            {
                RenewFactory();
                try
                {
                    return Channel.GetNewsMap(accountId);
                }
                catch (Exception ex2)
                {
                    Logger.Error("GetNewsMap() error: ", ex2);
                    return null;
                }
            }
        }
    }
}

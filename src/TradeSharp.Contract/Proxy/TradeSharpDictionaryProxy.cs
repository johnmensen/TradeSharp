using System;
using System.Collections.Generic;
using System.ServiceModel;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Contract.Proxy
{
    public class TradeSharpDictionaryProxy : ITradeSharpDictionary
    {
        private ChannelFactory<ITradeSharpDictionary> factory;
        private ITradeSharpDictionary channel;
        private readonly string endpointName;

        public TradeSharpDictionaryProxy(string endpointName)
        {
            this.endpointName = endpointName;
            RenewFactory();
        }

        private void RenewFactory()
        {
            try
            {
                if (factory != null) factory.Abort();
                factory = new ChannelFactory<ITradeSharpDictionary>(endpointName);
                channel = factory.CreateChannel();
            }
            catch (Exception ex)
            {
                Logger.Error("TradeSharpDictionaryProxy: невозможно создать прокси", ex);
                channel = null;
            }
        }

        #region ITradeSharpDictionary         
        public List<PlatformUser> GetAllPlatformUsers()
        {
            if (channel == null) throw new Exception("TradeSharpDictionaryProxy: связь не установлена");
            try
            {
                return channel.GetAllPlatformUsers();
            }
            catch (Exception)
            {
                RenewFactory();
                try
                {
                    return channel == null ? null : channel.GetAllPlatformUsers();
                }
                catch (Exception ex)
                {
                    Logger.Error("GetAllPlatformUsers()", ex);
                    return null;
                }
            }
        }

        public List<TradeTicker> GetTickers(out long lotByGroupHash)
        {
            if (channel == null) throw new Exception("TradeSharpDictionaryProxy: связь не установлена");
            try
            {
                return channel.GetTickers(out lotByGroupHash);
            }
            catch (Exception)
            {
                RenewFactory();
                lotByGroupHash = 0;
                try
                {
                    return channel == null ? null : channel.GetTickers(out lotByGroupHash);
                }
                catch (Exception ex)
                {
                    Logger.Error("GetTickers()", ex);
                    return null;
                }
            }
        }

        public LotByGroupDictionary GetLotByGroup()
        {
            if (channel == null) throw new Exception("TradeSharpDictionaryProxy: связь не установлена");
            try
            {
                return channel.GetLotByGroup();
            }
            catch (Exception)
            {
                RenewFactory();
                try
                {
                    return channel == null ? null : channel.GetLotByGroup();
                }
                catch (Exception ex)
                {
                    Logger.Error("GetLotByGroup()", ex);
                    return null;
                }
            }
        }

        public List<AccountGroup> GetAccountGroupsWithSessionInfo()
        {
            if (channel == null) throw new Exception("TradeSharpDictionaryProxy: связь не установлена");
            try
            {
                return channel.GetAccountGroupsWithSessionInfo();
            }
            catch (Exception)
            {
                RenewFactory();
                try
                {
                    return channel == null ? null : channel.GetAccountGroupsWithSessionInfo();
                }
                catch (Exception ex)
                {
                    Logger.Error("GetLotByGroup()", ex);
                    return null;
                }
            }
        }

        public ProviderSession[] GetQueueAndSession(string dealerCode)
        {
            if (channel == null) throw new Exception("TradeSharpDictionaryProxy: связь не установлена");
            try
            {
                return channel.GetQueueAndSession(dealerCode);
            }
            catch (Exception)
            {
                RenewFactory();
                try
                {
                    return channel == null ? null : channel.GetQueueAndSession(dealerCode);
                }
                catch (Exception ex)
                {
                    Logger.Error("GetQueueAndSession()", ex);
                    return null;
                }
            }
        }

        public Dictionary<string, object> GetMetadataByCategory(string catName)
        {
            if (channel == null) throw new Exception("TradeSharpDictionaryProxy: связь не установлена");
            try
            {
                return channel.GetMetadataByCategory(catName);
            }
            catch (Exception)
            {
                RenewFactory();
                try
                {
                    return channel == null ? null : channel.GetMetadataByCategory(catName);
                }
                catch (Exception ex)
                {
                    Logger.Error("GetMetadataByCategory()", ex);
                    return null;
                }
            }
        }

        public Dictionary<string, Dictionary<string, object>> GetAllMetadata()
        {
            if (channel == null) throw new Exception("TradeSharpDictionaryProxy: связь не установлена");
            try
            {
                return channel.GetAllMetadata();
            }
            catch (Exception)
            {
                RenewFactory();
                try
                {
                    return channel == null ? null : channel.GetAllMetadata();
                }
                catch (Exception ex)
                {
                    Logger.Error("GetAllMetadata()", ex);
                    return null;
                }
            }
        }

        public void DeleteMetadataItem(string catName, string paramName)
        {
            if (channel == null) throw new Exception("TradeSharpDictionaryProxy: связь не установлена");
            try
            {
                channel.DeleteMetadataItem(catName, paramName);
            }
            catch (Exception)
            {
                RenewFactory();
                try
                {
                    channel.DeleteMetadataItem(catName, paramName);
                }
                catch (Exception ex)
                {
                    Logger.Error("DeleteMetadataItem()", ex);
                    return;
                }
            }
        }

        public void AddOrReplaceMetadataItem(string catName, string paramName, object ptr)
        {
            if (channel == null) throw new Exception("TradeSharpDictionaryProxy: связь не установлена");
            try
            {
                channel.AddOrReplaceMetadataItem(catName, paramName, ptr);
            }
            catch (Exception)
            {
                RenewFactory();
                try
                {
                    channel.AddOrReplaceMetadataItem(catName, paramName, ptr);
                }
                catch (Exception ex)
                {
                    Logger.Error("AddOrReplaceMetadataItem()", ex);
                    return;
                }
            }
        }

        #endregion
    }
}
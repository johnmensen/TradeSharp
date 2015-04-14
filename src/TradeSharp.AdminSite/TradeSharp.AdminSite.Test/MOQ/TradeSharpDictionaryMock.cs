using System;
using System.Collections.Generic;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;

namespace TradeSharp.AdminSite.Test.MOQ
{
    public class TradeSharpDictionaryMock : ITradeSharpDictionary
    {
        #region TradeSharpDictionaryMock
        public List<PlatformUser> GetAllPlatformUsers()
        {
            throw new NotImplementedException();
        }

        public List<TradeTicker> GetTickers(out long lotByGroupHash)
        {
            lotByGroupHash = 0;
            var ticks = new List<TradeTicker>();
            using (var ctx = DatabaseContext.Instance.Make())
            {
                foreach (var tradeTicker in ctx.SPOT)
                    ticks.Add(LinqToEntity.DecorateTicker(tradeTicker));
            }
            return ticks;
        }

        public LotByGroupDictionary GetLotByGroup()
        {
            throw new NotImplementedException();
        }

        public List<AccountGroup> GetAccountGroupsWithSessionInfo()
        {
            throw new NotImplementedException();
        }

        public ProviderSession[] GetQueueAndSession(string dealerCode)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> GetMetadataByCategory(string catName)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, Dictionary<string, object>> GetAllMetadata()
        {
            throw new NotImplementedException();
        }

        public void DeleteMetadataItem(string catName, string paramName)
        {
            throw new NotImplementedException();
        }

        public void AddOrReplaceMetadataItem(string catName, string paramName, object ptr)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.Server.Repository
{
    class BrokerRepository : IBrokerRepository
    {
        public string BrokerCurrency { get; private set; }

        public BrokerRepository()
        {
            // настройки брокера
            var brokerSettings = GetMetadataByCategory("BROKER");
            object brokerCurx;
            if (brokerSettings.TryGetValue("Currency", out brokerCurx))
                BrokerCurrency = brokerCurx.ToString();
            if (string.IsNullOrEmpty(BrokerCurrency)) BrokerCurrency = "USD";
        }

        public Dictionary<string, object> GetMetadataByCategory(string catName)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var records = from rec in ctx.ENGINE_METADATA
                                  where rec.Category == catName
                                  select rec;
                    var dic = new Dictionary<string, object>();
                    // ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var rec in records)
                        dic.Add(rec.Name, Converter.GetObjectFromString(rec.ParamType, rec.Value));
                    // ReSharper restore LoopCanBeConvertedToQuery
                    return dic;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("EngineMetadata - GetMetadataByCategory() error", ex);
                return new Dictionary<string, object>();
            }
        }

        public List<AccountGroup> GetAccountGroupsWithSessionInfo()
        {
            var sessions = new List<AccountGroup>();
            using (var ctx = DatabaseContext.Instance.Make())
            {
                try
                {
                    var query = ctx.ACCOUNT_GROUP.Join(
                    ctx.DEALER_GROUP,
                    gr => gr.Code,
                    dg => dg.AccountGroup,
                    (Func<ACCOUNT_GROUP, DEALER_GROUP, AccountGroup>)
                    ((grp, dealGrp) => new AccountGroup
                    {
                        BrokerLeverage = (float)grp.BrokerLeverage,
                        Code = grp.Code,
                        Dealer = new DealerDescription
                        {
                            Code = dealGrp.Dealer
                        },
                        DefaultVirtualDepo = grp.DefaultVirtualDepo ?? 0,
                        MarginCallPercentLevel = (float)grp.MarginCallPercentLevel,
                        IsReal = grp.IsReal,
                        MessageQueue = dealGrp.MessageQueue,
                        Name = grp.Name,
                        SessionName = dealGrp.SessionName,
                        HedgingAccount = dealGrp.HedgingAccount,
                        StopoutPercentLevel = (float)grp.StopoutPercentLevel,
                        Markup = (AccountGroup.MarkupType)grp.MarkupType,
                        DefaultMarkupPoints = (float)grp.DefaultMarkupPoints
                    }
                    ));

                    return query.ToList();
                }
                catch (Exception ex)
                {
                    Logger.Error("DictionaryManager error - GetAccountGroupsWithSessionInfo()", ex);
                }
            }
            return sessions;
        }
    }
}

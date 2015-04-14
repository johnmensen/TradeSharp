using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Server.BL;
using TradeSharp.Server.Repository;
using TradeSharp.Util;

namespace TradeSharp.Server.Contract
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, 
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class DictionaryManager : ITradeSharpDictionary
    {
        #region Singletone

        private static readonly Lazy<DictionaryManager> instance =
            new Lazy<DictionaryManager>(() => new DictionaryManager());

        public static DictionaryManager Instance
        {
            get { return instance.Value; }
        }

        #endregion

        private readonly IBrokerRepository brokerRepository;

        // ReSharper disable InconsistentNaming
        private static readonly int updateIntervalMils = AppConfig.GetIntParam("Dictionary.UpdateIntervalMils", 1000 * 15);

        private static readonly int lockIntervalMils = AppConfig.GetIntParam("Dictionary.LockIntervalMils", 1000);
        // ReSharper restore InconsistentNaming

        #region Валютные пары и прочие авто-обновляемые списки
        private readonly ThreadSafeUpdatingList<GroupMarkup> groupMarkupList;

        private readonly ThreadSafeUpdatingList<PlatformUser> userList;
        #endregion
        
        private DictionaryManager()
        {
            groupMarkupList = new ThreadSafeUpdatingList<GroupMarkup>(lockIntervalMils, updateIntervalMils, GroupMarkupUpdateRoutine);
            userList = new ThreadSafeUpdatingList<PlatformUser>(lockIntervalMils, updateIntervalMils, UserUpdateRoutine);
            brokerRepository = new BrokerRepository();
        }

        private static List<GroupMarkup> GroupMarkupUpdateRoutine()
        {
            var dicGroup = new Dictionary<string, GroupMarkup>();
            using (var ctx = DatabaseContext.Instance.Make())
            {
                try
                {
                    foreach (var group in ctx.ACCOUNT_GROUP)
                    {
                        dicGroup.Add(group.Code, new GroupMarkup(group.Code)
                            {
                                MarkupType = (AccountGroup.MarkupType) group.MarkupType,
                                DefaultSpread = (float)group.DefaultMarkupPoints,
                                spreadByTicker = new Dictionary<string, float>()
                            });
                    }

                    foreach (var markup in ctx.MARKUP_BY_GROUP)
                    {
                        GroupMarkup group;
                        if (!dicGroup.TryGetValue(markup.Group, out group))
                            continue;
                        
                        if (!group.spreadByTicker.ContainsKey(markup.Spot))
                            group.spreadByTicker.Add(markup.Spot, (float)markup.MarkupAbs);
                    }

                    return dicGroup.Values.ToList();
                }
                catch (Exception ex)
                {
                    Logger.Error("DictionaryManager - ошибка в GroupMarkupUpdateRoutine()", ex);
                    return new List<GroupMarkup>();
                }
            }
        }

        private static List<PlatformUser> UserUpdateRoutine()
        {
            var users = new List<PlatformUser>();
            using (var ctx = DatabaseContext.Instance.Make())
            {
                try
                {
                    // ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var usr in ctx.PLATFORM_USER)
                    // ReSharper restore LoopCanBeConvertedToQuery
                    {
                        users.Add(LinqToEntity.DecoratePlatformUser(usr));
                    }                    
                }
                catch (Exception ex)
                {
                    Logger.Error("DictionaryManager - ошибка в UserUpdateRoutine()", ex);                    
                }
            }
            return users;
        }

        #region ITradeSharpDictionary

        public List<PlatformUser> GetAllPlatformUsers()
        {
            return userList.GetItems().Select(u => new PlatformUser
                {
                    Login = u.Login,
                    Name = u.Name,
                    Patronym = u.Patronym,
                    Description = u.Description,
                    Surname = u.Surname,
                    ID = u.ID,
                    Title = u.Title,
                    RegistrationDate = u.RegistrationDate
                }).ToList();
        }

        public List<AccountGroup> GetAccountGroupsWithSessionInfo()
        {
            return brokerRepository.GetAccountGroupsWithSessionInfo();
        }

        public List<TradeTicker> GetTickers(out long lotByGroupHash)
        {
            return TradingContractDictionary.Instance.GetTickers(out lotByGroupHash);
        }

        public LotByGroupDictionary GetLotByGroup()
        {
            return TradingContractDictionary.Instance.GetLotByGroup();
        }

        public ProviderSession[] GetQueueAndSession(string dealerCode)
        {
            var sessions = new List<ProviderSession>();
            using (var ctx = DatabaseContext.Instance.Make())
            {
                foreach (var dealGroup in ctx.DEALER_GROUP.Where(dg => dg.Dealer == dealerCode))
                {
                    if (!string.IsNullOrEmpty(dealGroup.MessageQueue) &&
                        string.IsNullOrEmpty(dealGroup.SessionName) == false)
                        sessions.Add(new ProviderSession(dealGroup.MessageQueue,
                            dealGroup.SessionName, dealGroup.HedgingAccount, dealGroup.SenderCompId));
                }
            }
            return sessions.ToArray();
        }

        public Dictionary<string, object> GetMetadataByCategory(string catName)
        {
            return brokerRepository.GetMetadataByCategory(catName);
        }

        public Dictionary<string, Dictionary<string, object>> GetAllMetadata()
        {
            var dic = new Dictionary<string, Dictionary<string, object>>();
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var records = from rec in ctx.ENGINE_METADATA select rec;
                    
                    // ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var rec in records)
                    {
                        Dictionary<string, object> subDic;
                        if (dic.ContainsKey(rec.Category))
                            subDic = dic[rec.Category];
                        else
                        {
                            subDic = new Dictionary<string, object>();
                            dic.Add(rec.Category, subDic);
                        }

                        var val = Converter.GetObjectFromString(rec.ParamType, rec.Value);
                        subDic.Add(rec.Name, val);
                    }
                    // ReSharper restore LoopCanBeConvertedToQuery
                    return dic;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("EngineMetadata - GetAllMetadata() error", ex);
                return dic;
            }
        }

        public void DeleteMetadataItem(string catName, string paramName)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var record = ctx.ENGINE_METADATA.FirstOrDefault(m => m.Category == catName &&
                                                                         m.Name == paramName);
                    if (record == null) return;
                    ctx.ENGINE_METADATA.Remove(record);
                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("EngineMetadata - DeleteMetadataItem() error", ex);
                return;
            } 
        }

        public void AddOrReplaceMetadataItem(string catName, string paramName, object ptr)
        {
            try
            {
                var strVal = Converter.GetStringFromObject(ptr);
                var strType = Converter.GetObjectTypeName(ptr);
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var record = ctx.ENGINE_METADATA.FirstOrDefault(m => m.Category == catName &&
                                                                         m.Name == paramName);
                    if (record != null)
                    {
                        record.ParamType = strType;
                        record.Value = strVal;
                    }
                    else
                    {
                        record = new ENGINE_METADATA
                                     {
                                         Category = catName, Name = paramName, 
                                         ParamType = strType, Value = strVal
                                     };
                        ctx.ENGINE_METADATA.Add(record);
                    }
                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("EngineMetadata - AddOrReplaceMetadataItem() error", ex);
                return;
            }
        }

        #endregion

        public List<GroupMarkup> GetMarkupByGroup()
        {
            return groupMarkupList.GetItems();
        }
    }
}
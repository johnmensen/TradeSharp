using System;
using System.Collections.Generic;
using System.Globalization;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Linq;
using Entity;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.SiteBridge.Lib.Contract;
using TradeSharp.SiteBridge.Lib.Distribution;
using TradeSharp.SiteBridge.Lib.Finance;
using TradeSharp.SiteBridge.Lib.Quotes;
using TradeSharp.Util;

namespace TradeSharp.SiteBridge.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public partial class AccountEfficiencyCache : IAccountStatistics
    {
        private static PerformerFormulasAndCriteriasSet defaultSet;

        private readonly IEfficiencyCalculator efficiencyCalculator;

        private readonly IDailyQuoteStorage dailyQuoteStorage; 

        /// <summary>
        /// Key:account id, Value:performer stat
        /// </summary>
        private readonly ThreadSafeStorage<int, AccountEfficiency> dicPerformers = new ThreadSafeStorage<int, AccountEfficiency>();
        
        private volatile bool isStopping;
        
        private Thread threadUpdateCache;
        
        private readonly int updateCacheIntervalSec = 12 * 60 * 60;
        
        private bool cacheUpdated;

        public AccountEfficiencyCache(IEfficiencyCalculator efficiencyCalculator, IDailyQuoteStorage dailyQuoteStorage)
        {
            this.efficiencyCalculator = efficiencyCalculator;
            this.dailyQuoteStorage = dailyQuoteStorage;
            updateCacheIntervalSec = AppConfig.GetIntParam("updateCacheSec", updateCacheIntervalSec);

            // кэш будет создан в UpdateCacheLoop в виду большой длительности создания
            // стартовать прослушку
            cacheUpdated = false;
        }

        public void Start()
        {
            isStopping = false;
            threadUpdateCache = new Thread(UpdateCacheLoop);
            threadUpdateCache.Start();
        }

        public void Stop()
        {
            isStopping = true;
            if (threadUpdateCache != null) threadUpdateCache.Join();
        }

        private void UpdateCacheLoop()
        {
            const int intervalMsecs = 100;
            var nSteps = updateCacheIntervalSec * 1000 / intervalMsecs;
            var step = nSteps;

            // шагаем, чтобы остановиться, когда нас попросят, не спать подолгу
            while (!isStopping)
            {
                Thread.Sleep(intervalMsecs);

                if (!cacheUpdated || step == 0)
                {
                    UpdateCache();
                    step = nSteps;
                }
                else
                    step--;
            }
        }

        /// <summary>
        /// кэш обновляется периодически для заданных акаунтов (accountIDs)
        /// </summary>
        public void UpdateCache()
        {
            var performers = TradeSharpServer.Instance.proxy.GetAllManagers(null);
            // добавить эталонные счета
            var topManagedAccounts = TradeSharpServer.Instance.proxy.GetCompanyTopPortfolioManagedAccounts() ?? new List<PerformerStat>();
            var missedAccounts = topManagedAccounts.Where(a => performers.All(p => p.Account != a.Account));
            performers.AddRange(missedAccounts);
            
            Logger.InfoFormat("UpdateCache({0})", performers.Count);
            
            // обновить кэш котировок
            using (new TimeLogger("Обновление котировок заняло "))
                dailyQuoteStorage.UpdateStorageSync();

            using (new TimeLogger("Расчет статистики занял "))
            foreach (var performer in performers)
            {
                if (isStopping)
                    return;
                AccountEfficiency efficiency;
                try
                {
                    efficiency = new AccountEfficiency(performer);
                    efficiencyCalculator.Calculate(efficiency);
                }
                catch (Exception ex)
                {
                    Logger.Error("UpdateCache - ошибка в AccountEfficiency ctor: ", ex);
                    continue;
                }
                try
                {
                    dicPerformers.UpdateValues(performer.Account, efficiency);
                }
                catch (Exception ex)
                {
                    Logger.Error("UpdateCache - ошибка в расчетах PerformerStat: ", ex);
                }
            }
            cacheUpdated = true;
            Logger.InfoFormat("UpdateCache() - {0} records OK", performers.Count);
        }

        public List<EquityOnTime> GetAccountProfit1000(int accountId)
        {
            // данные не готовы к работе
            if (!cacheUpdated) return new List<EquityOnTime>();

            List<EquityOnTime> lstEqOnTime;
            try
            {
                var pfs = dicPerformers.ReceiveValue(accountId);
                if (pfs == null) 
                    return new List<EquityOnTime>();
                lstEqOnTime = pfs.listProfit1000.ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в GetAccountProfit1000", ex);
                return new List<EquityOnTime>();
            }
            return lstEqOnTime.ToList();
        }

        public AccountEfficiency GetAccountEfficiency(int accountId)
        {
            return GetAccountEfficiencyShort(accountId, true, true);
        }

        public AccountEfficiency GetAccountEfficiencyShort(int accountId, 
            bool needOpenedDeals, bool needClosedDeals)
        {
            // данные не готовы к работе
            if (!cacheUpdated) return null;
            try
            {
                var pfs = dicPerformers.ReceiveValue(accountId);
                return pfs == null ? null : pfs.MakeCopy(needOpenedDeals, needClosedDeals, false);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в GetAccountEfficiency", ex);
                return null;
            }
        }

        public PerformerStat GetPerformerStatBySignalCatId(int signalCatId)
        {
            // данные не готовы к работе
            if (!cacheUpdated) return null;
            try
            {
                return (from pair in dicPerformers.ReceiveAllData() where pair.Value.Statistics.Service == signalCatId select pair.Value.Statistics).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в GetPerformerStatBySignalCatId", ex);
                return null;
            }
        }

        public PerformerStat GetPerformerByAccountId(int accountId)
        {
            if (!cacheUpdated) return null;
            try
            {
                var stat = dicPerformers.ReceiveValue(accountId);
                return stat == null ? null : stat.Statistics;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в GetPerformerByAccountId", ex);
                return null;
            }
        }

        public List<PerformerStat> GetAllPerformers(bool managersOnly)
        {
            // данные не готовы к работе
            if (!cacheUpdated)
            {
                Logger.Info("GetAllPerformers - данные не готовы");
                return new List<PerformerStat>();
            }

            var result = new List<PerformerStat>();
            foreach (var performer in dicPerformers.ReceiveAllData())
            {
                // пропустить счета, привязанные в app.config
                // у них нет привязки к торговым сигналам
                if (managersOnly && performer.Value.Statistics.Service == 0) continue;
                result.Add(performer.Value.Statistics);
            }

            Logger.InfoFormat("GetAllPerformers - {0} записей", result.Count);

            return result;
        }

        public List<PerformerStat> GetAllPerformersWithCriteria(bool managersOnly, string criteria, int count, bool ascending,
            float? marginScore, int serviceTypeMask)
        {
            // данные не готовы к работе
            if (!cacheUpdated)
            {
                Logger.Info("GetAllPerformersWithCriteria - данные не готовы");
                return new List<PerformerStat>();
            }

            ExpressionResolver resv;
            try
            {
                resv = new ExpressionResolver(criteria);
            }
            catch
            {
                resv = null;
            }

            var result = new List<PerformerStat>();
            var startTime = DateTime.Now;
            foreach (var performer in dicPerformers.ReceiveAllData())
            {
                if (managersOnly && performer.Value.Statistics.Service == 0) continue;
                var performerStat = performer.Value.Statistics;
                if (resv != null)
                    performerStat.UserScore = (float) PerformerCriteriaFunction.Calculate(resv, performerStat);
                else
                    performerStat.UserScore = performerStat.Profit;
                // отсеять тех, кто не удовлетворяет условию
                if (marginScore.HasValue)
                    //if (performerStat.UserScore.AreSame(marginScore.Value, 0.0001f)) continue;
                    if ((performerStat.UserScore <= marginScore.Value && !ascending) ||
                        (performerStat.UserScore >= marginScore.Value && ascending))
                        continue;
                if (serviceTypeMask != 0)
                {
                    if ((serviceTypeMask & 1) != 0)
                        if(performerStat.ServiceType != (int) PaidServiceType.Signals)
                            continue;
                    if ((serviceTypeMask & 2) != 0)
                        if (performerStat.ServiceType != (int) PaidServiceType.PAMM)
                            continue;
                }
                result.Add(performerStat);
            }

            // отсортировать и отобрать count результатов
            var total = result.Count;
            result.Sort(ComparePerformerStats);
            if (!ascending)
                result.Reverse();
            if (result.Count > count)
                result = result.GetRange(0, count);

            Logger.InfoFormat("GetAllPerformersWithCriteria (managersOnly={0}, criteria={1}, count={2}, margin={3})" +
                " - записей: {4}, время вычислений: {5} мс",
                managersOnly,
                criteria + (ascending ? "[asc]" : "[desc]"), 
                count, 
                marginScore,
                total, 
                DateTime.Now.Subtract(startTime).TotalMilliseconds);

            return result.ToList();
        }

        public List<MarketOrder> GetAccountDeals(int accountId, bool openedDeals)
        {
            var orders = openedDeals
                             ? DealStorage.Instance.GetOpenedDeals(accountId)
                             : DealStorage.Instance.GetClosedDeals(accountId);
            return orders ?? new List<MarketOrder>();
        }

        public PerformerFormulasAndCriteriasSet GetFormulasAndCriterias()
        {
            if (defaultSet != null) return defaultSet;
            var set = new PerformerFormulasAndCriteriasSet
            {
                formulaValues = PerformerCriteriaFunctionCollection.Instance.criterias.Select(c => c.Function).ToArray(),
                formulaTitles = PerformerCriteriaFunctionCollection.Instance.criterias.Select(c => c.Description).ToArray()
            };
            var funParams = PerformerStatField.fields.Where(f => !string.IsNullOrEmpty(f.ExpressionParamTitle)).ToList();
            set.parameterNames = funParams.Select(p => p.ExpressionParamName).ToArray();
            set.parameterTitles = funParams.Select(p => p.ExpressionParamTitle).ToArray();
            defaultSet = set;
            return set;
        }

        /// <summary>
        /// фильтры: имя свойства - значение - учет регистра
        /// </summary>
        public List<PerformerStat> GetPerformersByFilter(bool managersOnly, List<PerformerSearchCriteria> filters, int count)
        {
            // данные не готовы к работе
            if (!cacheUpdated)
            {
                Logger.Info("GetPerformersByFilter - данные не готовы");
                return new List<PerformerStat>();
            }

            var result = new List<PerformerStat>();
            foreach (var performer in dicPerformers.ReceiveAllData())
            {
                // пропустить счета, привязанные в app.config
                // у них нет привязки к торговым сигналам
                if (managersOnly && performer.Value.Statistics.Service == 0) continue;
                var fulfil = true;
                var perfStat = performer.Value.Statistics;
                foreach (var filter in filters)
                {
                    var property = perfStat.GetType().GetProperty(filter.propertyName);
                    if (property == null)
                    {
                        fulfil = false;
                        break;
                    }
                    if (filter.compradant == null)
                        continue;
                    var value = property.GetValue(perfStat);
                    if (value == null)
                    {
                        fulfil = false;
                        break;
                    }

                    var filterValue = filter.ignoreCase ? filter.compradant.ToLower() : filter.compradant;
                    var valueStr = filter.ignoreCase ? value.ToString().ToLower() : value.ToString();

                    var isOk = filter.checkWholeWord
                                   ? valueStr == filterValue
                                   : valueStr.Contains(filterValue);

                    if (!isOk)
                    {
                        fulfil = false;
                        break;
                    }
                }
                if (fulfil)
                    result.Add(perfStat);
                if (result.Count >= count)
                    break;
            }

            if (result.Count == 0)
                Logger.InfoFormat("По фильтру " + string.Join(", ", filters.Select(f => f.propertyName + 
                    ": " + f.compradant)) + " ничего не найдено");
            else
                Logger.InfoFormat("GetPerformersByFilter - {0} записей", result.Count);

            return result;
        }

        // x == y ? 0 : (x > y ? 1 : -1)
        private static int ComparePerformerStats(PerformerStat x, PerformerStat y)
        {
            if (x == null)
            {
                if (y == null)
                    return 0;
                return -1;
            }
            if (y == null)
                return 1;
            return x.UserScore.CompareTo(y.UserScore);
        }
    }    
}

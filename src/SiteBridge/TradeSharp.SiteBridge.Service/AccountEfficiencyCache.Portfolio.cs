using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.SiteBridge.Service
{
    public partial class AccountEfficiencyCache
    {
        public List<int> GetCompanyTopPortfolios()
        {
            using (var context = DatabaseContext.Instance.Make())
            {
                try
                {
                    var info = context.TOP_PORTFOLIO.Where(p => p.OwnerUser == null);
                    return info.Select(p => p.Id).ToList();
                }
                catch (Exception ex)
                {
                    Logger.Error("AccountEfficiencyCache.GetCompanyTopPortfolios", ex);
                    return null;
                }
            }
        }

        public int GetSubscribedTopPortfolioId(string userLogin)
        {
            using (var context = DatabaseContext.Instance.Make())
            {
                try
                {
                    var info = context.USER_TOP_PORTFOLIO.Where(p => p.PLATFORM_USER.Login == userLogin);
                    if (!info.Any())
                        return -1;
                    return info.First().Portfolio;
                }
                catch (Exception ex)
                {
                    Logger.Error("AccountEfficiencyCache.GetSubscribedTopPortfolio", ex);
                    return -1;
                }
            }
        }

        public TopPortfolio GetSubscribedTopPortfolio(string userLogin)
        {
            using (var ctx = DatabaseContext.Instance.Make())
            {
                try
                {
                    var info = (from utp in ctx.USER_TOP_PORTFOLIO
                                join tp in ctx.TOP_PORTFOLIO on utp.Portfolio equals tp.Id
                                join pu in ctx.PLATFORM_USER on utp.User equals pu.ID
                                where pu.Login == userLogin
                                select new TopPortfolio
                                    {
                                        Criteria = tp.Criteria,
                                        DescendingOrder = tp.DescendingOrder,
                                        Id = tp.Id,
                                        ManagedAccount = tp.ManagedAccount,
                                        MarginValue = (float?) tp.MarginValue,
                                        Name = tp.Name,
                                        ParticipantCount = tp.ParticipantCount,
                                        OwnerUser = tp.OwnerUser,
                                        TradeSettings = new AutoTradeSettings
                                            {
                                                TradeAuto = utp.AutoTrade ?? false,
                                                MaxLeverage = utp.MaxLeverage,
                                                PercentLeverage = utp.PercentLeverage ?? 0,
                                                HedgingOrdersEnabled = utp.HedgingOrdersEnabled,
                                                FixedVolume = utp.FixedVolume,
                                                MinVolume = utp.MinVolume,
                                                MaxVolume = utp.MaxVolume,
                                                VolumeRound = (VolumeRoundType?) utp.VolumeRound,
                                                StepVolume = utp.StepVolume
                                            }
                                    });
                    if (!info.Any())
                        return null;
                    return info.FirstOrDefault();
                }
                catch (Exception ex)
                {
                    Logger.Error("AccountEfficiencyCache.GetSubscribedTopPortfolio", ex);
                    return null;
                }
            }
        }

        public TopPortfolio GetTopPortfolio(int id, out AccountEfficiency userAccountEfficiency)
        {
            userAccountEfficiency = null;
            TopPortfolio result;

            try
            {
                // reading db
                using (var context = DatabaseContext.Instance.Make())
                {
                    try
                    {
                        var portfolio = context.TOP_PORTFOLIO.FirstOrDefault(p => p.Id == id);
                        if (portfolio == null)
                            return null;

                        result = new TopPortfolio
                            {
                                Id = portfolio.Id,
                                Name = portfolio.Name,
                                Criteria = portfolio.Criteria,
                                ParticipantCount = portfolio.ParticipantCount,
                                DescendingOrder = portfolio.DescendingOrder,
                                MarginValue = (float?) portfolio.MarginValue,
                                ManagedAccount = portfolio.ManagedAccount,
                                OwnerUser = portfolio.OwnerUser
                            };
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("AccountEfficiencyCache.GetTopPortfolio.db - read error", ex);
                        return null;
                    }
                }

                // Statistics
                if (result.ManagedAccount.HasValue)
                    result.Statistics = GetPerformerByAccountId(result.ManagedAccount.Value);

                // идентификаторы сигнальщиков
                // тут будут корректировки, потому что вхождение сигнальщика в топ не всегда на данный момент времени соответствует критерию
                // потому что обновление топов и обновлние статистики сигнальщиков выполняются в разное время, а результаты выборки хранятся только run-time
                var performers = GetAllPerformersWithCriteria(true, result.Criteria, result.ParticipantCount,
                                                              !result.DescendingOrder, result.MarginValue, 0);

                if (result.Statistics == null || performers == null)
                {
                    Logger.ErrorFormat("GetTopPortfolio() - статистика / перформеры не получены, кеш обновлен: {0}", cacheUpdated);
                    return result;
                }

                // ManagerIds
                result.ManagerIds = performers.Select(p => p.Account).ToList();

                // Manages
                result.Managers = performers;

                // Orders
                if (result.ManagedAccount.HasValue)
                    result.Orders = GetAccountDeals(result.ManagedAccount.Value, true);

                if (result.IsCompanyPortfolio)
                    return result;

                // ----------------
                // 4 user portfolio
                // ----------------
                if (!cacheUpdated)
                    return null;
                var managersFullStat = new List<AccountEfficiency>();

                // beginning of profit1000 chart
                if (result.ManagerIds == null)
                    throw new Exception("GetTopPortfolio(польз. портфель) - список Id менеджеров = null");

                DateTime? beginDate = null;
                try
                {
                    foreach (var managerId in result.ManagerIds)
                    {
                        var stat = dicPerformers.ReceiveValue(managerId);
                        if (stat == null)
                        {
                            Logger.Error("AccountEfficiencyCache.GetTopPortfolio.dicPerformers.TryGetValue returned null");
                            return null; // ManagerIds.Count != managersFullStat.Count
                        }
                        if (stat.listProfit1000 == null || stat.listProfit1000.Count == 0)
                            continue;
                        managersFullStat.Add(stat);
                        // detect beginning
                        var firstDate = stat.listProfit1000.Min(e => e.time);
                        if (!beginDate.HasValue)
                            beginDate = firstDate;
                        else
                            if (firstDate < beginDate.Value)
                                beginDate = firstDate;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("AccountEfficiencyCache.GetTopPortfolio - stat gen error", ex);
                    return null;
                }
            
                // userAccountEfficiency
                userAccountEfficiency = new AccountEfficiency
                    {
                        listProfit1000 = new List<EquityOnTime>(),
                    };

                // pofit1000 calc
                if (beginDate.HasValue) 
                    for (var date = beginDate.Value.Date; date < DateTime.Now; date = date.AddDays(1))
                    {
                        float equity = 0;
                        var equityCount = 0;
                        foreach (var fullStat in managersFullStat)
                        {
                            var equityOnTime = fullStat.listProfit1000.Find(e => e.time == date);
                            if (equityOnTime.time == default(DateTime))
                                continue;
                            equity += equityOnTime.equity;
                            equityCount++;
                        }
                        if (equityCount == 0) // данных на этот день недостаточно - пропускаем
                            continue;
                        equity /= equityCount;
                        userAccountEfficiency.listProfit1000.Add(new EquityOnTime(equity, date));
                    }

                // stats calc
                userAccountEfficiency.Statistics.DealsCount = managersFullStat.Sum(s => s.Statistics.DealsCount);
                Logger.InfoFormat("запрошен портфель - GetTopPortfolio({0}, {1} управляющих)", result.Name, result.Managers);

                return result;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в GetTopPortfolio", ex);
                throw;
            }
        }
    }
}

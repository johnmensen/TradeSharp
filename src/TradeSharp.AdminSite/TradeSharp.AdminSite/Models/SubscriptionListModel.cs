using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Models
{
    /// <summary>
    /// Представляет список сигналов, на которые подписан какой-либо счёт
    /// </summary>
    public class SubscriptionListModel
    {
        /// <summary>
        /// Текущий пользователь, для которого представлена подписка на сигналы
        /// </summary>
        public PlatformUser CurrentUser { get; set; }

        /// <summary>
        /// Список сигналов, на которые оформлена подписка текущего пользователя
        /// </summary>
        public List<SubscriptionModel> SubscriptionList { get; set; }

        /// <summary>
        /// Список сигналов, автором которых является текущий пользователь
        /// </summary>
        public List<ServiceTradeSignalModel> OwnerSignalList { get; set; }

        public SubscriptionListModel(int userId)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var user =  ctx.PLATFORM_USER.Single(x => x.ID == userId);
                    CurrentUser = LinqToEntity.DecoratePlatformUser(user);

                    OwnerSignalList = new List<ServiceTradeSignalModel>();
                    OwnerSignalList = ctx.SERVICE.Where(x => x.User == userId).Select(s => new ServiceTradeSignalModel
                        {
                            Id = s.ID,
                            User = userId,
                            AccountId = s.AccountId,
                            Comment = s.Comment,
                            Currency = s.Currency,
                            FixedPrice = s.FixedPrice,
                            ServiceType = (PaidServiceType)s.ServiceType,
                            UserLogin = s.PLATFORM_USER.Login,
                            CountSubscriber = ctx.SUBSCRIPTION.Count(y => y.Service == s.ID)
                        }).ToList();

                    

                    SubscriptionList = new List<SubscriptionModel>();
                    SubscriptionList = ctx.SUBSCRIPTION.Where(x => x.User == userId).Select(s => new SubscriptionModel
                    {
                        User = s.User,
                        SignalOwnerId = s.SERVICE1.PLATFORM_USER.ID,                    // Сигнальщик Id
                        SignalOwnerLogin = s.SERVICE1.PLATFORM_USER.Login,              // Сигнальщик Login
                        Service = s.SERVICE1.ID,
                        ServiceType = s.SERVICE1.ServiceType,
                        RenewAuto = s.RenewAuto,
                        TimeEnd = s.TimeEnd,
                        TimeStarted = s.TimeStarted
                    }).ToList();             
                }
            }
            catch (Exception ex)
            {
                Logger.Info("ServiceTradeSignalModel", ex);
            }
        }
    }
}
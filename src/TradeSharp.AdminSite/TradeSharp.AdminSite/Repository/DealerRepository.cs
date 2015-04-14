using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Contract;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Repository
{
    public class DealerRepository : IDealerRepository
    {
        /// <summary>
        /// Получить с сервера описание всех дилеров
        /// </summary>
        /// <returns>перечисление диллеров</returns>
        public IEnumerable<DealerDescription> GetAllDealerDescription()
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    return GetAllDealerDescription(ctx);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetAllDealerDescription()", ex);
                return new List<DealerDescription>();
            }
        }

        /// <summary>
        /// Получить с сервера описание всех дилеров, используя уже имеющийся контекст (что бы не пересоздавать его лишний раз)
        /// </summary>
        /// <param name="ctx">контекст</param>
        /// <returns>перечисление диллеров</returns>
        public IEnumerable<DealerDescription> GetAllDealerDescription(TradeSharpConnection ctx)
        {
            Logger.Info(string.Format("попытка запроса описания всех дилеров и их мапинг в класс {0}", typeof(DealerDescription).Name));
            var dealerDescription = new List<DealerDescription>();
            try
            {
                foreach (var dealer in ctx.DEALER)
                    dealerDescription.Add(LinqToEntity.DecorateDealerDescription(dealer));
            }
            catch (Exception ex)
            {
                Logger.Error("GetAllDealerDescription()", ex);
                return new List<DealerDescription>();
            }
            return dealerDescription;
        }

        /// <summary>
        /// Сохраниение внесённых изменений в "группу счетов", но только тех свойств, которые относятся к диллеру
        /// </summary>
        /// <param name="accGrp">Группа счетов</param>
        public bool SaveChangesDealerFealdInAccountGroup(AccountGroup accGrp)
        {
            Logger.Info(string.Format("попытка сохраниения внесённых изменений в описание диллера для группы счетов {0}", accGrp.Code));

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var dealerGroup = ctx.DEALER_GROUP.Single(x => x.ACCOUNT_GROUP.Code == accGrp.Code);

                    if (!string.IsNullOrEmpty(accGrp.SessionName)) dealerGroup.SessionName = accGrp.SessionName;
                    if (!string.IsNullOrEmpty(accGrp.SenderCompId)) dealerGroup.SenderCompId = accGrp.SenderCompId;
                    if (!string.IsNullOrEmpty(accGrp.HedgingAccount)) dealerGroup.HedgingAccount = accGrp.HedgingAccount;
                    if (!string.IsNullOrEmpty(accGrp.MessageQueue)) dealerGroup.MessageQueue = accGrp.MessageQueue;
                    ctx.SaveChanges();

                    Logger.Info(string.Format("Сохранины изменения в описание диллера для группы счетов {0}", accGrp.Code));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format(
                    "SaveChangesDealerFealdInAccountGroup(). Не удалось сохраниенить внесённые изменения в описание диллера для группы счетов {0}",
                    accGrp.Code), ex);
                return false;
            }
        }

        /// <summary>
        /// Сохраниение внесённых изменений в "Описание диллера"
        /// </summary>
        /// <param name="dealerDescription">Ссылка на редактируемую запись</param>
        public bool SaveDealerChanges(DealerDescription dealerDescription)
        {
            Logger.Info(string.Format("попытка сохраниения внесённых изменений в описание диллера {0}", dealerDescription.Code));
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var undecorateDealer = ctx.DEALER.First(x => x.Code == dealerDescription.Code);
                    LinqToEntity.UndecorateDealer(undecorateDealer, dealerDescription);
                    ctx.SaveChanges();

                    Logger.Info(string.Format("Сохранины изменения в описание диллера {0}", dealerDescription.Code));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Не удалось сохраненить внесённые изменения в описание дилера {0}", dealerDescription.Code), ex);
                return false;
            }
        }

        /// <summary>
        /// обновление данных о том, какой дилер управляет группой счетов 
        /// ищем в "DEALER_GROUP" завись с "AccountGroup == groupCode" и меняем её "Dealer" на указанный "dealerCode"
        /// </summary>
        /// <param name="groupCode">Новая группа счетов для дилера</param>
        /// <param name="dealerCode">Код дилера</param>
        public bool UpdateDealerForAccountGroup(string groupCode, string dealerCode)
        {
            Logger.Info(string.Format("попытка обновления группы счетов {0} дилера {1}", groupCode, dealerCode));
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var dealerGroup = ctx.DEALER_GROUP.FirstOrDefault(x => x.ACCOUNT_GROUP.Code == groupCode);
                    #region
                    if (dealerGroup == null)
                    {
                        ctx.DEALER_GROUP.Add(new DEALER_GROUP
                            {
                                Dealer = dealerCode,
                                AccountGroup = groupCode
                            });
                    }
                    else
                    {
                        var newDealerGroup = new DEALER_GROUP
                            {
                                Dealer = dealerCode,
                                AccountGroup = groupCode,
                                HedgingAccount = dealerGroup.HedgingAccount,
                                MessageQueue = dealerGroup.MessageQueue,
                                SenderCompId = dealerGroup.SenderCompId,
                                SessionName = dealerGroup.SessionName
                            };
                        try
                        {
                            ctx.DEALER_GROUP.Remove(dealerGroup);
                            ctx.DEALER_GROUP.Add(newDealerGroup);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(string.Format("Не удалось обновить дилера {0}", groupCode), ex);
                            return false;
                        }
                    }
                    #endregion
                    ctx.SaveChanges();
                    Logger.Info(string.Format("В группе {1} удачно обновлён дилер на {0}", dealerCode, groupCode));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Не удалось обновить в группе счетов {0} дилера {1}", groupCode, dealerCode), ex);
                return false;
            }
        }
    }
}
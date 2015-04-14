using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Contract;
using TradeSharp.SiteAdmin.Models.Items;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Repository
{
    public class AccountGroupsRepository : IAccountGroupsRepository
    {
        private readonly IDealerRepository dealerRepository;
        public AccountGroupsRepository()
        {
            dealerRepository = DependencyResolver.Current.GetService<IDealerRepository>();
        }

        /// <summary>
        /// Запрос групп счетов с сервера
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AccountGroupItem> GetAccountGroups(string groupCode = "")
        {
            Logger.Info(String.Format("попытка запросить c сервера все группы счетов и их мапинг в класс {0}", typeof(AccountGroupItem).Name));
            using (var ctx = DatabaseContext.Instance.Make())
            {
                var allDealers = dealerRepository.GetAllDealerDescription(ctx).ToList();
                IEnumerable<AccountGroupItem> result;
                try
                {
                    result = new List<AccountGroupItem>(ctx.GetGroupsWithAccountsWrapped(!String.IsNullOrEmpty(groupCode.Trim()) ? groupCode : null).
                                                                                          Select(ag => new AccountGroupItem
                                                                                              {
                                                                                                  Code = ag.Code,
                                                                                                  Name = ag.Name,
                                                                                                  IsReal = ag.IsReal,
                                                                                                  BrokerLeverage = (float)ag.BrokerLeverage,
                                                                                                  MarginCallPercentLevel = (float)ag.MarginCallPercentLevel,
                                                                                                  StopoutPercentLevel = (float)ag.StopoutPercentLevel,
                                                                                                  AccountsCount = ag.Accounts.HasValue ? ag.Accounts.Value : 0,
                                                                                                  Dealer = new DealerDescription
                                                                                                      {
                                                                                                          Code = ag.DealerCode,
                                                                                                          DealerEnabled = ag.DealerEnabled.HasValue && ag.DealerEnabled.Value,
                                                                                                          FileName = ag.FileName
                                                                                                      },
                                                                                                  MessageQueue = ag.MessageQueue,
                                                                                                  SessionName = ag.SessionName,
                                                                                                  HedgingAccount = ag.HedgingAccount,
                                                                                                  SenderCompId = ag.SenderCompId,
                                                                                                  SwapFree = ag.SwapFree,
                                                                                                  Markup = (AccountGroup.MarkupType)ag.MarkupType,
                                                                                                  DefaultMarkupPoints = (float)ag.DefaultMarkupPoints,
                                                                                                  DefaultVirtualDepo = ag.DefaultVirtualDepo.HasValue ? ag.DefaultVirtualDepo.Value : 0,
                                                                                                  Dealers = allDealers.Select(x => new SelectListItem
                                                                                                      {
                                                                                                          Text = x.Code,
                                                                                                          Value = x.Code,
                                                                                                          Selected = x.Code == ag.DealerCode
                                                                                                      }).ToList()
                                                                                              }));

                    foreach (var accountGroupRecord in result)
                    {
                        if (accountGroupRecord.Dealer.Code == null)
                            accountGroupRecord.Dealers.Insert(0, new SelectListItem { Text = Resource.TitleNotSelected, Value = null, Selected = true });
                    }

                }
                catch (Exception ex)
                {
                    var info = String.Format("Не удалось получить группы счетов {0}", groupCode);
                    Logger.Error(info, ex);
                    throw new Exception(info);
                }              
                return result;
            }
        }

        /// <summary>
        /// Сохраниение внесённых изменений в "группу счетов"
        /// </summary>
        /// <param name="accGrp">Ссылка на редактируемую группу счетов</param>
        public string SaveAccountGroupChanges(AccountGroup accGrp)
        {
            string message;
            Logger.Info(String.Format("попытка сохраниения внесённых изменений в группу счетов {0}", accGrp.Code));
           
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var undecorateAccountGroup = ctx.ACCOUNT_GROUP.First(x => x.Code == accGrp.Code);
                    LinqToEntity.UndecorateAccountGroup(undecorateAccountGroup, accGrp);
                    ctx.SaveChanges();

                    message = string.Format("{1} {0}.", accGrp.Code, Resource.MessageAccountGroupEditedSuccessful);
                }
            }
            
            catch (Exception ex)
            {
                Logger.Error(String.Format("Не удалось сохранить внесённые изменения в группу счетов {0}", accGrp.Code), ex);
                message = string.Format("{1} {0}.", accGrp.Code, Resource.ErrorMessageAccountGroupSave);
            }

            if (accGrp.Dealer != null)
            {
                message += dealerRepository.SaveChangesDealerFealdInAccountGroup(accGrp)
                    ? string.Format("{2} {0} {3} {1}", accGrp.Dealer, accGrp.Code, Resource.MessageAccountGroupSaveDealerInfo, Resource.MessageAccountGroupFor)
                    : string.Format("{1} {0}.", accGrp.Code, Resource.ErrorMessageAccountGroupSaveDeallerInfo);
            }
            else
            {
                message += string.Format("  {1} {0}.", accGrp.Code, Resource.ErrorMessageAccountGroupSaveDeallerInfo);
            }

            return message;
        }

        /// <summary>
        /// Добавление новой группы счетов
        /// </summary>
        /// <param name="newAccountGroup">Добавляемая группа счётов</param>
        /// <returns>Сообщение о результате выполнения запроса</returns>
        /// <remarks>Не тестируется</remarks>
        public string AddAccountGroup(AccountGroupItem newAccountGroup)
        {
            Logger.Info(string.Format("Попытка добавить новую группу четов {0} в БД.", newAccountGroup.Code));
            try
            {
                var undecorateAccountGroup = LinqToEntity.UndecorateAccountGroup(newAccountGroup);
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    ctx.ACCOUNT_GROUP.Add(undecorateAccountGroup);
                    ctx.SaveChanges();
                    return string.Format("{1} (# {0}).", newAccountGroup.Code, Resource.MessageAccountGroupAdd);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("AddAccountGroup()", ex);
            }
            return string.Format("{1} {0}.", newAccountGroup.Code, Resource.ErrorMessageAccountGroupAdd);
        }

        /// <summary>
        /// Удаляет группу счетов, если в ней нет счетов
        /// </summary>
        /// <param name="accountGroupCode">код группы, которую нужно удалить</param>
        public string DeleteVoidAccountGroup(string accountGroupCode)
        {
            Logger.Info(string.Format("Попытка удалить группу счетов {0}", accountGroupCode));
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var accountGroup = ctx.ACCOUNT_GROUP.FirstOrDefault(x => x.Code == accountGroupCode);
                    if (accountGroup == null) return string.Format("{1} {0}. {2}.", accountGroupCode, Resource.ErrorMessageAccountGroupDelete, Resource.ErrorMessageAccountGroupNotFound);

                    var countAccount = accountGroup.ACCOUNT.Count;
                    if (countAccount > 0) return string.Format("{2} {0}. {3} {1}. {4}.",
                        accountGroupCode, countAccount, Resource.ErrorMessageAccountGroupDelete, Resource.TextNumberAccountsInGroup, Resource.TextAccountGroupMayBeDellIfNoAccounts);

                    var deallerGroup = ctx.DEALER_GROUP.FirstOrDefault(x => x.AccountGroup == accountGroupCode);

                    if (deallerGroup != null)
                    {
                        ctx.DEALER_GROUP.Remove(deallerGroup);
                        Logger.Info(string.Format("Для группы счетов {0} удалён дилер", accountGroupCode));
                    }
                    else
                    {
                        Logger.Info(string.Format("У группы счетов {0} не найден дилер", accountGroupCode));
                    }

                    ctx.ACCOUNT_GROUP.Remove(accountGroup);
                    ctx.SaveChanges();

                    return string.Format("{1} (# {0}).", accountGroupCode, Resource.TextAccountGroupDell);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("DeleteVoidAccountGroup()", ex);
            }

            return string.Format("{1} {0}.", accountGroupCode, Resource.ErrorMessageAccountGroupDelete);
        }
    }
}
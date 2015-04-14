using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Contract;
using TradeSharp.SiteAdmin.Models.CommonClass;
using TradeSharp.SiteAdmin.Models.Items;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Repository
{
    public class TopPortfolioRepository : ITopPortfolioRepository
    {
        private readonly IUserRepository userRepository;
        public TopPortfolioRepository()
        {
            userRepository = DependencyResolver.Current.GetService<IUserRepository>();
        }

        public List<TopPortfolio> GetAllTopPortfolio()
        {
            var result = new List<TopPortfolio>();
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    result = new List<TopPortfolio>();
                    foreach (var item in ctx.TOP_PORTFOLIO.Where(x =>x .ManagedAccount.HasValue))
                        result.Add(LinqToEntity.DecoratePortfolio(item));                      
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetAllTopPortfolio()", ex);
            }
            return result;
        }

        public TopPortfolio GetTopPortfolio(int id)
        {
            TopPortfolio portfolio;
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var undecPortfolio = ctx.TOP_PORTFOLIO.FirstOrDefault(x => x.Id == id);
                    portfolio = LinqToEntity.DecoratePortfolio(undecPortfolio);
                }
            }
            catch (Exception ex)
            {
                var info = String.Format("Не удалось получить портфель {0}", id);
                Logger.Error(info, ex);
                return null;
            }
            return portfolio;
        }

        public bool SaveTopPortfolioChanges(TopPortfolioItem topPortfolioItem)
        {

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var undecoratePortfolio = ctx.TOP_PORTFOLIO.Single(a => a.Id == topPortfolioItem.Id);
                    LinqToEntity.UndecoratePortfolio(undecoratePortfolio, topPortfolioItem);
                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(
                    String.Format("Не удалось сохранить изменения в портфеле роботов {0}", topPortfolioItem.Id), ex);
            }
            return false;
        }

        public bool UpdateCriteria(int id, string newCriteria, double? newMarginValue)
        {

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var undecoratePortfolio = ctx.TOP_PORTFOLIO.Single(x => x.Id == id);
                    undecoratePortfolio.Criteria = newCriteria;
                    undecoratePortfolio.MarginValue = newMarginValue;
                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(String.Format("Не удалось сохранить изменения в портфеле роботов {0}", id), ex);
            }
            return false;
        }

        public string SubscribeOnPortfolioOnExistAccount(TopPortfolioItem newTopPortfolio)
        {
            string errorMessage;
            if (!newTopPortfolio.ManagedAccount.HasValue)
            {
                errorMessage = Resource.ErrorMessageCampaignAccount;
                Logger.Error(errorMessage);
                return errorMessage;
            }

            var ownerUser = userRepository.GetAccountOwner(newTopPortfolio.ManagedAccount.Value);
            if (ownerUser == null)
            {
                errorMessage = Resource.ErrorMessageNotFindUserAccountOwner;
                Logger.Error(errorMessage);
                return errorMessage;
            }

            var topPortfolio = AddTopPortfolio(newTopPortfolio);
            if (topPortfolio == null)
            {
                errorMessage = Resource.ErrorMessageTopPortfolioUnableAdd;
                Logger.Error(errorMessage);
                return errorMessage;
            }

            Logger.Info("Новый портфель роботов создан");
            try
            {
                var proxy = new TradeSharpServerTrade(EmptyTradeSharpCallbackReceiver.Instance).proxy;
                
                var status = proxy.SubscribeOnPortfolio(ProtectedOperationContext.MakeServerSideContext(),
                                                        ownerUser.Login, topPortfolio, null, Utils.GetDefaultPortfolioTradeSettings());
                if (status != RequestStatus.OK)
                {
                    errorMessage = Resource.ErrorMessageCampaignAccount;
                    Logger.Error(errorMessage);
                    return errorMessage;
                }
                return string.Empty;                
            }
            catch (Exception ex)
            {
                errorMessage = Resource.ErrorMessageUnableSignAccount + ": " + ex.Message;
                Logger.Error("AddTopPortfolio", ex);
            }
            return errorMessage;
        }

        public string SubscribeOnPortfolioOnNewAccount(TopPortfolioItem newTopPortfolio)
        {
            var accountRepository = DependencyResolver.Current.GetService<IAccountRepository>();

            string errorMessage;
            if (!accountRepository.AddAccount(newTopPortfolio.AddAccountModel))
            {
                errorMessage = Resource.ErrorMessageNotCreateAccountForTopPortfolio + " " + newTopPortfolio.AddAccountModel.UserName;
                Logger.Error("SubscribeOnPortfolioOnNewAccount() - " + errorMessage);
                return errorMessage;
            }

            newTopPortfolio.ManagedAccount = accountRepository.GetAccountId(newTopPortfolio.AddAccountModel.UserLogin);

            var topPortfolio = AddTopPortfolio(newTopPortfolio);
            if (topPortfolio == null)
            {
                errorMessage = Resource.ErrorMessageTopPortfolioUnableAdd;
                Logger.Error(errorMessage);
                return errorMessage;
            }

            Logger.Info("Новый портфель роботов создан");
            try
            {
                var proxy = new TradeSharpServerTrade(EmptyTradeSharpCallbackReceiver.Instance).proxy;
                var status = proxy.SubscribeOnPortfolio(ProtectedOperationContext.MakeServerSideContext(), 
                    newTopPortfolio.AddAccountModel.UserLogin, topPortfolio, null, Utils.GetDefaultPortfolioTradeSettings());
                if (status != RequestStatus.OK)
                {
                    errorMessage = Resource.ErrorMessageUnableSignAccount;
                    Logger.Error(errorMessage);
                    return errorMessage;
                }
                return string.Empty;                
            }
            catch (Exception ex)
            {
                errorMessage = Resource.ErrorMessageUnableSignAccount + ": " + ex.Message;
                Logger.Error("AddTopPortfolio", ex);
            }
            return errorMessage;
        }

        public bool DeletePortfolio(int id)
        {
            if (!UnsubscribeSubscribersFromPortfolio(id))
            {
                Logger.Error("Удаление портфеля id = " + id + " прервано т.к. произошла ошибка на этапе удаления подписки пользователей этого портфеля");
                return false;
            }

            if (!RemoveTopPortfolio(id))
            {
                Logger.Error("Произошла ошибка на этапе удаления портфеля id = " + id);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Выбираем пользователей, которые подписаны на портфель, и отписываем их от портфеля
        /// </summary>
        /// <param name="id">уникальный идентификатор портфеля</param>
        /// <returns></returns>
        private static bool UnsubscribeSubscribersFromPortfolio(int id)
        {
            var errorsUnsubscribe = new List<string>();
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var logins = ctx.USER_TOP_PORTFOLIO.Where(x => x.Portfolio == id).Select(x => x.PLATFORM_USER.Login);

                    var proxy = new TradeSharpServerTrade(EmptyTradeSharpCallbackReceiver.Instance).proxy;
                    Logger.Info("Начинаем отписывать пользователей от портфеля id = " + id);
                    foreach (var login in logins)
                    {
                        var result = proxy.UnsubscribePortfolio(ProtectedOperationContext.MakeServerSideContext(), login, false, true);
                        if (result != RequestStatus.OK) errorsUnsubscribe.Add(login);
                    }                    
                }
                if (errorsUnsubscribe.Count > 0)
                {
                    Logger.Error(string.Format("От портфеля {0} не удалось отписать следующих пользователей: {1}", id, string.Join(Environment.NewLine, errorsUnsubscribe)));
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("UnsubscribeOnPortfolio()", ex);
                return false;
            }
        }

        private static bool RemoveTopPortfolio(int id)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var removePortfolio = ctx.TOP_PORTFOLIO.Single(x => x.Id == id);
                    ctx.TOP_PORTFOLIO.Remove(removePortfolio);
                    ctx.SaveChanges();
                    Logger.Info("удалён портфель роботов " + id);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("RemoveTopPortfolio()", ex);
                return false;
            }
        }

        /// <summary>
        /// Добавляем в базу новую сущность
        /// </summary>
        /// <param name="newTopPortfolio">Объект содержит значения полей для новой сущность</param>
        /// <returns></returns>
        private static TopPortfolio AddTopPortfolio(TopPortfolioItem newTopPortfolio)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var topPortfolio = new TOP_PORTFOLIO
                    {
                        Name = newTopPortfolio.Name,
                        Criteria = newTopPortfolio.Criteria,
                        ParticipantCount = newTopPortfolio.ParticipantCount,
                        DescendingOrder = newTopPortfolio.DescendingOrder,
                        MarginValue = newTopPortfolio.MarginValue,
                        ManagedAccount = newTopPortfolio.ManagedAccount
                    };

                    ctx.TOP_PORTFOLIO.Add(topPortfolio);
                    ctx.SaveChanges();

                    return LinqToEntity.DecoratePortfolio(topPortfolio);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("AddTopPortfolio", ex);
                return null;
            }
        }
    }
}
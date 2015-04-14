using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MvcPaging;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Admin.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Models.CommonClass
{
    /// <summary>
    /// Вспомогательный класс для работы со счетами
    /// </summary>
    public static class AccountUtils
    {
        /// <summary>
        /// Возвращает модель представления для представления Accounts контроллера Management
        /// </summary>
        public static AccountViewModel GetAccountViewModel(List<AccountTag> accounts)
        {
            var listFilterGroups = Utils.SelectAllValuesFromTable<ACCOUNT_GROUP>(x => new SelectListItem{ Text = x.Code, Value = x.Code });
            listFilterGroups.Insert(0, new SelectListItem { Text = "", Value = "" }); // для фильтра добавляем пустой элемент для поля "сбросить фильтр" в выпадающим списке

            var listFilterCurrency = Utils.SelectAllValuesFromTable<COMMODITY>(x => new SelectListItem{ Text = x.Title, Value = x.Title });
            listFilterCurrency.Insert(0, new SelectListItem { Text = "", Value = "" }); // для фильтра добавляем пустой элемент для поля "сбросить фильтр" в выпадающим списке

            var viewModel = new AccountViewModel
            {
                CurrentPageItems = new PagedList<AccountTag>(accounts, 0, 10),
                SortColName = new List<KeyValuePair<string, int>> { new KeyValuePair<string, int>("SortId", 1 ) },
                StrSortColName = "SortId:1",
                FilterGroups = listFilterGroups,           //для выпадающего списка фильра
                FilterBalanceTickers = listFilterCurrency //для выпадающего списка фильра
            };
            return viewModel;
        }    
           
        /// <summary>
        /// Удаление счёта
        /// </summary>
        /// <param name="id">Уникальный идентификатор счёта</param>
        /// <returns>Результат выполнения операции</returns>
        public static string Delete(int id)
        {
            Logger.Info(String.Format("Начинаем удалять счёт {0}", id));
            try
            {
                List<PlatformUser> userWithoutAccountList;
                if (CleenupManager.DeleteAccount(id, out userWithoutAccountList))
                {
                    var retMessage = string.Format("{1} ({0}). {2}.", id, Resource.MessageAccountDeleted, Resource.MessageDoesNotUserNotHaveAccount);
                    if (!userWithoutAccountList.Any())
                    {
                        Logger.Info(retMessage);
                    }
                    else
                    {
                        foreach (var platformUser in userWithoutAccountList)
                            Logger.InfoFormat("пользователь {0} (id {1}) теперь не имеет ни одного счёта", platformUser.Login, platformUser.ID);

                        retMessage = string.Format("{1} ({0}). {2}.", id, Resource.MessageAccountDeleted, Resource.MessageUsersWithoutAccountsInLog);
                    }

                    return retMessage;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Delete()", ex);
            }
            return string.Format("{1} {0}.", id, Resource.ErrorMessageCanNotDellAccount);
        }
    }
}
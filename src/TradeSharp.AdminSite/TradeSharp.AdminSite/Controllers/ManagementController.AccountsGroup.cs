using System;
using System.Linq;
using System.Web.Mvc;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Models.CommonClass;
using TradeSharp.SiteAdmin.Models.Items;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Controllers
{
    /// <summary>
    /// Содержит действия для управления группами счетов
    /// </summary>
    public partial class ManagementController
    {
        /// <summary>
        /// Получает с сервера все группы счетов и сортирует их по указанному полю
        /// </summary>
        /// <param name="sortColumn">Поле по которому нужно отсортиорвать</param>
        /// <param name="message"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AccountGroups(string sortColumn, string message)
        {
            // Поскольку для групп счетов требуется только сортировка, то обойдёмся только передачей параметров в URL, а
            // формы и POST делать не будем
            var result = accountGroupsRepository.GetAccountGroups();

            #region Сортировка
            switch (!String.IsNullOrEmpty(sortColumn) ? sortColumn : "Code")
            {
                case "Code":
                    result = result.OrderByDescending(x => x.IsReal).ThenBy(x => x.Code);
                    break;
                case "Name":
                    result = result.OrderBy(x => x.Name);
                    break;
                case "IsReal":
                    result = result.OrderByDescending(x => x.IsReal);
                    break;
                case "BrokerLeverage":
                    result = result.OrderBy(x => x.BrokerLeverage);
                    break;
                case "MarginCallPercentLevel":
                    result = result.OrderBy(x => x.MarginCallPercentLevel);
                    break;
                case "StopoutPercentLevel":
                    result = result.OrderBy(x => x.StopoutPercentLevel);
                    break;
                case "Dealer":
                    result = result.OrderBy(x => x.Dealer.Code);
                    break;
                case "AccountsCount":
                    result = result.OrderByDescending(x => x.AccountsCount);
                    break;
            }
            #endregion

            if (!string.IsNullOrEmpty(message))ResultMessage = message;

            if (Request.IsAjaxRequest())
                return PartialView("AccountGroupsPartialTable", result);
            return View(result);
        }

        /// <summary>
        /// Переход на страницу добавления группы счетов
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AccountGroupAdd()
        {
            return View(new AccountGroupItem());
        }
        
        /// <summary>
        /// Добавление новой группы счетов
        /// </summary>
        /// <param name="newAccountGroup">ссылка на модель новой группы счетов</param>
        [HttpPost]
        public ActionResult AccountGroupAdd(AccountGroupItem newAccountGroup)
        {
            var result = accountGroupsRepository.AddAccountGroup(newAccountGroup);
            return RedirectToAction("AccountGroups", new { message = result });
        }

        /// <summary>
        /// Асинхронная проверка 'занятости' уникального идентификатора группы счетов (кода) для группы счетов. Это применяется при добавлении новой группы счетов.
        /// </summary>
        /// <param name="accountGroupCode">Код, который нужно проверить</param>
        [HttpPost]
        public ActionResult AccountGroupCheckCodeExistAjaxUpdate(string accountGroupCode)
        {
            return Json(new 
            { AccountGroupCodeExistHTML = Utils.CheckExistValuesFromTable<ACCOUNT_GROUP>(x => x.Code == accountGroupCode) ?
                string.Format("{0} {1}", Resource.TitleCode, Resource.TextIsAlreadyInUse) : ""}, 
                JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Получает группу счетов по коду и перенаправляет на форму редактирования
        /// </summary>
        /// <param name="code">код групы счетов</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditAccountGroupsDetails(string code)
        {
            AccountGroupItem result = null;
            try
            {
                result = accountGroupsRepository.GetAccountGroups(code).First();
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error(String.Format("EditAccountGroupsDetails - на сервере отсутствует группа счетов с кодом {0}", code), ex);
            }
            catch (Exception ex)
            {
                Logger.Error(String.Format("EditAccountGroupsDetails - ошибка полуения с сервера группы счетов по её коду {0}", code), ex);
            }
            return View(result);
        }

        /// <summary>
        /// Сохранение внесённых изменений в группу счетов
        /// </summary>
        /// <param name="model">группа счетов с обновлёными данными</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditAccountGroupsDetails(AccountGroupItem model)
        {
            var result = accountGroupsRepository.SaveAccountGroupChanges(model);
            return RedirectToAction("AccountGroups", new { message = result });
        }

        [HttpGet]
        public ActionResult EditAccountGroupsDetailsCancel()
        {
            return RedirectToAction("AccountGroups", new { message = Resource.MessageEditingCanceled });
        }

        /// <summary>
        /// Удаляет группу счетов в которой нет счетов
        /// </summary>
        /// <param name="accountGroupCode">код группы, которую нужно удалить</param>
        [HttpGet]
        public ActionResult DeleteVoidAccountGroup(string accountGroupCode)
        {
            var result = accountGroupsRepository.DeleteVoidAccountGroup(accountGroupCode);
            return RedirectToAction("AccountGroups", new { message = result });
        }
    }
}

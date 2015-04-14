using System;
using System.Collections.Generic;
using System.Data.Objects.SqlClient;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using MvcPaging;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Admin.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Models;
using TradeSharp.SiteAdmin.Models.CommonClass;
using TradeSharp.SiteAdmin.Models.Items;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Controllers
{
    /// <summary>
    /// Содержит действия для управления счетами пользователей
    /// </summary>
    public partial class ManagementController
    {
        /// <summary>
        /// Вытаскиваем все счета для таблици из "AccountsPartialTable.cshtml"
        /// </summary>
        [HttpGet]
        public ActionResult Accounts(string message)
        {
            var viewModel = AccountUtils.GetAccountViewModel(accountRepository.GetFilterAccountFromServer(null));

            if (!string.IsNullOrEmpty(message)) ResultMessage = message;

            if (Request.IsAjaxRequest())
                return PartialView("AccountsPartialTable", viewModel);
            return View("Accounts", viewModel);
        }

        /// <summary>
        /// Сортировка, переход на другую страницу или осуществление фильтрации таблици из "AccountsPartialTable.cshtml"
        /// </summary>
        /// <param name="model">Модель содержит текушие значения фильтров, номер текущей страници и т.п.</param>
        /// <param name="pageAccountAction">Параметр отражает действие пользователя - переход на др. страницу или сортировка</param>
        /// <param name="pageAccountArg">По какому столбцу произошла сортировка (SortId, например) или на страницу с каким номером перешол пользователь</param>
        [HttpPost]
        public ActionResult Accounts(AccountViewModel model, string pageAccountAction, string pageAccountArg)
        {
            var pageItems = FillAccountViewModel(model, pageAccountAction, pageAccountArg);

            ModelState.Remove("StrSortColName"); 
            ModelState.Remove("PageNomber");

            var newModel = new AccountViewModel
            {
                CurrentPageItems = pageItems,
                PageNomber = pageItems.PageIndex,
                SortColName = model.SortColName,
                StrSortColName = model.StrSortColName
            };

            if (Request.IsAjaxRequest())
                return PartialView("AccountsPartialTable", newModel);        
            return  View("Accounts", newModel);
        }

        /// <summary>
        /// Переход на страницу редактирования счёта
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AccountDetails(int accountId)
        {
            ViewData["listTickers"] = Utils.SelectAllValuesFromTable<COMMODITY>(x => new SelectListItem { Text = x.Title, Value = x.Title });
            ViewData["listGroups"] = Utils.SelectAllValuesFromTable<ACCOUNT_GROUP>(x => new SelectListItem{Text = x.Code,Value = x.Code});
            return View(accountRepository.GetAccount(accountId));
        }

        /// <summary>
        /// Произвести редактирование счёта и сохраниение введённых значений
        /// </summary>
        [HttpPost]
        public ActionResult AccountDetails(AccountItem account)
        {
            if (!ModelState.IsValid) return RedirectToAction("AccountDetails", new { accountId = account.ID });

            var result = string.Format(accountRepository.SaveAccountChanges(account) ?
                                      Resource.MessageEditingMade + " (" + Resource.TitleAccount + "  {0})." :
                                      Resource.ErrorMessageSaveDataChanges + " ("+ Resource.TitleAccount + "  {0}).", account.ID);


            ViewData["listTickers"] = Utils.SelectAllValuesFromTable<COMMODITY>(x => new SelectListItem { Text = x.Title, Value = x.Title });
            ViewData["listGroups"] = Utils.SelectAllValuesFromTable<ACCOUNT_GROUP>(x => new SelectListItem { Text = x.Code, Value = x.Code });
            return RedirectToAction("Accounts", new { message = result });
        }

        /// <summary>
        /// Переходим на форму добавления нового счёта
        /// </summary>
        [HttpGet]
        public ActionResult AccountAdd()
        {
            FillDropDownListForAccountView();
            var emptyAccount = new AddAccountModel
            {
                AccountMaxLeverage = 10
            };
            return View(emptyAccount);
        }

        /// <summary>
        /// Осуществление добавления нового счёта
        /// </summary>
        /// <param name="account">ссылка на редактируемый счёт</param>
        [HttpPost]
        public ActionResult AccountAdd(AddAccountModel account)
        {
            FillDropDownListForAccountView();
            if (!ModelState.IsValid) return View(account);
            var viewModel = AccountUtils.GetAccountViewModel(accountRepository.GetFilterAccountFromServer(null));

            ResultMessage =
                accountRepository.AddAccount(account) ?
                Resource.MessageAccauntAdded :
                Resource.ErrorMessageAccauntAdded;

            return View("Accounts", viewModel);
        }

        /// <summary>
        /// Переходим на форму редактирования информации о пользователе - владельце счёта
        /// </summary>
        /// <param name="ownerId">Уникальный идентификатор записи из PLATFORM_USER</param>
        [HttpGet]
        public ActionResult OwnerDetails(int ownerId)
        {
            var viewModel = userRepository.GetUserInfoById(ownerId);
            FillDropDownListForAccountView();
            return View(viewModel);
        }

        /// <summary>
        /// Осуществление редактирование информации о пользователе - владельце счёта
        /// </summary>
        /// <param name="upDateOwner">ссылка на редактируемого пользователя</param>
        [HttpPost]
        public ActionResult OwnerDetails(AccountUserModel upDateOwner)
        {
            var result = userRepository.EditUserInfo(upDateOwner) ?
                string.Format("{0} (#{1}).", Resource.MessageEditingMade, upDateOwner.UserLogin) :
                string.Format("{0} ({1} #{2}).", Resource.ErrorMessageSaveDataChanges, Resource.TitleUser, upDateOwner.UserLogin);
            return RedirectToAction("Accounts", new { message = result });
        }

        /// <summary>
        /// Отмена редактирования информации о пользователе
        /// </summary>
        public ActionResult OwnerDetailsEditCancel()
        {
            return RedirectToAction("Accounts", new { message = Resource.MessageEditingCanceled });
        }

        /// <summary>
        /// формирует модель для представления "AccountOwner", которое содержи таблицу соответствия "счёт => владелец"
        /// </summary>
        /// <param name="accountId">параметр НЕ равен null в случае, если модель должна быть сформирована в виде "показать всех подписчиков счёта accountId"</param>
        /// <param name="ownerId">параметр НЕ равен null в случае, если модель должна быть сформирована в виде "показать все счета пользователя ownerId"</param>
        /// <returns>объект типа AccountOwnerModel</returns>
        [HttpGet]
        public ActionResult AccountOwner(int? accountId = null, int? ownerId = null)
        {
            var accountOwnerItem = accountRepository.GetAccountOwnerModel(accountId, ownerId);
            
            // заполнить списки, которе содержат всех пользователей и все счета
            ViewData["listOwners"] = Utils.SelectAllValuesFromTable<PLATFORM_USER>(x => new SelectListItem{Text = x.Login,Value = SqlFunctions.StringConvert((double)x.ID).Trim()});
            ViewData["listAccounts"] = Utils.SelectAllValuesFromTable<ACCOUNT>(x => new SelectListItem
            {
                Text = SqlFunctions.StringConvert((double)x.ID).Trim(),
                Value = SqlFunctions.StringConvert((double)x.ID).Trim()
            });

            // заполнить списки, котовые содержат только тех пользователей или те счета, которые НЕ содержаться в модели данных "accountOwnerItem"
            // это сделано для того, что бы при добавлении, например, к пользователю ещё одного счёта, нельзя было бы добавить уже имеющийся у пользователя счёт
            ViewData["listOwnersForEdit"] = Utils.SelectAllValuesFromTable<PLATFORM_USER>(x => new SelectListItem
            {
                Text = x.Login,
                Value = SqlFunctions.StringConvert((double)x.ID).Trim()
            }).Where(x => !accountOwnerItem.Owners.Select(y => y.UserId).ToList().Contains(Convert.ToInt32(x.Value)));

            ViewData["listAccountsForEdit"] = Utils.SelectAllValuesFromTable<ACCOUNT>(x => new SelectListItem
            {
                Text = SqlFunctions.StringConvert((double)x.ID).Trim(),
                Value = SqlFunctions.StringConvert((double)x.ID).Trim()
            }).Where(x => !accountOwnerItem.Accounts.Select(y => y.ID).ToList().Contains(Convert.ToInt32(x.Value)));

            if (Request.IsAjaxRequest())
                switch (accountOwnerItem.AccountOwnerPartialViewType)
                {
                    case AccountOwnerPartialViewType.ForAccount:
                        return PartialView("AccountOwnerPartialForAccount", accountOwnerItem);
                    case AccountOwnerPartialViewType.ForOwner:
                        return PartialView("AccountOwnerPartialForUser", accountOwnerItem);
                } 

            return View("AccountOwner", accountOwnerItem);
        }

        /// <summary>
        /// Добавление/ удаление пользователя в управляющие счёта или добавление/ удаление счёта под управление какого либо пользователя
        /// </summary>
        /// <param name="accountId">Уникальный идентификатор счёта</param>
        /// <param name="ownerId">Уникальный идентификатор пользователя</param>
        /// <param name="right">0 - право на просмотр, 1 - право на торговлю</param>
        /// <param name="act">действие (удалить / добавить). Осторожно этот параметр не строго типизирован - просто строка "add" или "del"</param>
        /// <param name="accountOwnerPartialViewType">Флаг, в каком режиме формировать представление (все счета пользователя или все управляющие счёта)</param>
        [HttpGet]
        public ActionResult AccountOwnerEdit(int accountId, int ownerId, int right, string act, int accountOwnerPartialViewType)
        {
            accountRepository.EditAccountOwnerModel(accountId, ownerId, right, act);

            switch ((AccountOwnerPartialViewType)accountOwnerPartialViewType)
            {
                case AccountOwnerPartialViewType.ForAccount:
                    return RedirectToAction("AccountOwner", new RouteValueDictionary { { "accountId", accountId } });
                case AccountOwnerPartialViewType.ForOwner:
                    return RedirectToAction("AccountOwner", new RouteValueDictionary { { "ownerId", ownerId } });
                default:
                    return RedirectToAction("AccountOwner", new RouteValueDictionary { { "ownerId", ownerId } });
            }
        }

        /// <summary>
        /// Отмена редактирования счёта
        /// </summary>
        public ActionResult AccountActionCancel()
        {
            return RedirectToAction("Accounts", new { message = Resource.MessageEditingCanceled });
        }

        /// <summary>
        /// деактивировать счёт
        /// </summary>
        /// <param name="accountId">Уникальный идентификатор</param>
        /// <returns></returns>
        public ActionResult DeactivateAccount(string accountId)
        {
            int id;
            if (!int.TryParse(accountId, out id)) RedirectToAction("Accounts");

            var message = accountRepository.Deactivate(id) ?
                string.Format("{0} (#{1})", Resource.MessageAccountDeactivated, accountId)
                : string.Format("{0} #{1}.", Resource.ErrorMessageAccountDeactivated, accountId);

            return Accounts(message);
        }

        /// <summary>
        /// Удаление счёта
        /// </summary>
        /// <param name="accountId">Уникальный идентификатор счёта</param>
        public ActionResult DeleteAccount(string accountId)
        {
            int id;
            if (!int.TryParse(accountId, out id))
                RedirectToAction("Accounts", new { message = string.Format("{0}. {1} {2}", Resource.ErrorMessageIdMustBeInteger, Resource.TextCurrentValue, accountId) });

            string message;
            var signalCount = accountRepository.GetSignalCount(id);

            if (signalCount == null)
            {
                message = Resource.ErrorMessageDataAccess;
            }
            else
            {
                message = signalCount > 0
                              ? string.Format("{0}: {1} - {2}. {3}: {4}", 
                              Resource.ErrorMessage, 
                              Resource.ErrorMessageCanNotDellAccount,
                              Resource.MessageForRemovMustNotBeSignal,
                              Resource.MessageSignalCountNow,
                              signalCount)
                              : AccountUtils.Delete(id);
            }

     

            
            return Accounts(message);
        }

        /// <summary>
        /// Асинхронная проверка 'занятости' Email.
        /// </summary>
        /// <param name="userEmail">Email, который нужно проверить</param>
        [HttpPost]
        public ActionResult EmailCheckExistAjaxUpdate(string userEmail)
        {
            return Json(new { userEmailExistHTML = Utils.CheckExistValuesFromTable<PLATFORM_USER>(x => x.Email == userEmail) ?
                string.Format("Email {0}", Resource.TextIsAlreadyInUse) : "" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult LoginCheckExistAjaxUpdate(string login)
        {
            return Json(new { loginExistHTML = Utils.CheckExistValuesFromTable<PLATFORM_USER>(x => x.Login == login) ?
                string.Format("Login {0}", Resource.TextIsAlreadyInUse) : ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Пересчёт баланса счёта
        /// </summary>
        /// <param name="accountId">Уникальный идентификатор счёта</param>
        public ActionResult CorrectBalance(string accountId)
        {
            var acId = accountId.ToInt();
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var transfers = ctx.BALANCE_CHANGE.Where(x => x.AccountID == acId &&
                                                                  (x.ChangeType ==
                                                                   (int)BalanceChangeType.Profit ||
                                                                   x.ChangeType ==
                                                                   (int)BalanceChangeType.Loss ||
                                                                   x.ChangeType == (int)BalanceChangeType.Swap)).ToList();

                    foreach (var balanceChange in transfers)
                    {
                        ctx.BALANCE_CHANGE.Remove(balanceChange);
                    }

                    foreach (var position in ctx.POSITION_CLOSED.Where(x => x.AccountID == acId))
                    {
                        ctx.BALANCE_CHANGE.Add(new BALANCE_CHANGE
                        {
                            AccountID = accountId.ToInt(),
                            Amount = Math.Abs(position.ResultDepo),
                            ValueDate = position.TimeExit,
                            ChangeType = position.ResultDepo > 0
                                    ? (int)BalanceChangeType.Profit
                                    : (int)BalanceChangeType.Loss,
                            Description = String.Format("{0} #{1}", Resource.TextMarketOrderResult, position.ID)
                        });
                    }
                    ctx.SaveChanges();
                    var result = positionRepository.ReCalculateAccountBalance(ctx, acId)
                        ? string.Format("{0} ({1} #{2}).", Resource.MessageAccountTransactionsUpdated, Resource.TitleAccount, accountId)
                        : string.Format("{0} ({1} #{2}).",
                        Resource.MessageAccountTransactionsUpdatedWithoutBalance, Resource.TitleAccount, accountId);

                    return RedirectToAction("Accounts", new { message = result });
                }
            }
            catch (Exception ex)
            {
                Logger.Error("CorrectBalance()", ex);
            }

            var mes = string.Format("{0} {1}.", Resource.ErrorMessageAccountTransactionsUpdated, accountId);
            return RedirectToAction("Accounts", new { message = mes });
        }

        /// <summary>
        /// в объекте типа AccountViewModel заполняет актуальными данными свойства, относящие к постраничному разбиению и сортировке
        /// </summary>
        /// <param name="model">ссылка на объект модели. В этом объекте будем ооновлять значения свойств</param>
        /// <param name="pageAccountAction">идентификатор действия, которое было произведено пользователем</param>
        /// <param name="pageAccountArg">дополнительные данный (номер страници или порядок сортировки)</param>
        /// <returns></returns>
        private PagedList<AccountTag> FillAccountViewModel(AccountViewModel model, string pageAccountAction, string pageAccountArg)
        {
            model.SortColName = new List<KeyValuePair<string, int>>();
            FillSortColName(model);

            switch (pageAccountAction)
            {
                case "Paging":
                    model.PageNomber = String.IsNullOrEmpty(pageAccountArg) ? 0 : Convert.ToInt32(pageAccountArg);
                    break;
                case "Sort":
                    var direction = 1;
                    if (model.SortColName.Select(x => x.Key).Contains(pageAccountArg))
                        direction = -1 * model.SortColName.First(x => x.Key == pageAccountArg).Value;

                    model.SortColName.RemoveAll(x => x.Key == pageAccountArg);
                    model.SortColName.Add(new KeyValuePair<string, int>(pageAccountArg, direction));
                    break;
            }
            model.StrSortColName = String.Join(";", model.SortColName.Select(x => String.Format("{0}:{1}", x.Key, x.Value)));

            var pageItems = accountRepository.GetAllAccounts(model);
            return new PagedList<AccountTag>(pageItems, model.PageNomber, model.CurrentPageSize); //TODO непонятно, как это работает
        }

        /// <summary>
        /// заполняем свойство 'SortColName' из строки 'StrSortColName' 
        /// </summary>
        /// <param name="model">ссылка на модель представления, для того, что бы в ней можно было заполнить свойство 'SortColName'</param>
        /// <returns></returns>
        private static void FillSortColName(AccountViewModel model)
        {
            if (!string.IsNullOrEmpty(model.StrSortColName))
                foreach (var sortColName in model.StrSortColName.Split(';'))
                {
                    var strKeyValue = sortColName.Split(':');
                    // ReSharper disable RedundantAssignment
                    var direct = 1;
                    // ReSharper restore RedundantAssignment
                    Int32.TryParse(strKeyValue[1], out direct);
                    model.SortColName.Add(new KeyValuePair<string, int>(strKeyValue[0], direct));
                }
        }

        /// <summary>
        /// Заполнение выпадающих списков торговых инструментов, групп счетов, прав пользователей, ролей пользователей
        /// </summary>
        private void FillDropDownListForAccountView()
        {
            var data = Utils.GetDataToFillDropDownListForAccountView();

            ViewData["listTickers"] = data["listTickers"];
            ViewData["listGroups"] = data["listGroups"];
            ViewData["listUserRights"] = data["listUserRights"];
            ViewData["listUserRoles"] = data["listUserRoles"];
        }
   }
}
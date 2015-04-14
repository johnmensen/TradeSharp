using System;
using System.Collections.Generic;
using System.Data.Objects.SqlClient;
using System.Web.Mvc;
using System.Linq;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Models;
using TradeSharp.SiteAdmin.Models.CommonClass;
using TradeSharp.SiteAdmin.Models.Items;
using TradeSharp.Util;


namespace TradeSharp.SiteAdmin.Controllers
{
    public partial class TrustManagementController
    {
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

        private void FillTradeSignalDataCombobox()
        {
            var users = Utils.SelectAllValuesFromTableToList<PLATFORM_USER>(x => SqlFunctions.StringConvert((double?)x.ID).Trim() + "#" + x.Login);
            ViewData["listUserValue"] = users.Select(x => x.Split('#')[0]).ToList();
            ViewData["listUserText"] = users.Select(x => x.Split('#')[1]).ToList();

            // ReSharper disable SpecifyACultureInStringConversionExplicitly
            ViewData["serviceTypeValue"] = Enum.GetNames(typeof(PaidServiceType)).Select(x => ((int)Enum.Parse(typeof(PaidServiceType), x)).ToString()).ToList();
            ViewData["serviceTypeText"] = Enum.GetNames(typeof(PaidServiceType)).ToList();


            ViewData["listTickers"] = Utils.SelectAllValuesFromTableToList<COMMODITY>(x => x.Title);
            var usId = -1;
            if (users.Any() && int.TryParse(users.First().Split('#')[0], out usId))
                ViewData["listAccounts"] = accountRepository.GetAccountForUser(usId).Select(x => x.ID.ToString()).ToList();
            // ReSharper restore SpecifyACultureInStringConversionExplicitly
        }

        /// <summary>
        /// Вытаскиваем все портфели роботов для таблици из "AccountsPartialTable.cshtml"
        /// </summary>
        [HttpGet]
        public ActionResult TopPortfolios(string message)
        {
            var viewModel = topPortfolioRepository.GetAllTopPortfolio();
            if (!string.IsNullOrEmpty(message)) ResultMessage = message;
            return View("TopPortfolios", viewModel);
        }

        [HttpGet]
        public ActionResult AddTopPortfolio(string message, bool? createNewAccount)
        {
            FillDropDownListForAccountView();
            if (!string.IsNullOrEmpty(message))
                ResultMessage = message;

            var listHostAccount = accountRepository.GetAllAccounts();

            if (listHostAccount != null)
            {
                ViewBag.listHostAccount = listHostAccount;
                var topPortfolioItem = new TopPortfolioItem();
                if (createNewAccount.HasValue)
                    topPortfolioItem.CreateNewAccount = createNewAccount.Value;
                return View("TopPortfoliosAdd", topPortfolioItem);
            }
            return RedirectToAction("TopPortfolios", new { message = Resource.ErrorMessageUnableRetrieveServerListPortfolios });
        }
        
        [HttpPost]
        public ActionResult AddTopPortfolio(TopPortfolioItem topPortfolio, FormCollection formCollection)
        {
            if (topPortfolio.CreateNewAccount)
            {
                try
                {
                    topPortfolio.AddAccountModel = new AddAccountModel
                        {
                            AccountBalance = Convert.ToSingle(formCollection["AccountBalance"]),
                            AccountGroup = formCollection["AccountGroup"],
                            AccountCurrency = formCollection["AccountCurrency"],
                            AccountMaxLeverage = Convert.ToSingle(formCollection["AccountMaxLeverage"]),
                            UserLogin = formCollection["UserLogin"],
                            UserPassword = formCollection["UserPassword"],
                            UserName = formCollection["UserName"],
                            UserSurname = formCollection["UserSurname"],
                            UserPatronym = formCollection["UserPatronym"],
                            UserDescription = formCollection["UserDescription"],
                            UserEmail = formCollection["UserEmail"],
                            UserPhone1 = formCollection["UserPhone1"],
                            UserPhone2 = formCollection["UserPhone2"],
                            UserRightsMask =(UserAccountRights)Enum.Parse(typeof (UserAccountRights), formCollection["UserRightsMask"]),
                            UserRoleMask = (UserRole) Enum.Parse(typeof (UserRole), formCollection["UserRoleMask"]),
                            UserRegistrationDate = DateTime.Now,
                            WalletBalance = Convert.ToSingle(formCollection["WalletBalance"])
                        };
                }
                catch (Exception ex)
                {
                    Logger.Error("AddTopPortfolio() - Не валидно введённые данные.", ex);
                }
            }


            if (!ModelState.IsValid)
            {
                var errors = new List<string>();
                for (int i = 0; i < ModelState.Values.Count(); i++)
                {
                    var name = ModelState.Keys.ToList()[i];
                    errors.AddRange(ModelState.Values.ToList()[i].Errors.Select(x => name + ": " + x.ErrorMessage));
                }
                   
                return RedirectToAction("AddTopPortfolio", new { message = string.Join(Environment.NewLine, errors) });
            }
      
            var errorMessage = topPortfolio.CreateNewAccount ?
                                      topPortfolioRepository.SubscribeOnPortfolioOnNewAccount(topPortfolio) :
                                      topPortfolioRepository.SubscribeOnPortfolioOnExistAccount(topPortfolio);
            
           
            return RedirectToAction("TopPortfolios", new { message = errorMessage });
        }

        /// <summary>
        /// быстрое редактирование формуды и критери отсева для портфеля роботов
        /// </summary>
        /// <param name="strId">уникальный идентификатор портфеля</param>
        /// <param name="newCriteria">новое значение формулы</param>
        /// <param name="newMarginValue">новое значение критерия отсева</param>
        [HttpPost]
        public ActionResult FastEditCriteria(string strId, string newCriteria, string newMarginValue)
        {
            int id;
            if (!int.TryParse(strId, out id))
                return Json(new { status = false, errorString = String.Format("{0}:{1}", Resource.ErrorMessage, Resource.ErrorMessageIdMustBeInteger)}, JsonRequestBehavior.AllowGet);

            double? margValue = null;
            if (!string.IsNullOrEmpty(newMarginValue))
            {
                double marginValue;
                if (!double.TryParse(newMarginValue, out marginValue))
                    return Json(new { status = false, errorString = String.Format("{0}:{1}", Resource.ErrorMessage, Resource.ErrorMessageMustBeNumber)}, JsonRequestBehavior.AllowGet);

                margValue = marginValue;
            }

            if (!topPortfolioRepository.UpdateCriteria(id, newCriteria, margValue))//
                return Json(new { status = false, errorString = String.Format("{0}:{1}", Resource.ErrorMessage, Resource.ErrorMessageUnableUpdateRecordDB) }, JsonRequestBehavior.AllowGet);

// ReSharper disable SpecifyACultureInStringConversionExplicitly
            return Json(new { status = true, html = string.Format("{0} <span>{1}</span>", newCriteria, margValue.HasValue ? margValue.Value.ToString() : "") }, JsonRequestBehavior.AllowGet);
// ReSharper restore SpecifyACultureInStringConversionExplicitly
        }

        /// <summary>
        /// Формирует представление для редактирования портфеля роботов
        /// </summary>
        /// <param name="topPortfolioId">уникальный идентификатор портфеля</param>
        [HttpGet]
        public ActionResult TopPortfolioDetails(string topPortfolioId)
        {
            int id;
            if (!int.TryParse(topPortfolioId, out id))
                return RedirectToAction("TopPortfolioDetails", new { message = String.Format("{0}:{1}", Resource.ErrorMessage, Resource.ErrorMessageIdMustBeInteger) });

            var portfolio = topPortfolioRepository.GetTopPortfolio(id);
            if (portfolio == null)
                return RedirectToAction("TopPortfolioDetails", new { message = String.Format("{0}:{1}", Resource.ErrorMessage, Resource.ErrorMessageUnableGetDataFromServer) });

            var listHostAccount = accountRepository.GetAllAccounts();
            if (listHostAccount == null)
                return RedirectToAction("TopPortfolioDetails", new { message = String.Format("{0}:{1}", Resource.ErrorMessage, Resource.ErrorMessageUnableGetDataFromServer) });


            ViewBag.listHostAccount = listHostAccount;
            return View("TopPortfolioDetails", portfolio);
        }

        /// <summary>
        /// Редактирование портфеля роботов
        /// </summary>
        [HttpPost]
        public ActionResult TopPortfolioDetails(TopPortfolioItem topPortfolio)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("TopPortfolioDetails", new { message = String.Format("{0}:{1}", Resource.ErrorMessage, Resource.ErrorMessageInvalid) });

            var result = string.Format(topPortfolioRepository.SaveTopPortfolioChanges(topPortfolio) ?
                                      Resource.MessageEditingMade + " (" + Resource.TitleElement + "  {0})" :
                                      Resource.ErrorMessageUnableUpdateRecordDB + " (" + Resource.TitleElement + " {0})", 
                                      topPortfolio.Name + " / " + topPortfolio.ParticipantCount);

            return RedirectToAction("TopPortfolios", new { message = result });
        }

        /// <summary>
        /// Удаление портфеля роботов
        /// </summary>
        [HttpGet]
        public ActionResult DeleteTopPortfolio(string id)
        {
            var messageResult = Resource.MessagePortfolioDeleted;

            int remId;
            if (!int.TryParse(id, out remId))
                messageResult = string.Format("{0}: {1}", Resource.ErrorMessage, Resource.ErrorMessageIdMustBeInteger);

            if (!topPortfolioRepository.DeletePortfolio(remId))
                messageResult = string.Format("{0}: {1}", Resource.ErrorMessage, Resource.ErrorMessageUnableDellRecordDB);
                    //"Ошибка: не удалось удалить записи в базе данных. Возможно, не удалось удалить подписки пользователей этого портфеля.";

            return RedirectToAction("TopPortfolios", new {message = messageResult});
        }

        /// <summary>
        /// Отмена редактирования портфеля роботов
        /// </summary>
        [HttpGet]
        public ActionResult TopPortfolioActionCancel()
        {
            return RedirectToAction("TopPortfolios", new { message = Resource.MessagePortfolioEditCancel });
        }

        /// <summary>
        /// Асинхронная проверка "занятьсти" комбинации Имя - количество участников портфеля
        /// </summary>
        [HttpPost]
        public ActionResult NameTopPortfolioCheckExistAjaxUpdate(string name, string count)
        {
            int participantCount;
            if (!int.TryParse(count, out participantCount))
                return Json(new { nameTopPortfolioExistHTML = "", countParsError = Resource.ErrorMessageNumberParticipantsMustInteger }, JsonRequestBehavior.AllowGet);

            return Json(new {nameTopPortfolioExistHTML = 
                Utils.CheckExistValuesFromTable<TOP_PORTFOLIO>(x => x.Name == name && x.ParticipantCount == participantCount) ?
                Resource.ErrorMessageCombinationNameAndPortfolioNumberInDB : "",
                             countParsError = ""
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
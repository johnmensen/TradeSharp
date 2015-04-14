using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using MvcPaging;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Models;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Controllers
{
    public partial class TrustManagementController
    {
        [HttpGet]
        public ActionResult TradeSignal()
        {
            try
            {
                FillTradeSignalDataCombobox();
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var allItems = ctx.SERVICE.Select(x => new TradeSignalModel
                        {
                            ID = x.ID,
                            User = x.User,
                            Comment = x.Comment,
                            Currency = x.Currency,
                            AccountId = x.AccountId,
                            FixedPrice = x.FixedPrice,
                            ServiceType = x.ServiceType,
                            UserLogin = x.PLATFORM_USER.Login,
                            СountSubscriber = x.SUBSCRIPTION.Count
                        }).ToList();

                    var pageItems = new PagedList<TradeSignalModel>(allItems, 0, 10);
                    var model = new TradeSignalListModel
                        {
                            CurrentPageItems = pageItems,
                            CurrentPageSize = 10,
                            PageNomber = 0
                        };


                    if (Request.IsAjaxRequest())
                        return PartialView("TradeSignalPartialTable", model);
                    return View("TradeSignal", model);
                }
            }
            catch (Exception ex)
            {
                Logger.Info("PartialSpot - не удалось получить данные с сервера", ex);
                return null;
            }
        }

        [HttpPost]
        public ActionResult TradeSignal(TradeSignalListModel model, string pageUserAction, string pageUserArg)
        {
            FillTradeSignalDataCombobox();
            using (var ctx = DatabaseContext.Instance.Make())
            {
                var allItems = ctx.SERVICE.Select(x => new TradeSignalModel
                    {
                        ID = x.ID,
                        User = x.User,
                        Comment = x.Comment,
                        Currency = x.Currency,
                        AccountId = x.AccountId,
                        FixedPrice = x.FixedPrice,
                        ServiceType = x.ServiceType,
                        UserLogin = x.PLATFORM_USER.Login,
                        СountSubscriber = x.SUBSCRIPTION.Count
                    }).ToList();

                if (!string.IsNullOrEmpty(model.SignallerFilterText))
                    allItems = allItems.Where(x => x.UserLogin.Contains(model.SignallerFilterText.Trim())).ToList();

                switch (pageUserAction)
                {
                    case "Paging":
                        model.PageNomber = String.IsNullOrEmpty(pageUserArg) ? 0 : Convert.ToInt32(pageUserArg);
                        break;
                }


                var pageItems = new PagedList<TradeSignalModel>(allItems, model.PageNomber, model.CurrentPageSize);

                ModelState.Remove("PageNomber");

                var newModel = new TradeSignalListModel
                    {
                        CurrentPageItems = pageItems,
                        PageNomber = pageItems.PageIndex
                    };

                if (Request.IsAjaxRequest())
                    return PartialView("TradeSignalPartialTable", newModel);
                return View("TradeSignal", newModel);
            }
        }

        [HttpPost]
        public ActionResult AddNewTradeSignal(TradeSignalModel tradeSignalModel)
        {
            if (!ModelState.IsValid) return Json(new { status = false, errorString = Resource.ErrorMessageInvalid }, JsonRequestBehavior.AllowGet);
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var account = ctx.ACCOUNT.Single(x => x.ID == tradeSignalModel.AccountId);

                    ctx.SERVICE.Add(new SERVICE
                        {
                            User = tradeSignalModel.User,
                            Comment = tradeSignalModel.Comment,
                            Currency = account.Currency,
                            AccountId = account.ID,
                            FixedPrice = tradeSignalModel.FixedPrice,
                            ServiceType = tradeSignalModel.ServiceType
                        });

                    ctx.SaveChanges();
                    return Json(new {status = true}, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logger.Info("AddNewTradeSignal", ex);
                return Json(new { status = false, errorString = Resource.ErrorMessageUnableGetDataFromServer }, JsonRequestBehavior.AllowGet);
            }
            
        }

        [HttpPost]
        public ActionResult EditTradeSignal(TradeSignalModel tradeSignalModel)
        {
            if (!ModelState.IsValid) return Json(new { status = false, errorString = Resource.ErrorMessageInvalid }, JsonRequestBehavior.AllowGet);

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var account = ctx.ACCOUNT.Single(x => x.ID == tradeSignalModel.AccountId);
                    var itemEdit = ctx.SERVICE.Single(x => x.ID == tradeSignalModel.ID);

                    itemEdit.User = tradeSignalModel.User;
                    itemEdit.Comment = tradeSignalModel.Comment;
                    itemEdit.Currency = account.Currency;
                    itemEdit.AccountId = account.ID;
                    itemEdit.FixedPrice = tradeSignalModel.FixedPrice;
                    itemEdit.ServiceType = tradeSignalModel.ServiceType;

                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.Info("EditTradeSignal - не удалось получить данные с сервера", ex);
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DelTradeSignal(int itemId)
        {
            var result = new StringBuilder();
            try
            {
                foreach (var userLogin in userRepository.GetLoginSubscriber(itemId)) result.Append(tradeSignalRepository.UnSubscribe(userLogin, itemId)); // Отписывает всех подписчиков
                result.Append(tradeSignalRepository.DellSignal(itemId)); // удаляем сигнал из БД             
            }
            catch (Exception ex)
            {
                var message = Resource.ErrorMessageUnableUnsubscribeAandRemoveSignals;
                Logger.Info(result.ToString(), ex);
                return Json(new { status = false, errorString = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = true }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Асинхронное обновления списка счётов при изменении выбранного пользователя
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateAccountCombobox(string accId)
        {
            var accauntId = -1;
            return int.TryParse(accId, out accauntId) ?
                Json(new { status = true, accounts = accountRepository.GetAccountForUser(accauntId).Select(x => x.ID + " / " + x.Currency) }, JsonRequestBehavior.AllowGet) 
                : Json(new { status = false }, JsonRequestBehavior.AllowGet);
        }
    }
}

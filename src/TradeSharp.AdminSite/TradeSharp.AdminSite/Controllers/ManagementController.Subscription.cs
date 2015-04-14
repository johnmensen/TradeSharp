using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Contract.Entity;
using TradeSharp.SiteAdmin.Models;

namespace TradeSharp.SiteAdmin.Controllers
{
    public partial class ManagementController
    {
        public ActionResult Subscription(int userId)
        {
            FillSubscriptionDataCombobox(userId);
            var model = new SubscriptionListModel(userId);
            if (Request.IsAjaxRequest())
                return PartialView("SubscriptionPartialTable", model);
            return View("Subscription", model);
        }

        [HttpPost]
        public ActionResult AddSignal(ServiceTradeSignalModel item)
        {
            if (!ModelState.IsValid) return Json(new { status = false, errorString = Resource.ErrorMessageInvalid }, JsonRequestBehavior.AllowGet);

            var result = item.Id == -1 ? tradeSignalRepository.AddSignal(item) : tradeSignalRepository.EditSignal(item);
            
            return Json(String.IsNullOrEmpty(result)
                ? new { status = true, errorString = result }
                : new { status = false, errorString = result },
                JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DellSignal(int signalId)
        {
            if (!ModelState.IsValid)
                return Json(new { status = false, errorString = Resource.ErrorMessageInvalid }, JsonRequestBehavior.AllowGet);

            var result = new StringBuilder();

            foreach (var userLogin in userRepository.GetLoginSubscriber(signalId)) 
                result.Append(tradeSignalRepository.UnSubscribe(userLogin, signalId)); // Отписывает всех подписчиков

            result.Append(tradeSignalRepository.DellSignal(signalId)); // удаляем сигнал из БД
            
            return Json(String.IsNullOrEmpty(result.ToString())
                            ? new { status = true, errorString = result.ToString() }
                            : new { status = false, errorString = result.ToString() },
                        JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// Отписать пользователя от чих либо чужих сигналов
        /// </summary>
        [HttpPost]
        public ActionResult UnSubscription(int signalId, int userId)
        {
            if (!ModelState.IsValid) return Json(new { status = false, errorString = Resource.ErrorMessageInvalid }, JsonRequestBehavior.AllowGet);

            var usLogin = userRepository.GetUserInfoById(userId).UserLogin;

            var result = tradeSignalRepository.UnSubscribe(usLogin, signalId);
            return Json(String.IsNullOrEmpty(result)
                ? new { status = true, errorString = result }
                : new { status = false, errorString = result },
                JsonRequestBehavior.AllowGet);
        }
      
        private void FillSubscriptionDataCombobox(int userId)
        {
            ViewData["serviceTypeValue"] = Enum.GetNames(typeof(PaidServiceType)).Select(x => ((int)Enum.Parse(typeof(PaidServiceType), x)).ToString()).ToList();
            ViewData["serviceTypeText"] = Enum.GetNames(typeof(PaidServiceType)).ToList();

            var accounts = accountRepository.GetAccountForUser(userId);
            ViewData["listAccountsText"] = accounts.Select(x => x.ID.ToString() + "  -  " + x.Currency.ToString()).ToList();
            ViewData["listAccountsValue"] = accounts.Select(x => x.ID.ToString()).ToList();
        }   
    }
}

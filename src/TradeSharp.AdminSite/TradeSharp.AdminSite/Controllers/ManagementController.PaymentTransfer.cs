using System.Collections.Generic;
using System.Web.Mvc;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Linq;

namespace TradeSharp.SiteAdmin.Controllers
{
    public partial class ManagementController
    {
        [HttpGet]
        public ActionResult PaymentTransfer(string message)
        {
            if (!string.IsNullOrEmpty(message)) ResultMessage = message;
            var model = paymentTransferRepository.GetTransfers();
            return View(model);
        }

        [HttpGet]
        public ActionResult PaymentTransferDetails(string transferId)
        {
            if (!ModelState.IsValid) return RedirectToAction("PaymentTransfer", 
                new { message = string.Format("{0}: {1}", Resource.ErrorMessage, Resource.ErrorMessageInvalid)});

            int id;
            if (!int.TryParse(transferId, out id))
                return RedirectToAction("PaymentTransfer",
                    new { message = string.Format("{0}: {1}", Resource.ErrorMessage, Resource.ErrorMessageIdMustBeInteger)});

            var listUserPaySystem = new List<SelectListItem>();
            using (var ctx = DatabaseContext.Instance.Make())
            {
                foreach (var selectListItem in ctx.USER_PAYMENT_SYSTEM)
                {
                    var paymentSystemName = (PaymentSystem) selectListItem.SystemPayment;
                    var items = new SelectListItem
                        {
                            Text = string.Format("{0} {1}",selectListItem.PLATFORM_USER.Login,paymentSystemName),
// ReSharper disable SpecifyACultureInStringConversionExplicitly
                            Value = selectListItem.Id.ToString().Trim()
// ReSharper restore SpecifyACultureInStringConversionExplicitly
                        };
                    listUserPaySystem.Add(items);
                }
            }
            listUserPaySystem.Insert(0, new SelectListItem { Text = Resource.TitleSelectPaymentSystem, Value = "null" });
            ViewBag.listUserPaySystem = listUserPaySystem;

            var model = paymentTransferRepository.GetTransferById(id);
            return View(model);
        }

        /// <summary>
        /// Осуществить проводку платежа - сделать из неопознанного опознанный
        /// </summary>
        /// <param name="paymentSystemTransfer"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PaymentTransferDetails(PaymentSystemTransfer paymentSystemTransfer)
        {
            var mes = Resource.MessageTransferSuccessfullyRegistered;
            var result = TradeSharpServer.Instance.proxy.RegistrationUndefinedTransfer(paymentSystemTransfer.Id, paymentSystemTransfer.UserPaymentSys);
            if (!result) mes = Resource.MessageUnableRecoverPayment;
            return RedirectToAction("PaymentTransfer", new { message = mes });
        }

        /// <summary>
        /// Отмена редактирования платежа
        /// </summary>
        public ActionResult PaymentTransferCancel()
        {
            return RedirectToAction("PaymentTransfer", new { message = Resource.MessageEditingCanceled });
        }
    }
}

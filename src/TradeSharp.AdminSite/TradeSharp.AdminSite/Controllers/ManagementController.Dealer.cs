using System.Linq;
using System.Web.Mvc;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Models.Items;

namespace TradeSharp.SiteAdmin.Controllers
{
    public partial class ManagementController
    {
        [HttpGet]
        public ActionResult DealerDetails(string code)
        {
            var acc = DatabaseContext.Instance.Make().DEALER.FirstOrDefault(x => x.Code == code);

            if (acc != null)
                return Json(new
                {
                    code = acc.Code,
                    dealerEnabled = acc.DealerEnabled,
                    fileName = acc.FileName
                }, JsonRequestBehavior.AllowGet);
            return null;
        }

        [HttpGet]
        public ActionResult EditDealerDetails(string code)
        {
            using (var ctx = DatabaseContext.Instance.Make())
            {
                return View(LinqToEntity.DecorateDealerDescription(ctx.DEALER.FirstOrDefault(x => x.Code == code)));
            }
        }

        [HttpPost]
        public ActionResult EditDealerDetails(DealerDescription model)
        {
            var result = dealerRepository.SaveDealerChanges(model)
                ? string.Format("{0} #{1}", Resource.MessageAccountGroupSaveDealerInfo, model.Code)
                : string.Format("{0} #{1}.", Resource.ErrorMessageSaveDeallerInfo, model.Code);
            return RedirectToAction("AccountGroups", new { message = result });
        }

        /// <summary>
        /// Асинхронное обновление дилера группы счетов (при выборе из выпадающего списка)
        /// </summary>
        /// <param name="accountCode">Редактируемая група счетов</param>
        /// <param name="dealerCode">дилер</param>
        [HttpGet]
        public void EditAccountDealer(string accountCode, string dealerCode)
        {
            var accCode = accountCode.Split('-');
            if (accCode.Length < 2) return;

            var accountId = accCode[1];
            dealerRepository.UpdateDealerForAccountGroup(accountId, dealerCode);
        }

        /// <summary>
        /// Этот метод редактирует групу счетов, но только те свойства, которые относятся к диллеру
        /// </summary>
        /// <param name="code"></param>
        [HttpGet]
        public ActionResult EditDealerAccountGroups(string code)
        {
            return View(accountGroupsRepository.GetAccountGroups(code).FirstOrDefault());
        }

        /// <summary>
        /// Редактирование информации о дилере группы счетов
        /// </summary>
        /// <param name="model">ссылка на группу счетов</param>
        [HttpPost]
        public ActionResult EditDealerAccountGroups(AccountGroupItem model)
        {
            var result = dealerRepository.SaveChangesDealerFealdInAccountGroup(model) ?
                 string.Format("{0} {1} {2} {3}", 
                 Resource.MessageAccountGroupSaveDealerInfo, model.Dealer, Resource.MessageAccountGroupFor, model.Code) :
                 string.Format("{0} {1} {2}.", 
                 Resource.ErrorMessageSaveDeallerInfo, Resource.MessageAccountGroupFor, model.Code);
             return RedirectToAction("AccountGroups", new { message = result });
        }
    }
}

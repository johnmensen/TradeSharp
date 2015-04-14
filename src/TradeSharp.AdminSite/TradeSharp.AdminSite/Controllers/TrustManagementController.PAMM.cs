using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MvcPaging;
using TradeSharp.SiteAdmin.Models;
using TradeSharp.SiteAdmin.Models.Items;

namespace TradeSharp.SiteAdmin.Controllers
{
    public partial class TrustManagementController
    {
        [HttpGet]
        public ActionResult Pamm(string message)
        {
            var model = new PammModel();
            FillPammModel(model, true);
            if (Request.IsAjaxRequest())
                return PartialView("PammPartialTable", model);
            return View("Pamm", model);
        }

        [HttpPost]
        public ActionResult Pamm(PammModel model)
        {
            FillPammModel(model);
            if (Request.IsAjaxRequest())
                return PartialView("PammPartialTable", model);
            return View("Pamm", model);
        }

        [HttpGet]
        public ActionResult PammDetails(int id)
        {
            var model = pammRepository.GetPammById(id);
            return View("PammDetails", model);
        }

        [HttpPost]
        public ActionResult PammDetails(PammItem pammItem)
        {
            var model = new PammModel();
            FillPammModel(model);

            if (Request.IsAjaxRequest())
                return PartialView("PammPartialTable", model);
            return View("Pamm", model);
        }

        [HttpGet]
        public ActionResult PammHistory(int id)
        {
            var items = pammRepository.GetAccountHistory(id);
            var model = new PammHistoryModel(items);
            return View("PammHistory", model);
        }

        private void FillPammModel(PammModel model, bool voidItems = false)
        {
            if (model == null) model = new PammModel();
            if (model.PageNomber < 0) model.PageNomber = 0;
            if (!model.PageSizeItems.Contains(model.CurrentPageSize)) model.CurrentPageSize = model.PageSizeItems.First();
            
            var result = voidItems ? new List<PammItem>() : pammRepository.GetAllPamm(model.AnyInvestor);
            model.CurrentPageItems = (result == null || voidItems) ? new PagedList<PammItem>(new List<PammItem>(), 0, 1) :
                                                                     new PagedList<PammItem>(result, model.PageNomber, model.CurrentPageSize);
        }
    }
}
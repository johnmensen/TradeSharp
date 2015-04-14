using System;
using System.Web.Mvc;
using System.Linq;
using TradeSharp.SiteAdmin.Models.Report;

namespace TradeSharp.SiteAdmin.Controllers
{
    [Authorize]
    public class BrokerReportController : Controller
    {
        public ActionResult MakeBrokerReport(bool? realOnly)
        {
            var report = new ReportBroker(default(DateTime), false, realOnly ?? false);
            return View(report);
        }

        public ActionResult TraderRate(PerformerStatFilteredViewModel model)
        {
            if (model == null || !model.IsInitialized)
            {
                model = new PerformerStatFilteredViewModel {CountAccount = 25, IsInitialized = true};
            }

            var records = PerformerStatRecordStorage.GetRecords();
            string errorStr;
            model.FilterRecords(records, out errorStr);

            if (Request.IsAjaxRequest())
                return PartialView("TraderRatePartial", model);
            return View("TraderRate", model);
        }

        [HttpGet]
        public ActionResult EditFunction(string functionText)
        {
            functionText = Server.UrlDecode(functionText);
            var func = new SelectListItem();
            if (!string.IsNullOrEmpty(functionText))
            {
                var selectedFunc =
                    PerformerStatFilteredViewModel.FilterFunction.FirstOrDefault(f => f.Text == functionText);
                if (selectedFunc != null)                
                    func = selectedFunc;
            }

            return View(func);
        }

        [HttpGet]
        public ActionResult DeleteFunction(string functionText)
        {
            functionText = Server.UrlDecode(functionText);
            if (!string.IsNullOrEmpty(functionText))
                PerformerStatFilteredViewModel.DeleteFunction(functionText);
            // отправить обратно
            return RedirectToAction("TraderRate");
        }

        [HttpPost]
        public ActionResult EditFunction(SelectListItem functionWithComment)
        {
            if (functionWithComment == null) return View();
            PerformerStatFilteredViewModel.AddFunction(functionWithComment.Text, functionWithComment.Value);
            // перенаправить на страницу отчета
            return RedirectToAction("TraderRate");
        }

        public ActionResult CheckFunction(string functionText)
        {
            functionText = Server.UrlDecode(functionText);
            string error;
            PerformerStatFilteredViewModel.CheckFunction(functionText, out error);
            if (string.IsNullOrEmpty(error))
                error = "OK";
            return Content(error);
        }
    }
}

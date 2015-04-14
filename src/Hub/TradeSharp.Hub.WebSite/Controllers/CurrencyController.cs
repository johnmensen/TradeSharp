using System.Linq;
using System.Web.Mvc;
using TradeSharp.Hub.BL.Contract;
using TradeSharp.Hub.BL.Repository;
using TradeSharp.Hub.WebSite.Models;

namespace TradeSharp.Hub.WebSite.Controllers
{
    public partial class CurrencyController : Controller
    {
        private readonly ICurrencyRepository currencyRepository;

        public CurrencyController()
        {
            currencyRepository = new CurrencyRepository();
        }

        [HttpGet]
        public ActionResult Currency()
        {
            var model = GetModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult SortAndPage(PagerModel pager)
        {
            var model = GetModel(pager.SortBy, pager.SortAscending, pager.CurrentPageIndex, pager.PageSize);
            return PartialView(model);
        }

        [HttpGet]
        public ActionResult FillCurrencyEditForm(string code)
        {
            var сurrency = currencyRepository.GetCurrency(code);
            return View(сurrency);
        }

        [HttpPost]
        public ActionResult SaveCurrency(BL.Model.Currency currency)
        {
            string errorString;
            var status = ModelState.IsValid;

            if (!ModelState.IsValid)
            {
                errorString = "Ошибки заполнения данных: " + string.Join(", ",
                                          ViewData.ModelState.Values.Where(x => x.Errors.Count > 0).Select(v => string.Join(", ",
                                              v.Errors.Select(e => e.ErrorMessage))));
            }
            else
            {
                status = currencyRepository.AddOrUpdateCurrency(currency, out errorString);
            }
            
            return Json(new { status, errorString });
        }

        [HttpPost]
        public ActionResult DeleteCurrency(string code)
        {
            string errorString;
            var status = currencyRepository.DeleteCurrency(code, out errorString);        
            return Json(new { status, errorString });
        }  
    }
}

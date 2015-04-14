using System.Web.Mvc;
using TradeSharp.Hub.BL.Contract;
using TradeSharp.Hub.BL.Repository;
using TradeSharp.Hub.WebSite.Models;

namespace TradeSharp.Hub.WebSite.Controllers
{
    public partial class CustomerController : Controller
    {
        private readonly IServerInstanceRepository serverInstanceRepository;

        public CustomerController()
        {
            serverInstanceRepository = new ServerInstanceRepository();
        }
        
        [HttpGet]
        public ActionResult GetServerInstances()
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
    }
}

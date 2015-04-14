using System.Web.Mvc;
using TradeSharp.Hub.WebSite.Models;

namespace TradeSharp.Hub.WebSite.Controllers
{
    public class PagingController : Controller
    {
        public ActionResult PagerPartial(PagerModel model)
        {
            return View(model);
        }
    }
}

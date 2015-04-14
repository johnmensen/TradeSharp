using System.Web.Mvc;

namespace TradeSharp.Hub.WebSite.Controllers
{
    public class AdministrationController : Controller
    {
        [HttpGet]
        public ActionResult GetAdministratorPage()
        {
            return View();
        }
    }
}

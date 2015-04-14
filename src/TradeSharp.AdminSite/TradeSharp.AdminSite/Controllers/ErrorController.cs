using System;
using System.Web.Mvc;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.SiteAdmin.App_GlobalResources;

namespace TradeSharp.SiteAdmin.Controllers
{
    [Authorize]
    public class ErrorController : Controller
    {
        public ActionResult General(Exception exception)
        {
            return Content(string.Format("{0}: {1}", Resource.ErrorMessageCommon, exception), "text/plain");
        }

        public ActionResult Http404()
        {
            return Content(Resource.ErrorMessagePageNotFound, "text/plain");
        }

        public ActionResult Http403()
        {
            return Content(Resource.ErrorMessageAccessToPageDenied, "text/plain");
        }

        public ActionResult Http204(string description)
        {
            return Content(string.Format("{0} {1}", Resource.ErrorMessageUnableToFormDataModelInServer, description), "text/plain");
        }

        public ActionResult Http503(string description)
        {
            return Content(string.Format("{0} {1}", Resource.ErrorMessageServerNotHandleRequests, description), "text/plain");
        }
    }
}
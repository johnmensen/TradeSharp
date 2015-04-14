using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Contract.WebContract;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Controllers
{
    [Authorize]
    public class ServiceStateController : Controller
    {
        private static readonly string urlWatchSrv = AppConfig.GetStringParam("Env.WatchServiceUrl", "http://10.5.237.10:55061");
        private static readonly string userName = AppConfig.GetStringParam("Env.TradeSharpUser", "forexinvest\\asitaev");
        private static readonly string userPwrd = AppConfig.GetStringParam("Env.TradeSharpPwrd", "AndSit!qa");
        private const string QueryPtr = "/?formatquery=1";

        public ActionResult ListSystemServicesAjax()
        {
            List<TradeSharpServiceProcess> procList = null;

            // получить данные от сервиса WatchService
            try
            {
                string rawData;
                procList =
                    HttpParameter.DeserializeServerResponse(urlWatchSrv + QueryPtr, null, out rawData, userName, userPwrd)
                                 .Cast<TradeSharpServiceProcess>()
                                 .ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("ListSystemServicesAjax", ex);
                procList = new List<TradeSharpServiceProcess>();
            }

            return Json(new
            {
                Records = procList
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult StartStopServiceAjax(string name, bool start)
        {
            var shouldStart = start;
            if (string.IsNullOrEmpty(name)) 
                return Json(new
                {
                    result = Resource.ErrorMessage
                }, JsonRequestBehavior.AllowGet);

            // отправить запрос на останов сервиса
            var result = Resource.ErrorMessageServer;
            var request = new TradeSharpServiceStartStop
                {
                    ShouldStart = shouldStart,
                    SrvName = HttpUtility.UrlDecode(name.Trim())
                };
            try
            {
                string rawData;
                var procList =
                    HttpParameter.DeserializeServerResponse(urlWatchSrv + QueryPtr, new List<HttpParameter> { request }, 
                        out rawData, userName, userPwrd).Cast<ExecutionReport>().ToList();
                result = procList.Count > 0 ? (procList[0].IsOk ? "OK" : Resource.ErrorMessage + ": " + procList[0].Comment) :
                    rawData;
                if (string.IsNullOrEmpty(result)) result = Resource.ErrorMessageCommon;
            }
            catch (Exception ex)
            {
                Logger.Error("StartStopServiceAjax", ex);
            }
            return Json(new
            {
                result = result
            }, JsonRequestBehavior.AllowGet);
        }
    }
}

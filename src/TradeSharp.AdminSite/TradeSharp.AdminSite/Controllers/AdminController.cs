using System;
using System.Collections.Generic;
using System.Web.Mvc;
using TradeSharp.SiteAdmin.Contract;
using TradeSharp.SiteAdmin.Models;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private IServiceUnitStatusRepository unitStatusRepository;

        public AdminController(IServiceUnitStatusRepository unitStatusRepository)
        {
            this.unitStatusRepository = unitStatusRepository;
        }

        public ActionResult ServerUnit()
        {
            return View();
        }

        public ActionResult ServerUnitList()
        {
            var units = new ServerUnitInfoContext().ServerUnits;
            return PartialView("ServerUnitList", units);
        }

        public ActionResult SystemServices()
        {
            return View();
        }

        public ActionResult TerminalUsers()
        {
            return View();
        }
  
        public ActionResult UpdateManager()
        {
            return View();
        }

        public ActionResult UpdateManagerList()
        {
            var units = unitStatusRepository.GetServiceUnitStatuses();
            return PartialView("ServerUpdateList", units);
        }

        [HttpPost]
        public ActionResult StartOrStopServices(
            string[] serviceNames,
            string[] hostNames,
            bool shouldStartNotStop, bool shouldUpdate)
        {
            SharpCodeContract.Requires<ArgumentNullException>(serviceNames != null, "serviceNames = null");
            SharpCodeContract.Requires<ArgumentNullException>(hostNames != null, "hostNames = null");
            SharpCodeContract.Requires<ArgumentOutOfRangeException>(serviceNames.Length == hostNames.Length);

            var errorStrings = new List<string>();
            for (var i = 0; i < serviceNames.Length; i++)
            {
                var srv = serviceNames[i];
                var hostName = hostNames[i];

                string errorString;
                var isOk = shouldUpdate
                                ? unitStatusRepository.UpdateService(hostName, srv, out errorString)
                                : shouldStartNotStop
                                        ? unitStatusRepository.StartService(hostName, srv, out errorString)
                                        : unitStatusRepository.StopService(hostName, srv, out errorString);
                if (!isOk)
                    errorStrings.Add(errorString);
            }

            return Json(new
            {
                isOk = errorStrings.Count == 0,
                errorString = string.Join("\n", errorStrings)
            }, JsonRequestBehavior.AllowGet);
        }
    }
}

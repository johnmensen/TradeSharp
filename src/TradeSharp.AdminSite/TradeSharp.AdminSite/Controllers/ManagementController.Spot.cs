using System;
using System.Linq;
using System.Web.Mvc;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Models;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Controllers
{
    public partial class ManagementController
    {     
        public ActionResult Spot()
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var spotData = spotRepository.GetAllSpotSymbols(ctx);

                    SpotModel.CommodityList =
                        ctx.COMMODITY.Select(v => new SelectListItem {Text = v.Title, Value = v.Title}).ToList();

                    if (Request.IsAjaxRequest())
                        return PartialView("SpotPartialTable", spotData);
                    return View("Spot", spotData);
                }
            }
            catch (Exception ex)
            {
                Logger.Info("PartialSpot - не удалось получить данные с сервера", ex);
                return null;
            }
        }

        [HttpPost]
        public ActionResult AddNewSpot(SpotModel model)
        {
            if (!ModelState.IsValid) return Json(new { status = false, errorString = Resource.ErrorMessageInvalid }, JsonRequestBehavior.AllowGet);

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var result = spotRepository.AddNewSpotItem(ctx, model);
                    return String.IsNullOrEmpty(result) ?
                        Json(new { status = true }, JsonRequestBehavior.AllowGet) :
                        Json(new { status = false, errorString = Resource.ErrorMessage + ": " + result },
                        JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("AddNewSpot() error", ex);
                return Json(new { status = false, errorString = Resource.ErrorMessage + ": " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditSpot(SpotModel model)
        {
            if (!ModelState.IsValid) return Json(new { status = false, errorString = Resource.ErrorMessageInvalid }, JsonRequestBehavior.AllowGet);

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var result = spotRepository.EditSpotItem(ctx, model);
                    return String.IsNullOrEmpty(result) ? 
                        Json(new { status = true }, JsonRequestBehavior.AllowGet) :
                        Json(new { status = false, errorString = Resource.ErrorMessage + ": " + result },
                        JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("EditSpot() error", ex);
                return Json(new { status = false, errorString = Resource.ErrorMessage + ": " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DelSpot(string id)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var result = spotRepository.DeleteSpotItem(ctx, id);
                    return String.IsNullOrEmpty(result) ?
                        Json(new { status = true }, JsonRequestBehavior.AllowGet) :
                        Json(new { status = false, errorString = Resource.ErrorMessage + ": " + result },
                        JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("DelSpot() error", ex);
                return Json(new { status = false, errorString = Resource.ErrorMessage + ": " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}

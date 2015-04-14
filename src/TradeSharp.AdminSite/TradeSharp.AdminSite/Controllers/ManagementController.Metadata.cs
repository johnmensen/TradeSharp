using System;
using System.Web.Mvc;
using System.Linq;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Models.Items;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Controllers
{
    public partial class ManagementController
    {
        public ActionResult BaseActive()
        {
            return View("BaseActive");
        }

        public ActionResult ListBaseActive(string filter)
        {
            var lstActive = Models.CommonClass.BaseActive.GetBaseActiveList();
            if (!String.IsNullOrEmpty(filter))
            {
                var list = lstActive.Where(s => s.Title.StartsWith(filter, StringComparison.OrdinalIgnoreCase)).ToList();
                lstActive = list.Count > 0 ? list : lstActive;
            }
            return PartialView("BaseActiveList", lstActive);
        }

        public ActionResult EditActive(string title, string description, string code)
        {
            var currentErrorString = "";
            var currentStatus = true;
            
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var active = ctx.COMMODITY.FirstOrDefault(c => c.Title == title);
                    if (active == null)
                    {
                        currentErrorString = string.Format("{0} '{1}' {2}", Resource.TitleTradingInstrument, title, Resource.ErrorMessageNotFound);
                        currentStatus = false;                        
                    }
                    else
                    {
                        active.Description = description;
                        active.CodeFXI = code.ToIntSafe();
                        ctx.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                currentErrorString = ex.Message;
                currentStatus = false;
            }
            return Json(new
                    {
                        status = currentStatus,
                        errorString = currentErrorString
                    }, JsonRequestBehavior.AllowGet);
        }
    
        public ActionResult CreateActive(string title, string description, string code)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var active = ctx.COMMODITY.FirstOrDefault(c => c.Title == title);
                    if (active != null)
                    {
                        return Json(new
                        {
                            status = false,
                            errorString = string.Format("{0} '{1}' {2}", Resource.TitleTradingInstrument, title, Resource.TextAlreadyExists) 
                        }, JsonRequestBehavior.AllowGet);
                    }

                    active = new COMMODITY
                        {
                            Title = title,
                            Description = description, 
                            CodeFXI = code.ToIntSafe()
                        };
                    ctx.COMMODITY.Add(active);
                    ctx.SaveChanges();                    
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = false,
                    errorString = string.Format("{0}: {1}", Resource.ErrorMessageAdding, ex.Message)
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
                {
                    status = true,
                    errorString = ""
                }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// окно выбора - редактировать глобальные переменные терминала,
        /// настройки валют и валютных пар и т.д.
        /// </summary>
        public ActionResult MetadataManagement()
        {
            return View("MetadataManagement");
        }

        public ActionResult Metadata()
        {
            return View(DatabaseContext.Instance.Make().ENGINE_METADATA);
        }

        public ActionResult PartialMetadata()
        {
            return PartialView("MetadataPartialTable", DatabaseContext.Instance.Make().ENGINE_METADATA);
        }

        [HttpPost]
        public ActionResult ChangeMetadata(MetadataItem item)
        {
            if (!ModelState.IsValid) return Json(new { status = false, errorString = Resource.ErrorMessageInvalid }, JsonRequestBehavior.AllowGet);

            try
            {
                TradeSharpDictionary.Instance.proxy.AddOrReplaceMetadataItem(item.Category, item.Name,
                Converter.GetObjectFromString(item.DataType, item.Value));
            }
            catch (Exception ex)
            {
                Logger.Error("ChangeMetadata() - Ошибка редактирования БД. Не удалось изменить / добавить элемент " + item.Id, ex);
                return Json(new { status = false, errorString = string.Format("{0}: {1}", Resource.ErrorMessage, Resource.TextEditingDB) }, JsonRequestBehavior.AllowGet);
            }
            
            return Json(new { status = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DelMetadata(int id)
        {
            if (!ModelState.IsValid) return Json(new { status = false, errorString = Resource.ErrorMessageInvalid }, JsonRequestBehavior.AllowGet);

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var itemToDel = ctx.ENGINE_METADATA.FirstOrDefault(x => x.ID == id);
                    if (itemToDel == null) return Json(new { status = false, errorString = Resource.ErrorMessageDataNotFound }, JsonRequestBehavior.AllowGet);
                    ctx.ENGINE_METADATA.Remove(itemToDel);
                    ctx.SaveChanges();
                    return Json(new { status = true }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("DelMetadata() error", ex);
                return Json(new { status = false, errorString = Resource.ErrorMessage + ": " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
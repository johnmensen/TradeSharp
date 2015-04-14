using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.SiteAdmin.Models;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        [HttpPost]
        public ActionResult ChangeBalance(BalanceChangeRequest bc)
        {
            Logger.InfoFormat("Начинаем пополнять счёт {0}", bc.AccountId);
            var errors = new List<string>();
            if (bc.Amount <= 0)
                errors.Add(Resource.ErrorMessageAmountMustBePositive);
            var date = bc.ValueDate.ToDateTimeUniformSafe();
            if (!date.HasValue)
                errors.Add(string.Format("{0} {1}", Resource.ErrorMessageUnableParseDateTime, Resource.TextExampleCorrectFillDateTime));
            
            if (!string.IsNullOrEmpty(bc.Description) && bc.Description.Length > 60)
                bc.Description = bc.Description.Substring(0, 60);

            if (errors.Count > 0)
            {
                Logger.Error(string.Format("Не удалось пополнить счет {0}", bc.AccountId) + string.Join(", ", errors));
                return Json(new
                    {
                        status = false,
                        errorString = Resource.ErrorMessage + ": " + string.Join(", ", errors)
                    }, JsonRequestBehavior.AllowGet);}

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var bal = new BALANCE_CHANGE
                        {
                            AccountID = bc.AccountId,
                            Amount = (decimal)bc.Amount,
                            // ReSharper disable PossibleInvalidOperationException
                            ChangeType = (int)bc.ChangeType,
                            // ReSharper restore PossibleInvalidOperationException
                            ValueDate = date.Value,
                            Description = bc.Description
                        };
                    ctx.BALANCE_CHANGE.Add(bal);
                    var account = ctx.ACCOUNT.First(a => a.ID == bc.AccountId);
                    account.Balance +=
                        new BalanceChange
                            {
                                ChangeType = bc.ChangeType,
                                Amount = (decimal)bc.Amount,
                                CurrencyToDepoRate = 1
                            }.SignedAmountDepo;
                    ctx.SaveChanges();
                }

                Logger.InfoFormat("Счёт {0} пополнен", bc.AccountId);
                return Json(new
                {
                    status = true,
                    errorString = ""
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Не удалось пополнить счет {0}", bc.AccountId) + string.Join(", ", errors));
                return Json(new
                {
                    status = false,
                    errorString = Resource.ErrorMessage + ": " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
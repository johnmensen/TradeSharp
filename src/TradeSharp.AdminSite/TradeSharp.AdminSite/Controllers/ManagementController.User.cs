using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MvcPaging;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Contract.Entity;
using TradeSharp.SiteAdmin.Models;

namespace TradeSharp.SiteAdmin.Controllers
{
    public partial class ManagementController
    {
        [HttpGet]
        public ActionResult UserList(string message)
        {
            var allItems = userRepository.GetAllPlatformUser();
            var pageItems = new PagedList<AccountUserModel>(allItems, 0, 10);

            var model = new UserListModel
                {
                    CurrentPageItems = pageItems,
                    CurrentPageSize = 10,
                    PageNomber = 0
                };
                
            if (!string.IsNullOrEmpty(message)) ResultMessage = message;
           
            if (Request.IsAjaxRequest())
                return PartialView("UserListPartialTable", model);
            return View("UserList", model);
        }

        [HttpPost]
        public ActionResult UserList(UserListModel model, string pageUserAction, string pageUserArg)
        {
            var allItems = userRepository.GetAllPlatformUser();

            switch (pageUserAction)
            {
                case "Paging":
                    model.PageNomber = String.IsNullOrEmpty(pageUserArg) ? 0 : Convert.ToInt32(pageUserArg);
                    break;
            }
            var pageItems = new PagedList<AccountUserModel>(allItems, model == null ? 0 : model.PageNomber, model == null ? 10 : model.CurrentPageSize);

            ModelState.Remove("PageNomber");

            var newModel = new UserListModel
            {
                CurrentPageItems = pageItems,
                PageNomber = pageItems.PageIndex
            };

            if (Request.IsAjaxRequest())
                return PartialView("UserListPartialTable", newModel);
            return View("UserList", newModel);
        }

        [HttpGet]
        public ActionResult DeletePlatformUser(string userId)
        {
            int id;
            if (!int.TryParse(userId, out id))
                return RedirectToAction("UserList",
                                        new {
                                                message =String.Format("{0}: {1}", Resource.ErrorMessage, Resource.ErrorMessageIdMustBeInteger)
                                            });

            string resultMessage;
            var signalCount = userRepository.GetSignalCount(id);

            if (signalCount == null || signalCount > 0)
            {
                resultMessage = Resource.ErrorMessageDataAccess;
            }
            else
            {
                List<Account> accountsWoOwners;
                if (!userRepository.DeletePlatformUser(id, out accountsWoOwners))
                    return RedirectToAction("UserList",
                                            new {
                                                    message =String.Format("{0}: {1}", Resource.ErrorMessage,Resource.ErrorMessageUnableDellRecordDB)
                                                });

                resultMessage = Resource.MessageDellRecordDB;
                if (accountsWoOwners != null && accountsWoOwners.Count > 0)
                    resultMessage = Resource.MessageAccountsWithoutUsers + ": " +
                                    string.Join(Environment.NewLine, accountsWoOwners.Select(x => x.ID));
            }
            return RedirectToAction("UserList", new {message = resultMessage});
        }
    }
}

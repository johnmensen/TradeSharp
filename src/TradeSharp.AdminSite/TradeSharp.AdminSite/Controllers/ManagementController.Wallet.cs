using System.Web.Mvc;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Models;
using TradeSharp.SiteAdmin.Models.CommonClass;

namespace TradeSharp.SiteAdmin.Controllers
{
    public partial class ManagementController
    {
        [HttpGet]
        public ActionResult WalletDetails(int walletId)
        {
            var user = userRepository.GetUserInfoById(walletId);

            var model = new WalletModel
                {
                    UserLogin = user.UserLogin,
                    WalletBalance = user.WalletBalance,
                    WalletCurrency = user.WalletCurrency,
                    WalletId = walletId
                };

            ViewData["listTickers"] = Utils.SelectAllValuesFromTable<COMMODITY>(x => new SelectListItem {Text = x.Title, Value = x.Title});
            ViewData["listAccounts"] = accountRepository.GetAccountForUser(walletId);

            return View(model);
        }

        [HttpGet]
        public ActionResult WalletDetailsCancel()
        {
            return RedirectToAction("UserList", new { message = Resource.MessageEditPurseCanceled });
        }

        /// <summary>
        /// Перевод средств на счёт
        /// </summary>
        [HttpPost]
        public ActionResult AccountTransfer(WalletModel walletModel)
        {
            var updateRerult = walletRepository.UpdateBalance(walletModel.WalletId, walletModel.TransferVolume, true);

            var messageRerult = updateRerult
                                    ? string.Format("{2} {1}. {3} {0}.",
                                                    walletModel.TransferVolume, walletModel.WalletId,
                                                    Resource.MessageFundsTransferredToPurse, Resource.TextVolume)
                                    : string.Format("{0} : {1} {2}.",
                                        Resource.ErrorMessage, Resource.MessageFundsTransferredToPurse, walletModel.WalletId);
            if (updateRerult && walletModel.TransferToAccount)
            {
                var tratsferRerult = walletRepository.WalletTransfer(walletModel.WalletId, walletModel.TransferVolume,
                                                             walletModel.UserLogin, walletModel.AccountId, true);
                messageRerult += tratsferRerult
                                     ? string.Format("{0} {1} {2} {3}. {4} {5}",
                                                     Resource.MessageFundsTransferredToAccount, walletModel.AccountId, Resource.TextFromThePurse, walletModel.WalletId,
                                                     Resource.TextVolume, walletModel.TransferVolume)
                                     : string.Format(
                                         "{0} :  {1} #{2} {3} #{4}",
                                         Resource.ErrorMessage, Resource.MessageFundsTransferredToAccount, walletModel.AccountId,
                                         Resource.TextFromThePurse, walletModel.WalletId);

            }

            return RedirectToAction("UserList", new {message = messageRerult});
        }

        /// <summary>
        /// Перевод средств со счёта
        /// </summary>
        [HttpPost]
        public ActionResult TransferAccount(WalletModel walletModel)
        {
            var messageRerult = string.Empty;
            if (walletModel.TransferToWallet)
            {
                var tratsferRerult = walletRepository.WalletTransfer(
                    walletModel.WalletId, walletModel.TransferVolume,
                    walletModel.UserLogin, walletModel.AccountId, false);

                messageRerult = tratsferRerult
                                    ? string.Format("{3} {0} {4} {1}. {5} - {2}",
                                                    walletModel.AccountId, walletModel.WalletId, walletModel.TransferVolume,
                                                    Resource.MessageWithdrawFromAccount, Resource.TextToThePurse, Resource.TextVolume)
                                    : string.Format(
                                        "{2}: {3} {0} {4} {1}",
                                        walletModel.AccountId, walletModel.WalletId, Resource.ErrorMessage, Resource.MessageWithdrawFromAccount,
                                         Resource.TextToThePurse);
            }

            var updateRerult = walletRepository.UpdateBalance(walletModel.WalletId, walletModel.TransferVolume, false);
            messageRerult += updateRerult
                                 ? string.Format(" {2} #{0} - {1}",
                                                 walletModel.WalletId, walletModel.TransferVolume, Resource.MessageWithdrawFromWallet)
                                 : string.Format(
                                     " {0}: {2} #{1}", Resource.ErrorMessage, walletModel.WalletId, Resource.MessageWithdrawFromWallet);
            return RedirectToAction("UserList", new {message = messageRerult});
        }

        /// <summary>
        /// Смена валюты кошелька
        /// </summary>
        [HttpPost]
        public ActionResult ChangeCurrency(WalletModel walletModel)
        {
            var changeResult = walletRepository.ChangeCurrency(walletModel.WalletId, walletModel.WalletCurrency,
                                                       walletModel.RecalculationBalance);
            var messageRerult = changeResult
                                    ? string.Format("{2} #{0} - {1}.",
                                                    walletModel.WalletId, walletModel.WalletCurrency, Resource.TitleCurrencyPurse)
                                    : string.Format(
                                        "{1}:  {2} #{0}",
                                        walletModel.WalletCurrency, Resource.ErrorMessage, Resource.TitleCurrencyPurse);

            if (changeResult && walletModel.RecalculationBalance) messageRerult += string.Format(" {0}", Resource.MessageBalanceRecalculated);
            return RedirectToAction("UserList", new {message = messageRerult});
        }
    }
}

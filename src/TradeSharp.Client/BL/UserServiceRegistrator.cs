using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Entity;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace TradeSharp.Client.BL
{
    static class UserServiceRegistrator
    {
        private static IDialogBoxProvider dialogBoxProvider = new DialogBoxProvider();
        public static IDialogBoxProvider DialogBoxProvider
        {
            get { return dialogBoxProvider; }
            set { dialogBoxProvider = value; }
        }

        public static Account SelectTerminalUserAccount(bool realAccountsOnly)
        {
            // проверить наличие реального счета, если их несколько - дать выбор
            List<Account> accounts = null;
            AuthenticationResponse status;
            try
            {
                status = MainForm.serverProxyTrade.proxy.GetUserOwnedAccountsWithActualBalance(
                    AccountStatus.Instance.Login,
                    CurrentProtectedContext.Instance.MakeProtectedContext(),
                    false, out accounts);
            }
            catch (Exception ex)
            {
                Logger.Error("Error in BtnMakeSignalServiceClick().GetUserOwnedAccountsWithActualBalance", ex);
                status = AuthenticationResponse.ServerError;
            }
            if (status != AuthenticationResponse.OK)
            {
                dialogBoxProvider.ShowMessageBox(EnumFriendlyName<AuthenticationResponse>.GetString(status), "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            if (accounts == null || accounts.Count == 0)
            {
                dialogBoxProvider.ShowMessageBox("У вас нет открытых торговых счетов", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            var accountsReal = realAccountsOnly
                ? accounts.Where(a => DalAccountGroup.Instance.Groups.First(g => g.Code == a.Group).IsReal).ToList()
                : accounts;
            if (accountsReal.Count == 0)
            {
                dialogBoxProvider.ShowMessageBox("Среди ваших торговых счетов нет действительного (не виртуального) счета", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            // дать пользователю выбор среди счетов
            var accountSelected = accountsReal[0];
            if (accountsReal.Count > 1)
            {
                object selected;
                string inputText;
                if (!dialogBoxProvider.ShowComboDialog("Укажите торговый счет", accountsReal.Cast<object>().ToList(),
                                        out selected, out inputText, true))
                    return null;
                accountSelected = (Account)selected;
            }

            return accountSelected;
        }
    
        public static bool RegisterOrUpdateService(PaidService service)
        {
            // получить кошелек пользователя
            Wallet wallet;
            try
            {
                wallet =
                    TradeSharpWalletManager.Instance.proxy.GetUserWallet(
                        CurrentProtectedContext.Instance.MakeProtectedContext(),
                        AccountStatus.Instance.Login);
                if (wallet == null)
                    throw new Exception("Кошелек не заведен");
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("GetUserWallet({0}) error: {1}", AccountStatus.Instance.Login, ex);
                dialogBoxProvider.ShowMessageBox("Не удалось получить данные о кошельке", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // отправить на сервер запрос
            service.User = wallet.User;
            WalletError error;
            try
            {
                TradeSharpWalletManager.Instance.proxy.RegisterOrUpdateService(
                    CurrentProtectedContext.Instance.MakeProtectedContext(),
                    service, out error);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("TradeSharpWalletManager.RegisterOrUpdateService({0}) error: {1}", AccountStatus.Instance.Login, ex);
                error = WalletError.CommonError;
            }

            if (error == WalletError.OK)
            {
                dialogBoxProvider.ShowMessageBox("Сервис зарегистрирован в системе", "Подтверждение",
                                MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return true;
            }

            dialogBoxProvider.ShowMessageBox(EnumFriendlyName<WalletError>.GetString(error),
                "Ошибка регистрации сервера", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }
}

using System;
using System.Linq;
using TradeSharp.Client.BL;
using TradeSharp.Client.Forms;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Client
{
    public partial class MainForm
    {
        public void OpenNewAccountForm(object sender, EventArgs e)
        {
            var dlg = new ManageAccountForm(true);
            dlg.ShowDialog();
        }

        public bool Authenticate(string login, string password,
            out AuthenticationResponse response, out string authStatusString)
        {
            AccountStatus.Instance.connectionStatus = AccountConnectionStatus.NotConnected;
            AccountStatus.Instance.Login = login;
            try
            {
                var localTime = DateTime.Now.Ticks;
                var hash = CredentialsHash.MakeCredentialsHash(login, password, localTime);
                int sessionTag;
                response = serverProxyTrade.proxy.Authenticate(login, hash, terminalVersion,
                                                                UserSettings.Instance.TerminalId, localTime,
                                                                out sessionTag);
                // обновить контекст безопасности
                if (response == AuthenticationResponse.OK)
                    CurrentProtectedContext.Instance.OnAuthenticated(sessionTag);
                else
                    CurrentProtectedContext.Instance.OnAuthenticateFaulted();
            }
            catch (Exception ex)
            {
                response = AuthenticationResponse.ServerError;
                Logger.ErrorFormat("Ошибка аутентификации {0}", ex);
                authStatusString = EnumFriendlyName<AuthenticationResponse>.GetString(response);
                AccountStatus.Instance.connectionStatus = AccountConnectionStatus.ConnectionError;
                AccountStatus.Instance.isAuthorized = false;
                return false;
            }
            authStatusString = EnumFriendlyName<AuthenticationResponse>.GetString(response);

            if (new[] {AuthenticationResponse.AccountInactive, AuthenticationResponse.InvalidAccount,
                    AuthenticationResponse.ServerError, AuthenticationResponse.WrongPassword, AuthenticationResponse.NotAuthorized}.Contains(response))
            {
                AccountStatus.Instance.isAuthorized = false;
                AccountStatus.Instance.connectionStatus = AccountConnectionStatus.ConnectionError;
                return false;
            }
          
            // аутентификация успешна
            MainWindowTitle.Instance.UserTitle = login;
            AccountStatus.Instance.connectionStatus = AccountConnectionStatus.Connected;
            AccountStatus.Instance.isAuthorized = true;
            authStatusString = "Connected";
            // выполнить ряд действий, доступных после подключения
            OnAuthenticated();
            return true;
        }

        // ??? эта ф-ция вызывается только при автоматическом логине после запуска терминала
        public bool SelectAccount(int accountId)
        {
            var ctx = CurrentProtectedContext.Instance.MakeProtectedContext();
            if (ctx == null) return false;
            try
            {
                var rst = serverProxyTrade.proxy.SelectAccount(ctx, accountId);
                if (rst)
                    OnAccountSelected(accountId);
                return rst;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в SelectAccount", ex);
                return false;
            }
        }

        private void OnAccountSelected(int accountId)
        {
            AccountStatus.Instance.accountID = accountId;
            AccountStatus.Instance.isAccountSelected = true;
            UserSettings.Instance.LastAccounts = accountId.ToString();

            // подгрузить "недостающие" новости
            LoadNewsFromServer(accountId);
        }

        public void ReviveFactory()
        {
            var ctx = CurrentProtectedContext.Instance.MakeProtectedContext();
            if (ctx == null) return;
            try
            {
                serverProxyTrade.proxy.ReviveChannel(ctx, UserSettings.Instance.Login, AccountStatus.Instance.accountID, terminalVersion);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в ReviveFactory", ex);
            }
        }
    }
}
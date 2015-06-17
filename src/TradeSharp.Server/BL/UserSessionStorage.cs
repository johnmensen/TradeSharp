using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.Server.BL
{
    /// <summary>
    /// хранит информацию об активных пользовательских сеансах
    /// сеанс иденитфицируется логином пользователя, IP-адресом, уникальный ID торгового
    /// терминала
    /// </summary>
    class UserSessionStorage
    {
        private static UserSessionStorage instance;
        public static UserSessionStorage Instance
        {
            get { return instance ?? (instance = new UserSessionStorage()); }
        }

        private UserSessionStorage()
        {
        }

        /// <summary>
        /// terminalId - session
        /// </summary>
        private readonly Dictionary<long, UserSession>
            sessions = new Dictionary<long, UserSession>();
        private readonly ReaderWriterLock sessionLocker = new ReaderWriterLock();
        private const int SessionLockTimeout = 1000;

        public void ReviveChannel(ProtectedOperationContext ctx, ITradeSharpServerCallback callback, 
            string address, string login, int accountId, string terminalVersion)
        {
            int userId;
            string password;
            var response = CheckCredentials(login, out password, out userId);
            if (response != AuthenticationResponse.OK)
            {
                return;
            }

            try
            {
                sessionLocker.AcquireWriterLock(SessionLockTimeout);
            }
            catch (ApplicationException)
            {
                Logger.Error("ReviveChannel - unable to get writer lock to session storage");
                return;
            }
            try
            {
                UserSession session;
                sessions.TryGetValue(ctx.terminalId, out session);
                // сессия еще жива
                if (session != null)
                {
                    session.callback = callback;
                    session.accountId = accountId;
                    return;
                }
                // создать новую сессию
                session = new UserSession
                {
                    ip = address,
                    lastRequestClientTime = ctx.clientLocalTime,
                    login = login,
                    accountId = accountId,
                    loginTime = DateTime.Now,
                    sessionTag = (int)(DateTime.Now.Ticks / 3),
                    terminalId = ctx.terminalId,
                    callback = callback,
                    enabledAccounts = GetUserAccounts(userId),
                    terminalVersion = terminalVersion
                };
                sessions.Add(ctx.terminalId, session);
            }
            catch (Exception ex)
            {
                Logger.Error("ReviveChannel - error", ex);
            }
            finally
            {
                sessionLocker.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// вернуть тег сессии клиента, передаваемый затем в последующих запросах
        /// </summary>
        public AuthenticationResponse Authenticate(string login, string hashString, 
            string terminalVersion,
            long clientLocalTime, long terminalId, string address, ITradeSharpServerCallback callback,
            out int sessionTag)
        {
            sessionTag = 0;
            
            // проверка параметров
            if (string.IsNullOrEmpty(login)) return AuthenticationResponse.InvalidAccount;
            if (string.IsNullOrEmpty(hashString)) return AuthenticationResponse.WrongPassword;
            if (terminalId == 0)
            {
                Logger.Info("AuthenticationResponse usr (" + login + "): terminal Id is 0");
                return AuthenticationResponse.InvalidAccount;
            }
            if (clientLocalTime == 0)
            {
                Logger.Info("AuthenticationResponse usr (" + login + "): clientLocalTime is 0");
                return AuthenticationResponse.InvalidAccount;
            }

            // получить пользователя и его роль по логину
            string password;
            int userId;
            var response = CheckCredentials(login, out password, out userId);
            if (response != AuthenticationResponse.OK) return response;
            
            // проверить хеш из логина, пароля и локального времени клиента
            var userHash = CredentialsHash.MakeCredentialsHash(login, password, clientLocalTime);
            if (hashString != userHash) return AuthenticationResponse.WrongPassword;

            // проверить наличие сессии
            try
            {
                sessionLocker.AcquireWriterLock(SessionLockTimeout);
            }
            catch (ApplicationException)
            {
                Logger.Error("Authenticate - unable to get writer lock to session storage");
                return AuthenticationResponse.ServerError;
            }
            try
            {
                UserSession session;
                sessions.TryGetValue(terminalId, out session);
                // сессия еще жива
                if (session != null)
                {
                    session.lastRequestClientTime = clientLocalTime;
                    session.callback = callback;
                    sessionTag = session.sessionTag;
                    session.enabledAccounts = GetUserAccounts(userId);
                    return AuthenticationResponse.OK;
                }
                // создать новую сессию
                session = new UserSession
                              {
                                  ip = address,
                                  lastRequestClientTime = clientLocalTime,
                                  login = login,
                                  loginTime = DateTime.Now,
                                  sessionTag = (int) (DateTime.Now.Ticks/3),
                                  terminalId = terminalId,
                                  callback = callback,
                                  enabledAccounts = GetUserAccounts(userId),
                                  terminalVersion = terminalVersion,
                                  userId = userId
                              };
                sessions.Add(terminalId, session);
                sessionTag = session.sessionTag;
                return AuthenticationResponse.OK;
            }
            catch (Exception ex)
            {
                Logger.Error("UserSessionStorage - error in Authenticate", ex);
                return AuthenticationResponse.ServerError;
            }
            finally
            {
                sessionLocker.ReleaseWriterLock();
            }
        }

        public bool PermitUserOperation(ProtectedOperationContext ctx, bool isTradeOperation, bool checkAccountId)
        {
            return PermitUserOperation(ctx, null, null, null, isTradeOperation, checkAccountId);
        }

        public bool PermitUserOperation(ProtectedOperationContext ctx, ITradeSharpServerCallback callback, bool isTradeOperation, bool checkAccountId)
        {
            return PermitUserOperation(ctx, callback, null, null, isTradeOperation, checkAccountId);
        }

        /// <summary>
        /// сравнить переданные в запросе на торговую операцию параметры
        /// с переданным хэш-отпечатком, найти запись в хранилище сессий,
        /// обновить ее
        /// второй параметр опциональный
        /// </summary>
        public bool PermitUserOperation(ProtectedOperationContext ctx, 
            ITradeSharpServerCallback callback, int? accountId, string login, bool isTradeOperation, bool checkAccountId)
        {
            // отправитель - сервер?
            if (ctx == null)
            {
                Logger.ErrorFormat("PermitUserOperation - login is \"{0}\" - no context", login);
                return false;
            }
            if (ProtectedOperationContext.IsServerContext(ctx)) return true;

            var userHash = CredentialsHash.MakeOperationParamsHash(ctx.clientLocalTime, ctx.sessionTag, ctx.terminalId);
            if (ctx.hash != userHash)
            {
                Logger.InfoFormat("[{0: {1}] - hash is incorrect ({2}, correct hash: {3})",
                    login, accountId, ctx, userHash);
                return false;
            }
            // найти сессию
            try
            {
                sessionLocker.AcquireReaderLock(SessionLockTimeout);
            }
            catch (ApplicationException)
            {
                Logger.Error("PermitUserOperation read timout");
                return false;
            }
            try
            {
                UserSession session;
                sessions.TryGetValue(ctx.terminalId, out session);
                if (session == null) return false;
                if (!string.IsNullOrEmpty(login))
                    if (login != session.login)
                    {
                        Logger.InfoFormat("Session #{0}: login ({1}) != session login ({2})",
                            ctx.terminalId, login, session.login);
                        return false;
                    }
                
                // обновить сессию
                try
                {
                    sessionLocker.UpgradeToWriterLock(SessionLockTimeout);
                }
                catch (ApplicationException)
                {
                    Logger.Error("PermitUserOperation write timout");
                    return false;
                }
                if (accountId.HasValue || checkAccountId)
                {
                    var targetAccount = accountId ?? session.accountId;
                    var accountRights = session.enabledAccounts.FirstOrNull(a => a.a == targetAccount);
                    // дополнительно проверяется, есть ли права на торговую (или иную защищенную) операцию
                    var hasRights = accountRights.HasValue && (accountRights.Value.b || !isTradeOperation);
                    if (!hasRights)
                    {
                        Logger.InfoFormat("User {0} #{1} has insufficient rights (trade op: {2}, session: {3} / {4})",
                            login, targetAccount, isTradeOperation, ctx.terminalId,
                            string.Join(",", session.enabledAccounts.Select(a => string.Format("{0}:{1}",
                                a.a, a.b ? "en" : "dis"))));
                        //Logger.Error("PermitUserOperation: возврат false, session.enabledAccounts.Contains счет " +
                        //             session.accountId);
                        return false;
                    }
                }
                // прописать счет для сессии
                if (accountId.HasValue)
                {
                    session.accountId = accountId.Value;
                    Logger.Info("PermitUserOperation: Успешная смена счета " + accountId.Value);
                }
                session.lastRequestClientTime = ctx.clientLocalTime;
                if (callback != null) session.callback = callback;
                // доступ разрешен
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("PermitUserOperation error", ex);
                return false;
            }
            finally
            {
                sessionLocker.ReleaseLock();
            }
        }

        /// <summary>
        /// вернуть все сессии, потокобезопасный
        /// </summary>
        public List<UserSession> GetSessions()
        {
            try
            {
                sessionLocker.AcquireReaderLock(SessionLockTimeout);
            }
            catch (ApplicationException)
            {
                Logger.Error("GetSessions timeout");
                return new List<UserSession>();
            }
            try
            {
                var sessList = sessions.Values.Select(v => new UserSession(v)).ToList();
                return sessList;
            }
            finally
            {
                sessionLocker.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// вызывается, когда, после попытки отправки callback сессия не отвечает
        /// сессии удаляются из списка
        /// </summary>
        public void ExcludeStaleSessions(List<long> terminalIds)
        {
            try
            {
                sessionLocker.AcquireWriterLock(SessionLockTimeout);
            }
            catch (ApplicationException)
            {
                Logger.Error("ExcludeStaleSessions timeout");
            }
            try
            {
                foreach (var id in terminalIds) sessions.Remove(id);
            }
            finally
            {
                sessionLocker.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// клиент выполнил логаут
        /// </summary>
        public void Logout(ProtectedOperationContext ctx)
        {
            // проверить контекст
            var userHash = CredentialsHash.MakeOperationParamsHash(ctx.clientLocalTime, ctx.sessionTag, ctx.terminalId);
            if (ctx.hash != userHash) return;

            // найти сессию
            try
            {
                sessionLocker.AcquireWriterLock(SessionLockTimeout);
            }
            catch (ApplicationException)
            {
                Logger.Error("Logout read timout");
                return;
            }
            try
            {
                sessions.Remove(ctx.terminalId);                
            }
            catch (Exception ex)
            {
                Logger.Error("Logout error", ex);
                return;
            }
            finally
            {
                sessionLocker.ReleaseLock();
            }
        }

        /// <summary>
        /// отправить пользователю (по terminalId) некоторое сообщение
        /// </summary>
        public void SendMessageToUser(long terminalId, string message, ServiceMessageCategory cat)
        {
            try
            {
                sessionLocker.AcquireReaderLock(SessionLockTimeout);
            }
            catch (ApplicationException)
            {
                Logger.Error("SendMessageToUser read timout");                
            }
            try
            {
                UserSession session;
                sessions.TryGetValue(terminalId, out session);
                if (session == null) return;
                try
                {
                    sessionLocker.UpgradeToWriterLock(SessionLockTimeout);
                }
                catch (ApplicationException)
                {
                    return;
                }
                session.callback.ProcessServiceMessage(cat, message);
            }
            catch (Exception ex)
            {
                Logger.Error("SendMessageToUser error", ex);
                return;
            }
            finally
            {
                sessionLocker.ReleaseLock();
            }
        }

        private static AuthenticationResponse CheckCredentials(string login, out string password, out int userId)
        {
            password = string.Empty;
            userId = 0;
            using (var ctx = DatabaseContext.Instance.Make())
            {
                try
                {
                    var user = ctx.PLATFORM_USER.FirstOrDefault(ac => ac.Login == login);
                    if (user == null)
                    {
                        Logger.Info("CheckCredentials(" + login + "): user is not found");
                        return AuthenticationResponse.InvalidAccount;
                    }
                    password = user.Password;
                    userId = user.ID;
                    var accountRoles = ctx.PLATFORM_USER_ACCOUNT.FirstOrDefault(ac => ac.PlatformUser == user.ID);
                    if (accountRoles == null) return AuthenticationResponse.AccountInactive;
                    return AuthenticationResponse.OK;
                }
                catch (Exception ex)
                {
                    Logger.Error("Authenticate error", ex);
                    return AuthenticationResponse.ServerError;
                }
            }
        }
    
        private List<Cortege2<int, bool>> GetUserAccounts(int platformUserId)
        {
            var accounts = new List<Cortege2<int, bool>>();
            using (var ctx = DatabaseContext.Instance.Make())
            {
                try
                {
                    var accountRoles = (from ar in ctx.PLATFORM_USER_ACCOUNT
                                        where ar.PlatformUser == platformUserId
                                        select ar);
                    foreach (var acRole in accountRoles)
                    {
                        accounts.Add(new Cortege2<int, bool>(acRole.Account,
                            acRole.RightsMask != (int)AccountRights.Просмотр));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка в GetUserAccounts", ex);
                }
                return accounts;
            }
        }
    }    
}

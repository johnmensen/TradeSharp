using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Server.BL;
using TradeSharp.Util;

namespace TradeSharp.Server.Contract
{
    public partial class ManagerTrade
    {
        public AuthenticationResponse GetUserAccounts(string login, ProtectedOperationContext secCtx,
            out List<int> accounts, out List<AccountRights> roles)
        {
            accounts = new List<int>();
            roles = new List<AccountRights>();
            if (UserOperationRightsStorage.IsProtectedOperation(UserOperation.GetAccountDetail))
                if (!UserSessionStorage.Instance.PermitUserOperation(secCtx,
                    UserOperationRightsStorage.IsTradeOperation(UserOperation.GetAccountDetail), false))
                    return AuthenticationResponse.WrongPassword;
            
            using (var ctx = DatabaseContext.Instance.Make())
            {
                try
                {
                    var user = ctx.PLATFORM_USER.FirstOrDefault(ac => ac.Login == login);
                    if (user == null) return AuthenticationResponse.InvalidAccount;

                    var accountRoles = (from ar in ctx.PLATFORM_USER_ACCOUNT
                                        where ar.PlatformUser == user.ID
                                        select ar);
                    foreach (var acRole in accountRoles)
                    {
                        accounts.Add(acRole.Account);
                        roles.Add((AccountRights)acRole.RightsMask);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка в GetUserAccounts", ex);
                    return AuthenticationResponse.ServerError;
                }
                return AuthenticationResponse.OK;
            }
        }

        public AuthenticationResponse GetUserAccountsWithDetail(string login, ProtectedOperationContext secCtx,
            out List<Account> accounts)
        {
            accounts = new List<Account>();
            try
            {
                if (UserOperationRightsStorage.IsProtectedOperation(UserOperation.GetAccountDetail))
                    if (!UserSessionStorage.Instance.PermitUserOperation(secCtx,
                        UserOperationRightsStorage.IsTradeOperation(UserOperation.GetAccountDetail), false))
                        return AuthenticationResponse.WrongPassword;
            }
            catch (Exception ex)
            {
                Logger.Error("GetUserAccountsWithDetail - PermitUserOperation error", ex);
                return AuthenticationResponse.ServerError;
            }

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    try
                    {
                        var user = ctx.PLATFORM_USER.FirstOrDefault(ac => ac.Login == login);
                        if (user == null) return AuthenticationResponse.InvalidAccount;

                        accounts = (from ar in ctx.PLATFORM_USER_ACCOUNT
                                            join ac in ctx.ACCOUNT on ar.Account equals ac.ID
                                            where ar.PlatformUser == user.ID
                                            select ac).ToList().Select(LinqToEntity.DecorateAccount).ToList();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Ошибка в GetUserAccountsWithDetail", ex);
                        return AuthenticationResponse.ServerError;
                    }
                    return AuthenticationResponse.OK;
                }            
            }
            catch (Exception ex)
            {
                Logger.Error("GetUserAccountsWithDetail - error making DB context", ex);
                return AuthenticationResponse.ServerError;
            }
        }

        /// <summary>
        /// вернуть акаунты, которыми трейдер владеет!
        /// опционально - исключив демки
        /// по каждому акаунту посчитать средства и резерв. маржу
        /// </summary>
        public AuthenticationResponse GetUserOwnedAccountsWithActualBalance(string login, 
            ProtectedOperationContext secCtx, bool realOnly, out List<Account> accounts)
        {
            accounts = new List<Account>();
            try
            {
                if (UserOperationRightsStorage.IsProtectedOperation(UserOperation.GetAccountDetail))
                    if (!UserSessionStorage.Instance.PermitUserOperation(secCtx,
                        UserOperationRightsStorage.IsTradeOperation(UserOperation.GetAccountDetail), false))
                        return AuthenticationResponse.WrongPassword;
            }
            catch (Exception ex)
            {
                Logger.Error("GetUserOwnedAccountsWithActualBalance - PermitUserOperation error", ex);
                return AuthenticationResponse.ServerError;
            }

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    try
                    {
                        var user = ctx.PLATFORM_USER.FirstOrDefault(ac => ac.Login == login);
                        if (user == null) return AuthenticationResponse.InvalidAccount;

                        var accountRoles = (from ar in ctx.PLATFORM_USER_ACCOUNT
                                            where ar.PlatformUser == user.ID 
                                             && ar.RightsMask == (int) AccountRights.Управление                                             
                                            select ar);
                        foreach (var acRole in accountRoles)
                        {
                            var acId = acRole.Account;
                            var acc = realOnly 
                                ? ctx.ACCOUNT.FirstOrDefault(a => a.ID == acId && a.ACCOUNT_GROUP.IsReal)
                                : ctx.ACCOUNT.FirstOrDefault(a => a.ID == acId);
                            if (acc == null) continue;
                            
                            // заполнить данные о актуальном балансе и т.д. и т.п.
                            decimal equity, reservedMargin, exposure;
                            profitCalculator.CalculateAccountExposure(acId, out equity, out reservedMargin,
                                out exposure, TradeSharp.Contract.Util.BL.QuoteStorage.Instance.ReceiveAllData(), 
                                ManagerAccount.Instance, accountRepository.GetAccountGroup);
                            var accountDec = LinqToEntity.DecorateAccount(acc);
                            accountDec.Equity = equity;
                            accountDec.UsedMargin = reservedMargin;
                            accounts.Add(accountDec);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Ошибка в GetUserOwnedAccountsWithActualBalance", ex);
                        return AuthenticationResponse.ServerError;
                    }
                    return AuthenticationResponse.OK;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetUserOwnedAccountsWithActualBalance - error making DB context", ex);
                return AuthenticationResponse.ServerError;
            }
        }

        /// <summary>
        /// запомнить счет, с которым взаимодействует клиент
        /// </summary>
        public bool SelectAccount(ProtectedOperationContext ctx, int accountId)
        {
            return UserSessionStorage.Instance.PermitUserOperation(ctx, null, accountId, null, false, false);
        }

        public List<UserEvent> GetUserEvents(ProtectedOperationContext ctx, string userLogin,
            bool deleteReceivedEvents)
        {
            var evList = new List<UserEvent>();
            try
            {
                if (UserOperationRightsStorage.IsProtectedOperation(UserOperation.GetAccountDetail))
                    if (!UserSessionStorage.Instance.PermitUserOperation(ctx,
                        UserOperationRightsStorage.IsTradeOperation(UserOperation.GetAccountDetail), true))
                        return evList;
            }
            catch (Exception ex)
            {
                Logger.Error("GetUserEvents() - PermitUserOperation error", ex);
                return evList;
            }

            return UserEventStorage.Instance.GetUserEvents(userLogin);
        }

        public CreateReadonlyUserRequestStatus MakeOrDeleteReadonlyUser(ProtectedOperationContext secCtx,
            int accountId, bool makeNew, string pwrd, out PlatformUser user)
        {
            user = null;

            if (UserOperationRightsStorage.IsProtectedOperation(UserOperation.ChangeAccountSettings))
                if (!UserSessionStorage.Instance.PermitUserOperation(secCtx, 
                    UserOperationRightsStorage.IsTradeOperation(UserOperation.ChangeAccountSettings), false))
                    return CreateReadonlyUserRequestStatus.NotPermitted;

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var existingUserRecord = ctx.PLATFORM_USER_ACCOUNT.FirstOrDefault(ua => ua.Account == accountId &&
                                                                                            ua.RightsMask ==
                                                                                            (int) AccountRights.Просмотр);
                    if (!makeNew)
                    {
                        // удалить
                        if (existingUserRecord == null)
                            return CreateReadonlyUserRequestStatus.UserNotFound;
                        ctx.PLATFORM_USER_ACCOUNT.Remove(existingUserRecord);
                        // удалить самого пользователя платформы
                        var userRow = ctx.PLATFORM_USER.First(u => u.ID == existingUserRecord.PlatformUser);
                        ctx.PLATFORM_USER.Remove(userRow);
                        ctx.SaveChanges();
                        return CreateReadonlyUserRequestStatus.Success;                        
                    }
                    
                    // создать нового или редактировать старого
                    if (existingUserRecord != null)
                    {
                        var userRow = ctx.PLATFORM_USER.First(u => u.ID == existingUserRecord.PlatformUser);
                        user = LinqToEntity.DecoratePlatformUser(userRow);
                        userRow.Password = pwrd;
                        ctx.SaveChanges();
                        return CreateReadonlyUserRequestStatus.Success;
                    }

                    // таки создать нового
                    var newUser = new PLATFORM_USER
                        {
                            Title = "Read" + accountId,
                            Login = "Read" + accountId,
                            Password = pwrd,
                            RegistrationDate = DateTime.Now,
                            RoleMask = (int) UserRole.Trader
                        };
                    user = LinqToEntity.DecoratePlatformUser(newUser);
                    var usr = ctx.PLATFORM_USER.Add(newUser);
                    ctx.SaveChanges();
                    ctx.PLATFORM_USER_ACCOUNT.Add(new PLATFORM_USER_ACCOUNT
                        {
                            Account = accountId,
                            PlatformUser = usr.ID,
                            RightsMask = (int) AccountRights.Просмотр
                        });
                    ctx.SaveChanges();
                    return CreateReadonlyUserRequestStatus.Success;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в CreateReadonlyUserRequestStatus()", ex);
                return CreateReadonlyUserRequestStatus.CommonError;
            }
        }

        public RequestStatus QueryReadonlyUserForAccount(ProtectedOperationContext secCtx, int accountId,
                                                         out PlatformUser user)
        {
            user = null;

            if (UserOperationRightsStorage.IsProtectedOperation(UserOperation.GetAccountDetail))
                if (
                    !UserSessionStorage.Instance.PermitUserOperation(secCtx,
                                                                     UserOperationRightsStorage.IsTradeOperation(
                                                                         UserOperation.GetAccountDetail), false))
                    return RequestStatus.Unauthorized;

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var existingUserRecord = ctx.PLATFORM_USER_ACCOUNT.FirstOrDefault(ua => ua.Account == accountId &&
                                                                                            ua.RightsMask ==
                                                                                            (int)AccountRights.Просмотр);
                    if (existingUserRecord != null)
                        user = LinqToEntity.DecoratePlatformUser(ctx.PLATFORM_USER.First(u => u.ID == existingUserRecord.PlatformUser));
                    return RequestStatus.OK;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в QueryReadonlyUserForAccount()", ex);
                return RequestStatus.CommonError;
            }
        }

        public Account GetAccount(int accountId)
        {
            return accountRepository.GetAccount(accountId);
        }
    }
}
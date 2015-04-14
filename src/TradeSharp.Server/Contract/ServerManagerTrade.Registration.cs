using System;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Text;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Linq;
using TradeSharp.Server.BL;
using TradeSharp.Util;

namespace TradeSharp.Server.Contract
{
    public partial class ManagerTrade
    {
        public AuthenticationResponse ChangePassword(ProtectedOperationContext ctx, string login, string newPassword)
        {
            try
            {
                if (UserOperationRightsStorage.IsProtectedOperation(UserOperation.ChangeAccountSettings))
                    if (!UserSessionStorage.Instance.PermitUserOperation(ctx,
                        UserOperationRightsStorage.IsTradeOperation(UserOperation.ChangeAccountSettings), false))                
                        return AuthenticationResponse.NotAuthorized;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка в AuthenticationResponse({0}): {1}", login, ex);
                return AuthenticationResponse.ServerError;
            }
            using (var dbContext = DatabaseContext.Instance.Make())
            {
                try
                {
                    var user = dbContext.PLATFORM_USER.FirstOrDefault(u => u.Login == login);
                    if (user == null) return AuthenticationResponse.InvalidAccount;
                    user.Password = newPassword;
                    dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Ошибка в ChangePassword({0}) : {1}", login, ex);
                    return AuthenticationResponse.ServerError;
                }
            }
            return AuthenticationResponse.OK;
        }

        

        public AuthenticationResponse GetUserDetail(string login, string password, out PlatformUser userDecorated)
        {
            userDecorated = null;
            using (var dbContext = DatabaseContext.Instance.Make())
            {
                try
                {
                    var user = dbContext.PLATFORM_USER.FirstOrDefault(u => u.Login == login);
                    if (user == null) return AuthenticationResponse.InvalidAccount;
                    if (user.Password != password) return AuthenticationResponse.WrongPassword;
                    userDecorated = LinqToEntity.DecoratePlatformUser(user);
                    return AuthenticationResponse.OK;
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Ошибка в GetUserDetail({0}) : {1}", login, ex);
                    return AuthenticationResponse.ServerError;
                }
            }
        }

        public AuthenticationResponse ModifyUserAndAccount(string oldLogin, string oldPassword, PlatformUser userNew, 
            int? accountId, float accountMaxLeverage, out bool loginIsBusy)
        {
            loginIsBusy = false;

            using (var dbContext = DatabaseContext.Instance.Make())
            {
                try
                {
                    // получить пользователя и проверить логин/пароль
                    var user = dbContext.PLATFORM_USER.FirstOrDefault(u => u.Login == oldLogin);
                    if (user == null) return AuthenticationResponse.InvalidAccount;
                    if (user.Password != oldPassword) return AuthenticationResponse.WrongPassword;

                    if (userNew.Login != user.Login)
                    {// проверить доступность нового login-а
                        if (dbContext.PLATFORM_USER.Any(u => u.Login == userNew.Login))
                        {
                            loginIsBusy = true;
                            return AuthenticationResponse.NotAuthorized;
                        }
                    }

                    // обновить данные пользователя
                    user.Login = userNew.Login;
                    user.Name = userNew.Name;
                    user.Password = userNew.Password;
                    user.Patronym = userNew.Patronym;
                    user.Phone1 = userNew.Phone1;
                    user.Phone2 = userNew.Phone2;
                    user.Title = string.IsNullOrEmpty(userNew.Title) ? user.Title : userNew.Title;
                    user.Description = userNew.Description;


                    
                    // обновить счет (макс плечо)
                    if (accountId.HasValue)
                    {
                        var acc = dbContext.ACCOUNT.FirstOrDefault(a => a.ID == accountId.Value);
                        if (acc != null && ((int)(acc.MaxLeverage * 10) != (int)(accountMaxLeverage * 10)))
                        {
                            acc.MaxLeverage = (decimal) accountMaxLeverage;
                        }
                    }
                    dbContext.SaveChanges();
                    
                    return AuthenticationResponse.OK;
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Ошибка в ModifyUserAndAccount({0}) : {1}", oldLogin, ex);
                    return AuthenticationResponse.ServerError;
                }
            }
        }
    }    
}

/*
CREATE TABLE ACCOUNT
(
    ID INT PRIMARY KEY NOT NULL IDENTITY(1,1),
    Currency VARCHAR(6) NOT NULL REFERENCES COMMODITY(Title),
     Результат без учета открытых позиций 
	Balance DECIMAL(16,2) NOT NULL,
    UsedMargin DECIMAL(16,2) NOT NULL,
    AccountGroup VARCHAR(10) NOT NULL REFERENCES ACCOUNT_GROUP(Code),
    Description NVARCHAR(80) NULL,
	 Макс. плечо, при котором возможно открыть новую сделку
	  (более жесткая проверка, в сравнении с проверкой маржи) 
	MaxLeverage DECIMAL(8, 2) NOT NULL
)
*/

/*
CREATE TABLE PLATFORM_USER
(
	ID INT PRIMARY KEY NOT NULL IDENTITY(1,1),
	Title VARCHAR(50) NOT NULL,
	Name VARCHAR(50) NULL,
	Surname VARCHAR(50) NULL,
	Patronym VARCHAR(50) NULL,
	Description VARCHAR(50) NULL,
	Email VARCHAR(50) NULL,
	Phone1 VARCHAR(25) NULL,
	Phone2 VARCHAR(25) NULL,
	Login VARCHAR(25) NOT NULL,
	Password VARCHAR(25) NOT NULL,
	RoleMask INT NOT NULL
)
GO*/
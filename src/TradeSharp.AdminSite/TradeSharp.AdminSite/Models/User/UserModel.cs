using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using Localisation = TradeSharp.SiteAdmin.BL.Localisation;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Models.User
{
    public class UserModel
    {
        [Required(ErrorMessageResourceName = "LoginErrorMessage", ErrorMessageResourceType = typeof(Resource))]
        [Localisation.LocalizedDisplayName("Login")]
        public string Login { get; set; }

        [Required(ErrorMessageResourceName = "ErrorMessagePassword", ErrorMessageResourceType = typeof(Resource))]
        [DataType(DataType.Password)]
        [Localisation.LocalizedDisplayName("Password")]
        public string Password { get; set; }

        public bool CheckCredentialsOnServer(out string errorString)
        {
            errorString = string.Empty;
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var usr = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == Login &&
                                                                    u.Password == Password);
                    if (usr == null)
                    {
                        errorString = Resource.ErrorMessageUserNotFind;
                        return false;
                    }

                    if (((usr.RoleMask & (int) UserRole.Manager) == 0) &&
                        ((usr.RoleMask & (int) UserRole.Administrator) == 0))
                    {
                        errorString = Resource.ErrorMessageUserRights;
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в CheckCredentialsOnServer()", ex);
                errorString = Resource.ErrorMessageServer;
                return false;
            }
        }
    }
}
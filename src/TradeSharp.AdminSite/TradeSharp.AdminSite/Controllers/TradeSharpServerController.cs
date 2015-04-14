using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using TradeSharp.Contract.WebContract;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Controllers
{
    [Authorize]
    public class TradeSharpServerController : Controller
    {
        private static readonly string urlSrv = AppConfig.GetStringParam("Env.TradeSharpServiceUrl", "http://10.5.237.10:8061");
        private static readonly string userName = AppConfig.GetStringParam("Env.TradeSharpUser", "forexinvest\\asitaev");
        private static readonly string userPwrd = AppConfig.GetStringParam("Env.TradeSharpPwrd", "AndSit!qa");
        private static readonly string geoCoordsDefault = AppConfig.GetStringParam("DefaultGeoCoords", "56.86, 35.908");
        private static readonly string clientTerminalIp = AppConfig.GetStringParam("ClientTerminalIP", "192.168.0.1");
        private const string QueryPtr = "/?current_session=1";

        private static readonly Regex regRemovePort = new Regex("^[^:]+");

        public ActionResult ListTradeSharpUsersAjax()
        {
            List<TerminalUser> userList;
            var coordsArray = geoCoordsDefault.ToDoubleArrayUniform();
            var defaultLat = coordsArray.Length == 2 ? coordsArray[0] : 56.86;
            var defaultLon = coordsArray.Length == 2 ? coordsArray[1] : 35.908;

            // получить данные из БД
            var registredUsers = GetRegistrationData();

            // получить данные от сервиса WatchService
            try
            {
                string rawData;
                userList =
                    HttpParameter.DeserializeServerResponse(urlSrv + QueryPtr, null, out rawData, userName, userPwrd)
                                 .Cast<TerminalUser>()
                                 .ToList();
                userList.ForEach(u => u.IP = regRemovePort.Match(u.IP).Value);
            }
            catch (Exception ex)
            {
                Logger.Error("ListTradeSharpUsersAjax() error", ex);
                userList = new List<TerminalUser>();
            }

            return Json(new
            {
                Records = userList,
                RegistredUsers = registredUsers,
                defaultLatitude = defaultLat,
                defaultLongitude = defaultLon
            }, JsonRequestBehavior.AllowGet);
        }

        private List<TerminalUser> GetRegistrationData()
        {

            // получить данные о зарегистрированных пользователях
            var users = new List<TerminalUser>();
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    foreach (var user in ctx.PLATFORM_USER)
                    {
                        users.Add(new TerminalUser
                            {
                                Account = 0,
                                IP = clientTerminalIp,
                                Login = user.Login
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetRegistrationData() error", ex);
            }

            return users;
        }
    }
}

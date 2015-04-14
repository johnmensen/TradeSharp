using System.Web.Mvc;
using System.Web.Security;
using TradeSharp.SiteAdmin.Models.User;

namespace TradeSharp.SiteAdmin.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(UserModel user, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                string errorStr;
                if (user.CheckCredentialsOnServer(out errorStr))
                //if (FormsAuthentication.Authenticate(user.Login, user.Password))
                {
                    FormsAuthentication.SetAuthCookie(user.Login, true);
                    return Redirect(returnUrl ?? Url.Action("ServerUnit", "Admin"));
                }
                else
                {
                    ModelState.AddModelError("", errorStr);
                }
            }
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        public ActionResult Close()
        {
            return View();
        }
    }
}

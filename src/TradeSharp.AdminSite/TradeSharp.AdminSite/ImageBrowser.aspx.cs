using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.SiteAdmin.Contract;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin
{
    // .ashx
    public partial class ImageBrowser : System.Web.UI.Page
    {
        private IUserRepository userRepository;
        private IAccountStatistics dataSource;
        private readonly string[] avatarSize = new [] {"big", "small"};
        
        protected void Page_Load(object sender, EventArgs e)
        {
            userRepository = DependencyResolver.Current.GetService<IUserRepository>();
            Response.ContentType = "text/plain";
            int accountId;
            if (Request["accountId"] == null || !int.TryParse(Request["accountId"], out accountId))
            {
                Logger.Error("Не корректно задан парамер accountId");
                Response.BinaryWrite(GetErrorAvatar());
                return;
            }

            if (!avatarSize.Contains(Request["size"].ToLower()))
            {
                Logger.Error("Не корректно задан парамер size");
                Response.BinaryWrite(GetErrorAvatar());
                return;
            }

            var user = userRepository.GetAccountOwner(accountId);
            if (user == null)
            {
                Logger.Error("Не найден пользователь-владелец счёта " + accountId);
                Response.BinaryWrite(GetErrorAvatar());
                return;
            }
            
            dataSource = TradeSharpAccountStatistics.Instance.proxy;

            var userInfo = dataSource.GetUsersBriefInfo(new List<int> { user.ID }).FirstOrDefault();
            if (userInfo == null)
            {
                Logger.Error("Не удалось получить подробную информацию о пользователе " + user.Login);
                Response.BinaryWrite(GetErrorAvatar());
                return;
            }

            byte[] avatar;
            switch (Request["size"].ToLower())
            {
                case  "big":
                    avatar = dataSource.ReadFiles(new List<string> { userInfo.AvatarBigFileName }).FirstOrDefault();
                    break;
                case "small":
                    avatar = dataSource.ReadFiles(new List<string> { userInfo.AvatarSmallFileName }).FirstOrDefault();
                    break;
                default:
                    avatar = dataSource.ReadFiles(new List<string> { userInfo.AvatarBigFileName }).FirstOrDefault();
                    break;
            }
            if (avatar == null)
            {
                Logger.Error("Не удалось получить файл");
                Response.BinaryWrite(GetErrorAvatar());
                return;
            }

            Response.BinaryWrite(avatar);
        }

        private byte[] GetErrorAvatar() 
        {
            using (var ms = new MemoryStream())
            {

                var image = System.Drawing.Image.FromFile(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "")) + "\\images\\shared\\logo-znak.png");
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }
    }
}
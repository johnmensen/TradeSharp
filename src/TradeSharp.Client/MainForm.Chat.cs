using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Chat.Client.BL;
using TradeSharp.Chat.Client.Control;
using TradeSharp.Chat.Contract;
using TradeSharp.Client.BL;
using TradeSharp.Client.Forms;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace TradeSharp.Client
{
    public partial class MainForm
    {
        private void SetupChat()
        {
            ChatControl.SetupChatEnvironment(GetPlatformUsers, true);
            ChatEngine.MessageReceived += ChatEngineOnMessageReceived;
        }

        /// <summary>
        /// если сообщение пришло в чат, а чат открыт в какой-то вкладке, неактивной - 
        /// - моргнуть этой вкладкой
        /// </summary>
        private void ChatEngineOnMessageReceived(Message message)
        {
            var wndSets = nonChartWindows.FirstOrDefault(w => w.Window == NonChartWindowSettings.WindowCode.Chat);
            if (wndSets == null) return;

            var isChildVisible = MdiChildren.Any(c => c is ChatForm && c.Visible);
            if (!isChildVisible) // заморгать вкладкой
            try
            {
                Invoke(new Action<long>(l => bookmarkStrip.BlinkBookmark(l)), wndSets.ChartTab);
            }
            catch (Exception ex)
            {
                Logger.Error("ChatEngineOnMessageReceived()", ex);
            }
        }

        private void EnterChat()
        {
            var user = AllUsers.Instance.GetAllUsers().Find(u => u.Login == AccountStatus.Instance.Login);
            if (user == null)
                return;
            ChatSettings.Instance.Id = user.ID;
            if (ChatSettings.Instance.AutoLogin)
                ChatEngine.Login(user.ID);
        }

        private static List<PlatformUser> GetPlatformUsers()
        {
            try
            {
                return TradeSharpDictionary.Instance.proxy.GetAllPlatformUsers();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в GetPlatformUsers()", ex);
            }
            return new List<PlatformUser>();
        }
    }
}

using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using TradeSharp.Chat.Client.BL;
using TradeSharp.Chat.Client.Control;
using TradeSharp.Chat.StandAloneClient.Model;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Chat.StandAloneClient
{
    public partial class ChatForm : Form
    {
        private string[] arguments;

        private bool askForLogin;

        public ChatForm(string[] args)
        {
            InitializeComponent();
            arguments = args;
            ChatControl.SetupChatEnvironment(GetUsersFromDB, false);
        }

        private void ChatFormShown(object sender, EventArgs e)
        {
            chatControl1.Start(0, arguments.ToList());
            chatControl1.Engine.Entered +=
                () => Invoke(new Action(() =>
                    {
                        Text =
                            Text.Split(new[] {' '})[0] + " - " +
                            AllUsers.Instance.GetUser(chatControl1.Engine.CurrentUserId).NickName;
                    }));
            chatControl1.LoginRequested += () =>
                {
                    if (ChatSettings.Instance.Id == 0 || askForLogin)
                    {
                        chatControl1.Login();
                        ChatSettings.Instance.Id = chatControl1.Engine.CurrentUserId;
                    }
                    else
                        chatControl1.Login(ChatSettings.Instance.Id);
                };
            chatControl1.Engine.Exited += () => askForLogin = true;
        }

        private void ChatFormFormClosed(object sender, FormClosedEventArgs e)
        {
            chatControl1.Stop();
        }

        private List<PlatformUser> GetUsersFromDB()
        {
            using (var cxt = new MTS_LIVEEntities())
            {
                try
                {
                    return cxt.PLATFORM_USER.Select(x =>
                                                    new PlatformUser
                                                        {
                                                            ID = x.ID,
                                                            Title = x.Title,
                                                            Name = x.Name,
                                                            Surname = x.Surname,
                                                            Patronym = x.Patronym,
                                                            Description = x.Description,
                                                            Login = x.Login,
                                                            RoleMask = (UserRole)x.RoleMask
                                                        }
                        ).ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    //Logger.Error("Ошибка получения списка пользователей из БД", ex);
                    return new List<PlatformUser>();
                }
            }
        }
    }
}

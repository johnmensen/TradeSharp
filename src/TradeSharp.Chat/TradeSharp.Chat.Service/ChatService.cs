using System;
using System.ServiceModel;
using System.ServiceProcess;
using TradeSharp.Chat.Server.BL;
using TradeSharp.Chat.Service.BL;
using TradeSharp.Util;

namespace TradeSharp.Chat.Service
{
    public partial class ChatService : ServiceBase
    {
        private ChatReceiver chat;

        private ServiceHost serviceHostChat;

        private WebChatReceiver webChat;

        private ServiceHost serviceHostWebChat;

        private ServiceHost serviceHostStatus;
       
        public ChatService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Logger.Info("Starting ChatReceiver...");
            try
            {
                chat = new ChatReceiver();
                Logger.Info("ChatReceiver started");
            }
            catch (Exception ex)
            {
                Logger.Error("OnStart: error in ChatServer", ex);
                throw;
            }

            Logger.Info("Opening chat host...");
            serviceHostChat = new ServiceHost(chat);
            try
            {
                serviceHostChat.Open();
                Logger.Info("Chat host is opened");
            }
            catch (Exception ex)
            {
                Logger.Error("OnStart: error in chat host opening", ex);
                throw;
            }

            Logger.Info("Starting WebChatReceiver...");
            try
            {
                webChat = new WebChatReceiver(chat.Manager);
                Logger.Info("WebChatReceiver started");
            }
            catch (Exception ex)
            {
                Logger.Error("OnStart: error in ChatServer", ex);
                throw;
            }

            Logger.Info("Opening webchat host...");
            serviceHostWebChat = new ServiceHost(webChat);
            try
            {
                serviceHostWebChat.Open();
                Logger.Info("WebChat host is opened");
            }
            catch (Exception ex)
            {
                Logger.Error("OnStart: error in webchat host opening", ex);
                throw;
            }

            Logger.Info("Creating module status controller");
            serviceHostStatus = new ServiceHost(ModuleStatusController.Instance);
            try
            {
                serviceHostStatus.Open();
                Logger.Info("Module status controller host is ready");
            }
            catch (Exception ex)
            {
                Logger.Error("OnStart: error in module status host opening", ex);
                throw;
            }
        }

        protected override void OnStop()
        {
            if (serviceHostChat == null)
                return;
            try
            {
                serviceHostChat.Close();
                serviceHostWebChat.Close();
                chat.Stop();
            }
            catch (Exception ex)
            {
                Logger.Error("OnStop: error in host closing", ex);
            }

            try
            {
                serviceHostStatus.Close();
            }
            catch (Exception ex)
            {
                Logger.Error("OnStop: error in module status host closing", ex);
            }
        }
    }
}

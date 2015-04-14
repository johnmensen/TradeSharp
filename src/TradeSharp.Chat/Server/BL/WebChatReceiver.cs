using System;
using System.Collections.Generic;
using System.ServiceModel;
using TradeSharp.Chat.Contract;

namespace TradeSharp.Chat.Server.BL
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class WebChatReceiver : IWebChat
    {
        private ChatManager chatManager;

        public WebChatReceiver(ChatManager manager)
        {
            chatManager = manager;
        }

        public List<User> GetAllUsers(string room = "")
        {
            ChatResultCode errorCode;
            return chatManager.GetData4GetAllUsers(room, 0, out errorCode);
        }

        public List<Room> GetRooms()
        {
            ChatResultCode errorCode;
            return chatManager.GetData4GetRooms(0, out errorCode);
        }

        public ChatResultCode MoveToRoom(string login, string room, string password = "")
        {
            var userId = chatManager.GetUserIdByLogin(login);
            if (userId == -1)
                return ChatResultCode.UnknownUser;
            return chatManager.MoveToRoomWithNoAnswer(userId, room, password, 0);
        }

        public ChatResultCode SendMessage(string login, string room, string message)
        {
            var userId = chatManager.GetUserIdByLogin(login);
            if (userId == -1)
                return ChatResultCode.UnknownUser;
            return chatManager.SendMessageWithNoAnswer(userId, room, message, 0);
        }

        public List<Message> GetPendingMessages(string login, DateTime timeStamp, string room = "")
        {
            var userId = chatManager.GetUserIdByLogin(login);
            if (userId == -1)
                return null;
            ChatResultCode errorCode;
            return chatManager.GetData4GetPendingMessages(userId, timeStamp, room, 0, out errorCode);
        }
    }
}

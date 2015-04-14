using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace TradeSharp.Chat.Contract
{
    [ServiceContract]
    public interface IWebChat
    {
        [OperationContract]
        List<User> GetAllUsers(string room = "");

        [OperationContract]
        List<Room> GetRooms();

        [OperationContract]
        ChatResultCode MoveToRoom(string login, string room, string password = "");

        [OperationContract]
        ChatResultCode SendMessage(string login, string room, string message);

        [OperationContract]
        List<Message> GetPendingMessages(string login, DateTime timeStamp, string room = "");
    }
}

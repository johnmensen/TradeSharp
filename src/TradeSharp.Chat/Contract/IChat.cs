using System;
using System.ServiceModel;

namespace TradeSharp.Chat.Contract
{
    [ServiceContract(CallbackContract = typeof(IClientCallback))]
    public interface IChat
    {
        // user management
        [OperationContract(IsOneWay = false)]
        int GetAllUsers(string room = "");

        [OperationContract(IsOneWay = false)]
        int Enter(User user);

        [OperationContract(IsOneWay = false)]
        int Exit();

        // room management
        [OperationContract(IsOneWay = false)]
        int GetRooms();

        [OperationContract(IsOneWay = false)]
        int EnterRoom(string room, string password = "");

        [OperationContract(IsOneWay = false)]
        int MoveToRoom(int user, string room, string password = ""); // 4 future use

        [OperationContract(IsOneWay = false)]
        int LeaveRoom(string room);

        [OperationContract(IsOneWay = false)]
        int CreateRoom(Room room); // !!! non-informed

        [OperationContract(IsOneWay = false)]
        int DestroyRoom(string room); // !!! non-informed

        // messaging
        [OperationContract(IsOneWay = false)]
        int SendPrivateMessage(int receiver, string message); // 4 future use

        [OperationContract(IsOneWay = false)]
        int SendMessage(string room, string message);

        /// <summary>
        /// запрос на сообщения, позднее timeStamp в комнате room
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <param name="room"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = false)]
        int GetPendingMessages(DateTime timeStamp, string room = ""); // TODO: client timesync with server

        /// <summary>
        /// запрос на сообщения, позднее timeStamp от пользователя receiver
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <param name="receiver"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = false)]
        int GetPendingPrivateMessages(DateTime timeStamp, int receiver = 0);

        /// <summary>
        /// оживление; для поддержания WCF-соединения необходим периодический вызов
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void Ping();
    }

    public interface IClientCallback
    {
        // request management
        [OperationContract(IsOneWay = true)]
        void RequestStatusReceived(Answer answer);

        // user management
        // result of GetAllUsers
        [OperationContract(IsOneWay = true)]
        void AllUsersReceived(AnswerWithUsers answer);

        [OperationContract(IsOneWay = true)]
        void UserChanged(UserAction action);

        // room management
        // result of GetRooms
        [OperationContract(IsOneWay = true)]
        void RoomsReceived(AnswerWithRooms answer);

        // messaging
        [OperationContract(IsOneWay = true)]
        void MessageReceived(Message message);

        // result of GetPendingMessages
        [OperationContract(IsOneWay = true)]
        void PendingMessagesReceived(AnswerWithMessages answer);
    }
}

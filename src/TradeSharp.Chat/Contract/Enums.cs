
namespace TradeSharp.Chat.Contract
{
    public enum ChatResultCode
    {
        InProgress,
        Ok,
        Error,
        CommunicationError,
        ServerError,
        UnknownUser,
        UserExists,
        UnknownRoom,
        RoomExists,
        AlreadyEnteredRoom,
        AlreadyLeftRoom
    };

    public enum RequestCode
    {
        GetAllUsers,
        Enter,
        Exit,
        GetRooms,
        EnterRoom,
        MoveToRoom,
        LeaveRoom,
        CreateRoom,
        DestroyRoom,
        SendPrivateMessage,
        SendMessage,
        GetPendingMessages,
        GetPendingPrivateMessages,
        Ping,
        GetUserInfoEx,
        SetUserInfoEx
    }

    public enum AnswerCode
    {
        RequestStatusReceived,
        AllUsersReceived,
        UserChanged,
        RoomsReceived,
        MessageReceived,
        PendingMessagesReceived
    }
}

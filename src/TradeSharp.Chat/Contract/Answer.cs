using System;
using System.Collections.Generic;

namespace TradeSharp.Chat.Contract
{
    // для callback-интерфейса
    public class Answer
    {
        public int RequestId;
        public ChatResultCode Status;

        public Answer()
        {
            Status = ChatResultCode.Ok;
        }

        public Answer(int requestId, ChatResultCode status)
        {
            RequestId = requestId;
            Status = status;
        }

        public static string GetChatResultCodeString(ChatResultCode code)
        {
            switch (code)
            {
                case ChatResultCode.InProgress:
                    return "Выполняется";
                case ChatResultCode.Ok:
                    return "Готово";
                case ChatResultCode.Error:
                    return "Внутренняя ошибка";
                case ChatResultCode.CommunicationError:
                    return "Ошибка связи";
                case ChatResultCode.ServerError:
                    return "Внутренняя ошибка сервера";
                case ChatResultCode.UnknownUser:
                    return "Пользователь не существует";
                case ChatResultCode.UnknownRoom:
                    return "Комната не существует";
                case ChatResultCode.UserExists:
                    return "Такой пользователь уже существует";
                case ChatResultCode.RoomExists:
                    return "Такая комната уже существует";
                default:
                    return "Unhandled ChatResultCode";
            }
        }
    }

    // ответы на различные запросы; помимо данных ответа содержат данные из запроса
    public class AnswerWithUsers : Answer
    {
        public List<User> Users;
        public string Room;
    }

    public class AnswerWithRooms : Answer
    {
        public List<Room> Rooms;
    }

    public class AnswerWithMessages : Answer
    {
        public List<Message> Messages;
        public DateTime TimeStamp;
        public string Room;
        public int Receiver;
    }
}

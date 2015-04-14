using System;
using System.Collections.Generic;
using TradeSharp.Chat.Contract;

namespace TradeSharp.Chat.Client.BL
{
    public class ChatRequest
    {
        public int Id;
        public RequestCode Code;
        public List<object> Arguments;
        public ChatResultCode Status;

        public ChatRequest(RequestCode code, List<object> arguments, int id = 0, ChatResultCode status = ChatResultCode.InProgress)
        {
            Id = id;
            Code = code;
            Arguments = arguments;
            Status = status;
        }

        public ChatRequest(ChatRequest request)
        {
            Id = request.Id;
            Code = request.Code;
            Arguments = request.Arguments; // !!! arguments not copied
            Status = request.Status;
        }

        public override string ToString()
        {
            var result = "";
            result += Id != 0 ? "[" + Id.ToString() + "]\t" : "[ - ]\t";
            result += Status != ChatResultCode.InProgress ? Answer.GetChatResultCodeString(Status) + " (" : "";
            switch (Code)
            {
                case RequestCode.GetAllUsers:
                    result += "GetAllUsers" +
                             (!string.IsNullOrEmpty((string) Arguments[0]) ? " room: " + (string) Arguments[0] : "");
                    break;
                case RequestCode.Enter:
                    result += "Enter " + Arguments[0];
                    break;
                case RequestCode.Exit:
                    result += "Exit";
                    break;
                case RequestCode.GetRooms:
                    result += "GetRooms";
                    break;
                case RequestCode.EnterRoom:
                    result += "EnterRoom " + (string) Arguments[0];
                    break;
                case RequestCode.MoveToRoom:
                    result += "MoveToRoom user: " + (int) Arguments[0] + " room: " + (string) Arguments[1];
                    break;
                case RequestCode.LeaveRoom:
                    result += "LeaveRoom " + (string) Arguments[0];
                    break;
                case RequestCode.CreateRoom:
                    result += "CreateRoom " + Arguments[0];
                    break;
                case RequestCode.DestroyRoom:
                    result += "DestroyRoom " + (string) Arguments[0];
                    break;
                case RequestCode.SendPrivateMessage:
                    result += "SendPrivateMessage";
                    break;
                case RequestCode.SendMessage:
                    result += "SendMessage";
                    break;
                case RequestCode.GetPendingMessages:
                    result += "GetPendingMessages date: " + (DateTime) Arguments[0] +
                             (!string.IsNullOrEmpty((string) Arguments[1]) ? " room: " + (string) Arguments[1] : "");
                    break;
                case RequestCode.GetPendingPrivateMessages:
                    result += "GetPendingPrivateMessages date: " + (DateTime) Arguments[0] +
                              ((int) Arguments[1] == 0 ? " receiver: " + (int) Arguments[1] : "");
                    break;
                case RequestCode.Ping:
                    result += "Ping";
                    break;
                case RequestCode.GetUserInfoEx:
                    result += "GetUserInfoEx";
                    break;
                case RequestCode.SetUserInfoEx:
                    result += "SetUserInfoEx";
                    break;
                default:
                    result = "Unhandled code: " + (int)Code;
                    break;
            }
            if(Status != ChatResultCode.InProgress)
                result += ")";
            return result;
        }
    }
}

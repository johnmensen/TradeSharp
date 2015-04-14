using System;

namespace TradeSharp.Chat.Contract
{
    public class UserAction
    {
        public int UserId;

        public UserActionType ActionType;

        public string Room;

        public DateTime TimeStamp;

        // уведомление об этом событии произвести только для этого пользователя
        // исп. для блокирования широковещательного уведомления
        public int? TargetUserId; 
        
        public UserAction()
        {
        }

        public UserAction(UserAction userAction)
        {
            UserId = userAction.UserId;
            ActionType = userAction.ActionType;
            Room = userAction.Room;
            TimeStamp = userAction.TimeStamp;
            TargetUserId = userAction.TargetUserId;
        }

        public override string ToString()
        {
            var at = "";
            switch (ActionType)
            {
                case UserActionType.Enter:
                    at = "entered";
                    break;
                case UserActionType.Exit:
                    at = "has left";
                    break;
                case UserActionType.StatusChange:
                    at = "changed status";
                    break;
                case UserActionType.RoomEnter:
                    at = "entered room " + Room;
                    break;
                case UserActionType.RoomLeave:
                    at = "has left room " + Room;
                    break;
            }
            return "[" + TimeStamp + "] " + UserId + " " + at;
        }
    }
}

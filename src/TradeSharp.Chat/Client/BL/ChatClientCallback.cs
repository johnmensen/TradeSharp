using System.Collections.Generic;
using System.ServiceModel;
using TradeSharp.Chat.Contract;

namespace TradeSharp.Chat.Client.BL
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class ChatClientCallback : IClientCallback
    {
        public delegate void RequestStatusReceivedDel(int requestId, ChatResultCode status);
        public delegate void AllUsersReceivedDel(List<User> users, string room);
        public delegate void UserChangedDel(UserAction action);
        public delegate void RoomsReceivedDel(List<Room> rooms);
        public delegate void MessageReceivedDel(Message message);
        public delegate void PendingMessagesReceivedDel(List<Message> messages, string room);

        public RequestStatusReceivedDel RequestStatusReceivedD;
        public AllUsersReceivedDel AllUsersReceivedD;
        public UserChangedDel UserEntered;
        public UserChangedDel UserExited;
        public UserChangedDel UserEnteredRoom;
        public UserChangedDel UserLeftRoom;
        public RoomsReceivedDel RoomsReceivedD;
        public MessageReceivedDel MessageReceivedD;
        public PendingMessagesReceivedDel PendingMessagesReceivedD;

        public void RequestStatusReceived(Answer answer)
        {
            if (RequestStatusReceivedD != null)
                RequestStatusReceivedD(answer.RequestId, answer.Status);
        }

        public void AllUsersReceived(AnswerWithUsers answer)
        {
            RequestStatusReceived(answer);
            if (AllUsersReceivedD != null)
                AllUsersReceivedD(answer.Users, answer.Room);
        }

        public void UserChanged(UserAction action)
        {
            switch (action.ActionType)
            {
                case UserActionType.Enter:
                    if (UserEntered != null)
                        UserEntered(action);
                    break;
                case UserActionType.StatusChange:
                    break;
                case UserActionType.Exit:
                    if (UserExited != null)
                        UserExited(action);
                    break;
                case UserActionType.RoomEnter:
                    if (UserEnteredRoom != null)
                        UserEnteredRoom(action);
                    break;
                case UserActionType.RoomLeave:
                    if (UserLeftRoom != null)
                        UserLeftRoom(action);
                    break;
            }
        }

        public void RoomsReceived(AnswerWithRooms answer)
        {
            RequestStatusReceived(answer);
            if (RoomsReceivedD != null)
                RoomsReceivedD(answer.Rooms);
        }

        public void MessageReceived(Message message)
        {
            if (MessageReceivedD != null)
                MessageReceivedD(message);
        }

        public void PendingMessagesReceived(AnswerWithMessages answer)
        {
            RequestStatusReceived(answer);
            if (PendingMessagesReceivedD != null)
                PendingMessagesReceivedD(answer.Messages, answer.Room);
        }
    }
}

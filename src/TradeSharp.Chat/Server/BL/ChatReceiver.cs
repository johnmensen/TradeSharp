using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using TradeSharp.Chat.Contract;
using TradeSharp.Util;

namespace TradeSharp.Chat.Server.BL
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ChatReceiver : IChat
    {
        private int freeRequestId = 1;
        private int lockTimeout = 1000;
        private readonly Thread processingThread;
        private volatile bool isStopping;

        private readonly ThreadSafeQueue<Cortege3<IClientCallback, RequestCode, List<object>>> pendingInRequests =
            new ThreadSafeQueue<Cortege3<IClientCallback, RequestCode, List<object>>>();

        public ChatManager Manager;

        public ChatReceiver()
        {
            Manager = new ChatManager();
            processingThread = new Thread(DoProcess);
            processingThread.Start();
        }

        public void Stop()
        {
            isStopping = true;
            Manager.Stop();
        }

        public int GetAllUsers(string room)
        {
            var requestId = Interlocked.Increment(ref freeRequestId);
            pendingInRequests.InQueue(
                new Cortege3<IClientCallback, RequestCode, List<object>>(
                    OperationContext.Current.GetCallbackChannel<IClientCallback>(), RequestCode.GetAllUsers,
                    new List<object> {requestId, room}), lockTimeout);
            return requestId;
        }

        public int Enter(User user)
        {
            var requestId = Interlocked.Increment(ref freeRequestId);
            pendingInRequests.InQueue(
                new Cortege3<IClientCallback, RequestCode, List<object>>(
                    OperationContext.Current.GetCallbackChannel<IClientCallback>(), RequestCode.Enter,
                    new List<object> {requestId, user}), lockTimeout);
            return requestId;
        }

        public int Exit()
        {
            var requestId = Interlocked.Increment(ref freeRequestId);
            pendingInRequests.InQueue(
                new Cortege3<IClientCallback, RequestCode, List<object>>(
                    OperationContext.Current.GetCallbackChannel<IClientCallback>(), RequestCode.Exit,
                    new List<object> {requestId}), lockTimeout);
            return requestId;
        }

        public int GetRooms()
        {
            var requestId = Interlocked.Increment(ref freeRequestId);
            pendingInRequests.InQueue(
                new Cortege3<IClientCallback, RequestCode, List<object>>(
                    OperationContext.Current.GetCallbackChannel<IClientCallback>(), RequestCode.GetRooms,
                    new List<object> {requestId}), lockTimeout);
            return requestId;
        }

        public int EnterRoom(string room, string password = "")
        {
            var requestId = Interlocked.Increment(ref freeRequestId);
            pendingInRequests.InQueue(
                new Cortege3<IClientCallback, RequestCode, List<object>>(
                    OperationContext.Current.GetCallbackChannel<IClientCallback>(), RequestCode.EnterRoom,
                    new List<object> {requestId, room, password}), lockTimeout);
            return requestId;
        }

        public int MoveToRoom(int user, string room, string password = "")
        {
            var requestId = Interlocked.Increment(ref freeRequestId);
            pendingInRequests.InQueue(
                new Cortege3<IClientCallback, RequestCode, List<object>>(
                    OperationContext.Current.GetCallbackChannel<IClientCallback>(), RequestCode.MoveToRoom,
                    new List<object> {requestId, user, room, password}), lockTimeout);
            return requestId;
        }

        public int LeaveRoom(string room)
        {
            var requestId = Interlocked.Increment(ref freeRequestId);
            pendingInRequests.InQueue(
                new Cortege3<IClientCallback, RequestCode, List<object>>(
                    OperationContext.Current.GetCallbackChannel<IClientCallback>(), RequestCode.LeaveRoom,
                    new List<object> {requestId, room}), lockTimeout);
            return requestId;
        }

        public int CreateRoom(Room room)
        {
            var requestId = Interlocked.Increment(ref freeRequestId);
            pendingInRequests.InQueue(
                new Cortege3<IClientCallback, RequestCode, List<object>>(
                    OperationContext.Current.GetCallbackChannel<IClientCallback>(), RequestCode.CreateRoom,
                    new List<object> {requestId, room}), lockTimeout);
            return requestId;
        }

        public int DestroyRoom(string room)
        {
            var requestId = Interlocked.Increment(ref freeRequestId);
            pendingInRequests.InQueue(
                new Cortege3<IClientCallback, RequestCode, List<object>>(
                    OperationContext.Current.GetCallbackChannel<IClientCallback>(), RequestCode.DestroyRoom,
                    new List<object> {requestId, room}), lockTimeout);
            return requestId;
        }

        public int SendPrivateMessage(int receiver, string message)
        {
            var requestId = Interlocked.Increment(ref freeRequestId);
            pendingInRequests.InQueue(
                new Cortege3<IClientCallback, RequestCode, List<object>>(
                    OperationContext.Current.GetCallbackChannel<IClientCallback>(), RequestCode.SendPrivateMessage,
                    new List<object> {requestId, receiver, message}), lockTimeout);
            return requestId;
        }

        public int SendMessage(string room, string message)
        {
            var requestId = Interlocked.Increment(ref freeRequestId);
            pendingInRequests.InQueue(
                new Cortege3<IClientCallback, RequestCode, List<object>>(
                    OperationContext.Current.GetCallbackChannel<IClientCallback>(), RequestCode.SendMessage,
                    new List<object> {requestId, room, message}), lockTimeout);
            return requestId;
        }

        public int GetPendingMessages(DateTime timeStamp, string room = "")
        {
            var requestId = Interlocked.Increment(ref freeRequestId);
            pendingInRequests.InQueue(
                new Cortege3<IClientCallback, RequestCode, List<object>>(
                    OperationContext.Current.GetCallbackChannel<IClientCallback>(), RequestCode.GetPendingMessages,
                    new List<object> {requestId, timeStamp, room}), lockTimeout);
            return requestId;
        }

        public int GetPendingPrivateMessages(DateTime timeStamp, int receiver = 0)
        {
            var requestId = Interlocked.Increment(ref freeRequestId);
            pendingInRequests.InQueue(
                new Cortege3<IClientCallback, RequestCode, List<object>>(
                    OperationContext.Current.GetCallbackChannel<IClientCallback>(),
                    RequestCode.GetPendingPrivateMessages,
                    new List<object> {requestId, timeStamp, receiver}), lockTimeout);
            return requestId;
        }

        public void Ping()
        {
        }

        private void DoProcess()
        {
            while (!isStopping)
            {
                bool timeoutFlag;
                var requests = pendingInRequests.ExtractAll(lockTimeout, out timeoutFlag);
                if (timeoutFlag)
                    continue;
                foreach (var request in requests)
                {
                    switch (request.b)
                    {
                        case RequestCode.GetAllUsers:
                            Manager.GetAllUsersInternal(request.a, (int) request.c[0], (string) request.c[1]);
                            break;
                        case RequestCode.Enter:
                            Manager.EnterInternal(request.a, (int) request.c[0], (User) request.c[1]);
                            break;
                        case RequestCode.Exit:
                            Manager.ExitInternal(request.a, (int) request.c[0]);
                            break;
                        case RequestCode.GetRooms:
                            Manager.GetRoomsInternal(request.a, (int) request.c[0]);
                            break;
                        case RequestCode.EnterRoom:
                            Manager.EnterRoomInternal(request.a, (int) request.c[0], (string) request.c[1], (string) request.c[2]);
                            break;
                        case RequestCode.MoveToRoom:
                            Manager.MoveToRoomInternal(request.a, (int) request.c[0], (int) request.c[1], (string) request.c[2], (string) request.c[3]);
                            break;
                        case RequestCode.LeaveRoom:
                            Manager.LeaveRoomInternal(request.a, (int) request.c[0], (string) request.c[1]);
                            break;
                        case RequestCode.CreateRoom:
                            Manager.CreateRoomInternal(request.a, (int) request.c[0], (Room) request.c[1]);
                            break;
                        case RequestCode.DestroyRoom:
                            Manager.DestroyRoomInternal(request.a, (int) request.c[0], (string) request.c[1]);
                            break;
                        case RequestCode.SendPrivateMessage:
                            Manager.SendPrivateMessageInternal(request.a, (int) request.c[0], (int) request.c[1], (string) request.c[2]);
                            break;
                        case RequestCode.SendMessage:
                            Manager.SendMessageInternal(request.a, (int) request.c[0], (string) request.c[1], (string) request.c[2]);
                            break;
                        case RequestCode.GetPendingMessages:
                            Manager.GetPendingMessagesInternal(request.a, (int) request.c[0], (DateTime) request.c[1], (string) request.c[2]);
                            break;
                        case RequestCode.GetPendingPrivateMessages:
                            Manager.GetPendingPrivateMessagesInternal(request.a, (int) request.c[0], (DateTime) request.c[1], (int) request.c[2]);
                            break;
                    }
                }
                Thread.Sleep(100);
            }
        }
    }
}

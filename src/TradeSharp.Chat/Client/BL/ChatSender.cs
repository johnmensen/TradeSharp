using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Entity;
using TradeSharp.Chat.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.SiteBridge.Lib.Distribution;
using TradeSharp.Util;

namespace TradeSharp.Chat.Client.BL
{
    public class ChatSender : IChat
    {
        public delegate void RequestDel(ChatRequest request);
        public delegate void UserInfoExDel(UserInfoEx info);

        public ChatClientStable Chat;
        public UserInfoExCache UserInfoSource;
        public RequestDel RequestQueued;
        public RequestDel RequestQueuedOnServer;
        public RequestDel RequestProcessed;
        public ChatClientStable.ConnectionDel Connected;
        public ChatClientStable.ConnectionDel Disconnected;
        public UserInfoExDel UserInfoExReceived;

        private readonly ChatClientCallback chatCallback;
        // запросы, поставленные в очередь для отправки на сервер
        private readonly ThreadSafeQueue<ChatRequest> requests = new ThreadSafeQueue<ChatRequest>();
        // запросы, ожидающие ответа сервера
        private readonly ThreadSafeStorage<int, ChatRequest> pendingRequests = new ThreadSafeStorage<int, ChatRequest>();
        // ответы на запросы, которые еще не были поставлены в очередь
        private readonly ThreadSafeStorage<int, ChatResultCode> forwardAnswers = new ThreadSafeStorage<int, ChatResultCode>();
        private readonly Thread networkingThread;
        private int lockTimeout = 1000;
        private volatile bool isStopping;
        private ThreadSafeTimeStamp lastConnectionCheck = new ThreadSafeTimeStamp();

        public ChatSender(ChatClientCallback chatCallback)
        {
            this.chatCallback = chatCallback;
            this.chatCallback.RequestStatusReceivedD += OnRequestStatusReceive;
            networkingThread = new Thread(DoSend);
            networkingThread.Start();
        }

        public void Stop()
        {
            isStopping = true;
            chatCallback.RequestStatusReceivedD -= OnRequestStatusReceive;
        }

        public int GetAllUsers(string room = "")
        {
            QueueRequest(new ChatRequest(RequestCode.GetAllUsers, new List<object> {room}));
            return 0;
        }

        public int Enter(User user)
        {
            QueueRequest(new ChatRequest(RequestCode.Enter, new List<object> { user }));
            return 0;
        }

        public int Exit()
        {
            QueueRequest(new ChatRequest(RequestCode.Exit, new List<object>()));
            return 0;
        }

        public int GetRooms()
        {
            QueueRequest(new ChatRequest(RequestCode.GetRooms, new List<object>()));
            return 0;
        }

        public int EnterRoom(string room, string password = "")
        {
            QueueRequest(new ChatRequest(RequestCode.EnterRoom, new List<object> { room, password }));
            return 0;
        }

        public int MoveToRoom(int user, string room, string password = "")
        {
            // save room enter/exit
            QueueRequest(new ChatRequest(RequestCode.MoveToRoom, new List<object> { user, room, password }));
            return 0;
        }

        public int LeaveRoom(string room)
        {
            QueueRequest(new ChatRequest(RequestCode.LeaveRoom, new List<object> { room }));
            return 0;
        }

        public int CreateRoom(Room room)
        {
            QueueRequest(new ChatRequest(RequestCode.CreateRoom, new List<object> { room }));
            return 0;
        }

        public int DestroyRoom(string room)
        {
            QueueRequest(new ChatRequest(RequestCode.DestroyRoom, new List<object> { room }));
            return 0;
        }

        public int SendPrivateMessage(int receiver, string message)
        {
            QueueRequest(new ChatRequest(RequestCode.SendPrivateMessage, new List<object> {receiver, message}));
            return 0;
        }

        public int SendMessage(string room, string message)
        {
            QueueRequest( new ChatRequest(RequestCode.SendMessage, new List<object> {room, message}));
            return 0;
        }

        public int GetPendingMessages(DateTime timeStamp, string room = "")
        {
            QueueRequest(new ChatRequest(RequestCode.GetPendingMessages, new List<object> {timeStamp, room}));
            return 0;
        }

        public int GetPendingPrivateMessages(DateTime timeStamp, int receiver = 0)
        {
            QueueRequest(new ChatRequest(RequestCode.GetPendingPrivateMessages, new List<object> { timeStamp, receiver }));
            return 0;
        }

        public void Ping()
        {
            QueueRequest(new ChatRequest(RequestCode.Ping, new List<object>(), -1));
        }

        // own:
        public void GetUserInfoEx(int userId)
        {
            QueueRequest(new ChatRequest(RequestCode.GetUserInfoEx, new List<object> {userId}, -1));
        }

        public void SetUserInfoEx(UserInfoEx info)
        {
            QueueRequest(new ChatRequest(RequestCode.SetUserInfoEx, new List<object> {info}, -1));
        }

        // 4 additional processing 4 several functions
        public void CreateRequest(ChatRequest request)
        {
            switch (request.Code)
            {
                case RequestCode.EnterRoom:
                    EnterRoom((string) request.Arguments[0]);
                    break;
                case RequestCode.LeaveRoom:
                    LeaveRoom((string) request.Arguments[0]);
                    break;
                default:
                    QueueRequest(request);
                    break;
            }
        }

        // 4 external monitoring
        public bool HasAnyRequests()
        {
            var requestsHere = GetRequests();
            if (requestsHere.Count != 0)
                return true;
            var pendingRequestsHere = GetPendingRequests();
            if (pendingRequestsHere.Count != 0)
                return true;
            return false;
        }

        public List<ChatRequest> GetRequests()
        {
            bool timeoutFlag;
            return requests.ReceiveAllData(lockTimeout, out timeoutFlag).Where(r => r.Code != RequestCode.Ping).Select(r => new ChatRequest(r)).ToList();
        }

        public Dictionary<int, ChatRequest> GetPendingRequests()
        {
            /*if (!pendingRequestsLock.TryEnterReadLock(lockTimeout))
            {
                Console.WriteLine("ChatSender.GetPendingRequests: pendingRequestsReadLock timeout");
                return new Dictionary<int, ChatRequest>();
            }
            try
            {
                return pendingRequests.ToDictionary(r => r.Key, r => new ChatRequest(r.Value));
            }
            finally
            {
                pendingRequestsLock.ExitReadLock();
            }*/
            return pendingRequests.ReceiveAllData()
                                  .ToDictionary(chatRequest => chatRequest.Key,
                                                chatRequest => new ChatRequest(chatRequest.Value));
        }

        // 4 external control
        public void ClearRequests()
        {
            bool timeoutFlag;
            requests.ExtractAll(lockTimeout, out timeoutFlag);
            /*if (!pendingRequestsLock.TryEnterWriteLock(lockTimeout))
            {
                Console.WriteLine("ChatSender.ClearRequests: pendingRequestsWriteLock timeout");
                return;
            }*/
            pendingRequests.Clear();
            //pendingRequestsLock.ExitWriteLock();
        }

        private void QueueRequest(ChatRequest request)
        {
            requests.InQueue(request, lockTimeout);
            if (request.Id == -1) // do not inform about non-numbered requests
                return;
            request.Status = ChatResultCode.InProgress;
            request.Id = 0;
            if (RequestQueued != null)
                RequestQueued(new ChatRequest(request));
        }

        private void OnRequestStatusReceive(int requestId, ChatResultCode status)
        {
            ChatRequest request = null;
            /*if (!pendingRequestsLock.TryEnterReadLock(lockTimeout))
            {
                Console.WriteLine("ChatSender.OnRequestStatusReceive: pendingRequestsReadLock timeout");
                return;
            }*/
            try
            {
                if (!pendingRequests.ContainsKey(requestId))
                {
                    forwardAnswers.UpdateValues(requestId, status);
                    return;
                }
                request = pendingRequests.ReceiveValue(requestId);
                //request = pendingRequests[requestId];
                request.Status = status;
            }
            catch (Exception ex)
            {
                Logger.Info("ChatSender.OnRequestStatusReceive while reading", ex);
            }
            /*finally
            {
                pendingRequestsLock.ExitReadLock();
            }*/
            try
            {
                /*if (!pendingRequestsLock.TryEnterWriteLock(lockTimeout))
                {
                    Console.WriteLine("ChatSender.OnRequestStatusReceive: pendingRequestsWriteLock timeout");
                    return;
                }*/
                pendingRequests.Remove(requestId);
                //pendingRequestsLock.ExitWriteLock();
            }
            catch (Exception ex)
            {
                Logger.Info("ChatSender.OnRequestStatusReceive while removing", ex);
            }
            if (RequestProcessed != null && request != null)
                RequestProcessed(request);
        }

        private void DoSend()
        {
            Chat = new ChatClientStable(chatCallback, TerminalBindings.Chat);
            Chat.Connected += () => { if (Connected != null) Connected(); };
            Chat.Disconnected += () => { if (Disconnected != null) Disconnected(); };
            UserInfoSource = new UserInfoExCache(TradeSharpAccountStatistics.Instance.proxy);
            while (!isStopping)
            {
                bool timeoutFlag;
                var allRequests = requests.ExtractAll(lockTimeout, out timeoutFlag);
                if (timeoutFlag)
                    continue;
                // флаг повтора запроса;
                // перезапросы возникают из-за ошибок сети;
                // в этом случае ожидание между запросами увеличено, чтобы не загружать проц без пользы
                var repeatRequest = false;
                foreach (var request in allRequests)
                {
                    try
                    {
                        switch (request.Code)
                        {
                            case RequestCode.GetAllUsers:
                                request.Id = Chat.GetAllUsers((string) request.Arguments[0]);
                                break;
                            case RequestCode.Enter:
                                request.Id = Chat.Enter((User) request.Arguments[0]);
                                break;
                            case RequestCode.Exit:
                                request.Id = Chat.Exit();
                                break;
                            case RequestCode.GetRooms:
                                request.Id = Chat.GetRooms();
                                break;
                            case RequestCode.EnterRoom:
                                request.Id = Chat.EnterRoom((string) request.Arguments[0], (string) request.Arguments[1]);
                                break;
                            case RequestCode.MoveToRoom:
                                request.Id = Chat.MoveToRoom((int) request.Arguments[0], (string) request.Arguments[1],
                                                             (string) request.Arguments[2]);
                                break;
                            case RequestCode.LeaveRoom:
                                request.Id = Chat.LeaveRoom((string) request.Arguments[0]);
                                break;
                            case RequestCode.CreateRoom:
                                request.Id = Chat.CreateRoom((Room) request.Arguments[0]);
                                break;
                            case RequestCode.DestroyRoom:
                                request.Id = Chat.DestroyRoom((string) request.Arguments[0]);
                                break;
                            case RequestCode.SendPrivateMessage:
                                request.Id = Chat.SendPrivateMessage((int) request.Arguments[0],
                                                                     (string) request.Arguments[1]);
                                break;
                            case RequestCode.SendMessage:
                                request.Id = Chat.SendMessage((string) request.Arguments[0],
                                                              (string) request.Arguments[1]);
                                break;
                            case RequestCode.GetPendingMessages:
                                request.Id = Chat.GetPendingMessages((DateTime) request.Arguments[0],
                                                                     (string) request.Arguments[1]);
                                break;
                            case RequestCode.GetPendingPrivateMessages:
                                request.Id = Chat.GetPendingPrivateMessages((DateTime) request.Arguments[0],
                                                                            (int) request.Arguments[1]);
                                break;
                            case RequestCode.Ping:
                                Chat.Ping();
                                break;
                            case RequestCode.GetUserInfoEx:
                                var userinfo = UserInfoSource.GetUserInfo((int) request.Arguments[0]);
                                if (UserInfoExReceived != null)
                                    UserInfoExReceived(userinfo ?? new UserInfoEx {Id = (int) request.Arguments[0]});
                                break;
                            case RequestCode.SetUserInfoEx:
                                UserInfoSource.SetUserInfo((UserInfoEx)request.Arguments[0]);
                                break;
                        }
                        if (request.Id == 0)
                        {
                            QueueRequest(request); // if server refused request - try again
                            repeatRequest = true;
                        }
                        else if (request.Id != -1) // skip Ping, GetUserInfoEx, SetUserInfoEx
                        {
                            request.Status = ChatResultCode.InProgress;
                            pendingRequests.UpdateValues(request.Id, request);
                            /*if (pendingRequestsLock.TryEnterWriteLock(lockTimeout))
                            {
                                pendingRequests.Add(request.Id, request);
                                pendingRequestsLock.ExitWriteLock();
                            }
                            else
                                Console.WriteLine("ChatSender.DoSend: pendingRequestsWriteLock timeout");*/
                            var requestCopy = new ChatRequest(request);
                            if (RequestQueuedOnServer != null)
                                RequestQueuedOnServer(requestCopy);
                            if (forwardAnswers.ContainsKey(request.Id))
                            {
                                pendingRequests.Remove(request.Id);
                                requestCopy.Status = forwardAnswers.ReceiveValue(request.Id);
                                if (RequestProcessed != null)
                                    RequestProcessed(requestCopy);
                                forwardAnswers.Remove(request.Id);
                            }
                        }
                    }
                    catch (Exception ex) // probably communication error
                    {
                        Logger.ErrorFormat("DoSend exception: {0}", ex);
                        if (request.Code != RequestCode.Ping)
                            QueueRequest(request);
                        repeatRequest = true;
                    }
                }

                //проверка соединения - ping
                if (allRequests.Count == 0)
                {
                    if (DateTime.Now.Subtract(lastConnectionCheck.GetLastHit()).TotalSeconds > 15)
                    {
                        var request = new ChatRequest(RequestCode.Ping, new List<object>(), -1);
                        QueueRequest(request);
                        lastConnectionCheck.Touch();
                    }
                }

                Thread.Sleep(repeatRequest ? 1000 : 100);
            }
        }
    }
}

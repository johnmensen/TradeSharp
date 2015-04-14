using System;
using System.Linq;
using System.Threading;
using TradeSharp.Chat.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Chat.Client.BL
{
    // клиент, поддерживающий постоянное присутствие в чате
    // (при возникновении ошибок осуществляющий автоматический перезаход)
    public class ChatSenderStable : IChat
    {
        public delegate void ChatOpsDel();
        public delegate void LogDel(string message);

        public ChatOpsDel Entered;
        public ChatOpsDel Exited;
        public ChatOpsDel RequestQueueFilled;
        public ChatOpsDel RequestQueueCleared;
        public ChatSender.RequestDel RequestErrorOccurred;
        public LogDel Logged;
        public ChatSender.UserInfoExDel UserInfoExReceived;

        // призак входа в чат;
        // устанавливается по ответу от сервера
        public bool IsOnline { get; set; }

        private readonly ChatSender chatSender;
        // идентификатор пользователя, под которым осуществляется вход в чат;
        // устанавливается по необходимости клиента в поддержании присутствия в чате
        private volatile int currentUserId;
        // невыполненные запросы
        private readonly ThreadSafeList<ChatRequest> unAnsweredRequests = new ThreadSafeList<ChatRequest>();
        private const int LockTimeout = 1000;
        // поток для переподключения
        private readonly Thread liveServerThread;
        // период ожидания перед переподключением
        private readonly TimeSpan aliveTimeSpan = new TimeSpan(0, 0, 20);
        // запланированное время переподключения
        private readonly ThreadSafeTimeStamp liveServerCheckTime = new ThreadSafeTimeStamp();
        private volatile bool isStopping;

        public ChatSenderStable(ChatClientCallback chatCallback)
        {
            chatCallback.UserEntered += UserEntered;
            chatCallback.UserExited += UserExited;
            chatSender = new ChatSender(chatCallback);
            chatSender.Connected += Connected;
            chatSender.Disconnected += Disconnected;
            chatSender.RequestQueued += RequestQueuedOnClient;
            chatSender.RequestQueuedOnServer += RequestQueuedOnServer;
            chatSender.RequestProcessed += RequestProcessed;
            chatSender.UserInfoExReceived += UserInfoExReceivedInternal;
            liveServerThread = new Thread(CheckServerAlive);
            liveServerThread.Start();
        }

        // IChat:
        public int GetAllUsers(string room = "")
        {
            return chatSender.GetAllUsers(room);
        }

        public int Enter(User user)
        {
            currentUserId = user.ID;
            return chatSender.Enter(user);
        }

        public int Exit()
        {
            currentUserId = 0;
            // TODO: improve
            // ответ скорее всего не придет, потому что соединение будет разорвано раньше
            // поэтому считаем, что сервер нам ответил
            // тут все-таки нужно отправить Exit и выдержать паузу для доставки
            SendExited();
            return chatSender.Exit();
        }

        public int GetRooms()
        {
            return chatSender.GetRooms();
        }

        public int EnterRoom(string room, string password = "")
        {
            return chatSender.EnterRoom(room, password);
        }

        public int MoveToRoom(int user, string room, string password = "")
        {
            return chatSender.MoveToRoom(user, room, password);
        }

        public int LeaveRoom(string room)
        {
            return chatSender.LeaveRoom(room);
        }

        public int CreateRoom(Room room)
        {
            return chatSender.CreateRoom(room);
        }

        public int DestroyRoom(string room)
        {
            return chatSender.DestroyRoom(room);
        }

        public int SendPrivateMessage(int receiver, string message)
        {
            return chatSender.SendPrivateMessage(receiver, message);
        }

        public int SendMessage(string room, string message)
        {
            return chatSender.SendMessage(room, message);
        }

        public int GetPendingMessages(DateTime timeStamp, string room = "")
        {
            return chatSender.GetPendingMessages(timeStamp, room);
        }

        public int GetPendingPrivateMessages(DateTime timeStamp, int receiver = 0)
        {
            return chatSender.GetPendingPrivateMessages(timeStamp, receiver);
        }

        public void Ping()
        {
            chatSender.Ping();
        }

        // own:
        public void Stop()
        {
            isStopping = true;
            chatSender.Stop();
        }

        public void GetUserInfoEx(int userId)
        {
            chatSender.GetUserInfoEx(userId);
        }

        public void SetUserInfoEx(UserInfoEx info)
        {
            chatSender.SetUserInfoEx(info);
        }

        // internals:
        private void SendExited()
        {
            if (!IsOnline)
                return;
            IsOnline = false;
            if (Exited != null)
                Exited();
        }

        private void UserEntered(UserAction action)
        {
            if (isStopping)
                return;
            if (action.UserId != currentUserId)
                return;
            IsOnline = true;
            // отправляем запросы, не обработанные в предыдущем сеансе связи
            bool timeoutFlag;
            var requests = unAnsweredRequests.ExtractAll(LockTimeout, out timeoutFlag);
            foreach (var request in requests)
                chatSender.CreateRequest(request);
            if (Entered != null)
                Entered();
        }

        private void UserExited(UserAction action)
        {
            if (isStopping)
                return;
            if (action.UserId != currentUserId)
                return;
            SendExited();
        }

        // вызывается при восстановлении соденинения,
        // сразу же выполняем вход
        private void Connected()
        {
            if (isStopping)
                return;
            if(currentUserId != 0)
                chatSender.Enter(new User { ID = currentUserId });
        }

        // переподключение выполняется в ChatClientStable, этот вызов только для индикации в ПИ
        private void Disconnected()
        {
            if (isStopping)
                return;
            SendExited();
        }

        private void RequestQueuedOnClient(ChatRequest request)
        {
            if (isStopping)
                return;
            if (!liveServerCheckTime.GetLastHitIfHitted().HasValue)
                liveServerCheckTime.SetTime(DateTime.Now + aliveTimeSpan);
            else if (RequestQueueFilled != null)
                RequestQueueFilled();
            if (Logged != null)
                Logged(request.ToString());
        }

        private void RequestQueuedOnServer(ChatRequest request)
        {
            if (isStopping)
                return;
            if (!liveServerCheckTime.GetLastHitIfHitted().HasValue)
                liveServerCheckTime.SetTime(DateTime.Now + aliveTimeSpan);
            else if (RequestQueueFilled != null)
                RequestQueueFilled();
            if (Logged != null)
                Logged(request.ToString());
        }

        private void RequestProcessed(ChatRequest request)
        {
            if (isStopping)
                return;
            if (request.Status != ChatResultCode.Ok && RequestErrorOccurred != null)
                RequestErrorOccurred(request);
            if (Logged != null)
                Logged(request.ToString());
            // сбрасываем таймаут если мы вошли в чат и если отсутствуют неотвеченные запросы
            if (!chatSender.HasAnyRequests())
            {
                if (RequestQueueCleared != null)
                    RequestQueueCleared();
                if (currentUserId != 0 && IsOnline)
                    liveServerCheckTime.ResetHit();
            }
        }

        private void UserInfoExReceivedInternal(UserInfoEx info)
        {
            if (UserInfoExReceived != null)
                UserInfoExReceived(info);
        }

        private void CheckServerAlive()
        {
            while (!isStopping)
            {
                Thread.Sleep(100);
                var checkTime = liveServerCheckTime.GetLastHitIfHitted();
                if (!checkTime.HasValue)
                {
                    // по непонятным причинам, клиент иногда все-таки переходит в это состояние
                    // заставляем его переподключиться по таймеру
                    if (currentUserId != 0 && !IsOnline)
                        liveServerCheckTime.SetTime(DateTime.Now + aliveTimeSpan);
                    continue;
                }
                if (checkTime.Value > DateTime.Now)
                    continue;

                // переподключение:
                // запоминаем все невыполненные запросы, за исключением запросов на вход
                unAnsweredRequests.AddRange(
                    chatSender.GetPendingRequests()
                              .Select(r => r.Value)
                              .Where(r => r.Code != RequestCode.Enter && r.Code != RequestCode.Exit), LockTimeout);
                unAnsweredRequests.AddRange(
                    chatSender.GetRequests().Where(r => r.Code != RequestCode.Enter && r.Code != RequestCode.Exit),
                    LockTimeout);
                if (unAnsweredRequests.Count == 0)
                    continue;
                liveServerCheckTime.ResetHit();

                // 4 debug
                Console.WriteLine("timeout: renew connection");
                bool timeoutFlag;
                var reqs = unAnsweredRequests.GetAll(LockTimeout, out timeoutFlag);
                foreach(var req in reqs)
                    Console.WriteLine(req.ToString());
                // end of 4 debug

                if (IsOnline)
                {
                    var userId = currentUserId;
                    Exit();
                    currentUserId = userId;
                    // ожидаем перед разрывом соединения
                    Thread.Sleep(200); // TODO: improve
                }
                // очищаем очереди запросов
                chatSender.ClearRequests();
                // переподключаемся, перезаход выполняется в Connected()
                if (chatSender.Chat != null)
                    chatSender.Chat.RenewConnection();
            }
        }
    }
}

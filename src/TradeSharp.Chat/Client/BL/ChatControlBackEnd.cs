using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TradeSharp.Chat.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.SiteBridge.Lib.Contract;
using TradeSharp.Util;
using Timer = System.Timers.Timer;

namespace TradeSharp.Chat.Client.BL
{
    // конечный интерфейсный объект перед пользовательским интерфейсом
    // хранит всех пользователей и все комнаты
    // (при возникновении ошибок работы с комнатами осуществляет автоматический перезаход в них)
    public class ChatControlBackEnd
    {
        // 4 IChatSpamRobot
        public delegate void EnterRoomDel(string room, string password = "");
        public delegate void SendMessageInRoomDel(string message, string room, string password = "");

        public event ChatSenderStable.ChatOpsDel Entered;
        public event ChatSenderStable.ChatOpsDel Exited;
        public event ChatSenderStable.ChatOpsDel RequestQueueFilled;
        public event ChatSenderStable.ChatOpsDel RequestQueueCleared;
        public event ChatSender.RequestDel RequestErrorOccurred;
        public event ChatSenderStable.LogDel Logged;
        public event ChatSender.UserInfoExDel UserInfoExReceived;
        public event ChatClientCallback.AllUsersReceivedDel AllUsersReceived;
        public event ChatClientCallback.UserChangedDel UserEntered;
        public event ChatClientCallback.UserChangedDel UserExited;
        public event ChatClientCallback.UserChangedDel UserEnteredRoom;
        public event ChatClientCallback.UserChangedDel UserLeftRoom;
        public event ChatClientCallback.RoomsReceivedDel RoomsReceived;
        public event ChatClientCallback.MessageReceivedDel MessageReceived;
        public event ChatClientCallback.PendingMessagesReceivedDel PendingMessagesReceived;

        // (совпадает с ChatSettings.Instance.Id)
        public int CurrentUserId { get; private set; }

        private ChatClientCallback receiver;
        private ChatSenderStable sender;
        private readonly ThreadSafeList<User> onlineUsers = new ThreadSafeList<User>();
        // комнаты, существующие на момент последнего вызова GetRooms
        private readonly ThreadSafeList<Room> activeRooms = new ThreadSafeList<Room>();
        private readonly ThreadSafeStorage<string, List<Message>> postedMessages = new ThreadSafeStorage<string, List<Message>>();
        // комнаты, в которые вход выполнен успешно
        private readonly ThreadSafeList<string> enteredRooms = new ThreadSafeList<string>();
        // введенные пароли для комнат
        private readonly ThreadSafeStorage<string, string> roomPasswords = new ThreadSafeStorage<string, string>();
        private const string PrivateRoomName = "";
        private const int LockTimeout = 1000;
        // поток для переподключения
        private Thread liveRoomThread;
        // период ожидания перед переподключением
        private readonly TimeSpan aliveTimeSpan = new TimeSpan(0, 1, 0);
        // запланированное время переподключения
        private readonly ThreadSafeTimeStamp liveRoomCheckTime = new ThreadSafeTimeStamp();
        private volatile bool isStopping;
        // локальное хранилище сообщений
        private readonly ChatClientStorage storage = new ChatClientStorage();
        // 4 debug
        // таймер для спама
        private Timer spamTimer;
        private string spamRoom;
        private int spamMessageCount = 1;

        public ChatControlBackEnd()
        {
            isStopping = true;
            // чтение сообщений из локального хранилища
            var messages = storage.LoadMessagesFromFiles();
            foreach (var message in messages)
                SavePostedMessage(message);
        }

        // chat managing
        public void Start()
        {
            if (!isStopping)
                return;
            // чтение паролей на комнаты
            foreach (var room in ChatSettings.Instance.Rooms)
                roomPasswords.UpdateValues(room, ChatSettings.Instance.Passwords[ChatSettings.Instance.Rooms.IndexOf(room)]);
            // подготовка к сетевому взаимодействию
            receiver = new ChatClientCallback();
            receiver.AllUsersReceivedD += AllUsersReceivedInternal;
            receiver.UserEntered += UserEnteredInternal;
            receiver.UserExited += UserExitedInternal;
            receiver.UserEnteredRoom += UserEnteredRoomInternal;
            receiver.UserLeftRoom += UserLeftRoomInternal;
            receiver.RoomsReceivedD += RoomsReceivedInternal;
            receiver.MessageReceivedD += MessageReceivedInternal;
            receiver.PendingMessagesReceivedD += PendingMessagesReceivedInternal;
            sender = new ChatSenderStable(receiver);
            sender.Entered += EnteredInternal;
            sender.Exited += ExitedInternal;
            sender.RequestQueueFilled += RequestQueueFilledInternal;
            sender.RequestQueueCleared += RequestQueueClearedInternal;
            sender.RequestErrorOccurred += RequestErrorOccurredInternal;
            sender.Logged += LoggedInternal;
            sender.UserInfoExReceived += UserInfoExReceivedInternal;
            liveRoomCheckTime.SetTime(DateTime.Now + aliveTimeSpan);
            liveRoomThread = new Thread(CheckRoomEntered);
            liveRoomThread.Start();
            isStopping = false;
        }

        public void Stop()
        {
            if (isStopping || receiver == null || sender == null)
                return;
            isStopping = true;
            try
            {
// ReSharper disable DelegateSubtraction
                receiver.AllUsersReceivedD -= AllUsersReceivedInternal;
                receiver.UserEntered -= UserEnteredInternal;
                receiver.UserExited -= UserExitedInternal;
                receiver.UserEnteredRoom -= UserEnteredRoomInternal;
                receiver.UserLeftRoom -= UserLeftRoomInternal;
                receiver.RoomsReceivedD -= RoomsReceivedInternal;
                receiver.MessageReceivedD -= MessageReceivedInternal;
                receiver.PendingMessagesReceivedD -= PendingMessagesReceivedInternal;
                sender.Entered -= EnteredInternal;
                sender.Exited -= ExitedInternal;
                sender.RequestQueueFilled -= RequestQueueFilledInternal;
                sender.RequestQueueCleared -= RequestQueueClearedInternal;
                sender.RequestErrorOccurred -= RequestErrorOccurredInternal;
                sender.Logged -= LoggedInternal;
// ReSharper restore DelegateSubtraction
                if (sender.IsOnline)
                {
                    // выход из чата
                    sender.Exit();
                    // ожидание отправки команды на выход
                    Thread.Sleep(200);
                }
                // прекращение сетевого взаимодействия
                sender.Stop();
                // сохранение последних сообщений
                storage.Stop();
                // сохранение настроек
                ChatSettings.Instance.SaveSettings();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка остановки чата (ChatControlBackEnd.Stop)", ex);
            }
        }

        public void Login(int userId)
        {
            if (sender == null)
                return;
            try
            {
                // уже выполнен вход под другим пользователем - выходим
                if (CurrentUserId != 0 && CurrentUserId != userId)
                    Logout();
                CurrentUserId = userId;
                sender.Enter(new User { ID = CurrentUserId });
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка авторизации в чате (ChatControlBackEnd.Login)", ex);
            }
        }

        public void Logout()
        {
            if (sender == null)
                return;
            try
            {
                CurrentUserId = 0;
                onlineUsers.ExtractAll(LockTimeout);
                activeRooms.ExtractAll(LockTimeout);
                if (sender.IsOnline)
                    sender.Exit();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка выхода из чата (ChatControlBackEnd.Logout)", ex);
            }
        }

        public bool IsOnline()
        {
            if (sender == null)
                return false;
            try
            {
                return sender.IsOnline;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка определения состояния чата (ChatControlBackEnd.IsOnline)", ex);
                return false;
            }
        }

        public List<User> GetOnlineUsers()
        {
            bool timeoutFlag;
            return onlineUsers.GetAll(LockTimeout, out timeoutFlag).Select(u => new User(u)).ToList();
        }

        public List<Room> GetActiveRooms()
        {
            bool timeoutFlag;
            return activeRooms.GetAll(LockTimeout, out timeoutFlag).Select(r => new Room(r)).ToList();
        }

        public List<string> GetEnteredRooms()
        {
            bool timeoutFlag;
            return enteredRooms.GetAll(LockTimeout, out timeoutFlag);
        }

        public string GetRoomPassword(string room)
        {
            if (roomPasswords.ContainsKey(room))
                return roomPasswords.ReceiveValue(room);
            return "";
        }

        public void SetRoomPassword(string room, string password)
        {
            roomPasswords.UpdateValues(room, password);
        }

        public List<Message> GetMessages(string room)
        {
            if (!postedMessages.ContainsKey(room))
                return new List<Message>();
            return postedMessages.ReceiveValue(room).Select(m => new Message(m)).ToList();
        }

        /// <summary>
        /// получение приветных сообщений из кэша
        /// </summary>
        /// <param name="userId">пользователь, переписку с которым необходимо получить</param>
        /// <returns></returns>
        public List<Message> GetPrivateMessages(int userId)
        {
            if (!postedMessages.ContainsKey(PrivateRoomName))
                return new List<Message>();
            return postedMessages.ReceiveValue(PrivateRoomName).Where(
                m =>
                (m.Sender == CurrentUserId && m.Receiver == userId) ||
                (m.Sender == userId && m.Receiver == CurrentUserId))
                                 .Select(m => new Message(m))
                                 .ToList();
        }

        // 4 debug
        // запуск спама
        public void StartSpam(string room, int interval = 1000)
        {
            spamRoom = room;
            spamTimer = new Timer(interval);
            spamTimer.Elapsed += Spam;
            spamTimer.Start();
        }

        // 4 debug
        // остановка спама
        public void StopSpam()
        {
            spamTimer.Stop();
            spamTimer = null;
        }

        // chat commands
        public void GetAllUsers(string room = "")
        {
            try
            {
                if (!sender.IsOnline)
                {
                    Logger.Error("Внутренняя ошибка (ChatControlBackEnd.GetAllUsers)");
                    return;
                }
                sender.GetAllUsers(room);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка определения пользователей чата (ChatControlBackEnd.GetAllUsers)", ex);
            }
        }

        public void GetRooms()
        {
            try
            {
                if (!sender.IsOnline)
                {
                    Logger.Error("Внутренняя ошибка (ChatControlBackEnd.GetRooms)");
                    return;
                }
                sender.GetRooms();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка определения комнат чата (ChatControlBackEnd.GetRooms)", ex);
            }
        }

        public void EnterRoom(string room, string password = "")
        {
            // запоминаем введенный пароль
            SetRoomPassword(room, password);
            ChatSettings.Instance.AddRoom(room, password);
            try
            {
                if (!sender.IsOnline)
                {
                    Logger.Error("Внутренняя ошибка (ChatControlBackEnd.EnterRoom)");
                    return;
                }
                sender.EnterRoom(room, password);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка входа в комнату чата (ChatControlBackEnd.EnterRoom)", ex);
            }
        }

        public void LeaveRoom(string room)
        {
            ChatSettings.Instance.RemoveRoom(room);
            try
            {
                bool timeoutFlag;
                if (!enteredRooms.Contains(room, LockTimeout, out timeoutFlag))
                    return;
                if (!sender.IsOnline)
                {
                    Logger.Error("Внутренняя ошибка (ChatControlBackEnd.LeaveRoom)");
                    return;
                }
                sender.LeaveRoom(room);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка выхода из комнаты чата (ChatControlBackEnd.LeaveRoom)", ex);
            }
        }

        public void CreateRoom(Room room)
        {
            try
            {
                if (!sender.IsOnline)
                {
                    Logger.Error("Внутренняя ошибка (ChatControlBackEnd.CreateRoom)");
                    return;
                }
                sender.CreateRoom(room);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка создания комнаты чата (ChatControlBackEnd.CreateRoom)", ex);
            }
        }

        public void DestroyRoom(string room)
        {
            try
            {
                if (!sender.IsOnline)
                {
                    Logger.Error("Внутренняя ошибка (ChatControlBackEnd.DestroyRoom)");
                    return;
                }
                sender.DestroyRoom(room);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка удаления комнаты чата (ChatControlBackEnd.DestroyRoom)", ex);
            }
        }

        public void SendMessage(string room, string message)
        {
            try
            {
                bool timeoutFlag;
                if (!enteredRooms.Contains(room, LockTimeout, out timeoutFlag))
                    return;
                if (!sender.IsOnline)
                {
                    Logger.Error("Внутренняя ошибка (ChatControlBackEnd.SendMessage)");
                    return;
                }
                sender.SendMessage(room, message);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка отправки сообщения в чат (ChatControlBackEnd.SendMessage)", ex);
            }
        }

        public void SendMessageInRoom(string message, string room, string password = "")
        {
            try
            {
                bool timeoutFlag;
                if (!enteredRooms.Contains(room, LockTimeout, out timeoutFlag))
                    sender.EnterRoom(room, password);
                if (!sender.IsOnline)
                {
                    Logger.Error("Внутренняя ошибка (ChatControlBackEnd.SendMessageInRoom)");
                    return;
                }
                sender.SendMessage(room, message);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка отправки сообщения в комнату чата (ChatControlBackEnd.SendMessageInRoom)", ex);
            }
        }

        public void GetPendingPrivateMessages()
        {
            try
            {
                var lastDateTime = new DateTime();
                var messages = postedMessages.ReceiveValue(PrivateRoomName);
                if (messages != null && messages.Count != 0)
                    lastDateTime = messages.Last().TimeStamp;
                sender.GetPendingPrivateMessages(lastDateTime);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка получения истории приватной переписки (ChatControlBackEnd.GetPendingPrivateMessages)", ex);
            }
        }

        public void SendPrivateMessage(int receiverId, string message)
        {
            try
            {
                sender.SendPrivateMessage(receiverId, message);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка отправки приватного сообщения (ChatControlBackEnd.SendPrivateMessage)", ex);
            }
        }

        public void GetUserInfoEx(int userId)
        {
            sender.GetUserInfoEx(userId);
        }

        public void SetUserInfoEx(UserInfoEx info)
        {
            sender.SetUserInfoEx(info);
        }

        #region internals
        private void AllUsersReceivedInternal(List<User> users, string room)
        {
            if (isStopping)
                return;
            if (string.IsNullOrEmpty(room))
            {
                onlineUsers.ExtractAll(LockTimeout);
                onlineUsers.AddRange(users, LockTimeout);
            }
            if (AllUsersReceived != null)
                AllUsersReceived(users, room);
        }

        private void UserEnteredInternal(UserAction action)
        {
            if (isStopping)
                return;
            // TODO: отвязать клиента от БД
            var userData = AllUsers.Instance.GetUser(action.UserId) ?? new User { ID = action.UserId, Login = action.UserId.ToString() };
            onlineUsers.Add(userData, LockTimeout);
            if (UserEntered != null)
                UserEntered(action);
        }

        private void UserExitedInternal(UserAction action)
        {
            if (isStopping)
                return;
            if (!onlineUsers.TryRemove(u => u.ID == action.UserId, LockTimeout))
                return;
            if (UserExited != null)
                UserExited(action);
        }

        private void UserEnteredRoomInternal(UserAction action)
        {
            if (isStopping)
                return;
            if (action.UserId == CurrentUserId)
            {
                // сервер в некоторых случаях информирует о входе в комнату, в которую уже вошли
                // такой случай не может произойти с этим клиентом
                bool timeoutFlag;
                if (enteredRooms.Contains(action.Room, LockTimeout, out timeoutFlag))
                    return;
                enteredRooms.Add(action.Room, LockTimeout);
                // обновляем пароль на комнату
                ChatSettings.Instance.UpdatePassword(action.Room, roomPasswords.ReceiveValue(action.Room));
            }
            if (UserEnteredRoom != null)
                UserEnteredRoom(action);
            if (action.UserId != CurrentUserId)
                return;
            // читаем историю переписки
            var lastDateTime = new DateTime();
            var messages = postedMessages.ReceiveValue(action.Room);
            if (messages != null && messages.Count != 0)
                lastDateTime = messages.Last().TimeStamp;
            sender.GetPendingMessages(lastDateTime, action.Room);
        }

        private void UserLeftRoomInternal(UserAction action)
        {
            if (isStopping)
                return;
            if (action.UserId == CurrentUserId)
                enteredRooms.TryRemove(action.Room, LockTimeout);
            if (UserLeftRoom != null)
                UserLeftRoom(action);
        }

        private void RoomsReceivedInternal(List<Room> rooms)
        {
            activeRooms.ExtractAll(LockTimeout);
            activeRooms.AddRange(rooms, LockTimeout);
            if (RoomsReceived != null)
                RoomsReceived(rooms);
        }

        private void MessageReceivedInternal(Message message)
        {
            var newMessage = new Message(message);
            SavePostedMessage(newMessage);
            storage.SaveMessage(newMessage);
            if (MessageReceived != null)
                MessageReceived(message);
        }

        private void SavePostedMessage(Message message)
        {
            if (message.Receiver != 0)
                message.Room = PrivateRoomName;
            if (!postedMessages.ContainsKey(message.Room))
                postedMessages.UpdateValues(message.Room, new List<Message>());
            var messagesInRoom = postedMessages.ReceiveValue(message.Room);
            if (messagesInRoom.Count == 0 || messagesInRoom[messagesInRoom.Count - 1].TimeStamp >= message.TimeStamp)
                messagesInRoom.Add(message);
            else
            {
                var index = messagesInRoom.FindLastIndex(m => m.TimeStamp <= message.TimeStamp);
                messagesInRoom.Insert(index + 1, message);
            }
            postedMessages.UpdateValues(message.Room, messagesInRoom);
        }

        private void PendingMessagesReceivedInternal(List<Message> messages, string room)
        {
            foreach (var message in messages)
            {
                var newMessage = new Message(message);
                SavePostedMessage(newMessage);
                storage.SaveMessage(newMessage);
            }
            if (PendingMessagesReceived != null)
                PendingMessagesReceived(messages, room);
        }

        private void EnteredInternal()
        {
            if (isStopping)
                return;
            sender.GetAllUsers();
            foreach (var room in ChatSettings.Instance.Rooms)
                sender.EnterRoom(room, GetRoomPassword(room));
            if (Entered != null)
                Entered();
        }

        private void ExitedInternal()
        {
            if (isStopping)
                return;
            // на всякий случай очищаем список пользователей и комнат
            onlineUsers.ExtractAll(LockTimeout);
            activeRooms.ExtractAll(LockTimeout);
            enteredRooms.ExtractAll(LockTimeout);
            if (Exited != null)
                Exited();
        }

        private void RequestQueueFilledInternal()
        {
            if (RequestQueueFilled != null)
                RequestQueueFilled();
        }

        private void RequestQueueClearedInternal()
        {
            if (RequestQueueCleared != null)
                RequestQueueCleared();
        }

        private void RequestErrorOccurredInternal(ChatRequest request)
        {
            if (request.Code == RequestCode.EnterRoom)
            {
                var roomName = request.Arguments[0].ToString();
                // скорее всего пароль не подошел - удаляем его
                // но с таким же успехом это может быть ошибка связи
                // TODO: корректные пароли на комнаты могут быть утеряны
                roomPasswords.Remove(roomName);
            }
            if (RequestErrorOccurred != null)
                RequestErrorOccurred(request);
        }

        private void LoggedInternal(string message)
        {
            if (Logged != null)
                Logged(message);
        }

        private void UserInfoExReceivedInternal(UserInfoEx info)
        {
            if (UserInfoExReceived != null)
                UserInfoExReceived(info);
        }

        private void CheckRoomEntered()
        {
            while (!isStopping)
            {
                Thread.Sleep(100);
                if (!sender.IsOnline)
                    continue;
                var checkTime = liveRoomCheckTime.GetLastHit();
                if (checkTime > DateTime.Now)
                    continue;
                foreach (var room in ChatSettings.Instance.Rooms)
                {
                    bool timeoutFlag;
                    if (!enteredRooms.Contains(room, LockTimeout, out timeoutFlag))
                    {
                        try
                        {
                            // TODO: пароль берется из оперативного списка. если вход не удался, то пароль сбрасывается
                            sender.EnterRoom(room, GetRoomPassword(room));
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Ошибка входа в комнату чата (ChatControlBackEnd.CheckRoomEntered)", ex);
                        }
                    }
                }
                liveRoomCheckTime.SetTime(DateTime.Now + aliveTimeSpan);
            }
        }

        private void Spam(Object myObject, EventArgs myEventArgs)
        {
            if (!sender.IsOnline)
                return;
            bool timeoutFlag;
            if (!enteredRooms.Contains(spamRoom, LockTimeout, out timeoutFlag))
                sender.EnterRoom(spamRoom);
            sender.SendMessage(spamRoom, "Spam message " + spamMessageCount++);
        }
        #endregion internals
    }
}

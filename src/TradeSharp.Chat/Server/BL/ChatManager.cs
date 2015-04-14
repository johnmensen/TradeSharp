using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TradeSharp.Chat.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Chat.Server.BL
{
    public partial class ChatManager
    {
        //private int freeUserId = 10000; // obsolete
        private int freeRoomId = 1;
        private readonly ThreadSafeList<ServerUser> users = new ThreadSafeList<ServerUser>();
        private readonly ThreadSafeList<Room> rooms = new ThreadSafeList<Room>();
        private readonly TimeSpan roomExpireTime;
        // <userId, roomId>
        private readonly ThreadSafeList<Cortege2<int, int>> usersAndRooms = new ThreadSafeList<Cortege2<int, int>>();
        // <roomName, message>
        // у приватных сообщений roomName = ""
        private readonly ThreadSafeStorage<string, List<Message>> messages = new ThreadSafeStorage<string, List<Message>>();
        // действия пользователей; хранение не используется в логике работы - только для мониторинга
        private readonly ThreadSafeList<UserAction> userActions = new ThreadSafeList<UserAction>();
        private int lockTimeout = 1000;
        // поток для отправки
        private readonly Thread sendingThread;
        // поток для актуализации пользователей и комнат
        private readonly Thread liveThread;
        // поток для сохранения в БД
        private readonly Thread savingThread;
        // время последнего обновления userAccounts
        private DateTime lastRequestPlatformUserTime;

        private volatile bool isStopping;
        private readonly ThreadSafeQueue<ChatAnswer> answers = new ThreadSafeQueue<ChatAnswer>();

        public ChatManager()
        {
            roomExpireTime = new TimeSpan(0, AppConfig.GetIntParam("RoomExpireTime", 15), 0);
            GetUserAccountsFromDb();
            var loadedRooms = LoadRooms();
            foreach (var loadedRoom in loadedRooms)
                CreateRoomInternal(loadedRoom);
            var loadedMessages = LoadMessages();
            foreach (var loadedMessage in loadedMessages)
                AddMessageToStorage(loadedMessage);
            sendingThread = new Thread(DoSend);
            sendingThread.Start();
            liveThread = new Thread(UsersAndRoomsUpdate);
            liveThread.Start();
            savingThread = new Thread(SaveMessagesAndRooms);
            savingThread.Start();
        }

        public void Stop()
        {
            isStopping = true;
        }

        // for external monitoring
        public List<UserAction> GetUserChangesInternal(DateTime timeStamp)
        {
            var result = userActions.GetAll(ua => ua.TimeStamp > timeStamp, lockTimeout);
            return result;
        }

        #region processing client requests

        // 4 use in WebChatReceiver
        public List<User> GetData4GetAllUsers(string roomName, int requestId, out ChatResultCode errorCode)
        {
            List<User> result;
            errorCode = ChatResultCode.Ok;
            if (string.IsNullOrEmpty(roomName))
            {
                bool timeout;
                result = users.GetAll(lockTimeout, out timeout).Select(u => new User(u)).ToList();
                /*Logger.InfoFormat("GetData4GetAllUsers (no room) returs {0} items: {1}",
                                  answer.Users, string.Join(", ", answer.Users.Select(u =>
                                                                                      string.Format("[{0}] \"{1}\"",
                                                                                                    u.ID, u.FullName))));*/
            }
            else
            {
                var room = rooms.Find(r => r.Name == roomName, r => new Room(r), lockTimeout);
                if (room == null)
                {
                    Logger.InfoFormat("[{1}]GetData4GetAllUsers: No room with name {0}", roomName, requestId);
                    errorCode = ChatResultCode.UnknownRoom;
                    return null;
                }
                var userIds = usersAndRooms.GetAll(ur => ur.b == room.Id, lockTimeout).Select(ur => ur.a).ToList();
                result = users.GetAll(u => userIds.Contains(u.ID), lockTimeout).Select(u => new User(u)).ToList();
                /*Logger.InfoFormat("GetData4GetAllUsers ({0}) returs {1} items: {2}",
                                  roomName, answer.Users, string.Join(", ", answer.Users.Select(u =>
                                                                                                string.Format(
                                                                                                    "[{0}] \"{1}\"",
                                                                                                    u.ID, u.FullName))));*/
            }
            return result;
        }

        public void GetAllUsersInternal(IClientCallback client, int requestId, string roomName)
        {
            ChatResultCode errorCode;
            var answer = new AnswerWithUsers
                {
                    RequestId = requestId,
                    Status = ChatResultCode.Ok,
                    Room = roomName,
                    Users = GetData4GetAllUsers(roomName, requestId, out errorCode)
                };
            if (errorCode != ChatResultCode.Ok)
            {
                answers.InQueue(
                    new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                   new List<object> {new Answer(requestId, errorCode)}), lockTimeout);
                return;
            }
            answers.InQueue(new ChatAnswer(client, AnswerCode.AllUsersReceived, new List<object> {answer}),
                            lockTimeout);
        }

        public void EnterInternal(IClientCallback client, int requestId, User user)
        {
            if (user == null)
            {
                answers.InQueue(
                    new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                    new List<object> { new Answer(requestId, ChatResultCode.Error) }), lockTimeout);
                return;
            }

            var userId = user.ID;
            var existingUser = users.Find(u => u.ID == userId, lockTimeout);

            // подставляем данные о пользователе из БД
            var userAccount = GetUserAccount(user.ID);
            if (userAccount == null)
            {
                answers.InQueue(
                    new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                   new List<object> {new Answer(requestId, ChatResultCode.Error)}), lockTimeout);
                Logger.InfoFormat("[{1}]EnterInternal: user {0} not found in DB", user, requestId);
                return;
            }

            user = userAccount;
            // такого быть не должно - все пользователи из БД
            /*if (user.ID == 0)
                user.ID = freeUserId++;*/

            var broadcastInform = true;
            if (existingUser != null)
            {
                // заход под тем же ID
                if (existingUser.ClientCallback != client) // но клиент другой
                {
                    ExitInternal(GetUserId(existingUser.ClientCallback));
                    existingUser.ClientCallback = client;
                    users.Add(existingUser, lockTimeout);
                    Logger.InfoFormat("[{1}]EnterInternal: user {0} already entered, new client replaced old", user,
                                      requestId);
                }
                else
                {
                    broadcastInform = false;
                    Logger.InfoFormat("[{1}]EnterInternal: user {0} already entered", user, requestId);
                }
            }
            else
            {
                users.Add(new ServerUser(user) {ClientCallback = client}, lockTimeout);
                Logger.InfoFormat("[{1}]EnterInternal: user {0} entered", user, requestId);
            }


            answers.InQueue(
                new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                new List<object> { new Answer(requestId, ChatResultCode.Ok) }), lockTimeout);

            // оповестить о входе всех или только этого пользователя
            var action = new UserAction
                {
                    UserId = user.ID,
                    ActionType = UserActionType.Enter,
                    TimeStamp = DateTime.Now,
                    TargetUserId = broadcastInform ? null : (int?)existingUser.ID
                };
            userActions.Add(action, lockTimeout);
            SendUserAction(action);
        }

        public void ExitInternal(IClientCallback client, int requestId)
        {
            var userId = GetUserId(client);
            if (userId == -1)
            {
                answers.InQueue(
                    new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                    new List<object> { new Answer(requestId, ChatResultCode.UnknownUser) }), lockTimeout);
                Logger.InfoFormat("[{0}]ExitInternal: user already exited", requestId);
                return;
            }

            answers.InQueue(
                new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                new List<object> { new Answer(requestId, ChatResultCode.Ok) }), lockTimeout);
            Logger.InfoFormat("[{0}]ExitInternal: user exited", requestId);

            ExitInternal(userId);
        }

        public int GetUserIdByLogin(string login)
        {
            var user = users.Find(u => u.Login == login, u => new ServerUser(u), lockTimeout);
            if (user == null)
                return -1;
            return user.ID;
        }

        // for internal use
        private int GetUserId(IClientCallback client)
        {
            var user = users.Find(u => u.ClientCallback == client, u => new ServerUser(u), lockTimeout);
            if (user == null)
                return -1;
            return user.ID;
        }

        // for internal use
        private void ExitInternal(int userId)
        {
            if (userId == -1)
                return;

            // kick user from all rooms
            var roomsIds = usersAndRooms.GetAll(ur => ur.a == userId, lockTimeout).Select(ur => ur.b).ToList();
            foreach (var roomId in roomsIds)
            {
                var id = roomId;
                var room = rooms.Find(r => r.Id == id, lockTimeout);
                if(room != null)
                    MoveFromRoomInternal(userId, room.Name);
            }

            var user = users.Find(u => u.ID == userId, lockTimeout);
            var removed = users.TryRemove(u => u.ID == userId, lockTimeout);
            if (removed)
                Logger.InfoFormat("ExitInternal: user ID={0} exited", userId);
            else
                Logger.InfoFormat("ExitInternal: user ID={0} already exited", userId);

            var action = new UserAction
            {
                UserId = userId,
                ActionType = UserActionType.Exit,
                TimeStamp = DateTime.Now,
            };
            // сообщаем ему самому о том, что он покинул чат (SendUserAction не сможет, т.к. пользователя уже нет в списке)
            answers.InQueue(new ChatAnswer(user.ClientCallback, AnswerCode.UserChanged, new List<object> {action}),
                            lockTimeout);
            // и всем остальным
            userActions.Add(action, lockTimeout);
            SendUserAction(action);
        }

        public List<Room> GetData4GetRooms(int requestId, out ChatResultCode errorCode)
        {
            errorCode = ChatResultCode.Ok;
            bool timeoutFlag;
            // делаем копии (дескрипторов) комнат
            var result = rooms.GetAll(lockTimeout, out timeoutFlag).Select(r => new Room(r)).ToList();
            // и скрываем у них пароль
            result.ForEach(r => r.Password = new string('*', 8));
            return result;
        }

        public void GetRoomsInternal(IClientCallback client, int requestId)
        {
            ChatResultCode errorCode;
            var answer = new AnswerWithRooms
                {
                    RequestId = requestId,
                    Status = ChatResultCode.Ok,
                    Rooms = GetData4GetRooms(requestId, out errorCode)
                };
            if (errorCode != ChatResultCode.Ok)
            {
                answers.InQueue(
                    new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                   new List<object> {new Answer(requestId, errorCode)}), lockTimeout);
            }
            answers.InQueue(new ChatAnswer(client, AnswerCode.RoomsReceived, new List<object> {answer}), lockTimeout);
        }

        public void EnterRoomInternal(IClientCallback client, int requestId, string roomName, string password)
        {
            var userId = GetUserId(client);
            if (userId == -1)
            {
                answers.InQueue(
                    new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                    new List<object> { new Answer(requestId, ChatResultCode.UnknownUser) }), lockTimeout);
                Logger.InfoFormat("[{0}]EnterRoomInternal: user not found, room={1}", requestId, roomName);
                return;
            }
            MoveToRoomInternal(client, requestId, userId, roomName, password);
        }

        public ChatResultCode MoveToRoomWithNoAnswer(int userId, string roomName, string password, int requestId)
        {
            var user = users.Find(u => u.ID == userId, lockTimeout);
            if (user == null)
            {
                Logger.InfoFormat("[{0}]MoveToRoomWithNoAnswer: user not found, room={1}", requestId, roomName);
                return ChatResultCode.UnknownUser;
            }
            var room = rooms.Find(r => r.Name == roomName, lockTimeout);
            if (room == null)
            {
                Logger.InfoFormat("[{0}]MoveToRoomWithNoAnswer: room {1} not found, user={2}", requestId, roomName, user);
                return ChatResultCode.UnknownRoom;
            }
            bool timeoutFlag;
            var userAndRoomExists = usersAndRooms.Contains(new Cortege2<int, int> { a = userId, b = room.Id }, lockTimeout, out timeoutFlag);
            if (userAndRoomExists)
                return ChatResultCode.AlreadyEnteredRoom;
            // проверяем пароль
            if (!string.IsNullOrEmpty(room.Password) && room.Password != password)
            {
                Logger.InfoFormat("[{0}]MoveToRoomWithNoAnswer: password not correct, user={1}, room={2}", requestId, user, roomName);
                return ChatResultCode.Error;
            }
            usersAndRooms.Add(new Cortege2<int, int>(userId, room.Id), lockTimeout);
            room.UserCount++;
            // сбрасываем время автоматического удаления комнаты
            room.ExpireTime = null;
            Logger.InfoFormat("[{0}]MoveToRoomWithNoAnswer: user {1} entered room {2}", requestId, user, roomName);
            var action = new UserAction
            {
                UserId = userId,
                ActionType = UserActionType.RoomEnter,
                Room = roomName,
                TimeStamp = DateTime.Now
            };
            userActions.Add(action, lockTimeout);
            SendUserAction(action);
            return ChatResultCode.Ok;
        }

        public void MoveToRoomInternal(IClientCallback client, int requestId, int userId, string roomName, string password)
        {
            var errorCode = MoveToRoomWithNoAnswer(userId, roomName, password, requestId);
            // игнориуем ошибку, связанную с повторным входом в команту
            var userAndRoomExists = false;
            if (errorCode == ChatResultCode.AlreadyEnteredRoom)
            {
                userAndRoomExists = true;
                errorCode = ChatResultCode.Ok;
            }
            if (errorCode != ChatResultCode.Ok)
            {
                answers.InQueue(
                    new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                   new List<object> {new Answer(requestId, errorCode)}), lockTimeout);
                return;
            }
            answers.InQueue(
                new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                new List<object> { new Answer(requestId, ChatResultCode.Ok) }), lockTimeout);
            // в случае входа в комнату, в которую пользователь уже вошел
            // (из-за возможного сбоя клиента),
            // уведомить об этом только его самого
            if (userAndRoomExists)
            {
                var action = new UserAction
                    {
                        UserId = userId,
                        ActionType = UserActionType.RoomEnter,
                        Room = roomName,
                        TimeStamp = DateTime.Now,
                        TargetUserId = userId
                    };
                userActions.Add(action, lockTimeout);
                SendUserAction(action);
            }
        }

        public void LeaveRoomInternal(IClientCallback client, int requestId, string roomName)
        {
            var userId = GetUserId(client);
            if (userId == -1)
            {
                answers.InQueue(
                    new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                    new List<object> { new Answer(requestId, ChatResultCode.UnknownUser) }), lockTimeout);
                Logger.InfoFormat("[{0}]LeaveRoomInternal: user not found, room={1}", requestId, roomName);
                return;
            }

            MoveFromRoomInternal(client, requestId, userId, roomName);
        }

        private void MoveFromRoomInternal(IClientCallback client, int requestId, int userId, string roomName)
        {
            var user = users.Find(u => u.ID == userId, lockTimeout);
            if (user == null)
            {
                answers.InQueue(
                    new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                    new List<object> { new Answer(requestId, ChatResultCode.UnknownUser) }), lockTimeout);
                Logger.InfoFormat("[{0}]MoveFromRoomInternal: user not found, room={1}", requestId, roomName);
                return;
            }

            var room = rooms.Find(r => r.Name == roomName, lockTimeout);
            if (room == null)
            {
                answers.InQueue(
                    new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                    new List<object> { new Answer(requestId, ChatResultCode.UnknownRoom) }), lockTimeout);
                Logger.InfoFormat("[{0}]MoveFromRoomInternal: room {1} not found, user={2}", requestId, roomName, user);
                return;
            }

            answers.InQueue(
                new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                new List<object> { new Answer(requestId, ChatResultCode.Ok) }), lockTimeout);
            Logger.InfoFormat("[{0}]MoveFromRoomInternal: user {1} has left room {2}", requestId, user, roomName);

            MoveFromRoomInternal(userId, roomName);
        }

        private void MoveFromRoomInternal(int userId, string roomName)
        {
            var room = rooms.Find(r => r.Name == roomName, lockTimeout);
            if (room == null)
            {
                Logger.InfoFormat("MoveFromRoomInternal: room {0} not found, userId={1}", roomName, userId);
                return;
            }

            var userAndRoomExists = usersAndRooms.TryRemove(new Cortege2<int, int> {a = userId, b = room.Id}, lockTimeout);
            Logger.InfoFormat("MoveFromRoomInternal: user Id={0} left room {1}", userId, roomName);

            if (userAndRoomExists)
                room.UserCount--;

            var action = new UserAction
                {
                    UserId = userId,
                    ActionType = UserActionType.RoomLeave,
                    Room = roomName,
                    TimeStamp = DateTime.Now,
                    TargetUserId = userAndRoomExists ? null : (int?) userId
                };

            userActions.Add(action, lockTimeout);
            SendUserAction(action);

            // если комната пуста, то устанавливаем ей время автоматического удаления
            if (room.UserCount == 0)
                room.ExpireTime = DateTime.Now + roomExpireTime;
        }

        public void CreateRoomInternal(IClientCallback client, int requestId, Room room, bool detectOwner = true)
        {
            if (string.IsNullOrEmpty(room.Name))
            {
                answers.InQueue(
                    new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                    new List<object> { new Answer(requestId, ChatResultCode.Error) }), lockTimeout);
                return;
            }

            bool timeoutFlag;
            if (rooms.Contains(room, lockTimeout, out timeoutFlag))
            {
                answers.InQueue(
                    new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                    new List<object> { new Answer(requestId, ChatResultCode.RoomExists) }), lockTimeout);
                return;
            }

            // устанавливаем владельца
            var user = users.Find(u => u.ClientCallback == client, lockTimeout);
            if (user == null)
            {
                answers.InQueue(
                    new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                    new List<object> { new Answer(requestId, ChatResultCode.UnknownUser) }), lockTimeout);
                return;
            }

            // администратор может устанавливать любого владельца
            // а ткаже выставлять флаг сохранения комнаты даже в случае неактивности
            if (((int)user.RoleMask & (int)UserRole.Administrator) == 0)
            {
                room.Owner = user.ID;
                room.IsBound = false;
            }

            // указанный владелец не существует
            if (users.Find(u => u.ID == room.Owner, lockTimeout) == null)
                room.Owner = user.ID;

            answers.InQueue(
                new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                new List<object> { new Answer(requestId, ChatResultCode.Ok) }), lockTimeout);

            CreateRoomInternal(room);
        }

        public void CreateRoomInternal(Room room)
        {
            room.Id = freeRoomId++;
            // устанавливаем время, когда комната будет удалена из-за неактивности
            if (!room.IsBound && !room.ExpireTime.HasValue)
                room.ExpireTime = DateTime.Now + roomExpireTime;
            var newRoom = new Room(room);
            rooms.Add(newRoom, lockTimeout);
        }

        public void DestroyRoomInternal(IClientCallback client, int requestId, string roomName)
        {
            var user = users.Find(u => u.ClientCallback == client, lockTimeout);
            if (user == null)
            {
                answers.InQueue(
                    new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                    new List<object> { new Answer(requestId, ChatResultCode.UnknownUser) }), lockTimeout);
                return;
            }

            var room = rooms.Find(r => r.Name == roomName, lockTimeout);
            if (room == null)
            {
                answers.InQueue(
                    new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                    new List<object> { new Answer(requestId, ChatResultCode.UnknownRoom) }), lockTimeout);
                return;
            }

            // недостаточно прав для удаления комнаты
            if (room.Owner != user.ID && ((int)user.RoleMask & (int)UserRole.Administrator) == 0)
            {
                answers.InQueue(
                    new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                    new List<object> { new Answer(requestId, ChatResultCode.Error) }), lockTimeout);
                return;
            }

            answers.InQueue(
                new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                new List<object> { new Answer(requestId, ChatResultCode.Ok) }), lockTimeout);

            DestroyRoomInternal(room);
        }

        public void DestroyRoomInternal(Room room)
        {
            // kick all users from this room
            var userIds = usersAndRooms.GetAll(ur => ur.b == room.Id, lockTimeout).Select(ur => ur.a).ToList();
            foreach (var userId in userIds)
                MoveFromRoomInternal(userId, room.Name);
            rooms.TryRemove(room, lockTimeout);
        }

        public void SendPrivateMessageInternal(IClientCallback client, int requestId, int receiverId, string messageText)
        {
            var sender = users.Find(u => u.ClientCallback == client, lockTimeout);
            var receiver = users.Find(u => u.ID == receiverId, lockTimeout);
            if (sender == null || receiver == null)
            {
                answers.InQueue(
                    new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                    new List<object> { new Answer(requestId, ChatResultCode.UnknownUser) }), lockTimeout);
                return;
            }

            answers.InQueue(
                new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                new List<object> { new Answer(requestId, ChatResultCode.Ok) }), lockTimeout);

            var message = new Message
            {
                Sender = sender.ID,
                Receiver = receiverId,
                Text = messageText,
                TimeStamp = DateTime.Now
            };
            AddMessageToStorage(message);
            if (sender.ClientCallback != null)
                answers.InQueue(
                    new ChatAnswer(sender.ClientCallback, AnswerCode.MessageReceived, new List<object> { message }),
                    lockTimeout);
            if (receiver.ClientCallback != null)
                answers.InQueue(
                    new ChatAnswer(receiver.ClientCallback, AnswerCode.MessageReceived, new List<object> {message}),
                    lockTimeout);
        }

        public ChatResultCode SendMessageWithNoAnswer(int userId, string roomName, string messageText, int requestId)
        {
            var room = rooms.Find(r => r.Name == roomName, lockTimeout);
            if (room == null)
            {
                Logger.InfoFormat("[{0}]SendMessageWithNoAnswer: room {1} not found, user = {2}", requestId, roomName, userId);
                return ChatResultCode.UnknownRoom;
            }
            var message = new Message
            {
                Sender = userId,
                Room = roomName,
                Text = messageText,
                TimeStamp = DateTime.Now
            };
            AddMessageToStorage(message);
            SendMessage(message);
            return ChatResultCode.Ok;
        }

        public void SendMessageInternal(IClientCallback client, int requestId, string roomName, string messageText)
        {
            var userId = GetUserId(client);
            if (userId == -1)
            {
                answers.InQueue(
                    new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                    new List<object> { new Answer(requestId, ChatResultCode.UnknownUser) }), lockTimeout);
                return;
            }
            var errorCode = SendMessageWithNoAnswer(userId, roomName, messageText, requestId);
            if (errorCode != ChatResultCode.Ok)
            {
                answers.InQueue(
                    new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                    new List<object> { new Answer(requestId, errorCode) }), lockTimeout);
                return;
            }
            answers.InQueue(
                new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                               new List<object> {new Answer(requestId, ChatResultCode.Ok)}), lockTimeout);
        }

        public List<Message> GetData4GetPendingMessages(int userId, DateTime timeStamp, string roomName, int requestId, out ChatResultCode errorCode)
        {
            errorCode = ChatResultCode.Ok;
            var result = new List<Message>();
            var newMessages = GetMessagesInternal(timeStamp);
            if (string.IsNullOrEmpty(roomName))
            {
                var roomIds = usersAndRooms.GetAll(ur => ur.a == userId, lockTimeout).Select(ur => ur.b).ToList();
                var roomNames = rooms.GetAll(r => roomIds.Contains(r.Id), lockTimeout).Select(r => r.Name).ToList();
                // определяем все сообщения во всех комнатах, в которых сейчас находится пользователь
                result.AddRange(newMessages.Where(m => roomNames.Contains(m.Room)));
            }
            else
                result.AddRange(newMessages.Where(m => m.Room == roomName));
            return result;
        }

        public List<Message> GetData4GetPendingPrivateMessages(int currentUserId, int anotherUserId, DateTime timeStamp, int requestId, out ChatResultCode errorCode)
        {
            errorCode = ChatResultCode.Ok;
            var result = new List<Message>();
            var newMessages = GetMessagesInternal(timeStamp).Where(m => string.IsNullOrEmpty(m.Room)).ToList();
            result.AddRange(newMessages.Where(m => (m.Sender == currentUserId) && (anotherUserId == 0 || m.Receiver == anotherUserId)
                            || (m.Receiver == currentUserId) && (anotherUserId == 0 || m.Sender == anotherUserId)));
            return result;
        }

        public void GetPendingMessagesInternal(IClientCallback client, int requestId, DateTime timeStamp, string roomName)
        {
            var userId = GetUserId(client);
            if (userId == -1)
            {
                answers.InQueue(
                    new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                    new List<object> { new Answer(requestId, ChatResultCode.UnknownUser) }), lockTimeout);
                return;
            }
            ChatResultCode errorCode;
            var data = GetData4GetPendingMessages(userId, timeStamp, roomName, requestId, out errorCode);
            if (errorCode != ChatResultCode.Ok)
            {
                answers.InQueue(
                    new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                    new List<object> { new Answer(requestId, errorCode) }), lockTimeout);
                return;
            }
            answers.InQueue(new ChatAnswer(client, AnswerCode.PendingMessagesReceived, new List<object>
                {
                    new AnswerWithMessages
                        {
                            RequestId = requestId,
                            Status = ChatResultCode.Ok,
                            Messages = data,
                            TimeStamp = timeStamp,
                            Room = roomName
                        }
                }),
                            lockTimeout);
        }

        public void GetPendingPrivateMessagesInternal(IClientCallback client, int requestId, DateTime timeStamp, int anotherUserId)
        {
            ChatResultCode errorCode;
            var currentUserId = GetUserId(client);
            if (currentUserId == -1)
            {
                answers.InQueue(
                    new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                    new List<object> { new Answer(requestId, ChatResultCode.UnknownUser) }), lockTimeout);
                return;
            }
            var data = GetData4GetPendingPrivateMessages(currentUserId, anotherUserId, timeStamp, requestId, out errorCode);
            if (errorCode != ChatResultCode.Ok)
            {
                answers.InQueue(
                    new ChatAnswer(client, AnswerCode.RequestStatusReceived,
                                    new List<object> { new Answer(requestId, errorCode) }), lockTimeout);
                return;
            }
            answers.InQueue(new ChatAnswer(client, AnswerCode.PendingMessagesReceived, new List<object>
                {
                    new AnswerWithMessages
                        {
                            RequestId = requestId,
                            Status = ChatResultCode.Ok,
                            Messages = data,
                            TimeStamp = timeStamp,
                            Receiver = anotherUserId
                        }
                }),
                            lockTimeout);
        }

        // for internal use
        public List<Message> GetMessagesInternal(DateTime timeStamp)
        {
            var result = new List<Message>();
            var messageRooms = messages.GetKeys();
            foreach (var room in messageRooms)
            {
                var messagesInRoom = messages.ReceiveValue(room).ToList();
                for (var messageIndex = messagesInRoom.Count - 1; messageIndex >= 0; messageIndex--)
                {
                    var message = messagesInRoom[messageIndex];
                    if (message.TimeStamp <= timeStamp) // using message ordering in 'messages'
                        break;
                    result.Insert(0, new Message(message));
                }
            }
            return result;
        }

        // pushing message in local message storage in time order
        private void AddMessageToStorage(Message message)
        {
            var newMessage = new Message(message);
            if (newMessage.Receiver != 0)
                newMessage.Room = "";

            if (!messages.ContainsKey(newMessage.Room))
                messages.UpdateValues(newMessage.Room, new List<Message>());
            var messagesInRoom = messages.ReceiveValue(newMessage.Room);  // !!! unsafe
            if(messagesInRoom.Count == 0 || messagesInRoom[messagesInRoom.Count - 1].TimeStamp >= newMessage.TimeStamp)
                messagesInRoom.Add(newMessage);
            else
            {
                var index = messagesInRoom.FindLastIndex(m => m.TimeStamp <= newMessage.TimeStamp);
                messagesInRoom.Insert(index + 1, newMessage);
            }
            messages.UpdateValues(newMessage.Room, messagesInRoom);
        }

        // broadcast user inform
        private void SendUserAction(UserAction action)
        {
            bool timeoutFlag;
            var allUsers = users.GetAll(lockTimeout, out timeoutFlag);
            foreach (var user in allUsers)
            {
                if (action.TargetUserId.HasValue)
                    if (user.ID != action.TargetUserId)
                        continue;
                answers.InQueue(new ChatAnswer(user.ClientCallback, AnswerCode.UserChanged, new List<object> {action}),
                                lockTimeout);
            }
        }

        // broadcast messaging
        private void SendMessage(Message message)
        {
            bool timeoutFlag;
            var allUsers = users.GetAll(lockTimeout, out timeoutFlag);
            foreach (var user in allUsers)
            {
                answers.InQueue(
                    new ChatAnswer(user.ClientCallback, AnswerCode.MessageReceived, new List<object> {message}),
                    lockTimeout);
            }
        }

        #endregion

        // sending
        private void DoSend()
        {
            while (!isStopping)
            {
                bool timeoutFlag;
                var pendingAnswers = answers.ExtractAll(lockTimeout, out timeoutFlag);
                if (timeoutFlag)
                    continue;
                foreach (var answer in pendingAnswers)
                {
                    /*if (GetUserId(answer.Client) == -1)
                    {
                        Logger.Error("Client already removed - do not answer");
                        continue;
                    }*/
                    try
                    {
                        switch (answer.Code)
                        {
                            case AnswerCode.RequestStatusReceived:
                                answer.Client.RequestStatusReceived((Answer) answer.Arguments[0]);
                                break;
                            case AnswerCode.AllUsersReceived:
                                answer.Client.AllUsersReceived((AnswerWithUsers) answer.Arguments[0]);
                                break;
                            case AnswerCode.UserChanged:
                                answer.Client.UserChanged((UserAction) answer.Arguments[0]);
                                break;
                            case AnswerCode.RoomsReceived:
                                answer.Client.RoomsReceived((AnswerWithRooms) answer.Arguments[0]);
                                break;
                            case AnswerCode.MessageReceived:
                                answer.Client.MessageReceived((Message) answer.Arguments[0]);
                                break;
                            case AnswerCode.PendingMessagesReceived:
                                answer.Client.PendingMessagesReceived((AnswerWithMessages) answer.Arguments[0]);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        // исключаем замусоривание лога - видимо, для WCF обрывы связи считаются нормальным явлением
                        //Logger.ErrorFormat("DoSend exception: {0}", ex);
                        ExitInternal(GetUserId(answer.Client));
                    }
                }
                Thread.Sleep(100);
            }
        }

        private void UsersAndRoomsUpdate()
        {
            while (!isStopping)
            {
                bool timeoutFlag;
                var allRooms = rooms.GetAll(lockTimeout, out timeoutFlag).FindAll(r => !r.IsBound && r.ExpireTime.HasValue && DateTime.Now > r.ExpireTime.Value);
                foreach (var room in allRooms)
                    DestroyRoomInternal(room);
                Thread.Sleep(1000);
            }
        }
    }
}


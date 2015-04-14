using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TradeSharp.Chat.Client.BL;
using TradeSharp.Chat.Client.Form;
using TradeSharp.Chat.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;
using Message = TradeSharp.Chat.Contract.Message;
using Timer = System.Windows.Forms.Timer;

// ReSharper disable LocalizableElement
namespace TradeSharp.Chat.Client.Control
{
    public partial class ChatControl : UserControl
    {
        public event ChatSenderStable.ChatOpsDel LoginRequested;

        // чат (клиентский движок)
        private ChatControlBackEnd engine;
        public ChatControlBackEnd Engine
        {
            get { return engine; }
            set
            {
                if (value == null) return;
                if (engine == null)
                {
                    engine = value;
                    Initialize();
                }
                else
                    throw new Exception("Ошибка инициализации чата (ChatControl.Engine.set)");
            }
        }

        // комнаты с непрочитанным сообщениями
        private readonly List<TabPage> highlightedPages = new List<TabPage>();
        // таймер для анимации
        private readonly Timer animationTimer = new Timer();
        private bool animationPhase;
        private volatile bool isStopping = true;

        public ChatControl()
        {
            InitializeComponent();
            userTreeView.NodeMouseClick += (sender, args) => userTreeView.SelectedNode = args.Node;
            /*if (IsEmbedded)
                exitButton.Hide();*/
            Enable(false);
            userTreeView.Nodes.Add("others", "Остальные (0)", 2, 2);
            if (!ChatSettings.Instance.ShowLog)
                DeletePage(logTabPage);
            if (ChatSettings.Instance.AutoLogin)
                onOffButton.Hide();
        }

        // отключение элемента управления от чата
        public void StopControl()
        {
            isStopping = true;
            engine.Entered -= Entered;
            engine.Exited -= Exited;
            engine.RequestQueueFilled -= RequestQueueFilled;
            engine.RequestQueueCleared -= RequestQueueCleared;
            engine.RequestErrorOccurred -= RequestErrorOccurred;
            engine.Logged -= Logged;
            engine.UserInfoExReceived -= UserInfoExReceived;
            engine.AllUsersReceived -= AllUsersReceived;
            engine.UserEntered -= UserEntered;
            engine.UserExited -= UserExited;
            engine.UserEnteredRoom -= UserEnteredRoom;
            engine.UserLeftRoom -= UserLeftRoom;
            engine.MessageReceived -= MessageReceived;
            engine.PendingMessagesReceived -= PendingMessagesReceived;
            animationTimer.Stop();
        }

        // запуск чата (ChatControlBackEnd)
        // arguments[0] содержит Id пользователя и перекрывает userId
        public void Start(int userId = 0, List<string> arguments = null)
        {
            if (engine == null)
            {
                engine = new ChatControlBackEnd();
                Initialize();
            }
            engine.Start();
            if (arguments != null && arguments.Count >= 1)
                userId = arguments[0].ToInt(0);
            if (userId != 0)
            {
                ChatSettings.Instance.Id = userId;
                if (ChatSettings.Instance.AutoLogin)
                    engine.Login(userId);
            }

            animationTimer.Tick += OnAnimationTimerTicked;
            animationTimer.Interval = 500;
            animationTimer.Start();

            // 4 debug
            if (arguments != null && arguments.Count >= 3)
                engine.StartSpam(arguments[1], arguments[2].ToInt(1000));
        }

        // остановка чата (ChatControlBackEnd)
        public void Stop()
        {
            try
            {
                engine.Stop();
                StopControl();
                engine = null;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка остановки чата (ChatControl.Stop)", ex);
            }
        }

        // интерактивный вход
        public void Login()
        {
            try
            {
                if (engine.IsOnline() || IsEmbedded)
                    return;
                var loginForm = new LoginForm();
                if (loginForm.ShowDialog(this) == DialogResult.Cancel)
                    return;
                engine.Login(loginForm.GetId());
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка входа в чат (ChatControl.Login)", ex);
            }
        }

        // автоматический вход
        public void Login(int userId)
        {
            try
            {
                if (engine.IsOnline())
                    return;
                engine.Login(userId);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка входа в чат (ChatControl.Login)", ex);
            }
        }

        // выход
        public void Logout()
        {
            try
            {
                engine.Logout();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка выхода из чата (ChatControl.Logout)", ex);
            }
        }

        public void HideLogPage()
        {
            DeletePage(logTabPage);
            logTabPage = null;
        }

        // chat handlers
        private void AllUsersReceived(List<User> users, string room)
        {
            if (isStopping)
                return;
            try
            {
                Invoke(new Action<List<User>, string>(OnAllUsersReceived), users, room);
            }
            catch (Exception)
            {
            }
        }

        private void OnAllUsersReceived(List<User> users, string room)
        {
            if (users == null)
                return;
            if (string.IsNullOrEmpty(room))
            {
                ClearRoom("this");
                ClearRoom("others");
                foreach (var user in users)
                    AddToRoom("others", user.ID.ToString(), user.NickName);
            }
            else
            {
                if (tabControl.SelectedTab == null)
                    return;
                if (tabControl.SelectedTab.Name != room)
                    return;
                ClearRoom("this");
                foreach (var user in users)
                {
                    AddToRoom("this", user.ID.ToString(), user.NickName);
                    RemoveFromRoom("others", user.ID.ToString());
                }
            }
        }

        private void UserEntered(UserAction action)
        {
            if (isStopping)
                return;
            try
            {
                Invoke(new Action<int>(OnUserEnter), action.UserId);
            }
            catch (Exception)
            {
            }
        }

        private void OnUserEnter(int user)
        {
            var userData = AllUsers.Instance.GetUser(user) ?? new User { ID = user, Login = user.ToString() };
            AddToRoom("others", user.ToString(), userData.NickName);
        }

        private void UserExited(UserAction action)
        {
            if (isStopping)
                return;
            try
            {
                Invoke(new Action<int>(OnUserExit), action.UserId);
            }
            catch (Exception)
            {
            }
        }

        private void OnUserExit(int user)
        {
            RemoveFromRoom("this", user.ToString());
            RemoveFromRoom("others", user.ToString());
        }

        private void UserEnteredRoom(UserAction action)
        {
            if (isStopping)
                return;
            try
            {
                Invoke(new Action<UserAction>(OnUserEnterRoom), action);
            }
            catch (Exception)
            {
            }
        }

        private void OnUserEnterRoom(UserAction action)
        {
            if (action.UserId == engine.CurrentUserId)
            {
                if (!tabControl.TabPages.ContainsKey(action.Room)) // always false (obsolete code)
                    CreatePageForRoom(action.Room); // (obsolete code) пользователь не может зайти в комнату без собственной инициативы,
                                                    // а значит, вкладка этой комнаты уже существует
                UpdateUserTree();
                UpdateImageIndexForPage(action.Room);
            }

            // print message about enetering
            var user = AllUsers.Instance.GetUser(action.UserId);
            if (user == null)
                return;
            var page = tabControl.TabPages[action.Room];
            if (page != null && tabControl.SelectedTab != null && tabControl.SelectedTab.Name == action.Room)
            {
                var chatControl = (ChatMessagingControl) page.Controls[0];
                PrintMessage(chatControl,
                             new Message { Text = user.NickName + " вошел в комнату", TimeStamp = action.TimeStamp },
                             ChatMessagingControl.MessageStyle.Notify);
            }

            // add entered user to tree branch
            if (tabControl.SelectedTab == null || tabControl.SelectedTab.Name != action.Room)
                return;
            AddToRoom("this", action.UserId.ToString(), user.NickName);
            RemoveFromRoom("others", action.UserId.ToString());
        }

        private void UserLeftRoom(UserAction action)
        {
            if (isStopping)
                return;
            try
            {
                Invoke(new Action<UserAction>(OnUserLeaveRoom), action);
            }
            catch (Exception)
            {
            }
        }

        private void OnUserLeaveRoom(UserAction action)
        {
            var page = tabControl.TabPages[action.Room];
            ChatMessagingControl chatControl = null;
            if (page != null)
                chatControl = (ChatMessagingControl) page.Controls[0];

            if (action.UserId == engine.CurrentUserId)
            {
                highlightedPages.Remove(tabControl.TabPages[action.Room]);
                if (chatControl != null)
                    ShowErrorInChatControl(chatControl, action.TimeStamp, "Вы перестали быть участником этой комнаты");
                UpdateImageIndexForPage(action.Room);
                return;
            }

            // print message about leaving
            var user = AllUsers.Instance.GetUser(action.UserId);
            if (user == null)
                return;
            if (chatControl != null)
                PrintMessage(chatControl,
                             new Message {Text = user.NickName + " покинул комнату", TimeStamp = action.TimeStamp},
                             ChatMessagingControl.MessageStyle.Notify);

            // remove left user from tree branch
            if (tabControl.SelectedTab == null || tabControl.SelectedTab.Name != action.Room)
                return;
            RemoveFromRoom("this", action.UserId.ToString());
            AddToRoom("others", action.UserId.ToString(), user.NickName);
        }

        private void MessageReceived(Message message)
        {
            if (isStopping)
                return;
            try
            {
                BeginInvoke(new Action<Message>(OnMessageReceive), message);
            }
            catch (Exception)
            {
            }
        }

        // распечатка одиночного сообщения и при необходимости создание привата
        private void OnMessageReceive(Message message)
        {
            // ид и ключ другого пользователя (только для приватных сообщений)
            var userId = message.Sender == engine.CurrentUserId ? message.Receiver : message.Sender;
            var userKey = userId.ToString();
            var page = string.IsNullOrEmpty(message.Room)
                           ? tabControl.TabPages[userKey]
                           : tabControl.TabPages[message.Room];
            var printMessage = true; // признак распечатки сообщения
            if (string.IsNullOrEmpty(message.Room) && page == null) // приват закрыт - открываем
            {
                CreatePrivatePage(userId);
                printMessage = false;
                page = tabControl.TabPages[userKey];
            }
            if (page == null)
                return; // какая-то странная ошибка
            if (tabControl.SelectedTab != page)
                if (!highlightedPages.Contains(page))
                    highlightedPages.Add(page);
            // сообщение для новой вкладки будет распечатано после вызова GetPendingMessages,
            // поэтому делаем проверку, чтобы не получить дубликата последнего сообщения
            if (printMessage)
                PrintMessage((ChatMessagingControl) page.Controls[0], message);
        }

        private void PendingMessagesReceived(List<Message> messages, string room)
        {
            if (isStopping)
                return;
            try
            {
                if (string.IsNullOrEmpty(room))
                    BeginInvoke(new Action<List<Message>>(OnPendingPrivateMessagesReceive), messages);
                else
                    BeginInvoke(new Action<List<Message>, string>(OnPendingMessagesReceive), messages, room);
            }
            catch (Exception)
            {
            }
        }

        // распечатка сообщений в одной комнате (каждое сообщение не проверяется на комнату)
        private void OnPendingMessagesReceive(List<Message> messages, string room)
        {
            var page = tabControl.TabPages[room];
            if (!string.IsNullOrEmpty(room) && (page == null || messages.Count == 0))
                return;
            if (tabControl.SelectedTab != page)
            {
                if (!highlightedPages.Contains(page))
                    highlightedPages.Add(page);
            }
            else
                RemoveFromHighlightedPages(page); // на всякий непредвиденный случай исключем страницу из мигающих
            var chatControl = (ChatMessagingControl) page.Controls[0];
            foreach (var message in messages)
                PrintMessage(chatControl, message);
        }

        private void OnPendingPrivateMessagesReceive(List<Message> messages)
        {
            foreach (var message in messages)
            {
                var userId = message.Sender == engine.CurrentUserId ? message.Receiver : message.Sender;
                var userKey = userId.ToString();
                var page = tabControl.TabPages[userKey];
                // создаем вкладку
                if (page == null)
                    CreatePrivatePage(userId, false);
                page = tabControl.TabPages[userKey];
                if (page == null)
                    continue;
                // распечатываем сообщение
                PrintMessage((ChatMessagingControl) page.Controls[0], message);
                // подсвечиваем вкладку
                if (tabControl.SelectedTab != page)
                {
                    if (!highlightedPages.Contains(page))
                        highlightedPages.Add(page);
                }
                else
                    RemoveFromHighlightedPages(page);
            }
        }

        private void Entered()
        {
            if (isStopping)
                return;
            try
            {
                Invoke(new Action(OnEnter));
            }
            catch (Exception)
            {
            }
        }

        private void OnEnter()
        {
            Enable(true);
            onOffButton.Text = "Выход";
            onOffButton.ImageIndex = 8;
            roomsLinkLabel.Text = engine.IsOnline() ? "Комнаты..." : "Вход";
        }

        private void Exited()
        {
            if (isStopping)
                return;
            try
            {
                Invoke(new Action(OnExit));
            }
            catch (Exception)
            {
            }
        }

        private void OnExit()
        {
            Enable(false);
            onOffButton.Text = "Вход";
            onOffButton.ImageIndex = 7;
            roomsLinkLabel.Text = engine.IsOnline() ? "Комнаты..." : "Вход";
        }

        private void Enable(bool enabled)
        {
            foreach (TabPage page in tabControl.TabPages)
            {
                if (page == logTabPage)
                    continue;
                var chatControl = (ChatMessagingControl) page.Controls[0];
                chatControl.Enable(enabled);
            }
            roomButton.Enabled = enabled;
        }

        private void RequestQueueFilled()
        {
            if (isStopping)
                return;
            try
            {
                Invoke(new Action(() => toolStripStatusLabel.Text = "Ожидание ответа от сервера"));
            }
            catch (Exception)
            {
            }
        }

        private void RequestQueueCleared()
        {
            if (isStopping)
                return;
            try
            {
                Invoke(new Action(() => toolStripStatusLabel.Text = ""));
            }
            catch (Exception)
            {
            }
        }

        private void RequestErrorOccurred(ChatRequest request)
        {
            if (isStopping)
                return;
            try
            {
                BeginInvoke(new Action<ChatRequest>(OnRequestError), request);
            }
            catch (Exception)
            {
            }
        }

        private void OnRequestError(ChatRequest request)
        {
            if (request.Code == RequestCode.EnterRoom)
            {
                var roomName = request.Arguments[0].ToString();
                /*MessageBox.Show(this, "Невозможно выполнить вход в комнату " + roomName + "\nПричина: " +
                                      Answer.GetChatResultCodeString(request.Status), "Ошибка", MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);*/
                ShowErrorInRoom(roomName, DateTime.Now,
                                "Невозможно выполнить вход в комнату " + roomName + ". Причина: " +
                                Answer.GetChatResultCodeString(request.Status));
            }
            else if (request.Code == RequestCode.GetAllUsers && request.Arguments.Count != 0)
            {
                var roomName = request.Arguments[0].ToString();
                ShowErrorInRoom(roomName, DateTime.Now,
                                "Невозможно определить пользователей комнаты " + roomName + ". Причина: " +
                                Answer.GetChatResultCodeString(request.Status));
            }
            else
                ShowErrorInChat(DateTime.Now, request.Status);
        }

        private void Logged(string message)
        {
            if (isStopping)
                return;
            try
            {
                Invoke(new Action<string>(OnLog), message);
            }
            catch (Exception)
            {
            }
        }

        private void OnLog(string message)
        {
            //toolStripStatusLabel.Text = message;
            logRichTextBox.AppendText(message + Environment.NewLine);
        }

        private void UserInfoExReceived(UserInfoEx info)
        {
            if (isStopping)
                return;
            try
            {
                BeginInvoke(new Action<UserInfoEx>(OnUserInfoExReceive), info);
            }
            catch (Exception)
            {
            }
        }

        private void OnUserInfoExReceive(UserInfoEx info)
        {
            if (info == null)
                return;
            var form = new UserInfoForm();
            var user = AllUsers.Instance.GetUser(info.Id) ?? new User {ID = info.Id};
            form.SetUser(user);
            form.SetUserInfoEx(info);
            //form.SetReadOnly(engine.CurrentUserId != info.Id);
            form.SetReadOnly(true);
            if (form.ShowDialog(this) == DialogResult.Cancel)
                return;
            engine.SetUserInfoEx(form.GetUserInfo());
        }

        // UI handlers
        private void RoomButtonClick(object sender, EventArgs e)
        {
            var roomForm = new RoomListForm(engine);
            if (roomForm.ShowDialog(this) == DialogResult.Cancel)
                return;
            var room = roomForm.GetRoom();
            if (room == null)
                return;

            if (tabControl.TabPages.ContainsKey(room.Name))
            {
                tabControl.SelectTab(room.Name);
                return;
            }

            var password = "";
            if (!string.IsNullOrEmpty(room.Password))
            {
                password = engine.GetRoomPassword(room.Name);
                if(password == null)
                {
                    DialogResult result;
                    password = Dialogs.ShowInputDialog("Вход в комнату " + room.Name, "Введите пароль: ", true,
                                                       out result);
                    if (result == DialogResult.Cancel)
                        return;
                }
            }

            CreatePageForRoom(room.Name);

            engine.EnterRoom(room.Name, password);
            
            // если это первая вкладка, то SelectedIndexChanged почему-то не вызывается,
            // поэтому вызываем самостоятельно
            tabControl.SelectTab(room.Name);
            if (tabControl.TabPages.Count == 1)
                TabControlSelectedIndexChanged(this, new EventArgs());
        }

        private void SettingsButtonClick(object sender, EventArgs e)
        {
            var form = new SettingsForm();
            if (form.ShowDialog(this) == DialogResult.Cancel)
                return;
            if (ChatSettings.Instance.ShowLog != tabControl.TabPages.Contains(logTabPage))
                if (ChatSettings.Instance.ShowLog)
                    tabControl.TabPages.Insert(0, logTabPage);
                else
                    DeletePage(logTabPage);
            foreach (TabPage page in tabControl.TabPages)
            {
                if (page == logTabPage)
                    continue;
                var chatControl = (ChatMessagingControl) page.Controls[0];
                chatControl.Reset(engine);
            }
            onOffButton.Visible = !ChatSettings.Instance.AutoLogin;
            if(ChatSettings.Instance.AutoLogin && ChatSettings.Instance.Id != 0 && !engine.IsOnline())
                engine.Login(ChatSettings.Instance.Id);
        }

        private void TabControlSelectedIndexChanged(object sender, EventArgs e)
        {
            // возможно повторное срабатывание UpdateUserTree при открытии вкладки
            UpdateUserTree();
            RemoveFromHighlightedPages(tabControl.SelectedTab);
        }

        private void LeaveConversationButtonClicked(string key)
        {
            var page = tabControl.TabPages[key];
            if (page == null)
                return;
            DeletePage(page);
            if (!engine.GetEnteredRooms().Contains(key))
                return;
            ChatSettings.Instance.RemoveRoom(key);
            engine.LeaveRoom(key);
        }

        private void OnOffButtonClick(object sender, EventArgs e)
        {
            if (engine.IsOnline())
                Logout();
            else
            {
                if (LoginRequested != null)
                    LoginRequested();
                else
                {
                    if (ChatSettings.Instance.Id == 0)
                        Login();
                    else
                        Login(ChatSettings.Instance.Id);
                }
            }
        }

        private void UserInfoItemClick(Object sender, EventArgs e)
        {
            var node = userTreeView.SelectedNode;
            if (node == null)
                return;
            if (node.Parent == null)
                return;
            var user = AllUsers.Instance.GetUser(node.Name.ToInt());
            if (user == null)
                return;
            engine.GetUserInfoEx(user.ID);
        }

        private void RoomsLinkLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (engine.IsOnline())
                RoomButtonClick(this, new EventArgs());
            else
                OnOffButtonClick(this, new EventArgs());
        }

        private void OnAnimationTimerTicked(Object myObject, EventArgs myEventArgs)
        {
            if (isStopping)
                return;
            try
            {
                Invoke(new Action(AnimationTimerTicked));
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void AnimationTimerTicked()
        {
            animationPhase = !animationPhase;
            foreach (var page in highlightedPages)
                UpdateImageIndexForPage(page.Name);
        }

        private void UpdateUserTree()
        {
            ClearRoom("others");
            var users = engine.GetOnlineUsers();
            foreach (var user in users)
                AddToRoom("others", user.ID.ToString(), user.NickName);
            if (tabControl.SelectedTab == logTabPage || tabControl.SelectedTab == null || tabControl.SelectedTab.Name != tabControl.SelectedTab.Text)
            {
                userTreeView.Nodes.RemoveByKey("this");
                return;
            }
            var roomName = tabControl.SelectedTab.Name;
            var thisText = roomName + " (0)";
            var thisNode = userTreeView.Nodes["this"];
            if (thisNode == null)
                userTreeView.Nodes.Insert(0, "this", thisText, 2, 2);
            else
            {
                thisNode.Text = thisText; // избыточно
                ClearRoom("this");
            }

            if (engine.IsOnline())
                engine.GetAllUsers(roomName);
        }

        private void UpdateImageIndexForPage(string key)
        {
            var isRoom = engine.GetEnteredRooms().Contains(key);
            var index = isRoom ? (engine.IsOnline() ? 9 : 10) : (engine.IsOnline() ? 2 : 5);
            if (animationPhase)
                if (highlightedPages.Select(p => p.Name).Contains(key))
                    index = 4;
            var page = tabControl.TabPages[key];
            if (page != null)
                page.ImageIndex = index;
        }

        // internals
        private void Initialize()
        {
            // установка обработчиков
            engine.Entered += Entered;
            engine.Exited += Exited;
            engine.RequestQueueFilled += RequestQueueFilled;
            engine.RequestQueueCleared += RequestQueueCleared;
            engine.RequestErrorOccurred += RequestErrorOccurred;
            engine.Logged += Logged;
            engine.UserInfoExReceived += UserInfoExReceived;
            engine.AllUsersReceived += AllUsersReceived;
            engine.UserEntered += UserEntered;
            engine.UserExited += UserExited;
            engine.UserEnteredRoom += UserEnteredRoom;
            engine.UserLeftRoom += UserLeftRoom;
            engine.MessageReceived += MessageReceived;
            engine.PendingMessagesReceived += PendingMessagesReceived;
            isStopping = false;

            // визуализация состояния чата
            Enable(engine.IsOnline());
            var users = engine.GetOnlineUsers();
            foreach (var user in users)
                AddToRoom("others", user.ID.ToString(), user.NickName);
            foreach (var room in ChatSettings.Instance.Rooms)
                CreatePageForRoom(room);
            // TODO: enter last viewed room
            if (tabControl.TabPages.Count != 0)
                UpdateUserTree();
            // дублирование в OnEnter, OnExit
            onOffButton.Text = engine.IsOnline() ? "Выход" : "Вход";
            onOffButton.ImageIndex = engine.IsOnline() ? 8 : 7;
            roomsLinkLabel.Text = engine.IsOnline() ? "Комнаты..." : "Вход";
        }

        private void RemoveFromHighlightedPages(TabPage page)
        {
            if(highlightedPages.Remove(page))
                UpdateImageIndexForPage(page.Name);
        }

        private void ShowErrorInDialog(ChatResultCode code, string comment = "")
        {
            ShowErrorInDialog("Error: " + Answer.GetChatResultCodeString(code) + "\nComment: " + comment);
        }

        private void ShowErrorInDialog(string text)
        {
            MessageBox.Show(this, text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void ShowErrorInChat(DateTime time, ChatResultCode code, string comment = "")
        {
            if (code == ChatResultCode.Ok && string.IsNullOrEmpty(comment))
                return;
            ShowErrorInChat(time, Answer.GetChatResultCodeString(code) + ". " + comment);
        }

        private void ShowErrorInChat(DateTime time, string text)
        {
            foreach (TabPage page in tabControl.TabPages)
            {
                if (page == logTabPage)
                    continue;
                var chatControl = (ChatMessagingControl) page.Controls[0];
                ShowErrorInChatControl(chatControl, time, text);
            }
        }

        private void ShowErrorInRoom(string roomName, DateTime time, string text)
        {
            foreach (TabPage page in tabControl.TabPages)
            {
                if (page.Name == roomName)
                {
                    var chatControl = (ChatMessagingControl) page.Controls[0];
                    ShowErrorInChatControl(chatControl, time, text);
                    return;
                }
            }
        }

        private void ShowErrorInChatControl(ChatMessagingControl chatControl, DateTime time, string text)
        {
            PrintMessage(chatControl, new Message {Text = text, TimeStamp = time},
                         ChatMessagingControl.MessageStyle.Error);
        }

        // post room message in ChatMessagingControl w/o informing (highlighting)
        private void PrintMessage(Message message)
        {
            var index = tabControl.TabPages.IndexOfKey(message.Room);
            if (index == -1)
                return;
            var page = tabControl.TabPages[index];
            var chatControl = (ChatMessagingControl) page.Controls[0];
            PrintMessage(chatControl, message);
        }

        private void PrintMessage(ChatMessagingControl chatControl, Message message, ChatMessagingControl.MessageStyle style = ChatMessagingControl.MessageStyle.None)
        {
            chatControl.AddMessage(message, engine, style);
        }

        private static string CreateNewLabelWithCounter(string oldStr, bool? increment)
        {
            var oldLabelAndCounter = GetLabelAndCounter(oldStr);
            if (!oldLabelAndCounter.HasValue)
                return oldStr;
            var value = oldLabelAndCounter.Value.b;
            if (increment.HasValue)
                if (increment.Value)
                    value++;
                else
                    value--;
            else
                value = 0;
            return oldLabelAndCounter.Value.a + "(" + value + ")";
        }

        private static Cortege2<string, int>? GetLabelAndCounter(string labelWithCounter)
        {
            var bracketBeginIndex = labelWithCounter.LastIndexOf('(');
            var bracketEndIndex = labelWithCounter.LastIndexOf(')');
            if (bracketBeginIndex == -1 || bracketEndIndex == -1 || bracketBeginIndex >= bracketEndIndex)
                return null;
            var counterStr = labelWithCounter.Substring(bracketBeginIndex + 1, bracketEndIndex - bracketBeginIndex - 1);
            int value;
            Int32.TryParse(counterStr, out value);
            return new Cortege2<string, int>(labelWithCounter.Substring(0, bracketBeginIndex), value);
        }

        private void AddToRoom(string branchKey, string userKey, string userName)
        {
            var rootNode = userTreeView.Nodes[branchKey];
            if (rootNode == null)
                return;
            var nodes = rootNode.Nodes;
            if (nodes.ContainsKey(userKey))
                return;
            nodes.Add(userKey, userName, 3, 3);
            rootNode.Text = CreateNewLabelWithCounter(rootNode.Text, true);
        }

        private void RemoveFromRoom(string branchKey, string userKey)
        {
            var rootNode = userTreeView.Nodes[branchKey];
            if (rootNode == null)
                return;
            var nodes = rootNode.Nodes;
            if (!nodes.ContainsKey(userKey))
                return;
            nodes.RemoveByKey(userKey);
            rootNode.Text = CreateNewLabelWithCounter(rootNode.Text, false);
        }

        private void ClearRoom(string branchKey)
        {
            var rootNode = userTreeView.Nodes[branchKey];
            if (rootNode == null)
                return;
            rootNode.Nodes.Clear();
            rootNode.Text = CreateNewLabelWithCounter(rootNode.Text, null);
        }

        private void CreatePageForRoom(string roomName)
        {
            CreatePage(roomName, roomName);
            var newPage = tabControl.TabPages[roomName];
            var newChatControl = new ChatMessagingControl { Dock = DockStyle.Fill };
            newChatControl.MessageEnteted += message => engine.SendMessage(roomName, message);
            newChatControl.ConversationLeft += () => LeaveConversationButtonClicked(roomName);
            newChatControl.Enable(engine.IsOnline());
            newPage.Controls.Add(newChatControl);
            UpdateImageIndexForPage(roomName);
            // posting old messages
            engine.GetMessages(roomName).ForEach(m => PrintMessage(newChatControl, m));
        }

        private void CreatePrivatePage(int userId, bool getPendingMessages = true)
        {
            var user = AllUsers.Instance.GetUser(userId);
            if (user == null)
                return;
            var userKey = userId.ToString();
            CreatePage(userKey, user.NickName);
            var newPage = tabControl.TabPages[userKey];
            var newChatControl = new ChatMessagingControl { Dock = DockStyle.Fill };
            newChatControl.MessageEnteted += message => engine.SendPrivateMessage(userId, message);
            newChatControl.ConversationLeft += () => LeaveConversationButtonClicked(userKey);
            newChatControl.Enable(engine.IsOnline());
            newPage.Controls.Add(newChatControl);
            UpdateImageIndexForPage(userKey);
            // posting old messages
            engine.GetPrivateMessages(userId).ForEach(m => PrintMessage(newChatControl, m));
            if (getPendingMessages)
                engine.GetPendingPrivateMessages();
        }

        private void CreatePage(string key, string text)
        {
            tableLayoutPanel1.Hide();
            tabControl.Show();
            tabControl.TabPages.Add(key, text);
        }

        private void DeletePage(string roomName)
        {
            RemoveFromHighlightedPages(tabControl.TabPages[roomName]);
            tabControl.DeselectTab(roomName);
            tabControl.TabPages.RemoveByKey(roomName);
            if (tabControl.TabPages.Count == 0)
            {
                tabControl.Hide();
                tableLayoutPanel1.Show();
            }
        }

        private void DeletePage(TabPage page)
        {
            RemoveFromHighlightedPages(page);
            tabControl.DeselectTab(page);
            tabControl.TabPages.Remove(page);
            if (tabControl.TabPages.Count == 0)
            {
                tabControl.Hide();
                tableLayoutPanel1.Show();
            }
        }

        private void BeginPrivateConversationToolStripMenuItemClick(object sender, EventArgs e)
        {
            var node = userTreeView.SelectedNode;
            if (node == null)
                return;
            if (node.Parent == null)
                return;
            var userId = node.Name.ToInt();
            if (userId == engine.CurrentUserId)
                return;
            var user = AllUsers.Instance.GetUser(node.Name.ToInt());
            if (user == null)
                return;

            CreatePrivatePage(user.ID);

            tabControl.SelectTab(node.Name);
            if (tabControl.TabPages.Count == 1)
                TabControlSelectedIndexChanged(this, new EventArgs());
        }
    }
}
// ReSharper restore LocalizableElement

using System;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using TradeSharp.Chat.Contract;
using TradeSharp.Util;

namespace TradeSharp.Chat.Client.BL
{
    class ChatClient : DuplexClientBase<IChat>, IChat
    {
        public ChatClient(object callbackObject, string endPoint)
            : base(callbackObject, endPoint)
        {
            /*var binding = (WSDualHttpBinding) Endpoint.Binding;
            var originalUri = Endpoint.Address.Uri;
            var newUriString = originalUri.Scheme + "://" + originalUri.Host + ":" + FindPort() +
                               originalUri.AbsolutePath;
            binding.ClientBaseAddress = new Uri(newUriString);*/
        }

        private static int FindPort()
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 0);
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(endPoint);
                var local = (IPEndPoint)socket.LocalEndPoint;
                return local.Port;
            }
        }

        public int GetAllUsers(string room = "")
        {
            return Channel.GetAllUsers(room);
        }

        public int Enter(User user)
        {
            return Channel.Enter(user);
        }

        public int Exit()
        {
            return Channel.Exit();
        }

        public int GetRooms()
        {
            return Channel.GetRooms();
        }

        public int EnterRoom(string room, string password = "")
        {
            return Channel.EnterRoom(room, password);
        }

        public int MoveToRoom(int user, string room, string password = "")
        {
            return Channel.MoveToRoom(user, room, password);
        }

        public int LeaveRoom(string room)
        {
            return Channel.LeaveRoom(room);
        }

        public int CreateRoom(Room room)
        {
            return Channel.CreateRoom(room);
        }

        public int DestroyRoom(string room)
        {
            return Channel.DestroyRoom(room);
        }

        public int SendPrivateMessage(int receiver, string message)
        {
            return Channel.SendPrivateMessage(receiver, message);
        }

        public int SendMessage(string room, string message)
        {
            return Channel.SendMessage(room, message);
        }

        public int GetPendingMessages(DateTime timeStamp, string room = "")
        {
            return Channel.GetPendingMessages(timeStamp, room);
        }

        public int GetPendingPrivateMessages(DateTime timeStamp, int receiver = 0)
        {
            return Channel.GetPendingPrivateMessages(timeStamp, receiver);
        }

        public void Ping()
        {
            Channel.Ping();
        }
    }

    public class ChatClientStable : IChat
    {
        public delegate void ConnectionDel();

        public volatile bool HasConnection;

        public ConnectionDel Connected;

        public ConnectionDel Disconnected;
        
        private ChatClient internalObject;

        private ChatClientCallback callbackObject;

        private string endPoint;

        public ChatClientStable(ChatClientCallback callbackObject, string endPoint)
        {
            this.callbackObject = callbackObject;
            this.endPoint = endPoint;
            RenewConnection();
        }

        public bool CheckConnection()
        {
            if (internalObject != null)
                return true;
            RenewConnection();
            return internalObject != null;
        }

        public bool RenewConnection()
        {
            try
            {
                internalObject = new ChatClient(callbackObject, endPoint);
                internalObject.Ping();
                if (Connected != null)
                    Connected();
                HasConnection = true;
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("RenewConnection", ex);
                internalObject = null;
                HasConnection = false;
                return false;
            }
        }

        public int GetAllUsers(string room = "")
        {
            if (!CheckConnection())
                return 0;
            try
            {
                return internalObject.GetAllUsers(room);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                OnDisconnect();
                return 0;
            }
        }

        public int Enter(User user)
        {
            if (!CheckConnection())
                return 0;
            try
            {
                return internalObject.Enter(user);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                OnDisconnect();
                return 0;
            }
        }

        public int Exit()
        {
            if (!CheckConnection())
                return 0;
            try
            {
                return internalObject.Exit();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                OnDisconnect();
                return 0;
            }
        }

        public int GetRooms()
        {
            if (!CheckConnection())
                return 0;
            try
            {
                return internalObject.GetRooms();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                OnDisconnect();
                return 0;
            }
        }

        public int EnterRoom(string room, string password = "")
        {
            if (!CheckConnection())
                return 0;
            try
            {
                return internalObject.EnterRoom(room, password);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                OnDisconnect();
                return 0;
            }
        }

        public int MoveToRoom(int user, string room, string password = "")
        {
            if (!CheckConnection())
                return 0;
            try
            {
                return internalObject.MoveToRoom(user, room, password);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                OnDisconnect();
                return 0;
            }
        }

        public int LeaveRoom(string room)
        {
            if (!CheckConnection())
                return 0;
            try
            {
                return internalObject.LeaveRoom(room);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                OnDisconnect();
                return 0;
            }
        }

        public int CreateRoom(Room room)
        {
            if (!CheckConnection())
                return 0;
            try
            {
                return internalObject.CreateRoom(room);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                OnDisconnect();
                return 0;
            }
        }

        public int DestroyRoom(string room)
        {
            if (!CheckConnection())
                return 0;
            try
            {
                return internalObject.DestroyRoom(room);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                OnDisconnect();
                return 0;
            }
        }

        public int SendPrivateMessage(int receiver, string message)
        {
            if (!CheckConnection())
                return 0;
            try
            {
                return internalObject.SendPrivateMessage(receiver, message);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                OnDisconnect();
                return 0;
            }
        }

        public int SendMessage(string room, string message)
        {
            if (!CheckConnection())
                return 0;
            try
            {
                return internalObject.SendMessage(room, message);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                OnDisconnect();
                return 0;
            }
        }

        public int GetPendingMessages(DateTime timeStamp, string room = "")
        {
            if (!CheckConnection())
                return 0;
            try
            {
                return internalObject.GetPendingMessages(timeStamp, room);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                OnDisconnect();
                return 0;
            }
        }

        public int GetPendingPrivateMessages(DateTime timeStamp, int receiver = 0)
        {
            if (!CheckConnection())
                return 0;
            try
            {
                return internalObject.GetPendingPrivateMessages(timeStamp, receiver);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                OnDisconnect();
                return 0;
            }
        }

        // каким-то чудом при выключенном сервере internalObject != null, срабатывает ping и вызывается Disconnected()
        public void Ping()
        {
            if (!CheckConnection())
                return;
            try
            {
                internalObject.Ping();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                OnDisconnect();
            }
        }

        private void OnDisconnect()
        {
            internalObject = null;
            HasConnection = false;
            if (Disconnected != null)
                Disconnected();
            RenewConnection();
        }
    }
}

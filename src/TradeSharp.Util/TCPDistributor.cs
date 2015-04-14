using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TradeSharp.Util
{
    public class TcpDistributor
    {
        private readonly Encoding encoding;
        private Socket socket;
        private readonly List<Socket> clients = new List<Socket>();
        private readonly ReaderWriterLock clientsLocker = new ReaderWriterLock();
        private const int LockTimeout = 1000;

        public TcpDistributor(int port, Encoding encoding)
        {
            this.encoding = encoding;
            socket = new Socket(AddressFamily.InterNetwork,
                                SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Any, port));            
        }

        public TcpDistributor(int port)
        {
            encoding = Encoding.ASCII;
            socket = new Socket(AddressFamily.InterNetwork,
                                SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Any, port));
        }

        public void Start()
        {
            const int maxConn = 2000; // (int)socket.GetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.MaxConnections);
            socket.Listen(maxConn);
            socket.BeginAccept(AcceptCallback, socket);
        }

        public void Stop()
        {
            socket.Close();
            socket = null;
        }

        /// <summary>
        /// подключить клиента и занести его в список
        /// </summary>
        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                var sock = socket.EndAccept(ar);
                clientsLocker.AcquireWriterLock(LockTimeout);
                try
                {
                    clients.Add(sock);
                }
                finally
                {
                    clientsLocker.ReleaseWriterLock();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в AcceptCallback", ex);
            }
            finally
            {
                try
                {
                    socket.BeginAccept(AcceptCallback, socket);
                }
                catch
                {
                }
            }
        }

        public void DistributeStringData(string str)
        {
            if (string.IsNullOrEmpty(str)) return;
            var bytes = encoding.GetBytes(str);
            var staleClients = new List<Socket>();

            // получить доступ на чтение к списку клиентов
            try
            {
                clientsLocker.AcquireReaderLock(LockTimeout);
            }
            catch (Exception)
            {
                Logger.Error("DistributeStringData - read timeout");
                return;
            }

            // сделать копию списка клиентов
            List<Socket> clientsCopies;
            try
            {
                clientsCopies = clients.ToList();
            }
            finally
            {
                clientsLocker.ReleaseReaderLock();
            }

            // попытаться раздать клиентам сообщение
            foreach (var client in clientsCopies)
            {
                try
                {
                    if (client.Send(bytes) != bytes.Length)
                        staleClients.Add(client);
                }
                catch
                {
                    staleClients.Add(client);
                }
            }

            if (staleClients.Count == 0) return;
                
            // удалить отвалившихся клиентов
            try
            {
                clientsLocker.AcquireWriterLock(LockTimeout);
            }
            catch (Exception)
            {
                Logger.Error("DistributeStringData - write timeout");
                return;
            }

            try
            {                    
                foreach (var client in staleClients)                
                    clients.Remove(client);                
            }
            catch (Exception ex)
            {
                Logger.Error("DistributeStringData - impossible error", ex);
                return;
            }
            finally
            {
                clientsLocker.ReleaseWriterLock();
            }
        }
    }    


    /// <summary>
    /// An Asynchronous TCP Server that makes use of system managed threads
    /// and callbacks to stop the server ever locking up.
    /// </summary>
    public class TcpDistributorOld
    {
        private readonly TcpListener tcpListener;
        private readonly List<Client> clients = new List<Client>();
        private readonly ReaderWriterLock lockClient = new ReaderWriterLock();
        private const int LockTimeout = 1000;
        
        public TcpDistributorOld(int port)
        {
            tcpListener = new TcpListener(port);
        }

        /// <summary>
        /// Constructor for a new server using an IPAddress and Port
        /// </summary>
        /// <param name="localaddr">The Local IP Address for the server.</param>
        /// <param name="port">The port for the server.</param>
        public TcpDistributorOld(IPAddress localaddr, int port)
            : this()
        {
            tcpListener = new TcpListener(localaddr, port);
        }

        /// <summary>
        /// Constructor for a new server using an end point
        /// </summary>
        /// <param name="localEP">The local end point for the server.</param>
        public TcpDistributorOld(IPEndPoint localEP)
            : this()
        {
            tcpListener = new TcpListener(localEP);
        }

        /// <summary>
        /// Private constructor for the common constructor operations.
        /// </summary>
        private TcpDistributorOld()
        {
            Encoding = Encoding.Default;
            clients = new List<Client>();
        }


        private Encoding encoding = Encoding.ASCII;
        /// <summary>
        /// The encoding to use when sending / receiving strings.
        /// </summary>
        public Encoding Encoding
        {
            get { return encoding; }
            set { encoding = value; }
        }

        /// <summary>
        /// Starts the TCP Server listening for new clients.
        /// </summary>
        public void Start()
        {
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(AcceptTcpClientCallback, null);
        }

        /// <summary>
        /// Stops the TCP Server listening for new clients and disconnects
        /// any currently connected clients.
        /// </summary>
        public void Stop()
        {
            tcpListener.Stop();
            try
            {
                lockClient.AcquireReaderLock(LockTimeout);
            }
            catch (Exception)
            {
                Logger.Error("TcpDistributor::Stop() - read timeout");
                return;
            }
            try
            {
                foreach (var client in clients)
                {
                    client.TcpClient.Client.Disconnect(false);
                }
                try
                {
                    lockClient.UpgradeToWriterLock(LockTimeout);
                }
                catch (Exception)
                {
                    Logger.Error("TcpDistributor::Stop() - write timeout");
                    return;
                }
                clients.Clear();            
            }
            finally
            {
                lockClient.ReleaseLock();
            }
        }

        /// <summary>
        /// Writes a string to a given TCP Client
        /// </summary>
        /// <param name="tcpClient">The client to write to</param>
        /// <param name="data">The string to send.</param>
        public bool Write(TcpClient tcpClient, string data)
        {
            var bytes = Encoding.GetBytes(data);
            return Write(tcpClient, bytes);
        }

        /// <summary>
        /// Writes a string to all clients connected.
        /// </summary>
        /// <param name="data">The string to send.</param>
        public void DistributeStringData(string data)
        {
            try
            {
                lockClient.AcquireReaderLock(LockTimeout);
            }
            catch (Exception)
            {
                Logger.Error("DistributeStringData::DistributeStringData read timeout");
            }

            try
            {
                for (var i = 0; i < clients.Count; i++)
                {
                    var client = clients[i];
                    if (Write(client.TcpClient, data)) continue;

                    // удалить отвалившегося клиента
                    LockCookie cookie;
                    try
                    {
                        cookie = lockClient.UpgradeToWriterLock(LockTimeout);
                    }
                    catch (Exception)
                    {
                        Logger.Error("DistributeStringData::DistributeStringData write timeout");
                        continue;
                    }
                    try
                    {
                        clients.RemoveAt(i);
                        i--;
                    }
                    finally
                    {
                        lockClient.DowngradeFromWriterLock(ref cookie);
                    }
                }
            }
            finally
            {
                lockClient.ReleaseLock();
            }            
        }

        /// <summary>
        /// Writes a byte array to a given TCP Client
        /// </summary>
        /// <param name="tcpClient">The client to write to</param>
        /// <param name="bytes">The bytes to send</param>
        public bool Write(TcpClient tcpClient, byte[] bytes)
        {
            try
            {
                var networkStream = tcpClient.GetStream();
                networkStream.BeginWrite(bytes, 0, bytes.Length, WriteCallback, tcpClient);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("TcpDistributor::Write() error", ex);
                return false;
            }
        }

        /// <summary>
        /// Callback for the write opertaion.
        /// </summary>
        /// <param name="result">The async result object</param>
        private void WriteCallback(IAsyncResult result)
        {
            var tcpClient = result.AsyncState as TcpClient;
            if (tcpClient == null) return;
            try
            {
                var networkStream = tcpClient.GetStream();
                networkStream.EndWrite(result);
            }
            catch (Exception ex)
            {
                Logger.Error("TcpDistributor: write callback error", ex);
            }
        }

        /// <summary>
        /// Callback for the accept tcp client opertaion.
        /// </summary>
        /// <param name="result">The async result object</param>
        private void AcceptTcpClientCallback(IAsyncResult result)
        {
            TcpClient tcpClient;
            try
            {
                tcpClient = tcpListener.EndAcceptTcpClient(result);
            }
            catch (Exception ex)
            {
                Logger.Error("TcpDistributor::AcceptTcpClientCallback - end accept error", ex);
                return;
            }
            var buffer = new byte[tcpClient.ReceiveBufferSize];
            var client = new Client(tcpClient, buffer);

            try
            {
                lockClient.AcquireWriterLock(LockTimeout);
            }
            catch (Exception)
            {
                Logger.Error("TcpDistributor::AcceptTcpClientCallback - write timeout");
                return;
            }

            try
            {
                clients.Add(client);
            }
            finally
            {
                lockClient.ReleaseWriterLock();
            }

            
            //var networkStream = client.NetworkStream;
            try
            {
                //networkStream.BeginRead(client.Buffer, 0, client.Buffer.Length, ReadCallback, client);
                tcpListener.BeginAcceptTcpClient(AcceptTcpClientCallback, null);
            }
            catch (Exception ex)
            {
                Logger.Error("AcceptTcpClientCallback::BeginRead - BeginAccept error", ex);
            }
        }

        ///// <summary>
        ///// Callback for the read opertaion.
        ///// </summary>
        ///// <param name="result">The async result object</param>
        //private void ReadCallback(IAsyncResult result)
        //{
        //    var client = result.AsyncState as Client;
        //    if (client == null) return;
        //    NetworkStream networkStream = null;
        //    int read;
        //    try
        //    {
        //        networkStream = client.NetworkStream;
        //        read = networkStream.EndRead(result);
        //    }
        //    catch (Exception)
        //    {
        //        read = 0;
        //    }
        //    if (read == 0)
        //    {
        //        try
        //        {
        //            lockClient.AcquireWriterLock(LockTimeout);
        //        }
        //        catch (Exception)
        //        {
        //            Logger.Error("TcpDistributor::ReadCallback() - write timeout");
        //            return;
        //        }
        //        try
        //        {
        //            clients.Remove(client);
        //        }
        //        finally
        //        {
        //            lockClient.ReleaseWriterLock();
        //        }
        //        return;
        //    }
            
        //    if (networkStream != null)
        //        try
        //        {
        //            /*string data = */Encoding.GetString(client.Buffer, 0, read);
        //            // do something with the data object here.
        //            networkStream.BeginRead(client.Buffer, 0, client.Buffer.Length, ReadCallback, client);
        //        }
        //        catch (Exception ex)
        //        {
        //            Logger.Error("TcpDistributor: read callback error", ex);
        //        }
        //}
    }

    /// <summary>
    /// Internal class to join the TCP client and buffer together 
    /// for easy management in the server
    /// </summary>
    internal class Client
    {
        /// <summary>
        /// Constructor for a new Client
        /// </summary>
        /// <param name="tcpClient">The TCP client</param>
        /// <param name="buffer">The byte array buffer</param>
        public Client(TcpClient tcpClient, byte[] buffer)
        {
            if (tcpClient == null) throw new ArgumentNullException("tcpClient");
            if (buffer == null) throw new ArgumentNullException("buffer");
            TcpClient = tcpClient;
            Buffer = buffer;
        }

        /// <summary>
        /// Gets the TCP Client
        /// </summary>
        public TcpClient TcpClient { get; private set; }

        /// <summary>
        /// Gets the Buffer.
        /// </summary>
        public byte[] Buffer { get; private set; }

        /// <summary>
        /// Gets the network stream
        /// </summary>
        public NetworkStream NetworkStream { get { return TcpClient.GetStream(); } }
    }        
}

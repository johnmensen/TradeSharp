using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TradeSharp.Util;

namespace TradeSharp.SiteBridge.Lib.Distribution
{
    public class TCPManager
    {
        private readonly TcpListener listener;

        private readonly static ManualResetEvent tcpClientConnected = new ManualResetEvent(false);

        private readonly static ManualResetEvent tcpReceiving = new ManualResetEvent(false);

        private volatile bool isStopping;

        private readonly Thread threadListen;

        private readonly Encoding encoding = Encoding.ASCII;

        /// <summary>
        /// список подключившихся клиентов - 
        /// получателей котировок
        /// </summary>
        private readonly List<TcpClientInfo> clients = new List<TcpClientInfo>();

        private readonly ReaderWriterLock clientsLocker = new ReaderWriterLock();

        private const int LockTimeout = 2000;

        public OnDataReceivedDel OnDataReceived;

        public TCPManager(int port)
        {
            //var ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
            //var ipLocalEndPoint = new IPEndPoint(ipAddress, port);
            Logger.InfoFormat("TCPDistributor: старт прослушки порта {0}", port);
            try
            {
                if (port < IPEndPoint.MinPort ||
                    port > IPEndPoint.MaxPort)
                    throw new Exception(
                        string.Format("Port {0} is out of range: {1}..{2}",
                        port, IPEndPoint.MinPort, IPEndPoint.MaxPort));
                listener = new TcpListener(port);
                listener.Start();
                Logger.InfoFormat("TCPDistributor: слушает порт {0}", port);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в TCPManager.ctor", ex);
                return;
            }

            threadListen = new Thread(ListenLoop);
            threadListen.Start();
        }

        public void Stop()
        {
            isStopping = true;
            tcpClientConnected.Set();
            threadListen.Join();
            for (var i = 0; i < clients.Count; i++)
            {
                clients[i].receiveThread.Join();
            }
        }

        private void ListenLoop()
        {
            while (!isStopping)
            {
                tcpClientConnected.Reset();
                listener.BeginAcceptTcpClient(DoAcceptTcpClientCallback, listener);
                tcpClientConnected.WaitOne();
            }
        }

        private void DoAcceptTcpClientCallback(IAsyncResult ar)
        {
            var lst = (TcpListener)ar.AsyncState;
            TcpClient client = lst.EndAcceptTcpClient(ar);
            try
            {
                clientsLocker.AcquireWriterLock(LockTimeout);
            }
            catch
            {
                Logger.Error("TCPDistributor: deadlock при добавлении клиента");
                tcpClientConnected.Set();
                return;
            }
            try
            {
                var cliInf = new TcpClientInfo(client);
                clients.Add(cliInf);
                Logger.InfoFormat("Клиент [{0}:{1}] подключился",
                                  ((IPEndPoint)client.Client.LocalEndPoint).Address,
                                  ((IPEndPoint)client.Client.LocalEndPoint).Port);

                cliInf.receiveThread = new Thread(ThreadReceiveLoop);
                cliInf.receiveThread.Start(cliInf);
            }
            catch (Exception ex)
            {
                Logger.Error("TCPDistributor:DoAcceptTcpClientCallback", ex);
            }
            finally
            {
                clientsLocker.ReleaseWriterLock();
            }

            // signal the calling thread to continue.
            tcpClientConnected.Set();
        }

        public void DistributeString(string str)
        {
            var buffData = encoding.GetBytes(str);

            try
            {
                clientsLocker.AcquireReaderLock(LockTimeout);
            }
            catch
            {
                Logger.Error("TCPDistributor: deadlock при добавлении клиента");
                return;
            }
            try
            {
                for (var i = 0; i < clients.Count; i++)
                {
                    if (!clients[i].client.Connected)
                    {
                        // удалить подвисшего клиента из списка получателей
                        LockCookie cock;
                        try
                        {
                            cock = clientsLocker.UpgradeToWriterLock(LockTimeout);
                        }
                        catch
                        {
                            Logger.Error("TCPDistributor: deadlock при попытке удаления клиента");
                            continue;
                        }
                        clients.RemoveAt(i);
                        clientsLocker.DowngradeFromWriterLock(ref cock);
                        i--;
                        continue;
                    }

                    // отправить клиенту пачку котировок
                    SendToClient(clients[i].client, buffData);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("TCPDistributor:DistributeString", ex);
            }
            finally
            {
                clientsLocker.ReleaseLock();
            }
        }

        private static void SendToClient(TcpClient client, byte[] data)
        {
            try
            {
                client.Client.Send(data);
            }
            catch (Exception ex)
            {
                Logger.Error("TCPDistributor:SendToClient", ex);
            }
        }

        #region Receive Data Loop
        private void ThreadReceiveLoop(object clientObj)
        {
            var client = (TcpClientInfo)clientObj;

            while (!isStopping)
            {
                tcpReceiving.Reset();
                if (!client.client.Client.Connected) break;
                try
                {
                    client.client.Client.BeginReceive(client.buffer, 0, TcpClientInfo.BufSize, SocketFlags.None,
                                                      new AsyncCallback(ReceiveData), client);
                }
                catch (SocketException)
                {// клиент мог отвалиться
                    RemoveClientThreadSafe(client);
                }
                tcpReceiving.WaitOne();
            }
        }

        private void ReceiveData(IAsyncResult ar)
        {
            var cli = (TcpClientInfo)ar.AsyncState;
            var remote = cli.client.Client;
            int recv;
            try
            {
                recv = remote.EndReceive(ar);
            }
            catch (SocketException)
            {
                //Logger.Error("TCPDistributor:ReceiveData SocketException");
                tcpReceiving.Set();
                return;
            }
            catch (Exception ex)
            {
                Logger.Error("TCPDistributor:ReceiveData", ex);
                tcpReceiving.Set();
                return;
            }
            if (recv > 0)
            {
                string stringData = encoding.GetString(cli.buffer, 0, recv);
                if (OnDataReceived != null)
                    OnDataReceived(stringData, cli.address, cli.port);
            }

            tcpReceiving.Set();
        }

        private bool RemoveClientThreadSafe(TcpClientInfo client)
        {
            try
            {
                clientsLocker.AcquireWriterLock(LockTimeout);
            }
            catch (ApplicationException)
            {
                Logger.Error("RemoveClientThreadSafe - lock timeout");
                return false;
            }
            try
            {
                clients.Remove(client);
                return true;
            }
            finally
            {
                clientsLocker.ReleaseWriterLock();
            }
        }
        #endregion
    }

    public delegate void OnDataReceivedDel(string data, string addr, int port);

    class TcpClientInfo
    {
        public const int BufSize = 4096;
        public byte[] buffer = new byte[BufSize];
        public TcpClient client;
        public Thread receiveThread;

        public string address;
        public int port;

        public TcpClientInfo(TcpClient client)
        {
            this.client = client;
            address = ((IPEndPoint)client.Client.LocalEndPoint).Address.ToString();
            port = ((IPEndPoint)client.Client.LocalEndPoint).Port;
        }
    }
}

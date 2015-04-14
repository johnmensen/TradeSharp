using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TradeSharp.Util
{
    public class TcpReceiverClient : IDisposable
    {
        private IPAddress[] addresses;
        private readonly int port;
        private readonly WaitHandle addressesSet;
        private TcpClient tcpClient;
        private int failedConnectionCount;
        private volatile bool isStopping;
        private volatile bool connectionIsInProcess;
        private Thread reviveThread;

        private readonly int reconnectTimeoutMils = AppConfig.GetIntParam(
            "TcpReceiverClient.Timeout", 10 * 1000);

        private readonly bool workingHoursTimeoutOnly = AppConfig.GetBooleanParam(
            "TcpReceiverClient.Timeout.WorkingHoursOnly", true);

        private readonly ThreadSafeTimeStamp lastTimeMessageReceived = new ThreadSafeTimeStamp();

        private TcpPacketReceivedDel onTcpPacketReceived;
        public event TcpPacketReceivedDel OnTcpPacketReceived
        {
            add { onTcpPacketReceived += value; }
            remove { onTcpPacketReceived -= value; }
        }

        private readonly FloodSafeLogger logNoFlood = new FloodSafeLogger(1000 * 60);

        private const int MsgConnectionEstablishing = 1;
        private const int MsgConnectionEstablished = 2;
        private const int MsgDisconnected = 3;

        /// <summary>
        /// Construct a new client from a known IP Address
        /// </summary>
        /// <param name="address">The IP Address of the server</param>
        /// <param name="port">The port of the server</param>
        public TcpReceiverClient(IPAddress address, int port)
            : this(new[] { address }, port)
        {
        }

        /// <summary>
        /// Construct a new client where multiple IP Addresses for
        /// the same client are known.
        /// </summary>
        /// <param name="addresses">The array of known IP Addresses</param>
        /// <param name="port">The port of the server</param>
        public TcpReceiverClient(IPAddress[] addresses, int port)
            : this(port)
        {
            this.addresses = addresses;
        }

        /// <summary>
        /// Construct a new client where the address or host name of
        /// the server is known.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or address of the server</param>
        /// <param name="port">The port of the server</param>
        public TcpReceiverClient(string hostNameOrAddress, int port)
            : this(port)
        {
            addressesSet = new AutoResetEvent(false);
            Dns.BeginGetHostAddresses(hostNameOrAddress, GetHostAddressesCallback, null);
        }

        /// <summary>
        /// Private constuctor called by other constuctors
        /// for common operations.
        /// </summary>
        /// <param name="port"></param>
        private TcpReceiverClient(int port)
        {
            if (port < 0)
                throw new ArgumentException();
            this.port = port;
            tcpClient = new TcpClient();
            Encoding = Encoding.Default;
        }

        /// <summary>
        /// The encoding used to encode/decode string when sending and receiving.
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Attempts to connect to one of the specified IP Addresses
        /// </summary>
        public void Connect()
        {
            reviveThread = new Thread(PollRoutine);
            if (addressesSet != null)                
                addressesSet.WaitOne();
            // set the failed connection count to 0
            Interlocked.Exchange(ref failedConnectionCount, 0);
            // start the async connect operation
            tcpClient.BeginConnect(addresses, port, ConnectCallback, null);
            connectionIsInProcess = true;
            reviveThread.Start();
        }

        private void PollRoutine()
        {
            lastTimeMessageReceived.Touch();
            while (!isStopping)
            {
                Thread.Sleep(200);
                var isOk = !IsTimeout() && (IsConnected(tcpClient.Client) || connectionIsInProcess);
                if (isOk) continue;
                logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info, 
                    MsgDisconnected, 1000 * 60 * 30, "TcpReceiverClient:Disconnected");
                try
                {
                    tcpClient.Close();
                    tcpClient = new TcpClient();
                    tcpClient.BeginConnect(addresses, port, ConnectCallback, null);
                    lastTimeMessageReceived.Touch();
                    connectionIsInProcess = true;
                }
                catch (Exception ex)
                {
                }
            }
        }

        private bool IsTimeout()
        {
            var time = DateTime.Now;
            if (workingHoursTimeoutOnly)
                if (!WorkingDay.Instance.IsWorkingDay(time)) 
                    return false;
            var lastTime = lastTimeMessageReceived.GetLastHit();
            var milsSince = (time - lastTime).TotalMilliseconds;
            return milsSince >= reconnectTimeoutMils;
        }

        private static bool IsConnected(Socket socket)
        {
            if (!socket.Connected)
                return false;
            try
            {
                return !(socket.Available == 0 && socket.Poll(1, SelectMode.SelectRead));
            }
            catch (Exception)
            {                
                return false;
            }
        }

        public void Stop()
        {
            isStopping = true;
            reviveThread.Join();
            tcpClient.Close();
        }

        /// <summary>
        /// Callback for Connect operation
        /// </summary>
        /// <param name="result">The AsyncResult object</param>
        private void ConnectCallback(IAsyncResult result)
        {
            try
            {
                try
                {
                    tcpClient.EndConnect(result);
                }
                catch (Exception)
                {
                    // increment the failed connection count in a thread safe way
                    Interlocked.Increment(ref failedConnectionCount);
                    if (failedConnectionCount >= addresses.Length)
                    {
                        // we have failed to connect to all the IP Addresses
                        // connection has failed overall.
                        return;
                    }
                }
                try
                {
                    logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info, 
                        MsgConnectionEstablishing, 1000 * 60 * 30, "TcpReceiverClient:Connection establishing...");
                    // we are connected successfully.
                    var networkStream = tcpClient.GetStream();
                    var buffer = new byte[tcpClient.ReceiveBufferSize];
                    // now we are connected start asyn read operation.
                    networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
                    logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info, 
                        MsgConnectionEstablished, 1000 * 60 * 30, "TcpReceiverClient:Connection established");
                }
                catch (Exception ex)
                {
                    Logger.Error("TcpReceiverClient: get stream error", ex);
                }
               
            }
            finally
            {
                connectionIsInProcess = false;
            }
        }

        /// <summary>
        /// Callback for Read operation
        /// </summary>
        /// <param name="result">The AsyncResult object</param>
        private void ReadCallback(IAsyncResult result)
        {
            int read;
            NetworkStream networkStream;
            try
            {
                networkStream = tcpClient.GetStream();
                read = networkStream.EndRead(result);
            }
            catch
            {
                // an error has occured when reading
                return;
            }

            if (read == 0)
            {
                //The connection has been closed.
                return;
            }

            try
            {
                var buffer = result.AsyncState as byte[];
                if (buffer != null && buffer.Length > 0)
                {
                    var data = Encoding.GetString(buffer, 0, read);
                    lastTimeMessageReceived.Touch();

                    if (onTcpPacketReceived != null)
                        onTcpPacketReceived(data);

                    // then start reading from the network again.
                    networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("TcpReceiverClient: error in ReadCallback", ex);
            }
        }

        /// <summary>
        /// Callback for Get Host Addresses operation
        /// </summary>
        /// <param name="result">The AsyncResult object</param>
        private void GetHostAddressesCallback(IAsyncResult result)
        {
            addresses = Dns.EndGetHostAddresses(result);
            //Signal the addresses are now set
            ((AutoResetEvent)addressesSet).Set();
        }

        public void Dispose()
        {
            Stop();
        }
    }

    public delegate void TcpPacketReceivedDel(string data);
}

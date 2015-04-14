using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TradeSharp.QuoteService.Distribution
{
    public class TcpServer
    {
        private readonly TcpListener listener;
        private bool stopFlag;
        private readonly Thread listenThread;
        private NetworkStream clientStream;
        private readonly Encoding encoder;
        private const int BUFFER_LENGTH = 25000;
        private readonly byte[] buffer = new byte[BUFFER_LENGTH];

        public delegate void MessageDel(string msg, TcpClient client);

        public MessageDel onMessage;

        public TcpServer(int port)
        {
            listener = new TcpListener(port);
            listenThread = new Thread(ListenThreadFun);
            listenThread.Start();
            encoder = new ASCIIEncoding();
        }

        public TcpServer(int port, Encoding enc)
        {
            listener = new TcpListener(port);
            listenThread = new Thread(ListenThreadFun);
            listenThread.Start();
            encoder = enc;
        }

        public void Stop()
        {
            listener.Stop();
            stopFlag = true;
            listenThread.Join();
        }

        public bool SendToAll(string msg)
        {
            if (clientStream != null)
            {
                var buf = encoder.GetBytes(msg);
                clientStream.Write(buf, 0, buf.Length);
                clientStream.Flush();
                return true;
            }
            return false;
        }

        public bool SendToAll(byte[] msg)
        {
            if (clientStream != null)
            {
                clientStream.Write(msg, 0, msg.Length);
                clientStream.Flush();
                return true;
            }
            return false;
        }

        public bool SendToClient(string msg, TcpClient client)
        {
            var stream = client.GetStream();
            if (stream != null)
            {
                var buf = encoder.GetBytes(msg);
                stream.Write(buf, 0, buf.Length);
                stream.Flush();
                return true;
            }
            return false;
        }

        private void ListenThreadFun()
        {
            listener.Start();
            while (!stopFlag)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    var clientThread = new Thread(HandleClientComm);
                    clientThread.Start(client);
                }
                catch (SocketException)
                {
                    break;
                }
            }
        }

        private void HandleClientComm(object client)
        {
            var tcpClient = (TcpClient)client;
            clientStream = tcpClient.GetStream();

            while (true)
            {
                int bytesRead;

                try
                {
                    bytesRead = clientStream.Read(buffer, 0, BUFFER_LENGTH);
                }
                catch
                {
                    //a socket error has occured
                    break;
                }

                if (bytesRead == 0)
                {
                    break;
                }

                if (onMessage != null)
                    onMessage(encoder.GetString(buffer, 0, bytesRead), tcpClient);
            }

            tcpClient.Close();
            clientStream = null;
        }
    }

}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.QuoteService.Feeder
{
    /// <summary>
    /// прослушивает заданный порт UDP, получая
    /// котировки от советника МТ4
    /// </summary>
    class UdpFeeder : IQuoteFeeder
    {
        private readonly int targetPort;
       
        private volatile bool stopFlag;
        
        private Thread listenThread;
        
        private QuotesReceived onQuotesReceived;
        
        private readonly FloodSafeLogger logNoFlood = new FloodSafeLogger(1000 * 60);

        private const int LogMsgNewUDPMessage = 1;

        private const int LogMsgNewParsingError = 2;

        private const int LogMsgGotQuotes = 3;

        private const int LogMsgGotData = 4;

        private UdpClient client;

        private readonly bool parseQuoteMt4FeederFormat = AppConfig.GetBooleanParam("parseQuoteMt4FeederFormat", false);

        public event QuotesReceived OnQuotesReceived
        {
            add { onQuotesReceived += value; }
            remove { onQuotesReceived -= value; }
        }

        public UdpFeeder(int targetPort)
        {
            this.targetPort = targetPort;
            Logger.Info("Слушает UDP: " + targetPort);
        }

        public void SendMessage(string msg, string targetHost, int targetPort)
        {
            if (client == null) return;
            var buffer = Encoding.ASCII.GetBytes(msg);
            client.Send(buffer, buffer.Length, new IPEndPoint(IPAddress.Parse(targetHost), targetPort));
        }

        public void Start()
        {
            client = new UdpClient(targetPort);
            listenThread = new Thread(Listen);
            listenThread.Start();
        }

        public void Stop()
        {
            if (listenThread == null)
                return;
            stopFlag = true;
            client.Close();
            listenThread = null;
        }        

        private void Listen(object obj)
        {
            while (!stopFlag)
            {
                try
                {
                    var ep = new IPEndPoint(0, 0);
                    var data = client.Receive(ref ep);
                    logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info,
                        LogMsgGotData, 1000 * 60 * 60, "Получены данные: {0}",
                        data == null ? "null" : data.Length.ToString());
                    // парсинг котировок
                    if (onQuotesReceived != null)
                        ParseAndSendQuotes(data, data.Length);
                }
                catch (Exception ex)
                {
                    Logger.Error("Exception while receive quotes from socket", ex);
                }
            }
            Logger.Info("UDPFeeder - quit listen");
        }

        private void ParseAndSendQuotes(byte[] data, int count)
        {
            if (count == 0) return;

            List<string> names = null;
            List<QuoteData> quotes = null;

            if (parseQuoteMt4FeederFormat)
                names = ParseQuotesMt4Style(data, ref quotes);
            else
                QuoteFeederParser.ParseQuotes(data, count, out names, out quotes);
            
            if (onQuotesReceived != null)
                onQuotesReceived(names, quotes);
        }

        private List<string> ParseQuotesMt4Style(byte[] data, ref List<QuoteData> quotes)
        {
            string str;
            try
            {
                str = Encoding.ASCII.GetString(data);
            }
            catch
            {
                logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info,
                    LogMsgNewParsingError, 1000 * 60 * 60 * 8,
                    "UDP feeder: error converting [{0}] bytes to ASCII string", data.Length);
                return new List<string>();
            }
            
            var parts = str.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 4)
            {
                var quoteNames = new List<string> {parts[1]};
                var bid = parts[2].ToFloatUniformSafe();
                var ask = parts[3].ToFloatUniformSafe();

                if (bid.HasValue && ask.HasValue)
                    quotes = new List<QuoteData>
                    {
                        new QuoteData(bid.Value, ask.Value, DateTime.Now)
                    };
                logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info, LogMsgNewUDPMessage,
                    1000 * 60 * 60, "UDP feeder: parsed {0}, {1}, {2}",
                    quoteNames[0], bid, ask);
                return quoteNames;
            }
            logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info, LogMsgNewParsingError,
                    1000 * 60 * 60, "UDP feeder: string should contain 4 parts, got this: " + str);
            return new List<string>();
        }
    }        
}

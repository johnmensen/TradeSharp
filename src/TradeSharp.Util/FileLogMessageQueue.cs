using System;
using System.IO;
using System.Text;
using System.Threading;

namespace TradeSharp.Util
{
    /// <summary>
    /// класс асинхронно принимает сообщения, направляемые в лог,
    /// и, в отдельном потоке, сохраняет их в файлы
    /// </summary>
    public class FileMessageLogQueue
    {
        private static FileMessageLogQueue instance;
        public static FileMessageLogQueue Instance
        {
            get { return instance ?? (instance = new FileMessageLogQueue()); }
        }

        private FileMessageLogQueue()
        {
        }

        private readonly ThreadSafeQueue<MessageLogItem> queue = new ThreadSafeQueue<MessageLogItem>();
        private const int QueueTimeout = 500;
        private static readonly int savingIntervalMils = AppConfig.GetIntParam("FileStorage.SavingIntervalMils",
                                                                               5 * 1000);
        private static readonly int timerIntervalMils = AppConfig.GetIntParam("FileStorage.TimerIntervalMils",
                                                                               2 * 100);
        private static readonly string fileNameFormat = AppConfig.GetStringParam("FileStorage.FileNameFormat",
                                                                               "{0}\\msg{1:yyyy_MM}.txt");

        private static readonly Encoding encoding = EncodingFriendlyName.GetEncodingByName(
            AppConfig.GetStringParam("FileStorage.Encoding", "ASCII"), Encoding.ASCII);
        private volatile bool isStopping;
        private Thread savingThread;

        public void Start()
        {
            savingThread = new Thread(SavingRoutine);
            savingThread.Start();
        }

        public void Stop()
        {
            isStopping = true;
            savingThread.Join();
            SaveMessages();
        }

        public void LogMessage(string msg)
        {
            queue.InQueue(new MessageLogItem(msg), QueueTimeout);
        }

        private void SavingRoutine()
        {
            var countIters = savingIntervalMils / timerIntervalMils;
            var iterator = countIters;
            while (!isStopping)
            {
                Thread.Sleep(timerIntervalMils);
                iterator--;
                if (iterator == 0)
                {
                    SaveMessages();
                    iterator = countIters;
                }
            }
        }

        private void SaveMessages()
        {
            bool timeout;
            var msgList = queue.ExtractAll(QueueTimeout, out timeout);
            if (msgList.Count == 0) return;

            StreamWriter stream = null;
            string lastFileName = string.Empty;
            try
            {
                foreach (var msg in msgList)
                {
                    var fileName = string.Format(fileNameFormat,
                                                 ExecutablePath.ExecPath, msg.time);
                    if (fileName != lastFileName)
                    {
                        if (stream != null) stream.Dispose();
                        stream = new StreamWriter(fileName, true, encoding);
                        lastFileName = fileName;
                    }
                    stream.WriteLine(msg.ToString());
                }
            }
            finally
            {
                if (stream != null) stream.Dispose();
            }

        }
    }

    class FileMessageLogItem
    {
        public DateTime time;
        public string msg;

        public FileMessageLogItem(string msg)
        {
            this.msg = msg;
            time = DateTime.Now;
        }

        public override string ToString()
        {
            return string.Format("{0:yyyy-MM-dd HH:mm:ss} {1}", DateTime.Now, msg);
        }
    }
}

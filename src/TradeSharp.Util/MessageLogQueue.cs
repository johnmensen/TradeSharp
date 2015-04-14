using System;
using System.Threading;

namespace TradeSharp.Util
{
    public delegate void SaveMessageItemDlg(DateTime time, object msg);

    /// <summary>
    /// класс асинхронно принимает сообщения, направляемые в лог,
    /// и, в отдельном потоке, сохраняет их в файлы
    /// </summary>
    public class MessageLogQueue
    {
        private readonly ThreadSafeQueue<MessageLogItem> queue = new ThreadSafeQueue<MessageLogItem>();
        private const int QueueTimeout = 500;
        private readonly int savingIntervalMils;
        private readonly int timerIntervalMils;        
        private volatile bool isStopping;
        private Thread savingThread;
        private readonly SaveMessageItemDlg saveMessageItem;

        public MessageLogQueue(int savingIntervalMils, int timerIntervalMils, 
            SaveMessageItemDlg saveMessageItem)
        {
            this.savingIntervalMils = savingIntervalMils;
            this.timerIntervalMils = timerIntervalMils;
            this.saveMessageItem = saveMessageItem;
        }

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
            var countIters = savingIntervalMils/timerIntervalMils;
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

            foreach (var msg in msgList)
            {
                saveMessageItem(msg.time, msg.msg);
            }            
        }
    }

    class MessageLogItem
    {
        public DateTime time;
        public object msg;

        public MessageLogItem(object msg)
        {
            this.msg = msg;
            time = DateTime.Now;
        }

        public override string ToString()
        {
            return string.Format("[{0:dd.MM.yyyy HH:mm:ss}] {1}", time, msg);
        }
    }
}

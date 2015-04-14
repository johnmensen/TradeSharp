using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TradeSharp.Chat.Contract;
using TradeSharp.Util;

namespace TradeSharp.Chat.Client.BL
{
    // локальное хранилище сообщений чата
    class ChatClientStorage
    {
        private readonly string folder;
        private readonly ThreadSafeList<Message> pendingMessages = new ThreadSafeList<Message>();
        private const int LockTimeout = 1000;
        private readonly object fileLock = new object();
        private readonly Thread saveThread;
        private volatile bool isStopping;

        public ChatClientStorage(string folder = "/chat")
        {
            this.folder = folder;
            Directory.CreateDirectory(ExecutablePath.ExecPath + folder);
            saveThread = new Thread(SaveMessages);
            saveThread.Start();
        }

        public void Stop()
        {
            isStopping = true;
        }

        public void SaveMessage(Message message)
        {
            pendingMessages.Add(message, LockTimeout);
        }

        public List<Message> LoadMessagesFromFiles()
        {
            var result = new List<Message>();
            try
            {
                foreach (var fileName in Directory.GetFiles(ExecutablePath.ExecPath + folder, "*.txt"))
                {
                    var shortName = new FileInfo(fileName).Name;
                    var roomName = shortName.Substring(0, shortName.LastIndexOf(".txt"));
                    lock (fileLock)
                    {
                        using (var fs = new FileStream(fileName, FileMode.OpenOrCreate))
                        using (var sr = new StreamReader(fs))
                        {
                            var lineNumber = 0;
                            while (true)
                            {
                                var line = sr.ReadLine();
                                if (string.IsNullOrEmpty(line))
                                    break;
                                var words = line.Split(new[] {"&#32;"}, StringSplitOptions.None);
                                if (words.Length < 4)
                                {
                                    Logger.Error(string.Format("LoadMessagesFromFiles: format error: file: {0}, line: {1}", fileName, lineNumber));
                                    lineNumber++;
                                    continue;
                                }
                                var message = new Message
                                    {
                                        TimeStamp = DateTime.Parse(words[0]),
                                        Sender = words[1].ToInt(),
                                        Room = roomName,
                                        Receiver = words[2].ToInt(),
                                        Text = words[3].Replace("&#10;", Environment.NewLine)
                                    };
                                result.Add(message);
                                lineNumber++;
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Info("ChatClientStorage.LoadMessagesFromFiles", exception);
            }
            return result;
        }

        public void SaveMessagesToFiles()
        {
            foreach (var message in pendingMessages.ExtractAll(LockTimeout))
            {
                // TODO: save private messages
                if (string.IsNullOrEmpty(message.Room))
                    continue;
                lock (fileLock)
                {
                    using (var fs = new FileStream(ExecutablePath.ExecPath + folder + "/" + message.Room + ".txt", FileMode.Append))
                    using (var sw = new StreamWriter(fs))
                    {
                        message.Text = message.Text.Replace(Environment.NewLine, "&#10;");
                        message.Text = message.Text.Replace("\n", "&#10;"); // на случай, если остались разрывы
                        sw.WriteLine(message.TimeStamp.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffff") + "&#32;" +
                                     message.Sender + "&#32;" + message.Receiver + "&#32;" +
                                     message.Text);
                    }
                }
            }
        }

        private void SaveMessages()
        {
            while (!isStopping)
            {
                Thread.Sleep(1000);
                SaveMessagesToFiles();
            }
            SaveMessagesToFiles();
        }
    }
}

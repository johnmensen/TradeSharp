using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace FixDealer
{
    enum ErrorMessageType
    {
        ОшибкаОтправки = 0,
        ОтказСервера,
        НетОтвета
    }
    class ErrorMessage
    {
        public DateTime time;
        public ErrorMessageType messageType;
        public string message;
        public Exception exception;
        public ErrorMessage() { }
        public ErrorMessage(DateTime time, ErrorMessageType messageType, string message, Exception exception)
        {
            this.time = time;
            this.messageType = messageType;
            this.message = message;
            this.exception = exception;
        }
        public ErrorMessage(ErrorMessage msg)
        {
            time = msg.time;
            messageType = msg.messageType;
            message = msg.message;
            exception = msg.exception;
        }
        public override string ToString()
        {
            return string.Format("[{0:dd.MM HH:mm}] Тип:{1}{2}{3}.",
                                 time, messageType,
                                 string.IsNullOrEmpty(message) ? string.Empty : string.Format(" ({0})", message),
                                 exception == null ? string.Empty : string.Format(" исключение: {0}", exception));
        }
    }

    class ErrorStorage
    {
        //private static ErrorStorage instance;
        //public static ErrorStorage Instance
        //{
        //    get
        //    {
        //        return instance ?? (instance = new ErrorStorage());
        //    }
        //}
        public ErrorStorage()
        {
        }

        private List<ErrorMessage> messages = new List<ErrorMessage>();
        private ReaderWriterLock locker = new ReaderWriterLock();
        private const int LockTimeout = 1000;
        private const int MaxMessages = 5;

        public List<ErrorMessage> GetMessages()
        {
            try
            {
                locker.AcquireReaderLock(LockTimeout);
            }
            catch
            {
                return new List<ErrorMessage>();
            }
            try
            {
                return messages.Select(m => new ErrorMessage(m)).ToList();
            }
            finally
            {
                locker.ReleaseReaderLock();
            }
        }

        public string GetMessagesString()
        {
            try
            {
                locker.AcquireReaderLock(LockTimeout);
            }
            catch
            {
                return string.Empty;
            }
            try
            {
                return messages.Count == 0 ? string.Empty :
                    string.Join(Environment.NewLine,
                    messages.Select(m => new ErrorMessage(m)).ToList());
            }
            finally
            {
                locker.ReleaseReaderLock();
            }
        }

        public void ClearMessages()
        {
            try
            {
                locker.AcquireWriterLock(LockTimeout);
            }
            catch
            {
                return;
            }
            try
            {
                messages.Clear();
            }
            finally
            {
                locker.ReleaseWriterLock();
            }
        }
    
        public void AddMessage(ErrorMessage msg)
        {
            try
            {
                locker.AcquireWriterLock(LockTimeout);
            }
            catch
            {
                return;
            }
            try
            {
                messages.Add(msg);
                if (messages.Count > MaxMessages) messages.RemoveAt(0);
            }
            finally
            {
                locker.ReleaseWriterLock();
            }
        }
    }
}

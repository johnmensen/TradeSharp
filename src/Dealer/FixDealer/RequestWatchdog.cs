using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Entity;
using TradeSharp.Util;

namespace FixDealer
{
    /// <summary>
    /// сохраняет Id каждого отправленного запроса
    /// если запрос обработан - изымается из очереди слежения
    /// если с момента отправки запроса прошло более N милисекунд,
    /// генерирует ошибку, извлекая запрос из очереди
    /// </summary>
    class RequestWatchdog
    {
        private readonly ErrorStorage errorStorage;
        public RequestWatchdog(ErrorStorage errorStorage)
        {
            this.errorStorage = errorStorage;
            timerIntervalMil = AppConfig.GetIntParam("Watchdog.TimerInterval", 150);
            milsToReportError = AppConfig.GetIntParam("Watchdog.MilsToError", 10000);
        }

        private readonly int timerIntervalMil;
        private readonly int milsToReportError;
        private readonly List<RequestWatchdogItem> requests = new List<RequestWatchdogItem>();
        private readonly ReaderWriterLock locker = new ReaderWriterLock();
        private const int LockTimeout = 1000;
        private volatile bool isStopping;
        private Thread watchThread;

        public void AddRequest(RequestWatchdogItem req)
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
                requests.Add(req);
            }
            finally
            {
                locker.ReleaseWriterLock();
            }
        }

        public void ClearRequests(RequestWatchdogItem req)
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
                requests.Clear();
            }
            finally
            {
                locker.ReleaseWriterLock();
            }
        }
    
        public void OnRequestProcessed(int reqId)//, DealType side, string symbol)
        {
            try
            {
                locker.AcquireReaderLock(LockTimeout);
            }
            catch
            {
                return;
            }
            try
            {
                var index = requests.FindIndex(0, r => r.requestId == reqId);
                if (index < 0) return;
                locker.UpgradeToWriterLock(LockTimeout);
                requests.RemoveAt(index);
            }
            finally
            {
                locker.ReleaseLock();
            }
        }
    
        public void Start()
        {
            if (watchThread != null) return;
            isStopping = false;
            watchThread = new Thread(WatchLoop);
            watchThread.Start();
        }

        public void Stop()
        {
            if (watchThread == null) return;
            isStopping = true;
            watchThread.Join();
        }

        private void WatchLoop()
        {
            while (!isStopping)
            {
                Thread.Sleep(timerIntervalMil);
                WatchLoopIteration();
            }
        }

        private void WatchLoopIteration()
        {
            var errors = new List<ErrorMessage>();
            try
            {
                locker.AcquireReaderLock(LockTimeout);
            }
            catch
            {
                return;
            }
            try
            {
                var nowTime = DateTime.Now;
                if (!requests.Any(r => (nowTime - r.requestTime).TotalMilliseconds > milsToReportError)) return;
                locker.UpgradeToWriterLock(LockTimeout);
                for (var i = 0; i < requests.Count; i++)
                {
                    var req = requests[i];
                    if ((nowTime - req.requestTime).TotalMilliseconds > milsToReportError)
                    {
                        requests.RemoveAt(i);
                        i--;
                        errors.Add(new ErrorMessage(DateTime.Now, ErrorMessageType.НетОтвета,
                                                    string.Format("Запрос {0} ({1} {2}) устарел на {3} ms",
                                                                  req.requestId,
                                                                  req.requestType, req.requestSymbol, milsToReportError),
                                                    null));                        
                    }
                }
            }
            finally
            {
                locker.ReleaseLock();
            }
            if (errors.Count == 0) return;
            foreach (var er in errors)
                errorStorage.AddMessage(er);
        }
    }

    class RequestWatchdogItem
    {
        public int requestId;
        public string requestSymbol;
        public DealType requestType;
        public int requestVolume;
        public DateTime requestTime;
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using TradeSharp.Util;

namespace TradeSharp.Server.BL
{
    /// <summary>
    /// каждый торговый запрос сопровождается уникальным идентификатором запроса
    /// во избежание дублирования ордеров
    /// 
    /// класс хранит N последних Id по каждому счету и проверяет на уникальность 
    /// </summary>
    class RequestIdStorage
    {
        private class ThreadSafeArray
        {
            private readonly int[] reqIds;

            private readonly ReaderWriterLock locker = new ReaderWriterLock();

            private int nextIndex;

            public ThreadSafeArray(int len, int id)
            {
                reqIds = new int[len];
                reqIds[0] = id;
            }
        
            public bool ContainsValue(int id)
            {
                try
                {
                    locker.AcquireReaderLock(LockTimeout);
                }
                catch (ApplicationException)
                {
                    Logger.Error("RequestIdStorage read timeout");
                    return true;
                }
                try
                {
                    foreach (var v in reqIds)
                    {
                        if (v == 0) break;
                        if (v == id) return true;
                    }
                    // поместить новое значение
                    try
                    {
                        locker.UpgradeToWriterLock(LockTimeout);
                    }
                    catch (ApplicationException)
                    {
                        Logger.Error("RequestIdStorage write timeout");
                        return true;
                    }
                    nextIndex++;
                    if (nextIndex == reqIds.Length) nextIndex = 0;
                    reqIds[nextIndex] = id;
                    return false;
                }
                finally
                {
                    locker.ReleaseLock();
                }
            }
        }

        private static RequestIdStorage instance;

        public static RequestIdStorage Instance
        {
            get { return instance ?? (instance = new RequestIdStorage()); }
        }

        private readonly ReaderWriterLock locker = new ReaderWriterLock();

        private const int LockTimeout = 1000;

        private readonly Dictionary<int, ThreadSafeArray> accountRequests = new Dictionary<int, ThreadSafeArray>();

        private readonly int requestsCount = AppConfig.GetIntParam("RequestsPerAccount", 8);

        private RequestIdStorage()
        {            
        }

        public bool RequestsIsDoubled(int accountId, int reqId)
        {
            try
            {
                locker.AcquireReaderLock(LockTimeout);
            }
            catch (ApplicationException)
            {
                Logger.Error("RequestIsDoubled - read timeout");
                return true;
            }
            try
            {
                ThreadSafeArray arr;
                accountRequests.TryGetValue(accountId, out arr);
                if (arr != null)
                    return arr.ContainsValue(reqId);
                // запросы от акаунта еще не поступали - создать запись
                try
                {
                    locker.UpgradeToWriterLock(LockTimeout);
                }
                catch (ApplicationException)
                {
                    Logger.Error("RequestIsDoubled - write timeout");
                    return true;
                }
                arr = new ThreadSafeArray(requestsCount, reqId);
                accountRequests.Add(accountId, arr);
                return false;
            }
            finally
            {
                locker.ReleaseLock();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TradeSharp.Util
{
    /// <summary>
    /// список объектов, с разграничением доступа (потокобезопасный)
    /// хранит время последнего обращения и вызывает процедуру обновления
    /// при устаревании данных
    /// </summary>    
    public class ThreadSafeUpdatingList<T>
    {
        private List<T> items = new List<T>();
        private DateTime? lastUpdated;
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        private readonly int lockTimeout;
        private readonly int intervalUpdate;

        public delegate List<T> UpdateRoutineDel();
        private readonly UpdateRoutineDel upRoutine;

        public ThreadSafeUpdatingList(int lockTimeout, int intervalUpdate,
            UpdateRoutineDel upRoutine)
        {
            this.lockTimeout = lockTimeout;
            this.upRoutine = upRoutine;
            this.intervalUpdate = intervalUpdate;
        }

        public List<T> GetItems()
        {
            if (!locker.TryEnterReadLock(lockTimeout))
            {
                Logger.ErrorFormat("ThreadSafeUpdatingList<{0}> - таймаут чтения", typeof(T).Name);
                return new List<T>();
            }
            try
            {
                var milsElapsed = lastUpdated.HasValue ? (DateTime.Now - lastUpdated.Value).TotalMilliseconds : int.MaxValue;
                if (milsElapsed < intervalUpdate) return items.ToList();
            }
            finally
            {
                locker.ExitReadLock();
            }

            // обновить список
            var newItems = upRoutine();
            if (!locker.TryEnterWriteLock(lockTimeout))
            {
                Logger.ErrorFormat("ThreadSafeUpdatingList<{0}> - таймаут записи", typeof(T).Name);
                return new List<T>();
            }
            try
            {
                items = newItems;
                lastUpdated = DateTime.Now;
                return items.ToList();
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }
    
        public T GetItem(Predicate<T> selector)
        {
            if (!locker.TryEnterReadLock(lockTimeout))
            {
                Logger.ErrorFormat("ThreadSafeUpdatingList<{0}> - таймаут чтения", typeof(T).Name);
                return default(T);
            }
            try
            {
                var milsElapsed = lastUpdated.HasValue ? (DateTime.Now - lastUpdated.Value).TotalMilliseconds : int.MaxValue;
                if (milsElapsed < intervalUpdate) 
                    return items.FirstOrDefault(t => selector(t));
            }
            finally
            {
                locker.ExitReadLock();
            }

            // обновить список
            var newItems = upRoutine();
            if (!locker.TryEnterWriteLock(lockTimeout))
            {
                Logger.ErrorFormat("ThreadSafeUpdatingList<{0}> - таймаут записи", typeof(T).Name);
                return default(T);
            }
            try
            {
                items = newItems;
                lastUpdated = DateTime.Now;
                return items.FirstOrDefault(t => selector(t));
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }
    }
}

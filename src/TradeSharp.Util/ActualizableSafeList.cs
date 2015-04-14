using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TradeSharp.Util
{
    public class ActualizableSafeList<T>
    {
        private List<T> list = new List<T>();

        private readonly ThreadSafeTimeStamp lastUpdated = new ThreadSafeTimeStamp();

        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        private int lockTimeoutMils;
        
        private int updateTimeoutMils;

        public ActualizableSafeList(int lockTimeoutMils, int updateTimeoutMils)
        {
            this.lockTimeoutMils = lockTimeoutMils;
            this.updateTimeoutMils = updateTimeoutMils;
        }

        public void Update(List<T> values)
        {
            if (!locker.TryEnterWriteLock(lockTimeoutMils)) 
                return;
            try
            {
                list = values;
                lastUpdated.Touch();
            }
            finally 
            {
                locker.ExitWriteLock();
            }
        }

        public void Add(T value)
        {
            if (!locker.TryEnterWriteLock(lockTimeoutMils))
                return;
            try
            {
                list.Add(value);
                lastUpdated.Touch();
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public void Clear()
        {
            if (!locker.TryEnterWriteLock(lockTimeoutMils))
                return;
            try
            {
                list.Clear();
                lastUpdated.Touch();
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }
        
        public List<T> GetValues()
        {
            if (!locker.TryEnterReadLock(lockTimeoutMils))
                return new List<T>();
            try
            {
                return list.ToList();
            }
            finally
            {
                locker.ExitReadLock();
            }
        }
    
        public bool IsActual()
        {
            var lastHit = lastUpdated.GetLastHitIfHitted();
            if (!lastHit.HasValue) return false;
            return (DateTime.Now - lastHit.Value).TotalMilliseconds <= updateTimeoutMils;
        }

        public void MarkActual()
        {
            lastUpdated.Touch();
        }

        public void MarkInactual()
        {
            lastUpdated.ResetHit();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TradeSharp.Util
{
    public class ThreadSafeQueue<Q>
    {
        private readonly List<Q> queue = new List<Q>();

        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        public int LockTimeout = 2000;

        public int MaxLength { get; set; }

        public int? Count
        {
            get
            {
                if (!locker.TryEnterReadLock(LockTimeout)) return null;
                try
                {
                    return queue.Count;
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }
        }

        public bool InQueue(Q val, int timeout)
        {
            if (!locker.TryEnterWriteLock(timeout)) return false;
            try
            {
                queue.Add(val);
                if (MaxLength > 0 && queue.Count > MaxLength)
                    TrimQueue();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public bool InQueue(List<Q> val, int timeout)
        {
            if (!locker.TryEnterWriteLock(timeout)) return false;
            try
            {
                queue.AddRange(val);
                if (MaxLength > 0 && queue.Count > MaxLength)
                    TrimQueue();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public List<Q> ReceiveAllData(int timeout, out bool timeoutFlag)
        {
            timeoutFlag = false;
            if (!locker.TryEnterReadLock(timeout))
            {
                timeoutFlag = true;
                return new List<Q>();
            }
            try
            {
                if (queue.Count == 0)
                    return new List<Q>();
                return queue.ToList();
            }
            catch (Exception)
            {
                timeoutFlag = true;
                return new List<Q>();
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public Q Out(int timeout, out bool timeoutFlag)
        {
            timeoutFlag = false;
            if (!locker.TryEnterUpgradeableReadLock(timeout))
            {
                timeoutFlag = true;
                return default(Q);
            }
            try
            {
                if (queue.Count == 0)
                    return default(Q);
                Q copy = queue[0];
                if (!locker.TryEnterWriteLock(timeout))
                {
                    timeoutFlag = true;
                    return default(Q);
                }
                try
                {
                    queue.RemoveAt(0);
                    return copy;
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
            catch (Exception)
            {
                timeoutFlag = true;
            }
            finally
            {
                locker.ExitReadLock();
            }
            return default(Q);
        }

        public List<Q> ExtractAll(int timeout, out bool timeoutFlag)
        {
            timeoutFlag = false;
            if (!locker.TryEnterUpgradeableReadLock(timeout))
            {
                timeoutFlag = true;
                return new List<Q>();
            }
            try
            {
                if (queue.Count == 0)
                    return new List<Q>();
                var copy = queue.ToList();
                if (!locker.TryEnterWriteLock(timeout))
                {
                    timeoutFlag = true;
                    return new List<Q>();
                }
                try
                {
                    queue.Clear();
                    return copy;
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
            catch (Exception)
            {
                timeoutFlag = true;
                return new List<Q>();
            }
            finally
            {
                locker.ExitUpgradeableReadLock();
            }
        }
    
        private void TrimQueue()
        {
            while (queue.Count > MaxLength)
                queue.RemoveAt(0);
        }
    }
}

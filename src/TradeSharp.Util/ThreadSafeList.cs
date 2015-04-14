using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace TradeSharp.Util
{
    public class ThreadSafeList<T>
    {
        public delegate T CloneDel(T obj);

        private readonly List<T> list = new List<T>();
        
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        public int? Count
        {
            get
            {
                if (!locker.TryEnterReadLock(1000)) return null;
                try
                {
                    return list.Count;
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }
        }

        public bool Add(T item, int timeout)
        {
            if (!locker.TryEnterWriteLock(timeout)) return false;
            try
            {
                list.Add(item);
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

        /// <summary>
        /// добавить элемент. если длина списка превысила maxLength - удалить самый первый
        /// </summary>
        public bool Add(T item, int timeout, int maxLength)
        {
            if (!locker.TryEnterWriteLock(timeout)) return false;
            try
            {
                list.Add(item);
                if (list.Count > maxLength) list.RemoveAt(0);
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

        public bool AddRange(IEnumerable<T> items, int timeout)
        {
            if (!locker.TryEnterWriteLock(timeout)) return false;
            try
            {
                list.AddRange(items);
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

        public bool ReplaceRange(IEnumerable<T> items, int timeout)
        {
            if (!locker.TryEnterWriteLock(timeout)) return false;
            try
            {
                list.Clear();
                list.AddRange(items);
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

        public List<T> GetAll(int timeout, out bool timeoutFlag)
        {
            timeoutFlag = false;
            if (!locker.TryEnterReadLock(timeout))
            {
                timeoutFlag = true;
                return new List<T>();
            }
            try
            {
                var copy = list.ToList();
                return copy;
            }
            catch
            {
                timeoutFlag = true;
                return new List<T>();
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public List<T> GetAll(Predicate<T> selector, int timeout)
        {
            if (!locker.TryEnterReadLock(timeout))
            {
                return new List<T>();
            }
            try
            {
                var copy = list.Where(l => selector(l)).ToList();
                return copy;
            }
            catch
            {
                return new List<T>();
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public List<T> ExtractAll(int timeout, out bool timeoutFlag)
        {
            timeoutFlag = false;
            if (!locker.TryEnterUpgradeableReadLock(timeout))
            {
                timeoutFlag = true;
                return new List<T>();
            }
            try
            {
                if (list.Count == 0) return new List<T>();
                var copy = list.ToList();
                if (!locker.TryEnterWriteLock(timeout))
                {
                    timeoutFlag = true;
                    return new List<T>();
                }
                try
                {
                    list.Clear();
                    return copy;
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
            catch
            {
                timeoutFlag = true;
                return new List<T>();
            }
            finally
            {
                locker.ExitUpgradeableReadLock();
            }
        }

        public List<T> ExtractAll(int timeout)
        {
            if (!locker.TryEnterUpgradeableReadLock(timeout))
            {
                return new List<T>();
            }
            try
            {
                if (list.Count == 0) return new List<T>();
                var copy = list.ToList();
                if (!locker.TryEnterWriteLock(timeout))
                {
                    return new List<T>();
                }
                try
                {
                    list.Clear();
                    return copy;
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
            catch
            {
                return new List<T>();
            }
            finally
            {
                locker.ExitUpgradeableReadLock();
            }
        }

        public T ExtractFirst(int timeout, out bool timeoutFlag)
        {
            timeoutFlag = false;
            if (!locker.TryEnterUpgradeableReadLock(timeout))
            {
                timeoutFlag = true;
                return default(T);
            }
            try
            {
                if (list.Count == 0) return default(T);
                var copy = list[0];
                if (!locker.TryEnterWriteLock(timeout))
                {
                    timeoutFlag = true;
                    return default(T);
                }
                try
                {
                    list.RemoveAt(0);
                    return copy;
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
            catch
            {
                timeoutFlag = true;
                return default(T);
            }
            finally
            {
                locker.ExitUpgradeableReadLock();
            }
        }
    
        public bool Contains(T item, int timeout, out bool timeoutFlag)
        {
            timeoutFlag = false;
            if (!locker.TryEnterReadLock(timeout))
            {
                timeoutFlag = true;
                return false;
            }
            try
            {
                return list.Contains(item);
            }
            catch
            {
                timeoutFlag = true;
                return false;
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public T Find(Predicate<T> selector, int timeout)
        {
            if (!locker.TryEnterReadLock(timeout))
            {
                return default(T);
            }
            try
            {
                return list.FirstOrDefault(l => selector(l));
            }
            catch
            {
                return default(T);
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public T Find(Predicate<T> selector, CloneDel copyMaker, int timeout)
        {
            if (!locker.TryEnterReadLock(timeout))
            {
                return default(T);
            }
            try
            {
                var obj = list.FirstOrDefault(l => selector(l));
                if (null == obj || obj.Equals(default(T)))
                    return default(T);
                return copyMaker(obj);
            }
            catch
            {
                return default(T);
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public bool AddIfNotContains(T item, int timeout)
        {
            if (!locker.TryEnterUpgradeableReadLock(timeout)) return false;            
            try
            {
                if (list.Contains(item)) return true;
                if (!locker.TryEnterWriteLock(timeout)) return false;                
                try
                {
                    list.Add(item);
                    return true;
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                locker.ExitUpgradeableReadLock();
            }
        }
    
        public bool TryRemove(T item, int timeout)
        {
            if (!locker.TryEnterUpgradeableReadLock(timeout)) return false;
            try
            {
                if (!list.Contains(item)) return true;
                if (!locker.TryEnterWriteLock(timeout)) return false;
                try
                {
                    list.Remove(item);
                    return true;
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                locker.ExitUpgradeableReadLock();
            }
        }

        public bool TryRemove(Predicate<T> selector, int timeout)
        {
            if (!locker.TryEnterUpgradeableReadLock(timeout)) return false;
            try
            {
                var obj = list.FirstOrDefault(l => selector(l));
                if (obj == null || obj.Equals(default(T))) return false;
                if (!locker.TryEnterWriteLock(timeout)) return false;
                try
                {
                    list.Remove(obj);
                    return true;
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                locker.ExitUpgradeableReadLock();
            }
        }

        public bool TryClear(int timeout)
        {
            if (!locker.TryEnterUpgradeableReadLock(timeout)) return false;
            try
            {
                if (!locker.TryEnterWriteLock(timeout)) return false;
                try
                {
                    list.Clear();
                    return true;
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                locker.ExitUpgradeableReadLock();
            }
        }

        public T this[int index]
        {
            get
            {
                if (!locker.TryEnterReadLock(1000)) return default(T);
                try
                {
                    return list[index];
                }
                catch
                {
                    return default(T);
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }
            set
            {
                if (!locker.TryEnterWriteLock(1000)) return;
                try
                {
                    list[index] = value;
                }
                catch
                {                    
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
        }
    }
}

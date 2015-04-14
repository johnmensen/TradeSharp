using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TradeSharp.Util
{
    public class ThreadSafeStorage <K, V> : IStorage <K, V>
    {
        private Dictionary<K, V> dictionary = new Dictionary<K, V>();

        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        public int LockTimeout = 2000;

        public int? Count
        {
            get
            {
                if (!locker.TryEnterReadLock(LockTimeout)) return null;
                try
                {
                    return dictionary.Count;
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }
        }

        public void UpdateValues(K[] keys, V[] _values)
        {
            if(!locker.TryEnterWriteLock(LockTimeout))
            {
                Logger.Error("ThreadSafeStorage::UpdateValues - невозможно получить доступ на запись");
                return;
            }
            try
            {
                for (var i = 0; i < keys.Length; i++)
                {
                    if (dictionary.ContainsKey(keys[i]))
                        dictionary[keys[i]] = _values[i];
                    else
                        dictionary.Add(keys[i], _values[i]);
                }
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public void UpdateValues(K key, V _value)
        {
            if (!locker.TryEnterWriteLock(LockTimeout))
            {
                Logger.Error("ThreadSafeStorage::UpdateValues - невозможно получить доступ на запись");
                return;
            }
            try
            {
                if (dictionary.ContainsKey(key))
                    dictionary[key] = _value;
                else dictionary.Add(key, _value);
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public bool ContainsKey(K key)
        {
            if (!locker.TryEnterReadLock(LockTimeout))
            {
                Logger.Error("ThreadSafeStorage: невозможно получить доступ на чтение");
                return false;
            }
            try
            {
                return dictionary.ContainsKey(key);
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public List<K> GetKeys()
        {
            if (!locker.TryEnterReadLock(LockTimeout))
            {
                Logger.Error("ThreadSafeStorage: невозможно получить доступ на чтение");
                return new List<K>();
            }
            try
            {
                return dictionary.Keys.ToList();
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public V ReceiveValue(K key)
        {
            if (!locker.TryEnterReadLock(LockTimeout))
            {
                Logger.Error("ThreadSafeStorage: невозможно получить доступ на чтение");
                return default(V);
            }
            try
            {
                if (dictionary.ContainsKey(key)) return dictionary[key];
                return default(V);
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public V ReceiveValueCheckContains(K key, out bool contains)
        {
            contains = false;
            if (!locker.TryEnterReadLock(LockTimeout))
            {
                Logger.Error("ThreadSafeStorage: невозможно получить доступ на чтение");
                return default(V);
            }
            try
            {
                if (dictionary.ContainsKey(key))
                {
                    contains = true;
                    return dictionary[key];
                }
                return default(V);
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public Dictionary<K, V> ReceiveAllData()
        {
            if (!locker.TryEnterReadLock(LockTimeout))
            {
                Logger.Error("ThreadSafeStorage: невозможно получить доступ на чтение");
                return null;
            }
            try
            {
                return dictionary.ToDictionary(pair => pair.Key, pair => pair.Value);
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public V ExtractData(K key)
        {
            if (!locker.TryEnterUpgradeableReadLock(LockTimeout))
            {
                Logger.Error("ThreadSafeStorage: невозможно получить доступ на чтение");
                return default(V);
            }
            try
            {
                V val;
                if (!dictionary.TryGetValue(key, out val))
                    return val;                

                if (locker.TryEnterWriteLock(LockTimeout))
                {
                    dictionary.Remove(key);
                    locker.ExitWriteLock();
                    return val;
                }
            
                Logger.Error("ThreadSafeStorage: невозможно сделать апгрейд доступа на запись");
                return val;
            }
            finally
            {
                locker.ExitUpgradeableReadLock();
            }
        }

        public void Copy(IStorage<K, V> sourceDic)
        {
            if (!locker.TryEnterWriteLock(LockTimeout))
            {
                Logger.Error("ThreadSafeStorage::Copy - невозможно получить доступ на запись");
                return;
            }
            try
            {
                dictionary = sourceDic.ReceiveAllData();
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public void Copy(Dictionary<K, V> sourceDic)
        {
            if (!locker.TryEnterWriteLock(LockTimeout))
            {
                Logger.Error("ThreadSafeStorage::Copy - невозможно получить доступ на запись");
                return;
            }
            try
            {
                dictionary = sourceDic;
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public bool Remove(K key)
        {
            if (!locker.TryEnterWriteLock(LockTimeout))
            {
                Logger.Error("ThreadSafeStorage: невозможно получить доступ на запись");
                return false;
            }
            try
            {
                return dictionary.Remove(key);
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public void Clear()
        {
            if (!locker.TryEnterWriteLock(LockTimeout))
            {
                Logger.Error("ThreadSafeStorage::Clear - невозможно получить доступ на запись");
                return;
            }
            try
            {
                dictionary.Clear();
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }
    }
}

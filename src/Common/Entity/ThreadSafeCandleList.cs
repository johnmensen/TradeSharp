using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TradeSharp.Util;

namespace Entity
{
    /// <summary>
    /// словарь таких списков хранится в AtomCandleStorage
    /// сам по себе список потокобезопасный
    /// </summary>
    public class ThreadSafeCandleList
    {
        #region Данные
        private List<CandleData> candles;

        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        private const int LockTimeout = 1000;
        #endregion

        #region Открытые методы
        
        public ThreadSafeCandleList()
        {
            candles = new List<CandleData>();
        }

        public ThreadSafeCandleList(List<CandleData> candles)
        {
            this.candles = candles;
        }

        public void AddCandles(List<CandleData> newCandles)
        {
            if (!locker.TryEnterWriteLock(LockTimeout)) return;
            try
            {
                candles.AddRange(newCandles);
            }
            finally 
            {
                locker.ExitWriteLock();
            }            
        }

        public void RewriteCandles(List<CandleData> newCandles)
        {
            if (!locker.TryEnterWriteLock(LockTimeout)) return;
            try
            {
                candles = newCandles;
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public List<CandleData> GetAllCandles()
        {
            if (!locker.TryEnterReadLock(LockTimeout)) return null;
            try
            {
                return candles.ToList();
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public List<CandleData> GetCandlesOnRange(DateTime start, DateTime end)
        {
            if (!locker.TryEnterReadLock(LockTimeout)) return null;
            try
            {
                var shouldCopy = false;
                var selected = new List<CandleData>();

                foreach (var candleData in candles)
                {
                    if (shouldCopy)
                        selected.Add(candleData);
                    else
                        shouldCopy = candleData.timeOpen >= start;
                    if (candleData.timeOpen > end) break;
                }

                return selected;
            }
            finally
            {
                locker.ExitReadLock();
            }
        }
        
        public Cortege2<DateTime, DateTime>? GetCandlesDataRange()
        {
            if (!locker.TryEnterReadLock(LockTimeout)) return null;
            try
            {
                return candles.Count == 0
                           ? (Cortege2<DateTime, DateTime>?)null
                           : new Cortege2<DateTime, DateTime>(candles[0].timeOpen, candles[candles.Count - 1].timeOpen);
            }
            finally
            {
                locker.ExitReadLock();
            }
        }
        
        /*public DateTime? GetFirstGapStartTime(int candlesFromEnd)
        {
            if (!locker.TryEnterReadLock(LockTimeout)) return null;
            try
            {
                if (candles.Count == 0) return null;
                var startIndex = candles.Count - candlesFromEnd;
                if (startIndex < 0) startIndex = 0;
                var gaps = QuoteCacheManager.GetGaps(candles, startIndex);
                return gaps.Count > 0 ? gaps[0].start : (DateTime?)null;
            }
            finally
            {
                locker.ExitReadLock();
            }
        }*/

        #endregion
    }
}

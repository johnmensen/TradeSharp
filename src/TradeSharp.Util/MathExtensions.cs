using System;
using System.Collections.Generic;
using System.Linq;

namespace TradeSharp.Util
{
    /// <summary>
    /// методы расширения для реализации операций с последовательностями
    /// </summary>
    public static class MathExtensions
    {
        private static readonly Random random = new Random();

        #region Произведение
        public static decimal Product<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
        {
            var product = 1M;
            foreach (var i in source)
            {
                var val = selector(i);
                if (val.HasValue) product *= val.Value;
            }
            return product;
        }
        public static decimal Product<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
        {
            var product = 1M;
            foreach (var i in source)
            {
                var val = selector(i);
                product *= val;
            }
            return product;
        }
        public static double Product<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
        {
            var product = 1.0;
            foreach (var i in source)
            {
                var val = selector(i);
                if (val.HasValue) product *= val.Value;
            }
            return product;
        }
        public static double Product<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            var product = 1.0;
            foreach (var i in source)
            {
                var val = selector(i);
                product *= val;
            }
            return product;
        }
        public static float Product<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
        {
            var product = 1.0f;
            foreach (var i in source)
            {
                var val = selector(i);
                product *= val;
            }
            return product;
        }
        public static int Product<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
        {
            var product = 1;
            foreach (var i in source)
            {
                var val = selector(i);
                if (val.HasValue) product *= val.Value;
            }
            return product;
        }
        public static int Product<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            var product = 1;
            foreach (var i in source)
            {
                var val = selector(i);
                product *= val;
            }
            return product;
        }
        #endregion

        #region Работа с датами
        /// <summary>
        /// rValue &lt lValue - результат > 0
        /// </summary>        
        public static double MonthDifference(this DateTime lValue, DateTime rValue)
        {
            var monthsDiff = (lValue.Month - rValue.Month) + 12 * (lValue.Year - rValue.Year);
            var daysDiff = (lValue.Day - rValue.Day) / 30.0;
            return monthsDiff + daysDiff;
        }
        #endregion

        #region Поиск индексов
        public static int IndexOfMin<TSource>(this IEnumerable<TSource> source)
        {
            return IndexOfMin(source, v => v);
        }

        public static int IndexOfMin<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> selector)
        {
            int index = 1, minIndex = 0;
            var comparer = Comparer<TKey>.Default;

            using (IEnumerator<TSource> sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext()) return -1;
                var minVal = selector(sourceIterator.Current);

                while (sourceIterator.MoveNext())
                {
                    var curVal = selector(sourceIterator.Current);
                    if (comparer.Compare(curVal, minVal) < 0)
                    {
                        minIndex = index;
                        minVal = curVal;
                    }
                    index++;
                }
            }
            return minIndex;
        }

        public static int IndexOfMax<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> selector)
        {
            int index = 1, maxIndex = 0;
            var comparer = Comparer<TKey>.Default;

            using (IEnumerator<TSource> sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext()) return -1;
                var maxVal = selector(sourceIterator.Current);

                while (sourceIterator.MoveNext())
                {
                    var curVal = selector(sourceIterator.Current);
                    if (comparer.Compare(curVal, maxVal) > 0)
                    {
                        maxIndex = index;
                        maxVal = curVal;
                    }
                    index++;
                }
            }
            return maxIndex;
        }

        public static T GetRandomValue<T>(this IList<T> list)
        {
            if (list.Count == 0) return default(T);
            if (list.Count == 1) return list[0];
            return list[random.Next(list.Count)];
        }

        public static T Mode<T>(this IEnumerable<T> list)
        {
            // Initialize the return value
            T mode = default(T);

            // Test for a null reference and an empty list
            if (list != null && list.Any())
            {
                // Store the number of occurences for each element
                Dictionary<T, int> counts = new Dictionary<T, int>();

                // Add one to the count for the occurence of a character
                foreach (T element in list)
                {
                    if (counts.ContainsKey(element))
                        counts[element]++;
                    else
                        counts.Add(element, 1);
                }

                // Loop through the counts of each element and find the 
                // element that occurred most often
                int max = 0;

                foreach (KeyValuePair<T, int> count in counts)
                {
                    if (count.Value > max)
                    {
                        // Update the mode
                        mode = count.Key;
                        max = count.Value;
                    }
                }
            }

            return mode;
        }

        public static TKey Mode<T, TKey>(this IEnumerable<T> list, Func<T, TKey> selector)
        {
            // Initialize the return value
            var mode = default(TKey);

            // Test for a null reference and an empty list
            if (list != null && list.Any())
            {
                // Store the number of occurences for each element
                var counts = new Dictionary<TKey, int>();

                // Add one to the count for the occurence of a character
                foreach (T element in list)
                {
                    var val = selector(element);
                    if (counts.ContainsKey(val))
                        counts[val]++;
                    else
                        counts.Add(val, 1);
                }

                // Loop through the counts of each element and find the 
                // element that occurred most often
                int max = 0;

                foreach (var count in counts)
                {
                    if (count.Value > max)
                    {
                        // Update the mode
                        mode = count.Key;
                        max = count.Value;
                    }
                }
            }

            return mode;
        }

        public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (predicate == null) throw new ArgumentNullException("predicate");

            var retVal = 0;
            foreach (var item in items)
            {
                if (predicate(item)) return retVal;
                retVal++;
            }
            return -1;
        }
        ///<summary>Finds the index of the first occurence of an item in an enumerable.</summary>
        ///<param name="items">The enumerable to search.</param>
        ///<param name="item">The item to find.</param>
        ///<returns>The index of the first matching item, or -1 if the item was not found.</returns>
        public static int IndexOf<T>(this IEnumerable<T> items, T item) { return items.FindIndex(i => EqualityComparer<T>.Default.Equals(item, i)); }

        public static T? FirstOrNull<T>(this IEnumerable<T> items, Func<T, bool> predicate) where T : struct
        {
            if (items == null) throw new ArgumentNullException("items");
            if (predicate == null) throw new ArgumentNullException("predicate");

            foreach (var item in items)
            {
                if (predicate(item)) return item;
            }
            return null;
        }
        #endregion

        #region Сравнение
        public static bool HasSameContentsAs<T>(this ICollection<T> source, ICollection<T> other)
        {
            if (source.Count != other.Count)
            {
                return false;
            }
            var s = source
                .GroupBy(x => x)
                .ToDictionary(x => x.Key, x => x.Count());
            var o = other
                .GroupBy(x => x)
                .ToDictionary(x => x.Key, x => x.Count());
            int count;
            return s.Count == o.Count &&
                   s.All(x => o.TryGetValue(x.Key, out count) &&
                              count == x.Value);
        }
        
        public static bool AreSame(this float a, float b, float maxDeltaPercentToCountTheseNumbersTheSame)
        {
            if (a == b) return true;
            
            if (Math.Abs(b) < Math.Abs(a))
            {
                var c = a;
                a = b;
                b = c;
            }
            var deltaPc = 100f * Math.Abs(a - b) / b;
            return deltaPc <= maxDeltaPercentToCountTheseNumbersTheSame;
        }

        public static bool AreSame(this float a, float b)
        {
            return a.AreSame(b, 0.0025f);
        }

        public static bool Between<T>(this T a, T b, T c) where T : IComparable
        {
            return b.Equals(c) ? a.Equals(b) 
                : b.CompareTo(c) < 0 ? (a.CompareTo(b) >= 0 && a.CompareTo(c) <= 0)
                : (a.CompareTo(b) <= 0 && a.CompareTo(c) >= 0);
        }
        #endregion

        #region Сжатие коллекции
        public static List<double> ResizeAndResample(this IEnumerable<double> coll, int srcSize, int destSize)
        {
            var result = new List<double>();

            var scale = (double) destSize/srcSize;
            
            var lastIndex = 0;
            var index = 0.0;
            var sumY = 0.0;
            var countY = 0;

            foreach (var y in coll)
            {
                sumY += y;
                countY++;
                index += scale;
                if ((int) index <= lastIndex) continue;
                
                result.Add(sumY / countY);
                sumY = 0;
                countY = 0;
                lastIndex = (int) index;
            }
            if (result.Count < destSize && countY > 0)
                result.Add(sumY / countY);
            
            var lastVal = result.Count == 0 ? 0 : result[result.Count - 1];
            while (result.Count < destSize)
                result.Add(lastVal);
            return result;
        }
        #endregion
    }
}

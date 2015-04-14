using System;
using System.Collections.Generic;

namespace TradeSharp.Util
{
    public static class CollectionExtensions
    {
        public static bool RemoveByPredicate<T>(this IList<T> list, Predicate<T> p, bool removeAll = false)
        {
            var result = false;
            for (var i = 0; i < list.Count; i++)
            {
                if (!p(list[i])) continue;
                list.RemoveAt(i);
                if (!removeAll) return true;
                result = true;
                i--;
            }
            return result;
        }
    }
}
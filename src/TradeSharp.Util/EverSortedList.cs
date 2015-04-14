using System;
using System.Collections.Generic;

namespace TradeSharp.Util
{    
    public class EverSortedList<T> : List<T>
       where T : IComparable<T>
    {
        public new void Add(T item)
        {
            if (Count == 0)
            {
                //No list items
                base.Add(item);
                return;
            }
            if (item.CompareTo(this[Count - 1]) > 0)
            {
                //Bigger than Max
                base.Add(item);
                return;
            }
            int min = 0;
            int max = Count - 1;
            while ((max - min) > 1)
            {
                //Find half point
                int half = min + ((max - min) / 2);
                //Compare if it's bigger or smaller than the current item.
                int comp = item.CompareTo(this[half]);
                if (comp == 0)
                {
                    //Item is equal to half point
                    Insert(half, item);
                    return;
                }
                if (comp < 0) max = half;   //Item is smaller
                else min = half;   //Item is bigger
            }
            if (item.CompareTo(this[min]) <= 0) Insert(min, item);
            else Insert(min + 1, item);
        }
    }
}

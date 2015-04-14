using System;
using System.Collections;

namespace Candlechart.Core
{
    public class DateTimeArray : CollectionBase
    {
        public DateTime this[int index]
        {
            get { return (DateTime)List[index]; }
        }

        internal void Add(DateTime value)
        {
            List.Add(value);
        }

        internal new void Clear()
        {
            base.Clear();
        }
    }
}

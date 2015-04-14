using System.Collections.Generic;

namespace TradeSharp.Hub.BL.BL
{
    public class PagedListResult<T>
    {
        public List<T> items;

        public int totalRecordsCount;
    }
}

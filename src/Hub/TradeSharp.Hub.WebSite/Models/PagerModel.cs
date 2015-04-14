using System;

namespace TradeSharp.Hub.WebSite.Models
{
    public class PagerModel
    {
        public string SortBy { get; set; }

        public bool SortAscending { get; set; }

        public int CurrentPageIndex { get; set; }

        public int PageSize { get; set; }

        public int TotalRecordCount { get; set; }

        public int PagesCount
        {
            get { return (int)Math.Ceiling(TotalRecordCount / (float)PageSize); }
        }
    }
}
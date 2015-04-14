using System.Collections.Generic;

namespace TradeSharp.Hub.WebSite.Models
{
    public class GenericGridModel<T>
    {
        public PagerModel PagerModel { get; set; }

        public List<T> Items { get; set; }

        public GenericGridModel()
        {
            PagerModel = new PagerModel();
        }
    }
}
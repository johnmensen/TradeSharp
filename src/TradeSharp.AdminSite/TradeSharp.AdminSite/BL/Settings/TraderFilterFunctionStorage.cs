using System.Collections.Generic;
using System.Web.Mvc;
using Entity;

namespace TradeSharp.SiteAdmin.BL.Settings
{
    public class TraderFilterFunctionStorage
    {
        private List<TraderFilterFunction> functions = new List<TraderFilterFunction>();

        [PropertyXMLTag("Functions")]
        public List<TraderFilterFunction> Functions
        {
            get { return functions; }
            set { functions = value; }
        }
    }
    
    public class TraderFilterFunction
    {
        [PropertyXMLTag("Formula")]
        public string Function { get; set; }

        [PropertyXMLTag("Description")]
        public string Description { get; set; }

        public TraderFilterFunction()
        {            
        }

        public TraderFilterFunction(SelectListItem item)
        {
            Function = item.Text;
            Description = item.Value;
        }

        public SelectListItem MakeSelectItem()
        {
            return new SelectListItem
                {
                    Text = Function,
                    Value = Description
                };
        }
    }
}
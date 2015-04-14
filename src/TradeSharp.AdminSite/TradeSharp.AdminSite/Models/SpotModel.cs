using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using TradeSharp.SiteAdmin.App_GlobalResources;

namespace TradeSharp.SiteAdmin.Models
{
    public class SpotModel
    {
        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        [StringLength(6, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageRangeLess6")]
        public string ComBase { get; set; }

        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        [StringLength(6, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageRangeLess6")]
        public string ComCounter { get; set; }

        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        [StringLength(12, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageRangeLess12")]
        public string Title { get; set; }

        public int MinVolume { get; set; }
        public int MinStepVolume { get; set; }

        public decimal SwapBuy { get; set; }
        public decimal SwapSell { get; set; }

        [StringLength(25, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageRangeLess25")]
        public string Description { get; set; }

        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        public int Precise { get; set; }

        public int CodeFXI { get; set; }

        public static List<SelectListItem> CommodityList { get; set; }
    }
}
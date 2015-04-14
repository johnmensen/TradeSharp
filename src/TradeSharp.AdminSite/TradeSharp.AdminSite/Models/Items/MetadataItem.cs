using System.ComponentModel.DataAnnotations;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.SiteAdmin.BL.Localisation;

namespace TradeSharp.SiteAdmin.Models.Items
{
    public class MetadataItem
    {
        [LocalizedDisplayName("TitleUniqueIdentifier")]
        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        public int Id { get; set; }

        [LocalizedDisplayName("TitleCategory")]
        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        [StringLength(20, MinimumLength = 1, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageStringLength1_20")] 
        public string Category { get; set; }

        [LocalizedDisplayName("TitleUserName")]
        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        [StringLength(60, MinimumLength = 2, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageStringLength2_60")]
        public string Name { get; set; }

        [LocalizedDisplayName("TitleDataType")]
        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        [StringLength(10, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageStringLength0_10")]
        public string DataType { get; set; }

        [LocalizedDisplayName("TitleValue")]
        [StringLength(256, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageStringLength0_256")]
        public string Value { get; set; } 
    }
}
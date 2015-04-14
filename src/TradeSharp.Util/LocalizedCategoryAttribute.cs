using System.ComponentModel;

namespace TradeSharp.Util
{
    public class LocalizedCategoryAttribute : CategoryAttribute
    {
        public LocalizedCategoryAttribute(string resourceName) : base(Localizer.GetString(resourceName))
        {
        }
    }
}

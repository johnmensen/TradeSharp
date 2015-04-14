using System.ComponentModel;

namespace TradeSharp.Util
{
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        public LocalizedDescriptionAttribute(string resourceName)
            : base(Localizer.GetString(resourceName))
        {
        }
    }
}

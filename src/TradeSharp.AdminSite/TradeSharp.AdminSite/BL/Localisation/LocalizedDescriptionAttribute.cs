using System;
using System.ComponentModel;
using System.Globalization;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.BL.Localisation
{
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        static string Localize(string resourceName)
        {
            var displayName = string.Empty;
            try
            {
                displayName = Resource.ResourceManager.GetString(resourceName, CultureInfo.CurrentUICulture);
            }
            catch (Exception ex)
            {
                // ReSharper disable LocalizableElement
                Logger.Error("LocalizedDescriptionAttribute()", ex);
                // ReSharper restore LocalizableElement
            }
            return displayName;
        }
        public LocalizedDescriptionAttribute(string resourceName)
            : base(Localize(resourceName))
        {
        }
    }
}
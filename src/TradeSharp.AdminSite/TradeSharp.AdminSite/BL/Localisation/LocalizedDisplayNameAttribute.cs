using System;
using System.ComponentModel;
using System.Globalization;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.BL.Localisation
{
    /// <summary>
    /// класс для замены стандартного атрибута "Display", который "не дружит" с локализацией и не цепляет данные из файлов ресурсов
    /// </summary>
    public class LocalizedDisplayNameAttribute : DisplayNameAttribute
    {
        private readonly string resourceName;

        public LocalizedDisplayNameAttribute(string resourceName)
        {
            this.resourceName = resourceName;
        }

        public override string DisplayName
        {
            get
            {
                var displayName = string.Empty;
                try
                {
                    displayName = Resource.ResourceManager.GetString(resourceName, CultureInfo.CurrentUICulture);
                }
                catch (Exception ex)
                {
                    Logger.Error("LocalizedDisplayNameAttribute()", ex);
                }
                return displayName;
            }
        }
    }
}
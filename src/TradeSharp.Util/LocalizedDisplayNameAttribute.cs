using System.ComponentModel;

namespace TradeSharp.Util
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
                return Localizer.GetString(resourceName);
            }
        }
    }
}
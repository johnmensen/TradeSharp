using System;

namespace TradeSharp.SiteAdmin.Helper
{
    /// <summary>
    /// Вспомогательный класс, описывает одно какое-либо свойство. Для более удобной с представлением 'PositionEdit'
    /// </summary>
    public class PropertyMetaData
    {
        /// <summary>
        /// тип свойства
        /// </summary>
        public Type PropertyType { get; set; }

        /// <summary>
        /// имя свойства
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// человеко-понятный заголовок свойства
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// человеко-понятное имя свойства
        /// </summary>
        public string TitleSystemName { get; set; }

        /// <summary>
        /// текущее значение этого свойства
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// комментарий
        /// </summary>
        public string Comment { get; set; }
    }
}
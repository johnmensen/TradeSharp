using System;
using System.ComponentModel;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using TradeSharp.SiteAdmin.Models.Items;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Helper
{
    /// <summary>
    /// Вспомогательный класс, описывает одно какое-либо свойство. Для более удобной с представлением 'SafePositionEdit'
    /// </summary>
    public class SystemProperty
    {
        /// <summary>
        /// тип свойства
        /// </summary>
        public Type PropertyType { get; set; }

        /// <summary>
        /// имя свойства, которое указано в класса (например Side, StopLoss)
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// человеко-понятный заголовок свойства
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// человеко-понятный тип свойства (не System.Nullable`1... а int, Decimal)
        /// </summary>
        public string PropertyTypeName { get; set; }

        /// <summary>
        /// Комментарий к свойству, поясняющий, какие значения нужно записывать
        /// </summary>
        public string PropertyTypeComment { get; set; }

        /// <summary>
        /// текущее значение этого свойства
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// комментарий в таблице. Например, "Количество различных значений: 2 ..." 
        /// </summary>
        public string Comment { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Является ли это поле 'опасным' для редактирования
        /// </summary>
        public bool IsDanger { get; set; }
        
        /// <summary>
        /// Вспомогательное поля для хранения произвольного дополнительного объекта.
        /// Например, туда записывается общее одинаковое значения этого поля у нескольких объектов.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Валидация текущего сзачения свойства
        /// </summary>
        public bool Validation()
        {
            var metaProperty = GetPositionItemMetadataProperty();
            if (metaProperty == null) return true;
            var value = Converter.GetNullableObjectFromString(Value, metaProperty.PropertyType);
            var attrs = metaProperty.GetCustomAttributes();
            return attrs.Where(a => a is ValidationAttribute).Cast<ValidationAttribute>().All(attr => attr.IsValid(value));
        }

        /// <summary>
        /// Вспомогательный метод. Вытаскиваем значения атрибута 'DescriptionAttribute' этого свойства
        /// </summary>
        public void SetDescription()
        {
            var description = string.Empty;
            var metaProperty = GetPositionItemMetadataProperty();
            if (metaProperty != null)
            {
                var attrs = metaProperty.GetCustomAttributes().Where(a => a is DescriptionAttribute).Cast<DescriptionAttribute>().FirstOrDefault();
                if (attrs != null)
                    description = attrs.Description;
            }
            Description = description;
        }

        /// <summary>
        /// Вспомогательный метод
        /// </summary>
        public PropertyInfo GetPositionItemMetadataProperty()
        {
            var atts = typeof(PositionItem).GetCustomAttributes(typeof(MetadataTypeAttribute), true);
            if (atts.Length == 0) return null;
            var metaAttr = atts[0] as MetadataTypeAttribute;
            if (metaAttr == null) return null;
            var metaProperty = metaAttr.MetadataClassType.GetProperty(SystemName);
            return metaProperty;
        }
    }
}
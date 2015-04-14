using System;
using System.Data.SqlClient;

namespace TradeSharp.Util
{
    /// <summary>
    /// свойство-переменная для формул
    /// 
    /// применяется на свойствах классов, значения которых будут подставлены в ExpressionResolver
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ExpressionParameterNameAttribute : Attribute
    {
        public enum StatFieldType
        {
            Number = 0, Boolean = 1
        }

        public string ParamName { get; set; }

        public string ParamTitle { get; set; }

        public StatFieldType FieldType { get; set; }

        private SortOrder defaultSortOrder = SortOrder.Unspecified;
        
        public SortOrder DefaultSortOrder
        {
            get { return defaultSortOrder; }
            set { defaultSortOrder = value; }
        }

        /// <summary>
        /// для форматирования - например, форматировать значение поля в виде
        /// 11.23 % - где суфикс - строка " %"
        /// </summary>
        public string Suffix { get; set; }

        public ExpressionParameterNameAttribute(string name)
        {
            ParamName = name;
        }

        public ExpressionParameterNameAttribute(string name, string title)
        {
            ParamName = name;
            ParamTitle = title;
        }

        public ExpressionParameterNameAttribute(string name, string title, string suffix)
        {
            ParamName = name;
            ParamTitle = title;
            Suffix = suffix;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    // ExpressionParameterNameAttribute with localized title & optionally suffix
    public class LocalizedExpressionParameterNameAttribute : ExpressionParameterNameAttribute
    {
        public LocalizedExpressionParameterNameAttribute(string name) : base(name)
        {
        }

        public LocalizedExpressionParameterNameAttribute(string name, string title) : base(name, Localizer.GetString(title))
        {
        }

        public LocalizedExpressionParameterNameAttribute(string name, string title, string suffix, bool localizeSuffix = false)
            : base(name, Localizer.GetString(title), localizeSuffix ? Localizer.GetString(suffix) : suffix)
        {
        }
    }
}

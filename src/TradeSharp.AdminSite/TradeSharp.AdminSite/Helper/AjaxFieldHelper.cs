using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace TradeSharp.SiteAdmin.Helper
{
    public class AjaxFieldHelper
    {
        /// <summary>
        /// пример использования
        /// @AjaxFieldHelper.EditFieldId((BalanceChangeRequest s) => s.AccountId)
        /// </summary>
        public static IHtmlString EditFieldId<TObject, TProperty>(Expression<Func<TObject, TProperty>> expression)
        {
            var member = expression.Body as MemberExpression;
            var propName = member.Member.Name;
            return new HtmlString("edit" + propName);
        }

        /// <summary>
        /// рендерить строку вида <input type="text" style="width:150px" 
        /// data-validate="{ 'fieldType' : 'float', 'conditions' : [ { 'positive': 'true' } ] }" id="editFieldAmount"/>
        /// 
        /// или <input type="text" style="width:150px" data-validate="{ 'fieldType' : 'string', 'conditions' : [ { 'min_length': 0 }, { 'max_length' : 60} ] }" 
        /// id="editFieldComment"/>
        /// <param name="expression">Нужен для того, что бы сгенерировать id новому html-объекту из имени свойства, для которого формируется Input</param>
        /// </summary>
        public static IHtmlString Input<TObject, TProperty>(Expression<Func<TObject, TProperty>> expression,
            object htmlAttributes = null)
        {
            var member = expression.Body as MemberExpression;
            var propName = member.Member.Name;
            var propertyInfo = (PropertyInfo) member.Member;
            
            // атрибуты валидации
            var fieldType = FieldType(propertyInfo);

            // дополнительные проверки
            var attrs = propertyInfo.GetCustomAttributes();
            var conditions = new Dictionary<string, object>();
            foreach (var attr in attrs)
            {
                if (attr is StringLengthAttribute)
                {
                    var saAttr = (StringLengthAttribute) attr;
                    conditions.Add("max_length", saAttr.MaximumLength);
                    conditions.Add("min_length", saAttr.MinimumLength);
                    continue;
                }
            }
            var validateAttrStr = new StringBuilder("{ 'fieldType' : '" + fieldType + "'");
            if (conditions.Count > 0)
                validateAttrStr.Append(", 'conditions' : [ " +
                                       string.Join(", ", conditions.Select(p =>
                                                                           MakeValidateConditionString(p.Key, p.Value))) +
                                       " ]");
            validateAttrStr.Append(" }");

            // собрать тег
            var container = new TagBuilder("input");
            container.Attributes.Add("type", "text");
            container.Attributes.Add("data-validate", validateAttrStr.ToString());
            container.Attributes.Add("id", "edit" + propName);
            if (htmlAttributes != null)
                container.MergeAttributes(new RouteValueDictionary(htmlAttributes));

            return new HtmlString(container.ToString().Replace("&#39;", "\'").Replace("&#32;", " "));
        }

        /// <summary>
        /// во многом аналогична Input, но рендерит комбо-бокс (select)
        /// </summary>
        /// <param name="expression">Нужен для того, что бы сгенерировать id новому html-объекту из имени свойства, для которого формируется Combobox</param>
        public static IHtmlString Combobox<TObject, TProperty>(Expression<Func<TObject, TProperty>> expression,
            List<string> listValues, List<string> listTitles, object htmlAttributes = null)
        {
            var member = expression.Body as MemberExpression;
            var propName = member.Member.Name;
            
            // собрать тег
            var container = new TagBuilder("select");
            container.Attributes.Add("id", "edit" + propName);
            if (htmlAttributes != null)
                container.MergeAttributes(new RouteValueDictionary(htmlAttributes));

            // опции
            var optionsStr = new StringBuilder();
            for (var i = 0; i < listValues.Count; i++)
            {
                var strVal = listValues[i];
                var strTitle = (listTitles == null || listTitles.Count <= i) ? strVal : listTitles[i];

                optionsStr.AppendLine("        <option value=\"" + strVal + "\">" + strTitle + "</option>");
            }

            container.InnerHtml = optionsStr.ToString();
            return new HtmlString(container.ToString().Replace("&#39;", "\'").Replace("&#32;", " "));
        }

        private static string FieldType(PropertyInfo propertyInfo)
        {
            var fieldType =
                (propertyInfo.PropertyType == typeof (float) ||
                 propertyInfo.PropertyType == typeof (double) ||
                 propertyInfo.PropertyType == typeof (decimal) ||
                 propertyInfo.PropertyType == typeof (float?) ||
                 propertyInfo.PropertyType == typeof (double?) ||
                 propertyInfo.PropertyType == typeof (decimal?))
                    ? "float"
                    : (propertyInfo.PropertyType == typeof (int) ||
                       propertyInfo.PropertyType == typeof (uint) ||
                       propertyInfo.PropertyType == typeof (short) ||
                       propertyInfo.PropertyType == typeof (ushort) ||
                       propertyInfo.PropertyType == typeof (long) ||
                       propertyInfo.PropertyType == typeof (ulong) ||
                       propertyInfo.PropertyType == typeof (int?) ||
                       propertyInfo.PropertyType == typeof (short?) ||
                       propertyInfo.PropertyType == typeof (long?))
                          ? "int"
                          : propertyInfo.PropertyType == typeof (string) ? "string" : "undefined";
            return fieldType;
        }

        public static IHtmlString JsonToBePost(Type objectType, string containerName, string jsonObjectName)
        {
            var propList = new List<PropertyInfo>();
            foreach (var prop in objectType.GetProperties())
            {
                var hid = prop.GetCustomAttributes().OfType<BrowsableAttribute>().Any(atr => !(atr).Browsable);
                if (!hid)
                    propList.Add(prop);
            }

            var str =
                "          if (!validateInputs('" + containerName + "')) return false;\r\n" +
                "          var " + jsonObjectName + " = { " +
                string.Join(",\r\n", propList.Select(MakeJsonGetAccessor)) +
                " };";

            return new HtmlString(str);
        }

        private static string MakeValidateConditionString(string key, object condition)
        {
            var omitQuotes = IsNumericType(condition.GetType());
            return omitQuotes
                       ? string.Format("{{'{0}': {1}}}", key, condition)
                       : string.Format("{{'{0}': '{1}'}}", key, condition);
        }

        private static string MakeJsonGetAccessor(PropertyInfo pi)
        {
            //var omitQuotes = IsNumericType(pi.PropertyType);
            //return omitQuotes
            return string.Format("              {0}: $('#edit{0}').{1}", pi.Name, GetAccessorFuncByProperty(pi.PropertyType));
        }

        private static string GetAccessorFuncByProperty(Type propType)
        {
            return
                (propType == typeof (bool) || Nullable.GetUnderlyingType(propType) == typeof (bool))
                    ? "prop('checked')"
                    : "val()";
        }

        private static readonly HashSet<Type> numericTypes = new HashSet<Type>
        {
            typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)
        };

        internal static bool IsNumericType(Type type)
        {
            return numericTypes.Contains(type) ||
                   numericTypes.Contains(Nullable.GetUnderlyingType(type));
        }
    }
}
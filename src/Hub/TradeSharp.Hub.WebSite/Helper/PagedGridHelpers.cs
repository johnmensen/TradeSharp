using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using TradeSharp.Util;

namespace TradeSharp.Hub.WebSite.Helper
{
    public static class PagedGridHelpers
    {

        public static MvcHtmlString HeaderColumn(this HtmlHelper htmlHelper,
                                                 Type type, string propertyName, string sortProperty,
                                                 bool sortAscending,
                                                 string headerColumnStyle = "table-header-repeat line-left minwidth-1")
        {
            var currentProp = type.GetProperty(propertyName);

            //Содержит человеко-понятное имя свойства из атрибута
            var propTitle = currentProp.Name;
            try
            {
                var attrDisplName = currentProp.CustomAttributes.SingleOrDefault(x => x.AttributeType == typeof(DisplayNameAttribute));
                if (attrDisplName != null) propTitle = (string)attrDisplName.ConstructorArguments[0].Value;
            }
            catch (Exception ex)
            {
                Logger.Error("FillProrertyList() - не удалось прочитать значение DisplayName для свойства " + propertyName, ex);
            }

            return htmlHelper.HeaderColumn(propTitle, propertyName, sortProperty, sortAscending, headerColumnStyle);
        }

        public static MvcHtmlString HeaderColumn(this HtmlHelper htmlHelper,
            string title, string propertyName, string sortProperty,
            bool sortAscending,
            string headerColumnStyle = "table-header-repeat line-left minwidth-1")
        {
            var builderHeader = new TagBuilder("th");
            builderHeader.MergeAttribute("class", headerColumnStyle);

            var builderLink = new TagBuilder("a");
            builderLink.MergeAttribute("href", "#");
            if (sortProperty == propertyName)
            {
                var className = sortAscending ? "sort_asc" : "sort_desc";
                builderLink.MergeAttribute("class", className);
                if (sortAscending)
                    builderLink.MergeAttribute("data-order_asc", "1");
                else                
                    builderLink.MergeAttribute("data-order_asc", "0");
            }

            builderLink.MergeAttribute("onclick", "updateSortCriteria('" +
                propertyName + "', this)");
            builderLink.SetInnerText(title);

            builderHeader.InnerHtml += builderLink.ToString();

            return MvcHtmlString.Create(builderHeader.ToString());
        } 
    }
}
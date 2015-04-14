using System.Linq;
using System.Net;
using System.Text;
using TradeSharp.Util;

namespace TradeSharp.RobotFarm.BL.Web
{
    partial class WebServer
    {
        private void RenderPortfolioSettings(StringBuilder sb)
        {
            RenderAjaxSaveQuery(sb);

            var sets = RobotFarm.Instance.portfolioTradeSettings;
            var setVals = UiFieldAttribute.GetAttributeNameValue(sets);
            
            // сформировать таблицу для редактирования значений атрибутов
            RenderTableOpenTag(sb);
            RenderTableRowTag(sb, true);
            sb.AppendLine("      <td>Параметр</td><td>Значение</td></tr>");

            foreach (var setVal in setVals)
            {
                sb.AppendLine("");
                sb.Append("      <tr><td>" + setVal.DisplayName + "</td><td>");
                if (setVal.PropType.IsAssignableFrom(typeof(bool)))
                {
                    sb.Append("<input type=\"checkbox\" name=\"" + setVal.PropName + "\" ");
                    if (((bool?)setVal.Value) ?? false)
                        sb.Append("checked=\"checked\"");
                    sb.Append("/>");
                }
                else
                {
                    sb.Append("<input type=\"text\" name=\"" + setVal.PropName + "\" value=\"" + 
                        Converter.GetStringFromObject(setVal.Value) + "\"/>");
                }
                sb.Append("</td></tr>");
            }
            sb.AppendLine("    </table>");

            sb.AppendLine("<br/>");
            sb.Append("<a href=\"#\" onclick=\"");
            sb.Append("postChangesToPortfolio()");
            sb.Append("\">Сохранить изменения</a>");
            sb.AppendLine("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href=\"?\">Отменить</a>");
        }

        private bool PortfolioAjaxAction(HttpListenerContext context)
        {
            UiFieldAttribute.SetFieldsFromPropNameValue(
                context.Request.QueryString.AllKeys.ToDictionary(q => q, q => context.Request.QueryString[q]), 
                RobotFarm.Instance.portfolioTradeSettings, true);
            
            RobotFarm.Instance.SaveSettings();
            const string answer = "{\"status\": \"OK\"}";
            WriteJsonResponse(context, answer);
            return true;
        }

        private void RenderAjaxSaveQuery(StringBuilder sb)
        {
            sb.AppendLine("    <script type=\"text/javascript\">");
            sb.AppendLine("       function postChangesToPortfolio() {");
            sb.AppendLine("         var props = {};");
            sb.AppendLine("         $('input').each(function() {");
            sb.AppendLine("             if ($(this).attr('type') == 'checkbox')");
            sb.AppendLine("                 props[$(this).attr('name')] = $(this).prop('checked');");
            sb.AppendLine("             else props[$(this).attr('name')] = $(this).val();");
            sb.AppendLine("         });");
            sb.AppendLine("         var ptrStr = '?actionPortfolio=1';");
            sb.AppendLine("         for (var propertyName in props) {");
            sb.AppendLine("             ptrStr = ptrStr + '&' + propertyName + '=' + props[propertyName];");
            sb.AppendLine("         }");
            //sb.AppendLine("         alert(ptrStr); ");
            sb.AppendLine("         ajaxJsonFunction(ptrStr, null, function (rst) { alert(rst.status); });");
            sb.AppendLine("       }");
            sb.AppendLine("    </script>");
        }
    }
}
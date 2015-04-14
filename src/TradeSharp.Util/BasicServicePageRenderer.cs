using System.Text;

namespace TradeSharp.Util
{
    /// <summary>
    /// класс содержит набор методов для рендеринга шапки страницы сервиса
    /// средствами HttpRequestProcessor
    /// </summary>
    public static class BasicServicePageRenderer
    {
        public static void RenderHead(StringBuilder sb, string title, bool renderScripts,
            string ownStyles, string ownScripts)
        {
            // шапка
            sb.AppendLine("<html>");
            sb.AppendLine("  <head>");
            sb.AppendLine("  <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">");
            sb.AppendLine(string.Format("    <title>{0}</title>", title));

            // стандартные стили
            RenderStyles(sb, ownStyles);
            // и скрипты
            if (renderScripts)
                RenderScripts(sb, ownScripts);
            
            // заголовок
            sb.AppendLine("  </head>");
            sb.AppendLine("  <body>");
            sb.AppendLine(string.Format("    <h2>{0}</h2>", title));            
        }

        public static void RenderFooter(StringBuilder sb)
        {
            sb.AppendLine("  </body>");
            sb.AppendLine("</html>");
        }

        private static void RenderStyles(StringBuilder sb, string ownStyles)
        {
            sb.AppendLine("    <style type=\"text/css\">");
            sb.AppendLine("      div.pageBreaker {page-break-after: always;}");
            sb.AppendLine("      a {text-decoration: none; font-weight:bold; color:black;}");
            sb.AppendLine("      a:hover {color:#505035;}");
            sb.AppendLine("      table.lightTable { border-width: 0 0 1px 1px; border-spacing: 0; border-collapse: collapse; border-style: solid; }");
            sb.AppendLine("      table.lightTable tr td { margin: 0; padding: 4px; border-width: 1px 1px 0 0; border-style: solid; }");
            sb.AppendLine("      tr.rowHeader { background-color: #BBFFAB; font-weight:bold; }");
            sb.AppendLine("      td.fadeCell { color:#888888; }");
            sb.AppendLine("      ul.css-menu-3 { list-style: none; border-bottom: 5px solid #90BB90; border-top: 1px solid #609060; padding: 11px; background: #AFCCAF }");
            sb.AppendLine("      ul.css-menu-3 li { display: inline }");
            sb.AppendLine("      ul.css-menu-3 li a { color: #fefefe; text-decoration: none; background: #709070; border: 1px solid #709070; border-bottom: 1px solid #609060; margin: 0; padding: 10px 14px 10px 14px }");
            sb.AppendLine("      ul.css-menu-3 li a:hover { border-left: 1px solid #609060; border-right: 1px solid #609060 }");
            sb.AppendLine("      ul.css-menu-3 li a.selected { color: #fefefe; background: #90BB90; border: 1px #90BB90; solid; border-bottom: 2px solid #90BB90; padding: 10px 14px 10px 14px }");
            sb.AppendLine(ownStyles);
            sb.AppendLine("    </style>");
        }

        private static void RenderScripts(StringBuilder sb, string ownScripts)
        {
            sb.AppendLine("    <script type=\"text/javascript\">");
            sb.AppendLine("      function expand(senderId, targetId) {");
            sb.AppendLine("        var targetItem = document.getElementById(targetId);");
            sb.AppendLine("        var display = targetItem.style.display == 'none'");
            sb.AppendLine("          ? 'block' : 'none';");
            sb.AppendLine("        targetItem.style.display = display;");
            sb.AppendLine("        var sender = document.getElementById(senderId);");
            sb.AppendLine("        if (sender.innerHTML == 'развернуть')");
            sb.AppendLine("            sender.innerHTML = 'свернуть';");
            sb.AppendLine("        else sender.innerHTML = 'развернуть';");
            sb.AppendLine("      }");
            RenderAjaxScripts(sb);
            sb.AppendLine(ownScripts);
            sb.AppendLine("    </script>");
        }

        private static void RenderAjaxScripts(StringBuilder sb)
        {
            sb.AppendLine("       var processAjaxResult = null;");
            sb.AppendLine("       function ajaxFunction(requestParams) {");
            sb.AppendLine("         var ajaxRequest;");
            sb.AppendLine("         try {");
            sb.AppendLine("           ajaxRequest = new XMLHttpRequest();");
            sb.AppendLine("         } catch (e)");
            sb.AppendLine("         {");
            sb.AppendLine("           try {");
            sb.AppendLine("             ajaxRequest = new ActiveXObject(\"Msxml2.XMLHTTP\");");
            sb.AppendLine("           } catch (e)");
            sb.AppendLine("           {");
            sb.AppendLine("             try {");
            sb.AppendLine("                   ajaxRequest = new ActiveXObject(\"Microsoft.XMLHTTP\");");
            sb.AppendLine("             } catch (e)");
            sb.AppendLine("             {");
            sb.AppendLine("                alert(\"AJAX error\");");
            sb.AppendLine("             }");
            sb.AppendLine("           }");
            sb.AppendLine("         }");
            sb.AppendLine("         ajaxRequest.onreadystatechange = function(requestParams) {");
            sb.AppendLine("           if (ajaxRequest.readyState == 4 && processAjaxResult)");
            sb.AppendLine("              processAjaxResult(ajaxRequest.responseText);");
            sb.AppendLine("         }");
            sb.AppendLine("         var docUrl = document.URL;");
            sb.AppendLine("         if (docUrl.charAt(docUrl.length-1)=='#') docUrl = docUrl.substr(0, docUrl.length-1);");
            sb.AppendLine("         if (docUrl.charAt(docUrl.length-1)=='/') docUrl = docUrl.substr(0, docUrl.length-1);");
            sb.AppendLine("         var reqUrl = docUrl + '?' + requestParams;");
            sb.AppendLine("         ajaxRequest.open(\"GET\", reqUrl);");
            sb.AppendLine("         ajaxRequest.send(null);");
            sb.AppendLine("       }");
        }

        #region Рендеринг таблиц
        public static void RenderTableHeader(StringBuilder sb, string id, string userAttributes)
        {
            sb.AppendLine(string.Format(
                "      <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"lightTable\" id=\"{0}\" {1}>",
                id, userAttributes));
        }
        public static void RenderTableFooter(StringBuilder sb, string id)
        {
            sb.AppendLine("      </table>");
        }
        public static void RenderRowOpen(StringBuilder sb, bool isHeader, string userAttributes)
        {
            sb.AppendLine(string.Format("       <tr{0} {1}>",
                isHeader ? " class=\"rowHeader\"" : "", userAttributes));
        }
        public static void RenderRowClose(StringBuilder sb)
        {
            sb.AppendLine("       </tr>");
        }
        #endregion

        #region Ссылка - свернуть / развернуть
        public static void RenderExpandCollapseLink(StringBuilder sb, string linkId, string targetId)
        {
            sb.AppendLine(string.Format(
                "      <span id=\"{0}\" onclick=\"expand('{0}', '{1}')\" style=\"cursor:pointer\">развернуть</span>",
                linkId, targetId));
        }
        #endregion
    }
}
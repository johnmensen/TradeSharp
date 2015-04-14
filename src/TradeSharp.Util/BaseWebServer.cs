using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace TradeSharp.Util
{
    public abstract class BaseWebServer
    {
        public abstract string ServiceName { get; }

        public static readonly bool needAuthentication = AppConfig.GetBooleanParam("WebServer.Authentication", true);

        public abstract void ProcessHttpRequest(HttpListenerContext context);

        /// <summary>
        /// получить собственный порт, который должен прослушивать сервис
        /// </summary>
        public static int GetPortToListen()
        {
            return AppConfig.GetIntParam("WebServer.Port", 8071);
        }

        /// <summary>
        /// рендерить заголовок Html, содержащий стили и прочую атрибутику
        /// </summary>
        public void RenderHttpHead(StringBuilder sb,
            string strStyle,
            string strScript,
            bool insertAjaxRequestScript)
        {
            sb.AppendLine("<html>");
            sb.AppendLine("  <head>");
            // кодировка
            sb.AppendLine("    <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");
            // заголовок - имя
            var serviceName = ServiceName;
            sb.AppendLine(string.Format("    <title>{0}</title>", serviceName));
            // стили
            sb.AppendLine("    <style type=\"text/css\">");
            sb.AppendLine("      div.pageBreaker {page-break-after: always;}");
            sb.AppendLine("      a {text-decoration: none; font-weight:bold; color:black;}");
            sb.AppendLine("      a:hover {color:#505035;}");
            sb.AppendLine("      table.lightTable { border-width: 0 0 1px 1px; border-spacing: 0; border-collapse: collapse; border-style: solid; }");
            sb.AppendLine("      table.lightTable tr td { margin: 0; padding: 4px; border-width: 1px 1px 0 0; border-style: solid; }");
            sb.AppendLine("      table.lightTable tr:nth-child(2n) { background-color: #ECECEC; }");
            sb.AppendLine("      table.lightTable tr th { background-color: #BBFFAB; font-weight:bold; }");
            sb.AppendLine("      tr.rowHeader { background-color: #BBFFAB; font-weight:bold; }");
            sb.AppendLine("      td.fadeCell { color:#888888; }");
            sb.AppendLine("      ul.css-menu-3 { list-style: none; border-bottom: 5px solid #90BB90; border-top: 1px solid #609060; padding: 11px; background: #AFCCAF }");
            sb.AppendLine("      ul.css-menu-3 li { display: inline }");
            sb.AppendLine("      ul.css-menu-3 li a { color: #fefefe; text-decoration: none; background: #709070; border: 1px solid #709070; border-bottom: 1px solid #609060; margin: 0; padding: 10px 14px 10px 14px }");
            sb.AppendLine("      ul.css-menu-3 li a:hover { border-left: 1px solid #609060; border-right: 1px solid #609060 }");
            sb.AppendLine("      ul.css-menu-3 li a.selected { color: #fefefe; background: #90BB90; border: 1px #90BB90; solid; border-bottom: 2px solid #90BB90; padding: 10px 14px 10px 14px }");
            sb.AppendLine(strStyle);
            sb.AppendLine("    </style>");
            sb.AppendLine("    <script type=\"text/javascript\" src=\"http://ajax.googleapis.com/ajax/libs/jquery/1.10.2/jquery.min.js\"></script>");
            sb.AppendLine("    <script src=\"http://code.jquery.com/jquery-1.10.1.min.js\"></script>");
            sb.AppendLine("    <script src=\"http://code.jquery.com/jquery-migrate-1.2.1.min.js\"></script>");
            sb.AppendLine("    <script type=\"text/javascript\" language=\"JavaScript\">");

            RenderUtilScripts(sb);
            if (insertAjaxRequestScript) RenderAjaxScripts(sb);

            sb.AppendLine(strScript);
            sb.AppendLine("    </script>");
            sb.AppendLine("  </head>");
        }

        private static void RenderUtilScripts(StringBuilder sb)
        {
            // показывает на заданное время DIV с заданным сообщением
            sb.AppendLine("       function showMessage(msgDivId, msgText, delay)");
            sb.AppendLine("       {");
            sb.AppendLine("           var dv = document.getElementById(msgDivId);");
            sb.AppendLine("           if (!dv) return;");
            sb.AppendLine("           dv.innerHTML = msgText;");
            sb.AppendLine("           dv.style.display = 'block';");
            sb.AppendLine("           setTimeout(\"document.getElementById('\" + msgDivId +  \"').style.display = 'none';\", delay);");
            sb.AppendLine("       }");
            // в обработчиках типа onclick получить отправителя. Пример: onclick="alert(getSender(event))"
            sb.AppendLine("       function getSender(e) { return (e && e.target) || (window.event && window.event.srcElement); }");
        }

        private static void RenderAjaxScripts(StringBuilder sb)
        {
            sb.AppendLine("       var processAjaxResult = null;");
            sb.AppendLine("       function ajaxFunction(requestParams, resultFun) {");
            sb.AppendLine("         resultFun = resultFun || processAjaxResult;");
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
            sb.AppendLine("           if (ajaxRequest.readyState == 4 && resultFun)");
            sb.AppendLine("              resultFun(ajaxRequest.responseText);");
            sb.AppendLine("         }");
            sb.AppendLine("         var docUrl = document.URL;");
            sb.AppendLine("         if (docUrl.charAt(docUrl.length-1)=='#') docUrl = docUrl.substr(0, docUrl.length-1);");
            sb.AppendLine("         if (docUrl.charAt(docUrl.length-1)=='/') docUrl = docUrl.substr(0, docUrl.length-1);");
            sb.AppendLine("         docUrl = docUrl.split(\"?\")[0];");
            sb.AppendLine("         var reqUrl = docUrl + '?' + requestParams;");
            //sb.AppendLine("         alert(reqUrl);");
            sb.AppendLine("         ajaxRequest.open(\"GET\", reqUrl);");
            sb.AppendLine("         ajaxRequest.send(null);");
            sb.AppendLine("       }");

            

            sb.AppendLine("         ");
            sb.AppendLine("         ");
            sb.AppendLine("         ");
            sb.AppendLine("         ");
            sb.AppendLine("         ");
            sb.AppendLine("         ");
            sb.AppendLine("         ");
            sb.AppendLine("         ");
            sb.AppendLine("         ");
            sb.AppendLine("         ");
        }

        public void RenderBodyOpenTag(StringBuilder sb)
        {
            sb.AppendLine("  <body>");
            //sb.AppendLine("  <form name=\"controlForm\" action=\".\" method=\"post\">");
        }

        public void RenderBodyCloseTag(StringBuilder sb)
        {
            //sb.AppendLine("  </form>");
            sb.AppendLine("  </body>");
        }

        public void RenderHttpCloseTag(StringBuilder sb)
        {
            sb.AppendLine("</http>");
        }

        public static void RenderTableOpenTag(StringBuilder sb)
        {
            sb.Append("    <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"lightTable\">");
        }

        public static void RenderTableRowTag(StringBuilder sb, bool isHeader)
        {
            sb.Append(isHeader ? "    <tr class=\"rowHeader\">" : "<tr>");
        }

        public static void RenderPopupDiv(StringBuilder sb, 
            int left, int top,
            string divId = "divMessage")
        {
            sb.AppendLine("        <div id=\"" + divId + "\" style=\"display: none;position:absolute;" +
                          "left: " + left + "px; top: " + top + "px;padding:20px;border:1px solid #000;" +
                          "z-index: 9002;vertical-align: middle;text-align: center;background-color: White;\">");
            sb.AppendLine("             Message");
            sb.AppendLine("        </div>");
        }

        public static string GetRequestPostData(HttpListenerRequest request)
        {
            if (!request.HasEntityBody) return string.Empty;
            using (var body = request.InputStream)
            {
                using (var reader = new StreamReader(body, request.ContentEncoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// вернуть встроенный в HTML код (биты) картинки для подстановки в src=""
        /// </summary>
        public static string MakeEmbeddedPictureString(Bitmap pic)
        {
            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                pic.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                bytes = memoryStream.ToArray();
            }
            return "data:image/png;base64," + Convert.ToBase64String(bytes, Base64FormattingOptions.None);
        }

        /// <summary>
        /// проверить, разрешен ли пользователю доступ к веб-интерфейсу
        /// если нет - рендерить сообщение об ошибке аутентификации
        /// </summary>
        public static bool CheckCredentials(HttpListenerContext context, StringBuilder sb)
        {
            if (!needAuthentication) return true;

            string errorMsg = "Пользователь не аутентифицирован";
            if (context.User != null)
                //if (!context.User.Identity.IsAuthenticated)
                errorMsg = string.Empty;
            if (string.IsNullOrEmpty(errorMsg))
            {
                var userName = context.User.Identity.Name;
                // проверить, разрешены ли пользователю ресурсы
                var isEnabled = GetEnabledUserNames().Any(u => u.Equals(userName, StringComparison.OrdinalIgnoreCase));
                if (!isEnabled)
                    errorMsg = string.Format("Пользователю \"{0}\" не разрешен доступ к Web-интерфейсу BSEngine",
                                             userName);
            }
            if (string.IsNullOrEmpty(errorMsg)) return true;
            // вывести сообщение об ошибке авторизации
            sb.AppendLine("    <br />");
            sb.AppendLine("    <table style=\"border:1px solid #FF0000;empty-cells:show;background-color:#ff6666;\">");
            sb.AppendLine("      <tr style=\"height:30px\"><td></td>");
            sb.AppendLine("      <td style=\"color:#FFFFFF;font-weight:bolder\">Ошибка авторизации</td><td></td></tr>");
            sb.AppendLine("      <tr><td></td><td style=\"background-color:#ffffff;padding:30px;padding-top:20px;" +
                "padding-bottom:20px;border:1px solid #FF0000;font-weight:bold\">");
            sb.AppendLine(string.Format("      {0} </td><td></td></tr>", errorMsg));
            sb.AppendLine("      <tr><td></td><td></td><td></td></tr>");
            sb.AppendLine("    </table>");
            sb.AppendLine("    <br />");
            return false;
        }

        public static void WriteResponse(string resp, HttpListenerContext context)
        {
            var b = Encoding.UTF8.GetBytes(resp);
            context.Response.ContentLength64 = b.Length;
            context.Response.OutputStream.Write(b, 0, b.Length);
            context.Response.OutputStream.Close();
        }

        private static List<string> GetEnabledUserNames()
        {
            var enabledNames = new List<string>();
            // прочитать .config
            var enabledLogins = AppConfig.GetStringParam("WebServer.EnabledLogins", "");
            var logins = enabledLogins.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var loginFmt in
                logins.Select(login => login.Trim()).Where(loginFmt => !enabledNames.Contains(loginFmt)))
                enabledNames.Add(loginFmt);

            var disabledLogins = AppConfig.GetStringParam("WebServer.DisabledLogins", "");
            logins = disabledLogins.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var loginFmt in logins.Select(login => login.Trim()))
                enabledNames.Remove(loginFmt);

            return enabledNames;
        }
    }
}

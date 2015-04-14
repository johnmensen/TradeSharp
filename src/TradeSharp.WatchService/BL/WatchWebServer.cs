using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Linq;
using System.Reflection;
using System.Text;
using TradeSharp.Contract.WebContract;
using TradeSharp.Util;

namespace TradeSharp.WatchService.BL
{
    class WatchWebServer : BaseWebServer
    {
        private enum SiteMode
        {
            Units,
            Services,
            ErrorMessageOptions
        }

        private static WatchWebServer instance;

        public static WatchWebServer Instance
        {
            get { return instance ?? (instance = new WatchWebServer()); }
        }

        private readonly string imgMarkupStart, imgMarkupStop;

        private bool sendErrorMessage = true;
        public bool SendErrorMessage
        {
            get { return sendErrorMessage; }
        }

        private readonly Dictionary<SiteMode, string> siteModeName = new Dictionary<SiteMode, string>
            {
                {SiteMode.Units, "Состояние модулей"},
                {SiteMode.Services, "Службы Windows"},
                {SiteMode.ErrorMessageOptions, "Сообщения об ошибках"}
            };

        private readonly Dictionary<SiteMode, string> siteModeQuery = new Dictionary<SiteMode, string>
            {
                {SiteMode.Units, "?units=1"},
                {SiteMode.Services, "?services=1"},
                {SiteMode.ErrorMessageOptions, "?errorMessageOptions=0"}
            };
        
        public override string ServiceName
        {
            get { return "WatchService"; }
        }

        private WatchWebServer()
        {
            // прочитать картинки из ресурсов и рендерить их в строку
            imgMarkupStart = LoadResourceImageString("TradeSharp.WatchService.images.ico_play.png");
            imgMarkupStop = LoadResourceImageString("TradeSharp.WatchService.images.ico_stop.png");
        }

        /// <summary>
        /// выдать, на выбор, либо состояние наблюдаемых служб,
        /// либо состояние сервисов windows
        /// </summary>
        public override void ProcessHttpRequest(HttpListenerContext context)
        {
            string response;
            Logger.Info("KEYS: " + string.Join(";", context.Request.QueryString.AllKeys));

            try
            {
                if (context.Request.QueryString.AllKeys.Contains("formatquery"))
                // параметры запроса переданы в формате JSON
                    response = ProcessFormattedHttpRequest(context);
                else if (context.Request.QueryString.AllKeys.Contains("startservice"))
                // стартовать службу    
                    response = StartService(context.Request.QueryString["startservice"]);
                else if (context.Request.QueryString.AllKeys.Contains("stopservice"))
                // остановить службу
                    response = StopService(context.Request.QueryString["stopservice"]);
                else if (context.Request.QueryString.AllKeys.Contains("services"))
                // состояние служб
                    response = RenderServicesStatuses(context);
                else if (context.Request.QueryString.AllKeys.Contains("errorMessageOptions"))
                {
                    if (context.Request.QueryString["errorMessageOptions"] == "1")
                    sendErrorMessage = !sendErrorMessage;

                    response = RenderErrorMessageOptions(context);
                }
                else
                // состояние наблюдаемых процессов
                    response = RenderUnitsStatuses(context);
            }
            catch (Exception ex)
            {
                response = "error: " + ex;
            }
            
            // записать ответ в поток
            var r = Encoding.UTF8.GetBytes(response);
            context.Response.ContentLength64 = r.Length;
            context.Response.OutputStream.Write(r, 0, r.Length);
        }

        /// <summary>
        /// параметры запроса переданы в формате JSON
        /// 
        /// возвращает ответ, сериализованный в JSON
        /// если параметры запроса не прочитаны - вернуть состояние всех сервисов
        /// иначе - выполнить действие
        /// </summary>        
        private string ProcessFormattedHttpRequest(HttpListenerContext context)
        {
            using (var reader = new StreamReader(context.Request.InputStream,
                                     context.Request.ContentEncoding))
            {
                var text = reader.ReadToEnd();
                if (string.IsNullOrEmpty(text)) return GetServiceStateJSon();
                var ptrs = HttpParameter.DeserializeFromJSon(text);
                if (ptrs.Count == 0) return GetServiceStateJSon();

                // остановить / запустить сервис
                if (ptrs[0] is TradeSharpServiceStartStop)
                {
                    var cmd = (TradeSharpServiceStartStop) ptrs[0];
                    var report = new ExecutionReport();
                    if (cmd.ShouldStart)
                    {
                        var status = ServiceProcessManager.StartProcess(cmd.SrvName);
                        report.IsOk = status == ServiceProcessManager.StartProcessStatus.OK;
                        report.Comment = status.ToString();
                    }
                    else
                    {
                        var status = ServiceProcessManager.KillProcess(cmd.SrvName);
                        report.IsOk = status == ServiceProcessManager.KillProcessStatus.OK;
                        report.Comment = status.ToString();
                    }
                    return HttpParameter.SerializeInJSon(new List<HttpParameter> { report });
                }
            }

            return GetServiceStateJSon();
        }

        private string GetServiceStateJSon()
        {
            var srvStatus = ServiceProcessManager.GetProcessesStates();
            if (srvStatus.Count == 0) return string.Empty;
            return HttpParameter.SerializeInJSon(srvStatus.Cast<HttpParameter>().ToList());
        }

        private string StartService(string srvName)
        {
            Logger.InfoFormat("StartService({0})", srvName);
            return ServiceProcessManager.StartProcess(srvName).ToString();
        }

        private string StopService(string srvName)
        {
            Logger.InfoFormat("StopService({0})", srvName);
            return ServiceProcessManager.KillProcess(srvName).ToString();
        }

        private string RenderServicesStatuses(HttpListenerContext context)
        {
            var sb = new StringBuilder();
            RenderSiteHeader(sb, SiteMode.Services);

            // вывести состояние служб в виде таблицы
            var srvStatus = ServiceProcessManager.GetProcessesStates();
            if (srvStatus.Count > 0)
            {
                sb.AppendLine("      <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"lightTable\">");
                sb.AppendLine("        <tr class=\"rowHeader\"><td>Служба</td><td>Процесс</td><td>Состояние</td><td>Управление</td></tr>");

                foreach (var stat in srvStatus)
                {
                    sb.AppendLine();
                    sb.AppendFormat("        <tr><td>{0}</td><td>{1}</td><td>{2}</td>",
                                                stat.Title, stat.FileName, stat.Status);
                    
                    // добавить кнопки - остановить и запустить службу
                    sb.AppendFormat("<td><img alt=\"Остановить\" style=\"cursor:pointer\" src=\"{0}\" onclick=\"ajaxFunction('stopservice=' + encodeURIComponent('{1}'))\" /> ",
                        imgMarkupStop, stat.Name);
                    sb.AppendFormat("<img alt=\"Запустить\" style=\"cursor:pointer\" src=\"{0}\" onclick=\"ajaxFunction('startservice=' + encodeURIComponent('{1}'))\" /></td>",
                        imgMarkupStart, stat.Name);
                    sb.Append("</tr>");
                }

                sb.AppendLine("      </table>");
            }

            // закрыть документ
            RenderBodyCloseTag(sb);
            RenderHttpCloseTag(sb);
            return sb.ToString();
        }

        private string RenderUnitsStatuses(HttpListenerContext context)
        {
            var sb = new StringBuilder();
            RenderSiteHeader(sb, SiteMode.Units);

            var srvStatus = ServiceStatePool.Instance.GetServiceState();
            if (srvStatus.Count > 0)
            {
                sb.AppendLine("      <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"lightTable\">");
                sb.AppendLine("        <tr class=\"rowHeader\"><td>Служба</td><td>Состояние</td><td>Ошибка</td><td>Время ошибки</td></tr>");

                foreach (var stat in srvStatus)
                {
                    sb.AppendLine(string.Format("        <tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td></tr>",
                                                stat.Key, stat.Value.State, stat.Value.LastError, 
                                                stat.Value.LastErrorOccured.ToStringUniform()));
                }

                sb.AppendLine("      </table>");
            }

            // закрыть документ
            RenderBodyCloseTag(sb);
            RenderHttpCloseTag(sb);
            return sb.ToString();
        }

        private string RenderErrorMessageOptions(HttpListenerContext context)
        {
            var sb = new StringBuilder();
            RenderSiteHeader(sb, SiteMode.ErrorMessageOptions);

            //Отключить сообщения об ошибках
            sb.AppendLine(sendErrorMessage
                              ? "      <a href=\"?errorMessageOptions=1\">Отключить SMS сообщения о сбоях служб</a>"
                              : "      <a href=\"?errorMessageOptions=1\">Включить SMS сообщения о сбоях служб</a>");

            // закрыть документ
            RenderBodyCloseTag(sb);
            RenderHttpCloseTag(sb);
            return sb.ToString();
        }

        private void RenderSiteHeader(StringBuilder sb, SiteMode currentMode)
        {
            RenderBodyOpenTag(sb);
            RenderHttpHead(sb, "",
                "\n    processAjaxResult = function(resptext) { alert(resptext); }\n", 
                true);

            sb.AppendLine("    <h2>Сервис контроля состояния</h2>");
            sb.AppendLine();
            // рендерить "меню сайта"
            sb.AppendLine("    <hr/>");
            sb.AppendLine("      <p>");
            sb.AppendLine("        ");
            foreach (SiteMode mode in Enum.GetValues(typeof (SiteMode)))
            {
                var modeName = siteModeName[mode];
                if (mode == currentMode)
                    sb.Append(" " + modeName);
                else
                {
                    var query = siteModeQuery[mode];
                    sb.AppendFormat(" <a href=\"{0}\">{1}</a>", query, modeName);
                }
            }
            sb.AppendLine("      </p>");
            sb.AppendLine("    <hr/> <br/>");
        }        

        private static string LoadResourceImageString(string resName)
        {
            var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                resName);
            if (imageStream == null) return string.Empty;
            var bitmapBackgr = new Bitmap(imageStream);
            return MakeEmbeddedPictureString(bitmapBackgr);
        }
    }
}

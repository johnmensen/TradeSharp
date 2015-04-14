using System.Linq;
using System.Net;
using System.Text;
using TradeSharp.Util;

namespace TradeSharp.TradeSignalExecutor.BL
{
    internal class WebServer : BaseWebServer
    {
        private static WebServer instance;

        public static WebServer Instance
        {
            get { return instance ?? (instance = new WebServer()); }
        }

        public override string ServiceName
        {
            get { return "Процессор сигналов"; }
        }

        public override void ProcessHttpRequest(HttpListenerContext context)
        {
            var sb = new StringBuilder();
            RenderHttpHead(sb, "", "", true);
            
            // добавить шапку
            sb.AppendLine("<body style=\"padding-top:10px\">");
            
            // таблица отработанных сигналов
            RenderProcessedSignals(sb);

            sb.AppendLine("</body>\r\n</html>");

            // записать ответ
            var r = Encoding.UTF8.GetBytes(sb.ToString());
            context.Response.ContentLength64 = r.Length;

            context.Response.OutputStream.Write(r, 0, r.Length);
        }

        private void RenderProcessedSignals(StringBuilder sb)
        {
            var signals = Dealer.processedSignals.ToArray();
            if (signals.Length == 0)
            {
                sb.AppendLine("    <p>Нет данных по обработанным торговым сигналам.</p>");
                return;
            }

            // рендерить таблицу
            RenderTableOpenTag(sb);
            RenderTableRowTag(sb, true);
            sb.AppendLine("<td>Время</td> <td>Сигнал</td> <td>Получатели</td></tr>");

            foreach (var signal in signals)
            {
                sb.AppendLine("      <tr>");
                sb.AppendLine(string.Format("        <td>{0:dd MMM HH:mm:ss}</td> <td>{1}</td> <td>{2}</td>",
                                            signal.time, signal.signal, RenderSignalDeliveryStatus(signal)));
                sb.AppendLine("      </tr>");
            }

            sb.AppendLine("    </table>");
        }

        private static string RenderSignalDeliveryStatus(ProcessedSignal signal)
        {
            if (signal.subscriberAccountProcessingStatus.Count == 0)
                return "-";
            if (signal.subscriberAccountProcessingStatus.Count == 1)
            {
                var deliveryStatus = signal.subscriberAccountProcessingStatus.First();
                return "#" + deliveryStatus.Key + ": " + deliveryStatus.Value;
            }
            var sb = new StringBuilder("<a href=\"#\" onclick=\"$(this).next().toggle()\">Подписчиков: " +
                                       signal.subscriberAccountProcessingStatus.Count + "</a>");
            sb.AppendLine("      <div style=\"display:none\">");
            sb.Append(string.Join("<br/>", signal.subscriberAccountProcessingStatus.Select(p =>
                                                                                           "#" + p.Key + ": " + p.Value)));
            sb.AppendLine(" </div>");
            return sb.ToString();
        }
    }
}
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using TradeSharp.Delivery.Contract;
using TradeSharp.Util;

namespace TradeSharp.Delivery.Service.WebServer
{
    /// <summary>
    /// Этот класс реализует сервер, слушая порт и возвращая разметку по запросу
    /// </summary>
    class DeliveryWebServer : BaseWebServer
    {
        private static DeliveryWebServer instance;
        public static DeliveryWebServer Instance
        {
            get { return instance ?? (instance = new DeliveryWebServer()); }
        }

        private const string SrvName = "TradeSharp.Delivery";

        public override void ProcessHttpRequest(HttpListenerContext context)
        {
            var sb = new StringBuilder();
            RenderHttpHead(sb, "", GetScripts(), true);
            RenderBodyOpenTag(sb);

            if (context.Request.QueryString.HasKeys())
            {
                if (context.Request.QueryString.AllKeys.Contains("clearFailReceivers"))
                #region
                {
                    EmailSender.Instance.ClearFailReceiversList();
                    RenderHomePage(sb);
                }
                #endregion    
                if (context.Request.QueryString.AllKeys.Contains("clearErrors"))
                #region 
                {
                    EmailSender.Instance.ClearErrorList();
                    RenderHomePage(sb);
                }
                #endregion
                if (context.Request.QueryString.AllKeys.Contains("options"))
                #region
                {
                    OptionsPage(sb);
                }
                #endregion
                if (context.Request.QueryString.AllKeys.Contains("testMessage"))
                #region
                {
                    TestMessagePage(sb);
                }
                #endregion
                if (context.Request.QueryString.AllKeys.Contains("saveSettings"))
                #region
                {
                    if (context.Request.QueryString.AllKeys.Contains("sendMailToFailList"))
                        EmailSender.Instance.SendToAddressInFailList =
                            context.Request.QueryString["sendMailToFailList"].ToLower() == "true";

                    int minUrgency;
                        if (context.Request.QueryString.AllKeys.Contains("minUrgency") && 
                            int.TryParse(context.Request.QueryString["minUrgency"], out minUrgency)) 
                            EmailSender.Instance.MinUrgency = (UrgencyFlag)minUrgency;
                    
                    WriteResponse("OK", context);
                    return;
                }
                #endregion
                if (context.Request.QueryString.AllKeys.Contains("sendTestMessage"))
                #region
                {
                    using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                    {
                        var param = reader.ReadToEnd().Split('&').ToDictionary(x => x.Split('=')[0].Trim(), x => x.Split('=')[1].Trim());

                        var isMarkup = false;
                        if (param.Keys.Contains("isMarkup")) isMarkup = param["isMarkup"].ToLower() == "true";

                        if (param.Keys.Contains("adress") && param.Keys.Contains("body"))
                            EmailSender.Instance.DeliverEmail(new EmailMessage
                                {
                                    HTMLFormat = isMarkup,
                                    Urgency = UrgencyFlag.Normal,
                                    Receivers = new[] { param["adress"] },
                                    Title = "Тестовое сообщение",
                                    Body = param["body"]
                                });


                        WriteResponse("OK", context);
                        return;
                    }
                }
                #endregion
                
                context.Request.QueryString.Clear();
            }
            else
            {
                RenderHomePage(sb);
            }
            

            RenderBodyCloseTag(sb); 
            RenderHttpCloseTag(sb);

            // записать ответ
            WriteResponse(sb.ToString(), context);
        }

        /// <summary>
        /// Генерация html разметки страници с формой отправки тестового сообщения
        /// </summary>
        private static void TestMessagePage(StringBuilder sb)
        {
            sb.AppendLine("     <h2>Служба рассылки сообщений : отправка тестового сообщения</h2>");
            sb.AppendLine("     <span>Адрес электронной почты</span></br>");
            sb.AppendLine("     <input type='text' name='testMailAdress' id='testMailAdress' ></input></br>");
            sb.AppendLine("     <span>Текст тестового сообщение</span></br>");
            sb.AppendLine("     <textarea cols='40' rows='5' name='testMailBody' id='testMailBody' ></textarea></br>");
            sb.AppendLine("     <input type='checkbox' id='isMarkup' name='isMarkup' value='isMarkup'>разметка</input></br></br>");
            sb.AppendLine("     <a href='#'  onclick=\"" +

                          "     var arg1=$('#testMailAdress').val(); " +
                          "     var arg2=$('#testMailBody').val(); " +
                          "     var arg3=$('#isMarkup').prop('checked'); " +
                          "    $.ajax({ \r\n" +
                          "       type: 'POST', \r\n" +
                          "       url: '?sendTestMessage=1', \r\n" +
                          "       data: 'adress='+ arg1 +'&body='+ arg2 + '&isMarkup=' + arg3, \r\n" +
                          "       success: function (data, textStatus, jqXHR) { \r\n  " +
                          "       }, \r\n" +
                          "       error: function (jqXHR, textStatus, errorThrown) { \r\n " +
                          "       } \r\n" +
                          "   }); return false;\" \">Отправить</a>");
        }

        /// <summary>
        /// Генерация html разметки страници настроек службы
        /// </summary>
        private static void OptionsPage(StringBuilder sb)
        {
            sb.AppendLine("     <h2>Служба рассылки сообщений : настройки рассылки</h2>");
            sb.AppendLine("     <a href=\".?clearFailList=1\">Очистить список заблокированных адресов</a></br></br>");
            sb.AppendLine("     <span>Минимальный уровень срочности</span><select id='minUrgency' name='minUrgency'>");
            foreach (var urgency in (UrgencyFlag[])Enum.GetValues(typeof(UrgencyFlag)))
            {
                sb.AppendLine(EmailSender.Instance.MinUrgency == urgency
                                  ? string.Format("     <option value=\"{0}\" selected>{1}</option>", (int) urgency,
                                                  urgency)
                                  : string.Format("     <option value=\"{0}\" >{1}</option>", (int) urgency, urgency));
            }
            sb.AppendLine("     </select></br>");
            sb.AppendLine(EmailSender.Instance.SendToAddressInFailList ? 
                "     <input type='checkbox' id='sendMailToFailList' name='sendMailToFailList' value='dontSend' checked >Отправлять по списку заблокированных адресов</input></br></br>" :
                "     <input type='checkbox' id='sendMailToFailList' name='sendMailToFailList' value='dontSend' >Отправлять по списку заблокированных адресов</input></br></br>");

            sb.AppendLine("     <a href=\"#\"  onclick=\"if (!confirm('Сохранить настройки?')) return; " +
                          "var arg1=$('#sendMailToFailList').prop('checked'); " +
                          "var arg2=$('#minUrgency option:selected').val(); " +
                          "ajaxFunction('saveSettings=1&sendMailToFailList='+arg1+'&minUrgency='+arg2);  " +
                          "\">Сохранить настройки</a>");
        }

        /// <summary>
        /// Генерация html разметки страници с отчетом об ошибках
        /// </summary>
        private static void RenderHomePage(StringBuilder sb)
        {
            sb.AppendLine("     <h2>Служба рассылки сообщений : ошибки рассылки</h2>");
            sb.AppendLine("     <h3>Всего отправлено сообщений : " + EmailSender.Instance.TotalDelivered + "</h3>");
            sb.AppendLine("     <h3>В чёрном списке : " + EmailSender.Instance.FailReceiversCount + " адресов</h3>");

            sb.AppendLine("     <a style=\"color: #BF0036\" href=\".?clearErrors=1\">Очистить список ошибок</a></br>");
            sb.AppendLine("     <a style=\"color: #BF0036\" href=\".?clearFailReceivers=1\">Очистить список заблокированных адресов</a></br>");
            sb.AppendLine("     <a href=\".?options=1\"  target=\"_newtab\" \">Настройки отправки</a></br>");
            sb.AppendLine("     <a href=\".?testMessage=1\"  target=\"_newtab\" \">Тестовое сообщение</a></br></br>");

            RenderDeliveryStatus(sb);


            sb.AppendLine("     <table border='1' width='100%'>");
            sb.AppendLine("     <tr>");
            sb.AppendLine("     <th colspan=\"5\" style=\"background: #ccc; color:blue; cursor:pointer; \" onclick=\"$('#failListTableBody').toggle(0);\">Список ошибок</th>");
            sb.AppendLine("     </tr>");

            sb.AppendLine("     <tr>");
            sb.AppendLine("     <th width='250px'>Дата</th>");
            sb.AppendLine("     <th width='250px'>Адрес</th>");
            sb.AppendLine("     <th width='250px'>Время отправки</th>");
            sb.AppendLine("     <th>Ошибка</th>");
            sb.AppendLine("     <th width='150px'>Важность</th>");
            sb.AppendLine("     </tr>");

            sb.AppendLine("     <tbody id=\"failListTableBody\">");

            var getErrorListResult = EmailSender.Instance.GetErrorList();
            if (getErrorListResult == null)
            {
                sb.AppendLine("     <tr>");
                sb.AppendLine("     <td></td><td>Не удалось получить список ошибок</td><td></td><td></td><td></td>");
                sb.AppendLine("     </tr>");
            }
            else
            {
                foreach (var errors in getErrorListResult)
                {
                    sb.AppendLine("     <tr>");
                    sb.AppendLine(string.Format("<td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td>",
                                                errors.DateException.ToString(),
                                                errors.Receiver,
                                                errors.TimeSpan,
                                                errors.ReasonMessage, 
                                                errors.Urgency));
                    sb.AppendLine("     </tr>");
                }
            }
            sb.AppendLine("     </tbody>");
            sb.AppendLine("     </table>");
        }

        private static void RenderDeliveryStatus(StringBuilder sb)
        {
            sb.AppendLine("<br/>");

            var deliveringNumber = EmailSender.Instance.currentNomberMessagesInCurrentMail;
            var msgToDeliver = EmailSender.Instance.countMessagesInCurrentMail;
            var percentDelivered = msgToDeliver == 0 ? 0 : 100.0 * deliveringNumber / msgToDeliver;
            const int barAsteriksCount = 34;
            var barFilled = (int) Math.Round(barAsteriksCount*percentDelivered/100.0);
            var barEmpty = barAsteriksCount - barFilled;

            sb.AppendLine("<p>");
            sb.AppendLine("<span>Состояние отправки текущего сообщения : </span>  <span style=\"color:Black;margin:0;padding:0;\">" +
                string.Join("", new int[barFilled].Select(i => "*")) + "</span>" +
                "<span style=\"color:Silver;margin:0;padding:0;\">" +
                string.Join("", new int[barEmpty].Select(i => "o")) + "</span> " + deliveringNumber + " из " 
                + msgToDeliver + " (" + EmailSender.Instance.countMessagesInAllMail + ")");

            sb.AppendLine("<br/>");
            sb.AppendLine("<span>Текущий адрес отправки : " + EmailSender.Instance.currentReceiver + "</span>");
            sb.AppendLine("</p><br/>");

            

            sb.AppendLine("<br/>");
        }

        public override string ServiceName
        {
            get { return SrvName; }
        }

        private static string GetScripts()
        {
            return "$(document).ready(function () {$('#failListTableBody').toggle(0);});";
        }
    }
}

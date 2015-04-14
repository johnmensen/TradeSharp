using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using TradeSharp.Util;

namespace TradeSharp.WatchService.BL
{
    static class Reporter
    {
        private readonly static int startDayOff, startHourOff;
        private static int endDayOff, endHourOff;

        static Reporter()
        {
            startDayOff = int.Parse(AppConfig.GetStringParam("DayOff.StartDay", "6"));
            startHourOff = int.Parse(AppConfig.GetStringParam("DayOff.StartHour", "0"));
            endDayOff = int.Parse(AppConfig.GetStringParam("DayOff.EndDay", "1"));
            endHourOff = int.Parse(AppConfig.GetStringParam("DayOff.EndHour", "5"));
        }

        /// <summary>
        /// если выходной - не отправлять отчет
        /// </summary>        
        private static bool IsDayOff()
        {
            DateTime nowTime = DateTime.Now;
            var startWeekDay = nowTime.AddDays(-(int)nowTime.DayOfWeek).Date;
            var startDateOff = startWeekDay.AddDays(startDayOff).AddHours(
                startHourOff);
            var endDateOff = startWeekDay.AddDays(endDayOff).AddHours(
                endHourOff);

            if (startDayOff > endDayOff)
            {
                return nowTime <= endDateOff || nowTime >= startDateOff;
            }

            return nowTime >= startDateOff || nowTime <= endDateOff;
        }

        private static int minutesBetweenReportsOnSameError;
        private static int MinutesBetweenReportsOnSameError
        {
            get
            {
                if (minutesBetweenReportsOnSameError == 0)
                    minutesBetweenReportsOnSameError =
                        int.Parse(AppConfig.GetStringParam("MinutesBetweenReportsOnSameError", "60"));
                return minutesBetweenReportsOnSameError;
            }
        }

        public static void SendReports(ServiceStateUnit unit)
        {


            // проверка на дублирование отчета
            if (unit.HasBeenReported())
            {
                var minutesPassed = unit.LastReportTime == new DateTime()
                    ? int.MaxValue
                    : (int)(DateTime.Now - unit.LastReportTime).TotalMinutes;
                if (minutesPassed < MinutesBetweenReportsOnSameError) return;
            }

            // проверка на день недели и время суток (выходной)
            if (IsDayOff()) return;

            SendReportEmail(unit);
            if (WatchWebServer.Instance.SendErrorMessage) SendReportSMS(unit);
        }

        private static void SendReportEmail(ServiceStateUnit unit)
        {
            var from = AppConfig.GetStringParam("SMTP.MailFrom", "mailer@forexsignal.ru");
            var recps = AppConfig.GetStringParam("SMTP.Recepients", "xander77@rambler.ru");
            var body = string.Format("Состояние сервиса: {0}. Код ошибки: {1} ({2})",
                                     unit.LastServiceState.State,
                                     unit.LastServiceState.LastError,
                                     unit.LastServiceState.LastErrorOccured);
            var msg = new MailMessage(from, recps, string.Format("Ошибка сервиса {0}", unit.Name), body) { IsBodyHtml = false };
            //msg.BodyEncoding = Encoding.Unicode;
            try
            {
                var client = new SmtpClient();
                client.Send(msg);
                unit.UpdateLastReportedData();
                Logger.InfoFormat(CultureProvider.Common.ToString(), "Письмо ({0}) отправлено", body);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка доставки сообщения", ex);
            }
        }

        private static void SendReportSMS(ServiceStateUnit unit)
        {
            var body = string.Format("Error in service {0}", unit.Name);
            SMSNotifier.Instance.SendMessage(body);
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Robot.BL;
using TradeSharp.Util;

namespace TradeSharp.Robot.Robot
{
// ReSharper disable LocalizableElement
    [DisplayName("Контроль проседания")]
    public class DrawDownStopoutChecker : BaseRobot
    {
        public enum StateCode { OK = 0, Warning, Stopout }

        [PropertyXMLTag("HighWaterMark")]
        [DisplayName("HWM")]
        [Category("Основные")]
        [Description("High Water Mark, от нее отсчитывается DrawDown")]
        public decimal HighWaterMark { get; set; }

        private decimal percentYellow = 7.5M;
        [PropertyXMLTag("PercentYellow")]
        [DisplayName("% желтый")]
        [Category("Основные")]
        [Description("% просадки - уведомление")]
        public decimal PercentYellow
        {
            get { return percentYellow; }
            set { percentYellow = value; }
        }

        private decimal percentRed = 10;
        [PropertyXMLTag("PercentRed")]
        [DisplayName("% красный (стоп)")]
        [Category("Основные")]
        [Description("% просадки - стопаут")]
        public decimal PercentRed
        {
            get { return percentRed; }
            set { percentRed = value; }
        }

        private int intervalCheckMils = 1000 * 20;
        [PropertyXMLTag("IntervalCheckMils")]
        [DisplayName("Интервал проверки, мс")]
        [Category("Служебные")]
        [Description("Интервал проверки условий, милисекунд")]
        public int IntervalCheckMils
        {
            get { return intervalCheckMils; }
            set { intervalCheckMils = value; }
        }

        private DateTime dealStart = DateTime.Now;
        [PropertyXMLTag("DealStart")]
        [DisplayName("Время послед. сделки")]
        [Category("Служебные")]
        [Description("Закрывать сделки, открытые до указанного времени")]
        public DateTime DealStart
        {
            get { return dealStart; }
            set { dealStart = value; }
        }

        private int minMinutesBetweenWarning = 12*60;
        [PropertyXMLTag("MinMinutesBetweenWarning")]
        [DisplayName("Интервал предупреждений, мин")]
        [Category("Служебные")]
        [Description("Мин. интервал рассылки предупреждений, мин")]
        public int MinMinutesBetweenWarning
        {
            get { return minMinutesBetweenWarning; }
            set { minMinutesBetweenWarning = value; }
        }

        private int minMinutesBetweenStopout = 4 * 60;
        [PropertyXMLTag("MinMinutesBetweenStopout")]
        [DisplayName("Интервал сообщ. стопаута, мин")]
        [Category("Служебные")]
        [Description("Мин. интервал рассылки сообщений о стопауте, мин")]
        public int MinMinutesBetweenStopout
        {
            get { return minMinutesBetweenStopout; }
            set { minMinutesBetweenStopout = value; }
        }

        [PropertyXMLTag("Phones")]
        [DisplayName("Телефоны")]
        [Category("Рассылка")]
        [Description("Номера телефонов (через пробел)")]
        public string Phones { get; set; }

        private string gatewayUrl = "http://service.qtelecom.ru/public/http/";
        [PropertyXMLTag("GatewayUrl")]
        [DisplayName("Шлюз - адрес")]
        [Category("Рассылка")]
        [Description("Адрес (URL) SMS-шлюза")]
        public string GatewayUrl
        {
            get { return gatewayUrl; }
            set { gatewayUrl = value; }
        }

        private string gatewayUser = "10736";
        [PropertyXMLTag("GatewayUser")]
        [DisplayName("Шлюз - польз.")]
        [Category("Рассылка")]
        [Description("Логин пользователя SMS-шлюза")]
        public string GatewayUser
        {
            get { return gatewayUser; }
            set { gatewayUser = value; }
        }

        private string gatewayPwrd = "52415520";
        [PropertyXMLTag("GatewayPassword")]
        [DisplayName("Шлюз - пароль")]
        [Category("Рассылка")]
        [Description("Пароль пользователя SMS-шлюза")]
        public string GatewayPassword
        {
            get { return gatewayPwrd; }
            set { gatewayPwrd = value; }
        }

        private bool deliveryEnabled = true;
        [PropertyXMLTag("DeliveryEnabled")]
        [DisplayName("Рассылка разрешена")]
        [Category("Рассылка")]
        [Description("Рассылка разрешена (True) или запрещена (False)")]
        public bool DeliveryEnabled
        {
            get { return deliveryEnabled; }
            set { deliveryEnabled = value; }
        }

        private bool stopoutEnabled = true;
        [PropertyXMLTag("StopoutEnabled")]
        [DisplayName("Стопаут разрешен")]
        [Category("Основные")]
        [Description("Стопаут разрешен (True) или запрещен (False)")]
        public bool StopoutEnabled
        {
            get { return stopoutEnabled; }
            set { stopoutEnabled = value; }
        }

        private Encoding encodingSms = Encoding.UTF8;
        [PropertyXMLTag("EncodingSms")]
        [DisplayName("Кодировка SMS")]
        [Category("Рассылка")]
        [Description("Кодировка отправляемых SMS-сообщений")]
        public string EncodingSms
        {
            get { return EncodingFriendlyName.GetEncodingName(encodingSms); }
            set { encodingSms = EncodingFriendlyName.GetEncodingByName(value, Encoding.UTF8); }
        }

        private decimal depoWarning;
        private decimal depoStopout;
        private DateTime? lastCheckTime;
        private DateTime? lastMessageTime;
        private StateCode lastState;
        private MarketOrderSafeStorage orderStorage;
        
        public override BaseRobot MakeCopy()
        {
            var bot = new DrawDownStopoutChecker
                          {
                              HighWaterMark = HighWaterMark,
                              PercentYellow = PercentYellow,
                              PercentRed = PercentRed,
                              IntervalCheckMils = IntervalCheckMils,
                              DealStart = DealStart,
                              Phones = Phones,
                              GatewayUrl = GatewayUrl,
                              GatewayUser = GatewayUser,
                              GatewayPassword = GatewayPassword,
                              DeliveryEnabled = DeliveryEnabled,
                              StopoutEnabled = StopoutEnabled,
                              MinMinutesBetweenWarning = MinMinutesBetweenWarning,
                              MinMinutesBetweenStopout = MinMinutesBetweenStopout,
                              EncodingSms = EncodingSms
                          };
            CopyBaseSettings(bot);
            return bot;
        }

        public override void Initialize(BacktestServerProxy.RobotContext grobotContext, CurrentProtectedContext protectedContext)
        {
            base.Initialize(grobotContext, protectedContext);

            depoWarning = HighWaterMark*(1 - PercentYellow/100M);
            depoStopout = HighWaterMark*(1 - PercentRed/100M);
            lastCheckTime = null;
            lastState = StateCode.OK;
            orderStorage = new MarketOrderSafeStorage(1000, 10 * 1000, grobotContext);            
        }

        public override Dictionary<string, DateTime> GetRequiredSymbolStartupQuotes(DateTime startTrade)
        {
            if (Graphics.Count == 0) return null;
            return new Dictionary<string, DateTime> { { Graphics[0].a, startTrade } };
        }

        public override List<string> OnQuotesReceived(string[] names, CandleDataBidAsk[] quotes, bool isHistoryStartOff)
        {
            var evtList = new List<string>();
            var deltaMils = lastCheckTime.HasValue
                                ? (DateTime.Now - lastCheckTime.Value).TotalMilliseconds
                                : int.MaxValue;
            if (deltaMils >= IntervalCheckMils)
            {
                lastState = CheckDeposit();
                lastCheckTime = DateTime.Now;
            }
            if (lastState == StateCode.OK) return evtList;
            // осуществить стопаут?
            var countStopout = new Cortege2<int, int>(0, 0);
            if (lastState == StateCode.Stopout) countStopout = PerformStopout();                        
            // разослать сообщение
            PerformDelivery(countStopout);
            if (countStopout.a > 0)
            {
                Logger.InfoFormat("DrawDownStopoutChecker: закрыто {0} сделок из {1}",
                    countStopout.a, countStopout.a + countStopout.b);
            }
            return evtList;
        }

        private StateCode CheckDeposit()
        {
            var equity = robotContext.AccountInfo.Equity;
            if (equity < depoStopout) return StateCode.Stopout;
            if (equity < depoWarning) return StateCode.Warning;
            return StateCode.OK;
        }

        /// <summary>
        /// закрывает сделки, открытые до DealStart
        /// возвращает количество закрытых сделок
        /// </summary>        
        private Cortege2<int, int> PerformStopout()
        {
            if (!StopoutEnabled) return new Cortege2<int, int>(0, 0);
            var orders = orderStorage.CurrentOrders;
            if (orders.Count == 0) return new Cortege2<int, int>(0, 0);
            var ordersToClose = orders.Where(o => o.TimeEnter <= DealStart);
            int countOk = 0, countError = 0;
            foreach (var order in ordersToClose)
            {
                var status = robotContext.SendCloseRequest(protectedContext.MakeProtectedContext(),
                    robotContext.AccountInfo.ID, order.ID, PositionExitReason.ClosedByRobot);
                if (status == RequestStatus.OK)
                {
                    Logger.InfoFormat("DrawDownStopoutChecker: счет {0} - успешно отправлен запрос на закрытие сделки {1}", 
                        robotContext.AccountInfo.ID, order.ID);
                    countOk++;
                }
                else
                {
                    Logger.InfoFormat("DrawDownStopoutChecker: счет {0} - запрос на закрытие сделки {1} отправлен с ошибкой ({2})",
                        robotContext.AccountInfo.ID, order.ID, status);
                    countError++;
                }
            }
            return new Cortege2<int, int>(countOk, countError);
        }
    
        private void PerformDelivery(Cortege2<int, int> countClosed)
        {
            if (!DeliveryEnabled) return;
            // проверка на зафлаживание 
            var minsSinceMessage = !lastMessageTime.HasValue
                                       ? int.MaxValue
                                       : (DateTime.Now - lastMessageTime.Value).TotalMinutes;
            if (lastState == StateCode.Warning && minsSinceMessage < MinMinutesBetweenWarning)
                return;
            if (lastState == StateCode.Stopout && minsSinceMessage < MinMinutesBetweenStopout)
                return;
            lastMessageTime = DateTime.Now;
            // сформировать текст сообщения
            var equity = robotContext.AccountInfo.Equity;
            var message =
                lastState == StateCode.Warning
                    ? string.Format(
                        "Account {0}: equity is {1}, HWM is {2}. Loss is {3:f2}% - stopout is to be at {4:f2}%",
                        robotContext.AccountInfo.ID, (int) equity, HighWaterMark,
                        100*(HighWaterMark - equity)/HighWaterMark, PercentRed)
                    : string.Format(
                        "Account {0}: equity is {1}, HWM is {2}. Loss is {3:f2}% - stopout performed {4} closed of total {5}",
                        robotContext.AccountInfo.ID, (int) equity, HighWaterMark,
                        100*(HighWaterMark - equity)/HighWaterMark,
                        countClosed.a, countClosed.a + countClosed.b);
            // разослать
            SendSms(message);
        }

        private void SendSms(string text)
        {
            var phones = Phones.Split(new[] {' ', ','}, StringSplitOptions.RemoveEmptyEntries);
            if (phones.Length == 0) return;
        
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(GatewayUrl);
            var postData = string.Format("user={0}&pass={1}&action=post_sms&message={2}&target={3}",
                GatewayUser, GatewayPassword, HttpUtility.UrlEncode(text),
                string.Join(",", phones));

            //postData = HttpUtility.UrlEncode(postData);
            var data = encodingSms.GetBytes(postData);

            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            httpWebRequest.ContentLength = data.Length;
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            try
            {
                var newStream = httpWebRequest.GetRequestStream();
                newStream.Write(data, 0, data.Length);
                newStream.Close();
                var response = (HttpWebResponse)httpWebRequest.GetResponse();
                var status = response.StatusDescription;
                Logger.InfoFormat("SMS отправлено: \"{0}\". Статус: {1}", text, status);

                var respData = response.GetResponseStream();
                using (var sr = new StreamReader(respData, Encoding.UTF8))
                {
                    var respStr = sr.ReadToEnd();
                    Logger.InfoFormat(respStr);
                }                
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Ошибка при отправке SMS"), ex);
            }
        }
    }
    // ReSharper restore LocalizableElement
}

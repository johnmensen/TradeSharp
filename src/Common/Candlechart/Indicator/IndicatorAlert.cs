using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net;
using System.Linq;
using System.Text;
using System.Web;
using Candlechart.Chart;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    [LocalizedDisplayName("TitleAlert")]
    [LocalizedCategory("TitleServiceIndicators")]
    [TypeConverter(typeof(PropertySorter))]
    public class IndicatorAlert : BaseChartIndicator, IChartIndicator
    {
        [Browsable(false)]
        public override string Name { get { return Localizer.GetString("TitleAlert"); } }

        public override BaseChartIndicator Copy()
        {
            var al = new IndicatorAlert();
            Copy(al);
            return al;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var al = (IndicatorAlert)indi;
            CopyBaseSettings(al);
            al.clLine = clLine;
            al.lineWidth = lineWidth;
            al.lineStyle = lineStyle;
            al.AlertLevels = AlertLevels;
            al.SendSmsFlag = SendSmsFlag;
            al.SendEmailFlag = SendEmailFlag;
            al.SendSmsPhones = SendSmsPhones;
            al.SendEmailList = SendEmailList;
        }

        [LocalizedDisplayName("TitleCreateDrawingPanel")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageCreateDrawingPanelDescription")]
        [Browsable(false)]
        public override bool CreateOwnPanel { get; set; }

        private bool isPanelVisible;
        [LocalizedDisplayName("TitleShowDrawingPanel")]
        [LocalizedCategory("TitleMain")]
        [Browsable(false)]
        public bool IsPanelVisible { get { return isPanelVisible; } set { isPanelVisible = value; } }

        [LocalizedDisplayName("TitleSourceSeries")]
        [LocalizedCategory("TitleMain")]
        [Browsable(false)]
        public virtual string SeriesSourcesDisplay { get; set; }

        private Color clLine = Color.Blue;
        [LocalizedDisplayName("TitleColor")]
        [LocalizedDescription("MessageMAColorDescription")]
        [LocalizedCategory("TitleVisuals")]
        [PropertyXMLTag("Robot.LineColor")]
        [PropertyOrder(1)]
        public Color ClLine
        {
            get { return clLine; }
            set { clLine = value; }
        }

        private DashStyle lineStyle = DashStyle.Solid;
        [LocalizedDisplayName("TitleLineStyle")]
        [LocalizedDescription("MessageLineStyleDescription")]
        [LocalizedCategory("TitleVisuals")]
        [PropertyXMLTag("Robot.LineStyle")]
        [PropertyOrder(2)]
        public DashStyle LineStyle
        {
            get { return lineStyle; }
            set { lineStyle = value; }
        }

        private decimal lineWidth = 1;
        [LocalizedDisplayName("TitleThickness")]
        [LocalizedDescription("MessageThicknessDescription")]
        [LocalizedCategory("TitleVisuals")]
        [PropertyXMLTag("Robot.LineWidts")]
        [PropertyOrder(3)]
        public decimal LineWidth
        {
            get { return lineWidth; }
            set { lineWidth = value; }
        }

        [LocalizedDisplayName("TitleAlertPriceLevels")]
        [LocalizedDescription("MessageAlertPriceLevelsDescription")]
        [LocalizedCategory("TitleMain")]
        [PropertyXMLTag("Robot.AlertLevels")]
        [PropertyOrder(4)]
        public string AlertLevels { get; set; }

        [LocalizedDisplayName("TitleSendSMS")]
        [LocalizedDescription("MessageSendSMSDescription")]
        [LocalizedCategory("TitleNotification")]
        [PropertyXMLTag("Robot.SendSmsFlag")]
        [PropertyOrder(5)]
        public bool SendSmsFlag { get; set; }

        [LocalizedDisplayName("TitlePhonesForSMS")]
        [LocalizedDescription("MessagePhonesForSMSDescription")]
        [LocalizedCategory("TitleNotification")]
        [PropertyXMLTag("Robot.SendSmsPhones")]
        [PropertyOrder(6)]
        public string SendSmsPhones { get; set; }

        [LocalizedDisplayName("TitleSendEmail")]
        [LocalizedDescription("MessageSendEmailDescription")]
        [LocalizedCategory("TitleNotification")]
        [PropertyXMLTag("Robot.SendEmailFlag")]
        [PropertyOrder(7)]
        public bool SendEmailFlag { get; set; }

        [LocalizedDisplayName("TitleEmailAddressesList")]
        [LocalizedDescription("MessageEmailAddressesListDescription")]
        [LocalizedCategory("TitleNotification")]
        [PropertyXMLTag("Robot.SendEmailList")]
        [PropertyOrder(8)]
        public string SendEmailList { get; set; }


        private TrendLineSeries series;

        private List<AlertLevel> alertsList = new List<AlertLevel>();
        
        public IndicatorAlert()
        {
            CreateOwnPanel = false;
            SendEmailFlag = true;
            SendSmsFlag = true;
        }

        public void BuildSeries(ChartControl chart)
        {
            series.data.Clear();
            alertsList.Clear();
            BuildAlertLevels(chart.StockSeries.CurrentPriceString.ToFloatUniformSafe());
            var prices = AlertLevels.ToDecimalArrayUniform();
            foreach (var price in prices)
            {
                // строим линии
                var line = new TrendLine
                {
                    LineStyle = TrendLine.TrendLineStyle.Линия,
                    LineColor = clLine,
                    ShapeAlpha = 192,
                    ShapeFillColor = ClLine
                };
                line.AddPoint(0, (double)price);
                line.AddPoint(owner.StockSeries.DataCount, (double)price);
                series.data.Add(line);
            }
            
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            // инициализируем индикатор
            EntitleIndicator();
            series = new TrendLineSeries(Name);
            SeriesResult = new List<Series.Series> { series };
        }

        public void Remove()
        {
            if (series != null) series.data.Clear();
            alertsList.Clear();
        }

        public void AcceptSettings()
        {
            series.ForeColor = ClLine;
            series.LineWidth = (float)lineWidth;
        }

        private void BuildAlertLevels(float? currPrice)
        {
            alertsList.Clear();
            if (currPrice == null) return;
            var prices = AlertLevels.ToFloatArrayUniform();
            foreach (var price in prices)
            {
                alertsList.Add(price > currPrice
                                   ? new AlertLevel() {Price = price, LevelType = AlertLevelType.FromDown}
                                   : new AlertLevel() {Price = price, LevelType = AlertLevelType.FromUp});
            }
        }

        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (alertsList.Count == 0)
                BuildAlertLevels(updatedCandle.close);
            if (series.data.Count == 0)
            {
                var prices = AlertLevels.ToDecimalArrayUniform();
                foreach (var level in prices)
                {
                    // строим линии
                    var line = new TrendLine
                    {
                        LineStyle = TrendLine.TrendLineStyle.Линия,
                        LineColor = clLine,
                        ShapeAlpha = 192,
                        ShapeFillColor = ClLine
                    };
                    line.AddPoint(0, (double)level);
                    line.AddPoint(owner.StockSeries.DataCount, (double)level);
                    series.data.Add(line);    
                }
                
            }

            // проверяем срабатывание уровней
            for (var i = 0; i < alertsList.Count; i++)
            {
                if (!alertsList[i].CheckLevel(updatedCandle.close)) continue;

                try
                {
                    var wc = new WebClient
                    {
                        Credentials = new NetworkCredential("BrokerSvc@forexinvest.local", "Br0ker$201!")
                    };
                    var url = AppConfig.GetStringParam("Publish.Url", "http://forexinvest.ru:8095");
                    //wc.Credentials = CredentialCache.DefaultCredentials;
                    var serverRequest = WebRequest.Create(url);
                    serverRequest.Credentials = new NetworkCredential("BrokerSvc@forexinvest.local", "Br0ker$201!");
                    var serverResponse = serverRequest.GetResponse();
                    serverResponse.Close();

                    // отправляем теперь команду на публикацию
                    var msg = HttpUtility.UrlEncode(string.Format("Сработал сигнал по {0}: текущая котировка {1}", owner.Symbol, updatedCandle.close), Encoding.GetEncoding(1251));
                    var phones = HttpUtility.UrlEncode(SendSmsPhones, Encoding.GetEncoding(1251));
                    var emails = HttpUtility.UrlEncode(SendEmailList, Encoding.GetEncoding(1251));
                    var qs = new NameValueCollection { { "command", SendCommandType.Alert.ToString() }, { "msg", msg }, {"sms", SendSmsFlag.ToString()}, 
                    { "email", SendEmailFlag.ToString() } , { "phones", phones} , { "emails", emails}};
                    wc.QueryString = qs;
                    var bret = wc.UploadString(url, "POST", string.Empty);
                    wc.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка отправки алерта на сервер:", ex);
                }
                alertsList.RemoveAt(i);
                AlertLevels = string.Join(" ", alertsList.Select(a => a.Price.ToStringUniform()));
                i--;
            }
            
        }

        public string GetHint(int x, int y, double index, double price, int tolerance)
        {
            return string.Empty;
        }

        public List<IChartInteractiveObject> GetObjectsUnderCursor(int screenX, int screenY, int tolerance)
        {
            return new List<IChartInteractiveObject>();
        }
    }

    public enum AlertLevelType
    {
        FromUp = 0,
        FromDown
    } 

    public struct AlertLevel
    {
        public float Price;
        public AlertLevelType LevelType;

        public bool CheckLevel(float price)
        {
            if (LevelType == AlertLevelType.FromUp && Price >= price) return true;
            return LevelType == AlertLevelType.FromDown && Price <= price;
        }
    }
}

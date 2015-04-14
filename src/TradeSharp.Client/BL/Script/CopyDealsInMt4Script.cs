using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using Candlechart;
using Candlechart.ChartMath;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.BL.Script
{
    [DisplayName("Сделки в MT4")]
    public class CopyDealsInMt4Script : TerminalScript
    {
        #region Параметры
        [DisplayName("Суффикс инстр.")]
        [Category("MT4")]
        [Description("Суффикс инструмента в МТ4")]
        [PropertyXMLTag("Mt4TickerSuffix")]
        public string Mt4TickerSuffix { get; set; }

        private string hostMt4 = "127.0.0.1";
        [Category("MT4")]
        [PropertyXMLTag("HostMt4")]
        [DisplayName("Адрес комп. MT4")]
        [Description("Адрес компьютера-терминала МТ4 (127.0.0.1 - локальный адрес)")]
        public string HostMt4
        {
            get { return hostMt4; }
            set { hostMt4 = value; }
        }

        public int portMt4 = 8011;
        [Category("MT4")]
        [PropertyXMLTag("PortMt4")]
        [DisplayName("Порт (MT4)")]
        [Description("Порт, который слушает советник МТ4, на предмет сообщений от робота")]
        public int PortMt4
        {
            get { return portMt4; }
            set { portMt4 = value; }
        }
        #endregion

        private readonly Encoding encoding = Encoding.ASCII;

        public CopyDealsInMt4Script()
        {
            ScriptTarget = TerminalScriptTarget.Тикер;
            ScriptName = "Сделки в MT4";
        }

        public override string ActivateScript(CandleChartControl chart, PointD worldCoords)
        {
            return ActivateScript(chart.Symbol);
        }

        public override string ActivateScript(string ticker)
        {
            var orders = MarketOrdersStorage.Instance.MarketOrders.Where(o => o.Symbol == ticker).ToList();
            if (orders.Count == 0) return string.Empty;

            if (MessageBox.Show("Будет отправлено " + orders.Count + " команд. Продолжить?",
                                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return string.Empty;

            if (!string.IsNullOrEmpty(Mt4TickerSuffix))
                ticker = ticker + Mt4TickerSuffix;

            var countOk = 0;
            foreach (var order in orders)
            {
                var volumeMt4 = order.Volume;
                var cmd = string.Format("{0}_{1}_{2}_{3}",
                                        order.Side > 0 ? "BUY" : "SELL",
                                        volumeMt4, ticker, order.ID);
                try
                {
                    var data = encoding.GetBytes(cmd);
                    new UdpClient().Send(data, data.Length, HostMt4, PortMt4);
                    countOk++;
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка отправки сообщения в МТ4", ex);
                }
            }

            return "Отправлено " + countOk + " из " + orders.Count + " команд";
        }

        public override string ActivateScript(bool byTrigger)
        {
            throw new Exception("Неверный тип вызова скрипта \"CopyDealsInMt4Script\"");
        }
    }
}

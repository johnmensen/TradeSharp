using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Candlechart;
using Candlechart.ChartMath;
using Entity;
using TradeSharp.Util;

namespace DemoPlugin.Dialog
{
    public partial class CalculatorForm : Form
    {
        private readonly CandleChartControl chart;

        private readonly PointD worldCoords;

        private enum OptionType
        {
            Classic = 0, Touch
        }

        public CalculatorForm()
        {
            InitializeComponent();
        }

        public CalculatorForm(CandleChartControl chart, PointD worldCoords) : this()
        {
            this.chart = chart;
            this.worldCoords = worldCoords;
            var time = chart.chart.StockSeries.GetCandleOpenTimeByIndex((int)worldCoords.X);
            var price = worldCoords.Y;
            dpTimeCalc.Value = time;
            dpTimeExpires.Value = time.AddMinutes(chart.Timeframe.TotalMinutes * 200);
            tbPriceAtTime.Text = price.ToStringUniformPriceFormat(true);
            tbStrikePrice.Text = "0.0";
        }

        private void btnCalcPremium_Click_1(object sender, System.EventArgs e)
        {
            var optTypeString = (string) cbOptionType.SelectedItem;
            var optionSide = optTypeString.StartsWith("PUT") ? -1 : 1;
            var optionType = optTypeString.EndsWith("TOUCH") ? OptionType.Touch : OptionType.Classic;
            var strike = tbStrikePrice.Text.ToFloatUniform();
            var closeTime = dpTimeExpires.Value;

            var candles = AtomCandleStorage.Instance.GetAllMinuteCandles(chart.Symbol);
            var deltas = candles.Select(c => c.close - c.open).OrderBy(d => d).ToList();
            

            foreach (var candle in candles)
            {
                if (candle.timeOpen < dpTimeCalc.Value) continue;
                //if (candle.timeClose > )
                if (optionType == OptionType.Touch)
                {
                    var touched = optionSide > 0 ? (candle.high > strike) : (candle.low < strike);
                    if (touched)
                    {
                        LoggMessageSafeFormat("Страйк достигнут ({0})",
                            optionSide > 0
                                ? candle.high.ToStringUniformPriceFormat()
                                : candle.low.ToStringUniformPriceFormat());
                        break;
                    }
                }
                
                
            }
        }

        private void LoggMessageSafeFormat(string format, params object[] ptrs)
        {
            if (ptrs.Length == 0) LoggMessageSafe(format);
            else LoggMessageSafe(string.Format(format, ptrs));
        }

        private void LoggMessageSafe(string msg)
        {
            if (InvokeRequired)
                Invoke(new Action<string>(LoggMessageUnsafe), msg);
            else
                tbResult.AppendText(msg);
        }

        private void LoggMessageUnsafe(string msg)
        {
            tbResult.AppendText(msg);
        }
    }
}

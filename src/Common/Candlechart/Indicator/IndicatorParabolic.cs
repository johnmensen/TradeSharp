using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using Candlechart.Chart;
using Candlechart.Controls;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    [DisplayName("Parabolic SAR")]
    [LocalizedCategory("TitleTrending")]
    [TypeConverter(typeof(PropertySorter))]
    // ReSharper disable InconsistentNaming
    public class IndicatorParabolic : BaseChartIndicator, IChartIndicator
    {
        #region Основные настройки

        public override BaseChartIndicator Copy()
        {
            var macd = new IndicatorParabolic();
            Copy(macd);
            return macd;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var parabolic = (IndicatorParabolic)indi;
            CopyBaseSettings(parabolic);
            parabolic.step = step;
            parabolic.stepDelta = stepDelta;
            parabolic.LineStyle = LineStyle;
            parabolic.LineColor = LineColor;
        }

        [Browsable(false)]
        public override string Name { get { return "Parabolic SAR"; } }

        private float step = 0.02f;
        [LocalizedDisplayName("TitleStepInPSAR")]
        [LocalizedCategory("TitleMain")]
        [Description("Шаг индикатора, ускорение")]
        public float Step
        {
            get { return step; }
            set { step = value; }
        }

        private float stepDelta = 0.02f;
        [LocalizedDisplayName("TitleDeltaInPSAR")]
        [LocalizedCategory("TitleMain")]
        [Description("Приращение шага индикатора")]
        public float StepDelta
        {
            get { return stepDelta; }
            set { stepDelta = value; }
        }

        [LocalizedDisplayName("TitleCreateDrawingPanel")]
        [LocalizedCategory("TitleMain")]
        [Description("Создавать свою панель отрисовки")]
        public override bool CreateOwnPanel { get; set; }

        #endregion

        #region Визуальные настройки

        private readonly static Color[] lineColors = new[] { Color.Red, Color.Green, Color.Blue, Color.DarkViolet, Color.DarkOrange };

        private static int curColorIndex;

        [LocalizedDisplayName("TitleLineColor")]
        [LocalizedCategory("TitleVisuals")]
        [Description("Цвет линии")]
        public Color LineColor { get; set; }

        [LocalizedDisplayName("TitleLineType")]
        [LocalizedCategory("TitleVisuals")]
        [Description("TitleLineType")]
        [Editor(typeof(TrendLineStyleUIEditor), typeof(UITypeEditor))]
        public TrendLine.TrendLineStyle LineStyle { get; set; }

        #endregion

        private TrendLineSeries mainLine;
        
        public IndicatorParabolic()
        {
            LineColor = lineColors[curColorIndex++];
            if (curColorIndex >= lineColors.Length) curColorIndex = 0;
        }

        public void BuildSeries(ChartControl chart)
        {
            mainLine.data.Clear();

            var candles = chart.StockSeries.Data.Candles;
            if (candles == null || candles.Count < 5) return;

            var sar = candles[0].high;
            float h = candles[0].high, l = candles[0].low;
            var trend = -1;
            var trendLength = 0;

            AddLinePoint(0, sar);

            for (var i = 1; i < candles.Count; i++)
            {
                var candle = candles[i];
                if (trend < 0 && candle.high > sar)
                {
                    // разворот
                    trend = 1;
                    sar = candle.low;
                    l = candle.low;
                    h = candle.high;
                    trendLength = 0;
                } else if (trend > 0 && candle.low < sar)
                {
                    // разворот
                    trend = -1;
                    sar = candle.high;
                    l = candle.low;
                    h = candle.high;
                    trendLength = 0;
                }
                else
                {
                    var resultStep = stepDelta * trendLength + step;
                    trendLength++;
                    sar = sar + resultStep * (trend > 0 ? h - sar : l - sar);
                    if (h < candle.high) h = candle.high;
                    if (l > candle.low) l = candle.low;
                }

                AddLinePoint(i, sar);
            }
        }

        private void AddLinePoint(int index, float price)
        {
            var line = new TrendLine
                {
                    LineColor = LineColor,
                    LineStyle = LineStyle
                };
            line.AddPoint(index - 0.4, price);
            line.AddPoint(index + 0.4, price);
            mainLine.data.Add(line);
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            // инициализируем индикатор
            mainLine = new TrendLineSeries(Name);
            
            SeriesResult = new List<Series.Series> { mainLine };
            EntitleIndicator();
        }

        public void Remove()
        {
        }

        public void AcceptSettings()
        {
            foreach (var line in mainLine.data)
            {
                line.LineColor = LineColor;
                line.LineStyle = LineStyle;
            }
        }

        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (newCandles.Count > 0)
                BuildSeries(owner);
        }

        public string GetHint(int x, int y, double index, double price, int tolerance)
        {
            return string.Empty;
        }

        public List<IChartInteractiveObject> GetObjectsUnderCursor(int screenX, int screenY, int tolerance)
        {
            return new List<IChartInteractiveObject>();
        }

        public override string GenerateNameBySettings()
        {
            string colorStr;
            GetKnownColor(LineColor.ToArgb(), out colorStr);
            return string.Format("Parabolic[{0}]", colorStr);
        }
    }
    // ReSharper restore InconsistentNaming
}

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Candlechart.Chart;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    [DisplayName("RSI")]
    [LocalizedCategory("TitleOscillator")]
    [TypeConverter(typeof(PropertySorter))]
// ReSharper disable InconsistentNaming
    class IndicatorRSI : BaseChartIndicator, IChartIndicator
    {
        #region Основные настройки

        public override BaseChartIndicator Copy()
        {
            var rsi = new IndicatorRSI();
            Copy(rsi);
            return rsi;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var rsi = (IndicatorRSI) indi;
            CopyBaseSettings(rsi);
            rsi.Period = Period;
            rsi.MarginPercent = MarginPercent;
            rsi.LineColor = LineColor;
            rsi.seriesRSI = seriesRSI;
            rsi.seriesBound = seriesBound;
        }

        [Browsable(false)]
        public override string Name { get { return "RSI"; } }

        private int period = 14;
        [LocalizedDisplayName("TitlePeriod")]
        [LocalizedCategory("TitleMain")]
        [Description("Период индикатора, свечей")]
        public int Period
        {
            get { return period; }
            set { period = value; }
        }

        private int marginPercent = 20;
        [LocalizedDisplayName("TitleBordersInPercent")]
        [LocalizedCategory("TitleMain")]
        [Description("Границы перекупленности перепроданности, %")]
        public int MarginPercent
        {
            get { return marginPercent; }
            set { marginPercent = value; }
        }

        #endregion

        #region Визуальные настройки

        private readonly static Color[] lineColors = new [] { Color.Red, Color.Green, Color.Blue, Color.DarkViolet, Color.DarkOrange };
        private static int curColorIndex;
        
        [LocalizedDisplayName("TitleLineColor")]
        [LocalizedCategory("TitleVisuals")]
        [Description("Цвет линии")]
        public Color LineColor { get; set; }

        #endregion

        private LineSeries seriesRSI;

        private RegionSeries seriesBound;

        public IndicatorRSI()
        {
            LineColor = lineColors[curColorIndex++];
            if (curColorIndex >= lineColors.Length) curColorIndex = 0;
        }

        public void BuildSeries(ChartControl chart)
        {
            seriesRSI.Data.Clear();
            seriesBound.data.Clear();
            // границы
            var reg = new BarRegion { Color = Color.Black, UpperBound = 100 - marginPercent, LowerBound = marginPercent };
            seriesBound.data.Add(reg);
            // RSI
            for (var i = 0; i < Period; i++)
                seriesRSI.Data.Add(50);

            var dataCount = GetSourceDataCount();
            for (var i = Period; i < dataCount; i++)
            {
                double sumU = 0, sumD = 0;
                for (var j = 0; j < Period; j++)
                {
                    var delta = GetSourcePrice(i - j)
                                - GetSourcePrice(i - j - 1);
                    if (delta < 0) sumD -= delta;
                    else sumU += delta;
                }
                double RSI;
                if (sumU == sumD && sumU == 0) RSI = 50;
                else if (sumD == 0) RSI = 100;
                else
                    RSI = 100 - 100.0 / (1 + sumU / sumD);
                seriesRSI.Data.Add(RSI);
            }
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            // инициализируем индикатор
            seriesRSI = new LineSeries(Name)
            {
                LineColor = LineColor,
                Transparent = true,
                ShiftX = 1,
                DrawShadow = DrawShadow,
                ShadowWidth = ShadowWidth,
                ShadowColor = Color.Gray,
                ShadowAlpha = ShadowAlpha,                
            };
            seriesBound = new RegionSeries(Localizer.GetString("TitleRSIBorders")) { DrawAsFrame = true };
            SeriesResult = new List<Series.Series> { seriesRSI, seriesBound };
            EntitleIndicator();
        }

        public void Remove()
        {            
        }

        public void AcceptSettings()
        {
            seriesRSI.LineColor = LineColor;
        }

        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            BuildSeries(owner);
        }

        public string GetHint(int x, int y, double index, double price, int tolerance)
        {
            return string.Format("{0} = {1:f5}", UniqueName, seriesRSI.Data[(int)index]);
        }

        public List<IChartInteractiveObject> GetObjectsUnderCursor(int screenX, int screenY, int tolerance)
        {
            return new List<IChartInteractiveObject>();
        }

        private double GetSourcePrice(int index)
        {
            return GetSourcePrice(index, 0);
        }

        public override string GenerateNameBySettings()
        {
            string colorStr;
            GetKnownColor(LineColor.ToArgb(), out colorStr);
            return string.Format("RSI[{0}]{1}",
                                 Period, string.IsNullOrEmpty(colorStr) ? "" : string.Format("({0})", colorStr));
        }
    }
    // ReSharper restore InconsistentNaming
}

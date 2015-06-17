using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Candlechart.Chart;
using Candlechart.Core;
using Candlechart.Series;
using Candlechart.Theme;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    [DisplayName("Momentum")]
    [LocalizedCategory("TitleOscillator")]
    [TypeConverter(typeof(PropertySorter))]
    public class IndicatorMomentum : BaseChartIndicator, IChartIndicator
    {
        #region members
        [Browsable(false)]
        public override string Name { get { return "Momentum"; } }

        private int period = 14;
        [LocalizedDisplayName("TitlePeriod")]
        [LocalizedDescription("MessageCCIPeriodDescription")]
        [LocalizedCategory("TitleMain")]
        [PropertyOrder(1)]
        public int Period
        {
            get { return period; }
            set { period = value; }
        }

        private Color colorLine = Color.DarkOliveGreen;
        [LocalizedDisplayName("TitleColor")]
        [LocalizedDescription("MessageCurveColorDescription")]
        [LocalizedCategory("TitleVisuals")]
        [PropertyOrder(50)]
        public Color ColorLine
        {
            get { return colorLine; }
            set { colorLine = value; }
        }

        private Color colorBorder = Color.DimGray;
        [LocalizedDisplayName("TitleBorder")]
        [LocalizedDescription("MessageBorderColorDescription")]
        [LocalizedCategory("TitleVisuals")]
        [PropertyOrder(51)]
        public Color ColorBorder
        {
            get { return colorBorder; }
            set { colorBorder = value; }
        }

        private LineSeries series;
        private PartSeries seriesBounds;

        #endregion

        public override BaseChartIndicator Copy()
        {
            var mm = new IndicatorMomentum();
            Copy(mm);
            return mm;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var cci = (IndicatorMomentum)indi;
            CopyBaseSettings(cci);
            cci.Period = Period;
            cci.ColorLine = ColorLine;
            cci.ColorBorder = ColorBorder;
            cci.series = series;
            cci.seriesBounds = seriesBounds;
        }

        public void BuildSeries(ChartControl chart)
        {
            series.Data.Clear();

            if (SeriesSources.Count == 0 || !(SeriesSources[0] is IPriceQuerySeries)) return;
            var sources = (IPriceQuerySeries)SeriesSources[0];
            var candles = SeriesSources[0] is StockSeries ? ((StockSeries)SeriesSources[0]).Data.Candles : null;

            var count = SeriesSources[0].DataCount;
            if (count == 0 || Period == 0 || count <= (2 * Period)) return;

            // линия 100%
            seriesBounds.parts.Add(new List<PartSeriesPoint>
                                       {
                                           new PartSeriesPoint(1, 100M),
                                           new PartSeriesPoint(count, 100M)
                                       });

            // пустое начало графика
            for (var i = 0; i < Period; i++)
                series.Data.Add(0);

            // посчитать индикатор
            for (var i = Period; i < count; i++)
            {
                var nowValue = candles == null
                    ? (sources.GetPrice(i) ?? 0)
                    : candles[i].close;

                var oldValue = candles == null
                    ? (sources.GetPrice(i - Period) ?? 0)
                    : candles[i - Period].close;

                var mm = oldValue == 0 ? 100 : 100 * nowValue / oldValue;
                series.Data.Add(mm);
            }
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            series = new LineSeries("Momentum")
            {
                LineColor = Color.Teal,
                ShiftX = 1
            };
            seriesBounds = new PartSeries("Momentum Pivot") { LineStyle = LineStyle.Dot };
            SeriesResult = new List<Series.Series> { series, seriesBounds };
            EntitleIndicator();
        }

        public void Remove()
        {
        }

        public void AcceptSettings()
        {
            series.LineColor = colorLine;
            series.Transparent = true;
            seriesBounds.LineColor = ColorBorder;
            if (DrawPane != null && DrawPane != owner.StockPane)
                DrawPane.Title = string.Format("{0} [{1}]", UniqueName, Period);
        }

        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (newCandles != null)
                if (newCandles.Count > 0) BuildSeries(owner);
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
            GetKnownColor(ColorLine.ToArgb(), out colorStr);
            return string.Format("Momentum[{0}]{1}",
                                 Period, string.IsNullOrEmpty(colorStr) ? "" : string.Format("({0})", colorStr));
        }
    }
}

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Candlechart.Chart;
using Candlechart.Core;
using Candlechart.Series;
using Candlechart.Theme;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    [LocalizedDisplayName("TitleStochastic")]
    [LocalizedCategory("TitleOscillator")]
    [TypeConverter(typeof(PropertySorter))]
    public class IndicatorStochastic : BaseChartIndicator, IChartIndicator
    {
        #region members

        [Browsable(false)]
        public override string Name { get { return Localizer.GetString("TitleStochastic"); } }

        private int period = 20;
        [LocalizedDisplayName("TitlePeriod")]
        [Description("Период стохастика")]
        [LocalizedCategory("TitleMain")]
        [PropertyOrder(1)]
        public int Period
        {
            get { return period; }
            set { period = value; }
        }

        private int periodMA = 5;
        [LocalizedDisplayName("TitleSmoothStochasticPeriodShort")]
        [Description("Период сглаживания стохастика")]
        [LocalizedCategory("TitleMain")]
        [PropertyOrder(2)]
        public int PeriodMA
        {
            get { return periodMA; }
            set { periodMA = value; }
        }

        private Color colorLineK = Color.DarkGreen;
        [LocalizedDisplayName("TitleStochasticCurveColorShort")]
        [Description("Цвет кривой стохастика")]
        [LocalizedCategory("TitleVisuals")]
        [PropertyOrder(50)]
        public Color ColorLineK
        {
            get { return colorLineK; }
            set { colorLineK = value; }
        }

        private Color colorLineD = Color.Red;
        [LocalizedDisplayName("TitleSmoothStochasticCurveColorShort")]
        [Description("Цвет сглаженной кривой")]
        [LocalizedCategory("TitleVisuals")]
        [PropertyOrder(51)]
        public Color ColorLineD
        {
            get { return colorLineD; }
            set { colorLineD = value; }
        }

        private int lowerBound = 20;
        [LocalizedDisplayName("TitleLowerLimit")]
        [Description("Нижняя граница (0 - не показывать)")]
        [LocalizedCategory("TitleVisuals")]
        [PropertyOrder(52)]
        public int LowerBound
        {
            get { return lowerBound; }
            set { lowerBound = value; }
        }

        private int upperBound = 80;
        [LocalizedDisplayName("TitleUpperLimit")]
        [Description("Верхняя граница (0 или 100 - не показывать)")]
        [LocalizedCategory("TitleVisuals")]
        [PropertyOrder(53)]
        public int UpperBound
        {
            get { return upperBound; }
            set { upperBound = value; }
        }

        private LineSeries seriesK, seriesD;
        private PartSeries seriesBounds;
        private RestrictedQueue<float> queue, queueMA;

        #endregion

        public override BaseChartIndicator Copy()
        {
            var cci = new IndicatorStochastic();
            Copy(cci);
            return cci;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var indiSt = (IndicatorStochastic)indi;
            CopyBaseSettings(indiSt);
            indiSt.Period = Period;
            indiSt.ColorLineK = ColorLineK;
            indiSt.ColorLineD = ColorLineD;
            indiSt.periodMA = periodMA;
            indiSt.seriesK = seriesK;
            indiSt.seriesD = seriesD;
            indiSt.LowerBound = LowerBound;
            indiSt.UpperBound = UpperBound;
        }

        public void BuildSeries(ChartControl chart)
        {
            seriesK.Data.Clear();
            seriesD.Data.Clear();
            seriesBounds.parts.Clear();

            if (SeriesSources.Count == 0) return;
            var source = SeriesSources[0];
            if (source.DataCount == 0) return;

            if (source is CandlestickSeries == false &&
                source is LineSeries == false) return;

            // границы
            if (lowerBound > 0)
                seriesBounds.parts.Add(new List<PartSeriesPoint>
                                       {
                                           new PartSeriesPoint(1, lowerBound),
                                           new PartSeriesPoint(source.DataCount, lowerBound)
                                       });
            if (upperBound > 0 && upperBound < 100)
            seriesBounds.parts.Add(new List<PartSeriesPoint>
                                       {
                                           new PartSeriesPoint(1, upperBound),
                                           new PartSeriesPoint(source.DataCount, upperBound)
                                       });            

            queue = new RestrictedQueue<float>(period);
            queueMA = new RestrictedQueue<float>(periodMA);

            for (var i = 0; i < source.DataCount; i++)
            {
                var price =
                    source is CandlestickSeries
                        ? ((CandlestickSeries) source).Data.Candles[i].close
                        : ((LineSeries) source).GetPrice(i) ?? 0;
                queue.Add(price);

                if (queue.Length < period)
                {
                    seriesK.Data.Add(50);
                    seriesD.Data.Add(50);
                    continue;
                }
                var minValue = float.MaxValue;
                var maxValue = float.MinValue;
                foreach (var p in queue)
                {
                    if (p < minValue) minValue = p;
                    if (p > maxValue) maxValue = p;
                }
                var range = maxValue - minValue;
                var k = range == 0 ? 50 : 100*(price - minValue)/range;
                queueMA.Add(k);
                var d = queueMA.Length == periodMA ? queueMA.Average() : k;
                
                seriesK.Data.Add(k);
                seriesD.Data.Add(d);
            }            
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            seriesK = new LineSeries("Stochastic")
            {
                LineColor = colorLineK,
                ShiftX = 1,
                Transparent = true
            };
            seriesD = new LineSeries("Stochastic MA")
            {
                LineColor = colorLineD,
                ShiftX = 1,
                LineDashStyle = DashStyle.Dot,
                Transparent = true
            };
            seriesBounds = new PartSeries("Stochastic Bounds") { LineStyle = LineStyle.Dot };
            SeriesResult = new List<Series.Series> { seriesK, seriesD, seriesBounds };
            EntitleIndicator();
        }

        public void Remove()
        {
            if (seriesK == null || seriesD == null) return;
            seriesK.Data.Clear();
            seriesD.Data.Clear();
            seriesBounds.parts.Clear();
        }

        public void AcceptSettings()
        {
            seriesK.LineColor = colorLineK;
            seriesD.LineColor = colorLineD;
            
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
            GetKnownColor(ColorLineK.ToArgb(), out colorStr);
            return string.Format("Stochastic [{0}]{1}",
                                 Period, 
                                 string.IsNullOrEmpty(colorStr) ? "" : string.Format("({0})", colorStr));
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Candlechart.Chart;
using Candlechart.ChartMath;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    [LocalizedDisplayName("TitleBollingerBands")]
    [LocalizedCategory("TitleTrending")]
    [TypeConverter(typeof(PropertySorter))]
    public class IndicatorBollinger : BaseChartIndicator, IChartIndicator
    {
        [Browsable(false)]
        public override string Name { get { return Localizer.GetString("TitleBollingerBands"); } }

        [LocalizedDisplayName("TitleCreateDrawingPanel")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageCreateDrawingPanelDescription")]
        public override bool CreateOwnPanel { get; set; }

        private int period = 20;
        [LocalizedDisplayName("TitlePeriod")]
        [LocalizedDescription("MessageMAPeriodAndVolatilityIntervalDescription")]
        [LocalizedCategory("TitleMain")]
        public int Period
        {
            get { return period; }
            set { period = value; }
        }

        private Color clLine = Color.Green;
        [LocalizedDisplayName("TitleColor")]
        [LocalizedDescription("MessageLineColorDescription")]
        [LocalizedCategory("TitleVisuals")]
        public Color ClLine
        {
            get { return clLine; }
            set { clLine = value; }
        }
        
        private DashStyle lineStyle = DashStyle.Solid;
        [LocalizedDisplayName("TitleBandStyle")]
        [LocalizedDescription("MessageLineStyleDescription")]
        [LocalizedCategory("TitleVisuals")]
        public DashStyle LineStyle
        {
            get { return lineStyle; }
            set { lineStyle = value; }
        }

        private DashStyle lineStyleMid = DashStyle.Dot;
        [LocalizedDisplayName("TitleMALineStyle")]
        [LocalizedDescription("MessageMALineStyleDescription")]
        [LocalizedCategory("TitleVisuals")]
        public DashStyle LineStyleMid
        {
            get { return lineStyleMid; }
            set { lineStyleMid = value; }
        }

        private MovAvgType maType = MovAvgType.Простая;
        [LocalizedDisplayName("TitleMALineType")]
        [LocalizedDescription("MessageMALineTypeDescription")]
        [LocalizedCategory("TitleMain")]
        public MovAvgType MaType
        {
            get { return maType; }
            set { maType = value; }
        }

        private decimal lineWidth = 1;
        [LocalizedDisplayName("TitleThickness")]
        [LocalizedDescription("MessageThicknessDescription")]
        [LocalizedCategory("TitleVisuals")]
        public decimal LineWidth
        {
            get { return lineWidth; }
            set { lineWidth = value; }
        }

        private CandlePriceType priceType = CandlePriceType.Close;
        [LocalizedDisplayName("TitlePrice")]
        [LocalizedDescription("MessagePriceTypeDescription")]
        [LocalizedCategory("TitleMain")]
        public CandlePriceType PriceType
        {
            get { return priceType; }
            set { priceType = value; }
        }

        [LocalizedDisplayName("TitleOffsetInBars")]
        [LocalizedDescription("MessageOffsetInBarsDescription")]
        [LocalizedCategory("TitleMain")]
        public int ShiftX { get; set; }

        private float kVolatile = 2;
        [LocalizedDisplayName("TitleVolatilityMultiplier")]
        [LocalizedDescription("MessageVolatilityMultiplierDescription")]
        [LocalizedCategory("TitleMain")]
        public float KVolatile
        {
            get { return kVolatile; }
            set { kVolatile = value; }
        }

        private LineSeries series = new LineSeries("BMA") { Transparent = true };
        private LineSeries seriesUp = new LineSeries("BUp") { Transparent = true };
        private LineSeries seriesDn = new LineSeries("BDn") { Transparent = true };

        private RestrictedQueue<float> queue;
        private RestrictedQueue<float> queueDisper;
        
        /// <summary>
        /// предыдущее значение для скользящей средней
        /// </summary>
        private float? smmaPrev;

        public IndicatorBollinger()
        {            
        }

        public IndicatorBollinger(IndicatorBollinger indi)
        {
        }

        public override BaseChartIndicator Copy()
        {
            var indi = new IndicatorBollinger();
            Copy(indi);
            return indi;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var indiBol = (IndicatorBollinger)indi;
            CopyBaseSettings(indiBol);
            indiBol.Period = Period;
            indiBol.ClLine = ClLine;
            indiBol.MaType = MaType;
            indiBol.PriceType = PriceType;
            indiBol.ShiftX = ShiftX;
            indiBol.series = series;
            indiBol.smmaPrev = smmaPrev;
            indiBol.KVolatile = KVolatile;
            indiBol.LineWidth = LineWidth;
            indiBol.LineStyle = LineStyle;
            indiBol.LineStyleMid = LineStyleMid;
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            SeriesResult = new List<Series.Series> { series, seriesUp, seriesDn };
            
            series.LineColor = clLine;
            series.LineWidth = (float)lineWidth;
            series.LineDashStyle = lineStyleMid;

            seriesUp.LineColor = clLine;
            seriesUp.LineWidth = (float)lineWidth;
            seriesUp.LineDashStyle = lineStyle;

            seriesDn.LineColor = clLine;
            seriesDn.LineWidth = (float)lineWidth;
            seriesDn.LineDashStyle = lineStyle;

            EntitleIndicator();
        }

        public void Remove()
        {
            if (series == null) return;
            series.Data.Clear();
            seriesUp.Data.Clear();
            seriesDn.Data.Clear();
        }

        public void AcceptSettings()
        {
            series.LineColor = ClLine;
            series.LineDashStyle = lineStyleMid;
            series.LineWidth = (float)lineWidth;
            series.ShiftX = ShiftX + 1;

            seriesUp.LineColor = ClLine;
            seriesUp.LineDashStyle = lineStyle;
            seriesUp.LineWidth = (float)lineWidth;
            seriesUp.ShiftX = ShiftX + 1;

            seriesDn.LineColor = ClLine;
            seriesDn.LineDashStyle = lineStyle;
            seriesDn.LineWidth = (float)lineWidth;
            seriesDn.ShiftX = ShiftX + 1;

            if (CreateOwnPanel)
            {
                SeriesResult = new List<Series.Series> { series, seriesUp, seriesDn };    
            }
            if (DrawPane != null && DrawPane != owner.StockPane)
                DrawPane.Title = string.Format("{0} [{1}]", UniqueName, Period);            
        }

        public void BuildSeries(ChartControl chart)
        {
            smmaPrev = null;
            series.Data.Clear();
            seriesUp.Data.Clear();
            seriesDn.Data.Clear();
            
            if (chart.StockSeries.DataCount < period) return;

            queue = new RestrictedQueue<float>(period);
            queueDisper = new RestrictedQueue<float>(period);

            // построить индюк
            BuildBollinger(SeriesSources[0]);            
        }

        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (newCandles.Count == 0) return;
            // построить индюк
            BuildSeries(owner);
        }

        private void BuildBollinger(Series.Series source)        
        {
            series.Data.Clear();
            seriesUp.Data.Clear();
            seriesDn.Data.Clear();
            if (source is CandlestickSeries == false &&
                source is LineSeries == false) return;

            float sum = 0;
            var frameLen = 0;
            
            for (var i = 0; i < source.DataCount; i++)
            {
                var price = GetPrice(source, i);

                // простая СС
                if (maType == MovAvgType.Простая)
                {
                    sum += price;
                    if (frameLen < period) frameLen++;
                    else
                    {
                        sum -= GetPrice(source, i - period);
                    }
                    series.Data.Add(sum/frameLen);
                }

                // сглаженная СС
                else
                {
                    if (smmaPrev.HasValue)
                    {
                        var smma = ((period - 1)*smmaPrev.Value + price)/period;
                        series.Data.Add(smma);
                        smmaPrev = smma;
                    }
                    else
                    {
                        queue.Add(price);
                        if (queue.Length == period)
                            smmaPrev = queue.Average();

                        series.Data.Add(smmaPrev ?? price);
                    }
                }

                // волатильность (СКО)
                // очередь из квадратов отклонений
                var lastMedian = series.Data[series.Data.Count - 1];
                var dev = lastMedian - price;
                queueDisper.Add((float)(dev * dev));

                var ssd = queueDisper.Length == period
                              ? Math.Sqrt(queueDisper.Average())
                              : 0;
                seriesUp.Data.Add(lastMedian + kVolatile * ssd);
                seriesDn.Data.Add(lastMedian - kVolatile * ssd);
            }
        }

        private float GetPrice(Series.Series source, int i)
        {
            var price =
                source is CandlestickSeries
                    ? ((CandlestickSeries) source).Data.Candles[i].GetPrice(priceType)
                    : ((LineSeries) source).GetPrice(i) ?? 0;
            return price;
        }

        /// <summary>
        /// на входе - экранные координаты
        /// </summary>        
        public string GetHint(int x, int y, double index, double price, int tolerance)
        {            
            if (series == null) return string.Empty;
            if (series.Owner == null) return string.Empty;

            var ptClient = series.Owner.ChartToClient(new Point(x, y));
            var pt = Conversion.ScreenToWorld(ptClient, series.Owner.WorldRect, series.Owner.CanvasRect);            

            // ReSharper disable InconsistentNaming
            var indexMA = (int) (pt.X + 0.5);
            // ReSharper restore InconsistentNaming
            if (indexMA < 0 || indexMA >= series.Data.Count) return string.Empty;
            var movAvg = series.Data[indexMA];            

            // вычислить расстояние от экранной точки курсора до экранной точки Машки (высоту)
            var ptMAscreen = Conversion.WorldToScreen(new PointD(pt.X, movAvg),
                                                      series.Owner.WorldRect, series.Owner.CanvasRect);
            ptMAscreen = series.Owner.CanvasPointToClientPoint(ptMAscreen);

            var deltaY = Math.Abs(ptMAscreen.Y - ptClient.Y);
            return deltaY < tolerance ? string.Format("MA[{0}, {1}]: {2:f4}", MaType, Period, movAvg) : string.Empty;
        }

        public List<IChartInteractiveObject> GetObjectsUnderCursor(int screenX, int screenY, int tolerance)
        {
            return new List<IChartInteractiveObject>();
        }        
    }
}

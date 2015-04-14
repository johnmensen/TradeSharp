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
    public enum MovAvgType
    {
        Простая = 0, Сглаженная
    }

    [LocalizedDisplayName("TitleMovingAverage")]
    [LocalizedCategory("TitleTrending")]
    [TypeConverter(typeof(PropertySorter))]
    // ReSharper disable InconsistentNaming
    public class IndicatorMA : BaseChartIndicator, IChartIndicator
    // ReSharper restore InconsistentNaming
    {
        [Browsable(false)]
        public override string Name { get { return Localizer.GetString("TitleMovingAverage"); } }

        public override BaseChartIndicator Copy()
        {
            var ma = new IndicatorMA();
            Copy(ma);
            return ma;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var ma = (IndicatorMA) indi;
            CopyBaseSettings(ma);
            ma.Period = Period;
            ma.ClLine = ClLine;
            ma.MaType = MaType;
            ma.PriceType = PriceType;
            ma.ShiftX = ShiftX;
            ma.series = series;
            ma.smmaPrev = smmaPrev;
        }

        [LocalizedDisplayName("TitleCreateDrawingPanel")]
        [LocalizedCategory("TitleMain")]
        [Description("Создавать свою панель отрисовки")]
        public override bool CreateOwnPanel { get; set; }

        private int period = 14;
        [LocalizedDisplayName("TitleMAPeriod")]
        [Description("Период СС")]
        [LocalizedCategory("TitleMain")]
        [PropertyOrder(1, 1)]
        public int Period
        {
            get { return period; }
            set { period = value; }
        }

        private Color clLine = Color.Brown;
        [LocalizedDisplayName("TitleColor")]
        [Description("Цвет СС")]
        [LocalizedCategory("TitleVisuals")]
        [PropertyOrder(1, 2)]
        public Color ClLine
        {
            get { return clLine; }
            set { clLine = value; }
        }
        
        private DashStyle lineStyle = DashStyle.Solid;
        [LocalizedDisplayName("TitleLineStyle")]
        [Description("Стиль линии индикатора (сплошная, штирх, штрих-пунктир...)")]
        [LocalizedCategory("TitleVisuals")]
        public DashStyle LineStyle
        {
            get { return lineStyle; }
            set { lineStyle = value; }
        }

        private MovAvgType maType = MovAvgType.Простая;
        [LocalizedDisplayName("TitleMAType")]
        [Description("Тип СС (простая, сглаженная)")]
        [LocalizedCategory("TitleMain")]
        public MovAvgType MaType
        {
            get { return maType; }
            set { maType = value; }
        }

        private decimal lineWidth = 1;
        [LocalizedDisplayName("TitleThickness")]
        [Description("Толщина линии индикатора, пикселей")]
        [LocalizedCategory("TitleVisuals")]
        public decimal LineWidth
        {
            get { return lineWidth; }
            set { lineWidth = value; }
        }

        private CandlePriceType priceType = CandlePriceType.Close;
        [LocalizedDisplayName("TitleMAPrice")]
        [Description("Цена, от которой считается СС - закрытие, медиана")]
        [LocalizedCategory("TitleMain")]
        public CandlePriceType PriceType
        {
            get { return priceType; }
            set { priceType = value; }
        }

        [LocalizedDisplayName("TitleOffsetInBars")]
        [Description("Смещение вправо (>0) или влево (<0), баров")]
        [LocalizedCategory("TitleMain")]
        public int ShiftX { get; set; }

        private LineSeries series = new LineSeries("СС") { Transparent = true };

        private RestrictedQueue<float> queue;
        
        /// <summary>
        /// предыдущее значение для скользящей средней
        /// </summary>
        private float? smmaPrev;

        public IndicatorMA()
        {
            CreateOwnPanel = false;
        }

        public IndicatorMA(IndicatorMA indi)
        {
            
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            SeriesResult = new List<Series.Series>{series};
            series.LineColor = clLine;
            series.LineWidth = (float)lineWidth;
            series.LineDashStyle = lineStyle;

            EntitleIndicator();
        }

        public void Remove()
        {
            if (series != null) series.Data.Clear();
        }

        public void AcceptSettings()
        {
            series.LineColor = ClLine;
            series.LineDashStyle = lineStyle;
            series.LineWidth = (float)lineWidth;
            series.ShiftX = ShiftX + 1;
            if (CreateOwnPanel)
            {
                SeriesResult = new List<Series.Series> { series };    
            }
            if (DrawPane != null && DrawPane != owner.StockPane)
                DrawPane.Title = string.Format("{0} [{1}]", UniqueName, Period);
        }

        public void BuildSeries(ChartControl chart)
        {
            smmaPrev = null;
            series.Data.Clear();
            
            var candles = chart.StockSeries.Data.Candles;
            if (candles.Count < period) return;

            queue = new RestrictedQueue<float>(period);
            // построить МА-шку
            BuildMA(SeriesSources[0]);
        }

        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (updatedCandle == null && newCandles.Count == 0) return;
            BuildSeries(owner);            
        }

        // ReSharper disable InconsistentNaming
        private void BuildMA(Series.Series source)
        // ReSharper restore InconsistentNaming
        {
            series.Data.Clear();
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
                    series.Data.Add(sum / frameLen);
                    continue;
                }

                // сглаженная СС
                if (smmaPrev.HasValue)
                {
                    var smma = ((period - 1) * smmaPrev.Value + price) / period;
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
        }

        private float GetPrice(Series.Series source, int i)
        {
            var price =
                source is CandlestickSeries
                    ? ((CandlestickSeries)source).Data.Candles[i].GetPrice(priceType)
                    : series.GetPrice(i) ?? 0;
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
            return deltaY < tolerance ? string.Format("MA[{0}, {1}]: {2}", MaType, Period, movAvg.ToStringUniformPriceFormat(true)) : string.Empty;
        }

        public List<IChartInteractiveObject> GetObjectsUnderCursor(int screenX, int screenY, int tolerance)
        {
            return new List<IChartInteractiveObject>();
        }        
    }
}

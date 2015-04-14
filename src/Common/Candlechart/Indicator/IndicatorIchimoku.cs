using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Candlechart.Chart;
using Candlechart.ChartMath;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    [LocalizedDisplayName("TitleIchimokuIndicator")]
    [LocalizedCategory("TitleTrending")]
    [TypeConverter(typeof(PropertySorter))]
    class IndicatorIchimoku : BaseChartIndicator, IChartIndicator
    {
        #region Свойства

        [LocalizedDisplayName("TitleCreateDrawingPanel")]
        [LocalizedCategory("TitleMain")]
        [Description("Создавать свою панель отрисовки")]
        public override bool CreateOwnPanel { get; set; }

        [Browsable(false)]
        public override string Name { get { return Localizer.GetString("TitleIchimokuIndicator"); } }

        private int periodS = 9;
        [LocalizedDisplayName("TitleShortTimeInterval")]
        [Description("Количесво свечей короткого временного промежутка")]
        [LocalizedCategory("TitleMain")]
        public int PeriodS
        {
            get { return periodS; }
            set { if (periodS < PeriodM && periodS < PeriodL) periodS = value; }
        }

        private int periodM = 26;
        [LocalizedDisplayName("TitleAverageTimeInterval")]
        [Description("Количесво свечей среднего временного промежутка")]
        [LocalizedCategory("TitleMain")]
        public int PeriodM
        {
            get { return periodM; }
            set { if (periodM > periodS && periodM < PeriodL) periodM = value; }
        }

        private int periodL = 52;
        [LocalizedDisplayName("TitleLongTimeInterval")]
        [Description("Количесво свечей длинного временного промежутка")]
        [LocalizedCategory("TitleMain")]
        public int PeriodL
        {
            get { return periodL; }
            set { if (periodL > periodM && periodL > periodS) periodL = value; }
        }

        private Color colorTencan = Color.Red;
        [LocalizedDisplayName("TitleTenkanLine")]
        [Description("Tencan")]
        [LocalizedCategory("TitleMain")]
        public Color ColorTencan
        {
            get { return colorTencan; }
            set { colorTencan = value; }
        }

        private Color colorKijyn = Color.Blue;
        [LocalizedDisplayName("TitleKijunLine")]
        [Description("Kijyn")]
        [LocalizedCategory("TitleMain")]
        public Color ColorKijyn
        {
            get { return colorKijyn; }
            set { colorKijyn = value; }
        }

        private Color colorSenkouA = Color.Chocolate;
        [LocalizedDisplayName("TitleSenkouALine")]
        [Description("SenkouA")]
        [LocalizedCategory("TitleMain")]
        public Color ColorSenkouA
        {
            get { return colorSenkouA; }
            set { colorSenkouA = value; }
        }

        private Color colorSenkouB = Color.DimGray;
        [LocalizedDisplayName("TitleSenkouBLine")]
        [Description("SenkouB")]
        [LocalizedCategory("TitleMain")]
        public Color ColorSenkouB
        {
            get { return colorSenkouB; }
            set { colorSenkouB = value; }
        }

        private Color colorChikou = Color.ForestGreen;
        [LocalizedDisplayName("TitleChikou")]
        [Description("Chikou")]
        [LocalizedCategory("TitleMain")]
        public Color ColorChikou
        {
            get { return colorChikou; }
            set { colorChikou = value; }
        }

        private Color colorCloudA = Color.Red;
        [LocalizedDisplayName("TitleKumoA")]
        [Description("Цвет облака, если Senkou A находится выше Senkou B")]
        [LocalizedCategory("TitleMain")]
        public Color ColorCloudA
        {
            get { return colorCloudA; }
            set { colorCloudA = value; }
        }

        private Color colorCloudB = Color.RoyalBlue;
        [LocalizedDisplayName("TitleKumoB")]
        [Description("Цвет облака, если Senkou B находится выше Senkou A")]
        [LocalizedCategory("TitleMain")]
        public Color ColorCloudB
        {
            get { return colorCloudB; }
            set { colorCloudB = value; }
        }

        // 5 линий индикатора
        private TrendLineSeries tencan;
        private TrendLineSeries kijyn;
        private TrendLineSeries senkouA;
        private TrendLineSeries senkouB;
        private TrendLineSeries chikou;

        // облако
        private PolygonSeries cloud;
        //SeriesSources
        #endregion

        public IndicatorIchimoku()
        {
            CreateOwnPanel = false;
        }

        #region IChartIndicator
        public void BuildSeries(ChartControl chart)
        {
            Calculation();            
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {          
            owner = chart;
            tencan = new TrendLineSeries(string.Format("{0} - {1}", Localizer.GetString("TitleIchimokuIndicator"), Localizer.GetString("TitleTenkanLineSmall")));
            kijyn = new TrendLineSeries(string.Format("{0} - {1}", Localizer.GetString("TitleIchimokuIndicator"), Localizer.GetString("TitleKijunLineSmall")));
            senkouA = new TrendLineSeries(string.Format("{0} - {1}", Localizer.GetString("TitleIchimokuIndicator"), Localizer.GetString("TitleSenkouALineSmall")));
            senkouB = new TrendLineSeries(string.Format("{0} - {1}", Localizer.GetString("TitleIchimokuIndicator"), Localizer.GetString("TitleSenkouBLineSmall")));
            chikou = new TrendLineSeries(string.Format("{0} - {1}", Localizer.GetString("TitleIchimokuIndicator"), Localizer.GetString("TitleChikouSmall")));
            cloud = new PolygonSeries(string.Format("{0} - {1}", Localizer.GetString("TitleIchimokuIndicator"), Localizer.GetString("TitleKumoSmall")))
                {
                    colorCloudA = ColorCloudA,
                    colorCloudB = ColorCloudB
                };

            SeriesResult = new List<Series.Series> { tencan, kijyn, senkouA, senkouB, chikou, cloud };
            EntitleIndicator();
        }

        public void Remove()
        {
            if (tencan != null) tencan.data.Clear();
            if (kijyn != null) kijyn.data.Clear();
            if (senkouA != null) senkouA.data.Clear();
            if (senkouB != null) senkouB.data.Clear();
            if (chikou != null) chikou.data.Clear();
            if (cloud != null) cloud.data.Clear();
        }

        public void AcceptSettings()
        {
            SeriesResult = new List<Series.Series> { tencan, kijyn, senkouA, senkouB, chikou, cloud };
            if (DrawPane != null && DrawPane != owner.StockPane) DrawPane.Title = UniqueName;
        }

        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (newCandles.Count != 0)
            {
                // добавилась крайняя свеча графика
                Calculation();
            }
        }

        #endregion      

        #region BaseChartIndicator
        public override BaseChartIndicator Copy()
        {
            var indi = new IndicatorIchimoku();
            Copy(indi);
            
            return indi;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var ichimoku = (IndicatorIchimoku)indi;
            CopyBaseSettings(ichimoku);
            ichimoku.PeriodS = PeriodS;
            ichimoku.PeriodM = PeriodM;
            ichimoku.PeriodL = PeriodL;
            ichimoku.ColorChikou = ColorChikou;
            ichimoku.ColorKijyn = ColorKijyn;
            ichimoku.ColorSenkouA = ColorSenkouA;
            ichimoku.ColorSenkouB = ColorSenkouB;
            ichimoku.ColorTencan = ColorTencan;
            ichimoku.ColorCloudA = ColorCloudA;
            ichimoku.ColorCloudB = ColorCloudB;
        }

        public string GetHint(int x, int y, double index, double price, int tolerance)
        {
            var lineKijyn = kijyn.data.FirstOrDefault(l => Math.Round(l.linePoints[0].X) == Math.Round(index));
            var lineTencan = tencan.data.FirstOrDefault(l => Math.Round(l.linePoints[0].X) == Math.Round(index));
            var lineSenkouA = senkouA.data.FirstOrDefault(l => Math.Round(l.linePoints[0].X) == Math.Round(index));
            var lineSenkouB = senkouB.data.FirstOrDefault(l => Math.Round(l.linePoints[0].X) == Math.Round(index));
            var lineChikou = chikou.data.FirstOrDefault(l => Math.Round(l.linePoints[0].X) == Math.Round(index));

            return string.Format(
                "tencan = {0} \n\r kijyn = {1} \n\r senkouA = {2} \n\r senkouB = {3} \n\r chikou = {4}",
                lineTencan != null ? lineTencan.linePoints[0].Y.ToString() : string.Empty,
                lineKijyn != null ? lineKijyn.linePoints[0].Y.ToString() : string.Empty,
                lineSenkouA != null ? lineSenkouA.linePoints[0].Y.ToString() : string.Empty,
                lineSenkouB != null ? lineSenkouB.linePoints[0].Y.ToString() : string.Empty,
                lineChikou != null ? lineChikou.linePoints[0].Y.ToString() : string.Empty);
        }

        public List<IChartInteractiveObject> GetObjectsUnderCursor(int screenX, int screenY, int tolerance)
        {
            return new List<IChartInteractiveObject>();
        }
        #endregion

        private void Calculation()
        {
            if (SeriesSources[0] is CandlestickSeries == false && SeriesSources[0] is LineSeries == false) return;
            Remove();

            var series = SeriesSources[0] as CandlestickSeries;
            var data = series != null ? series.Data.Candles : new List<CandleData>();
                                  //: ((LineSeries)SeriesSources[0]).Data.DataArray.ToList();


            for (var i = 0; i < data.Count; i++)
            {
                if (i > periodS)
                {
                    var source = data.Skip(i - periodS).Take(periodS).ToList();
                    var tencanVal = (source.Min(x => x.low) + source.Max(x => x.high)) / 2;
                    var tencanLastY = tencan.data.Any() ? tencan.data.Last().linePoints.Last().Y : tencanVal;

                    tencan.data.Add(MakeTrendShelf(i, tencanVal, ColorTencan, -1, tencanLastY - tencanVal));
                }

                if (i > periodM)
                {
                    var source = data.Skip(i - periodM).Take(periodM).ToList();
                    var kijynVal = (source.Min(x => x.low) + source.Max(x => x.high)) / 2;
                    var kijynLastY = kijyn.data.Any() ? kijyn.data.Last().linePoints.Last().Y : kijynVal;

                    kijyn.data.Add(MakeTrendShelf(i, kijynVal, ColorKijyn, -1, kijynLastY - kijynVal));
                }

                if (i > periodL)
                {
                    var source = data.Skip(i - periodL).Take(periodL).ToList();
                    CalculationCloudStep(source, i);
                }
            }

            DrawingCloud();
        }

        /// <summary>
        /// Расчёт наобходимых данных для построения тех линий, для которых нужен "большой" период: senkouA, senkouB (облако) и chikou
        /// В этом методе отдельно рассчитываются tencan и kijyn нужные для senkouA, senkouB и chikou
        /// </summary>
        private void CalculationCloudStep(List<CandleData> source, int lastPointNumder)
        {
            var tencanSource = source.Skip(source.Count() - periodS).ToList();
            var tencanVal = (tencanSource.Min(x => x.low) + tencanSource.Max(x => x.high)) / 2;

            var kijynSource = source.Skip(source.Count() - periodM).ToList();
            var kijynVal = (kijynSource.Min(x => x.low) + kijynSource.Max(x => x.high)) / 2;

            var senkouAVal = (tencanVal + kijynVal)/2;
            var senkouBVal = (source.Min(x => x.low) + source.Max(x => x.high)) / 2;
            var chikouVal = source.Last().close;

            var senkouALastY = senkouA.data.Any() ? senkouA.data.Last().linePoints.Last().Y : senkouAVal;
            var senkouBLastY = senkouB.data.Any() ? senkouB.data.Last().linePoints.Last().Y : senkouBVal;
            var chikouLastY = chikou.data.Any() ? chikou.data.Last().linePoints.Last().Y : chikouVal;

            senkouA.data.Add(MakeTrendShelf(lastPointNumder + periodM, senkouAVal, ColorSenkouA, -1, senkouALastY - senkouAVal));
            senkouB.data.Add(MakeTrendShelf(lastPointNumder + periodM, senkouBVal, ColorSenkouB, -1, senkouBLastY - senkouBVal));
            chikou.data.Add(MakeTrendShelf(lastPointNumder - periodM, chikouVal, ColorChikou, -1, chikouLastY - chikouVal));
        }

        /// <summary>   
        /// Вспомогательный метод прорисовки отрезка 
        /// </summary>
        private TrendLine MakeTrendShelf(double x, double y, Color color, double dx = 0, double dy = 0)
        {
            var line = new TrendLine { LineStyle = TrendLine.TrendLineStyle.Отрезок, LineColor = color };
            line.linePoints.Add(new PointD(x + dx, y + dy));
            line.linePoints.Add(new PointD(x, y));
            return line;
        }

        /// <summary>
        /// Вспомогательный метод прорисовки облака
        /// </summary>
        private void DrawingCloud()
        {
            for (var i = 0; i < senkouA.data.Count; i++)
            {
                cloud.data.Add(new PolygonSeries.DoubleLinePoint
                {
                    x = senkouA.data[i].linePoints[0].X,
                    yA = senkouA.data[i].linePoints[0].Y,
                    yB = senkouB.data[i].linePoints[0].Y
                });
            }
        }
    }    
}
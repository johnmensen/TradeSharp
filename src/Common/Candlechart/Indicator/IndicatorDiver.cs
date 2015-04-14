using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Text;
using Candlechart.Chart;
using Candlechart.ChartMath;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Util;
using System.Linq;

namespace Candlechart.Indicator
{
    [LocalizedDisplayName("TitleDivergences")]
    [LocalizedCategory("TitleDivergences")]
    [TypeConverter(typeof(PropertySorter))]
    public class IndicatorDiver : BaseChartIndicator, IChartIndicator
    {
        public override BaseChartIndicator Copy()
        {
            var div = new IndicatorDiver();
            Copy(div);
            return div;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var div = (IndicatorDiver) indi;
            CopyBaseSettings(div);
            div.DiverType = DiverType;
            div.PeriodExtremum = PeriodExtremum;
            div.MaxPastExtremum = MaxPastExtremum;
            div.MarginUpper = MarginUpper;
            div.MarginLower = MarginLower;
            div.ColorArrowUp = ColorArrowUp;
            div.ColorArrowDown = ColorArrowDown;
            div.seriesDivArrow = seriesDivArrow;
            div.seriesRegion = seriesRegion;
            div.IndicatorDrawStyle = IndicatorDrawStyle;
            div.IsVisible = IsVisible;
            div.WaitOneBar = WaitOneBar;
        }

        #region Данные и свойства - основные

        [Browsable(false)]
        public override string Name
        {
            get { return Localizer.GetString("TitleDivergences"); }
        }

        [LocalizedDisplayName("TitleCreateDrawingPanel")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageCreateDrawingPanelDescription")]
        public override bool CreateOwnPanel { get; set; }

        public enum DrawStyle
        {
            Стрелки = 0,
            Регионы = 1,
            Треугольнички = 2,
            ЦветныеСвечи = 3
        }

        [LocalizedDisplayName("TitleDrawingStyle")]
        [LocalizedCategory("TitleVisuals")]
        [LocalizedDescription("MessageDrawingStyleDescription")]
        public DrawStyle IndicatorDrawStyle { get; set; }

        private TrendLineSeries seriesDivArrow;

        private RegionSeries seriesRegion;

        private int SourceDataCount
        {
            get
            {
                if (SeriesSources.Count < 1) return 0;
                return SeriesSources[0] is StockSeries
                                ? ((StockSeries)SeriesSources[0]).Data.Count
                                : SeriesSources[0] is LineSeries
                                      ? ((LineSeries)SeriesSources[0]).Data.Count : 0;
            }
        }

        #endregion

        #region Настройки дивергов

        public enum DivergenceType { Классические = 0, ОтКвазиЭкстремумов = 1 }

        [LocalizedDisplayName("TitleDivergenceType")]
        [LocalizedCategory("TitleDivergences")]
        [LocalizedDescription("MessageDivergenceTypeDescription")]
        public DivergenceType DiverType { get; set; }
        
        private int periodExtremum = 6;
        [LocalizedDisplayName("TitleExtremumInBarsShort")]
        [LocalizedCategory("TitleDivergences")]
        [LocalizedDescription("MessageExtremumInBarsDescription")]
        public int PeriodExtremum
        {
            get { return periodExtremum; }
            set { periodExtremum = value; }
        }

        private int maxPastExtremum = 18;
        [LocalizedDisplayName("TitleCandleCountToExtremumShort")]
        [LocalizedCategory("TitleDivergences")]
        [LocalizedDescription("MessageCandleCountToExtremumDescription")]
        public int MaxPastExtremum
        {
            get { return maxPastExtremum; }
            set { maxPastExtremum = value; }
        }

        [LocalizedDisplayName("TitleConfirmationBar")]
        [LocalizedCategory("TitleDivergences")]
        [LocalizedDescription("MessageConfirmationBarDescription")]
        public bool WaitOneBar { get; set; }

        private double marginUpper = 1;
        [LocalizedDisplayName("TitleZoneUpperLimit")]
        [LocalizedCategory("TitleDivergences")]
        [LocalizedDescription("MessageZoneUpperLimitDescription")]
        public double MarginUpper
        {
            get { return marginUpper; }
            set { marginUpper = value; }
        }

        private double marginLower = -1;
        [LocalizedDisplayName("TitleZoneLowerLimit")]
        [LocalizedCategory("TitleDivergences")]
        [LocalizedDescription("MessageZoneLimitDescription")]
        public double MarginLower
        {
            get { return marginLower; }
            set { marginLower = value; }
        }

        #endregion

        #region Данные - визуальные

        private Color colorArrowUp = Color.Blue;
        [LocalizedDisplayName("TitleBullDivergenceColor")]
        [LocalizedCategory("TitleVisuals")]
        [LocalizedDescription("MessageBullDivergenceColorDescription")]
        public Color ColorArrowUp { get { return colorArrowUp; } set { colorArrowUp = value; } }

        private Color colorArrowDown = Color.Red;
        [LocalizedDisplayName("TitleBearishDivergenceColor")]
        [LocalizedCategory("TitleVisuals")]
        [LocalizedDescription("MessageBearishDivergenceColorDescription")]        
        public Color ColorArrowDown { get { return colorArrowDown; } set { colorArrowDown = value; } }
        
        private static int indicatorThemeIndex;

        #endregion

        #region Настройки отрисовки

        [LocalizedDisplayName("TitleSourceSeries")]
        [LocalizedDescription("MeesageSourceSeriesDescription")]
        [LocalizedCategory("TitleMain")]
        [Editor("Candlechart.Indicator.CheckedListBoxSeriesUITypeEditor, System.Drawing.Design.UITypeEditor",
            typeof(UITypeEditor))]
        public override string SeriesSourcesDisplay { get; set; }

        private bool isVisible = true;
        [LocalizedDisplayName("TitleShowOnChart")]
        [LocalizedDescription("MessageShowDivergenceOnChartDescription")]
        [LocalizedCategory("TitleMain")]
        public bool IsVisible
        {
            get { return isVisible; }
            set { isVisible = value; }
        }

        #endregion

        public IndicatorDiver()
        {
            CreateOwnPanel = false;
        }

        public void BuildSeries(ChartControl chart)
        {
            seriesDivArrow.data.Clear();
            seriesRegion.data.Clear();
            if (SeriesSources.Count == 0) return;
            if (IndicatorDrawStyle == DrawStyle.Регионы || IndicatorDrawStyle == DrawStyle.ЦветныеСвечи)
            {
                if (SeriesSources[0] is CandlestickSeries)
                    seriesRegion.SingleBarRegionWidth = 0.5d*((CandlestickSeries) SeriesSources[0]).BarWidthPercent/100;
            }

            if (!IsVisible) return;

            if (SeriesSources == null) return;
            if (SeriesSources.Count != 2)
            {
                Logger.ErrorFormat("{0}: неверно заданы источники данных (должно быть {1})", 
                    UniqueName, SeriesSources.Count);
                return;
            }
            
            // строить диверги
            var spans = DiverType == DivergenceType.Классические
                            ? FindDivergencePointsClassic()
                            : FindDivergencePointsQuasi();
            foreach (var span in spans)
            {
                AddDivergenceSign(span.start, span.end, span.sign);
            }
            // объединить соседние регионы
            if (IndicatorDrawStyle == DrawStyle.Регионы)
                MergeAdjacentRegions();
        }

        private void MergeAdjacentRegions()
        {
            if (seriesRegion.data.Count <= 0) return;
            var mergedRegions = new List<BarRegion>();
            for (var i = 0; i < seriesRegion.data.Count;)
            {
                var regUnion = seriesRegion.data[i];
                var j = i + 1;
                for (; j < seriesRegion.data.Count; j++)
                {
                    if (seriesRegion.data[j].Color != regUnion.Color ||
                        seriesRegion.data[j].IndexStart.Value != regUnion.IndexEnd.Value)
                        break;
                    
                    // поглотить регион
                    regUnion.IndexEnd = regUnion.IndexEnd + 1;
                    if (regUnion.UpperBound.HasValue)
                    {
                        regUnion.UpperBound = Math.Max(seriesRegion.data[j].UpperBound.Value,
                                                        regUnion.UpperBound.Value);
                        regUnion.LowerBound = Math.Min(seriesRegion.data[j].LowerBound.Value,
                                                        regUnion.LowerBound.Value);
                    }                    
                }
                i = j;
                mergedRegions.Add(regUnion);
            }
            seriesRegion.data = mergedRegions;
        }

        /// <summary>
        /// полу-классические диверы: первая точка должна быть экстремумом
        /// </summary>
        private List<DiverSpan> FindDivergencePointsClassic()
        {
            return Divergency.FindDivergencePointsClassic(SourceDataCount,
                                                          PeriodExtremum, 
                                                          MaxPastExtremum, 
                                                          GetSourcePrice,
                                                          GetIndexPrice,
                                                          WaitOneBar);
        }

        private List<DiverSpan> FindDivergencePointsQuasi()
        {
            return Divergency.FindDivergencePointsQuasi(SourceDataCount,
                                                        MarginUpper,
                                                        MarginLower,
                                                        GetSourcePrice,
                                                        GetIndexPrice);
        }

        private void AddDivergenceSign(int startIndex, int endIndex, int sign)
        {
            if (IndicatorDrawStyle == DrawStyle.Регионы)
                AddDivergenceRegion(endIndex, sign);
            else
                if (IndicatorDrawStyle == DrawStyle.Стрелки)
                    AddDivergenceArrow(startIndex, endIndex, sign);
                else if (IndicatorDrawStyle == DrawStyle.Треугольнички)
                    AddDivergenceTriangle(endIndex, sign);
                else
                    AddDivergenceColorBar(endIndex, sign);
        }

        private void AddDivergenceRegion(int endIndex, int sign)
        {
            var reg = new BarRegion
                          {
                              IndexStart = endIndex,
                              IndexEnd = endIndex + 1,
                              Color = sign < 0 ? ColorArrowDown : ColorArrowUp
                          };
            // найти цены
            var seriesSrc = SeriesSources[0];
            if (seriesSrc is StockSeries)
            {
                var candle = ((StockSeries) seriesSrc).Data.Candles[endIndex];
                reg.UpperBound = Math.Max(candle.open, candle.close);
                reg.LowerBound = Math.Min(candle.open, candle.close);
            }
            seriesRegion.data.Add(reg);
        }

        private void AddDivergenceArrow(int startIndex, int endIndex, int sign)
        {
            var dataCount = SourceDataCount;
            if (startIndex < 0 || startIndex >= dataCount) return;
            if (endIndex < 0 || endIndex >= dataCount) return;
            
            var line = new TrendLine();
            line.linePoints.Add(new PointD(startIndex, GetSourcePrice(startIndex)));
            line.linePoints.Add(new PointD(endIndex, GetSourcePrice(endIndex)));
            line.LineStyle = TrendLine.TrendLineStyle.Стрелка;
            line.LineColor = sign > 0 ? ColorArrowUp : ColorArrowDown;
            seriesDivArrow.data.Add(line);
        }

        private void AddDivergenceTriangle(int endIndex, int sign)
        {
            var dataCount = SourceDataCount;
            if (endIndex < 0 || endIndex >= dataCount) return;
            var price = GetSourcePrice(endIndex);

            var line = new TrendLine
                           {
                               LineStyle = TrendLine.TrendLineStyle.СвечнаяСтрелка,
                               LineColor = sign > 0 ? ColorArrowUp : ColorArrowDown,
                               ShapeAlpha = 192,
                               ShapeFillColor = sign > 0 ? ColorArrowUp : ColorArrowDown
                           };
            line.AddPoint(endIndex, price - sign * price * 0.01);
            line.AddPoint(endIndex, price);
            seriesDivArrow.data.Add(line);
        }

        private void AddDivergenceColorBar(int endIndex, int sign)
        {
            if (SeriesSources[0] is CandlestickSeries == false) return;
            var candles = ((CandlestickSeries) SeriesSources[0]).Data.Candles;
            if (endIndex < 0 || endIndex >= candles.Count) return;

            var color = sign > 0 ? colorArrowUp : sign < 0 ? colorArrowDown : (Color?)null;
            if (!color.HasValue) return;
            //seriesRegion.CustomAlphaChannel = true;
            seriesRegion.data.Add(new BarRegion
                                      {
                                          IndexStart = endIndex,
                                          IndexEnd = endIndex,
                                          Color = color.Value,
                                          LowerBound = Math.Min(candles[endIndex].open, candles[endIndex].close),
                                          UpperBound = Math.Max(candles[endIndex].open, candles[endIndex].close)
                                      });            
        }

        private double GetSourcePrice(int index)
        {
            return GetSourceSeriesPrice(index, 0);
        }

        private double GetIndexPrice(int index)
        {
            return GetSourceSeriesPrice(index, 1);
        }

        private double GetSourceSeriesPrice(int index, int indexSeries)
        {
            if (SeriesSources[indexSeries] is StockSeries)
            {
                var stockSrc = (StockSeries)SeriesSources[indexSeries];
                return index < 0 || index >= stockSrc.Data.Count
                           ? 0 : (double)stockSrc.Data.Candles[index].close;
            }
            if (SeriesSources[indexSeries] is LineSeries)
            {
                var lineSrc = (LineSeries)SeriesSources[indexSeries];
                return index < 0 || index >= lineSrc.Data.Count
                           ? 0 : lineSrc.Data[index];
            }
            return 0;
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            seriesDivArrow = new TrendLineSeries(Localizer.GetString("TitleDivergences"));
            seriesRegion = new RegionSeries(Localizer.GetString("TitleDivergenceRegions")) { CustomAlphaChannel = true };
            // инициализируем индикатор
            EntitleIndicator();
            SeriesResult = new List<Series.Series> { seriesDivArrow, seriesRegion };
            // цвета
            /*ColorArrowDown = IndicatorColorScheme.colorsOfRed[indicatorThemeIndex];
            ColorArrowUp = IndicatorColorScheme.colorsOfBlue[indicatorThemeIndex++];
            if (indicatorThemeIndex >= IndicatorColorScheme.PresetColorsCount)
                indicatorThemeIndex = 0;*/
        }

        public void Remove()
        {
            if (seriesDivArrow != null) seriesDivArrow.data.Clear();
            if (seriesRegion != null) seriesRegion.data.Clear();
        }

        public void AcceptSettings()
        {
        }

        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (updatedCandle == null) return;
            if (seriesRegion != null && seriesRegion.DataCount > 0)
                UpdateLastRegion();
            if (newCandles == null) return;
            if (newCandles.Count == 0) return;
            BuildSeries(owner);
        }

        /// <summary>
        /// регион должен строго накрыть послед. свечку, не перекрывая ее
        /// </summary>
        private void UpdateLastRegion()
        {
            if (SeriesSources.Count == 0 || SeriesSources[0] is CandlestickSeries == false) return;
            var candles = ((CandlestickSeries) SeriesSources[0]).Data.Candles;            
            
            // найти последний регион
            var reg = seriesRegion.data[0];
            for (var i = 1; i < seriesRegion.DataCount; i++)
            {
                if (seriesRegion.data[i].IndexStart > reg.IndexStart)
                    reg = seriesRegion.data[i];
            }
            if (candles.Count != (reg.IndexStart + 1)) return;
            var candle = candles[candles.Count - 1];
            reg.LowerBound = Math.Min(candle.open, candle.close);
            reg.UpperBound = Math.Max(candle.open, candle.close);
        }

        public string GetHint(int x, int y, double index, double price, int tolerance)
        {
            if (SeriesSources.Count < 2) return string.Empty;
            
            var roundedIndex = (int) Math.Round(index);
            var areas = seriesRegion.data.Where(r => r.IndexEnd.Value <= index && 
                r.IndexStart.Value >= index).ToList();
            var arrows = seriesDivArrow.data.Where(a => a.linePoints[1].X == roundedIndex).ToList();
            if (areas.Count == 0 && arrows.Count == 0) return string.Empty;
            var indiData = new StringBuilder(string.Format("{0} {1}:",
                                                           DiverType == DivergenceType.Классические
                                                               ? Localizer.GetString("TitleClassicalSmallShort")
                                                               : Localizer.GetString("TitleQuasiSmall"),
                                                           Localizer.GetString("TitleDivergencesSmall")));
            indiData.AppendFormat("/{0}, {1}/", SeriesSources[0].Name, SeriesSources[1].Name);
            var countUp = areas.Count(a => a.Color == ColorArrowUp) + arrows.Count(a => a.LineColor == colorArrowUp);
            var countDn = areas.Count(a => a.Color == ColorArrowDown) + arrows.Count(a => a.LineColor == ColorArrowDown);
            if (countUp > 0)
            {
                indiData.AppendFormat(", {0}: {1}", Localizer.GetString("TitleBullDivergencesSmall"), countUp);
            }
            if (countDn > 0)
            {
                indiData.AppendFormat(", {0}: {1}", Localizer.GetString("TitleBearishDivergencesSmall"), countDn);
            }
            return indiData.ToString();
        }

        public List<IChartInteractiveObject> GetObjectsUnderCursor(int screenX, int screenY, int tolerance)
        {
            return new List<IChartInteractiveObject>();
        }

        private List<object> GetDiversUnderCursor(int screenX, int screenY, int tolerance)
        {
            var list = new List<object>();
            if (DrawPane == null) return list;
            var ptClient = owner.PointToClient(new Point(screenX, screenY));

            foreach (var arrow in seriesDivArrow.data)
            {
                if (arrow.linePoints.Count != 2) continue;
                var a = Conversion.WorldToScreen(new PointD(arrow.linePoints[0].X, arrow.linePoints[0].Y),
                    DrawPane.WorldRect, DrawPane.CanvasRect);
                var b = Conversion.WorldToScreen(new PointD(arrow.linePoints[1].X, arrow.linePoints[1].Y),
                    DrawPane.WorldRect, DrawPane.CanvasRect);
                if (Geometry.IsDotInArea(new PointD(ptClient.X, ptClient.Y), a, b, tolerance))
                    list.Add(arrow);
            }
            foreach (var arrow in seriesRegion.data)
            {
                var a = Conversion.WorldToScreen(new PointD(arrow.IndexStart.Value, (double)arrow.UpperBound.Value),
                    DrawPane.WorldRect, DrawPane.CanvasRect);
                var b = Conversion.WorldToScreen(new PointD(arrow.IndexEnd.Value, (double)arrow.LowerBound.Value),
                    DrawPane.WorldRect, DrawPane.CanvasRect);
                if (Geometry.IsDotInArea(new PointD(ptClient.X, ptClient.Y), a, b, tolerance))
                    list.Add(arrow);
            }
            return list;
        }
    }
}

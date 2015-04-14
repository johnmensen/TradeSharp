using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using Candlechart.Chart;
using Candlechart.ChartMath;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    [LocalizedDisplayName("TitleExtrema")]
    [LocalizedCategory("TitleDivergences")]
    [TypeConverter(typeof(PropertySorter))]
    public class IndicatorExtremum : BaseChartIndicator, IChartIndicator
    {
        #region Настройки

        [LocalizedDisplayName("TitleShowVertexes")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageShowVertexesDescription")]
        public bool ShowBars { get; set; }

        public enum ExtremumBarDirection { СнизуВверх = 0, СверхуВниз }

        [LocalizedDisplayName("TitleVertexBeamDirection")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageVertexBeamDirectionDescription")]
        public ExtremumBarDirection ExtremumDirection { get; set; }

        [LocalizedDisplayName("TitleShowBorders")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageShowZonesDescription")]
        public bool ShowMarginZones { get; set; }

        private double margUpper = 1;
        [LocalizedDisplayName("TitleZoneUpperLimit")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageZoneUpperLimitDescription")]
        public double MargUpper
        {
            get { return margUpper; }
            set { margUpper = value; }
        }

        private double margLower = -1;
        [LocalizedDisplayName("TitleZoneLowerLimit")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageZoneLowerLimitDescription")]
        public double MargLower
        {
            get { return margLower; }
            set { margLower = value; }
        }

        public enum ExtremumSearchType
        {
            КвазиЭкстремумы = 0, Классические
        }

        [LocalizedDisplayName("TitleExtremaType")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageExtremaTypeDescription")]
        public ExtremumSearchType ExtremumType { get; set; }

        private int classicExtremumBar = 6;
        [LocalizedDisplayName("TitleClassicExtremumBarShort")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageClassicExtremumBarDescription")]
        public int ClassicExtremumBar
        {
            get { return classicExtremumBar; }
            set { classicExtremumBar = value; }
        }

        #endregion

        [LocalizedDisplayName("TitleSourceSeries")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MeesageSourceSeriesDescription")]
        [Editor("Candlechart.Indicator.CheckedListBoxSeriesUITypeEditor, System.Drawing.Design.UITypeEditor",
            typeof(UITypeEditor))]
        public override string SeriesSourcesDisplay { get; set; }

        [LocalizedDisplayName("TitleCreateDrawingPanel")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageCreateDrawingPanelDescription")]
        public override bool CreateOwnPanel { get; set; }

        private RegionSeries seriesMargins;
        private TrendLineSeries seriesExtr;

        public override BaseChartIndicator Copy()
        {
            var cpy = new IndicatorExtremum();
            Copy(cpy);
            return cpy;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var cpy = (IndicatorExtremum) indi;
            CopyBaseSettings(indi);
            cpy.ShowBars = ShowBars;
            cpy.ExtremumDirection = ExtremumDirection;
            cpy.ShowMarginZones = ShowMarginZones;
            cpy.MargUpper = MargUpper;
            cpy.MargLower = MargLower;
            cpy.ExtremumType = ExtremumType;
            cpy.ClassicExtremumBar = ClassicExtremumBar;
        }

        [Browsable(false)]
        public override string Name
        {
            get { return Localizer.GetString("TitleExtrema"); }
        }

        public void BuildSeries(ChartControl chart)
        {
            seriesMargins.data.Clear();
            seriesExtr.data.Clear();
            // нарисовать границы
            if (ShowMarginZones)
            {
                seriesMargins.data.Add(new BarRegion
                                              {
                                                  Color = Color.FromArgb(40, Color.Green),
                                                  UpperBound = 900000,
                                                  LowerBound = (float)margUpper
                                              });
                seriesMargins.data.Add(new BarRegion
                                              {
                                                  Color = Color.FromArgb(40, Color.Maroon),
                                                  UpperBound = (float)margLower,
                                                  LowerBound = -900000
                                              });
            }
            // рисовать палочки экстремумов
            if (!ShowBars) return;
            if (SeriesSources.Count == 0) return;

            var srcCount = GetSourceDataCount();
            srcCount = Math.Min(owner.StockSeries.Data.Count, srcCount);


            if (ExtremumType == ExtremumSearchType.КвазиЭкстремумы)
                BuildQuasiExtremums(srcCount);
            else
                if (ExtremumType == ExtremumSearchType.Классические)
                    BuildClassicExtremums(srcCount);
        }

        private void BuildQuasiExtremums(int srcCount)
        {
            double? lastPeak = null;
            var peakPrice = 0.0;
            
            for (var i = 0; i < srcCount; i++)
            {
                var index = GetSourcePrice(i, 0);
                var indexNext = i < srcCount ? GetSourcePrice(i + 1, 0) : index;
                // если находимся в зоне перекупленности / перепроданности,
                // если след. точка ниже (выше для перепроданности),
                // если текущая точка выше последнего максимума (ниже минимума...)

                var margUp = index >= MargUpper;
                var margDn = index <= MargLower;

                if ((margUp && indexNext < index && 
                     index > (lastPeak.HasValue ? lastPeak.Value : double.MinValue)) ||
                    (margDn && indexNext > index && index < (lastPeak.HasValue ? lastPeak.Value : double.MaxValue)))
                {// + или - экстремум 
                    // нарисовать палочку от peakPrice вверх или вниз                    
                    var seriesPeakBar = SeriesSources.Count == 2
                                            ? SeriesSources[1]
                                            : SeriesSources[0];
                    if (seriesPeakBar is StockSeries)
                    {
                        var stockSrc = (StockSeries)seriesPeakBar;
                        peakPrice = i < 0 || i >= stockSrc.Data.Count
                                        ? 0 
                                        : ExtremumDirection == ExtremumBarDirection.СверхуВниз
                                              ? (double)stockSrc.Data.Candles[i].low :
                                                                                         (double)stockSrc.Data.Candles[i].high;
                    }
                    if (seriesPeakBar is LineSeries)
                    {
                        var lineSrc = (LineSeries)seriesPeakBar;
                        peakPrice = i < 0 || i >= lineSrc.Data.Count
                                        ? 0 : lineSrc.Data[i];
                    }

                    var line = new TrendLine();
                    line.linePoints.Add(new PointD(i, peakPrice));
                    line.linePoints.Add(ExtremumDirection == ExtremumBarDirection.СверхуВниз
                                            ? new PointD(i, double.MinValue)
                                            : new PointD(i, double.MaxValue));

                    seriesExtr.data.Add(line);
                    // нарисовать столбик от цены вверх или вниз
                    lastPeak = index;
                    continue;
                }
                if (!margUp && margDn == false)
                {// конец отсчета экстремума
                    lastPeak = null;
                    continue;
                }                
            }
        }

        private void BuildClassicExtremums(int srcCount)
        {
            var seriesPeakBar = SeriesSources.Count == 2
                                            ? SeriesSources[1]
                                            : SeriesSources[0];
            
            for (var i = ClassicExtremumBar; i < srcCount - ClassicExtremumBar; i++)
            {
                bool isMin = true, isMax = true;
                var priceExt = GetSourcePrice(i, 0);
                for (var j = i - ClassicExtremumBar; j <= i + ClassicExtremumBar; j++)
                {
                    if (j == i) continue;
                    var price = GetSourcePrice(j, 0);
                    if (price > priceExt) isMax = false;
                    if (price < priceExt) isMin = false;
                    if (!isMax && !isMin) break;
                }
                if (isMin || isMax)
                {
                    double peakPrice = 0;
                    if (seriesPeakBar is StockSeries)
                    {
                        var stockSrc = (StockSeries)seriesPeakBar;
                        peakPrice = i < 0 || i >= stockSrc.Data.Count
                                        ? 0
                                        : ExtremumDirection == ExtremumBarDirection.СверхуВниз
                                              ? (double) stockSrc.Data.Candles[i].low
                                              : (double) stockSrc.Data.Candles[i].high;
                    }
                    if (seriesPeakBar is LineSeries)
                    {
                        var lineSrc = (LineSeries)seriesPeakBar;
                        peakPrice = i < 0 || i >= lineSrc.Data.Count
                                        ? 0 : lineSrc.Data[i];
                    }

                    var line = new TrendLine();
                    line.linePoints.Add(new PointD(i, peakPrice));
                    line.linePoints.Add(ExtremumDirection == ExtremumBarDirection.СверхуВниз
                                            ? new PointD(i, double.MinValue)
                                            : new PointD(i, double.MaxValue));

                    seriesExtr.data.Add(line);
                }
            }
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            seriesMargins = new RegionSeries(Localizer.GetString("TitleExtremaBordes")) {CustomAlphaChannel = true};
            seriesExtr = new TrendLineSeries(Localizer.GetString("TitleExtrema"));
            SeriesResult = new List<Series.Series> { seriesMargins, seriesExtr };
            EntitleIndicator();
        }

        public void Remove()
        {
            seriesExtr.data.Clear();
            seriesMargins.data.Clear();
        }

        public void AcceptSettings()
        {
        }

        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (newCandles == null) return;
            if (newCandles.Count == 0) return;
        }

        public string GetHint(int x, int y, double index, double price, int tolerance)
        {
            return string.Empty;
        }

        public List<IChartInteractiveObject> GetObjectsUnderCursor(int screenX, int screenY, int tolerance)
        {
            return new List<IChartInteractiveObject>();
        }
    }
}

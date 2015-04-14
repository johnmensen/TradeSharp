using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Candlechart.Chart;
using Candlechart.Core;
using Candlechart.Series;
using System.Linq;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    [LocalizedDisplayName("TitleERatio")]
    [LocalizedCategory("TitleTradingIndicatorsShort")]
    [TypeConverter(typeof(PropertySorter))]
    public class IndicatorEdgeRatio : BaseChartIndicator, IChartIndicator
    {
        #region members

        [Browsable(false)]
        public override string Name { get { return Localizer.GetString("TitleERatio"); } }

        private int period = 100;
        [LocalizedDisplayName("TitlePeriod")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageERatioPeriodDescription")]
        public int Period { get { return period; } set { period = value; } }

        public enum SourceType
        {
            ИндикаторОрдеров = 0, МаркерыСделок = 1, КоментарииРоботов = 2
        }

        [LocalizedDisplayName("TitleDataSource")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageERatioDataSourceDescription")]
        public SourceType IndiSourceType { get; set; }

        private LineSeries seriesRatio;

        #endregion

        public override BaseChartIndicator Copy()
        {
            var cci = new IndicatorEdgeRatio();
            Copy(cci);
            return cci;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var indiEdge = (IndicatorEdgeRatio)indi;
            CopyBaseSettings(indiEdge);
            indiEdge.seriesRatio = seriesRatio;            
            indiEdge.Period = Period;
            indiEdge.IndiSourceType = IndiSourceType;
        }

        public void BuildSeries(ChartControl chart)
        {
            if (DrawPane != owner.StockPane) 
                DrawPane.Title = string.Format("{0} [{1}]", UniqueName, Period);
            seriesRatio.Data.Clear();
            BuildERatioMarker();
        }

        private List<Cortege3<int, int, float>> GetEntryPoints()
        {
            if (IndiSourceType == SourceType.МаркерыСделок)
                return owner.Owner.seriesMarker.data.Where(m => m.MarkerType == DealMarker.DealMarkerType.Вход).Select(
                    m => new Cortege3<int, int, float>((int)m.candleIndex, (int)m.Side, (float)m.Price)).ToList();
            
            if (IndiSourceType == SourceType.КоментарииРоботов)
                return owner.Owner.seriesAsteriks.data.Where(a => a.Shape == AsteriskTooltip.ShapeType.СтрелкаВверх ||
                                                           a.Shape == AsteriskTooltip.ShapeType.СтрелкаВниз).Select(
                                                               a => new Cortege3<int, int, float>(a.CandleIndex,
                                                                                           a.Shape ==
                                                                                           AsteriskTooltip.ShapeType.
                                                                                               СтрелкаВверх
                                                                                               ? 1
                                                                                               : -1, a.Price)).ToList();
            //if (IndiSourceType == SourceType.ИндикаторОрдеров)
            var indi = owner.Owner.indicators.FirstOrDefault(i => i is IndicatorOrders);
            return indi == null ? new List<Cortege3<int, int, float>>() : ((IndicatorOrders) indi).GetEntryPoints();
        }

        private void BuildERatioMarker()
        {
            var enters = GetEntryPoints();
            if (enters.Count == 0) return;
            // точка отсчета
            seriesRatio.Data.Add(1);
            var candles = owner.StockSeries.Data.Candles;
            
            var arrayPro = new double[Period];
            var arrayCons = new double[Period];

            foreach (var marker in enters)
            {
                var candleIndex = marker.a;
                if (candleIndex < 0 || candleIndex >= candles.Count) continue;
                var candle = candles[candleIndex];

                var startIndex = candleIndex + 1;
                var startPrice = (double) candle.close;
                double maxDown = 0, maxUp = 0;

                // если цена маркера ближе к цене открытия - 
                // - полагаем, что маркер поставлен на open свечки
                var deltaOpen = Math.Abs(candle.open - marker.c);
                var deltaClose = Math.Abs(candle.close - marker.c);
                if (deltaOpen < deltaClose)
                {
                    startPrice = (double) candle.open;
                    startIndex--;
                }

                var endIndex = Math.Min(startIndex + Period, candles.Count - 1);
                var arrayRatioIndex = 0;
                for (; startIndex < endIndex; startIndex++)
                {
                    var deltaMax = (double)candles[startIndex].high - startPrice;
                    var deltaMin = startPrice - (double)candles[startIndex].low;
                    if (deltaMax > maxUp) maxUp = deltaMax;
                    if (deltaMin > maxDown) maxDown = deltaMin;

                    arrayPro[arrayRatioIndex] += marker.b > 0 ? maxUp : maxDown;
                    arrayCons[arrayRatioIndex++] += marker.b > 0 ? maxDown : maxUp;
                }
            }
            // делим Pro на Cons
            for (var  i = 0; i < Period; i++)
            {
                seriesRatio.Data.Add(arrayCons[i] == 0 ? 1 : arrayPro[i] / arrayCons[i]);
            }
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            seriesRatio = new LineSeries("E-Ratio")
            {
                LineColor = Color.Blue,
                ShiftX = 1,
                MarkerSpanPoints = 2,
                MinPixelsBetweenMarkers = 10
            };
            SeriesResult = new List<Series.Series> { seriesRatio };
            // инициализируем индикатор
            EntitleIndicator();
        }

        public void Remove()
        {            
        }

        public void AcceptSettings()
        {            
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
    }
}

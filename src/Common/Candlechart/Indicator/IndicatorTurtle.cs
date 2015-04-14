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
    [LocalizedDisplayName("TitleTurtleLikeFormations")]
    [LocalizedCategory("TitleGraphicsAnalysisShort")]
    [TypeConverter(typeof(PropertySorter))]
    public class IndicatorTurtle : BaseChartIndicator, IChartIndicator
    {
        public override BaseChartIndicator Copy()
        {
            var turtle = new IndicatorTurtle();
            Copy(turtle);
            return turtle;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var turtle = (IndicatorTurtle) indi;
            CopyBaseSettings(turtle);
            turtle.Period = Period;
            turtle.ClLine = ClLine;
            turtle.series = series;
            turtle.Shape = Shape;
        }

        #region Members

        [Browsable(false)]
        public override string Name { get { return Localizer.GetString("TitleTurtleLikeFormations"); } }
        
        [LocalizedDisplayName("TitleCreateDrawingPanel")]
        [LocalizedCategory("TitleMain")]
        [Description("Создавать свою панель отрисовки")]
        [Browsable(false)]
        public override bool CreateOwnPanel { get; set; }

        private int period = 8;
        [LocalizedDisplayName("TitlePeriod")]
        [Description("Период")]
        [LocalizedCategory("TitleMain")]
        public int Period
        {
            get { return period; }
            set { period = value; }
        }

        private Color clLine = Color.Brown;
        [LocalizedDisplayName("TitleColor")]
        [Description("Цвет")]
        [LocalizedCategory("TitleMain")]
        public Color ClLine
        {
            get { return clLine; }
            set { clLine = value; }
        }

        public enum IndicatorShape { Черепаха = 0, Пэкмен }
        [LocalizedDisplayName("TitleFigure")]
        [Description("Отображаемая фигурка")]
        [LocalizedCategory("TitleVisuals")]
        public IndicatorShape Shape { get; set; }

        private TrendLineSeries series = new TrendLineSeries(Localizer.GetString("TitleTurtles"));

        public IndicatorTurtle()
        {
            CreateOwnPanel = false;
        }

        #endregion

        public void BuildSeries(ChartControl chart)
        {            
            series.data.Clear();
            //var candles = chart.StockSeries.Data.Candles;

            if (SeriesSources[0].GetType() == typeof(CandlestickSeries))
            {
                var candles = ((StockSeries) SeriesSources[0]).Data.Candles;
                if (candles.Count < period) return;

                for (var i = period; i < candles.Count; i += period)
                {
                    var turtle = new TrendLine
                                     {
                                         LineColor = clLine,
                                         ShapeAlpha = 128,
                                         ShapeFillColor = Color.Green,
                                         LineStyle = Shape == IndicatorShape.Черепаха ?
                                            TrendLine.TrendLineStyle.Черепаха : TrendLine.TrendLineStyle.Пэкмен
                                     };
                    turtle.AddPoint(i - period, (double) candles[i - period].open);
                    turtle.AddPoint(i, (double) candles[i].close);
                    series.data.Add(turtle);
                }
            }
            else
            {
                if (SeriesSources[0].GetType()== typeof(LineSeries))
                {
                    var candles = ((LineSeries)SeriesSources[0]).Data;
                    if (candles.Count < period) return;

                    for (var i = period; i < candles.Count; i += period)
                    {
                        var turtle = new TrendLine
                        {
                            LineColor = clLine,
                            ShapeAlpha = 128,
                            ShapeFillColor = Color.Green,
                            LineStyle = Shape == IndicatorShape.Черепаха ? 
                                TrendLine.TrendLineStyle.Черепаха : TrendLine.TrendLineStyle.Пэкмен
                        };
                        turtle.AddPoint(i - period, (double)candles[i - period]);
                        turtle.AddPoint(i, (double)candles[i]);
                        series.data.Add(turtle);
                    }   
                }
            }
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            SeriesResult = new List<Series.Series>{series};
            EntitleIndicator();
        }

        public void Remove()
        {
            series.data.Clear();
        }

        public void AcceptSettings()
        {
        }

        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (newCandles != null)
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
    }
}

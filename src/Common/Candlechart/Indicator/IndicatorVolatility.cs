using System;
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
    [LocalizedDisplayName("TitlePriceSeriesVolatility")]
    [LocalizedCategory("TitleOscillator")]
    [TypeConverter(typeof(PropertySorter))]
    public class IndicatorVolatility : BaseChartIndicator, IChartIndicator
    {
        public override BaseChartIndicator Copy()
        {
            var iv = new IndicatorVolatility();
            Copy(iv);
            return iv;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var iv = (IndicatorVolatility)indi;
            CopyBaseSettings(iv);

            ColorLine = iv.ColorLine;
            Period = iv.Period;
        }

        public override string Name
        {
            get { return "Volatility"; }
        }

        private Color colorLine = Color.Teal;
        [LocalizedDisplayName("TitleColor")]
        [Description("Цвет кривой")]
        public Color ColorLine
        {
            get { return colorLine; }
            set { colorLine = value; }
        }

        private int period = 50;
        [LocalizedDisplayName("TitlePeriod")]
        [Description("Период исторических данных для вычисления волатильности")]
        public int Period
        {
            get { return period; }
            set { period = value; }
        }

        private LineSeries series;

        public void BuildSeries(ChartControl chart)
        {
            series.Data.Clear();
            
            var sources = (StockSeries)SeriesSources[0];
            var candles = sources.Data.Candles;
            if (candles.Count == 0 || Period == 0 || candles.Count <= Period) return;
                        
            var periodRoot = Math.Sqrt(period);
            double totalPath = 0, totalPathPredicted = 0;
            var deltas = new float[period];

            for (var j = period + 1; j < candles.Count; j++)
            {
                // вычисляю изменение цены
                //var averageChange = (candles[j].close - candles[j - Period].close) / Period;
                //float overall = 0;
                //dispers = 0;
                //for (var i = j - period; i <= j; i++)
                //{
                //    overall += Math.Abs((candles[i].close - candles[i - 1].close) - averageChange);                    
                //}
                float avg = 0;
                for (var i = 0; i < period; i++)
                {
                    var delta = (candles[j - i].close - candles[j - i - 1].close) / candles[j - i - 1].close;
                    deltas[i] = delta;
                    avg += avg;
                }
                avg /= period;

                double overall = 0;
                for (var i = 0; i < period; i++)
                    overall += (deltas[i] - avg) * (deltas[i] - avg);
                overall /= period;

                var sigma = Math.Sqrt(overall);
                series.Data.Add(sigma * 100);

                var priceDelta = Math.Abs(candles[j].close - candles[j - period].close);
                var predictedDelta = periodRoot * sigma;
                totalPath += priceDelta;
                totalPathPredicted += predictedDelta;
            }

            var countSteps = candles.Count - period - 1;
            if (countSteps > 0)
            {
                totalPath /= countSteps;
                totalPathPredicted /= countSteps;
                TradeSharp.Util.Logger.InfoFormat("path is: {0:f5}, predicted value: {1:f5}", 
                    totalPath, totalPathPredicted);
            }
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            series = new LineSeries("price changes")
            {
                LineColor = ColorLine,
                ShiftX = Period + 1
            };
            SeriesResult = new List<Series.Series> { series };
            EntitleIndicator();
        }

        public void Remove()
        {
            if (series != null) series.Data.Clear();   
        }

        public void AcceptSettings()
        {
            series.LineColor = colorLine;
            series.Transparent = true;
            if (CreateOwnPanel)
            {
                SeriesResult = new List<Series.Series> { series };
            }
            if (DrawPane != null && DrawPane != owner.StockPane)
                DrawPane.Title = UniqueName;
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

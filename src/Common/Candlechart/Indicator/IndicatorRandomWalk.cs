using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;
using Candlechart.Chart;
using Candlechart.ChartMath;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    class CustomXmlSerializationColorArrayAttribute : CustomXmlSerializationAttribute
    {
        public override void SerializeProperty(object proValue, XmlNode parent)
        {
            var colorAttribute = new StringBuilder();
            var value = proValue as Color[];
            if (value != null)
                foreach (var color in value)
                {
                    colorAttribute.Append(string.Format("{0},{1},{2},{3};", color.A, color.R, color.G, color.B));
                }
            parent.Attributes.Append(parent.OwnerDocument.CreateAttribute("colors")).Value = colorAttribute.ToString().TrimEnd(';'); 
        }

        public override object DeserializeProperty(XmlNode parent)
        {
            var result = new List<Color>();
            if (parent.Attributes != null && parent.Attributes["colors"] != null)
                foreach (var color in parent.Attributes["colors"].Value.Split(';'))
                {
                    var colorComponent = color.Split(',');
                    result.Add(Color.FromArgb(Convert.ToInt32(colorComponent[0]), 
                                              Convert.ToInt32(colorComponent[1]), 
                                              Convert.ToInt32(colorComponent[2]), 
                                              Convert.ToInt32(colorComponent[3])));
                }
            return result.ToArray();
        }
    }

    [LocalizedDisplayName("TitleRandomWalkModelForecastShort")]
    [LocalizedCategory("TitleTrending")]
    [TypeConverter(typeof(PropertySorter))]
    public class IndicatorRandomWalk : BaseChartIndicator, IChartIndicator
    {
        #region Свойства

        [Browsable(false)]
        public override string Name { get { return Localizer.GetString("TitleRandomWalkModelForecastShort"); } }

        [LocalizedDisplayName("TitleCreateDrawingPanel")]
        [LocalizedCategory("TitleMain")]
        [Description("Создавать свою панель отрисовки")]
        public override bool CreateOwnPanel { get; set; }

        private int period = 200;
        [LocalizedDisplayName("TitleCandleCount")]
        [Description("Количесво свечей для расчета волатильности")]
        [LocalizedCategory("TitleMain")]
        public int Period
        {
            get { return period; }
            set { period = value; }
        }

        private int[] percentile = { 90, 70 };
        [LocalizedDisplayName("TitlePercentiles")]
        [Description("Перцентили")]
        [LocalizedCategory("TitleMain")]
        public string Percentile
        {
            get
            {
                return string.Join(" ", percentile);
            }
            set
            {
                var percs = value.ToIntArrayUniform();
                if (percs.Length == 0) return;
                if (percs.Any(p => p <= 0 || p >= 100)) return;
                percentile = percs;

                if (SeriesSources != null) Calculation(SeriesSources[0]);
            }
        }

        private Color[] clLine = { Color.DarkBlue, Color.DarkGreen, Color.DarkOrange };
        [LocalizedDisplayName("TitleColors")]
        [Description("Цвета линий")]
        [LocalizedCategory("TitleVisuals")]
        [CustomXmlSerializationColorArray]
        public Color[] ClLine
        {
            get { return clLine; }
            set { clLine = value; }
        }

        private int forwardSteps = 10;
        [LocalizedDisplayName("TitleCandleCountForward")]
        [Description("На сколько свеч строить вперед")]
        [LocalizedCategory("TitleMain")]
        public int ForwardSteps
        {
            get { return forwardSteps; }
            set { forwardSteps = value; }
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

        [LocalizedDisplayName("TitleOffsetInBars")]
        [Description("Смещение вправо (>0) или влево (<0), баров")]
        [LocalizedCategory("TitleMain")]
        public int ShiftX { get; set; }
        
        #endregion

        #region Поля

        /// <summary>
        /// Массив хранит значения всех delta для обоих процентилей
        /// </summary>
        private double[,] percentileDeltaValues;

        /// <summary>
        /// Последняя актуальная цена крайней свечки
        /// </summary>
        private float lastCandlestickPrice;
        private TrendLineSeries seriesUp;
        private TrendLineSeries seriesDown;

        #endregion

        public IndicatorRandomWalk()
        {
            CreateOwnPanel = false;
        }

        public IndicatorRandomWalk(IndicatorRandomWalk indi)
        {
        }

        public override BaseChartIndicator Copy()
        {
            var indi = new IndicatorRandomWalk();
            Copy(indi);
            return indi;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var randomWalk = (IndicatorRandomWalk)indi;
            CopyBaseSettings(randomWalk);
            randomWalk.Period = Period;
            randomWalk.ClLine = ClLine;
            randomWalk.ShiftX = ShiftX;
            randomWalk.LineWidth = LineWidth;
            randomWalk.percentile = percentile.ToArray();
            randomWalk.forwardSteps = forwardSteps;

        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            seriesUp =
                new TrendLineSeries(string.Format("{0} - {1}", Localizer.GetString("TitleRandomWalkModelForecastShort"),
                                                  Localizer.GetString("TitleUpperLimitSmall")));
            seriesDown =
                new TrendLineSeries(string.Format("{0} - {1}", Localizer.GetString("TitleRandomWalkModelForecastShort"),
                                                  Localizer.GetString("TitleLowerLimitSmall")));
            SeriesResult = new List<Series.Series> { seriesUp, seriesDown };

            seriesUp.LineWidth = (float)lineWidth;
            seriesDown.LineWidth = (float)lineWidth;
            

            EntitleIndicator();
        }

        public void Remove()
        {
            if (seriesUp == null || seriesDown == null) return;
            seriesUp.data.Clear();
            seriesDown.data.Clear();
        }


        public void AcceptSettings()
        {
            seriesUp.LineWidth = (float)lineWidth;
            seriesDown.LineWidth = (float)lineWidth;

            if (CreateOwnPanel)
            {
                SeriesResult = new List<Series.Series> { seriesUp, seriesDown };
            }
            
            if (DrawPane != null && DrawPane != owner.StockPane)
                DrawPane.Title = string.Format("{0} [{1}]", UniqueName, Period);
        }

        /// <summary>
        /// Вызывается при изменении параметоров крайней свечи или при добавлении новой
        /// </summary>
        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (newCandles.Count == 0)
            {
                UpdateRandomWalkLine(SeriesSources[0]);
            }
            else
            {
                Calculation(SeriesSources[0]);
            }
        }


        public void BuildSeries(ChartControl chart) 
        {
             Calculation(SeriesSources[0]);            
        }


        /// <summary>
        /// Расчитываем дисперсию и дельты для каждой процентили.
        /// Если количество свечей не изменяется (а изменяется оно очень редко), то "disper" и "delta" персчитывать не нужно. Они зависят только от 
        /// количества свечей.
        /// </summary>
        private void Calculation(Series.Series source)
        {
            var disper = GetMathExpected(source);

            percentileDeltaValues = new double[percentile.Length, forwardSteps];
            for (var i = 0; i < percentile.Length; i++)
                for (var j = 0; j < forwardSteps; j++)
                    percentileDeltaValues[i, j] = QNorm(percentile[i] / 100d, 0, Math.Sqrt(j + 1) * disper, true, false);

            MakeRandomWalkLine(source);
        }


        /// <summary>
        /// Добавление точек в серии "seriesUp" и "seriesDown" для прорисовки линии
        /// </summary>
        private void MakeRandomWalkLine(Series.Series source)
        {
            if (source is CandlestickSeries == false &&
                source is LineSeries == false) return;

            lastCandlestickPrice = source.DataCount > 0
                                       ? (source is CandlestickSeries
                                              ? ((CandlestickSeries) source).Data.Candles[source.DataCount - 1].close
                                              : (((LineSeries) source).GetPrice(source.DataCount - 1) ?? 0))
                                       : 0;

            seriesUp.data.Clear();
            seriesDown.data.Clear();

            var colorIndex = 0;
            var signs = new Dictionary<int, TrendLineSeries> { {1, seriesUp}, { -1, seriesDown }};


            for (var i = 0; i < percentileDeltaValues.GetLength(0); i++)  // перебираем все процентили
            {
                var color = clLine[colorIndex++];  // Задаём цвет
                if (colorIndex >= clLine.Length) colorIndex = 0;

                for (var j = 0; j < percentileDeltaValues.GetLength(1) ; j++)  // перебираем все дельты на порцентили
                {
                    var x = source.DataCount + j - 1;

                    foreach (var signSeries in signs)
                    {
                        var sign = signSeries.Key;
                        var series = signSeries.Value;

                        series.data.Add(MakeTrendShelf(x, lastCandlestickPrice + sign * percentileDeltaValues[i, j], color, 1));
                        if (j != 0)
                            series.data.Add(MakeTrendShelf(x,
                                                             lastCandlestickPrice + sign * percentileDeltaValues[i, j - 1],
                                                             color, dy: sign * percentileDeltaValues[i, j] - sign * percentileDeltaValues[i, j - 1]));
                    }
                }
            }
        }

        /// <summary>
        /// Обновление положения серий "seriesUp" и "seriesDown" (смещение по y). 
        /// </summary>
        private void UpdateRandomWalkLine(Series.Series source)
        {
            if (source is CandlestickSeries == false &&
                source is LineSeries == false) return;

            if (seriesUp == null) return;
            if (seriesUp.data.Count == 0)
            {
                BuildSeries(owner);
                return;
            }

            var newCandlestickPrice = source is CandlestickSeries
                           ? ((CandlestickSeries)source).Data.Candles[source.DataCount - 1].close
                           : (((LineSeries)source).GetPrice(source.DataCount - 1) ?? 0);
            var deltaPrice =  newCandlestickPrice - lastCandlestickPrice;

            for (var i = 0; i < seriesUp.data.Count; i++)
            {
                for (var j = 0; j < seriesUp.data[i].linePoints.Count; j++)
                {
                    seriesUp.data[i].linePoints[j] = new PointD(seriesUp.data[i].linePoints[j].X,
                                                                seriesUp.data[i].linePoints[j].Y + deltaPrice);

                    seriesDown.data[i].linePoints[j] = new PointD(seriesDown.data[i].linePoints[j].X,
                                                                  seriesDown.data[i].linePoints[j].Y + deltaPrice);
                }               
            }
            lastCandlestickPrice = newCandlestickPrice;
        }

        /// <summary>   
        /// Вспомогательный метод прорисовки отрезка 
        /// </summary>
        /// <returns></returns>
        private TrendLine MakeTrendShelf(double x, double y, Color color, double dx = 0, double dy = 0)
        {
            var line = new TrendLine { LineStyle = TrendLine.TrendLineStyle.Отрезок, LineColor = color };

            line.linePoints.Add(new PointD(x + ShiftX, y));
            line.linePoints.Add(new PointD(x + ShiftX + dx, y + dy));

            return line;
        }

        /// <summary>
        /// подсчитывает СКО
        /// </summary>
        /// <returns></returns>
        private double GetMathExpected(Series.Series source)
        {
            var deltaCandlesSumm = 0d;

            var start = source.DataCount - period;
            if (start < 1) start = 1;
            var count = 0;
            for (var j = start; j < source.DataCount; j++, count++)
            {
                var delta =
                    source is CandlestickSeries
                        ? ((CandlestickSeries)source).Data.Candles[j].close -
                          ((CandlestickSeries)source).Data.Candles[j].open
                        : (((LineSeries)source).GetPrice(j) - ((LineSeries)source).GetPrice(j - 1)) ?? 0;

                deltaCandlesSumm += (delta * delta);
            }


            return count == 0 ? 0 : Math.Sqrt(deltaCandlesSumm / count);
        }

        #region Вспомогательные расчётные методы
        public static double QNorm(double p, double mu, double sigma, bool lowerTail, bool logP)
        {
            if (double.IsNaN(p) || double.IsNaN(mu) || double.IsNaN(sigma)) return (p + mu + sigma);
            double ans;
            bool isBoundaryCase = Rqp01Boundaries(p, double.NegativeInfinity, double.PositiveInfinity, lowerTail, logP, out ans);
            if (isBoundaryCase) return (ans);
            if (sigma < 0) return (double.NaN);
            if (sigma == 0) return (mu);

            double p_ = RdtQIv(p, lowerTail, logP);
            double q = p_ - 0.5;
            double r, val;

            if (Math.Abs(q) <= 0.425)  // 0.075 <= p <= 0.925
            {
                r = .180625 - q * q;
                val = q * (((((((r * 2509.0809287301226727 +
                           33430.575583588128105) * r + 67265.770927008700853) * r +
                         45921.953931549871457) * r + 13731.693765509461125) * r +
                       1971.5909503065514427) * r + 133.14166789178437745) * r +
                     3.387132872796366608)
                / (((((((r * 5226.495278852854561 +
                         28729.085735721942674) * r + 39307.89580009271061) * r +
                       21213.794301586595867) * r + 5394.1960214247511077) * r +
                     687.1870074920579083) * r + 42.313330701600911252) * r + 1.0);
            }
            else
            {
                r = q > 0 ? RdtCIv(p, lowerTail, logP) : p_;
                r = Math.Sqrt(-((logP && ((lowerTail && q <= 0) || (!lowerTail && q > 0))) ? p : Math.Log(r)));

                if (r <= 5)              // <==> min(p,1-p) >= exp(-25) ~= 1.3888e-11
                {
                    r -= 1.6;
                    val = (((((((r * 7.7454501427834140764e-4 +
                            .0227238449892691845833) * r + .24178072517745061177) *
                          r + 1.27045825245236838258) * r +
                         3.64784832476320460504) * r + 5.7694972214606914055) *
                       r + 4.6303378461565452959) * r +
                      1.42343711074968357734)
                     / (((((((r *
                              1.05075007164441684324e-9 + 5.475938084995344946e-4) *
                             r + .0151986665636164571966) * r +
                            .14810397642748007459) * r + .68976733498510000455) *
                          r + 1.6763848301838038494) * r +
                         2.05319162663775882187) * r + 1.0);
                }
                else                     // very close to  0 or 1 
                {
                    r -= 5.0;
                    val = (((((((r * 2.01033439929228813265e-7 +
                            2.71155556874348757815e-5) * r +
                           .0012426609473880784386) * r + .026532189526576123093) *
                         r + .29656057182850489123) * r +
                        1.7848265399172913358) * r + 5.4637849111641143699) *
                      r + 6.6579046435011037772)
                     / (((((((r *
                              2.04426310338993978564e-15 + 1.4215117583164458887e-7) *
                             r + 1.8463183175100546818e-5) * r +
                            7.868691311456132591e-4) * r + .0148753612908506148525)
                          * r + .13692988092273580531) * r +
                         .59983220655588793769) * r + 1.0);
                }
                if (q < 0.0) val = -val;
            }

            return (mu + sigma * val);
        }
        private static bool Rqp01Boundaries(double p, double left, double right, bool lowerTail, bool logP, out double ans)
        {
            if (logP)
            {
                if (p > 0.0)
                {
                    ans = double.NaN;
                    return (true);
                }
                if (p == 0.0)
                {
                    ans = lowerTail ? right : left;
                    return (true);
                }
                if (p == double.NegativeInfinity)
                {
                    ans = lowerTail ? left : right;
                    return (true);
                }
            }
            else
            {
                if (p < 0.0 || p > 1.0)
                {
                    ans = double.NaN;
                    return (true);
                }
                if (p == 0.0)
                {
                    ans = lowerTail ? left : right;
                    return (true);
                }
                if (p == 1.0)
                {
                    ans = lowerTail ? right : left;
                    return (true);
                }
            }
            ans = double.NaN;
            return (false);
        }
        private static double RdtQIv(double p, bool lowerTail, bool logP)
        {
            return (logP ? (lowerTail ? Math.Exp(p) : -ExpM1(p)) : RdLval(p, lowerTail));
        }
        private static double RdtCIv(double p, bool lowerTail, bool logP)
        {
            return (logP ? (lowerTail ? -ExpM1(p) : Math.Exp(p)) : RdCval(p, lowerTail));
        }
        private static double RdLval(double p, bool lowerTail)
        {
            return lowerTail ? p : 0.5 - p + 0.5;
        }
        private static double RdCval(double p, bool lowerTail)
        {
            return lowerTail ? 0.5 - p + 0.5 : p;
        }
        private static double ExpM1(double x)
        {
            if (Math.Abs(x) < 1e-5)
                return x + 0.5 * x * x;
            else
                return Math.Exp(x) - 1.0;
        }
        #endregion

        /// <summary>
        /// на входе - экранные координаты
        /// </summary>       
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Candlechart.Chart;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    [LocalizedDisplayName("TitleSigmaAnalysis")]
    [LocalizedCategory("TitleAnalytics")]
    [TypeConverter(typeof(PropertySorter))]
    public class IndicatorSigma : BaseChartIndicator, IChartIndicator
    {
        [Browsable(false)]
        public override string Name { get { return Localizer.GetString("TitleSigmaAnalysis"); } }

        public override BaseChartIndicator Copy()
        {
            var ell = new IndicatorSigma();
            Copy(ell);
            return ell;
        }

        //private readonly SeriesComment seriesComment = new SeriesComment("Сигма-анализ");

        public override void Copy(BaseChartIndicator indi)
        {
            var ell = (IndicatorSigma)indi;
            CopyBaseSettings(ell);
        }

        [LocalizedDisplayName("TitleCreateDrawingPanel")]
        [LocalizedCategory("TitleMain")]
        [Description("Создавать свою панель отрисовки")]
        public override bool CreateOwnPanel { get { return false; } set { } }

        private const int CommentMagic = 100500;

        private static int nextAngle = 120;

        public IndicatorSigma()
        {
        }

        public void BuildSeries(ChartControl chart)
        {
            var commentText = GetStatText();
            if (string.IsNullOrEmpty(commentText)) return;
            var candles = chart.StockSeries.Data.Candles;
            
            // добавить комментарий с текстом
            var series = chart.Owner.seriesComment;
            var indiTitle = MakeCommentTitle();
            var comment = series.data.FirstOrDefault(c => c.Magic == CommentMagic && c.Text.StartsWith(indiTitle));
            if (comment != null)
            {
                comment.Text = commentText;
                return;
            }

            // таки создать новый комментарий
            comment = new ChartComment
                {
                    ArrowAngle = nextAngle,
                    ArrowLength = 50,
                    Magic = CommentMagic,
                    Text = commentText,
                    Owner = series,
                    PivotIndex = candles.Count - 1,
                    PivotPrice = candles[candles.Count - 1].open,
                };
            nextAngle += 45;
            series.data.Add(comment);
        }

        private string MakeCommentTitle()
        {
            return "Ряд " + SeriesSources[0].Owner.Title;
        }

        private string GetStatText()
        {
            if (SeriesSources.Count < 0) return "";
            if (SeriesSources[0].DataCount < 100) return "";

            float[] rawData = null;
            if (SeriesSources[0].GetType() == typeof (StockSeries))
            {
                var candles = ((StockSeries) SeriesSources[0]).Data.Candles;
                rawData = candles.Select(c => c.close - c.open).ToArray();
            }
            else if (SeriesSources[0] is IPriceQuerySeries)
            {
                var seriesPrice = (IPriceQuerySeries) SeriesSources[0];
                rawData = new float[SeriesSources[0].DataCount - 1];
                var prevVal = seriesPrice.GetPrice(0);
                for (var i = 1; i < SeriesSources[0].DataCount; i++)
                {
                    var price = seriesPrice.GetPrice(i);
                    rawData[i - 1] = (price - prevVal) ?? 0;
                    prevVal = price;
                } 
            }
            if (rawData == null) return "";

            // по временному ряду определить СКО
            var avg = rawData.Average();
            var sko = Math.Sqrt(rawData.Sum(r => (r - avg)*(r - avg))/rawData.Length);

            // посчитать вероятность K сигма
            var kArray = new[] {1.0, 1.5, 2, 2.5, 3};
            var indiTitle = MakeCommentTitle();
            var text = indiTitle + Environment.NewLine + "sigma: " + sko.ToStringUniform(5) + 
                Environment.NewLine + string.Join(Environment.NewLine, kArray.Select(k =>
                       string.Format("P[{0:f2}]: {1:f4}% / {2:f4}%", k, CalcProbKSigma(rawData, sko, k), CalcProbKSigmaNormal(sko, k))));
            return text;
        }

        private static double CalcProbKSigma(float[] rawData, double sigma, double k)
        {
            var kSigma = k * sigma;
            return 100.0 * rawData.Count(r => r > kSigma) / rawData.Length;
        }

        private static double CalcProbKSigmaNormal(double sigma, double k)
        {
            var f = ProbabilityCore.IntegralFunc(k*sigma, sigma, 0);
            return 100.0 * (1 - f);
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            SeriesResult = new List<Series.Series> { /*seriesComment*/ };
            EntitleIndicator();
        }

        public void Remove()
        {
            //seriesComment.data.Clear();
        }

        public void AcceptSettings()
        {
            if (DrawPane == null) return;
        }

        /// <summary>
        /// пересчитать индикатор для последней добавленной свечки
        /// </summary>        
        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (newCandles.Count == 0) return;
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

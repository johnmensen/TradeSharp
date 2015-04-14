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
    [LocalizedDisplayName("TitleFibonacciProjectionsShort")]
    [LocalizedCategory("TitleGraphicsAnalysisShort")]
    [TypeConverter(typeof(PropertySorter))]
    public class IndicatorFiboProjections : BaseChartIndicator, IChartIndicator
    {
        struct ExtremumPair
        {
            public Cortege2<int, float> start;
            public Cortege2<int, float> end;
        }

        #region Настройки

        [Browsable(false)]
        public override string Name { get { return Localizer.GetString("TitleProjections"); } }

        public enum PivotSource { ЗигЗаг = 0, Фрактал = 1 }

        [LocalizedDisplayName("TitleSourcePointsShort")]
        [LocalizedDescription("MessageSourcePointSelectionAlgorithmDescription")]
        [LocalizedCategory("TitleMain")]
        public PivotSource Source { get; set; }

        [LocalizedDisplayName("TitleFractalPrice")]
        [LocalizedDescription("MessageFractalPriceTypeDescription")]
        [LocalizedCategory("TitleFractals")]
        public IndicatorFractal.PriceType FractalPriceType { get; set; }

        private int fractalPeriod = 2;
        [LocalizedDisplayName("TitleFractalPeriod")]
        [LocalizedDescription("MessageFractalPeriodDescription")]
        [LocalizedCategory("TitleFractals")]
        public int FractalPeriod
        {
            get { return fractalPeriod; }
            set { fractalPeriod = value; }
        }

        private int maxBarsBtwFractals = 50;
        [LocalizedDisplayName("TitleBarCountBetweenFractals")]
        [LocalizedDescription("MessageBarCountBetweenFractalsDescription")]
        [LocalizedCategory("TitleFractals")]
        public int MaxBarsBtwFractals
        {
            get { return maxBarsBtwFractals; }
            set { maxBarsBtwFractals = value; }
        }
        
        private float thresholdPercent = 1;
        [LocalizedDisplayName("TitleThresholdInPercents")]
        [LocalizedDescription("MessageThresholdInPercentsDescription")]
        [LocalizedCategory("TitleZigzag")]
        public float ThresholdPercent
        {
            get { return thresholdPercent; }
            set { thresholdPercent = value; }
        }

        [LocalizedDisplayName("TitleCreateDrawingPanel")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageCreateDrawingPanelDescription")]
        public override bool CreateOwnPanel { get; set; }

        private int fiboThreashold = 5;
        [LocalizedDisplayName("TitleProjectionThresholdInPoints")]
        [LocalizedDescription("MessageProjectionThresholdDescription")]
        [LocalizedCategory("TitleMain")]
        public int FiboThreashold
        {
            get { return fiboThreashold; }
            set { fiboThreashold = value; }
        }

        private float[] fiboProjections = new[]
                                              {
                                                  0.236f, 0.382f,
                                                  1.618f, 2, 2.618f
                                              };
        [LocalizedDisplayName("TitleExtensionLevels")]
        [LocalizedDescription("MessageExtensionLevelsDescription")]
        [LocalizedCategory("TitleMain")]
        public string FiboProjections
        {
            get { return fiboProjections.ToStringUniform(" "); }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    fiboProjections = value.ToFloatArrayUniform();
            }
        }

        private int fiboProjLength = 30;
        [LocalizedDisplayName("TitleProjectionLength")]
        [LocalizedDescription("MessageProjectionLengthDescription")]
        [LocalizedCategory("TitleMain")]
        public int FiboProjLength
        {
            get { return fiboProjLength; }
            set { fiboProjLength = value; }
        }

        private Color lineColor = Color.Red;
        [LocalizedDisplayName("TitleLineColor")]
        [LocalizedDescription("MessageLineColorDescription")]
        [LocalizedCategory("TitleVisuals")]
        public Color LineColor
        {
            get { return lineColor; }
            set { lineColor = value; }
        }

        private bool hideFailed = true;
        [LocalizedDisplayName("TitleHideMisses")]
        [LocalizedDescription("MessageHideExtensionMissesDescription")]
        [LocalizedCategory("TitleVisuals")]
        public bool HideFailed
        {
            get { return hideFailed; }
            set { hideFailed = value; }
        }

        private bool hideProjLine = true;
        [LocalizedDisplayName("TitleHideLines")]
        [LocalizedDescription("MessageHideLinesDescription")]
        [LocalizedCategory("TitleVisuals")]
        public bool HideProjLine
        {
            get { return hideProjLine; }
            set { hideProjLine = value; }
        }

        [LocalizedDisplayName("TitleZigzagPrice")]
        [LocalizedDescription("MessageZigzagPriceDescription")]
        [LocalizedCategory("TitleZigzag")]
        public ZigZagSource ZigZagSourceType { get; set; }

        #endregion

        private ProjectionSeries seriesProj = new ProjectionSeries(Localizer.GetString("TitleProjections"));

        public void BuildSeries(ChartControl chart)
        {
            seriesProj.data.Clear();
            if (chart.StockSeries.Data.Count < 3) return;

            List<ExtremumPair> pairs;

            if (Source == PivotSource.ЗигЗаг)
            {
                // получить вершины ЗЗ
                pairs = new List<ExtremumPair>();
                var pivots = ZigZag.GetPivots(chart.StockSeries.Data.Candles, ThresholdPercent, ZigZagSourceType);
                for (var i = 1; i < pivots.Count; i++)
                    pairs.Add(new ExtremumPair {start = pivots[i - 1], end = pivots[i]});
            }
            else
            {
                // получить вершины из "фракталов"
                pairs = GetPivotsByFractals();
            }
            // построить "проекции" Фибо
            MakeFiboProj(pairs);
        }

        public override BaseChartIndicator Copy()
        {
            var fibo = new IndicatorFiboProjections();
            Copy(fibo);
            return fibo;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var fibo = (IndicatorFiboProjections)indi;
            CopyBaseSettings(fibo);
            fibo.ThresholdPercent = ThresholdPercent;
            fibo.FiboThreashold = FiboThreashold;
            fibo.FiboProjections = FiboProjections;
            fibo.FiboProjLength = FiboProjLength;
            fibo.LineColor = LineColor;
            fibo.seriesProj = seriesProj;
            fibo.ZigZagSourceType = ZigZagSourceType;
            fibo.Source = Source;
            fibo.FractalPriceType = FractalPriceType;
            fibo.FractalPeriod = FractalPeriod;
            fibo.MaxBarsBtwFractals = MaxBarsBtwFractals;
            fibo.HideFailed = HideFailed;
            fibo.HideProjLine = HideProjLine;
        }

        private List<ExtremumPair> GetPivotsByFractals()
        {
            var candles = owner.StockSeries.Data.Candles;

            // найти все "фракталы"
            // индекс свечи - знак - цена
            var fractals = new List<Cortege3<int, int, float>>();
            for (var i = fractalPeriod; i < candles.Count - fractalPeriod; i ++)
            {
                bool isMin = true, isMax = true;
                for (var j = i - fractalPeriod; j <= i + fractalPeriod; j++)
                {
                    if (j == i) continue;
                    if (GetCandlePrice(candles[i], true) <= GetCandlePrice(candles[j], true))
                        isMax = false;
                    if (GetCandlePrice(candles[i], false) >= GetCandlePrice(candles[j], false))
                        isMin = false;
                    if (!isMax && !isMin) break;
                }
                if (isMax)
                    fractals.Add(new Cortege3<int, int, float>(i, 1, GetCandlePrice(candles[i], true)));
                if (isMin)
                    fractals.Add(new Cortege3<int, int, float>(i, -1, GetCandlePrice(candles[i], false)));
            }

            // сформировать из фракталов пары
            var pairs = new List<ExtremumPair>();
            for (var i = fractals.Count - 1; i > 0; i--)
            {
                var fract = fractals[i];
                float lastExtremum = 0;
                for (var j = i - 1; j >= 0; j--)
                {
                    // от конечной точки пары до начальной ограниченное количество свеч
                    if ((fract.a - fractals[j].a) > MaxBarsBtwFractals) break;
                    if (fractals[j].b == fract.b) continue; // min -> max, max -> min
                    // если уже была точка выше - ниже..
                    if (lastExtremum != 0)
                    {
                        if (fract.b < 0 && fractals[j].c < lastExtremum) continue;
                        if (fract.b > 0 && fractals[j].c > lastExtremum) continue;
                    }
                    // добавить проекцию
                    pairs.Add(new ExtremumPair
                        {
                            start = new Cortege2<int, float>(fractals[j].a, fractals[j].c),
                            end = new Cortege2<int, float>(fract.a, fract.c),
                        });
                    // запомнить крайнюю точку
                    lastExtremum = fractals[j].c;
                }
            }

            return pairs;
        }

        private float GetCandlePrice(CandleData candle, bool needMax)
        {
            return FractalPriceType == IndicatorFractal.PriceType.Close
                       ? candle.close
                       : FractalPriceType == IndicatorFractal.PriceType.HighLow
                             ? (needMax ? candle.high : candle.low)
                             : /*Price == PriceType.OpenClose ?*/
                             (needMax ? Math.Max(candle.open, candle.close) : Math.Min(candle.open, candle.close));
        }

        private void MakeFiboProj(List<ExtremumPair> pivotPairs)
        {
            if (pivotPairs.Count == 0) return;
            var candles = owner.StockSeries.Data.Candles;
            if (candles.Count == 0) return;

            var fiboLevels = /*IsCorrection ? fiboCorrections : */fiboProjections;

            var threasholdAbs = DalSpot.Instance.GetAbsValue(owner.Symbol, (float)fiboThreashold);
            // массив "оценок попаданий"
            var levelGrades = new Dictionary<int, int>();
            for (var i = 0; i < fiboLevels.Length; i++)
                levelGrades.Add(i, 0);

            // разобрать уровни на "расширения" и коррекции
            var levelsExt = fiboLevels.Where(l => l >= 1).Select(l => l - 1).ToArray();
            var levelsCorr = fiboLevels.Where(l => l < 1).ToArray();

            foreach (var pair in pivotPairs)
            {
                var a = pair.start;
                var b = pair.end;
                // получить массив "уровней"
                var delta = a.b - b.b;
                var levels = fiboLevels.Select(proj => b.b + delta * proj).ToList();
                // пройтись по свечкам до fiboProjLength и найти самый верхний "затронутый" уровень
                var lastCandle = b.a + fiboProjLength;
                lastCandle = lastCandle > candles.Count ? candles.Count : lastCandle;
                // 1 - проекции строятся вверх, -1 - вниз
                var fiboDir = a.b > b.b ? 1 : -1;

                var reachedLevel = GetFiboLevelReached(
                    b.a + 1, lastCandle, levels, threasholdAbs, fiboDir, levelGrades);

                if (reachedLevel == -1) continue; // расширение не строится
                // проверить, не добавлен ли такой же уровень
                var spanAdded = false;
                foreach (var existedSpan in seriesProj.data)
                {
                    if (existedSpan.points.Count == 2)
                        if (existedSpan.points[0].a == a.a &&
                            existedSpan.points[0].b == a.b &&
                            existedSpan.points[1].a == b.a &&
                            existedSpan.points[1].b == b.b)
                        {
                            spanAdded = true;
                            break;
                        }
                }
                if (spanAdded) continue;
                // новая проекция
                var span = new ProjectionPair
                {
                    IsExtension = true,
                    IsCorrection = true,
                    Color = LineColor,
                    ProjectionLength = fiboProjLength,
                    levelsCorr = levelsCorr,
                    levelsProj = levelsExt,
                    HideFarParts = HideFailed,
                    Owner = seriesProj,
                    HideLine = HideProjLine
                };
                span.points.Add(new Cortege2<int, float> { a = a.a, b = a.b });
                span.points.Add(new Cortege2<int, float> { a = b.a, b = b.b });
                span.CalculateProjections();
                span.Name = string.Format("{0} {1} ({2} {3}%)", Localizer.GetString("TitleFibonacciProjectionShort"),
                                          seriesProj.data.Count + 1, Localizer.GetString("TitleZigzagShort"),
                                          ThresholdPercent);
                seriesProj.data.Add(span);
            }
            // вывести оценки в лог
            //owner.Owner.LogMessage(string.Format("ЗигЗаг: оценка уровней Фибо - {0} ",
            //    levelGrades.Sum(pair => pair.Value)));
            //var gradeBuilder = new StringBuilder();
            //for (var i = 0; i < fiboProjections.Length; i++)
            //{
            //    gradeBuilder.AppendFormat("[{0:f3} : {1}] ",
            //                              fiboProjections[i], levelGrades[i]);
            //}
            //owner.Owner.LogMessage(gradeBuilder.ToString());
        }

        private int GetFiboLevelReached(
            int startCandle,
            int lastCandle,
            List<float> levels,
            float thresholdAbs,
            int fiboDir,
            Dictionary<int, int> grades)
        {
            var reachedLevel = -1;
            var candles = owner.StockSeries.Data.Candles;

            for (var j = startCandle; j < lastCandle; j++)
            {
                var candle = candles[j];
                for (var i = levels.Count - 1; i >= 0; i--)
                {
                    var distance = fiboDir > 0
                        ? levels[i] - candle.high
                        : candle.low - levels[i];
                    if (distance <= thresholdAbs)
                        if (reachedLevel < i) reachedLevel = i;
                    // посмотреть - касание уровня или уровень пробит свечей
                    var levelGrade = 0;
                    // если свеча по обе стороны уровня -1 очко
                    // если свеча касается уровня в пределах порога чувствительности +1 очко
                    if (candle.high > (levels[i] + thresholdAbs) &&
                        candle.low < (levels[i] - thresholdAbs)) levelGrade = -1;
                    else
                    {
                        distance = Math.Min(Math.Abs(candle.high - levels[i]),
                                            Math.Abs(candle.low - levels[i]));
                        if (distance <= thresholdAbs) levelGrade = 1;
                    }
                    // добавить в таблицу оценок
                    if (levelGrade != 0)
                        grades[i] = grades[i] + levelGrade;
                }
            }
            return reachedLevel;
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            SeriesResult = new List<Series.Series> { seriesProj };
            EntitleIndicator();
        }

        public void Remove()
        {
            if (seriesProj != null) seriesProj.data.Clear();
        }

        public void AcceptSettings()
        {
        }

        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (newCandles == null) return;
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

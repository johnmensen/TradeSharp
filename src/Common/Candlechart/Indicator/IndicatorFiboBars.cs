using System.Collections.Generic;
using System.ComponentModel;
using Candlechart.Chart;
using Candlechart.ChartMath;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Util;
using System.Linq;

namespace Candlechart.Indicator
{
    [LocalizedDisplayName("TitleTurnBars")]
    [LocalizedCategory("TitleGraphicsAnalysisShort")]
    [TypeConverter(typeof(PropertySorter))]
    public class IndicatorFiboBars : BaseChartIndicator, IChartIndicator
    {
        private readonly TurnBarSeries seriesTurnBar = new TurnBarSeries(Localizer.GetString("TitleTurnBars"));
        
        public override BaseChartIndicator Copy()
        {
            var fibo = new IndicatorFiboBars();
            Copy(fibo);
            return fibo;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var fibo = (IndicatorFiboBars)indi;
            CopyBaseSettings(fibo);
            fibo.ThresholdPercent = ThresholdPercent;
            fibo.FiboFilterBars = FiboFilterBars;
            fibo.FiboLevels = FiboLevels;
            fibo.FiboMarks = FiboMarks;
            fibo.DontSumDegree = DontSumDegree;
            fibo.CalculateHits = CalculateHits;
            fibo.EstimateMarks = EstimateMarks;
            fibo.ZigZagSourceType = ZigZagSourceType;
            fibo.HideOldBars = HideOldBars;

            fibo.seriesTurnBar.barsKey.Clear();
            foreach (var bar in seriesTurnBar.barsKey)            
                fibo.seriesTurnBar.barsKey.Add(new TurnBar(bar));
            fibo.seriesTurnBar.barsTurn.Clear();
            foreach (var bar in seriesTurnBar.barsTurn)
                fibo.seriesTurnBar.barsTurn.Add(new TurnBar(bar));
        }

        [Browsable(false)]
        public override string Name { get { return Localizer.GetString("TitleTurnBars"); } }

        private float thresholdPercent = 1f;
        [LocalizedDisplayName("TitleThresholdInPercents")]
        [LocalizedDescription("MessageThresholdInPercentsDescription")]
        [LocalizedCategory("TitleMain")]
        public float ThresholdPercent
        {
            get { return thresholdPercent; }
            set { thresholdPercent = value; }
        }

        [LocalizedDisplayName("TitleCreateDrawingPanel")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageCreateDrawingPanelDescription")]
        public override bool CreateOwnPanel { get; set; }

        [LocalizedDisplayName("TitleFilter")]
        [LocalizedCategory("TitleProjections")]
        //[Description("Мин \"степень подтверждения\"")]
        public int FiboFilterBars
        {
            get { return seriesTurnBar.fibonacciTurnBarFilter; }
            set { seriesTurnBar.fibonacciTurnBarFilter = value; }
        }

        [LocalizedDisplayName("TitleProjectionLevels")]
        [LocalizedCategory("TitleProjections")]
        //[Description("Мин \"степень подтверждения\"")] // duplicated - all skipped
        public string FiboLevels
        {
            get { return string.Join(",", seriesTurnBar.fibonacciSeries); }
            set { seriesTurnBar.fibonacciSeries = value.ToIntArrayUniform(); }
        }

        [LocalizedDisplayName("TitleProjectionMarks")]
        [LocalizedCategory("TitleProjections")]
        [LocalizedDescription("MessageProjectionMarksDescription")]
        public string FiboMarks
        {
            get { return string.Join(",", seriesTurnBar.fibonacciMarks); }
            set { seriesTurnBar.fibonacciMarks = value.ToIntArrayUniform(); }
        }

        [LocalizedDisplayName("TitleDontCalculateConfirmation")]
        [LocalizedCategory("TitleProjections")]
        [LocalizedDescription("MessageDontCalculateConfirmationDescription")]
        public bool DontSumDegree
        {
            get { return seriesTurnBar.DontSumDegree; }
            set { seriesTurnBar.DontSumDegree = value; }
        }

        [LocalizedDisplayName("TitleCalculateHits")]
        [LocalizedCategory("TitleAssessment")]
        [LocalizedDescription("MessageCalculateHitsDescription")]
        public bool CalculateHits { get; set; }

        private int[] estimateMarks = new [] {1};
        [LocalizedDisplayName("TitleAssessmentForHit")]
        [LocalizedCategory("TitleAssessment")]
        [LocalizedDescription("MessageAssessmentForHitDescription")]
        public string EstimateMarks
        {
            get { return string.Join(";", estimateMarks); }
            set { estimateMarks = value.ToIntArrayUniform(); }
        }

        [LocalizedDisplayName("TitleZigzagPrice")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageZigzagPriceDescription")]
        public ZigZagSource ZigZagSourceType { get; set; }

        private bool hideOldBars = true;
        [LocalizedDisplayName("TitleHideOldBars")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageHideOldBarsDescription")]
        public bool HideOldBars
        {
            get { return hideOldBars; }
            set { hideOldBars = value; }
        }

        public void BuildSeries(ChartControl chart)
        {
            seriesTurnBar.barsKey.Clear();
            seriesTurnBar.barsTurn.Clear();
            if (chart.StockSeries.Data.Count < 3) return;
            // получить вершины ЗЗ
            var pivots = ZigZag.GetPivots(chart.StockSeries.Data.Candles, ThresholdPercent, ZigZagSourceType);
            MakeFiboBars(pivots);
            if (CalculateHits) DoCalculateHits(pivots);
        }

        private void MakeFiboBars(List<Cortege2<int, float>> pivots)
        {
            if (pivots.Count < 2) return;
            var candles = owner.StockSeries.Data.Candles;
            if (candles.Count == 0) return;

            // по каждому pivot-у поставить ключевой бар            
            foreach (var pt in pivots)
                seriesTurnBar.barsKey.Add(new TurnBar(new List<int> { pt.a }, true, owner));
            seriesTurnBar.CountTurnBars();
        }

        private void DoCalculateHits(List<Cortege2<int, float>> pivotsZz)
        {
            var candles = owner.StockSeries.Data.Candles;
            if (candles.Count == 0) return;
            if (seriesTurnBar.barsTurn.Count == 0) return;
            if (pivotsZz.Count == 0) return;
            
            // считать попадания в вершину либо +- 1 бар для "разворотных баров"
            var barCandleIndexes = seriesTurnBar.barsTurn.SelectMany(bar => bar.candleIndexes);
            var hitCountsFibo = CalculatePivotsHits(pivotsZz, 
                barCandleIndexes);
            
            // тот же подсчет для равномерно распределенных точек
            var countIntervals = seriesTurnBar.barsTurn.Count + 1;
            var intervalCandles = candles.Count/(double) countIntervals;
            var pointsEven = new List<int>();
            for (var startIndex = intervalCandles; startIndex < candles.Count; startIndex += intervalCandles)
                pointsEven.Add((int)startIndex);
            var hitCountsEven = CalculatePivotsHits(pivotsZz, pointsEven);
            // вывод в лог
            Logger.InfoFormat("{0} {1}", owner.Symbol, 
                BarSettingsStorage.Instance.GetBarSettingsFriendlyName(owner.Timeframe));
            Logger.InfoFormat("Оценка баров разворота (ZZ {0:f1}%, {1} свеч):", thresholdPercent, candles.Count);
            Logger.InfoFormat("{0} вершин ZZ, {1} Б.Р. Попаданий: {2}-Б.Р., {3}-равномерно",
                              pivotsZz.Count, barCandleIndexes.Count(), hitCountsFibo, hitCountsEven);
        }

        private int CalculatePivotsHits(List<Cortege2<int, float>> pivotsZz, 
            IEnumerable<int> indexes)
        {
            var hitsCount = 0;
            foreach (var index in indexes)
            {
                var curIndex = index;
                if (pivotsZz.Any(p => p.a == curIndex)) hitsCount += estimateMarks[0];
                for (var i = 1; i < estimateMarks.Length; i++)
                {
                    int leftIndex = curIndex - i, rightIndex = curIndex + i;
                    if (pivotsZz.Any(p => p.a == leftIndex)) hitsCount += estimateMarks[i];
                    if (pivotsZz.Any(p => p.a == rightIndex)) hitsCount += estimateMarks[i];
                }
            }
            return hitsCount;
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            SeriesResult = new List<Series.Series> { seriesTurnBar };
            EntitleIndicator();
        }

        public void Remove()
        {
            if (seriesTurnBar != null)
            {
                seriesTurnBar.barsKey.Clear();
                seriesTurnBar.barsTurn.Clear();
            }
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

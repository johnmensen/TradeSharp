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
    [LocalizedDisplayName("TitleCurrentProjections")]
    [LocalizedCategory("TitleGraphicsAnalysisShort")]
    [TypeConverter(typeof(PropertySorter))]
    public class FiboForkIndicator : BaseChartIndicator, IChartIndicator
    {
        #region Основные настройки

        private float[] levelsProj = { 0.618f, 1, 1.618f };

        [LocalizedDisplayName("TitleStretchChart")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageStretchChartDescription")]
        [PropertyOrder(1)]
        public bool ExtendChartBounds { get; set; }

        [LocalizedDisplayName("TitleCorrectionLevels")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageCorrectionLevelsDescription")]
        [PropertyOrder(2)]
        public string LevelsProj
        {
            get { return string.Join(" ", levelsProj.Select(l => l.ToStringUniform())); }
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                var parts = value.ToFloatArrayUniform();
                if (parts.Length == 0) return;
                levelsProj = parts;
            }
        }

        private float[] levelsCorr = { 0.236f, 0.382f, 0.618f };

        [LocalizedDisplayName("TitleExpansionLevels")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageExpansionLevelsDescription")]
        [PropertyOrder(3)]
        public string LevelsCorr
        {
            get { return string.Join(" ", levelsCorr.Select(l => l.ToStringUniform())); }
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                var parts = value.ToFloatArrayUniform();
                if (parts.Length == 0) return;
                levelsCorr = parts;
            }
        }

        [LocalizedDisplayName("TitleShowCorrection")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageShowCorrectionDescription")]
        [PropertyOrder(4)]
        public bool CorrectionEnabled { get; set; }

        private float thresholdPercent = 1;
        [LocalizedDisplayName("TitleZigzagThresholdInPercentsShort")]
        [LocalizedDescription("MessageThresholdInPercentsDescription")]
        [LocalizedCategory("TitleMain")]
        [PropertyOrder(5)]
        public float ThresholdPercent
        {
            get { return thresholdPercent; }
            set { thresholdPercent = value; }
        }

        [LocalizedDisplayName("TitleZigzagPrice")]
        [LocalizedDescription("MessageZigzagPriceDescription")]
        [LocalizedCategory("TitleMain")]
        [PropertyOrder(6)]
        public ZigZagSource ZigZagSourceType { get; set; }

        private int correctionBcLength = 5;
        [LocalizedDisplayName("TitleCorrectionBCLengthShort")]
        [LocalizedDescription("MessageCorrectionBCLengthDescription")]
        [LocalizedCategory("TitleVisuals")]
        [PropertyOrder(7)]
        public int CorrectionBcLength
        {
            get { return correctionBcLength; }
            set { correctionBcLength = value; }
        }

        private int correctionAbLength = 10;
        [LocalizedDisplayName("TitleCorrectionABLengthShort")]
        [LocalizedDescription("MessageCorrectionABLengthDescription")]
        [LocalizedCategory("TitleVisuals")]
        [PropertyOrder(8)]
        public int CorrectionAbLength
        {
            get { return correctionAbLength; }
            set { correctionAbLength = value; }
        }

        #endregion

        #region Визуальные настройки

        private int projectionBars = 20;
        [LocalizedDisplayName("TitleCorrectionLength")]
        [LocalizedDescription("MessageCorrectionLengthDescription")]
        [LocalizedCategory("TitleVisuals")]
        [PropertyOrder(9)]
        public int ProjectionBars
        {
            get { return projectionBars; }
            set { projectionBars = value; }
        }
        
        private Color colorLineUpper = Color.DarkGreen;
        [LocalizedDisplayName("TitleColor")]
        [LocalizedDescription("MessageLineColorDescription")]
        [LocalizedCategory("TitleVisuals")]
        [PropertyOrder(10)]
        public Color ColorLineUpper
        {
            get { return colorLineUpper; }
            set { colorLineUpper = value; }
        }

        private Color colorLineLower = Color.DarkRed;
        [LocalizedDisplayName("TitleColor")]
        [LocalizedDescription("MessageLineColorDescription")]
        [LocalizedCategory("TitleVisuals")]
        [PropertyOrder(11)]
        public Color ColorLineLower
        {
            get { return colorLineLower; }
            set { colorLineLower = value; }
        }

        [LocalizedDisplayName("TitleCreateDrawingPanel")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageCreateDrawingPanelDescription")]
        [PropertyOrder(12)]
        public override bool CreateOwnPanel { get { return false; } set {} }

        [LocalizedDisplayName("TitleMarkers")]
        [LocalizedCategory("TitleVisuals")]
        [LocalizedDescription("MessageMarkersDescription")]
        [PropertyOrder(13)]
        public ProjectionPair.MarkerPlacement Markers { get; set; }

        #endregion

        /// <summary>
        /// серия проекций
        /// </summary>
        private ProjectionSeries spans;
        
        public override BaseChartIndicator Copy()
        {
            var opt = new FiboForkIndicator();
            Copy(opt);
            return opt;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var fork = (FiboForkIndicator)indi;
            CopyBaseSettings(fork);
            fork.levelsProj = levelsProj.ToArray();
            fork.levelsCorr = levelsCorr.ToArray();
            fork.ThresholdPercent = ThresholdPercent;
            fork.ZigZagSourceType = ZigZagSourceType;
            fork.ColorLineUpper = ColorLineUpper;
            fork.ColorLineLower = ColorLineLower;
            fork.ProjectionBars = ProjectionBars;
            fork.CorrectionBcLength = CorrectionBcLength;
            fork.CorrectionAbLength = CorrectionAbLength;
            fork.CorrectionEnabled = CorrectionEnabled;
            fork.ExtendChartBounds = ExtendChartBounds;
            fork.Markers = Markers;
        }

        public override string Name
        {
            get { return Localizer.GetString("TitleCurrentProjections"); }
        }

        /// <summary>
        /// от двух пар последних точек ЗЗ построить проекции
        /// </summary>
        public void BuildSeries(ChartControl chart)
        {
            spans.data.Clear();
            //spans.ExtendYAxis = ExtendChartBounds;
            // получить точки Зиг-Зага
            var pivots = ZigZag.GetPivots(chart.StockSeries.Data.Candles, ThresholdPercent, ZigZagSourceType);
            if (pivots.Count < 3) return;
            // построить проекции
            var pointA = pivots[pivots.Count - 3];
            var pointB = pivots[pivots.Count - 2];
            var pointC = pivots[pivots.Count - 1];

            var spanA = new ProjectionPair(pointA.a, pointA.b)
                            {
                                HideFarParts = false,
                                Color = pointB.b > pointA.b ? ColorLineLower : ColorLineUpper,
                                ProjectionLength = ProjectionBars,
                                LevelsProj = LevelsProj,
                                Markers = Markers
                            };

            spanA.AddPoint(pointB.a, pointB.b);
            var spanB = new ProjectionPair(pointB.a, pointB.b)
                            {
                                HideFarParts = false,
                                Color = pointB.b < pointA.b ? ColorLineLower : ColorLineUpper,
                                ProjectionLength = ProjectionBars,
                                LevelsProj = LevelsProj,                                
                                Markers = Markers
                            };
            spanB.AddPoint(pointC.a, pointC.b);
            spans.data.Add(spanA);
            spans.data.Add(spanB);
            // коррекции?
            if (!CorrectionEnabled) return;
            if (correctionBcLength > 0)
            {
                var spanC = new ProjectionPair(pointB.a, pointB.b)
                                {
                                    HideFarParts = false,
                                    Color = pointB.b < pointA.b ? ColorLineLower : ColorLineUpper,
                                    ProjectionLength = CorrectionBcLength,
                                    LevelsCorr = LevelsCorr,
                                    IsExtension = false,
                                    IsCorrection = true,
                                    Markers = Markers
                                };
                spanC.AddPoint(pointC.a, pointC.b);
                spans.data.Add(spanC);
            }
            if (correctionAbLength > 0)
            {
                var spanC = new ProjectionPair(pointA.a, pointA.b)
                {
                    HideFarParts = false,
                    Color = pointB.b < pointA.b ? ColorLineLower : ColorLineUpper,
                    ProjectionLength = CorrectionAbLength,
                    LevelsCorr = LevelsCorr,
                    IsExtension = false,
                    IsCorrection = true,
                    Markers = Markers
                };
                spanC.AddPoint(pointB.a, pointB.b);
                spans.data.Add(spanC);
            }
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            // инициализируем индикатор
            spans = new ProjectionSeries(Localizer.GetString("TitleProjections"));
            SeriesResult = new List<Series.Series> { spans };
            EntitleIndicator();
        }

        public void Remove()
        {
            if (spans != null) spans.data.Clear();
        }

        public void AcceptSettings()
        {            
        }

        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (updatedCandle == null && (newCandles == null || newCandles.Count == 0))
                return;
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

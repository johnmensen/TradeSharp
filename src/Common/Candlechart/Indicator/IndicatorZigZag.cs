using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using Candlechart.Chart;
using Candlechart.ChartMath;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    [LocalizedDisplayName("TitleZigzag")]
    [LocalizedCategory("TitleTrending")]
    [TypeConverter(typeof(PropertySorter))]
    public class IndicatorZigZag : BaseChartIndicator, IChartIndicator
    {
        [Browsable(false)]
        public override string Name { get { return Localizer.GetString("TitleZigzag"); } }

        public override BaseChartIndicator Copy()
        {
            var zig = new IndicatorZigZag();
            Copy(zig);
            return zig;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var zig = (IndicatorZigZag) indi;
            CopyBaseSettings(zig);
            zig.LineColor = LineColor;
            zig.LineWidth = LineWidth;
            zig.LineStyle = LineStyle;
            zig.ThresholdPercent = ThresholdPercent;
            zig.seriesZigZag = seriesZigZag;
            zig.ZigZagSourceType = ZigZagSourceType;
        }

        [LocalizedDisplayName("TitleCreateDrawingPanel")]
        [LocalizedCategory("TitleMain")]
        [Description("Создавать свою панель отрисовки")]
        public override bool CreateOwnPanel { get; set; }

        #region Визуальные

        private Color lineColor = Color.Red;
        [LocalizedDisplayName("TitleLineColor")]
        [Description("Цвет линии индикатора")]
        [LocalizedCategory("TitleVisuals")]
        public Color LineColor
        {
            get { return lineColor; }
            set { lineColor = value; }
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

        private DashStyle lineStyle = DashStyle.Solid;
        [LocalizedDisplayName("TitleLineStyle")]
        [Description("Стиль линии индикатора (сплошная, штирх, штрих-пунктир...)")]
        [LocalizedCategory("TitleVisuals")]
        public DashStyle LineStyle
        {
            get { return lineStyle; }
            set { lineStyle = value; }
        }

        #endregion

        private float thresholdPercent = 1;
        [LocalizedDisplayName("TitleThresholdInPercents")]
        [Description("Пороговая величина, %, на которую должен отклониться курс для формирования нового экстремума")]
        [LocalizedCategory("TitleMain")]
        public float ThresholdPercent
        {
            get { return thresholdPercent; }
            set { thresholdPercent = value; }
        }

        [LocalizedDisplayName("TitleZigzagPrice")]
        [Description("Цены Зиг-Зага")]
        [LocalizedCategory("TitleMain")]
        public ZigZagSource ZigZagSourceType { get; set; }
        
        private LineSeries seriesZigZag;

        public IndicatorZigZag()
        {
            CreateOwnPanel = false;
        }

        public void BuildSeries(ChartControl chart)
        {
            if (DrawPane != owner.StockPane)
                DrawPane.Title = UniqueName;
            seriesZigZag.Data.Clear();
            if (chart.StockSeries.Data.Count < 3) return;
            
            seriesZigZag.LineColor = lineColor;
            seriesZigZag.LineWidth = (float)lineWidth;
            seriesZigZag.LineDashStyle = lineStyle;

            // индекс - индикатор
            var pivots = ZigZag.GetPivots(chart.StockSeries.Data.Candles, ThresholdPercent, ZigZagSourceType);
            if (pivots.Count == 0) return;

            // построить отрезки
            var lastPivot = pivots[0];
            for (var i = 1; i < pivots.Count; i++)
            {
                var pivot = pivots[i];
                var step = (double)(pivot.b - lastPivot.b) / (pivot.a - lastPivot.a);
                for (var j = lastPivot.a; j < pivot.a; j++)
                {
                    seriesZigZag.Data.Add((double)lastPivot.b + step * (j - lastPivot.a));
                }
                lastPivot = pivot;
            }
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            // инициализируем индикатор
            seriesZigZag = new LineSeries(Localizer.GetString("TitleZigzag"))
            {
                Transparent = true,
                LineColor = lineColor,
                LineWidth = ((float)LineWidth),
                ShiftX = 1,
                DashPattern = new float[] { 3, 5 }
            };
            SeriesResult = new List<Series.Series> { seriesZigZag };
            EntitleIndicator();
        }

        public void Remove()
        {
            seriesZigZag.Data.Clear();
        }

        public void AcceptSettings()
        {
        }        

        /// <summary>
        /// пересчитать индикатор для последней добавленной свечки
        /// </summary>        
        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
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

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Candlechart.Chart;
using Candlechart.Core;
using Candlechart.Indicator;
using Candlechart.Series;
using Entity;
using TradeSharp.Util;

namespace DemoPlugin
{
    [LocalizedDisplayName("TitleDemoTrendAndFlat")]
    [LocalizedCategory("TitleTrending")]
    [TypeConverter(typeof (PropertySorter))]
    public class DemoTrendFlat : BaseChartIndicator, IChartIndicator
    {
        [Browsable(false)]
        public override string Name
        {
            get { return Localizer.GetString("TitleDemoTrendAndFlat"); }
        }

        [LocalizedDisplayName("TitleCreateDrawingPanel")]
        [LocalizedCategory("TitleMain")]
        [Description("Создавать свою панель отрисовки")]
        public override bool CreateOwnPanel
        {
            get { return true; }
            set { }
        }

        private int periodSlow = 20;

        [LocalizedDisplayName("TitleSlowMovingAveragePeriodShort")]
        [Description("Период медленной скользящей средней")]
        [LocalizedCategory("TitleMain")]
        public int PeriodSlow
        {
            get { return periodSlow; }
            set { periodSlow = value; }
        }

        private int periodFast = 5;

        [LocalizedDisplayName("TitleFastMovingAveragePeriodShort")]
        [Description("Период быстрой скользящей средней")]
        [LocalizedCategory("TitleMain")]
        public int PeriodFast
        {
            get { return periodFast; }
            set { periodFast = value; }
        }

        private Color clLine = Color.Black;

        [LocalizedDisplayName("TitleColor")]
        [Description("Цвет линии")]
        [LocalizedCategory("TitleVisuals")]
        public Color ClLine
        {
            get { return clLine; }
            set { clLine = value; }
        }

        private Color clBarsPositive = Color.Green;

        [LocalizedDisplayName("TitleGrowthColor")]
        [Description("Цвет положительных приращений")]
        [LocalizedCategory("TitleVisuals")]
        public Color ClBarsPositive
        {
            get { return clBarsPositive; }
            set { clBarsPositive = value; }
        }

        private Color clBarsNegative = Color.Red;

        [LocalizedDisplayName("TitleFallColor")]
        [Description("Цвет отрицательных приращений")]
        [LocalizedCategory("TitleVisuals")]
        public Color ClBarsNegative
        {
            get { return clBarsNegative; }
            set { clBarsNegative = value; }
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

        private decimal lineWidth = 1;

        [LocalizedDisplayName("TitleThickness")]
        [Description("Толщина линии индикатора, пикселей")]
        [LocalizedCategory("TitleVisuals")]
        public decimal LineWidth
        {
            get { return lineWidth; }
            set { lineWidth = value; }
        }

        private CandlePriceType priceType = CandlePriceType.Close;

        [LocalizedDisplayName("TitlePrice")]
        [Description("Цена, от которой считаются средние")]
        [LocalizedCategory("TitleMain")]
        public CandlePriceType PriceType
        {
            get { return priceType; }
            set { priceType = value; }
        }

        private LineSeries seriesLine = new LineSeries("TrendFlat") {Transparent = true};
        private HistogramSeries seriesDeltas = new HistogramSeries("TrendFlat Profit");

        private RestrictedQueue<float> queueFast;
        private RestrictedQueue<float> queueSlow;

        /// <summary>
        /// предыдущее значение для скользящей средней
        /// </summary>
        private float? smmaPrev;

        public DemoTrendFlat()
        {
        }

        public DemoTrendFlat(DemoTrendFlat indi)
        {
        }

        public override BaseChartIndicator Copy()
        {
            var indi = new DemoTrendFlat();
            Copy(indi);
            return indi;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var indiTf = (DemoTrendFlat) indi;
            CopyBaseSettings(indiTf);
            indiTf.ClLine = ClLine;
            indiTf.PriceType = PriceType;
            indiTf.seriesLine = seriesLine;
            indiTf.seriesDeltas = seriesDeltas;
            indiTf.periodSlow = periodSlow;
            indiTf.periodFast = periodFast;
            indiTf.ClBarsPositive = ClBarsPositive;
            indiTf.ClBarsNegative = ClBarsNegative;
            indiTf.LineWidth = LineWidth;
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            SeriesResult = new List<Series> {seriesLine, seriesDeltas};

            seriesLine.LineColor = clLine;
            seriesLine.LineWidth = (float) lineWidth;
            seriesLine.LineDashStyle = lineStyle;

            EntitleIndicator();
        }

        public void Remove()
        {
            if (seriesLine == null) return;
            seriesLine.Data.Clear();
            seriesDeltas.data.Clear();
        }

        public void AcceptSettings()
        {
            seriesLine.LineColor = clLine;
            seriesLine.LineWidth = (float) lineWidth;
            seriesLine.LineDashStyle = lineStyle;

            if (CreateOwnPanel)
            {
                SeriesResult = new List<Series> {seriesLine, seriesDeltas};
            }
            if (DrawPane != null && DrawPane != owner.StockPane)
                DrawPane.Title = string.Format("{0} [{1}/{2}]", UniqueName, PeriodSlow, PeriodFast);
        }

        public void BuildSeries(ChartControl chart)
        {
            smmaPrev = null;
            seriesLine.Data.Clear();
            seriesDeltas.data.Clear();

            if (chart.StockSeries.DataCount <= periodSlow) return;

            queueFast = new RestrictedQueue<float>(periodFast);
            queueSlow = new RestrictedQueue<float>(periodSlow);

            // построить индюк
            BuildIndi(SeriesSources[0]);
        }

        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (updatedCandle == null && newCandles.Count == 0) return;
            // построить индюк
            BuildSeries(owner);
        }

        private void BuildIndi(Series source)
        {
            if (source is CandlestickSeries == false) return;
            var candles = ((CandlestickSeries) source).Data.Candles;

            var currentSign = 0; // текущая операция (покупка/продажа)
            var curIndex = 0;
            foreach (var candle in candles)
            {
                var price = candle.GetPrice(priceType);
                // обновить средние                        
                queueFast.Add(price);
                queueSlow.Add(price);

                // посчитать профит
                var delta = 0f;
                if (currentSign != 0)
                {
                    delta = (candle.close - candle.open) * currentSign;
                }
                seriesDeltas.data.Add(new HistogramBar
                    {
                        color = delta >= 0 ? ClBarsPositive : ClBarsNegative,
                        index = curIndex++,
                        y = delta
                    });
                // определить знак
                if (queueSlow.Length < periodSlow) continue;
                var maSlow = queueSlow.Average();
                var maFast = queueFast.Average();
                var newSign = maFast > maSlow ? 1 : maFast < maSlow ? -1 : 0;
                if (newSign != 0) currentSign = newSign;
            }
        }

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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using Candlechart.Chart;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Util;
using System.Linq;

namespace Candlechart.Indicator
{
    [LocalizedDisplayName("TitleProjectionAndDivergences")]
    [LocalizedCategory("TitleDivergences")]
    [TypeConverter(typeof(PropertySorter))]
    public class IndicatorFiboDiver : BaseChartIndicator, IChartIndicator
    {
        /// <summary>
        /// внутреннее описание дивера + расширения
        /// </summary>
        struct DiverTag
        {
            public int candleIndex;
            public int side;
            public float price;
            public string description;
        }

        #region Данные и свойства - основные

        [Browsable(false)]
        public override string Name
        {
            get { return Localizer.GetString("TitleProjectionAndDivergences"); }
        }

        [LocalizedDisplayName("TitleCreateDrawingPanel")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageCreateDrawingPanelDescription")]
        public override bool CreateOwnPanel { get; set; }

        public enum DrawStyle
        {
            Текст = 0,
            Стрелка = 1            
        }

        [LocalizedDisplayName("TitleDrawingStyle")]
        [LocalizedCategory("TitleVisuals")]
        [LocalizedDescription("MessageDrawingStyleArrowsOrTextDescription")]
        public DrawStyle IndicatorDrawStyle { get; set; }

        private SeriesComment seriesComment;

        #endregion

        #region Основные настройки

        private int redrawSeconds = 5;
        [LocalizedDisplayName("TitleRedrawingTimeInSeconds")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageRedrawingTimeDescription")]
        public int RedrawSeconds
        {
            get { return redrawSeconds; }
            set { redrawSeconds = value; }
        }

        private int maxPastDiver = 50;
        [LocalizedDisplayName("TitleCandleCountFromExtensionShort")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageCandleCountFromExtensionDescription")]
        public int MaxPastDiver
        {
            get { return maxPastDiver; }
            set { maxPastDiver = value; }
        }

        #endregion

        #region Настройки отрисовки

        [LocalizedDisplayName("TitleSourceSeries")]
        [LocalizedDescription("MeesageSourceSeriesDescription")]
        [LocalizedCategory("TitleMain")]
        [Editor("Candlechart.Indicator.CheckedListBoxSeriesUITypeEditor, System.Drawing.Design.UITypeEditor",
            typeof(UITypeEditor))]
        public override string SeriesSourcesDisplay { get; set; }

        private bool isVisible = true;
        [LocalizedDisplayName("TitleShowOnChart")]
        [LocalizedDescription("MessageShowDivergenceOnChartDescription")]
        [LocalizedCategory("TitleMain")]
        public bool IsVisible
        {
            get { return isVisible; }
            set { isVisible = value; }
        }

        #endregion

        #region Визуальные настройки
        
        private int arrowLengthPx = 60;
        [LocalizedDisplayName("TitleArrowLength")]
        [LocalizedCategory("TitleVisuals")]
        [LocalizedDescription("MessageArrowLengthDescription")]
        public int ArrowLengthPx
        {
            get { return arrowLengthPx; }
            set { arrowLengthPx = value; }
        }

        private bool showArrow = true;
        [LocalizedDisplayName("TitleArrow")]
        [LocalizedCategory("TitleVisuals")]
        [LocalizedDescription("MessageDrawArrowDescription")]
        public bool ShowArrow
        {
            get { return showArrow; }
            set { showArrow = value; }
        }

        [LocalizedDisplayName("TitleStroke")]
        [LocalizedCategory("TitleVisuals")]
        [LocalizedDescription("MessageDrawStroke")]
        public bool ShowBox { get; set; }

        #endregion

        private readonly ThreadSafeTimeStamp lastUpdateTime = new ThreadSafeTimeStamp();

        public IndicatorFiboDiver()
        {
            CreateOwnPanel = false;
            lastUpdateTime.Touch();
        }

        public override BaseChartIndicator Copy()
        {
            var div = new IndicatorFiboDiver();
            Copy(div);
            return div;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var div = (IndicatorFiboDiver)indi;
            CopyBaseSettings(div);
            div.MaxPastDiver = MaxPastDiver;
            div.seriesComment = seriesComment;
            div.IndicatorDrawStyle = IndicatorDrawStyle;
            div.IsVisible = IsVisible;
            div.RedrawSeconds = RedrawSeconds;
            div.ShowArrow = ShowArrow;
            div.ShowBox = ShowBox;
            div.ArrowLengthPx = ArrowLengthPx;
        }

        public void BuildSeries(ChartControl chart)
        {
            seriesComment.data.Clear();
            if (SeriesSources.Count == 0) return;
            
            if (!IsVisible) return;

            if (SeriesSources == null) return;
            if (SeriesSources.Count != 1)
            {
                Logger.ErrorFormat("{0}: неверно заданы источники данных (должно быть 1)", 
                    UniqueName);
                return;
            }

            // сбацать коменты
            var tags = UpdateMarks();
            MakeCommentsByTags(tags);
            lastUpdateTime.Touch();
        }

        /// <summary>
        /// получить отметки, которые потом будут добавлены на график
        /// </summary>
        private List<DiverTag> UpdateMarks()
        {
            // получить отметки, которые потом будут добавлены на график
            var diversFound = new List<DiverTag>();
            
            var candles = owner.StockSeries.Data.Candles;
            if (candles.Count == 0) return diversFound;

            // все стрелки дивергенций записать в словарь:
            // ключ - индекс свечи, значение - массив стрелок
            var diverByIndex = new Dictionary<int, List<TrendLine>>();
            foreach (var indi in owner.Owner.indicators)
            {
                if (indi is IndicatorDiver == false) continue;
                var indiDiv = (IndicatorDiver) indi;
                if (indiDiv.SeriesResult.Count == 0) continue;
                var seriesTrend = indiDiv.SeriesResult.FirstOrDefault(s => s is TrendLineSeries) as TrendLineSeries;
                if (seriesTrend == null) continue;

                foreach (var divArrow in seriesTrend.data)
                {
                    if (divArrow.linePoints.Count < 2) continue;
                    var candleIndex = (int)Math.Round(divArrow.linePoints[1].X);
                    List<TrendLine> lines;
                    if (diverByIndex.TryGetValue(candleIndex, out lines))
                        lines.Add(divArrow);
                    else
                    {
                        lines = new List<TrendLine> { divArrow };
                        diverByIndex.Add(candleIndex, lines);
                    }
                }
            }

            foreach (var proj in owner.Owner.seriesProjection.data)
            {
                if (proj.points.Count < 2) continue;

                // для каждой проекции отсчитываем вперед N свеч и проверяем выполнения условия, заодно ищем дивер
                var candlesForward = Math.Min(proj.ProjectionLength, maxPastDiver);
                var startCandle = proj.points[1].a + 1;
                var endCandle = Math.Min(startCandle + candlesForward, candles.Count - 1);

                var projectionIsReached = false;
                var projPrice = 0f;

                for (var i = startCandle; i <= endCandle; i++)
                {
                    var candle = candles[i];
                    // проверить условие расширения / коррекции
                    if (projPrice == 0)
                    {
                        var k = proj.IsCorrection
                                    ? proj.levelsCorr[0]
                                    : proj.levelsProj[0] + 1;
                        var delta = proj.points[1].b - proj.points[0].b;
                        projPrice = proj.points[1].b - delta * k;
                    }
                    if (!projectionIsReached)
                    {
                        projectionIsReached = proj.points[1].b > proj.points[0].b
                                                  ? candle.low <= projPrice
                                                  : candle.high >= projPrice;
                    }

                    // получить диверы на текущей свечке
                    if (!projectionIsReached) continue;
                    List<TrendLine> arrows;
                    if (!diverByIndex.TryGetValue(i, out arrows)) continue;
                    foreach (var arrow in arrows)
                    {
                        diversFound.Add(new DiverTag
                            {
                                candleIndex = i,
                                price = (float) arrow.linePoints[1].Y,
                                side = arrow.linePoints[1].Y >= arrow.linePoints[0].Y ? 1 : -1,
                                description =
                                    string.Format("{0} {1}",
                                                  proj.IsCorrection
                                                      ? Localizer.GetString("TitleCorrection")
                                                      : Localizer.GetString("TitleProjection"),
                                                  projPrice.ToStringUniformPriceFormat())
                            });
                    }
                }
            }

            return diversFound;
        }

        private void MakeCommentsByTags(List<DiverTag> tags)
        {
            foreach (var diverTag in tags)
            {
                var comm = new ChartComment
                    {
                        ArrowLength = arrowLengthPx,
                        ArrowAngle = diverTag.side > 0 ? 270 : 90,
                        Text = "!", //diverTag.description,
                        PivotIndex = diverTag.candleIndex,
                        PivotPrice = diverTag.price,
                        HideArrow = !ShowArrow,
                        HideBox = !ShowBox,
                        ColorText = Color.Red
                    };
                seriesComment.data.Add(comm);
            }
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            seriesComment = new SeriesComment(Localizer.GetString("TitleProjectionAndDivergences"));
            // инициализируем индикатор
            EntitleIndicator();
            SeriesResult = new List<Series.Series> { seriesComment };
        }

        public void Remove()
        {
            if (seriesComment != null) seriesComment.data.Clear();
        }

        public void AcceptSettings()
        {
        }

        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (updatedCandle == null) return;            

            // перерисовка должна производиться не чаще, чем раз в N секунд
            // (если свеча просто обновилась)
            if (newCandles == null || newCandles.Count == 0)
            {
                var deltaSeconds = (DateTime.Now - lastUpdateTime.GetLastHit()).TotalSeconds;
                if (deltaSeconds < redrawSeconds) return;
            }

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


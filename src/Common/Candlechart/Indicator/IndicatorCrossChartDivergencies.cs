using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Xml;
using Candlechart.Chart;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    /// <summary>
    /// индикатор хранит набор параметров:
    /// таймфрейм - формула индекса - настройки дивергенций
    /// </summary>
    [LocalizedDisplayName("TitleMultiTFAndIndex")]
    [LocalizedCategory("TitleDivergences")]
    [TypeConverter(typeof(PropertySorter))]
    public class IndicatorCrossChartDivergencies : BaseChartIndicator, IChartIndicator, IChartQueryIndicator
    {
        public override string Name
        {
            get { return Localizer.GetString("TitleMultiTFAndIndex"); }
        }

        #region Свойства

        private List<MultyTimeframeIndexSettings> sets = new List<MultyTimeframeIndexSettings>();
        [LocalizedDisplayName("TitleDivergenceSettings")]
        [CrossChartSeriesSetAttribute]
        [Editor(typeof(CrossChartDiverUITypeEditor), typeof(UITypeEditor))]
        public List<MultyTimeframeIndexSettings> Sets
        {
            get { return sets; }
            set { sets = value; }
        }

        //[DisplayName("Создавать панель отрисовки"), Category("Основные"), Description("Создавать свою панель отрисовки")]
        public override bool CreateOwnPanel { get; set; }

        #endregion

        #region Данные
        /// <summary>
        /// хранит маленькие значки диверов
        /// </summary>
        private TrendLineSeries seriesTrendLine;
        #endregion

        #region Данные - визуальные

        private Color colorArrowUp = Color.Blue;

        [LocalizedDisplayName("TitleBullDivergenceColor")]
        [LocalizedCategory("TitleVisuals")]
        [LocalizedDescription("MessageBullDivergenceColorDescription")]
        public Color ColorArrowUp { get { return colorArrowUp; } set { colorArrowUp = value; } }

        private Color colorArrowDown = Color.Red;
        [LocalizedDisplayName("TitleBearishDivergenceColor")]
        [LocalizedDescription("MessageBearishDivergenceColorDescription")]
        [LocalizedCategory("TitleVisuals")]
        public Color ColorArrowDown { get { return colorArrowDown; } set { colorArrowDown = value; } }

        private static int indicatorThemeIndex;
        #endregion

        #region IChartQueryIndicator
        private GetOuterChartsDel getOuterCharts;

        public event GetOuterChartsDel GetOuterCharts
        {
            add { getOuterCharts += value; }
            remove { getOuterCharts -= value; }
        }
        #endregion

        public IndicatorCrossChartDivergencies()
        {
            CreateOwnPanel = false;
        }

        public override BaseChartIndicator Copy()
        {
            var ind = new IndicatorCrossChartDivergencies();
            Copy(ind);
            return ind;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var ind = (IndicatorCrossChartDivergencies) indi;
            CopyBaseSettings(ind);
            ind.ColorArrowUp = ColorArrowUp;
            ind.ColorArrowDown = ColorArrowDown;
            // настройки индексов - диверов
            ind.sets.Clear();
            foreach (var set in sets)            
                ind.sets.Add(set.MakeCopy());            
        }

        public List<string> GetRequiredChartIds()
        {
            return sets.Select(s => s.chartId).ToList();
        }

        public void BuildSeries(ChartControl chart)
        {
            seriesTrendLine.data.Clear();
            // актуализировать серии-источники
            ActualizeSeries();
            // по каждой паре серий строить диверы и переводить их Х
            // координаты в масштаб графика (если ТФ отличаются)
            var signs = new Dictionary<int, List<DiverMarker>>();
            foreach (var src in sets)
            {
                if (src.seriesDest == null || src.seriesSrc == null) continue;
                if (src.seriesDest.isDisposed || src.seriesSrc.isDisposed) continue;
                if (src.seriesDest is IPriceQuerySeries == false ||
                    src.seriesSrc is IPriceQuerySeries == false) continue;
                BuildDivers(src, signs);
            }

            // отобразить
            foreach (var pair in signs)
            {
                var markers = pair.Value;
                var index = pair.Key;
                var sign = Math.Sign(markers.Sum(m => m.sign));
                if (sign == 0) sign = markers[0].sign;
                var price = (double)(owner.StockSeries.GetPrice(index) ?? 0);

                var text = string.Join(", ",
                    markers.Select(m => string.Format("{0}:{1}", m.title, m.sign > 0 ? '^' : 'v')));
                                
                var line = new TrendLine
                {
                    LineStyle = TrendLine.TrendLineStyle.СвечнаяСтрелка,
                    LineColor = sign > 0 ? ColorArrowUp : ColorArrowDown,
                    ShapeAlpha = 192,
                    ShapeFillColor = sign > 0 ? ColorArrowUp : ColorArrowDown,
                    Comment = text
                };
                line.AddPoint(index, price - sign * price * 0.01);
                line.AddPoint(index, price);
                seriesTrendLine.data.Add(line);
            }
        }

        /// <summary>
        /// построить дивергенции
        /// </summary>
        private void BuildDivers(MultyTimeframeIndexSettings src, Dictionary<int, List<DiverMarker>> signs)
        {
            // получить диверы
            var divers =
                src.DiverType == IndicatorDiver.DivergenceType.Классические
                    ? FindDivergencePointsClassic(src)
                    : FindDivergencePointsQuasi(src);
            
            // добавить их в словарь
            foreach (var diver in divers)
            {
                // пересчитать индекс свечки
                var index = diver.end;
                if (src.seriesDest is StockSeries)
                {
                    var time = ((StockSeries) src.seriesDest).GetCandleOpenTimeByIndex(index);
                    index = owner.StockSeries.GetIndexByCandleOpen(time);
                }

                // создать для свечки список маркеров или добавить готовый
                var marker = new DiverMarker(src.InverseDivergency ? -diver.sign : diver.sign,
                    src.TimeframeAndSymbol);
                List<DiverMarker> markers;
                signs.TryGetValue(index, out markers);
                if (markers == null)
                {
                    markers = new List<DiverMarker> { marker };
                    signs.Add(index, markers);
                }
                else
                    markers.Add(marker);
            }
        }

        private static IEnumerable<DiverSpan> FindDivergencePointsClassic(MultyTimeframeIndexSettings src)
        {
            var diverPairs = new List<DiverSpan>();
            var srcCount = src.seriesDest.DataCount;

            // найти экстремумы            
            var extremums = new List<Point>(); // index - sign
            for (var i = src.periodExtremum; i < srcCount - src.periodExtremum - 1; i++)
            {
                bool isPeak = true, isLow = true;
                var price = src.GetDestPrice(i) ?? 0;
                for (var j = i - src.periodExtremum; j <= i + src.periodExtremum; j++)
                {
                    if (j == i) continue;
                    var priceNear = src.GetDestPrice(j);
                    if (priceNear > price) isPeak = false;
                    if (priceNear < price) isLow = false;
                    if (!isPeak && isLow == false) break;
                }
                if (isPeak) extremums.Add(new Point(i, 1));
                if (isLow) extremums.Add(new Point(i, -1));
            }

            // от каждого экстремумы поиск вперед до M баров - поиск экстремума на интервале [i-N...i+1]
            foreach (var extr in extremums)
            {
                var signExtr = extr.Y;
                var indexExtr = extr.X;
                var priceExtr = src.GetDestPrice(indexExtr);

                for (var i = extr.X + 1; i <= (extr.X + src.maxPastExtremum); i++)
                {
                    if (i >= (srcCount - 1)) break;
                    // новый "экстремум" должен быть выше старого
                    var price = src.GetDestPrice(i) ?? 0;
                    if ((signExtr > 0 && price <= priceExtr) ||
                        (signExtr < 0 && price >= priceExtr)) continue;
                    // за новым "экстремумом" должна следовать точка ниже (выше для минимума)
                    var nextPrice = src.GetDestPrice(i + 1) ?? 0;
                    if ((signExtr > 0 && price <= nextPrice) ||
                        (signExtr < 0 && price >= nextPrice)) continue;
                    // сравнение с дельтой индекса
                    // !! смещение индекса
                    var srcPriceI = src.GetSourcePrice(i) ?? 0;
                    var srcPriceEx = src.GetSourcePrice(indexExtr) ?? 0;
                    var deltaIndex = Math.Sign(srcPriceI - srcPriceEx);
                    if (signExtr == deltaIndex) continue;
                    // экстремум найден
                    priceExtr = price;
                    diverPairs.Add(new DiverSpan(indexExtr, i, -signExtr));
                }
            }

            return diverPairs;
        }

        private List<DiverSpan> FindDivergencePointsQuasi(MultyTimeframeIndexSettings src)
        {
            var divers = new List<DiverSpan>();
            var srcCount = src.seriesDest.DataCount;

            float? lastPeak = null;
            var peakPrice = 0f;
            var extremumSign = 0;
            var startIndex = 0;

            for (var i = 0; i < srcCount; i++)
            {
                var index = src.GetSourcePrice(i) ?? 0;
                var indexNext = i < srcCount ? (src.GetSourcePrice(i + 1) ?? 0) : index;
                // если находимся в зоне перекупленности / перепроданности,
                // если след. точка ниже (выше для перепроданности),
                // если текущая точка выше последнего максимума (ниже минимума...)

                var margUp = index >= src.marginUpper;
                var margDn = index <= src.marginLower;

                if ((margUp && indexNext < index &&
                    index > (lastPeak.HasValue ? lastPeak.Value : float.MinValue)) ||
                    (margDn && indexNext > index && index < (lastPeak.HasValue ? lastPeak.Value : float.MaxValue)))
                {// + или - экстремум 
                    peakPrice = src.GetDestPrice(i) ?? 0;                        
                    extremumSign = margUp ? 1 : -1;
                    startIndex = i;
                    lastPeak = index;
                    continue;
                }
                if (!margUp && margDn == false)
                {// конец отсчета экстремума
                    extremumSign = 0;
                    lastPeak = null;
                    continue;
                }
                // отсчет экстремума - сравнить дельту цены и дельту индекса
                var deltaPrice = (src.GetDestPrice(i) ?? 0) - peakPrice;
                var deltaIndex = index - lastPeak;

                if (extremumSign > 0)
                    if (deltaPrice > 0 && deltaIndex < 0)
                    // медвежий дивер
                    {
                        divers.Add(new DiverSpan(startIndex, i, -1));
                        peakPrice = src.GetDestPrice(i) ?? 0;
                    }
                if (extremumSign < 0)
                    if (deltaPrice < 0 && deltaIndex > 0)
                    // бычий дивер
                    {
                        divers.Add(new DiverSpan(startIndex, i, 1));
                        peakPrice = src.GetDestPrice(i) ?? 0;
                    }
            }
            return divers;
        }

        /// <summary>
        /// актуализировать список outerSeries
        /// </summary>
        private void ActualizeSeries()
        {
            if (sets.Count == 0) return;
            // получить список графиков, серии которых необходимо обновить
            var chartsIdsToUpdate = new List<string>();            
            foreach (var src in sets)
            {
                if (src.seriesSrc != null && src.seriesSrc.isDisposed) src.seriesSrc = null;
                if (src.seriesSrc == null) chartsIdsToUpdate.Add(src.chartId);

                if (src.seriesDest != null && src.seriesDest.isDisposed) src.seriesDest = null;
                if (src.seriesDest == null) chartsIdsToUpdate.Add(src.chartId);
            }

            if (chartsIdsToUpdate.Count == 0) return;
            
            // запросить объекты графиков            
            List<CandleChartControl> charts;
            getOuterCharts(chartsIdsToUpdate.Distinct().ToList(), out charts);
            if (charts == null || charts.Count == 0) return;
            
            // обновить каждую серию
            foreach (var src in sets)
            {
                var chartId = src.chartId;
                var chart = charts.FirstOrDefault(c => c.UniqueId == chartId);
                if (chart == null) continue;
                src.TimeframeAndSymbol = BarSettingsStorage.Instance.GetBarSettingsFriendlyName(chart.Timeframe);
                if (chart.Symbol != owner.Symbol)
                    src.TimeframeAndSymbol += string.Format(" ({0})", chart.Symbol);

                if (src.seriesDest == null)
                    src.seriesDest = GetOuterSeries(chart, src.FullyQualifiedSeriesDestName);
                if (src.seriesSrc == null)
                    src.seriesSrc = GetOuterSeries(chart, src.FullyQualifiedSeriesSrcName);
            }
        }

        private static Series.Series GetOuterSeries(CandleChartControl chart,
            string seriesFullName)
        {
            if (string.IsNullOrEmpty(seriesFullName)) return null;
            // получить серии от графика
            if (seriesFullName == Localizer.GetString("TitleCourse")) return chart.chart.StockSeries;

            var nameParts = seriesFullName.Split(new[] { Separators.IndiNameDelimiter[0] }, 
                StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length < 2)
            {
                Logger.InfoFormat("GetOuterSeries (chart {0}, {1}): series \"{0}\" is bad-formatted",
                    chart.Symbol, chart.Timeframe.ToString(), seriesFullName);
                return null;
            }
            var indiName = nameParts[0];
            var seriesName = nameParts[1];
            var indi = chart.indicators.FirstOrDefault(i => i.UniqueName == indiName);
            if (indi == null) return null;
            // найти серию
            return indi.SeriesResult.FirstOrDefault(s => s.Name == seriesName);
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            // инициализируем индикатор
            EntitleIndicator();
            seriesTrendLine = new TrendLineSeries(Name);
            SeriesResult = new List<Series.Series> { seriesTrendLine };
        }

        public void Remove()
        {
        }

        public void AcceptSettings()
        {
            // временно пусто
        }

        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (newCandles == null) return;
            if (newCandles.Count == 0) return;
            BuildSeries(owner);
        }

        public string GetHint(int x, int y, double index, double price, int tolerance)
        {
            var line = seriesTrendLine.data.FirstOrDefault(l =>
                                                Math.Round(l.linePoints[0].X) == Math.Round(index));
            return line != null ? line.Comment : string.Empty;
        }

        public List<IChartInteractiveObject> GetObjectsUnderCursor(int screenX, int screenY, int tolerance)
        {
            return new List<IChartInteractiveObject>();
        }
    }

    /// <summary>
    /// ТФ, формула индекса, настройки дивергенций
    /// </summary>
    public class MultyTimeframeIndexSettings
    {
        /// <summary>
        /// UniqueId чарта, на который ссылается индикатор
        /// </summary>
        public string chartId;

        /// <summary>
        /// заполняется в коде ActualizeSeries
        /// </summary>
        public string TimeframeAndSymbol { get; set; }

        /// <summary>
        /// серия указанного чарта, с которой берутся данные
        /// источник дивергенций (индекс)
        /// </summary>
        public string FullyQualifiedSeriesSrcName { get; set; }

        /// <summary>
        /// серия указанного чарта, с которой берутся данные
        /// цель дивергенций (курс)
        /// </summary>
        public string FullyQualifiedSeriesDestName { get; set; }

        /// <summary>
        /// инвертировть диверы: бычьи становятся медвежьими и наоборот
        /// </summary>
        public bool InverseDivergency { get; set; }

        /// <summary>
        /// "индекс" - обновляется в коде ActualizeSeries
        /// </summary>
        public Series.Series seriesSrc;
        
        /// <summary>
        /// "курс" - обновляется в коде ActualizeSeries
        /// </summary>
        public Series.Series seriesDest;
        
        #region настройки дивергенций
        public IndicatorDiver.DivergenceType DiverType { get; set; }
        public int periodExtremum = 6;
        public int maxPastExtremum = 18;
        public double marginUpper = 1;
        public double marginLower = -1;
        #endregion

        public float? GetSourcePrice(int index)
        {
            return ((IPriceQuerySeries) seriesSrc).GetPrice(index);
        }

        public float? GetDestPrice(int index)
        {
            return ((IPriceQuerySeries)seriesDest).GetPrice(index);
        }

        public MultyTimeframeIndexSettings MakeCopy()
        {
            return new MultyTimeframeIndexSettings
                       {
                           chartId = chartId,
                           FullyQualifiedSeriesSrcName = FullyQualifiedSeriesSrcName,
                           FullyQualifiedSeriesDestName = FullyQualifiedSeriesDestName,
                           DiverType = DiverType,
                           periodExtremum = periodExtremum,
                           maxPastExtremum = maxPastExtremum,
                           marginUpper = marginUpper,
                           marginLower = marginLower,
                           TimeframeAndSymbol = TimeframeAndSymbol,
                           InverseDivergency = InverseDivergency
                       };
        }
    }

    /// <summary>
    /// описывает отметку на графике: знак дивера,
    /// таймфрейм и валютная пара
    /// </summary>
    struct DiverMarker
    {
        public int sign;
        public string title;
        
        public DiverMarker(int sign)
        {
            this.sign = sign;
            title = string.Empty;
        }

        public DiverMarker(int sign, string title)
        {
            this.sign = sign;
            this.title = title;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class CrossChartSeriesSetAttribute : CustomXmlSerializationAttribute
    {
        public override void SerializeProperty(object proValue, XmlNode parent)
        {
            var sets = (List<MultyTimeframeIndexSettings>) proValue;
            foreach (var set in sets)
            {
                var node = parent.AppendChild(parent.OwnerDocument.CreateElement("chartSeries"));
                var atr = node.Attributes.Append(parent.OwnerDocument.CreateAttribute("chartId"));
                atr.Value = set.chartId;

                atr = node.Attributes.Append(parent.OwnerDocument.CreateAttribute("seriesDest"));
                atr.Value = set.FullyQualifiedSeriesDestName;

                atr = node.Attributes.Append(parent.OwnerDocument.CreateAttribute("seriesSrc"));
                atr.Value = set.FullyQualifiedSeriesSrcName;

                atr = node.Attributes.Append(parent.OwnerDocument.CreateAttribute("tfSmb"));
                atr.Value = set.TimeframeAndSymbol;

                atr = node.Attributes.Append(parent.OwnerDocument.CreateAttribute("diverType"));
                atr.Value = set.DiverType.ToString();

                atr = node.Attributes.Append(parent.OwnerDocument.CreateAttribute("maxPastExtr"));
                atr.Value = set.maxPastExtremum.ToString();

                atr = node.Attributes.Append(parent.OwnerDocument.CreateAttribute("periodExtremum"));
                atr.Value = set.periodExtremum.ToString();

                atr = node.Attributes.Append(parent.OwnerDocument.CreateAttribute("marginUpper"));
                atr.Value = set.marginUpper.ToStringUniform();

                atr = node.Attributes.Append(parent.OwnerDocument.CreateAttribute("marginLower"));
                atr.Value = set.marginLower.ToStringUniform();
            }
        }

        public override object DeserializeProperty(XmlNode parent)
        {
            var sets = new List<MultyTimeframeIndexSettings>();
            foreach (var child in parent.ChildNodes)
            {
                if (child is XmlElement == false) continue;
                var node = (XmlElement) child;
                if (node.Name != "chartSeries") continue;

                var set = new MultyTimeframeIndexSettings();
                set.chartId = node.Attributes["chartId"].Value;
                set.FullyQualifiedSeriesDestName = node.Attributes["seriesDest"].Value;
                set.FullyQualifiedSeriesSrcName = node.Attributes["seriesSrc"].Value;
                set.TimeframeAndSymbol = node.Attributes["tfSmb"].Value;
                set.DiverType = (IndicatorDiver.DivergenceType)
                    Enum.Parse(typeof(IndicatorDiver.DivergenceType), node.Attributes["diverType"].Value);
                set.maxPastExtremum = node.Attributes["maxPastExtr"].Value.ToInt();
                set.periodExtremum = node.Attributes["periodExtremum"].Value.ToInt();
                set.marginUpper = node.Attributes["marginUpper"].Value.ToDoubleUniform();
                set.marginLower = node.Attributes["marginLower"].Value.ToDoubleUniform();

                sets.Add(set);
            }
            return sets;
        }
    }
}

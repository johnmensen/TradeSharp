using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Candlechart.Chart;
using Candlechart.ChartMath;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    [LocalizedDisplayName("TitleOptionLevels")]
    [LocalizedCategory("TitleStockIndicatorsShort")]
    [TypeConverter(typeof(PropertySorter))]
    public class OptionLevelIndicator : BaseChartIndicator, IChartIndicator
    {
        public override BaseChartIndicator Copy()
        {
            var opt = new OptionLevelIndicator();
            Copy(opt);
            return opt;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var opt = (OptionLevelIndicator) indi;
            CopyBaseSettings(opt);
            opt.options.AddRange(options);
            opt.seriesSpan = seriesSpan;
            opt.lineHints.Clear();
            foreach (var hint in lineHints)
                opt.lineHints.Add(hint.Key, hint.Value);
            opt.AllowedOptionTypes = AllowedOptionTypes;
            opt.AllowedOptionStyles = AllowedOptionStyles;
            opt.LevelType = LevelType;
            opt.WeightType = WeightType;
            opt.ChannelID = ChannelID;
            opt.MinOI = MinOI;
            opt.MinVolume = MinVolume;
            opt.LengthHours = LengthHours;
            opt.StretchYAxis = StretchYAxis;
            opt.LabelLeft = LabelLeft;
            opt.LabelRight = LabelRight;
            opt.ColorPut = ColorPut;
            opt.ColorGet = ColorGet;
            opt.DaysToHighlight = DaysToHighlight;
            opt.MaxPointsDistanceToShow = MaxPointsDistanceToShow;
            opt.needRebuildLevels = needRebuildLevels;
            opt.SummaryLevelStyle = SummaryLevelStyle;
        }

        [Browsable(false)]
        public override string Name { get { return Localizer.GetString("TitleOptionLevels"); } }

        private readonly List<OptionData> options = new List<OptionData>();
        private SeriesSpanWithText seriesSpan = new SeriesSpanWithText(Localizer.GetString("TitleLevelsOrSegments"));
        private readonly Dictionary<SpanWithText, string> lineHints = new Dictionary<SpanWithText, string>();
        private readonly LineSeries seriesChannelMid = new LineSeries("mid line") { Transparent = true, LineColor = Color.Green};
        private readonly LineSeries seriesChannelCall = new LineSeries("call line") { Transparent = true };
        private readonly LineSeries seriesChannelPut = new LineSeries("put line") { Transparent = true };
        
        public enum OptionLevelType
        {
            Раздельные = 0,
            Суммарные
        }

        public enum OptionLevelWeightType
        {
            ОткрытыйИнтерес = 0,
            Объем
        }

        public enum AllowedOptionType
        {
            Все = 0, CALL, PUT
        }

        public enum AllowedOptionStyle
        {
            Все = 0, American, European
        }

        public enum LevelLabel
        {
            Нет = 0, ОткрИнтерес, Объем, Экспирация
        }

        [LocalizedDisplayName("TitleShowCallPut")]
        [Description("Отображать CALL/PUT или все опционы")]
        [LocalizedCategory("TitleMain")]
        public AllowedOptionType AllowedOptionTypes { get; set; }

        [LocalizedDisplayName("TitleShowAmericanEurope")]
        [Description("Отображать американские/европейские/все")]
        [LocalizedCategory("TitleMain")]
        public AllowedOptionStyle AllowedOptionStyles { get; set; }

        private OptionLevelType levelType = OptionLevelType.Суммарные;
        [LocalizedDisplayName("TitleUnionOfLevels")]
        [Description("Тип уровней: отдельно или суммарные CALL - PUT")]
        [LocalizedCategory("TitleMain")]
        public OptionLevelType LevelType
        {
            get { return levelType; }
            set { levelType = value; }
        }

        public enum SummaryLevelLineStyle { Каналы = 0, Раздельные }
        [LocalizedDisplayName("TitleSummaryLevelsLineStyleShort")]
        [Description("Тип линий для суммарных уровней")]
        [LocalizedCategory("TitleMain")]
        public SummaryLevelLineStyle SummaryLevelStyle { get; set; }

        private OptionLevelWeightType weightType = OptionLevelWeightType.ОткрытыйИнтерес;
        [LocalizedDisplayName("TitleUnionType")]
        [Description("В качестве веса берется либо открытый интерес, либо объем")]
        [LocalizedCategory("TitleMain")]
        public OptionLevelWeightType WeightType
        {
            get { return weightType; }
            set { weightType = value; }
        }

        private int channelID = 2;
        [LocalizedDisplayName("TitleChannel")]
        [Description("ID канала, содержащего биржевые данные")]
        [LocalizedCategory("TitleMain")]
        public int ChannelID
        {
            get { return channelID; }
            set { channelID = value; }
        }

        private int minOI = 500;
        [LocalizedDisplayName("TitleMinmalOpenInterestShort")]
        [Description("Мин. открытый интерес для отображения на графике")]
        [LocalizedCategory("TitleMain")]
        public int MinOI
        {
            get { return minOI; }
            set { minOI = value; }
        }

        private int minVolume = 0;
        [LocalizedDisplayName("TitleMinimalVolume")]
        [Description("Мин. объем для отображения на графике")]
        [LocalizedCategory("TitleMain")]
        public int MinVolume
        {
            get { return minVolume; }
            set { minVolume = value; }
        }

        private int lengthHours = 24;
        [LocalizedDisplayName("TitleLengthInHours")]
        [Description("Длина отрезка, часов")]
        [LocalizedCategory("TitleMain")]
        public int LengthHours
        {
            get { return lengthHours; }
            set { lengthHours = value; }
        }

        [LocalizedDisplayName("TitleScaleY")]
        [Description("Растягивать ось Y под уровни")]
        [LocalizedCategory("TitleMain")]
        public bool StretchYAxis
        {
            get { return seriesSpan.StretchYAxis; }
            set { seriesSpan.StretchYAxis = value; }
        }

        [LocalizedDisplayName("TitleLeftLabel")]
        [Description("Текст для метки слева")]
        [LocalizedCategory("TitleVisuals")]
        public LevelLabel LabelLeft { get; set; }

        [LocalizedDisplayName("TitleRightLabel")]
        [Description("Текст для метки справа")]
        [LocalizedCategory("TitleVisuals")]
        public LevelLabel LabelRight { get; set; }
        
        private Color colorPut = Color.Red;
        [LocalizedDisplayName("TitlePutColor")]
        [Description("Цвет PUT-уровней")]
        [LocalizedCategory("TitleVisuals")]
        public Color ColorPut
        {
            get { return colorPut; }
            set { colorPut = value;  }
        }

        private Color colorGet = Color.Blue;
        [LocalizedDisplayName("TitleCallColor")]
        [Description("Цвет CALL-уровней")]
        [LocalizedCategory("TitleVisuals")]
        public Color ColorGet
        {
            get { return colorGet; }
            set { colorGet = value; }
        }

        private int daysToHighlight = 15;
        [LocalizedDisplayName("TitleHighlightInDays")]
        [Description("Выделять опционы за N дней до истечения")]
        [LocalizedCategory("TitleVisuals")]
        public int DaysToHighlight
        {
            get { return daysToHighlight; }
            set { daysToHighlight = value; }
        }

        [LocalizedDisplayName("TitleMaximumNumberOfPointToMarketShort")]
        [Description("Скрывать уровни дальше N пп от рынка (0 - показ всех)")]
        [LocalizedCategory("TitleVisuals")]
        public int MaxPointsDistanceToShow { get; set; }

        [LocalizedDisplayName("TitleCreateDrawingPanel")]
        [LocalizedCategory("TitleMain")]
        [Description("Создавать свою панель отрисовки")]
        [Browsable(false)]
        public override bool CreateOwnPanel { get; set; }

        /// <summary>
        /// взводится при изменении параметров индюка,
        /// обновления списка OptionData...
        /// если всзведен - BuildSeries() обновляет опционные уровни (series...)
        /// </summary>
        private bool needRebuildLevels = true;

        public void BuildSeries(ChartControl chart)
        {
            if (!needRebuildLevels) return;

            seriesSpan.data.Clear();
            seriesChannelCall.Data.Clear();
            seriesChannelMid.Data.Clear();
            seriesChannelPut.Data.Clear();
            lineHints.Clear();

            var candles = chart.StockSeries.Data.Candles;
            if (candles.Count == 0) return;
            
            // добавить опционные уровни
            if (levelType == OptionLevelType.Раздельные) BuildSeparateOL();
            else BuildSummaryOL();
            needRebuildLevels = false;
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            // инициализируем индикатор
            SeriesResult = new List<Series.Series> { seriesSpan };
            EntitleIndicator();
            
            owner.Owner.OnNewsReceived += OnNewsReceived;
            // показать уже существующие уровни
            needRebuildLevels = true;
            List<News> lstNews;
            NewsStorage.Instance.ReadNews(channelID, out lstNews);
            if (lstNews != null)
                if (lstNews.Count > 0)
                    ProcessNews(lstNews.ToArray());
        }

        /// <summary>
        /// построить отдельные уровни
        /// </summary>
        private void BuildSeparateOL()
        {
            foreach (var opt in options)
            {
                //if (opt.DatePublished > owner.EndTime) continue;
                if (opt.OpenInterest < minOI || opt.Volume < minVolume) continue;
                if ((AllowedOptionTypes == AllowedOptionType.CALL && opt.Type == OptionType.PUT)
                    || (AllowedOptionTypes == AllowedOptionType.PUT && opt.Type == OptionType.CALL))
                    continue;
                if ((AllowedOptionStyles == AllowedOptionStyle.American && opt.Style == OptionStyle.European)
                    || (AllowedOptionStyles == AllowedOptionStyle.European && opt.Style == OptionStyle.American))
                    continue;

                AddOptionLevel(opt, false);
            }            
        }

        private void BuildSummaryOL()
        {
            if (options.Count == 0) return;

            // count date - level * [OI | Volume], sum OI, sum Volume
            var dicSumCall = new Dictionary<DateTime, Cortege3<float, float, float>>();
            var dicSumPut = new Dictionary<DateTime, Cortege3<float, float, float>>();

            foreach (var opt in options)
            {
                if ((AllowedOptionStyles == AllowedOptionStyle.American && opt.Style == OptionStyle.European)
                    || (AllowedOptionStyles == AllowedOptionStyle.European && opt.Style == OptionStyle.American))
                    continue;

                var multiplier = WeightType == OptionLevelWeightType.ОткрытыйИнтерес
                                     ? opt.OpenInterest
                                     : opt.Volume;

                if (opt.Type == OptionType.CALL)
                {
                    var sumCall = opt.OptionLevelClose*multiplier;
                    if (!dicSumCall.ContainsKey(opt.DatePublished))
                        dicSumCall.Add(opt.DatePublished,
                                       new Cortege3<float, float, float>(sumCall, opt.OpenInterest, opt.Volume));
                    else
                    {
                        sumCall += dicSumCall[opt.DatePublished].a;
                        var totalOI = dicSumCall[opt.DatePublished].b + opt.OpenInterest;
                        var totalVolume = dicSumCall[opt.DatePublished].c + opt.Volume;
                        dicSumCall[opt.DatePublished] =
                            new Cortege3<float, float, float>(sumCall, totalOI, totalVolume);
                    }
                }
                else
                {
                    var sumPut = opt.OptionLevelClose*multiplier;
                    if (!dicSumPut.ContainsKey(opt.DatePublished))
                        dicSumPut.Add(opt.DatePublished,
                                      new Cortege3<float, float, float>(sumPut, opt.OpenInterest, opt.Volume));
                    else
                    {
                        sumPut += dicSumPut[opt.DatePublished].a;
                        var totalOI = dicSumPut[opt.DatePublished].b + opt.OpenInterest;
                        var totalVolume = dicSumPut[opt.DatePublished].c + opt.Volume;
                        dicSumPut[opt.DatePublished] =
                            new Cortege3<float, float, float>(sumPut, totalOI, totalVolume);
                    }
                }
            }

            var sumOpts = new List<OptionData>();
            foreach (var date in dicSumCall)
            {
                if (date.Value.a == 0) continue;
                var strike = date.Value.a/(WeightType == OptionLevelWeightType.ОткрытыйИнтерес 
                    ? date.Value.b : date.Value.c);
                var opt = new OptionData(OptionType.CALL, OptionStyle.American,
                                         options[0].BaseActive, date.Key,
                                         new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                                         strike, 0, 0, 0, (int) date.Value.b, (int) date.Value.c);
                sumOpts.Add(opt);
            }
            foreach (var date in dicSumPut)
            {
                if (date.Value.a == 0) continue;
                var strike = date.Value.a/(WeightType == OptionLevelWeightType.ОткрытыйИнтерес
                                               ? date.Value.b
                                               : date.Value.c);
                var opt = new OptionData(OptionType.PUT, OptionStyle.American,
                                         options[0].BaseActive, date.Key,
                                         new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                                         strike, 0, 0, 0, (int) date.Value.b, (int) date.Value.c);
                sumOpts.Add(opt);
            }
            // медианный уровень
            List<Cortege2<DateTime, float>> channelCall = null, channelPut = null, channelMid = null;
            if (SummaryLevelStyle == SummaryLevelLineStyle.Каналы)
            {
                channelCall = new List<Cortege2<DateTime, float>>();
                channelPut = new List<Cortege2<DateTime, float>>();
                channelMid = new List<Cortege2<DateTime, float>>();
            }

            foreach (var callData in dicSumCall)
            {
                if (!dicSumPut.ContainsKey(callData.Key)) continue;
                var putData = dicSumPut[callData.Key];

                var oi = callData.Value.b + putData.b;
                var volume = callData.Value.c + putData.c;
                var denominator = WeightType == OptionLevelWeightType.ОткрытыйИнтерес ? oi : volume;
                var strike = (callData.Value.a + putData.a)/denominator;

                var opt = new OptionData(WeightType == OptionLevelWeightType.ОткрытыйИнтерес
                                             ? callData.Value.b > putData.b ? OptionType.CALL : OptionType.PUT
                                             : callData.Value.c > putData.c ? OptionType.CALL : OptionType.PUT,
                                         OptionStyle.American,
                                         options[0].BaseActive, callData.Key,
                                         new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                                         strike, 0, 0, 0, (int) oi, (int) volume);
                
                // добавить мед. уровень
                if (SummaryLevelStyle == SummaryLevelLineStyle.Каналы)
                    channelMid.Add(new Cortege2<DateTime, float>(
                                       opt.DatePublished,
                                       opt.OptionLevelClose));
                else
                    AddOptionLevel(opt, true);
            }

            foreach (var opt in sumOpts)
            {
                if (SummaryLevelStyle == SummaryLevelLineStyle.Каналы)
                {
                    var pivot = new Cortege2<DateTime, float>(opt.DatePublished, opt.OptionLevelClose);
                    if (opt.Type == OptionType.CALL) channelCall.Add(pivot);
                    else channelPut.Add(pivot);
                }
                else 
                    AddOptionLevel(opt, false);
            }
            // построить каналы a-la Bollinger?
            if (SummaryLevelStyle == SummaryLevelLineStyle.Каналы)
            {
                BuildLineSeriesFromPivots(channelCall, seriesChannelCall);
                BuildLineSeriesFromPivots(channelPut, seriesChannelPut);
                BuildLineSeriesFromPivots(channelMid, seriesChannelMid);
            }
        }

        private void BuildLineSeriesFromPivots(List<Cortege2<DateTime, float>> pivots, LineSeries targetSeries)
        {
            if (pivots.Count == 0) return;
            var curIndex = 0;
            var candles = owner.StockSeries.Data.Candles;
            if (candles.Count == 0) return;
            targetSeries.Data.Add(candles[0].close);
            foreach (var candle in candles)
            {
                var curLevel = candle.close;
                var candleDate = candle.timeOpen;
                var itemIndex = pivots.FindIndex(curIndex, p => p.a > candleDate);
                if (itemIndex < 0) break;
                if (itemIndex > 0)
                {
                    var prevPivot = pivots[itemIndex - 1];
                    if (prevPivot.a <= candleDate)
                    {
                        curIndex = itemIndex;
                        curLevel = pivots[itemIndex - 1].b;
                    }
                }
                targetSeries.Data.Add(curLevel);
            }
        }

        private void AddOptionLevel(OptionData opt, bool isMedian)
        {
            var color = isMedian ? Color.Green : opt.Type == OptionType.PUT ? colorPut : colorGet;

            var linePart = new SpanWithText
                               {
                                   Price = opt.OptionLevelClose,
                                   LineColor = color,
                                   Name = string.Format("{0} {1:f4} OI:{2}", opt.Type, opt.OptionLevelClose, opt.OpenInterest)
                               };
            var indexStart = owner.StockSeries.GetIndexByCandleOpen(opt.DatePublished);
            var indexEnd = owner.StockSeries.GetIndexByCandleOpen(opt.DatePublished.AddHours(LengthHours));

            // проверить расстояние до рынка в пп
            // как мин. расстояние до уровня
            if (MaxPointsDistanceToShow > 0)
            {
                var candles = owner.StockSeries.Data.Candles;
                var distanceAbs = float.MaxValue;
                for (var i = indexStart < 0 ? 0 : indexStart; i < indexEnd; i++)
                {
                    if (i >= candles.Count) break;
                    var dist = Math.Min(
                        Math.Abs(candles[i].high - opt.OptionLevelClose),
                        Math.Abs(candles[i].low - opt.OptionLevelClose));
                    if (dist < distanceAbs) distanceAbs = dist;
                }
                var distPoints = DalSpot.Instance.GetPointsValue(owner.Symbol, distanceAbs);
                if (distPoints > MaxPointsDistanceToShow) return;
            }

            linePart.StartIndex = indexStart;
            linePart.EndIndex = indexEnd;
            var daysLeft = (opt.DateExpiration - opt.DatePublished).TotalDays;
            if (daysLeft <= daysToHighlight) linePart.LineWidth = 2;
            // показать метки
            if (LabelLeft != LevelLabel.Нет) AddOptionLabel(linePart, LabelLeft, true, opt);
            if (LabelRight != LevelLabel.Нет) AddOptionLabel(linePart, LabelRight, false, opt);
            seriesSpan.data.Add(linePart);
            // подсказка по уровню
            lineHints.Add(linePart, string.Format("{0:f4}{1} {2} (strike:{3:f4} close:{4:f4} OI:{5} volm:{6})",
                                                  opt.OptionLevelClose,
                                                  opt.Type,
                                                  opt.Style == OptionStyle.American ? "AM" : "EU",
                                                  opt.StrikePrice,
                                                  opt.ContractClose,
                                                  opt.OpenInterest,
                                                  opt.Volume));
        }

        private static void AddOptionLabel(SpanWithText span, LevelLabel label, bool isLeft, OptionData opt)
        {
            string text = label == LevelLabel.Объем
                              ? opt.Volume.ToString()
                              : label == LevelLabel.ОткрИнтерес
                                    ? opt.OpenInterest.ToString()
                                    : string.Format("{0:MMMyy}", opt.DateExpiration);

            if (isLeft) span.TextLeft = text;
            else span.TextRight = text;
        }

        void OnNewsReceived(News[] news)
        {
            ProcessNews(news);
        }

        private void ProcessNews(News[] news)
        {
            if (news.Length == 0) return;
            needRebuildLevels = true;
            // добавить уровни
            foreach (var ns in news)
            {
                if (ns.ChannelId != channelID || string.IsNullOrEmpty(ns.Body)) continue;
                var opt = OptionData.Parse(ns.Body);
                if (opt == null) continue;
                // проверить - совпадает ли базовый актив
                if (opt.BaseActive != owner.Symbol) continue;
                options.Add(opt);
            }
            BuildSeries(owner);
        }

        public void Remove()
        {
            if (seriesSpan != null) seriesSpan.data.Clear();
            owner.Owner.OnNewsReceived -= OnNewsReceived;
        }

        public void AcceptSettings()
        {
            needRebuildLevels = true;
            // серии
            if (SummaryLevelStyle == SummaryLevelLineStyle.Раздельные ||
                LevelType == OptionLevelType.Раздельные)
                SeriesResult = new List<Series.Series> { seriesSpan };
            else
            {
                seriesChannelCall.LineColor = ColorGet;
                seriesChannelPut.LineColor = ColorPut;                
                SeriesResult = new List<Series.Series> {seriesChannelCall, seriesChannelMid, seriesChannelPut};
            }
        }

        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (newCandles != null)
                if (newCandles.Count > 0)
                    BuildSeries(owner);
        }

        public string GetHint(int x, int y, double index, double price, int tolerance)
        {
            return string.Empty;
            //if (seriesSpan.Owner == null) return string.Empty;
            //var ptClient = seriesSpan.Owner.PointToClient(new Point(x, screenY));
            //var sb = new StringBuilder();
            //foreach (var line in seriesSpan.data)
            //{
            //    if (!IsInLine(line, ptClient.X, ptClient.Y, tolerance)) continue;
            //    if (lineHints.ContainsKey(line)) sb.AppendLine(lineHints[line]);
            //}
            //return sb.ToString();
        }

        public List<IChartInteractiveObject> GetObjectsUnderCursor(int screenX, int screenY, int tolerance)
        {
            return new List<IChartInteractiveObject>();
            //var objList = new List<IChartInteractiveObject>();
            //if (seriesSpan.Owner == null) return objList;
            //var ptClient = seriesSpan.Owner.PointToClient(new Point(screenX, screenY));

            //foreach (var line in seriesSpan.data)
            //{
            //    if (IsInLine(line, ptClient.X, ptClient.Y, tolerance)) objList.Add(line);
            //}
            //return objList;
        }

        private bool IsInLine(SpanWithText line, int x, int y, int tolerance)
        {
            // чувствительность, пикс                        
            if (seriesSpan.Owner == null) return false;            
                        
            // расстояние до прямой в экранных коорд.
            var ptA = Conversion.WorldToScreen(new PointD(line.StartIndex, (double)line.Price),
                                                      seriesSpan.Owner.WorldRect, seriesSpan.Owner.CanvasRect);            
            var ptB = Conversion.WorldToScreen(new PointD(line.EndIndex, (double)line.Price),
                                                      seriesSpan.Owner.WorldRect, seriesSpan.Owner.CanvasRect);
            //screenPointA = seriesSpan.Owner.CanvasPointToClientPoint(screenPointA);
            //screenPointB = seriesSpan.Owner.CanvasPointToClientPoint(screenPointB);

            var distX = x < ptA.X ? (ptA.X - x) : x > ptB.X ? (ptB.X - x) : 0;
            var distY = Math.Abs(y - ptA.Y);
            return distX < tolerance && distY < tolerance;
        }
    }
}

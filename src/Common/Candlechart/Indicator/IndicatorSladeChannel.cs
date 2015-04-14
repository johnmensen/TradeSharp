using System;
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
    [LocalizedDisplayName("TitleSlidingChannel")]
    [LocalizedCategory("TitleGraphicsAnalysisShort")]
    [TypeConverter(typeof(PropertySorter))]
    public class IndicatorSladeChannel : BaseChartIndicator, IChartIndicator
    {
        public override BaseChartIndicator Copy()
        {
            var sc = new IndicatorSladeChannel();
            Copy(sc);
            return sc;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var sc = (IndicatorSladeChannel)indi;
            CopyBaseSettings(sc);
            sc.ClUpLine = ClUpLine;
            sc.ClDownLine = ClDownLine;
            sc.LineStyle = LineStyle;
            sc.LineWidth = LineWidth;
            sc.MaxBody = MaxBody;
            sc.MinBody = MinBody;
            sc.CountBefore = CountBefore;
            sc.CountAfter = CountAfter;
            sc.ChannelState = ChannelState;
            sc.ShowAllChannels = ShowAllChannels;
            sc.CountForward = CountForward;
            sc.TradePoints = TradePoints;
        }

        [Browsable(false)]
        public override string Name { get { return Localizer.GetString("TitleSlidingChannel"); } }

        [LocalizedDisplayName("TitleCreateDrawingPanel")]
        [LocalizedCategory("TitleMain")]
        [Description("Создавать свою панель отрисовки")]
        [Browsable(false)]
        public override bool CreateOwnPanel { get; set; }
        
        #region Визуальные настройки

        private Color clUpLine = Color.Blue;
        [LocalizedDisplayName("TitleHighsColor")]
        [Description("Цвет канала, построенного по максимумам")]
        [LocalizedCategory("TitleVisuals")]
        [PropertyXMLTag("Robot.UpLineColor")]
        public Color ClUpLine
        {
            get { return clUpLine; }
            set { clUpLine = value; }
        }
        
        private Color clDownLine = Color.Green;
        [LocalizedDisplayName("TitleLowsColor")]
        [Description("Цвет канала, построенного по минимумам")]
        [LocalizedCategory("TitleVisuals")]
        [PropertyXMLTag("Robot.DownLineColor")]
        public Color ClDownLine
        {
            get { return clDownLine; }
            set { clDownLine = value; }
        }

        private DashStyle lineStyle = DashStyle.Solid;
        [LocalizedDisplayName("TitleLineStyle")]
        [Description("Стиль линии индикатора (сплошная, штирх, штрих-пунктир...)")]
        [LocalizedCategory("TitleVisuals")]
        [PropertyXMLTag("Robot.LineStyle")]
        public DashStyle LineStyle
        {
            get { return lineStyle; }
            set { lineStyle = value; }
        }

        private decimal lineWidth = 1;
        [LocalizedDisplayName("TitleThickness")]
        [Description("Толщина линии индикатора, пикселей")]
        [LocalizedCategory("TitleVisuals")]
        [PropertyXMLTag("Robot.LineWidts")]
        public decimal LineWidth
        {
            get { return lineWidth; }
            set { lineWidth = value; }
        }

        #endregion

        #region основные настройки

        [LocalizedDisplayName("TitleSourceSeries")]
        [LocalizedCategory("TitleMain")]
        [Browsable(false)]
        public virtual string SeriesSourcesDisplay { get; set; }
        
        private int maxBody = 20;
        [LocalizedDisplayName("TitleMaximumCandleCountInChannelShort")]
        [Description("Максимальное количество свечей внутри канала")]
        [LocalizedCategory("TitleMain")]
        public int MaxBody
        {
            get { return maxBody; }
            set { maxBody = value; }
        }
        
        private int minBody = 3;
        [LocalizedDisplayName("TitleMinimumCandleCountInChannelShort")]
        [Description("Минимальное количество свечей внутри канала")]
        [LocalizedCategory("TitleMain")]
        public int MinBody
        {
            get { return minBody; }
            set { minBody = value; }
        }
        
        private int countBefore = 1;
        [LocalizedDisplayName("TitleCandleCountBeforeExtremumInChannelShort")]
        [Description("Количество свечей до экстремума внутри канала")]
        [LocalizedCategory("TitleMain")]
        public int CountBefore
        {
            get { return countBefore; }
            set { countBefore = value; }
        }

        private int countAfter = 2;
        [LocalizedDisplayName("TitleCandleCountAfterExtremumInChannelShort")]
        [Description("Количество свечей после экстремума внутри канала")]
        [LocalizedCategory("TitleMain")]
        public int CountAfter
        {
            get { return countAfter; }
            set { countAfter = value; }
        }

        private ChannelStateInfo channelState = ChannelStateInfo.НетКанала;
        [LocalizedDisplayName("TitleIndicatorState")]
        [Description("Тип текущего построенного канала")]
        [LocalizedCategory("TitleMain")]
        public ChannelStateInfo ChannelState 
        {
            get { return channelState; }
            set { channelState = value; }
        }

        private bool showAllChannels = true;
        [LocalizedDisplayName("TitleDrawOutdatedChannels")]
        [Description("Показать на графике все каналы")]
        [LocalizedCategory("TitleMain")]
        public bool ShowAllChannels
        {
            get { return showAllChannels; }
            set { showAllChannels = value; }
        }

        #endregion

        [Serializable]
        public enum ChannelStateInfo
        {
            НетКанала = 0, ПостроенПоМаксимумам, ПостроенПоМинимумам
        }

        private bool tradePoints = true;
        [LocalizedDisplayName("TitleShowEnterPoints")]
        [Description("Рисовать точки входов на касании цены границы канала")]
        [LocalizedCategory("TitleMain")]
        public bool TradePoints
        {
            get { return tradePoints; }
            set { tradePoints = value; }
        }

        private int countForward = 4;
        [LocalizedDisplayName("TitleForwardChannelDrawing")]
        [Description("Количество свечей после экстремума, на сколько продлевать линии старых каналов")]
        [LocalizedCategory("TitleMain")]
        public int CountForward
        {
            get { return countForward; }
            set { countForward = value; }
        }

        private SeriesAsteriks commentSeries;
        private TrendLineSeries series;
        private Cortege3<PointD, PointD, PointD> currChannel = new Cortege3<PointD, PointD, PointD>();

        // x координата точки b активного канала, чтобы следующий четный не рисовался в истории
        private double lastIndexB;
        // x координата точки a активного канала, чтобы следующий четный не рисовался в истории
        private double lastIndexA;

        public IndicatorSladeChannel()
        {
            CreateOwnPanel = false;
            ChannelState = ChannelStateInfo.НетКанала;
        }

        public void BuildSeries(ChartControl chart)
        {
            //series.data.Clear();
            // строим линию
            var sources = (StockSeries)SeriesSources[0];
            var candles = sources.Data.Candles;
            if (candles.Count == 0 || candles.Count <= maxBody * 3) return;

            // это нужно чтобы нарисовался на загрузке тот канал который указан в настройках
            if (series.data.Count == 0)
                switch (ChannelState)
                {
                    case ChannelStateInfo.ПостроенПоМаксимумам:
                        ChannelState = ChannelStateInfo.ПостроенПоМинимумам;
                        break;
                    case ChannelStateInfo.ПостроенПоМинимумам:
                        ChannelState = ChannelStateInfo.ПостроенПоМаксимумам;
                        break;
                }
            
            if (series.data.Count == 0 && ShowAllChannels)
            {
                for (var i = candles.Count - 1; i >= maxBody * 3; i--)
                {
                    BuildChannel(candles, i);
                }
            }
            else
            {
                // если надо, то все каналы отрисованы
                BuildChannel(candles, candles.Count - 1);
            }
        }

        private void BuildChannel(List<CandleData> candles, int from)
        {
            var channel = new Cortege3<PointD, PointD, PointD> { };
            var state = ChannelStateInfo.НетКанала;
            switch (ChannelState)
            {
                case ChannelStateInfo.НетКанала:
                    // канала еще нет, ищем сначала по верхним точкам, если не находим, пробуем по нижним найти
                    var channelhigh = GetHighLineChannel(candles, from);
                    var channellow = GetLowLineChannel(candles, from);
                    if ((channelhigh.a == new PointD(0, 0) || channelhigh.b == new PointD(0, 0))
                        && (channellow.a == new PointD(0, 0) || channellow.b == new PointD(0, 0)))
                        break;

                    if (channelhigh.a == new PointD(0, 0) || channelhigh.b == new PointD(0, 0))
                    {
                        state = ChannelStateInfo.ПостроенПоМинимумам;
                        channel = channellow;
                        break;
                    }
                    if ((channellow.a == new PointD(0, 0) || channellow.b == new PointD(0, 0)))
                    {
                        state = ChannelStateInfo.ПостроенПоМаксимумам;
                        channel = channelhigh;
                        break;
                    }

                    if (channelhigh.b.X > channellow.b.X || (channelhigh.b.X == channellow.b.X && channelhigh.a.X > channellow.a.X))
                    {
                        state = ChannelStateInfo.ПостроенПоМаксимумам;
                        channel = channelhigh;
                        break;
                    }
                    state = ChannelStateInfo.ПостроенПоМинимумам;
                    channel = channellow;
                    break;
                    
                case ChannelStateInfo.ПостроенПоМаксимумам:
                    channel = GetLowLineChannel(candles, from);
                    if (channel.a == new PointD(0, 0) || channel.b == new PointD(0, 0))
                        return;
                    state = ChannelStateInfo.ПостроенПоМинимумам;
                    break;
                case ChannelStateInfo.ПостроенПоМинимумам:
                    channel = GetHighLineChannel(candles, from);
                    if (channel.a == new PointD(0, 0) || channel.b == new PointD(0, 0))
                        return;
                    state = ChannelStateInfo.ПостроенПоМаксимумам;
                    break;
            }
            
            

            if (lastIndexB == 0 && lastIndexA == 0)
            {
                lastIndexA = channel.a.X;
                lastIndexB = channel.b.X;
            }

            if (lastIndexB == channel.b.X && lastIndexA < channel.a.X)
            {
                // найденный канал старше текущего, игнорируем
                return;
            }

            if (!ShowAllChannels && lastIndexB > channel.b.X)
            {
                // найденный канал уже старый, игнорируем его
                channel = currChannel;
            }
            var a = channel.a;
            var b = channel.b;
            var k = (b.Y - a.Y) / (b.X - a.X);
            // стираем старый канал
            if (!ShowAllChannels)
                series.data.Clear();

            // находим точку на текущей свече + 1, туда и продлим канал
            var bx = 0;
            if (series.data.Count == 0)
                bx = candles.Count;
            else
                bx = (int)b.X + CountAfter + CountForward;

            if (TradePoints)
            {
                // просто покажем точки входа и все
                // посчитаем точки входа если они есть
                for (var i = (int)b.X + CountAfter + 1; i < b.X + CountAfter + CountForward; i++)
                {
                    if (candles.Count <= i) break;
                    var highY = b.Y + k*(i - b.X);
                    var lowY = channel.c.Y + k*(i - channel.c.X);
                    if (highY <= candles[i].high && highY >= candles[i].low)
                    {
                        var detailed = string.Format("S {0}", highY);
                        var tip = new AsteriskTooltip(detailed, detailed)
                        {
                            Price = (float)highY,
                            CandleIndex = i,
                            DateStart = candles[i].timeOpen,
                            Sign = "e",
                            Shape = AsteriskTooltip.ShapeType.СтрелкаВниз,
                            ColorFill = Color.Pink,
                            ColorLine = Color.Black,
                            ColorText = Color.Black
                        };
                        commentSeries.data.Add(tip);
                        break;
                    }
                    if (lowY >= candles[i].low && lowY <= candles[i].high)
                    {
                        var detailed = string.Format("B {0}", lowY);
                        var tip = new AsteriskTooltip(detailed, detailed)
                        {
                            Price = (float)lowY,
                            CandleIndex = i,
                            DateStart = candles[i].timeOpen,
                            Sign = "e",
                            Shape = AsteriskTooltip.ShapeType.СтрелкаВверх,
                            ColorFill = Color.Green,
                            ColorLine = Color.Black,
                            ColorText = Color.Black
                        };
                        commentSeries.data.Add(tip);
                        break;
                    }
                }
            }
            else
            {
                var line = new TrendLine
                               {
                                   LineColor = state == ChannelStateInfo.ПостроенПоМаксимумам ? ClUpLine : ClDownLine,
                                   LineStyle = TrendLine.TrendLineStyle.Отрезок
                               };
                // линия по двум экстремумам
                line.linePoints.Add(new PointD(a.X - countBefore, b.Y + k*(a.X - countBefore - b.X)));
                line.linePoints.Add(new PointD(bx, b.Y + k*(bx - b.X)));
                series.data.Add(line);


                line = new TrendLine
                           {
                               LineColor = state == ChannelStateInfo.ПостроенПоМаксимумам ? ClUpLine : ClDownLine,
                               LineStyle = TrendLine.TrendLineStyle.Отрезок
                           };
                var c = channel.c;
                // линия по одному экстремуму
                line.linePoints.Add(new PointD(a.X - countBefore, c.Y + k*(a.X - countBefore - c.X)));
                line.linePoints.Add(new PointD(bx, c.Y + k*(bx - c.X)));
                series.data.Add(line);
            }
            ChannelState = state;
            currChannel = channel;
            lastIndexA = currChannel.a.X;
            lastIndexB = currChannel.b.X;
        }
        
        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            // инициализируем индикатор
            EntitleIndicator();
            series = new TrendLineSeries(Name);
            commentSeries = new SeriesAsteriks(Name);
            SeriesResult = new List<Series.Series> { series, commentSeries};
        }

        public void Remove()
        {
            if (series != null) series.data.Clear();
            if (commentSeries != null) commentSeries.data.Clear();
        }

        public void AcceptSettings()
        {
            series.ForeColor = ClUpLine;
            series.LineWidth = (float)lineWidth;
            series.data.Clear();
            commentSeries.data.Clear();
        }

        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (newCandles != null)
                if (newCandles.Count > 0) BuildSeries(owner);
        }

        public string GetHint(int x, int y, double index, double price, int tolerance)
        {
            return string.Empty;
        }

        public List<IChartInteractiveObject> GetObjectsUnderCursor(int screenX, int screenY, int tolerance)
        {
            return new List<IChartInteractiveObject>();
        }

        private Cortege3<PointD, PointD, PointD> GetHighLineChannel(List<CandleData> candles, int from)
        {
            // цикл поиска точки b
            for (var i = from - 1 - countAfter; i >= from - 1 - countAfter - maxBody - 1; i--)
            {
                var b = new PointD(i, candles[i].high);
                // по всем свечкам делаем цикл построения линий от candles[i] до первой границы, ищем линию, в которую укладываются все свечи
                // то есть сдвигаем точку a на шаг назад и снова строим линии - ищем выполнение условий
                // цикл по точкам a
                for (var j = i - minBody - 1; j >= i - maxBody - 1; j--)
                {
                    // есть точки a и b вычисляем k - угловой коэффициент
                    var a = new PointD(j, candles[j].high);
                    var k = (b.Y - a.Y) / (b.X - a.X);
                    
                    // прогоняем проверочные циклы
                    var isFound = true;

                    for (var index = j - countBefore; index <= i + countAfter; index++)
                    {
                        if (index == j || index == i) continue;
                        // получаем значение y на индексе i прямой, проведенной через максимум свечи по индексу b_i
                        var yb = candles[index].high + k*(i - index);
                        if (yb <= b.Y) continue;
                        isFound = false;
                        break;
                    }
                    if (!isFound) continue;
                        
                    // условия выполнились, теперь надо проверить нижнюю границу, чтобы все свечки в нее вписались
                    var low = j;
                    var y_low = candles[low].low + k * (i - j);
                    for (var index = j; index <= i; index++)
                    {
                        var y_index = candles[index].low + k *(i - index);
                        if (y_low <= y_index) continue;
                        low = index;
                        y_low = y_index;
                    }
                    // теперь проверим свечи за экстремумами

                    for (var l = j - countBefore; l < j; l++)
                    {
                        var y = candles[l].low + k*(i - l);
                        if (y >= y_low) continue;
                        isFound = false;
                        break;
                    }
                    if (!isFound) continue;
                    for (var l = i + 1; l <= i + countAfter; l++)
                    {
                        var y = candles[l].low + k * (i - l);
                        if (y >= y_low) continue;
                        isFound = false;
                        break;
                    }
                    if (!isFound) continue;
                    return new Cortege3<PointD, PointD, PointD>(new PointD(j, candles[j].high), new PointD(i, candles[i].high), new PointD(low, candles[low].low));
                }
            }
            return new Cortege3<PointD, PointD, PointD>(new PointD(0, 0), new PointD(0, 0), new PointD(0, 0));
        }

        private Cortege3<PointD, PointD, PointD> GetLowLineChannel(List<CandleData> candles, int from)
        {
            // цикл поиска точки b
            for (var i = from - 1 - countAfter; i >= from - 1 - countAfter - maxBody - 1; i--)
            {
                var b = new PointD(i, candles[i].low);
                // по всем свечкам делаем цикл построения линий от candles[i] до первой границы, ищем линию, в которую укладываются все свечи
                // то есть сдвигаем точку a на шаг назад и снова строим линии - ищем выполнение условий
                // цикл по точкам a
                for (var j = i - minBody - 1; j >= i - maxBody - 1; j--)
                {
                    // есть точки a и b вычисляем k - угловой коэффициент
                    var a = new PointD(j, candles[j].low);
                    var k = (b.Y - a.Y) / (b.X - a.X);

                    // прогоняем проверочные циклы
                    var isFound = true;

                    for (var index = j - countBefore; index <= i + countAfter; index++)
                    {
                        if (index == j || index == i) continue;
                        // получаем значение y на индексе i прямой, проведенной через максимум свечи по индексу b_i
                        var yb = candles[index].low + k * (i - index);
                        if (yb >= b.Y) continue;
                        isFound = false;
                        break;
                    }
                    if (!isFound) continue;

                    // условия выполнились, теперь надо проверить верхнюю границу, чтобы все свечки в нее вписались
                    var high = j;
                    var y_high = candles[high].high + k * (i - j);
                    for (var index = j; index <= i; index++)
                    {
                        var y_index = candles[index].high + k * (i - index);
                        if (y_high >= y_index) continue;
                        high = index;
                        y_high = y_index;
                    }
                    // теперь проверим свечи за экстремумами

                    for (var l = j - countBefore; l < j; l++)
                    {
                        var y = candles[l].high + k * (i - l);
                        if (y <= y_high) continue;
                        isFound = false;
                        break;
                    }
                    if (!isFound) continue;
                    for (var l = i + 1; l <= i + countAfter; l++)
                    {
                        var y = candles[l].high + k * (i - l);
                        if (y <= y_high) continue;
                        isFound = false;
                        break;
                    }
                    if (!isFound) continue;
                    return new Cortege3<PointD, PointD, PointD>(new PointD(j, candles[j].low), new PointD(i, candles[i].low), new PointD(high, candles[high].high));
                }
            }
            return new Cortege3<PointD, PointD, PointD>(new PointD(0, 0), new PointD(0, 0), new PointD(0, 0));
        }
    }
}

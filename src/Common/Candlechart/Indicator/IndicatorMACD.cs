using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Candlechart.Chart;
using Candlechart.ChartMath;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
// ReSharper disable LocalizableElement
    [DisplayName("MACD")]
    [LocalizedCategory("TitleOscillator")]
    [TypeConverter(typeof(PropertySorter))]
// ReSharper disable InconsistentNaming
    public class IndicatorMACD : BaseChartIndicator, IChartIndicator
    {
        #region Основные настройки

        public override BaseChartIndicator Copy()
        {
            var macd = new IndicatorMACD();
            Copy(macd);
            return macd;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var macd = (IndicatorMACD) indi;
            CopyBaseSettings(macd);
            macd.fastEMA = fastEMA;
            macd.slowEMA = slowEMA;
            macd.signalSMA = signalSMA;
            macd.LineColor1 = LineColor1;
            macd.LineColor2 = LineColor2;
            macd.SignalLine = SignalLine;
            macd.MainLine = MainLine;
            macd.FastEMALine= FastEMALine;
            macd.SlowEMALine = SlowEMALine;
        }

        [Browsable(false)]
        public override string Name { get { return "MACD"; } }

        private int fastEMA = 12;
        [DisplayName("FastEMA")]
        [LocalizedCategory("TitleMain")]
        [Description("Период быстрой средней, свечей")]
        public int FastEMA
        {
            get { return fastEMA; }
            set { fastEMA = value; }
        }

        private int slowEMA = 26;
        [DisplayName("SlowEMA")]
        [LocalizedCategory("TitleMain")]
        [Description("Период медленной средней, свечей")]
        public int SlowEMA
        {
            get { return slowEMA; }
            set { slowEMA = value; }
        }

        private int signalSMA = 9;
        [DisplayName("SignalSMA")]
        [LocalizedCategory("TitleMain")]
        [Description("Период сглаживания, свечей")]
        public int SignalSMA
        {
            get { return signalSMA; }
            set { signalSMA = value; }
        }
        #endregion

        #region Визуальные настройки

        private readonly static Color[] lineColors = new [] { Color.Red, Color.Green, Color.Blue, Color.DarkViolet, Color.DarkOrange };
        private static int curColorIndex;
        
        [LocalizedDisplayName("TitleLineColor1")]
        [LocalizedCategory("TitleVisuals")]
        [Description("Цвет линии 1")]
        public Color LineColor1 { get; set; }

        [LocalizedDisplayName("TitleLineColor2")]
        [LocalizedCategory("TitleVisuals")]
        [Description("Цвет линии 2")]
        public Color LineColor2 { get; set; }

        #endregion

        private LineSeries SignalLine;
        private LineSeries MainLine;
        private SeriesData FastEMALine;
        private SeriesData SlowEMALine;
        

        public IndicatorMACD()
        {
            LineColor1 = lineColors[curColorIndex++];
            if (curColorIndex >= lineColors.Length) curColorIndex = 0;
            LineColor2 = lineColors[curColorIndex++];
            if (curColorIndex >= lineColors.Length) curColorIndex = 0;
        }

        public void BuildSeries(ChartControl chart)
        {
            SignalLine.Data.Clear();
            MainLine.Data.Clear();
            FastEMALine.Clear();
            SlowEMALine.Clear();
            var start = Math.Max(FastEMA, SlowEMA);
            for (var j = 0; j < start; j++)
            {
                FastEMALine.Add(GetSourcePrice(j));
                SlowEMALine.Add(GetSourcePrice(j));
                SignalLine.Data.Add(0);
                MainLine.Data.Add(0);
            }

            var dataCount = GetSourceDataCount();
            if (dataCount < start) return;
            var i = start;
            var sum = 0.0d;
            while ( i < dataCount)
            {
                // вычисляем главную линию
                // FastEma
                FastEMALine.Add(FastEMALine[i - 1] + (2.0d / (FastEMA + 1) * (GetSourcePrice(i) - FastEMALine[i - 1])));
                // SlowEma
                SlowEMALine.Add(SlowEMALine[i - 1] + (2.0d / (SlowEMA + 1) * (GetSourcePrice(i) - SlowEMALine[i - 1])));
                // сама главная линия
                MainLine.Data.Add(FastEMALine[i] - SlowEMALine[i]);
                // Рассчитываем сигнальную линию
                sum = 0.0d;
                for (var j = 0; j < SignalSMA; j++)
                    sum += (MainLine.Data[i - j]);
                SignalLine.Data.Add(sum / SignalSMA);
                i++;
            }
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            // инициализируем индикатор
            MainLine = new LineSeries(Name + " Main")
            {
                LineColor = LineColor1,
                Transparent = true,
                ShiftX = 1,
                DrawShadow = DrawShadow,
                ShadowWidth = ShadowWidth,
                ShadowColor = Color.Gray,
                ShadowAlpha = ShadowAlpha,                
            };
            SignalLine = new LineSeries(Name + " Signal")
            {
                LineColor = LineColor2,
                Transparent = true,
                ShiftX = 1,
                DrawShadow = DrawShadow,
                ShadowWidth = ShadowWidth,
                ShadowColor = Color.Gray,
                ShadowAlpha = ShadowAlpha,
            };          
            FastEMALine = new SeriesData();
            SlowEMALine = new SeriesData();

            SeriesResult = new List<Series.Series> { MainLine, SignalLine};
            EntitleIndicator();
        }

        public void Remove()
        {            
        }

        public void AcceptSettings()
        {
            MainLine.LineColor = LineColor1;
            SignalLine.LineColor = LineColor2;
        }

        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            BuildSeries(owner);
        }

        public string GetHint(int x, int y, double index, double price, int tolerance)
        {
            return string.Format("{0} = {1:f5}", UniqueName, SignalLine.Data[(int)index]);
        }

        public List<IChartInteractiveObject> GetObjectsUnderCursor(int screenX, int screenY, int tolerance)
        {
            return new List<IChartInteractiveObject>();
        }

        private double GetSourcePrice(int index)
        {
            return GetSourcePrice(index, 0);
        }

        public override string GenerateNameBySettings()
        {
            string colorStr;
            GetKnownColor(LineColor1.ToArgb(), out colorStr);
            return string.Format("MACD[{0},{1},{2}]",
                                 FastEMA, SlowEMA, SignalSMA);
        }
    }
    // ReSharper restore InconsistentNaming
    // ReSharper restore LocalizableElement
}

using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Candlechart.ChartMath;
using TradeSharp.Util;

namespace Candlechart.Series
{
    public abstract class StockSeries : Series, IPriceQuerySeries
    {
        public enum CandleDrawMode
        {
            Candle = 0,
            Bar = 1,
            Line = 2
        }

        private CandleDrawMode barDrawMode = CandleDrawMode.Candle;
        public CandleDrawMode BarDrawMode
        {
            get
            {
                return barDrawMode;
            }

            set
            {
                barDrawMode = value;
            }
        }

        [Browsable(true)]
        [Category("Настройки видимости")]
        [DisplayName("Последняя котировка")]
        public bool ShowLastQuote
        {
            get;
            set;
        }

        [Browsable(true), Category("Настройки видимости"), DisplayName("Автопрокрутка графика")]
        public bool AutoScroll { get; set; }

        [Browsable(true)]
        [Category("Настройки видимости")]
        [DisplayName("Отступ от края")]
        [Description("Отступ от правого края - количество баров")]
        public int BarOffset { get; set; }

        // Fields
        private StockSeriesData data;

        public override int DataCount { get { return data.Count; } }

        // Methods
        protected StockSeries(string name)
            : base(name)
        {
            data = new StockSeriesData();
        }

        internal override string CurrentPriceString
        {
            get
            {
                return string.Empty;
            }
        }

        public StockSeriesData Data
        {
            get { return data; }
        }

        private Color? upFillColor;
        public Color UpFillColor
        {
            get { return upFillColor ?? Chart.visualSettings.StockSeriesUpFillColor; }
            set { upFillColor = value; }
        }

        private Color? upLineColor;
        public Color UpLineColor
        {
            get { return upLineColor ?? Chart.visualSettings.StockSeriesUpLineColor; }
            set { upLineColor = value; }
        }

        private Color? downFillColor;
        public Color DownFillColor
        {
            get { return downFillColor ?? Chart.visualSettings.StockSeriesDownFillColor; }
            set { downFillColor = value; }
        }

        private Color? downLineColor;
        public Color DownLineColor
        {
            get { return downLineColor ?? Chart.visualSettings.StockSeriesDownLineColor; }
            set { downLineColor = value; }
        }

        public const double MinimumXDelta = 1.0;

        private Color? barNeutralColor;
        public Color BarNeutralColor
        {
            get { return barNeutralColor ?? Chart.visualSettings.BarNeutralColor; }
            set { barNeutralColor = value; }
        }

        private float barLineWidth = 1;
        public float BarLineWidth
        {
            get { return barLineWidth; }
            set { barLineWidth = value; }
        }
        
        /// <summary>
        /// цветовая схема, если null - берется из VisualSettings
        /// </summary>
        /*public Color? CustomColorFillUp { get; set; }
        public Color? CustomColorFillDn { get; set; }
        public Color? CustomColorFillEmpty { get; set; }
        public Color? CustomColorOutlineUp { get; set; }
        public Color? CustomColorOutlineDn { get; set; }*/

        internal void CopyFrom(StockSeries stockSeries)
        {
            if (stockSeries == null) return;
            Owner = stockSeries.Owner;
            Name = stockSeries.Name;
            BackColor = stockSeries.BackColor;
            ForeColor = stockSeries.ForeColor;
            LineWidth = stockSeries.LineWidth;
            NumberDecimalDigits = stockSeries.NumberDecimalDigits;
            upFillColor = stockSeries.upFillColor;
            upLineColor = stockSeries.upLineColor;
            downFillColor = stockSeries.downFillColor;
            downLineColor = stockSeries.downLineColor;
            BarNeutralColor = stockSeries.BarNeutralColor;
            data = stockSeries.Data;
        }

        internal virtual int FindNearestLeftBar(double xValue)
        {
            return (int)Math.Floor(xValue);
        }

        internal virtual int FindNearestRightBar(double xValue)
        {
            return (int)Math.Ceiling(xValue);
        }

        internal virtual double GetBarLeftEdge(int index)
        {
            return index - 0.5;
        }

        internal virtual double GetBarRightEdge(int index)
        {
            return index + 0.5;
        }

        public override bool GetXExtent(ref double left, ref double right)
        {
            if (Data.Count <= 0)
                return false;
            if (Chart.StockSeries.GetBarLeftEdge(Data.StartIndex) < left)
            {
                left = Chart.StockSeries.GetBarLeftEdge(Data.StartIndex);
            }
            if (Chart.StockSeries.GetBarRightEdge(Data.LastIndex) > right)
            {
                right = Chart.StockSeries.GetBarRightEdge(Data.LastIndex);
            }
            return true;
        }

        public bool GetIndexRange(ref double start, ref double end)
        {
            if (Data.Count <= 0)
                return false;
            start = Data.StartIndex;
            end = Data.LastIndex;
            return true;
        }

        public bool GetTimeExtent(ref DateTime start, ref DateTime end)
        {
            if (Data == null)
            {
                Logger.Error("GetTimeExtent: data is null");
                return false;
            }
            if (Data.Count <= 0) return false;
            var startRecord = Data[Data.StartIndex];
            var endRecord = Data[Data.LastIndex];

            if (startRecord == null)
            {
                Logger.Error("GetTimeExtent: startRecord is null");
                return false;
            }
            if (endRecord == null)
            {
                Logger.Error("GetTimeExtent: endRecord is null");
                return false;
            }

            start = Data[Data.StartIndex].timeOpen;
            end = Data[Data.LastIndex].timeOpen;
            return true;
        }

        //internal virtual double GetXValue(double index)
        //{
        //    return (index * 1.0);
        //}

        public override bool GetYExtent(double left, double right, ref double top, ref double bottom)
        {
            if (Data.Count <= 0)
            {
                return false;
            }
            var flag = false;
            for (var i = 0; i < Data.Count; i++)
            {
                double xValue = i;
                if ((xValue >= left) && (xValue <= right) && (Data[i].open > 0 || Data[i].close > 0))
                {
                    flag = true;
                    if ((double)Data[i].high > top)
                    {
                        top = (double)Data[i].high;
                    }
                    if ((double)Data[i].low < bottom)
                    {
                        bottom = (double)Data[i].low;
                    }
                }
            }
            return flag;
        }

        /// <summary>
        /// Получить индекс свечи по ее времени открытия
        /// </summary>        
        public int GetIndexByCandleOpen(DateTime open)
        {
            if (Data.Candles.Count == 0) return 0;
            
            // индекс левее интервала
            if (open < Data.Candles[0].timeOpen) return 0;
            
            // индекс на интервале
            if (open <= Data.Candles[Data.Candles.Count - 1].timeClose)
            {
                return GetIndexStricktlyOnInterval(open);
            }
            if (Chart.Owner == null) return 0;

            // если за пределами интервала (правее)
            var index = Data.Candles.Count - 1;
            var realMargin = Data.Candles[Data.Candles.Count - 1].timeOpen;

            var minutes = (int) (open - realMargin).TotalMinutes;
            var wholeBarLen = Chart.Timeframe.Intervals.Sum();
            var wholeBars = minutes / wholeBarLen;

            index += wholeBars * Chart.Timeframe.Intervals.Count;
            var time = realMargin.AddMinutes(wholeBarLen*wholeBars);
                
            var intervalIndex = 0;
            while (time < open)
            {
                time = time.AddMinutes(Chart.Timeframe.Intervals[intervalIndex++]);
                index++;
                if (intervalIndex == Chart.Timeframe.Intervals.Count)
                    intervalIndex = 0;
            }
            return index;
        }

        /// <summary>
        /// Получить индекс свечи по ее времени открытия строго на интервале
        /// </summary>
        private int GetIndexStricktlyOnInterval(DateTime time)
        {            
            var candles = Data.Candles;
            // простой перебор
            if (Data.Count < 30)
            {
                for (var i = 0; i < candles.Count; i++)
                {
                    if (candles[i].timeOpen <= time && candles[i].timeClose > time)
                        return i;
                    if (candles[i].timeOpen > time)
                        return i - 1;
                }
                return candles.Count - 1;
            }
            
            // перебор методом золотого сечения
            const int minDelta = 3;
            int startIndex = 0, endIndex = candles.Count;

            while ((endIndex - startIndex) > minDelta)
            {
                var midIndex = (int)((startIndex + endIndex) * 0.5);
                var midDate = candles[midIndex].timeOpen;
                if (time < midDate)                
                    endIndex = midIndex;
                else                
                    startIndex = midIndex;                
            }
            // перебрать на оставшемся коротеньком интервале
            for (var i = startIndex; i < endIndex; i++)
            {
                if (candles[i].timeOpen <= time && candles[i].timeClose > time)
                    return i;
                if (candles[i].timeOpen > time)
                    return i - 1;
            }
            //throw new Exception("GetIndexStricktly failed to find index");
            return startIndex + 1;
        }

        public double GetDoubleIndexByTime(DateTime time, bool allowedNegativeIndex = false)
        {
            var candles = Data.Candles;
            if (candles.Count == 0) return 0;
            var index = -2;
            
            if (time >= candles[0].timeOpen && time < candles[candles.Count - 1].timeClose)
                index = GetIndexStricktlyOnInterval(time);
            else
            {
                if (time == candles[candles.Count - 1].timeClose)
                    index = candles.Count - 1;
            }
            if (index >= 0)
            {
                var candle = Data.Candles[index];
                var nextCandle = index < Data.Candles.Count - 1 ? Data.Candles[index + 1] : candle;
                
                var deltaTotal = (nextCandle.timeOpen - candle.timeOpen).TotalMinutes;
                var deltaTime = (time - candle.timeOpen).TotalMinutes;
                var dIndex = deltaTotal > 0 ? index + deltaTime/deltaTotal : index;
                return dIndex;
            }
            
            var firstTime = Data.Candles[0].timeOpen;
            //var lastTime = Data.Candles[Data.Candles.Count - 1].timeClose;
            if (time <= firstTime) 
                return allowedNegativeIndex ? -1 : 0;
            return Data.Candles.Count;
        }

        /// <summary>
        /// Расчитать время открытия свечи по ее индексу
        /// </summary>        
        public DateTime GetCandleOpenTimeByIndex(int index)
        {
            if (Data.Candles.Count == 0 || index < 0) return new DateTime();
            if (index < Data.Candles.Count)
                return Data.Candles[index].timeOpen;
            index = index - Data.Candles.Count + 1;
            int wholeBarLen = 0;
            foreach (var inter in Chart.Timeframe.Intervals)
                wholeBarLen += inter;
            int wholeInters = index / Chart.Timeframe.Intervals.Count;
            int partInters = index % Chart.Timeframe.Intervals.Count;
            int totalMinutes = wholeInters * wholeBarLen + Chart.Timeframe.StartMinute;
            for (var i = 0; i < partInters; i++)
                totalMinutes += Chart.Timeframe.Intervals[i];
            return Data.Candles[Data.Candles.Count - 1].timeOpen.AddMinutes(totalMinutes);
        }

        // Properties
        public float? GetPrice(int index)
        {
            if (index < 0) return null;
            if (Data.Candles.Count <= index) return null;
            return Data.Candles[index].close;
        }
    }
}
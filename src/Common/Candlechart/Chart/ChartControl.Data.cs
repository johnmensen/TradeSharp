using System;
using System.Collections.Generic;
using System.ComponentModel;
using Entity;

namespace Candlechart.Chart
{
    public partial class ChartControl
    {
        private bool _needDealData = true;              
        private ChartCacheMode _cacheMode = ChartCacheMode.NoCache;        

        [Browsable(true)]
        public bool NeedDealData
        {
            get
            {
                return _needDealData;
            }
            set
            {
                _needDealData = value;
            }
        }        

        /// <summary>
        /// Начало интервала отображения
        /// </summary>
        [Browsable(true)]
        public DateTime StartTime { get; set; }
        /// <summary>
        /// Конец интервала отображения
        /// </summary>        
        [Browsable(true)]
        public DateTime EndTime { get; set; }

        private string symbol;
        /// <summary>
        /// ID валютной пары (international name)
        /// </summary>                
        [Browsable(true)]
        public string Symbol
        {
            get { return symbol; }
            set
            {
                symbol = value;
                MakeStockPaneTitle();
                Precision = DalSpot.Instance.GetPrecision(value);
                priceFormat = "f" + Precision;
            }
        }

        /// <summary>
        /// кол-во знаков после запятой
        /// </summary>
        public int Precision { get; private set; }

        private string priceFormat = "f4";
        public string PriceFormat
        {
            get { return priceFormat; }            
        }

        private BarSettings timeframe;
        /// <summary>
        /// Матрица баров (начальное смещение и интервалы)
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public BarSettings Timeframe
        {
            get { return timeframe; }
            set
            {
                timeframe = value; 
                MakeStockPaneTitle();
            }
        }

        /// <summary>
        /// Опции кеширования
        /// </summary>
        [Browsable(false)]
        public ChartCacheMode CacheMode
        {
            get
            {
                return _cacheMode;
            }
            set
            {
                _cacheMode = value;
            }
        }

        private void MakeStockPaneTitle()
        {
            if (StockPane == null) return;
            StockPane.Title = string.IsNullOrEmpty(Symbol) && Timeframe == null ? ""
                : string.IsNullOrEmpty(Symbol) ? Timeframe.Title : Timeframe == null 
                ? Symbol.ToUpper() : string.Format("{0} {1}", Symbol.ToUpper(), 
                    BarSettingsStorage.Instance.GetBarSettingsFriendlyName(Timeframe));
        }
    }

    public enum ChartCacheMode
    {
        NoCache,        // кеширование отключено
        ExactCandle,    // кеш по свече-валюте (время не учитывается)
        ExactTime       // кеш по свече-валюте-времени (для сайта)
    }

    class FillChartAsyncArgument
    {
        /// <summary>
        /// Если false - перезаписать массив свечей
        /// true - дописать свечи
        /// </summary>
        public bool ShouldAppendCandles { get; set; }
        /// <summary>
        /// Начало интервала обновления (актуально для ShouldAppendCandles=true)
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// Также используется методом обновления
        /// </summary>
        public DateTime EndTime { get; set; }
        public FillChartAsyncArgument(bool shouldAppendCandles)
        {
            ShouldAppendCandles = shouldAppendCandles;
        }
    }
    class FillChartAsyncResult
    {
        public bool ShouldAppendCandles { get; set; }
        public List<CandleData> candles;
    }
}
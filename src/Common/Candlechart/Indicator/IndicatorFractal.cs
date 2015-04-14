using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Candlechart.Chart;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using System.Linq;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    [LocalizedDisplayName("TitleFractal")]
    [LocalizedCategory("TitleOscillator")]
    [TypeConverter(typeof(PropertySorter))]
    public class IndicatorFractal : BaseChartIndicator, IChartIndicator
    {
        #region [ Поля ]

        //? На что влияет название которое здесь задается?
        /* А
           Когда пользователь добавляет на график либо редактирует индикатор, он может выбрать
           серии-источник данных. Эта SeriesAsterisk, в теории, может стать источником данных (единственным 
           либо одним из нескольких) для какого-нибудь другого индикатора. При выборе он видит список из названий
         */

        private SeriesAsteriks series = new SeriesAsteriks(Localizer.GetString("TitleFractalLabels"));
        // Смещение чтобы стрелка не липла к столбику на графике
        /* А 
          Если это смещение в единицах цены, то на графике в отдельных случаях оно будет слишком малым, в других - слишком большим
         */
        private const float offset = 0.000f;

        [Browsable(false)]
        public override string Name { get { return Localizer.GetString("TitleFractal"); } }

        #endregion

        //? Правильное ли описание я задал индикаторам?
        /* А    Да. Кроме категории (но это неважно) */

        #region [ Параметры индикатора ]

        private int barsLeft = 2;
        [LocalizedDisplayName("TitleBarCountFromLeft")]
        [LocalizedDescription("MessageBarCountFromLeftDescription")]
        [LocalizedCategory("TitleMain")]
        public int BarsLeft
        {
            get { return barsLeft; }
            set { barsLeft = value; }
        }

        private int barsRight = 2;
        [LocalizedDisplayName("TitleBarCountFromRight")]
        [LocalizedDescription("MessageBarCountFromRightDescription")]
        [LocalizedCategory("TitleMain")]
        public int BarsRight
        {
            get { return barsRight; }
            set { barsRight = value; }
        }

        public enum PriceType { HighLow = 0, OpenClose = 1, Close = 2 }

        [LocalizedDisplayName("TitlePriceType")]
        [LocalizedDescription("MessagePriceTypeForExtremumDescription")]
        [LocalizedCategory("TitleMain")]
        public PriceType Price { get; set; }

        #endregion

        #region Визуальные настройки

        private Color clArrowHigh = Color.Blue;
        [LocalizedDisplayName("TitleTopColor")]
        [LocalizedDescription("MessageTopFractalColorDescription")]
        [LocalizedCategory("TitleVisuals")]
        public Color ClArrowHigh
        {
            get { return clArrowHigh; }
            set { clArrowHigh = value; }
        }

        private Color clArrowLow = Color.Red;
        [LocalizedDisplayName("TitleBottomColor")]
        [LocalizedDescription("MessageBottomFractalColorDescription")]
        [LocalizedCategory("TitleVisuals")]
        public Color ClArrowLow
        {
            get { return clArrowLow; }
            set { clArrowLow = value; }
        }

        #endregion

        public IndicatorFractal()
        {
            // Этим кодом заставляем индикатор рисоваться в той же панели что и основной график
            /* А
               В принципе, можно не привязывать строго фрактал к основной панели. Тогда в 
             * BuildSeries построения пойдут не от chart.StockSeries.Data.Candles, а от
             * SeriesSource[0], но там необязательно будут свечи (CandlestickSeries)
             */
            CreateOwnPanel = false;
        }

        public void BuildSeries(ChartControl chart)
        {
            series.data.Clear();
            var candles = chart.StockSeries.Data.Candles;
            if (candles.Count == 0) return;
            
            for (int i = barsLeft; i < candles.Count - barsRight - 1; i++)
            {
                // Коллекции баров слева и справа от тегущего
                var barsToTheLeft = candles.Skip(i - barsLeft).Take(barsLeft);
                var barsToTheRight = candles.Skip(i + 1).Take(barsRight);

                // Является ли фракталом верха. Справа и слева должны быть бары с более низкими максимумами
                if (barsToTheLeft.All(c => GetCandlePrice(c, true) < GetCandlePrice(candles[i], true)) &&
                    barsToTheRight.All(c => GetCandlePrice(c, true) < GetCandlePrice(candles[i], true)))
                {
                    // Отрисовать верх
                    DrawIndi(true, candles[i], i);
                }

                // Является ли фракталом вниз. Справа и слева должны быть бары с более высокими минимумами
                if (barsToTheLeft.All(c => GetCandlePrice(c, false) > GetCandlePrice(candles[i], false)) &&
                    barsToTheRight.All(c => GetCandlePrice(c, false) > GetCandlePrice(candles[i], false)))
                {
                    // Отрисовать верх
                    DrawIndi(false, candles[i], i);
                }
            }
        }

        private float GetCandlePrice(CandleData candle, bool needMax)
        {
            return Price == PriceType.Close
                       ? candle.close
                       : Price == PriceType.HighLow
                             ? (needMax ? candle.high : candle.low)
                             : /*Price == PriceType.OpenClose ?*/
                             (needMax ? Math.Max(candle.open, candle.close) : Math.Min(candle.open, candle.close));
        }

        /// <summary>
        /// Отрисовать индекс на графике
        /// </summary>
        /// <param name="isHigh">тру - если фрактал верха</param>
        /// <param name="candle">текущая свеча</param>
        /// <param name="index">индекс текущей свечи</param>
        private void DrawIndi(bool isHigh, CandleData candle, int index)
        {
            //? Из за недостатка комментариев я не очень понял какие поля зачему нужны у этого класса
            // Думаю, что ты и так знаешь, но классы которыми нужно оперировать при отрисовке, надо бы описать в документации

            /* А 
               Конечно, надо :)
              */
            var tip = new AsteriskTooltip("fractal", string.Empty)
            {
                Price = isHigh ? (candle.high + offset) : (candle.low - offset),
                CandleIndex = index,
                //DateStart = candle.timeOpen,//? Что это и нужно ли здесь
                Sign = "", //? Что это и нужно ли здесь
                Shape = isHigh
                            ? AsteriskTooltip.ShapeType.ГалкаВверх
                            : AsteriskTooltip.ShapeType.ГалкаВниз,
                ColorFill = isHigh ? clArrowHigh : clArrowLow,
                ColorLine = Color.Black,
                ColorText = Color.Black,
                Radius = 6 //? Что это и нужно ли здесь
            };
            /* А
               Sign - текст, выводимый посередине
               Radius - каждая фигурка задана в "векторной" форме, Radius задает ее измерение (ширину, вроде) в пикс., пропорционально меняется другое измерение
               DateStart можно не указывать, если задан CandleIndex
             */
            series.data.Add(tip);
        }

        #region [ Отнаследованные и перегруженные методы ]

        public override BaseChartIndicator Copy()
        {
            var indi = new IndicatorFractal();
            Copy(indi);
            return indi;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var indiTf = (IndicatorFractal)indi;
            CopyBaseSettings(indiTf);
            indiTf.barsRight = barsRight;
            indiTf.barsLeft = barsLeft;
            indiTf.clArrowHigh = clArrowHigh;
            indiTf.clArrowLow = clArrowLow;
            indiTf.Price = Price;
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            //? Обязательно ли эти строки писать тут, а не скажем в конструкторе?
            /* А 
               Индикатор создается через reflection, было удобней отдельно сделать метод добавления на график
               Хотя и там можно было бы подсунуть эти параметры... надо посмотреть
             */
            owner = chart;
            SeriesResult = new List<Series.Series> { series };
            EntitleIndicator();
        }

        public void Remove()
        {
            if (series != null) series.data.Clear();
        }

        public void AcceptSettings()
        {
            //? Зачем создавать новый лист, если этот параметр true?
            /* А
               Это, скорее, копипаст. В данном примере - не нужно 
             */
            if (CreateOwnPanel)
            {
                SeriesResult = new List<Series.Series> { series };
            }
            if (DrawPane != null && DrawPane != owner.StockPane)
                DrawPane.Title = string.Format("{0} [{1}/{2}]", UniqueName, barsLeft, barsRight);   
        }

        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (updatedCandle == null && newCandles.Count == 0) return;
            // построить индюк
            BuildSeries(owner);
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

        #endregion
    }
}

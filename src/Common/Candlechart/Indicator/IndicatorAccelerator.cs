using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Candlechart.Chart;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    [LocalizedDisplayName("TitleAccelerationAndSlowdown")]
    [LocalizedCategory("TitleOscillator")]
    [TypeConverter(typeof(PropertySorter))]
    public class IndicatorAccelerator : BaseChartIndicator, IChartIndicator
    {
        #region Основные / наследуемые настройки

        [Browsable(false)]
        public override string Name { get { return Localizer.GetString("TitleAccelerationAndSlowdown"); } }

        /* А 
           Это свойство задает, будет ли индикатор (по-умолчанию) рисоваться в
           окне графика либо в отдельном окошке
        */

        [LocalizedDisplayName("TitleCreateDrawingPanel")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageCreateDrawingPanelDescription")]
        public override bool CreateOwnPanel { get { return true; } set { } }

        #endregion

        #region Параметры индикатора

        private int periodSlow = 34;
        [LocalizedDisplayName("TitleSlowMovingAveragePeriodShort")]
        [LocalizedDescription("MessageSlowMovingAveragePeriodDescription")]
        [LocalizedCategory("TitleMain")]
        public int PeriodSlow
        {
            get { return periodSlow; }
            set { periodSlow = value; }
        }

        private int periodFast = 5;
        [LocalizedDisplayName("TitleFastMovingAveragePeriodShort")]
        [LocalizedDescription("MessageFastMovingAveragePeriodDescription")]
        [LocalizedCategory("TitleMain")]
        public int PeriodFast
        {
            get { return periodFast; }
            set { periodFast = value; }
        }

        private int periodAwesome = 5;
        [LocalizedDisplayName("TitleAwesomePeriod")]
        [LocalizedDescription("MessageAwesomePeriodDescription")]
        [LocalizedCategory("TitleMain")]
        public int PeriodAwesome
        {
            get { return periodAwesome; }
            set { periodAwesome = value; }
        }

        #endregion

        #region Визуальные настройки

        private Color clBarsPositive = Color.Green;
        [LocalizedDisplayName("TitleGrowthColor")]
        [LocalizedDescription("MessageGrowthColorDescription")]
        [LocalizedCategory("TitleVisuals")]
        public Color ClBarsPositive
        {
            get { return clBarsPositive; }
            set { clBarsPositive = value; }
        }

        private Color clBarsNegative = Color.Red;
        [LocalizedDisplayName("TitleFallColor")]
        [LocalizedDescription("MessageFallColorDescription")]
        [LocalizedCategory("TitleVisuals")]
        public Color ClBarsNegative
        {
            get { return clBarsNegative; }
            set { clBarsNegative = value; }
        }

        #endregion

        #region Переменные - члены

        //? Лазил в этот класс. Немного просмотрел, понял зачем он. Но описание в двух словах нужно имхо.
        // и я того же мнения (MSI)
        private HistogramSeries seriesDeltas;

        private RestrictedQueue<float> queueFast;
        private RestrictedQueue<float> queueSlow;
        private RestrictedQueue<float> queueAwesomes;

        #endregion

        public IndicatorAccelerator()
        {        
            //? как можно сделать защиту от дурака? например если periodSlow меньше periodFast или меньше нуля или больше какого то разумного значения?
            // Где лучше делать проверку, нужно ли это и как сообщить пользователю что он лошара? Через MessageBox.show?
            
            /* A: можно в AcceptSettings, плюс, попозже, я добавлю для индикаторов возможность писать сообщения в окно сообщений терминала
             * появится делегат */
        }

        public override BaseChartIndicator Copy()
        {
            var indi = new IndicatorAccelerator();
            Copy(indi);
            return indi;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var indiTf = (IndicatorAccelerator)indi;
            CopyBaseSettings(indiTf);
            indiTf.seriesDeltas = seriesDeltas;
            indiTf.periodSlow = periodSlow;
            indiTf.periodFast = periodFast;
            indiTf.periodAwesome = periodAwesome;
            indiTf.ClBarsPositive = ClBarsPositive;
            indiTf.ClBarsNegative = ClBarsNegative;
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            seriesDeltas = new HistogramSeries("Accelerator");
            SeriesResult = new List<Series.Series> { seriesDeltas };
            
            EntitleIndicator();
        }

        public void Remove()
        {
            seriesDeltas.data.Clear();            
        }

        public void AcceptSettings()
        {
            if (DrawPane != null && DrawPane != owner.StockPane)
                DrawPane.Title = string.Format("{0} [{1}/{2}]", UniqueName, PeriodSlow, PeriodFast);        
        }

        public void BuildSeries(ChartControl chart)
        {
            seriesDeltas.data.Clear();

            if (SeriesSources[0].DataCount <= periodSlow) return;

            queueFast = new RestrictedQueue<float>(periodFast);
            queueSlow = new RestrictedQueue<float>(periodSlow);
            queueAwesomes = new RestrictedQueue<float>(periodAwesome);

            // построить индюк
            BuildIndi(SeriesSources[0]);            
        }

        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (updatedCandle == null && newCandles.Count == 0) return;
            // построить индюк
            BuildSeries(owner);
        }

        private void BuildIndi(Series.Series source)        
        {
            if (source is IPriceQuerySeries == false) return;

            var curIndex = 0;
            var prevAccel = 0f;
            for (var j = 0; j < source.DataCount; j++)
            {
                var medianPrice = source is CandlestickSeries
                        ? (((CandlestickSeries)source).Data.Candles[j].high +
                        ((CandlestickSeries)source).Data.Candles[j].low) * 0.5f 
                        : (((IPriceQuerySeries)source).GetPrice(j) ?? 0);
                
                queueFast.Add(medianPrice);
                queueSlow.Add(medianPrice);

                // Если нечего рисовать то пропускаем
                var accelerator = 0f;
                /* А
                 * добил график пустыми столбиками в начале, чтобы крайний столбик был там же, где крайняя свечка
                 */
                if (queueSlow.Length >= periodSlow)
                {
                    var maSlow = queueSlow.Average();
                    var maFast = queueFast.Average();

                    //AO = SMA (MEDIAN PRICE, 5) — SMA (MEDIAN PRICE, 34)
                    var awesome = maFast - maSlow;
                    queueAwesomes.Add(awesome);

                    // Собственно значение ускорения/замедления
                    // AC = AO — SMA (AO, 5)
                    if (queueAwesomes.Length == periodAwesome)
                        accelerator = awesome - queueAwesomes.Average();
                }
                //? Класс HistogramBar не описал но вроде он не хитрый и понятно по примеру как его юзать. Для документации достаточно описаь поля и предназначение имхо
                /* А
                 * я чуть иначе раскрасил акселератор, канонически
                 */
                seriesDeltas.data.Add(new HistogramBar
                {
                    color = accelerator >= prevAccel ? ClBarsPositive : ClBarsNegative,
                    index = curIndex++,
                    y = accelerator
                });
                prevAccel = accelerator;
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

        public override string GenerateNameBySettings()
        {
            return string.Format("Accelerator[{0}/{1}/{2}]", PeriodSlow, PeriodFast, PeriodAwesome);
        }
    }
}

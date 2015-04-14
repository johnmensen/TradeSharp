using System;
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
    [LocalizedDisplayName("TitleRenko")]
    [LocalizedCategory("TitleTrending")]
    [TypeConverter(typeof(PropertySorter))]
    // ReSharper disable InconsistentNaming
    class IndicatorRenco : BaseChartIndicator, IChartIndicator
    {
        #region Основные настройки

        public override BaseChartIndicator Copy()
        {
            var renco = new IndicatorRenco();
            Copy(renco);
            return renco;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var renco = (IndicatorRenco)indi;
            CopyBaseSettings(renco);
            renco.RencoSize = RencoSize;
            renco.ColorBarDn = ColorBarDn;
            renco.ColorBarUp = ColorBarUp;
            renco.ColorOutlineUp = ColorOutlineUp;
            renco.ColorOutlineDn = ColorOutlineDn;
            renco.TransparentEmptyBricks = TransparentEmptyBricks;
            renco.ColorBarEmpty = ColorBarEmpty;
            renco.VolatilityScale = VolatilityScale;
            renco.VolatilityType = VolatilityType;
            renco.AutosizePeriod = AutosizePeriod;
            renco.BrickSizeAuto = BrickSizeAuto;            
        }

        [Browsable(false)]
        public override string Name { get { return Localizer.GetString("TitleRenko"); } }

        private int rencoSize = 50;
        [LocalizedDisplayName("TitleBrickSize")]
        [Description("Размер кирпича Ренко, пунктов")]
        [LocalizedCategory("TitleMain")]
        public int RencoSize
        {
            get { return rencoSize; }
            set { rencoSize = value; }
        }

        [LocalizedDisplayName("TitleBrickAutosize")]
        [Description("Размер кирпича Ренко считается от волатильности")]
        [LocalizedCategory("TitleMain")]
        public bool BrickSizeAuto { get; set; }

        private int autosizePeriod = 30;
        [LocalizedDisplayName("TitleVolatilityPeriod")]
        [Description("Период измерения волатильности для Ренко")]
        [LocalizedCategory("TitleMain")]
        public int AutosizePeriod
        {
            get { return autosizePeriod; }
            set { autosizePeriod = value; }
        }

        public enum RencoVolatilityType { Размах = 0, ATR = 1 }
        [LocalizedDisplayName("TitleVolitilityType")]
        [Description("Тип измерения волатильности для Ренко")]
        [LocalizedCategory("TitleMain")]
        public RencoVolatilityType VolatilityType { get; set; }

        private double volatilityScale = 1;
        [LocalizedDisplayName("TitleVolatilityScale")]
        [Description("Масштаб размера кирпича от текущей волатильности")]
        [LocalizedCategory("TitleMain")]
        public double VolatilityScale
        {
            get { return volatilityScale; }
            set { volatilityScale = value; }
        }

        #endregion

        #region Визуальные настройки

        private Color 
            colorBarUp = Color.FromArgb(255, 20, 220, 20), 
            colorBarDn = Color.FromArgb(255, 220, 20, 20),
            colorOutlineUp = Color.FromArgb(255, 10, 90, 10),
            colorOutlineDn = Color.FromArgb(255, 90, 10, 10),
            colorBarEmpty = Color.FromArgb(128, 128, 128, 128);

        [LocalizedDisplayName("TitleGrowthBricksColorShort")]
        [Description("Цвет растущих кирпичиков")]
        [LocalizedCategory("TitleVisuals")]
        public Color ColorBarUp { get { return colorBarUp; } set { colorBarUp = value; } }

        [LocalizedDisplayName("TitleFallBricksColorShort")]
        [Description("Цвет падающих кирпичиков")]
        [LocalizedCategory("TitleVisuals")]
        public Color ColorBarDn { get { return colorBarDn; } set { colorBarDn = value; } }

        [LocalizedDisplayName("TitleGrowthBricksStrokeShort")]
        [Description("Обводка растущих кирпичиков")]
        [LocalizedCategory("TitleVisuals")]
        public Color ColorOutlineUp { get { return colorOutlineUp; } set { colorOutlineUp = value; } }

        [LocalizedDisplayName("TitleFallBricksStrokeShort")]
        [Description("Обводка падающих кирпичиков")]
        [LocalizedCategory("TitleVisuals")]
        public Color ColorOutlineDn { get { return colorOutlineDn; } set { colorOutlineDn = value; } }

        private bool transparentEmptyBricks = true;
        [LocalizedDisplayName("TitleTransparentBricks")]
        [Description("Пустотелые кирпичи рисуются прозрачными")]
        [LocalizedCategory("TitleVisuals")]
        public bool TransparentEmptyBricks { get { return transparentEmptyBricks; } set { transparentEmptyBricks = value; } }

        [LocalizedDisplayName("TitleHollowBrickColor")]
        [Description("Цвет пустотелых кирпичей, если они непрозрачные")]
        [LocalizedCategory("TitleVisuals")]
        public Color ColorBarEmpty { get { return colorBarEmpty; } set { colorBarEmpty = value; } }

        #endregion

        private CandlestickSeries indSeriesRenco;

        public void BuildSeries(ChartControl chart)
        {
            if (DrawPane != owner.StockPane)
                DrawPane.Title = UniqueName;
            indSeriesRenco.Data.Clear();
            if (SeriesSources.Count == 0) return;
            int dataCount = GetSourceDataCount();
            if (dataCount == 0) return;

            indSeriesRenco.DownFillColor = ColorBarDn;
            indSeriesRenco.UpFillColor = ColorBarUp;
            indSeriesRenco.BarNeutralColor = !TransparentEmptyBricks ? ColorBarEmpty : Color.Empty;
            indSeriesRenco.UpLineColor = ColorOutlineUp;
            indSeriesRenco.DownLineColor = ColorOutlineDn;

            int rencoBrickSize = RencoSize;
            var rencoSizeAbs = (double)DalSpot.Instance.GetAbsValue(chart.Symbol, (float)rencoBrickSize);
            // начальный кирпич
            var startPrice = 0.0;
            var seriesSrc = SeriesSources[0];
            if (seriesSrc is CandlestickSeries)
                startPrice = ((CandlestickSeries)seriesSrc).Data[0].open;
            else
                if (seriesSrc is LineSeries)
                    startPrice = ((LineSeries) seriesSrc).Data[0];
            var startClose = GetSourcePrice(0, 0);
            
            var rencoBricks = new List<RencoBrick>
                                  {
                                      new RencoBrick {open = startPrice, close = startClose, index = -1}
                                  };

            // классический Ренко - кирпичеги не привязаны ко времени
            // привязка будет ниже
            for (var i = 0; i < dataCount; i++)
            {
                var brickSize = rencoSizeAbs;
                if (BrickSizeAuto)
                    if ((VolatilityType == RencoVolatilityType.ATR && seriesSrc is CandlestickSeries) ||
                        VolatilityType == RencoVolatilityType.Размах)
                        brickSize = GetRencoBrickAutoSizeAbs(rencoSizeAbs, i);

                var lastBrick = rencoBricks[rencoBricks.Count - 1];
                var lastSign = lastBrick.close >= lastBrick.open ? 1 : -1;
                var price = GetSourcePrice(i, 0);

                var deltaClose = (int)((price - lastBrick.close) / brickSize);
                var deltaOpen = (int)((price - lastBrick.open) / brickSize);

                if (deltaClose != 0 && Math.Sign(deltaClose) == lastSign)
                    AddRencoBricks(rencoBricks, i, deltaClose, brickSize, lastBrick.close);
                else
                    if (deltaOpen != 0 && Math.Sign(deltaOpen) != lastSign)
                        AddRencoBricks(rencoBricks, i, deltaOpen, brickSize, lastBrick.open);
            }

            // Ренко в масштабе времени
            // (добивается пустотелыми кирпичами)
            var currentRencoIndex = 0;
            var currentSign = 0;
            float lastClose = 0;
            var candles = owner.StockSeries.Data.Candles;

            for (var i = 0; i < dataCount; i++)
            {
                if (i >= candles.Count) return;
                var candleHasBody = false;
                while (currentRencoIndex < (rencoBricks.Count - 1))
                {
                    if (rencoBricks[currentRencoIndex + 1].index != i) break;
                    candleHasBody = true;
                    currentSign = rencoBricks[currentRencoIndex + 1].close >
                                  rencoBricks[currentRencoIndex + 1].open
                                      ? 1
                                      : -1;
                    currentRencoIndex++;
                }
                var newClose = lastClose + currentSign * RencoSize;
                var candle = new CandleData(lastClose,
                                            Math.Max(newClose, lastClose),
                                            Math.Min(newClose, lastClose),
                                            newClose,
                                            candles[i].timeOpen,
                                            candles[i].timeClose);
                if (!candleHasBody) candle.customColor = Color.Empty;
                indSeriesRenco.Data.Candles.Add(candle);
                lastClose = newClose;
            }
            // поднять по оси У
            var minY = indSeriesRenco.Data.Candles.Min(c => c.close);
            if (minY < 0)
            {
                for (var i = 0; i < indSeriesRenco.Data.Candles.Count; i++)
                {
                    indSeriesRenco.Data.Candles[i].open -= minY;
                    indSeriesRenco.Data.Candles[i].close -= minY;
                    indSeriesRenco.Data.Candles[i].high -= minY;
                    indSeriesRenco.Data.Candles[i].low -= minY;
                }
            }            
        }

        private static void AddRencoBricks(List<RencoBrick> rencoBricks, int i,
           int delta, double rencoSizeAbs, double startLevel)
        {
            var sign = Math.Sign(delta);
            for (var nBrick = 0; nBrick < Math.Abs(delta); nBrick++)
            {
                var brick = new RencoBrick
                {
                    open = startLevel + nBrick * rencoSizeAbs * sign,
                    close = startLevel + (nBrick + 1) * rencoSizeAbs * sign,
                    index = i
                };
                rencoBricks.Add(brick);
            }
        }

        private double GetRencoBrickAutoSizeAbs(double defaultSize, int candleIndex)
        {
            var startIndex = candleIndex - AutosizePeriod;
            if (startIndex < 0) return defaultSize;
            
            if (VolatilityType == RencoVolatilityType.ATR)
            {
                var volatility = 0.0;
                var series = (CandlestickSeries) SeriesSources[0];
                for (var i = startIndex; i < candleIndex; i++)
                {
                    volatility += series.Data.Candles[i].high - series.Data.Candles[i].low;
                }
                volatility /= AutosizePeriod;
                return VolatilityScale*volatility;
            }
            if (VolatilityType == RencoVolatilityType.Размах)
            {
                double min = double.MaxValue, max = double.MinValue;
                if (SeriesSources[0] is CandlestickSeries)
                {
                    var candleSeries = (CandlestickSeries)SeriesSources[0];
                    for (var i = startIndex; i < candleIndex; i++)
                    {
                        if (candleSeries.Data.Candles[i].high > max)
                            max = candleSeries.Data.Candles[i].high;
                        if (candleSeries.Data.Candles[i].low < min)
                            min = candleSeries.Data.Candles[i].low;
                    }
                    var range = max - min;
                    return range > 0 ? range * VolatilityScale : defaultSize;
                }
                if (SeriesSources[0] is LineSeries)
                {
                    var lineSeries = (LineSeries)SeriesSources[0];
                    for (var i = startIndex; i < candleIndex; i++)
                    {
                        if (lineSeries.Data[i] > max) max = lineSeries.Data[i];
                        if (lineSeries.Data[i] < min) min = lineSeries.Data[i];
                    }
                    var range = max - min;
                    return range > 0 ? range * VolatilityScale : defaultSize;
                }
            }
            return defaultSize;
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            indSeriesRenco = new CandlestickSeries(Name) {DownFillColor = ColorBarDn, UpFillColor = ColorBarUp};
            SeriesResult = new List<Series.Series> { indSeriesRenco };
            EntitleIndicator();
        }

        public void Remove()
        {
        }

        public void AcceptSettings()
        {
            indSeriesRenco.DownFillColor = ColorBarDn;
            indSeriesRenco.UpFillColor = ColorBarUp;
        }

        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (newCandles == null) return;
            if (newCandles.Count == 0) return;
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
    // ReSharper restore InconsistentNaming

    struct RencoBrick
    {
        public double open;
        public double close;
        public int index;
    }
}

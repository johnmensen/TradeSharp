using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using Candlechart.Chart;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;
using TradeSharp.Util.Forms;

namespace Candlechart.Indicator
{
    // ReSharper disable LocalizableElement
    [LocalizedDisplayName("TitleUserIndex")]
    [LocalizedCategory("TitleCalculatedIndicatorsShort")]
    [TypeConverter(typeof(PropertySorter))]
    public partial class IndicatorIndexFunction : BaseChartIndicator, IChartIndicator, IHistoryQueryIndicator
    {
        private LineSeries seriesLine;

        private ExpressionResolver[] resolvers = new ExpressionResolver[5];

        private Random randomGener;

        public override BaseChartIndicator Copy()
        {
            var ind = new IndicatorIndexFunction();
            Copy(ind);
            return ind;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var ind = (IndicatorIndexFunction) indi;
            CopyBaseSettings(ind);
            ind.ShiftX = ShiftX;
            ind.ColorLine = ColorLine;
            ind.ColorFill = ColorFill;
            ind.FloodFill = FloodFill;
            ind.seriesLine = seriesLine;
            ind.resolvers = new ExpressionResolver[5];
            for (var i = 0; i < resolvers.Length; i++)
                ind.resolvers[i] = resolvers[i];
            ind.randomGener = randomGener;
            ind.SubstituteInfinite = SubstituteInfinite;
            ind.SubstituteNegInfinite = SubstituteNegInfinite;
            ind.SubstituteNaN = SubstituteNaN;
            ind.MinMinutesGapToUpdateCache = MinMinutesGapToUpdateCache;
        }

        [Browsable(false)]
        public override string Name { get { return Localizer.GetString("TitleUserIndex"); } }

        [LocalizedDisplayName("TitleSourceSeries")]
        [Description("Серии источников-данных для индикатора")]
        [LocalizedCategory("TitleMain")]
        [Editor("Candlechart.Indicator.CheckedListBoxSeriesUITypeEditor, System.Drawing.Design.UITypeEditor",
            typeof(UITypeEditor))]
        public override string SeriesSourcesDisplay { get; set; }

        [LocalizedDisplayName("TitleFormula1")]
        [Description("Формула индекса 1")]
        [LocalizedCategory("TitleMain")]
        [Editor(typeof(FormulaUIEditor), typeof(UITypeEditor))]
        public string IndexFormula1
        {
            get { return resolvers[0] == null ? "" : resolvers[0].Formula; }
            set { UpdateResolver(value, 0); }
        }

        [LocalizedDisplayName("TitleFormula2")]
        [Description("Формула индекса 2")]
        [LocalizedCategory("TitleMain")]
        [Editor(typeof(FormulaUIEditor), typeof(UITypeEditor))]
        public string IndexFormula2
        {
            get { return resolvers[1] == null ? "" : resolvers[1].Formula; }
            set { UpdateResolver(value, 1); }
        }

        [LocalizedDisplayName("TitleFormula3")]
        [Description("Формула индекса 3")]
        [LocalizedCategory("TitleMain")]
        [Editor(typeof(FormulaUIEditor), typeof(UITypeEditor))]
        public string IndexFormula3
        {
            get { return resolvers[2] == null ? "" : resolvers[2].Formula; }
            set { UpdateResolver(value, 2); }
        }

        [LocalizedDisplayName("TitleFormula4")]
        [Description("Формула индекса 4")]
        [LocalizedCategory("TitleMain")]
        [Editor(typeof(FormulaUIEditor), typeof(UITypeEditor))]
        public string IndexFormula4
        {
            get { return resolvers[3] == null ? "" : resolvers[3].Formula; }
            set { UpdateResolver(value, 3); }
        }

        [LocalizedDisplayName("TitleFormula5")]
        [Description("Формула индекса 5")]
        [LocalizedCategory("TitleMain")]
        [Editor(typeof(FormulaUIEditor), typeof(UITypeEditor))]
        public string IndexFormula5
        {
            get { return resolvers[4] == null ? "" : resolvers[4].Formula; }
            set { UpdateResolver(value, 4); }
        }

        private void UpdateResolver(string formula, int index)
        {
            if (resolvers[index] != null)
                if (resolvers[index].Formula == formula) return;
            try
            {
                resolvers[index] = string.IsNullOrEmpty(formula) ? null 
                    : new ExpressionResolver(formula);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка разбора выражения[{0}] [{1}]: {2}",
                    index, formula, ex);
                resolvers[index] = null;
            }
        }

        [LocalizedDisplayName("TitlePlusInfinityReplacement")]
        [Description("Значение, заменяющее +бесконечность")]
        [LocalizedCategory("TitleExceptions")]
        public double? SubstituteInfinite { get; set; }

        [LocalizedDisplayName("TitleMinusInfinityReplacement")]
        [Description("Значение, заменяющее -бесконечность")]
        [LocalizedCategory("TitleExceptions")]
        public double? SubstituteNegInfinite { get; set; }

        [LocalizedDisplayName("TitleNaNReplacement")]
        [Description("Значение, заменяющее недействительное значение")]
        [LocalizedCategory("TitleExceptions")]
        public double? SubstituteNaN { get; set; }

        [LocalizedDisplayName("TitleXOffset")]
        [Description("Смещение по оси Х, отсчетов")]
        [LocalizedCategory("TitleMain")]
        public int ShiftX { get; set; }

        private Color colorLine = Color.Black;
        [LocalizedDisplayName("TitleLineColor")]
        [Description("Цвет линии")]
        [LocalizedCategory("TitleVisuals")]
        public Color ColorLine
        {
            get { return colorLine; }
            set { colorLine = value; }
        }

        private Color colorFill = Color.Silver;
        [LocalizedDisplayName("TitleFillingColor")]
        [Description("Цвет заливки (если включена)")]
        [LocalizedCategory("TitleVisuals")]
        public Color ColorFill
        {
            get { return colorFill; }
            set { colorFill = value; }
        }

        private bool floodFill = true;
        [LocalizedDisplayName("TitleFilling")]
        [Description("Закрашивать область под кривой")]
        [LocalizedCategory("TitleVisuals")]
        public bool FloodFill { get { return floodFill; } set { floodFill = value; } }

        private static int indicatorThemeIndex;

        public void BuildSeries(ChartControl chart)
        {
            BuildSeries(chart, true);
        }

        private void BuildSeries(ChartControl chart, bool shouldLoadTickers)
        {
            if (DrawPane != owner.StockPane)
                DrawPane.Title = UniqueName;
            seriesLine.Data.Clear();
            Series.Series seriesSrc = chart.StockSeries;
            var dataCount = seriesSrc is StockSeries
                                ? ((StockSeries)seriesSrc).Data.Count
                                : seriesSrc is LineSeries
                                      ? ((LineSeries)seriesSrc).Data.Count : 0;
            var varNames = new List<string>();
            foreach (var resv in resolvers)
            {
                if (resv == null) continue;
                var resvVars = resv.GetVariableNames();
                foreach (var varName in resvVars.Where(varName => !varNames.Contains(varName)))
                {
                    varNames.Add(varName);
                }
            }

            randomGener = new Random(DateTime.Now.Millisecond);
            if (shouldLoadTickers) LoadTickers(varNames);
            var tickerNames = DalSpot.Instance.GetTickerNames();
            
            bool hasResolvers = resolvers.Any(resv => resv != null);

            for (var i = 0; i < dataCount; i++)
            {
                if (!hasResolvers)
                {
                    seriesLine.Data.Add(0);
                    continue;
                }
                var values = GetVariableValues(i, seriesSrc, dataCount, varNames, tickerNames);
                // выражение содержит "переменную", которая нигде не определена
                if (values.Count == 0 && varNames.Count > 0)
                {
                    seriesLine.Data.Add(0);
                    continue;
                }
                double totalResult = 0;

                for (var j = 0; j < resolvers.Length; j++)
                {
                    if (resolvers[j] == null) continue;

                    double result;
                    if (!resolvers[j].Calculate(values, out result))
                        result = SubstituteNaN ?? 0;

                    if (SubstituteNegInfinite.HasValue && double.IsNegativeInfinity(result))
                        result = SubstituteNegInfinite.Value;
                    else if (SubstituteInfinite.HasValue && double.IsInfinity(result))
                        result = SubstituteInfinite.Value;                    
                    // обновляем список переменных
                    var resultFuncName = string.Format("fn{0}", j + 1);
                    if (values.ContainsKey(resultFuncName))
                        values[resultFuncName] = result;
                    else values.Add(resultFuncName, result);
                    // сохраняем последний результат
                    totalResult = result;
                }
                seriesLine.Data.Add(totalResult);
            }
        }

        private Dictionary<string, double> GetVariableValues(int index, Series.Series seriesSrc, 
            int dataCount, List<string> variableNames, string[] tickerNames)
        {
            var varValues = new Dictionary<string, double>();
            if (variableNames == null) return new Dictionary<string, double>();
            // специальные имена переменных:
            // open, low, high, close - текущие OHLC-уровни для свечной серии
            // для серий типа LineSeries доступен только close
            // close#2 - цена два бара назад
            // i - индекс текущей свечи
            // count - количество данных в основном ряду
            // year, month, day, weekday, hour, minute, second
            // eurusd, audjpy#100 - close соотв. валютной пары
            // random - равномерно распред. СЧ на интервале 0..1
            
            // переменные с неизвестными именами инициализируются нулями
            foreach (var name in variableNames)
            {
                var variableName = name;
                if (name == "i")
                {
                    varValues.Add(name, index);
                    continue;
                }
                if (name == "count")
                {
                    varValues.Add(name, dataCount);
                    continue;
                }

                var tickerName = tickerNames.FirstOrDefault(tn => 
                    variableName.StartsWith(tn, StringComparison.OrdinalIgnoreCase));
                
                if (!string.IsNullOrEmpty(tickerName))
                {// котировка
                    var price = 1.0;
                    if (tickerData.ContainsKey(tickerName))
                    {
                        var tickerQuotes = tickerData[tickerName];
                        if (tickerQuotes.Count > 0)
                        {
                            var datIndex = index - GetDeltaIndexFromSharpSeparatedString(variableName);
                            datIndex = datIndex < 0
                                           ? 0 : datIndex >= tickerQuotes.Count ? tickerQuotes.Count - 1 : datIndex;
                            price = tickerQuotes[datIndex];                            
                        }
                    }
                    varValues.Add(name, price);
                    continue;
                }
                if (name == "random")
                {
                    varValues.Add(name, randomGener.NextDouble());
                    continue;
                }
                if (name.StartsWith("year") || name.StartsWith("month") || 
                    name.StartsWith("day") || name.StartsWith("weekday") ||
                    name.StartsWith("hour") || name.StartsWith("minute") ||
                    name.StartsWith("second"))
                {
                    // получить дату текущей либо одной из предыдущих точек графика
                    var datIndex = index - GetDeltaIndexFromSharpSeparatedString(name);

                    var curDate = DateTime.Now;
                    if (seriesSrc is StockSeries)
                    {
                        if (datIndex >= 0 && datIndex < ((StockSeries)seriesSrc).Data.Count)
                            curDate = ((StockSeries)seriesSrc).Data.Candles[datIndex].timeOpen;
                    }                        
                    else
                    {
                        if (datIndex >= 0 && datIndex < owner.StockSeries.Data.Count)
                            curDate = owner.StockSeries.Data.Candles[datIndex].timeOpen;
                    }

                    if (name.StartsWith("year")) varValues.Add(name, curDate.Year);
                    else if (name.StartsWith("month")) varValues.Add(name, curDate.Month);
                    else if (name.StartsWith("day")) varValues.Add(name, curDate.Day);
                    else if (name.StartsWith("weekday")) varValues.Add(name, (int)curDate.DayOfWeek);
                    else if (name.StartsWith("hour")) varValues.Add(name, curDate.Hour);
                    else if (name.StartsWith("minute")) varValues.Add(name, curDate.Minute);
                    else if (name.StartsWith("second")) varValues.Add(name, curDate.Second);

                    continue;
                }
                if (name.StartsWith("close") || name.StartsWith("open") || name.StartsWith("low") || name.StartsWith("high"))
                {// "close", "close#17" ...
                    var datIndex = index - GetDeltaIndexFromSharpSeparatedString(name);                    
                    // вытащить данные из серии
                    if (seriesSrc is StockSeries)
                    {
                        var candles = ((StockSeries)seriesSrc).Data.Candles;
                        if (datIndex < 0 || datIndex >= candles.Count)
                        {
                            varValues.Add(name, 0);
                            continue;
                        }
                        if (name.StartsWith("open")) varValues.Add(name, (double)candles[datIndex].open);
                        else if (name.StartsWith("high")) varValues.Add(name, (double)candles[datIndex].high);
                        else if (name.StartsWith("low")) varValues.Add(name, (double) candles[datIndex].low);
                        else if (name.StartsWith("close"))
                            varValues.Add(name, (double)candles[datIndex].close);
                    }
                    if (seriesSrc is LineSeries)
                    {
                        var serLine = (LineSeries) seriesSrc;
                        if (datIndex < 0 || datIndex >= serLine.Data.Count)
                        {
                            varValues.Add(name, 0);
                            continue;
                        }
                        varValues.Add(name, serLine.Data[datIndex]);
                    }
                    continue;
                }
                if (name.StartsWith("src"))
                {
                    var dotIndex = name.IndexOf('.');
                    var seriesName = dotIndex != -1 ? name.Substring(3, dotIndex - 2) : name.Substring(3);
                    var seriesIndex = seriesName.ToInt(0);
                    if (seriesIndex == 0)
                    {
                        varValues.Add(name, 0);
                        continue;
                    }
                    seriesIndex--;
                    if (seriesIndex < 0 || seriesIndex >= SeriesSources.Count)
                    {
                        varValues.Add(name, 0);
                        continue;
                    }
                    var series = SeriesSources[seriesIndex];
                    var dataIndex = index - GetDeltaIndexFromSharpSeparatedString(name);
                    var stockSeries = series as StockSeries;
                    if (stockSeries != null)
                    {
                        var candles = stockSeries.Data.Candles;
                        if (dataIndex < 0 || dataIndex >= candles.Count)
                        {
                            varValues.Add(name, 0);
                            continue;
                        }
                        var candleValue = dotIndex != -1 ? name.Substring(dotIndex + 1) : "";
                        if (candleValue == "open")
                            varValues.Add(name, candles[dataIndex].open);
                        else if (candleValue == "high")
                            varValues.Add(name, candles[dataIndex].high);
                        else if (candleValue == "low")
                            varValues.Add(name, candles[dataIndex].low);
                        else // "close" & default
                            varValues.Add(name, candles[dataIndex].close);
                        continue;
                    }
                    var lineSeries = series as LineSeries;
                    if (lineSeries != null)
                    {
                        if (dataIndex < 0 || dataIndex >= lineSeries.Data.Count)
                        {
                            varValues.Add(name, 0);
                            continue;
                        }
                        varValues.Add(name, lineSeries.Data[dataIndex]);
                        continue;
                    }
                    varValues.Add(name, 0);
                    continue;
                }
                // переменная не определена вообще - инициализируем 0-м
                varValues.Add(name, 0);
            }
            return varValues;
        }

        /// <summary>
        /// пример: для EURUSD#15 вернуть 15 (без знака!)
        /// </summary>        
        private static int GetDeltaIndexFromSharpSeparatedString(string varName)
        {
            var indexSharp = varName.IndexOf('#');
            if (indexSharp < 0) return 0;            
            var partIndex = varName.Substring(indexSharp + 1);
            var deltaIndex = partIndex.ToIntSafe();
            return deltaIndex ?? 0;            
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            // инициализируем индикатор
            EntitleIndicator();
            seriesLine = new LineSeries(Name);
            SeriesResult = new List<Series.Series> { seriesLine };
            // цветовая схема
            ColorLine = IndicatorColorScheme.colorsLine[indicatorThemeIndex];
            ColorFill = IndicatorColorScheme.colorsFill[indicatorThemeIndex++];
            if (indicatorThemeIndex >= IndicatorColorScheme.PresetColorsCount)
                indicatorThemeIndex = 0;
        }

        public void Remove()
        {
        }

        public void AcceptSettings()
        {
            seriesLine.ShiftX = ShiftX + 1;
            seriesLine.LineColor = ColorLine;            
            seriesLine.Transparent = !FloodFill;
            seriesLine.BackColor = ColorFill;
        }
        
        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (newCandles == null) return;
            if (newCandles.Count == 0) return;
            if (tickerData == null) return;

            // дополнить списки tickerData
            foreach (var ticker in tickerData)
            {
                var list = ticker.Value;
                var quote = QuoteStorage.Instance.ReceiveValue(ticker.Key);
                var lastPrice = quote != null ? (double)quote.bid : list.Count > 0 ? list[list.Count - 1] : 0;
                list.AddRange(newCandles.Select(t => lastPrice));
            }

            BuildSeries(owner, false);                
        }

        public string GetHint(int x, int y, double index, double price, int tolerance)
        {
            return string.Format("{0} = {1:f5}", UniqueName, seriesLine.Data[(int)index]);
        }

        public List<IChartInteractiveObject> GetObjectsUnderCursor(int screenX, int screenY, int tolerance)
        {
            return new List<IChartInteractiveObject>();
        }

        public override string GenerateNameBySettings()
        {
            return string.IsNullOrEmpty(FormulaParameterForm.lastUsedTemplate) ?
                Localizer.GetString("TitleUserIndex") : FormulaParameterForm.lastUsedTemplate;
        }
    }
    // ReSharper restore LocalizableElement
}

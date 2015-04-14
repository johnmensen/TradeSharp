using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using Candlechart.Core;
using Candlechart.Indicator;
using Entity;
using TradeSharp.Util;

namespace Candlechart
{
    /*
     * в модуле собран код по графическим индикаторам (СС, Зиг-Заг...)
     */
    public partial class CandleChartControl
    {
        public UpdateTickersCacheForRobotsExDel updateTickersCacheForRobots;

        public const string IndicatorsFileName = "indicator.xml";
        public readonly List<IChartIndicator> indicators = new List<IChartIndicator>();

        private static readonly Regex regxIndiNameSuffix = new Regex(@"(?<=\()\d+");

        /// <summary>
        /// на асинхронное обновление котировки
        /// </summary>        
        public void ProcessQuotesByIndicators(CandleData updatedCandle, List<CandleData> newCandles)
        {
            foreach (var indi in indicators)
            {
                indi.OnCandleUpdated(updatedCandle, newCandles);
            }
        }

        public void BuildIndicators(bool loadQuotesFromServer)
        {
            // для индексных индикаторов - обновить котировки
            if (loadQuotesFromServer)
            {
                var tickersToLoad = new Dictionary<string, DateTime>();
                foreach (IHistoryQueryIndicator indi in indicators.Where(i => i is IHistoryQueryIndicator))
                {
                    var indiTickers = indi.GetRequiredTickersHistory(null);
                    foreach (var tick in indiTickers)
                    {
                        if (tickersToLoad.ContainsKey(tick.Key))
                        {
                            if (tickersToLoad[tick.Key] > tick.Value)
                                tickersToLoad[tick.Key] = tick.Value;
                            continue;
                        }
                        tickersToLoad.Add(tick.Key, tick.Value);
                    }
                }

                if (tickersToLoad.Count > 0)
                    updateTickersCacheForRobots(tickersToLoad, 5);
            }

            // построить индикаторы
            foreach (var indi in indicators)
            {
                indi.BuildSeries(chart);
            }
        }

        #region Save
        public void SaveIndicatorSettings()
        {
            var path = string.Format("{0}\\{1}",
                ExecutablePath.ExecPath, IndicatorsFileName);
            try
            {
                SaveIndicatorSettings(path);
                Logger.Info("Индикаторы сохранены \"" + path + "\"");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка сохранения индикаторов \"" + path + "\"", ex);
            }
        }

        public void SaveIndicatorSettings(string path)
        {
            var doc = new XmlDocument();
            XmlElement docItem = null;
            if (File.Exists(path))
            {
                try
                {
                    doc.Load(path);
                    docItem = doc.DocumentElement;
                    if (docItem != null)
                        if (docItem.Name != "indicators")
                        {
                            docItem = null;
                            doc = new XmlDocument();
                        }
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка загрузки настроек индикатора", ex);
                    docItem = null;
                }
            }
            if (docItem == null)
                docItem = (XmlElement)doc.AppendChild(doc.CreateElement("indicators"));
            // если узел данного графика уже определен - удалить его
            var chartNodes = 
                docItem.SelectNodes(string.Format("/*/chart[@Id='{0}']",
                                              chart.Owner.UniqueId));
            if (chartNodes != null)
                if (chartNodes.Count > 0)                
                    docItem.RemoveChild(chartNodes[0]);

            // создать под корневым элементом узел для индикатора
            var chartNode = (XmlElement) docItem.AppendChild(doc.CreateElement("chart"));
            var atrId = chartNode.Attributes.Append(doc.CreateAttribute("Id"));
            atrId.Value = chart.Owner.UniqueId;
            SaveIndicatorSettings(chartNode);
            // сохранить
            doc.Save(path);
        }

        public void SaveIndicatorSettings(XmlElement chartNode)
        {
            foreach (var indi in indicators)
            {
                var xmlNode = chartNode.AppendChild(chartNode.OwnerDocument.CreateElement("indicator"));
                BaseChartIndicator.MakeIndicatorXMLNode(indi, xmlNode);
            }
        }

        
        #endregion

        #region Load
        public void LoadIndicatorSettings(XmlNode chartNode)
        {
            if (chartNode == null) return;
            var list = chartNode.SelectNodes("indicator");
            if (list == null) return;
            foreach (XmlElement node in list)
            {
                // - 1 - создать экземпляр класса
                var indi = BaseChartIndicator.LoadIndicator(node);
                if (indi == null) continue;

                if (indi is IChartQueryIndicator)
                    ((IChartQueryIndicator) indi).GetOuterCharts += getOuterCharts;
                
                // - 2 - добавить в список индикаторов и на график
                // добавляем в список индикаторов
                indicators.Add(indi);
                indi.Add(chart, null);
                indi.AcceptSettings();
            }
        }
        #endregion

        #region Устранение петель

        /// <summary>
        /// Индикатор может ссылаться 
        /// </summary>        
        /// <param name="misorderedArcs">нарушения в порядке загрузки</param>
        /// <param name="brokenArcs">петли</param>
        /// <returns>есть порванные дуги</returns>
        public bool FindBrokenIndicatorArcs(List<string> misorderedArcs,
            List<string> brokenArcs)
        {
            // упрощенный алгоритм Дейкстры - поиск пути из вершины А в вершину А
            var nodes = indicators.Select((t, i) => new IndicatorPathNode {indicator = t, order = i}).ToArray();

            // - 1 - найти нарушения порядка загрузки
            foreach (var indi in nodes)
            {
                if (indi.indicator.SeriesSources == null) continue;
                foreach (var src in indi.indicator.SeriesSources)
                {
                    var srcSeries = src;
                    // найти индикатор, которому принадлежит серия
                    var seriesOwner = nodes.FirstOrDefault(n => n.indicator.SeriesResult.Contains(srcSeries));
                    if (seriesOwner == null) continue;
                    if (seriesOwner.order >= indi.order)
                    {
                        misorderedArcs.Add(string.Format("{0}[{1}]: \"{2}\" ссылается на {3}[{4}]",
                            indi.indicator.UniqueName, indi.order, src.Name, seriesOwner.indicator.UniqueName, 
                            seriesOwner.order));
                    }
                }
            }
            // - 2 - найти петли (путь из вершины N в вершину N)
            var loops = new List<string>();
            foreach (var indi in nodes)
            {
                foreach (var nod in nodes) nod.visited = false;
                var pathInside = new StringBuilder(string.Format("[{0}]", indi.order));

                FindLoopsForNode(indi, indi, pathInside, loops, nodes);
                if (loops.Count > 0) brokenArcs.AddRange(loops);
            }

            return misorderedArcs.Count > 0 || brokenArcs.Count > 0;
        }

        private static void FindLoopsForNode(IndicatorPathNode nodeA,
            IndicatorPathNode nodeCur,
            StringBuilder path,
            List<string> loops,
            IndicatorPathNode[] nodes)
        {
            if (nodeCur.indicator.SeriesSources == null) return;
            foreach (var src in nodeCur.indicator.SeriesSources)
            {
                var srcSeries = src;
                // найти индикатор, которому принадлежит серия
                var seriesOwner = nodes.FirstOrDefault(n => n.indicator.SeriesResult.Contains(srcSeries));
                if (seriesOwner == null) continue;

                // найдена петля
                if (seriesOwner == nodeA)
                {
                    path.AppendFormat("->[{0}]", nodeA.order);
                    loops.Add(path.ToString());
                    continue;
                }

                if (!seriesOwner.visited)
                {
                    var pathInside = new StringBuilder(path.ToString());
                    pathInside.AppendFormat("->[{0}]", seriesOwner.order);
                    // рекурсия
                    FindLoopsForNode(nodeA, seriesOwner, pathInside, loops, nodes);
                }
                if (nodeCur != nodeA) nodeCur.visited = true;
            }
        }
        #endregion

        #region Целостность связей панелей - источников
        public void UpdateIndicatorPanesAndSeries()
        {
            // отвязать индикаторы с неактуальных панелей,
            // убрать панели
            foreach (var indi in indicators)
                ClearIndicatorPane(indi);
            // создать панели для индикаторов, если выставлен флаг
            foreach (var indi in indicators)
                CreateIndicatorPane(indi);
            // связать индикаторы с панелями
            foreach (var indi in indicators)
                if (!indi.CreateOwnPanel) indi.DrawPane = null;
            foreach (var indi in indicators)
                EnsureIndicatorPanel(indi);
            // установить серии-источники
            foreach (var indi in indicators)
                UpdateSourceSeries(indi);


            foreach (BaseChartIndicator indi in indicators)
                if (indi.ownPane != null) indi.ownPane.Title = indi.UniqueName;
            
        }

        public void RemoveIndicator(IChartIndicator indi)
        {
            if (indi.CreateOwnPanel)
            {
                foreach (var ind in indicators)
                {
                    if (ind == indi) continue;
                    if (ind.DrawPane == indi.DrawPane)
                    {// перенести все серии на панель по-умолчанию - курс
                        foreach (var series in ind.SeriesResult)
                        {
                            ind.DrawPane.Series.Remove(series);
                            chart.StockPane.Series.Add(series);
                        }
                        ind.DrawPane = chart.StockPane;
                        ind.DrawPaneDisplay = Localizer.GetString("TitleCourse");
                    }
                }
                // убрать саму панель
                if (chart.Panes.ContainsPane(indi.DrawPane)) chart.Panes.Remove(indi.DrawPane);
            }
            // убрать серии индикатора с панели
            if (indi.DrawPane != null)
            {
                foreach (var sr in indi.SeriesResult)
                {
                    if (indi.DrawPane.Series.ContainsSeries(sr))
                        indi.DrawPane.Series.Remove(sr);
                }
            }

            indi.Remove();

            // убрать сам индикатор
            indicators.Remove(indi);            
        }

        private void ClearIndicatorPane(IChartIndicator indi)
        {
            var indiPaneCode = indi.CreateOwnPanel ? indi.Name :
                string.IsNullOrEmpty(indi.DrawPaneDisplay)
                                   ? Localizer.GetString("TitleCourse") : indi.DrawPaneDisplay;
            // проверить соответствие панели ее строковому представлению
            if (indi.DrawPane != null)
            {
                // панель актуальна, не нужно устанавливать
                if (indiPaneCode == indi.GetFullyQualifiedPaneName()) return;
                if (indi.DrawPane.Name == indiPaneCode) return;
                // индикатор более не строится на данной панельке

                // снять индикатор с панели
                foreach (var series in indi.SeriesResult)
                    if (indi.DrawPane.Series.ContainsSeries(series))
                        indi.DrawPane.Series.Remove(series);

                // если панель была создана самим индикатором
                // удалить панель из списка
                if (indi.DrawPane == ((BaseChartIndicator)indi).ownPane)
                {
                    chart.Panes.Remove(indi.DrawPane);
                }
            }
        }

        private void CreateIndicatorPane(IChartIndicator indi)
        {
            // назначить индикатору панель
            if (!indi.CreateOwnPanel || ((BaseChartIndicator)indi).ownPane != null) return;

            var newPane = new Pane(indi.Name, chart);
            ((BaseChartIndicator)indi).ownPane = newPane;
            
            indi.DrawPane = newPane;
            foreach (var series in indi.SeriesResult)
                newPane.Series.Add(series);
            chart.Panes.Add(newPane);
        }

        private void EnsureIndicatorPanel(IChartIndicator indi)
        {
            if (indi.DrawPane != null) return;
            if (indi.CreateOwnPanel) return;

            var indiPaneCode = string.IsNullOrEmpty(indi.DrawPaneDisplay)
                                   ? Localizer.GetString("TitleCourse") : indi.DrawPaneDisplay;
            if (indi.CreateOwnPanel) indi.DrawPaneDisplay = string.Empty;
            if (string.IsNullOrEmpty(indi.DrawPaneDisplay) && indi.CreateOwnPanel == false)
                indi.DrawPaneDisplay = Localizer.GetString("TitleCourse");

            // если привязка к окну котировки...
            if (indiPaneCode == Localizer.GetString("TitleCourse"))
                indi.DrawPane = chart.StockPane;
            else
                foreach (var ind in indicators)
                {
                    if (indiPaneCode != ind.GetFullyQualifiedPaneName()) continue;
                    if (ind.DrawPane == null)
                    {// индикатор строится в окне индикатора, который сам строится в чужом
                        // окне, и оно для него (ind) еще не установлено - требуется рекурсия
                        EnsureIndicatorPanel(ind);
                    }
                    // теперь ind.DrawPane точно не null
                    indi.DrawPane = ind.DrawPane;
                }
            // положить серии на панель
            if (indi.DrawPane != null)
            foreach (var series in indi.SeriesResult)
                indi.DrawPane.Series.Add(series);
        }

        public void EnsureUniqueName(IChartIndicator indi)
        {
            if (indicators.Count(i => i.UniqueName == indi.UniqueName) > 1)
                MakeNewIndiName(indi);
        }

        private void MakeNewIndiName(IChartIndicator indi)
        {
            // если суфикс уже добавлен - выделить его
            decimal? indiSuffix = null;
            var matches = regxIndiNameSuffix.Matches(indi.UniqueName);
            var suffixStart = -1;
            if (matches.Count > 0)
            {
                indiSuffix = matches[matches.Count - 1].Value.ToInt();
                suffixStart = matches[matches.Count - 1].Index;
            }

            var newSuffix = (indiSuffix ?? 1) + 1;
            var nameRoot = suffixStart < 0 ? indi.UniqueName 
                : indi.UniqueName.Substring(suffixStart - 1);
            string newName;
            while (true)
            {
                newName = string.Format("{0}({1})", nameRoot, newSuffix++);
                var testedName = newName;
                if (!indicators.Any(ind => ind.UniqueName == testedName)) break;
            }
            indi.UniqueName = newName;
        }

        public void RefreshDisplaySeriesAndPanels(string oldname, string newName)
        {
            foreach (var indi in indicators.Where(indi => indi.SeriesSourcesDisplay != null))
            {
                indi.SeriesSourcesDisplay = indi.SeriesSourcesDisplay.Replace(oldname +  Separators.IndiNameDelimiter[0], newName + Separators.IndiNameDelimiter[0]);
                if (indi.DrawPaneDisplay != null)
                    indi.DrawPaneDisplay = indi.DrawPaneDisplay.Replace(oldname + Separators.IndiNameDelimiter[0], newName + Separators.IndiNameDelimiter[0]);
            }
        }

        private void UpdateSourceSeries(IChartIndicator indi)
        {
            indi.SeriesSources = new List<Series.Series>();
            var seriesSrcString = string.IsNullOrEmpty(indi.SeriesSourcesDisplay)
                                          ? Localizer.GetString("TitleCourse") : indi.SeriesSourcesDisplay;
            // текстовое представление серий заполнено, значит считали из файла настроек и надо найти
            // серию источник из списка индикаторов
            // проводим разбор серий
            var strSeries = seriesSrcString.Split(Separators.SourcesDelimiter, StringSplitOptions.None);
            // strSeries содержит набор <имя индикатора>.<имя серии источника>
            foreach (var series in strSeries)
            {
                if (series == Localizer.GetString("TitleCourse"))
                {                    
                    indi.SeriesSources.Add(chart.StockSeries);                    
                }
                else
                {
                    // есть имя индикатора и серия, надо найти в списке
                    var indiSeries = series.Split(Separators.IndiNameDelimiter, StringSplitOptions.None); 
                    if (indiSeries.Count() != 2) continue;

                    foreach (var indicator in indicators)
                    {
                        if (indicator.UniqueName != indiSeries[0]) continue;
                        // нашли индикатор, ищем серию
                        foreach (var s in indicator.SeriesResult.Where(s => s.Name == indiSeries[1]))
                            indi.SeriesSources.Add(s);
                    }
                }
            }
        }       
        #endregion

        #region Мышиная возня
        private bool IndicatorsProcessMouseUp(MouseEventArgs e)
        {
            var updated = false;
            var keys = ModifierKeys;
            foreach (BaseChartIndicator indi in indicators)
            {
                if (indi.OnMouseButtonUp(e, keys)) updated = true;
            }
            return updated;
        }

        private bool IndicatorsProcessMouseDown(MouseEventArgs e)
        {
            var updated = false;
            var keys = ModifierKeys;
            foreach (BaseChartIndicator indi in indicators)
            {
                if (indi.OnMouseButtonDown(e, keys)) updated = true;
            }
            return updated;
        }

        private bool IndicatorsProcessMouseMove(MouseEventArgs e)
        {
            var updated = false;
            var keys = ModifierKeys;
            foreach (BaseChartIndicator indi in indicators)
            {
                if (indi.OnMouseButtonMove(e, keys)) updated = true;
            }
            return updated;
        }
        #endregion
    }
    class IndicatorPathNode
    {
        public IChartIndicator indicator;
        public bool visited;
        public int order;
    }
}
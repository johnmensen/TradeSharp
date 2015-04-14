using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Candlechart.ChartMath;
using Candlechart.Controls;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Util;

namespace Candlechart
{
    /*
     * в модуле собран код по графическим объектам (линии тренда, эллипсы и т.д.)
     */
    public partial class CandleChartControl
    {
        public delegate void ChartToolChangedDel(ChartTool newTool);

        private ChartToolChangedDel onChartToolChanged;

        public event ChartToolChangedDel ChartToolChanged
        {
            add { onChartToolChanged += value; }
            remove { onChartToolChanged -= value; }
        }

        private readonly List<InteractiveObjectSeries> listInteractiveSeries = new List<InteractiveObjectSeries>();
        public ProjectionSeries seriesProjection;
        public SeriesComment seriesComment;
        public SeriesEllipse seriesEllipse;
        public SeriesPopup seriesPopup;
        public SeriesFiboSpan seriesFiboSpan;
        public TrendLineSeries seriesTrendLine;
        public SeriesFiboChannel seriesFiboChannel;
        public SeriesMarker seriesMarker;
        public SeriesAsteriks seriesAsteriks;
        public TurnBarSeries seriesTurnBar;

        /// <summary>
        /// текущий выбранный инструмент свечного графика
        /// </summary>
        public enum ChartTool
        {
            None = 0,
            Cursor,   // выделение графических объектов (курсор)
            TurnBar,    // разворотные бары (Фибоначчи) - мега Грааль
            Projection, // золотое сечение
            Comment,    // коментарий
            Ellipse,    // эллипс
            FiboSpan,   // гор-зт расширение Фибо (по двум точкам)
            TrendLine,  // трендовые линии (просто линии)
            FiboChannel,// канал Фибоначчи
            Marker,     // маркеры сделок
            Asteriks,   // маленький маркер с раскрывающимся коментарием
            Cross,      // через курсор мыши рисовать линии
            Script      // вызвать скрипт
        }

        private ChartTool activeChartTool = ChartTool.Cursor;
        public ChartTool ActiveChartTool
        {
            get { return activeChartTool; }
            set
            {
                if (value == activeChartTool) return;
                chart.interactiveToolsEnabled =
                    value == ChartTool.Cursor || value == ChartTool.Cross; // || value == ChartTool.Script;

                if (activeChartTool == ChartTool.Cursor) // выйти из режима редактирования
                    QuitEditMode();

                if (value == ChartTool.Cursor || value == ChartTool.Cross) // || value == ChartTool.Script)
                    ActivateNonSeriesChartTool(value);

                activeChartTool = value;
            }
        }

        public List<SeriesEditParameter> seriesEditParameters;

        private bool adjustObjectColorsOnCreation = true;
        /// <summary>
        /// если флаг взведен - цвет нового, только что созданного объекта, подстроится под цветовую схему
        /// </summary>
        public bool AdjustObjectColorsOnCreation
        {
            get { return adjustObjectColorsOnCreation; }
            set { adjustObjectColorsOnCreation = value; }
        }

        private void InitializeSeries()
        {
            seriesProjection = new ProjectionSeries("Проекции");
            seriesComment = new SeriesComment("Коментарии");
            seriesEllipse = new SeriesEllipse("Эллипсы");
            seriesPopup = new SeriesPopup("Информация по графику");
            seriesFiboSpan = new SeriesFiboSpan("Горизонтальная проекция");
            seriesTrendLine = new TrendLineSeries("Трендовые линии");
            seriesFiboChannel = new SeriesFiboChannel("Пропорциональные каналы");
            seriesMarker = new SeriesMarker("Маркеры");
            seriesAsteriks = new SeriesAsteriks("Подсказки");
            seriesTurnBar = new TurnBarSeries("Бары разворота");

            // список серий с интерактивными объектами
            listInteractiveSeries.Add(seriesProjection);
            
            listInteractiveSeries.Add(seriesEllipse);            
            listInteractiveSeries.Add(seriesFiboSpan);
            listInteractiveSeries.Add(seriesTrendLine);
            listInteractiveSeries.Add(seriesFiboChannel);
            listInteractiveSeries.Add(seriesMarker);
            listInteractiveSeries.Add(seriesAsteriks);
            listInteractiveSeries.Add(seriesTurnBar);
            listInteractiveSeries.Add(seriesComment);

            chart.StockPane.Series.Add(seriesMarker);
            chart.StockPane.Series.Add(seriesFiboChannel);
            chart.StockPane.Series.Add(seriesTrendLine);
            chart.StockPane.Series.Add(seriesFiboSpan);
            chart.StockPane.Series.Add(seriesEllipse);
            chart.StockPane.Series.Add(seriesPopup);
            chart.StockPane.Series.Add(seriesComment);
            chart.StockPane.Series.Add(seriesProjection);
            chart.StockPane.Series.Add(seriesAsteriks);
            chart.StockPane.Series.Add(seriesTurnBar);
        }
 
        /// <summary>
        /// выбрать, какой из двух инструментов - масштаб или перекрестие
        /// будет доступен по левой кнопке мыши
        /// </summary>
        private void ActivateNonSeriesChartTool(ChartTool newTool)
        {
            //if (newTool == ChartTool.Script) return;
            crossTool.MouseButton = newTool == ChartTool.Cross
                                        ? MouseButtons.Left
                                        : MouseButtons.Middle;
            zoomTool.MouseDragButton = newTool == ChartTool.Cursor
                                        ? MouseButtons.Left
                                        : MouseButtons.Middle;
        }

        public void ShowObjectsDialog()
        {
            var objects = new List<IChartInteractiveObject>();
            foreach (var series in listInteractiveSeries)
            {
                series.AddObjectsInList(objects);
            }
            var dlg = new ObjectGridForm(objects, this, false);
            dlg.ShowDialog();
        }

        /// <summary>
        /// вызывается самим графиком, когда необходимо сменить инструмент редактирования
        /// основная форма получает уведомление и переключает кнопку
        /// </summary>
        private void SwitchChartTool(ChartTool tool)
        {
            if (activeChartTool == tool) return;
            ActiveChartTool = tool;
            if (onChartToolChanged != null)
                onChartToolChanged(tool);
        }

        /// <summary>
        /// обработать событие - отпущена кнопка мыши
        /// добавить новую точку в трендовую линию, добавить текстовый маркер либо 
        /// что-нибудь иное
        /// </summary>
        /// <returns>true если надо перерисовать (RedrawChartSafe)</returns>
        public bool SeriesProcessMouseButton(MouseEventArgs e, bool isMouseDown)
        {
            if (ActiveChartTool == ChartTool.Cursor) return false;
            foreach (var series in listInteractiveSeries)
            {
                IChartInteractiveObject objToEdit;
                var rst = isMouseDown
                              ? series.OnMouseDown(e, seriesEditParameters, ActiveChartTool, ModifierKeys, out objToEdit)
                              : series.OnMouseUp(seriesEditParameters, e, ActiveChartTool, ModifierKeys, out objToEdit);
                if (!rst) continue;
                
                // обновить время модификации объектов
                timeUpdateObjects.Touch();

                // переключиться в режим редактирования объекта?
                if (objToEdit != null)
                {
                    SwitchChartTool(ChartTool.Cursor);
                    SelectObject(objToEdit);
                }
                break;
            }
            return true;
        }

        public PointD MouseToWorldCoords(int x, int y)
        {
            var clientPoint = chart.PointToScreen(new Point(x, y));
            clientPoint = chart.StockPane.PointToClient(clientPoint);
            return Conversion.ScreenToWorld(new PointD(clientPoint.X, clientPoint.Y),
               chart.StockPane.WorldRect, chart.StockPane.CanvasRect);
        }

        public Point WorldToChartCoords(double x, double y)
        {
            var ptScreen = Conversion.WorldToScreen(new PointD(x, y),
               chart.StockPane.WorldRect, chart.StockPane.CanvasRect);
            return chart.StockPane.ClientToChart(
                new Point((int) Math.Round(ptScreen.X), (int) Math.Round(ptScreen.Y)));
        }

        /// <summary>
        /// по координатам курсора в области чарта получить
        /// индекс свечи (a) и ближайшую цену (b) из high, low, open, close
        /// </summary>        
        public Cortege2<int, float>? GetCandlePointByMouseCoord(int x, int y)
        {
            var pointD = Conversion.ScreenToWorld(new PointD(x, y),
               chart.StockPane.WorldRect, chart.StockPane.CanvasRect);
            // получить индекс
            var index = (int)(pointD.X + 0.5);
            if (index < 0 || index >= chart.StockSeries.Data.Count) return null;
            // получить цену
            var candle = chart.StockSeries.Data[index];
            float a = Math.Abs((float)pointD.Y - candle.open),
                    b = Math.Abs((float)pointD.Y - candle.close),
                    c = Math.Abs((float)pointD.Y - candle.high),
                    d = Math.Abs((float)pointD.Y - candle.low);
            float price = a <= b && a <= c && a <= d ? candle.open
                : b <= a && b <= c && b <= d ? candle.close
                : c <= a && c <= b && c <= d ? candle.high
                : candle.low;
            return new Cortege2<int, float> { a = index, b = price };
        }

        public void SaveObjects(XmlElement xmlRoot)
        {
            if (xmlRoot == null)
                // ReSharper disable LocalizableElement
                throw new ArgumentException("SaveObjects - передан пустой узел дерева", "xmlRoot");
                // ReSharper restore LocalizableElement
            foreach (var series in listInteractiveSeries)
            {
                if (series.DataCount == 0) continue;
                // создать узел с именем - тип серии
                var newNode = (XmlElement)xmlRoot.AppendChild(xmlRoot.OwnerDocument.CreateElement("series"));
                var atrId = newNode.Attributes.Append(xmlRoot.OwnerDocument.CreateAttribute("type"));
                atrId.Value = series.GetType().ToString();
                try
                {
                    var lst = new List<IChartInteractiveObject>();
                    series.AddObjectsInList(lst);
                    // сохранить объекты
                    foreach (var obj in lst) obj.SaveInXML(newNode, this);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("[{0}] ошибка сохранения серии объектов {1}: {2}", 
                        Symbol, series.GetType(), ex);
                }
            }
        }

        public void LoadObjects(XmlElement xmlRoot)
        {
            LoadObjects(xmlRoot, false, false);
        }

        public void LoadObjects(XmlElement xmlRoot, bool trimOutOfHistoryObjects, bool ajustColorScheme)
        {
            if (xmlRoot == null) return;
            foreach (XmlElement nodeSeries in xmlRoot.ChildNodes)
            {
                // получить тип серии
                if (nodeSeries.Attributes["type"] == null) continue;
                var typeStr = nodeSeries.Attributes["type"].Value;
                if (string.IsNullOrEmpty(typeStr)) continue;

                foreach (var series in listInteractiveSeries)
                {
                    if (series.GetType().ToString() == typeStr)
                    {
                        // очистить серию
                        while (series.DataCount > 0)
                            series.RemoveObjectByNum(0);

                        // загрузить объекты
                        foreach (XmlElement nodeObject in nodeSeries.ChildNodes)
                        {
                            IChartInteractiveObject obj = null;
                            try
                            {
                                obj = series.LoadObject(nodeObject, this, trimOutOfHistoryObjects);
                            }
                            catch (Exception ex)
                            {
                                Logger.ErrorFormat("Ошибка загрузки объекта серии {0}: {1}", series.GetType().Name, ex);
                            }
                            if (obj == null) continue;
                            if (ajustColorScheme) obj.AjustColorScheme(this);
                        }
                        series.ProcessLoadingCompleted(this);
                    }
                }
            }
        }

        /// <summary>
        /// проверить цвета объектов, индикаторов и инструментов, чтобы они были видны на фоне графика
        /// </summary>
        public void AdjustColors()
        {
            // серии с объектами
            foreach (var series in listInteractiveSeries)
                series.AdjustColorScheme(this);    
            // быстрые кнопки
            foreach (var btn in chart.StockPane.customButtons)
                btn.SetThemeByBackColor(chart.visualSettings.ChartBackColor);
        }

        public void DeleteObjectsByMagic(int magic)
        {
            foreach (var series in listInteractiveSeries)
            {
                for (var i = 0; i < series.DataCount; i++)
                {
                    var obj = series.GetObjectByNum(i);
                    if (obj.Magic == magic)
                    {
                        series.RemoveObjectByNum(i);
                        i--;
                    }
                }
            }
        }

        public void DeleteSeriesObject(IChartInteractiveObject obj)
        {
            foreach (var series in listInteractiveSeries)
            {
                series.RemoveObjectFromList(obj);
            }
        }

        /// <summary>
        /// применить настройки серий из UserSettings
        /// </summary>
        public void SetupSeries(SeriesSettings sets)
        {
            if (sets == null) return;
            // бары Фибо
            if (sets.FiboLevels != null && sets.FiboLevels.Count > 0)
                seriesTurnBar.fibonacciSeries = sets.FiboLevels.ToArray();
            seriesTurnBar.fibonacciTurnBarFilter = sets.FiboFilter;
            if (sets.FiboMarks != null && sets.FiboMarks.Count > 0)
                seriesTurnBar.fibonacciMarks = sets.FiboMarks.ToArray();
            seriesTurnBar.DontSumDegree = sets.FiboDontSumDegrees;
        }

        public SeriesSettings SaveSeriesSettings(SeriesSettings sets)
        {
            if (sets == null) sets = new SeriesSettings();
            // бары Фибо
            sets.FiboLevels = seriesTurnBar.fibonacciSeries.ToList();
            sets.FiboFilter = seriesTurnBar.fibonacciTurnBarFilter;
            sets.FiboDontSumDegrees = seriesTurnBar.DontSumDegree;
            sets.FiboMarks = seriesTurnBar.fibonacciMarks.ToList();
            return sets;
        }
    
        /// <summary>
        /// загрузить настройки серий
        /// (размер шрифта, цветовая схема по-умолчанию и т.п.)
        /// </summary>
        public static void LoadSeriesSettingsFromXml(XmlElement nodeRoot)
        {
            if (nodeRoot == null) return;
            var seriesTypes = InteractiveObjectSeries.GetObjectSeriesTypes();

            foreach (XmlElement node in nodeRoot)
            {
                var seriesName = node.Name;
                // найти серию
                var series = seriesTypes.FirstOrDefault(s => s.Name == seriesName);
                if (series == null) continue;
                // инициализировать статические параметры серии
                PropertyXMLTagAttribute.InitStaticProperties(series, node, false);
            }
        }

        /// <summary>
        /// сохранить настройки серий (см LoadSeriesSettingsFromXml)
        /// </summary>
        public static void SaveSeriesSettingsInXml(XmlElement nodeRoot)
        {
            var seriesTypes = InteractiveObjectSeries.GetObjectSeriesTypes();
            foreach (var series in seriesTypes)
            {
                var nodeSeries = nodeRoot.OwnerDocument.CreateElement(series.Name);
                if (!PropertyXMLTagAttribute.SaveStaticProperties(series, nodeSeries, false)) continue;
                if (nodeSeries.ChildNodes.Count == 0) continue;
                nodeRoot.AppendChild(nodeSeries);
            }
        }
    }
}
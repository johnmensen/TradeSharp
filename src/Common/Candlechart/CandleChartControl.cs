using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using Candlechart.ChartMath;
using Candlechart.Controls;
using Candlechart.Core;
using Candlechart.Indicator;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace Candlechart
{
    public partial class CandleChartControl : UserControl
    {
        #region Делегаты
        public delegate void ShowQuoteArchiveDel(string symbol);
        public delegate void NewOrderDel(string symbol);
        public delegate void NewsReceivedDel(News[] news);
        public delegate void PositionsReceivedDel(MarketOrder[] pos);
        public delegate void PendingOrdersReceivedDel(PendingOrder[] ord);
        public delegate void ClosedOrdersReceivedDel(List<MarketOrder> pos);
        public delegate List<MarketOrder> GetMarketOrdersDel();
        public delegate List<PendingOrder> GetPendingOrdersDel();
        public delegate List<MarketOrder> GetClosedOrdersDel();
        public delegate void ChartScaleChangedDel(DateTime left, DateTime right, CandleChartControl chart);
        public delegate void CursorCrossUpdatedDel(DateTime? time, double? price, CandleChartControl sender);
        public delegate void WindowTitleUpdatedDel(string title);
        public delegate void FavoriteIndicatorsAreUpdatedDel(List<string> favIndis);
        public delegate List<string> GetFavoriteIndicatorsDel();
        public delegate int[] GetDefaultFastDealVolumesDel();
        public delegate void UpdateFastVolumesDel(int[] volumes);
        public delegate void MakeTradeDel(DealType side, string symbol, int volume);
        public delegate void SyncQuoteHistoryDel(List<string> tickers, DateTime timeStart, bool showQuoteForm);
        #endregion

        #region События

        private Action<MarketOrder> onShowWindowEditMarketOrder;
        public event Action<MarketOrder> OnShowWindowEditMarketOrder
        {
            add { onShowWindowEditMarketOrder += value; }
            remove { onShowWindowEditMarketOrder -= value; }
        }

        private Action<MarketOrder> onShowWindowInfoOnClosedOrder;
        public event Action<MarketOrder> OnShowWindowInfoOnClosedOrder
        {
            add { onShowWindowInfoOnClosedOrder += value; }
            remove { onShowWindowInfoOnClosedOrder -= value; }
        }

        private Action<PendingOrder> onShowWindowEditPendingOrder;
        public event Action<PendingOrder> OnShowWindowEditPendingOrder
        {
            add { onShowWindowEditPendingOrder += value; }
            remove { onShowWindowEditPendingOrder -= value; }
        }

        private Action<MarketOrder> onEditMarketOrderRequest;
        public event Action<MarketOrder> OnEditMarketOrderRequest
        {
            add { onEditMarketOrderRequest += value; }
            remove { onEditMarketOrderRequest -= value; }
        }

        private ChartScaleChangedDel onScaleChanged;
        public event ChartScaleChangedDel OnScaleChanged
        {
            add { onScaleChanged += value; }
            remove { onScaleChanged -= value; }
        }

        public CursorCrossUpdatedDel onCursorCrossUpdated;
        public event CursorCrossUpdatedDel CursorCrossIsUpdated
        {
            add { onCursorCrossUpdated += value; }
            // ReSharper disable DelegateSubtraction
            remove { onCursorCrossUpdated -= value; }
            // ReSharper restore DelegateSubtraction
        }

        private NewOrderDel newOrder;
        public event NewOrderDel NewOrder
        {
            add { newOrder += value; }
            // ReSharper disable DelegateSubtraction
            remove { newOrder -= value; }
            // ReSharper restore DelegateSubtraction
        }

        private ShowQuoteArchiveDel showQuoteArchive;
        public event ShowQuoteArchiveDel ShowQuoteArchive
        {
            add { showQuoteArchive += value; }
            // ReSharper disable DelegateSubtraction
            remove { showQuoteArchive -= value; }
            // ReSharper restore DelegateSubtraction
        }

        private SyncQuoteHistoryDel syncQuoteHistory;
        public event SyncQuoteHistoryDel SyncQuoteHistory
        {
            add { syncQuoteHistory += value; }
            // ReSharper disable DelegateSubtraction
            remove { syncQuoteHistory -= value; }
            // ReSharper restore DelegateSubtraction
        }

        private Action<List<int>> editMarketOrders;
        public event Action<List<int>> EditMarketOrders
        {
            add { editMarketOrders += value; }
            remove { editMarketOrders -= value; }
        }

        private Action visualSettingsSetupCalled;
        public event Action VisualSettingsSetupCalled
        {
            add { visualSettingsSetupCalled += value; }
            remove { visualSettingsSetupCalled -= value; }
        }

        private Action<string> profitByTickerRequested;
        public event Action<string> ProfitByTickerRequested
        {
            add { profitByTickerRequested += value; }
            remove { profitByTickerRequested -= value; }
        }
        
        public MakeTradeDel makeTrade;

        public Func<int?> getAccountId;

        public Func<int, List<News>> getAllNewsByChannel;

        public GetFavoriteIndicatorsDel getFavoriteIndicators;

        public GetDefaultFastDealVolumesDel getDefaultFastDealVolumes;

        private FavoriteIndicatorsAreUpdatedDel favoriteIndicatorsAreUpdated;

        public event FavoriteIndicatorsAreUpdatedDel FavoriteIndicatorsAreUpdated
        {
            add { favoriteIndicatorsAreUpdated += value; }
            remove { favoriteIndicatorsAreUpdated -= value; }
        }

        public void CallFavoriteIndicatorsAreUpdated(List<string> favIndis)
        {
            if (favoriteIndicatorsAreUpdated != null)
                favoriteIndicatorsAreUpdated(favIndis);
        }

        private UpdateFastVolumesDel updateFastVolumes;

        public event UpdateFastVolumesDel UpdateFastVolumes
        {
            add { updateFastVolumes += value; }
            remove { updateFastVolumes -= value; }
        }

        private NewsReceivedDel onNewsReceived;
        public event NewsReceivedDel OnNewsReceived
        {
            add { onNewsReceived += value; }
            remove { onNewsReceived -= value; }
        }

        private PositionsReceivedDel onPositionsReceived;
        public event PositionsReceivedDel OnPositionsReceived
        {
            add { onPositionsReceived += value; }
            remove { onPositionsReceived -= value; }
        }

        private PendingOrdersReceivedDel onPendingOrdersReceived;
        public event PendingOrdersReceivedDel OnPendingOrdersReceived
        {
            add { onPendingOrdersReceived += value; }
            remove { onPendingOrdersReceived -= value; }
        }

        private ClosedOrdersReceivedDel onClosedOrdersReceived;
        public event ClosedOrdersReceivedDel OnClosedOrdersReceivedDel
        {
            add { onClosedOrdersReceived += value; }
            remove { onClosedOrdersReceived -= value; }
        }

        /// <summary>
        /// метод для получения других окошек графиков,
        /// данные с которых могут использовать индикаторы
        /// </summary>
        public GetOuterChartsDel getOuterCharts;

        private OrdersUpdatedDel marketOrdersUpdated;
        public event OrdersUpdatedDel MarketOrdersUpdated
        {
            add { marketOrdersUpdated += value; }
            remove { marketOrdersUpdated -= value; }
        }

        private OrdersUpdatedDel pendingOrdersUpdated;
        public event OrdersUpdatedDel PendingOrdersUpdated
        {
            add { pendingOrdersUpdated += value; }
            remove { pendingOrdersUpdated -= value; }
        }

        private OrdersUpdatedDel closedOrdersUpdated;
        public event OrdersUpdatedDel ClosedOrdersUpdated
        {
            add { closedOrdersUpdated += value; }
            remove { closedOrdersUpdated -= value; }
        }

        private WindowTitleUpdatedDel onWindowTitleUpdated;
        public event WindowTitleUpdatedDel OnWindowTitleUpdated
        {
            add { onWindowTitleUpdated += value; }
            remove { onWindowTitleUpdated -= value; }
        }

        private Action<List<int>> closeMarketOrders;
        public event Action<List<int>> CloseMarketOrders
        {
            add { closeMarketOrders += value; }
            remove { closeMarketOrders -= value; }
        }

        public GetMarketOrdersDel receiveMarketOrders;
        
        public GetPendingOrdersDel receivePendingOrders;
        
        public Action enforceClosedOrdersUpdate;

        public Func<string, List<string>> getRobotsByTicker;

        public Action<string> onRobotSelected;

        public void CallEditMarketOrders(List<int> orderIds)
        {
            if (editMarketOrders != null)
                editMarketOrders(orderIds);
        }

        public void CallShowWindowEditMarketOrder(MarketOrder order)
        {
            if (onShowWindowEditMarketOrder != null)
                onShowWindowEditMarketOrder(order);
        }

        public void CallShowWindowEditPendingOrder(PendingOrder order)
        {
            if (onShowWindowEditPendingOrder != null)
                onShowWindowEditPendingOrder(order);
        }

        public void CallEditMarketOrderRequest(MarketOrder order)
        {
            if (onEditMarketOrderRequest != null)
                onEditMarketOrderRequest(order);
        }

        public void CallShowWindowInfoOnClosedOrder(MarketOrder order)
        {
            if (onShowWindowInfoOnClosedOrder != null)
                onShowWindowInfoOnClosedOrder(order);
        }

        public void CallCloseMarketOrders(List<int> orderIds)
        {
            if (closeMarketOrders != null)
                closeMarketOrders(orderIds);
        }

        private Action openScriptSettingsDialog;

        public event Action OpenScriptSettingsDialog
        {
            add { openScriptSettingsDialog += value; }
            remove { openScriptSettingsDialog -= value; }
        }
        #endregion

        #region Данные
        /// <summary>
        /// время последнего обновления настроек индикаторов
        /// </summary>
        public static readonly ThreadSafeTimeStamp timeUpdateIndicators = new ThreadSafeTimeStamp();
        /// <summary>
        /// время последнего обновления настроек объектов
        /// </summary>
        public static readonly ThreadSafeTimeStamp timeUpdateObjects = new ThreadSafeTimeStamp();
        /// <summary>
        /// если флаг взведен, чарт не распространяет событие изменения масштаба
        /// флаг сбрасывается сразу после проверки
        /// </summary>
        private bool DoNotSpreadScale { get; set; }

        private string extraTitle;
        /// <summary>
        /// Дополнение к заголовку окна (обычно - суммарная позиция по инструменту графика)
        /// </summary>
        public string ExtraTitle
        {
            get { return extraTitle; }
            set
            {
                extraTitle = value;
                UpdateWindowTitle();
            }
        }
        /// <summary>
        /// флаг взводится после того, как чарт полностью сформировался
        /// </summary>
        public bool SynchronizationEnabled { get; set; }
        /// <summary>
        /// задается программно
        /// необходим для ассоциации сохраненных форм и объектов (эллипсов, коментариев и т.п.)
        /// </summary>
        public string UniqueId { get; set; }
        
        // масштабирование
        private const double ZoomInRate = 4D;
        private const double ZoomOutRate = 0.3D;

        /// <summary>
        /// к инструменту можно обратиться для запрета масштабирования
        /// во время редактирования объектов
        /// </summary>
        private ZoomTool zoomTool;

        /// <summary>
        /// инструмент - перекрестие (изначально доступен по правой кнопке мыши)
        /// </summary>
        private CrossTool crossTool;

        /// <summary>
        /// разрешить доп. визуальные стили (например, заливки - текстуры)
        /// </summary>
        public bool EnableExtendedVisualStyles { get; set; }
        #endregion

        #region Объекты
        private CandlePacker candlePacker;
        #endregion

        #region Свойства-обертки
        public string Symbol
        {
            get { return chart.Symbol; }
            set { chart.Symbol = value; }
        }

        public string timeframeString;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public BarSettings Timeframe
        {
            get { return chart.Timeframe; }
            set
            {
                chart.Timeframe = value;
                timeframeString = BarSettingsStorage.Instance.GetBarSettingsFriendlyName(value);
                UpdateWindowTitle();
            }
        }

        public Point mouseCursor;

        /// <summary>
        /// Текущий шаблон, применённый к графику
        /// </summary>
        [Browsable(false)]
        public string CurrentTemplateName { get; set; }
        #endregion

        #region Обработчики
        public void ChartMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {// подготовить меню
                MakeupPopupMenu(e.X, e.Y);
                return;
            }
            if (e.Button == MouseButtons.Middle)
                return;

            if (e.Button == MouseButtons.Left)
            {
                // старт драга курсора по оси Y?
                // (изменение масштаба по Y)
                if (chart.StockPane.YAxis.ContainsCursor(e.X, e.Y))
                    return;
            }

            var needRedraw = false;
            
            // вызвать скрипт
            if (ActiveChartTool == ChartTool.Script)
            {
                ActivateScript();
                return;
            }
            
            // редактировать выбранный объект
            if (ActiveChartTool == ChartTool.Cursor && editingObject != null)
            {
                if (EditChartObjectMouseDown(e.X, e.Y, ModifierKeys))
                    needRedraw = true;
            }

            // выбрать объект
            if (ActiveChartTool == ChartTool.Cursor && needRedraw == false)
                needRedraw = TryEditObject(e.X, e.Y);

            if (ActiveChartTool == ChartTool.Cursor)
            {
                if (IndicatorsProcessMouseDown(e))
                {
                    needRedraw = true;
                    chart.skipZoomming = true;
                }
            }
            else // добавление точки в объект серии...
                needRedraw |= SeriesProcessMouseButton(e, true);
            
            if (needRedraw) 
                Invalidate();
        }

        private void ActivateScript()
        {
            if (menuitemScripts.DropDownItems.Count == 0) return;

            // определить координаты время - цена для вызова скрипта
            var point = chart.StockPane.PointToClient(Cursor.Position);
            var pointWorld = Conversion.ScreenToWorld(new PointD(point.X, point.Y),
               chart.StockPane.WorldRect, chart.StockPane.CanvasRect);

            var scriptNames = menuitemScripts.DropDownItems.Cast<ToolStripItem>().Select(i => i.Text).ToList();
            if (scriptNames.Count == 0)
            {
                // скриптов для графика нет - открыть окно настройки скриптов
                if (openScriptSettingsDialog != null)
                    openScriptSettingsDialog();
                return;
            }

            // выбрать скрипт из списка
            var scriptObjects = menuitemScripts.DropDownItems.Cast<ToolStripItem>().Select(i => i.Tag).ToList();
            var dlg = new SelectScriptDialog(scriptNames, scriptObjects,
                chart.StockSeries.GetCandleOpenTimeByIndex((int) Math.Abs(pointWorld.X)),
                (float) pointWorld.Y, () =>
                    {
                        if (openScriptSettingsDialog != null)
                            openScriptSettingsDialog();
                    });
            if (dlg.ShowDialog() != DialogResult.OK) return;

            // скорректировать точку привязки?
            pointWorld = new PointD(dlg.SelectedTime.HasValue ? chart.StockSeries.GetDoubleIndexByTime(dlg.SelectedTime.Value) 
                : pointWorld.X, dlg.SelectedPrice ?? pointWorld.Y);
            
            if (scriptMenuItemActivated != null)
                scriptMenuItemActivated(this, pointWorld, dlg.SelectedScript);
        }

        public void ChartMouseMove(object sender, MouseEventArgs e)
        {
            if (chart.StockPane.YAxis.DragModeIsOn)
            {
                // изменить масштаб по оси Y
                if ((ModifierKeys & Keys.Shift) != Keys.Shift)
                {
                    if (chart.StockPane.YAxis.ChangeScaleByDragCursor(e.X, e.Y))
                        RedrawChartSafe();
                }
                else
                {// изменить смещение оси Y
                    if (chart.StockPane.YAxis.ChangeShiftYByDragCursor(e.X, e.Y))
                        RedrawChartSafe();                    
                }
                return;
            }

            var needRedraw = false;
            if (ActiveChartTool == ChartTool.Cursor)
            {
                // получить данные индикаторов в точке курсора
                if (IndicatorsProcessMouseMove(e))
                {                    
                    RedrawChartSafe();
                    return;
                }

                // "подсветить" (выделить жирным) объект
                IChartInteractiveObject selectedObj = null;
                var ptScreen = chart.PointToScreen(new Point(e.X, e.Y));
                foreach (var series in listInteractiveSeries)
                {
                    selectedObj = series.GetObjectsUnderCursor(ptScreen.X, ptScreen.Y, MouseHitTolerancePix).FirstOrDefault();
                    if (selectedObj != null) break;
                }
                if (UpdateSelectedObject(selectedObj))
                    needRedraw = true;
            }
            // режим "резинка" при создании объекта графика?
            else if (ActiveChartTool != ChartTool.Cursor)
            {
                foreach (var series in listInteractiveSeries)
                {
                    var redraw = series.OnMouseMove(e, ModifierKeys, ActiveChartTool);
                    if (redraw) needRedraw = true;
                }
                // перерисовать
                if (needRedraw)
                {
                    chart.Invalidate();
                    return;
                }
            }
            
            // перетащить - растянуть объект
            if (ActiveChartTool == ChartTool.Cursor)
                if (EditChartObjectMouseMove(e.X, e.Y))
                    return;
            
            if (needRedraw)
                chart.Invalidate();
        }

        public void ChartMouseUp(object sender, MouseEventArgs e)
        {
            if (chart.StockPane.YAxis.DragModeIsOn)
            {
                // закончить "перетаскивание" масштаба вертикальной оси
                chart.StockPane.YAxis.DragModeIsOn = false;
                return;
            }

            var needRedraw = false;
            if (ActiveChartTool == ChartTool.Cursor)
            {// показать подсказку
                if (IndicatorsProcessMouseUp(e))
                    needRedraw = true;
                base.OnMouseUp(e);
            }

            if (ActiveChartTool == ChartTool.Cursor)
                EditChartObjectMouseUp();

            // обработка отпускания мыши в сериях графических объектов
            if (ActiveChartTool != ChartTool.Cursor)
                needRedraw |= SeriesProcessMouseButton(e, false);

            // если нужна перерисовка
            if (needRedraw) 
                RedrawChartSafe();
            base.OnMouseUp(e);
        }

        private void ChartKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '+')
                ZoomIn();
            else if (e.KeyChar == '-')
                ZoomOut();            
            //if (e.KeyChar == 'p' || e.KeyChar == 'P' ||
            //    e.KeyChar == 'з' || e.KeyChar == 'З')
            //    SaveAsImageCurrentFolder();
            // масштабирование
            if (e.KeyChar == (char)26 ||
                e.KeyChar == 'z' || e.KeyChar == 'Z' ||
                e.KeyChar == 'я' || e.KeyChar == 'Я')
                if ((ModifierKeys & Keys.Control) == Keys.Control)
                {
                    chart.ZoomBackward();
                    RedrawChartSafe();
                }
            if (e.KeyChar == (char)25 ||
                e.KeyChar == 'y' || e.KeyChar == 'Y' ||
                e.KeyChar == 'н' || e.KeyChar == 'Н')
                if ((ModifierKeys & Keys.Control) == Keys.Control)
                {
                    chart.ZoomForward();
                    RedrawChartSafe();
                }

            if (e.KeyChar == (char)6 ||
                e.KeyChar == 'f' || e.KeyChar == 'F' ||
                e.KeyChar == 'а' || e.KeyChar == 'А')
                if ((ModifierKeys & Keys.Control) == Keys.Control)
                {
                    chart.ScaleFreezed = !chart.ScaleFreezed;
                    RedrawChartSafe();
                }


            base.OnKeyPress(e);
        }
        #endregion

        static CandleChartControl()
        {
            SetupChartIcons();
        }

        public CandleChartControl()
        {
            InitializeComponent();

            if (DesignMode)
                return;

            chart.Owner = this;
            chart.StockPane.Name = Localizer.GetString("TitleCourse");

            // серии (граф. объекты)
            InitializeSeries();
            
            // визуальные инструменты
            zoomTool = new ZoomTool
            {
                Name = "Zoom",
                Enabled = true,
                ForeColor = Color.Blue,
                BackColor = Color.FromArgb(50, Color.Green)
            };
            chart.InteractivityTools.Add(zoomTool);
            var scrollTool = new ScrollTool { Name = "Scroll", Enabled = true };
            chart.InteractivityTools.Add(scrollTool);
            var infoTool = new InfoTool("info") { Enabled = true };
            chart.InteractivityTools.Add(infoTool);
            crossTool = new CrossTool("cross") { Enabled = true };
            chart.InteractivityTools.Add(crossTool);
            
            // привязать обработчики
            chart.MouseUp += ChartMouseUp;
            
            // визуальные
            chart.StockSeries.BarOffset = 5;
            chart.StockSeries.ShowLastQuote = true;
            
            // иконки состояния
            chart.StockPane.AddPaneStrip("Candlechart.images.pane_strip.png", 1, 0);
            chart.OnScaleChanged += Window_OnScaleChanged;
            
            // меню
            SetupMenuImages();
        }

        public static void ResetControlTags(Control control)
        {
            control.Tag = null;
            foreach (Control childControl in control.Controls)
            {
                ResetControlTags(childControl);

                if (childControl.ContextMenuStrip != null)
                {
                    foreach (ToolStripItem menuItem in childControl.ContextMenuStrip.Items)
                        ResetToolStripItemTags(menuItem);
                }
                var menu = childControl as ToolStrip;
                if (menu == null)
                    continue;
                foreach (ToolStripItem menuItem in menu.Items)
                    ResetToolStripItemTags(menuItem);
            }
        }

        private static void ResetToolStripItemTags(ToolStripItem item)
        {
            item.Tag = null;
            var dropDownItem = item as ToolStripDropDownItem;
            if (dropDownItem == null)
                return;
            foreach (ToolStripItem childItem in dropDownItem.DropDownItems)
                ResetToolStripItemTags(childItem);
        }

        private void Window_OnScaleChanged(double left, double right)
        {
            if (!SynchronizationEnabled) return;
            if (DoNotSpreadScale)
            {
                DoNotSpreadScale = false;
                return;
            }
            var dateStart = chart.StockSeries.GetCandleOpenTimeByIndex((int)left);
            var dateEnd = chart.StockSeries.GetCandleOpenTimeByIndex((int)right);
            if (onScaleChanged != null) onScaleChanged(dateStart, dateEnd, this);
            UpdateWindowTitle();
        }

        public void SetScale(DateTime timeStart, DateTime timeEnd)
        {
            if (!SynchronizationEnabled) return;
            
            var indexStart =
                chart.StockSeries.GetDoubleIndexByTime(timeStart);
            var indexEnd =
                chart.StockSeries.GetDoubleIndexByTime(timeEnd);
            if (indexStart < 0) indexStart = 0;
            if (indexEnd > chart.StockSeries.Data.Count)
                indexEnd = chart.StockSeries.Data.Count;
            if (indexEnd < 0) indexEnd = 0;

            if (indexStart < indexEnd)
            {
                chart.SetScrollView((int)indexStart, (int)indexEnd);
                DoNotSpreadScale = true;
                RedrawChartSafe();
            }            
        }

        public void OnCursorCrossChanged(DateTime? timeCross, double? price)
        {
            // перевести время в модельную координату
            if (timeCross == null || price == null)
            {
                chart.StockPane.CursorCrossCoords = null;
                return;
            }
            
            var x = chart.StockSeries.GetDoubleIndexByTime(timeCross.Value);
            chart.StockPane.CursorCrossCoords = new PointD(x, price.Value);
        }

        /// <summary>
        /// построить свечки (вариант - бары или кривая)
        /// построить индикаторы
        /// пересчитать? свечки
        /// </summary>
        public void RebuildChart(bool updateQuotesForIndicators)
        {
            var minuteCandles = AtomCandleStorage.Instance.GetAllMinuteCandles(Symbol);
            if (minuteCandles == null)
                 minuteCandles = new List<CandleData>();
            
            UpdateCandles(minuteCandles);
            chart.FitChart();
            
            // отразить изменения ТФ в сериях
            foreach (var series in listInteractiveSeries)                
                series.OnTimeframeChanged();

            // индикаторы
            BuildIndicators(updateQuotesForIndicators);

            // немножко урезать отображаемый интервал
            ShrinkDisplayedIntervalAuto();

            chart.Invalidate();            
        }

        /// <summary>
        /// если на графике слишком много свечек - автоматически показать последние N свечей
        /// </summary>
        public void ShrinkDisplayedIntervalAuto()
        {
            const int candlesToShrink = 300;
            if (chart.StockSeries.Data.Candles.Count < candlesToShrink) return;
            var start = chart.StockSeries.Data.Candles.Count - candlesToShrink;
            var end = chart.StockSeries.Data.Candles.Count - 1 + chart.StockSeries.BarOffset;
            chart.SetScrollView(start, end);
        }

        /// <summary>
        /// упаковать котировки в свечки
        /// </summary>
        public void UpdateCandles(List<CandleData> minuteCandles)
        {
            chart.StockSeries.Data.Clear();
            // родной интервал равен m1 - не производить никаких действий
            if (Timeframe.IsAtomic)
            {
                foreach (var candle in minuteCandles)
                    chart.StockSeries.Data.Candles.Add(new CandleData(candle));
                return;
            }

            // упаковать m1 в свечи нужного ТФ
            candlePacker = new CandlePacker(chart.Timeframe);
            foreach (var minuteCandle in minuteCandles)
            {
                var candle = candlePacker.UpdateCandle(minuteCandle);
                if (candle != null) chart.StockSeries.Data.Candles.Add(candle);
            }
            // добавить незавершенную свечу
            var shouldAdd = true;
            if (chart.StockSeries.Data.Candles.Count > 0)
                if (chart.StockSeries.Data.Candles[chart.StockSeries.Data.Candles.Count - 1].timeOpen ==
                    candlePacker.CurrentCandle.timeOpen) shouldAdd = false;
            if (shouldAdd && candlePacker.CurrentCandle != null)
                chart.StockSeries.Data.Candles.Add(candlePacker.CurrentCandle);
        }                

        #region Масштабирование
        public void ZoomIn()
        {
            if (chart.Window.LeftPos == chart.Window.RightPos)
                return; //максимальное увеличение - 1 бар на графике

            var increment = (int)Math.Ceiling((chart.Window.RightPos - chart.Window.LeftPos) / ZoomInRate);

            chart.SetScrollView(chart.Window.LeftPos + increment, chart.Window.RightPos - increment);
            chart.Invalidate();
        }

        public void ZoomOut()
        {
            if (chart.Window.LeftPos == chart.Window.MinimumPos && chart.Window.RightPos == chart.Window.MaximumPos)
                return; // чарт уже отображен в минимальном масштабе

            var span = (int)((chart.Window.Right - chart.Window.Left == 0) ? 1 : chart.Window.Right - chart.Window.Left);
            var increment = (int)Math.Ceiling(span * ZoomOutRate);

            chart.SetScrollView(chart.Window.LeftPos - increment, chart.Window.RightPos + increment);
            chart.Invalidate();
        }
        #endregion

        public void UpdateWindowTitle()
        {
            if (onWindowTitleUpdated == null) return;
            var open = chart.StockSeries.GetCandleOpenTimeByIndex((int)chart.StockPane.WorldRect.Left + 1);
            var worldRight = (int)chart.StockPane.WorldRect.Right;
            if (worldRight > chart.StockSeries.DataCount) worldRight = chart.StockSeries.DataCount - 1;
            var close = chart.StockSeries.GetCandleOpenTimeByIndex(worldRight);
            var title = Symbol + " " + timeframeString + " " + open.ToString("dd.MM.yyyy") + 
                " - " + close.ToString("dd.MM.yyyy") + (ExtraTitle ?? "");
            
            onWindowTitleUpdated(title);
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
            base.OnDragEnter(e);
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);
            
            // добавить на график индикатор?
            var passedObject = e.Data.GetData(typeof(CandleChartDroppingObject)) as CandleChartDroppingObject;
            if (passedObject == null) return;

            if (passedObject.TypeOfValue == CandleChartDroppingObject.ValueType.Indicator)
            {
                var indi = (IChartIndicator)Activator.CreateInstance((Type)passedObject.value);
                AddNewIndicator(indi);
                // открыть окно настройки индикатора
                new IndicatorSettingsWindow { Indi = indi }.ShowDialog();
                return;
            }

            // вызвать событие - активация скрипта
            if (passedObject.TypeOfValue == CandleChartDroppingObject.ValueType.Script)
            {
                var point = chart.StockPane.PointToClient(Cursor.Position);
                var pointWorld = Conversion.ScreenToWorld(new PointD(point.X, point.Y),
                   chart.StockPane.WorldRect, chart.StockPane.CanvasRect);
                scriptMenuItemActivated(this, pointWorld, passedObject.value);
            }
        }
    }
}

using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Candlechart.ChartMath;
using Candlechart.Core;
using Entity;
using Candlechart.Series;
using TradeSharp.Util;

namespace Candlechart.Chart
{    
    [Description("Financial charting component"), 
     DefaultProperty("ChartType"),
     ToolboxBitmap(typeof(ChartControl), "ChartControl.ChartControl.bmp")]
    public partial class ChartControl : UserControl
    {
        public CandleChartControl Owner { get; set; }
        private readonly ChartFrame _chartFrame;
        private readonly InteractivityToolCollection _interactivityTools;
        private readonly PaneCollection _panes;
        private ChartType _chartType;
        private bool _firstTime = true;
        private Window _window;
        private IContainer components;
        private CandleRange candleRange;
        private ToolTip tooltip = new ToolTip { ShowAlways = true };
        private Image buffer;
        private FloodSafeLogger logNoFlood = new FloodSafeLogger(1000 * 10);
        private const int LogMsgDrawError = 1;

        public CandleRange CandleRange
        {
            get
            {
                return candleRange;
            }
            set
            {
                candleRange = value;
            }
        }

        public bool interactiveToolsEnabled = true;

        /// <summary>
        /// флаг взводится в обработчике выше в иерархии. когда взведен - 
        /// инструменты не обрабатывают нажатие, после чего флаг сбрасывается
        /// </summary>
        public bool toolSkipMouseDown;
        /// <summary>
        /// см toolSkipMouseDown
        /// </summary>
        public bool toolSkipMouseUp;
        /// <summary>
        /// см toolSkipMouseDown
        /// </summary>
        public bool toolSkipMouseMove;
        /// <summary>
        /// стек изменений масштаба
        /// </summary>
        private readonly Stack<Cortege2<int, int>?> StackZoom = new Stack<Cortege2<int, int>?>();
        private bool scaleFreezed = false;
        /// <summary>
        /// если флаг взведен - зум чарта (ZoomTool) не применяется
        /// </summary>
        public bool skipZoomming;

        /// <summary>
        /// масштаб заморожен - масштабирование запрещено, разрешается "перелистывание" графика
        /// автоматически снимается в некоторых случаях
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ScaleFreezed
        {
            get { return scaleFreezed; }
            set
            {
                scaleFreezed = value;
                StockPane.paneIconStrip.SetState(scaleFreezed ? 1 : 0);
            }
        }

        public delegate void ScaleChangedDel(double left, double right);
        private ScaleChangedDel onScaleChanged;
        public event ScaleChangedDel OnScaleChanged
        {
            add { onScaleChanged += value; }
            remove { onScaleChanged -= value; }
        }

        public const int LeftMargin = 0;

        public int RightMargin
        {
            get { return StockSeries.BarOffset + StockSeries.DataCount; }
        }

        public ChartControl()
        {
            InitializeComponent();
            visualSettings = new ChartVisualSettings(this);
            SetStyle((ControlStyles)0x22012, true);            
            _chartFrame = new ChartFrame();
            _panes = new PaneCollection(this);
            _interactivityTools = new InteractivityToolCollection(this);
            CreateStockSeries();            
            _window = new Window(this);
            candleRange = new CandleRange(StockSeries);             
            Timeframe = new BarSettings();
            Timeframe.Intervals.Add(60);
            //DoubleBuffered = true;
        }

        internal ChartFrame ChartFrame
        {
            get { return _chartFrame; }
        }

        [Description("Type of financial chart."), Category("Appearance"), DefaultValue(0)]
        public ChartType ChartType
        {
            get { return _chartType; }
            set
            {
                if (_chartType != value)
                {
                    _chartType = value;
                    CreateStockSeries();
                    Invalidate();
                }
            }
        }

        internal Rectangle ClientRect
        {
            get
            {
                int leftFrameWidth = ChartFrame.LeftFrameWidth;
                int rightFrameWidth = ChartFrame.RightFrameWidth;
                int topFrameWidth = ChartFrame.TopFrameWidth;
                int bottomFrameWidth = ChartFrame.BottomFrameWidth;
                int height = Height;
                int width = Width;
                if (width < MinimumSize.Width)
                {
                    width = MinimumSize.Width;
                }
                if (height < MinimumSize.Height)
                {
                    height = MinimumSize.Height;
                }
                return new Rectangle(leftFrameWidth, topFrameWidth, (width - leftFrameWidth) - rightFrameWidth,
                                     (height - topFrameWidth) - bottomFrameWidth);
            }
        }

        [Browsable(false)]
        public InteractivityToolCollection InteractivityTools
        {
            get { return _interactivityTools; }
        }

        internal int InterPaneGap
        {
            get
            {
                if (visualSettings.ChartFrameStyle != Candlechart.Theme.FrameStyle.Flat) return 0;                
                return -1;
            }
        }

        [Category("Appearance"), Description("The number of bars to pad the left side of the chart."), DefaultValue(0)]
        public int LeftBars
        {
            get { return Window.LeftBars; }
            set
            {
                Window.LeftBars = value;
                if (DesignMode)
                {
                    FitChart();
                    Invalidate();
                }
            }
        }

        [Browsable(false)]
        public int LeftPos
        {
            get { return Window.LeftPos; }
        }

        [Browsable(false)]
        public int MaximumPos
        {
            get { return Window.MaximumPos; }
        }

        [Browsable(false)]
        public int MinimumPos
        {
            get { return Window.MinimumPos; }
        }

        [Browsable(false)]
        public PaneCollection Panes
        {
            get { return _panes; }
        }

        [DefaultValue(0), Category("Appearance"), Description("The number of bars to pad the right side of the chart.")]
        public int RightBars
        {
            get { return StockSeries.BarOffset; }
            set
            {
                StockSeries.BarOffset = value;
                XAxis.ShiftFromRight = value;
                Window.RightBars = value;
                if (DesignMode)
                {
                    FitChart();
                    Invalidate();
                }
            }
        }

        [Browsable(false)]
        public int RightPos
        {
            get { return Window.RightPos; }
        }

        [Browsable(false)]
        public Pane StockPane
        {
            get { return Panes.StockPane; }
        }

        [Browsable(false)]
        public StockSeries StockSeries
        {
            get { return StockPane.StockSeries; }
        }

        internal Window Window
        {
            get { return _window; }
        }

        [Browsable(false)]
        public XAxis XAxis
        {
            get { return Panes.XAxisPane.XAxis; }
        }

        private YAxisAlignment yAlignment = YAxisAlignment.Right;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
         Description("Determines the alignment location of the Y axis."), 
         Browsable(false), Category("Appearance")]
        public YAxisAlignment YAxisAlignment
        {
            get { return yAlignment; }
            set { yAlignment = value; }
        }

        internal int YAxisWidth { get; set; }

        [Browsable(true)]
        [Description("Status bar to display item's data")]
        public TextBox ItemTextBox { get; set; }

        // Events
        public event ViewChangedEventHandler ViewChanged;

        // Methods
        private void ConstructSample()
        {
            SuspendLayout();
            StockPane.PercentHeight = 75f;
            Pane pane = Panes.Add("VOL");
            pane.PercentHeight = 25f;
            pane.YAxis.MinAuto = false;
            pane.YAxis.Min = 0.0;
            pane.PaneFrame.TitleBoxVisible = false;
            ResumeLayout();
            var series = new VolumeSeries("Volume");
            var random = new Random();
            int num = random.Next(100, 150);
            DateTime date = DateTime.Today.AddDays(1.0);
            for (int i = 0; i < 100; i++)
            {
                int maxValue = num + random.Next(5, 0x15);
                int minValue = num - random.Next(5, 0x15);
                int num4 = random.Next(minValue, maxValue);
                int num5 = random.Next(0x186a0, 0x989680);
                if (date.DayOfWeek == DayOfWeek.Friday)
                {
                    date = date.AddDays(3.0);
                }
                else if (date.DayOfWeek == DayOfWeek.Saturday)
                {
                    date = date.AddDays(2.0);
                }
                else
                {
                    date = date.AddDays(1.0);
                }
                StockSeries.Data.Add(num, maxValue, minValue, num4, date);
                series.Data.Add(num5);
                num += random.Next(-15, 0x15);
            }
            pane.Series.Add(series);
            var series2 = new LineSeries("MA 10");
            series2.Data.SetDataArray(Formula.ExponentialMovingAverage(StockSeries.Data, 10));
            StockPane.Series.Add(series2);
            FitChart();
        }

        private void CreateStockSeries()
        {
            StockSeries stockSeries = StockPane.StockSeries;
            switch (ChartType)
            {
                case ChartType.Candlestick:
                    StockPane.StockSeries = new CandlestickSeries("StockSymbol");
                    break;

                case ChartType.OhlcBar:
                    StockPane.StockSeries = new OhlcBarSeries("StockSymbol");
                    break;

                case ChartType.StockLine:
                    StockPane.StockSeries = new StockLineSeries("StockSymbol");
                    break;
            }
            StockPane.StockSeries.CopyFrom(stockSeries);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        internal void Draw(Graphics g)
        {
            Draw(g, new Rectangle(0, 0, Width, Height));
        }

        private void Draw(Graphics g, Rectangle clipRectangle)
        {
            buffer = null;

            g.Clear(BackColor);
            //GraphicsContainer container = g.BeginContainer();
            //g.SetClip(ClientRect);
            //g.TranslateTransform(ClientRect.Left, ClientRect.Top);
            //Rectangle rectangle = Conversion.ParentToChild(clipRectangle, ClientRect);
            Panes.Draw(g, clipRectangle);
            //g.EndContainer(container);            
        }

        private void DrawInBuffer()
        {
            if (Width <= 0 || Height <= 0) return;

            try
            {            
                if (buffer == null)
                    buffer = new Bitmap(Width, Height);
                else
                {
                    if (buffer.Width != Width || buffer.Height != Height)
                    {
                        buffer.Dispose();
                        buffer = new Bitmap(Width, Height);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("DrawInBuffer - ошибка выделения памяти", ex);
                throw new Exception("Ошибка выделения памяти (рисование)");
            }
            
            try
            {
                using (var g = Graphics.FromImage(buffer))
                {
                    g.Clear(BackColor);
                    //var container = g.BeginContainer();
                    //g.SetClip(ClientRect);
                    //g.TranslateTransform(ClientRect.Left, ClientRect.Top);
                    //Rectangle rectangle = Conversion.ParentToChild(new Rectangle(0, 0, Width - 1, Height - 1), ClientRect);
                    var rectangle = new Rectangle(0, 0, Width, Height);
                    try
                    {
                        Panes.Draw(g, rectangle);
                    }
                    catch (Exception ex)
                    {
                        logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                                                              LogMsgDrawError, 1000*60*5,
                                                              "ChartControl.DrawInBuffer error: {0}", ex);
                    }
                    //g.EndContainer(container);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("ChartControl.DrawInBuffer error: width={0}, height={1}, {2}", Width, Height, ex);
            }
        }

        public void RepaintAndInvalidate()
        {
            DrawInBuffer();
            Refresh();
        }

        public void FitChart()
        {
            double num;
            double num2;
            if (FitXAxis(out num, out num2))
            {
                Window.UpdateScrollLimits(num, num2);
                Window.SetScrollView(MinimumPos, MaximumPos);
            }
            else
            {
                Window.Reset();
            }
            ResetWorldRect();
            SetRange();
        }

        public void ShiftChart(int indexShift)
        {
            double left = 0, right = 0;
            StockSeries.GetXExtent(ref left, ref right);
            Window.UpdateScrollLimits(left, right);            
            Window.SetScrollView(MinimumPos, MaximumPos);            
            SetRange();
            ResetWorldRect();
        }

        private bool FitXAxis(out double left, out double right)
        {
            bool flag = false;             
            left = double.MaxValue;
            right = double.MinValue;
            foreach (Pane pane in Panes.PositionList)
            {
                bool flag2 = pane.FindXExtent(ref left, ref right);
                flag |= flag2;
            }
            return flag;
        }

        private void InitializeComponent()
        {
            components = new Container();
            MouseLeave += OnMouseLeave;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            Panes.Layout();
        }

        public string GetTipText(int x, int y)
        {
            var clientPoint = PointToScreen(new Point(x, y));
            clientPoint = StockPane.PointToClient(clientPoint);

            var pointD = Conversion.ScreenToWorld(new PointD(clientPoint.X, clientPoint.Y),
               StockPane.WorldRect, StockPane.CanvasRect);

            var index = (int)(pointD.X + 0.5);
            if (index < 0 || index >= (StockSeries.Data.Count + RightBars)) return string.Empty;

            var tipText = new StringBuilder();
            if (index < StockSeries.Data.Count)
                tipText.AppendLine(string.Format(
                    "Свеча[{0}]:\n{1:dd/MM/yyyy HH:mm}\nopen {2}\nhigh {3}\nlow {4}\nclose {5}",
                    index, StockSeries.Data[index].timeOpen, 
                    StockSeries.Data[index].open.ToStringUniformPriceFormat(true),
                    StockSeries.Data[index].high.ToStringUniformPriceFormat(true),
                    StockSeries.Data[index].low.ToStringUniformPriceFormat(true),
                    StockSeries.Data[index].close.ToStringUniformPriceFormat(true)));
            foreach (var indicator in Owner.indicators)
            {
                try
                {
                    tipText.AppendLine(indicator.GetHint(x, y, index, pointD.Y, CandleChartControl.MouseHitTolerancePix));
                }
                catch
                {                    
                }                
            }
            return tipText.ToString();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            
            if (!toolSkipMouseDown && interactiveToolsEnabled)
                InteractivityTools.HandleMouseDown(e);
            toolSkipMouseDown = false;
            
            // проверка клика на иконках статуса
            var ptScreen = new Point(e.X, e.Y);
            
            if (StockPane.paneIconStrip.CheckMouseHit(new MouseEventArgs(e.Button, 
                e.Clicks, ptScreen.X, ptScreen.Y, e.Delta)))
            {
                ScaleFreezed = StockPane.paneIconStrip.GetStateMask() == 1;
            }

            // кнопки на графике
            if (e.Button == MouseButtons.Left)
                PaneButtonsOnLeftMouseDown(e.X, e.Y);

            // клик на горз. оси
            XAxis.StartDragRightBar(e);
        }
        
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (!toolSkipMouseUp && interactiveToolsEnabled)
                InteractivityTools.HandleMouseUp(e);
            toolSkipMouseUp = false;

            // кнопки на графике
            PaneButtonsOnLeftMouseUp();

            // закончить перемещение маркера по оси Х
            XAxis.EndDragLastBar();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!toolSkipMouseMove && interactiveToolsEnabled)
                InteractivityTools.HandleMouseMove(e);
            toolSkipMouseMove = false;

            // кнопки на графике
            PaneButtonsOnMouseMove(e.X, e.Y);

            // возможно, переместить метку по горз. оси
            XAxis.DoDragRightBar(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            InteractivityTools.HandleMouseWheel(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_firstTime)
            {
                _firstTime = false;
                if (DesignMode)
                {
                    ConstructSample();
                }
            }
            base.OnPaint(e);

            var windowRect = new Rectangle(0, 0, Width, Height);
            if (buffer != null)
            {
                // если размер буфера не соответствует области обновления,
                // то пересоздаем его (возможно, это результат изменения размеров окон до их отрисовки)
                if (buffer.Width != Width || buffer.Height != Height) 
                    DrawInBuffer();
                e.Graphics.DrawImage(buffer, windowRect); // ClipRectangle
                buffer = null;
                return;
            }

            Draw(e.Graphics, windowRect); //e.ClipRectangle);
            //if (tooltip.Tag == null) return;
            //var oldPoint = (Point)tooltip.Tag;
            //DrawCursorCross(oldPoint);
        }

        protected internal void OnViewChanged(EventArgs e)
        {
            if (ViewChanged != null)
            {
                ViewChanged(this, e);
            }
        }

        private void ResetWorldRect()
        {
            foreach (Pane pane in Panes.PositionList)
            {
                pane.ResetWorldRect(true);
            }
        }

        public void ScrollBy(int amount)
        {
            int oldLeft = Window.LeftPos, oldRight = Window.RightPos;
            Window.ScrollBy(amount);
            if (onScaleChanged != null && (oldLeft != Window.LeftPos || oldRight != Window.RightPos))
                onScaleChanged(Window.LeftPos, Window.RightPos);
        }

        public void ScrollTo(int leftPos)
        {
            int oldLeft = Window.LeftPos, oldRight = Window.RightPos;
            Window.ScrollTo(leftPos);
            if (onScaleChanged != null && (oldLeft != Window.LeftPos || oldRight != Window.RightPos))
                onScaleChanged(Window.LeftPos, Window.RightPos);
        }

        public void ScrollToEnd()
        {
            Window.ScrollToEnd();
        }

        public void SetScrollView(int leftPos, int rightPos)
        {
            if (ScaleFreezed)
            {// прокрутка или изменение масштаба?
                var DX = rightPos - leftPos;
                var oldDX = Math.Round(Window.Right - Window.Left);
                var isScaling = DX != oldDX;
                if (isScaling) return;
            }

            SetScrollView(leftPos, rightPos, true);
        }

        public void SetScrollView(int leftPos, int rightPos, bool saveZoom)
        {
            if (Window.LeftPos == leftPos && Window.RightPos == rightPos) return;
            // запомнить масштаб в стеке
            if (saveZoom)
                StackZoom.Push(new Cortege2<int, int>(leftPos, rightPos));
            Window.SetScrollView(leftPos, rightPos);
            if (onScaleChanged != null)
                onScaleChanged(Window.LeftPos, Window.RightPos);
        }

        /// <summary>
        /// "вспомнить" масштаб из стека
        /// </summary>
        public void ZoomBackward()
        {
            if (ScaleFreezed) return;
            var zoom = StackZoom.StepBack();
            if (!zoom.HasValue) return;
            SetScrollView(zoom.Value.a, zoom.Value.b, false);
        }

        /// <summary>
        /// "вернуть" масштаб из стека
        /// </summary>
        public void ZoomForward()
        {
            if (ScaleFreezed) return;
            var zoom = StackZoom.StepForward();
            if (!zoom.HasValue) return;
            SetScrollView(zoom.Value.a, zoom.Value.b, false);
        }

        public void GetScrollView(ref int leftPos, ref int rightPos)
        {
            leftPos = Window.LeftPos;
            rightPos = Window.RightPos;
        }

        public void UpdateScrollLimits()
        {
            double num;
            double num2;
            if (FitXAxis(out num, out num2))
            {
                Window.UpdateScrollLimits(num, num2);
            }
        }        

        public void RedrawStockPane()
        {
            if (StockPane == null) return;
            StockPane.Draw(CreateGraphics());
        }

        private bool linkClosedDeals = true;

        /// <summary>
        /// Соединять пунктиром открытие/закрытие сделки
        /// </summary>
        [Browsable(true), Category("Appearance"), DefaultValue(true)]
        public bool LinkClosedDeals
        {
            get { return linkClosedDeals; } 
            set { linkClosedDeals = value; }
        }

        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //private ChartVisualSettings visualSettings;

        /// <summary>
        /// Настройки рисования
        /// </summary>
        //public ChartVisualSettings VisualSettings
        //{
        //    get { return visualSettings; }
        //    set
        //    {
        //        visualSettings = value;
        //        visualSettings.ApplyToChart(this);                
        //    }
        //}        

        public bool SetRange()
        {
            DateTime start = DateTime.Now, end = DateTime.Now;
            if (!StockSeries.GetTimeExtent(ref start, ref end))
                return false;
            double left = 0.0, right = 0.0;
            if (!StockSeries.GetIndexRange(ref left, ref right))
                return false;
            CandleRange.StartIndex = left;
            CandleRange.EndIndex = right;
            CandleRange.Start = start;
            CandleRange.End = end;
            if (onScaleChanged != null) onScaleChanged(left, right);
            return true;
        }
    }

    public enum ChartType
    {
        Candlestick,
        OhlcBar,
        StockLine
    }

    public delegate void ViewChangedEventHandler(object sender, EventArgs e);
}
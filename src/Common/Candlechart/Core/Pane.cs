using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using Candlechart.Chart;
using Candlechart.ChartMath;
using Candlechart.Series;
using Candlechart.Theme;
using Entity;
using TradeSharp.Util;
using SeriesCollection=Candlechart.Series.SeriesCollection;

namespace Candlechart.Core
{
    public partial class Pane
    {
        #region Fields
        private readonly PaneFrame paneFrame;
        private readonly SeriesCollection series;
        private readonly YAxis yAxis;
        private Rectangle bounds = new Rectangle(0, 0, 0, 0);
        private Rectangle canvasRect;
        private int minimumHeight;
        private string name;
        private float percentHeight;
        private StockSeries stockSeries;
        public string BackgroundImageResourceName { get; set; }
        public Bitmap bitmapBackgr;
        private int BackgroundShiftX
        {
            get
            {
                return Chart.YAxisAlignment == YAxisAlignment.Right ? 8 : 63;
            }
        }
        private const int BackgroundShiftY = 30;
        public string BackgroundTitleText { get; set; }
        public int BackgroundTitleShiftX { get; set; }
        public int BackgroundTitleShiftY { get; set; }
        public string Title
        {
            get { return paneFrame.Title; }
            set { paneFrame.Title = value; }
        }        
        public bool ShowTitle
        {
            get { return paneFrame.ShowTitle; }
            set { paneFrame.ShowTitle = value; }
        }
        public PaneIconStrip paneIconStrip;

        public bool TitleBoxVisible
        {
            get { return paneFrame.TitleBoxVisible; }
            set { paneFrame.TitleBoxVisible = value; }
        }

        /// <summary>
        /// минимальный размер (ширина либо высота панели), при котором логотип все еще рисуется
        /// </summary>
        private const int MinSizeToDrawLogo = 640;

        private double customScale = 1D;
        /// <summary>
        /// масштабный коэффициент, позволяющий пользователю сжать или растянуть график по оси Y
        /// </summary>
        public double CustomScale
        {
            get { return customScale; }
            set { customScale = value; }
        }

        /// <summary>
        /// смещение оси Y вверх или вниз
        /// </summary>
        public double ShiftYrelative { get; set; }

        /// <summary>
        /// иконки - "кнопки", рисуемые поверх всего и реагирующие на нажатия
        /// </summary>
        public ChartIcon.ChartIcon[] customButtons;
        #endregion 

        public Pane(string name, ChartControl owner)
        {
            Name = name;
            Owner = owner;
            paneFrame = new PaneFrame(this);
            yAxis = new YAxis(this);
            series = new SeriesCollection(this);
            ResetWorldRect(false);
        }

        public Pane(string name, ChartControl owner, string backgroundImageName)
        {
            Name = name;
            Owner = owner;
            paneFrame = new PaneFrame(this);
            yAxis = new YAxis(this);
            series = new SeriesCollection(this);
            ResetWorldRect(false);
            BackgroundImageResourceName = backgroundImageName;
            if (!string.IsNullOrEmpty(BackgroundImageResourceName))
                LoadBackImage(backgroundImageName);
        }

        /// <summary>
        /// установить коэфф. масштаба, false, если значение 
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        public bool SetScale(double scale)
        {
            if (scale < 0.25 || scale > 4 || scale == customScale) return false;
            customScale = scale;
            return true;
        }

        public bool SetShiftYrelative(double shift)
        {
            if (shift < -0.8 || shift > 0.8 || shift == ShiftYrelative) return false;
            ShiftYrelative = shift;
            return true;
        }

        public void AddPaneStrip(string resxName, int totalImages, int maskSelected)
        {
            paneIconStrip = new PaneIconStrip(resxName, totalImages, 58, 15, maskSelected);
        }

        public void LoadBackImage(string backgroundImageName)
        {
            if (string.IsNullOrEmpty(backgroundImageName)) return;

            // localization
            if (LocalizedResourceManager.Instance != null)
            {
                try
                {
                    var resourceName = backgroundImageName.Replace("Candlechart.images.", "").Replace(".png", "");
                    bitmapBackgr = (Bitmap) LocalizedResourceManager.Instance.GetObject(resourceName);
                }
                catch(Exception ex)
                {
                    Logger.Error("Ошибка в LoadBackImage", ex);
                }
                return;
            }

            var asm = Assembly.GetExecutingAssembly();
            var imageStream = asm.GetManifestResourceStream(
                backgroundImageName);
            if (imageStream != null)            
                bitmapBackgr = new Bitmap(imageStream);
        }

        internal Rectangle Bounds
        {
            get { return bounds; }
            set { bounds = value; }
        }

        internal virtual Rectangle CanvasRect
        {
            get { return canvasRect; }
            set
            {
                canvasRect = value;
                if (canvasRect.Width < 0)
                {
                    canvasRect.Width = 0;
                }
                if (canvasRect.Height < 0)
                {
                    canvasRect.Height = 0;
                }
            }
        }

        protected ChartControl Chart
        {
            get { return Owner; }
        }

        internal Rectangle ClientRect
        {
            get
            {
                var leftFrameWidth = PaneFrame.LeftFrameWidth;
                var rightFrameWidth = PaneFrame.RightFrameWidth;
                var topFrameWidth = PaneFrame.TopFrameWidth;
                var bottomFrameWidth = PaneFrame.BottomFrameWidth;
                var width = (Width - leftFrameWidth) - rightFrameWidth;
                var height = (Height - topFrameWidth) - bottomFrameWidth;
                if (width < 0)
                {
                    width = 0;
                }
                if (height < 0)
                {
                    height = 0;
                }
                return new Rectangle(leftFrameWidth, topFrameWidth, width, height);
            }
        }

        private bool HasNoObjects
        {
            get { return ((StockSeries == null) && (Series.Count == 0)); }
        }

        public int Height
        {
            get { return bounds.Height; }
            set { bounds.Size = new Size(bounds.Width, value); }
        }

        internal int Left
        {
            get { return bounds.Left; }
            set { bounds.Location = new Point(value, bounds.Top); }
        }

        internal Point Location
        {
            get { return bounds.Location; }
            set { bounds.Location = value; }
        }

        public int MinimumHeight
        {
            get { return minimumHeight; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("MinimumHeight", "Minimum height cannot be negative.");
                }
                minimumHeight = value;
            }
        }

        internal int MinimumPaneHeight
        {
            get
            {
                if (Chart.visualSettings.ChartFrameStyle == FrameStyle.Flat)
                {
                    return (((MinimumHeight + PaneFrame.TopFrameWidth) + PaneFrame.BottomFrameWidth) - 1);
                }
                if (Chart.visualSettings.ChartFrameStyle == FrameStyle.ThreeD)
                {
                    return ((MinimumHeight + PaneFrame.TopFrameWidth) + PaneFrame.BottomFrameWidth);
                }
                return MinimumHeight;
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Name cannot be a null or empty string.", "Name");
                }
                name = value;
            }
        }

        internal ChartControl Owner { get; set; }

        public PaneFrame PaneFrame
        {
            get { return paneFrame; }
        }

        public float PercentHeight
        {
            get { return percentHeight; }
            set
            {
                if (value < 0f)
                {
                    throw new ArgumentOutOfRangeException("PercentageHeight", "Percentage cannot be negative.");
                }
                percentHeight = value;
            }
        }

        public SeriesCollection Series
        {
            get { return series; }
        }

        public Size Size
        {
            get { return bounds.Size; }
            set { bounds.Size = value; }
        }

        internal StockSeries StockSeries
        {
            get { return stockSeries; }
            set
            {
                stockSeries = value;
                stockSeries.Owner = this;
            }
        }

        public int Top
        {
            get { return bounds.Top; }
            set { bounds.Location = new Point(bounds.Left, value); }
        }

        internal int Width
        {
            get { return bounds.Width; }
            set { bounds.Size = new Size(value, bounds.Height); }
        }

        internal virtual RectangleD WorldRect { get; set; }

        public YAxis YAxis
        {
            get { return yAxis; }
        }

        private void AdjustLayout()
        {
            CanvasRect = new Rectangle(0, 0, ClientRect.Width, ClientRect.Height);
            YAxis.TrimCanvas();
        }

        internal Point ChartToClient(Point pt)
        {
            pt = Conversion.ParentToChild(pt, Chart.ClientRect);
            pt = Conversion.ParentToChild(pt, Bounds);
            pt = Conversion.ParentToChild(pt, ClientRect);
            return pt;
        }

        internal Rectangle ChartToClient(Rectangle rect)
        {
            rect = Conversion.ParentToChild(rect, Chart.ClientRect);
            rect = Conversion.ParentToChild(rect, Bounds);
            rect = Conversion.ParentToChild(rect, ClientRect);
            return rect;
        }

        internal Point ClientToChart(Point pt)
        {
            pt = Conversion.ChildToParent(pt, ClientRect);
            pt = Conversion.ChildToParent(pt, Bounds);
            pt = Conversion.ChildToParent(pt, Chart.ClientRect);
            return pt;
        }

        internal Rectangle ClientToChart(Rectangle rect)
        {
            rect = Conversion.ChildToParent(rect, ClientRect);
            rect = Conversion.ChildToParent(rect, Bounds);
            rect = Conversion.ChildToParent(rect, Chart.ClientRect);
            return rect;
        }

        private void DoInitialLayout(Graphics g)
        {
            CanvasRect = new Rectangle(0, 0, ClientRect.Width, ClientRect.Height);
            YAxis.PrepareToDraw(g);
            YAxis.TrimCanvas();
        }

        internal virtual void Draw(Graphics g)
        {
            try
            {
                using (var brushes = new BrushesStorage())
                using (var pens = new PenStorage())
                {
                    AdjustLayout();
                    DrawBackground(g, brushes);
                    DrawFrame(g);
                    GraphicsContainer container = g.BeginContainer();
                    g.TranslateTransform(ClientRect.X, ClientRect.Y);
                    DrawYAxis(g);
                    DrawStockSeries(g);
                    DrawSeries(g);
                    if (paneIconStrip != null) paneIconStrip.Draw(g);
                    DrawCursorCross(g, brushes);
                    g.EndContainer(container);
                    // нарисовать иконки - кнопочки
                    DrawButtons(g, brushes, pens);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Pane.Draw()", ex);
                throw;
            }
        }

        private void DrawButtons(Graphics g, BrushesStorage brushes, PenStorage pens)
        {
            if (customButtons == null) return;
            foreach (var btn in customButtons)
            {
                btn.Draw(g, brushes, pens);
            }
        }
        
        public void DrawButtons(Graphics g)
        {
            if (customButtons == null) return;
            using (var brushes = new BrushesStorage())
            using (var pens = new PenStorage())
                foreach (var btn in customButtons)
                {
                    btn.Draw(g, brushes, pens);
                }
        }
        
        private void DrawBackground(Graphics g, BrushesStorage brushes)
        {
            if (Chart == null || Chart.StockPane == null || Owner == null || Chart.Owner == null || brushes == null ||
                Chart.visualSettings == null)
                return;
            using (var brush = brushes.GetBrush(Chart.visualSettings.PaneBackColor))
            {
                g.FillRectangle(brush, 0, 0, Width, Height);
            }

            // нарисовать логотипчик
            if (bitmapBackgr != null && Math.Min(Width, Height) > MinSizeToDrawLogo)
            {                
                g.DrawImage(bitmapBackgr, BackgroundShiftX, BackgroundShiftY, bitmapBackgr.Width, bitmapBackgr.Height);                
            }
            
            // подписать окно (только для StockPane)
            if (this == Chart.StockPane)
            {
                var text = Chart.Symbol + " " + Chart.Owner.timeframeString;
                using (var brush = brushes.GetBrush(Chart.visualSettings.SeriesForeColor))
                {
                    g.DrawString(text, Chart.Font, brush, 3, 3);
                }
            }
            
            if (string.IsNullOrEmpty(BackgroundTitleText)) return;
            using (var f = new Font(FontFamily.GenericSansSerif, 16, FontStyle.Bold))                    
            {
                using (var brText = brushes.GetBrush(Chart.visualSettings.PaneFrameTextColor))
                {
                    g.DrawString(BackgroundTitleText, f, brText, BackgroundTitleShiftX, BackgroundTitleShiftY);
                }
            }
        }

        private void DrawFrame(Graphics g)
        {
            PaneFrame.Draw(g);
        }

        private void DrawSeries(Graphics g)
        {
            GraphicsContainer container = g.BeginContainer();
            g.SetClip(CanvasRect);
            g.TranslateTransform(CanvasRect.Left, CanvasRect.Top);
            foreach (Series.Series s in Series)
            {
                s.Draw(g, WorldRect, new Rectangle(0, 0, CanvasRect.Width, CanvasRect.Height));
            }
            g.EndContainer(container);
        }

        private void DrawStockSeries(Graphics g)
        {
            if (StockSeries != null)
            {
                GraphicsContainer container = g.BeginContainer();
                g.SetClip(CanvasRect);
                g.TranslateTransform(CanvasRect.Left, CanvasRect.Top);
                StockSeries.Draw(g, WorldRect, new Rectangle(0, 0, CanvasRect.Width, CanvasRect.Height));
                g.EndContainer(container);
            }
        }

        private void DrawYAxis(Graphics g)
        {
            GraphicsContainer container = g.BeginContainer();
            g.SetClip(new Rectangle(0, 0, ClientRect.Width, ClientRect.Height));
            YAxis.Draw(g);
            g.EndContainer(container);
        }

        internal bool FindXExtent(ref double left, ref double right)
        {
            bool flag = false;
            if (StockSeries != null)
                flag |= StockSeries.GetXExtent(ref left, ref right);            
            foreach (Series.Series s in Series)
                flag |= s.GetXExtent(ref left, ref right);
            return flag;
        }

        internal bool FindYExtent(double left, double right, ref double top, ref double bottom)
        {
            var flag = false;
            if (StockSeries != null)
                flag |= StockSeries.GetYExtent(left, right, ref top, ref bottom);            
            foreach (Series.Series s in Series)
                flag |= s.GetYExtent(left, right, ref top, ref bottom);
            return flag;
        }

        internal void FitYAxis()
        {
            var minValue = double.MinValue;
            var maxValue = double.MaxValue;
            const double pricePaddingKoeff = 0.05;

            var flag = FindYExtent(Chart.Window.Left, Chart.Window.Right, ref minValue, ref maxValue);
            if (!YAxis.MaxAuto)
            {
                minValue = YAxis.Max;
            }
            if (!YAxis.MinAuto)
            {
                maxValue = YAxis.Min;
            }
            if (flag)
            {
                if ((minValue - maxValue) < double.Epsilon)
                {
                    minValue += Window.InitialYExtent;
                    maxValue -= Window.InitialYExtent;
                }
                else
                {
                    var deltaPrice = minValue - maxValue;
                    if (YAxis.MaxAuto)
                        minValue += deltaPrice * pricePaddingKoeff;                    
                    if (YAxis.MinAuto)                    
                        maxValue -= deltaPrice * pricePaddingKoeff;                    
                }
                var deltaScale = (customScale - 1)*(minValue - maxValue);
                var height = (minValue - maxValue) + deltaScale;
                var min = maxValue - deltaScale * 0.5 + ShiftYrelative * height;

                WorldRect = new RectangleD(Chart.Window.Left, min, Chart.Window.Width, height);
            }
            else if (HasNoObjects)
            {
                ResetWorldRect(true);
            }
            else
            {
                var deltaScale = (customScale - 1) * WorldRect.Height;
                var height = WorldRect.Height + deltaScale;
                var top = WorldRect.Top - deltaScale * 0.5 + ShiftYrelative * height;

                WorldRect = new RectangleD(Chart.Window.Left, top, Chart.Window.Width, height);
            }
        }

        /// <summary>
        /// перевести точку из экранных координат в коорд. графика - график
        /// с ушами, но без шапки
        /// </summary>        
        internal Point PointToClient(Point pt)
        {
            pt = Chart.PointToClient(pt);
            pt = ChartToClient(pt);
            return pt;
        }
        
        internal PointD ScreenToWorld(Point pt)
        {
            var ptClient = PointToClient(pt);
            return Conversion.ScreenToWorld(ptClient, WorldRect, CanvasRect);
        }

        /// <summary>
        /// перевести точку из отображаемых экранных координат (внутри канваса, без шкалы)
        /// в координаты панели (без шапки - координаты клиента)
        /// </summary>
        internal PointD CanvasPointToClientPoint(PointD pt)
        {
            return new PointD(pt.X + CanvasRect.X, pt.Y + CanvasRect.Y);
        }

        internal Point PointToScreen(Point pt)
        {
            pt = ClientToChart(pt);
            pt = Chart.PointToScreen(pt);
            return pt;
        }

        internal void PrepareToDraw(Graphics g)
        {
            FitYAxis();
            DoInitialLayout(g);
        }

        internal Rectangle RectangleToClient(Rectangle rect)
        {
            rect = Chart.RectangleToClient(rect);
            rect = ChartToClient(rect);
            return rect;
        }

        internal Rectangle RectangleToScreen(Rectangle rect)
        {
            rect = ClientToChart(rect);
            rect = Chart.RectangleToScreen(rect);
            return rect;
        }

        internal void ResetWorldRect(bool useWindow)
        {
            double initialYExtent = Window.InitialYExtent;
            double y = -Window.InitialYExtent;
            if (!YAxis.MaxAuto && !YAxis.MinAuto)
            {
                initialYExtent = YAxis.Max;
                y = YAxis.Min;
            }
            else if (!YAxis.MaxAuto)
            {
                initialYExtent = YAxis.Max;
                y = YAxis.Max - (Window.InitialYExtent * 2);
            }
            else if (!YAxis.MinAuto)
            {
                initialYExtent = YAxis.Min + (Window.InitialYExtent * 2);
                y = YAxis.Min;
            }
            if ((initialYExtent - y) < double.Epsilon)
            {
                initialYExtent += Window.InitialYExtent;
                y -= Window.InitialYExtent;
            }
            WorldRect = useWindow ?
                new RectangleD(Chart.Window.Left, y, Chart.Window.Width, initialYExtent - y) :
                new RectangleD(0.0, y, 0.0, initialYExtent - y);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

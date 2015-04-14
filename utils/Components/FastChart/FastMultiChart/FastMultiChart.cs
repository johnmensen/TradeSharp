using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace FastMultiChart
{
    public partial class FastMultiChart : UserControl
    {
        public delegate int GetScaleValueDel(object value, FastMultiChart chart);
        public delegate object GetValueDel(int scaleValue, FastMultiChart chart);
        public delegate object GetDivisionValue(object value, FastMultiChart chart);
        public delegate object GetMinScaleDivisionDel(int expectedValue, FastMultiChart chart);
        public delegate string GetStringValueDel(object value, FastMultiChart chart);

        enum DraggingBorder { None, Min, Max }

        private GetScaleValueDel getXScaleValue;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GetScaleValueDel GetXScaleValue
        {
            get { return getXScaleValue; }
            set
            {
                getXScaleValue = value;
                foreach (var graph in Graphs)
                    graph.GetXScaleValue = value;
            }
        }

        private GetScaleValueDel getYScaleValue;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GetScaleValueDel GetYScaleValue
        {
            get { return getYScaleValue; }
            set
            {
                getYScaleValue = value;
                foreach (var graph in Graphs)
                    graph.GetYScaleValue = value;
            }
        }

        public GetValueDel GetXValue, GetYValue;
        public GetDivisionValue GetXDivisionValue, GetYDivisionValue;
        public GetMinScaleDivisionDel GetMinXScaleDivision, GetMinYScaleDivision;
        public GetStringValueDel GetXStringValue, GetYStringValue, GetXStringScaleValue, GetYStringScaleValue;

        public readonly ObservableCollection<Graph> Graphs = new ObservableCollection<Graph>();

        //source data range
        public int MinX, MaxX; // !!! for each series in each graph

        // source visuals
        private int scaleDivisionXMinPixel = 70;
        public int ScaleDivisionXMinPixel { get { return scaleDivisionXMinPixel; } set { scaleDivisionXMinPixel = value; } }

        private int scaleDivisionXMaxPixel = -1;
        public int ScaleDivisionXMaxPixel { get { return scaleDivisionXMaxPixel; } set { scaleDivisionXMaxPixel = value; } }

        private int scaleDivisionYMinPixel = 20;
        public int ScaleDivisionYMinPixel { get { return scaleDivisionYMinPixel; } set { scaleDivisionYMinPixel = value; } }
        
        private int scaleDivisionYMaxPixel = 60;
        public int ScaleDivisionYMaxPixel { get { return scaleDivisionYMaxPixel; } set { scaleDivisionYMaxPixel = value; } }
        
        // отступы от краев до области с графиком
        private int marginXLeft = 10;
        public int MarginXLeft { get { return marginXLeft; } set { marginXLeft = value; } }
        
        private int marginXRight = 10;
        public int MarginXRight { get { return marginXRight; } set { marginXRight = value; } }
        
        private int marginYTop = 10;
        public int MarginYTop { get { return marginYTop; } set { marginYTop = value; } }
        
        private int marginYBottom = 10;
        public int MarginYBottom { get { return marginYBottom; } set { marginYBottom = value; } }
        
        private int scrollBarHeight = 80;
        public int ScrollBarHeight { get { return scrollBarHeight; } set { scrollBarHeight = value; } }

        private int innerMarginXLeft = 10;
        public int InnerMarginXLeft { get { return innerMarginXLeft; } set { innerMarginXLeft = value; } }

        private bool showScaleDivisionXLabel = true;
        public bool ShowScaleDivisionXLabel
        {
            get { return showScaleDivisionXLabel; }
            set
            {
                showScaleDivisionXLabel = value;
                Invalidate();
                UpdateScale();
                UpdateScrollBarScale();
            }
        }

        private bool showScaleDivisionYLabel = true;
        public bool ShowScaleDivisionYLabel
        {
            get { return showScaleDivisionYLabel; }
            set
            {
                showScaleDivisionYLabel = value;
                Invalidate();
                UpdateScale();
            }
        }

        private bool showHints = true;
        public bool ShowHints
        {
            get { return showHints; }
            set
            {
                showHints = value;
                Invalidate();
            }
        }

        private bool renderPolygons = true; // move to series
        public bool RenderPolygons
        {
            get { return renderPolygons; }
            set
            {
                renderPolygons = value;
                Invalidate();
            }
        }

        private bool autoScale = true, ready = false, showScrollBar = true;
        private const int captureArea = 3;

        // calculated data range
        private int scrollBarMinX, scrollBarMaxX, scrollBarMinY, scrollBarMaxY;
        [Browsable(false)]
        public int ScrollBarMinX { get { return scrollBarMinX; } }
        [Browsable(false)]
        public int ScrollBarMaxX { get { return scrollBarMaxX; } }

        private int labelMarginX = 30;
        [Browsable(false)]
        public int LabelMarginX { get { return showScaleDivisionXLabel ? labelMarginX : 0; } }

        private int labelMarginY = 30; // make calc
        [Browsable(false)]
        public int LabelMarginY { get { return showScaleDivisionYLabel ? labelMarginY : 0; } }

        // calculated visuals
        private float scrollBarScaleX, scrollBarScaleY;

        private float scaleX;
        [Browsable(false)]
        public float ScaleX { get { return scaleX; } }

        //private int scrollBarScaleDivisionX, scrollBarScaleDivisionY;

        private int scaleDivisionX;
        [Browsable(false)]
        public int ScaleDivisionX { get { return scaleDivisionX; } }

        // interaction
        private DraggingBorder draggingBorder = DraggingBorder.None;

        private int highlightedXPixel = -1;

        public FastMultiChart()
        {
            InitializeComponent();

            GetXScaleValue = GetXScaleValueDefault;
            GetYScaleValue = GetYScaleValueDefault;
            GetXValue = GetXValueDefault;
            GetYValue = GetYValueDefault;
            GetXDivisionValue = GetDivisionValueDefault;
            GetYDivisionValue = GetDivisionValueDefault;
            GetMinXScaleDivision = GetXMinScaleDivisionDefault;
            GetMinYScaleDivision = GetYMinScaleDivisionDefault;
            GetXStringValue = GetXStringValueDefault;
            GetYStringValue = GetYStringValueDefault;
            GetXStringScaleValue = GetXStringValueDefault;
            GetYStringScaleValue = GetYStringValueDefault;

            var g = new Graph(this);
            Graphs.Add(g);

            DoubleBuffered = true;

            UpdateMinMax();
            UpdateScrollBarMinMax();
        }

        public void Initialize()
        {
            UpdateScrollBarMinMax();
            MinX = scrollBarMinX;
            MaxX = scrollBarMaxX;
            UpdateMinMax();
        }

        private int GetXScaleValueDefault(object value, FastMultiChart chart)
        {
            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return 0;
            }
        }

        private int GetYScaleValueDefault(object value, FastMultiChart chart)
        {
            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return 0;
            }
        }

        private object GetXValueDefault(int scaleValue, FastMultiChart chart)
        {
            return scaleValue;
        }

        private object GetYValueDefault(int scaleValue, FastMultiChart chart)
        {
            return scaleValue;
        }

        private object GetDivisionValueDefault(object value, FastMultiChart chart)
        {
            return value;
        }

        private object GetXMinScaleDivisionDefault(int expectedValue, FastMultiChart chart)
        {
            return expectedValue;
        }

        private object GetYMinScaleDivisionDefault(int expectedValue, FastMultiChart chart)
        {
            return expectedValue;
        }

        private string GetXStringValueDefault(object value, FastMultiChart chart)
        {
            return value.ToString();
        }

        private string GetYStringValueDefault(object value, FastMultiChart chart)
        {
            return value.ToString();
        }

        private void UpdateScrollBarMinMax()
        {
            if (!showScrollBar)
                return;
            var allX = new List<int>();
            var allY = new List<int>();
            foreach (var graph in Graphs)
                foreach (var series in graph.Series)
                {
                    allX.AddRange(series.Keys);
                    var values = new List<Cortege2<int, object>>(series.Values);
                    allY.AddRange(values.Select(v => v.a));
                }
            scrollBarMinX = allX.Count == 0 ? 0 : allX.Min();
            scrollBarMaxX = allX.Count == 0 ? 0 : allX.Max();
            scrollBarMinY = allY.Count == 0 ? 0 : allY.Min();
            scrollBarMaxY = allY.Count == 0 ? 0 : allY.Max();
            UpdateScrollBarScale();
        }

        private void UpdateScrollBarScale()
        {
            if (scrollBarMinX == scrollBarMaxX)
                scrollBarScaleX = 0;
            else
                scrollBarScaleX = (float)((ClientSize.Width - marginXRight - marginXLeft - LabelMarginX - 1) / (scrollBarMaxX - scrollBarMinX + 0.0));
            if (scrollBarMinY == scrollBarMaxY)
                scrollBarScaleY = 0;
            else
                scrollBarScaleY = (float)((scrollBarHeight - 1) / (scrollBarMaxY - scrollBarMinY + 0.0));
            //scrollBarScaleDivisionX = FindMinScaleDivision(scrollBarScaleX, scaleDivisionXMinPixel, scaleDivisionXMaxPixel, getXScaleDivision, GetXScaleValue);
            //scrollBarScaleDivisionY = FindMinScaleDivision(scrollBarScaleY, scaleDivisionYMinPixel, scaleDivisionYMaxPixel, getYScaleDivision, GetYScaleValue);
        }

        private void UpdateMinMax()
        {
            ready = false;
            foreach (var graph in Graphs)
            {
                var foundMinMax = false;
                foreach (var series in graph.Series)
                {
                    if (series.Count == 0)
                    {
                        series.BeginIndex = 0;
                        series.EndIndex = -1;
                        continue;
                    }
                    var keys = new List<int>(series.Keys);
                    series.BeginIndex = keys.FindIndex(k => k > MinX);
                    if (series.BeginIndex > 0)
                        series.BeginIndex--;
                    series.EndIndex = keys.FindLastIndex(k => k < MaxX);
                    if (series.BeginIndex < 0 || series.EndIndex < 0)
                    {
                        series.BeginIndex = 0;
                        series.EndIndex = -1;
                        continue;
                    }
                    if (series.EndIndex < keys.Count - 1)
                        series.EndIndex++;
                    if (!foundMinMax)
                    {
                        graph.MinY = series[keys[series.BeginIndex]].a;
                        graph.MaxY = graph.MinY;
                        foundMinMax = true;
                    }
                    for (var i = series.BeginIndex; i <= series.EndIndex; i++)
                    {
                        var y = series[keys[i]].a;
                        if (y < graph.MinY)
                            graph.MinY = y;
                        if (y > graph.MaxY)
                            graph.MaxY = y;
                    }
                }
            }
            UpdateScale();
            ready = true;
        }

        private int FindMinScaleDivision(float scale, int scaleDivisionMinPixel, int scaleDivisionMaxPixel,
                                      GetMinScaleDivisionDel getMinScaleDivision, GetScaleValueDel getScaleValue)
        {
            if (scale <= 0)
                return -1;
            var result = -1;
            var found = false;
            var scaleDivisionMin = scaleDivisionMinPixel / scale;
            var scaleDivisionMax = scaleDivisionMaxPixel < 0
                                        ? Int32.MaxValue - 1
                                        : (int)(scaleDivisionMaxPixel / scale);
            // некоторые реализации getMinScaleDivision всегда выдают одно и то же значение вне зависимости от увеличения expectedValue
            // поэтому, если после 1000 итераций решения нет, то поиск прекратить
            var count = 0;
            for (var expectedValue = (int)scaleDivisionMin; expectedValue <= scaleDivisionMax; expectedValue++) // !!! шаг маловат для Int32.MaxValue, но равен суткам для DateTime
            {
                var divObject = getMinScaleDivision(expectedValue, this);
                result = getScaleValue(divObject, this);
                if (result >= scaleDivisionMin && result <= scaleDivisionMax)
                {
                    found = true;
                    break;
                }
                if (count > 1000)
                    break;
                count++;
            }
            if (!found)
                result = -1;
            return result;
        }

        private void UpdateScale()
        {
            if (autoScale)
            {
                if (MinX == MaxX)
                    scaleX = 0;
                else
                    scaleX = (float)((ClientSize.Width - marginXRight - marginXLeft - LabelMarginX - 1 - innerMarginXLeft) / (MaxX - MinX + 0.0));
                if (scaleX < 0)
                    scaleX = 0;
                var marginYBottomAddition = marginYBottom + (showScrollBar ? scrollBarHeight : 0);
                foreach (var graph in Graphs)
                {
                    if (graph.MinY == graph.MaxY)
                        graph.ScaleY = 0;
                    else
                        graph.ScaleY = (float)((ClientSize.Height - marginYBottomAddition - marginYTop - LabelMarginY - 1) / (graph.MaxY - graph.MinY + 0.0));
                    if (graph.ScaleY < 0)
                        graph.ScaleY = 0;
                }
            }
            scaleDivisionX = FindMinScaleDivision(scaleX, scaleDivisionXMinPixel, scaleDivisionXMaxPixel, GetMinXScaleDivision, GetXScaleValue);
            foreach (var graph in Graphs)
                graph.ScaleDivisionY = FindMinScaleDivision(graph.ScaleY, scaleDivisionYMinPixel, scaleDivisionYMaxPixel, GetMinYScaleDivision, GetYScaleValue);
        }

        private void FastMultiChartPaint(object sender, PaintEventArgs e)
        {
            if (!ready)
                return;
            var marginYBottomAddition = marginYBottom + (showScrollBar ? scrollBarHeight : 0);
            e.Graphics.FillRectangle(new SolidBrush(Color.White), new Rectangle(new Point(0, 0), ClientSize));

            // оси
            var mainLinesPen = new Pen(Color.Gray);
            var commonLinesPen = new Pen(Color.LightGray);
            GraphicsState originalState;
            foreach (var graph in Graphs)
            {
                originalState = e.Graphics.Save();
                e.Graphics.Clip =
                    new Region(new Rectangle(marginXLeft, marginYTop, ClientSize.Width - marginXRight - marginXLeft,
                                             ClientSize.Height - marginYBottomAddition - marginYTop));
                e.Graphics.TranslateTransform(marginXLeft, ClientSize.Height - marginYBottomAddition - 1 + graph.MinY * graph.ScaleY);

                // ось Y
                // !!! пока без расечта scaleDivisionYMinPixel
                if (graph.ScaleDivisionY > 0)
                {
                    // расчет делений
                    var yLabels = new Dictionary<int, string>();
                    var yLabelMaxWidth = -1;
                    for (var divY = graph.ScaleDivisionY; divY <= graph.MaxY; divY = GetYScaleValue(GetYDivisionValue(GetYValue(divY + graph.ScaleDivisionY, this), this), this))
                    {
                        if (divY < graph.MinY)
                            continue;
                        var label = GetYStringScaleValue(GetYValue(divY, this), this); // есть проблема дробности: GetYDivisionValue возвращает точную цену деления, но после GetYValue(GetYScaleValue(...)) она может измениться
                        var width = (int)e.Graphics.MeasureString(label, Font).Width + 1;
                        if (yLabelMaxWidth < width)
                            yLabelMaxWidth = width;
                        yLabels.Add(divY, label);
                    }
                    for (var divY = -graph.ScaleDivisionY; divY >= graph.MinY; divY = GetYScaleValue(GetYDivisionValue(GetYValue(divY - graph.ScaleDivisionY, this), this), this))
                    {
                        var label = GetYStringScaleValue(GetYValue(divY, this), this);
                        var width = (int)e.Graphics.MeasureString(label, Font).Width + 1;
                        if (yLabelMaxWidth < width)
                            yLabelMaxWidth = width;
                        yLabels.Add(divY, label);
                    }
                    labelMarginX = yLabelMaxWidth;
                    UpdateScale();
                    UpdateScrollBarScale();

                    // отрисовка делений с подписями
                    foreach (var yLabel in yLabels)
                    {
                        var divY = yLabel.Key;
                        var divYPixels = divY * graph.ScaleY;
                        e.Graphics.DrawLine(commonLinesPen, LabelMarginX, -divYPixels - LabelMarginY, ClientSize.Width, -divYPixels - LabelMarginY);
                        e.Graphics.DrawString(yLabels[divY], Font, new SolidBrush(Color.Black), new RectangleF(0, -divYPixels - LabelMarginY + 2, LabelMarginX, graph.ScaleDivisionY * graph.ScaleY - 4), new StringFormat { Alignment = StringAlignment.Far, FormatFlags = StringFormatFlags.NoWrap });
                    }
                }
                // ось X
                // !!! ось пока без расчета LabelMarginY, scaleDivisionXMinPixel
                if (scaleDivisionX > 0)
                    for (var divX = scaleDivisionX; divX <= MaxX; divX = GetXScaleValue(GetXDivisionValue(GetXValue(divX + scaleDivisionX, this), this), this))
                    {
                        if (divX < MinX)
                            continue;
                        var divXPixels = (divX - MinX) * scaleX;
                        e.Graphics.DrawLine(commonLinesPen, divXPixels + LabelMarginX, -graph.MinY * graph.ScaleY - LabelMarginY, divXPixels + LabelMarginX, -graph.MaxY * graph.ScaleY - LabelMarginY);
                        e.Graphics.DrawString(GetXStringScaleValue(GetXValue(divX, this), this), Font, new SolidBrush(Color.Black), new RectangleF(divXPixels + LabelMarginX, -graph.MinY * graph.ScaleY - LabelMarginY + 2, scaleDivisionX * scaleX, LabelMarginY - 4), new StringFormat { FormatFlags = StringFormatFlags.NoWrap });
                    }

                // отображение осей
                e.Graphics.DrawLine(mainLinesPen, 0, -LabelMarginY, ClientSize.Width, -LabelMarginY);
                e.Graphics.DrawLine(mainLinesPen, LabelMarginX, -graph.MinY * graph.ScaleY - LabelMarginY, LabelMarginX, -graph.MaxY * graph.ScaleY - LabelMarginY);
                e.Graphics.DrawString(GetYStringScaleValue(GetYValue(0, this), this), Font, new SolidBrush(Color.Black), new RectangleF(0, -LabelMarginY + 2, LabelMarginX, graph.ScaleDivisionY * graph.ScaleY - 4), new StringFormat { Alignment = StringAlignment.Far });
                e.Graphics.Restore(originalState);
            }

            foreach (var graph in Graphs) // !!! divide into separate horizontal spaces
            {
                originalState = e.Graphics.Save();
                e.Graphics.Clip =
                    new Region(new Rectangle(marginXLeft + LabelMarginX, marginYTop, ClientSize.Width - marginXRight - marginXLeft - LabelMarginX,
                                             ClientSize.Height - marginYBottomAddition - marginYTop - LabelMarginY));
                e.Graphics.TranslateTransform(marginXLeft + LabelMarginX, ClientSize.Height - marginYBottomAddition - 1 + graph.MinY * graph.ScaleY - LabelMarginY);

                // данные
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                foreach (var series in graph.Series)
                {
                    if (series.BeginIndex >= series.EndIndex)
                        continue;
                    var keys = series.Keys;
                    var points = new List<PointF>();
                    for (var i = series.BeginIndex; i <= series.EndIndex; i++)
                    {
                        var x = keys[i];
                        var y = series[x].a;
                        points.Add(new PointF((x - MinX) * scaleX, -y * graph.ScaleY));
                    }
                    e.Graphics.DrawLines(series.Pen, points.ToArray());
                    if (RenderPolygons)
                    {
                        points.Insert(0, new PointF((keys[series.BeginIndex] - MinX) * scaleX, -graph.MinY * graph.ScaleY));
                        points.Add(new PointF((keys[series.EndIndex] - MinX) * scaleX, -graph.MinY * graph.ScaleY));
                        points.Add(new PointF((keys[series.BeginIndex] - MinX) * scaleX, -graph.MinY * graph.ScaleY));
                        e.Graphics.FillPolygon(new SolidBrush(Color.FromArgb(64, series.Pen.Color)), points.ToArray());
                    }
                }

                // подсказка
                if (showHints)
                {
                    var graphSize = new Size(ClientSize.Width - LabelMarginX, ClientSize.Height - marginYBottomAddition - LabelMarginY);
                    const int rectCornerRadius = 10;
                    var hints = new List<ChartHint>();
                    foreach (var series in graph.Series)
                        if (series.HighlightedXIndex != -1)
                        {
                            var x = series.Keys[series.HighlightedXIndex];
                            var xPixel = (x - MinX) * scaleX;
                            var y = series[x].a;
                            var yPixel = y * graph.ScaleY;

                            // cross-point
                            const int circleRadius = 4;
                            e.Graphics.FillEllipse(new SolidBrush(series.Pen.Color), xPixel - circleRadius,
                                                   -yPixel - circleRadius, circleRadius * 2, circleRadius * 2);

                            var rectText = series.XMemberTitle + " = " + GetXStringValue(GetXValue(x, this), this) + "\n" +
                                           series.YMemberTitle + " = " + GetYStringValue(GetYValue(y, this), this);
                            var textSize = e.Graphics.MeasureString(rectText, Font);
                            var rect = new Rectangle((int)xPixel + rectCornerRadius * 2, (int)-yPixel + rectCornerRadius,
                                                     (int)Math.Ceiling(textSize.Width), (int)Math.Ceiling(textSize.Height));
                            hints.Add(new ChartHint { Rect = rect, Text = rectText, Data = series });
                        }
                    // re-positioning hint on bottom
                    var ordHints = hints.OrderBy(hint => hint.Rect.Top);
                    var prevY = 0;
                    // to save changes change Cortege3 to class
                    for (var hintIndex = 0; hintIndex < ordHints.Count(); hintIndex++)
                    {
                        var hint = ordHints.ElementAt(hintIndex);
                        var rect = hint.Rect;
                        var rectWoTransform = new Rectangle(rect.X - rectCornerRadius, rect.Y - rectCornerRadius,
                                                            rect.Width + rectCornerRadius * 2,
                                                            rect.Height + rectCornerRadius * 2);
                        rectWoTransform.Offset(-(marginXLeft - LabelMarginX), (graphSize.Height + (int)(graph.MinY * graph.ScaleY)));
                        if (rectWoTransform.Top < prevY)
                        {
                            rect.Offset(0, prevY - rectWoTransform.Top);
                            rectWoTransform.Offset(0, prevY - rectWoTransform.Bottom);
                            hint.Rect = rect;
                        }
                        prevY = rectWoTransform.Bottom + rectCornerRadius;
                    }
                    ordHints = hints.OrderByDescending(hint => hint.Rect.Top);
                    prevY = graphSize.Height;
                    e.Graphics.SmoothingMode = SmoothingMode.Default;
                    for (var hintIndex = 0; hintIndex < ordHints.Count(); hintIndex++)
                    {
                        var hint = ordHints.ElementAt(hintIndex);
                        // re-positioning hint on top
                        var rect = hint.Rect;
                        var rectWoTransform = new Rectangle(rect.X - rectCornerRadius, rect.Y - rectCornerRadius,
                                                            rect.Width + rectCornerRadius * 2,
                                                            rect.Height + rectCornerRadius * 2);
                        rectWoTransform.Offset(-(marginXLeft - LabelMarginX), (graphSize.Height + (int)(graph.MinY * graph.ScaleY)));
                        if (rectWoTransform.Bottom > prevY)
                        {
                            rect.Offset(0, prevY - rectWoTransform.Bottom);
                            rectWoTransform.Offset(0, prevY - rectWoTransform.Bottom);
                        }
                        if (rectWoTransform.Right > graphSize.Width)
                            rect.Offset(-rectWoTransform.Width - rectCornerRadius * 2, 0);
                        prevY = rectWoTransform.Top - rectCornerRadius;

                        // drawing hint !!! simplify to path
                        var tipPen = new Pen(hint.Data.Pen.Color, 0);
                        var rectBrush = new SolidBrush(Color.FromArgb(192, Color.White));
                        e.Graphics.FillRectangle(rectBrush, rect);
                        e.Graphics.DrawString(hint.Text, Font, new SolidBrush(tipPen.Color), rect);
                        e.Graphics.FillRectangle(rectBrush, rect.X, rect.Y - rectCornerRadius, rect.Width, rectCornerRadius);
                        e.Graphics.DrawLine(tipPen, rect.X, rect.Y - rectCornerRadius, rect.X + rect.Width, rect.Y - rectCornerRadius);
                        e.Graphics.FillRectangle(rectBrush, rect.X + rect.Width, rect.Y, rectCornerRadius, rect.Height);
                        e.Graphics.DrawLine(tipPen, rect.X + rect.Width + rectCornerRadius, rect.Y, rect.X + rect.Width + rectCornerRadius, rect.Y + rect.Height);
                        e.Graphics.FillRectangle(rectBrush, rect.X, rect.Y + rect.Height, rect.Width, rectCornerRadius);
                        e.Graphics.DrawLine(tipPen, rect.X, rect.Y + rect.Height + rectCornerRadius, rect.X + rect.Width, rect.Y + rect.Height + rectCornerRadius);
                        e.Graphics.FillRectangle(rectBrush, rect.X - rectCornerRadius, rect.Y, rectCornerRadius, rect.Height);
                        e.Graphics.DrawLine(tipPen, rect.X - rectCornerRadius, rect.Y, rect.X - rectCornerRadius, rect.Y + rect.Height);
                        e.Graphics.FillPie(rectBrush, rect.X - rectCornerRadius, rect.Y - rectCornerRadius, rectCornerRadius * 2, rectCornerRadius * 2, 180, 90);
                        e.Graphics.DrawArc(tipPen, rect.X - rectCornerRadius, rect.Y - rectCornerRadius, rectCornerRadius * 2, rectCornerRadius * 2, 180, 90);
                        e.Graphics.FillPie(rectBrush, rect.X + rect.Width - rectCornerRadius, rect.Y - rectCornerRadius, rectCornerRadius * 2, rectCornerRadius * 2, -90, 90);
                        e.Graphics.DrawArc(tipPen, rect.X + rect.Width - rectCornerRadius, rect.Y - rectCornerRadius, rectCornerRadius * 2, rectCornerRadius * 2, -90, 90);
                        e.Graphics.FillPie(rectBrush, rect.X + rect.Width - rectCornerRadius, rect.Y + rect.Height - rectCornerRadius, rectCornerRadius * 2, rectCornerRadius * 2, 0, 90);
                        e.Graphics.DrawArc(tipPen, rect.X + rect.Width - rectCornerRadius, rect.Y + rect.Height - rectCornerRadius, rectCornerRadius * 2, rectCornerRadius * 2, 0, 90);
                        e.Graphics.FillPie(rectBrush, rect.X - rectCornerRadius, rect.Y + rect.Height - rectCornerRadius, rectCornerRadius * 2, rectCornerRadius * 2, 90, 90);
                        e.Graphics.DrawArc(tipPen, rect.X - rectCornerRadius, rect.Y + rect.Height - rectCornerRadius, rectCornerRadius * 2, rectCornerRadius * 2, 90, 90);
                    }

                    // x-line
                    e.Graphics.DrawLine(new Pen(Color.Red), highlightedXPixel, -graph.MaxY * graph.ScaleY, highlightedXPixel, -graph.MinY * graph.ScaleY);
                }

                e.Graphics.Restore(originalState);
            }

            // деления скролл-бара ?

            // данные скролл-бара
            if (!showScrollBar)
                return;
            if (scrollBarScaleX <= 0 || scrollBarScaleY <= 0)
                return;
            e.Graphics.Clip =
                new Region(new Rectangle(marginXLeft, ClientSize.Height - marginYBottom - scrollBarHeight,
                                         ClientSize.Width - marginXRight - marginXLeft, scrollBarHeight));
            e.Graphics.TranslateTransform(marginXLeft + LabelMarginX, ClientSize.Height - marginYBottom - 1 + scrollBarMinY * scrollBarScaleY);
            e.Graphics.ScaleTransform(1, -1);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            foreach (var graph in Graphs)
                foreach (var series in graph.Series)
                {
                    var keys = series.Keys;
                    if (keys.Count < 1)
                        continue;
                    var points = new List<PointF>();
                    for (var i = 0; i < keys.Count; i++)
                    {
                        var x = keys[i];
                        var y = series[x].a;
                        points.Add(new PointF((x - scrollBarMinX) * scrollBarScaleX, y * scrollBarScaleY));
                    }
                    if (RenderPolygons)
                    {
                        points.Insert(0, new PointF((keys[0] - scrollBarMinX) * scrollBarScaleX, scrollBarMinY * scrollBarScaleY));
                        points.Add(new PointF((keys[keys.Count - 1] - scrollBarMinX) * scrollBarScaleX, scrollBarMinY * scrollBarScaleY));
                        e.Graphics.FillPolygon(new SolidBrush(Color.FromArgb(128, series.Pen.Color)), points.ToArray());
                    }
                    else
                        e.Graphics.DrawLines(series.Pen, points.ToArray());
                }

            // оси скролл-бара
            e.Graphics.SmoothingMode = SmoothingMode.Default;
            e.Graphics.DrawLine(mainLinesPen, 0, 0, ClientSize.Width, 0);
            e.Graphics.DrawLine(mainLinesPen, 0, scrollBarMinY * scrollBarScaleY, 0, scrollBarMaxY * scrollBarScaleY);

            // невидимые зоны - затенение в скролл-баре данных, невидимых для основой области
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(128, Color.Gray)), 0, scrollBarMinY * scrollBarScaleY, (MinX - scrollBarMinX) * scrollBarScaleX, scrollBarHeight);
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(128, Color.Gray)), (MaxX - scrollBarMinX) * scrollBarScaleX, scrollBarMinY * scrollBarScaleY, ClientSize.Width - (MaxX - scrollBarMinX) * scrollBarScaleX, scrollBarHeight);

            // "рукоятки" невидимых зон
            var handlePosX = (MinX - scrollBarMinX) * scrollBarScaleX - captureArea;
            var handlePosY = scrollBarMinY * scrollBarScaleY + scrollBarHeight / 4;
            e.Graphics.FillRectangle(new SolidBrush(draggingBorder == DraggingBorder.Min ? Color.LightPink : Color.White), handlePosX, handlePosY, captureArea * 2, scrollBarHeight / 2);
            e.Graphics.DrawRectangle(new Pen(Color.Black), handlePosX, handlePosY, captureArea * 2, scrollBarHeight / 2);
            handlePosX = (MaxX - scrollBarMinX) * scrollBarScaleX - captureArea;
            e.Graphics.FillRectangle(new SolidBrush(draggingBorder == DraggingBorder.Max ? Color.LightPink : Color.White), handlePosX, handlePosY, captureArea * 2, scrollBarHeight / 2);
            e.Graphics.DrawRectangle(new Pen(Color.Black), handlePosX, handlePosY, captureArea * 2, scrollBarHeight / 2);
        }

        private DraggingBorder GetBorder(Point location)
        {
            if (location.Y < ClientSize.Height - marginYBottom - scrollBarHeight)
                return DraggingBorder.None;
            if (location.Y > ClientSize.Height - marginYBottom)
                return DraggingBorder.None;
            if ((location.X >= marginXLeft + LabelMarginX + (MinX - scrollBarMinX) * scrollBarScaleX - captureArea) &&
                (location.X <= marginXLeft + LabelMarginX + (MinX - scrollBarMinX) * scrollBarScaleX + captureArea))
                return DraggingBorder.Min;
            if ((location.X >= marginXLeft + LabelMarginX + (MaxX - scrollBarMinX) * scrollBarScaleX - captureArea) &&
                (location.X <= marginXLeft + LabelMarginX + (MaxX - scrollBarMinX) * scrollBarScaleX + captureArea))
                return DraggingBorder.Max;
            return DraggingBorder.None;
        }

        private void CalcScrollBarWindow(Point centerPosition, out int min, out int max)
        {
            var x = (int)((centerPosition.X - marginXLeft - LabelMarginX) / scrollBarScaleX) + scrollBarMinX;
            var size = MaxX - MinX;
            min = x - size / 2;
            max = x + size / 2;
            if (size % 2 == 1)
                max++;
            if (min < scrollBarMinX)
            {
                min = scrollBarMinX;
                max = min + size;
            }
            if (max > scrollBarMaxX)
            {
                max = scrollBarMaxX;
                min = max - size;
            }
        }

        private void FastMultiChartResize(object sender, EventArgs e)
        {
            Invalidate();
            UpdateScale();
            UpdateScrollBarScale();
        }

        private void FastMultiChartKeyPress(object sender, KeyPressEventArgs e)
        {
            /*if (e.KeyChar == '+')
                scaleX++;
            if (e.KeyChar == '-')
                if (scaleX > 1)
                    scaleX--;
            if (e.KeyChar == '*')
                scaleY++;
            if (e.KeyChar == '/')
                if (scaleY > 1)
                    scaleY--;
            UpdateScale();
            Invalidate();*/
        }

        private void FastMultiChartMouseMove(object sender, MouseEventArgs e)
        {
            var update = false;
            if (draggingBorder != DraggingBorder.None)
            {
                var x = (int)((e.Location.X - marginXLeft - LabelMarginX) / scrollBarScaleX) + scrollBarMinX;
                if (draggingBorder == DraggingBorder.Min)
                {
                    if (x < scrollBarMinX)
                        x = scrollBarMinX;
                    if (x > MaxX - 1)
                        x = MaxX - 1;
                    update = x != MinX;
                    MinX = x;
                }
                if (draggingBorder == DraggingBorder.Max)
                {
                    if (x > scrollBarMaxX)
                        x = scrollBarMaxX;
                    if (x < MinX + 1)
                        x = MinX + 1;
                    update = x != MaxX;
                    MaxX = x;
                }
                if (update)
                {
                    UpdateMinMax();
                    Invalidate();
                }
                return;
            }

            // moving scroll bar window
            if (e.Button == MouseButtons.Left && e.Location.Y >= ClientSize.Height - marginYBottom - scrollBarHeight && e.Location.Y <= ClientSize.Height - marginYBottom)
            {
                int newMin, newMax;
                CalcScrollBarWindow(e.Location, out newMin, out newMax);
                if (newMin != MinX || newMax != MaxX)
                {
                    MinX = newMin;
                    MaxX = newMax;
                    UpdateMinMax();
                    Invalidate();
                }
            }
            var border = GetBorder(e.Location);
            if (border != DraggingBorder.None)
                Cursor = Cursors.SizeWE;
            else
                Cursor = Cursors.Default;

            // finding highlight x-line
            if (e.Location.Y >= marginYTop && e.Location.Y <= ClientSize.Height - marginYBottom - scrollBarHeight)
            {
                var newHighlightedXPixel = e.Location.X - LabelMarginX - marginXLeft;
                var minHl = Math.Min(highlightedXPixel, newHighlightedXPixel);
                var maxHl = Math.Max(highlightedXPixel, newHighlightedXPixel);
                Invalidate(new Rectangle(minHl + LabelMarginX + marginXLeft, marginYTop, maxHl - minHl + 1 + LabelMarginX + marginXLeft, ClientSize.Height - marginYBottom - scrollBarHeight));
                highlightedXPixel = newHighlightedXPixel;
                foreach (var graph in Graphs) // !!! divide into separate horizontal spaces
                {
                    foreach (var series in graph.Series)
                    {
                        var xIndex = -1;
                        var minDelta = -1;
                        var keys = series.Keys;
                        for (var i = series.BeginIndex; i <= series.EndIndex; i++)
                        {
                            var x = keys[i];
                            var xPixel = (x - MinX) * scaleX;
                            var delta = (int)Math.Abs(highlightedXPixel - xPixel);
                            if (minDelta == -1)
                            {
                                minDelta = delta;
                                xIndex = i;
                            }
                            else if (delta < minDelta)
                            {
                                minDelta = delta;
                                xIndex = i;
                            }
                        }
                        if (series.HighlightedXIndex != xIndex)
                        {
                            series.HighlightedXIndex = xIndex;
                            update = true;
                        }
                    }
                }
                if(update)
                    Invalidate(); 
                return;
            }
            foreach (var graph in Graphs)
                foreach (var series in graph.Series)
                    if (series.HighlightedXIndex != -1)
                    {
                        series.HighlightedXIndex = -1;
                        update = true;
                    }
            if (highlightedXPixel != -1)
            {
                highlightedXPixel = -1;
                update = true;
            }
            if(update)
                Invalidate();
        }

        private void FastMultiChartMouseDown(object sender, MouseEventArgs e)
        {
            draggingBorder = GetBorder(e.Location);
            if (draggingBorder != DraggingBorder.None)
                return;
            if (e.Button == MouseButtons.Left && e.Location.Y >= ClientSize.Height - marginYBottom - scrollBarHeight &&
                e.Location.Y <= ClientSize.Height - marginYBottom)
            {
                int newMin, newMax;
                CalcScrollBarWindow(e.Location, out newMin, out newMax);
                if (newMin != MinX || newMax != MaxX)
                {
                    MinX = newMin;
                    MaxX = newMax;
                    UpdateMinMax();
                    Invalidate();
                }
            }
        }

        private void FastMultiChartMouseUp(object sender, MouseEventArgs e)
        {
            if (draggingBorder != DraggingBorder.None)
            {
                draggingBorder = DraggingBorder.None;
                Invalidate();
            }
        }
    }

    public class Graph
    {
        private FastMultiChart.GetScaleValueDel getXScaleValue;
        public FastMultiChart.GetScaleValueDel GetXScaleValue
        {
            get { return getXScaleValue; }
            set
            {
                getXScaleValue = value;
                foreach (var series in Series)
                    series.GetXScaleValue = value;
            }
        }

        private FastMultiChart.GetScaleValueDel getYScaleValue;
        public FastMultiChart.GetScaleValueDel GetYScaleValue
        {
            get { return getYScaleValue; }
            set
            {
                getYScaleValue = value;
                foreach (var series in Series)
                    series.GetYScaleValue = value;
            }
        }

        public string Title { get; set; }

        // calculated data range
        public int MinY, MaxY;
        public float ScaleY;
        public int ScaleDivisionY;

        public readonly ObservableCollection<Series> Series = new ObservableCollection<Series>();

        public FastMultiChart Owner;

        public Graph(FastMultiChart owner)
        {
            Owner = owner;
            GetXScaleValue = owner.GetXScaleValue;
            GetYScaleValue = owner.GetYScaleValue;
            Series.CollectionChanged += delegate
                {
                    foreach (var series in Series)
                    {
                        series.Owner = owner;
                        series.GetXScaleValue = getXScaleValue;
                        series.GetYScaleValue = getYScaleValue;
                    }
                };
        }
    }

    public class Series
    {
        public FastMultiChart.GetScaleValueDel GetXScaleValue, GetYScaleValue;

        public string Title { get; set; }

        public int Count { get { return data.Count; } }

        public Pen Pen { get; set; }

        private readonly SortedList<int, Cortege2<int, Object>> data = new SortedList<int, Cortege2<int, Object>>();

        public string XMember { get; set; }

        public string YMember { get; set; }

        public string XMemberTitle { get; set; }

        public string YMemberTitle { get; set; }

        public int BeginIndex { get; set; }

        public int EndIndex { get; set; }

        public int HighlightedXIndex { get; set; }

        public FastMultiChart Owner;

        public Series(string xMember = "", string yMember = "", Pen pen = null)
        {
            Pen = pen ?? new Pen(Color.Black);
            BeginIndex = 0;
            EndIndex = -1;
            HighlightedXIndex = -1;
            XMember = xMember;
            YMember = yMember;
        }

        public void Add(Object value)
        {
            if (string.IsNullOrEmpty(XMember) || string.IsNullOrEmpty(YMember) || GetXScaleValue == null || GetYScaleValue == null)
                return;
            if (data.Count == 0)
                MakeTitles(value.GetType());
            var xProperty = value.GetType().GetProperty(XMember);
            var yProperty = value.GetType().GetProperty(YMember);
            try
            {
                data.Add(GetXScaleValue(xProperty.GetValue(value, null), Owner), new Cortege2<int, object>(GetYScaleValue(yProperty.GetValue(value, null), Owner), value));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void AddRange(IEnumerable<Object> values)
        {
            if (string.IsNullOrEmpty(XMember) || string.IsNullOrEmpty(YMember) || GetXScaleValue == null || GetYScaleValue == null)
                return;
            
            foreach (var value in values)
            {
                if (data.Count == 0)
                    MakeTitles(value.GetType());
                var xProperty = value.GetType().GetProperty(XMember);
                var yProperty = value.GetType().GetProperty(YMember);
                try
                {
                    data.Add(GetXScaleValue(xProperty.GetValue(value, null), Owner), new Cortege2<int, object>(GetYScaleValue(yProperty.GetValue(value, null), Owner), value));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private void MakeTitles(Type typeOfVal)
        {
            if (string.IsNullOrEmpty(XMemberTitle))
            {
                var prop = typeOfVal.GetProperty(XMember);
                XMemberTitle = prop.Name;
                var dispAttr = prop.GetCustomAttributes(typeof (DisplayNameAttribute), true);
                if (dispAttr.Length > 0)
                    XMemberTitle = ((DisplayNameAttribute) dispAttr[0]).DisplayName;
            }

            if (string.IsNullOrEmpty(YMemberTitle))
            {
                var prop = typeOfVal.GetProperty(YMember);
                YMemberTitle = prop.Name;
                var dispAttr = prop.GetCustomAttributes(typeof (DisplayNameAttribute), true);
                if (dispAttr.Length > 0)
                    YMemberTitle = ((DisplayNameAttribute) dispAttr[0]).DisplayName;
            }
        }

        public void Clear()
        {
            data.Clear();
        }

        public IList<int> Keys { get { return data.Keys; } }

        public Cortege2<int, Object> this[int key] { get { return data[key]; } }

        public IList<Cortege2<int, Object>> Values { get { return data.Values; } }
    }

    class ChartHint
    {
        public Rectangle Rect;
        public string Text;
        public Series Data;
    }
}

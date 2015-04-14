using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Candlechart.Chart;
using Candlechart.ChartMath;
using Candlechart.Core;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Series
{
    /// <summary>
    /// горизонтальные проекции Фибоначчи
    /// </summary>
    [LocalizedSeriesToolButton(CandleChartControl.ChartTool.FiboSpan, "TitleHorizontalFibonacciProjections", ToolButtonImageIndex.HorzSpan)]
    public class SeriesFiboSpan : InteractiveObjectSeries
    {
        private float fontSize = 8;
        public float FontSize
        {
            get { return fontSize; }
            set { fontSize = value; }
        }

        /// <summary>
        /// исходные точки проекций
        /// </summary>
        public List<FiboSpan> data = new List<FiboSpan>();
        public override int DataCount { get { return data.Count; } }

        /// <summary>
        /// готовые проекции (см MakeProjections)
        /// </summary>
        public List<FiboSpanProjection> projections = new List<FiboSpanProjection>();
        
        private bool drawText = true;
        public bool DrawText
        {
            get { return drawText; }
            set { drawText = value; }
        }

        public SeriesFiboSpan(string name)
            : base(name, CandleChartControl.ChartTool.FiboSpan)
        {
        }

        public override bool GetXExtent(ref double left, ref double right)
        {
            return false;
        }

        public override bool GetYExtent(double left, double right, ref double top, ref double bottom)
        {
            return false;
        }

        public override void Draw(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            base.Draw(g, worldRect, canvasRect);
            
            using (var font = new Font(Chart.Font.FontFamily, fontSize))
            using (var penStorage = new PenStorage())
            using (var brushStorage = new BrushesStorage())
            {
                var brush = brushStorage.GetBrush(Color.White);

                foreach (var span in data)
                {
                    DrawSpan(g, worldRect, canvasRect, span, brush, penStorage);
                }

                foreach (var proj in projections)
                {
                    DrawProjection(g, worldRect, canvasRect, proj, brush, font, 
                        penStorage, brushStorage);
                }
            }
        }

        public void DrawSpan(Graphics g, RectangleD worldRect, Rectangle canvasRect, 
            FiboSpan span, Brush brushWhite, PenStorage penStorage)
        {
            if (span.points.Count == 0) return;
            var pen = penStorage.GetPen(span.Color);            
            var ptA = Conversion.WorldToScreen(new PointD(span.points[0].X, span.points[0].Y),
                                                                worldRect, canvasRect);                
            if (span.points.Count == 2)
            {
                var ptB = Conversion.WorldToScreen(new PointD(span.points[1].X, span.points[1].Y),
                                                                worldRect, canvasRect);
                g.DrawLine(pen, ptA.ToPointF(), ptB.ToPointF());
                DrawMarker(g, pen, brushWhite, ptB);
            }
            DrawMarker(g, pen, brushWhite, ptA);            
        }

        public void DrawProjection(Graphics g, RectangleD worldRect, Rectangle canvasRect, FiboSpanProjection proj, 
            Brush brushWhite, Font font, PenStorage penStorage, BrushesStorage brushes)
        {
            // нижняя точка
            var projLow = Conversion.WorldToScreen(new PointD(proj.Index, 0), worldRect, canvasRect);
            projLow.Y = canvasRect.Bottom;
            // верхняя точка
            PointD projHigh = proj.UpperPrice.HasValue
                ? Conversion.WorldToScreen(new PointD(proj.Index, proj.UpperPrice.Value), worldRect, canvasRect)
                : new PointD(projLow.X, canvasRect.Top);

            var dashStyle = proj.AckDegree == 1 ? DashStyle.Dash : DashStyle.Solid;
            var pen = penStorage.GetPen(proj.Color, 1, dashStyle);
            
            g.DrawLine(pen, (float)projLow.X, (float)projLow.Y, (float)projHigh.X, (float)projHigh.Y);
            
            if (proj.AckDegree > 1)
            {// показать степень подтверждения
                const int textOffset = 37;
                const int textSize = 18, textSize2 = 9;
                g.FillEllipse(brushWhite, (float)projLow.X - textSize2, (float)projLow.Y - textOffset - textSize2,
                    textSize, textSize);
                g.DrawEllipse(pen, (float)projLow.X - textSize2, (float)projLow.Y - textOffset - textSize2,
                    textSize, textSize);
                var text = proj.AckDegree.ToString();
                var textSz = g.MeasureString(text, Chart.Font);
                var textLeft = (float) projLow.X - textSz.Width/2;
                var textTop = (float) projLow.Y - textSz.Height/2 - textOffset;

                var brush = brushes.GetBrush(proj.Color);
                g.DrawString(text, font, brush, textLeft, textTop);

                textTop += (textSize + 4);
                var timeStr = Chart.StockSeries.GetCandleOpenTimeByIndex(proj.Index).ToString("dd MMM HH:ss");
                textSz = g.MeasureString(timeStr, Chart.Font);
                textLeft = (float) projLow.X - textSz.Width/2;                        
                
                g.FillRectangle(brushWhite, textLeft - 2, textTop - 2, textSz.Width + 4, textSz.Height + 3);
                g.DrawRectangle(pen, textLeft - 2, textTop - 2, textSz.Width + 4, textSz.Height + 3);
                g.DrawString(timeStr, Chart.Font, brush, textLeft, textTop);                
            }
        }

        private static void DrawMarker(Graphics g, Pen pen, Brush brush, PointD center)
        {
            var points = new PointF[3];
            points[0].X = (float) center.X - 3;
            points[0].Y = (float)center.Y - 4;
            points[1].X = (float)center.X + 4;
            points[1].Y = (float)center.Y;
            points[2].X = (float)center.X - 3;
            points[2].Y = (float)center.Y + 4;
            g.DrawPolygon(pen, points);
            g.FillPolygon(brush, points);
        }
    
        /// <summary>
        /// построить проекции
        /// </summary>
        public void MakeProjections()
        {
            projections.Clear();
            foreach (var span in data)
            {
                if (!span.Completed) continue;
                var projs = span.GetProjections();
                foreach (var index in projs)
                {
                    var hasProj = false;
                    foreach (var proj in projections)
                    {
                        if (proj.Index == index)
                        {// подтвердить проекцию
                            proj.AckDegree++;
                            proj.Color = Color.Red;
                            hasProj = true;
                            break;
                        }
                    }
                    if (hasProj) continue;
                    // создать новую проекцию
                    var newProj = new FiboSpanProjection {Index = index, Color = span.Color};
                    if (index >= 0 && index < Chart.StockSeries.Data.Count) 
                        newProj.UpperPrice = Chart.StockSeries.Data[index].low;
                    projections.Add(newProj);
                }
            }
            // отфильтровать проекции
            if (projections.Count > 1)
            {
                projections = projections.OrderBy(p => p.Index).ToList();
                for (var i = 0; i < projections.Count; i++)
                {
                    var leftDegree = i == 0 ? 0 : projections[i - 1].Index == projections[i].Index - 1
                                         ? projections[i - 1].AckDegree : 0;
                    var rightDegree = i == (projections.Count - 1) ? 0 :
                        projections[i + 1].Index == projections[i].Index + 1 ? projections[i + 1].AckDegree : 0;
                    if (projections[i].AckDegree < leftDegree || projections[i].AckDegree < rightDegree)
                    {// отфильтровать
                        projections[i].Filtered = true;
                    }
                }
                for (var i = 0; i < projections.Count; i++)
                {
                    if (projections[i].Filtered)
                    {
                        projections.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        protected override void OnMouseDown(List<SeriesEditParameter> parameters,
            MouseEventArgs e, Keys modifierKeys, out IChartInteractiveObject objectToEdit)
        {
            objectToEdit = null;
            if (e.Button != MouseButtons.Left) return;
            var pointD = Chart.Owner.MouseToWorldCoords(e.X, e.Y);
            var index = (int)Math.Round(pointD.X);
            if (index >= 0 && index < Chart.StockPane.StockSeries.Data.Count)
                pointD.Y = (double)(Chart.StockPane.StockSeries.Data[index].low);

            var incompleted = data.Find(s => s.Completed == false);
            if (incompleted != null)
            {
                incompleted.AddPoint(pointD.X, pointD.Y);
                MakeProjections();
            }
            else
            {
                var span = new FiboSpan(this);
                span.AddPoint(pointD.X, pointD.Y);
                if (Owner.Owner.Owner.AdjustObjectColorsOnCreation)
                    span.AjustColorScheme(Owner.Owner.Owner);
                data.Add(span);
            }                
        }

        public override void AddObjectsInList(List<IChartInteractiveObject> interObjects)
        {
            foreach (var item in data) interObjects.Add(item);
        }

        public override IChartInteractiveObject LoadObject(XmlElement objectNode, CandleChartControl owner, bool trimObjectsOutOfHistory = false)
        {
            var obj = new FiboSpan(this);
            obj.LoadFromXML(objectNode, owner);
            data.Add(obj);
            return obj;
        }

        public override void RemoveObjectFromList(IChartInteractiveObject interObject)
        {
            if (interObject == null) return;
            if (interObject is FiboSpan == false) return;
            data.Remove((FiboSpan)interObject);
            MakeProjections();
        }

        public override void RemoveObjectByNum(int num)
        {
            data.RemoveAt(num);
        }

        public override IChartInteractiveObject GetObjectByNum(int num)
        {
            return data[num];
        }

        public override List<IChartInteractiveObject> GetObjectsUnderCursor(int screenX, int screenY, int tolerance)
        {
            var ptClient = Owner.PointToClient(new Point(screenX, screenY));
            var list = new List<IChartInteractiveObject>();
            foreach (var span in data)
            {
                if (!span.Completed) continue;
                var a = Conversion.WorldToScreen(new PointD(span.points[0].X, span.points[0].Y),
                    Owner.WorldRect, Owner.CanvasRect);
                var b = Conversion.WorldToScreen(new PointD(span.points[1].X, span.points[1].Y),
                    Owner.WorldRect, Owner.CanvasRect);
                if (Geometry.IsDotInArea(new PointD(ptClient.X, ptClient.Y), a, b, 5))
                    list.Add(span);
            }
            return list;
        }

        public override void ProcessLoadingCompleted(CandleChartControl owner)
        {
            MakeProjections();
        }

        public override void AdjustColorScheme(CandleChartControl chart)
        {
            foreach (var obj in data)
                obj.AjustColorScheme(chart);
        }

        public override IChartInteractiveObject FindObject(Func<IChartInteractiveObject, bool> predicate, out int objIndex)
        {
            objIndex = -1;
            for (var i = 0; i < data.Count; i++)
            {
                if (predicate(data[i]))
                {
                    objIndex = i;
                    return data[i];
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Пара точек, из которых строится проекция вероятного роста цены    
    /// </summary>
    public class FiboSpan : IChartInteractiveObject
    {
        [Browsable(false)]
        public InteractiveObjectSeries Owner { get; set; }

        private SeriesFiboSpan ownerSeries;
        
        [Browsable(false)]
        public string ClassName { get { return Localizer.GetString("TitleHorizontalProjectionsShort"); } }

        private static int nextFiboSpanNum = 1;

        /// <summary>
        /// лист точек вида index, price
        /// </summary>
        public readonly List<PointD> points = new List<PointD>();
        
        /// <summary>
        /// проекции
        /// </summary>
        public List<decimal> series = new List<decimal> {0.618M, 1, 1.618M, 2.618M};

        public List<decimal> Series
        {
            get { return series; }
            set { series = value; }
        }

        /// <summary>
        /// определить уровни в виде строки (для окна свойств)
        /// </summary>
        [LocalizedDisplayName("TitleProjectionMultipliers")]
        [LocalizedCategory("TitleMain")]
        [PropertyOrder(1, 1)]
        public string SeriesHumanReadable
        {
            get
            {
                return series.Count == 0 ? "" : series.ToStringUniform(" ");
            }
            set
            {
                series.Clear();
                if (!string.IsNullOrEmpty(value))
                    series = value.ToDecimalArrayUniform().ToList();
                // пересчитать проекции
                ownerSeries.MakeProjections();
            }
        }

        [DisplayName("Magic")]
        [LocalizedCategory("TitleMain")]
        public int Magic { get; set; }

        public bool Completed
        {
            get { return points.Count == 2; }
        }

        public bool Selected { get; set; }

        private Color color = Color.DarkGray;
        /// <summary>
        /// цвет линии
        /// </summary>        
        [LocalizedDisplayName("TitleColor")]
        [LocalizedCategory("TitleVisuals")]
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }
        
        public FiboSpan(SeriesFiboSpan ownerSeries)
        {
            this.ownerSeries = ownerSeries;
            Name = string.Format("{0} {1}", ClassName, nextFiboSpanNum++);
        }

        public void AddPoint(double index, double price)
        {
            if (points.Count < 2)
                points.Add(new PointD(index, price));
        }
        
        /// <summary>
        /// получить список индексов-расширений
        /// </summary>        
        public List<int> GetProjections()
        {
            var lst = new List<int>();
            if (!Completed) return lst;

            var endIndex = (int)Math.Max(points[1].X, points[0].X);
            var len = (int)Math.Abs(points[1].X - points[0].X);
            foreach (var level in series)
            {
                lst.Add(endIndex + (int)(len * level));
            }
            return lst;
        }

        #region IChartInteractiveObject

        [LocalizedDisplayName("TitleName")]
        [LocalizedCategory("TitleMain")]
        public string Name
        {
            get; set;
        }

        public DateTime? DateStart
        {
            get { return null; }
            set { }
        }

        public int IndexStart
        {
            get
            {
                return points.Count == 0 ? 0 : (int) points[0].X;
            }
        }

        public void SaveInXML(XmlElement parentNode, CandleChartControl owner)
        {
            var node = parentNode.AppendChild(parentNode.OwnerDocument.CreateElement("FiboSpan"));
            
            var nameAttr = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("name"));
            nameAttr.Value = Name;

            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("magic")).Value = Magic.ToString();

            var seriesAttr = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("series"));
            seriesAttr.Value = SeriesHumanReadable;

            var colorAttr = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("color"));
            colorAttr.Value = Color.ToArgb().ToString();

            if (points.Count > 0)
            {
                var time = owner.chart.StockSeries.GetCandleOpenTimeByIndex((int) points[0].X);
                var axAttr = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("firstTimeStamp"));
                axAttr.Value = time.ToString("ddMMyyyy HHmmss", CultureProvider.Common);
                var ayAttr = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("firstY"));
                ayAttr.Value = points[0].Y.ToString(CultureProvider.Common);
            }

            if (points.Count > 1)
            {
                var time = owner.chart.StockSeries.GetCandleOpenTimeByIndex((int)points[1].X);
                var axAttr = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("secondTimeStamp"));
                axAttr.Value = time.ToString("ddMMyyyy HHmmss", CultureProvider.Common);
                var ayAttr = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("secondY"));
                ayAttr.Value = points[1].Y.ToString(CultureProvider.Common);
            }
        }

        public void LoadFromXML(XmlElement itemNode, CandleChartControl owner)
        {            
            if (itemNode.Attributes["name"] != null) Name = itemNode.Attributes["name"].Value;
            if (itemNode.Attributes["magic"] != null) Magic = itemNode.Attributes["magic"].Value.ToIntSafe() ?? 0;
            if (itemNode.Attributes["series"] != null)
                if (!string.IsNullOrEmpty(itemNode.Attributes["series"].Value))
                    Series = itemNode.Attributes["series"].Value.ToDecimalArrayUniform().ToList();
            if (itemNode.Attributes["color"] != null) 
                Color = Color.FromArgb(int.Parse(itemNode.Attributes["color"].Value));
            if (itemNode.Attributes["firstTimeStamp"] != null)
            {
                var timeStr = itemNode.Attributes["firstTimeStamp"].Value;
                var time = DateTime.ParseExact(timeStr, "ddMMyyyy HHmmss", CultureProvider.Common);
                var index = owner.chart.StockSeries.GetIndexByCandleOpen(time);
                points.Add(new PointD(index, double.Parse(itemNode.Attributes["firstY"].Value, 
                    CultureProvider.Common)));
            }
            if (itemNode.Attributes["secondTimeStamp"] != null)
            {
                var timeStr = itemNode.Attributes["secondTimeStamp"].Value;
                var time = DateTime.ParseExact(timeStr, "ddMMyyyy HHmmss", CultureProvider.Common);
                var index = owner.chart.StockSeries.GetIndexByCandleOpen(time);
                points.Add(new PointD(index, double.Parse(itemNode.Attributes["secondY"].Value, 
                    CultureProvider.Common)));
            }
        }

        public ChartObjectMarker IsInMarker(int screenX, int screenY, Keys modifierKeys)
        {
            return null;
        }

        public void OnMarkerMoved(ChartObjectMarker marker)
        {
            throw new NotImplementedException();
        }

        public void AjustColorScheme(CandleChartControl chart)
        {
            var clBack = chart.chart.visualSettings.ChartBackColor;
            Color = ChartControl.ChartVisualSettings.AdjustColor(Color, clBack);
        }

        public Image CreateSample(Size sizeHint)
        {
            if (points.Count < 2)
                return null;

            // готовим данные для миниатюры
            var rangeX = (int) Math.Round(points[1].X - points[0].X);
            if (rangeX < 1)
                rangeX = 1;
            var projections = new[]
                {
                    new FiboSpanProjection
                        {
                            AckDegree = 1,
                            Index = (int) (points[1].X + 0.5) + rangeX
                        },
                    new FiboSpanProjection
                        {
                            AckDegree = 2,
                            Index = (int) (points[1].X + 0.5) + rangeX * 2
                        },
                    new FiboSpanProjection
                        {
                            AckDegree = 3,
                            Index = (int) (points[1].X + 0.5) + rangeX * 3
                        }
                };

            // рисуем
            var minX = points.Select(p => p.X).Min();
            var minY = points.Select(p => p.Y).Min();
            var maxX = points.Select(p => p.X).Max();
            var maxY = points.Select(p => p.Y).Max();
            foreach (var projection in projections)
                if (projection.Index > maxX)
                    maxX = projection.Index;
            rangeX = (int)(maxX - minX);
            var rangeY = maxY - minY;
            minX-= rangeX / 10;
            maxX += rangeX / 10;
            minY -= rangeY / 10;
            maxY += rangeY / 10;
            var worldRect = new RectangleD(minX, minY, maxX - minX, maxY - minY);
            var brushStorage = new BrushesStorage();
            var brush = brushStorage.GetBrush(Color.White);
            var penStorage = new PenStorage();
            var canvasRect = new Rectangle(new Point(0, 0), sizeHint);
            var result = new Bitmap(sizeHint.Width, sizeHint.Height);
            var g = Graphics.FromImage(result);
            ownerSeries.DrawSpan(g, worldRect, canvasRect, this, brush, penStorage);
            foreach (var projection in projections)
            {
                ownerSeries.DrawProjection(g, worldRect, canvasRect, projection, brush,
                                           new Font(FontFamily.GenericSansSerif, 8), penStorage, brushStorage);
            }
            return result;
        }

        #endregion
    }

    /// <summary>
    /// проекция роста цены
    /// </summary>
    public class FiboSpanProjection
    {
        /// <summary>
        /// индекс бара
        /// </summary>
        public int Index { get; set; }

        private int ackDegree = 1;
        /// <summary>
        /// степень подтверждения
        /// </summary>
        public int AckDegree
        {
            get { return ackDegree; }
            set { ackDegree = value; }
        }

        /// <summary>
        /// верхняя отметка, до которой рисуется линия (до low бара, если он есть)
        /// </summary>
        public float? UpperPrice { get; set; }

        private Color color = Color.DarkSalmon;
        /// <summary>
        /// цвет линии
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        /// <summary>
        /// используется при построении проекций
        /// </summary>
        public bool Filtered { get; set; }
    }
}

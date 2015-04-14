using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Candlechart.ChartGraphics;
using Candlechart.ChartMath;
using Entity;

namespace Candlechart.Core
{
    /// <summary>
    /// векторный графический объект,
    /// задается набором точек
    /// масштабируется до нужных геометрических размеров и разворачивается на заданный угол
    /// </summary>
    public class VectorGraphObject
    {
        private readonly List<PointArray> lines = new List<PointArray>();        
        private PointF pivotPoint;

        private float width, height;

        public float Width
        {
            get { return width; }
        }

        public float Height
        {
            get { return height; }
        }


        private int alpha = 255;
        /// <summary>
        /// канал прозрачности - 255-непрозрачный
        /// </summary>
        public int Alpha
        {
            get { return alpha; }
            set { alpha = value; }
        }        
        
        protected VectorGraphObject()
        {            
        }

        public VectorGraphObject(PointF _pivotPoint, List<Polygone> _polygones, List<Polyline> _polylines)
        {
            pivotPoint = _pivotPoint;
            lines = new List<PointArray>();
            foreach (var poly in _polygones) lines.Add(poly);
            foreach (var poly in _polylines) lines.Add(poly);
            // определить размеры
            float minX = lines.Min(p => p.MinX), maxX = lines.Max(p => p.MaxX);
            float minY = lines.Min(p => p.MinY), maxY = lines.Max(p => p.MaxY);
            width = maxX - minX;
            height = maxY - minY;
        }

        public VectorGraphObject(PointF _pivotPoint, List<PointArray> _lines)
        {
            pivotPoint = _pivotPoint;
            lines = _lines;            
            // определить размеры
            float minX = lines.Min(p => p.MinX), maxX = lines.Max(p => p.MaxX);
            float minY = lines.Min(p => p.MinY), maxY = lines.Max(p => p.MaxY);
            width = maxX - minX;
            height = maxY - minY;
        }

        public VectorGraphObject Copy()
        {
            var vo = new VectorGraphObject {pivotPoint = pivotPoint, alpha = alpha, width = width, height = height};
            foreach (var poly in lines)
            {
                vo.lines.Add(poly.Copy());
            }
            return vo;
        }

        /// <summary>
        /// изменить цвета всех линий на переданный
        /// </summary>        
        public void PaintLines(Color cl)
        {
            foreach (Polyline poly in lines) poly.LineColor = cl;
        }

        /// <summary>
        /// изменить цвета всех заливок на переданный
        /// </summary>        
        public void PaintFills(Color cl)
        {
            foreach (Polyline poly in lines)
                if (poly is Polygone) ((Polygone)poly).FillColor = cl;
        }

        /// <summary>
        /// повернуть вокруг pivotPoint
        /// </summary>
        /// <param name="angle">в радианах</param>
        public void Rotate(float angle)
        {
            foreach (var line in lines)
            {
                for (var i = 0; i < line.points.Length; i++)
                {
                    var p = line.points[i];
                    var x = p.X - pivotPoint.X;
                    var y = p.Y - pivotPoint.Y;
                    line.points[i].X = (float)(x * Math.Cos(angle) - y * Math.Sin(angle) + pivotPoint.X);
                    line.points[i].Y = (float)(x * Math.Sin(angle) + y * Math.Cos(angle) + pivotPoint.Y);
                }
            }
        }

        /// <summary>
        /// перенести по вектору
        /// </summary>
        /// <param name="vector">вектор перемещения</param>
        public void Move(PointF vector)
        {
            foreach (var line in lines)
            {
                for (var i = 0; i < line.points.Length; i++)
                {                    
                    line.points[i].X += vector.X;
                    line.points[i].Y += vector.Y;                    
                }
            }
            pivotPoint.X += vector.X;
            pivotPoint.Y += vector.Y;                    
        }

        /// <summary>
        /// перенести в точку
        /// </summary>
        /// <param name="dest">конечная точка</param>
        public void Move2Point(PointF dest)
        {
            Move(new PointF(dest.X - pivotPoint.X, dest.Y - pivotPoint.Y));            
        }

        /// <summary>
        /// масштабировать
        /// </summary>        
        public void Scale(float kX, float kY)
        {
            foreach (var line in lines)
            {
                for (var i = 0; i < line.points.Length; i++)
                {                    
                    line.points[i].X *= kX;
                    line.points[i].Y *= kY;
                }
            }
            pivotPoint.X *= kX;
            pivotPoint.Y *= kY; 
        }

        public void Draw(Graphics g)
        {
            foreach (Polyline poly in lines)
            {
                if (poly.GetType() == typeof(Polygone))
                {
                    g.FillPolygon(new SolidBrush(Color.FromArgb(alpha, ((Polygone) poly).FillColor)),
                                  poly.points);
                    g.DrawPolygon(new Pen(poly.LineColor), poly.points);
                }
                else
                    g.DrawLines(new Pen(poly.LineColor), poly.points);
            }
        }

        public void Draw(Graphics g, PenStorage penDic, float penWidth)
        {
            foreach (Polyline poly in lines)
            {
                if (poly.GetType() == typeof(Polygone))
                {
                    g.FillPolygon(new SolidBrush(Color.FromArgb(alpha, ((Polygone)poly).FillColor)),
                                  poly.points);
                    g.DrawPolygon(penDic.GetPen(poly.LineColor, penWidth), poly.points);
                }
                else
                    g.DrawLines(penDic.GetPen(poly.LineColor, penWidth), poly.points);
            }
        }

        public void Draw(Graphics g, PenStorage penDic)
        {
            Draw(g, penDic, 1);
        }

        public bool IsPointIn(Point pt, float tolerance)
        {
            var ptF = new PointF(pt.X, pt.Y);
            foreach (var poly in lines)
            {
                if (poly is Polygone)
                {
                    if (Geometry.IsInPolygon(ptF, poly.points))
                        return true;
                    continue;
                }
                
                for (var i = 0; i < poly.points.Length - 1; i++)
                {
                    if (Geometry.IsDotOnSpan(ptF, poly.points[i], poly.points[i + 1], tolerance))
                        return true;
                }
            }
            return false;
        }
    }

    public abstract class PointArray
    {
        public PointF[] points;
        public float MinX
        {
            get
            {
                return points.Min(p => p.X);                
            }
        }
        public float MaxX
        {
            get
            {
                return points.Max(p => p.X);
            }
        }
        public float MinY
        {
            get
            {
                return points.Min(p => p.Y);
            }
        }
        public float MaxY
        {
            get
            {
                return points.Max(p => p.Y);
            }
        }        

        public abstract PointArray Copy();
    }

    public class Polyline : PointArray
    {
        private Color lineColor = Color.Black;
        public Color LineColor
        {
            get { return lineColor; }
            set { lineColor = value; }
        }
        public override PointArray Copy()
        {
            var pa = new Polyline { points = new PointF[points.Length], LineColor = LineColor };
            for (var i = 0; i < points.Length; i++)
                pa.points[i] = points[i];
            return pa;
        }
    }

    public class Polygone : Polyline
    {
        private Color fillColor = Color.White;
        public Color FillColor
        {
            get { return fillColor; }
            set { fillColor = value; }
        }
        public override PointArray Copy()
        {
            var pa = new Polygone { points = new PointF[points.Length], LineColor = LineColor, FillColor = FillColor };
            for (var i = 0; i < points.Length; i++)
                pa.points[i] = points[i];
            return pa;
        }
    }
}

using System;
using System.Drawing;
using TradeSharp.Util;

namespace Candlechart.ChartMath
{
    public static partial class Geometry
    {
        #region Длина отрезка
        public static double GetSpanLength(PointD a, PointD b)
        {
            return Math.Sqrt((a.X - b.X)*(a.X - b.X) + (a.Y - b.Y)*(a.Y - b.Y));
        }

        public static float GetSpanLength(PointF a, PointF b)
        {
            return (float)Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        public static double GetSpanLength(double x1, double y1, double x2, double y2)
        {
            return GetSpanLength(new PointD(x1, y1), new PointD(x2, y2));
        }
        #endregion

        #region Попадание в отрезок / прямую / область
        public static bool IsDotOnSpan(int x, int y, int x1, int y1, int x2, int y2, int maxDelta)
        {
            return IsDotOnSpan(new Point(x, y), new Point(x1, y1), new Point(x2, y2), maxDelta);
        }

        public static bool IsDotOnSpan(Point a, Point p1, Point p2, int maxDelta)
        {
            double left, right, top, bottom;
            if (p1.X > p2.X)
            {
                left = p2.X;
                right = p1.X;
            }
            else
            {
                left = p1.X;
                right = p2.X;
            }
            if (p1.Y > p2.Y)
            {
                bottom = p2.Y;
                top = p1.Y;
            }
            else
            {
                bottom = p1.Y;
                top = p2.Y;
            }
            if (a.X < (left - maxDelta) || a.X > (right + maxDelta) || (a.Y < bottom - maxDelta) || (a.Y > top + maxDelta)) return false;
            return IsDotOnLine(a, p1, p2, maxDelta);
        }

        public static bool IsDotOnSpan(PointD a, PointD p1, PointD p2, double maxDelta)
        {
            double left, right, top, bottom;
            if (p1.X > p2.X)
            {
                left = p2.X;
                right = p1.X;
            }
            else
            {
                left = p1.X;
                right = p2.X;
            }
            if (p1.Y > p2.Y)
            {
                bottom = p2.Y;
                top = p1.Y;
            }
            else
            {
                bottom = p1.Y;
                top = p2.Y;
            }
            if (a.X < (left - maxDelta) || a.X > (right + maxDelta) || (a.Y < bottom - maxDelta) || (a.Y > top + maxDelta)) return false;
            return IsDotOnLine(a, p1, p2, maxDelta);
        }

        public static bool IsDotOnSpan(PointF a, PointF p1, PointF p2, float maxDelta)
        {
            double left, right, top, bottom;
            if (p1.X > p2.X)
            {
                left = p2.X;
                right = p1.X;
            }
            else
            {
                left = p1.X;
                right = p2.X;
            }
            if (p1.Y > p2.Y)
            {
                bottom = p2.Y;
                top = p1.Y;
            }
            else
            {
                bottom = p1.Y;
                top = p2.Y;
            }
            if (a.X < (left - maxDelta) || a.X > (right + maxDelta) || (a.Y < bottom - maxDelta) || (a.Y > top + maxDelta)) return false;
            return IsDotOnLine(a, p1, p2, maxDelta);
        }

        public static bool IsDotOnLine(Point p, Point lineA, Point lineB, int maxDelta)
        {
            if (lineA.X == lineB.X && lineA.Y == lineB.Y)
                return p.X == lineA.X && p.Y == lineB.Y;

            var dx = lineB.X - lineA.X;
            var dy = lineA.Y - lineB.Y;
            var len = (float)Math.Sqrt(dx * dx + dy * dy);
            float dist = dy * p.X + dx * p.Y + (lineA.X * lineB.Y - lineB.X * lineA.Y);
            dist /= len;

            return Math.Abs(dist) <= maxDelta;
        }

        public static bool IsDotOnLine(PointF p, PointF lineA, PointF lineB, float maxDelta)
        {
            if (lineA.X == lineB.X && lineA.Y == lineB.Y)
                return p.X == lineA.X && p.Y == lineB.Y;

            var dx = lineB.X - lineA.X;
            var dy = lineA.Y - lineB.Y;
            var len = (float)Math.Sqrt(dx * dx + dy * dy);
            var dist = dy * p.X + dx * p.Y + (lineA.X * lineB.Y - lineB.X * lineA.Y);
            dist /= len;

            return Math.Abs(dist) <= maxDelta;
        }

        public static bool IsDotOnLine(PointD p, PointD lineA, PointD lineB, double maxDelta)
        {
            if (lineA.X == lineB.X && lineA.Y == lineB.Y)
                return p.X == lineA.X && p.Y == lineB.Y;

            var dx = lineB.X - lineA.X;
            var dy = lineA.Y - lineB.Y;
            var len = Math.Sqrt(dx*dx + dy*dy);
            var dist = dy*p.X + dx*p.Y + (lineA.X*lineB.Y - lineB.X*lineA.Y);
            dist /= len;

            return Math.Abs(dist) <= maxDelta;
        }

        public static bool IsDotInArea(PointD p, PointD a, PointD b, double tolerance)
        {
            if (a.X <= b.X)
            {
                if (a.X - p.X > tolerance) return false;
                if (p.X - b.X > tolerance) return false;
            }
            else
            {
                if (b.X - p.X > tolerance) return false;
                if (p.X - a.X > tolerance) return false;
            }

            if (a.Y <= b.Y)
            {
                if (a.Y - p.Y > tolerance) return false;
                if (p.Y - b.Y > tolerance) return false;
            }
            else
            {
                if (b.Y - p.Y > tolerance) return false;
                if (p.Y - a.Y > tolerance) return false;
            }
            return true;
        }
        #endregion

        #region Проекция точки на линию
        public static PointF GetProjectionPointOnLine(PointF target, PointF a, PointF b)
        {
            // отдельно рассматриваются вертикальная и горизональная линии
            if (a.X.RoughCompares(b.X, 0.5f)) return new PointF(a.X, target.Y);
            if (a.Y.RoughCompares(b.Y, 0.5f)) return new PointF(target.X, a.Y);

            var kB = (a.X - b.X) / (b.Y - a.Y);
            var kC = a.X + kB * a.Y;

            var x = (kC - kB * target.Y + kB * kB * target.X) / (kB * kB + 1f);
            var y = (kC - x) / kB;

            return new PointF(x, y);
        }
        #endregion

        #region Афинные преобразования
        /// <summary>
        /// повернуть точку на угол относительно начала координат
        /// </summary>        
        public static PointF RotatePoint(PointF point, double angle)
        {
            return new PointF
                       {
                           X = ((float)(point.X * Math.Cos(angle) - point.Y * Math.Sin(angle))),
                           Y = ((float)(point.X * Math.Sin(angle) + point.Y * Math.Cos(angle)))
                       };
        }

        public static PointD RotatePoint(PointD point, double angle)
        {
            return new PointD
                       {
                           X = ((point.X * Math.Cos(angle) - point.Y * Math.Sin(angle))),
                           Y = ((point.X * Math.Sin(angle) + point.Y * Math.Cos(angle)))
                       };
        }
        
        /// <summary>
        /// повернуть точку на угол относительно центра
        /// </summary>        
        public static PointF RotatePoint(PointF point, PointF center, double angle)
        {
            var x = point.X - center.X;
            var y = point.Y - center.Y;
            return new PointF
                       {
                           X = ((float)(x * Math.Cos(angle) - y * Math.Sin(angle))) + center.X,
                           Y = ((float)(x * Math.Sin(angle) + y * Math.Cos(angle))) + center.Y
                       };
        }

        public static PointD RotatePoint(PointD point, PointD center, double angle)
        {
            var x = point.X - center.X;
            var y = point.Y - center.Y;
            return new PointD
                       {
                           X = ((x * Math.Cos(angle) - y * Math.Sin(angle))) + center.X,
                           Y = ((x * Math.Sin(angle) + y * Math.Cos(angle))) + center.Y
                       };
        }
        #endregion

        #region Эллипсы
        /// <summary>
        /// получить параметры эллипса, заданного
        /// полуосью (a, b) и третьей точкой (c)
        /// </summary>
        /// <param name="a">одна точка полуоси</param>
        /// <param name="b">вторая точка полуоси</param>
        /// <param name="c">точка на эллипсе</param>
        /// <param name="alpha">выходной - угол поворота эллипса</param>
        /// <param name="A">полуось эллипса</param>
        /// <param name="B">полуось эллипса</param>
        /// <param name="cx">центр эллипса</param>
        /// <param name="cy">центр эллипса</param>
        public static bool GetEllipseParams(PointF a, PointF b, PointF c, out double alpha,
                                            out float A, out float B, out float cx, out float cy)
        {
            // перенести СК в центр
            cx = (a.X + b.X) * 0.5F;
            cy = (a.Y + b.Y) * 0.5F;
            a.X -= cx;
            a.Y -= cy;
            b.X -= cx;
            b.Y -= cy;
            c.X -= cx;
            c.Y -= cy;
            // развернуть СК
            alpha = Math.Atan2(b.Y - a.Y, b.X - a.X);
            a = RotatePoint(a, -alpha);
            //b = RotatePoint(b, -alpha);
            c = RotatePoint(c, -alpha);
            A = Math.Abs(a.X);
            // получить вторую полуось
            if (A == 0)
            {
                B = float.NaN;
                return false;
            }
            var discr = 1F - c.X * c.X / (A * A);
            if (discr <= 0)
            {
                B = float.NaN;
                return false;
            }
            B = (float)Math.Sqrt(c.Y * c.Y / discr);
            alpha = -alpha;
            return true;
        }

        /// <summary>
        /// получить массив точек кривых Безье, аппрокс. эллипс
        /// </summary>
        /// <param name="alpha">угол поворота (рад)</param>
        /// <param name="a">большая полуось</param>
        /// <param name="b">малая полуось</param>
        /// <param name="cx">центер (X)</param>
        /// <param name="cy">центр (Y)</param>
        /// <returns>массив точек кривых Безье эллипса (для Graphics.DrawBeziers)</returns>
        public static PointF[] GetEllipseBezierPoints(double alpha, float a, float b, float cx, float cy)
        {
            const float MP = 0.55228475F;
            var ca = (float)Math.Cos(alpha);
            var sa = (float)Math.Sin(alpha);
            var aca = a * ca;
            var asa = a * sa;
            var bca = b * ca;
            var bsa = b * sa;
            var cx2 = 2 * cx;
            var cy2 = 2 * cy;

            var bezPoints = new PointF[13];
            bezPoints[0] = TransformPoint(1, 0, cx, cy, aca, bca, asa, bsa);
            bezPoints[1] = TransformPoint(1, MP, cx, cy, aca, bca, asa, bsa);
            bezPoints[2] = TransformPoint(MP, 1, cx, cy, aca, bca, asa, bsa);
            bezPoints[3] = TransformPoint(0, 1, cx, cy, aca, bca, asa, bsa);
            bezPoints[4] = TransformPoint(-MP, 1, cx, cy, aca, bca, asa, bsa);
            bezPoints[5] = TransformPoint(-1, MP, cx, cy, aca, bca, asa, bsa);
            for (var i = 0; i <= 5; i++)
            {
                bezPoints[i + 6] = new PointF(cx2 - bezPoints[i].X, cy2 - bezPoints[i].Y);
            }
            bezPoints[12] = bezPoints[0];
            return bezPoints;
        }

        private static PointF TransformPoint(float x, float y, float cx, float cy,
                                             float aca, float bca, float asa, float bsa)
        {
            var rx = cx + x * aca + y * bsa;
            var ry = cy - x * asa + y * bca;
            return new PointF(rx, ry);
        }
        #endregion

        #region Полигоны
        public static bool IsInPolygon(Point p, Point[] poly)
        {
            Point p1, p2;
            if (poly.Length < 3) return false;
            var inside = false;
            
            var oldPoint = new Point(poly[poly.Length - 1].X, poly[poly.Length - 1].Y);

            foreach (var t in poly)
            {
                var newPoint = new Point(t.X, t.Y);
                if (newPoint.X > oldPoint.X)
                {
                    p1 = oldPoint;
                    p2 = newPoint;
                }
                else
                {
                    p1 = newPoint;
                    p2 = oldPoint;
                }

                if ((newPoint.X < p.X) == (p.X <= oldPoint.X)
                    && (p.Y - (long)p1.Y) * (p2.X - p1.X)
                    < (p2.Y - (long)p1.Y) * (p.X - p1.X))                
                    inside = !inside;
                
                oldPoint = newPoint;
            }

            return inside;
        }

        public static bool IsInPolygon(PointF p, PointF[] poly)
        {
            PointF p1, p2;
            if (poly.Length < 3) return false;
            var inside = false;

            var oldPoint = new PointF(poly[poly.Length - 1].X, poly[poly.Length - 1].Y);

            foreach (var t in poly)
            {
                var newPoint = new PointF(t.X, t.Y);
                if (newPoint.X > oldPoint.X)
                {
                    p1 = oldPoint;
                    p2 = newPoint;
                }
                else
                {
                    p1 = newPoint;
                    p2 = oldPoint;
                }

                if ((newPoint.X < p.X) == (p.X <= oldPoint.X)
                    && (p.Y - p1.Y) * (p2.X - p1.X) < (p2.Y - p1.Y) * (p.X - p1.X))
                    inside = !inside;

                oldPoint = newPoint;
            }

            return inside;
        }

        public static bool IsInPolygon(PointD p, PointD[] poly)
        {
            PointD p1, p2;
            if (poly.Length < 3) return false;
            var inside = false;

            var oldPoint = new PointD(poly[poly.Length - 1].X, poly[poly.Length - 1].Y);

            foreach (var t in poly)
            {
                var newPoint = new PointD(t.X, t.Y);
                if (newPoint.X > oldPoint.X)
                {
                    p1 = oldPoint;
                    p2 = newPoint;
                }
                else
                {
                    p1 = newPoint;
                    p2 = oldPoint;
                }

                if ((newPoint.X < p.X) == (p.X <= oldPoint.X)
                    && (p.Y - p1.Y) * (p2.X - p1.X) < (p2.Y - p1.Y) * (p.X - p1.X))
                    inside = !inside;

                oldPoint = newPoint;
            }

            return inside;
        }
        #endregion
    }

    /// <summary>
    /// прямоугольник, сторонами которого являются цены (верх-низ) и время (лево-право)
    /// </summary>
    public class TimePriceRect
    {
        public DateTime start, end;
        public decimal low, high;
        public bool IsDotInRect(DateTime time, decimal price)
        {
            if (start <= end)
            {
                if (time < start || time > end) return false;
            }
            else
            {
                if (time > start || time < end) return false;
            }

            if (low < high)
            {
                if (price < low || price > high) return false;
            }
            else
            {
                if (price > low || price < high) return false;
            }
            return true;
        }
    }
}
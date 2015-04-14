using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Candlechart.Core;

namespace Candlechart.ChartMath
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PointD
    {
        public double X, Y;
        public static readonly PointD Empty;
        public static readonly PointD MaxValue = new PointD(double.MaxValue, double.MaxValue);

        public PointD(double _x, double _y) : this()
        {
            X = _x;
            Y = _y;
        }

        public PointF ToPointF()
        {
            return new PointF((float)X, (float)Y);
        }

        public Point ToPoint()
        {
            return new Point((int)X, (int)Y);
        }

        public Point Round()
        {
            return new Point((int)X, (int)Y);
        }

        public bool IsEmpty
        {
            get { return ((X == Empty.X) && (Y == Empty.Y)); }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PointD))
            {
                return false;
            }
            var td = (PointD)obj;
            return ((X == td.X) && (Y == td.Y));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return ("{X=" + X + ", Y=" + Y + "}");
        }

        public static Point Ceiling(PointD value)
        {
            return new Point((int)Math.Ceiling(value.X), (int)Math.Ceiling(value.Y));
        }

        public static Point Round(PointD value)
        {
            return new Point((int)Math.Round(value.X), (int)Math.Round(value.Y));
        }

        public static Point Truncate(PointD value)
        {
            return new Point((int)value.X, (int)value.Y);
        }

        public static PointD operator +(PointD point, SizeD size)
        {
            return new PointD(point.X + size.Width, point.Y + size.Height);
        }

        public static PointD operator -(PointD point, SizeD size)
        {
            return new PointD(point.X - size.Width, point.Y - size.Height);
        }

        public static bool operator ==(PointD point1, PointD point2)
        {
            return ((point1.X == point2.X) && (point1.Y == point2.Y));
        }

        public static bool operator !=(PointD point1, PointD point2)
        {
            return (point1 != point2);
        }

        public static implicit operator PointD(Point point)
        {
            return new PointD(point.X, point.Y);
        }

        public static implicit operator PointD(PointF point)
        {
            return new PointD(point.X, point.Y);
        }

        public static explicit operator PointF(PointD point)
        {
            return new PointF((float)point.X, (float)point.Y);
        }

        public static explicit operator SizeD(PointD point)
        {
            return new SizeD(point.X, point.Y);
        }

        #region operators
        public static PointD operator *(PointD p, double k)
        {
            p.X = p.X * k;
            p.Y = p.Y * k;
            return p;
        }

        public static PointD operator +(PointD a, PointD vector)
        {
            a.X = a.X + vector.X;
            a.Y = a.Y + vector.Y;
            return a;
        }

        public static PointD operator -(PointD a, PointD vector)
        {
            a.X = a.X - vector.X;
            a.Y = a.Y - vector.Y;
            return a;
        }
        #endregion
    }
}

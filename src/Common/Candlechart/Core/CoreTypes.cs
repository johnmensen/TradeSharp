using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Candlechart.ChartMath;

namespace Candlechart.Core
{
    public enum PriceField
    {
        Open,
        High,
        Low,
        Close
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RectangleD
    {
        private double _x;
        private double _y;
        private double _width;
        private double _height;
        public static readonly RectangleD Empty;

        public RectangleD(double x, double y, double width, double height)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }

        public RectangleD(PointD point, SizeD size)
        {
            _x = point.X;
            _y = point.Y;
            _width = size.Width;
            _height = size.Height;
        }

        public double X
        {
            get { return _x; }
            set { _x = value; }
        }

        public double Y
        {
            get { return _y; }
            set { _y = value; }
        }

        public double Width
        {
            get { return _width; }
            set { _width = value; }
        }

        public double Height
        {
            get { return _height; }
            set { _height = value; }
        }

        public double Left
        {
            get { return _x; }
        }

        public double Top
        {
            get { return _y; }
        }

        public double Right
        {
            get { return (_x + _width); }
        }

        public double Bottom
        {
            get { return (_y + _height); }
        }

        public SizeD Size
        {
            get { return new SizeD(_width, _height); }
            set
            {
                _width = value.Width;
                _height = value.Height;
            }
        }

        public PointD Location
        {
            get { return new PointD(_x, _y); }
            set
            {
                _x = value.X;
                _y = value.Y;
            }
        }

        public bool IsEmpty
        {
            get { return (((_x == 0.0) && (_y == 0.0)) && ((_width == 0.0) && (_height == 0.0))); }
        }

        public bool Contains(double x, double y)
        {
            if ((_x > x) || (x >= (_x + _width)))
            {
                return false;
            }
            return ((_y <= y) && (y < (_y + _height)));
        }

        public bool Contains(PointD point)
        {
            return Contains(point.X, point.Y);
        }

        public bool Contains(RectangleD rect)
        {
            if ((_x > rect.Left) || (rect.Right >= (_x + _width)))
            {
                return false;
            }
            return ((_y <= rect.Top) && (rect.Bottom < (_y + _height)));
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RectangleD))
            {
                return false;
            }
            var ed = (RectangleD) obj;
            return ((((_x == ed.X) && (_y == ed.Y)) && (_width == ed.Width)) && (_height == ed.Height));
        }

        public static RectangleD FromLTRB(double left, double top, double right, double bottom)
        {
            return new RectangleD(left, top, right - left, bottom - top);
        }

        public override int GetHashCode()
        {
            return (((((int) X) ^ ((((int) Y) << 13) | (((int) Y) >> 0x13))) ^
                     ((((int) Width) << 0x1a) | (((int) Width) >> 6))) ^
                    ((((int) Height) << 7) | (((int) Height) >> 0x19)));
        }

        public static RectangleD Inflate(RectangleD rect, double width, double height)
        {
            RectangleD ed = rect;
            ed.Inflate(width, height);
            return ed;
        }

        public void Inflate(double width, double height)
        {
            _x -= width;
            _y -= height;
            _width += width*2.0;
            _height += height*2.0;
        }

        public void Inflate(SizeD size)
        {
            Inflate(size.Width, size.Height);
        }

        public void Intersect(RectangleD rect)
        {
            RectangleD ed = Intersect(rect, this);
            _x = ed.X;
            _y = ed.Y;
            _width = ed.Width;
            _height = ed.Height;
        }

        public static RectangleD Intersect(RectangleD a, RectangleD b)
        {
            double x = (a.Left >= b.Left) ? a.Left : b.Left;
            double num3 = (a.Right >= b.Right) ? a.Right : b.Right;
            double y = (a.Top >= b.Top) ? a.Top : b.Top;
            double num4 = (a.Bottom >= b.Bottom) ? a.Bottom : b.Bottom;
            if ((x <= num3) && (y <= num4))
            {
                return new RectangleD(x, y, num3 - x, num4 - y);
            }
            return Empty;
        }

        public bool IntersectsWith(RectangleD rect)
        {
            if ((rect.X >= (_x + _width)) || (_x >= (rect.X + rect.Width)))
            {
                return false;
            }
            return ((rect.Y < (_y + _height)) && (_y < (rect.Y + rect.Height)));
        }

        public void Offset(double x, double y)
        {
            _x += x;
            _y += y;
        }

        public void Offset(PointD point)
        {
            _x += point.X;
            _y += point.Y;
        }

        public static RectangleD Union(RectangleD a, RectangleD b)
        {
            double x = (a.Left <= b.Left) ? a.Left : b.Left;
            double y = (a.Top <= b.Top) ? a.Top : b.Top;
            double num2 = (a.Right >= b.Right) ? a.Right : b.Right;
            double num4 = (a.Bottom >= b.Bottom) ? a.Bottom : b.Bottom;
            return new RectangleD(x, y, num2 - x, num4 - y);
        }

        public override string ToString()
        {
            return ("{X=" + X + ", Y=" + Y + ", Width=" + Width + ", Height=" + Height + "}");
        }

        public static bool operator ==(RectangleD rect1, RectangleD rect2)
        {
            return ((((rect1.X == rect2.X) && (rect1.Y == rect2.Y)) && (rect1.Width == rect2.Width)) &&
                    (rect1.Height == rect2.Height));
        }

        public static bool operator !=(RectangleD rect1, RectangleD rect2)
        {
            return (rect1 != rect2);
        }

        public static implicit operator RectangleD(Rectangle rect)
        {
            return new RectangleD(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static implicit operator RectangleD(RectangleF rect)
        {
            return new RectangleD(rect.X, rect.Y, rect.Width, rect.Height);
        }
    }        

    [StructLayout(LayoutKind.Sequential)]
    public struct SizeD
    {
        private double _width;
        private double _height;
        public static readonly SizeD Empty;
        public static readonly SizeD MaxValue;

        public SizeD(double width, double height)
        {
            _width = width;
            _height = height;
        }

        public SizeD(SizeD size)
        {
            _width = size.Width;
            _height = size.Height;
        }

        public double Width
        {
            get { return _width; }
            set { _width = value; }
        }

        public double Height
        {
            get { return _height; }
            set { _height = value; }
        }

        public bool IsEmpty
        {
            get { return ((_width == 0.0) && (_height == 0.0)); }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SizeD))
            {
                return false;
            }
            var ed = (SizeD) obj;
            return ((Width == ed.Width) && (Height == ed.Height));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return ("{Width=" + Width + ", Height=" + Height + "}");
        }

        public static Size Round(SizeD value)
        {
            return new Size((int) Math.Round(value.Width), (int) Math.Round(value.Height));
        }

        public static SizeD operator +(SizeD size1, SizeD size2)
        {
            return new SizeD(size1.Width + size2.Width, size1.Height + size2.Height);
        }

        public static SizeD operator -(SizeD size1, SizeD size2)
        {
            return new SizeD(size1.Width - size2.Width, size1.Height - size2.Height);
        }

        public static bool operator ==(SizeD size1, SizeD size2)
        {
            return ((size1.Width == size2.Width) && (size1.Height == size2.Height));
        }

        public static bool operator !=(SizeD size1, SizeD size2)
        {
            return (size1 != size2);
        }

        public static implicit operator SizeD(Size size)
        {
            return new SizeD(size.Width, size.Height);
        }

        public static implicit operator SizeD(SizeF size)
        {
            return new SizeD(size.Width, size.Height);
        }

        public static explicit operator SizeF(SizeD size)
        {
            return new SizeF((float) size.Width, (float) size.Height);
        }

        public static explicit operator PointD(SizeD size)
        {
            return new PointD(size.Width, size.Height);
        }

        static SizeD()
        {
            MaxValue = new SizeD(double.MaxValue, double.MaxValue);
        }
    }        
}
using System.Drawing;

namespace Candlechart.ChartMath
{
    public static partial class Geometry
    {
        public static PointF MultiplyScalar(this PointF a, float scale)
        {
            return new PointF(a.X*scale, a.Y*scale);
        }

        public static PointF MultiplyScalar(this PointF a, float scaleX, float scaleY)
        {
            return new PointF(a.X * scaleX, a.Y * scaleY);
        }

        public static PointD MultiplyScalar(this PointD a, float scale)
        {
            return new PointD(a.X * scale, a.Y * scale);
        }

        public static PointD MultiplyScalar(this PointD a, double scale)
        {
            return new PointD(a.X * scale, a.Y * scale);
        }

        public static PointD MultiplyScalar(this PointD a, double scaleX, double scaleY)
        {
            return new PointD(a.X * scaleX, a.Y * scaleY);
        }

        public static PointD MultiplyScalar(this PointD a, float scaleX, float scaleY)
        {
            return new PointD(a.X * scaleX, a.Y * scaleY);
        }

        public static PointF DivideScalar(this PointF a, float scale)
        {
            return new PointF(a.X / scale, a.Y / scale);
        }

        public static PointF DivideScalar(this PointF a, float scaleX, float scaleY)
        {
            return new PointF(a.X / scaleX, a.Y / scaleY);
        }

        public static PointD DivideScalar(this PointD a, float scale)
        {
            return new PointD(a.X / scale, a.Y / scale);
        }

        public static PointD DivideScalar(this PointD a, float scaleX, float scaleY)
        {
            return new PointD(a.X / scaleX, a.Y / scaleY);
        }
    }
}

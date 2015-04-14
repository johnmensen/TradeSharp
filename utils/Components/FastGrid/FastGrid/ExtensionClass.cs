using System.Drawing;

namespace FastGrid
{
    public static class ExtensionClass
    {
        public static Point GetCenter(this Rectangle rectangle)
        {
            return new Point(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2);
        }

        public static Rectangle MoveCenter(this Rectangle rectangle, Point point)
        {
            var center = rectangle.GetCenter();
            var x = rectangle.X + point.X - center.X;
            var y = rectangle.Y + point.Y - center.Y;
            return new Rectangle(x, y, rectangle.Width, rectangle.Height);
        }
    }
}

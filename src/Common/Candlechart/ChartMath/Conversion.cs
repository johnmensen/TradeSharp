using System;
using System.Drawing;
using Candlechart.Core;

namespace Candlechart.ChartMath
{
    internal static class Conversion
    {
        internal static Point ChildToParent(Point pt, Rectangle bounds)
        {
            return new Point(pt.X + bounds.Location.X, pt.Y + bounds.Location.Y);
        }

        internal static Rectangle ChildToParent(Rectangle rect, Rectangle bounds)
        {
            return new Rectangle(rect.X + bounds.Location.X, rect.Y + bounds.Location.Y, rect.Width, rect.Height);
        }

        internal static Point ParentToChild(Point pt, Rectangle bounds)
        {
            return new Point(pt.X - bounds.Location.X, pt.Y - bounds.Location.Y);
        }

        internal static Rectangle ParentToChild(Rectangle rect, Rectangle bounds)
        {
            return new Rectangle(rect.X - bounds.Location.X, rect.Y - bounds.Location.Y, rect.Width, rect.Height);
        }

        internal static PointD ScreenToWorld(PointD screenPt, RectangleD worldRect, RectangleD screenRect)
        {
            if ((Math.Abs(screenRect.Width) > double.Epsilon) && (Math.Abs(screenRect.Height) > double.Epsilon))
            {
                var td = new PointD(0.0, 0.0)
                             {
                                 X =
                                     ((((screenPt.X - screenRect.Left)/screenRect.Width)*worldRect.Width) +
                                      worldRect.Left),
                                 Y =
                                     ((((screenRect.Height - (screenPt.Y - screenRect.Top))/screenRect.Height)*
                                       worldRect.Height) +
                                      worldRect.Top)
                             };
                return td;
            }
            return PointD.Empty;
        }

        internal static SizeD ScreenToWorld(SizeD screenSize, RectangleD worldRect, RectangleD screenRect)
        {
            if ((Math.Abs(screenRect.Width) > double.Epsilon) && (Math.Abs(screenRect.Height) > double.Epsilon))
            {
                var ed = new SizeD(0.0, 0.0)
                             {
                                 Width = ((screenSize.Width/screenRect.Width)*worldRect.Width),
                                 Height = ((screenSize.Height/screenRect.Height)*worldRect.Height)
                             };
                return ed;
            }
            return SizeD.Empty;
        }

        internal static PointD WorldToScreen(PointD worldPt, RectangleD worldRect, RectangleD screenRect)
        {
            if ((Math.Abs(worldRect.Width) <= double.Epsilon) || (Math.Abs(worldRect.Height) <= double.Epsilon))            
                return worldPt;
            
            if (double.IsNegativeInfinity(worldPt.Y)) worldPt.Y = worldRect.Bottom - 1;
            else if (double.IsInfinity(worldPt.Y)) worldPt.Y = worldRect.Top + 1;            

            var td = new PointD
                         {
                             X = ((((worldPt.X - worldRect.Left)/worldRect.Width)*screenRect.Width) + screenRect.Left),
                             Y =
                                 ((screenRect.Height -
                                   (((worldPt.Y - worldRect.Top)/worldRect.Height)*screenRect.Height)) +
                                  screenRect.Top)
                         };
            const double maxDimx = 1048576.0;
            if (td.X > maxDimx) td.X = maxDimx;            
            if (td.X < -maxDimx) td.X = -maxDimx;
            if (td.Y > maxDimx) td.Y = maxDimx;
            if (td.Y < -maxDimx) td.Y = -maxDimx;
            return td;
        }

        internal static double GetSpanLenInScreenCoords(PointD worldPtA, PointD worldPtB,
            RectangleD worldRect, RectangleD canvasRect)
        {
            var scrA = WorldToScreen(new PointD(worldPtA.X, worldPtA.Y), worldRect, canvasRect);
            var scrB = WorldToScreen(new PointD(worldPtB.X, worldPtB.Y), worldRect, canvasRect);
            return Math.Sqrt((scrA.X - scrB.X) * (scrA.X - scrB.X) +
                                  (scrA.Y - scrB.Y) * (scrA.Y - scrB.Y));
        }

        internal static PointF WorldToScreen(PointF worldPt, RectangleD worldRect, RectangleD screenRect)
        {
            if ((Math.Abs(worldRect.Width) <= double.Epsilon) || (Math.Abs(worldRect.Height) <= double.Epsilon))
                return worldPt;

            if (float.IsNegativeInfinity(worldPt.Y)) worldPt.Y = (float)worldRect.Bottom - 1;
            else if (float.IsInfinity(worldPt.Y)) worldPt.Y = (float)worldRect.Top + 1;

            var td = new PointF
            {
                X = ((((worldPt.X - (float)worldRect.Left) / (float)worldRect.Width) * (float)screenRect.Width) + (float)screenRect.Left),
                Y =
                    (((float)screenRect.Height -
                      (((worldPt.Y - (float)worldRect.Top) / (float)worldRect.Height) * (float)screenRect.Height)) +
                     (float)screenRect.Top)
            };
            const float maxDimx = 1048576.0f;
            if (td.X > maxDimx) td.X = maxDimx;
            if (td.X < -maxDimx) td.X = -maxDimx;
            if (td.Y > maxDimx) td.Y = maxDimx;
            if (td.Y < -maxDimx) td.Y = -maxDimx;
            return td;
        }

        internal static PointF WorldToScreenF(PointD worldPt, RectangleD worldRect, RectangleD screenRect)
        {
            if ((Math.Abs(worldRect.Width) <= double.Epsilon) || (Math.Abs(worldRect.Height) <= double.Epsilon))
            {
                return worldPt.ToPointF();
            }
            if (double.IsNegativeInfinity(worldPt.Y)) worldPt.Y = worldRect.Bottom - 1;
            else if (double.IsInfinity(worldPt.Y)) worldPt.Y = worldRect.Top + 1;

            var td = new PointF
            {
                X = (float)((((worldPt.X - worldRect.Left) / worldRect.Width) * screenRect.Width) + screenRect.Left),
                Y = (float)
                    ((screenRect.Height -
                      (((worldPt.Y - worldRect.Top) / worldRect.Height) * screenRect.Height)) +
                     screenRect.Top)
            };
            const float maxDimx = 1048576f;
            if (td.X > maxDimx) td.X = maxDimx;
            if (td.X < -maxDimx) td.X = -maxDimx;
            if (td.Y > maxDimx) td.Y = maxDimx;
            if (td.Y < -maxDimx) td.Y = -maxDimx;
            return td;
        }

        internal static SizeD WorldToScreen(SizeD worldSize, RectangleD worldRect, RectangleD screenRect)
        {
            if ((Math.Abs(worldRect.Width) > double.Epsilon) && (Math.Abs(worldRect.Height) > double.Epsilon))
            {
                var ed = new SizeD(0.0, 0.0)
                             {
                                 Width = ((worldSize.Width/worldRect.Width)*screenRect.Width),
                                 Height = ((worldSize.Height/worldRect.Height)*screenRect.Height)
                             };
                return ed;
            }
            return worldSize;
        }
    }    
}

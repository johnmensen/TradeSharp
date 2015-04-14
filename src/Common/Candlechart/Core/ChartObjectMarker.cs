using System;
using System.Drawing;
using Candlechart.ChartMath;
using Entity;

namespace Candlechart.Core
{
    /// <summary>
    /// маленький прямоугольничек, за который объект можно
    /// подвинуть или растянуть
    /// </summary>
    public class ChartObjectMarker
    {
        /// <summary>
        /// размер маркера в пикселях
        /// </summary>
        public const int Size = 8;
        public const int Size2 = 4;

        /// <summary>
        /// центр маркера в координатах модели
        /// </summary>
        public PointD centerModel;

        /// <summary>
        /// центр маркера в координатах экрана
        /// </summary>
        public PointD centerScreen;

        public enum MarkerAction { Resize = 0, Move = 1 }

        /// <summary>
        /// маркер передвигает фигуру или растягивает ее
        /// </summary>
        public MarkerAction action;

        public void CalculateScreenCoords(RectangleD worldRect, Rectangle canvasRect)
        {
            centerScreen = Conversion.WorldToScreen(centerModel, worldRect, canvasRect);
        }

        public void RecalculateModelCoords(RectangleD worldRect, Rectangle canvasRect)
        {
            centerModel = Conversion.ScreenToWorld(centerScreen, worldRect, canvasRect);
        }
        
        public void Draw(Graphics g, PenStorage penStorage, BrushesStorage brushStorage)
        {
            var pen = penStorage.GetPen(Color.Black);
            var brush = brushStorage.GetBrush(Color.White);
            if (action == MarkerAction.Move)
            {
                var points = new[]
                                    {
                                        new Point((int) centerScreen.X - Size2, (int) centerScreen.Y),
                                        new Point((int) centerScreen.X, (int) centerScreen.Y - Size2),
                                        new Point((int) centerScreen.X + Size2, (int) centerScreen.Y),
                                        new Point((int) centerScreen.X, (int) centerScreen.Y + Size2)
                                    };
                g.FillPolygon(brush, points);
                g.DrawPolygon(pen, points);
                return;
            }
            g.FillRectangle(brush, (float)centerScreen.X - Size2, (float)centerScreen.Y - Size2, Size, Size);
            g.DrawRectangle(pen, (float) centerScreen.X - Size2, (float) centerScreen.Y - Size2, Size, Size);            
        }

        public bool IsIn(int x, int y)
        {
            return Math.Abs(centerScreen.X - x) <= Size2 &&
                   Math.Abs(centerScreen.Y - y) <= Size2;
        }

        public bool IsIn(int x, int y, RectangleD worldRect, Rectangle canvasRect)
        {
            CalculateScreenCoords(worldRect, canvasRect);
            return Math.Abs(centerScreen.X - x) <= Size2 &&
                   Math.Abs(centerScreen.Y - y) <= Size2;
        }
    }
}

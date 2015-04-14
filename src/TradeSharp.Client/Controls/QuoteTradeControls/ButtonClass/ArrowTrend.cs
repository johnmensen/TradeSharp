using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Candlechart.ChartMath;
using Entity;

namespace TradeSharp.Client.Controls.QuoteTradeControls.ButtonClass
{
    /// <summary>
    /// Класс инкапсулирует методы и свойства для отрисовки стрелочек тренда - красных и зелёных
    /// </summary>
    public class ArrowTrend
    {
        /// <summary>
        /// Положение стрелки на кнопке (не масштабированное)
        /// </summary>
        public PointF OriginalLocation { get; set; }

        /// <summary>
        /// Размер стрелки
        /// </summary>
        public Size ArrowSize { get; set; }

        /// <summary>
        /// Ссылка на хранилище кистей
        /// </summary>
        public BrushesStorage Brushes { get; set; }

        /// <summary>
        /// Ссылка на хранилище карандашей для обводки
        /// </summary>
        public PenStorage Pens { get; set; }

        /// <summary>
        /// Угол, на который следует развернуть стрелку
        /// </summary>
        public int DirectionAngle
        {
            set
            {
                directionAngle = value;
                
                var directionAngleRad = Math.PI * value / 14;
                if (directionAngleRad == 0)
                {
                    arrowBrush = Brushes.GetBrush(Color.Blue) as SolidBrush;
                    currentBorderPen = Pens.GetPen(Color.FromArgb(155, 55, 82, 222), 3);
                }
                else
                {
                    if (directionAngleRad < 0)
                    {
                        arrowBrush = Brushes.GetBrush(Color.FromArgb(255, 115, 197, 107)) as SolidBrush;
                        currentBorderPen = Pens.GetPen(Color.FromArgb(155, 135, 217, 127), 3);
                    }
                    else
                    {
                        arrowBrush = Brushes.GetBrush(Color.FromArgb(255, 239, 82, 72)) as SolidBrush;
                        currentBorderPen = Pens.GetPen(Color.FromArgb(155, 255, 102, 92), 3);
                    }
                }
            }
        }

        /// <summary>
        /// Масштаб
        /// </summary>
        public float Sx { get; set; }
        public float Sy { get; set; }

        /// <summary>
        /// хранит рассчитанные точки всех возможные положения стрелки
        /// </summary>
        private static readonly Dictionary<int, PointF[]> arrowStates;

        /// <summary>
        /// Точки стрелки в оригинальном виде и размере
        /// </summary>
        private static readonly PointF[] arrowOriginalPointArray = new[]
                                                           {  
                                                               new PointF(0, -2), 
                                                               new PointF(3, 0),
                                                               new PointF(0, 2),
                                                               new PointF(0.5f, 1),
                                                               new PointF(-3.2f, 1),
                                                               new PointF(-3.2f, -1),
                                                               new PointF(0.5f, -1)
                                                           };


        private SolidBrush arrowBrush = new SolidBrush(Color.FromArgb(255, 115, 197, 107));
        private Pen currentBorderPen = new Pen(Color.FromArgb(155, 135, 217, 127), 3);
        private int directionAngle;

        static ArrowTrend()
        {
            arrowStates = new Dictionary<int, PointF[]>();
            for (var i = -7; i <= 7; i++)
            {
                arrowStates.Add(i, arrowOriginalPointArray.Select(p => Geometry.RotatePoint(new PointF(p.X * 0.66f, p.Y * 0.66f),
                                                                                            Math.PI*i/14)).ToArray());
            }
        }

        public void Draw(Graphics graphic)
        {
            var points =
                arrowStates[directionAngle].Select(
                    p => new PointF((p.X + OriginalLocation.X) * Sx, (p.Y + OriginalLocation.Y) * Sy)).ToArray();

            graphic.FillPolygon(arrowBrush, points);
            graphic.DrawPolygon(currentBorderPen, points);
        }
    }
}

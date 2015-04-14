using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Entity;

namespace TradeSharp.Client.Controls.QuoteTradeControls.ButtonClass
{
    /// <summary>
    /// Инкапсулирует методы и свойства необходимые для прорисовки фигурной кнопки на канве контрола типа "QuoteTradeControl" 
    /// </summary>
    public class QuotePartButton
    {
        /// <summary>
        /// Отступ кнопки от верхнего края контрола "QuoteTradeControl"
        /// </summary>
        public int PaddingTopProportion
        {
            get { return paddingTopProportion; }
            set { paddingTopProportion = value; }
        }

        /// <summary>
        /// Отступ кнопки от верхнего левого/правого края контрола "QuoteTradeControl"
        /// </summary>
        public int PaddingLeftRightProportion
        {
            get { return paddingLeftRightProportion; }
            set { paddingLeftRightProportion = value; }
        }

        /// <summary>
        /// Нахадится ли указатель мыши над кнопкой
        /// </summary>
        public bool MouseAboveButton
        {
            get { return mouseAboveButton; }
            set
            {
                mouseAboveButton = value;
                if (mouseAboveButton)
                    currentBrush = mousePressed ? brushesStor.GetBrush(colorScheme[ButtonFigureColorScheme.Light]) as SolidBrush : brushesStor.GetBrush(colorScheme[ButtonFigureColorScheme.Pressed]) as SolidBrush;
                else
                    currentBrush = mousePressed ? brushesStor.GetBrush(colorScheme[ButtonFigureColorScheme.Light]) as SolidBrush : brushesStor.GetBrush(colorScheme[ButtonFigureColorScheme.Normal]) as SolidBrush;
            }
        }

        /// <summary>
        /// Зажата ли левая кнопка мыши
        /// </summary>
        public bool MousePressed
        {
            get { return mousePressed; }
            set
            {
                mousePressed = value;
                if (mousePressed)
                {
                    if (mouseAboveButton) currentBrush = brushesStor.GetBrush(colorScheme[ButtonFigureColorScheme.Light]) as SolidBrush;
                }
                else
                {
                    currentBrush = mouseAboveButton ? brushesStor.GetBrush(colorScheme[ButtonFigureColorScheme.Pressed]) as SolidBrush : brushesStor.GetBrush(colorScheme[ButtonFigureColorScheme.Normal]) as SolidBrush;
                }
            }
        }

        public string Title { get; set; }  //  Заголовок кнопки (BUY/SELL)
        public float? Value { get; set; }   //  Величина значения Bid или Ask для кнопки 
        public float ScaleX { get; set; }
        public float ScaleY { get; set; }
        public LinearGradientBrush ButtonBrush { get; set; }
        public GraphicsPath Shape { get; private set; }       // Точки фигуры кнопки

        // Оригинальные значения отступов от краёв
        private int paddingTopProportion = 7;
        private int paddingLeftRightProportion = 4;
        private const int PaddingTopBuySellIndicator = 2;
        private const int PaddingTopArrow = 12;
        private bool mouseAboveButton;
        private bool mousePressed;
        private Matrix scaleMatrix;
        private readonly bool isLeft;   // Левая кнопка (SELL) или правая кнопка (BUY). Не удалять
        private BrushesStorage brushesStor;
        private PenStorage pensStor;
        private SolidBrush currentBrush;
        private Dictionary<ButtonFigureColorScheme, Color> colorScheme = new Dictionary<ButtonFigureColorScheme, Color>();

        /// <summary>
        /// Значения разнастей предыдуших показаний. Нужно для отслеживания скорости изменения Ask или Bid
        /// </summary>
        private readonly RestrictedQueue<int> trandDeltaValues = new RestrictedQueue<int>(3) {0, 0, 0};

        /// <summary>
        /// Матрица смещения
        /// </summary>
        private readonly Matrix translateMatrix;

        /// <summary>
        /// индикатор цены текущей катировки
        /// </summary>
        private readonly BuySellIndicator buySellIndicator;

        /// <summary>
        /// Стрелка указывающая тренд
        /// </summary>
        private readonly ArrowTrend arrowTrend;

        public QuotePartButton(float scaleX, float scaleY, BrushesStorage brushes, PenStorage pens, bool isLeft = true)
        {
            brushesStor = brushes;
            pensStor = pens;
            colorScheme.Add(ButtonFigureColorScheme.Normal, Color.FromArgb(255, 180, 247, 180));
            colorScheme.Add(ButtonFigureColorScheme.Pressed, Color.FromArgb(255, 200, 247, 210));
            colorScheme.Add(ButtonFigureColorScheme.Light, Color.FromArgb(255, 160, 195, 180));

            currentBrush = brushesStor.GetBrush(colorScheme[ButtonFigureColorScheme.Normal]) as SolidBrush;

            ScaleX = scaleX;
            ScaleY = scaleY;
            this.isLeft = isLeft;
            translateMatrix = new Matrix();

            string buySellIndicatorHaderText;
            if (isLeft)
            { 
                translateMatrix.Translate(paddingLeftRightProportion / 2, PaddingTopProportion);
                buySellIndicatorHaderText = "Bid";
            }
            else
            {
                translateMatrix.Translate(QuotePartButtonFigure.OriginalWidth * 2 + paddingLeftRightProportion / 2, PaddingTopProportion);
                translateMatrix.Scale(-1, 1);
                buySellIndicatorHaderText = "Ask";
            }

            buySellIndicator = new BuySellIndicator
                                   {
                                       ScaleX = scaleX,
                                       ScaleY = scaleY,
                                       OriginalLocation = isLeft 
                                            ? 
                                            new Point(
                                                Convert.ToInt32(paddingLeftRightProportion),
                                                Convert.ToInt32(PaddingTopProportion + PaddingTopBuySellIndicator)) 
                                            : 
                                            new Point(
                                                Convert.ToInt32(QuotePartButtonFigure.OriginalWidth + paddingLeftRightProportion),
                                                Convert.ToInt32(PaddingTopProportion + PaddingTopBuySellIndicator)),
                                       HaderText = buySellIndicatorHaderText,
                                       Volume = null
                                   };


            arrowTrend = new ArrowTrend
                             {
                                 Sx = ScaleX,
                                 Sy = ScaleY, 
                                 Brushes = brushes,
                                 Pens = pens,
                                 OriginalLocation = isLeft ? new PointF(QuotePartButtonFigure.OriginalWidth - paddingLeftRightProportion - 2, PaddingTopArrow) :
                                                             new PointF(QuotePartButtonFigure.OriginalWidth * 2 - paddingLeftRightProportion - 2, PaddingTopArrow) 
                             };
        }

        /// <summary>
        /// Прорисовка кнопки на элементе "Graphics" контрола "QuoteTradeControl" 
        /// </summary>
        /// <param name="graphics">Объект, на котором будт осущесвлятся прорисовка</param>
        public void Draw(Graphics graphics)
        {
            scaleMatrix = new Matrix();
            scaleMatrix.Scale(ScaleX, ScaleY);
            Shape = QuotePartButtonFigure.instanceFigure.Clone() as GraphicsPath;
            Shape.Transform(translateMatrix);
            Shape.Transform(scaleMatrix);

            graphics.FillPath(currentBrush, Shape);
            graphics.DrawPath(pensStor.GetPen(Color.FromArgb(255, 64, 66, 64)), Shape);

            if (buySellIndicator != null)
            {
                if (Value != null)
                {
                    // Запоминаем значение для вычсления направления стрелки тренда
                    if (buySellIndicator.Volume == Value)
                        trandDeltaValues.Add(0);
                    else
                        trandDeltaValues.Add(buySellIndicator.Volume > Value ? -1 : 1);

                    buySellIndicator.Volume = Value;
                }
                
                buySellIndicator.ScaleX = ScaleX;
                buySellIndicator.ScaleY = ScaleY;
                
                buySellIndicator.Draw(graphics, brushesStor);
            }

            if (arrowTrend != null)
            {
                arrowTrend.Sx = ScaleX;
                arrowTrend.Sy = ScaleY;


                arrowTrend.DirectionAngle = trandDeltaValues.GetItemByIndex(0, true) * 1 + trandDeltaValues.GetItemByIndex(1, true) * 2 + trandDeltaValues.GetItemByIndex(2, true) * 4;
                
                arrowTrend.Draw(graphics);
            }
        }
    }
}
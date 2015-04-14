using System.Drawing;
using System.Drawing.Drawing2D;

namespace TradeSharp.Client.Controls.QuoteTradeControls.ButtonClass
{
    /// <summary>
    /// Генерирует точки фигурных кнопок (оригинального размера).
    /// </summary>
    static class QuotePartButtonFigure
    {
        const int CornerRadius = 4;
        const int IndicatorWidthProportion = 7;
        const int VolumeControlWidthProportion = 15; 

        const int IndicatorHeightProportion = 6;
        const int IndicatorPaddingProportion = 12;       // Расстояние от верхнего края контрола, до верхнего края иникатора спреда       
        const int VolumeControlHeightProportion = 7;
        const int VolumeControlPaddingProportion = 3;  // Расстояние от нижнего края иникатора спреда, до верхнего края выпадающего списка с объёмами торгов    

        /// <summary>
        /// Экземпляр рассчитанных точек фигуры кнопки 
        /// </summary>
        public static GraphicsPath instanceFigure;

        /// <summary>
        /// Оригинальная ширина. значение это свойства, по сути, может быть любым, но не меньше чем "VolumeControlWidthProportion"
        /// </summary>
        public static int OriginalWidth
        {
            get
            {
                return 31;
            }
        }

        /// <summary>
        /// Оригинальная высота
        /// </summary>
        public static int OriginalHeight
        {
            get
            {
                return IndicatorHeightProportion +
                       IndicatorPaddingProportion +
                       VolumeControlHeightProportion +
                       VolumeControlPaddingProportion;
            }
        }

        /// <summary>
        /// Оригинальный размер выемки, предназначенной для выпадающего списка с торговыми объёмами
        /// </summary>
        public static Size OriginalVolumeSize
        {
            get
            {
                return new Size(VolumeControlWidthProportion, VolumeControlHeightProportion);
            }
        }

        /// <summary>
        /// Оригинальный размер отступа выемки, предназначенной для выпадающего списка с торговыми объёмами, от краёв контрола
        /// </summary>
        public static Point OriginalVolumePadding
        {
            get
            {
                return new Point(OriginalWidth - VolumeControlWidthProportion, OriginalHeight - VolumeControlHeightProportion);
            }
        }

        /// <summary>
        /// Содержит кординаты выемки для индикатора текущего спреда (для удобства прорисовки)
        /// </summary>
        public static Point SpreadPosition { get; set; }

        /// <summary>
        /// Высота индикатора текущего спреда
        /// </summary>
        public static int SpreadHeight
        {
            get { return IndicatorHeightProportion; }
        }

        /// <summary>
        /// Ширина индикатора текущего спреда
        /// </summary>
        public static int SpreadWidth
        {
            get { return IndicatorWidthProportion * 2; }
        }
     
        static QuotePartButtonFigure()
        {
            instanceFigure = GetPath();
        }       

        /// <summary>
        /// расчёт точек фигуры кнопки
        /// </summary>
        /// <returns>экземпляр рассчитанных точек фигуры кнопки</returns>
        private static GraphicsPath GetPath()
        {
            var corLeftTop = new Rectangle(0, 0, CornerRadius, CornerRadius);
            
            var leftPartPath = new GraphicsPath();
            leftPartPath.StartFigure();
            leftPartPath.AddArc(corLeftTop, -90, -90);

            var lastPoint = AddPathLine(leftPartPath, new Point(0, CornerRadius), 0, CornerRadius);
            lastPoint = AddPathLine(leftPartPath, lastPoint, 0, 
                IndicatorPaddingProportion + IndicatorHeightProportion + VolumeControlPaddingProportion + VolumeControlHeightProportion);
            lastPoint = AddPathLine(leftPartPath, lastPoint, OriginalWidth - VolumeControlWidthProportion,
                IndicatorPaddingProportion + IndicatorHeightProportion + VolumeControlPaddingProportion + VolumeControlHeightProportion);
            lastPoint = AddPathLine(leftPartPath, lastPoint, OriginalWidth - VolumeControlWidthProportion,
                IndicatorPaddingProportion + IndicatorHeightProportion + VolumeControlPaddingProportion);
            lastPoint = AddPathLine(leftPartPath, lastPoint, OriginalWidth, IndicatorPaddingProportion + IndicatorHeightProportion + VolumeControlPaddingProportion);
            lastPoint = AddPathLine(leftPartPath, lastPoint, OriginalWidth, IndicatorPaddingProportion + IndicatorHeightProportion);

            lastPoint = AddPathLine(leftPartPath, lastPoint, OriginalWidth - IndicatorWidthProportion, IndicatorPaddingProportion + IndicatorHeightProportion);
            SpreadPosition = lastPoint = AddPathLine(leftPartPath, lastPoint, OriginalWidth - IndicatorWidthProportion, IndicatorPaddingProportion);
            
            lastPoint = AddPathLine(leftPartPath, lastPoint, OriginalWidth, IndicatorPaddingProportion);
            AddPathLine(leftPartPath, lastPoint, OriginalWidth, 0);  
            leftPartPath.CloseFigure();
            
            return leftPartPath;
        }

        /// <summary>
        /// Вспомогательный метод добавления новой линии в фигуру кнопки
        /// </summary>
        /// <param name="path">Путь фигуры , в который добавляется линия</param>
        /// <param name="lastPoint">Предыдущая точка - с неё начинается линия</param>
        /// <param name="newX">координата X следующей точки</param>
        /// <param name="newY">координата Y следующей точки</param>
        /// <returns>Конечная точка - с неё начинается следующая линия</returns>
        private static Point AddPathLine(GraphicsPath path, Point lastPoint, int newX, int newY)
        {
            var nextPoint = new Point {X = newX, Y = newY};

            path.AddLine(lastPoint, nextPoint);
            return nextPoint;
        }
    }
}
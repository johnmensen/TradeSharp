using Entity;

// ReSharper disable CheckNamespace
namespace TradeSharp.Robot.Robot
// ReSharper restore CheckNamespace
{
    // ReSharper disable UnusedAutoPropertyAccessor.Local
    // ReSharper disable InconsistentNaming
    /// <summary>
    /// В эту часть класса выносятся свойства, которые понадобятся для тестирования робота
    /// </summary>
    public partial class IchimokuRobot
    {
        /// <summary>
        /// Киджун основного тайм фрейма
        /// </summary>
        public double kijunMain { get; private set; }
        public int planning { get; private set; }

        public string ticker { get; private set; }
        public CandlePacker mainCandlePacker { get; private set; }
        public CandlePacker dayCandlePacker { get; private set; }
        public CandlePacker hourCandlePacker { get; private set; }

        /// <summary>
        /// Свеча основного тайм фрейма
        /// </summary>
        public CandleData candleMainTimeFrame;

        /// <summary>
        /// массив очередей (с вытеснением) из свечей для расчёта киджуна на двух вспомогательных тайм-фреймах (H4 и D1, например) 
        /// </summary>
        public RestrictedQueue<CandleData>[] subQueue { get; private set; }


        /// <summary>
        /// очередь с вытеснением из свечей для расчёта киджуна на основном тайм-фрейме (M30, например) 
        /// </summary>
        public RestrictedQueue<CandleData> shiftBackPoints { get; private set; }

        /// <summary>
        /// Произошедшие пересечения на последних М свечах
        /// </summary>
        public RestrictedQueue<bool> happenIntersection { get; private set; }

        /// <summary>
        /// Была ли открыта сделка на текущей свече
        /// </summary>
        /// <remarks>
        /// Эту переменную приходится вручную сбрасывать в "false" при каждом поступлении навой котировки (при каждом вызове "OnQuotesReceived") 
        /// </remarks>
        public bool OpenedDealOnCurrentCandle { get; private set; }
    }
    // ReSharper restore InconsistentNaming
    // ReSharper restore UnusedAutoPropertyAccessor.Local
}

using System.ComponentModel;

namespace TradeSharp.RobotFarm.Request
{
    public enum FarmState
    {
        [Description("Остановлена")]
        Stopped = 0,
        [Description("Запускается")]
        Starting = 1,
        [Description("Останавливается")]
        Stopping = 2,
        [Description("Запущена")]
        Started = 3,
    }
}

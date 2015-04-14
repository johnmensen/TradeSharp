using System.Collections.Generic;
using Entity;

namespace TradeSharp.Robot.Robot
{
    public class RobotMarkClear : RobotMark
    {
        public RobotMarkClear()
        {
        }

        public RobotMarkClear(string symbol, string timeframe, string code)
        {
            Symbol = symbol;
            Timeframe = timeframe;
            HintCode = code;
        }

        public RobotMarkClear(string symbol, BarSettings timeframe, string code)
        {
            Symbol = symbol;
            HintCode = code;
            Timeframe = BarSettingsStorage.Instance.GetBarSettingsFriendlyName(timeframe);
        }

        protected override void ParseString(Dictionary<string, string> dicValue)
        {
        }

        public override string ToString()
        {
            return base.ToString("clear");
        }
    }
}

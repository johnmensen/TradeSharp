using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TradeSharp.Robot.Robot
{
    /// <summary>
    /// отметка, оставляемая роботом на графике
    /// базовый класс, от него наследуют RobotHint и RobotMarkClear
    /// </summary>
    public abstract class RobotMark
    {
        public enum HintType
        {
            Коментарий = 0, Покупка, Продажа,
            Поджатие, Стоп, Тейк, Линия
        }

        public string HintCode { get; set; }
        public string Timeframe { get; set; }
        public string Symbol { get; set; }
        public HintType RobotHintType { get; set; }

        protected static readonly string[] formatDelimiter = new[] { "#;" };

        protected abstract void ParseString(Dictionary<string, string> dicValue);

        public static RobotMark ParseString(string formatedStr)
        {
            if (string.IsNullOrEmpty(formatedStr)) return null;
            var parts = formatedStr.Split(formatDelimiter, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4) return null;
            if (parts[0] != "fmt") return null;
            if (parts[1] != "type=hint" &&
                parts[1] != "type=clear") return null;

            var dic = parts.Select(part => part.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries)).Where(
                subParts => subParts.Length == 2).ToDictionary(subParts => subParts[0], subParts => subParts[1]);

            var mark = parts[1] == "type=hint" ? (RobotMark)(new RobotHint()) : new RobotMarkClear();
            mark.ParseString(dic);

            string dicValue;
            if (dic.TryGetValue("RobotHintType", out dicValue))
            {
                try
                {
                    mark.RobotHintType = (HintType)Enum.Parse(typeof(HintType), dicValue);
                }
                catch { }
            }
            if (dic.TryGetValue("Timeframe", out dicValue)) mark.Timeframe = dicValue;
            if (dic.TryGetValue("Symbol", out dicValue)) mark.Symbol = dicValue;
            if (dic.TryGetValue("Code", out dicValue)) mark.HintCode = dicValue;

            return mark;
        }

        public string ToString(string hintType)
        {
            var sb = new StringBuilder("#;fmt#;type=" + hintType + "#;RobotHintType=" + RobotHintType);
            if (!string.IsNullOrEmpty(Symbol)) sb.AppendFormat("#;Symbol={0}", Symbol);
            if (!string.IsNullOrEmpty(Timeframe)) sb.AppendFormat("#;Timeframe={0}", Timeframe);
            if (!string.IsNullOrEmpty(HintCode)) sb.AppendFormat("#;Code={0}", HintCode);
            return sb.ToString();
        }
    }
}

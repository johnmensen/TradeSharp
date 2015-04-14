using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using TradeSharp.Util;

namespace TradeSharp.Robot.Robot
{
    /// <summary>
    /// сообщение робота, отображаемое на графике в виде
    /// коментария (SeriesAsterisk)
    /// </summary>
    public class RobotHint : RobotMark
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public float? Price { get; set; }
        public DateTime? Time { get; set; }
        /// <summary>
        /// значок коментария
        /// </summary>
        public string Sign { get; set; }

        private int? Transparency { get; set; }
        private int? TransparencyText { get; set; }
        public Color? ColorLine { get; set; }
        public Color? ColorFill { get; set; }
        public Color? ColorText { get; set; }
        
        public RobotHint()
        {            
        }

        public RobotHint(string symbol, string timeframe, string text, string title, string sign, float price)
        {
            Timeframe = timeframe;
            Symbol = symbol;
            Text = text;
            Title = title;
            Sign = sign;
            Price = price;
        }

        protected override void ParseString(Dictionary<string, string> dic)
        {
            string dicValue;
            if (dic.TryGetValue("Title", out dicValue)) Title = dicValue;
            if (dic.TryGetValue("Text", out dicValue)) Text = dicValue;
            if (dic.TryGetValue("Sign", out dicValue)) Sign = dicValue;
            
            if (dic.TryGetValue("Price", out dicValue)) Price = dicValue.ToFloatUniformSafe();
            if (dic.TryGetValue("Time", out dicValue)) Time = dicValue.ToDateTimeUniformSafe();
            if (dic.TryGetValue("Transparency", out dicValue)) Transparency = dicValue.ToIntSafe();
            if (dic.TryGetValue("TransparencyText", out dicValue)) TransparencyText = dicValue.ToIntSafe();

            if (dic.TryGetValue("ColorLine", out dicValue)) ColorLine = Color.FromArgb(dicValue.ToInt());
            if (dic.TryGetValue("ColorFill", out dicValue)) ColorFill = Color.FromArgb(dicValue.ToInt());
            if (dic.TryGetValue("ColorText", out dicValue)) ColorText = Color.FromArgb(dicValue.ToInt());
        }

        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString("hint") + 
                string.Format("#;Title={0}#;Text={1}#;Sign={2}#;",
                Title, Text, Sign));

            if (Price.HasValue) sb.AppendFormat("#;Price={0}", Price.Value.ToStringUniform());
            if (Time.HasValue) sb.AppendFormat("#;Time={0}", Time.Value.ToStringUniform());
            if (Transparency.HasValue) sb.AppendFormat("#;Transparency={0}", Transparency.Value);
            if (TransparencyText.HasValue) sb.AppendFormat("#;TransparencyText={0}", TransparencyText.Value);
            if (ColorLine.HasValue) sb.AppendFormat("#;ColorLine={0}", ColorLine.Value.ToArgb());
            if (ColorFill.HasValue) sb.AppendFormat("#;ColorFill={0}", ColorFill.Value.ToArgb());
            if (ColorText.HasValue) sb.AppendFormat("#;ColorText={0}", ColorText.Value.ToArgb());
            return sb.ToString();
        }
    }
}

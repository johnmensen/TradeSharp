using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Candlechart;
using Candlechart.ChartMath;
using Candlechart.Series;
using Entity;
using TradeSharp.Robot.Robot;
using TradeSharp.Util;

namespace TradeSharp.Client.BL.Script
{
    [DisplayName("Состояние роботов")]
    public class ShowRobotSummaryScript : TerminalScript
    {
        public const string CommentSpecName = "ShowRobotSummary";

        public override bool CanBeTriggered
        {
            get { return true; }
        }

        private readonly ThreadSafeTimeStamp lastTimeCalled = new ThreadSafeTimeStamp();

        public ShowRobotSummaryScript()
        {
            ScriptTarget = TerminalScriptTarget.График;
            ScriptName = "Состояние роботов";
            Trigger = new ScriptTriggerNewQuote
            {
                quotesToCheck = new List<string>()
            };
        }
        
        public override string ActivateScript(CandleChartControl chart, PointD worldCoords)
        {
            var robots = MainForm.Instance.RobotFarm.GetRobotsAsIs().ToList();
            var commentText = GetCommentForChart(chart.Symbol, chart.Timeframe, robots);
            if (string.IsNullOrEmpty(commentText)) return "Нет роботов для " + chart.Symbol + " " + 
                BarSettingsStorage.Instance.GetBarSettingsFriendlyName(chart.Timeframe);

            // разместить комментарий на графике в указанной точке, удалить такой же, если был добавлен
            var comment = chart.seriesComment.data.FirstOrDefault(c => c.Name == CommentSpecName);
            if (comment != null)
            {
                chart.seriesComment.data.Remove(comment);
                chart.RedrawChartSafe();
                return string.Empty;
            }

            var colorFill = Color.LightGreen;
            var colorText = chart.chart.BackColor.GetBrightness() < 0.4f ? Color.White : Color.Black;

            comment = new ChartComment
            {
                FillTransparency = 80,
                ColorFill = colorFill,
                HideArrow = true,
                ArrowAngle = 90,
                ArrowLength = 1,
                PivotIndex = worldCoords.X,
                PivotPrice = worldCoords.Y,
                Owner = chart.seriesComment,
                Name = CommentSpecName,
                Text = commentText,
                ColorText = colorText,
                Color = colorText
            };
            chart.seriesComment.data.Add(comment);
            chart.RedrawChartSafe();
            return string.Empty;
        }

        public override string ActivateScript(string ticker)
        {
            throw new Exception("Неверный тип вызова скрипта \"ShowRobotSummary\"");
        }

        public override string ActivateScript(bool byTrigger)
        {
            if (!byTrigger)
                throw new Exception("Неверный тип вызова скрипта \"ShowRobotSummary\"");

            var nowTime = DateTime.Now;
            var lastHit = lastTimeCalled.GetLastHitIfHitted();
            if (lastHit.HasValue)
            {
                var deltaSeconds = (nowTime - lastHit.Value).TotalSeconds;
                if (deltaSeconds < 0.5) return "";
            }
            lastTimeCalled.Touch();

            var robots = MainForm.Instance.RobotFarm.GetRobotsAsIs().ToList();
            if (robots.Count == 0) return "";

            var charts = MainForm.Instance.GetChartList(true);
            foreach (var chart in charts)
            {
                var commentText = GetCommentForChart(chart.Symbol, chart.Timeframe, robots);
                if (string.IsNullOrEmpty(commentText)) continue;

                var comment = chart.seriesComment.data.FirstOrDefault(c => c.Name == CommentSpecName);
                if (comment != null)
                    comment.Text = commentText;
            }
            
            return "";
        }

        private string GetCommentForChart(string ticker, BarSettings timeframe, List<BaseRobot> robots)
        {
            var robotStates = robots.Where(r => r.Graphics.Any(g => g.a == ticker && g.b == timeframe)).Select(s =>
                "Робот \"" + s.GetUniqueName() + "\"\n" + s.ReportState());
            return string.Join("\n\n", robotStates);
        }
    }
}

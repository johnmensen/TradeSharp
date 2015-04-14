using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Candlechart;
using Candlechart.ChartMath;
using Entity;
using TradeSharp.Robot.Robot;

namespace TradeSharp.Client.BL.Script
{
    // ReSharper disable LocalizableElement
    [DisplayName("Цена для CS робота")]
    public class ScriptSetStopRobotPrice : TerminalScript
    {
        public ScriptSetStopRobotPrice()
        {
            ScriptTarget = TerminalScriptTarget.График;
            ScriptName = "Цена для CS робота";
        }

        public override string ActivateScript(CandleChartControl chart, PointD worldCoords)
        {
            if (MainForm.Instance.RobotFarm.State != RobotFarm.RobotFarmState.Stopped)
            {
                const string msg = "Роботы должны быть не активны на момент настройки";
                MessageBox.Show(msg);
                return msg;
            }

            // отыскать киборга
            var robots = MainForm.Instance.RobotFarm.GetRobotCopies();
            if (robots == null || robots.Count == 0) robots = RobotFarm.LoadRobots();
            if (robots == null) robots = new List<BaseRobot>();

            var fiboLevelRobots = robots.Where(r => r is StopRobot).Cast<StopRobot>();
            var targetRobots =
                fiboLevelRobots.Where(r => r.Graphics.Any(g => g.a == chart.Symbol && 
                    g.b.Equals(chart.Timeframe))).ToList();
            if (targetRobots.Count == 0)
            {
                var msg = string.Format("Роботов для {0} {1} не найдено", chart.Symbol,
                                        BarSettingsStorage.Instance.GetBarSettingsFriendlyName(chart.Timeframe));
                MessageBox.Show(msg);
                return msg;
            }

            // вызвать диалог
            var dlg = new SetupStopRobotDlg(targetRobots, worldCoords.Y);
            if (dlg.ShowDialog() == DialogResult.Cancel) return "";

            dlg.SelectedRobot.StopLevel = dlg.SelectedPrice;
            dlg.SelectedRobot.Side = dlg.SelectedSide;
            

            // обновить настройки роботов
            MainForm.Instance.RobotFarm.SetRobotSettings(robots);
            RobotFarm.SaveRobots(robots);
            return "Настройка робота CS произведена";
        }

        public override string ActivateScript(bool byTrigger)
        {
            throw new Exception("Неверный тип вызова скрипта \"ScriptSetFiboLevelRobotPoint\"");
        }

        public override string ActivateScript(string ticker)
        {
            throw new Exception("Неверный тип вызова скрипта \"ScriptSetFiboLevelRobotPoint\"");
        }
    }
    // ReSharper restore LocalizableElement
}

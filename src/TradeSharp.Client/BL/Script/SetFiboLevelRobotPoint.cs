using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Candlechart;
using Candlechart.ChartMath;
using Candlechart.Series;
using Entity;
using TradeSharp.Client.Forms;
using TradeSharp.Robot.Robot;
using TradeSharp.Util;

namespace TradeSharp.Client.BL.Script
{
    // ReSharper disable LocalizableElement
    [DisplayName("Цена для робота FX")]
    public class ScriptSetFiboLevelRobotPoint : TerminalScript
    {
        #region Параметры

        private bool adjustTime = true;
        [DisplayName("Обновлять время")]
        [Category("Основные")]
        [Description("Обновлять время старта моделирования")]
        [PropertyXMLTag("AdjustTime")]
        public bool AdjustTime
        {
            get { return adjustTime; }
            set { adjustTime = value; }
        }

        public enum RobotSourceType { Тестер = 0, РоботыLive }
        
        [DisplayName("Роботы")]
        [Category("Основные")]
        [Description("Порядок выбора роботов для установки точек")]
        [PropertyXMLTag("RobotSourceType")]
        public RobotSourceType RobotSource { get; set; }

        public enum PricePointType { A = 0, B }

        [DisplayName("Цена (A - B)")]
        [Category("Основные")]
        [Description("Ценовая точка")]
        [PropertyXMLTag("PricePoint")]
        public PricePointType PricePoint { get; set; }

        public enum PriceRefineType { Ближайшая = 0, КакЕсть = 1, Закрытия = 2 }

        [DisplayName("Уточнить цену")]
        [Category("Основные")]
        [Description("Тип \"уточнения\" цены - прилепить ее к ближайшей отметки свечи, к цене закрытия либо оставить, как есть")]
        [PropertyXMLTag("RefineType")]
        public PriceRefineType RefineType { get; set; }
        #endregion

        public override bool CanBeTriggered
        {
            get { return true; }
        }

        public ScriptSetFiboLevelRobotPoint()
        {
            ScriptTarget = TerminalScriptTarget.График;
            ScriptName = "Цена для робота FX";
        }

        public override string ActivateScript(CandleChartControl chart, PointD worldCoords)
        {
            // отыскать киборга
            List<BaseRobot> robots;
            if (RobotSource == RobotSourceType.РоботыLive)
            {
                if (MainForm.Instance.RobotFarm.State != RobotFarm.RobotFarmState.Stopped)
                {
                    const string msg = "Роботы должны быть не активны на момент настройки";
                    MessageBox.Show(msg);
                    return msg;
                }

                robots = MainForm.Instance.RobotFarm.GetRobotCopies();
                if (robots == null || robots.Count == 0) robots = RobotFarm.LoadRobots();
            }
            else // if (RobotSource == RobotSourceType.Тестер)
            {
                if (RoboTesterForm.Instance == null)
                {
                    MessageBox.Show("Окно тестера роботов не запущено");
                    return "Окно тестера роботов не запущено";
                }
                robots = RoboTesterForm.Instance.GetUsedRobots();
            }
            if (robots == null) robots = new List<BaseRobot>();
            
            var fiboLevelRobots = robots.Where(r => r is FiboLevelRobot).Cast<FiboLevelRobot>();
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

            // если таких киборгов несколько - предложить выбор
            var robot = targetRobots[0];

            if (targetRobots.Count > 1)
            {
                var number = 1;
                var robotNames = targetRobots.Select(r => (object) string.Format("Робот {0} (A:{1}, B:{2})", number++,
                            DalSpot.Instance.FormatPrice(chart.Symbol, r.PriceA),
                            DalSpot.Instance.FormatPrice(chart.Symbol, r.PriceB))).ToList();

                object selObj;
                string selText;
                if (!Dialogs.ShowComboDialog("Укажите робота", robotNames, out selObj, out selText, true))
                    return "Робот не выбран";

                var selIndex = robotNames.IndexOf(selText);
                if (selIndex < 0) return "Робот не выбран";
                robot = targetRobots[selIndex];
            }

            // уточнить цену
            var price = RefinePrice(chart, worldCoords);
             
            // подставить цену для робота
            var timeOfPoint = chart.chart.StockSeries.GetCandleOpenTimeByIndex((int)Math.Round(worldCoords.X));

            if (PricePoint == PricePointType.A)
            {
                robot.PriceA = (decimal) price;
                robot.TimeOfA = timeOfPoint;
            }
            else
            {
                robot.PriceB = (decimal)price;
                robot.TimeOfB = timeOfPoint;
            }

            // показать на графике
            ShowPriceOnChart(chart, worldCoords);

            // обновить настройки роботов
            if (RobotSource == RobotSourceType.РоботыLive)
            {
                MainForm.Instance.RobotFarm.SetRobotSettings(robots);
                RobotFarm.SaveRobots(robots);
                if (PricePoint == PricePointType.B)
                    MainForm.Instance.ShowRobotPortfolioDialog(robot.GetUniqueName());
            }
            else
            {
                if (RoboTesterForm.Instance == null)
                {
                    MessageBox.Show("Окно тестера роботов не запущено");
                    return "Окно тестера роботов не запущено";
                }
                RoboTesterForm.Instance.SaveRobots(robots);
                RoboTesterForm.Instance.ReadLastRobotSettings();

                // обновить время моделирования
                if (robots.Count == 1)
                {
                    var time = GetTimeByWorldCoord(chart, worldCoords);
                    if (time.HasValue)
                        RoboTesterForm.Instance.TimeStart = time.Value;
                }

                if (PricePoint == PricePointType.B)
                    MainForm.Instance.EnsureRoboTesterForm(robot.GetUniqueName());
            }

            return PricePoint == PricePointType.A ? "Точка A задана успешно" : "Точка B задана успешно";
        }

        /// <summary>
        /// поставить отметку на графике
        /// </summary>
        private void ShowPriceOnChart(CandleChartControl chart, PointD worldCoords)
        {
            var text = PricePoint == PricePointType.A ? "A" : "B";
            var name = "ScriptSetFiboMark" + text;

            // удалить старую отметку с графика
            int index;
            chart.seriesAsteriks.FindObject(a => a.Name == name, out index);
            if (index >= 0)
                chart.seriesAsteriks.RemoveObjectByNum(index);

            // добавить новую отметку
            var mark = new AsteriskTooltip(name, text)
                           {
                               Price = (float) worldCoords.Y,
                               CandleIndex = (int) (Math.Round(worldCoords.X)),
                               ColorLine = chart.chart.visualSettings.SeriesForeColor,
                               Sign = text
                           };
            mark.ColorText = mark.ColorLine;
            mark.ColorFill = chart.chart.visualSettings.ChartBackColor;
            mark.Shape = AsteriskTooltip.ShapeType.Звезда;

            chart.seriesAsteriks.data.Add(mark);
        }

        /// <summary>
        /// уточнить цену
        /// </summary>        
        private float RefinePrice(CandleChartControl chart, PointD worldCoords)
        {
            var price = (float) worldCoords.Y;
            if (RefineType == PriceRefineType.КакЕсть) return price;
            
            var candleIndex = (int) (worldCoords.X + 0.5);
            if (candleIndex < 0 || candleIndex >= chart.chart.StockSeries.DataCount) return price;
            var candle = chart.chart.StockSeries.Data.Candles[candleIndex];

            if (RefineType == PriceRefineType.Закрытия) return candle.close;

            var deltas = new []
                             {
                                 Math.Abs(candle.open - price), Math.Abs(candle.high - price),
                                 Math.Abs(candle.low - price), Math.Abs(candle.close - price)
                             };

            var minIndex = deltas.IndexOfMin();
            return minIndex == 0 ? candle.open : minIndex == 1 ? candle.high : minIndex == 2 ? candle.low : candle.close;
        }

        private DateTime? GetTimeByWorldCoord(CandleChartControl chart, PointD worldCoords)
        {
            var candleIndex = (int)(worldCoords.X + 0.5);
            if (candleIndex < 0 || candleIndex >= chart.chart.StockSeries.DataCount) return null;
            var candle = chart.chart.StockSeries.Data.Candles[candleIndex];
            return candle.timeClose.AddMinutes(2);
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Candlechart.Series;
using Entity;
using TradeSharp.Client.BL;
using TradeSharp.Client.Forms;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Robot.BacktestServerProxy;
using TradeSharp.Robot.Robot;

namespace TradeSharp.Client
{
    public partial class MainForm
    {
        private void StartStopRobotFarm()
        {
            var state = robotFarm.State;
            if (state == RobotFarm.RobotFarmState.Stopped || state == RobotFarm.RobotFarmState.Undefined)
            {
                robotFarm.Start(AccountStatus.Instance.AccountData);
            }
            if (state == RobotFarm.RobotFarmState.Started)
            {
                robotFarm.Stop();
            }
        }

        /// <summary>
        /// изменилось состояние "фермы" роботов - обновить иконку кнопки запуска/останова
        /// </summary>        
        private void RobotFarmStateChanged(RobotFarm.RobotFarmState state)
        {
            var del = new RobotFarm.StateChangedDel(UpdateRobotIconUnsafe);
            BeginInvoke(del, state);
        }

        /// <summary>
        /// обновление состояния кнопок и пунктов меню, связанных с роботами
        /// </summary>
        /// <param name="state"></param>
        private void UpdateRobotIconUnsafe(RobotFarm.RobotFarmState state)
        {
            var indexImage = state == RobotFarm.RobotFarmState.Started
                                 ? 12
                                 : state == RobotFarm.RobotFarmState.StartingUp
                                       ? 13
                                       : state == RobotFarm.RobotFarmState.Stopped
                                             ? 11
                                             : state == RobotFarm.RobotFarmState.Stopping ? 14 : 15;
            // проверяем есть ли кнопка на панели системных кнопок
            var pressedBtn = GetPressedCommonButton(SystemToolButton.RobotsStart);
            if (pressedBtn != null)
                pressedBtn.ImageIndex = indexImage;

            // кнопка "Запустить роботов" управляется из MainForm.ToolStripBtnClick + MainForm.ExecuteCommonButtonsCommand
            var checkBox = pressedBtn as CheckBox;
            if (checkBox != null)
                checkBox.Checked = state == RobotFarm.RobotFarmState.Started;

            // меняем доступность пунктов меню
            menuitemRobotPortfolio.Enabled = state == RobotFarm.RobotFarmState.Stopped;
            menuitemRobotState.Enabled = state == RobotFarm.RobotFarmState.Started;
            // также меняем соответствующие кнопки панели инструментов
            var btn = GetPressedCommonButton(SystemToolButton.RobotPortfolio);
            if (btn != null)
            {
                checkBox = btn as CheckBox;
                if (checkBox != null)
                    checkBox.Enabled = menuitemRobotPortfolio.Enabled;
            }
            btn = GetPressedCommonButton(SystemToolButton.RobotState);
            if (btn != null)
            {
                checkBox = btn as CheckBox;
                if (checkBox != null)
                    checkBox.Enabled = menuitemRobotState.Enabled;
            }
        }

        /// <summary>
        /// результаты моделирования необходимо отобразить на графиках
        /// (в виде коментариев и т.д.)
        /// </summary>        
        private void OnRobotResultsBoundToCharts(Dictionary<BaseRobot, ChartWindowSettings> robotBindings,
            List<RobotLogEntry> robotLogEntries, List<MarketOrder> posClosed, List<MarketOrder> posOpened)
        {
            // предложить убрать коментарии с графиков,
            // выбрать коментарии
            var dlg = new ResultDisplaySettingsForm();
            if (dlg.ShowDialog() == DialogResult.Cancel) return;
            // убрать коментарии?
            var removeOldComments = dlg.RemoveOldMarkers;
            if (removeOldComments)
            {
                var chartsToPurge = Charts.Where(c => robotBindings.Any(
                    rb => rb.Value.TabPageId == c.bookmarkId && rb.Value.Symbol == c.chart.Symbol && 
                        rb.Value.Timeframe == c.chart.timeframeString));
                foreach (var chart in chartsToPurge)
                    chart.chart.seriesAsteriks.data.Clear();
            }

            foreach (var robotBinding in robotBindings)
            {
                // найти график для робота
                var timeframe = robotBinding.Value.Timeframe;
                var ticker = robotBinding.Value.Symbol;
                var pageId = robotBinding.Value.TabPageId;
                var robot = robotBinding.Key;
                var chart = Charts.First(c => c.chart.timeframeString == timeframe && c.chart.Symbol == ticker
                    && c.bookmarkId == pageId);

                // выбрать все сообщения для графика
                var logEntries = robotLogEntries.Where(l => l.Robot == robot);
                var messagesPlainList = new List<string>();
                foreach (var logEntry in logEntries)
                    messagesPlainList.AddRange(logEntry.Messages);

                // добавить коментарии на график
                foreach (var msg in messagesPlainList)
                {
                    var mark = RobotMark.ParseString(msg);
                    if (mark == null || mark is RobotHint == false)
                        continue;

                    var hint = (RobotHint) mark;
                    if (!hint.Price.HasValue || !hint.Time.HasValue) continue;

                    AddOrRemoveRobotHintOnChart(hint, chart);
                }

                // добавить информацию о сделках
                if (!dlg.ShowMarkers) continue;

                // выбрать сделки для отображения
                var deals4Chart = posClosed.Where(p => p.Symbol == chart.chart.Symbol).ToList();
                deals4Chart.AddRange(posOpened.Where(p => p.Symbol == chart.chart.Symbol));

                foreach (var deal in deals4Chart)
                {
                    // отметки входа
                    if (dlg.ShowEnters)
                    {
                        var detailed = string.Format("#{0} {1} {2:dd.MM.yyyy HH:mm} по {3}",
                            deal.ID, deal.Side > 0 ? "B" : "S", deal.TimeEnter, deal.PriceEnter);
                        if (deal.IsClosed)
                        {
                            var result = (deal.PriceExit.Value - deal.PriceEnter) * deal.Side;
                            var points = DalSpot.Instance.GetPointsValue(deal.Symbol, result);
                            detailed += string.Format(" - {0:dd.MM.yyyy HH:mm} по {1}, {2:f0} пп",
                                                     deal.TimeExit.Value, deal.PriceExit.Value,
                                                     points);
                        }
                        var tip = new AsteriskTooltip(detailed, detailed)
                        {
                            Price = deal.PriceEnter,
                            CandleIndex = chart.chart.chart.StockSeries.GetIndexByCandleOpen(deal.TimeEnter),
                            DateStart = deal.TimeEnter,
                            Sign = "e",
                            Shape = deal.Side > 0 ? AsteriskTooltip.ShapeType.СтрелкаВверх :
                                                AsteriskTooltip.ShapeType.СтрелкаВниз,
                            ColorFill = deal.Side > 0 ? Color.Green : Color.Salmon,
                            ColorLine = Color.Black,
                            ColorText = Color.Black,
                            Radius = 5
                        };
                        if (!dlg.ShowDetailedEnters)
                            tip.Name = string.Format("Сделка #{0}, {1}", deal.ID, deal.Side > 0 ? "Buy" : "Sell");
                        chart.chart.seriesAsteriks.data.Add(tip);
                    }

                    if (dlg.ShowExits && deal.IsClosed)
                    {
                        var detailed = string.Format("Выход #{0} {1} {2:dd.MM.yyyy HH:mm} по {3} - {4:dd.MM.yyyy HH:mm} по {5}",
                                                     deal.ID, deal.Side > 0 ? "B" : "S", deal.TimeEnter, deal.PriceEnter,
                                                     deal.TimeExit.Value, deal.PriceExit.Value);

                        var tip = new AsteriskTooltip(detailed, detailed)
                        {
                            Price = deal.PriceExit.Value,
                            CandleIndex =
                                chart.chart.chart.StockSeries.GetIndexByCandleOpen(
                                    deal.TimeExit.Value),
                            DateStart = deal.TimeExit.Value,
                            Sign = "q",
                            Shape = AsteriskTooltip.ShapeType.Квадрат,
                            ColorFill = deal.Side > 0 ? Color.Green : Color.Salmon,
                            ColorLine = Color.Black,
                            ColorText = Color.Black,
                            Radius = 5
                        };
                        if (!dlg.ShowDetailedExits)
                            tip.Name = string.Format("Закрытие #{0}, {1}", deal.ID, deal.Side > 0 ? "Buy" : "Sell");
                        chart.chart.seriesAsteriks.data.Add(tip);
                    }
                }
            }
        }

        /// <summary>
        /// добавить на график отрезок или звездочку, согласно размышлениям робота
        /// </summary>
        private void AddOrRemoveRobotHintOnChart(RobotMark hint, ChartForm chart)
        {
            // добавить комментарий на график?
            if (hint is RobotHint)
            {
                AddRobotHintOnChart((RobotHint)hint, chart);
                return;
            }

            // убрать комментарий с графика?
            var markClear = (RobotMarkClear) hint;
            if (hint.RobotHintType == RobotMark.HintType.Линия)
            {
                var linesToRemove = chart.chart.seriesTrendLine.data.Where(l => l.Name == markClear.HintCode).ToList();
                foreach (var line in linesToRemove)
                    chart.chart.seriesTrendLine.RemoveObjectFromList(line);
            }
            else
            {
                var asteriskToRemove = chart.chart.seriesAsteriks.data.Where(l => l.Name == markClear.HintCode).ToList();
                foreach (var asteriks in asteriskToRemove)
                    chart.chart.seriesAsteriks.RemoveObjectFromList(asteriks);
            }
        }
        
        private void AddRobotHintOnChart(RobotHint hint, ChartForm chart)
        {
            // добавить отрезочек с комментарием
            if (hint.RobotHintType == RobotMark.HintType.Линия)
            {
                AddRobotHintLineOnChart(hint, chart);
                return;
            }

            // добавить звездочку
            var toolTip = new AsteriskTooltip(hint.Title, hint.Text)
                              {
                                  Owner = chart.chart.seriesAsteriks,
                                  Price = hint.Price.Value,
                                  CandleIndex =
                                      chart.chart.chart.StockSeries.GetIndexByCandleOpen(
                                          hint.Time.Value),
                                  DateStart = hint.Time.Value,
                                  Sign = hint.Sign,
                                  Radius = 5,
                                  Shape =
                                      hint.RobotHintType == RobotMark.HintType.Стоп
                                          ? AsteriskTooltip.ShapeType.Квадрат
                                          : hint.RobotHintType == RobotMark.HintType.Тейк
                                                ? AsteriskTooltip.ShapeType.Квадрат
                                                : hint.RobotHintType == RobotMark.HintType.Покупка
                                                      ? AsteriskTooltip.ShapeType.СтрелкаВверх
                                                      : hint.RobotHintType == RobotMark.HintType.Продажа
                                                            ? AsteriskTooltip.ShapeType.СтрелкаВниз
                                                            : hint.RobotHintType ==
                                                              RobotMark.HintType.Поджатие
                                                                  ? AsteriskTooltip.ShapeType.Звезда
                                                                  : AsteriskTooltip.ShapeType.Круг
                              };
            if (!string.IsNullOrEmpty(hint.HintCode))
                toolTip.Name = hint.HintCode;
            if (hint.ColorFill.HasValue) toolTip.ColorFill = hint.ColorFill.Value;
            if (hint.ColorLine.HasValue) toolTip.ColorLine = hint.ColorLine.Value;
            if (hint.ColorText.HasValue) toolTip.ColorText = hint.ColorText.Value;

            chart.chart.seriesAsteriks.data.Add(toolTip);
        }

        private void AddRobotHintLineOnChart(RobotHint hint, ChartForm chart)
        {
            var pivotIndex = chart.chart.chart.StockSeries.GetIndexByCandleOpen(
                hint.Time.Value);

            var line = new TrendLine
                {
                    Comment = hint.Text,
                    DateStart = hint.Time.Value,
                    LineColor = hint.ColorLine ?? chart.chart.chart.visualSettings.SeriesForeColor,
                    ShapeFillColor = hint.ColorFill ?? chart.chart.chart.visualSettings.SeriesBackColor,
                    LineStyle = TrendLine.TrendLineStyle.Отрезок,
                    Owner = chart.chart.seriesTrendLine
                };
            if (!string.IsNullOrEmpty(hint.HintCode))
                line.Name = hint.HintCode;
            line.AddPoint(pivotIndex, hint.Price.Value);
            // точку конца отрезка сместить вправо на N свечек
            line.AddPoint(pivotIndex + 10, hint.Price.Value);
            chart.chart.seriesTrendLine.data.Add(line);
        }

        public void ShowRobotHintsSafe(BaseRobot robot, List<RobotMark> hints)
        {
            foreach (var hint in hints)
            {
                // определить тикер и таймфрейм
                var ticker = hint.Symbol;
                var timeframe = BarSettingsStorage.Instance.GetBarSettingsByName(hint.Timeframe);

                if (string.IsNullOrEmpty(ticker) && robot.Graphics.Count > 0)
                    ticker = robot.Graphics[0].a;

                if (timeframe == null && robot.Graphics.Count > 0)
                    timeframe = robot.Graphics[0].b;

                if (string.IsNullOrEmpty(ticker)) continue;

                // отыскать график
                foreach (var chart in Charts.Where(c => c.chart.Symbol == ticker && (
                    timeframe == null || c.chart.Timeframe == timeframe)))
                {
                    AddOrRemoveRobotHintOnChart(hint, chart);
                }
            }
        }

        /// <summary>
        /// открыть окно состояния роботов
        /// </summary>
        private void MenuitemRobotStateClick(object sender, EventArgs e)
        {
            ShowRobotStateDialog(string.Empty);
        }

        public void ShowRobotStateDialog(string selectedRobotUniqueName)
        {
            var dlg = new RobotStateDialog(robotFarm, selectedRobotUniqueName);
            dlg.ShowDialog();
        }
    }
}

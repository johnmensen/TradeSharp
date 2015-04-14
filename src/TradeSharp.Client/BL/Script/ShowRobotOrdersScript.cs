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
    [DisplayName("Сделки робота")]
    public class ShowRobotOrdersScript : TerminalScript
    {
        public const string CommentSpecName = "RobotStateScript";

        #region Параметры
        
        public Color colorBuy = Color.DarkCyan;
        [Category("Визуальные")]
        [PropertyXMLTag("ColorBuy")]
        [DisplayName("Цвет покупок")]
        [Description("Цвет маркеров - покупок")]
        public Color ColorBuy
        {
            get { return colorBuy; }
            set { colorBuy = value; }
        }

        public Color colorSell = Color.Coral;
        [Category("Визуальные")]
        [PropertyXMLTag("ColorSell")]
        [DisplayName("Цвет продаж")]
        [Description("Цвет маркеров - продаж")]
        public Color ColorSell
        {
            get { return colorSell; }
            set { colorSell = value; }
        }
        #endregion

        #region Переменные состояния

        private BaseRobot selectedBot;

        private CandleChartControl chart;

        private PointD scriptActivatedCoords;
        #endregion

        public ShowRobotOrdersScript()
        {
            ScriptTarget = TerminalScriptTarget.График;
            ScriptName = "Сделки робота";

            Trigger = new ScriptTriggerNewQuote
            {
                quotesToCheck = new List<string>()
            };
        }

        public override string ActivateScript(CandleChartControl chart, PointD worldCoords)
        {
            this.chart = chart;
            scriptActivatedCoords = worldCoords;

            // найти робота / роботов на данный тикер / ТФ
            var robots = MainForm.Instance.RobotFarm.GetRobotCopies();
            robots = robots.Where(r => r.Graphics.Any(g => g.a == chart.chart.Symbol && g.b == chart.chart.Timeframe))
                      .ToList();
            if (robots.Count == 0) return "Нет роботов для " + chart.Symbol + ":" + chart.Timeframe;
            selectedBot = robots[0];
            // если роботов несколько - предложить пользователю выбрать интересующего
            if (robots.Count > 1)
            {
                object selectedRobot;
                string inputText;

                if (!Dialogs.ShowComboDialog("Укажите робота",
                                                             robots.Cast<object>().ToList(), out selectedRobot,
                                                             out inputText))
                    return "робот не выбран из списка";
                selectedBot = (BaseRobot) selectedRobot;
            }

            ShowRobotDataOnChart();
            
            return "";
        }

        public override string ActivateScript(string ticker)
        {
            throw new Exception("Неверный тип вызова скрипта \"ShowRobotOrdersScript\"");
        }

        public override string ActivateScript(bool byTrigger)
        {
            if (!byTrigger)
                throw new Exception("Неверный тип вызова скрипта \"ShowRobotOrdersScript\"");
            if (selectedBot == null)
                return "";
            // обновить объекты графика
            return "";
        }

        private void ShowRobotDataOnChart()
        {
            var orders = MarketOrdersStorage.Instance.MarketOrders.Where(o => o.Symbol == chart.Symbol
                && o.Magic == selectedBot.Magic).ToList();

            // показать ордера на графике
            foreach (var order in orders)
            {
                var candleIndex = (int)Math.Round(chart.chart.StockSeries.GetDoubleIndexByTime(order.TimeEnter));
                var shape = order.Side > 0
                                ? AsteriskTooltip.ShapeType.СтрелкаВверх
                                : AsteriskTooltip.ShapeType.СтрелкаВниз;

                var objectExists = chart.seriesAsteriks.data.Any(a =>
                    a.Price.RoughCompares(order.PriceEnter, 0.00001f) && a.CandleIndex == candleIndex &&
                    a.Shape == shape);
                if (objectExists) continue;
                
                var asterisk = new AsteriskTooltip(order.Side < 0 ? "SELL" : "BUY",
                                                   selectedBot.GetUniqueName() + ": " + order.ToStringShort())
                {
                    ColorFill = order.Side < 0 ? colorSell : colorBuy,
                    Price = order.PriceEnter,
                    CandleIndex = candleIndex,
                    DateStart = order.TimeEnter,
                    Shape = shape,
                    Sign = order.Side > 0 ? "b" : "s",
                    Owner = chart.seriesAsteriks
                };
                chart.seriesAsteriks.data.Add(asterisk);
            }

            AddChartCommentOnRobotState(orders.Count);
        }

        private void AddChartCommentOnRobotState(int dealsOpened)
        {
            var commentLines = new List<string>
                {
                    selectedBot.GetUniqueName()
                };

            if (selectedBot is FiboLevelRobot)
            {
                var fiboBot = ((FiboLevelRobot) selectedBot);
                commentLines.Add("открыто " + dealsOpened + " из " + fiboBot.MaxDealsInSeries + " сделок");
                commentLines.Add("цена A: " + fiboBot.PriceA.ToStringUniformPriceFormat() + ", цена B: " +
                    fiboBot.PriceB.ToStringUniformPriceFormat());
            }
            else
            {
                commentLines.Add("открыто " + dealsOpened + " сделок");
            }
            var commentStr = string.Join(Environment.NewLine, commentLines);

            // найти на графике существующий комментарий или создать новый
            var comment = chart.seriesComment.data.FirstOrDefault(c => c.Name == CommentSpecName);
            if (comment == null)
            {
                comment = new ChartComment
                    {
                        Name = CommentSpecName,
                        PivotIndex = scriptActivatedCoords.X,
                        PivotPrice = scriptActivatedCoords.Y,
                        HideArrow = true,
                        Color = chart.chart.visualSettings.SeriesForeColor,
                        ColorFill = Color.DarkTurquoise,
                        Text = commentStr,
                        Owner = chart.seriesComment
                    };
                chart.seriesComment.data.Add(comment);
            }
            else
            {
                comment.Text = commentStr;
            }
        }
    }
}

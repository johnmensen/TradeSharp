using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Candlechart;
using Candlechart.ChartMath;
using Candlechart.Series;
using Entity;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Client.BL.Script
{
    [LocalizedDisplayName("TitleSummaryPositionCommentScript")]
    public class SummaryPositionCommentScript : TerminalScript
    {
        public const string CommentSpecName = "SummaryPosition";

        public enum CommentDetailType { Подробно = 0, Кратко = 1 }
        [LocalizedDisplayName("TitleCommentDetailType")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("TitleCommentDetailTypeDescription")]
        [PropertyXMLTag("DetailType")]
        public CommentDetailType DetailType { get; set; }

        public SummaryPositionCommentScript()
        {
            ScriptTarget = TerminalScriptTarget.График;
            ScriptName = Localizer.GetString("TitleSummaryPositionCommentScript");
                //"Сумм. позиция на графике";
            Trigger = new ScriptTriggerNewQuote
                {
                    quotesToCheck = new List<string>()
                };
        }

        public override string ActivateScript(CandleChartControl chart, PointD worldCoords)
        {
            var comment = chart.seriesComment.data.FirstOrDefault(c => c.Name == CommentSpecName);
            // удалить существующий
            if (comment != null)
            {
                chart.seriesComment.data.Remove(comment);
                chart.RedrawChartSafe();
                return string.Empty;
            }

            // добавить новый
            MarketOrder sumPos;
            string scriptText;
            GetCommentText(chart, out scriptText, out sumPos);
            if (sumPos == null) return string.Empty;

            var colorFill = sumPos.ResultDepo > 0
                                ? Color.LightGreen : sumPos.ResultDepo < 0 ? Color.LightCoral : Color.Gray;

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
                    Text = scriptText,
                    ColorText = colorText,
                    Color = colorText
                };
            chart.seriesComment.data.Add(comment);
            chart.RedrawChartSafe();
            return string.Empty;
        }

        private static void GetCommentText(CandleChartControl chart, out string scriptText, out MarketOrder sumPos)
        {
            scriptText = string.Empty;
            sumPos = null;

            var marketOrders = MarketOrdersStorage.Instance.MarketOrders;
            if (marketOrders == null || marketOrders.Count == 0) return;
            var orders = MarketOrdersStorage.Instance.MarketOrders.Where(o => o.Symbol == chart.Symbol).ToList();
            
            var quote = QuoteStorage.Instance.ReceiveValue(chart.Symbol);
            var lastPrice = chart.chart.StockSeries.DataCount == 0
                                ? (float?) null
                                : chart.chart.StockSeries.Data[chart.chart.StockSeries.DataCount - 1].close;
            sumPos = DalSpot.Instance.CalculateSummaryOrder(orders, quote, lastPrice);
            if (sumPos == null) return;
            var accountData = AccountStatus.Instance.AccountData;
            var depoCurx = accountData == null ? "" : accountData.Currency;
            var equity = accountData == null ? 0 : accountData.Equity;

            // суммарная позиция
            scriptText = "[b]" + MakeCommentByPosition(sumPos, depoCurx, equity);
            // если есть и покупки, и продажи - суммировать по ним
            if (orders.Any(o => o.Side > 0) && orders.Any(o => o.Side < 0))
            {
                var sumBuy = DalSpot.Instance.CalculateSummaryOrder(orders.Where(o => o.Side > 0).ToList(), quote, lastPrice);
                var sumSell = DalSpot.Instance.CalculateSummaryOrder(orders.Where(o => o.Side < 0).ToList(), quote, lastPrice);
                scriptText = scriptText + Environment.NewLine + Environment.NewLine + "[Green]" +
                             MakeCommentByPosition(sumBuy, depoCurx, equity);
                scriptText = scriptText + Environment.NewLine + Environment.NewLine + "[#ff0000]" +
                             MakeCommentByPosition(sumSell, depoCurx, equity);
            }            
        }

        private static string MakeCommentByPosition(MarketOrder sumPos, string depoCurx, decimal equity)
        {
            // вход туда-то по такой-то
            var scriptText = string.Format("{0}: {1} {2}\n" +
                                           "по {3}",
                                           sumPos.Symbol,
                                           sumPos.Side == 0 ? "-" : sumPos.Side > 0 ? "BUY" : "SELL",
                                           sumPos.Volume.ToStringUniformMoneyFormat(),
                                           sumPos.PriceEnter.ToStringUniformPriceFormat(true));
            // такой-то стоп, такой-то тейк
            if (sumPos.StopLoss > 0 || sumPos.TakeProfit > 0)
            {
                scriptText = scriptText + Environment.NewLine;
                var parts = new List<string>();
                if (sumPos.StopLoss.HasValue)
                    parts.Add("SL: " + sumPos.StopLoss.Value.ToStringUniformPriceFormat());
                if (sumPos.TakeProfit.HasValue)
                    parts.Add("TP: " + sumPos.TakeProfit.Value.ToStringUniformPriceFormat());
                scriptText = scriptText + string.Join(", ", parts);
            }

            // такой-то результат
            var profitLoss = sumPos.ResultPoints < 0 ? "Убыток" : "Прибыль";
            var percentProfit = equity <= 0
                                    ? "-"
                                    : (100*(decimal)sumPos.ResultDepo/equity).ToString("f2");
            scriptText = scriptText + string.Format("\n{4}, {0}: {1}\n{4}, пп: {2}\n{4}, %: {3}",
                                           depoCurx,
                                           sumPos.ResultDepo.ToStringUniformMoneyFormat(true),
                                           sumPos.ResultPoints.ToString("f0"),
                                           percentProfit,
                                           profitLoss);
            return scriptText;
        }

        public override string ActivateScript(string ticker)
        {
            throw new Exception("Неверный тип вызова скрипта \"SummaryPositionCommentScript\"");
        }

        public override string ActivateScript(bool byTrigger)
        {
            if (!byTrigger)
                throw new Exception("Неверный тип вызова скрипта \"SummaryPositionCommentScript\"");

            // обновить комментарии на графиках
            var charts = MainForm.Instance.GetChartList(true);
            foreach (var chart in charts)
            {
                var comment = chart.seriesComment.data.FirstOrDefault(c => c.Name == CommentSpecName);
                if (comment == null) continue;

                // обновить текст
                MarketOrder sumPos;
                string scriptText;
                GetCommentText(chart, out scriptText, out sumPos);
                if (sumPos == null) continue;
                
                // обновить объект - комментарий
                var colorFill = sumPos.ResultDepo > 0
                                    ? Color.LightGreen : sumPos.ResultDepo < 0 ? Color.LightCoral : Color.Gray;
                comment.Text = scriptText;
                comment.ColorFill = colorFill;
            }

            return string.Empty;
        }
    }
}

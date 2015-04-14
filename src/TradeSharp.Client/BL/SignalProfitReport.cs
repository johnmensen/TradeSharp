using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Entity;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;
using QuoteStorage = TradeSharp.Contract.Util.BL.QuoteStorage;

namespace TradeSharp.Client.BL
{
    /// <summary>
    /// отчет о доходности управления по различным категориям торговых сигналов
    /// + сводный отчет
    /// </summary>
    static class SignalProfitReport
    {
        public static string MakeReport(int accountId, float balance, string depoCurx)
        {
            var quotes = QuoteStorage.Instance.ReceiveAllData();
            // результаты по открытым сделкам...
            var signalResult = GetOpenDealsResult(balance, depoCurx, quotes);
            // присовокупить результаты по закрытым
            GetClosedDealsResult(balance, depoCurx, quotes, signalResult);
            // рендерить результаты
            var path = string.Format("{0}\\report_{1}_{2:dd_MMM_yyyy HH_mm}.html",
                                     ExecutablePath.ExecPath, accountId, DateTime.Now);
            var result = RenderResultsHTML(accountId, depoCurx, signalResult, path);
            return result ? path : string.Empty;
        }

        private static bool RenderResultsHTML(int accountId, string depoCurx, Dictionary<int, TradeSignalProfit> signalResult, string path)
        {
            try
            {
                using (var sw = new StreamWriter(path, false, Encoding.UTF8))
                {
                    // рендерить шапку
                    RenderResultsHeader(sw, accountId);
                    // рендерить отдельно результаты по каждому сигналу
                    foreach (var pair in signalResult)
                    {
                        RenderResultsTable(sw, pair.Key, pair.Value, depoCurx);
                    }
                    // рендерить закрывающие теги
                    sw.WriteLine("  </body>");
                    sw.WriteLine("</html>");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("SignalProfitReport - ошибка рендеринга", ex);
                return false;
            }
            return true;
        }

        private static void RenderResultsTable(StreamWriter sw, int signalCatId, TradeSignalProfit results,
            string depoCurrency)
        {
            //// получить название категории торговых сигналов
            //var signalCat = categories.FirstOrDefault(c => c.Id == signalCatId);
            //var signalName = signalCat == null ? "Торговые сигналы #" + signalCatId : signalCat.Title;
            //var sumResultDepo = results.totalClosedResult + results.totalOpenResultDepo;
            //var isProfit = sumResultDepo >= 0;

            //// таблица на 7 колонок, в шапке - название торг. сигнала, затем суммарный результат,
            //// затем открытые позы и, наконец, закрытые позы
            //sw.WriteLine("    <table id=\"" + signalCatId + "\">");

            //// название торг. сигналов
            //sw.WriteLine("      <tr class=\"" + (isProfit ? "rowCaption" : "rowCaptionRed") + "\">");
            //sw.WriteLine("        <td colspan=\"7\">" + signalName + "</td></tr>");

            //// общий результат
            //sw.WriteLine("      <tr>");
            //sw.WriteLine("        <td colspan=\"3\">Суммарный результат: " + sumResultDepo.ToStringUniformMoneyFormat() +
            //    " " + depoCurrency + "</td>");
            //sw.WriteLine("        <td colspan=\"4\">Сделок всего: " + results.totalOpenPosCount + results.totalClosedDealsCount + "</td>");
            //sw.WriteLine("      </tr>");

            //// детализация открытых позиций
            //sw.WriteLine("      <tr class=\"rowCaption\">");
            //sw.WriteLine("        <td colspan=\"7\">Открытые позиции</td>");
            //sw.WriteLine("      </tr>");

            //foreach (var pos in results.openDeals)
            //{
            //    sw.WriteLine("      <tr>");
            //    sw.WriteLine(string.Format(
            //            "         <td>{0} {1}</td> <td>{2}</td> <td>{3}</td>  <td>вход: {4}</td> <td>{5}</td> <td>{6:f0} пп</td> <td>{7:f2}%</td>",
            //            pos.Side > 0 ? "BUY" : pos.Side < 0 ? "SELL" : "-",
            //            pos.Volume.ToString("n0"),
            //            pos.Symbol,
            //            pos.Comment,
            //            DalSpot.Instance.FormatPrice(pos.Symbol, pos.PriceEnter),
            //            pos.ResultDepo.ToStringUniformMoneyFormat() + " " + depoCurrency,
            //            pos.ResultPoints,
            //            pos.ResultBase));
            //    sw.WriteLine("      </tr>");
            //}

            //// суммарная открытая поза
            //if (results.openDeals.Count > 0)
            //{
            //    sw.WriteLine("      <tr>");
            //    sw.WriteLine(string.Format(
            //        "<td colspan=\"3\">Сделок всего: {0}</td> <td colspan=\"2\">Общий результат: {1} {2}</td> " +
            //        "<td colspan=\"2\">Процент от средств: {3:f2}%</td>",
            //        results.totalOpenPosCount,
            //        results.totalOpenResultDepo.ToStringUniformMoneyFormat(),
            //        depoCurrency,
            //        results.totalOpenResultPercent));

            //    sw.WriteLine("      </tr>");
            //}

            //// детализация закрытых поз
            //sw.WriteLine("      <tr class=\"rowCaption\">");
            //sw.WriteLine("        <td colspan=\"7\">Закрытые позиции</td>");
            //sw.WriteLine("      </tr>");

            //foreach (var pos in results.closedDeals)
            //{
            //    sw.WriteLine("      <tr>");
            //    sw.WriteLine(string.Format(
            //        "         <td>{0} {1}</td> <td>{2}</td> <td>{3}</td> <td colspan=\"3\">{4:f0} пп</td>",
            //            (pos.Side > 0 ? "BUY " : pos.Side < 0 ? "SELL " : "- ") + pos.Volume.ToString("n0"),
            //            pos.Symbol,
            //            pos.Comment,
            //            pos.ResultDepo.ToStringUniformMoneyFormat() + " " + depoCurrency,
            //            pos.ResultPoints));
            //    sw.WriteLine("      </tr>");
            //}

            //// суммарная закрытая поза
            //if (results.closedDeals.Count > 1)
            //{
            //    sw.WriteLine("      <tr>");
            //    sw.WriteLine(string.Format(
            //        "<td colspan=\"3\">Сделок закрыто всего: {0}</td> <td colspan=\"2\">Общий результат: {1} {2}</td>",
            //        results.totalClosedDealsCount,
            //        results.totalClosedResult.ToStringUniformMoneyFormat(),
            //        depoCurrency));

            //    sw.WriteLine("      </tr>");
            //}

            //sw.WriteLine("    </table>");
        }

        private static void RenderResultsHeader(StreamWriter sw, int accountId)
        {
            sw.WriteLine("<html>");
            sw.WriteLine("  <head>");
            sw.WriteLine("    <title>Отчет по управлению счетом №" + accountId + "</title>");
            sw.WriteLine("    <style>");
            sw.WriteLine("          .break { page-break-before: always; }");
            sw.WriteLine("          table { border:0px; border-collapse:collapse; }");
            sw.WriteLine("          tr { border-width:1px; }");
            sw.WriteLine("          td { font-family: \"Minion Pro\"; font-size: 75%; padding:0px; 75%; padding-right:3px; padding-left:3px; }");
            sw.WriteLine("          .rowCaption { color:#FFFFFF; font-family: \"Arno Pro\"; font-size: 110%;" +
                                    " font-weight:800; text-align:center; background-color:#105008; padding-top:0; margin-top:0; }");
            sw.WriteLine("          .rowCaptionRed { color:#FFFFFF; font-family: \"Arno Pro\"; font-size: 110%;" +
                                    " font-weight:800; text-align:center; background-color:#602008; padding-top:0; margin-top:0; }");
            sw.WriteLine("    </style>");
            sw.WriteLine("  </head>");
            sw.WriteLine("  <body>");
            sw.WriteLine("    <h2>Результаты управления счетом №" + accountId + " на " +
                DateTime.Now.ToStringUniform() + "</h2>");
        }

        private static Dictionary<int, TradeSignalProfit> GetOpenDealsResult(
            float balance, string depoCurx, Dictionary<string, QuoteData> quotes)
        {
            var signalResult = new Dictionary<int, TradeSignalProfit>();
            // получить открытые сделки
            var orders = MarketOrdersStorage.Instance.MarketOrders;
            foreach (var order in orders)
            {
                int signalCatId, parentDealId;
                if (!MarketOrder.GetTradeSignalFromDeal(order, out signalCatId, out parentDealId)) continue;

                TradeSignalProfit result;
                signalResult.TryGetValue(signalCatId, out result);
                if (result == null)
                {
                    result = new TradeSignalProfit();
                    signalResult.Add(signalCatId, result);
                }
                result.openDeals.Add(order);
            }
            if (signalResult.Count == 0) return signalResult;

            // агрегировать открытые сделки - суммарные позы по ордерам
            foreach (var result in signalResult.Values)
            {
                // список суммарных поз по каждому тикеру - EURUSD, ...
                if (result.openDeals.Count == 0) continue;
                var sumPos = PositionSummary.GetPositionSummary(quotes, result.openDeals, depoCurx, balance);
                result.openDeals.Clear();

                foreach (var pos in sumPos)
                {
                    pos.Command = MakeDealsCountString(pos.orders.Count);
                    if (string.IsNullOrEmpty(pos.Symbol)) continue;

                    result.openDeals.Add(new MarketOrder
                    {
                        Side = pos.Side,
                        Symbol = pos.Symbol,
                        Volume = pos.Volume,
                        Comment = pos.Command,
                        PriceEnter = pos.AveragePrice,
                        ResultPoints = pos.ProfitInPoints,
                        ResultDepo = pos.Profit,
                        ResultBase = pos.ProfitInPercent
                    });
                }
                // суммарная поза
                var totalPos = sumPos.First(p => string.IsNullOrEmpty(p.Symbol));
                sumPos.Remove(totalPos);

                result.totalOpenPosCount = sumPos.Sum(p => p.orders.Count);
                result.totalOpenResultDepo = sumPos.Sum(p => p.Profit);
                result.totalOpenResultPercent = 100 * result.totalOpenResultDepo / balance;
                result.totalVolumeOpenDepo = totalPos.Volume;
            }

            return signalResult;
        }

        private static void GetClosedDealsResult(
            float balance, string depoCurx,
            Dictionary<string, QuoteData> quotes,
            Dictionary<int, TradeSignalProfit> signalResult)
        {
            List<MarketOrder> orders;
            try
            {
                var status = TradeSharpAccount.Instance.proxy.GetHistoryOrders(AccountStatus.Instance.accountID, null, out orders);
                if (status != RequestStatus.OK)
                {
                    Logger.ErrorFormat("SignalProfitReport - не удалось получить закрытые сделки ({0})", status);
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("SignalProfitReport - не удалось получить закрытые сделки:", ex);
                return;
            }
            foreach (var order in orders)
            {
                int signalCatId, parentDealId;
                if (!MarketOrder.GetTradeSignalFromDeal(order, out signalCatId, out parentDealId)) continue;

                TradeSignalProfit result;
                signalResult.TryGetValue(signalCatId, out result);
                if (result == null)
                {
                    result = new TradeSignalProfit();
                    signalResult.Add(signalCatId, result);
                }
                result.closedDeals.Add(order);
            }

            // суммарные позы
            foreach (var result in signalResult.Values)
            {
                // список суммарных поз по каждому тикеру - EURUSD, ...
                if (result.closedDeals.Count == 0) continue;
                var sumPos = PositionSummary.GetPositionSummary(quotes, result.closedDeals, depoCurx, balance);
                result.closedDeals.Clear();

                foreach (var pos in sumPos)
                {
                    pos.Command = MakeDealsCountString(pos.orders.Count);
                    if (string.IsNullOrEmpty(pos.Symbol)) continue;

                    result.closedDeals.Add(new MarketOrder
                    {
                        Side = pos.Side,
                        Symbol = pos.Symbol,
                        Volume = pos.Volume,
                        Comment = pos.Command,
                        ResultPoints = pos.ProfitInPoints,
                        ResultDepo = pos.Profit,
                        ResultBase = pos.ProfitInPercent
                    });
                }
                // суммарная поза
                var totalPos = sumPos.First(p => string.IsNullOrEmpty(p.Symbol));
                sumPos.Remove(totalPos);
                result.totalClosedDealsCount = sumPos.Sum(p => p.orders.Count);
                result.totalClosedResult = sumPos.Sum(p => p.Profit);
                result.totalClosedVolume = totalPos.Volume;
            }
        }

        private static string MakeDealsCountString(int count)
        {
            var countStr = count.ToString();
            if (countStr.EndsWith("0") || (count > 10 && countStr[countStr.Length - 2] == '1'))
                return countStr + " сделок";
            return countStr.EndsWith("1") ? countStr + " сделка" : countStr + " сделки";
        }
    }

    class TradeSignalProfit
    {
        public string SignalTitle { get; set; }

        #region Открытые позиции
        public List<MarketOrder> openDeals = new List<MarketOrder>();

        public int totalVolumeOpenDepo;

        public float totalOpenResultDepo;

        public float totalOpenResultPercent;

        public int totalOpenPosCount;
        #endregion

        #region Закрытые сделки
        public List<MarketOrder> closedDeals = new List<MarketOrder>();

        public int totalClosedVolume;

        public float totalClosedResult;

        public int totalClosedDealsCount;
        #endregion
    }
}

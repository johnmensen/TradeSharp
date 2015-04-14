using System;
using System.Collections.Generic;
using System.Text;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Client.BL.Report
{
    abstract class ReportRender
    {
        protected Account account;
        protected AccountStatistics stat;
        protected List<MarketOrder> openedOrders;
        protected List<MarketOrder> closedOrders;
        protected bool renderClosedDeals;

        public string RenderReport(Account account, AccountStatistics stat,
            List<MarketOrder> openedOrders, List<MarketOrder> closedOrders,
            
            bool renderClosedDeals)
        {
            this.account = account;
            this.stat = stat;
            this.openedOrders = openedOrders;
            this.closedOrders = closedOrders;
            this.renderClosedDeals = renderClosedDeals;

            var sb = new StringBuilder();
            RenderDocumentHead(sb);
            RenderBody(sb);
            RenderDocumentCloseTag(sb);
            return sb.ToString();
        }

        private void RenderBody(StringBuilder sb)
        {
            // заголовок (доход по счету такому-то...)
            RenderAccountTitle(sb, "  ");

            // таблица статистики по счету
            RenderStatTableHeader(sb, "  ");
            RenderStatTable(sb, "  ");
            RenderStatTableFooter(sb, "  ");

            // график средств / баланса
            RenderProfitChart(sb, "  ");

            // таблица - суммарные позы
            if (openedOrders.Count > 0)
            {
                var quotes = QuoteStorage.Instance.ReceiveAllData();
                var listSum = PositionSummary.GetPositionSummary(quotes, openedOrders, account.Currency);

                RenderSummaryOpenDealsTableHeader(sb, "  ");
                RenderSummaryOpenDealsTable(sb, "  ", listSum);
                RenderSummaryOpenDealsTableFooter(sb, "  ");

                // таблица открытых сделок
                RenderOpenDealsTableHeader(sb, "  ");
                RenderOpenDealsTable(sb, "  ");
                RenderOpenDealsTableFooter(sb, "  ");
            }         

            if (renderClosedDeals)
            {
                RenderClosedDealsTableHeader(sb, "  ");
                RenderClosedDealsTable(sb, "  ");
                RenderClosedDealsTableFooter(sb, "  ");
            }
        }

        protected virtual void RenderAccountTitle(StringBuilder sb, string indent)
        {
            indent += "  ";
            sb.AppendLine(string.Format(
                "{0}<h3>Состояние счета №{1} на {2:dd MMM yyyy HH:mm}</h3>",
                indent, account.ID, DateTime.Now));
            
            sb.AppendLine(string.Format(
                "{0}<p>Текущий баланс счета: {1} {3}<br/>Прибыль по открытым позициям: {2} {3}</p>",
                indent, account.Equity.ToStringUniformMoneyFormat(),
                (account.Equity - account.Balance).ToStringUniformMoneyFormat(),
                account.Currency));
        }

        protected abstract void RenderDocumentHead(StringBuilder sb);

        protected virtual void RenderDocumentCloseTag(StringBuilder sb)
        {
            sb.AppendLine("  </body>");
            sb.AppendLine("</html>");
        }

        protected abstract void RenderStatTableHeader(StringBuilder sb, string indent);

        protected virtual void RenderStatTableFooter(StringBuilder sb, string indent)
        {
            sb.AppendLine(indent + "</table>");
        }

        protected abstract void RenderStatTable(StringBuilder sb, string indent);

        protected abstract void RenderSummaryOpenDealsTableHeader(StringBuilder sb, string indent);

        protected virtual void RenderSummaryOpenDealsTableFooter(StringBuilder sb, string indent)
        {
            sb.AppendLine(indent + "</table>");
        }

        protected abstract void RenderSummaryOpenDealsTable(StringBuilder sb, string indent,
                                                            List<PositionSummary> positions);
        
        protected abstract void RenderOpenDealsTableHeader(StringBuilder sb, string indent);

        protected virtual void RenderOpenDealsTableFooter(StringBuilder sb, string indent)
        {
            sb.AppendLine(indent + "</table>");
        }

        protected abstract void RenderOpenDealsTable(StringBuilder sb, string indent);

        protected abstract void RenderProfitChart(StringBuilder sb, string indent);

        protected abstract void RenderClosedDealsTableHeader(StringBuilder sb, string indent);

        protected virtual void RenderClosedDealsTableFooter(StringBuilder sb, string indent)
        {
            sb.AppendLine(indent + "</table>");
        }

        protected abstract void RenderClosedDealsTable(StringBuilder sb, string indent);
    }
}

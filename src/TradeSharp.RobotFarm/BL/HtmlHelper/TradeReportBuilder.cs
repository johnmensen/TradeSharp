using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;
using TradeSharp.Util.Serialization;

namespace TradeSharp.RobotFarm.BL.HtmlHelper
{
    class TradeReportBuilder
    {
        class AccountTrades
        {
            public string AccountTitle { get; set; }

            public int ClosedTrades { get; set; }

            public int OpenedTrades { get; set; }

            public decimal ClosedVolumeDepo { get; set; }

            public decimal Exposure { get; set; }

            public decimal OpenProfit { get; set; }

            public string ErrorMessage { get; set; }

            public DateTime? LastOpen { get; set; }

            public static string RenderCaption()
            {
                return "      <tr class=\"rowHeader\"> <td>Счет</td>" +
                    "<td>Закрыто</td> <td>Открыто</td> <td>Закрыт объем</td> <td>Экспозиция</td> <td>Прибыль</td> <td>Послед. открытие</td> </tr>";
            }

            public override string ToString()
            {
                if (!string.IsNullOrEmpty(ErrorMessage))
                    return " <tr> <td colspan=\"7\">" + ErrorMessage + "</td> </tr>";

                var color = Exposure == 0 ? "#606060" : OpenProfit > 0 ? "#0040E0" : "#BB0000";
                return string.Format("      <tr style=\"color:{7}\"> <td>{0}</td> <td>{1}</td> " +
                    "<td>{2}</td> <td>{3}</td> <td>{4}</td> <td>{5}</td> <td>{6}</td> </tr>",
                    AccountTitle,
                    ClosedTrades.ToStringUniformMoneyFormat(),
                    OpenedTrades.ToStringUniformMoneyFormat(),
                    ClosedVolumeDepo.ToStringUniformMoneyFormat(),
                    Exposure.ToStringUniformMoneyFormat(),
                    OpenProfit.ToStringUniformMoneyFormat(),
                    LastOpen.HasValue ? LastOpen.Value.ToStringUniform() : "-",
                    color);
            }
        }

        private readonly DateTime startDate;

        public TradeReportBuilder(int daysCount)
        {
            if (daysCount < 1) daysCount = 7;
            startDate = DateTime.Now.AddDays(-daysCount);
        }

        public void BuildReport(StringBuilder sb)
        {
            sb.AppendLine("    <br/> <h3>Торговля</h3>");
            sb.AppendLine("    <br/> <table class=\"lightTable\">");
            sb.AppendLine(AccountTrades.RenderCaption());

            // получить количество открытых / закрытых по счетам сделок за указанное количество дней
            var records = new List<AccountTrades>();
            foreach (var account in RobotFarm.Instance.Accounts.OrderBy(a => a.AccountId))
            {
                var record = MakeAccountTradesRecord(account);
                records.Add(record);
                sb.AppendLine(record.ToString());
            }

            var sumRecord = new AccountTrades
            {
                AccountTitle = "- Суммарно -",
                ClosedTrades = records.Sum(r => r.ClosedTrades),
                OpenedTrades = records.Sum(r => r.OpenedTrades),
                ClosedVolumeDepo = records.Sum(r => r.ClosedVolumeDepo),
                Exposure = records.Sum(r => r.Exposure),
                OpenProfit = records.Sum(r => r.OpenProfit),                
            };
            sumRecord.LastOpen = records.Count == 0
                ? DateTime.MinValue
                : records.Max(r => r.LastOpen ?? DateTime.MinValue);
            if (sumRecord.LastOpen == DateTime.MinValue)
                sumRecord.LastOpen = null;
            sb.AppendLine(sumRecord.ToString());

            sb.AppendLine("    </table>");
        }

        private AccountTrades MakeAccountTradesRecord(FarmAccount account)
        {
            var record = new AccountTrades
            {
                AccountTitle = "#" + account.AccountId + " / " + account.UserLogin
            };

            var context = account.GetContext();
            if (context == null)
                return new AccountTrades
                {
                    ErrorMessage = "контекст не создан"
                };

            var ordersClosed = GetAccountHistoryOrders(account) ?? new List<MarketOrder>();
            record.ClosedTrades = ordersClosed.Count;
            var errors = new List<string>();

            var quotes = QuoteStorage.Instance.ReceiveAllData();
            record.ClosedVolumeDepo = DalSpot.Instance.CalculateExposure(ordersClosed,
                quotes, context.AccountInfo.Currency, errors);

            var ordersOpened = GetAccountOpenedOrders(account);
            record.OpenedTrades = ordersOpened.Count;
            record.Exposure = DalSpot.Instance.CalculateExposure(ordersOpened,
                quotes, context.AccountInfo.Currency, errors);
            record.OpenProfit = (decimal) DalSpot.Instance.CalculateOpenedPositionsCurrentResult(ordersOpened,
                quotes, context.AccountInfo.Currency);
            var lastClosedEnter = ordersClosed.Count > 0 ? ordersClosed.Max(o => o.TimeEnter) : DateTime.MinValue;
            var lastOpenEnter = ordersOpened.Count > 0 ? ordersOpened.Max(o => o.TimeEnter) : DateTime.MinValue;
            var maxEnterTime = lastClosedEnter > lastOpenEnter ? lastClosedEnter : lastOpenEnter;
            record.LastOpen = maxEnterTime == DateTime.MinValue ? (DateTime?) null : maxEnterTime;

            return record;
        }

        private List<MarketOrder> GetAccountHistoryOrders(FarmAccount account)
        {
            var context = account.GetContext();
            byte[] buffer;
            context.GetHistoryOrdersCompressed(account.AccountId, startDate, out buffer);
            if (buffer == null || buffer.Length == 0) 
                return null;

            try
            {
                using (var reader = new SerializationReader(buffer))
                {
                    var array = reader.ReadObjectArray(typeof(MarketOrder));
                    if (array != null && array.Length > 0)
                        return array.Cast<MarketOrder>().ToList();
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetHistoryOrdersUncompressed() - serialization error", ex);
                return null;
            }
        }

        private List<MarketOrder> GetAccountOpenedOrders(FarmAccount account)
        {
            var context = account.GetContext();
            try
            {
                List<MarketOrder> orderList;
                context.GetMarketOrders(account.AccountId, out orderList);
                return orderList;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в GetMarketOrders()", ex);
            }
            return null;
        }
    }
}

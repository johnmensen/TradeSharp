using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entity;
using TradeSharp.Client.Util;
using TradeSharp.Util;

namespace TradeSharp.Client.BL
{
    class AccountStatusSummaryTable
    {
        public static void UpdateAccountInfo(Action<List<StatItem>> updateTable)
        {
            if (AccountStatus.Instance.connectionStatus != AccountConnectionStatus.Connected
                || !AccountStatus.Instance.isAuthorized ||
                AccountStatus.Instance.AccountData == null)
            {
                var statEmptyList = new List<StatItem>
                                   {
                                       new StatItem(Localizer.GetString("TitleUser"), Localizer.GetString("TitleNotAuthorized")),
                                       new StatItem(Localizer.GetString("TitleFunds"), Localizer.GetString("TitleNotAvailableShort")),
                                       new StatItem(Localizer.GetString("TitleBalance"), Localizer.GetString("TitleNotAvailableShort")),
                                       new StatItem(Localizer.GetString("TitleReserve"), Localizer.GetString("TitleNotAvailableShort"))
                                   };
                updateTable(statEmptyList);
                return;
            }
            var acData = AccountStatus.Instance.AccountData;
            var sb = new StringBuilder();
            sb.AppendLine(String.Format("{0} \"{1}\", {2}{3}",
                                        Localizer.GetString("TitleUser"),
                                        AccountStatus.Instance.Login,
                                        Localizer.GetString("TitleAccountNumberSmall"),
                                        AccountStatus.Instance.accountID));
            sb.AppendLine(String.Format("{0}: {1} {2}: {3} {4}: {5} {6}",
                                        Localizer.GetString("TitleFunds"),
                                        acData.Equity.ToStringUniformMoneyFormat(),
                                        Localizer.GetString("TitleBalance"),
                                        acData.Balance.ToStringUniformMoneyFormat(),
                                        Localizer.GetString("TitleProfit"),
                                        (acData.Equity - acData.Balance).ToStringUniformMoneyFormat(),
                                        AccountStatus.Instance.AccountData.Currency));

            var depoCurx = AccountStatus.Instance.AccountData.Currency;
            var group = DalAccountGroup.Instance.Groups.FirstOrDefault(g => g.Code == acData.Group);
            var statList = new List<StatItem>
                {
                    new StatItem(Localizer.GetString("TitleUser"), AccountStatus.Instance.Login),
                    new StatItem(Localizer.GetString("TitleAccount"), AccountStatus.Instance.accountID.ToString()),
                    new StatItem(Localizer.GetString("TitleAccountType"),
                                 @group == null
                                     ? "-"
                                     : @group.IsReal
                                           ? Localizer.GetString("TitleReal")
                                           : Localizer.GetString("TitleDemo")),
                    new StatItem(Localizer.GetString("TitleAccountGroup"), @group == null ? "-" : @group.Name),
                    new StatItem(Localizer.GetString("TitleFunds"),
                                 acData.Equity.ToStringUniformMoneyFormat() + " " + depoCurx),
                    new StatItem(Localizer.GetString("TitleBalance"),
                                 acData.Balance.ToStringUniformMoneyFormat() + " " + depoCurx),
                    new StatItem(Localizer.GetString("TitleProfit"),
                                 (acData.Equity - acData.Balance).ToStringUniformMoneyFormat() + " " + depoCurx)
                };
            updateTable(statList);
        }
    }

    internal class StatItem
    {
        public string Name { get; set; }

        public string Result { get; set; }

        public string Tag { get; set; }

        public StatItem()
        {
        }

        public StatItem(string name)
        {
            Name = name;
        }

        public StatItem(string name, string res)
        {
            Name = name;
            Result = res;
        }

        public StatItem(string name, string res, string tag)
        {
            Name = name;
            Result = res;
            Tag = tag;
        }
    }
}

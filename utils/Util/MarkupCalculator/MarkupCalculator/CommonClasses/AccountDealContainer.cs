using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace MarkupCalculator.CommonClasses
{
    public class AccountDealContainer
    {
        public static string[] SymbolsUsed { get; set; }

        public int Id { get; set; }
        public List<MarketOrder> Deals { get; set; }
        public List<BalanceChange> Transactions { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }

        private string depoCurrency = "USD";
        public string DepoCurrency
        {
            get { return depoCurrency; }
            set { depoCurrency = value; }
        }

        public AccountDealContainer(string dealFileName)
        {
            var serializer = new XmlSerializer(typeof (AccountXml));

            using (var reader = new FileStream(dealFileName, FileMode.Open))
            {
                var accountXml = (AccountXml) (serializer.Deserialize(reader));

                Id = accountXml.Id;

                Deals = new List<MarketOrder>();
                foreach (var deal in accountXml.Deals.Items.Where(deal => SymbolsUsed.Contains(deal.Symbol.Split('.')[0])))
                {
                    Deals.Add(new MarketOrder
                        {
                            #region
                            AccountID = accountXml.Id,

                            ID = deal.Ticket,
                            Symbol = deal.Symbol.Split('.')[0],
                            Comment = deal.Comment,
                            Side = deal.DealType,
                            StopLoss = deal.SL,
                            TakeProfit = deal.TP,
                            Swap = deal.Swap,

                            PriceEnter = deal.Open,
                            PriceExit = deal.Close,

                            TimeEnter =
                                deal.OpenTime.ToDateTimeUniformSafe(AccountXml.DateTimeFormatInfo) ?? DateTime.Now,
                            TimeExit =
                                deal.CloseTime.ToDateTimeUniformSafe(AccountXml.DateTimeFormatInfo) ?? DateTime.Now,

                            Volume = (int) (deal.Lots*100000)
                            #endregion
                        });
                }

                Transactions = new List<BalanceChange>
                    {
                        new BalanceChange
                            {
                                #region
                                AccountID = accountXml.Id,
                                ChangeType = BalanceChangeType.Deposit,
                                ID = 1,
                                Amount = accountXml.InitBalance,
                                Currency = depoCurrency,
                                CurrencyToDepoRate = 1.00m,
                                ValueDate =
                                    accountXml.Deals.Items.First()
                                              .OpenTime.ToDateTimeUniformSafe(AccountXml.DateTimeFormatInfo) ??
                                    DateTime.Now
                                #endregion
                            }
                    };
                foreach (var balance in accountXml.Balances.Items)
                {
                    Transactions.Add(new BalanceChange
                        {
                            #region
                            AccountID = accountXml.Id,
                            Currency = depoCurrency,
                            ChangeType = BalanceChangeType.Deposit,
                            CurrencyToDepoRate = 1.00m,
                            ID = balance.Ticket,
                            Amount = balance.Amount,
                            ValueDate =
                                balance.Date.ToDateTimeUniformSafe(AccountXml.DateTimeFormatInfo) ?? DateTime.Now
                            #endregion
                        });
                }
            }

            var minTimeEnter = Deals.Min(x => x.TimeEnter);
            var minValueDate = Transactions.Min(x => x.ValueDate);
            TimeStart = minTimeEnter < minValueDate ? minTimeEnter : minValueDate;

            var maxTimeExit = Deals.Max(x => x.TimeExit != null ? x.TimeExit.Value : new DateTime());
            var maxTimeEnter = Deals.Max(x => x.TimeEnter);
            var maxValueDate = Transactions.Max(x => x.ValueDate);
            TimeEnd = (new[] {maxTimeExit, maxTimeEnter, maxValueDate}).Max();
        }
    }
}

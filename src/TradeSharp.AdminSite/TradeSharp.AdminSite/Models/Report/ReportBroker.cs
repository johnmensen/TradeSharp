using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;

namespace TradeSharp.SiteAdmin.Models.Report
{
    /// <summary>
    /// формирует отчет по доходности брокера
    /// (от заведенных счетов до маркапа)
    /// </summary>
    public partial class ReportBroker
    {
        public DateTime StartDate { get; set; }

        public bool RealOnly { get; set; }

        public string BrokerCurrency { get; set; }

        /// <summary>
        /// Отчет строится с указанной даты, с опциональным
        /// разбиением по месяцам
        /// </summary>
        public ReportBroker(DateTime startDate, bool buildProfitPerMonth, bool processRealOnly)
        {
            StartDate = startDate;
            RealOnly = processRealOnly;
            Errors = new List<string>();
            BuildTotalReport();
        }

        private void BuildTotalReport()
        {
            var timeSpan = new Dictionary<string, double>();
            using (var ctx = DatabaseContext.Instance.Make())
            {
                ReadBrokerMetadata(ctx);

                // автоматически рассчитать дату старта
                if (StartDate == default(DateTime))
                    StartDate = ctx.POSITION.Min(p => p.TimeEnter);

                // количество заведенных за период счетов и общее количество счетов
                var accounts = ctx.ACCOUNT.ToList();
                var realAccounts = new List<ACCOUNT>();
                foreach (var account in accounts)
                {
                    AccountsCount++;
                    if (account.ACCOUNT_GROUP.IsReal)
                    {
                        if (account.TimeCreated <= StartDate) AccountsAdded++;
                        realAccounts.Add(account);
                        continue;
                    }
                    
                    AccountsDemoCount++;
                    if (account.TimeCreated <= StartDate) AccountsDemoAdded++;
                }
                
                // группы
                Groups = ctx.GetGroupsWithAccounts(null).Select(g => new GroupWithAccounts
                    {
                        Name = g.Name,
                        Code = g.Code,
                        IsReal = g.IsReal,
                        Accounts = g.Accounts ?? 0
                    }).OrderByDescending(g => g.Accounts * (g.IsReal ? 100 : 1)).ToList();
                
                // объемы - распределение по валютным парам
                var timeStart = DateTime.Now; //
                var dicTurnover = new Dictionary<string, TurnoverByActive>();
                foreach (var pos in ctx.POSITION_CLOSED.Where(p => p.TimeEnter >= StartDate))
                {
                    var dealsCount = pos.TimeExit < StartDate ? 2 : 1;
                    var volm = pos.Volume * dealsCount;

                    if (dicTurnover.ContainsKey(pos.Symbol))
                    {
                        dicTurnover[pos.Symbol].Turnover += volm;
                        dicTurnover[pos.Symbol].DealsCount += dealsCount;
                    }
                    else
                        dicTurnover.Add(pos.Symbol, new TurnoverByActive
                            {
                                DealsCount = dealsCount,
                                Ticker = pos.Symbol,
                                Turnover = volm
                            });
                }
                timeSpan.Add("pos closed", (DateTime.Now - timeStart).TotalMilliseconds); // долго блин

                foreach (var pos in ctx.POSITION.Where(p => p.TimeEnter >= StartDate && ((PositionState)p.State) == PositionState.Opened))
                {
                    if (dicTurnover.ContainsKey(pos.Symbol))
                    {
                        dicTurnover[pos.Symbol].Turnover += pos.Volume;
                        dicTurnover[pos.Symbol].DealsCount++;
                    }
                    else
                        dicTurnover.Add(pos.Symbol, new TurnoverByActive
                        {
                            DealsCount = 1,
                            Ticker = pos.Symbol,
                            Turnover = pos.Volume
                        });
                }
                // упорядочить и агрегировать оборот по сделкам
                TurnoverByPair = dicTurnover.Values.OrderByDescending(v => v.DealsCount).ToList();
                var sumCount = TurnoverByPair.Sum(d => d.DealsCount);
                if (sumCount > 1)
                    TurnoverByPair.ForEach(t => t.DealsPercent = t.DealsCount * 100f / sumCount);
                
                // заведенные-выведенные средства
                TotalIncome = new Dictionary<string, double>();
                TotalOutcome = new Dictionary<string, double>();

                foreach (var trans in ctx.GetDepoTransfers().Where(t => t.IsReal))
                {
                    var dest = trans.ChangeType == (int)BalanceChangeType.Deposit
                        ? TotalIncome : TotalOutcome;
                    if (dest.ContainsKey(trans.Currency))
                        dest[trans.Currency] = dest[trans.Currency] + (double)trans.Amount;
                    else
                        dest.Add(trans.Currency, (double)trans.Amount);
                }
                
                // слепить средства в одну таблицу
                CurrencyDepWith = new List<CurrencyDepositWithdrawal>();
                foreach (var inc in TotalIncome)
                {
                    CurrencyDepWith.Add(new CurrencyDepositWithdrawal
                        {
                            Currency = inc.Key,
                            Deposit = inc.Value
                        });
                }
                foreach (var oc in TotalOutcome)
                {
                    var curx = oc.Key;
                    var item = CurrencyDepWith.FirstOrDefault(c => c.Currency == curx);
                    if (item == null)
                    {
                        item = new CurrencyDepositWithdrawal
                            {
                                Currency = curx,
                                Withdraw = oc.Value
                            };
                        CurrencyDepWith.Add(item);
                    }
                    else
                        item.Withdraw = oc.Value;                    
                }
                CurrencyDepWith = CurrencyDepWith.OrderByDescending(c => c.Deposit).ToList();
                
                // маркап и прибыль - все по закрытым позам
                foreach (var bill in ctx.GetOrderBills())
                {
                    var isReal = bill.IsReal;

                    if (isReal)
                    {
                        TotalMarkupReal += bill.MarkupBroker;
                        CloseProfit += bill.ProfitBroker;
                        if (bill.ProfitBroker > 0)
                            CloseGrossProfit += bill.ProfitBroker;
                        else
                            CloseGrossLoss -= bill.ProfitBroker;                        
                    }
                    else
                    {
                        TotalMarkupDemo += bill.MarkupBroker;
                        CloseProfitDemo += bill.ProfitBroker;
                        if (bill.ProfitBroker > 0)
                            CloseGrossProfitDemo += bill.ProfitBroker;
                        else
                            CloseGrossLossDemo -= bill.ProfitBroker;   
                    }
                }

                // посчитать перформанс
                CalculateAccountProfit(ctx, realAccounts);
            }
        }
    
        private void ReadBrokerMetadata(TradeSharpConnection ctx)
        {
            var currencyData = ctx.ENGINE_METADATA.FirstOrDefault(r => 
                r.Category == "BROKER" && r.Name == "Currency"); // !!
            BrokerCurrency = currencyData != null ? currencyData.Value : "USD";
        }
    }
}
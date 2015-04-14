using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Xml;
using Entity;
using EntityFramework.BulkInsert.Extensions;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Linq;
using TradeSharp.Robot.BacktestServerProxy;
using TradeSharp.Robot.Robot;
using TradeSharp.Util;

namespace TradeSharp.FakeUser.BL
{
    class TradeHistoryMaker
    {
        private readonly int reverseClosedDealProb;

        private readonly int withdrawProb;

        private readonly int probWithdrawWhenLoss, probWithdrawWhenProfit;

        public readonly bool testOnly;

        private readonly int startDepo;

        private readonly DateTime startTime, endTime;

        private readonly Random rand = new Random();

        public readonly List<AccountPerformance> performance = new List<AccountPerformance>();

        public TradeHistoryMaker(int reverseClosedDealProb, int withdrawProb,
            int probWithdrawWhenLoss, int probWithdrawWhenProfit,
            bool testOnly, int startDepo, DateTime startTime, DateTime endTime)
        {
            this.reverseClosedDealProb = reverseClosedDealProb;
            this.withdrawProb = withdrawProb;
            this.probWithdrawWhenLoss = probWithdrawWhenLoss;
            this.probWithdrawWhenProfit = probWithdrawWhenProfit;
            this.testOnly = testOnly;
            this.startDepo = startDepo;
            this.startTime = startTime;
            this.endTime = endTime;
        }

        public void MakeHistory(XmlElement accountNode)
        {
            List<BaseRobot> robots;

            try
            {
                robots = MakeRobots(accountNode);
            }
            catch (Exception ex)
            {
                Logger.Error("Error making BaseRobots' list", ex);
                throw;
            }

            try
            {
                var accNode = accountNode.GetElementsByTagName("AccountId");
                if (accNode.Count == 0)
                {
                    Logger.Error("MakeHistory() - AccountId was not provided");
                    throw new Exception("MakeHistory() - AccountId was not provided");
                }
                if (accNode[0].Attributes["value"] == null)
                {
                    Logger.Error("MakeHistory() - AccountId.value was not provided");
                    throw new Exception("MakeHistory() - AccountId.value was not provided");
                }
                var accountId = accNode[0].Attributes["value"].Value.ToInt();

                List<BalanceChange> transfers;
                var accountData = GetAccountData(accountId, out transfers);
                var transfersInDbCount = transfers.Count;

                var context = MakeRobotContext(accountData, robots);
                Logger.InfoFormat("Старт теста по счету {0}", accountId);

                var totalDealsClosed = 0;
                while (true)
                {
                    DateTime modelTime, firstRealTime;
                    var cannotStep = context.MakeStep(out modelTime, out firstRealTime);
                    if (cannotStep)
                        break;

                    // пополнить / вывести средства?
                    CheckDepositWithdraw(context, transfers, modelTime);

                    if (context.PosHistory.Count > totalDealsClosed)
                    {
                        totalDealsClosed = context.PosHistory.Count;
                        var lastClosedDeal = context.PosHistory.Last();
                        if (lastClosedDeal.ResultDepo < 0) // инвертировать убыточную сделку?
                            InvertLossDeal(context, lastClosedDeal);
                    }
                    
                }

                // сохранить историю
                Logger.InfoFormat("Сохранение истории по счету {0}", accountId);
                if (!testOnly)
                    SaveTrackInDatabase(context, accountId, transfers, transfersInDbCount); 
                else
                    AddAccountPerformanceRecord(context);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка формирования истории", ex);
                throw;
            }
        }

        private void AddAccountPerformanceRecord(RobotContextBacktest context)
        {
            try
            {
                performance.Add(new AccountPerformance
                {
                    AccountId = context.AccountInfo.ID,
                    DealsCount = context.PosHistory.Count + context.Positions.Count,
                    SummaryResult = (decimal)context.PosHistory.Sum(p => p.ResultDepo)
                                    + context.AccountInfo.Equity - context.AccountInfo.Balance,
                    SummaryVolume = (long)context.PosHistory.Sum(p => p.VolumeInDepoCurrency)
                });
            }
            catch (Exception ex)
            {
                Logger.Error("Error in AddAccountPerformanceRecord()", ex);
            }            
        }

        private void InvertLossDeal(RobotContextBacktest context, MarketOrder lastClosedDeal)
        {
            if (rand.Next(100) >= reverseClosedDealProb) 
                return;
            lastClosedDeal.ResultDepo = -lastClosedDeal.ResultDepo;
            lastClosedDeal.Side = -lastClosedDeal.Side;
            lastClosedDeal.ResultPoints = -lastClosedDeal.ResultPoints;
            var delta = lastClosedDeal.ResultDepo * 2;
            context.AccountInfo.Balance += (decimal) delta;
            context.AccountInfo.Equity += (decimal) delta;
        }

        private RobotContextBacktest MakeRobotContext(ACCOUNT accountData, List<BaseRobot> robots)
        {
            var timeFrom = testOnly
                ? accountData.TimeCreated
                : accountData.TimeCreated.AddMinutes(rand.Next(60*24*9));

            try
            {
                var context = new RobotContextBacktest((tickers, end) => { })
                {
                    TimeFrom = timeFrom
                };
                context.TimeFrom = timeFrom;
                context.TimeTo = endTime;

                context.AccountInfo = new Account
                {
                    Currency = accountData.Currency,
                    Group = accountData.AccountGroup,
                    Balance = accountData.Balance,
                    Equity = accountData.Balance,
                    ID = accountData.ID
                };
                foreach (var robot in robots)
                {
                    context.SubscribeRobot(robot);
                    robot.Initialize(context, CurrentProtectedContext.Instance);
                }

                context.InitiateTest();
                return context;
            }
            catch (Exception ex)
            {
                Logger.Error("Error in MakeRobotContext()", ex);
                throw;
            }            
        }

        private static List<BaseRobot> MakeRobots(XmlElement accountNode)
        {
            var robots = new List<BaseRobot>();
            try
            {
                foreach (XmlElement robotNode in accountNode.GetElementsByTagName("robot"))
                {
                    var inodes = robotNode.SelectNodes("Robot.TypeName");
                    var inode = (XmlElement)inodes[0];
                    var title = inode.Attributes["value"].Value;
                    var robot = RobotCollection.MakeRobot(title);
                    PropertyXMLTagAttribute.InitObjectProperties(robot, robotNode, false);
                    robots.Add(robot);
                }
                return robots;
            }
            catch (Exception ex)
            {
                Logger.Error("Error in MakeRobots()", ex);
                throw;
            }            
        }

        private ACCOUNT GetAccountData(int accountId, out List<BalanceChange> transfers)
        {
            if (testOnly)
            {
                transfers = new List<BalanceChange>
                {
                    new BalanceChange
                    {
                        AccountID = accountId,
                        Amount = startDepo,
                        ChangeType = BalanceChangeType.Deposit,
                        Currency = "USD",
                        ValueDate = startTime
                    }
                };

                return new ACCOUNT
                {
                    ID = accountId,
                    Balance = startDepo,
                    Currency = "USD",
                    AccountGroup = "Demo",
                    TimeCreated = startTime
                };
            }

            ACCOUNT accountData;
            try
            {
                using (var conn = DatabaseContext.Instance.Make())
                {
                    accountData = conn.ACCOUNT.First(a => a.ID == accountId);
                    transfers = conn.BALANCE_CHANGE.Where(bc =>
                        bc.AccountID == accountId).ToList().Select(LinqToEntity.DecorateBalanceChange).ToList();
                    if (transfers.Count == 0)
                    {
                        // добавить начальное пополнение счета
                        var firstBc = new BALANCE_CHANGE
                        {
                            AccountID = accountId,
                            ValueDate = accountData.TimeCreated,
                            ChangeType = (int) BalanceChangeType.Deposit,
                            Description = "initial depo",
                            Amount = startDepo
                        };
                        conn.BALANCE_CHANGE.Add(firstBc);
                        conn.SaveChanges();

                        var pa = conn.PLATFORM_USER_ACCOUNT.First(p => p.Account == accountId);

                        var trans = new TRANSFER
                        {
                            Amount = firstBc.Amount,
                            ValueDate = firstBc.ValueDate,
                            BalanceChange = firstBc.ID,
                            Comment = "initial depo",
                            TargetAmount = firstBc.Amount,
                            User = pa.PlatformUser
                        };
                        conn.TRANSFER.Add(trans);
                        conn.SaveChanges();
                        transfers.Add(LinqToEntity.DecorateBalanceChange(firstBc));
                    }
                }
                return accountData;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Error in GetAccountData({0}): {1}", accountId, ex);
                throw;
            }
        }

        private static void SaveTrackInDatabase(RobotContextBacktest context, int accountId, 
            List<BalanceChange> transfers,
            int transfersInDbCount)
        {
            try
            {
                Logger.InfoFormat("Сохранение изменений в БД для счета {0}", accountId);

                int nextPosId;
                using (var conn = DatabaseContext.Instance.Make())
                {
                    nextPosId = Math.Max(conn.POSITION.Max(p => p.ID), conn.POSITION_CLOSED.Max(p => p.ID)) + 1;
                }

                Logger.InfoFormat("Запись {0} позиций для счета {1}",
                    context.PosHistory.Count, accountId);

                // закрытые ордера
                var listPos = new List<POSITION_CLOSED>();
                foreach (var pos in context.PosHistory)
                {
                    var orderClosed = LinqToEntity.UndecorateClosedPosition(pos);
                    orderClosed.ID = ++nextPosId;
                    listPos.Add(orderClosed);
                }

                using (var conn = DatabaseContext.Instance.Make())
                {
                    conn.BulkInsert(listPos);
                    conn.SaveChanges();
                }

                // трансферы...
                var listTrans =
                    transfers.Skip(transfersInDbCount).Select(t =>
                        new BALANCE_CHANGE
                        {
                            AccountID = accountId,
                            Amount = t.Amount,
                            ChangeType = (int) t.ChangeType,
                            ValueDate = t.ValueDate,
                        }).ToList();
                // + трансферы по закрытым ордерам
                foreach (var pos in context.PosHistory)
                {
                    var transfer = new BALANCE_CHANGE
                    {
                        AccountID = accountId,
                        Amount = (decimal) Math.Abs(pos.ResultDepo),
                        ChangeType = (int) (pos.ResultDepo >= 0
                            ? BalanceChangeType.Profit
                            : BalanceChangeType.Loss),
                        ValueDate = pos.TimeExit.Value,
                        Position = pos.ID,
                    };
                    listTrans.Add(transfer);
                }

                using (var conn = DatabaseContext.Instance.Make())
                {
                    conn.BulkInsert(listTrans);
                    conn.SaveChanges();
                }

                // открытые сделки - как есть
                using (var conn = DatabaseContext.Instance.Make())
                {
                    foreach (var pos in context.Positions)
                    {
                        var orderOpened = LinqToEntity.UndecorateOpenedPosition(pos);
                        orderOpened.ID = ++nextPosId;
                        conn.POSITION.Add(orderOpened);
                    }

                    conn.SaveChanges();
                }
                Logger.InfoFormat("Сохранение успешно для счета {0}: {1} сделок сохранено",
                    accountId, context.PosHistory.Count + context.Positions.Count);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка сохранения трека пользователя в БД", ex);
            }
        }

        private void CheckDepositWithdraw(RobotContextBacktest context, List<BalanceChange> transfers, DateTime curTime)
        {
            const int minMinutesSinceLastTransfer = 60;

            var lastTrans = transfers[transfers.Count - 1];

            var minutesSinceLastTransfer = (curTime - lastTrans.ValueDate).TotalMinutes;
            if (minutesSinceLastTransfer < minMinutesSinceLastTransfer)
                return;

            // не выводить слишком часто
            if (rand.Next(withdrawProb) > 0)
                return;

            // не выводить вне рабочих часов
            if (curTime.Hour < 8 || curTime.Hour > 19)
                if (rand.Next(100) < 85) 
                    return;

            // ввод / вывод
            var initBalance = transfers[0].Amount;
            var lessThanInit = context.AccountInfo.Equity < initBalance;
            var probWdth = lessThanInit ? probWithdrawWhenLoss : probWithdrawWhenProfit;
            var sign = rand.Next(100) < probWdth ? BalanceChangeType.Withdrawal : BalanceChangeType.Deposit;

            // объем
            var amount = Math.Ceiling(rand.Next(1, 90) * context.AccountInfo.Equity / 100);
            if (rand.Next(100) < 70)
            {
                var amount10 = (int)Math.Ceiling(amount / 10);
                amount = amount10 * 10;
            }
            var amountSigned = sign == BalanceChangeType.Deposit ? amount : -amount;

            var moneyLeft = context.AccountInfo.Equity + amountSigned;
            if (moneyLeft < 100)
            {
                return;
                sign = BalanceChangeType.Deposit;
                amountSigned = Math.Abs(amount);
            }
            
            // добавить в список
            transfers.Add(new BalanceChange
                {
                    AccountID = lastTrans.AccountID,
                    Amount = amount,
                    ChangeType = sign,
                    ValueDate = curTime
                });
            context.AccountInfo.Balance += amountSigned;
            context.AccountInfo.Equity += amountSigned;

            //Logger.InfoFormat("Счет #{0}, средства {1}: {2} {3} USD",
            //    lastTrans.AccountID, context.AccountInfo.Equity.ToStringUniformMoneyFormat(),
            //    sign, amount);
        }        
    }

    class AccountPerformance
    {
        public int AccountId { get; set; }

        public decimal SummaryResult { get; set; }

        public int DealsCount { get; set; }

        public long SummaryVolume { get; set; }

        public static string MakeTableHeader()
        {
            return "Account;Summary Result;Deals Count;Summary Volume";
        }

        public override string ToString()
        {
            return string.Format("{0};{1};{2};{3}",
                AccountId, SummaryResult.ToStringUniformMoneyFormat(),
                DealsCount, SummaryVolume);
        }
    }
}

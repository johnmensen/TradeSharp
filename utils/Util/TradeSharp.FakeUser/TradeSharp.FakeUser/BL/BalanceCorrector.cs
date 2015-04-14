using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.FakeUser.BL
{
    class BalanceCorrector
    {
        public static List<string> CorrectBalance(List<int> actIds, int minAmount, int maxAmount, int maxDelta)
        {
            var rand = new Random();
            var messages = new List<string>();
            foreach (var acId in actIds)
            {
                var accountId = acId;
                var targetAmount = rand.Next(minAmount, maxAmount);
                using (var conn = DatabaseContext.Instance.Make())
                {
                    var sumDeltaBalance = conn.BALANCE_CHANGE.Where(b => b.AccountID == accountId).Sum(b =>
                        (b.ChangeType == (int)BalanceChangeType.Loss ||
                         b.ChangeType == (int)BalanceChangeType.Withdrawal)
                            ? -b.Amount
                            : b.Amount);
                    var accountBalance = conn.ACCOUNT.Where(a => a.ID == accountId).Select(a => a.Balance).First();
                    if (accountBalance != sumDeltaBalance)
                    {
                        var acc = conn.ACCOUNT.First(a => a.ID == accountId);
                        acc.Balance = sumDeltaBalance;
                    }

                    var delta = Math.Abs(targetAmount - sumDeltaBalance);
                    if (delta > maxDelta)
                    {
                        // пополнить - вывести
                        var amount = targetAmount - sumDeltaBalance;
                        var bc = new BALANCE_CHANGE
                        {
                            AccountID = accountId,
                            ChangeType =
                                amount > 0 ? (int)BalanceChangeType.Deposit : (int)BalanceChangeType.Withdrawal,
                            Description = amount > 0 ? "rebalance (depo)" : "rebalance (wdth)",
                            ValueDate = DateTime.Now,
                            Amount = delta
                        };
                        conn.BALANCE_CHANGE.Add(bc);
                        conn.SaveChanges();

                        var ownerId = (from pa in conn.PLATFORM_USER_ACCOUNT
                                       join a in conn.ACCOUNT on pa.Account equals a.ID
                                       select pa.PlatformUser).First();

                        // проводка
                        var trans = new TRANSFER
                        {
                            Amount = delta,
                            ValueDate = DateTime.Now,
                            TargetAmount = delta,
                            BalanceChange = bc.ID,
                            Comment = bc.Description,
                            User = ownerId
                        };
                        conn.TRANSFER.Add(trans);

                        messages.Add(string.Format("#{0}: {1} -> {2} USD",
                            accountId, sumDeltaBalance.ToStringUniformMoneyFormat(),
                            targetAmount.ToStringUniformMoneyFormat()));
                    }
                    else
                    {
                        messages.Add(string.Format("#{0}: {1} USD",
                            accountId, sumDeltaBalance.ToStringUniformMoneyFormat()));
                    }
                    conn.SaveChanges();
                }
            }

            return messages;
        }
    }
}

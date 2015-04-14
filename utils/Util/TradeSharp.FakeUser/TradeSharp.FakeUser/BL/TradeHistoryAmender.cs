using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;

namespace TradeSharp.FakeUser.BL
{
    class TradeHistoryAmender
    {
        private static readonly Random rand = new Random();

        private int flipProb;

        private bool amendProfitable;

        public List<string> AmendHistory(List<int> actIds, int flipProb, bool amendProfitable)
        {
            this.flipProb = flipProb;
            this.amendProfitable = amendProfitable;
            var messages = new List<string>();
            foreach (var acId in actIds)
            {
                AmendHistory(acId, messages);
            }
            return messages;
        }

        public void AmendHistory(int acId, List<string> messages)
        {
            using (var conn = DatabaseContext.Instance.Make())
            {
                decimal dealsResult = conn.POSITION_CLOSED.Where(p => p.AccountID == acId).Sum(p => p.ResultDepo);
                // не поправлять уже профитный счет?
                if (!amendProfitable && dealsResult > 0)
                {
                    messages.Add(string.Format("счет {0} уже в профите ", acId));
                    return;
                }

                var closedDeals = conn.POSITION_CLOSED.Where(p => p.AccountID == acId && p.ResultDepo < 0).ToList();
                var deltaAmount = 0M;
                var flipCount = 0;
                foreach (var deal in closedDeals)
                {
                    if (rand.Next(0, 100) >= flipProb) continue;
                    flipCount++;
                    deltaAmount = AmendDeal(deal, conn, deltaAmount);
                }

                if (deltaAmount > 0)
                {
                    CorrectBalance(acId, deltaAmount, conn);
                }
                messages.Add(string.Format("счет {0}: {1} сделок инвертировано на сумму {2} (исходная: {3})",
                    acId, flipCount, deltaAmount, dealsResult));

                conn.SaveChanges();
            }
        }

        private static void CorrectBalance(int acId, decimal deltaAmount, TradeSharpConnection conn)
        {
            // поправить депозит
            var bc = new BALANCE_CHANGE
            {
                AccountID = acId,
                ValueDate = DateTime.Now,
                Amount = deltaAmount,
                ChangeType = (int) BalanceChangeType.Withdrawal,
                Description = "public offering"
            };
            conn.BALANCE_CHANGE.Add(bc);
            conn.SaveChanges();
            var userId = conn.PLATFORM_USER_ACCOUNT.First(a => a.Account == acId).PlatformUser;
            var tr = new TRANSFER
            {
                ValueDate = bc.ValueDate,
                BalanceChange = bc.ID,
                Amount = -deltaAmount,
                Comment = "public offering",
                User = userId,
                TargetAmount = - deltaAmount
            };
            conn.TRANSFER.Add(tr);
        }

        private static decimal AmendDeal(POSITION_CLOSED deal, TradeSharpConnection conn, decimal deltaAmount)
        {
            // "поправить" сделку
            deal.Side = - deal.Side;
            deal.ResultDepo = - deal.ResultDepo;
            var bc = conn.BALANCE_CHANGE.FirstOrDefault(b => b.Position == deal.ID);
            if (bc != null)
            {
                bc.ChangeType = (int) BalanceChangeType.Profit;
                var trans = conn.TRANSFER.FirstOrDefault(t => t.BalanceChange == bc.ID);
                deltaAmount += (bc.Amount*2);
                if (trans != null)
                {
                    trans.Amount = -trans.Amount;
                    trans.TargetAmount = -trans.TargetAmount;
                }
            }
            return deltaAmount;
        }
    }
}

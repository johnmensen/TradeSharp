using System;
using System.Linq;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;

namespace TradeSharp.Test.Moq
{
    static class AccountShareTestDataMaker
    {
        public static void MakePammData(TradeSharpConnection conn,
            out ACCOUNT testAccount, out PLATFORM_USER shareOwner, out ACCOUNT eurAccount)
        {
            testAccount = conn.ACCOUNT.First(a => a.Balance > 0
                && a.POSITION.Count > 0 && a.ACCOUNT_SHARE.Count == 0 && a.ACCOUNT_GROUP.IsReal);
            var ownerId = testAccount.PLATFORM_USER_ACCOUNT.First().PlatformUser;

            shareOwner = conn.PLATFORM_USER.First(u => u.ID == ownerId);
            var shareOwnerId = shareOwner.ID;
            var shareOwners = conn.PLATFORM_USER.Where(u => u.ID != shareOwnerId).Take(2).ToArray();
            conn.ACCOUNT_SHARE.Add(new ACCOUNT_SHARE
            {
                Account = testAccount.ID,
                Share = 50,
                ShareOwner = shareOwner.ID
            });
            conn.ACCOUNT_SHARE.Add(new ACCOUNT_SHARE
            {
                Account = testAccount.ID,
                Share = 30,
                ShareOwner = shareOwners[0].ID
            });
            conn.ACCOUNT_SHARE.Add(new ACCOUNT_SHARE
            {
                Account = testAccount.ID,
                Share = 20,
                ShareOwner = shareOwners[1].ID
            });

            // счет в евро
            eurAccount = new ACCOUNT
            {
                Balance = 10500,
                Currency = "EUR",
                AccountGroup = conn.ACCOUNT_GROUP.First(g => !g.IsReal).Code,
                TimeCreated = DateTime.Now,
                Status = (int)Account.AccountStatus.Created
            };
            conn.ACCOUNT.Add(eurAccount);
            conn.SaveChanges();

            // сделки по евровому счету
            conn.POSITION.Add(new POSITION
            {
                AccountID = eurAccount.ID,
                Side = (int)DealType.Sell,
                Symbol = "EURUSD",
                State = (int)PositionState.Opened,
                TimeEnter = eurAccount.TimeCreated,
                PriceEnter = (decimal)(Contract.Util.BL.QuoteStorage.Instance.ReceiveValue("EURUSD").bid + 0.002f)
            });
            conn.SaveChanges();
        }
    }
}

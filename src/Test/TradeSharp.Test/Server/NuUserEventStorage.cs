using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Test.Moq;
using TradeSharp.Util;

namespace TradeSharp.Test.Server
{
    [TestFixture]
    public class UserEventStorage
    {
        private static readonly Random rnd = new Random();

        private TradeSharpConnectionPersistent connectionPersistent;

        private List<UserEvent> sampleUserEvents;

        private int?[] accountIds;
        private PLATFORM_USER[] targetUsers;

        [TestFixtureSetUp]
        public void Setup()
        {
            // настроить фейковую БД из файлов CSV
            SetupDatabase();

            // получить из БД десяток счетов и их владельцев
            using (var ctx = DatabaseContext.Instance.Make())
            {
                var accounts = (from account in ctx.ACCOUNT select (int?) account.ID).Take(5).ToList();
                accountIds = accounts.Concat(Enumerable.Range(0, 10).Select(r => (int?)null)).OrderBy(a => rnd.Next()).ToArray();
                targetUsers = (from us in ctx.PLATFORM_USER select us).Take(10).ToArray();                
            }

            const int eventsCount = 1000;
            var timeStart = DateTime.Now.AddSeconds(-eventsCount);

            sampleUserEvents = new List<UserEvent>();            
            for (var i = 0; i < eventsCount; i++)
            {
                var evt = new UserEvent
                    {
                        AccountId = accountIds[i % accountIds.Length],
                        Action = AccountEventAction.DefaultAction,
                        Time = timeStart.AddSeconds(i),
                        User = targetUsers[i % targetUsers.Length].ID
                    };
                evt.Code = evt.AccountId.HasValue ? AccountEventCode.TradeSignal : AccountEventCode.AccountModified;
                evt.Text = evt.Code + " " + i;
                evt.Title = EnumFriendlyName<AccountEventCode>.GetString(evt.Code);
                sampleUserEvents.Add(evt);
            }
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            connectionPersistent.Cleanup();
        }

        private void SetupDatabase()
        {
            connectionPersistent = TradeSharpConnectionPersistent.InitializeTradeSharpConnection();
        }

        [Test]
        public void TestStatic()
        {
            try
            {
                TradeSharp.Server.BL.UserEventStorage.Instance.PushEvents(sampleUserEvents);
                var usEvents = TradeSharp.Server.BL.UserEventStorage.Instance.GetUserEvents(targetUsers[0].Login);
                var realEvtCount = sampleUserEvents.Count(e => e.User == targetUsers[0].ID);
                Assert.AreEqual(realEvtCount, usEvents.Count, "UserEventStorage - got all pushed events");
                usEvents = TradeSharp.Server.BL.UserEventStorage.Instance.GetUserEvents(targetUsers[0].Login);
                Assert.AreEqual(0, usEvents.Count, "UserEventStorage - subsequent request gives 0 records");

                usEvents = TradeSharp.Server.BL.UserEventStorage.Instance.GetUserEvents(targetUsers[1].Login);
                realEvtCount = sampleUserEvents.Count(e => e.User == targetUsers[1].ID);
                Assert.AreEqual(realEvtCount, usEvents.Count, "UserEventStorage - got all pushed events one more time");
            }
            catch (Exception ex)
            {
                Assert.Fail("Ошибка в TestStatic: " + ex);
            }
        }
    }
}

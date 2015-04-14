using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using TradeSharp.Server.BL;

namespace TradeSharp.Test.Server
{
    [TestFixture]
    public class NuUserSettingsStorage
    {
        [Test]
        public void TestReadWrite()
        {
            var storage = new UserSettingsStorage();
            storage.SaveUserSettings(100, "s100");
            var sets = storage.LoadUserSettings(100);
            Assert.IsNotNullOrEmpty(sets, "TestReadWrite - данные должны быть прочитаны");
            Assert.AreEqual("s100", sets, "TestReadWrite - данные должны быть прочитаны корректно");
        }

        [Test]
        public void TestReadWriteMultyThread()
        {
            var storage = new UserSettingsStorage();
            var users = new [] {1, 2, 4, 10, 15, 21, 1011, 1012};
            var sets = users.Select(u => "sets" + u).ToArray();

            Parallel.Invoke(users.Select((u, i) => new Action(() =>
                {
                    storage.SaveUserSettings(u, sets[i]);
                    Thread.Sleep(50);
                })).ToArray());

            var setsRead = users.Select(storage.LoadUserSettings).ToArray();

            Assert.IsTrue(setsRead.SequenceEqual(sets), "TestReadWrite (многопоточный) - данные сохранены а затем прочитаны корректно");
        }
    }
}

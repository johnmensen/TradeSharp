using System;
using System.Linq;
using NUnit.Framework;
using TradeSharp.SiteAdmin.Models.Items;

namespace TradeSharp.AdminSite.Test.DifficultTests
{
    /// <summary>
    /// тесты для AccountGroupDb
    /// </summary>
    partial class NuDbAccess
    {
        [Test]
        public void GetAccountGroupsNull()
        {
            var accountGroups = accountGroupsRepository.GetAccountGroups().ToArray();

            Assert.AreEqual(4, accountGroups.Length);
            FieldValidation(accountGroups[0]);
        }


        [Test]
        public void GetAccountGroups()
        {
            var accountGroup = accountGroupsRepository.GetAccountGroups("CFHABD").ToArray();

            Assert.AreEqual(1, accountGroup.Length);
            FieldValidation(accountGroup[0]);
        }

        /// <summary>
        /// Проверяет - заполненны ли все необходимые поля (это больше от регрессионных ошибок)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static void FieldValidation(AccountGroupItem item)
        {
            if (item == null) throw new ArgumentNullException("item");
            Assert.IsNotNull(item);
            Assert.IsNotNull(item.Code);
            Assert.IsNotNull(item.Name);
            Assert.IsNotNull(item.IsReal);
            Assert.IsNotNull(item.BrokerLeverage);
            Assert.IsNotNull(item.MarginCallPercentLevel);
            Assert.IsNotNull(item.StopoutPercentLevel);
            Assert.IsNotNull(item.AccountsCount);
            Assert.IsNotNull(item.Dealer);
            Assert.IsNotNull(item.MessageQueue);
            Assert.IsNotNull(item.SessionName);
            Assert.IsNotNull(item.HedgingAccount);
            Assert.IsNotNull(item.SenderCompId);
            Assert.IsNotNull(item.SwapFree);
            Assert.IsNotNull(item.Markup);
            Assert.IsNotNull(item.DefaultMarkupPoints);
            Assert.IsNotNull(item.DefaultVirtualDepo);
            Assert.IsNotNull(item.Dealers);
        }
    }
}

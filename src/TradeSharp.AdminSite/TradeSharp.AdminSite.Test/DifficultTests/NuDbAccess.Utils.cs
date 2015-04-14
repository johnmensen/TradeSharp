using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NUnit.Framework;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.SiteAdmin.Models.CommonClass;
using TradeSharp.SiteAdmin.Models.Items;
using TradeSharp.Util;

namespace TradeSharp.AdminSite.Test.DifficultTests
{
    partial class NuDbAccess
    {
        /// <summary>
        /// Проверяем метод простой выбрки из указанной БД по передаваемому лямбда выражению
        /// На случай регриссионных ошибок, если вдруг будет меняться логика метода SelectAllValuesFromTable
        /// </summary>
        [Test]
        public void NuSelectAllValuesFromTable()
        {
            // ReSharper disable RedundantAssignment
            var commodityActual = -1;
            // ReSharper restore RedundantAssignment
            var commodityExpected1 = Utils.SelectAllValuesFromTable<COMMODITY>(x => new SelectListItem { Text = x.Title, Value = x.Title });
            var commodityExpectedCount = commodityExpected1.Count;

            using (var ctx = connectionPersistent)
            {
                commodityActual = ctx.COMMODITY.Count();
            }

            Assert.IsNotNull(commodityExpectedCount);
            Assert.AreEqual(commodityActual, commodityExpectedCount);

            var commodityExpected2 = Utils.SelectAllValuesFromTable<COMMODITY>(null);
            Assert.IsNotNull(commodityExpected2);
            Assert.AreEqual(new List<SelectListItem>(), commodityExpected2);

            DatabaseContext.InitializeFake(exceptionConnectionPersistent);
            exceptionConnectionPersistent.TestException = new EntityException("DB context has failed");
            var commodityExpected3 = Utils.SelectAllValuesFromTable<COMMODITY>(null);
            Assert.IsNotNull(commodityExpected3);
            Assert.AreEqual(new List<SelectListItem>(), commodityExpected3);


            DatabaseContext.InitializeFake(exceptionConnectionPersistent);
            exceptionConnectionPersistent.TestException = new Exception("DatabaseContext Exception");
            var commodityExpected4 = Utils.SelectAllValuesFromTable<COMMODITY>(null);
            Assert.IsNotNull(commodityExpected4);
            Assert.AreEqual(new List<SelectListItem>(), commodityExpected4);
        }

        [Test]
        public void CheckExistValuesFromTable()
        {
            using (var ctx = connectionPersistent)
            {
                PLATFORM_USER accId = null;
                try
                {
                    accId = ctx.PLATFORM_USER.First();
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Тестирование невозможно. В таблице ACCOUNT тестовой БД не найдело записей.", ex);
                    Assert.Fail("Тестирование невозможно. В таблице ACCOUNT файловой БД не найдело записей.");
                }

                var emailExist = Utils.CheckExistValuesFromTable<PLATFORM_USER>(x => x.Email == accId.Email);
                Assert.IsTrue(emailExist);

                var emailNotExist = Utils.CheckExistValuesFromTable<PLATFORM_USER>(x => x.Email == "notExisting@mail.com");
                Assert.IsFalse(emailNotExist);

                DatabaseContext.InitializeFake(exceptionConnectionPersistent);
                exceptionConnectionPersistent.TestException = new EntityException("DB context has failed");
                var commodityExpected1 = Utils.CheckExistValuesFromTable<COMMODITY>(null);
                Assert.IsNotNull(commodityExpected1);
                Assert.IsFalse(commodityExpected1);


                DatabaseContext.InitializeFake(exceptionConnectionPersistent);
                exceptionConnectionPersistent.TestException = new Exception("DatabaseContext Exception");
                var commodityExpected2 = Utils.CheckExistValuesFromTable<COMMODITY>(null);
                Assert.IsNotNull(commodityExpected2);
                Assert.IsFalse(commodityExpected2);
            }
        }

        /// <summary>
        /// Тестируется преобразование коллекции, полученной из хранимой процедуры GetPositionList в список из PositionItem.
        /// Этот тест регриссионный т.к. сейчас там всё просто, но немного хардкодно (свойства присваиваются "руками")
        /// </summary>
        [Test]
        public void NuDecoratPositionItems()
        {
            using (var ctx = connectionPersistent)
            {
                var accId = -1;
                try
                {
                    accId = ctx.ACCOUNT.First().ID;
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Тестирование невозможно. В таблице ACCOUNT тестовой БД не найдело записей.", ex);
                    Assert.Fail("Тестирование невозможно. В таблице ACCOUNT файловой БД не найдело записей.");
                }

                int totalCountPositeion;
                var allPositions = ctx.GetPositionListWrapped(10, accId, null, null, null, null, null, null, null, null, out totalCountPositeion);
                Assert.IsNotNull(allPositions);
                Assert.LessOrEqual(allPositions.Count, 10);

                var allPositions1 = Utils.DecoratPositionItems(allPositions);
                Assert.IsNotNull(allPositions1);
                Assert.AreEqual(allPositions.Count, allPositions1.Count);

                var allPositions2 = Utils.DecoratPositionItems(null);
                Assert.IsNotNull(allPositions2);
                Assert.AreEqual(new List<PositionItem>(), allPositions2);

                var allPositions3 = Utils.DecoratPositionItems(new List<GetPositionList_Result>());
                Assert.IsNotNull(allPositions3);
                Assert.AreEqual(new List<PositionItem>(), allPositions3);

                DatabaseContext.InitializeFake(exceptionConnectionPersistent);
                exceptionConnectionPersistent.TestException = new Exception("DatabaseContext Exception");
                var commodityExpected = Utils.DecoratPositionItems(new List<GetPositionList_Result> { null, null, null });
                Assert.IsNotNull(commodityExpected);
                Assert.AreEqual(new List<PositionItem>(), commodityExpected);
            }
        }

        /// <summary>
        /// Тестируем генерирование человеко-понятного описания какого либо типа
        /// </summary>
        [Test]
        public void NuGetTypeDescription()
        {
            var typeDescription1 = Utils.GetTypeDescription(typeof(int));
            var typeDescription2 = Utils.GetTypeDescription(typeof(PositionExitReason));
            var typeDescription3 = Utils.GetTypeDescription(typeof(int?));
            var typeDescription4 = Utils.GetTypeDescription(typeof(MarketOrder));

            Assert.IsNotNull(typeDescription1);
            Assert.IsNotNull(typeDescription2);
            Assert.IsNotNull(typeDescription3);
            Assert.IsNotNull(typeDescription4);

            Assert.AreEqual(new Tuple<string, string>("int", Resource.TypeDescriptionInt), typeDescription1);
            Assert.AreEqual(new Tuple<string, string>("PositionExitReason", Resource.TypeDescriptionEnum + " (0, 1, 2...)"), typeDescription2);
            Assert.AreEqual(new Tuple<string, string>("int", Resource.TypeDescriptionInt), typeDescription3);
            Assert.AreEqual(new Tuple<string, string>("MarketOrder", string.Format("{0}, {1} MarketOrder", Resource.TextUnknownDataType, Resource.TextFailedGetDescriptionFor)), typeDescription4);
        }

        [Test]
        public void NuGetDataToFillDropDownListForAccountView()
        {
            using (var ctx = connectionPersistent)
            {
                var res = Utils.GetDataToFillDropDownListForAccountView();
                Assert.IsTrue(res.Keys.Contains("listTickers"));
                Assert.IsTrue(res.Keys.Contains("listGroups"));
                Assert.IsTrue(res.Keys.Contains("listUserRights"));
                Assert.IsTrue(res.Keys.Contains("listUserRoles"));

                var tikers = res["listTickers"] as List<SelectListItem>;
                var groups = res["listGroups"] as List<SelectListItem>;

                Assert.IsNotNull(tikers);
                Assert.IsNotNull(groups);

                Assert.AreEqual(ctx.COMMODITY.Count(), tikers.Count);
                Assert.AreEqual(ctx.ACCOUNT_GROUP.Count(), groups.Count);
            }
        }
    }
}
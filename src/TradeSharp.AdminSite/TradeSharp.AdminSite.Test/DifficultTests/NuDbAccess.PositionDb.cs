using System;
using System.Collections.Generic;
using System.Linq;
using Entity;
using NUnit.Framework;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Helper;
using TradeSharp.SiteAdmin.Models;
using TradeSharp.Util;

namespace TradeSharp.AdminSite.Test.DifficultTests
{
    /// <summary>
    /// тесты для PositionRepository
    /// </summary>
    partial class NuDbAccess
    {
        /// <summary>
        /// Тест первичного запроса списка сделок, без каких либо фильтров.
        /// </summary>
        [Test]
        public void NuGetPositionListByAccountId()
        {
            var accId = -1;

            using (var ctx = connectionPersistent)
            {
                try
                {
                    accId = ctx.ACCOUNT.First().ID;
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Тестирование невозможно. В таблице ACCOUNT тестовой БД не найдело записей.", ex);
                    Assert.Fail("Тестирование невозможно. В таблице ACCOUNT файловой БД не найдело записей.");
                }
            }

            var positionList1 = positionRepository.GetPositionList(accId);

            Assert.NotNull(positionList1);
            Assert.NotNull(positionList1.Positions);


            var positionList2 = positionRepository.GetPositionList(-1);

            Assert.NotNull(positionList2);
            Assert.NotNull(positionList2.Positions);

            DatabaseContext.InitializeFake(exceptionConnectionPersistent);
            var positionList3 = positionRepository.GetPositionList(-1);
            Assert.NotNull(positionList3);
            Assert.Null(positionList3.Positions);
        }

        /// <summary>
        /// Тест запроса списка сделок, с применением фильтров.
        /// </summary>
        [Test]
        public void NuGetPositionListByItem()
        {
            var accId = -1;

            using (var ctx = connectionPersistent)
            {
                try
                {
                    accId = ctx.ACCOUNT.First().ID;
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Тестирование невозможно. В таблице ACCOUNT тестовой БД не найдело записей.", ex);
                    Assert.Fail("Тестирование невозможно. В таблице ACCOUNT файловой БД не найдело записей.");
                }
            }


            var positionList1 = new PositionListModel
            {
                AccountId = accId,
            };

            var positionListResult1 = positionRepository.GetPositionList(positionList1);
            Assert.NotNull(positionListResult1);
            Assert.NotNull(positionListResult1.Positions);

            var positionList2 = new PositionListModel
            {
                AccountId = accId,
                Side = DealType.Buy
            };

            var positionListResult2 = positionRepository.GetPositionList(positionList2);
            Assert.NotNull(positionListResult2);
            Assert.NotNull(positionListResult2.Positions);


            var positionList3 = new PositionListModel
            {
                AccountId = accId,
                Status = PositionState.Closed
            };

            var positionListResul3 = positionRepository.GetPositionList(positionList3);
            Assert.NotNull(positionListResul3);
            Assert.NotNull(positionListResul3.Positions);


            var positionListResul4 = positionRepository.GetPositionList(null);
            Assert.NotNull(positionListResul4);
            Assert.IsNull(positionListResul4.Positions);
        }

        [Test]
        public void GetPositionItemDetails()
        {
            List<POSITION> positions = null;
            List<POSITION_CLOSED> closePositions = null;
            var unvalidPositionIds = new List<int> { -1, -2, -3, -4, -5 };


            using (var ctx = connectionPersistent)
            {
                try
                {
                    positions = ctx.POSITION.Take(100).ToList();
                    closePositions = ctx.POSITION_CLOSED.Take(100).ToList();
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("GetPositionItemDetails() ошибка обращения к файловой БД", ex);
                    Assert.Fail("GetPositionItemDetails() ошибка обращения к файловой БД");
                }

                Assert.IsTrue(positions.Any(), "Тестирование невозможно. В таблице POSITION тестовой БД не найдело необходимых записей.");
                Assert.IsTrue(closePositions.Any(), "Тестирование невозможно. В таблице POSITION_CLOSED тестовой БД не найдело необходимых записей.");
            }

            foreach (var pos in positions)
            {
                var positionDetails = positionRepository.GetPositionItemDetails(pos.ID);

                Assert.IsNotNull(positionDetails);
                Assert.AreEqual(positionDetails.AccountID, pos.AccountID);
                Assert.AreEqual(positionDetails.PriceEnter, pos.PriceEnter);
                Assert.AreEqual(positionDetails.TimeEnter, pos.TimeEnter);
                Assert.AreEqual(positionDetails.Symbol, pos.Symbol);
                Assert.AreEqual(positionDetails.Volume, pos.Volume);
                Assert.IsFalse(positionDetails.IsClosed, pos.ID.ToString());
                Assert.IsTrue(positionDetails.State == PositionState.Opened ||
                                positionDetails.State == PositionState.StartOpened ||
                                positionDetails.State == PositionState.StartClosed);
                Assert.IsNull(positionDetails.PriceExit);
                Assert.IsNull(positionDetails.TimeExit);
            }


            foreach (var pos in closePositions)
            {
                var positionDetails = positionRepository.GetPositionItemDetails(pos.ID);

                Assert.IsNotNull(positionDetails);
                Assert.AreEqual(positionDetails.AccountID, pos.AccountID);
                Assert.AreEqual(positionDetails.PriceEnter, pos.PriceEnter);
                Assert.AreEqual(positionDetails.TimeEnter, pos.TimeEnter);
                Assert.AreEqual(positionDetails.Symbol, pos.Symbol);
                Assert.AreEqual(positionDetails.Volume, pos.Volume);
                Assert.IsFalse(positionDetails.IsOpened);
                Assert.IsTrue(positionDetails.IsClosed);
                Assert.AreEqual(positionDetails.State, PositionState.Closed);
                Assert.AreEqual(positionDetails.PriceExit, pos.PriceExit);
                Assert.AreEqual(positionDetails.TimeExit, pos.TimeExit);
            }

            foreach (var posId in unvalidPositionIds)
            {
                var positionDetails = positionRepository.GetPositionItemDetails(posId);
                Assert.IsNull(positionDetails);
            }
        }

        /// <summary>
        /// Метод 'GetPositionsById' простой, но в нём есть несколько самопальных преобразований
        /// </summary>
        [Test]
        public void NuGetPositionsById()
        {
            int[] arrId;
            int[] mixId;

            using (var ctx = connectionPersistent)
            {
                var openPosId = ctx.POSITION.Select(x => x.ID);
                var closPosId = ctx.POSITION_CLOSED.Select(x => x.ID).Take(50);

                arrId = openPosId.Concat(closPosId).ToArray();
                mixId = arrId.Take(2).Concat(new[] { -1, -2, -3 }).ToArray();
            }

            // валидные параметры
            var actualPositionValidParams = positionRepository.GetPositionsById(arrId);
            Assert.AreNotEqual(actualPositionValidParams, null, "GetPositionsById() вернул null при валидный параметрах");
            Assert.AreEqual(arrId.Length, actualPositionValidParams.Count, "GetPositionsById() вернул не верный результат при валидный параметрах");

            // не валидные параметры
            var actualPositionUnvalidParams = positionRepository.GetPositionsById(new[] { -1, -20, -18 });
            Assert.AreNotEqual(actualPositionValidParams, null, "GetPositionsById() вернул null при не валидный параметрах");
            Assert.AreEqual(0, actualPositionUnvalidParams.Count, "GetPositionsById() вернул не верный результат при не валидный параметрах");

            // null параметры
            var actualPositionNullParams = positionRepository.GetPositionsById(null);
            Assert.AreNotEqual(actualPositionValidParams, null, "GetPositionsById() вернул null при null параметрах");
            Assert.AreEqual(0, actualPositionNullParams.Count, "GetPositionsById() вернул не верный результат при null параметрах");

            // параметры вперемешку
            var actualPositionMixParams = positionRepository.GetPositionsById(mixId);
            Assert.AreNotEqual(actualPositionMixParams, null, "GetPositionsById() вернул null при 'перемешанных' параметрах");
            Assert.AreEqual(2, actualPositionMixParams.Count, "GetPositionsById() вернул не верный результат при 'перемешанных' параметрах");
        }

        [Test]
        public void NuUpdateSavePositionItem()
        {
            List<POSITION> positions = null;
            List<POSITION_CLOSED> closePositions = null;
            try
            {
                using (var ctx = connectionPersistent)
                {
                    var account = ctx.ACCOUNT.First(x => x.AccountGroup.ToLower() == "demo");
                    positions = account.POSITION.Take(3).ToList();
                    closePositions = account.POSITION_CLOSED.Take(3).ToList();
                }
                Assert.IsTrue(positions.Any(), "Тестирование невозможно. В таблице POSITION тестовой БД не найдело необходимых записей.");
                Assert.IsTrue(closePositions.Any(), "Тестирование невозможно. В таблице POSITION_CLOSED тестовой БД не найдело необходимых записей.");
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("NuUpdateSavePositionItem() ошибка обращения к файловой БД", ex);
                Assert.Fail("NuUpdateSavePositionItem() ошибка обращения к файловой БД");
            }

            var systemProperty = new List<SystemProperty>
                {
                    new SystemProperty
                        {
                            PropertyType = typeof(int),
                            SystemName = "Takeprofit",
                            IsDanger = false,
                            Value = "10"
                        }
                };
            var resOpened = positionRepository.UpdateSavePositionItem(string.Join(", ", positions.Select(x => x.ID)), systemProperty, PositionState.Opened);
            Assert.IsTrue(resOpened);
            foreach (var position in positions)
            {
                Assert.NotNull(position);
                Assert.AreEqual(10, position.Takeprofit);
            }

            var resClosed = positionRepository.UpdateSavePositionItem(string.Join(", ", closePositions.Select(x => x.ID)), systemProperty, PositionState.Closed);
            Assert.IsTrue(resClosed);
            foreach (var position in closePositions)
            {
                Assert.NotNull(position);
                Assert.AreEqual(10, position.Takeprofit);
            }

            // Неадекватный параметр
            var unValidSystemProperty = new List<SystemProperty>
                {
                    new SystemProperty
                        {
                            PropertyType = typeof(int),
                            SystemName = "unValidSystemProperty",
                            IsDanger = false,
                            Value = "unValid"
                        }
                };

            
            //DatabaseContext.InitializeFake(null);
            var resUnvalid = positionRepository.UpdateSavePositionItem(string.Join(", ", closePositions.Select(x => x.ID)), unValidSystemProperty, PositionState.Closed);
            Assert.IsTrue(resUnvalid);  
            foreach (var position in closePositions)
            {
                Assert.NotNull(position);
                Assert.AreEqual(10, position.Takeprofit);
            }
            
        }
    }
}
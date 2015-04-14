using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Test.Entity
{
    [TestFixture]
    public class NuOrderFilterAndSortOrder
    {
        [Test]
        public void TestFilter()
        {
            var orders = MakeOrders();

            var filter = new OrderFilterAndSortOrder
            {
                filterExpertComment = "FX",
                filterTicker = "USDCAD",
                filterMagic = 5,
                sortAscending = false,
                sortByTimeEnter = true,
                takeCount = 2
            };

            var filtered = filter.ApplyFilter(orders.AsQueryable()).ToList();

            Assert.AreEqual(2, filtered.Count, "OrderFilterAndSortOrder - должен вернуть ровно 2 записи");
            Assert.AreEqual(2, filtered[0].ID, "OrderFilterAndSortOrder - должны быть отсортированы по убыванию времени входа");
        }

        private List<MarketOrder> MakeOrders()
        {
            return new List<MarketOrder>
                {
                    new MarketOrder
                        {
                            ID = 1,
                            ExpertComment = "FX",
                            Symbol = "USDCAD",
                            Magic = 5,
                            TimeEnter = new DateTime(2014, 3, 11, 20, 5, 51)
                        },
                    new MarketOrder
                        {
                            ID = 2,
                            ExpertComment = "FX",
                            Symbol = "USDCAD",
                            Magic = 5,
                            TimeEnter = new DateTime(2014, 4, 10, 20, 5, 51)
                        },
                    new MarketOrder
                        {
                            ID = 3,
                            ExpertComment = "FX",
                            Symbol = "USDCAD",
                            Magic = 0,
                            TimeEnter = new DateTime(2014, 4, 10, 20, 5, 51)
                        },
                    new MarketOrder
                        {
                            ID = 4,
                            ExpertComment = "FX",
                            Symbol = "EURCAD",
                            Magic = 5,
                            TimeEnter = new DateTime(2014, 4, 10, 20, 5, 51)
                        },
                    new MarketOrder
                        {
                            ID = 5,
                            ExpertComment = "FX",
                            Symbol = "USDCAD",
                            Magic = 5,
                            TimeEnter = new DateTime(2014, 2, 10, 20, 5, 51)
                        },
                };
        }
    }
}

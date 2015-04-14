using System;
using System.Linq;
using NUnit.Framework;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Test.Client
{
    [TestFixture]
    public class NuNewsMap
    {
        [Test]
        public void TestEquality()
        {
            var a = MakeTestMap();
            var b = MakeTestMap();
            Assert.IsTrue(a.AreSame(b), "a = a");

            b.records = b.records.Select(r => new NewsMapRecord(r.date.AddMinutes(3), 
                r.recordsCount)).ToArray();
            
            Assert.IsFalse(a.AreSame(b), "a != b");
        }

        [Test]
        public void TestSaveLoad()
        {
            var filePath = ExecutablePath.ExecPath + "\\newsMap.xml";
            var map = MakeTestMap();

            try
            {
                map.SaveInFile(filePath);
            }
            catch (Exception ex)
            {
                Assert.Fail("Failed to save map: {0}", ex);
            }

            NewsMap mapLoaded = null;
            try
            {
                mapLoaded = NewsMap.LoadFromFile(filePath);
            }
            catch (Exception ex)
            {
                Assert.Fail("Failed to load map: {0}", ex);
            }
            
            Assert.IsNotNull(mapLoaded, "Map loaded - not null");
            Assert.IsTrue(map.AreSame(mapLoaded), "loaded map is unchanged");
        }

        [Test]
        public void TestDifference()
        {
            var mapCli = new NewsMap
                {
                    channelIds = new[] {1, 3},
                    records = new[]
                        {
                            new NewsMapRecord(new DateTime(2013, 12, 2), 141),
                            new NewsMapRecord(new DateTime(2013, 12, 3), 102),
                            new NewsMapRecord(new DateTime(2013, 12, 4), 151),
                            new NewsMapRecord(new DateTime(2013, 12, 5), 151),
                            new NewsMapRecord(new DateTime(2013, 12, 6), 150),
                        }
                };
            var mapSrv = MakeTestMap();
            NewsMap mapDiff = null;
            try
            {
                mapDiff = mapCli.MakeMapOfLackedNews(mapSrv);
            }
            catch (Exception ex)
            {
                Assert.Fail("Failed to make NewsMap diff: {0}", ex);
            }

            Assert.AreEqual(5, mapDiff.channelIds[0], "lacked channels");
            Assert.AreEqual(3, mapDiff.records.Length, "records lacked - count is OK");
        }
    
        private static NewsMap MakeTestMap()
        {
            return new NewsMap
            {
                channelIds = new[] { 1, 5 },
                records = new[]
                        {
                            new NewsMapRecord(new DateTime(2013, 12, 1), 1),
                            new NewsMapRecord(new DateTime(2013, 12, 2), 141),
                            new NewsMapRecord(new DateTime(2013, 12, 3), 141),
                            new NewsMapRecord(new DateTime(2013, 12, 4), 151),
                            new NewsMapRecord(new DateTime(2013, 12, 5), 151),
                            new NewsMapRecord(new DateTime(2013, 12, 6), 150),
                            new NewsMapRecord(new DateTime(2013, 12, 7), 151),
                        }
            };
        }
    }
}

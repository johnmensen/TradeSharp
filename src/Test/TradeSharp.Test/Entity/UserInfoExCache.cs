using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.Components.DictionaryAdapter;
using Moq;
using NUnit.Framework;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using System.Drawing;
using TradeSharp.SiteBridge.Lib.Distribution;
using TradeSharp.Util;

namespace TradeSharp.Test.Entity
{
    [TestFixture]
    class NuUserInfoExCache
    {
        private Mock<IAccountStatistics> source;
        private UserInfoExCache cache; // объект проверки
        private UserInfoEx dbInfo;
        private readonly List<string> calls = new List<string>();
            
        [TestFixtureSetUp]
        public void StartTests()
        {
            source = new Mock<IAccountStatistics>();
            source.Setup(s => s.GetUserInfo(It.IsAny<int>())).Returns<int>(GetUserInfo);
            source.Setup(s => s.GetUsersBriefInfo(It.IsAny<List<int>>())).Returns<List<int>>(GetUsersBriefInfo);
            source.Setup(s => s.SetUserInfo(It.IsAny<UserInfoEx>())).Returns<UserInfoEx>(SetUserInfo);
            source.Setup(s => s.ReadFiles(It.IsAny<List<string>>())).Returns<List<string>>(ReadFiles);
            source.Setup(s => s.WriteFile(It.IsAny<string>(), It.IsAny<byte[]>())).Returns<string, byte[]>(WriteFile);
        }

        [Test]
        public void GetUserInfo()
        {
            dbInfo = new UserInfoEx
            {
                Id = 1,
                AvatarBigFileName = "AvatarBig.png",
                AvatarSmallFileName = "AvatarSmall.png",
                AboutFileName = "About.rtf",
                Contacts = "my@mail.ru",
                AvatarBig = SystemIcons.Information.ToBitmap(),
                AvatarSmall = SystemIcons.Warning.ToBitmap(),
                About = "About"
            };

            cache = new UserInfoExCache(source.Object);
            var path = ExecutablePath.ExecPath + "\\files";
            Assert.IsTrue(Directory.Exists(path), "cache directory " + path + " not found");

            // 1st test
            // db with 3 files, cache is empty
            calls.Clear();
            cache.GetUserInfo(1);
            Assert.AreEqual(4, calls.Count, "1: request count mismatch");
            Assert.IsTrue(calls.Contains("GetUserBriefInfo 1"), "1: GetUserBriefInfo not found");
            Assert.IsTrue(calls.Contains("ReadFile " + dbInfo.AvatarBigFileName), "1: ReadFile AvatarBigFileName not found");
            Assert.IsTrue(calls.Contains("ReadFile " + dbInfo.AvatarSmallFileName), "1: ReadFile AvatarSmallFileName not found");
            Assert.IsTrue(calls.Contains("ReadFile " + dbInfo.AboutFileName), "1: ReadFile AboutFileName not found");

            // 2nd test
            // db with 3 files, cache is filled
            calls.Clear();
            cache.GetUserInfo(1);
            Assert.AreEqual(1, calls.Count, "2: request count mismatch");
            Assert.IsTrue(calls.Contains("GetUserBriefInfo 1"), "2: GetUserBriefInfo not found");

            // 3rd test
            // 1 file is changed in db, cache is filled
            dbInfo.AvatarBig = SystemIcons.Error.ToBitmap();
            calls.Clear();
            cache.GetUserInfo(1);
            Assert.AreEqual(2, calls.Count, "3: request count mismatch");
            Assert.IsTrue(calls.Contains("GetUserBriefInfo 1"), "3: GetUserBriefInfo not found");
            Assert.IsTrue(calls.Contains("ReadFile " + dbInfo.AvatarBigFileName), "3: ReadFile AvatarBigFileName not found");

            // 4th test
            // 1 file deleted from db, then added, cache is filled
            dbInfo.AvatarBigFileName = null;
            dbInfo.AvatarBig = null;
            calls.Clear();
            cache.GetUserInfo(1);
            Assert.AreEqual(1, calls.Count, "4.1: request count mismatch");
            Assert.IsTrue(calls.Contains("GetUserBriefInfo 1"), "4.1: GetUserBriefInfo not found");
            calls.Clear();
            dbInfo.AvatarBigFileName = "AvatarBig.png";
            dbInfo.AvatarBig = SystemIcons.Error.ToBitmap();
            cache.GetUserInfo(1);
            Assert.AreEqual(1, calls.Count, "4.2: request count mismatch");
            Assert.IsTrue(calls.Contains("GetUserBriefInfo 1"), "4.2: GetUserBriefInfo not found");
        }

        [Test]
        public void SetUserInfo()
        {
            cache = new UserInfoExCache(source.Object);
            dbInfo = new UserInfoEx {Id = 1};
            var info = new UserInfoEx
                {
                    Id = 1,
                    Contacts = "my@mail.ru",
                    AvatarBig = SystemIcons.Information.ToBitmap(),
                    AvatarSmall = SystemIcons.Warning.ToBitmap(),
                    About = "About"
                };

            // 1st test
            // db is empty, cache is empty
            calls.Clear();
            cache.SetUserInfo(info); // info with 3 files
            Assert.AreEqual(2, calls.Count, "1: request count mismatch");
            Assert.IsTrue(calls.Contains("GetUserBriefInfo 1"), "1: GetUserBriefInfo not found");
            Assert.IsTrue(calls.Contains("SetUserInfo 1"), "1: SetUserInfo not found");

            // 2nd test
            // db with 3 files, cache is filled
            info.AvatarBig = SystemIcons.Error.ToBitmap();
            calls.Clear();
            cache.SetUserInfo(info); // info with 3 files, 1 file changed
            Assert.AreEqual(3, calls.Count, "2: request count mismatch");
            Assert.IsTrue(calls.Contains("GetUserBriefInfo 1"), "2: GetUserBriefInfo not found");
            Assert.IsTrue(calls.Contains("WriteFile " + dbInfo.AvatarBigFileName), "2: GetUserBriefInfo not found");
            Assert.IsTrue(calls.Contains("SetUserInfo 1"), "2: SetUserInfo not found");

            // 3rd test
            // db with 3 files, cache is filled
            info.AvatarBig = null;
            info.AvatarSmall = null;
            info.About = null;
            calls.Clear();
            cache.SetUserInfo(info); // info with no files
            Assert.AreEqual(1, calls.Count, "3: request count mismatch");
            Assert.IsTrue(calls.Contains("SetUserInfo 1"), "3: SetUserInfo not found");
        }

        [Test]
        public void SetVoidUserInfo()
        {
            var sourceVoid1 = new Mock<IAccountStatistics>();
            sourceVoid1.Setup(s => s.GetUserInfo(It.IsAny<int>())).Returns<int>(GetUserInfo);
            sourceVoid1.Setup(s => s.GetUsersBriefInfo(It.IsAny<List<int>>())).Returns<List<int>>(ids =>
            {
                calls.Add(string.Format("GetUserBriefInfo {0}", 1));
                return null;
            });
            sourceVoid1.Setup(s => s.SetUserInfo(It.IsAny<UserInfoEx>())).Returns<UserInfoEx>(SetUserInfo);
            sourceVoid1.Setup(s => s.ReadFiles(It.IsAny<List<string>>())).Returns<List<string>>(names => null);
            sourceVoid1.Setup(s => s.WriteFile(It.IsAny<string>(), It.IsAny<byte[]>())).Returns<string, byte[]>(WriteFile);

            cache = new UserInfoExCache(sourceVoid1.Object);

            calls.Clear();
            cache.SetUserInfo(new UserInfoEx
            {
                Id = 1,
                Contacts = "my@mail.ru",
                AvatarBig = SystemIcons.Information.ToBitmap(),
                AvatarSmall = SystemIcons.Warning.ToBitmap(),
                About = "About"
            });

            Assert.IsTrue(calls.Contains("GetUserBriefInfo 1"), "2: GetUserBriefInfo not found");


            dbInfo = new UserInfoEx
            {
                Id = 1,
                AvatarBigFileName = "AvatarBig.png",
                AboutFileName = "About.rtf",
                Contacts = "my@mail.ru",
                AvatarBig = SystemIcons.Information.ToBitmap(),
                AvatarSmall = SystemIcons.Warning.ToBitmap(),
                About = "About"
            };
            sourceVoid1.Setup(s => s.GetUsersBriefInfo(It.IsAny<List<int>>())).Returns<List<int>>(GetUsersBriefInfo);
            cache = new UserInfoExCache(sourceVoid1.Object);
            cache.SetUserInfo(new UserInfoEx
            {
                Id = 1,
                Contacts = "my@mail.ru",
                AvatarBig = SystemIcons.Information.ToBitmap(),
                AvatarSmall = SystemIcons.Warning.ToBitmap(),
                About = "About"

            });
        }

        /// <summary>
        /// Вызываем метод GetUserInfo при инициализированном null-ом поля dataSource в UserInfoExCache
        /// </summary>
        [Test]
        public void GetUserInfoException()
        {
            cache = new UserInfoExCache(null);

            calls.Clear();
            cache.GetUserInfo(1);
            Assert.AreEqual(0, calls.Count, "1: request count mismatch");
        }

        /// <summary>
        /// Тестируем метод GetUsersInfo с разными пользователя (в том числе и null)
        /// </summary>
        [Test]
        public void GetVoidUserInfo()
        {
            var sourceVoid = new Mock<IAccountStatistics>();
            sourceVoid.Setup(s => s.GetUserInfo(It.IsAny<int>())).Returns<int>(GetUserInfo);
            sourceVoid.Setup(s => s.GetUsersBriefInfo(It.IsAny<List<int>>())).Returns<List<int>>(ids =>
            {
                calls.Add(string.Format("GetUserBriefInfo {0}", 1));
                return new List<UserInfoEx> { null };
            });
            sourceVoid.Setup(s => s.SetUserInfo(It.IsAny<UserInfoEx>())).Returns<UserInfoEx>(SetUserInfo);
            sourceVoid.Setup(s => s.ReadFiles(It.IsAny<List<string>>())).Returns<List<string>>(names => null);
            sourceVoid.Setup(s => s.WriteFile(It.IsAny<string>(), It.IsAny<byte[]>())).Returns<string, byte[]>(WriteFile);

            cache = new UserInfoExCache(sourceVoid.Object);

            //TODO Эти тесты почти ничего не тестируют
            calls.Clear();
            var res1 = cache.GetUsersInfo(new List<int> { 1 });
            Assert.IsNull(res1);
            Assert.IsTrue(calls.Contains("GetUserBriefInfo 1"), "GetUserBriefInfo not found");

            calls.Clear();
            var res2 = cache.GetUsersInfo(null);
            Assert.IsNull(res2);
            Assert.AreEqual(0, calls.Count);
        }


        [TestFixtureTearDown]
        public void StopTests()
        {
            Directory.Delete(ExecutablePath.ExecPath + "\\files", true);
        }

        private UserInfoEx GetUserInfo(int id)
        {
            calls.Add(string.Format("GetUserInfo {0}", id));
            return id != dbInfo.Id ? null : dbInfo;
        }

        private UserInfoEx SetUserInfo(UserInfoEx info)
        {
            calls.Add(string.Format("SetUserInfo {0}", info.Id));
            if (info.Id != dbInfo.Id)
                return null;
            if (info.AvatarBigHashCode != dbInfo.AvatarBigHashCode)
                dbInfo.AvatarBig = info.AvatarBig;
            if (info.AvatarSmallHashCode != dbInfo.AvatarSmallHashCode)
                dbInfo.AvatarSmall = info.AvatarSmall;
            if (info.AboutHashCode != dbInfo.AboutHashCode)
                dbInfo.About = info.About;
            dbInfo.Contacts = info.Contacts;
            if (dbInfo.AvatarBig != null)
            {
                if (string.IsNullOrEmpty(dbInfo.AvatarBigFileName))
                    dbInfo.AvatarBigFileName = "AvatarBig.png";
            }
            else
                dbInfo.AvatarBigFileName = null;
            if (dbInfo.AvatarSmall != null)
            {
                if (string.IsNullOrEmpty(dbInfo.AvatarSmallFileName))
                    dbInfo.AvatarSmallFileName = "AvatarSmall.png";
            }
            else
                dbInfo.AvatarBigFileName = null;
            if (dbInfo.About != null)
            {
                if (string.IsNullOrEmpty(dbInfo.AboutFileName))
                    dbInfo.AboutFileName = "About.png";
            }
            else
                dbInfo.AboutFileName = null;
            var result = new UserInfoEx
                {
                    Id = dbInfo.Id,
                    AvatarBigFileName = dbInfo.AvatarBigFileName,
                    AvatarSmallFileName = dbInfo.AvatarSmallFileName,
                    AboutFileName = dbInfo.AboutFileName,
                    Contacts = dbInfo.Contacts,
                    AvatarBigHashCode = dbInfo.AvatarBigHashCode,
                    AvatarSmallHashCode = dbInfo.AvatarSmallHashCode,
                    AboutHashCode = dbInfo.AboutHashCode
                };
            return result;
        }

        private List<UserInfoEx> GetUsersBriefInfo(List<int> ids)
        {
            if (ids == null)
            {
                calls.Add("GetUsersBriefInfo null");
                return null;
            }
            var result = new List<UserInfoEx>();
            foreach (var id in ids)
            {
                calls.Add(string.Format("GetUserBriefInfo {0}", id));
                if (id == dbInfo.Id)
                    result.Add(new UserInfoEx
                        {
                            Id = dbInfo.Id,
                            AvatarBigFileName = dbInfo.AvatarBigFileName,
                            AvatarSmallFileName = dbInfo.AvatarSmallFileName,
                            AboutFileName = dbInfo.AboutFileName,
                            Contacts = dbInfo.Contacts,
                            AvatarBigHashCode = dbInfo.AvatarBigHashCode,
                            AvatarSmallHashCode = dbInfo.AvatarSmallHashCode,
                            AboutHashCode = dbInfo.AboutHashCode
                        });
            }
            return result;
        }

        private List<byte[]> ReadFiles(List<string> names)
        {
            var result = new List<byte[]>();
            foreach (var name in names)
            {
                calls.Add("ReadFile " + name);
                if (name == dbInfo.AvatarBigFileName)
                    result.Add(dbInfo.AvatarBigData);
                else if(name == dbInfo.AvatarSmallFileName)
                    result.Add(dbInfo.AvatarSmallData);
                else if(name == dbInfo.AboutFileName)
                    result.Add(dbInfo.AboutData);
                else
                    result.Add(null);
            }
            return result;
        }

        private bool WriteFile(string name, byte[] data)
        {
            calls.Add("WriteFile " + name);
            if (name == dbInfo.AvatarBigFileName)
            {
                dbInfo.AvatarBigData = data;
                return true;
            }
            if (name == dbInfo.AvatarSmallFileName)
            {
                dbInfo.AvatarSmallData = data;
                return true;
            }
            if (name == dbInfo.AboutFileName)
            {
                dbInfo.AboutData = data;
                return true;
            }
            return false;
        }
    }
}
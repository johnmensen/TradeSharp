using System;
using System.Data.EntityClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using NUnit.Framework;
using Ninject;
using Ninject.Web.Mvc;
using TradeSharp.AdminSite.Test.App_Start;
using TradeSharp.AdminSite.Test.MOQ;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Contract;
using TradeSharp.SiteAdmin.Repository;

namespace TradeSharp.AdminSite.Test.DifficultTests
{
    [TestFixture]
    partial class NuDbAccess
    {
        private IKernel kernel;

        private TradeSharpConnectionPersistent connectionPersistent;
        private TradeSharpConnectionPersistent exceptionConnectionPersistent;

        private IAccountRepository accountRepository;
        private IAccountGroupsRepository accountGroupsRepository;
        private IDealerRepository dealerRepository;
        private IPositionRepository positionRepository;

        [TestFixtureSetUp]
        public void Setup()
        {
            var request = new SimpleWorkerRequest("", "", "", null, new StringWriter());
            var context = new HttpContext(request);
            //context.
            //HttpContext.Current = context;


            kernel = new StandardKernel(new TestServiceModule());
            DependencyResolver.SetResolver(new NinjectDependencyResolver(kernel));

            accountRepository = new AccountRepository();
            accountGroupsRepository = new AccountGroupsRepository();
            dealerRepository = new DealerRepository();
            positionRepository = new PositionRepository();
        }

        [TestFixtureTearDown]
        public void Teardown()
        {
            connectionPersistent.Cleanup();
        }

        [SetUp]
        public void SetupMethods()
        {
            EntityConnection connection;
            connectionPersistent = TradeSharpConnectionPersistent.InitializeTradeSharpConnection(out connection);
            exceptionConnectionPersistent = new TradeSharpConnectionPersistent(connection)
            {
                TestException = new Exception("DB context has failed")
            };
            DatabaseContext.InitializeFake(connectionPersistent);
        }

        /// <summary>
        /// Метод проверяет адекватность выпадающих списков для фильтров, на случай предупреждения регриссионных ошибок
        /// </summary>
        [Test]
        public void NuGetAccountViewModel()
        {
            /*
            var accViewModel = AccountDb.GetAccountViewModel(accountRepository.GetFilterAccountFromServer(null));

            var commidityFilterItemCount = -1;
            var accountGroupFilterItemCount = -1;
            using (var ctx = connectionPersistent)
            {
                commidityFilterItemCount = ctx.COMMODITY.Count() + 1;
                accountGroupFilterItemCount = ctx.ACCOUNT_GROUP.Count() + 1;
            }

            Assert.AreEqual(accViewModel.FilterBalanceTickers.Count(), commidityFilterItemCount);
            Assert.AreEqual(accViewModel.FilterGroups.Count(), accountGroupFilterItemCount);
            */
        }     

        /// <summary>
        /// Тестируем редактирование пользовательского аккаунта (класс AccountUserModel)
        /// </summary>
        [Test]
        public void NuEditUserInfo()
        {
            /*
            var currentDateTime = DateTime.Now;

            var userId = -1;
            using (var ctx = connectionPersistent)
            {
                userId = ctx.PLATFORM_USER.First().ID;
            }
            var user = UserDb.GetUserInfoById(userId);

            user.Title = "TestTitle";
            user.UserEmail = "Test@test.test";
            user.UserName = "TestName";
            user.UserPatronym = "TestPatronym";
            user.UserSurname = "TestSurname";
            user.UserDescription = "TestDescription";
            user.UserLogin = "TestLogin";
            user.UserPassword = "TestPassword";
            user.UserPhone1 = "11111111";
            user.UserPhone2 = "11111111";
            user.UserRoleMask = UserRole.Администратор;
            user.UserRightsMask = new Dictionary<int, UserAccountRights>()
                {
                    {1, UserAccountRights.Просмотр}
                };
            user.UserRegistrationDate = currentDateTime;
            UserDb.EditUserInfo(user);


            var userNew = UserDb.GetUserInfoById(userId);

            Assert.IsNullOrEmpty(userNew.Title);
            Assert.AreEqual("Test@test.test", userNew.UserEmail);
            Assert.AreEqual("TestName", userNew.UserName);
            Assert.AreEqual("TestPatronym", userNew.UserPatronym);
            Assert.AreEqual("TestSurname", userNew.UserSurname);
            Assert.AreEqual("TestDescription", userNew.UserDescription);
            Assert.AreEqual("TestLogin", userNew.UserLogin);
            Assert.AreEqual("TestPassword", userNew.UserPassword);
            Assert.AreEqual("11111111", userNew.UserPhone1);
            Assert.AreEqual("11111111", userNew.UserPhone2);
            Assert.AreEqual(UserRole.Администратор, userNew.UserRoleMask);
            //Assert.AreEqual(1, userNew.UserRightsMask.Keys.Count);
            //Assert.AreEqual(UserAccountRights.Просмотр, userNew.UserRightsMask.First().Value);
            Assert.AreNotEqual(currentDateTime, userNew.UserRegistrationDate);
            */
        }

        /// <summary>
        /// Тестируем формирование модели для представления назначения и редактирования владельцев счетов
        /// </summary>
        [Test]
        public void NuGetAccountOwnerModel()
        {
            using (var ctx = connectionPersistent)
            {
                // Валидные параметры

                var ownerId = ctx.PLATFORM_USER_ACCOUNT.First().PlatformUser;
                var accountOwnerModel1 = accountRepository.GetAccountOwnerModel(null, ownerId);
                var itemsCount1 = accountOwnerModel1.Accounts.Count;
                var realItemsCount1 = ctx.PLATFORM_USER_ACCOUNT.Count(x => x.PlatformUser == ownerId);
                Assert.AreEqual(itemsCount1, realItemsCount1);


                var acountId = ctx.PLATFORM_USER_ACCOUNT.First().Account;
                var accountOwnerModel2 = accountRepository.GetAccountOwnerModel(acountId, null);
                var itemsCount2 = accountOwnerModel2.Accounts.Count;
                var realItemsCount2 = ctx.PLATFORM_USER_ACCOUNT.Count(x => x.Account == acountId);
                Assert.AreEqual(itemsCount2, realItemsCount2);

                // Невалидные параметры

                var accountOwnerModelError1 = accountRepository.GetAccountOwnerModel(acountId, ownerId);
                Assert.IsNotNull(accountOwnerModelError1.Accounts);
                Assert.IsNotNull(accountOwnerModelError1.Owners);
                Assert.IsNotNull(accountOwnerModelError1.AccountOwnerPartialViewType);

                Assert.AreEqual(0, accountOwnerModelError1.Accounts.Count);
                Assert.AreEqual(0, accountOwnerModelError1.Owners.Count);


                var accountOwnerModelError2 = accountRepository.GetAccountOwnerModel(null, null);
                Assert.IsNotNull(accountOwnerModelError2.Accounts);
                Assert.IsNotNull(accountOwnerModelError2.Owners);
                Assert.IsNotNull(accountOwnerModelError2.AccountOwnerPartialViewType);

                Assert.AreEqual(0, accountOwnerModelError2.Accounts.Count);
                Assert.AreEqual(0, accountOwnerModelError2.Owners.Count);
            }
        }

        /// <summary>
        /// Тестируем метод назначения и редактирования владельцев счетов
        /// </summary>
        [Test]
        public void NuEditAccountOwnerModel()
        {
            using (var ctx = connectionPersistent)
            {
                var owner = ctx.PLATFORM_USER_ACCOUNT.First();
                var accountId = ctx.PLATFORM_USER_ACCOUNT.Where(x => x.PlatformUser == owner.PlatformUser).Select(x => x.Account).ToList();
                var countRecords = accountId.Count;
                var newAccountID = ctx.PLATFORM_USER_ACCOUNT.First(x => !accountId.Contains(x.Account)).Account;

                // Валидное добавление 
                accountRepository.EditAccountOwnerModel(newAccountID, owner.PlatformUser, 0, "add");

                var testRecordsAfterAdd = ctx.PLATFORM_USER_ACCOUNT.Where(x => x.PlatformUser == owner.PlatformUser).ToList();
                var countRecordsAfterAdd = testRecordsAfterAdd.Count;

                Assert.AreEqual(countRecords + 1, countRecordsAfterAdd);


                // Валидное удаление
                accountRepository.EditAccountOwnerModel(newAccountID, owner.PlatformUser, 0, "del");
                var testRecordsAfterDel = ctx.PLATFORM_USER_ACCOUNT.Where(x => x.PlatformUser == owner.PlatformUser).ToList();
                var countRecordsAfterDel = testRecordsAfterDel.Count;

                Assert.AreEqual(countRecords, countRecordsAfterDel);


                // Не валидное добавление
                // Пытаемся второй раз добавить такой же аккаутн
                var newAccountErrorID = ctx.PLATFORM_USER_ACCOUNT.First(x => accountId.Contains(x.Account)).Account;
                accountRepository.EditAccountOwnerModel(newAccountErrorID, owner.PlatformUser, 0, "add");
                var testRecordsAfterErrorAdd = ctx.PLATFORM_USER_ACCOUNT.Where(x => x.PlatformUser == owner.PlatformUser).ToList();
                var countRecordsAfterErrorAdd = testRecordsAfterErrorAdd.Count;

                Assert.AreEqual(countRecords, countRecordsAfterErrorAdd); // Количество записей не должно измениться

                // Не валидное удаление
                // Пытаемся удалить несуществующую запись
                accountRepository.EditAccountOwnerModel(-10, owner.PlatformUser, 0, "del");
                var testRecordsAfterErrorDell = ctx.PLATFORM_USER_ACCOUNT.Where(x => x.PlatformUser == owner.PlatformUser).ToList();
                var countRecordsAfterErrorDell = testRecordsAfterErrorDell.Count;

                Assert.AreEqual(countRecords, countRecordsAfterErrorDell); // Количество записей не должно измениться
            }
        }     
    }
}
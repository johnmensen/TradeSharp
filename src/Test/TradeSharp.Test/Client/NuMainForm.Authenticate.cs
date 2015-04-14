using System;
using NUnit.Framework;
using TradeSharp.Client;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Proxy;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Test.Moq;
using TradeSharp.Util;

namespace TradeSharp.Test.Client
{
    [TestFixture]
    public class NuMainForm
    {
        delegate AuthenticationResponse AuthenticateDel(string login, string hash,
            string terminalVersion, long terminalId, long clientTime, out int sessionTag);

        private MainForm mainFormDebug;
        private ITradeSharpServerTrade fakeTradeServer;
        private string methodAuthenticate;
        private string methodSelectAccount;

        AuthenticationResponse response;
        string authStatusString;

        [TestFixtureSetUp]
        public void TestSetup()
        {
            Localizer.ResourceResolver = new MockResourceResolver();
            mainFormDebug = new MainForm(true);
            fakeTradeServer = ProxyBuilder.Instance.GetImplementer<ITradeSharpServerTrade>();

            int currentSessionTag;
            methodAuthenticate = ProxyBuilder.GetMethodName<ITradeSharpServerTrade>(a => a.Authenticate("", "", "", 0, 0, out currentSessionTag));
            methodSelectAccount = ProxyBuilder.GetMethodName<ITradeSharpServerTrade>(a => a.SelectAccount(null, 0));

            TradeSharpServerTradeProxy.fakeProxy = fakeTradeServer;
        }

        [TestFixtureTearDown]
        public void TestTeardown()
        {
            Localizer.ResourceResolver = null;
        }

        [SetUp]
        public void Setup()
        {
            MainForm.serverProxyTrade = new TradeSharpServerTrade(mainFormDebug);
            ((IMockableProxy)fakeTradeServer).MockMethods.Clear();
        }

        [TearDown]
        public void Teardown()
        {

        }

        [Test]
        public void TestAuthenticateWithOutConnection()
        {
            MainForm.serverProxyTrade = null;
            var result = mainFormDebug.Authenticate("", "", out response, out authStatusString);

            Assert.IsFalse(result, "must be 'False' when 'MainForm.serverProxyTrade = null'");
            Assert.AreEqual(AuthenticationResponse.ServerError, response, "must be 'ServerError' when 'MainForm.serverProxyTrade = null'");
            // TODO: тестировать локализацию
            Assert.AreEqual("theEnumAuthenticationResponseServerError", authStatusString, "must be 'Ошибка сервера' when 'MainForm.serverProxyTrade = null'");
        }

        [Test]
        public void TestAuthenticatePasswordFail()
        {
            ((IMockableProxy)fakeTradeServer).MockMethods.Add(methodAuthenticate,
                new AuthenticateDel((string login, string hash, string terminalVersion, long terminalId, long clientTime, out int sessionTag) =>
                {
                    sessionTag = 0;
                    return AuthenticationResponse.WrongPassword;
                }));

            var result = mainFormDebug.Authenticate("", "", out response, out authStatusString);
            Assert.IsFalse(result, "must be 'False' when wrong password");
            Assert.AreEqual(AuthenticationResponse.WrongPassword, response, "must be 'WrongPassword' when wrong password");
            // TODO: тестировать локализацию
            Assert.AreEqual("theEnumAuthenticationResponseWrongPassword", authStatusString, "must be 'Неверный пароль' when wrong password");
        }

        [Test]
        public void TestAuthenticateOk()
        {
            ((IMockableProxy)fakeTradeServer).MockMethods.Add(methodAuthenticate,
                new AuthenticateDel((string login, string hash, string terminalVersion, long terminalId, long clientTime, out int sessionTag) =>
                {
                    sessionTag = 0;
                    return AuthenticationResponse.OK;
                }));

            var result = mainFormDebug.Authenticate("", "", out response, out authStatusString);
            Assert.True(result, "must be 'True' when all 'Ok'");
            Assert.AreEqual(AuthenticationResponse.OK, response, "must be 'OK' when all 'Ok'");
            Assert.AreEqual("Connected", authStatusString, "must be 'Connected' when all 'Ok'");
        }   

        [Test]
        public void SelectAccount()
        {
            Assert.IsFalse(mainFormDebug.SelectAccount(0), "must be 'False' when sessionTag = 0");

            CurrentProtectedContext.Instance.OnAuthenticated(1);
            ((IMockableProxy)fakeTradeServer).MockMethods.Add(methodSelectAccount,
                new Func<ProtectedOperationContext, int, bool>((ctx, accountId) => true));

            Assert.IsTrue(mainFormDebug.SelectAccount(0), "must be 'True' when All 'Ok'");
        }
    }
}

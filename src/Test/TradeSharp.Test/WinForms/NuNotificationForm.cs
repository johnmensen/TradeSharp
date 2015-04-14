using System.Globalization;
using System.IO;
using System.Windows.Forms;
using NUnit.Framework;
using TradeSharp.Util.NotificationControl;


namespace TradeSharp.Test.WinForms
{
    #if CI
    #else
    //[TestFixture]
    #endif
    public class NuNotificationForm
    {
        private string testFileFolderPath = string.Empty;

        #if CI
        #else
        [TestFixtureSetUp]
        #endif
        public void TestSetup()
        {
            var directoryName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", ""));
            if (directoryName != null)
                testFileFolderPath = Path.Combine(directoryName.Replace("\\bin\\Debug", ""), "AnyFilesToTest\\NotificationForm");
            
        }

        #if CI
        #else
        [TestFixtureTearDown]
        #endif
        public void TestTeardown()
        {
        }

        #if CI
        #else
        [SetUp]
        #endif
        public void Setup()
        {
        }

        #if CI
        #else
        [TearDown]
        #endif
        public void Teardown()
        {

        }

        #if CI
        #else
        //[Test]
        #endif
        public void ShowMessage()
        {
            const string testText = "Основной текст сообщения Основной текст сообщения Основной текст " +
                                    "сообщения Основной текст сообщения Основной текст сообщения Основной " +
                                    "текст сообщения Основной текст сообщения Основной текст сообщения Основной " +
                                    "текст сообщения Основной текст сообщения Основной текст сообщения Основной " +
                                    "текст сообщения Основной текст сообщения Основной текст сообщения Основной " +
                                    "текст сообщения Основной текст сообщения Основной текст сообщения Основной " +
                                    "текст сообщения Основной текст сообщения Основной текст сообщения Основной " +
                                    "текст сообщения Основной текст сообщения Основной текст сообщения Основной " +
                                    "текст сообщения Основной текст сообщения Основной текст сообщения Основной " +
                                    "текст сообщения Основной текст сообщения Основной текст сообщения Основной " +
                                    "текст сообщения Основной текст сообщения Основной текст сообщения Основной " +
                                    "текст сообщения Основной текст сообщения Основной текст сообщения Основной " +
                                    "текст сообщения Основной текст сообщения Основной текст сообщения Основной " +
                                    "текст сообщения Основной текст сообщения Основной текст сообщения";

            bool repeatNotification;
            var res1 = NotificationBox.Show(testText, "Заголовок", out repeatNotification);
            var res2 = NotificationBox.Show(testText, "Заголовок", MessageBoxIcon.Error, out repeatNotification);
            var res3 = NotificationBox.Show(testText, "Заголовок", MessageBoxButtons.OKCancel, out repeatNotification);
            var res4 = NotificationBox.Show(testText, "Заголовок", MessageBoxButtons.AbortRetryIgnore, out repeatNotification);
            var res5 = NotificationBox.Show(testText, "Заголовок", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Warning, out repeatNotification);
        }
    }
}

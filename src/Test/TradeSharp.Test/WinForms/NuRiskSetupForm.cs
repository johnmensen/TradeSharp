using System;
using System.Linq;
using NUnit.Framework;
using TestStack.White.UIItems;
using TradeSharp.Client.Forms;
using TradeSharp.Util;
//using QuoteStorage = TradeSharp.Contract.Util.BL.QuoteStorage;
//using Application = TestStack.White.Application;

namespace TradeSharp.Test.WinForms
{
    /// <summary>
    /// Тестирование формы RiskSetupForm и её контролов
    /// </summary>
    #if CI
    #else
    [TestFixture]
    #endif
    public class NuRiskSetupForm : WinFormTest
    {
        #region Контролы с панели riskSetupControl из формы RiskSetupForm
        private Panel riskSetupControl;
        private WinFormTextBox tbTickerCount;
        private WinFormTextBox tbDealByTickerCount;
        private WinFormTextBox tbLeverage;
        private WinFormTextBox tbBalance;
        private WinFormTextBox tbOrderLeverage;
        private WinFormTextBox tbResultedVolume;
        private WinFormTextBox tbResultRounded;
        private TestStack.White.UIItems.ListBoxItems.ComboBox cbTicker;
        #endregion

        #if CI
        #else
        [TestFixtureSetUp]
        #endif
        public void SetupMethods()
        {
            //QuoteStorage.Instance.UpdateValues("EURUSD", new QuoteData(1.3520f, 1.3522f, DateTime.Now));
            //TradeSharpDictionary.Initialize(MoqTradeSharpDictionary.Mock);
        }

        #if CI
        #else
        [TestFixtureTearDown]
        #endif
        public void TearDownMethods()
        {
        }

        #if CI
        #else
        [SetUp]
        #endif
        public void SetupTest()
        {
            if (!InitEmptyApplication(typeof(RiskSetupForm).AssemblyQualifiedName))
                Assert.Fail("Не удалось запустить 'пустое' приложение для тестирования.");
            try
            {
                var windows = application.GetWindows();

                if (windows == null || windows.Count == 0)
                {
                    Logger.InfoFormat("TestStack did not found any window, but process has: {0}", 
                        application.Process.MainWindowTitle);
                }

                Assert.IsNotNull(windows, "NuRiskSetupForm - no forms at all");
                Assert.Greater(windows.Count, 0, "NuRiskSetupForm - no forms at all (0 count)");
                window = windows[0]; //application.GetWindow("Настройки риска", InitializeOption.NoCache);
                Logger.Debug("Окно RiskSetupForm приложения открыто");
            }
            catch (Exception ex)
            {
                application.Kill();
                Logger.Error("не удалось открыть окно приложения. ", ex);
                Assert.Fail("не удалось открыть окно приложения. " + ex.Message);
            }

            InitUiControls();
        }

        #if CI
        #else
        [TearDown]
        #endif
        public void TearDownTest()
        {
            if (application == null) return;

            try
            {
                application.Kill();
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Не удалось корректно закрыть 'Пустое' приложение.", ex);
            }
        }

        /// <summary>
        /// Тестирование ввода данных в TextBox-ы контрола RiskSetupControl
        /// </summary>
        #if CI
        #else
        [Test]
        #endif
        public void RiskSetupControlTextBoxTest()
        {
            var tickerCount = 4;
            var dealByTicker = 3;
            var totalLeverage = 24;
            var balance = 100000M;
            const string ticker = "EURUSD";
            
            tbBalance.Text = balance.ToStringUniform();
            tbTickerCount.Text = tickerCount.ToString();
            tbDealByTickerCount.Text = dealByTicker.ToString();
            tbLeverage.Text = totalLeverage.ToString();
            cbTicker.SetValue(ticker);
            
            var expectedDealLeverage = totalLeverage/(decimal) dealByTicker/tickerCount;

            // ожидать завершения расчета и обновления контролов
            System.Threading.Thread.Sleep(250);
            var resultedDealLevrg = tbOrderLeverage.Text.ToDecimalUniform();
            Assert.AreEqual(expectedDealLeverage, resultedDealLevrg, "RiskSetupControl - плечо сделки посчитано неверно");
            var volumeCalculated = tbResultedVolume.Text.ToDecimalUniformSafe() ?? 0;
            //Assert.AreNotEqual(0, volumeCalculated, "RiskSetupControl - объем сделки посчитан правильно");
        }

        /// <summary>
        /// Инициалиация контролов формы, которые будем тестировать.
        /// </summary>
        private void InitUiControls()
        {
            riskSetupControl = window.Items.FirstOrDefault(x => x.Id == "riskSetupControl") as Panel;
            Assert.NotNull(riskSetupControl, string.Format("Ссылка на элемент {0} равна null.", riskSetupControl));

            tbTickerCount = riskSetupControl.Items.FirstOrDefault(x => x.Id == "tbTickerCount") as WinFormTextBox;
            Assert.NotNull(tbTickerCount, string.Format("Ссылка на элемент {0} равна null.", tbTickerCount));

            tbDealByTickerCount = riskSetupControl.Items.FirstOrDefault(x => x.Id == "tbDealByTickerCount") as WinFormTextBox;
            Assert.NotNull(tbDealByTickerCount, string.Format("Ссылка на элемент {0} равна null.", tbDealByTickerCount));

            tbLeverage = riskSetupControl.Items.FirstOrDefault(x => x.Id == "tbLeverage") as WinFormTextBox;
            Assert.NotNull(tbLeverage, string.Format("Ссылка на элемент {0} равна null.", tbLeverage));

            tbBalance = riskSetupControl.Items.FirstOrDefault(x => x.Id == "tbBalance") as WinFormTextBox;
            Assert.NotNull(tbBalance, string.Format("Ссылка на элемент {0} равна null.", tbBalance));

            tbOrderLeverage = riskSetupControl.Items.FirstOrDefault(x => x.Id == "tbOrderLeverage") as WinFormTextBox;
            Assert.NotNull(tbOrderLeverage, string.Format("Ссылка на элемент {0} равна null.", tbOrderLeverage));

            tbResultedVolume = riskSetupControl.Items.FirstOrDefault(x => x.Id == "tbResultedVolume") as WinFormTextBox;
            Assert.NotNull(tbResultedVolume, string.Format("Ссылка на элемент {0} равна null.", tbResultedVolume));

            tbResultRounded = riskSetupControl.Items.FirstOrDefault(x => x.Id == "tbResultRounded") as WinFormTextBox;
            Assert.NotNull(tbResultRounded, string.Format("Ссылка на элемент {0} равна null.", tbResultRounded));

            cbTicker = riskSetupControl.Items.First(x => x.Id == "cbTicker") as TestStack.White.UIItems.ListBoxItems.ComboBox;
            Assert.NotNull(tbResultRounded, string.Format("Ссылка на элемент {0} равна null.", cbTicker));
        }
    }
}
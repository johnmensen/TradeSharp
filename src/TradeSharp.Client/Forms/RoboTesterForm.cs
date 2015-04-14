using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using Entity;
using TradeSharp.Client.BL;
using TradeSharp.Client.Controls;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Robot.BacktestServerProxy;
using TradeSharp.Robot.Robot;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class RoboTesterForm : Form, IMdiNonChartWindow
    {
        private const int DefaultStartBalance = 1000000;

        private readonly RobotContextBacktest robotContext = 
            new RobotContextBacktest(MainForm.Instance.UpdateTickersCacheForRobots);        

        public delegate void RobotResultsBoundToChartsDel(
            Dictionary<BaseRobot, ChartWindowSettings> robotBindings,
            List<RobotLogEntry> robotLogEntries,
            List<MarketOrder> dealsOpened,
            List<MarketOrder> dealsClosed);

        private RobotResultsBoundToChartsDel onRobotResultsBoundToCharts;
        public event RobotResultsBoundToChartsDel OnRobotResultsBoundToCharts
        {
            add { onRobotResultsBoundToCharts += value; }
            remove { onRobotResultsBoundToCharts -= value; }
        }

        private readonly BackgroundWorker worker = new BackgroundWorker();
        private volatile bool stoppingTest;
        private delegate void UpdateProgressDel(double progress);
        private readonly string controlButtonNameStart;
        private readonly string controlButtonNameStop;

        public DateTime TimeStart
        {
            get { return dtDateFrom.Value; }
            set { dtDateFrom.Value = value; }
        }

        public static RoboTesterForm Instance { get; private set; }
        
        public RoboTesterForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
            controlButtonNameStart = Localizer.GetString("TitleLAUNCH");
            controlButtonNameStop = Localizer.GetString("TitleAbort");
            cbUseSelectedDate.Checked = UserSettings.Instance.UseTestPeriod;
            dtDateFrom.Value = UserSettings.Instance.TestPeriodFrom == DateTime.MinValue ? 
                DateTime.Now.Date.AddYears(-1) : UserSettings.Instance.TestPeriodFrom;
            dtDateTo.Value = UserSettings.Instance.TestPeriodTo == DateTime.MinValue ? DateTime.Now: UserSettings.Instance.TestPeriodTo;
            cbLogTrace.Checked = UserSettings.Instance.SaveLog;

            worker.WorkerSupportsCancellation = true;
            worker.DoWork += WorkerDoWork;
            worker.RunWorkerCompleted += WorkerRunWorkerCompleted;
            worker.WorkerReportsProgress = true;

            InitContextAccountSettings();
            Instance = this;

            // запомнить окошко
            MainForm.Instance.AddNonChartWindowSets(new NonChartWindowSettings
            {
                Window = WindowCode,
                WindowPos = Location,
                WindowSize = Size,
                WindowState = WindowState.ToString()
            });
        }

        #region IMdiNonChartWindow
        public NonChartWindowSettings.WindowCode WindowCode
        {
            get { return NonChartWindowSettings.WindowCode.RobotTest; }
        }
        
        public int WindowInnerTabPageIndex
        {
            get { return 0; }
            set { }
        }

        private Action<Form> formMoved;
        public event Action<Form> FormMoved
        {
            add { formMoved += value; }
            remove { formMoved -= value; }
        }

        /// <summary>
        /// перемещение формы завершено - показать варианты Drop-a
        /// </summary>
        private Action<Form> resizeEnded;
        public event Action<Form> ResizeEnded
        {
            add { resizeEnded += value; }
            remove { resizeEnded -= value; }
        }

        #endregion

        private void BtnStartClick(object sender, EventArgs e)
        {
            if (worker.IsBusy)
            {
                // тест в процессе, пользователь пытается остановить тест
                stoppingTest = true;
                return;
            }

            stoppingTest = false;
            progressBar.Value = 0;
            // сохраним настройки 
            UserSettings.Instance.UseTestPeriod = cbUseSelectedDate.Checked;
            UserSettings.Instance.TestPeriodFrom = dtDateFrom.Value;
            UserSettings.Instance.TestPeriodTo = dtDateTo.Value;
            UserSettings.Instance.SaveLog = cbLogTrace.Checked;
            UserSettings.Instance.SaveSettings();
            
            if (cbUseSelectedDate.Checked)
            {
                robotContext.TimeFrom = dtDateFrom.Value;
                robotContext.TimeTo = dtDateTo.Value;
            }
            else
            {
                robotContext.TimeFrom = new DateTime(1999, 1, 1, 1, 0, 0);
                robotContext.TimeTo = DateTime.Now;
                dtDateFrom.Value = robotContext.TimeFrom;
                dtDateTo.Value = robotContext.TimeTo;
            }

            robotContext.ClearAllTradeHistory();
            foreach (var robot in robotPortfolioControl.GetUsedRobots())
            {
                robot.Initialize(robotContext, CurrentProtectedContext.Instance);
                robotContext.SubscribeRobot(robot);
            }

            robotContext.LogRobots = cbLogTrace.Checked;
            robotContext.UpdateTickerCache = cbUpdateQuotes.Checked;
            robotContext.InitiateTest();

            // сменить заголовок кнопки
            btnStart.Text = controlButtonNameStop;

            worker.RunWorkerAsync();
        }

        private void WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (stoppingTest) break;
                try
                {
                    DateTime modelTime, firstDate;
                    if (robotContext.MakeStep(out modelTime, out firstDate)) break;
                    // прогресс
                    var passedHours = (modelTime - firstDate).TotalHours;
                    var totalHours = (robotContext.TimeTo - firstDate).TotalHours;
                    var progress = totalHours == 0
                                       ? 1.0
                                       : passedHours / totalHours;
                    UpdateProgressAsynch(progress);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    break;
                }                
            }
        }

        private void WorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            robotContext.FinalizeTest();
            foreach (var robot in robotPortfolioControl.GetUsedRobots())
            {
                robot.DeInitialize();
                robotContext.UnsubscribeRobot(robot);
            }

            MessageBox.Show(this,
                Localizer.GetString("MessageBacktestCompleted"), 
                Localizer.GetString("TitleInformation"), 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            // сменить заголовок кнопки
            btnStart.Text = controlButtonNameStart;
        }

        private void UpdateProgressAsynch(double progress)
        {
            Invoke(new UpdateProgressDel(UpdateProgressSynch), progress);
        }

        private void UpdateProgressSynch(double progress)
        {
            var newValue = (int)(progressBar.Maximum * progress);
            if (newValue > progressBar.Maximum)
                newValue = progressBar.Maximum;
            progressBar.Value = newValue;
        }

        private void BtnAccountSettingsClick(object sender, EventArgs e)
        {
            var dlg = new PropertiesDlg(new Account(robotContext.AccountInfo), Localizer.GetString("TitleAccountSettings"));
            if (dlg.ShowDialog() == DialogResult.OK)
                robotContext.AccountInfo = (Account) dlg.objectsList[0];
        }

        private void BtnResultClick(object sender, EventArgs e)
        {
            var statForm = new StatInfoForm(robotContext);
            statForm.Show(this);
        }

        private void InitContextAccountSettings()
        {
            if (robotContext.AccountInfo != null) return;
            Account account;
            if (AccountStatus.Instance.AccountData != null)
            {
                account = new Account(AccountStatus.Instance.AccountData)
                              {
                                  Balance = DefaultStartBalance,
                                  Equity = DefaultStartBalance,
                                  UsedMargin = 0
                              };
            }
            else
                account = new Account
                    {
                        Group = DalAccountGroup.Instance.Groups.Length == 0
                                    ? "Demo" : DalAccountGroup.Instance.Groups[0].Code,
                        Balance = 100000,
                        Equity = 100000,
                        Currency = "USD",
                        ID = 0,
                        MaxLeverage = 50,
                        UsedMargin = 0
                    };
            
            robotContext.AccountInfo = new Account(account);            
        }

        private void BtnSetupTestGroupClick(object sender, EventArgs e)
        {
            var dlg = new PropertiesDlg(new AccountGroup
                                            {
                                                BrokerLeverage = robotContext.groupDefault.BrokerLeverage,
                                                Code = robotContext.groupDefault.Code,
                                                DefaultVirtualDepo = robotContext.groupDefault.DefaultVirtualDepo,
                                                IsReal = robotContext.groupDefault.IsReal,
                                                MarginCallPercentLevel = robotContext.groupDefault.MarginCallPercentLevel,
                                                Name = robotContext.groupDefault.Name,
                                                StopoutPercentLevel = robotContext.groupDefault.StopoutPercentLevel

                                            }, Localizer.GetString("TitleGroupSettings"));
            if (dlg.ShowDialog() == DialogResult.OK)
                robotContext.groupDefault = (AccountGroup) dlg.objectsList[0];
        }

        private void BtnShowRobotMarkersClick(object sender, EventArgs e)
        {
            if (robotContext.robotLogEntries.Count == 0 &&
                robotContext.Positions.Count == 0 && robotContext.PosHistory.Count == 0) return;
            var charts = MainForm.Instance.GetChartSymbolTimeframeList();
            if (charts.Count == 0)
            {
                MessageBox.Show(
                    Localizer.GetString("MessageNoChartToShowBacktestResults"), 
                    Localizer.GetString("TitleError"));
                return;
            }
            
            // открыть диалог - робот - графики            
            var robots = robotPortfolioControl.GetUsedRobots();
            var dlg = new RobotBindChartForm(robots, charts);
            if (dlg.ShowDialog() == DialogResult.Cancel) return;
            var robotBindings = dlg.GetRobotChartBindings();

            // отобразить результаты на графиках вызвавшего окна
            if (robotBindings.Count == 0) return;
            if (onRobotResultsBoundToCharts != null)
                onRobotResultsBoundToCharts(robotBindings, robotContext.robotLogEntries,
                    robotContext.Positions, robotContext.PosHistory);
        }

        // ReSharper disable MemberCanBeMadeStatic.Local
        private void RoboTesterFormFormClosing(object sender, FormClosingEventArgs e)
        // ReSharper restore MemberCanBeMadeStatic.Local
        {
            SaveTempPortfolio();
            Instance = null;
            // убрать окошко из конфигурации
            if (e.CloseReason == CloseReason.UserClosing ||
                e.CloseReason == CloseReason.None)
                MainForm.Instance.RemoveNonChartWindowSets(WindowCode);
        }

        public void ReadLastRobotSettings()
        {
            if (string.IsNullOrEmpty(RobotPortfolioSetupControl.lastSavedPath)) return;
            if (!File.Exists(RobotPortfolioSetupControl.lastSavedPath)) return;
            robotPortfolioControl.ReadRobotsSettings(RobotPortfolioSetupControl.lastSavedPath);
        }

        public List<BaseRobot> GetUsedRobots()
        {
            return robotPortfolioControl.GetUsedRobots();
        }
    
        public void SaveRobots(List<BaseRobot> robots)
        {
            robotPortfolioControl.SaveRobots(robots);
        }

        public void ShowRobotSettings(string robotUniqueName)
        {
            robotPortfolioControl.ShowDialogforRobotGivenByItsUniqueName(robotUniqueName);
        }

        private void BtnShowDealPointsFormClick(object sender, EventArgs e)
        {
            if (robotContext.Positions.Count == 0 && robotContext.PosHistory.Count == 0) return;

            var dealsTotal = robotContext.Positions.ToList();
            dealsTotal.AddRange(robotContext.PosHistory);

            new DealCollectionPointsForm(dealsTotal).ShowDialog();
        }
    
        private void SaveTempPortfolio()
        {
            try
            {
                SaveRobots(robotPortfolioControl.GetUsedRobots());
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в SaveTempPortfolio()", ex);
            }
        }

        private void RoboTesterFormLoad(object sender, EventArgs e)
        {
            // попытаться загрузить последний сохраненный портфель
            try
            {
                if (File.Exists(RobotPortfolioSetupControl.lastSavedPath))
                    robotPortfolioControl.ReadRobotsSettings(RobotPortfolioSetupControl.lastSavedPath);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в RoboTesterFormLoad()", ex);
            }
        }

        private void RoboTesterFormResizeEnd(object sender, EventArgs e)
        {
            if (resizeEnded != null)
                resizeEnded(this);
        }

        private void RoboTesterFormMove(object sender, EventArgs e)
        {
            if (formMoved != null)
                formMoved(this);
        }
    }
}

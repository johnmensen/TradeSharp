using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Entity;
using FastGrid;
using FastMultiChart;
using TradeSharp.Contract.Entity;
using TradeSharp.Robot.BacktestServerProxy;
using TradeSharp.Robot.Robot;
using TradeSharp.UI.Util.Forms;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    /// <summary>
    /// статистика сугубо по роботу
    /// </summary>
    public partial class StatInfoForm : Form
    {
        struct ChartPoint
        {
            public DateTime Date { get; set; }

            public float Equity { get; set; }
        }

        class StatisticsParam
        {
            public string Title { get; set; }

            public object Value { get; set; }

            public Func<object, string> formatter;

            public string Description { get; set; }

            public StatisticsParam()
            {
            }

            public StatisticsParam(string title, object value, string description,
                Func<object, string> formatter)
            {
                Title = title;
                Value = value;
                this.formatter = formatter;
                Description = description;
            }
        }

        private readonly RobotContextBacktest robotContext;

        public StatInfoForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
        }

        public StatInfoForm(RobotContextBacktest robotContext) : this()
        {
            this.robotContext = robotContext;
            var orderTags = robotContext.PosHistory.ToList();
            orderTags.AddRange(robotContext.Positions.ToList());
            AccountHistoryCtrl.historyGrid.DataBind(orderTags);

            SetupGrids();
            SetupCharts();
            BuildEquityCurve();
            CalculateStatistics();
            ShowLog();
        }

        private void SetupGrids()
        {
            var blankEvent = new GridLogEvent();
            // таблица - лог роботов
            gridLog.Columns.Add(new FastColumn(blankEvent.Property(p => p.Time), Localizer.GetString("TitleTime"))
                {
                    ColumnWidth = 90,
                    SortOrder = FastColumnSort.Ascending
                });
            gridLog.Columns.Add(new FastColumn(blankEvent.Property(p => p.RobotTitle), Localizer.GetString("TitleRobot"))
                {
                    ColumnMinWidth = 50
                });
            gridLog.Columns.Add(new FastColumn(blankEvent.Property(p => p.Message), Localizer.GetString("TitleMessage"))
                {
                    ColumnMinWidth = 40
                });
            gridLog.CheckSize();
            gridLog.CalcSetTableMinWidth();

            // статистика
            var blankStat = new StatisticsParam();
            gridStat.Columns.Add(new FastColumn(blankStat.Property(p => p.Title), Localizer.GetString("TitleParameter"))
                {
                    //SortOrder = FastColumnSort.Ascending,
                    ColumnMinWidth = 55
                });
            gridStat.Columns.Add(new FastColumn(blankStat.Property(p => p.Value), Localizer.GetString("TitleValue"))
                {
                    ColumnMinWidth = 45,
                    rowFormatter = valueObject => ((StatisticsParam) valueObject).formatter == null
                                                      ? ((StatisticsParam) valueObject).Value.ToString()
                                                      : ((StatisticsParam) valueObject).formatter(
                                                          ((StatisticsParam) valueObject).Value)
                });
            gridStat.Columns.Add(new FastColumn(blankStat.Property(p => p.Description),
                                                Localizer.GetString("TitleDescription"))
                {
                    ColumnMinWidth = 55
                });
            gridStat.CheckSize();
            gridStat.CalcSetTableMinWidth();
        }

        private void BtnCloseClick(object sender, EventArgs e)
        {
            Close();
        }

        private void BuildEquityCurve()
        {
            chart.Graphs[0].Series[0].Clear();
            chart.Graphs[0].Series[0].Clear();
            if (robotContext.dailyEquityExposure.Count == 0) return;

            foreach (var pt in robotContext.dailyEquityExposure)
            {
                chart.Graphs[0].Series[0].Add(new ChartPoint { Date = pt.a, Equity = pt.b });
            }

            chart.Initialize();
        }

        private void SetupCharts()
        {
            chart.GetXScaleValue = FastMultiChartUtils.GetDateTimeScaleValue;
            chart.GetXValue = FastMultiChartUtils.GetDateTimeValue;
            chart.GetXDivisionValue = FastMultiChartUtils.GetDateTimeDivisionValue;
            chart.GetMinXScaleDivision = FastMultiChartUtils.GetDateTimeMinScaleDivision;
            chart.GetMinYScaleDivision = FastMultiChartUtils.GetDoubleMinScaleDivision;
            chart.GetXStringValue = FastMultiChartUtils.GetDateTimeStringValue;
            chart.GetXStringScaleValue = FastMultiChartUtils.GetDateTimeStringScaleValue;
            var blank = new ChartPoint();
            chart.Graphs[0].Series.Add(new Series(blank.Property(p => p.Date), blank.Property(p => p.Equity),
                                                  new Pen(Color.FromArgb(80, 5, 5), 2f))
                {
                    XMemberTitle = Localizer.GetString("TitleDate"),
                    YMemberTitle = Localizer.GetString("TitleBalance")
                });
        }

        private void CalculateStatistics()
        {
            if (!robotContext.lastTestStart.HasValue) return;

            var countDeals = robotContext.PosHistory.Count;
            var countOpen = robotContext.PosHistory.Count(p => p.IsClosed == false);
            var initialBalance = robotContext.AccountInfo.Balance;
            var finalBalance = (decimal)(robotContext.dailyEquityExposure.Count == 0 ? 0 :
                robotContext.dailyEquityExposure[robotContext.dailyEquityExposure.Count - 1].b);
            var twr = initialBalance == 0 ? 0 : 100 * finalBalance / initialBalance;
            
            // подсчет макс. и среднего плеча
            var lstLeverage = (from t in robotContext.dailyEquityExposure where t.c != 0 select t.c/t.b).ToList();
            var maxLeverage = lstLeverage.Count == 0 ? 0 : lstLeverage.Max();
            var avgLeverage = lstLeverage.Count == 0 ? 0 : lstLeverage.Average();
            // стопауты
            var sbStopouts = new StringBuilder();
            foreach (var so in robotContext.stopoutEventTimes)            
                sbStopouts.AppendFormat("{0:dd.MM.yyy HH:mm}  ", so);            

            // подсчет макс. относительного проседания
            var drawDown = 0f;
            var absDrawDown = 0f;
            if (robotContext.dailyEquityExposure.Count > 0)
            {
                for (var i = 0; i < robotContext.dailyEquityExposure.Count - 1; i++)
                {
                    var tempDrawDown = 0f;
                    var startBalance = robotContext.dailyEquityExposure[i].b;
                    var j = i + 1;
                    for (; j < robotContext.dailyEquityExposure.Count; j++)
                    {
                        var curBal = robotContext.dailyEquityExposure[j].b;
                        if (curBal >= startBalance) break;
                        var curDd = startBalance - curBal;
                        if (curDd > tempDrawDown) tempDrawDown = curDd;
                    }
                    if (tempDrawDown > absDrawDown) absDrawDown = tempDrawDown;
                    if (startBalance > 0) tempDrawDown /= startBalance;
                    if (tempDrawDown > drawDown) drawDown = tempDrawDown;
                }
            }
            drawDown *= 100;

            // ReSharper disable UseObjectOrCollectionInitializer
            var statParams = new List<StatisticsParam>();
            // ReSharper restore UseObjectOrCollectionInitializer
            statParams.Add(new StatisticsParam(Localizer.GetString("TitleDuration"),
                                               (int)
                                               (robotContext.lastTestEnd.Value - robotContext.lastTestStart.Value)
                                                   .TotalSeconds, Localizer.GetString("TitleTestDurationInSeconds"),
                                               o => ((int) o).ToString()));
            if (robotContext.dailyEquityExposure.Count > 0)
                statParams.Add(new StatisticsParam(Localizer.GetString("TitleInterval"),
                                                   string.Format("{0:dd.MM.yyyy} - {1:dd.MM.yyyy}",
                                                                 robotContext.dailyEquityExposure[0].a,
                                                                 robotContext.dailyEquityExposure[
                                                                     robotContext.dailyEquityExposure.Count - 1].a),
                                                   Localizer.GetString("TitleTestInterval"),
                                                   null));
            statParams.Add(new StatisticsParam(Localizer.GetString("TitleInitialDeposit"), initialBalance,
                                               Localizer.GetString("TitleAccountBalanceOnTestBeginning"),
                                               o =>
                                               ((decimal) o).ToStringUniformMoneyFormat() + " " +
                                               robotContext.AccountInfo.Currency));
            statParams.Add(new StatisticsParam(Localizer.GetString("TitleFinalDeposit"), finalBalance,
                                               Localizer.GetString("TitleAccountBalanceOnTestEnd"),
                                               o =>
                                               ((decimal) o).ToStringUniformMoneyFormat() + " " +
                                               robotContext.AccountInfo.Currency));
            statParams.Add(new StatisticsParam(Localizer.GetString("TitleDealsTotal"), countDeals,
                                               Localizer.GetString("TitleDealsTotalOpenedAndClosed"), null));
            statParams.Add(new StatisticsParam(Localizer.GetString("TitleDealsOpened"), countOpen,
                                               Localizer.GetString("TitleDealsOpened"), null));
            statParams.Add(new StatisticsParam(Localizer.GetString("TitleProfit"), twr - 100,
                                               Localizer.GetString("TitleTerminalWealthRatioInPercents"),
                                               o => ((decimal) o).ToStringUniformMoneyFormat()));
            statParams.Add(new StatisticsParam(Localizer.GetString("TitleMaximumLeverage"), maxLeverage,
                                               Localizer.GetString("TitleMaximumLeverageInTestInUnits"),
                                               o => ((float) o).ToStringUniformMoneyFormat()));
            statParams.Add(new StatisticsParam(Localizer.GetString("TitleAverageLeverage"), avgLeverage,
                                               Localizer.GetString("TitleAverageLeverageInTestInUnits"),
                                               o => ((float) o).ToStringUniformMoneyFormat()));
            statParams.Add(new StatisticsParam(Localizer.GetString("TitleMaximumDrawdownShort"), absDrawDown,
                                               Localizer.GetString("TitleMaximumRelativeDrawdownInPercents"),
                                               o => ((float) o).ToStringUniformMoneyFormat()));
            if (finalBalance > initialBalance)
            {
                var fv = absDrawDown == 0 ? 0 : ((float) (finalBalance - initialBalance) / absDrawDown);
                statParams.Add(new StatisticsParam(Localizer.GetString("TitleRestoreFactor"), fv,
                                                   Localizer.GetString("TitleRestoreFactorInUnits"),
                                                   o => ((float) o).ToStringUniformMoneyFormat()));
            }
            if (sbStopouts.Length > 0)
            {
                statParams.Add(new StatisticsParam(Localizer.GetString("TitleStopOut"), sbStopouts.Length,
                                                   Localizer.GetString("TitleStopOutExecutedInTimes"), null));
            }
            
            gridStat.DataBind(statParams);
        }

        private void BtnExportStatClick(object sender, EventArgs e)
        {
            if (robotContext.dailyEquityExposure.Count == 0) return;
            var setupForm = new ExportSetupForm();
            if (setupForm.ShowDialog() != DialogResult.OK) return;
            // выбрать файл
            if (saveFileDialog.ShowDialog() != DialogResult.OK) return;

            // сохранить кривую доходности и плечо
            using (var sw = new StreamWriterLog(saveFileDialog.FileName, false, setupForm.ExportEncoding))
            {
                sw.WriteLine(string.Join(new string(setupForm.ColumnSeparator, 1), "Time", "Equity", "Leverage"));
                foreach (var rec in robotContext.dailyEquityExposure)
                {
                    var timeStr = rec.a.ToString(setupForm.DateTimeFormat);
                    var strEq = rec.b.ToStringUniform();
                    var strLev = rec.b.ToStringUniform();
                    if (setupForm.FloatSeparator == ',')
                    {
                        strEq = strEq.Replace('.', ',');
                        strLev = strLev.Replace('.', ',');
                    }
                    var line = string.Join(new string(setupForm.ColumnSeparator, 1),
                                           timeStr, strEq, strLev);
                    sw.WriteLine(line);
                }
            }
        }

        private void MenuitemExportClick(object sender, EventArgs e)
        {
            var orders = AccountHistoryCtrl.historyGrid.rows.Select(r => (MarketOrder) r.ValueObject).ToList();
            new ExportPositionsForm(orders).ShowDialog();
        }
    
        private void ShowLog()
        {
            var eventList = new List<GridLogEvent>();
            foreach (var logEntry in robotContext.robotLogEntries)
            {
                GridLogEvent.MakeEvent(logEntry, false, eventList);
            }
            gridLog.DataBind(eventList);
        }        
    }

    class GridLogEvent
    {
        public DateTime Time { get; set; }

        public string RobotTitle { get; set; }

        public string Message { get; set; }

        public static void MakeEvent(RobotLogEntry entry, bool showHints, List<GridLogEvent> eventList)
        {
            foreach (var msg in entry.Messages)
            {
                var hint = RobotHint.ParseString(msg);
                if (hint != null && !showHints) continue;

                var evt = new GridLogEvent
                              {
                                  Message = msg,
                                  Time = entry.Time,
                                  RobotTitle = entry.Robot.TypeName + ":" +
                                               entry.Robot.Magic
                              };
                eventList.Add(evt);
            }
        }
    }
}

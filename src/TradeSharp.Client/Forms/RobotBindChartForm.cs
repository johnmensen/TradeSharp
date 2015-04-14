using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TradeSharp.Client.BL;
using TradeSharp.Client.Controls.Bookmark;
using TradeSharp.Robot.Robot;

namespace TradeSharp.Client.Forms
{
    public partial class RobotBindChartForm : Form
    {
        private readonly List<BaseRobot> robots;
        /// <summary>
        /// tab - symbol - timeframe
        /// </summary>
        private readonly List<ChartWindowSettings> charts;

        public RobotBindChartForm()
        {
            InitializeComponent();
        }

        public RobotBindChartForm(List<BaseRobot> robots, List<ChartWindowSettings> charts) : this()
        {
            this.robots = robots;
            this.charts = charts;
        }

        public Dictionary<BaseRobot, ChartWindowSettings> GetRobotChartBindings()
        {
            var robBindings = new Dictionary<BaseRobot, ChartWindowSettings>();
            var strBindings = (List<RobotChartBinding>)grid.DataSource;
            for (var i = 0; i < robots.Count; i++)
            {
                var chartsStr = strBindings[i].SelectedCharts;
                if (chartsStr == RobotChartBinding.EmptyChartsPlaceholder) continue;

                var parts = chartsStr.Split(new[] {':'}, StringSplitOptions.None);
                var bookmark = BookmarkStorage.Instance.FindBookmark(parts[0]);
                var ticker = parts[1];
                var timeFrame = parts[2];
                var chart = charts.First(c => c.TabPageId == bookmark.Id && 
                    c.Symbol == ticker && c.Timeframe == timeFrame);

                robBindings.Add(robots[i], chart);
            }
            return robBindings;
        }

        private void InitGrid()
        {
            // забить список подстановки            
            var colChartOptions = new List<string> { RobotChartBinding.EmptyChartsPlaceholder };
            colChartOptions.AddRange(charts.Select(c => string.Format("{0}:{1}:{2}",
                BookmarkStorage.Instance[c.TabPageId].Title, c.Symbol, c.Timeframe)));
            colChart.DataSource = colChartOptions;
            // забить колонки
            var gridData = robots.Select(robot => new RobotChartBinding(robot)).ToList();
            grid.DataSource = gridData;
        }

        private void RobotBindChartFormLoad(object sender, EventArgs e)
        {
            InitGrid();
        }
    }

    class RobotChartBinding
    {
        public const string EmptyChartsPlaceholder = "-";

        public string Robot { get; set; }
        public string RobotCharts { get; set; }
        public string SelectedCharts { get; set; }

        public RobotChartBinding() {}
        public RobotChartBinding(BaseRobot robot)
        {
            Robot = robot.TypeName;
            RobotCharts = robot.HumanRTickers;
            SelectedCharts = EmptyChartsPlaceholder;
        }
    }
}

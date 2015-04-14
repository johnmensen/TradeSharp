using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using FastChart.Chart;
using FastGrid;
using NewsRobot;
using TradeSharp.Client;
using TradeSharp.Util;

namespace NewsAnalysisScript
{
    public partial class ResultsForm : Form
    {
        private List<RobotNews> robotNews;
        private List<RobotNewsStat> robotNewsStats;

        public ResultsForm()
        {
            InitializeComponent();
            newsFastGrid.Columns.Add(new FastColumn("MatchCriteria", "Критерий")
            {
                ColumnWidth = 55,
                ImageList = imageListGrid,
                IsHyperlinkStyleColumn = true,
                HyperlinkActiveCursor = Cursors.Hand
            });
            newsFastGrid.Columns.Add(new FastColumn("CountryCode", "Страна"));
            newsFastGrid.Columns.Add(new FastColumn("Title", "Наименование"));
            newsFastGrid.Columns.Add(new FastColumn("FollowCount", "Следование"));
            newsFastGrid.Columns.Add(new FastColumn("Count", "Наблюдений"));
            newsFastGrid.Columns.Add(new FastColumn("Average", "Среднее")
                                         /*{
                                             FormatString = "f7",
                                             colorColumnFormatter = delegate(object c, out Color? bc, out Color? fc)
                                                                        {
                                                                            fc = ((double) c >= 0)
                                                                                     ? Color.DarkBlue
                                                                                     : Color.DarkRed;
                                                                            bc = null;
                                                                        }
                                         }*/);
            indexFastChart.Axes.Add(new FastAxis(FastAxisDirection.X, false)
                                        /*{
                                            ColorMainGrid = Color.DarkGray,
                                            LineColor = Color.Black,
                                            MinPixelsPerPoint = 55,
                                            DrawMainGrid = true,
                                            DrawSubGrid = true,
                                            ColorSubGrid = Color.Silver
                                        }*/);
            indexFastChart.Axes.Add(new FastAxis(FastAxisDirection.Y, false)
                                        /*{
                                            AlwaysShowNil = true,
                                            ColorMainGrid = Color.DarkGray,
                                            LineColor = Color.Black,
                                            MinPixelsPerPoint = 25,
                                            DrawMainGrid = true,
                                            AutoScale100 = false
                                        }*/);
            var series = new FastSeries("", FastSeriesType.Линия, indexFastChart.Axes[0], indexFastChart.Axes[1],
                                        false);
            indexFastChart.series.Add(series);
        }

        public void SetNews(List<RobotNews> news)
        {
            robotNews = news;
        }

        public void SetStats(List<RobotNewsStat> stats)
        {
            robotNewsStats = stats;
            newsFastGrid.DataBind(stats);
        }

        private void NewsFastGridUserHitCell(object sender, MouseEventArgs mouseEventArgs, int rowIndex, FastColumn col)
        {
            if (rowIndex < 0 || rowIndex >= newsFastGrid.rows.Count)
                return;
            if (col.PropertyName == "MatchCriteria")
            {
                DialogResult rst;
                var critStr = Dialogs.ShowInputDialog("Критерий сортировки",
                                        "критерий", true, "Наблюдений: 10, порог: 20 - 80 (%)",
                                        out rst);
                if (rst != DialogResult.OK) return;
                var numbers = critStr.ToIntArrayUniform();
                if (numbers.Length != 3) return;

                foreach (var item in robotNewsStats)
                {
                    var percent = item.FollowCountNumber;
                    item.MatchCriteria = item.DeltaIndexes.Count >= numbers[0]
                                         && (percent <= numbers[1] || percent >= numbers[2]);
                }

                newsFastGrid.DataBind(robotNewsStats);
                newsFastGrid.Invalidate();

                return;
            }

            var stat = (RobotNewsStat)newsFastGrid.rows[rowIndex].ValueObject;
            var series = indexFastChart.series[0];
            //series.Name = stat.Title;
            series.points.Clear();
            for (var i = 0; i < stat.DeltaIndexes.Count; i++ )
                series.points.Add(new FastSeriesPoint(i, Math.Abs(stat.DeltaIndexes[i])));
            indexFastChart.Invalidate();
        }

        private void ToXmlButtonClick(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Файл в формате XML (*.xml)|*.xml";
            if (saveFileDialog.ShowDialog(MainForm.Instance) == DialogResult.Cancel)
                return;
            var doc = new XmlDocument();
            var rootElement = doc.CreateElement("settings");
            foreach (var stat in robotNewsStats)
            {
                if (stat.Count == 0)
                    continue;
                if (!stat.MatchCriteria)
                    continue;

                var newsElement = doc.CreateElement("News");

                var element = doc.CreateElement("CountryCode");
                var attribute = doc.CreateAttribute("value");
                attribute.Value = stat.CountryCode;
                element.Attributes.Append(attribute);
                newsElement.AppendChild(element);

                element = doc.CreateElement("NewsTitle");
                attribute = doc.CreateAttribute("value");
                attribute.Value = stat.Title;
                element.Attributes.Append(attribute);
                newsElement.AppendChild(element);

                element = doc.CreateElement("Weight");
                attribute = doc.CreateAttribute("value");
                attribute.Value = stat.FollowCountNumber > 50 ? "1" : "-1";
                element.Attributes.Append(attribute);
                newsElement.AppendChild(element);

                element = doc.CreateElement("FollowCount");
                attribute = doc.CreateAttribute("value");
                attribute.Value = stat.FollowCount;
                element.Attributes.Append(attribute);
                newsElement.AppendChild(element);

                rootElement.AppendChild(newsElement);
            }
            doc.AppendChild(rootElement);
            doc.Save(saveFileDialog.FileName);
        }

        private void SaveNewsButtonClick(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Текстовый файл (*.txt)|*.txt";
            if (saveFileDialog.ShowDialog(MainForm.Instance) == DialogResult.Cancel)
                return;
            RobotNews.SaveToFile(saveFileDialog.FileName, robotNews);
        }
    }
}

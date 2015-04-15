using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Entity;
using FastChart.Chart;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Reports.Lib.Contract;
using TradeSharp.SiteBridge.Lib.Finance;
using TradeSharp.Util;
using System.Linq;

namespace TradeSharp.Reports.Lib.Report
{
    public class ReportInvestorMonthly : BaseReport
    {
        private const string TemplateName = "report_monthly.html";
        private const string OutputFileName = "report_monthly.pdf";

        private const string NameOfTimestamp = "timestamp";
        private const string NameOfChartProfit1000 = "imgChartProfit1000";
        private const string NameOfChartProfitHistogram = "imgChartProfitHistogram";
        private const string NameOfTableMonthly = "tabMonthProfit";

        private const string StyleMonthlyTableEvenRow = "monthlyTableEvenRow";
        private const string RowStatHistoryData = "rowHistoricalData";
        private const string RowRisk = "rowRisk";
        private const string RowRetByYear = "rowRetByYear";
        private const string RowLatestReturns = "rowLatestReturns";
        private const string RowCorrelation = "rowCorrelation";

        private const string HtmlLineSeparator = "<br/>";
        private const string HtmlReplacePreffix = "#";

        /// <summary>
        /// VAMI states for Value Added Monthly Index
        /// </summary>
        private const int VamiStart = 1000;

        private List<EquityOnTime> profitDic;
        private readonly IndexGrabberProxy indexGrabberProxy;
        private readonly string benchmarkA, benchmarkB;
        private List<EquityOnTime> profitBenchmarkA, profitBenchmarkB;
        private List<EquityOnTime> vamiBenchmarkA, vamiBenchmarkB;

        public ReportInvestorMonthly(string benchmarkA, string benchmarkB)
        {
            indexGrabberProxy = new IndexGrabberProxy(TerminalBindings.BindingIndexGrabber);
            this.benchmarkA = benchmarkA;
            this.benchmarkB = benchmarkB;
        }

        public static string GetResultFileName(string destFolder)
        {
            return string.Format("{0}\\{1}", destFolder, OutputFileName);
        }

        public override string MakePdf(int accountId,
            string templateFolder, string destFolder, string tempFolder)
        {
            var templatePath = string.Format("{0}\\{1}", templateFolder, TemplateName);
            if (!File.Exists(templatePath))
                throw new Exception(string.Format("Невозможно прочитать шаблон \"{0}\"", templatePath));
            XmlDocument doc;
            XmlElement tagBody;
            try
            {
                doc = new XmlDocument();
                doc.Load(templatePath);
                tagBody = (XmlElement)doc.DocumentElement.SelectSingleNode("body");
                if (tagBody == null) throw new Exception(string.Format("Документ \"{0}\": body is null", templatePath));
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("ReportInvestorMonthly: ошибка чтения документа \"{0}\": {1}", 
                    templatePath, ex);
                return string.Empty;
            }

            // очистить временный каталог
            ClearTempFolder(tempFolder);
            
            // получить доходность и статистику по сделкам
            profitDic = TradeSharpAccountStatistics.Instance.proxy.GetAccountProfit1000(accountId);
            if (profitDic.Count == 0)
            {
                Logger.ErrorFormat("ReportInvestorMonthly: для счета {0} нет данных о балансе", 
                    accountId);
                return string.Empty;
            }
            // доходность бенчмарков
            var indexData = indexGrabberProxy.GetIndexData();
            if (indexData.Count == 0)
            {
                Logger.Error("Нет данных по бенчмаркам");
                return string.Empty;
            }
            if (!indexData.ContainsKey(benchmarkA))
            {
                Logger.ErrorFormat("Нет данных по бенчмарку \"{0}\"", benchmarkA);
                return string.Empty;
            }
            if (!indexData.ContainsKey(benchmarkB))
            {
                Logger.ErrorFormat("Нет данных по бенчмарку \"{0}\"", benchmarkB);
                return string.Empty;
            }
            
            profitBenchmarkA = indexData[benchmarkA].Select(t => new EquityOnTime(t.b, t.a)).ToList();
            profitBenchmarkB = indexData[benchmarkB].Select(t => new EquityOnTime(t.b, t.a)).ToList();
            // по бенчмаркам получить доходность на $1000 вирт. баксов
            CalculateBenchmarksVAMI();

            // посчитать статистику и заполнить шаблон            
            if (!ActualizePattern(tagBody, tempFolder, indexData)) return string.Empty;
            PostProcessDocument(tagBody);
            
            // время отчета
            MakeTimeSpan(tagBody);

            var resultedTemplate = string.Format("{0}\\{1}", tempFolder, TemplateName.Replace(".html", "_tmp.html"));
            try
            {
                var docEncod = doc.FirstChild.NodeType == XmlNodeType.XmlDeclaration
                                   ? Encoding.GetEncoding(((XmlDeclaration)doc.FirstChild).Encoding)
                                   : Encoding.UTF8;
                using (var sw = new StreamWriter(resultedTemplate, false, docEncod))
                    doc.Save(sw);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка сохранения актуализированного документа ({0}): {1}", resultedTemplate, ex);
                return string.Empty;
            }
            
            // создать pdf
            var destPath = GetResultFileName(destFolder);
            try
            {
                PdfFromXHtml.Instance.MakePdf(resultedTemplate, destPath);
            }
            catch (Exception ex)
            {
                Logger.Error("ReportInvestorMonthly: ошибка рендеринга pdf", ex);
                return string.Empty;
            }
            return destPath;
        }

        private void ClearTempFolder(string tempFolder)
        {
            if (!Directory.Exists(tempFolder)) return;
            int numTries = 0, numFaults = 0;
            foreach (var fileName in Directory.GetFiles(tempFolder, "*.*"))
            {
                numTries++;
                try
                {
                    File.Delete(fileName);
                }
                catch
                {
                    numFaults++;
                }
            }
            if (numFaults > 0) Logger.InfoFormat("Очистка временного каталога \"{0}\": {1} из {2} файлов не удалены",
                tempFolder, numFaults, numTries);
        }

        private void MakeTimeSpan(XmlElement tagBody)
        {
            var nodeTimestamp = FindElement(tagBody, "td", NameOfTimestamp);
            if (nodeTimestamp == null) return;
            nodeTimestamp.InnerText = string.Format(CultureInfo.GetCultureInfo("Ru-ru"),
                                                    "{0:dd MMM yyy}", DateTime.Now);
        }

        /// <summary>
        /// заменить названия бенчмарков и пр. шаблоны
        /// </summary>        
        private void PostProcessDocument(XmlElement tagStart)
        {
            var dicReplace = new Dictionary<string, string>
                                 {{"#Benchmark1#", benchmarkA}, {"#Benchmark2#", benchmarkB}};
            foreach (var node in tagStart.ChildNodes)
            {
                if (node is XmlElement == false) continue;
                var elt = (XmlElement) node;
                if (elt.InnerText.StartsWith(HtmlReplacePreffix))
                {
                    foreach (var pair in dicReplace)
                        if (elt.InnerText == pair.Key)
                        {
                            elt.InnerText = pair.Value;
                            break;
                        }
                }
                foreach (var child in elt.ChildNodes) 
                    if (child is XmlElement) PostProcessDocument((XmlElement)child);
            }
        }

        private bool ActualizePattern(XmlElement tagBody, string tempFolder,
            Dictionary<string, List<Util.Cortege2<DateTime, float>>> indexData)
        {
            var nodeImgChartProfit1000 = FindElement(tagBody, "img", NameOfChartProfit1000);
            if (nodeImgChartProfit1000 == null) return false;
            var chartSize = new Size(350, 200);
            if (nodeImgChartProfit1000.Attributes["width"] != null &&
                nodeImgChartProfit1000.Attributes["height"] != null)
            {
                chartSize = new Size(nodeImgChartProfit1000.Attributes["width"].Value.ToInt(),
                    nodeImgChartProfit1000.Attributes["height"].Value.ToInt());
            }
            try
            {
                var chartFilePath = MakeProfitChart(tempFolder, chartSize);
                nodeImgChartProfit1000.Attributes["src"].Value = chartFilePath;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка отрисовки графика доходности", ex);
                return false;
            }

            var nodeImgChartProfitHistogram = FindElement(tagBody, "img", NameOfChartProfitHistogram);
            if (nodeImgChartProfitHistogram == null) return false;
            if (nodeImgChartProfitHistogram.Attributes["width"] != null &&
                nodeImgChartProfitHistogram.Attributes["height"] != null)
            {
                chartSize = new Size(nodeImgChartProfitHistogram.Attributes["width"].Value.ToInt(),
                    nodeImgChartProfitHistogram.Attributes["height"].Value.ToInt());
            }
            try
            {
                var chartFilePath = MakeProfitHistogram(profitDic, tempFolder, chartSize);
                nodeImgChartProfitHistogram.Attributes["src"].Value = chartFilePath;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка отрисовки гистограммы доходности", ex);
                return false;
            }
            var nodeMonthlyProfit = FindElement(tagBody, "table", NameOfTableMonthly);
            if (nodeMonthlyProfit == null) return false;

            var listProfit = profitDic;
            // собрать доходность по месяцам
            var prevDepo = listProfit[0].equity;
            var prevDate = listProfit[0].time;
            var profitMonthly = new List<float>();
            foreach (var pt in listProfit)
            {
                if (pt.time.Month == prevDate.Month) continue;
                var profit = (pt.equity - prevDepo) / prevDepo;
                prevDepo = pt.equity;
                prevDate = pt.time;
                profitMonthly.Add(profit);
            }

            FillMonthlyProfitTable(listProfit, nodeMonthlyProfit, tagBody);
            CalculateStatistics(tagBody, listProfit, profitMonthly);
            CalculateCorrelation(tagBody, profitMonthly);
            FillLatestReturnsTable(tagBody, profitMonthly);

            return true;
        }

        private void CalculateStatistics(XmlElement tagBody,
            List<EquityOnTime> listProfit,
            List<float> profitMonthly)
        {
            if (profitMonthly.Count < 2) return;
            var benchmarkDataVAMI = new[] { vamiBenchmarkA, vamiBenchmarkB };
            var benchmarkDataROR = new[] { profitBenchmarkA, profitBenchmarkB };

            // табличка Historical Data
            var stat = new MonthlyReportStatistics(listProfit, profitMonthly);
            var row = FindElement(tagBody, "tr", RowStatHistoryData);
            if (row == null)
            {
                Logger.ErrorFormat("ReportInvestorMonthly - не найден элемент <tr name=\"{0}\">",
                                   RowStatHistoryData);
                return;
            }
            var cellFund = (XmlElement)row.ChildNodes[2];
            cellFund.InnerXml = string.Format("{1:f2}%{0}{2:f0}{0}{3:f2}%{0}{4:f2}%{0}{5:f2}%{0}{6:f2}%{0}{7:f2}%",
                                              HtmlLineSeparator, stat.cumulativeReturn, stat.cumulativeVAMI, 
                                              stat.meanReturn, stat.compoundRoRmonth,
                                              stat.largestMonthGain, stat.largestMonthLoss, 
                                              stat.percentPositiveMonths);

            var rowRisk = FindElement(tagBody, "tr", RowRisk);
            // табличка Risk по фонду            
            if (rowRisk == null)
            {
                Logger.ErrorFormat("ReportInvestorMonthly - не найден элемент <tr name=\"{0}\">",
                                   RowRisk);
                return;
            }
            cellFund = (XmlElement)rowRisk.ChildNodes[1];
            cellFund.InnerXml = string.Format("{1:f2}%{0}{2:f2}%{0}{3:f2}%{0}{4:f2}%{0}{5:f2}%{0}{6:f1}",
                                              HtmlLineSeparator, stat.meanDeviation, stat.sharpRatio,
                                              stat.downsideDeviation8pc, stat.sortinoRatio8pc,
                                              stat.maxRelDrawdown, stat.drawDownMonths);

            for (var i = 0; i < 2; i++)
            {
                var cellBench = (XmlElement)row.ChildNodes[3 + i];
                //var monthlyProfitPlain = benchmarkData[i].Select(d => d.b).ToList();
                stat = new MonthlyReportStatistics(benchmarkDataVAMI[i], benchmarkDataROR[i].Select(p => p.equity).ToList());
                cellBench.InnerXml = string.Format("{1:f2}%{0}{2:f0}{0}{3:f2}%{0}{4:f2}%{0}{5:f2}%{0}{6:f2}%{0}{7:f2}%",
                                              HtmlLineSeparator, stat.cumulativeReturn, stat.cumulativeVAMI,
                                              stat.meanReturn, stat.compoundRoRmonth,
                                              stat.largestMonthGain, stat.largestMonthLoss,
                                              stat.percentPositiveMonths);
                var cellBenchRisk = (XmlElement)rowRisk.ChildNodes[2 + i];
                cellBenchRisk.InnerXml = string.Format("{1:f2}%{0}{2:f2}%{0}{3:f2}%{0}{4:f2}%{0}{5:f2}%{0}{6:f1}",
                                              HtmlLineSeparator, stat.meanDeviation, stat.sharpRatio,
                                              stat.downsideDeviation8pc, stat.sortinoRatio8pc,
                                              stat.maxRelDrawdown, stat.drawDownMonths);
            }            
        }

        private void CalculateCorrelation(XmlElement tagBody, List<float> fundRorMonthly)
        {
            var rowCorrelation = FindElement(tagBody, "tr", RowCorrelation);
            if (rowCorrelation == null)
            {
                Logger.ErrorFormat("Не найден элемент \"{0}\"", RowCorrelation);
                return;
            }

            var fundRor = fundRorMonthly;
            /*var prevVami = 0M;
            foreach (var vami in profitDic)
            {
                if (prevVami == 0)
                {
                    prevVami = VamiStart;
                    continue;
                }
                fundRor.Add(vami.Value / prevVami - 1);
                prevVami = vami.Value;
            }*/
            var avgFundRor = fundRor.Average();
            // берутся списки vamiBenchmark ..., т.к. там выровнены даты с
            // доходностью фонда
            var benchData = new[] {vamiBenchmarkA, vamiBenchmarkB};
            for (var i = 0; i < benchData.Count(); i++)
            {
                var benchmarkRor = new List<float>();
                for (var j = 1; j < benchData[i].Count; j++)
                {
                    benchmarkRor.Add(benchData[i][j].equity / benchData[i][j-1].equity - 1);
                }
                var avgBenchRor = benchmarkRor.Average();
                var denominator = benchmarkRor.Sum(br => (br - avgBenchRor)*(br - avgBenchRor));
                // получить коэфф Beta
                var numerator = 0f;
                for (var j = 0; j < benchmarkRor.Count; j++)                
                {
                    if (j == fundRor.Count) break;
                    numerator += (benchmarkRor[j] - avgBenchRor)*(fundRor[j] - avgFundRor);
                }
                var beta = denominator == 0 ? 0 : numerator/denominator;
                // коэфф Alpha
                var alpha = avgBenchRor == 0 ? 0 : avgFundRor - beta * avgBenchRor;
                // аннуализированный :) Альфа
                var alphAnn = Math.Pow(1 + (double) alpha, 12) - 1;
                // корреляция
                var sdevFund = fundRor.Sum(br => (br - avgFundRor) * (br - avgFundRor));
                var corr = sdevFund == 0 || denominator == 0
                               ? 0
                               : numerator / (float)Math.Sqrt((sdevFund * denominator));
                corr *= 100;
                // R^2
                float rsqNum = 0;
                for (var j = 0; j < benchmarkRor.Count; j++)
                {
                    rsqNum += (alpha + beta*benchmarkRor[j])*(alpha + beta*benchmarkRor[j]);
                }
                var rSqr = sdevFund == 0 ? 0 : rsqNum/sdevFund;

                // вкорячить в таблицу
                var cell = (XmlElement) rowCorrelation.ChildNodes[i + 1];
                cell.InnerXml = string.Format("{1:f2}%{0}{2:f2}{0}{3:f2}%{0}{4:f2}%{0}{5:f2}",
                                              HtmlLineSeparator, alpha * 100, beta, alphAnn * 100, corr, rSqr);
            }
        }

        private static XmlElement FindElement(XmlElement parent, string tagName, string nameAtrValue)
        {
            var xpath = string.Format(".//{0}[@name='{1}']", tagName, nameAtrValue);
            var item = (XmlElement)parent.SelectSingleNode(xpath);
            if (item == null)
                Logger.ErrorFormat("ReportInvestorMonthly: в шаблоне не найден {0}(name={1})",
                    tagName, nameAtrValue);
            return item;
        }

        /// <summary>
        /// вернуть полный путь к файлу с картинкой доходности
        /// </summary>
        private string MakeProfitChart(string tempFolder, Size chartSize)
        {
            var chart = new FastChart.Chart.FastChart
                            {
                                ShowLegend = true,
                                LegendPlacement = FastChart.Chart.FastChart.ChartLegendPlacement.Справа,
                                BorderStyle = BorderStyle.None,
                                DrawMargin = false
                            };
            var axisX = new FastAxis(FastAxisDirection.X, true)
                            {
                                ColorMainGrid = Color.FromArgb(205, 205, 205),
                                ColorSubGrid = Color.FromArgb(220, 220, 220),
                                DrawMainGrid = true,
                                DrawSubGrid = true,
                                Step = new FastChart.Chart.Cortege2<double, TimeSpan>(0,
                                                                                      new TimeSpan(356, 0, 0, 0)),
                                LabelFormat = "yyyy"
                            };
            var maxProfit = (double) profitDic.Max(p => p.equity);
            var axisY = new FastAxis(FastAxisDirection.Y, false)
                            {
                                ColorMainGrid = Color.FromArgb(205, 205, 205),
                                ColorSubGrid = Color.FromArgb(220, 220, 220),
                                DrawMainGrid = true,
                                DrawSubGrid = true,
                                AutoScale100 = false,
                                MinValue = new FastChart.Chart.Cortege2<double, DateTime>(0, default(DateTime)),
                                MaxValue = new FastChart.Chart.Cortege2<double, DateTime>(maxProfit * 1.1, 
                                    default(DateTime))
                            };
            if (maxProfit > 1000)
            {
                var stepSize = maxProfit < 2000 ? 200 : maxProfit < 4000 ? 500 : 1000;
                axisY.Step = new FastChart.Chart.Cortege2<double, TimeSpan>(stepSize, default(TimeSpan));
            }
            
            chart.Axes.Add(axisX);
            chart.Axes.Add(axisY);
            var seriesFund = new FastSeries("УК", FastSeriesType.Линия, axisX, axisY, false)
                             {PenLine = new Pen(Color.FromArgb(80, 5, 5), 2f), AntiAlias = true };
            seriesFund.points.AddRange(profitDic.Select(p => new FastSeriesPoint(p.time, p.equity)).ToList());
            chart.series.Add(seriesFund);
            // серии бенчмарков
            var seriesBmA = new FastSeries(benchmarkA, FastSeriesType.Линия, axisX, axisY, false) { PenLine = new Pen(Color.FromArgb(5, 80, 5), 2f), AntiAlias = true };
            var seriesBmB = new FastSeries(benchmarkB, FastSeriesType.Линия, axisX, axisY, false) { PenLine = new Pen(Color.FromArgb(5, 5, 80), 2f), AntiAlias = true };
            seriesBmA.points.AddRange(MakeBenchmarkProfit1000Points(vamiBenchmarkA));
            seriesBmB.points.AddRange(MakeBenchmarkProfit1000Points(vamiBenchmarkB));
            chart.series.Add(seriesBmA);
            chart.series.Add(seriesBmB);

            chart.Width = chartSize.Width;
            chart.Height = chartSize.Height;
            
            var img = new Bitmap(chartSize.Width, chartSize.Height);
            chart.DrawToBitmap(img, new Rectangle(0, 0, chartSize.Width, chartSize.Height));
            var path = string.Format("{0}\\chartProfit1000_{1}.png", tempFolder, DateTime.Now.Millisecond);
            img.Save(path, ImageFormat.Png);
            return path;
        }
    
        private void CalculateBenchmarksVAMI()
        {
            var benchProfits = new [] {profitBenchmarkA, profitBenchmarkB};            
            var startDate = profitDic.First().time;
            foreach (var profit in benchProfits)
            {
                var startIndex = profit.FindIndex(p => p.time >= startDate);

                var benchmarkVAMI = new List<EquityOnTime> { new EquityOnTime(VamiStart, profit[startIndex].time) };
                var curRate = (float)VamiStart;
                foreach (var dat in profit)
                {
                    if (dat.time < startDate) continue;
                    benchmarkVAMI.Add(new EquityOnTime(curRate = curRate*(1 + dat.equity), dat.time.AddDays(1)));
                }

                if (vamiBenchmarkA == null) vamiBenchmarkA = benchmarkVAMI;
                else vamiBenchmarkB = benchmarkVAMI;
            }
        }

        /// <summary>
        /// получить данные по доходности бенчмарков так, чтобы на каждую точку
        /// доходности фонда имелись данные по индексу
        /// на входе - доходность по индексу (бенчмарку)
        /// </summary>        
        private List<FastSeriesPoint> MakeBenchmarkProfit1000Points(List<EquityOnTime> profit)
        {
            var curIndex = 0;
            var curRate = (float)VamiStart;
            var listVami = new List<FastSeriesPoint>();
            
            foreach (var datePair in profitDic)
            {
                for (var i = curIndex; i < profit.Count; i++)
                {
                    if (profit[i].time <= datePair.time)
                    {
                        curRate = profit[i].equity;
                        curIndex = i;
                    }
                    else break;
                }
                listVami.Add(new FastSeriesPoint(datePair.time, (double)curRate));
            }
            return listVami;
        }

        private static string MakeProfitHistogram(
            List<EquityOnTime> dicProfit,
            string tempFolder,
            Size chartSize)
        {
            if (dicProfit.Count == 0) return string.Empty;
            // - 1 - получить данные для гистограммы
            var listProfit = dicProfit.ToList();
            
            var prevDate = listProfit[0].time;
            var prevDepo = listProfit[0].equity;

            var data = new List<float>();
            // индекс - год
            var newYearIndex = new List<FastChart.Chart.Cortege2<int, int>>();

            foreach (var pt in listProfit)
            {
                if (pt.time.Month == prevDate.Month) continue;
                if (pt.time.Year != prevDate.Year) newYearIndex.Add(new FastChart.Chart.Cortege2<int, int>(
                    data.Count, pt.time.Year));                    
                prevDate = pt.time;
                data.Add(100 * (pt.equity - prevDepo) / prevDepo);
                prevDepo = pt.equity;
            }
            if (listProfit[listProfit.Count - 2].time.Month != listProfit[listProfit.Count - 1].time.Month)
                data.Add(100 * (listProfit[listProfit.Count - 2].equity - listProfit[listProfit.Count - 1].equity) /
                    listProfit[listProfit.Count - 1].equity);
            if (data.Count < 2) return string.Empty;

            // нарисовать картинку
            var min = data.Min();
            var max = data.Max();
            const int paddingTop = 5, paddingBottom = 35, paddingLeft = 35, paddingRight = 15;
            const int marksYleft = 5;
            const int barWidthFix = 4;

            var scaleY = (chartSize.Height - paddingTop - paddingBottom)/(max - min);
            var zeroY = paddingTop + (int)(scaleY * max);
            var pureWidth = chartSize.Width - paddingLeft - paddingRight;
            var spaceForBar = pureWidth / data.Count;
            var barWidth = spaceForBar < barWidthFix ? spaceForBar : barWidthFix;
            if (barWidth < 1) barWidth = 1;
            var halfWidth = (int)Math.Ceiling(barWidth/2.0);

            var bmp = new Bitmap(chartSize.Width, chartSize.Height);

            // подписи оси Y
            var stepY = Math.Max(max, -min);
            stepY = stepY < 1 ? 0.1f : stepY < 2 ? 0.2f : stepY < 4 ? 0.25f : stepY < 8 ? 0.5f : stepY < 12 ? 1 
                : stepY < 15 ? 2 : 4;
            var marksY = new List<float>();
            for (var y = 0f; y < max; y+=stepY) marksY.Add(y);
            for (var y = -stepY; y > min; y -= stepY) marksY.Add(y);
            
            // собственно рисование
            using (var g = Graphics.FromImage(bmp))
            using (var brushUp = new SolidBrush(Color.FromArgb(25, 150, 10)))
            using (var brushDn = new SolidBrush(Color.FromArgb(150, 25, 10)))
            using (var penOutline = new Pen(Color.Black))
            using (var font = new Font("Calibri", 9.0f, FontStyle.Regular))
            using (var brushFont = new SolidBrush(Color.Black))
            {
                for (var i = 0; i < data.Count; i++)
                {
                    var x = paddingLeft + pureWidth * i / (data.Count - 1);
                    var y = zeroY - scaleY * data[i];

                    var upY = Math.Min(y, zeroY);
                    var dnY = Math.Max(y, zeroY);

                    var rect = new Rectangle(x - halfWidth, (int)upY,
                                                barWidth, (int)(dnY - upY));
                    var brush = data[i] > 0 ? brushUp : brushDn;
                    g.FillRectangle(brush, rect);
                    g.DrawRectangle(penOutline, rect);
                }
                // подписи по осям
                var stringFormat = new StringFormat {LineAlignment = StringAlignment.Center};
                foreach (var mark in marksY)
                {
                    var y = zeroY - scaleY * mark;
                    g.DrawString(string.Format("{0}%", mark), font, brushFont, marksYleft, (float)y, stringFormat);
                }
                stringFormat = new StringFormat { Alignment = StringAlignment.Center };
                foreach (var year in newYearIndex)
                {
                    var x = paddingLeft + pureWidth * year.a / (data.Count - 1);
                    g.DrawString(year.b.ToString(), font, brushFont,
                        x, chartSize.Height - paddingBottom + 3, stringFormat);
                }
            }

            var path = string.Format("{0}\\chartProfitHistogram_{1}.png", tempFolder, DateTime.Now.Millisecond);
            bmp.Save(path, ImageFormat.Png);
            return path;
        }

        private void FillMonthlyProfitTable(List<EquityOnTime> listProfit,
            XmlElement table, XmlElement tagBody)
        {
            if (listProfit.Count < 2) return;
            var prevDate = listProfit[0].time;
            var prevDepo = listProfit[0].equity;
            var startYear = listProfit[0].time.Year;
            // год - месяц - прибыль
            var data = new List<Cortege3<int, int, float?>>();
            var prevYear = startYear;
            
            foreach (var pt in listProfit)
            {
                if (pt.time.Month == prevDate.Month) continue;                
                var profit = 100*(pt.equity - prevDepo)/prevDepo;
                prevDepo = pt.equity;
                data.Add(new Cortege3<int, int, float?>(prevYear, prevDate.Month, profit));
                prevYear = pt.time.Year;
                prevDate = pt.time;
            }
            // добить список месяцами в начале и в конце
            for (var i = listProfit[0].time.Month - 1; i > 0; i--)
                data.Insert(0, new Cortege3<int, int, float?>(startYear, i, null));
            var lastYear = listProfit[listProfit.Count - 1].time.Year;
            for (var i = listProfit[listProfit.Count - 1].time.Month; i <= 12; i++)
                data.Add(new Cortege3<int, int, float?>(lastYear, i, null));

            // сформировать таблицу: 12 столбцов, строк по количеству лет
            XmlElement row = null;
            var isRowEven = false;
            lastYear = startYear;
            var listProfitByYearFund = new List<FastChart.Chart.Cortege2<int, float>>();

            var percentProduct = 1f;
            foreach (var dat in data)
            {
                var needNewRow = dat.a != lastYear || row == null;
                lastYear = dat.a;
                if (needNewRow)
                {
                    if (row != null)
                    {
                        var yearProfit = (percentProduct - 1)*100;
                        row.AppendChild(table.OwnerDocument.CreateElement("td")).InnerText =
                            string.Format("{0:f2}%", yearProfit);
                        listProfitByYearFund.Add(new FastChart.Chart.Cortege2<int, float>(lastYear - 1, yearProfit));
                    }
                    row = (XmlElement) table.AppendChild(table.OwnerDocument.CreateElement("tr"));
                    if (isRowEven)
                        row.Attributes.Append(table.OwnerDocument.CreateAttribute("class")).Value =
                            StyleMonthlyTableEvenRow;
                    isRowEven = !isRowEven;
                    row.AppendChild(table.OwnerDocument.CreateElement("td")).InnerXml = 
                        string.Format("<b>{0}</b>", dat.a);
                    percentProduct = 1;
                }
                if (dat.c.HasValue) 
                    percentProduct *= (dat.c.Value / 100f + 1);
                row.AppendChild(table.OwnerDocument.CreateElement("td")).InnerText = dat.c.HasValue ?
                    string.Format("{0:f2}%", dat.c.Value) : "-";
            }
            if (row != null)
            {
                row.AppendChild(table.OwnerDocument.CreateElement("td")).InnerText =
                    string.Format("{0:f2}%", (percentProduct - 1) * 100);                
            }

            // заполнить мини-табличку "returns by year"
            var listProfitByYearBench = new List<List<Util.Cortege2<int, float>>> 
                { new List<Util.Cortege2<int, float>>(), new List<Util.Cortege2<int, float>>() };
            foreach (var pair in listProfitByYearFund)
            {
                var year = pair.a;
                var dataA = profitBenchmarkA.Where(p => p.time.Year == year);
                listProfitByYearBench[0].Add(
                    new Util.Cortege2<int, float>(year, dataA.Count() == 0 ? 0 : dataA.Product(p => p.equity + 1)));
                var dataB = profitBenchmarkB.Where(p => p.time.Year == year);
                listProfitByYearBench[1].Add(
                    new Util.Cortege2<int, float>(year, dataB.Count() == 0 ? 0 : dataB.Product(p => p.equity + 1)));
            }

            row = FindElement(tagBody, "tr", RowRetByYear);
            if (row == null)
            {
                Logger.ErrorFormat("ReportInvestorMonthly - не найден элемент <tr name=\"{0}\">",
                                   RowRetByYear);
                return;
            }
            var cellYears = (XmlElement)row.ChildNodes[0];
            var cellYearsResultFund = (XmlElement)row.ChildNodes[1];
            cellYears.InnerXml = string.Join(HtmlLineSeparator, listProfitByYearFund.Select(p => p.a));            
            cellYearsResultFund.InnerXml = string.Join(HtmlLineSeparator, listProfitByYearFund.Select(p => 
                string.Format("{0:f2}%", p.b)));

            var cellYearsResultBenchA = (XmlElement)row.ChildNodes[2];
            cellYearsResultBenchA.InnerXml = string.Join(HtmlLineSeparator, listProfitByYearBench[0].Select(p =>
                string.Format("{0:f2}%", p.b)));
            var cellYearsResultBenchB = (XmlElement)row.ChildNodes[3];
            cellYearsResultBenchB.InnerXml = string.Join(HtmlLineSeparator, listProfitByYearBench[1].Select(p =>
                string.Format("{0:f2}%", p.b)));
        }

        private void FillLatestReturnsTable(XmlElement tagBody, List<float> profitMonthly)
        {
            if (profitMonthly.Count < 1) return;
            var row = FindElement(tagBody, "tr", RowLatestReturns);
            if (row == null)
            {
                Logger.ErrorFormat("ReportInvestorMonthly - не найден элемент <tr name=\"{0}\">",
                                   RowLatestReturns);
                return;
            }

            const int countRates = 3;
            var cells = new List<XmlElement>
                            {
                                (XmlElement) row.ChildNodes[2],
                                (XmlElement) row.ChildNodes[3],
                                (XmlElement) row.ChildNodes[4]
                            };
            var profits = new List<List<float>>
                              {
                                  profitMonthly,
                                  profitBenchmarkA.Select(p => p.equity).ToList(),
                                  profitBenchmarkB.Select(p => p.equity).ToList()
                              };
            
            for (var i = 0; i < countRates; i++)
            {
                var cellDest = cells[i];
                // доходность фонда
                var lastIndex = profits[i].Count - 1;
                var month1 = 100 * profits[i][lastIndex];
                var month3 = profits[i].Count < 3 ? 0 :
                    100 * (profits[i].GetRange(profits[i].Count - 3 - 1, 3).Product(p => 1 + p) - 1);
                var month6 = profits[i].Count < 6 ? 0 :
                    100 * (profits[i].GetRange(profits[i].Count - 6 - 1, 6).Product(p => 1 + p) - 1);
                var year1 = profits[i].Count < 12 ? 0 :
                    100 * (profits[i].GetRange(profits[i].Count - 12 - 1, 12).Product(p => 1 + p) - 1);
                var year2 = profits[i].Count < 24 ? 0 :
                    100 * (profits[i].GetRange(profits[i].Count - 24 - 1, 24).Product(p => 1 + p) - 1);
                var year3 = profits[i].Count < 36 ? 0 :
                    100 * (profits[i].GetRange(profits[i].Count - 36 - 1, 36).Product(p => 1 + p) - 1);

                cellDest.InnerXml = string.Join(HtmlLineSeparator,
                new List<float> { month1, month3, month6, year1, year2, year3 }.Select(
                    p => string.Format("{0:f2}%", p)));
            }            
        }
    }
}

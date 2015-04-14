using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FastChart.Chart;
using TradeSharp.Util;

namespace TradeSharp.Client.BL.Report
{
    class ReportRenderClassic : ReportRender
    {
        protected override void RenderDocumentHead(StringBuilder sb)
        {
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<style type=\"text/css\">");
            sb.AppendLine(".rowCaption");
            sb.AppendLine(" {");
            sb.AppendLine("    color:#FFFFFF;");
            sb.AppendLine("    font-family: \"Arno Pro\";");
            sb.AppendLine("    font-size: 110%;");
            sb.AppendLine("    font-weight:800;");
            sb.AppendLine("    text-align:center;");
            sb.AppendLine("    background-color:#105008;");
            sb.AppendLine("    padding-top:0;");
            sb.AppendLine("    margin-top:0;");
            sb.AppendLine("        }");
            sb.AppendLine(" .monthlyTableEvenRow");
            sb.AppendLine(" {");
            sb.AppendLine("    background-color:#D2D2D2;");
            sb.AppendLine("        }");
            sb.AppendLine(" table");
            sb.AppendLine(" {");
            sb.AppendLine("    border:0px;");
            sb.AppendLine("    border-collapse:collapse;");
            sb.AppendLine("        }");
            sb.AppendLine(" tr");
            sb.AppendLine(" {");
            sb.AppendLine("    border-width:1px;");
            sb.AppendLine("        }");
            sb.AppendLine(" td");
            sb.AppendLine(" {");
            sb.AppendLine("    font-family: \"Minion Pro\";");
            sb.AppendLine("    font-size: 85%;");
            sb.AppendLine("    padding:4px;    ");
            sb.AppendLine("        }");
            sb.AppendLine(" h1");
            sb.AppendLine(" {");
            sb.AppendLine("    font-size: 120%;");
            sb.AppendLine("        }");
            sb.AppendLine("</style>      ");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
        }

        protected override void RenderStatTableHeader(StringBuilder sb, string indent)
        {
            indent += "  ";
            sb.AppendLine(string.Format("{0}<table callpadding=\"0\" cellspacing=\"0\" border=\"0\">",
                                        indent));
        }

        protected override void RenderStatTable(StringBuilder sb, string indent)
        {
            var dicParam = new Dictionary<string, string>
                               {
                                   { "Начальный баланс счета", stat.InitialBalance.ToStringUniformMoneyFormat() + " " + account.Currency },
                                   { "Текущий баланс счета", account.Equity.ToStringUniformMoneyFormat() + " " + account.Currency },
                                   { "Общая прибыль / убыток", (stat.sumClosedResult + stat.sumOpenResult).ToStringUniformMoneyFormat() + " " + account.Currency },
                                   { "Прибыль / убыток по открытым позициям", stat.sumOpenResult.ToStringUniformMoneyFormat() + " " + account.Currency },
                                   { "Макс. относительное проседание", stat.Statistics.MaxRelDrawDown.ToString("f2") + "%" },
                                   { "С. геом. доходность, год", (stat.ProfitGeomYear * 100).ToString("f2") + "%" },
                                   { "С. геом. доходность, месяц", (stat.ProfitGeomMonth * 100).ToString("f2") + "%" },
                                   { "С. геом. доходность, день", (stat.ProfitGeomDay * 100).ToString("f2") + "%" }
                               };
            foreach (var ptr in dicParam)
            {
                sb.AppendLine(string.Format("{0}  <tr> <td>{1}</td> <td>{2}</td> </tr>",
                                        indent, ptr.Key, ptr.Value));
            }

            
        }

        protected override void RenderSummaryOpenDealsTableHeader(StringBuilder sb, string indent)
        {
            sb.AppendLine(string.Format("{0}<h3>Открытые позиции</h3>", indent));
            indent += "  ";
            sb.AppendLine(string.Format("{0}<table callpadding=\"0\" cellspacing=\"0\" border=\"0\">",
                                        indent));
            indent += "  ";
            sb.AppendLine(string.Format("{0}<tr class=\"rowCaption\">", indent));
            sb.AppendLine(string.Format("{0}  <td>Контракт<td> <td>Тип</td> <td>Объем</td> <td>Плечо</td> <td>Вход</td> <td>Прибыль</td>", indent));
            sb.AppendLine(string.Format("{0}</tr>", indent));
        }

        protected override void RenderSummaryOpenDealsTable(StringBuilder sb, string indent, List<PositionSummary> positions)
        {
            indent += "    ";
            var isOdd = false;
            foreach (var pos in positions)
            {
                if (string.IsNullOrEmpty(pos.Symbol))
                    pos.Symbol = "<b>суммарная</b>";

                if (isOdd)
                    sb.AppendLine(string.Format("{0}<tr>", indent));
                else
                    sb.AppendLine(string.Format("{0}<tr style=\"background-color:#CECECE\">", indent));
                isOdd = !isOdd;

                sb.AppendLine(string.Format("{0}  <td>{1}<td> <td>{2}</td> <td>{3}</td> <td>{4}</td> <td>{5}</td> <td style=\"text-align:right\">{6}</td>", indent,
                    pos.Symbol, pos.Side > 0 ? "BUY" : "SELL", pos.Volume.ToStringUniformMoneyFormat(),
                    pos.Leverage.ToString("f2"),
                    pos.AveragePrice.ToStringUniformPriceFormat(),
                    pos.Profit.ToStringUniformMoneyFormat() + " " + account.Currency));

                sb.AppendLine(string.Format("{0}</tr>", indent));
            }
        }

        protected override void RenderOpenDealsTableHeader(StringBuilder sb, string indent)
        {
            sb.AppendLine(string.Format("{0}</br>", indent));
            indent += "  ";
            sb.AppendLine(string.Format("{0}<table callpadding=\"0\" cellspacing=\"0\" border=\"0\">",
                                        indent));
            indent += "  ";
            sb.AppendLine(string.Format("{0}<tr class=\"rowCaption\">", indent));
            sb.AppendLine(string.Format("{0}  <td>Контракт<td> <td>Тип</td> <td>Объем</td> <td>Вход</td> <td>Время</td> <td>SL</td> <td>TP</td>", indent));
            sb.AppendLine(string.Format("{0}</tr>", indent));
        }

        protected override void RenderOpenDealsTable(StringBuilder sb, string indent)
        {
            indent += "    ";
            var isOdd = false;

            foreach (var pos in openedOrders)
            {
                if (isOdd)
                    sb.AppendLine(string.Format("{0}<tr>", indent));
                else
                    sb.AppendLine(string.Format("{0}<tr style=\"background-color:#CECECE\">", indent));
                isOdd = !isOdd;

                sb.AppendLine(string.Format("{0}  <td>{1}<td> <td>{2}</td> <td>{3}</td> <td>{4}</td> <td>{5:dd.MM.yyyy HH:mm}</td> <td>{6}</td> <td>{7}</td>", 
                    indent,
                    pos.Symbol, 
                    pos.Side > 0 ? "BUY" : "SELL", 
                    pos.Volume.ToStringUniformMoneyFormat(),
                    pos.PriceEnter.ToStringUniformPriceFormat(),
                    pos.TimeEnter,
                    pos.StopLoss.ToStringUniformPriceFormat(" "),
                    pos.TakeProfit.ToStringUniformPriceFormat(" ")));

                sb.AppendLine(string.Format("{0}</tr>", indent));
            }
        }

        protected override void RenderClosedDealsTableHeader(StringBuilder sb, string indent)
        {
            sb.AppendLine(string.Format("{0}<h3>Закрытые позиции</h3>", indent));
            indent += "  ";
            sb.AppendLine(string.Format("{0}<table callpadding=\"0\" cellspacing=\"0\" border=\"0\">",
                                        indent));
            indent += "  ";
            sb.AppendLine(string.Format("{0}<tr class=\"rowCaption\">", indent));
            sb.AppendLine(string.Format("{0}  <td>Контракт<td> <td>Тип</td> <td>Объем</td> <td>Вход</td> <td>Время</td> <td>Выход</td> <td>Время</td> <td>Прибыль</td>", indent));
            sb.AppendLine(string.Format("{0}</tr>", indent));
        }

        protected override void RenderClosedDealsTable(StringBuilder sb, string indent)
        {
            indent += "    ";
            var isOdd = false;

            foreach (var pos in closedOrders)
            {
                if (isOdd)
                    sb.AppendLine(string.Format("{0}<tr>", indent));
                else
                    sb.AppendLine(string.Format("{0}<tr style=\"background-color:#CECECE\">", indent));
                isOdd = !isOdd;

                sb.AppendLine(string.Format("{0}  <td>{1}<td> <td>{2}</td> <td>{3}</td> <td>{4}</td> <td>{5:dd.MM.yyyy HH:mm}</td> <td>{6}</td> <td>{7:dd.MM.yyyy HH:mm}</td> <td style=\"text-align:right\">{8}</td>",
                    indent,
                    pos.Symbol,
                    pos.Side > 0 ? "BUY" : "SELL",
                    pos.Volume.ToStringUniformMoneyFormat(),
                    pos.PriceEnter.ToStringUniformPriceFormat(),
                    pos.TimeEnter,
                    pos.PriceExit.Value.ToStringUniformPriceFormat(),
                    pos.TimeExit,
                    pos.ResultDepo.ToStringUniformMoneyFormat() + " " + account.Currency));

                sb.AppendLine(string.Format("{0}</tr>", indent));
            }
        }

        protected override void RenderProfitChart(StringBuilder sb, string indent)
        {
            var series = new List<List<FastSeriesPoint>>();
            var names = new List<string> {"средства"};

            series.Add(stat.listEquity.Select(e => new FastSeriesPoint(e.time, e.equity)).ToList());
            //if (stat.balanceByDate != null)
            //{
            //    series.Add(stat.balanceByDate.Select(e => new FastSeriesPoint(e.a, e.b)).ToList());
            //    names.Add("баланс");
            //}

            RenderChart(sb, "Динамика баланса счета, " + account.Currency, 800, 500, series, names);
        }

        private void RenderChart(StringBuilder sb, string title, int width, int height, 
            List<List<FastSeriesPoint>> series, List<string> seriesNames)
        {
            var seriesColors = new [] {Color.DarkBlue, Color.DarkOliveGreen, Color.Firebrick};

            var needYearsInFormat = stat.listEquity[stat.listEquity.Count - 1].time.Year != stat.listEquity[0].time.Year;
            var formatString = needYearsInFormat ? "dd.MM.yyyy" : "dd MMM";

            var chart = new FastChart.Chart.FastChart
            {
                ShowLegend = series.Count > 1,
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
                LabelFormat = formatString
            };

            // границы оси Y
            var min = double.MaxValue;
            var max = double.MinValue;
            foreach (var ser in series)
            {
                foreach (var pt in ser)
                {
                    if (min > pt.y) min = pt.y;
                    if (max < pt.y) max = pt.y;
                }
            }
            min = GetLowerBound(min);
            
            var axisY = new FastAxis(FastAxisDirection.Y, false)
            {
                ColorMainGrid = Color.FromArgb(205, 205, 205),
                ColorSubGrid = Color.FromArgb(220, 220, 220),
                DrawMainGrid = true,
                DrawSubGrid = true,
                AutoScale100 = false,
                MinValue = new FastChart.Chart.Cortege2<double, DateTime>(min, default(DateTime)),
                MaxValue = new FastChart.Chart.Cortege2<double, DateTime>(max * 1.05, default(DateTime))
            };
            

            chart.Axes.Add(axisX);
            chart.Axes.Add(axisY);

            // добавить серии и точки
            for (var i = 0; i < series.Count; i++)
            {
                var ser = new FastSeries(seriesNames[i], FastSeriesType.Линия, axisX, axisY, false)
                              {
                                  PenLine = new Pen(seriesColors[i], 2f), AntiAlias = true
                              };
                ser.points.AddRange(series[i]);
                chart.series.Add(ser);
            }

            chart.Width = width;
            chart.Height = height;

            // рендерить чарт в картинку
            var img = new Bitmap(width, height);
            chart.DrawToBitmap(img, new Rectangle(0, 0, width - 1, height - 1));

            // включить картинку в HTML
            var str = BaseWebServer.MakeEmbeddedPictureString(img);
            var imgStr = string.Format(
                "<img src=\"{0}\" alt=\"Доходность по счету\" style=\"cursor:pointer\" /> ", str);

            sb.AppendLine("<br/>");
            sb.AppendLine(imgStr);
            sb.AppendLine(string.Format("    <p style=\"font-weight:bold\">{0}</p>", title));
        }

        /// <summary>
        /// !! включить в чарт
        /// </summary>
        private static double GetLowerBound(double x)
        {
            if (x <= 0) return x;
            if (x < 100) return 0;
            if (x < 200) return 100;
            if (x < 1000) return ((int)(x / 100)) * 100;
            if (x < 1500) return 1000;
            if (x < 2000) return 1000;
            if (x < 5000) return ((int)(x / 500)) * 500;
            if (x < 20000) return ((int)(x / 1000)) * 1000;
            return ((int)(x / 10000)) * 10000;
        }
    }
}

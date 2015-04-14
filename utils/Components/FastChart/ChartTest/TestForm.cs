using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using FastChart.Chart;

namespace ChartTest
{
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();

            fastChart1.LoadBackImageFromResource(Assembly.GetExecutingAssembly(), 
                "ChartTest.img.logo_fxi_invert_transp.png");            
        }

        private void fastChart1_DoubleClick(object sender, EventArgs e)
        {            
            fastChart1.Axes.Add(new FastAxis(FastAxisDirection.X, true));
            fastChart1.Axes.Add(new FastAxis(FastAxisDirection.Y, false));
            fastChart1.Axes.Add(new FastAxis(FastAxisDirection.Y, false));
            var series = new FastSeries("EURUSD", FastSeriesType.Линия, fastChart1.Axes[0], fastChart1.Axes[1], true)
            {
                PenLine = new Pen(Color.DarkGray) { Width = 2 },
                AntiAlias = true
            };

            var rnd = new Random();
            var start = 1.2940;

            for (var date = DateTime.Now.AddHours(-500); date <= DateTime.Now; date = date.AddHours(1))
            {
                start += (rnd.Next(121) - 60) / 10000.0;
                series.points.Add(new FastSeriesPoint(date, start));
            }
            fastChart1.series.Add(series);


            series = new FastSeries("DJ100", FastSeriesType.Линия,
                fastChart1.Axes[0], fastChart1.Axes[2], true)
            {
                PenLine = new Pen(Color.DeepPink) { Width = 2 },
                AntiAlias = true
            };
            start = 1800.0;
            for (var date = DateTime.Now.AddHours(-500); date <= DateTime.Now; date = date.AddHours(1))
            {
                start += (rnd.Next(121) - 60) / 15.0;
                series.points.Add(new FastSeriesPoint(date, start));
            }
            fastChart1.series.Add(series);
            fastChart1.Invalidate();
        }
    }
}

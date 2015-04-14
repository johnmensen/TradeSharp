using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FastMultiChart;

namespace FastMultiChartTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            // spec part
            fastMultiChart1.GetXScaleValue = FastMultiChartUtils.GetDateTimeScaleValue;
            //fastMultiChart1.GetYScaleValue = GetYScaleValueSpec;
            fastMultiChart1.GetYScaleValue = (value, chart) => (int)((double) value * 1000);
            fastMultiChart1.GetXValue = FastMultiChartUtils.GetDateTimeValue;
            //fastMultiChart1.GetYValue = GetYValueSpec;
            fastMultiChart1.GetYValue = (value, chart) => value / 1000.0;
            fastMultiChart1.GetXDivisionValue = FastMultiChartUtils.GetDateTimeDivisionValue;
            fastMultiChart1.GetMinXScaleDivision = FastMultiChartUtils.GetDateTimeMinScaleDivision;
            fastMultiChart1.GetMinYScaleDivision = FastMultiChartUtils.GetDoubleMinScaleDivision;
            fastMultiChart1.GetXStringValue = FastMultiChartUtils.GetDateTimeStringValue;
            //fastMultiChart1.GetYStringValue = GetYStringScaleValueSpec;
            fastMultiChart1.GetXStringScaleValue = FastMultiChartUtils.GetDateTimeStringScaleValue;
            //fastMultiChart1.GetYStringScaleValue = GetYStringScaleValueSpec;
            //end of spec part

            // spec part
            // series 0
            fastMultiChart1.Graphs[0].Series.Add(new Series("T", "D", new Pen(Color.Blue, 2)));
            var nowDate = DateTime.Now.Date;
            var depo = 1000d;
            var rnd = new Random();
            for (var date = nowDate.AddYears(-3); date <= nowDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday)
                    continue;
                var percent = (rnd.Next(1000) - 500d) / 25000;
                var largeChange = rnd.Next(100) < 5;
                if (largeChange) percent *= 4;
                var delta = depo * percent;
                depo += delta;
                fastMultiChart1.Graphs[0].Series[0].Add(new GraphData { T = date, D = depo });
            }

            // series 1
            fastMultiChart1.Graphs[0].Series.Add(new Series("T", "Y", new Pen(Color.Green, 2)));
            for (var i = 0; i < 100; i++)
            {
                fastMultiChart1.Graphs[0].Series[1].Add(new GraphData { X = i * 10, Y = 2 * rnd.Next(200), T = new DateTime(2012, 8, 14) + new TimeSpan(i * 10, 0, 0, 0) });
                fastMultiChart1.Graphs[0].Series[1].Add(new GraphData { X = i * 10 + 1, Y = 1 * rnd.Next(200), T = new DateTime(2012, 8, 15) + new TimeSpan(i * 10, 0, 0, 0) });
                fastMultiChart1.Graphs[0].Series[1].Add(new GraphData { X = i * 10 + 2, Y = 3 * rnd.Next(200), T = new DateTime(2012, 8, 16) + new TimeSpan(i * 10, 0, 0, 0) });
                fastMultiChart1.Graphs[0].Series[1].Add(new GraphData { X = i * 10 + 3, Y = 6 * rnd.Next(200), T = new DateTime(2012, 8, 17) + new TimeSpan(i * 10, 0, 0, 0) });
                fastMultiChart1.Graphs[0].Series[1].Add(new GraphData { X = i * 10 + 4, Y = 10 * rnd.Next(200), T = new DateTime(2012, 8, 18) + new TimeSpan(i * 10, 0, 0, 0) });
                fastMultiChart1.Graphs[0].Series[1].Add(new GraphData { X = i * 10 + 5, Y = 9 * rnd.Next(200), T = new DateTime(2012, 8, 19) + new TimeSpan(i * 10, 0, 0, 0) });
                fastMultiChart1.Graphs[0].Series[1].Add(new GraphData { X = i * 10 + 6, Y = 7 * rnd.Next(200), T = new DateTime(2012, 8, 20) + new TimeSpan(i * 10, 0, 0, 0) });
                fastMultiChart1.Graphs[0].Series[1].Add(new GraphData { X = i * 10 + 7, Y = 4 * rnd.Next(200), T = new DateTime(2012, 8, 21) + new TimeSpan(i * 10, 0, 0, 0) });
                fastMultiChart1.Graphs[0].Series[1].Add(new GraphData { X = i * 10 + 8, Y = -5 * rnd.Next(200), T = new DateTime(2012, 8, 22) + new TimeSpan(i * 10, 0, 0, 0) });
                fastMultiChart1.Graphs[0].Series[1].Add(new GraphData { X = i * 10 + 9, Y = 5 * rnd.Next(200), T = new DateTime(2012, 8, 23) + new TimeSpan(i * 10, 0, 0, 0) });
            }
            // end of spec part
            fastMultiChart1.Initialize();
        }
        // spec part
        private int GetYScaleValueSpec(object value, FastMultiChart.FastMultiChart chart)
        {
            return (int)((double)value * 10);
        }

        private object GetYValueSpec(int scaleValue, FastMultiChart.FastMultiChart chart)
        {
            return scaleValue / 10.0;
        }

        private string GetYStringScaleValueSpec(object value, FastMultiChart.FastMultiChart chart)
        {
            return ((double)value).ToString();
        }
        //end of spec part
    }

    public struct Cortege2<A, B>
    {
        public A a { get; set; }
        public B b { get; set; }
        public Cortege2(A _a, B _b)
            : this()
        {
            a = _a;
            b = _b;
        }
        public bool IsDefault()
        {
            return Equals(default(Cortege2<A, B>));
        }
        public override string ToString()
        {
            return string.Format("{0};{1}", a, b);
        }
    }

    //spec part
    class GraphData
    {
        public int X { get; set; }
        public double Y { get; set; }
        [DisplayName("Дата")]
        public DateTime T { get; set; }
        public double D { get; set; }
    }
    //end of spec part
}

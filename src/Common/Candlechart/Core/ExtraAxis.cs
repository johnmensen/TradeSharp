using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using Candlechart.Chart;
using Candlechart.ChartMath;
using Candlechart.Series;
using Candlechart.Theme;

namespace Candlechart.Core
{
    public class ExXAxis : Axis
    {
        private readonly ArrayList _dates;
        private readonly ArrayList _grid;
        private readonly XAxisLabelStrategy _labelStrategy;
        private readonly ArrayList _majorLabels;
        private readonly ArrayList _minorLabels;

        // Methods
        internal ExXAxis(Pane owner)
            : base(owner)
        {
            Period = Period.Other;
            _dates = new ArrayList();
            _majorLabels = new ArrayList();
            _minorLabels = new ArrayList();
            _grid = new ArrayList();
            _labelStrategy = new XAxisLabelStrategy();
        }

        internal Rectangle AxisRect
        {
            get
            {
                Rectangle clientRect = Owner.ClientRect;
                clientRect.Offset(-clientRect.Left, -clientRect.Top);
                return clientRect;
            }
        }

        private ArrayList Dates
        {
            get { return _dates; }
        }

        private double DaysPerBar { get; set; }

        internal int FixedHeight
        {
            get { return ((Font.Height + 6) * 2); }
        }

        internal ArrayList Grid
        {
            get { return _grid; }
        }

        private XAxisLabelStrategy LabelStrategy
        {
            get { return _labelStrategy; }
        }

        private int LastIndex { get; set; }

        internal ArrayList MajorLabels
        {
            get { return _majorLabels; }
        }

        internal ArrayList MinorLabels
        {
            get { return _minorLabels; }
        }

        public Period Period { get; set; }

        private int StartIndex { get; set; }

        public override Color BackColor
        {
            get { return Chart.visualSettings.XAxisBackColor; }
        }

        public override Color ForeColor
        {
            get { return Chart.visualSettings.XAxisForeColor; }
        }

        public override Color TextColor
        {
            get { return Chart.visualSettings.XAxisTextColor; }
        }

        public override Font Font
        {
            get { return Chart.visualSettings.XAxisFont; }
        }

        public override Color GridLineColor
        {
            get { return Chart.visualSettings.XAxisGridLineColor; }
        }

        public override GridLineStyle GridLineStyle
        {
            get { return Chart.visualSettings.XAxisGridLineStyle; }
        }

        public override bool GridLineVisible
        {
            get { return Chart.visualSettings.XAxisGridLineVisible; }
        }

        public override Color GridBandColor
        {
            get { return Chart.visualSettings.XAxisGridBandColor; }
        }

        public override bool GridBandVisible
        {
            get { return Chart.visualSettings.XAxisGridBandVisible; }
        }

        private void DetermineDates()
        {
            Dates.Clear();
            DateTime startDate = GetStartDate();
            int month = startDate.Month;
            int year = startDate.Year;
            var dayOfWeek = (int)startDate.DayOfWeek;
            DateTime time2 = startDate.AddDays(-7.0);
            int startIndex = StartIndex;
            while (startIndex <= LastIndex)
            {
                double xValue = startIndex;
                bool startWeek = false;
                bool startMonth = false;
                bool startYear = false;
                if (((((int)startDate.DayOfWeek < dayOfWeek) || (startDate.DayOfWeek == DayOfWeek.Monday)) ||
                     (startDate.Subtract(time2).Days >= 7)) &&
                    ((startDate.DayOfWeek != DayOfWeek.Saturday) && (startDate.DayOfWeek != DayOfWeek.Sunday)))
                {
                    time2 = startDate;
                    startWeek = true;
                }
                dayOfWeek = (int)startDate.DayOfWeek;
                if (startDate.Month != month)
                {
                    month = startDate.Month;
                    startMonth = true;
                }
                if (startDate.Year != year)
                {
                    year = startDate.Year;
                    startYear = true;
                }
                Dates.Add(new XAxisLabelInfo(startDate, xValue, startWeek, startMonth, startYear));
                GetNextDate(ref startDate, ref startIndex);
            }
        }

        private void DetermineDaysPerBar()
        {
            if (Chart.StockSeries.Data.Count > 1)
            {
                TimeSpan span = (Chart.StockSeries.Data[Chart.StockSeries.Data.Count - 1].timeOpen -
                                 Chart.StockSeries.Data[0].timeOpen);
                DaysPerBar = (span.Days) / ((double)(Chart.StockSeries.Data.Count - 1));
                if (DaysPerBar < 0.005) DaysPerBar = 0.005; // !!
            }
            else
            {
                DaysPerBar = 1.4;
            }
        }

        private void DetermineLabels(Graphics g)
        {
            string str;
            MajorLabels.Clear();
            MinorLabels.Clear();
            Grid.Clear();
            string format = LabelStrategy.ShowLongMonth ? "MMMM" : "MMM";
            if (LabelStrategy.ShowDayLabel)
            {
                foreach (XAxisLabelInfo info in Dates)
                {
                    str = info.Date.Day.ToString();
                    MinorLabels.Add(new XAxisMinorLabel(info.Date, info.X, str, info.StartWeek));
                    if (info.StartMonth)
                    {
                        if (LabelStrategy.ShowCharMonth)
                        {
                            str = info.Date.ToString(format).Substring(0, 1);
                        }
                        else
                        {
                            str = info.StartYear ? info.Date.ToString("yyyy") : info.Date.ToString(format);
                        }
                        MajorLabels.Add(new XAxisMajorLabel(info.Date, info.X, str, true));
                    }
                    if (info.StartWeek)
                    {
                        Grid.Add(new XAxisLabelInfo(info.Date, info.X, false, false, false));
                    }
                }
            }
            else if (LabelStrategy.ShowWeekLabel)
            {
                //Label_02AB: // !!
                foreach (XAxisLabelInfo info2 in Dates)
                {
                    if (info2.StartWeek || LabelStrategy.ShowDayTick)
                    {
                        str = info2.StartWeek ? info2.Date.Day.ToString() : string.Empty;
                        MinorLabels.Add(new XAxisMinorLabel(info2.Date, info2.X, str, info2.StartWeek));
                        if (info2.StartWeek)
                        {
                            Grid.Add(new XAxisLabelInfo(info2.Date, info2.X, false, false, false));
                        }
                    }
                    if (!info2.StartMonth)
                    {
                        // goto Label_02AB; // !!
                        continue;
                    }
                    if (LabelStrategy.ShowCharMonth)
                    {
                        str = info2.Date.ToString(format).Substring(0, 1);
                    }
                    else
                    {
                        str = info2.StartYear ? info2.Date.ToString("yyyy") : info2.Date.ToString(format);
                    }
                    MajorLabels.Add(new XAxisMajorLabel(info2.Date, info2.X, str, true));
                }
            }
            else if (LabelStrategy.ShowMonthLabel)
            {
                //Label_0401:
                foreach (XAxisLabelInfo info3 in Dates)
                {
                    if (!info3.StartWeek && !info3.StartMonth)
                    {
                        //goto Label_0401;
                        continue;
                    }
                    if (info3.StartMonth)
                    {
                        str = info3.Date.ToString(format);
                        if (LabelStrategy.ShowCharMonth)
                        {
                            str = str.Substring(0, 1);
                        }
                        MinorLabels.Add(new XAxisMinorLabel(info3.Date, info3.X, str, true));
                        if (info3.StartYear)
                        {
                            str = info3.Date.ToString("yyyy");
                            MajorLabels.Add(new XAxisMajorLabel(info3.Date, info3.X, str, true));
                        }
                        Grid.Add(new XAxisLabelInfo(info3.Date, info3.X, false, false, false));
                        continue;
                    }
                    if (LabelStrategy.ShowWeekTick)
                    {
                        MinorLabels.Add(new XAxisMinorLabel(info3.Date, info3.X, string.Empty, false));
                    }
                }
            }
            else if (LabelStrategy.ShowQuarterLabel)
            {
                //Label_0557:
                foreach (XAxisLabelInfo info4 in Dates)
                {
                    if (!info4.StartMonth)
                    {
                        //goto Label_0557; !!
                        continue;
                    }
                    if ((info4.Date.Month % 3) == 1)
                    {
                        str = info4.Date.ToString(format);
                        if (LabelStrategy.ShowCharMonth)
                        {
                            str = str.Substring(0, 1);
                        }
                        MinorLabels.Add(new XAxisMinorLabel(info4.Date, info4.X, str, true));
                        Grid.Add(new XAxisLabelInfo(info4.Date, info4.X, false, false, false));
                    }
                    else if (LabelStrategy.ShowMonthTick)
                    {
                        MinorLabels.Add(new XAxisMinorLabel(info4.Date, info4.X, string.Empty, false));
                    }
                    if (info4.StartYear)
                    {
                        str = info4.Date.ToString("yyyy");
                        MajorLabels.Add(new XAxisMajorLabel(info4.Date, info4.X, str, true));
                    }
                }
            }
            else if (LabelStrategy.Show1YearLabel)
            {
                foreach (XAxisLabelInfo info5 in Dates)
                {
                    if (info5.StartMonth)
                    {
                        if (info5.StartYear)
                        {
                            str = info5.Date.ToString("yyyy");
                            MinorLabels.Add(new XAxisMinorLabel(info5.Date, info5.X, str, true));
                            Grid.Add(new XAxisLabelInfo(info5.Date, info5.X, false, false, false));
                        }
                        else if (((info5.Date.Month % 3) == 1) && LabelStrategy.ShowQuarterTick)
                        {
                            MinorLabels.Add(new XAxisMinorLabel(info5.Date, info5.X, string.Empty, false));
                        }
                    }
                }
            }
            else if (LabelStrategy.Show5YearLabel)
            {
                foreach (XAxisLabelInfo info6 in Dates)
                {
                    if (info6.StartMonth && info6.StartYear)
                    {
                        if ((info6.Date.Year % 5) == 0)
                        {
                            str = info6.Date.ToString("yyyy");
                            MinorLabels.Add(new XAxisMinorLabel(info6.Date, info6.X, str, true));
                            Grid.Add(new XAxisLabelInfo(info6.Date, info6.X, false, false, false));
                        }
                        else
                        {
                            MinorLabels.Add(new XAxisMinorLabel(info6.Date, info6.X, string.Empty, false));
                        }
                    }
                }
            }
        }

        private void DetermineLabelStrategy(Graphics g)
        {
            LabelStrategy.ShowDayLabel = true;
            int num = 0;
            int num2 = 7;
            int num3 = 0;
            int num4 = 0x1f;
            int num5 = 0;
            int num6 = 270;
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            foreach (XAxisLabelInfo info in Dates)
            {
                num++;
                num3++;
                num5++;
                if (info.StartWeek)
                {
                    if (flag)
                    {
                        num2 = (num < num2) ? num : num2;
                    }
                    flag = true;
                    num = 0;
                }
                if (info.StartMonth)
                {
                    if (flag2)
                    {
                        num4 = (num3 < num4) ? num3 : num4;
                    }
                    flag2 = true;
                    num3 = 0;
                    if (!info.StartYear)
                    {
                        continue;
                    }
                    if (flag3)
                    {
                        num6 = (num5 < num6) ? num5 : num6;
                    }
                    flag3 = true;
                    num5 = 0;
                }
            }
            RectangleD worldRect = Owner.WorldRect;
            Rectangle canvasRect = Owner.CanvasRect;
            SizeF ef = g.MeasureString("00.", Font);
            SizeF ef2 = g.MeasureString("00.", Font);
            SizeF ef3 = g.MeasureString("September.", Font);
            SizeF ef4 = g.MeasureString("Sep.", Font);
            SizeF ef5 = g.MeasureString("S.", Font);
            SizeF ef6 = g.MeasureString("2005.", Font);
            SizeD ed2 = Conversion.WorldToScreen(new SizeD(StockSeries.MinimumXDelta, 0.0), worldRect,
                                                 canvasRect);
            LabelStrategy.ShowDayTick = ed2.Width > 8.0;
            LabelStrategy.ShowWeekTick = (ed2.Width * num2) > 8.0;
            LabelStrategy.ShowMonthTick = (ed2.Width * num4) > 8.0;
            LabelStrategy.ShowQuarterTick = ((ed2.Width * num4) * 3.0) > 8.0;
            LabelStrategy.ShowDayLabel = ed2.Width > ef.Width;
            LabelStrategy.ShowWeekLabel = (ed2.Width * num2) > ef2.Width;
            LabelStrategy.ShowMonthLabel = (ed2.Width * num4) > ef5.Width;
            LabelStrategy.ShowQuarterLabel = ((ed2.Width * num4) * 3.0) > ef5.Width;
            LabelStrategy.Show1YearLabel = (ed2.Width * num6) > ef6.Width;
            LabelStrategy.Show5YearLabel = ((ed2.Width * num6) * 5.0) > ef6.Width;
            if (LabelStrategy.ShowMonthLabel)
            {
                LabelStrategy.ShowLongMonth = (ed2.Width * num4) > ef3.Width;
                LabelStrategy.ShowShortMonth = ((ed2.Width * num4) > ef4.Width) && ((ed2.Width * num4) <= ef3.Width);
                LabelStrategy.ShowCharMonth = ((ed2.Width * num4) > ef5.Width) && ((ed2.Width * num4) <= ef4.Width);
            }
            else
            {
                LabelStrategy.ShowLongMonth = ((ed2.Width * num4) * 3.0) > ef3.Width;
                LabelStrategy.ShowShortMonth = (((ed2.Width * num4) * 3.0) > ef4.Width) &&
                                               (((ed2.Width * num4) * 3.0) <= ef3.Width);
                LabelStrategy.ShowCharMonth = (((ed2.Width * num4) * 3.0) > ef5.Width) &&
                                              (((ed2.Width * num4) * 3.0) <= ef4.Width);
            }
        }

        private void DetermineRange(Graphics g)
        {
            RectangleD worldRect = Owner.WorldRect;
            Rectangle canvasRect = Owner.CanvasRect;
            PointF tf = PointD.Truncate(Conversion.ScreenToWorld(new PointD(0.0, 0.0), worldRect, canvasRect));
            PointF tf2 = PointD.Ceiling(Conversion.ScreenToWorld(new PointD(AxisRect.Right, 0.0), worldRect, canvasRect));
            StartIndex = (int)tf.X;
            LastIndex = (int)tf2.X;
        }

        internal override void Draw(Graphics g)
        {
            DetermineDaysPerBar();
            DetermineRange(g);
            DetermineDates();
            DetermineLabelStrategy(g);
            DetermineLabels(g);
            DrawBackground(g);
            DrawLabels(g);
        }

        private void DrawBackground(Graphics g)
        {
            var brush = new SolidBrush(BackColor);
            var pen = new Pen(ForeColor);
            using (brush)
            {
                g.FillRectangle(brush, AxisRect);
            }
            using (pen)
            {
                g.DrawLine(pen, AxisRect.Left, (AxisRect.Top + Font.Height) + 6, AxisRect.Right - 1,
                           (AxisRect.Top + Font.Height) + 6);
            }
        }

        private void DrawLabels(Graphics g)
        {
            RectangleD worldRect = Owner.WorldRect;
            Rectangle canvasRect = Owner.CanvasRect;
            var brush = new SolidBrush(TextColor);
            var brush2 = new SolidBrush(BackColor);
            var pen = new Pen(ForeColor);
            using (brush)
            {
                using (brush2)
                {
                    using (pen)
                    {
                        Point point;
                        SizeF ef;
                        foreach (XAxisMajorLabel label in MajorLabels)
                        {
                            point =
                                PointD.Round(Conversion.WorldToScreen(new PointD(label.X, 0.0), worldRect, canvasRect));
                            ef = g.MeasureString(label.Label, Font);
                            if (label.Label != string.Empty)
                            {
                                g.FillRectangle(brush2, (point.X + 2), ((Font.Height + 6) + 1), ef.Width,
                                                (Font.Height + 6));
                            }
                            if (label.FullTick)
                            {
                                g.DrawLine(pen, point.X, 0, point.X, AxisRect.Height - 1);
                            }
                            else
                            {
                                g.DrawLine(pen, point.X, Font.Height + 6, point.X, AxisRect.Height - 1);
                            }
                            if (label.Label != string.Empty)
                            {
                                g.DrawString(label.Label, Font, brush, (point.X + 2), (Font.Height + 9));
                            }
                        }
                        foreach (XAxisMinorLabel label2 in MinorLabels)
                        {
                            point =
                                PointD.Round(Conversion.WorldToScreen(new PointD(label2.X, 0.0), worldRect, canvasRect));
                            ef = g.MeasureString(label2.Label, Font);
                            if (label2.Label != string.Empty)
                            {
                                g.FillRectangle(brush2, (point.X + 2), 0f, ef.Width, (Font.Height + 6));
                            }
                            if (label2.FullTick)
                            {
                                g.DrawLine(pen, point.X, 0, point.X, Font.Height + 6);
                            }
                            else
                            {
                                g.DrawLine(pen, point.X, 0, point.X, 2);
                            }
                            if (label2.Label != string.Empty)
                            {
                                g.DrawString(label2.Label, Font, brush, (point.X + 2), 3f);
                            }
                        }
                    }
                }
            }
        }

        private void GetNextDate(ref DateTime date, ref int index)
        {
            if ((index >= 0) && (index < (Chart.StockSeries.Data.Count - 1)))
            {
                index++;
                date = Chart.StockSeries.Data[index].timeOpen;
            }
            else
            {
                index++;
                date = Period == Period.Daily ? GetNextWeekday(date, 1) : date.AddDays(DaysPerBar);
            }
        }

        private static DateTime GetNextWeekday(DateTime date, int steps)
        {
            DateTime time = date;
            for (int i = 0; i < steps; i++)
            {
                time = time.AddDays(1.0);
                while ((time.DayOfWeek == DayOfWeek.Saturday) || (time.DayOfWeek == DayOfWeek.Sunday))
                {
                    time = time.AddDays(1.0);
                }
            }
            return time;
        }

        private static DateTime GetPreviousWeekday(DateTime date, int steps)
        {
            DateTime time = date;
            for (int i = 0; i < steps; i++)
            {
                time = time.AddDays(-1.0);
                while ((time.DayOfWeek == DayOfWeek.Saturday) || (time.DayOfWeek == DayOfWeek.Sunday))
                {
                    time = time.AddDays(-1.0);
                }
            }
            return time;
        }

        private DateTime GetStartDate()
        {
            DateTime today;
            int startIndex;
            if (StartIndex < 0)
            {
                if (Chart.StockSeries.Data.Count == 0)
                {
                    startIndex = 0;
                    today = DateTime.Today;
                }
                else
                {
                    startIndex = 0;
                    today = Chart.StockSeries.Data[startIndex].timeOpen;
                }
                while (startIndex > StartIndex)
                {
                    startIndex--;
                    today = Period == Period.Daily ? GetPreviousWeekday(today, 1) : today.AddDays(-DaysPerBar);
                }
            }
            else if (StartIndex >= Chart.StockSeries.Data.Count)
            {
                if (Chart.StockSeries.Data.Count == 0)
                {
                    startIndex = 0;
                    today = DateTime.Today;
                }
                else
                {
                    startIndex = Chart.StockSeries.Data.Count - 1;
                    today = Chart.StockSeries.Data[startIndex].timeOpen;
                }
                while (startIndex < StartIndex)
                {
                    startIndex++;
                    if (Period == Period.Daily)
                    {
                        today = GetNextWeekday(today, 1);
                    }
                    else
                    {
                        today = today.AddDays(DaysPerBar);
                    }
                }
            }
            else
            {
                startIndex = StartIndex;
                today = Chart.StockSeries.Data[startIndex].timeOpen;
            }
            //startIndex = GetStartIndex(startIndex, ref today);
            GetNextDate(ref today, ref startIndex);
            StartIndex = startIndex;
            return today;
        }

        private int GetStartIndex(int startIndex, ref DateTime today)
        {
            int year = today.Year;
            while (today.Year == year)
            {
                if ((startIndex >= 1) && (startIndex < Chart.StockSeries.Data.Count))
                {
                    startIndex--;
                    today = Chart.StockSeries.Data[startIndex].timeOpen;
                }
                else
                {
                    startIndex--;
                    if (Period == Period.Daily)
                    {
                        today = GetPreviousWeekday(today, 1);
                        continue;
                    }
                    today = today.AddDays(-DaysPerBar);
                }
            }
            int month = today.Month;
            while (today.Month == month)
            {
                if ((startIndex >= 1) && (startIndex < Chart.StockSeries.Data.Count))
                {
                    startIndex--;
                    today = Chart.StockSeries.Data[startIndex].timeOpen;
                }
                else
                {
                    startIndex--;
                    if (Period == Period.Daily)
                    {
                        today = GetPreviousWeekday(today, 1);
                        continue;
                    }
                    today = today.AddDays(-DaysPerBar);
                }
            }
            DateTime time2 = today;
            var dayOfWeek = (int)today.DayOfWeek;
            do
            {
                if ((startIndex >= 1) && (startIndex < Chart.StockSeries.Data.Count))
                {
                    startIndex--;
                    today = Chart.StockSeries.Data[startIndex].timeOpen;
                }
                else
                {
                    startIndex--;
                    today = Period == Period.Daily ? GetPreviousWeekday(today, 1) : today.AddDays(-DaysPerBar);
                }
            } while (((int)today.DayOfWeek < dayOfWeek) && (time2.Subtract(today).Days < 7));
            return startIndex;
        }

        internal void PrepareToDraw(Graphics g)
        {
        }
    }

    internal class XAxisLabelInfo
    {
        // Fields
        private readonly DateTime _date;
        private readonly bool _startMonth;
        private readonly bool _startWeek;
        private readonly bool _startYear;
        private readonly double _x;

        // Methods
        public XAxisLabelInfo(DateTime date, double x, bool startWeek, bool startMonth, bool startYear)
        {
            _date = date;
            _x = x;
            _startWeek = startWeek;
            _startMonth = startMonth;
            _startYear = startYear;
        }

        // Properties
        public DateTime Date
        {
            get { return _date; }
        }

        public bool StartMonth
        {
            get { return _startMonth; }
        }

        public bool StartWeek
        {
            get { return _startWeek; }
        }

        public bool StartYear
        {
            get { return _startYear; }
        }

        public double X
        {
            get { return _x; }
        }
    }

    internal class XAxisLabelStrategy
    {
        // Fields

        // Properties
        public bool Show1YearLabel { get; set; }

        public bool Show5YearLabel { get; set; }

        public bool ShowCharMonth { get; set; }

        public bool ShowDayLabel { get; set; }

        public bool ShowDayTick { get; set; }

        public bool ShowLongMonth { get; set; }

        public bool ShowMonthLabel { get; set; }

        public bool ShowMonthTick { get; set; }

        public bool ShowQuarterLabel { get; set; }

        public bool ShowQuarterTick { get; set; }

        public bool ShowShortMonth { get; set; }

        public bool ShowWeekLabel { get; set; }

        public bool ShowWeekTick { get; set; }
    }

    internal class XAxisMajorLabel
    {
        // Fields
        private readonly DateTime _date;
        private readonly bool _fullTick;
        private readonly string _label;
        private readonly double _x;

        // Methods
        public XAxisMajorLabel(DateTime date, double x, string label, bool fullTick)
        {
            _date = date;
            _x = x;
            _label = label;
            _fullTick = fullTick;
        }

        // Properties
        public DateTime Date
        {
            get { return _date; }
        }

        public bool FullTick
        {
            get { return _fullTick; }
        }

        public string Label
        {
            get { return _label; }
        }

        public double X
        {
            get { return _x; }
        }
    }

    internal class XAxisMinorLabel
    {
        // Fields
        private readonly DateTime _date;
        private readonly bool _fullTick;
        private readonly string _label;
        private readonly double _x;

        // Methods
        public XAxisMinorLabel(DateTime date, double x, string label, bool fullTick)
        {
            _date = date;
            _x = x;
            _label = label;
            _fullTick = fullTick;
        }

        // Properties
        public DateTime Date
        {
            get { return _date; }
        }

        public bool FullTick
        {
            get { return _fullTick; }
        }

        public string Label
        {
            get { return _label; }
        }

        public double X
        {
            get { return _x; }
        }
    }

    internal class XAxisPane : Pane
    {
        // Fields
        private readonly XAxis _xAxis;

        // Methods
        internal XAxisPane(ChartControl owner)
            : base("XAxis", owner)
        {
            _xAxis = new XAxis(this);
        }

        // Properties
        internal override Rectangle CanvasRect
        {
            get
            {
                var rectangle = new Rectangle(0, 0, ClientRect.Width, ClientRect.Height);
                if (rectangle.Width < 0)
                {
                    rectangle.Width = 0;
                }
                if (rectangle.Height < 0)
                {
                    rectangle.Height = 0;
                }
                return rectangle;
            }
        }

        internal int FixedHeight
        {
            get { return ((XAxis.FixedHeight + PaneFrame.TopBorderWidth) + PaneFrame.BottomBorderWidth); }
        }

        internal override RectangleD WorldRect
        {
            get { return Owner.StockPane.WorldRect; }
        }

        public XAxis XAxis
        {
            get { return _xAxis; }
        }

        internal override void Draw(Graphics g)
        {
            PaneFrame.TitleBoxVisible = false;
            PaneFrame.CanResize = false;
            PaneFrame.Draw(g);
            GraphicsContainer container = g.BeginContainer();
            g.TranslateTransform(ClientRect.X, ClientRect.Y);
            DrawXAxis(g);
            g.EndContainer(container);
        }

        private void DrawXAxis(Graphics g)
        {
            GraphicsContainer container = g.BeginContainer();
            g.SetClip(new Rectangle(0, 0, ClientRect.Width, ClientRect.Height));
            XAxis.Draw(g);
            g.EndContainer(container);
        }
    }

    public enum YAxisAlignment
    {
        Left,
        Right,
        Both
    }

    internal class YAxisLabelInfo
    {
        // Fields
        public double Exponent;
        public double MajorStep;
        public double Max;
        public double Min;
        public double MinorStep;
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using Candlechart.Chart;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    [LocalizedDisplayName("TitleDaySeparators")]
    [LocalizedCategory("TitleVisualization")]
    [TypeConverter(typeof(PropertySorter))]
    class IndicatorDaysRuler : BaseChartIndicator, IChartIndicator
    {
        #region Массивы цветов и стилей

        private const int StyleIndexDay = 0;
        private const int StyleIndexWeek = 1;
        private const int StyleIndexMonth = 2;

        private readonly Color[] colors = new [] { Color.Brown, Color.Brown, Color.RosyBrown };
        private readonly float[] widths = new float[] { 1, 1, 1 };
        private readonly DashStyle[] styles = new[] { DashStyle.Dot, DashStyle.Dash, DashStyle.Solid };
        
        #endregion

        #region Флаги
        
        private bool showDays = true;
        [LocalizedDisplayName("TitleShowDays")]
        [LocalizedDescription("MessageShowDaySeparatorDescription")]
        [LocalizedCategory("TitleMain")]
        public bool ShowDays
        {
            get { return showDays; }
            set { showDays = value; }
        }

        private bool showWeeks = true;
        [LocalizedDisplayName("TitleShowWeeks")]
        [LocalizedDescription("MessageShowWeekSeparatorDescription")]
        [LocalizedCategory("TitleMain")]
        public bool ShowWeeks
        {
            get { return showWeeks; }
            set { showWeeks = value; }
        }

        private bool showMonths = true;
        [LocalizedDisplayName("TitleShowMonths")]
        [LocalizedDescription("MessageShowMonthSeparatorDescription")]
        [LocalizedCategory("TitleMain")]
        public bool ShowMonths
        {
            get { return showMonths; }
            set { showMonths = value; }
        }

        #endregion

        #region Цвета линий

        [LocalizedDisplayName("TitleDaySeparatorColor")]
        [LocalizedDescription("MessageDaySeparatorColorDescription")]
        [LocalizedCategory("TitleVisuals")]
        public Color LineColorDays
        {
            get { return colors[StyleIndexDay]; }
            set { colors[StyleIndexDay] = value; }
        }

        [LocalizedDisplayName("TitleWeekSeparatorColor")]
        [LocalizedDescription("MessageWeekSeparatorColorDescription")]
        [LocalizedCategory("TitleVisuals")]
        public Color LineColorWeeks
        {
            get { return colors[StyleIndexWeek]; }
            set { colors[StyleIndexWeek] = value; }
        }

        [LocalizedDisplayName("TitleMonthSeparatorColor")]
        [LocalizedDescription("MessageMonthSeparatorColorDescription")]
        [LocalizedCategory("TitleVisuals")]
        public Color LineColorMonths
        {
            get { return colors[StyleIndexMonth]; }
            set { colors[StyleIndexMonth] = value; }
        }

        #endregion

        #region Стили линий

        [LocalizedDisplayName("TitleDaySeparatorLineStyle")]
        [LocalizedDescription("MessageLineStyleDescription")]
        [LocalizedCategory("TitleVisuals")]
        public DashStyle DaysLineStyle
        {
            get { return styles[StyleIndexDay]; }
            set { styles[StyleIndexDay] = value; }
        }

        [LocalizedDisplayName("TitleWeekSeparatorLineStyle")]
        [LocalizedDescription("MessageLineStyleDescription")]
        [LocalizedCategory("TitleVisuals")]
        public DashStyle WeeksLineStyle
        {
            get { return styles[StyleIndexWeek]; }
            set { styles[StyleIndexWeek] = value; }
        }

        [LocalizedDisplayName("TitleMonthSepartorLineStyle")]
        [LocalizedDescription("MessageLineStyleDescription")]
        [LocalizedCategory("TitleVisuals")]
        public DashStyle MonthsLineStyle
        {
            get { return styles[StyleIndexMonth]; }
            set { styles[StyleIndexMonth] = value; }
        }

        #endregion

        #region Толщина линий

        [LocalizedDisplayName("TitleDayLineThickness")]
        [LocalizedDescription("MessageDayLineThicknessDescription")]
        [LocalizedCategory("TitleVisuals")]
        public float DaysLineWidth
        {
            get { return widths[StyleIndexDay]; }
            set { widths[StyleIndexDay] = value; }
        }

        [LocalizedDisplayName("TitleWeekLineThickness")]
        [LocalizedDescription("MessageWeekLineThicknessDescription")]
        [LocalizedCategory("TitleVisuals")]
        public float WeeksLineWidth
        {
            get { return widths[StyleIndexWeek]; }
            set { widths[StyleIndexWeek] = value; }
        }

        [LocalizedDisplayName("TitleMonthLineThickness")]
        [LocalizedDescription("MessageMonthLineThicknessDescription")]
        [LocalizedCategory("TitleVisuals")]
        public float MonthsLineWidth
        {
            get { return widths[StyleIndexMonth]; }
            set { widths[StyleIndexMonth] = value; }
        }

        #endregion

        private readonly TrendLineSeries series = new TrendLineSeries(Localizer.GetString("TitleSeparators"));
        
        public override BaseChartIndicator Copy()
        {
            var dr = new IndicatorDaysRuler();
            Copy(dr);
            return dr;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var dr = (IndicatorDaysRuler)indi;
            CopyBaseSettings(dr);
            dr.ShowDays = ShowDays;
            dr.ShowMonths = ShowDays;
            dr.ShowWeeks = ShowWeeks;

            for (var i = 0; i < widths.Length; i++) dr.widths[i] = widths[i];
            for (var i = 0; i < colors.Length; i++) dr.colors[i] = colors[i];
            for (var i = 0; i < styles.Length; i++) dr.styles[i] = styles[i];
        }

        public IndicatorDaysRuler()
        {
            CreateOwnPanel = false;
        }

        [Browsable(false)]
        public override string Name { get { return Localizer.GetString("TitlePeriodSeparator"); } }

        public void BuildSeries(ChartControl chart)
        {
            if (series != null) series.data.Clear();

            if (!ShowMonths && !ShowDays && !ShowWeeks) return;
            
            var candles = chart.StockSeries.Data.Candles;
            int? lastDay = null;
            int? lastWeek = null; 
            int? lastMonth = null;
            
            for (var i = 0; i < candles.Count; i++)
            {
                var time = candles[i].timeOpen;
                // месяцы
                if (ShowMonths)
                {
                    if (!lastMonth.HasValue)
                        lastMonth = time.Month;
                    else if (time.Month != lastMonth.Value)
                    {
                        lastMonth = time.Month;
                        AddLine(StyleIndexMonth, i, (double) candles[i].open);
                        continue;
                    }
                }
                // недели
                if (ShowWeeks)
                {
                    var week = GetWeek(time);
                    if (!lastWeek.HasValue)
                        lastWeek = week;
                    else if (week != lastWeek.Value)
                    {
                        lastWeek = week;
                        AddLine(StyleIndexWeek, i, (double)candles[i].open);
                        continue;
                    }
                }
                // дни
                if (ShowDays && chart.Timeframe.Intervals[0] < 1440)
                {
                    if (!lastDay.HasValue) lastDay = time.Day;
                    else if (time.Day != lastDay)
                    {
                        lastDay = time.Day;
                        AddLine(StyleIndexDay, i, (double)candles[i].open);
                    }
                }
            } 
        }

        private void AddLine(int scaleIndex, int i, double spanPrice)
        {            
            var line = new TrendLine
            {
                LineColor = colors[scaleIndex],
                LineStyle = TrendLine.TrendLineStyle.Линия,
                PenDashStyle = styles[scaleIndex],
                PenWidth = widths[scaleIndex]
            };
            line.AddPoint(i, 0);
            line.AddPoint(i, spanPrice);
            series.data.Add(line);
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            SeriesResult = new List<Series.Series> { series };
            
            EntitleIndicator();
        }

        public void Remove()
        {
            if (series != null) series.data.Clear();
        }

        public void AcceptSettings()
        {
        }

        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (newCandles.Count == 0) return;
            BuildSeries(owner);
        }

        public string GetHint(int x, int y, double index, double price, int tolerance)
        {
            return string.Empty;
        }

        public List<IChartInteractiveObject> GetObjectsUnderCursor(int screenX, int screenY, int tolerance)
        {
            return new List<IChartInteractiveObject>();
        }
    
        private static int GetWeek(DateTime date)
        {
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                date, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }
    }
}

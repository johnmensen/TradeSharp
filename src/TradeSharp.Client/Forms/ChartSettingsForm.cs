using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using Candlechart.Chart;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class ChartSettingsForm : Form
    {
        private readonly ChartForm activeChart;

        private readonly IList<ChartForm> allCharts;

        private readonly ColorSchemePicker colorPicker;

        public ChartSettingsForm()
        {
            InitializeComponent();
        }

        public ChartSettingsForm(ChartForm activeChart, IList<ChartForm> allCharts)
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
            this.activeChart = activeChart;
            this.allCharts = allCharts;
            cbYAxis.Items.AddRange(EnumItem<YAxisAlignment>.items.Cast<object>().ToArray());
            cbChartType.Items.AddRange(EnumItem<StockSeries.CandleDrawMode>.items.Cast<object>().ToArray());
            cbTheme.Items.AddRange(EnumItem<ChartControl.Themes>.items.Cast<object>().ToArray());

            // компонент выбора цветовой схемы
            colorPicker = new ColorSchemePicker(pictureBoxCandle);
            colorPicker.OnPickColor += colorPicker_OnPickColor;
            cbUseMarkerPrice.Checked = activeChart == null ? true : activeChart.chart.chart.StockSeries.ShowLastQuote;
            cbAutoScroll.Checked = activeChart == null ? true : activeChart.chart.chart.StockSeries.AutoScroll;
            InitColorPicker();

            // заполнить значения полей
            if (activeChart != null)
                InitFields(activeChart);
            else
            {
                InitFields(allCharts);
                btnAccept.Enabled = false;
            }
        }

        private void colorPicker_OnPickColor(Color cl, int colorIndex)
        {
            colorDialog.Color = cl;
            if (colorDialog.ShowDialog() != DialogResult.OK) return;
            colorPicker.SetColor(colorIndex, colorDialog.Color);
            colorPicker.Draw();
            pictureBoxCandle.Invalidate();
        }

        private void InitColorPicker()
        {
            if (activeChart != null)
            {
                colorPicker.colorBackground = activeChart.chart.chart.visualSettings.PaneBackColor;
                colorPicker.colorFillGrowth = activeChart.chart.chart.visualSettings.StockSeriesUpFillColor;
                colorPicker.colorFillSlope = activeChart.chart.chart.visualSettings.StockSeriesDownFillColor;
                colorPicker.colorOutlineGrowth = activeChart.chart.chart.visualSettings.StockSeriesUpLineColor;
                colorPicker.colorOutlineSlope = activeChart.chart.chart.visualSettings.StockSeriesDownLineColor;
            }            
            colorPicker.Draw();
        }

        private void InitFields(ChartForm chart)
        {
            InitFieldsSeries(new List<ChartForm> { chart });
            tbShiftBars.Text = chart.chart.chart.StockSeries.BarOffset.ToString();
            cbChartType.SelectedIndex = EnumItem<StockSeries.CandleDrawMode>.items.FindIndex(m => m.Value == chart.chart.chart.StockSeries.BarDrawMode);
            cbTheme.SelectedIndex = EnumItem<ChartControl.Themes>.items.FindIndex(s => s.Value == chart.chart.chart.visualSettings.Theme);
        }

        private void InitFields(IList<ChartForm> charts)
        {
            if (charts.Count == 0) return;

            if (charts.Count == 1)
            {
                InitFields(charts[0]);
                return;
            }

            InitFieldsSeries(charts);

            var barOffsetVector = charts.Select(c => c.chart.chart.StockSeries.BarOffset);
            if (barOffsetVector.AreEqual()) 
                tbShiftBars.Text = charts[0].chart.chart.StockSeries.BarOffset.ToString();
        }

        private void InitFieldsSeries(IList<ChartForm> charts)
        {
            if (charts == null) return;
            if (charts.Count == 0) return;

            var fiboSeriesVector = charts.Select(c => c.chart.seriesTurnBar.fibonacciSeries);
            tbTurnBarFiboSequence.Text = fiboSeriesVector.AreEqual()
                                             ? string.Join(",", charts[0].chart.seriesTurnBar.fibonacciSeries) : "";

            var fiboMarksVector = charts.Select(c => c.chart.seriesTurnBar.fibonacciMarks);
            tbTurnBarFiboMarks.Text = fiboMarksVector.AreEqual()
                                          ? string.Join(",", charts[0].chart.seriesTurnBar.fibonacciMarks) : "";

            var fiboFilterVector = charts.Select(c => c.chart.seriesTurnBar.fibonacciTurnBarFilter);
            tbTurnBarFilter.Text = fiboFilterVector.AreEqual()
                                       ? charts[0].chart.seriesTurnBar.fibonacciTurnBarFilter.ToString() : "";

            cbTurnBarsDontSumDegrees.Checked = charts[0].chart.seriesTurnBar.DontSumDegree;

            var hasDistinctAlign = charts.Count < 2
                                       ? false
                                       : charts.Any(c => c.chart.chart.YAxisAlignment != charts[0].chart.chart.YAxisAlignment);
            cbYAxis.SelectedIndex = !hasDistinctAlign ? EnumItem<YAxisAlignment>.items.FindIndex(a => a.Value == charts[0].chart.chart.YAxisAlignment) : -1;
        }

        private void AcceptSettings(IList<ChartForm> charts)
        {
            foreach (var chart in charts)
            {
                // смещение в барах
                chart.chart.chart.StockSeries.BarOffset = int.Parse(tbShiftBars.Text);
                
                // цветовая схема
                if (cbTheme.SelectedItem != null)
                {
                    var theme = ((EnumItem<ChartControl.Themes>)cbTheme.SelectedItem).Value;
                    if (chart.chart.chart.visualSettings.Theme != theme)
                    {
                        chart.chart.chart.visualSettings.Theme = theme;
                        chart.chart.chart.visualSettings.ApplyTheme();
                        // подстроить цвета
                        chart.chart.AdjustColors();
                    }
                }

                // цвета
                chart.chart.chart.visualSettings.StockSeriesUpFillColor = colorPicker.colorFillGrowth;
                chart.chart.chart.visualSettings.StockSeriesDownFillColor = colorPicker.colorFillSlope;
                chart.chart.chart.visualSettings.StockSeriesUpLineColor = colorPicker.colorOutlineGrowth;
                chart.chart.chart.visualSettings.StockSeriesDownLineColor = colorPicker.colorOutlineSlope;
                chart.chart.chart.visualSettings.PaneBackColor = colorPicker.colorBackground;
                //chart.chart.chart.visualSettings.PaneFrameBackColor = colorPicker.colorBackground;
                
                chart.chart.chart.StockSeries.ShowLastQuote = cbUseMarkerPrice.Checked;
                chart.chart.chart.AutoScroll = cbAutoScroll.Checked;

                if (cbChartType.SelectedItem != null)
                    chart.chart.chart.StockSeries.BarDrawMode = ((EnumItem<StockSeries.CandleDrawMode>)cbChartType.SelectedItem).Value;

                // расположение оси
                if (cbYAxis.SelectedItem != null)
                    chart.chart.chart.YAxisAlignment = ((EnumItem<YAxisAlignment>)cbYAxis.SelectedItem).Value;

                // серии
                ApplySeriesSettingsToChart(chart);

                chart.chart.UpdateChartIconsState();

                // применить настройки
                chart.Invalidate();
            }
        }

        private void ApplySeriesSettingsToChart(ChartForm chart)
        {
            chart.chart.seriesTurnBar.fibonacciSeries = tbTurnBarFiboSequence.Text.ToIntArrayUniform();
            chart.chart.seriesTurnBar.fibonacciMarks = tbTurnBarFiboMarks.Text.ToIntArrayUniform();
            chart.chart.seriesTurnBar.fibonacciTurnBarFilter = tbTurnBarFilter.Text.ToInt();
            chart.chart.seriesTurnBar.DontSumDegree = cbTurnBarsDontSumDegrees.Checked;
            chart.chart.chart.RightBars = tbShiftBars.Text.ToInt();
        }

        private void BtnApplyToAllClick(object sender, EventArgs e)
        {// применить ко всем окнам
            AcceptSettings(allCharts);
        }
        
        private void BtnAcceptClick(object sender, EventArgs e)
        {// применить к выделенному окну
            if (activeChart == null) return;
            AcceptSettings(new List<ChartForm> { activeChart });            
        }

        private void CbThemeSelectedIndexChanged(object sender, EventArgs e)
        {
            var theme = ((EnumItem<ChartControl.Themes>)cbTheme.SelectedItem).Value;
            var visSet = new ChartControl.ChartVisualSettings {Theme = theme};
            visSet.ApplyTheme();

            colorPicker.colorFillGrowth = visSet.StockSeriesUpFillColor;
            colorPicker.colorFillSlope = visSet.StockSeriesDownFillColor;
            colorPicker.colorOutlineGrowth = visSet.StockSeriesUpLineColor;
            colorPicker.colorOutlineSlope = visSet.StockSeriesDownLineColor;
            colorPicker.colorBackground = visSet.PaneBackColor;
            colorPicker.Draw(true);
        }

        private void ChartSettingsFormLoad(object sender, EventArgs e)
        {
            tabControl.TabPages["tabPageSettings"].Enabled = HiddenModes.ManagerMode;
        }
    }

    class ColorSchemePicker
    {
        public Color colorBackground = Color.White;
        public Color colorOutlineGrowth = Color.Gray;
        public Color colorOutlineSlope = Color.Gray;
        public Color colorFillGrowth = Color.White;
        public Color colorFillSlope = Color.Black;

        private readonly PictureBox picture;

        private readonly Point[] pickPoints = new[]
                                         {
                                             new Point(9, 9),
                                             new Point(42, 15), new Point(57, 61),
                                             new Point(23, 41), new Point(79, 32)
                                         };

        private const int PickSize = 4;

        public delegate void OnPickColorDel(Color cl, int colorIndex);

        private event OnPickColorDel onPickColor;
        public event OnPickColorDel OnPickColor
        {
            add { onPickColor += value; }
            remove { onPickColor -= value; }
        }

        public ColorSchemePicker(PictureBox picture)
        {
            this.picture = picture;
            picture.MouseMove += OnPictureMouseMove;
            picture.MouseUp += OnPictureMouseUp;
        }

        public void Draw(bool invalidate = false)
        {
            if (picture.Image == null)
                picture.Image = new Bitmap(picture.Width, picture.Height);
            using (var gr = Graphics.FromImage(picture.Image))
            {
                // залить
                using (var br = new SolidBrush(colorBackground))
                {
                    gr.FillRectangle(br, 0, 0, picture.Width, picture.Height);
                }
                // нарисовать бары
                using (var brUp = new SolidBrush(colorFillGrowth))
                using (var brDn = new SolidBrush(colorFillSlope))
                using (var penUp = new Pen(colorOutlineGrowth))
                using (var penDn = new Pen(colorOutlineSlope))
                {
                    gr.FillRectangle(brUp, new Rectangle(14, 23, 17, 44));
                    gr.FillRectangle(brDn, new Rectangle(70, 18, 17, 44));
                    gr.DrawRectangle(penUp, new Rectangle(14, 23, 17, 44));
                    gr.DrawRectangle(penDn, new Rectangle(70, 18, 17, 44));
                    gr.DrawLine(penUp, 22, 7, 22, 22);
                    gr.DrawLine(penUp, 22, 15, 37, 15);
                    gr.DrawLine(penDn, 78, 63, 78, 72);
                    gr.DrawLine(penDn, 78, 68, 62, 66);
                }
                // нарисовать квадратики выбора цвета                
                using (var penUp = new Pen(Color.LightGray))
                using (var penDn = new Pen(Color.DarkGray))
                    for (var i = 0; i < pickPoints.Length; i++)
                    {
                        int upX = pickPoints[i].X - PickSize;
                        int upY = pickPoints[i].Y - PickSize;
                        int dnX = pickPoints[i].X + PickSize;
                        int dnY = pickPoints[i].Y + PickSize;
                        using (var br = new SolidBrush(GetColor(i)))
                        {
                            gr.FillRectangle(br, upX, upY, PickSize * 2, PickSize * 2);
                        }
                        gr.DrawLine(penUp, upX, upY, upX, dnY);
                        gr.DrawLine(penUp, upX, upY, dnX, upY);
                        gr.DrawLine(penDn, dnX, dnY, upX, dnY);
                        gr.DrawLine(penDn, dnX, dnY, dnX, upY);
                    }
            }
            if (invalidate) picture.Invalidate();
        }

        private void OnPictureMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) return;
            if (!IsInPickArea(e.X, e.Y)) return;
            for (var i = 0; i < pickPoints.Length; i++)            
            {
                var pt = pickPoints[i];
                var deltaX = Math.Abs(e.X - pt.X);
                var deltaY = Math.Abs(e.Y - pt.Y);
                if (deltaX <= PickSize && deltaY <= PickSize)
                {
                    // отобразить диалог выбора цвета
                    if (onPickColor != null) onPickColor(GetColor(i), i);
                }
            }
        }        
    
        private bool IsInPickArea(int x, int y)
        {
            //x -= picture.Left;
            //y -= picture.Top;
            foreach (var pt in pickPoints)
            {
                var deltaX = Math.Abs(x - pt.X);
                var deltaY = Math.Abs(y - pt.Y);
                if (deltaX <= PickSize && deltaY <= PickSize) return true;
            }
            return false;
        }

        private void OnPictureMouseMove(object sender, MouseEventArgs e)
        {
            var isIn = IsInPickArea(e.X, e.Y);
            if (isIn && picture.Cursor != Cursors.Hand)
                picture.Cursor = Cursors.Hand;
            else
                if (!isIn && picture.Cursor == Cursors.Hand) picture.Cursor = Cursors.Default;
        }    
    
        public void SetColor(int i, Color cl)
        {
            if (i == 0) colorBackground = cl;
            else if (i == 1) colorOutlineGrowth = cl;
            else if (i == 2) colorOutlineSlope = cl;
            else if (i == 3) colorFillGrowth = cl;
            else if (i == 4) colorFillSlope = cl;
        }

        private Color GetColor(int i)
        {
            if (i == 0) return colorBackground;
            if (i == 1) return colorOutlineGrowth;
            if (i == 2) return colorOutlineSlope;
            if (i == 3) return colorFillGrowth;
            /*if (i == 4) */return colorFillSlope;
        }
    }
}

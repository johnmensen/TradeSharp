using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FastChart.Chart
{
    public partial class FastChart
    {
        [DisplayName("Показывать легенду")]
        [Category("Легенда")]
        [Browsable(true)]
        public bool ShowLegend { get; set; }

        public enum ChartLegendPlacement
        {
            Вверху = 0, Справа
        }

        private ChartLegendPlacement legendPlacement = ChartLegendPlacement.Вверху;
        [DisplayName("Место легенды")]
        [Category("Легенда")]
        [Browsable(true)]
        public ChartLegendPlacement LegendPlacement
        {
            get { return legendPlacement; }
            set { legendPlacement = value; }
        }

        public Brush legendBrush = new SolidBrush(Color.White);
        [DisplayName("Фон легенды")]
        [Category("Легенда")]
        [Browsable(true)]
        public Color LegendBackground
        {
            get { return legendBrush is SolidBrush ? ((SolidBrush) legendBrush).Color : Color.White;}
            set { if (legendBrush is SolidBrush) legendBrush = new SolidBrush(value); }
        }

        public Brush legendShadowBrush = new SolidBrush(Color.DimGray);
        [DisplayName("Тень легенды")]
        [Category("Легенда")]
        [Browsable(true)]
        public Color LegendShadow
        {
            get { return legendShadowBrush is SolidBrush ? ((SolidBrush)legendShadowBrush).Color : Color.White; }
            set { if (legendShadowBrush is SolidBrush) legendShadowBrush = new SolidBrush(value); }
        }

        [DisplayName("Рисовать тень")]
        [Category("Легенда")]
        [Browsable(true)]
        public bool DrawShadow { get; set; }

        public Pen penLegend = new Pen(Color.Black);
        [DisplayName("Рисовать рамку")]
        [Category("Легенда")]
        [Browsable(true)]
        public bool DrawLegendMargin { get; set; }

        [DisplayName("Цвет рамки")]
        [Category("Легенда")]
        [Browsable(true)]
        public Color LegendMarginColor
        {
            get { return penLegend.Color; }
            set { penLegend.Color = value; }
        }

        [DisplayName("Толщина рамки")]
        [Category("Легенда")]
        [Browsable(true)]
        public float LegendMarginWidth
        {
            get { return penLegend.Width; }
            set { penLegend.Width = value; }
        }

        private int legendPadding = 15;
        [DisplayName("Отступ легенды")]
        [Category("Легенда")]
        [Browsable(true)]
        public int LegendPadding
        {
            get { return legendPadding; }
            set { legendPadding = value; }
        }

        private int legendInnerPadding = 10;
        [DisplayName("Отступ внутри легенды")]
        [Category("Легенда")]
        [Browsable(true)]
        public int LegendInnerPadding
        {
            get { return legendInnerPadding; }
            set { legendInnerPadding = value; }
        }

        private int lengthMinWidth = 100;
        [DisplayName("Мин ширина легенды")]
        [Category("Легенда")]
        [Browsable(true)]
        public int LengthMinWidth
        {
            get { return lengthMinWidth; }
            set { lengthMinWidth = value; }
        }

        /// <returns>размер, отъедаемый легендой от графика</returns>
        public Size DrawLegend(PaintEventArgs e, FastChart owner)
        {
            if (!ShowLegend || owner.series.Count == 0) return new Size(0, 0);
            const int legRecHeight = 40;
            var legendHeight = owner.series.Count*legRecHeight;
            var legendHeightFull = LegendInnerPadding * 2 + legendHeight;
                        
            if (Width < (LengthMinWidth + 120 + LegendPadding * 2) 
                || Height < (legendHeightFull + 100 + LegendPadding * 2))
                return new Size(0, 0);

            var rect =
                LegendPlacement == ChartLegendPlacement.Вверху
                    ? new Rectangle(Math.Max(Width/5, LegendPadding), LegendPadding,
                                    Math.Min(Width*4/5, LegendPadding), legendHeightFull)
                    : new Rectangle(Width - lengthMinWidth - LegendPadding, LegendPadding,
                                    lengthMinWidth, legendHeightFull);

            // нарисовать тень
            if (DrawShadow)
            {
                rect.Offset(4, 4);
                e.Graphics.FillRectangle(legendShadowBrush, rect);
                rect.Offset(-4, -4);
            }
            // закрасить
            e.Graphics.FillRectangle(legendBrush, rect);
            // рамка
            if (DrawLegendMargin)
                e.Graphics.DrawRectangle(penLegend, rect);
            
            // текст
            var textY = rect.Top + LegendInnerPadding;
            var textX = rect.Left + LegendInnerPadding;
            using (var brushText = new SolidBrush(LegendMarginColor))
            {
                foreach (var sr in owner.series)
                {
                    var txtSz = e.Graphics.MeasureString(sr.Name, owner.Font);
                    // точка
                    using (var brushDot = new SolidBrush(sr.PenLine != null 
                        ? sr.PenLine.Color : Color.DarkGray))
                    {
                        e.Graphics.FillRectangle(brushDot, textX, textY + legRecHeight / 2 - 4,
                            4, 4);
                    }
                    // текст
                    e.Graphics.DrawString(sr.Name, owner.Font, brushText, textX + 7,
                        textY + legRecHeight / 2 - txtSz.Height / 2);

                    textY += legRecHeight;
                }
            }

            return new Size(lengthMinWidth + LegendPadding * 2,
                legendHeightFull + LegendPadding * 2);
        }
    }
}

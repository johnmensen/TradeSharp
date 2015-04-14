using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace FastChart.Chart
{
    public partial class FastChart
    {
        private readonly List<FastTheme> themes =
            new List<FastTheme>
                {
                    new FastTheme(FastThemeName.Строгая)
                        {
                            UseGradient = true,
                            ClGradStart = Color.Silver,
                            ClGradEnd = Color.Snow,
                            Padding = 10,
                            ChartFont = new Font(FontFamily.GenericSansSerif, 9),
                            DrawMargin = true,
                            ColorAxes = new [] { Color.FromArgb(0, 0, 10), Color.FromArgb(50, 50, 50), Color.FromArgb(110, 110, 110) },
                            ColorAxesGrids = new [] { Color.FromArgb(180, 180, 180), Color.FromArgb(210, 210, 210) }
                        },
                    new FastTheme(FastThemeName.Изысканная)
                        {
                            UseGradient = true,
                            ClGradStart = Color.FromArgb(255, 230, 230),
                            ClGradEnd = Color.FromArgb(230, 255, 255),
                            Padding = 14,
                            ChartFont = new Font("Calibri", 8),
                            GradAngle = 80,
                            DrawMargin = true,
                            PenMargin = new Pen(Color.DarkGray),
                            ColorAxes = new [] { Color.FromArgb(30, 30, 10), Color.FromArgb(60, 60, 30), Color.FromArgb(110, 110, 60) },
                            ColorAxesGrids = new [] { Color.FromArgb(220, 210, 190), Color.FromArgb(240, 240, 200) }
                        }
                };

        private FastThemeName currentTheme = FastThemeName.Нет;
        [Category("Визуальные")]
        [Description("Тема оформления")]
        [Browsable(true)]
        [DisplayName("Тема")]
        public FastThemeName CurrentTheme
        {
            get { return currentTheme; }
            set
            {
                currentTheme = value;
                if (currentTheme != FastThemeName.Нет)
                {
                    var theme = themes.Find(th => th.Name == currentTheme);
                    if (theme != null)
                        theme.ApplyToChart(this);
                }
            }
        }

    }
}

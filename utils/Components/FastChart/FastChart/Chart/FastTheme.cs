using System.Drawing;

namespace FastChart.Chart
{
    public enum FastThemeName
    {
        Нет = 0, Строгая, Изысканная
    }
    
    public class FastTheme
    {
        public FastThemeName Name { get; set; }

        public int Padding { get; set; }

        public bool DrawMargin { get; set; }

        public Pen PenMargin { get; set; }
        
        public Brush BrushArea { get; set; }

        public bool UseGradient { get; set; }

        public Color ClGradStart { get; set; }

        public Color ClGradEnd { get; set; }

        public float GradAngle { get; set; }

        public Font ChartFont { get; set; }

        public Color[] ColorAxes = new Color[0];

        public Color[] ColorAxesGrids = new Color[0];

        public void ApplyToChart(FastChart chart)
        {
            chart.Padding = Padding;
            chart.DrawMargin = DrawMargin;
            if (PenMargin != null) chart.penMargin = PenMargin;
            if (BrushArea != null) chart.brushArea = BrushArea;
            chart.UseGradient = UseGradient;
            chart.ClGradStart = ClGradStart;
            chart.ClGradEnd = ClGradEnd;
            chart.GradAngle = GradAngle;
            if (ChartFont != null) chart.Font = ChartFont;
            if (ColorAxes.Length > 0)
            {
                var clIndex = 0;
                foreach (var ax in chart.Axes.FindAll(a => a.Direction == FastAxisDirection.X))
                {
                    if (ax.LineColor.HasValue) continue;
                    ax.LineColor = ColorAxes[clIndex++];
                    if (clIndex >= ColorAxes.Length) clIndex = 0;
                }
                clIndex = 0;
                foreach (var ax in chart.Axes.FindAll(a => a.Direction == FastAxisDirection.Y))
                {
                    if (ax.LineColor.HasValue) continue;
                    ax.LineColor = ColorAxes[clIndex++];
                    if (clIndex >= ColorAxes.Length) clIndex = 0;
                }
            }
            if (ColorAxesGrids.Length > 0)
            {
                var clIndex = 0;
                foreach (var ax in chart.Axes.FindAll(a => a.Direction == FastAxisDirection.X))
                {
                    if (ax.ColorMainGrid.HasValue) continue;
                    ax.ColorMainGrid = ColorAxesGrids[clIndex++];
                    if (clIndex >= ColorAxesGrids.Length) clIndex = 0;
                }
                clIndex = 0;
                foreach (var ax in chart.Axes.FindAll(a => a.Direction == FastAxisDirection.Y))
                {
                    if (ax.ColorMainGrid.HasValue) continue;
                    ax.ColorMainGrid = ColorAxesGrids[clIndex++];
                    if (clIndex >= ColorAxesGrids.Length) clIndex = 0;
                }
            }
        }

        public FastTheme(FastThemeName name)
        {
            Name = name;
        }
    }
}

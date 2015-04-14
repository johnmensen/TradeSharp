using System.ComponentModel;
using System.Drawing;

namespace FastChart.Chart
{
    /*
     Визуальные настройки
     */
    public partial class FastChart
    {
        private int padding = 10;
        [Category("Визуальные")]
        [Description("Отступ от края области рисования, пикс.")]
        [Browsable(true)]
        [DisplayName("Отступ")]
        public new int Padding
        {
            get { return padding; }
            set { padding = value; }
        }

        private bool drawMargin = true;
        [Category("Визуальные")]
        [Description("Рисовать рамку для всей диаграммы")]
        [Browsable(true)]
        [DisplayName("Рамка")]
        public bool DrawMargin
        {
            get { return drawMargin; }
            set { drawMargin = value; }
        }

        public Pen penMargin = new Pen(Color.Black);
        [Category("Визуальные")]
        [Description("Цвет рамки диаграммы")]
        [Browsable(true)]
        [DisplayName("Цвет рамки")]
        public Color PenMarginColor
        {
            get { return penMargin.Color; }
            set { penMargin.Color = value; }
        }

        [Category("Визуальные")]
        [Description("Толщина рамки диаграммы")]
        [Browsable(true)]
        [DisplayName("Толщина рамки")]
        public float PenMarginWidth
        {
            get { return penMargin.Width; }
            set { penMargin.Width = value; }
        }

        public Brush brushArea = new SolidBrush(Color.White);

        [Category("Визуальные")]
        [Description("Градиентная заливка области диаграммы")]
        [Browsable(true)]
        [DisplayName("Градиентная заливка")]
        public bool UseGradient { get; set; }

        private Color clGradStart = Color.Silver, clGradEnd = Color.Snow;
        [Category("Визуальные")]
        [Description("Стартовый цвет градиента")]
        [Browsable(true)]
        [DisplayName("Градиент 1")]
        public Color ClGradStart
        {
            get { return clGradStart; }
            set { clGradStart = value; }
        }

        [Category("Визуальные")]
        [Description("Финальный цвет градиента")]
        [Browsable(true)]
        [DisplayName("Градиент 2")]
        public Color ClGradEnd
        {
            get { return clGradEnd; }
            set { clGradEnd = value; }
        }

        private float gradAngle = 90;
        [Category("Визуальные")]
        [Description("Направление вектора градиента")]
        [Browsable(true)]
        [DisplayName("Градиент - направление")]
        public float GradAngle
        {
            get { return gradAngle; }
            set { gradAngle = value; }
        }
    }
}

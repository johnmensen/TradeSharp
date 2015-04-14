using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace FastChart.Chart
{
    public enum FastAxisDirection
    {
        X, Y
    }

    [Serializable]
    public class FastAxis
    {
        #region Данные
        private bool autoScale100 = true;
        /// <summary>
        /// подобрать масштаб автоматически, чтобы уместились все данные
        /// </summary>
        [DisplayName("Авто масштаб")]
        [Category("Основные")]
        [Browsable(true)]
        public bool AutoScale100
        {
            get { return autoScale100; }
            set { autoScale100 = value; }
        }
        /// <summary>
        /// ось X или Y
        /// </summary>
        [DisplayName("Направление")]
        [Category("Основные")]
        [Browsable(true)]
        public FastAxisDirection Direction { get; set; }
        /// <summary>
        /// измерение оси - число или время
        /// </summary>
        [DisplayName("Временная ось")]
        [Category("Основные")]
        [Browsable(true)]
        public bool IsDateTime { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Cortege2<double, DateTime> MinValue { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Cortege2<double, DateTime> MaxValue { get; set; }

        [DisplayName("Мин. значение")]
        [Category("Основные")]
        [Browsable(true)]
        public string MinValueStr
        {
            get { return Cortege2String(MinValue); }
            set { MinValue = String2Cortege(value); }
        }

        [DisplayName("Макс. значение")]
        [Category("Основные")]
        [Browsable(true)]
        public string MaxValueStr
        {
            get { return Cortege2String(MaxValue); }
            set { MaxValue = String2Cortege(value); }
        }

        [DisplayName("Отображать '0'")]
        [Category("Основные")]
        [Browsable(true)]
        public bool AlwaysShowNil { get; set; }

        [DisplayName("Польз. формат меток")]
        [Category("Визуальные")]
        [Browsable(true)]
        public string LabelFormat { get; set; }

        [DisplayName("Полная запись времени")]
        [Category("Визуальные")]
        [Browsable(true)]
        public bool LongTimeString { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Cortege2<double, TimeSpan>? Step { get; set; }

        private int stepAccuracy = 50;
        /// <summary>
        /// точность шага, 1...100 (рекомендовано: 5, 10, 25, 50, 100)
        /// </summary>
        [DisplayName("Временная ось")]
        [Category("Основные")]
        [Browsable(true)]
        public int StepAccuracy { get { return stepAccuracy; } set { stepAccuracy = value; } }

        [DisplayName("Осн. сетка")]
        [Category("Визуальные")]
        [Browsable(true)]
        public bool DrawMainGrid { get; set; }
        
        [DisplayName("Доп. сетка")]
        [Category("Визуальные")]
        [Browsable(true)]
        public bool DrawSubGrid { get; set; }

        private int minPixelsPerPoint = 40;

        [DisplayName("Мин шаг сетки")]
        [Category("Визуальные")]
        [Browsable(true)]
        public int MinPixelsPerPoint
        {
            get { return minPixelsPerPoint; }
            set { minPixelsPerPoint = value; }
        }

        [DisplayName("Цвет линий")]
        [Category("Визуальные")]
        [Browsable(true)]
        public Color? LineColor
        {
            get; set;
        }

        [DisplayName("Цвет осн. сетки")]
        [Category("Визуальные")]
        [Browsable(true)]
        public Color? ColorMainGrid { get; set; }

        [DisplayName("Цвет доп. сетки")]
        [Category("Визуальные")]
        [Browsable(true)]
        public Color? ColorSubGrid { get; set; }

        [DisplayName("Вертикальный текст")]
        [Category("Визуальные")]
        [Browsable(true)]
        public bool VerticalText { get; set; }        
        #endregion

        public FastAxis(FastAxisDirection _direction, bool isDateTime)
        {            
            Direction = _direction;
            IsDateTime = isDateTime;
        }

        public FastAxis()
        {
            Direction = FastAxisDirection.X;
            IsDateTime = false;
        }

        public void Draw(FastChart owner, PaintEventArgs e, Point o, Point tip, int width)
        {
            var lineCl = LineColor.HasValue ? LineColor.Value : Color.Black;
            
            var start = !AlwaysShowNil ? MinValue : new Cortege2<double, DateTime>(0, DateTime.MinValue);
            var step = CalculateStep(start, o, tip);
            if (step.a == 0 && step.b == TimeSpan.Zero)
                // нет делений
                return;
            
            var lblFmt = DeriveFormatFromStep(step);

            var cur = start;
            // сместить текущую отметку на ближ. целое значение шага
            if (!IsDateTime)
            {
                cur.a = ((int)(cur.a / step.a)) * step.a;
            }

            var drawFormat = new StringFormat(VerticalText ?
                StringFormatFlags.DirectionVertical : StringFormatFlags.NoWrap);
            var lenPx = Direction == FastAxisDirection.X 
                ? Math.Abs(tip.X - o.X) : Math.Abs(tip.Y - o.Y);
            
            using (var brush = new SolidBrush(lineCl))
            {
                using (var pen = new Pen(lineCl))
                {
                    e.Graphics.DrawLine(pen, o.X, o.Y, tip.X, tip.Y);
                    using (var penGridMain = new Pen(ColorMainGrid.HasValue ? ColorMainGrid.Value : Color.DarkGray))
                    {
                        while (true)
                        {
                            // след. значение
                            if (!IsDateTime)
                            {
                                if (step.a == 0) break;
                                cur.a += step.a;
                            }
                            else
                            {
                                if (step.b == TimeSpan.Zero) break;
                                cur.b += step.b;
                            }
                            // вышли за пределы шкалы?
                            if (!IsDateTime) if (cur.a > MaxValue.a) break;
                            if (IsDateTime) if (cur.b > MaxValue.b) break;                            

                            // текст подписи
                            var labelTxt = !IsDateTime
                                               ? cur.a.ToString(lblFmt)
                                               : cur.b.ToString(lblFmt);
                            // размеры, занимаемые подписью
                            var txtSzOrig = e.Graphics.MeasureString(labelTxt, owner.Font);
                            var txtSz = VerticalText
                                            ? new SizeF(txtSzOrig.Height, txtSzOrig.Width)
                                            : txtSzOrig;
                            // координата
                            var ptDist = !IsDateTime
                                             ? (cur.a - start.a)/(MaxValue.a - start.a)
                                             : (cur.b - start.b).TotalMilliseconds/
                                               (MaxValue.b - start.b).TotalMilliseconds;
                            ptDist *= lenPx;
                            var ptDot = Direction == FastAxisDirection.X
                                            ? new Point(o.X + (int) ptDist, o.Y)
                                            : new Point(o.X, o.Y - (int) ptDist);
                            var ptLeftUp = Direction == FastAxisDirection.X
                                               ? new PointF(ptDot.X - txtSz.Width/2, ptDot.Y + 3)
                                               : new PointF(ptDot.X - txtSz.Width - 3, ptDot.Y - txtSz.Height/2);
                            // нарисовать текст
                            e.Graphics.DrawString(labelTxt, owner.Font, brush,
                                                  ptLeftUp.X, ptLeftUp.Y, drawFormat);                            
                            // нарисовать засечку
                            try
                            {
                                if (Direction == FastAxisDirection.X)
                                    e.Graphics.DrawLine(pen, ptDot.X, ptDot.Y - 2, ptDot.X, ptDot.Y + 2);
                                else
                                    e.Graphics.DrawLine(pen, ptDot.X - 2, ptDot.Y, ptDot.X + 2, ptDot.Y);
                            }
                            catch (Exception ex)
                            {
                                owner.lastError = ex.ToString();
                                return;
                            }

                            // нарисовать сетку
                            if (DrawMainGrid)
                            {
                                if (Direction == FastAxisDirection.X)
                                    e.Graphics.DrawLine(penGridMain, ptDot.X, ptDot.Y + 2, ptDot.X, ptDot.Y - width);
                                else
                                    e.Graphics.DrawLine(penGridMain, ptDot.X + 2, ptDot.Y, ptDot.X + width, ptDot.Y);                                
                            }
                        }// while

                        // нарисовать рамку
                        if (DrawMainGrid)
                        {
                            if (Direction == FastAxisDirection.X)
                                e.Graphics.DrawLine(penGridMain, o.X, o.Y - width, tip.X, o.Y - width);
                            else
                                e.Graphics.DrawLine(penGridMain, o.X + width, o.Y, o.X + width, tip.Y);
                        }

                    }// main grid pen
                }// brush
            }
        }

        #region Служебные методы
        private string Cortege2String(Cortege2<double, DateTime> c)
        {
            return IsDateTime
                       ? c.b.ToString("dd.MM.yyyy HH:mm:ss")
                       : c.a.ToString("f5");
        }
        private Cortege2<double, DateTime> String2Cortege(string s)
        {
            return new Cortege2<double, DateTime>
                (IsDateTime ? 0 : double.Parse(s, CultureInfo.InvariantCulture),
                 IsDateTime
                     ? DateTime.ParseExact(s, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture)
                     : DateTime.MinValue);
        }

        
        private Cortege2<double, TimeSpan> CalculateStep(Cortege2<double, DateTime> start, Point o, Point tip)
        {
            if (Step.HasValue) return Step.Value;
            try
            {
                var len = Direction == FastAxisDirection.X ? Math.Abs(tip.X - o.X) : Math.Abs(tip.Y - o.Y);
                var numMinSteps = len/minPixelsPerPoint;
                if (numMinSteps < 1) return new Cortege2<double, TimeSpan>(0, TimeSpan.Zero);

                if (!IsDateTime)
                {
                    var fractStep = (MaxValue.a - start.a)/numMinSteps;
                    return new Cortege2<double, TimeSpan>(NearestCeilingStep(fractStep),
                                                          TimeSpan.MinValue);
                }

                var fractSpan = (MaxValue.b - start.b).TotalMilliseconds/numMinSteps;
                var span = NearestCeilingTimeSpan(fractSpan);
                return new Cortege2<double, TimeSpan>(0, span);
            }
            catch
            {                
                return new Cortege2<double, TimeSpan>(0, TimeSpan.MaxValue);
            }
        }

        private string DeriveFormatFromStep(Cortege2<double, TimeSpan> step)
        {
            if (!string.IsNullOrEmpty(LabelFormat)) return LabelFormat;
            if (!IsDateTime)
            {
                var abs = Math.Abs(step.a);
                if (abs > 100) return  "f0";
                if (abs > 15) return "f1";
                if (abs > 1) return "f2";
                if (abs > 0.1) return "f3";
                if (abs > 0.01) return "f4";
                return "g";                
            }
            if (step.b > new TimeSpan(30, 0, 0, 0))
                return LongTimeString ? "dd.MM.yyyy" : "dd.MM";                
            
            if (step.b > new TimeSpan(1, 0, 0, 0))
                return LongTimeString ? "dd.MM HH:mm" : "dd.MM";
                            
            if (step.b > new TimeSpan(0, 8, 0, 0))
                return LongTimeString ? "dd.MM HH:mm" : "dd HH:mm";
                
            if (step.b > new TimeSpan(0, 1, 0, 0))            
                return LongTimeString ? "dd HH:mm" : "HH:mm";                
            
            if (step.b > new TimeSpan(0, 0, 0, 5))
                return LongTimeString ? "HH:mm:ss" : "mm:ss";
                
            return LongTimeString ? "mm:ss:ffff" : "ss:ffff";
        }

        private double NearestCeilingStep(double d)
        {
            if (d == 0) return 0;
            var signD = Math.Sign(d);
            d = Math.Abs(d);

            // d = 0.017
            var k = Math.Log10(d);
            // k будет содержать порядок числа
            k = Math.Ceiling(k);
            var k10 = Math.Pow(10, k);
            // k = -1
            var mantis = d / k10;
            // mantis = 0.17
            var m100 = mantis*100;
            // m100 = 17
            double step = StepAccuracy * Math.Ceiling(m100 / StepAccuracy);
            // step = 20
            var rst = step*signD*k10/100;
            if (rst == double.NaN) 
                throw 
                    new Exception("NaN");
            return rst;
        }
    
        private TimeSpan NearestCeilingTimeSpan(double totalMils)
        {
            if (totalMils <= 750)                            
                return new TimeSpan(0, 0, 0, 0, (int) NearestCeilingStep(totalMils));            

            if (totalMils <= 1000)
                return new TimeSpan(0, 0, 0, 1); // 1 sec

            if (totalMils <= 60000)
                return new TimeSpan(0, 0, 0, (int)NearestCeilingStep(totalMils / 1000));

            var totalMins = totalMils/60000;
            if (totalMins <= 45)
                return new TimeSpan(0, (int)Math.Ceiling(NearestCeilingStep(totalMins)), 0);

            if (totalMins <= 60)
                return new TimeSpan(1, 0, 0);

            var totalHours = totalMins/60;
            if (totalHours <= 18)
                return new TimeSpan((int)Math.Ceiling(NearestCeilingStep(totalHours)), 0, 0); // n hours
            if (totalHours <= 24)
                return new TimeSpan(1, 0, 0, 0); // 1 day

            var totalDays = totalHours/24;
            if (totalDays <= 5)
                return new TimeSpan((int)Math.Ceiling(NearestCeilingStep(totalDays)), 
                    0, 0, 0); // n days
            if (totalDays <= 7)
                return new TimeSpan(7, 0, 0, 0); // n days
            return new TimeSpan((int)Math.Ceiling(NearestCeilingStep(totalDays)), 0, 0, 0); // n days
        }
        #endregion
    }
}

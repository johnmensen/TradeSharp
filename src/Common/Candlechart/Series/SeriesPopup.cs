using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Candlechart.ChartGraphics;
using Candlechart.Core;

namespace Candlechart.Series
{
    public class SeriesPopup : Series
    {
        public List<ChartPopup> data = new List<ChartPopup>();
        public override int DataCount { get { return data.Count; } }

        public ChartPopup this[string title]
        {
            get
            {
                return data.Find(p => p.Title == title);
            }
        }

        public SeriesPopup(string name)
            : base(name)
        {
        }
        public override bool GetXExtent(ref double left, ref double right)
        {
            return false;
        }
        public override bool GetYExtent(double left, double right, ref double top, ref double bottom)
        {
            return false;
        }
        public override void Draw(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {            
            base.Draw(g, worldRect, canvasRect);            
            foreach (var popup in data)
            {
                if (!popup.Visible) continue;
                // - 1 - вычислить размер
                if ((popup.Width == 0 && popup.Height == 0) || popup.AutoSize)
                {
                    var szTitle = g.MeasureString(popup.Title, Chart.Font);
                    var szText = g.MeasureString(popup.Text, Chart.Font);
                    var maxWidth = szTitle.Width + ChartPopup.captionWidth;
                    if (szText.Width > maxWidth) 
                        maxWidth = szText.Width;
                    popup.Width = (int)maxWidth + ChartPopup.padding * 2;
                    popup.Height = (int)szText.Height + ChartPopup.captionWidth + ChartPopup.padding * 2;
                }
                // - 2 - нарисовать обводку
                using (var pen = new Pen(popup.FontColor))
                {
                    if (popup.Selected)
                    {
                        pen.DashStyle = DashStyle.Dot;
                        // пометить левый верхний угол
                        g.DrawLine(pen, popup.Left, popup.Top - 3, popup.Left, popup.Top - 8);
                        g.DrawLine(pen, popup.Left - 3, popup.Top, popup.Left - 8, popup.Top);
                    }
                    DrawPopupArea(g, popup);
                    g.DrawRectangle(pen, popup.Left, popup.Top + ChartPopup.captionWidth, popup.Width, popup.Height);
                    g.FillRectangle(popup.BrushTop, popup.Left, popup.Top, popup.Width, ChartPopup.captionWidth);
                    g.DrawRectangle(pen, popup.Left, popup.Top, popup.Width, ChartPopup.captionWidth);
                    // нарисовать крестик
                    g.DrawLine(pen, popup.Left + popup.Width - 4, popup.Top + 4,
                        popup.Left + popup.Width - ChartPopup.captionWidth + 4, popup.Top + ChartPopup.captionWidth - 4);
                    g.DrawLine(pen, popup.Left + popup.Width + 4 - ChartPopup.captionWidth, popup.Top + 4,
                        popup.Left + popup.Width - 4, popup.Top + ChartPopup.captionWidth - 4);
                }
                // - 3 - вывести текст
                using (var brushText = new SolidBrush(popup.FontColor))
                {
                    g.DrawString(popup.Title, Chart.Font, brushText, 
                        popup.Left + ChartPopup.padding, popup.Top + ChartPopup.padding);
                    g.DrawString(popup.Text, Chart.Font, brushText, popup.Left + ChartPopup.padding, 
                        popup.Top + ChartPopup.padding + ChartPopup.captionWidth);
                }
            }
        }

        /// <summary>
        /// нарисовать подложку - просто закрасить кистью или заблурить (BlurFactor > 0)
        /// </summary>        
        private void DrawPopupArea(Graphics g, ChartPopup popup)
        {
            if (popup.BlurFactor == 0)
            {
                g.FillRectangle(popup.BrushArea, popup.Left, popup.Top + ChartPopup.captionWidth, popup.Width, popup.Height);
            }
            else
            {
                using (var img = BitmapProcessor.CaptureControl(Chart, g))                
                using (var blured = BitmapProcessor.GetBluredImage(img, 
                    new Rectangle(popup.Left, popup.Top + ChartPopup.captionWidth, popup.Width, popup.Height), popup.BlurFactor))
                    g.DrawImage(blured, popup.Left, popup.Top + ChartPopup.captionWidth);                
            }
        }        
        
        private void TranslateCoords(ref int x, ref int y)
        {
            var clientPoint = Chart.PointToScreen(new Point(x, y));
            clientPoint = Owner.PointToClient(clientPoint);
            x = clientPoint.X - Owner.CanvasRect.Left;
            y = clientPoint.Y;
        }

        public bool ProcessMouseDown(int x, int y)
        {            
            TranslateCoords(ref x, ref y);            
            foreach (var popup in data)
            {
                if (popup.Selected)
                {
                    popup.Left = x;
                    popup.Top = y;
                    popup.Selected = false;
                    return true;
                }
            }
            for (var i = data.Count - 1; i >= 0; i--)                
            {
                var popup = data[i];
                if (!popup.Visible) continue;
                if (popup.InCloseButtonArea(x, y))
                {
                    popup.Visible = false;
                    return true;
                }

                if (popup.InTopPaneArea(x, y))
                {
                    popup.Selected = true;                    
                    return true;
                }                
            }
            return false;
        }        
    }

    /// <summary>
    /// "окно" графика - может быть скрыто или показано,
    /// перетаскивается пользователем
    /// </summary>
    public class ChartPopup
    {
        public const int captionWidth = 22, padding = 4;
        public bool Selected { get; set; }
        public float BlurFactor { get; set; }
        public bool Visible { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Text { get; set; }
        public string Title { get; set; }
        private bool autoSize = true;
        public bool AutoSize
        {
            get
            {
                return autoSize;
            }
            set
            {
                autoSize = value;
            }
        }
       
        public ChartPopup()
        {            
        }

        private Brush brushArea = new SolidBrush(Color.FromArgb(100, 220, 215, 200));
        public Brush BrushArea
        {
            get { return brushArea; }
            set { brushArea = value; }
        }

        private Brush brushTop = new SolidBrush(Color.FromArgb(225, 220, 215, 200));
        public Brush BrushTop
        {
            get { return brushTop; }
            set { brushTop = value; }
        }

        private Color fontColor = Color.Black;
        public Color FontColor
        {
            get { return fontColor; }
            set { fontColor = value; }
        }

        public bool InCloseButtonArea(int x, int y)
        {
            if (y < Top || y > (Top + captionWidth)) return false;
            if (x > (Left + Width) || x < (Left + Width - captionWidth))
                return false;
            return true;
        }

        public bool InTopPaneArea(int x, int y)
        {
            if (y < Top || y > (Top + captionWidth)) return false;
            if (x > (Left + Width) || x < Left) return false;
            return true;
        }
    }
}

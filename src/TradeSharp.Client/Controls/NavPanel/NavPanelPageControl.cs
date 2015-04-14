using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Entity;

namespace TradeSharp.Client.Controls.NavPanel
{
    /// <summary>
    /// закладка для закрепляемого слева окошка
    /// </summary>
    class NavPanelPageControl : Control
    {
        public class Pictogram
        {
            #region Общие визуальные настройки

            private static readonly Color colorNormal = SystemColors.ActiveCaptionText;

            private static readonly Color colorHovered = SystemColors.ButtonShadow;

            private static readonly Color colorPressed = SystemColors.ActiveCaptionText;

            #endregion

            #region Статическая коллекция фигур
            public static Point[] figureExpand = new[] { new Point(2, 3), new Point(8, 3), new Point(5, 6) };
            public static Point[] figureCollapse = new[] { new Point(2, 6), new Point(8, 6), new Point(5, 3) };
            public static Point[] figureCross = new[]
                {
                    new Point(2, 0), new Point(5, 3),
                    new Point(8, 0), new Point(10, 2),
                    new Point(7, 5), new Point(10, 8),
                    new Point(8, 10), new Point(5, 7),
                    new Point(2, 10), new Point(0, 8), 
                    new Point(3, 5), new Point(0, 2)
                };
            #endregion

            public Rectangle area;

            public Point[] figurePoints;

            public Action onClick;

            public enum DrawState { Normal = 0, Hovered, Pressed }

            public DrawState state;

            public Pictogram(Rectangle area, Point[] figurePoints)
            {
                this.area = area;
                this.figurePoints = figurePoints;
            }

            public bool IsIn(int x, int y)
            {
                return area.Contains(x, y);
            }

            public void Draw(Graphics g, PenStorage pens)
            {
                var oldMode = g.SmoothingMode;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                var pen =
                    pens.GetPen(state == DrawState.Normal
                            ? colorNormal
                            : state == DrawState.Hovered ? colorHovered : colorPressed, 0.7f);
                
                g.DrawPolygon(pen, figurePoints.Select(p => new Point(p.X + area.Left, p.Y + area.Top)).ToArray());

                g.SmoothingMode = oldMode;
            }
        }

        public string Code { get; set; }

        public string Title { get; set; }

        public bool IsExpanded { get; set; }

        public Pictogram pictExpand, pictClose;

        private readonly Pictogram[] pictograms = new Pictogram[2];

        private Font fontTitle;

        private int heightInExpandedMode = 250;

        #region Статическая коллекция закладок

        public const string PageCodeAccount = "Account";
        public const string PageCodeDeals = "Deals";
        public const string PageCodeIndicators = "Indicators";
        public const string PageCodeQuotes = "Quotes";
        public const string PageCodeScript = "script";

        public static readonly string[] pageCodes = new []
            {
                PageCodeAccount, PageCodeDeals, PageCodeIndicators, PageCodeQuotes, PageCodeScript
            };

        public static readonly Dictionary<string, string> pageTitles = new Dictionary<string, string>
            {
                { PageCodeAccount, "Счет" }, { PageCodeDeals, "Сделки" }, { PageCodeIndicators, "Индикаторы" },
                { PageCodeQuotes , "Котировки" }, { PageCodeScript, "Скрипты" }
            };
        #endregion

        #region Статические свойства и константы, отвечающие за отображение контрола
        private const int HeaderHeight = 20;
        private const int PaddingPictogram = 3;
        private const int PaddingText = 4;
        #endregion

        public NavPanelPageControl(string code, bool isExpanded, Action<NavPanelPageControl> closeClicked)
        {
            Code = code;
            IsExpanded = isExpanded;
            Height = isExpanded ? 100 : HeaderHeight;
            Title = pageTitles[code];
            fontTitle = new Font(Font, FontStyle.Bold);

            pictExpand = new Pictogram(new Rectangle(0, 0, 16, 16),
                                       isExpanded ? Pictogram.figureCollapse : Pictogram.figureExpand)
                {
                    onClick = OnExpand
                };
            pictClose = new Pictogram(new Rectangle(0, 0, 16, 16), Pictogram.figureCross)
                {
                    onClick = () => closeClicked(this)
                };
            pictograms[0] = pictExpand;
            pictograms[1] = pictClose;

            // добавить в контрол содержимое
            Control content =
                code == PageCodeAccount
                    ? new NavPageAccountControl()
                    : code == PageCodeIndicators
                          ? new NavPageIndicatorsControl()
                          : code == PageCodeDeals
                                ? new NavPageDealsControl()
                                : code == PageCodeQuotes ? new NavPageQuoteControl()
                                    : (Control) new NavPaneScriptControl();
            content.Dock = DockStyle.Bottom;
            content.Parent = this;
            Controls.Add(content);
            
            // контрол (содержимое) запросил изменить высоту
            if (content is INavPageContent)
            {
                ((INavPageContent) content).ContentHeightChanged += ht =>
                    {
                        var newHeight = ht + HeaderHeight;
                        heightInExpandedMode = newHeight;
                        if (IsExpanded) Height = newHeight;
                    };
            }

            ArrangePictograms();
        }

        /// <summary>
        /// нарисовать заголовочек и контрол - содержимое
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // заголовочек
            var rectHr = new Rectangle(0, 0, Width, HeaderHeight);
            e.Graphics.FillRectangle(SystemBrushes.ButtonFace, rectHr);
            e.Graphics.DrawLine(SystemPens.ButtonShadow, 0, HeaderHeight - 1, Width - 1, HeaderHeight - 1);
            e.Graphics.DrawLine(SystemPens.ButtonShadow, Width - 1, HeaderHeight - 1, Width - 1, 0);
            e.Graphics.DrawLine(SystemPens.ButtonHighlight, 0, 0, Width - 1, 0);
            e.Graphics.DrawLine(SystemPens.ButtonHighlight, 0, 0, 0, HeaderHeight - 1);

            // текст заголовка
            var textWidth = pictograms[0].area.Left - PaddingPictogram - PaddingText;
            if (textWidth > 0)
            {
                e.Graphics.DrawString(Title, fontTitle, SystemBrushes.ControlText, PaddingText, HeaderHeight / 2, 
                    new StringFormat { LineAlignment = StringAlignment.Center });
            }
            
            // пиктограммы
            using (var pens = new PenStorage())
            foreach (var pict in pictograms)
            {
                pict.Draw(e.Graphics, pens);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            // масштабировать контрол
            if (Controls.Count > 0)
                Controls[0].Height = Height - HeaderHeight;
            // перенести картинки
            ArrangePictograms();
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            
            // подсветка пиктограмм
            foreach (var pict in pictograms)
            {
                var isIn = pict.IsIn(e.X, e.Y);
                if ((!isIn && pict.state == Pictogram.DrawState.Normal) ||
                    (isIn && pict.state != Pictogram.DrawState.Normal)) continue;
                
                pict.state = isIn ? Pictogram.DrawState.Hovered : Pictogram.DrawState.Normal;
                Invalidate(pict.area);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            
            // подсветить кнопку
            foreach (var pict in pictograms)
            {
                var isIn = pict.IsIn(e.X, e.Y);
                if ((!isIn && pict.state != Pictogram.DrawState.Pressed) ||
                    (isIn && pict.state == Pictogram.DrawState.Pressed)) continue;

                pict.state = isIn ? Pictogram.DrawState.Pressed : Pictogram.DrawState.Normal;
                Invalidate(pict.area);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            foreach (var pict in pictograms)
            {
                if (pict.state != Pictogram.DrawState.Pressed) continue;
                pict.state = pict.area.Contains(e.X, e.Y) ? Pictogram.DrawState.Hovered : Pictogram.DrawState.Normal;
                Invalidate(pict.area);
                // вызвать событие
                pict.onClick();
            }
        }

        private void OnExpand()
        {
            IsExpanded = !IsExpanded;
            pictExpand.figurePoints = IsExpanded ? Pictogram.figureCollapse : Pictogram.figureExpand;
            // развернуть - свернуть
            Height = IsExpanded ? heightInExpandedMode : HeaderHeight;
            Invalidate();
        }

        private void ArrangePictograms()
        {
            if (pictograms[0] == null) return;
            var picTop = (HeaderHeight - pictExpand.area.Height)/2;
            pictExpand.area.X = Width - PaddingPictogram*2 - pictClose.area.Width - pictExpand.area.Width;
            pictExpand.area.Y = picTop;
            pictClose.area.X = Width - PaddingPictogram - pictClose.area.Width;
            pictClose.area.Y = picTop;
        }
    }
}

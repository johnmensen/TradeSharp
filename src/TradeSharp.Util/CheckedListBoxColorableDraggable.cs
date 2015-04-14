using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TradeSharp.Util
{
    public class CheckedListBoxColorableDraggable : ListBox
    {
        private int markSize = 9;
        public int MarkSize
        {
            get { return markSize; }
            set { markSize = value; }
        }

        private int markPadding = 2;
        public int MarkPadding
        {
            get { return markPadding; }
            set { markPadding = value; }
        }

        [DisplayName("Цвета строк")]
        [Description("Список цветов строк по индексам, мб. null")]
        [Category("UserDrawn")]
        public IList<Color> RowColors { get; set; }

        [DisplayName("Перетаскивание элементов")]
        [Description("Перетаскивание элементов разрешено")]
        [Category("UserDrawn")]
        public bool DraggingEnabled { get; set; }

        private List<bool> checkFlags = new List<bool>();

        private int hoveredCheckboxIndex = -1;

        private int mouseDownItemIndex = -1;

        public bool this[int index]
        {
            get
            {
                EnsureCheckFlagsListCount();
                if (index < 0 || index >= checkFlags.Count) return false;
                return checkFlags[index];
            }
            set
            {
                EnsureCheckFlagsListCount();
                if (index < 0 || index >= checkFlags.Count) return;
                checkFlags[index] = value;
                Invalidate();
            }
        }

        private Rectangle? checkRectangle, prevCheckedRect;

        public CheckedListBoxColorableDraggable()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
        }

        public List<object> GetCheckedItems()
        {
            if (checkFlags.Count == 0)
                return new List<object>();
            var objects = new List<object>(Items.Count);
            for (var i = 0; i < Items.Count; i++)
            {
                if (checkFlags[i])
                    objects.Add(Items[i]);
            }
            return objects;
        }

        protected override void OnMeasureItem(MeasureItemEventArgs e)
        {
            base.OnMeasureItem(e);
            EnsureCheckFlagsListCount();
            e.ItemWidth += (MarkPadding * 2 + MarkSize);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (Items.Count == 0) return;

            // фон
            e.DrawBackground();
            var g = e.Graphics;
            g.FillRectangle(SystemBrushes.Window, e.Bounds);

            // текст
            var brush = SystemBrushes.WindowText;
            Brush userBrush = null;
            if (RowColors != null && RowColors.Count > e.Index)
            {
                userBrush = new SolidBrush(RowColors[e.Index]);
                brush = userBrush;
            }

            g.DrawString(
                Items[e.Index].ToString(),
                e.Font,
                brush,
                new PointF(e.Bounds.X + markSize + markPadding * 2, e.Bounds.Y));

            // отмечен / не отмечен / наведена мышь
            var isChecked = false;
            if (checkFlags.Count > e.Index)
                isChecked = checkFlags[e.Index];

            checkRectangle = new Rectangle(e.Bounds.X + markPadding, e.Bounds.Y + markPadding,
                                          markSize, e.Bounds.Height - markPadding * 2);
            g.DrawRectangle(SystemPens.WindowText, checkRectangle.Value);
            // наведена мышь...
            if (e.Index == hoveredCheckboxIndex)
                g.FillRectangle(SystemBrushes.GrayText, checkRectangle.Value);
            // отмечен
            if (isChecked)
            {
                var rectMark = checkRectangle.Value;
                rectMark.Inflate(-2, -2);
                g.FillRectangle(SystemBrushes.WindowText, rectMark);
            }

            if (userBrush != null)
                userBrush.Dispose();

            // выделение
            e.DrawFocusRectangle();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (mouseDownItemIndex < 0) return;

            var hovIndex = IndexFromPoint(e.X, e.Y);
            if (hovIndex == hoveredCheckboxIndex) return;
            hoveredCheckboxIndex = hovIndex;

            if (prevCheckedRect.HasValue && prevCheckedRect.Value != checkRectangle.Value)
                Invalidate(prevCheckedRect.Value);
            if (checkRectangle.HasValue)
                Invalidate(checkRectangle.Value);
            prevCheckedRect = checkRectangle;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left && DraggingEnabled)
                mouseDownItemIndex = IndexFromPoint(e.X, e.Y);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button != MouseButtons.Left)
                return;

            try
            {
                if (hoveredCheckboxIndex >= 0 && hoveredCheckboxIndex != mouseDownItemIndex
                    && mouseDownItemIndex >= 0 && DraggingEnabled)
                {
                    // не нажимать, но перетащить
                    var itemA = Items[mouseDownItemIndex];
                    var checkState = this[mouseDownItemIndex];

                    Items.RemoveAt(mouseDownItemIndex);
                    checkFlags.RemoveAt(mouseDownItemIndex);

                    var targetIndex = mouseDownItemIndex < hoveredCheckboxIndex
                        ? hoveredCheckboxIndex - 1 : hoveredCheckboxIndex;
                    Items.Insert(targetIndex, itemA);
                    checkFlags.Insert(targetIndex, checkState);
                    mouseDownItemIndex = -1;
                    Invalidate();
                    return;
                }

                var index = IndexFromPoint(e.X, e.Y);
                if (index >= 0)
                {
                    // нажать (отметить)
                    EnsureCheckFlagsListCount();
                    checkFlags[index] = !checkFlags[index];
                    //mouseDownItemIndex = -1;
                    Invalidate();
                }                
            }
            finally
            {
                //mouseDownItemIndex = -1;
            }
        }

        protected override void OnMouseLeave(System.EventArgs e)
        {
            base.OnMouseLeave(e);
            if (hoveredCheckboxIndex >= 0)
            {
                hoveredCheckboxIndex = -1;
                Invalidate();
            }
        }

        private void EnsureCheckFlagsListCount()
        {
            if (checkFlags.Count != Items.Count)
            {
                if (checkFlags.Count < Items.Count)
                {
                    while (checkFlags.Count < Items.Count)
                        checkFlags.Add(false);
                    return;
                }

                checkFlags = new List<bool>(Items.Count);
            }
        }
    }
}

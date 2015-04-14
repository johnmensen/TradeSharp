using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Entity;

namespace TradeSharp.UI.Util.Control
{
    public partial class TickerComboBox : ComboBox
    {
        private readonly List<string> favTickers = new List<string>();
        private readonly List<string> otherTickers = new List<string>();

        public TickerComboBox()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            favTickers.AddRange(DalSpot.Instance.GetTickerNames(true));
            otherTickers.AddRange(DalSpot.Instance.GetTickerNames().Where(t => !favTickers.Contains(t)));
            var items = new List<string>();
            items.AddRange(favTickers);
            items.AddRange(otherTickers);
            DataSource = items;
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            MeasureItem += OnMeasureItem;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= Items.Count)
                return;
            e.DrawBackground();
            e.DrawFocusRectangle();
            var item = Items[e.Index];
            if (item == null)
                return;
            var font = e.Font;
            if (favTickers.Contains(item.ToString()))
                font = new Font(font, FontStyle.Bold);
            e.Graphics.DrawString(item.ToString(), font, new SolidBrush(e.ForeColor), e.Bounds.Left, e.Bounds.Top);
            base.OnDrawItem(e);
        }

        private void OnMeasureItem(object sender, MeasureItemEventArgs e)
        {
            var maxSize = new Size();
            var boldFont = new Font(Font, FontStyle.Bold);
            using (var graphics = CreateGraphics())
                foreach (var item in Items)
                {
                    var text = item.ToString();
                    var size = graphics.MeasureString(text, favTickers.Contains(item.ToString()) ? boldFont : Font).ToSize();
                    if (size.Width > maxSize.Width)
                        maxSize.Width = size.Width;
                    if (size.Height > maxSize.Height)
                        maxSize.Height = size.Height;
                }
            e.ItemWidth = maxSize.Width;
            e.ItemHeight = maxSize.Height;
            DropDownWidth = maxSize.Width;
        }
    }
}

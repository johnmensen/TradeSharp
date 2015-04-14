using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Candlechart.ChartMath;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Controls
{
    public partial class TrendLineTypeDialog : Form
    {
        public TrendLineTypeDialog()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
        }

        private TrendLine.TrendLineStyle selectedStyle = TrendLine.TrendLineStyle.Линия;

        public TrendLine.TrendLineStyle SelectedStyle
        {
            get { return selectedStyle; }
            set
            {
                selectedStyle = value;
                if (listView == null || listView.Items.Count == 0) return;
                var selItems = listView.Items.Find(value.ToString(), true);
                selItems[0].Selected = true;
            }
        }

        private void TrendLineTypeDialogLoad(object sender, EventArgs e)
        {
            // заполнить список линий
            var w = imageListLarge.ImageSize.Width;
            var h = imageListLarge.ImageSize.Height;
            var rectWorld = new RectangleD(0, 0, w, h);
            var rectCanvas = new Rectangle(0, 0, w, h);

            var colors = new [] {Color.BlanchedAlmond, Color.GreenYellow, Color.PaleTurquoise};
            var colorIndex = 0;

            foreach (TrendLine.TrendLineStyle lineType in Enum.GetValues(typeof(TrendLine.TrendLineStyle)))
            {
                var bmp = new Bitmap(w, h);
                var line = new TrendLine();
                line.AddPoint(h * 0.23, w - 5);
                line.AddPoint(h * 0.77, 5);
                if (lineType == TrendLine.TrendLineStyle.Отрезок 
                    || lineType == TrendLine.TrendLineStyle.Окружность || lineType == TrendLine.TrendLineStyle.ЛинияСМаркерами
                    || lineType == TrendLine.TrendLineStyle.ОтрезокСМаркерами)
                {
                    line.linePoints[1] = new PointD(h * 0.32, w - 16);
                    line.linePoints[0] = new PointD(h * 0.68, 16);
                }

                line.LineColor = Color.Black;
                line.ShapeFillColor = colors[colorIndex];
                line.LineStyle = lineType;
                colorIndex++;
                if (colorIndex == colors.Length)
                    colorIndex = 0;
                
                using (var gr = Graphics.FromImage(bmp))
                using (var brushes = new BrushesStorage())
                using (var pens = new PenStorage())
                {
                    gr.SmoothingMode = SmoothingMode.AntiAlias;
                    line.Draw(gr, rectWorld, rectCanvas, pens, brushes);
                }

                imageListLarge.Images.Add(bmp);
                var item = listView.Items.Add(lineType.ToString(),
                                              EnumFriendlyName<TrendLine.TrendLineStyle>.GetString(lineType),
                                              imageListLarge.Images.Count - 1);
                item.Tag = lineType;
                if (lineType == selectedStyle)
                    item.Selected = true;
            }

            // привязать картинки к списку
            listView.LargeImageList = imageListLarge;
            listView.SmallImageList = imageListLarge;
        }

        private void ListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            var hasSel = listView.SelectedIndices.Count > 0;
            btnAccept.Enabled = hasSel;
            if (hasSel)
                selectedStyle = (TrendLine.TrendLineStyle)listView.SelectedItems[0].Tag;
        }

        private void ListViewMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (btnAccept.Enabled)
                DialogResult = DialogResult.OK;
        }

        private void TrendLineTypeDialogFormClosed(object sender, FormClosedEventArgs e)
        {
            var imgList = imageListLarge.Images.Cast<Image>().ToList();
            imageListLarge.Images.Clear();
            foreach (var image in imgList)
                image.Dispose();            
        }
    }
}

using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace TradeSharp.Util.Controls
{
    public class ImagedComboBox : ComboBox
    {
        public string DisplayImageIndexMember { get; set; }
        public string DisplayImageMember { get; set; }
        public ImageList ImageList { get; set; }

        public ImagedComboBox()
        {
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
            var image = GetImage(item);
            var imageSize = new Size();
            if (image != null)
            {
                imageSize = image.Size;
                e.Graphics.DrawImage(image, e.Bounds.Left, e.Bounds.Top);
            }
            e.Graphics.DrawString(GetText(item), e.Font, new SolidBrush(e.ForeColor), e.Bounds.Left + imageSize.Width,
                                  e.Bounds.Top);
            base.OnDrawItem(e);
        }

        private string GetText(object item)
        {
            if (string.IsNullOrEmpty(DisplayMember))
                return null;
            var textProperty = item.GetType().GetProperty(DisplayMember);
            if (textProperty == null)
                return null;
            return (string) textProperty.GetValue(item, null);
        }

        private Image GetImage(object item)
        {
            Image image = null;
            PropertyInfo imageProperty = null;
            PropertyInfo imageIndexProperty = null;
            if (!string.IsNullOrEmpty(DisplayImageMember))
                imageProperty = item.GetType().GetProperty(DisplayImageMember);
            if (!string.IsNullOrEmpty(DisplayImageIndexMember))
                imageIndexProperty = item.GetType().GetProperty(DisplayImageIndexMember);
            if (imageIndexProperty != null && ImageList != null)
            {
                var key = (string)imageIndexProperty.GetValue(item, null);
                if (!string.IsNullOrEmpty(key))
                    image = ImageList.Images[key];
            }
            else if (imageProperty != null)
            {
                image = (Image)imageProperty.GetValue(item, null);
            }
            return image;
        }

        private void OnMeasureItem(object sender, MeasureItemEventArgs e)
        {
            var maxSize = new Size();
            using (var graphics = CreateGraphics())
                foreach (var item in Items)
                {
                    var text = GetText(item);
                    var size = graphics.MeasureString(text, Font).ToSize();
                    var image = GetImage(item);
                    if (image != null)
                    {
                        size.Width += image.Width;
                        if (size.Height < image.Height)
                            size.Height = image.Height;
                    }
                    if (size.Width > maxSize.Width)
                        maxSize.Width = size.Width;
                    if (size.Height > maxSize.Height)
                        maxSize.Height = size.Height;
                }
            DropDownWidth = maxSize.Width;
            e.ItemWidth = maxSize.Width;
            e.ItemHeight = maxSize.Height;
        }
    }

    public class DropDownItem
    {
        public string Text { get; set; }
        public Image Image { get; set; }
        public string ImageKey { get; set; }

        public DropDownItem(string value, string key)
        {
            Text = value;
            ImageKey = key;
        }

        public DropDownItem(string value, Image picture)
        {
            Text = value;
            Image = picture;
        }
    }
}

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TradeSharp.Util.Controls
{
    public class ImagedLabelList : UserControl
    {
        [Editor("System.Windows.Forms.Design.StringCollectionEditor,System.Design",
             "System.Drawing.Design.UITypeEditor, System.Drawing")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<string> Labels { get; set; }

        public ImageList Images { get; set; }

        public int Indent { get; set; }

        public ImagedLabelList()
        {
            Labels = new List<string>();
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return GetAutoSize();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Labels == null || Images == null)
                return;
            List<string> keys;
            List<string> values;
            GetKeysAndValues(out keys, out values);
            var offset = 0;
            using(var graphics = CreateGraphics())
            using(var brush = new SolidBrush(ForeColor))
                for (var i = 0; i < keys.Count; i++)
                {
                    if (Images.Images.ContainsKey(keys[i]))
                        graphics.DrawImage(Images.Images[keys[i]], 0, offset);
                    var size = graphics.MeasureString(values[i], Font).ToSize();
                    var height = size.Height + 1;
                    if (Images.ImageSize.Height > height)
                        height = Images.ImageSize.Height;
                    graphics.DrawString(values[i], Font, brush,
                                        new RectangleF(Images.ImageSize.Width + Indent, offset, size.Width + 1, height),
                                        new StringFormat
                                            {
                                                LineAlignment = StringAlignment.Center,
                                                FormatFlags = StringFormatFlags.NoWrap
                                            });
                    offset += height;
                }
        }

        protected override void SetBoundsCore(int x, int y, int width, int height,
                BoundsSpecified specified)
        {
            if (AutoSize && (specified & BoundsSpecified.Size) != 0)
            {
                var size = GetAutoSize();
                width = size.Width;
                height = size.Height;
            }
            base.SetBoundsCore(x, y, width, height, specified);
        }

        private Size GetAutoSize()
        {
            if (Labels == null || Images == null)
                return Size.Empty;
            var result = new Size();
            List<string> keys;
            List<string> values;
            GetKeysAndValues(out keys, out values);
            using(var graphics = CreateGraphics())
                foreach (var text in values)
                {
                    var size = graphics.MeasureString(text, Font).ToSize();
                    var width = size.Width + Images.ImageSize.Width + Indent;
                    if (width > result.Width)
                        result.Width = width;
                    var height = size.Height;
                    if (Images.ImageSize.Height > height)
                        height = Images.ImageSize.Height;
                    result.Height += height;
                }
            return result;
        }

        private void GetKeysAndValues(out List<string> keys, out List<string> values)
        {
            keys = new List<string>();
            values = new List<string>();
            for (var i = 0; i < Labels.Count; i++)
            {
                var str = Labels[i];
                var text = str;
                string key = null;
                var keyBeginPos = str.IndexOf('[');
                if (keyBeginPos != -1)
                {
                    var keyEndPos = str.IndexOf(']', keyBeginPos);
                    if (keyEndPos != -1)
                        key = str.Substring(keyBeginPos + 1, keyEndPos - keyBeginPos - 1);
                    text = str.Substring(keyEndPos + 1);
                    keys.Add(key);
                    values.Add(text);
                }
            }
        }
    }
}

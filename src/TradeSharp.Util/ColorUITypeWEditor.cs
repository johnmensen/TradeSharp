using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;

namespace TradeSharp.Util
{
// ReSharper disable InconsistentNaming
    public class ColorUITypeWEditor : UITypeEditor
// ReSharper restore InconsistentNaming
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var form = new ColorDialog();
            if (value != null)
                form.Color = (Color) value;
            if (form.ShowDialog() == DialogResult.Cancel)
                return value;
            return form.Color;
        }
    }
}

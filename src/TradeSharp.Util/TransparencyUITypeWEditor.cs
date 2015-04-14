using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using TradeSharp.Util.Controls;
using TradeSharp.Util.Forms;

namespace TradeSharp.Util
{
    // ReSharper disable InconsistentNaming
    public class TransparencyUITypeWEditor : UITypeEditor
    // ReSharper restore InconsistentNaming
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var ctrlTransp = new TransparencyControl();
            if (value != null)
                ctrlTransp.Transparency = (int) value;
            var form = new DialogWithControl {MainControl = ctrlTransp, Text = "Прозрачность"};
            if (form.ShowDialog() == DialogResult.Cancel)
                return value;
            return ctrlTransp.Transparency;
        }
    }
}

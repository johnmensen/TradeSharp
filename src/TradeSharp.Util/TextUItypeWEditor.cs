using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using TradeSharp.Util.Forms;

namespace TradeSharp.Util
{
    public class TextUItypeWEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var str = "";
            if (value != null)
                str = (string) value;
            var form = new SimpleDialog(Localizer.GetString("TitleInput"), Localizer.GetString("MessageEnterText"), true, str) { InputRichText = true };
            if (form.ShowDialog() == DialogResult.Cancel)
                return value;
            return form.InputValue;
        }
    }
}

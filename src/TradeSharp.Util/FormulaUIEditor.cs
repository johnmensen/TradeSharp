using System;
using System.Drawing.Design;
using System.Windows.Forms;
using TradeSharp.Util.Forms;

namespace TradeSharp.Util
{
    public class FormulaUIEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, 
            IServiceProvider provider, object value)
        {
            var formula = value == null ? string.Empty : (string) value;
            var dlg = new FormulaEditorForm(formula);
            return dlg.ShowDialog() == DialogResult.OK ? dlg.Formula : value;
        }

        public override bool IsDropDownResizable
        {
            get { return false; }            
        }        
    }
}

using System;
using System.Drawing.Design;

namespace NewsRobot.UI
{
    public class FormulaEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context,
            IServiceProvider provider, object value)
        {
            var formula = value == null ? string.Empty : (string)value;
            var dlg = new FormulaEditForm(formula);
            return dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK ? dlg.Formula : value;
        }

        public override bool IsDropDownResizable
        {
            get { return false; }
        }
    }
}

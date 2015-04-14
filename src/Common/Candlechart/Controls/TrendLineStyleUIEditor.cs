using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using Candlechart.Series;

namespace Candlechart.Controls
{
    // ReSharper disable InconsistentNaming
    public class TrendLineStyleUIEditor : UITypeEditor
    // ReSharper restore InconsistentNaming
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context,
                                         IServiceProvider provider, object value)
        {
            var style = (TrendLine.TrendLineStyle) value;
            var dlg = new TrendLineTypeDialog
                {
                    SelectedStyle = style
                };
            return dlg.ShowDialog() == DialogResult.OK ? dlg.SelectedStyle : value;
        }

        public override bool IsDropDownResizable
        {
            get { return false; }
        }
    }

    public class TrendLineStyleTypeConverter : TypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return false;
        }
    }
}

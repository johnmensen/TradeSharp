using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Candlechart.Controls;

namespace Candlechart.Indicator
{
    public class CrossChartDiverUITypeEditor : UITypeEditor
    {
        private IWindowsFormsEditorService es;
        
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            es = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (es == null) return value;

            var indi = (IndicatorCrossChartDivergencies)context.Instance;
            // открыть модальный диалог выбора пары / ТФ
            var dlg = new CrossChartDivergenciesSettingsWindow(indi);
            return dlg.ShowDialog() != DialogResult.OK ? value : dlg.sets;
        }
    }
}

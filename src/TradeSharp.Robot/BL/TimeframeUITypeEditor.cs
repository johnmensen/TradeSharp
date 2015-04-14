using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Entity;

namespace TradeSharp.Robot.BL
{
    public class TimeframeUITypeEditor : UITypeEditor
    {
        private IWindowsFormsEditorService es;
        private BarSettings bot;

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            es = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (es == null) return value;

            bot = BarSettingsStorage.Instance.GetBarSettingsByName((string) value);
            // открыть модальный диалог выбора пары / ТФ
            var dlg = new TimeframeSettingsForm(bot);
            return dlg.ShowDialog() != DialogResult.OK ? value 
                : BarSettingsStorage.Instance.GetBarSettingsFriendlyName(dlg.Timeframe);
        }
    }
}

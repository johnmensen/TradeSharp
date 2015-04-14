using System;
using System.Drawing.Design;
using System.Windows.Forms;

namespace TradeSharp.Client.BL.Script
{
    public class ScriptTriggerUIEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context,
            IServiceProvider provider, object value)
        {
            var trigger = (TerminalScriptTrigger)value;
            var dlg = new TriggerSetupDialog(trigger);
            return dlg.ShowDialog() == DialogResult.OK ? dlg.Trigger : trigger;
        }

        public override bool IsDropDownResizable
        {
            get { return false; }
        }
    }
}

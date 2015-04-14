using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using TradeSharp.Util.Controls;

namespace TradeSharp.Util
{
    // ReSharper disable InconsistentNaming
    public class TransparencyUITypeEditor : UITypeEditor
    // ReSharper restore InconsistentNaming
    {
        private IWindowsFormsEditorService es;
        
        public TransparencyControl ctrlTransp = new TransparencyControl();

        public TransparencyUITypeEditor()
        {
            ctrlTransp.DoubleClick += OnDblClick;
            ctrlTransp.Leave += CtrlTranspLeave;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override bool IsDropDownResizable
        {
            get { return true; }
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            es = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (es == null) return null;

            var transp = (int)value;
            ctrlTransp.Transparency = transp;
            es.DropDownControl(ctrlTransp);
            return ctrlTransp.Transparency;
        }

        private void CtrlTranspLeave(object sender, EventArgs e)
        {
            //indi.SeriesSourcesDisplay = ctrlTransp.SelectedItem == null ? string.Empty : (string)ctrlTransp.SelectedItem;
        }

        private void OnDblClick(Object sender, EventArgs e)
        {
            es.CloseDropDown();
        }
    }
}

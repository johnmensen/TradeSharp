using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    public class ComboBoxDrawPaneUITypeEditor : UITypeEditor
    {
        private IWindowsFormsEditorService es;
        private IChartIndicator indi;
        public ListBox lb = new ListBox();

        
        public ComboBoxDrawPaneUITypeEditor()
        {
            lb.Leave += Leave;
            lb.DoubleClick += OnDblClick;
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

            indi = (IChartIndicator)context.Instance;
            LoadItems();

            lb.Sorted = true;
            es.DropDownControl(lb);
            return value;
        }

        private void LoadItems()
        {
            lb.Items.Clear();
            var chart = ((BaseChartIndicator)indi).owner.Owner;

            lb.Items.Add(Localizer.GetString("TitleCourse"));
            lb.Items.Add(indi.Name);
            foreach (var i in chart.indicators)
            {
                if (i != indi && i.CreateOwnPanel) lb.Items.Add(i.GetFullyQualifiedPaneName()); // !!
                // сравнение с копией
            }
            if (!string.IsNullOrEmpty(indi.DrawPaneDisplay))
                lb.SelectedItem = lb.Items.Contains(indi.DrawPaneDisplay) ? indi.DrawPaneDisplay : Localizer.GetString("TitleCourse");
        }

        private void Leave(Object sender, EventArgs e)
        {
            if (indi == null || lb.SelectedItem == null) return;
            var paneName = (string) lb.SelectedItem;

            indi.DrawPaneDisplay = (string)lb.SelectedItem;
            if (paneName == Localizer.GetString("TitleCourse") || paneName.Contains(Separators.IndiNameDelimiter[0]))
                indi.CreateOwnPanel = false;
            if (paneName == indi.Name)
                indi.CreateOwnPanel = true;
        }
        private void OnDblClick(Object sender, EventArgs e)
        {
            Leave(sender, e);
            es.CloseDropDown();
        }
    }
}

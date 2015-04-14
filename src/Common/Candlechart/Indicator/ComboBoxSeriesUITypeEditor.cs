using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
// ReSharper disable InconsistentNaming
    public class ComboBoxSeriesUITypeEditor : UITypeEditor
// ReSharper restore InconsistentNaming
    {
        private IWindowsFormsEditorService es;
        private IChartIndicator indi;
        public ListBox lb = new ListBox();

        public ComboBoxSeriesUITypeEditor()
        {            
            lb.DoubleClick += OnDblClick;
            lb.Leave += LbLeave;
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
            lb.Items.Add(Localizer.GetString("TitleCourse"));
            var chart = ((BaseChartIndicator) indi).owner.Owner;                
            foreach (var i in chart.indicators)
            {
                if (i == indi) continue;
                foreach (var ser in i.SeriesResult)
                {
                    lb.Items.Add(i.UniqueName + Separators.IndiNameDelimiter[0] + ser.Name);                    
                }
            }
            lb.SelectedItem = indi.SeriesSourcesDisplay;
        }

        private void LbLeave(object sender, EventArgs e)
        {
            indi.SeriesSourcesDisplay = lb.SelectedItem == null ? string.Empty : (string)lb.SelectedItem;
        }
        
        private void OnDblClick(Object sender, EventArgs e)
        {            
            es.CloseDropDown();
        }
    }
}

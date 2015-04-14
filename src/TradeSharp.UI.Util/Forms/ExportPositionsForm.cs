using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using TradeSharp.Contract.Entity;

namespace TradeSharp.UI.Util.Forms
{
    public partial class ExportPositionsForm : Form
    {
        private readonly List<MarketOrder> exportedDeals;

        private bool modeExport = false;

        public ExportPositionsForm()
        {
            InitializeComponent();

            SetupInterface();
        }

        public ExportPositionsForm(List<MarketOrder> exportedDeals)
        {
            InitializeComponent();

            this.exportedDeals = exportedDeals;
            modeExport = true;
            SetupInterface();
        }

        private void SetupInterface()
        {
            cbDigitSeparator.SelectedIndex = 0;
            cbGroupSeparator.SelectedIndex = 0;
            btnSaveLoad.ImageIndex = modeExport ? 1 : 0;
            
            // заполнить список полей
            foreach (var propInf in typeof(MarketOrder).GetProperties())
            {
                var atrs = propInf.GetCustomAttributes(typeof(DisplayNameAttribute), false);
                if (atrs.Length == 0) continue;
                var dispName = ((DisplayNameAttribute) atrs[0]).DisplayName;
                var fieldInfo = new MarketOrderExportField(propInf.Name, dispName);
                var node = treeView.Nodes.Add(fieldInfo.FieldTitle, fieldInfo.FieldTitle);
                node.Tag = fieldInfo;
                node.Checked = true;
            }
        }
    }

    class MarketOrderExportField
    {
        public string FieldName { get; set; }
        public string FieldTitle { get; set; }
        
        public MarketOrderExportField() {}

        public MarketOrderExportField(string name, string title)
        {
            FieldName = name;
            FieldTitle = title;            
        }
    }
}

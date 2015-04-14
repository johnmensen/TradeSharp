using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using FastGrid;
using TradeSharp.Client.BL;
using TradeSharp.Client.Forms;
using TradeSharp.Util;

namespace TradeSharp.Client.Controls
{
    public partial class HotKeysTableControl : UserControl
    {      
        public HotKeysTableControl()
        {
            InitializeComponent();

            SetupGrid();
            DataBind();
        }

        public List<ApplicationMessageBinding> AppBindings
        {
            get { return grid.GetRowValues<ApplicationMessageBinding>(false).ToList(); }
        }

        public void DataBind()
        {
            var hotKeyList = AppMessageFilter.HotKeys;
            grid.DataBind(hotKeyList.ToList());
        }

        public void CancelChanges()
        {
            AppMessageFilter.ApplyDiffString(UserSettings.Instance.HotKeyList);
            var hotKeyList = AppMessageFilter.HotKeys;
            grid.DataBind(hotKeyList.ToList());
        }

        private void SetupGrid()
        {
            var blank = new ApplicationMessageBinding(ApplicationMessage.Quotes, Keys.F1, ApplicationMessageBinding.WindowMessage.KeyDown);
            grid.Columns.Add(new FastColumn(blank.Property(p => p.Title), Localizer.GetString("TitleAction"))
                {
                    ColumnMinWidth = 200,
                    IsHyperlinkStyleColumn = true,
                    HyperlinkActiveCursor = Cursors.Hand
                });
            grid.Columns.Add(new FastColumn(blank.Property(p => p.Key), Localizer.GetString("TitleCombination"))
                {
                    ColumnWidth = 200,
                    IsHyperlinkStyleColumn = true,
                    HyperlinkActiveCursor = Cursors.Hand
                });
            
            grid.CalcSetTableMinWidth();
            grid.UserHitCell += GridUserHitCell;
        }

        void GridUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            var objHotKey = grid.rows[rowIndex].ValueObject as ApplicationMessageBinding;
            if (e.Button != MouseButtons.Left || objHotKey == null)
                return;
            if (col.PropertyName == objHotKey.Property(p => p.Key))
            {
                var dlg = new HotKeySetForm(objHotKey);
                if (dlg.ShowDialog() != DialogResult.OK) return;
                grid.UpdateRow(rowIndex, objHotKey);
                grid.InvalidateRow(rowIndex);
            }
            else if (col.PropertyName == objHotKey.Property(p => p.Title))
            {
                txtbxDescription.Text = objHotKey.ActionDescription;
            }
        }        
    
        public void ApplySettings()
        {
            var bindings = grid.GetRowValues<ApplicationMessageBinding>(false).ToList();
            AppMessageFilter.HotKeys = bindings;
            var difString = AppMessageFilter.GetDiffString();
            UserSettings.Instance.HotKeyList = difString;
        }
    }
}

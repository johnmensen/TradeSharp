using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Candlechart.Core;
using FastGrid;
using TradeSharp.Util;

namespace Candlechart.Controls
{
    public partial class ChooseIndicatorDialog : Form
    {
        private bool gridMode;

        public string SelectedIndiName { get; private set; }

        private readonly CandleChartControl owner;

        public ChooseIndicatorDialog()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
        }

        public ChooseIndicatorDialog(CandleChartControl owner) : this()
        {
            this.owner = owner;
            SetupTreeGrid();
            ChangeView(true);
        }

        private void BtnTreeClick(object sender, EventArgs e)
        {
            ChangeView(false);
        }

        private void BtnTableClick(object sender, EventArgs e)
        {
            ChangeView(true);
        }

        private void ChangeView(bool isGrid)
        {
            if (gridMode == isGrid) return;
            gridMode = isGrid;

            btnTable.FlatAppearance.BorderSize = isGrid ? 2 : 1;
            btnTree.FlatAppearance.BorderSize = isGrid ? 1 : 2;
            if (isGrid)
            {
                treeView.Visible = false;
                gridView.Visible = true;
                return;
            }
            gridView.Visible = false;
            treeView.Visible = true;
        }        
    
        private void SetupTreeGrid()
        {
            var blank = new IndicatorDescription(string.Empty, string.Empty, false);
            gridView.Columns.Add(new FastColumn(blank.Property(p => p.IsFavorite), "*")
                {
                    ColumnWidth = 25,
                    ImageList = imageListGrid,
                    SortOrder = FastColumnSort.Descending,
                    IsHyperlinkStyleColumn = true,
                    HyperlinkActiveCursor = Cursors.Hand
                });
            gridView.Columns.Add(new FastColumn(blank.Property(p => p.Name), Localizer.GetString("TitleName"))
                {
                    SortOrder = FastColumnSort.Ascending,
                    ColumnMinWidth = 120
                });
            gridView.Columns.Add(new FastColumn(blank.Property(p => p.Category), Localizer.GetString("TitleCategory"))
                {
                    ColumnMinWidth = 100
                });
            
            gridView.CalcSetTableMinWidth();
            gridView.UserHitCell += GridViewOnUserHitCell;
        }

        private void GridViewOnUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            if (!col.IsHyperlinkStyleColumn) return;
            // добавить / исключить из списка выбранных
            var desc = (IndicatorDescription) gridView.rows[rowIndex].ValueObject;
            var isFav = !desc.IsFavorite;
            desc.IsFavorite = isFav;
            // сохранить изменения
            var favList = gridView.rows.Select(r => ((IndicatorDescription) r.ValueObject)).Where(d => 
                d.IsFavorite).Select(d => d.TypeName).ToList();
            owner.CallFavoriteIndicatorsAreUpdated(favList);
            // перерисовать
            gridView.UpdateRow(rowIndex, desc);
            gridView.InvalidateCell(col, rowIndex);
        }

        private void ChooseIndicatorDialogLoad(object sender, EventArgs e)
        {
            if (!DesignMode) FillIndicators();
        }

        private void FillIndicators()
        {
            var listIndis = new List<IndicatorDescription>();
            var favIndis = owner.getFavoriteIndicators();

            foreach (var tp in PluginManager.Instance.typeIndicators)
            {
                var attrName =
                    Attribute.GetCustomAttributes(tp, true).FirstOrDefault(a => a is LocalizedDisplayNameAttribute) as
                    DisplayNameAttribute;
                if (attrName == null)
                    attrName =
                        Attribute.GetCustomAttributes(tp, true).FirstOrDefault(a => a is DisplayNameAttribute) as
                        DisplayNameAttribute;
                if (attrName == null)
                    continue;

                var attrCat =
                    Attribute.GetCustomAttributes(tp, true).FirstOrDefault(a => a is LocalizedCategoryAttribute) as
                    CategoryAttribute;
                if (attrCat == null)
                    attrCat =
                        Attribute.GetCustomAttributes(tp, true).FirstOrDefault(a => a is CategoryAttribute) as
                        CategoryAttribute;
                var catName = attrCat == null ? Localizer.GetString("TitleMain") : attrCat.Category;

                var isFav = favIndis.Contains(tp.Name);

                listIndis.Add(new IndicatorDescription(attrName.DisplayName, catName, isFav ) { TypeName = tp.Name });
            }

            // прибайндить к гриду
            gridView.DataBind(listIndis);

            // записать в дерево
            foreach (var desc in listIndis)
            {
                var cat = string.IsNullOrEmpty(desc.Category) ? "-" : desc.Category;
                
                // найти или создать родительский узел для категории
                TreeNode node;
                var parentNodes = treeView.Nodes.Find(cat, false);
                if (parentNodes.Length > 0) node = parentNodes[0];
                else
                {
                    node = treeView.Nodes.Add(cat, cat);
                }

                // добавить узел
                var itemNode = node.Nodes.Add(desc.Name, desc.Name);
                itemNode.Tag = desc;
            }
        }

        // ReSharper disable InconsistentNaming
        private void BtnOKClick(object sender, EventArgs e)
        // ReSharper restore InconsistentNaming
        {
            // выбрать из грида
            if (gridView.Visible)
            {
                var selRow = gridView.rows.FirstOrDefault(r => r.Selected);
                if (selRow == null) return;
                SelectedIndiName = ((IndicatorDescription) selRow.ValueObject).Name;
                DialogResult = DialogResult.OK;
                return;
            }
            
            // выбрать из дерева
            if (treeView.SelectedNode == null) return;
            if (treeView.SelectedNode.Tag == null ||
                treeView.SelectedNode.Tag is IndicatorDescription == false) return;
            SelectedIndiName = ((IndicatorDescription)treeView.SelectedNode.Tag).Name;
            DialogResult = DialogResult.OK;
        }

        private void GridViewUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            if (e.Button != MouseButtons.Left || e.Clicks <= 1) return;
            SelectedIndiName = ((IndicatorDescription)gridView.rows[rowIndex].ValueObject).Name;
            DialogResult = DialogResult.OK;
        }

        private void TreeViewNodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag == null || e.Node.Tag is IndicatorDescription == false) return;
            SelectedIndiName = ((IndicatorDescription)e.Node.Tag).Name;
            DialogResult = DialogResult.OK;
        }

        private void ChooseIndicatorDialogHelpRequested(object sender, HelpEventArgs hlpevent)
        {
            HelpManager.Instance.ShowHelp(this, HelpManager.IndicatorWindow);
        }        
    }
}

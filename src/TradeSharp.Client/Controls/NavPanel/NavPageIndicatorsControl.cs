using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Candlechart.Controls;
using Candlechart.Core;
using Entity;
using FastGrid;
using TradeSharp.Client.BL;

namespace TradeSharp.Client.Controls.NavPanel
{
    public partial class NavPageIndicatorsControl : UserControl
    {
        public NavPageIndicatorsControl()
        {
            InitializeComponent();
            SetupGrid();
            LoadIndicators();
        }

        private void SetupGrid()
        {
            grid.BackColor = SystemColors.ButtonFace;
            grid.ColorAltCellBackground = SystemColors.ButtonHighlight;
            grid.ColorCellBackground = SystemColors.ButtonHighlight;
            grid.ColorSelectedCellBackground = SystemColors.ButtonFace;
            grid.SelectEnabled = false;

            grid.Columns.Add(new FastColumn("IsFavorite", "*")
            {
                ColumnWidth = 25,
                ImageList = imageListGrid,
                SortOrder = FastColumnSort.Descending,
                IsHyperlinkStyleColumn = true,
                HyperlinkActiveCursor = Cursors.Hand
            });
            grid.Columns.Add(new FastColumn("Name", "Название")
            {
                ColumnMinWidth = 50,
                SortOrder = FastColumnSort.Ascending
            });
            grid.Columns.Add(new FastColumn("Category", "Тип")
            {
                ColumnMinWidth = 50
            });
            grid.MouseDown += GridMouseDown;
        }

        private void GridMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            var cell = grid.GetCellUnderCursor(e.X, e.Y);
            if (!cell.HasValue) return;
            var indi = (IndicatorDescription)grid.rows[cell.Value.Y].ValueObject;
            grid.DoDragDrop(
                new CandleChartDroppingObject(indi.indicatorType, CandleChartDroppingObject.ValueType.Indicator), DragDropEffects.All);
        }

        private void LoadIndicators()
        {
            var listIndis = new List<IndicatorDescription>();
            var favIndis = UserSettings.Instance.FavoriteIndicators.ToList();

            foreach (var tp in PluginManager.Instance.typeIndicators)
            {
                var attrName = (DisplayNameAttribute)Attribute.GetCustomAttribute(tp,
                    typeof(DisplayNameAttribute));
                if (attrName == null) continue;
                var attrCat = (CategoryAttribute)Attribute.GetCustomAttribute(tp,
                    typeof(CategoryAttribute));
                var catName = attrCat == null ? "Основные" : attrCat.Category;

                var isFav = favIndis.Contains(tp.Name);
                listIndis.Add(new IndicatorDescription(attrName.DisplayName, catName, isFav) { indicatorType = tp });
            }

            // прибайндить к гриду
            grid.DataBind(listIndis);
        }
    }
}

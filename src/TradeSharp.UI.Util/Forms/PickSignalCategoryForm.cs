using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FastGrid;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.UI.Util.Forms
{
    public partial class PickSignalCategoryForm : Form
    {
        private readonly List<PaidService> categories;
        private readonly bool enableSignalMakerOption;

        private const string ColTitleSubscribe = "Подписка";
        private const string ColTitleMakeSingal = "Рассылка";

        /// <summary>
        /// выбранная пользователем категория торг. сигналов
        /// </summary>
        public PaidService SelectedCategory { get; private set; }

        public bool SubscriptionSelected { get; private set; }

        public bool SignalMakingSelected { get; private set; }

        public PickSignalCategoryForm()
        {
            InitializeComponent();
            SetupGrid();
        }

        public PickSignalCategoryForm(List<PaidService> categories, bool enableSignalMakerOption)
        {
            InitializeComponent();

            this.categories = categories;
            this.enableSignalMakerOption = enableSignalMakerOption;
            SetupGrid();
        }

        private void SetupGrid()
        {
            var fontBold = new Font(Font, FontStyle.Bold);
            grid.Columns.Add(new FastColumn("Comment", "Сигнал") { SortOrder = FastColumnSort.Ascending, ColumnMinWidth = 60 });
            grid.Columns.Add(new FastColumn("FixedPrice", "Описание")
                {
                    ColumnMinWidth = 40,
                    formatter = value => ((decimal)value).ToStringUniformMoneyFormat(true)
                });
            grid.Columns.Add(new FastColumn("Comment", ColTitleSubscribe)
                                 {
                                     formatter = v => "подписаться",
                                     ColumnWidth = 68,
                                     IsHyperlinkStyleColumn = true,
                                     HyperlinkFontActive = fontBold,
                                     HyperlinkActiveCursor = Cursors.Hand
                                 });
            if (enableSignalMakerOption)
                grid.Columns.Add(new FastColumn("Title", ColTitleMakeSingal)
                {
                    formatter = v => "рассылать",
                    ColumnWidth = 60,
                    IsHyperlinkStyleColumn = true,
                    HyperlinkFontActive = fontBold,
                    HyperlinkActiveCursor = Cursors.Hand
                });
            
            grid.MinimumTableWidth =
                grid.Columns.Sum(c => c.ColumnWidth > 0 ? c.ColumnWidth : c.ColumnMinWidth > 0 ? c.ColumnMinWidth : 60);
            grid.UserHitCell += GridUserHitCell;
        }

        private void GridUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            if (!col.IsHyperlinkStyleColumn || e.Button != MouseButtons.Left) return;

            SubscriptionSelected = col.Title == ColTitleSubscribe;
            SignalMakingSelected = col.Title == ColTitleMakeSingal;
            if (!SubscriptionSelected && !SignalMakingSelected) return;
            SelectedCategory = (PaidService)grid.rows[rowIndex].ValueObject;
            DialogResult = DialogResult.OK;
        }

        private void PickSignalCategoryFormLoad(object sender, System.EventArgs e)
        {
            if (categories == null) return;
            grid.DataBind(categories);
        }
    }    
}

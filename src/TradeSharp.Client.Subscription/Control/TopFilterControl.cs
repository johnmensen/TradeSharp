using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FastGrid;
using TradeSharp.Client.Subscription.Dialog;
using TradeSharp.Contract.Entity;
using System.Linq;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;
using TradeSharp.Util.Forms;

namespace TradeSharp.Client.Subscription.Control
{
    public partial class TopFilterControl : UserControl
    {
        public EventHandler PerformerCriteriaFunctionCollectionChanged;

        public EventHandler PerformerCriteriaFunctionChanged;

        public EventHandler CollapseButtonClicked;

        public EventHandler RefreshButtonClicked;

        public EventHandler CreatePortfolioButtonClicked;

        public PerformerStatField SortField
        {
            get { return SelectedFunction.SortField; }
            set
            {
                SelectedFunction.SortField = value;
                expressionLabel.Text = SelectedFunction.Function;
            }
        }

        public int SortFieldIndex
        {
            get
            {
                if (SelectedFunction.SortField == null)
                    return -1;
                return PerformerStatField.fields.IndexOf(SelectedFunction.SortField);
            }
            set
            {
                var fields = PerformerStatField.fields;
                if (value < 0 || value >= fields.Count)
                    SelectedFunction.SortField = null;
                else
                    SelectedFunction.SortField = fields[value];
                expressionLabel.Text = SelectedFunction.Function;
            }
        }

        // (исп. только для хранения)
        public SortOrder SortOrder;

        public bool RefreshButtonEnabled
        {
            set { refreshButton.Enabled = value; }
        }

        public PerformerCriteriaFunction SelectedFunction = new PerformerCriteriaFunction();

        private readonly List<FilterObject> filterObjects = new List<FilterObject>();

        private Color[] refreshButtonColors;

        private int refreshButtonColorIndex;

        public TopFilterControl()
        {
            InitializeComponent();

            // 4 refresh button
            refreshButtonColors = new[]
                {
                    refreshButton.BackColor,
                    HslColor.AdjuctBrightness(refreshButton.BackColor, 0.9f),
                    HslColor.AdjuctBrightness(refreshButton.BackColor, 0.85f),
                    HslColor.AdjuctBrightness(refreshButton.BackColor, 0.8f),
                    HslColor.AdjuctBrightness(refreshButton.BackColor, 0.85f),
                    HslColor.AdjuctBrightness(refreshButton.BackColor, 0.9f),
                    refreshButton.BackColor,
                    HslColor.AdjuctBrightness(refreshButton.BackColor, 1.1f),
                    HslColor.AdjuctBrightness(refreshButton.BackColor, 1.15f),
                    HslColor.AdjuctBrightness(refreshButton.BackColor, 1.2f),
                    HslColor.AdjuctBrightness(refreshButton.BackColor, 1.15f),
                    HslColor.AdjuctBrightness(refreshButton.BackColor, 1.1f)
                };

            var blank = new FilterObject();
            fastGrid.Columns.Add(new FastColumn(blank.Property(p => p.Selected), "*")
                {
                    ColumnWidth = 30,
                    ImageList = gridImageList,
                    IsHyperlinkStyleColumn = true,
                    HyperlinkActiveCursor = Cursors.Hand,
                    SortOrder = FastColumnSort.Descending
                });
            fastGrid.Columns.Add(new FastColumn(blank.Property(p => p.Group), Localizer.GetString("TitleGroup"))
                {
                    ColumnWidth = 120,
                    SortOrder = FastColumnSort.Descending
                });
            fastGrid.Columns.Add(new FastColumn(blank.Property(p => p.Title), Localizer.GetString("TitleCriterion"))
                {
                    //SortOrder = FastColumnSort.Ascending
                });
            fastGrid.Columns.Add(new FastColumn(blank.Property(p => p.Operator), " ")
                {
                    ColumnWidth = 30,
                    IsHyperlinkStyleColumn = true,
                    HyperlinkActiveCursor = Cursors.Hand,
                    formatter = v => ExpressionResolver.GetOperatorString((ExpressionOperator) v)
                });
            fastGrid.Columns.Add(new FastColumn(blank.Property(p => p.Value), Localizer.GetString("TitleValue"))
                {
                    IsEditable = true
                });

            RebindData();

            fastGrid.FieldValueChanged += delegate
                {
                    UpdateExpression();
                };

            collapseButton.Click += (sender, args) =>
                {
                    if (CollapseButtonClicked != null)
                        CollapseButtonClicked(this, new EventArgs());
                };

            refreshButton.Click += (sender, args) =>
                {
                    if (RefreshButtonClicked != null)
                        RefreshButtonClicked(this, new EventArgs());
                };

            createPortfolioButton.Click += (sender, args) =>
                {
                    if (CreatePortfolioButtonClicked != null)
                        CreatePortfolioButtonClicked(this, new EventArgs());
                };

            // secret button
            showExpressionLabelButton.Click += (sender, args) => { expressionLabel.Visible = !expressionLabel.Visible; };
        }

        public void SetExpression(PerformerCriteriaFunction function)
        {
            SortOrder = function.PreferredSortOrder;
            SetExpression(function.Function);
        }

        public void SetExpression(string expression)
        {
            expressionLabel.Text = expression;
            RebindData();
            SelectedFunction.Function = expression;
            if (!SelectedFunction.IsExpressionParsed)
                return;
            var errors = "";
            foreach (var filter in SelectedFunction.Filters)
            {
                var filterObject = filterObjects.FirstOrDefault(o => o.Name == filter.a.ExpressionParamName);
                if (filterObject == null)
                {
                    errors += filter.a.ExpressionParamName + ExpressionResolver.GetOperatorString(filter.b) +
                                filter.c + "\n";
                    continue;
                }
                filterObject.Selected = true;
                filterObject.Operator = filter.b;
                filterObject.Value = filter.c;
            }
            if (!string.IsNullOrEmpty(errors))
            {
                MessageBox.Show(this, "Невозможно отобразить следующие критерии:\n" + errors, "Предупреждение",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            fastGrid.DataBind(filterObjects);
        }

        private void RebindData()
        {
            filterObjects.Clear();
            const int importantFieldsCount = 6;
            for (var i = 0; i < PerformerStatField.fields.Count; i++)
            {
                var field = PerformerStatField.fields[i];
                if (string.IsNullOrEmpty(field.ExpressionParamTitle))
                    continue;
                var filterObject = new FilterObject(field)
                    {
                        Group = i < importantFieldsCount ? Localizer.GetString("TitlePrimaries") : Localizer.GetString("TitleAdditionals")
                    };
                filterObjects.Add(filterObject);
            }
            fastGrid.DataBind(filterObjects);
        }

        private void SaveButtonClick(object sender, EventArgs e)
        {
            var dialog = new SimpleDialog(Localizer.GetString("TitleExpressionCreation"),
                                          Localizer.GetString("MessageEnterFormulaComment"), true, SelectedFunction.Description);
            if (dialog.ShowDialog(this) == DialogResult.Cancel)
                return;
            var newF = new PerformerCriteriaFunction(SelectedFunction)
                {
                    Description = dialog.InputValue
                };
            PerformerCriteriaFunctionCollection.Instance.criterias.Add(newF);
            PerformerCriteriaFunctionCollection.Instance.SelectedFunction = newF;
            PerformerCriteriaFunctionCollection.Instance.WriteToFile();
            if (PerformerCriteriaFunctionCollectionChanged != null)
                PerformerCriteriaFunctionCollectionChanged(this, new EventArgs());
        }

        private void ExpressionEditButtonClick(object sender, EventArgs e)
        {
            if (new PerformerCriteriaFunctionForm().ShowDialog() != DialogResult.OK)
                return;
            if (PerformerCriteriaFunctionCollectionChanged != null)
                PerformerCriteriaFunctionCollectionChanged(this, new EventArgs());
            SetExpression(PerformerCriteriaFunctionCollection.Instance.SelectedFunction.Function);
        }

        private void UpdateExpression()
        {
            // если что-то изменили в таблице, то формула обновляется;
            // если таблица в предыдущей итерации не смогла представить ее, то старая формула стирается,
            // и новая начинает формироваться по таблице
            var expression = "";
            foreach (var fiterObject in filterObjects)
            {
                if (!fiterObject.Selected)
                    continue;
                expression = fiterObject.UpdateExpression(expression);
            }
            // избегаем ошибки парсинга в PerformerStatField.ParseSimpleFormula, задавая поле для сортировки
            if (SortField == null)
            {
                var blank = new PerformerStat();
                SortField = PerformerStatField.fields.FirstOrDefault(f => f.PropertyName == blank.Property(p => p.Profit));
                if (SortField == null)
                    return;
            }
            expression = (!string.IsNullOrEmpty(expression) ? "(" + expression + ")*" : "") + SortField.ExpressionParamName;
            SelectedFunction = new PerformerCriteriaFunction
            {
                Function = expression,
                PreferredSortOrder = SortOrder,
                MarginValue = 0
            };
            expressionLabel.Text = SelectedFunction.Function;
            if (PerformerCriteriaFunctionChanged != null)
                PerformerCriteriaFunctionChanged(this, new EventArgs());
        }

        private void FastGridUserHitCell(object sender, MouseEventArgs mouseEventArgs, int rowIndex, FastColumn col)
        {
            var filterObject = (FilterObject) fastGrid.rows[rowIndex].ValueObject;
            if (col.PropertyName == filterObject.Property(p => p.Selected))
            {
                filterObject.Selected = !filterObject.Selected;
                fastGrid.UpdateRow(rowIndex, filterObject);
                fastGrid.InvalidateRow(rowIndex);
                UpdateExpression();
            }
            if (col.PropertyName == filterObject.Property(p => p.Operator))
            {
                operatorsContextMenuStrip.Tag = filterObject;
                foreach (ToolStripMenuItem item in operatorsContextMenuStrip.Items)
                {
                    item.Checked = PerformerCriteriaFunction.GetExpressionOperatorString(filterObject.Operator) == item.Text;
                }
                operatorsContextMenuStrip.Show(fastGrid, fastGrid.GetCellCoords(col, rowIndex));
            }
        }

        private void OperatorsContextMenuStripItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e == null)
                return;
            var filter = operatorsContextMenuStrip.Tag as FilterObject;
            if (filter == null)
                return;
            var item = e.ClickedItem;
            if (item == null)
                return;
            filter.Operator = PerformerCriteriaFunction.GetExpressionOperatorByString(item.Text);
            var rowIndex = fastGrid.rows.FindIndex(r => r.ValueObject == filter);
            fastGrid.UpdateRow(rowIndex, filter);
            fastGrid.InvalidateRow(rowIndex);
            UpdateExpression();
        }

        /// <summary>
        /// анимировать элементы интерфейса
        /// </summary>
        private void TimerWhistlerFarterTick(object sender, EventArgs e)
        {
            if (refreshButtonColors == null)
                return;
            if (!refreshButton.Enabled)
            {
                refreshButton.BackColor = refreshButtonColors[0];
                return;
            }
            refreshButton.BackColor = refreshButtonColors[refreshButtonColorIndex++];
            if (refreshButtonColorIndex >= refreshButtonColors.Length)
                refreshButtonColorIndex = 0;
        }
    }

    public class FilterObject
    {
        public bool Selected { get; set; }

        public string Group { get; set; }

        public string Name { get; set; }

        public string Title { get; set; }

        public ExpressionOperator Operator { get; set; }

        public double Value { get; set; }

        public FilterObject(PerformerStatField field = null)
        {
            if (field == null)
                return;
            Name = field.ExpressionParamName;
            Title = field.ExpressionParamTitle;
            if (field.DefaultOperator.HasValue)
                Operator = field.DefaultOperator.Value;
            if (field.DefaultValue.HasValue)
                Value = field.DefaultValue.Value;
        }

        // конкатенация выражения этого объекта с существующим выражением;
        // используется для представления формулы из FilterObject-ов в виде строки
        public string UpdateExpression(string expression)
        {
            return expression + (!string.IsNullOrEmpty(expression) ? "&" : "") + "(" + Name +
                   PerformerCriteriaFunction.GetExpressionOperatorString(Operator) + Value + ")";
        }
    }
}

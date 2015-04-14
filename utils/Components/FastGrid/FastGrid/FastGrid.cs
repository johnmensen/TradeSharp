using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Linq;

namespace FastGrid
{
    public delegate void ColorCellByValueDel(object cellValue, out Color? backColor, out Color? fontColor);

    public delegate void DrawCellTextDel(int columnIndex, FastColumn column, FastCell cell, BrushesStorage brushes,
                                         Graphics g, Point leftTop, int cellWidth, int cellHeight,
                                         Font font, Brush brushFont, Color? fontColor, int rowIndex, int cellPadding);

    public partial class FastGrid : UserControl
    {
        public delegate string[] FormatObjectFieldsDel(object rowValueObject, List<FastColumn> columns);

        public delegate bool FieldValueBeforeEditDel(FastColumn col, int rowIndex, object editingValue);

        public delegate void FieldValueChangingDel(FastColumn col, int rowIndex,
                                                   object editingObject, ref object newValue, out bool cancelEdit);

        public delegate void FieldValueChangedDel(FastColumn col, int rowIndex, object editedObject);

        public delegate void GroupingFunctionDel(object data, out object groupData, out string groupLabel);

        public FormatObjectFieldsDel rowExtraFormatter;

        /// <summary>
        /// колонки таблицы. set используется только для изменения порядка следования
        /// если пользователю разрешено перемещение колонок,
        /// то доступ к ним необходимо осуществлять только по имени
        /// </summary>
        private readonly List<FastColumn> columns = new List<FastColumn>();
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<FastColumn> Columns
        {
            get
            {
                return columns;
            }
            set // установить порядок следования колонок
            {
                if (!value.All(c => columns.Contains(c)))
                    return;
                foreach (var column in value)
                {
                    MoveColumn(columns.IndexOf(column), value.IndexOf(column));
                }
                Invalidate();
            }
        }

        public List<GroupingFunctionDel> GroupingFunctions;

        public List<Comparison<object>> GroupingComparisons;

        public readonly List<FastRow> rows = new List<FastRow>();

        private readonly List<FastRow> collapsedRows = new List<FastRow>();

        private Dictionary<FastColumn, PropertyInfo> columnProperty = new Dictionary<FastColumn, PropertyInfo>();

        private readonly VScrollBar scrollBarV = new VScrollBar();

        private readonly HScrollBar scrollBarH = new HScrollBar();

        private readonly Panel scrollPanel = new Panel();

        public ColorCellByValueDel colorFormatter;

        private bool selectEnabled = true;
        public bool SelectEnabled
        {
            get { return selectEnabled; }
            set { selectEnabled = value; }
        }

        public bool MultiSelectEnabled { get; set; }

        /// <summary>
        /// (obsolete: use Control.MinimumSize)
        /// </summary>
        public int? MinimumTableWidth { get; set; }

        /// <summary>
        /// first row is sticked and is not sorted
        /// </summary>
        public bool StickFirst { get; set; }

        /// <summary>
        /// last row is sticked and is not sorted
        /// </summary>
        public bool StickLast { get; set; }

        public enum CellEditModeTrigger { LeftClick = 0, LeftDoubleClick = 1, RightClick = 2 }

        public CellEditModeTrigger CellEditMode { get; set; }

        private readonly Cursor defaultCursor;

        private FieldValueBeforeEditDel fieldValueBeforeEdit;
        public event FieldValueBeforeEditDel FieldValueBeforeEdit
        {
            add { fieldValueBeforeEdit += value; }
            remove { fieldValueBeforeEdit -= value; }
        }

        private FieldValueChangingDel fieldValueChanging;
        public event FieldValueChangingDel FieldValueChanging
        {
            add { fieldValueChanging += value; }
            remove { fieldValueChanging -= value; }
        }

        private FieldValueChangedDel fieldValueChanged;
    
        public event FieldValueChangedDel FieldValueChanged
        {
            add { fieldValueChanged += value; }
            remove { fieldValueChanged -= value; }
        }

        private DrawCellTextDel userDrawCellText;
        public event DrawCellTextDel UserDrawCellText
        {
            add
            {
                var handlerAdded = userDrawCellText == null;
                userDrawCellText += value;
                if (handlerAdded)
                    foreach (var row in rows)
                        row.UserDrawCellText += OnUserDrawCellText;
            }
            remove
            {
                userDrawCellText -= value;
                if (userDrawCellText == null)
                    foreach (var row in rows)
                        row.UserDrawCellText -= OnUserDrawCellText;
            }
        }

        public ImageList ImageList;

        private IContainer components;

        // methods
        public FastGrid()
        {
            InitializeComponent();

            // scrolling
            scrollBarV.Parent = this;
            scrollBarV.Visible = false;
            scrollBarV.Value = 0;
            scrollBarV.SmallChange = 1;            
            scrollBarV.Scroll += ScrollBarVScroll;

            scrollBarH.Parent = this;
            scrollBarH.Visible = false;
            scrollBarH.Value = 0;
            scrollBarH.SmallChange = 1;
            scrollBarH.Scroll += ScrollBarScrollH;

            scrollPanel.Parent = this;
            scrollPanel.Visible = false;

            defaultCursor = Cursor;

            DoubleBuffered = true;
        }
        
        #region Sorting

        private void MoveColumn(int srcIndex, int destIndex)
        {
            var movingColumn = columns[srcIndex];
            if (destIndex > srcIndex)
            {
                columns.Insert(destIndex, movingColumn);
                columns.RemoveAt(srcIndex);
                foreach (var row in rows)
                {
                    var moveingCell = row.cells[srcIndex];
                    row.cells.Insert(destIndex, moveingCell);
                    row.cells.RemoveAt(srcIndex);
                }
            }
            else if (destIndex < srcIndex)
            {
                columns.RemoveAt(srcIndex);
                columns.Insert(destIndex, movingColumn);
                foreach (var row in rows)
                {
                    var moveingCell = row.cells[srcIndex];
                    row.cells.RemoveAt(srcIndex);
                    row.cells.Insert(destIndex, moveingCell);
                }
            }
        }

        public void OrderRows()
        {
            var cellComparer = new CellComparer(columns);
            var endIndex = 0;
            while (endIndex < rows.Count)
            {
                var startIndex = rows.FindIndex(endIndex, r => !r.IsGroupingRow);
                if (startIndex == -1)
                    break;
                endIndex = rows.FindIndex(startIndex, r => r.IsGroupingRow);
                if (endIndex == -1)
                    endIndex = rows.Count;
                if (!cellComparer.HasSortableColumns) return;
                rows.Sort(startIndex, endIndex - startIndex, cellComparer);
            }
        }

        #endregion
    
        #region Keyboard

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            if ((e.Control && e.KeyCode == Keys.C) ||
                (e.Shift && e.KeyCode == Keys.Insert))
            {
                // copy string
                var selRows = rows.Where(r => r.Selected);
                var sb = new StringBuilder();
                foreach (var row in selRows)
                {
                    sb.AppendLine(string.Join(" ", row.cells.Select(c => c.CellString).ToArray()));
                }
                if (sb.Length > 0) Clipboard.SetText(sb.ToString());
            }
        }

        #endregion

        #region Grouping

        public void Regroup()
        {
            var dataRows = rows.Where(r => !r.IsGroupingRow).ToList();
            //dataRows.ForEach(r => r.OwnerGroupingRow = null);
            rows.Clear();
            rows.AddRange(Group(dataRows, 0));
        }

        public List<FastRow> Group(List<FastRow> dataRows, int level)
        {
            if (GroupingFunctions == null || level >= GroupingFunctions.Count)
                return dataRows;
            var funct = GroupingFunctions[level];
            if (funct == null)
                return dataRows;

            var groupDicts = new Dictionary<FastRow, List<FastRow>>();
            foreach (var dataRow in dataRows)
            {
                object groupKey;
                string label;
                funct(dataRow.ValueObject, out groupKey, out label);
                var group = groupDicts.FirstOrDefault(g =>
                    {
                        var a = g.Key.ValueObject as IComparable;
                        var b = groupKey as IComparable;
                        if (a == null || b == null)
                            return false;
                        return a.CompareTo(b) == 0;
                    }).Key;
                if (group == null)
                {
                    group = new FastRow(this, groupKey, new Dictionary<FastColumn, PropertyInfo>()) { IsGroupingRow = true };
                    columns.ForEach(c => group.cells.Add(new FastCell {CellString = label}));
                    groupDicts.Add(group, new List<FastRow>());
                }
                // пока "кидаем" все строки в исходном порядке
                dataRow.OwnerGroupingRow = group;
                groupDicts[group].Add(dataRow);
            }

            // сортируем строки группирования, группируем "закинутые" вложенные строки, формируем результат
            var groupKeysObject = groupDicts.Select(kv => kv.Key.ValueObject).ToList();
            if (GroupingComparisons != null && level < GroupingComparisons.Count)
            {
                groupKeysObject.Sort(GroupingComparisons[level]);
            }
            var result = new List<FastRow>();
            foreach (var groupKeyObject in groupKeysObject)
            {
                var groupKey = groupDicts.Keys.FirstOrDefault(k => k.ValueObject == groupKeyObject);
                var groupValue = groupDicts[groupKey];
                var groupValues = Group(groupValue, level + 1).ToList();
                groupValue.Clear();
                groupValue.AddRange(groupValues);
                result.Add(groupKey);
                result.AddRange(groupValue);
            }
            return result;
        }

        #endregion

        #region Misc

        public void CalcSetTableMinWidth(int defaultColumnWidth = 65)
        {
            MinimumTableWidth = columns.Sum(c => c.ColumnWidth > 0
                                                     ? c.ColumnWidth
                                                     : c.ColumnMinWidth > 0 ? c.ColumnMinWidth : defaultColumnWidth);
        }

        private void OnUserDrawCellText(int columnIndex, FastColumn column, FastCell cell, BrushesStorage brushes,
                                        Graphics g, Point leftTop, int cellWidth, int cellHeightI,
                                        Font font, Brush brushFont, Color? fontColor, int rowIndex, int cellPaddingI)
        {
            if (userDrawCellText != null)
                userDrawCellText(columnIndex, column, cell, brushes, g, leftTop, cellWidth, cellHeightI, font,
                                 brushFont, fontColor, rowIndex, cellPaddingI);
        }

        #endregion

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FastGrid));
            this.ImageList = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // ImageList
            // 
            this.ImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ImageList.ImageStream")));
            this.ImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.ImageList.Images.SetKeyName(0, "collapse");
            this.ImageList.Images.SetKeyName(1, "expand");
            // 
            // FastGrid
            // 
            this.Name = "FastGrid";
            this.ResumeLayout(false);

        }
    }    
}

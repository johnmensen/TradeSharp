using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FastGrid
{
    public partial class FastGrid
    {
        private int lastHitRow = -1;
        [Browsable(false)]
        public int LastHitRow
        {
            get { return lastHitRow; }
        }
        private FastColumn lastHitColumn;
        [Browsable(false)]
        public FastColumn LastHitColumn
        {
            get { return lastHitColumn; }
        }

        private Point? lastHoveredCell;
        [Browsable(false)]
        public Point? LastHoveredCell
        {
            get { return lastHoveredCell; }
            private set
            {
                if (!lastHoveredCell.HasValue && value == null) return;
                if (lastHoveredCell.HasValue && value.HasValue)
                    if (lastHoveredCell == value) return;
                
                // turn cell's light mode off
                if (value == null)
                {
                    if (columns[lastHoveredCell.Value.X].IsHyperlinkStyleColumn)
                        InvalidateCell(lastHoveredCell.Value.X, lastHoveredCell.Value.Y);
                    lastHoveredCell = null;
                    Cursor = defaultCursor;
                    return;
                }

                // redraw new cell?
                if (columns[value.Value.X].IsHyperlinkStyleColumn)
                {
                    if (columns[value.Value.X].HyperlinkActiveCursor != null)
                        Cursor = columns[value.Value.X].HyperlinkActiveCursor;
                    InvalidateCell(value.Value.X, value.Value.Y);
                }
                else Cursor = defaultCursor;
 
                // redraw old cell?
                if (lastHoveredCell.HasValue)
                    if (columns[lastHoveredCell.Value.X].IsHyperlinkStyleColumn)
                        InvalidateCell(lastHoveredCell.Value.X, lastHoveredCell.Value.Y);

                // redraw row?
                if ((value.HasValue && columns[value.Value.X].ShowClippedContent))
                {
                    InvalidateRow(value.Value.Y);
                }
                if (lastHoveredCell.HasValue && columns[lastHoveredCell.Value.X].ShowClippedContent)
                {
                    InvalidateRow(lastHoveredCell.Value.Y);
                }
                
                lastHoveredCell = value;
            }
        }

        /// <summary>
        /// cursor position when dragging begins
        /// = null if dragging was not started
        /// </summary>
        private Point? startDragCoords;

        private bool dragStarted = false;

        /// <summary>
        /// current cursor position during dragging
        /// = null for minor dragging (column moving was ignored - it was less then threshold)
        /// </summary>
        private Point? dragCoords;

        private int resizingColumnIndex = -1;
        private int resizingColumnStartWidth;
        private Point startResizingCoords;
        //private Point lastResizingCoords;

        private UserHitCellDel userHitCell;
        public event UserHitCellDel UserHitCell
        {
            add { userHitCell += value; }
            remove { userHitCell -= value; }
        }

        private UserHitCellDel contextMenuRequested;
        public event UserHitCellDel ContextMenuRequested
        {
            add { contextMenuRequested += value; }
            remove { contextMenuRequested -= value; }
        }

        private SelectedChangedDel selectionChanged;
        public event SelectedChangedDel SelectionChanged
        {
            add { selectionChanged += value; }
            remove { selectionChanged -= value; }
        }

        // obsolete, use universal ColumnSettingsChanged
        private SortOrderChangedDel sortOrderChanged;
        public event SortOrderChangedDel SortOrderChanged
        {
            add { sortOrderChanged += value; }
            remove { sortOrderChanged -= value; }
        }

        private ColumnSettingsChangedDel columnSettingsChanged;
        public event ColumnSettingsChangedDel ColumnSettingsChanged
        {
            add { columnSettingsChanged += value; }
            remove { columnSettingsChanged -= value; }
        }

        #region Mouse handlers
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (columns == null)
                return;
            if (e.Button == MouseButtons.Left && IsInHeader(e.X, e.Y))
            {
                var colIndex = GetResizeSplitterIndex(e.X, e.Y);
                // start dragging column header
                if (colIndex == -1)
                {
                    startDragCoords = e.Location;
                    dragStarted = false;
                    dragCoords = null;
                }
                // start resizing column
                else
                {
                    resizingColumnIndex = colIndex;
                    startResizingCoords = e.Location;
                    //lastResizingCoords = e.Location;
                    resizingColumnStartWidth = columns[colIndex].ResultedWidth;
                }
            }
            if (e.Button == MouseButtons.Right)
            {
                bool inHeader;
                var coords = GetCellUnderCursor(e.X, e.Y, out inHeader);
                if (coords.HasValue && contextMenuRequested != null)
                    contextMenuRequested(this, e, coords.Value.Y, columns[coords.Value.X]);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (columns == null) return;
            if (dragCoords.HasValue && startDragCoords.HasValue)
            {
                bool srcInHeader, destInHeader;
                // what to move
                var srcPos = GetCellUnderCursor(startDragCoords.Value.X, startDragCoords.Value.Y, out srcInHeader);
                // where to insert
                var destPos = GetCellUnderCursor(dragCoords.Value.X, dragCoords.Value.Y, out destInHeader);
                if (srcPos.HasValue && srcInHeader/* && destInHeader*/)
                {
                    var insertIndex = -1;
                    if (destPos.HasValue)
                    {
                        var columnPosition = GetColumnPosition(destPos.Value.X);
                        if (columnPosition.HasValue)
                        {
                            var columnUnserCursor = columns[destPos.Value.X];
                            insertIndex = destPos.Value.X;
                            if (dragCoords.Value.X - columnPosition.Value > columnUnserCursor.ResultedWidth / 2)
                                insertIndex++;
                        }
                    }
                    else
                    {
                        var firstColumnPosition = GetColumnPosition(0);
                        if (firstColumnPosition.HasValue && dragCoords.Value.X < firstColumnPosition.Value)
                            insertIndex = 0;
                        else
                        {
                            var lastColumnPosition = GetColumnPosition(columns.Count - 1);
                            if (lastColumnPosition.HasValue && dragCoords.Value.X > lastColumnPosition.Value)
                                insertIndex = columns.Count;
                        }
                    }
                    // column moving
                    if (insertIndex >= 0)
                    {
                        MoveColumn(srcPos.Value.X, insertIndex);
                        if (columnSettingsChanged != null)
                            columnSettingsChanged();
                    }
                }
                startDragCoords = null;
                dragCoords = null;
                Invalidate();
                return;
            }
            startDragCoords = null;
            // dragging started but ignored by threshold moving
            if (dragStarted)
            {
                dragStarted = false;
                return;
            }
            // resize column complete
            if (resizingColumnIndex != -1)
            {
                if (columnSettingsChanged != null)
                    columnSettingsChanged();
                resizingColumnIndex = -1;
                return;
            }
            // check mouse hit cell's header
            if (CheckMouseUpInHeader(e)) return;
            if (CheckMouseHitCell(e)) return;
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (columns == null) return;
            if (CheckMouseHitCell(e)) return;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (columns == null) return;
            // dragging column header
            if (startDragCoords.HasValue)
            {
                // dragging threshold for ignoring minor moving
                const int dragThreshold = 5;
                if (Math.Abs(e.Location.X - startDragCoords.Value.X) > dragThreshold)
                {
                    dragStarted = true;
                    dragCoords = e.Location;
                }
                else
                    dragCoords = null;
                Invalidate();
            }
            // resizing column
            else if (resizingColumnIndex != -1)
            {
                var minWidth = GetMinWidth(resizingColumnIndex);
                var newWidth = resizingColumnStartWidth + e.X - startResizingCoords.X;
                if (newWidth < minWidth)
                    newWidth = minWidth;
                columns[resizingColumnIndex].ResultedWidth = newWidth;
                CheckSize();
                Invalidate();
            }
            // changing mouse cursor for resizing
            else
            {
                if (IsInHeader(e.X, e.Y))
                    Cursor = GetResizeSplitterIndex(e.X, e.Y) != -1 ? Cursors.VSplit : Cursors.Default;
                LastHoveredCell = GetCellUnderCursor(e.X, e.Y);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            LastHoveredCell = null;
            base.OnMouseLeave(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                // scroll up-down
                var delta = e.Delta > 0 ? -1 : 1;
                if (ScrollUpDown(delta))
                    Invalidate();
            }
            base.OnMouseWheel(e);
        }
        #endregion

        #region Check mouse hit header
        private bool CheckMouseUpInHeader(MouseEventArgs e)
        {
            if (columns.Count == 0) return false;
            // check Y-coord
            if (e.Y < 0 || e.Y >= CaptionHeight) return false;
            // check specific column
            var left = -scrollBarH.Value;
            foreach (var col in columns)
            {
                var right = left + col.ResultedWidth;
                if (e.X >= left && e.X < right)
                {// do hit column
                    ProcessMouseUpInHeader(e, col);
                    return true;
                }
                left = right;
            }
            return false;
        }

        protected virtual void ProcessMouseUpInHeader(MouseEventArgs e, FastColumn col)
        {
            // change sort mode by left mouse button
            if (e.Button == MouseButtons.Left)
            {
                var oldOrder = col.SortOrder;
                col.SortOrder = col.SortOrder == FastColumnSort.None
                                    ? FastColumnSort.Ascending
                                    : col.SortOrder == FastColumnSort.Ascending
                                            ? FastColumnSort.Descending
                                            : FastColumnSort.None;
                if (sortOrderChanged != null)
                    sortOrderChanged(col, oldOrder, col.SortOrder);
                if (columnSettingsChanged != null)
                    columnSettingsChanged();
                OrderRows();
                Invalidate();
            }
        }
        #endregion

        #region Hit cell
        
        public Point GetCellCoords(FastColumn column, int rowIndex)
        {
            var shiftX = scrollBarH.Visible ? scrollBarH.Value : 0;
            var startIndex = scrollBarV.Value >= rows.Count ? 0 : scrollBarV.Value;

            var popTop = CellHeight * (rowIndex - startIndex) + /*Top +*/ CaptionHeight + 1;
            var popLeft = 1 - shiftX + Columns.TakeWhile(col => col != column).Sum(col => col.ResultedWidth);

            return new Point(popLeft, popTop);
        }
        
        private bool CheckMouseHitCell(MouseEventArgs e)
        {
            var cell = GetCellUnderCursor(e.X, e.Y);
            if (!cell.HasValue)
                return false;

            if (rows[cell.Value.Y].IsGroupingRow)
            {
                var row = rows[cell.Value.Y];
                if (row.Collapsed)
                {
                    var movedRows = collapsedRows.Where(r => r.OwnerGroupingRow == row).ToList();
                    collapsedRows.RemoveAll(movedRows.Contains);
                    rows.InsertRange(rows.IndexOf(row) + 1, movedRows);
                    row.Collapsed = false;
                }
                else
                {
                    var movedRows = rows.Where(r => r.OwnerGroupingRow == row).ToList();
                    rows.RemoveAll(movedRows.Contains);
                    collapsedRows.AddRange(movedRows);
                    row.Collapsed = true;
                }
                Invalidate();
                return true;
            }

            // process selection
            // lastHitRow & lastHitColumn store previous selection 4 correct selection
            ProcessMouseHitCell(columns[cell.Value.X], cell.Value.Y, e);

            // store cell hit
            lastHitRow = cell.Value.Y;
            lastHitColumn = columns[cell.Value.X];

            // edit value
            if (lastHitColumn.IsEditable)
            {
                if ((CellEditMode == CellEditModeTrigger.LeftClick && e.Button == MouseButtons.Left) ||
                    (CellEditMode == CellEditModeTrigger.LeftDoubleClick && e.Clicks > 1 &&
                     e.Button == MouseButtons.Left) ||
                    (CellEditMode == CellEditModeTrigger.RightClick && e.Button == MouseButtons.Right))
                {
                    var allow = true;
                    if (fieldValueBeforeEdit != null)
                        allow = fieldValueBeforeEdit(lastHitColumn, lastHitRow,
                                                     rows[lastHitRow].cells[columns.IndexOf(lastHitColumn)]);
                    if (allow)
                    {
                        ShowEditField(cell.Value);
                        return true;
                    }
                }
            }

            // fire the event
            if (userHitCell != null)
                userHitCell(this, e, lastHitRow, lastHitColumn);
            return true;
        }

        private void ShowEditField(Point cellCoords)
        {
            var objRow = rows[lastHitRow];
            var objIndex = columns.IndexOf(lastHitColumn);

            var shiftX = scrollBarH.Visible ? scrollBarH.Value : 0;
            var startIndex = scrollBarV.Value >= rows.Count ? 0 : scrollBarV.Value;
            
            var popTop = CellHeight * (cellCoords.Y - startIndex) + /*Top +*/ CaptionHeight + 1;
            var popLeft = /*Left + */1 - shiftX;
            
            foreach (var col in columns)
            {
                if (col == lastHitColumn) break;
                popLeft += col.ResultedWidth;
            }

            // в зависимости от типа значения показать определенный диалог
            var targetType = columnProperty[lastHitColumn].PropertyType;
            if (targetType == typeof(bool) || targetType.IsSubclassOf(typeof(Enum)))
                ShowMenuStripField(popTop, popLeft, objIndex, objRow);
            else            
                ShowTextBoxField(popTop, popLeft, objIndex, objRow);
        }

        private void ShowMenuStripField(int popTop, int popLeft, int objIndex, FastRow objRow)
        {
            var pop = new PopupListBox(objRow.cells[objIndex].CellValue,
                                       lastHitRow,
                                       lastHitColumn,
                                       columnProperty[lastHitColumn].PropertyType, OnValueEntered, this);
            pop.ShowOptions(popLeft, popTop);
        }

        private void ShowTextBoxField(int popTop, int popLeft, int objIndex, FastRow objRow)
        {
            var pop = new PopupTextBox(objRow.cells[objIndex].CellValue,
                                       lastHitColumn.ResultedWidth, CellHeight,
                                       lastHitRow,
                                       lastHitColumn,
                                       columnProperty[lastHitColumn], OnValueEntered);
            pop.Show(this, popLeft, popTop);
        }

        private void OnValueEntered(FastColumn col, int rowIndex, object newValue)
        {
            var valueObj = rows[rowIndex].ValueObject;

            if (fieldValueChanging != null)
            {
                bool cancel;
                fieldValueChanging(col, rowIndex, valueObj, ref newValue, out cancel);
                if (cancel) return;
            }

            // изменить значение ячейки
            try
            {
                var colPropType = columnProperty[col];

                // если null...
                if (newValue == null || (newValue is string && (string) newValue == ""))
                {
                    if (!colPropType.PropertyType.IsValueType ||
                        colPropType.PropertyType == typeof (string))
                    {
                        colPropType.SetValue(valueObj, null, null);
                        if (fieldValueChanged != null)
                            fieldValueChanged(col, rowIndex, valueObj);
                    }
                }
                else if (colPropType.PropertyType == newValue.GetType())
                {
                    colPropType.SetValue(valueObj, newValue, null);
                    if (fieldValueChanged != null)
                        fieldValueChanged(col, rowIndex, valueObj);
                }
                else
                    return;
            }
            catch
            {
                return;
            }

            // обновить ячейку
            UpdateRow(rowIndex, valueObj);
            // перерисовать
            InvalidateCell(col, rowIndex);
        }

        // changing selection
        protected virtual void ProcessMouseHitCell(FastColumn col,
            int rowIndex, MouseEventArgs e)
        {
            // do not select string by hitting hyperlink
            if (col.IsHyperlinkStyleColumn) return;

            // select cell
            if (SelectEnabled && e.Button == MouseButtons.Left)
            {
                var singleSelectMode = true;
                if (MultiSelectEnabled)
                    singleSelectMode =
                        ModifierKeys != Keys.Shift && ModifierKeys != Keys.Control;
                // single line select
                if (singleSelectMode)
                {
                    // deselect all but current
                    for (var i = 0; i < rows.Count; i++)
                        rows[i].Selected = i == rowIndex;
                }
                // multi-line select
                else
                {
                    if (ModifierKeys == Keys.Control)
                    {// add or remove selected
                        rows[rowIndex].Selected = !rows[rowIndex].Selected;
                    }
                    if (ModifierKeys == Keys.Shift)
                    {
                        var beginSelectionIndex = Math.Min(rowIndex, lastHitRow);
                        if (beginSelectionIndex == -1)
                            beginSelectionIndex = rowIndex;
                        var endSelectionIndex = Math.Max(rowIndex, lastHitRow);
                        for (var i = beginSelectionIndex; i <= endSelectionIndex; i++)
                            rows[i].Selected = true;
                    }
                }
                
                if (selectionChanged != null)
                    selectionChanged(e, rowIndex, col);
            }
            // redraw
            Invalidate();
        }
        #endregion

        #region Calc cell
        public int? GetColumnPosition(int index)
        {
            var result = -scrollBarH.Value;
            for (var colIndex = 0; colIndex < columns.Count; colIndex++)
            {
                if (colIndex == index)
                    return result;
                var column = columns[colIndex];
                if (!column.Visible)
                    continue;
                result += column.ResultedWidth;
            }
            return null;
        }

        public bool IsInHeader(int x, int y)
        {
            if (y < 0 || y > CaptionHeight)
                return false;
            if (y <= CaptionHeight)
                return true;
            return false;
        }

        /// <summary>
        /// определение координаты ячейки по экранным
        /// </summary>
        /// <param name="x">X-coordinate in pixels</param>
        /// <param name="y">Y-coordinate in pixels</param>
        /// <param name="inHeader">header cell flag</param>
        /// <returns>Point.X is a column index, Point.Y is a row index</returns>
        public Point? GetCellUnderCursor(int x, int y, out bool inHeader)
        {
            inHeader = IsInHeader(x, y);
            var result = GetCellUnderCursor(x, y);
            if (result != null)
                return result; // мы в области данных
            if (!inHeader)
                return null; // мы за пределами таблицы
            x += scrollBarH.Value;
            // обрабатываем заголовок
            var left = 0;
            for (var colIndex = 0; colIndex < columns.Count; colIndex++)
            {
                var col = columns[colIndex];
                var right = left + col.ResultedWidth;
                if (x >= left && x < right)
                    return new Point(colIndex, -1);
                left = right;
            }
            return null;
        }

        public Point? GetCellUnderCursor(int x, int y)
        {
            if (rows.Count == 0)
                return null;
            // check Y-coord
            if (IsInHeader(x, y))
                return null;
            x += scrollBarH.Value;
            // check hit column
            FastColumn column = null;
            var left = 0;
            var colIndex = 0;
            foreach (var col in columns)
            {
                var right = left + col.ResultedWidth;
                if (x >= left && x < right)
                {
                    // do hit
                    column = col;
                    break;
                }
                left = right;
                colIndex++;
            }
            if (column == null) return null;
            // check row hit 
            var startRow = scrollBarV.Value >= rows.Count ? 0 : scrollBarV.Value;
            var cellY = y - CaptionHeight;
            var cellIndex = cellY / CellHeight;
            cellIndex += startRow;
            if (cellIndex >= rows.Count) return null;
            return new Point(colIndex, cellIndex);
        }

        private int GetResizeSplitterIndex(int x, int y)
        {
            if (!IsInHeader(x, y))
                return -1;
            // запрещено изменять при автоподгоне размеров колонок с автошириной
            // из-за неустраненных глюков с автоподгоном
            if (fitWidth)
                return -1;
            var shiftX = scrollBarH.Visible ? scrollBarH.Value : 0;
            var posX = -shiftX;
            const int area = 5;
            for (var index = 0; index < columns.Count; index++)
            {
                var column = columns[index];
                if (!column.Visible)
                    continue;
                posX += column.ResultedWidth;
                if (column.ColumnWidth > 0)
                    continue;
                if (x >= posX - area && x <= posX + area)
                    return index;
                if (posX > x)
                    break;
            }
            return -1;
        }
        #endregion
    }

    public delegate void SortOrderChangedDel(FastColumn col, FastColumnSort oldOrder, FastColumnSort newOrder);

    public delegate void ColumnSettingsChangedDel();

    public delegate void UserHitCellDel(object sender, MouseEventArgs e, int rowIndex, FastColumn col);

    public delegate void SelectedChangedDel(MouseEventArgs e, int rowIndex, FastColumn col);
}
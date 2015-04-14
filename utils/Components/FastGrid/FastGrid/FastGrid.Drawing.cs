using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace FastGrid
{
    public partial class FastGrid
    {
        #region Методы - рисование

        protected override void OnPaint(PaintEventArgs e)
        {
            // horz scrolling
            var shiftX = scrollBarH.Value;
            // painting
            using (var brushColl = new BrushesStorage())
            using (var penUp = new Pen(colorCellOutlineUpper))
            using (var penDn = new Pen(colorCellOutlineLower))
            {
                var brushFillUnSel = brushColl.GetBrush(ColorCellBackground);
                var brushFillAlt = brushColl.GetBrush(ColorAltCellBackground);
                var brushFillSel = brushColl.GetBrush(ColorSelectedCellBackground);
                var brushFillAnchor = brushColl.GetBrush(ColorAnchorCellBackground);
                var brushFont = brushColl.GetBrush(ColorCellFont);
                var brushFontSel = brushColl.GetBrush(ColorSelectedCellFont);
                
                // draw column title
                var pointLeft = new Point(-shiftX, 0);
                var totalWidth = 0;
                foreach (var column in columns)
                {
                    if (!column.Visible)
                        continue;
                    // draw the cell
                    column.Draw(e.Graphics, FontHeader ?? Font, CaptionHeight,
                                column.ResultedWidth, pointLeft, penUp, penDn, brushColl);
                    pointLeft = new Point(pointLeft.X + column.ResultedWidth, pointLeft.Y);
                    totalWidth += column.ResultedWidth;
                }

                // draw strings
                var fontCell = FontCell ?? Font;
                var leftTop = new Point(-shiftX, CaptionHeight + 1);
                // first displayed row index
                var startIndex = scrollBarV.Value >= rows.Count ? 0 : scrollBarV.Value;
                for (var i = startIndex; i < rows.Count; i++)
                {
                    var row = rows[i];
                    var isAlt = (i & 1) != 0;
                    var brushBack = row.Selected ? brushFillSel 
                        : row.anchor != FastRow.AnchorPosition.NoAnchor ? brushFillAnchor
                        : isAlt ? brushFillAlt : brushFillUnSel;
                    var brFont = row.Selected ? brushFontSel : brushFont;
                    var font = (row.Selected && FontSelectedCell != null) ? FontSelectedCell :
                        (row.anchor != FastRow.AnchorPosition.NoAnchor && FontAnchoredRow != null)
                        ? FontAnchoredRow : fontCell;

                    // draw horz cells margins
                    e.Graphics.DrawLine(penUp, leftTop.X, leftTop.Y, leftTop.X + totalWidth, leftTop.Y);
                    e.Graphics.DrawLine(penDn, leftTop.X, leftTop.Y + 1, leftTop.X + totalWidth, leftTop.Y + 1);

                    row.DrawRow(e.Graphics,
                        i,
                        leftTop,
                        CellHeight,
                        columns,
                        penUp,
                        penDn,
                        brushBack,
                        brFont,
                        font,
                        CellPadding, brushColl);
                    leftTop = new Point(leftTop.X, leftTop.Y + CellHeight);
                    if (leftTop.Y >= Height) break;
                }

                // draw dragging
                if (dragCoords.HasValue)
                {
                    // dragging column
                    bool inHeader;
                    var draggingColumnPos = GetCellUnderCursor(startDragCoords.Value.X, startDragCoords.Value.Y, out inHeader);
                    if (draggingColumnPos.HasValue && inHeader)
                    {
                        // drop position
                        var columnUnderCursorPos = GetCellUnderCursor(dragCoords.Value.X, dragCoords.Value.Y, out inHeader);
                        var draggingColumn = columns[draggingColumnPos.Value.X];
                        int? columnPosition = null;
                        if (columnUnderCursorPos.HasValue) // drag to another column
                        {
                            var columnUnderCursor = columns[columnUnderCursorPos.Value.X];
                            if(columnUnderCursor != draggingColumn)
                                columnPosition = GetColumnPosition(columnUnderCursorPos.Value.X); // block dragging to itself
                            if (columnPosition.HasValue &&
                                dragCoords.Value.X - columnPosition.Value > columnUnderCursor.ResultedWidth / 2) // right part of header
                            {
                                columnPosition = columnPosition.Value + columnUnderCursor.ResultedWidth;
                                // block dragging
                                var nextColumn = columns.Find(c => c.Visible && columns.IndexOf(c) > columnUnderCursorPos.Value.X);
                                if (nextColumn == draggingColumn)
                                    columnPosition = null;
                            }
                            else // left part of header
                            {
                                // block dragging
                                var prevColumn = columns.FindLast(c => c.Visible && columns.IndexOf(c) < columnUnderCursorPos.Value.X);
                                if (prevColumn == draggingColumn)
                                    columnPosition = null;
                            }
                        }
                        else
                        {
                            var firstColumnPosition = GetColumnPosition(0);
                            if (firstColumnPosition.HasValue && dragCoords.Value.X < firstColumnPosition.Value) // drag to the left from the first column
                            {
                                var firstColumn = columns.FirstOrDefault(c => c.Visible);
                                if (firstColumn != draggingColumn)
                                    columnPosition = firstColumnPosition.Value;
                            }
                            else // drag to the right from the last column
                            {
                                var lastColumnPosition = GetColumnPosition(columns.Count - 1);
                                if (lastColumnPosition.HasValue && dragCoords.Value.X > lastColumnPosition.Value)
                                {
                                    var lastColumn = columns.LastOrDefault(c => c.Visible);
                                    if (lastColumn != draggingColumn)
                                        columnPosition = lastColumnPosition.Value + lastColumn.ResultedWidth;
                                }
                            }
                        }
                        if (columnPosition.HasValue)
                            e.Graphics.FillRectangle(new SolidBrush(Color.Black), columnPosition.Value - 1, 0, 3, Height);
                        columnPosition = GetColumnPosition(draggingColumnPos.Value.X);
                        if (columnPosition.HasValue)
                        {
                            brushColl.GetBrush(draggingColumn.ColorFillUpper);
                            brushColl.GetBrush(draggingColumn.ColorFillLower);
                            brushColl.SetAlpha(192);
                            e.Graphics.CompositingMode = CompositingMode.SourceOver;
                            draggingColumn.Draw(e.Graphics, FontHeader ?? Font, CaptionHeight,
                                        draggingColumn.ResultedWidth,
                                        new Point(
                                            columnPosition.Value + dragCoords.Value.X - startDragCoords.Value.X, 0),
                                        penUp, penDn, brushColl);
                            e.Graphics.CompositingMode = CompositingMode.SourceCopy;
                            brushColl.SetAlpha(0);
                        }
                    }
                }
            }

            if (scrollBarV.Visible)
            {
                var totalHeight = CellHeight * rows.Count + CaptionHeight + 1;
                var itemsLeft = (int)Math.Ceiling((totalHeight - Height) / (double)CellHeight);
                RecalcScrollBounds(itemsLeft);
            }
        }

        #endregion

        #region Методы - корректировка размеров

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CheckSize();
            Invalidate();
        }

        private int GetMinWidth(bool recalcMinWidth = false)
        {
            var result = 0;
            for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
            {
                var minWidth = GetMinWidth(columnIndex, recalcMinWidth);
                result += minWidth > 0 ? minWidth : 0;
            }
            return result;
        }

        private int GetMinWidth(int columnIndex, bool recalcMinWidth = false)
        {
            var column = columns[columnIndex];
            if (!column.Visible)
                return 0;
            if (column.ColumnWidth > 0)
                return column.ColumnWidth;
            var extraSpace = cellPadding * 2;
            if (recalcMinWidth)
            {
                var textWidth = TextRenderer.MeasureText(column.Title, FontHeader ?? Font).Width + extraSpace;
                for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
                {
                    var s = TextRenderer.MeasureText(rows[rowIndex].cells[columnIndex].CellString, Font);
                    if (s.Width + extraSpace > textWidth)
                        textWidth = s.Width + extraSpace;
                }
                column.ColumnMinWidth = textWidth;
            }
            return column.ColumnMinWidth;
        }

        /// <summary>
        /// актуализация размеров всех видимых колонок таблицы
        /// вызов необходим после изменения флагов видимости
        /// </summary>
        /// <param name="recalcMinWidths">установить ColumnMinWidth</param>
        public void CheckSize(bool recalcMinWidths = false)
        {
            if (Width == 0 || Height == 0) // невидимая таблица
                return;

            // scrollBarV
            var totalHeight = CellHeight * rows.Count + CaptionHeight + 1;
            var scrollBarVVisible = totalHeight > Height;
            scrollBarV.Visible = scrollBarVVisible;
            if (scrollBarVVisible)
            {
                scrollBarV.SetBounds(Width - scrollBarV.Width, CaptionHeight,
                                     scrollBarV.Width, Height - CaptionHeight);
            }
            else
            {
                scrollPanel.Visible = false;
                scrollBarV.Value = 0;
            }

            // columns
            var visibleColumns = columns.Where(c => c.Visible).ToList();
            var minWidth = GetMinWidth(recalcMinWidths);
            /*if (visibleColumns.Any(c => c.ColumnWidth <= 0 && c.ColumnMinWidth <= 0)) // force ColumnMinWidth set
            {
                minWidth = GetMinWidth(true);
                Console.WriteLine("{0}.CheckSize: invalid use of FastGrid: both ColumnWidth and ColumnMinWidth not set for one of columns; call CheckSize(true) explicitly", Name);
            }*/
            // ensure ResultedWidth set
            foreach (var column in visibleColumns)
            {
                if (!column.Visible)
                    continue;
                if (column.ColumnWidth > 0) // for columns with fixed width
                    column.ResultedWidth = column.ColumnWidth;
                else if(column.ColumnMinWidth > 0 && column.ResultedWidth < column.ColumnMinWidth) // for sizable columns
                    column.ResultedWidth = column.ColumnMinWidth;
            }
            // autosize
            var totalWidth = visibleColumns.Sum(c => c.ResultedWidth);
            if (totalWidth <= 0) // all column autosizable & ResultedWidth not set
                totalWidth = minWidth;
            var wdFree = Width - totalWidth - (scrollBarVVisible ? scrollBarV.Width : 0);
            /*if (wdFree < 0)
                wdFree = 0;*/
            var autosizableColumns = visibleColumns.Where(c => c.ColumnWidth <= 0).ToList();
            var recalcRel = autosizableColumns.Any(c => c.RelativeWidth <= 0); // if unaccounted columns exist
            if (!recalcRel)
            {
                var sum = autosizableColumns.Sum(c => c.RelativeWidth);
                if ((sum > 1.01) || (sum < 0.99))
                    recalcRel = true; // percentage sum is invalid 
            }
            // recalc RelativeWidth for sizable columns
            if (recalcRel)
            {
                // force relative widths reset
                foreach (var column in autosizableColumns)
                    column.RelativeWidth = 1.0 / autosizableColumns.Count;
            }
            // recalc ResultedWidth for sizable columns
            // totalWidth == 0 - probably, it's the first call of CheckSize
            if (recalcRel || fitWidth || (totalWidth == 0))
            {
                foreach (var column in autosizableColumns)
                {
                    column.ResultedWidth += (int) (wdFree * column.RelativeWidth);
                    if (column.ResultedWidth < column.ColumnMinWidth)
                        column.ResultedWidth = column.ColumnMinWidth;
                }
            }

            // scrollBarH
            totalWidth = visibleColumns.Sum(c => c.ResultedWidth);
            var scrollBarHVisible = totalWidth > Width - (scrollBarVVisible ? scrollBarV.Width : 0);
            scrollBarH.Visible = scrollBarHVisible;
            if (scrollBarHVisible)
            {
                scrollBarH.SetBounds(0, Height - scrollBarH.Height,
                                     Width - (scrollBarVVisible ? scrollBarV.Width : 0), scrollBarH.Height);
                scrollBarH.Maximum = totalWidth;
                try
                {
                    scrollBarH.LargeChange = Width - (scrollBarVVisible ? scrollBarV.Width : 0);
                }
                catch
                {
                }
                if (scrollBarVVisible)
                {
                    scrollBarV.SetBounds(Width - scrollBarV.Width, CaptionHeight,
                                         scrollBarV.Width, Height - CaptionHeight - scrollBarH.Height);
                    scrollPanel.Visible = true;
                    scrollPanel.SetBounds(scrollBarV.Left, scrollBarV.Bottom, scrollBarV.Width, scrollBarH.Height);
                }
            }
            else
            {
                scrollPanel.Visible = false;
                scrollBarH.Value = 0;
            }
        }

        public void InvalidateColumns(int startColumn, int endColumn)
        {
            // get refreshing cells coords
            var top = CaptionHeight + 1;
            var bottom = Height - 1;

            var left = 0;
            var x = -scrollBarH.Value;
            for (var i = 0; i < columns.Count; i++)
            {
                if (i == startColumn) left = x;
                x += columns[i].ResultedWidth;
                if (i == endColumn) break;
            }
            var right = x;
            Invalidate(new Rectangle(left, top, right - left, bottom - top));
        }

        public void InvalidateRow(int row)
        {
            var startRow = scrollBarV.Value >= rows.Count ? 0 : scrollBarV.Value;
            var y = CaptionHeight + CellHeight * (row - startRow);
            Invalidate(new Rectangle(-scrollBarH.Value, y, Width, CellHeight));
        }

        public void InvalidateCell(FastColumn col, int row)
        {
            if (!col.Visible) return;
            InvalidateCell(columns.IndexOf(col), row);
        }

        public void InvalidateCell(int col, int row)
        {
            if (col < 0 || col >= columns.Count || row < 0 || row >= rows.Count) return;

            // find column's coords
            var x = -scrollBarH.Value;
            for (var i = 0; i < col; i++)
            {
                if (!columns[i].Visible) continue;
                x += columns[i].ResultedWidth;
            }
            
            // find row's coords
            var startRow = scrollBarV.Value >= rows.Count ? 0 : scrollBarV.Value;
            var y = CaptionHeight + CellHeight * (row - startRow);            

            // redraw
            Invalidate(new Rectangle(x, y, columns[col].ResultedWidth, CellHeight));
        }
        
        #endregion
    }
}
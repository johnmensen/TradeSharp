using System;
using System.Collections.Generic;
using System.Linq;

namespace FastGrid
{
    /// <summary>
    /// provides default comparing methods for ordering rows
    /// </summary>
    public class CellComparer : IComparer<FastRow>
    {
        public readonly List<FastColumn> columns;
        
        public CellComparer(List<FastColumn> columns)
        {
            this.columns = columns;
        }

        public bool HasSortableColumns
        {
            get
            {
                return columns.Any(col => col.SortOrder != FastColumnSort.None);
            }
        }

        public int Compare(FastRow rowA, FastRow rowB)
        {
            if (rowA.anchor != rowB.anchor)
            {
                // check anchored strings
                if (rowA.anchor == FastRow.AnchorPosition.AnchorTop)
                    return -1;
                if (rowA.anchor == FastRow.AnchorPosition.AnchorBottom)
                    return 1;
                return rowB.anchor == FastRow.AnchorPosition.AnchorTop ? 1 : -1;
            }

            // compare values if possible
            // overwise - compare string representation
            var weight = 1;
            var order = 0;
            for (var i = columns.Count - 1; i >= 0; i--)
            {
                var col = columns[i];
                if (!col.Visible) continue;
                if (col.SortOrder == FastColumnSort.None) continue;
                var sign = col.SortOrder == FastColumnSort.Ascending ? 1 : -1;

                var cellA = rowA.cells[i].CellValue;
                var cellB = rowB.cells[i].CellValue;

                if (cellA != null && cellB != null)
                {
                    // have both values
                    if (cellA is IComparable && cellB is IComparable)
                        order += ((IComparable)cellA).CompareTo(cellB) * weight * sign;
                    else
                        order += rowA.cells[i].CellString.CompareTo(rowB.cells[i].CellString) * sign;
                }
                else
                {
                    // have just one value - non-null values is always greater than null
                    if (cellA == null && cellB != null) order += weight*sign;
                    else if (cellA != null && cellB == null) order -= weight*sign;
                }
                weight *= 2;
            }

            return Math.Sign(order);
        }
    }
}

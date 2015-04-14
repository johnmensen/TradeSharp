using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace FastGrid
{
    /// <summary>
    /// типизированная обертка вокруг таблицы
    /// тип T - тип данных, хранящихся в строке
    /// </summary>
    public class FastGridTyped<T>
    {
        public FastGrid grid;

        public delegate void UserHitCellTypedDel(object sender, MouseEventArgs e, int rowIndex, FastColumn col, T rowObject);

        private UserHitCellTypedDel userHitCell;
        public event UserHitCellTypedDel UserHitCell
        {
            add { userHitCell += value; }
            remove { userHitCell -= value; }
        }

        public FastGridTyped(FastGrid grid)
        {
            this.grid = grid;
            grid.UserHitCell += (sender, args, index, col) =>
                {
                    if (userHitCell != null)
                        userHitCell(sender, args, index, col, (T) grid.rows[index].ValueObject);
                };
        }

        public IEnumerable<T> GetBoundValues()
        {
            return grid.rows.Select(r => (T) r.ValueObject);
        }

        public IEnumerable<T> GetBoundValuesFromSelectedRows()
        {
            return grid.rows.Where(r => r.Selected).Select(r => (T)r.ValueObject);
        }

        public IEnumerable<T> GetBoundValues(bool? rowSelectFilter)
        {
            return rowSelectFilter.HasValue 
                ? grid.rows.Where(r => r.Selected == rowSelectFilter.Value).Select(r => (T)r.ValueObject)
                : grid.rows.Select(r => (T)r.ValueObject);
        }

        public void UpdateRow(int rowIndex, T val)
        {
            grid.UpdateRow(rowIndex, val);            
        }

        public T GetRowObject(int rowIndex)
        {
            return (T) grid.rows[rowIndex].ValueObject;
        }

        public T this[int key]
        {
            get
            {
                return GetRowObject(key);
            }
            set
            {
                UpdateRow(key, value);
            }
        }
    }
}

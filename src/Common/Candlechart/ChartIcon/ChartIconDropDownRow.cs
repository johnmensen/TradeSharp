using System.Drawing;
using System.Linq;

namespace Candlechart.ChartIcon
{
    public interface IChartIconDropDownRow
    {
        ChartIconDropDownCell[] GetCells();
    }

    public class ChartIconDropDownRowCells
    {
        public ChartIconDropDownCell[] cells;

        public enum RowType { Row = 0, ArrowUp, ArrowDown }

        public RowType TypeOfRow { get; set; }

        public override string ToString()
        {
            if (TypeOfRow == RowType.Row)
                return string.Join(", ", cells.Select(c => c.CellValue));
            return TypeOfRow == RowType.ArrowUp ? "up arrow" : "down arrow";
        }

        public ChartIconDropDownRowCells()
        {
        }

        public ChartIconDropDownRowCells(RowType typeOfRow)
        {
            TypeOfRow = typeOfRow;
        }

        public ChartIconDropDownRowCells(object valueObject, DropDownList.FormatValueDel formatter)
        {
            TypeOfRow = RowType.Row;
            if (valueObject is IChartIconDropDownRow)
            {
                cells = ((IChartIconDropDownRow) valueObject).GetCells();
                return;
            }
            cells = new[]
                {
                    new ChartIconDropDownCell
                        {
                            CellString = formatter(valueObject),
                            CellValue = valueObject
                        }
                };
        }
    }

    public class ChartIconDropDownCell
    {
        public object CellValue { get; set; }

        public string CellString { get; set; }

        public Color? ColorFont { get; set; }

        public Color? ColorBack { get; set; }

        public FontStyle? FontStyle { get; set; }
    }
}

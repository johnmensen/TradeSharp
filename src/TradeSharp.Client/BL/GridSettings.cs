using System;
using System.Collections.Generic;
using Entity;
using FastGrid;
using System.Linq;
using TradeSharp.Util;

namespace TradeSharp.Client.BL
{
    /// <summary>
    /// настройки грида со сделками, отложенными ордерами либо чем-то еще
    /// </summary>
    public class GridSettings
    {
        public const string ListAccountOpenedOrders = "OPORD";
        public const string ListAccountClosedOrders = "CLORD";
        public const string ListAccountPendingOrders = "PDORD";
        public const string ListAccountSummaryOrders = "SMORD";

        [PropertyXMLTag("GridCode")]
        public string GridCode { get; set; }

        // в качестве идентификаторов (тут они по старому именуются как Titles) используются значения св-ва PropertyName
        [PropertyXMLTag("Titles")]
        public string TitlesString { get; set; }

        [PropertyXMLTag("SortOrderString")]
        public string SortOrdersString { get; set; }

        [PropertyXMLTag("RelativeColumnsWidthString")]
        public string RelativeColumnsWidthString { get; set; }

        public List<string> Titles
        {
            get
            {
                if (string.IsNullOrEmpty(TitlesString))
                    return new List<string>();
                return TitlesString.Split(new[] { ';' }).ToList();
            }
            set
            {
                if (value == null)
                    TitlesString = "";
                else
                    TitlesString = string.Join(";", value);
            }
        }

        public List<FastColumnSort> SortOrders
        {
            get
            {
                var sortOrders = new List<FastColumnSort>();
                if (string.IsNullOrEmpty(SortOrdersString))
                    return sortOrders;
                foreach (var letter in SortOrdersString)
                {
                    if (letter == '1')
                        sortOrders.Add(FastColumnSort.Ascending);
                    else if (letter == '2')
                        sortOrders.Add(FastColumnSort.Descending);
                    else
                        sortOrders.Add(FastColumnSort.None);
                }
                return sortOrders;
            }
            set
            {
                if (value == null || value.Count == 0)
                {
                    SortOrdersString = "";
                    return;
                }
                SortOrdersString = string.Join("",
                    value.Select(c => c == FastColumnSort.None ? "0" : c == FastColumnSort.Ascending ? "1" : "2"));
            }
        }

        public List<double> RelativeColumnsWidth
        {
            get
            {
                var relativeColumnsWidth = new List<double>();
                if (string.IsNullOrEmpty(RelativeColumnsWidthString))
                    return relativeColumnsWidth;
                var splitChars = new [] { ';' };
                relativeColumnsWidth.AddRange(RelativeColumnsWidthString.Split(splitChars).ToList().Select(str => 
                    str.Replace(",", ".").ToDoubleUniformSafe() ?? 1d));
                return relativeColumnsWidth;
            }
            set
            {
                if (value == null || value.Count == 0)
                {
                    RelativeColumnsWidthString = "";
                    return;
                }
                RelativeColumnsWidthString = string.Join(";", value.Select(w =>  w.ToString("F2")));
            }
        }

        public static GridSettings EnsureSettings(string gridCode)
        {
            var list = UserSettings.Instance.GridList;
            var sets = list.FirstOrDefault(l => l.GridCode == gridCode);
            if (sets == null)
            {
                sets = new GridSettings { GridCode = gridCode };
                list.Add(sets);
            }
            return sets;
        }

        public void ApplyToGrid(FastGrid.FastGrid grid)
        {
            var titles = Titles;
            var columns = new List<FastColumn>();
            // видимость колонок
            foreach (var column in grid.Columns)
            {
                if (!titles.Contains(column.PropertyName))
                    column.Visible = false;
                columns.Add(column);
            }
            // порядок колонок
            for (var titleIndex = titles.Count - 1; titleIndex >= 0; titleIndex--)
            {
                var columnIndex = columns.FindIndex(c => c.PropertyName == titles[titleIndex]);
                if (columnIndex == -1)
                    continue;
                var column = columns[columnIndex];
                columns.RemoveAt(columnIndex);
                columns.Insert(0, column);
            }
            // проверка на правильность настроек
            var allCorrect = titles.All(title => grid.Columns.Any(c => c.PropertyName == title));
            if (allCorrect && columns.Any(c => c.Visible)) // не допускать невидимости всех колонок
                grid.Columns = columns;
            else
                grid.Columns.ForEach(c => c.Visible = true);
            // сортировка записей по отдельным колонкам
            var orders = SortOrders;
            if (orders != null)
                for (var i = 0; i < orders.Count; i++)
                {
                    if (i >= grid.Columns.Count)
                        break;
                    grid.Columns[i].SortOrder = orders[i];
                }
            // установка относительной ширины
            var relativeColumnsWidth = RelativeColumnsWidth;
            if (relativeColumnsWidth != null)
                for (var i = 0; i < relativeColumnsWidth.Count; i++)
                {
                    if (i >= grid.Columns.Count)
                        break;
                    grid.Columns[i].RelativeWidth = relativeColumnsWidth[i];
                }
        }

        public void DeriveFromGrid(FastGrid.FastGrid grid)
        {
            var visibleColumns = grid.Columns.Where(c => c.Visible).ToList();
            Titles = visibleColumns.Select(c => c.PropertyName).ToList();
            SortOrders = visibleColumns.Select(c => c.SortOrder).ToList();
            RelativeColumnsWidth = visibleColumns.Select(c => c.RelativeWidth).ToList();
        }
    }
}

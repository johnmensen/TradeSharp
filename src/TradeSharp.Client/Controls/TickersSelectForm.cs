using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Entity;
using FastGrid;
using TradeSharp.Client.BL;

namespace TradeSharp.Client.Controls
{
    public partial class TickersSelectForm : Form
    {
        public List<TickerTag> SelectedTickers
        {
            get { return gridAllTickers.rows.Select(r => (TickerTag)r.ValueObject).Where(t => t.IsSelected).ToList(); }
        }

        public TickersSelectForm()
        {
            InitializeComponent();
            DialogResult = DialogResult.Cancel;
            cbEditSelecting.Text = "Фильтр";

            // таблицы
            LoadAllTickers();
        }

        private void LoadAllTickers()
        {
            // заполнить список выбранных тикеров
            var selTickers = QuoteTableSettings.Instance.GetSettings().Select(cellInfo => 
                new TickerTag {Name = cellInfo.Ticker, Precision = cellInfo.Precision}).ToList();

            // Получить избранные валютные пары
            var favTickers = DalSpot.Instance.GetTickerNames(true);

            // заполнить список всех доступных тикеров
            var names = DalSpot.Instance.GetTickerNames();
            var tickers = new List<TickerTag>();
            foreach (var name in names)
            {
                tickers.Add(new TickerTag 
                { 
                    Name = name, Precision = DalSpot.Instance.GetPrecision(name),
                    IsSelected = selTickers.Any(x => x.Name == name),
                    IsFavorite = favTickers.Contains(name)
                });
            }
            SetupGrid();
            gridAllTickers.DataBind(tickers);
            gridAllTickers.CheckSize();
        }


        private void SetupGrid()
        {
            gridAllTickers.Columns.Add(new FastColumn("IsSelected", "Показ.")
            {
                ColumnWidth = 66,
                ImageList = imageListIsSelected,
                CellHAlignment = StringAlignment.Center,
                SortOrder = FastColumnSort.Descending
            });
            gridAllTickers.Columns.Add(new FastColumn("IsFavorite", "*")
            {
                ColumnWidth = 25,
                ImageList = imageListIsFavorite,
                CellHAlignment = StringAlignment.Center,
                SortOrder = FastColumnSort.Ascending
            });
            gridAllTickers.Columns.Add(new FastColumn("Name", "Инструмент")
            {
                SortOrder = FastColumnSort.Descending,
                CellHAlignment = StringAlignment.Center,
                ColumnMinWidth = 100
            });

            gridAllTickers.UserHitCell += GridUserHitCell;
            gridAllTickers.MultiSelectEnabled = true;
            gridAllTickers.CalcSetTableMinWidth();  
        }

        /// <summary>
        /// Обработчик клика по строке
        /// </summary>
        private void GridUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            if (e.Button == MouseButtons.Left)
            {
                var ticker = (TickerTag)gridAllTickers.rows[rowIndex].ValueObject;
                switch (col.PropertyName)
                {
                    case "IsSelected":
                        ticker.IsSelected = !ticker.IsSelected;
                        break;
                    case "IsFavorite":
                        ticker.IsFavorite = !ticker.IsFavorite;
                        break;  
                }
                gridAllTickers.UpdateRow(rowIndex, ticker);
                gridAllTickers.InvalidateCell(col, rowIndex);
            }
        }

        /// <summary>
        /// Обработчик изменения выбранного значения в выпадающем списке
        /// </summary>
        private void CbEditSelectingSelectedIndexChanged(object sender, EventArgs e)
        {
            var ticks = gridAllTickers.rows.Select(r => (TickerTag)r.ValueObject).ToList();

            switch (cbEditSelecting.SelectedIndex)
            {
                case 0:
                    ticks.ForEach(t => t.IsSelected = true);
                    break;
                
                case 1:
                    ticks.ForEach(t => t.IsSelected = t.IsFavorite);
                    break;
                
                case 2:
                    ticks.ForEach(t => t.IsSelected = false);
                    break;
            }

            gridAllTickers.DataBind(ticks);
            gridAllTickers.Invalidate();
        }

        private void BtnAcceptClick(object sender, EventArgs e)
        {
            var gridTickers = gridAllTickers.rows.Select(r => ((TickerTag)r.ValueObject));
            var favGridTickers = gridTickers.Where(t => t.IsFavorite).Select(t => t.Name).ToArray();

            DalSpot.Instance.SetFavoritesList(favGridTickers);
            UserSettings.Instance.FavoriteTickers = DalSpot.Instance.GetTickerNames(true).ToList();
            UserSettings.Instance.SaveSettings();
        }
    }

    /// <summary>
    /// Класс инкапсулирует свойства, отображаемые в таблице выбора катеровок. Список объектов этого класса привязывается к таблице
    /// </summary>
    public class TickerTag
    {
        public bool IsSelected { get; set; }
        public string Name { get; set; }
        public bool IsFavorite { get; set; }
        public int Precision { get; set; }
    }
}

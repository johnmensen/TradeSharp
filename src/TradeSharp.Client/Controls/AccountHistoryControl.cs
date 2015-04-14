using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FastGrid;
using TradeSharp.Client.Forms;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.UI.Util.Forms;
using TradeSharp.Util;

namespace TradeSharp.Client.Controls
{
    public partial class AccountHistoryControl : UserControl
    {
        public EntityFilter filterHistoryPos;
        private List<MarketOrder> ordersList;

        private ConfigColumnsRequestedDel configColumnsRequested;
        public event ConfigColumnsRequestedDel ConfigColumnsRequested
        {
            add { configColumnsRequested += value; }
            remove { configColumnsRequested -= value; }
        }

        private static readonly FastColumn[] allColumns;

        static AccountHistoryControl()
        {
            var blank = new MarketOrder();
            allColumns = new []
                {
                    new FastColumn(blank.Property(p => p.ID), "#") { SortOrder = FastColumnSort.Ascending },
                    new FastColumn(blank.Property(p => p.Volume), Localizer.GetString("TitleSum"))
                                        {
                                            formatter = v => ((int)v).ToStringUniformMoneyFormat()                                            
                                        },
                    new FastColumn(blank.Property(p => p.Symbol), Localizer.GetString("TitleSymbol")),
                    new FastColumn(blank.Property(p => p.Side), Localizer.GetString("TitleType"))
                    {
                        // отобразить BUY - SELL либо BalanceChangeType
                        formatter = s => 
                            (int) s == 1 ? "BUY" : (int) s == -1 ? "SELL" 
                                : ((int) s / 10) == (int) BalanceChangeType.Deposit ? Localizer.GetString("TitleInvestment")
                                : ((int) s / 10) == (int) BalanceChangeType.Withdrawal ? Localizer.GetString("TitleWithdrawal")
                                : ((int) s / 10) == (int) BalanceChangeType.Profit ? Localizer.GetString("TitleDealProfit")
                                : ((int) s / 10) == (int) BalanceChangeType.Loss ? Localizer.GetString("TitleDealLoss") : "",                
                                colorColumnFormatter = delegate(object cellValue, out Color? backColor, out Color? fontColor)
                        {
                            backColor = null;
                            fontColor = (int) cellValue == 1 ? Color.Green 
                                : (int) cellValue == -1 ? Color.Red : Color.Black;
                        }
                    },
                    new FastColumn(blank.Property(p => p.PriceEnter), Localizer.GetString("TitleEnter"))
                                        {
                                            formatter = (s => (float) s > 0 ? 
                                                ((float) s).ToString("f4") : "")                                            
                                        },
                    new FastColumn(blank.Property(p => p.TimeEnter), Localizer.GetString("TitleTime"))
                    {
                        formatter = (s => (DateTime) s == default(DateTime) ? "" : ((DateTime) s).ToStringUniform())
                    },
                    new FastColumn(blank.Property(p => p.PriceExit), Localizer.GetString("TitleExit")) { FormatString = "f4" },
                    new FastColumn(blank.Property(p => p.TimeExit), Localizer.GetString("TitleTime"))
                    {
                        FormatString = "dd.MM.yyyy HH:mm:ss"
                    },
                    new FastColumn(blank.Property(p => p.ResultDepo), Localizer.GetString("TitleResult"))
                    {
                        formatter = c => Math.Abs((float) c) < 0.01f ? "" : ((float) c).ToStringUniformMoneyFormat(),
                        colorColumnFormatter = delegate(object cellValue, out Color? backColor, out Color? fontColor)
                        {
                            backColor = null;
                            fontColor = null;
                            if (cellValue == null) return;
                            var resDepo = (float)cellValue;
                            fontColor = resDepo >= 0 ? Color.Green : Color.Red;     
                        }
                    },
                    new FastColumn(blank.Property(p => p.ResultPoints), Localizer.GetString("TitlePLInPoints"))
                    {
                        formatter = c => Math.Abs((float) c) < 0.02f ? "" : ((float) c).ToStringUniformMoneyFormat(),
                        colorColumnFormatter = delegate(object cellValue, 
                            out Color? backColor, out Color? fontColor)
                        {
                            backColor = null;
                            fontColor = null;
                            if (cellValue == null) return;
                            var resPoints = (float) cellValue;
                            fontColor = resPoints >= 0 ? Color.Green : Color.Red;                        
                        }
                    },
                    new FastColumn(blank.Property(p => p.StopLoss), "S/L") { FormatString = "f4" },
                    new FastColumn(blank.Property(p => p.TakeProfit), "T/P") { FormatString = "f4" },
                    new FastColumn(blank.Property(p => p.Comment), Localizer.GetString("TitleCommentShort")),
                    new FastColumn(blank.Property(p => p.ExpertComment), Localizer.GetString("TitleRobotCommentShort")),
                    new FastColumn(blank.Property(p=>p.ExitReason), Localizer.GetString("TitleClosing"))
                                        {
                                            formatter = o => EnumFriendlyName<PositionExitReason>.GetString((PositionExitReason) o)
                                        }
                };
        }

        /// <param name="managerMode">режим администратора</param>        
        public AccountHistoryControl(bool managerMode, int? accId) : this()
        {
            //isManagerMode = ManagerMode;
            //accountId = accId;
        }

        public AccountHistoryControl()
        {
            filterHistoryPos = new EntityFilter(typeof(MarketOrder));

            InitializeComponent();

            SetupGrid();
        }

        private void SetupGrid()
        {
            foreach (var column in allColumns)
                historyGrid.Columns.Add(column);
            historyGrid.ColorAltCellBackground = Color.FromArgb(220, 220, 220);
            historyGrid.MultiSelectEnabled = true;
            //historyGrid.MinimumTableWidth = historyGrid.Columns.Count * 75;
            historyGrid.FontAnchoredRow = new Font(Font, FontStyle.Bold);
            historyGrid.StickLast = true;
            historyGrid.ContextMenuRequested += OnContextMenuRequest;
        }

        public void BindOrdersAndTransfers(List<MarketOrder> orders, List<BalanceChange> transfers)
        {
            ordersList = orders;
            historyGrid.rows.Clear();
            var filtOrders = ordersList.Where(filterHistoryPos.PredicateFunc).ToList();
            if (filtOrders.Count == 0)
            {
                historyGrid.Invalidate();
                return;
            }

            // подготовить суммарный ордер, но добавить его в конце
            MarketOrder summaryOrder = null;
            if (filtOrders.Count > 1)
            {
                summaryOrder = new MarketOrder
                                   {
                                       ResultDepo = filtOrders.Sum(o => o.ResultDepo),
                                       ResultPoints = filtOrders.Sum(o => o.ResultPoints),
                                       Symbol = "сумм."
                                   };
            }

            // из трансферов собрать фиктивные ордера
            if (transfers != null)
            foreach (var trans in transfers)
            {
                if (trans.ChangeType == BalanceChangeType.Profit ||
                    trans.ChangeType == BalanceChangeType.Loss) continue;                
                var order = new MarketOrder
                                {
                                    Comment = trans.Description,
                                    Symbol = trans.Currency,
                                    PriceEnter = (float)trans.CurrencyToDepoRate,
                                    PriceExit = (float)trans.CurrencyToDepoRate,
                                    ID = trans.ID,
                                    Volume = (int) trans.Amount,
                                    VolumeInDepoCurrency = (float)trans.AmountDepo,
                                    TimeEnter = trans.ValueDate,
                                    TimeExit = trans.ValueDate,
                                    Side = (int) trans.ChangeType*10,
                                    ResultDepo = (float)trans.AmountDepo
                                };
                filtOrders.Add(order);
            }

            // суммарный ордер
            if (summaryOrder != null) filtOrders.Add(summaryOrder);
            
            historyGrid.DataBind(filtOrders, typeof(MarketOrder));
            historyGrid.CheckSize(true);
        }
        
        private void ФильтрToolStripMenuItemClick(object sender, EventArgs e)
        {
            var dlg = new FilterPropertiesDlg(filterHistoryPos);
            if (dlg.ShowDialog() != DialogResult.OK) return;
            filterHistoryPos = dlg.filter;
            
            historyGrid.DataBind(ordersList.Where(filterHistoryPos.PredicateFunc).ToList());
            historyGrid.CheckSize(true);
        }

        private void HistoryGridUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            if (e.Button == MouseButtons.Left)
            {
                var order = (MarketOrder) historyGrid.rows[rowIndex].ValueObject;
                var dlg = new PositionForm(order);
                dlg.ShowDialog();
            }
        }

        private void OnContextMenuRequest(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            if(rowIndex < 0)
                contextMenuStrip.Show(historyGrid, e.Location);
        }

        private void ConfigColumnsMenuItemClick(object sender, EventArgs e)
        {
            // новое окно с настройкой колонок отключено
            /*var options = allColumns.Select(c => c.PropertyName).Cast<object>().ToList();
            var checkState = allColumns.Select(c => historyGrid.Columns.Any(ac => ac.PropertyName == c.PropertyName)).ToList();

            // дать пользователю выбрать столбцы
            if (!CheckedOrderedListBoxDialog.ShowDialog(ref options, ref checkState, "Выберите столбцы"))
                return;

            var orders = historyGrid.GetRowValues<MarketOrder>(false).ToList();
            columnVisibility.Clear();
            for (var i = 0; i < options.Count; i++)
            {
                var col = allColumns.First(c => c.PropertyName == (string) options[i]);
                columnVisibility.Add(col, checkState[i]);
            }

            // забить сетку
            SetupGridColumns();
            historyGrid.DataBind(orders);*/

            if (configColumnsRequested != null)
                configColumnsRequested(historyGrid);
        }

        public delegate void ConfigColumnsRequestedDel(FastGrid.FastGrid grid);

        private void MenuitemSaveInFileClick(object sender, EventArgs e)
        {
            if (historyGrid.rows.Count == 0) return;

            // выбрать формат
            var formatOptions = new List<object>
                                    {
                                        new CsvFileFormatOption(false, " "),
                                        new CsvFileFormatOption(false, ";"),
                                        new CsvFileFormatOption(false, ((char)9).ToString()),
                                        new CsvFileFormatOption(false, ","),
                                        new CsvFileFormatOption(true, " "),
                                        new CsvFileFormatOption(true, ";"),
                                        new CsvFileFormatOption(true, ((char)9).ToString()),
                                    };
            object selectedFormatObj;
            string selText;
            if (!Dialogs.ShowComboDialog("Формат файла", formatOptions, out selectedFormatObj, out selText, true))
                return;

            var selFormat = (CsvFileFormatOption) selectedFormatObj;

            // сформировать строки
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(selFormat.separator, historyGrid.Columns.Select(c => c.Title)));

            var cellValues = new string[historyGrid.Columns.Count];
            foreach (var row in historyGrid.rows)
            {
                var cellIndex = 0;
                foreach (var cell in row.cells)
                {
                    cellValues[cellIndex++] = selFormat.MakeString(cell.CellValue);
                }
                sb.AppendLine(string.Join(selFormat.separator, cellValues));
            }

            // сохранить
            var dlg = new SaveFileDialog
                          {
                              Title = "Сохранить сделки",
                              DefaultExt = "xls",
                              Filter = "XLS (*.xls)|*.xls|CSV (*.csv)|*.csv|Text (*.txt)|txt|Все файлы|*.*",
                              FilterIndex = 0,
                              FileName =
                                  string.Format("счет_{0}_{1:dd_MMM_HHmm}.xls", AccountStatus.Instance.accountID,
                                                DateTime.Now)
                          };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            
            using (var sw = new StreamWriter(dlg.FileName, false, Encoding.ASCII))
            {
                sw.Write(sb.ToString());
            }
        }
    }

    class CsvFileFormatOption
    {
        public bool useCommaDecimalPoint;

        public string separator;

        public CsvFileFormatOption(bool useCommaDecimalPoint, string separator)
        {
            this.useCommaDecimalPoint = useCommaDecimalPoint;
            this.separator = separator;
        }

        public string MakeString(object val)
        {
            if (val == null) return string.Empty;
            if (val is string) return (string) val;
            if (val is float) return useCommaDecimalPoint
                ? ((float) val).ToStringUniform().Replace(".", ",") : ((float) val).ToStringUniform();
            if (val is DateTime) return ((DateTime) val).ToStringUniform();
            
            return val.ToString();
        }

        public override string ToString()
        {
            return string.Format("Дробная часть: {0}, разделитель: \"{1}\"",
                                 useCommaDecimalPoint ? "запятая" : "точка",
                                 separator[0] == (char) 9 ? "табуляция" : separator);
        }
    }
}

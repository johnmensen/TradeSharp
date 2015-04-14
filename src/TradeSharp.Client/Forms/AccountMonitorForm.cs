using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using Entity;
using FastGrid;
using TradeSharp.Client.BL;
using TradeSharp.Client.Subscription.Dialog;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using System.Linq;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;
using QuoteStorage = TradeSharp.Contract.Util.BL.QuoteStorage;

namespace TradeSharp.Client.Forms
{
    // ReSharper disable LocalizableElement
    // ReSharper disable ParameterTypeCanBeEnumerable.Local
    public partial class AccountMonitorForm : Form, IMdiNonChartWindow
    {
        #region Члены
        private delegate void UpdateOrdersTreeDel(List<PendingOrder> orders);

        private delegate void UpdateDealsTreeDel(List<MarketOrder> orders);

        private delegate void SetAccountStatusDel(List<StatItem> items);

        private const int DefaultColumnWidth = 75;
        
        private List<MarketOrder> historyOrders;
        
        private int currAccountNo;
        
        private string AccountCurrency
        {
            get
            {                
                if (AccountStatus.Instance.AccountData == null) return string.Empty;
                return AccountStatus.Instance.AccountData.Currency;                
            }
        }

        private readonly BackgroundWorker workerLoadHistory = new BackgroundWorker();
        #endregion

        #region Члены (цветовая схема)

        /// <summary>
        /// цвета фона первой суммарной колонки (плечо) от текущего значения плеча
        /// </summary>
        private static Dictionary<float, Color> dicLeverageLevel;

        private static readonly Color colorTextAboveLeverage = Color.Red;

        private static readonly Color colorTextSideBuy = Color.Green;

        private static readonly Color colorTextSideSell = Color.Red;

        private static readonly Color colorTextProfit = Color.DarkOliveGreen;

        private static readonly Color colorTextLoss = Color.Maroon;

        private static readonly Color colorAltCell = Color.FromArgb(230, 230, 230);

        private static readonly Color colorPositionIsHanged = Color.Gray;

        private static readonly Color colorTextPositionInProfit = Color.FromArgb(170, 255, 230);

        private static readonly Color colorTextPositionProtected = Color.FromArgb(170, 210, 255);

        #endregion

        #region Настройка GUI

        private readonly Font fontBold;

        private void SetupAccountGrid()
        {
            var blankStatItem = new StatItem();
            dgAccount.Columns.Add(new FastColumn(blankStatItem.Property(p => p.Name), Localizer.GetString("TitleName")) {RelativeWidth = 0.2});
            dgAccount.Columns.Add(new FastColumn(blankStatItem.Property(p => p.Result), Localizer.GetString("TitleValue")) {RelativeWidth = 0.8});
            dgAccount.ColorAltCellBackground = colorAltCell;
            dgAccount.MinimumTableWidth = dgAccount.Columns.Count * DefaultColumnWidth;
            dgAccount.CheckSize(true);

            gridPendingOrders.CheckSize(true);
        }

        private void SetupGridSummaryMarketOrders()
        {
            var blank = new PositionSummary();
            dgSummary.Columns.Add(new FastColumn(blank.Property(p => p.Leverage), Localizer.GetString("TitleLeverage"))
                {
                    ColumnFont = fontBold,
                    colorColumnFormatter = delegate(object cellValue, out Color? backColor, out Color? fontColor)
                        {
                            fontColor = null;
                            backColor = colorTextAboveLeverage;
                            if (cellValue == null) return;
                            var lev = (float) cellValue;
                            foreach (var pair in dicLeverageLevel.Where(pair => pair.Key >= lev))
                            {
                                backColor = pair.Value;
                                break;
                            }
                        }
                });
            if (HiddenModes.ManagerMode)
                dgSummary.Columns.Add(new FastColumn(blank.Property(p => p.LeverageProtected), Localizer.GetString("TitleFreeLeverage"))
                    {
                        ColumnFont = fontBold
                    });
            dgSummary.Columns.Add(new FastColumn(blank.Property(p => p.Symbol), Localizer.GetString("TitleSymbol")));
            dgSummary.Columns.Add(new FastColumn(blank.Property(p => p.Side), Localizer.GetString("TitleType"))
                {
                    formatter = (s => (int) s > 0 ? "BUY" : (int) s == 0 ? "" : "SELL"),
                    colorColumnFormatter = delegate(object cellValue, out Color? backColor, out Color? fontColor)
                        {
                            backColor = null;
                            fontColor = (int) cellValue == 1 ? colorTextSideBuy : colorTextSideSell;
                        }
                });
            dgSummary.Columns.Add(new FastColumn(blank.Property(p => p.Volume), Localizer.GetString("TitleSum"))
                {
                    formatter = v => Math.Abs((int) v).ToStringUniformMoneyFormat()
                });
            if (HiddenModes.ManagerMode)
                dgSummary.Columns.Add(new FastColumn(blank.Property(p => p.VolumeProtected), Localizer.GetString("TitleFreeSum")) {ColumnFont = fontBold});
            dgSummary.Columns.Add(new FastColumn(blank.Property(p => p.AveragePrice), Localizer.GetString("TitleEnterPrice"))
                {
                    formatter = (s => (float) s == 0 ? "" : s.ToString()),
                    ColumnFont = fontBold
                });
            dgSummary.Columns.Add(new FastColumn(blank.Property(p => p.Profit), Localizer.GetString("TitleOpenedPL"))
                {
                    colorColumnFormatter = ColorColumnSide,
                    formatter = value => ((float) value).ToStringUniformMoneyFormat()
                });
            dgSummary.Columns.Add(new FastColumn(blank.Property(p => p.ProfitInPoints), Localizer.GetString("TitlePLInPoints"))
                {
                    colorColumnFormatter = ColorColumnSide,
                    formatter = value => ((int)value).ToStringUniformMoneyFormat()
                });
            dgSummary.Columns.Add(new FastColumn(blank.Property(p => p.ProfitInPercent), Localizer.GetString("TitlePLInPercents"))
                {
                    colorColumnFormatter = ColorColumnSide
                });
            dgSummary.Columns.Add(new FastColumn(blank.Property(p => p.Exposition), Localizer.GetString("TitleExposition"))
                {
                    colorColumnFormatter = ColorColumnSide,
                    formatter = v => ((int) v).ToStringUniformMoneyFormat()
                });
            dgSummary.Columns.Add(new FastColumn(blank.Property(p=>p.TakeProfit), "T/P")
            {
                IsHyperlinkStyleColumn = true,
                HyperlinkFontActive = new Font(Font, FontStyle.Bold),
                HyperlinkActiveCursor = Cursors.Hand
            });
            dgSummary.Columns.Add(new FastColumn(blank.Property(p => p.StopLoss), "S/L")
                {
                    IsHyperlinkStyleColumn = true,
                    HyperlinkFontActive = fontBold,
                    HyperlinkActiveCursor = Cursors.Hand
                });
            dgSummary.Columns.Add(new FastColumn(blank.Property(p => p.Command), Localizer.GetString("TitleOrder"))
                {
                    IsHyperlinkStyleColumn = true,
                    HyperlinkFontActive = fontBold,
                    HyperlinkActiveCursor = Cursors.Hand
                });
            dgSummary.Columns.Add(new FastColumn(blank.Property(p => p.Equity), Localizer.GetString("TitleFunds")));

            dgSummary.UserHitCell += GridOpenPosUserHitCell;
            dgSummary.ColorAltCellBackground = colorAltCell;
            dgSummary.CalcSetTableMinWidth(85);

            // применить настройки
            var sets = GridSettings.EnsureSettings(GridSettings.ListAccountSummaryOrders);
            sets.ApplyToGrid(dgSummary);
            dgSummary.ColumnSettingsChanged += OnDgSummaryColumnSettingsChange;
            dgSummary.DataBind(new List<PositionSummary>(), typeof(PositionSummary));
            dgSummary.CheckSize(true);
        }

        private void SetupGridMarketOrders()
        {
            var blank = new MarketOrder();
            gridOpenPos.MultiSelectEnabled = true;
            gridOpenPos.Columns.Add(new FastColumn(blank.Property(p => p.ID), "#"));
            gridOpenPos.Columns.Add(new FastColumn(blank.Property(p=>p.Volume), Localizer.GetString("TitleSum")));
            gridOpenPos.Columns.Add(new FastColumn(blank.Property(p => p.Symbol), Localizer.GetString("TitleSymbol")));
            gridOpenPos.Columns.Add(new FastColumn(blank.Property(p => p.Side), Localizer.GetString("TitleType")));
            gridOpenPos.Columns.Add(new FastColumn(blank.Property(p => p.PriceEnter), Localizer.GetString("TitleEnter")));
            gridOpenPos.Columns.Add(new FastColumn(blank.Property(p => p.TimeEnter), Localizer.GetString("TitleTime")));
            gridOpenPos.Columns.Add(new FastColumn(blank.Property(p => p.ResultDepo), Localizer.GetString("TitleOpenedPL")));
            gridOpenPos.Columns.Add(new FastColumn(blank.Property(p => p.ResultPoints), Localizer.GetString("TitlePLInPoints")));
            gridOpenPos.Columns.Add(new FastColumn(blank.Property(p => p.StopLoss), "S/L"));
            gridOpenPos.Columns.Add(new FastColumn(blank.Property(p => p.TakeProfit), "T/P"));
            gridOpenPos.Columns.Add(new FastColumn(blank.Property(p => p.Trailing), Localizer.GetString("TitleTrailing")));
            gridOpenPos.Columns.Add(new FastColumn(blank.Property(p => p.Comment), Localizer.GetString("TitleCommentShort")));
            gridOpenPos.Columns.Add(new FastColumn(blank.Property(p => p.ExpertComment), Localizer.GetString("TitleRobotCommentShort"))
                {
                    IsHyperlinkStyleColumn = true,
                    HyperlinkFontActive = fontBold,
                    HyperlinkActiveCursor = Cursors.Hand,
                    rowFormatter = value =>
                        {
                            var order = (MarketOrder) value;
                            if (string.IsNullOrEmpty(order.ExpertComment)) return string.Empty;

                            int signalCat, parentDeal;
                            if (MarketOrder.GetTradeSignalFromDeal(order, out signalCat, out parentDeal))
                                return "сигнал #" + signalCat;
                            return order.ExpertComment;
                        }
                });
            gridOpenPos.Columns.Add(new FastColumn(blank.Property(p => p.Magic), "Magic"));
            gridOpenPos.Columns.Add(new FastColumn(blank.Property(p => p.State), Localizer.GetString("TitleState")));

            gridOpenPos.UserHitCell += GridOpenPosUserHitCell;
            gridOpenPos.ColorAltCellBackground = colorAltCell;
            gridOpenPos.MinimumTableWidth = gridOpenPos.Columns.Count * DefaultColumnWidth;

            gridOpenPos.colorFormatter = delegate(object cellValue, out Color? backColor, out Color? fontColor)
                {
                    backColor = null;
                    fontColor = null;
                    var pos = (MarketOrder) cellValue;
                    if (pos.State != PositionState.Opened)
                    {
                        backColor = colorPositionIsHanged;
                        return;
                    }
                    var quote = QuoteStorage.Instance.ReceiveValue(pos.Symbol);
                    if (pos.StopLoss.HasValue && pos.StopLoss > 0)
                        if ((pos.PriceEnter <= pos.StopLoss.Value && pos.Side > 0) ||
                            (pos.PriceEnter >= pos.StopLoss.Value && pos.Side < 0))
                        {
                            backColor = colorTextPositionInProfit;
                            return;
                        }
                    if (quote == null) return;
                    if ((pos.Side > 0 && pos.PriceEnter < quote.bid) || (pos.Side < 0 && pos.PriceEnter > quote.ask))
                        backColor = colorTextPositionProtected;
                };
            gridOpenPos.rowExtraFormatter = delegate(object obj, List<FastColumn> columns)
                {
                    return GetFormattedMarketOrder(obj as MarketOrder, columns);
                };

            gridOpenPos.ColumnSettingsChanged += OnGridOpenPosColumnSettingsChange;
            // применить настройки из файла настроек
            var sets = GridSettings.EnsureSettings(GridSettings.ListAccountOpenedOrders);
            sets.ApplyToGrid(gridOpenPos);
        }

        private void SetupGridPendingOrders()
        {
            var blank = new PendingOrder();
            gridPendingOrders.MultiSelectEnabled = true;
            gridPendingOrders.Columns.Add(new FastColumn(blank.Property(p => p.ID), "#")
                {
                    SortOrder = FastColumnSort.Ascending,
                });
            gridPendingOrders.Columns.Add(new FastColumn(blank.Property(p => p.Volume), Localizer.GetString("TitleSum"))
                {
                    FormatString = "n0"
                });
            gridPendingOrders.Columns.Add(new FastColumn(blank.Property(p => p.Symbol), Localizer.GetString("TitleSymbol")));
            gridPendingOrders.Columns.Add(new FastColumn(blank.Property(p => p.Side), Localizer.GetString("TitleDeal"))
                {
                    formatter = (s => (int) s > 0 ? "BUY" : "SELL")
                });
            gridPendingOrders.Columns.Add(new FastColumn(blank.Property(p => p.PriceSide), Localizer.GetString("TitleType"))
                {
                    formatter = (s => (int) s == 1 ? "STOP" : "LIMIT")
                });
            gridPendingOrders.Columns.Add(new FastColumn(blank.Property(p => p.PriceFrom), Localizer.GetString("TitlePrice"))
                {
                    formatter = p => ((float) p).ToStringUniformPriceFormat(true)
                });
            gridPendingOrders.Columns.Add(new FastColumn(blank.Property(p => p.StopLoss), "S/L") {FormatString = "f5"});
            gridPendingOrders.Columns.Add(new FastColumn(blank.Property(p => p.TakeProfit), "T/P") {FormatString = "f5"});
            gridPendingOrders.Columns.Add(new FastColumn(blank.Property(p => p.PriceTo), Localizer.GetString("TitleLimit")) {FormatString = "f5"});
            gridPendingOrders.Columns.Add(new FastColumn(blank.Property(p => p.TimeFrom), Localizer.GetString("TitleStart"))
                {
                    FormatString = "dd.MM.yyyy HH:mm:ss",
                });
            gridPendingOrders.Columns.Add(new FastColumn(blank.Property(p => p.TimeTo), Localizer.GetString("TitleCompletion"))
                {
                    FormatString = "dd.MM.yyyy HH:mm:ss",
                });
            gridPendingOrders.Columns.Add(new FastColumn(blank.Property(p => p.Magic), "Magic"));
            gridPendingOrders.Columns.Add(new FastColumn(blank.Property(p => p.Comment), Localizer.GetString("TitleCommentShort")));
            gridPendingOrders.Columns.Add(new FastColumn(blank.Property(p => p.ExpertComment), Localizer.GetString("TitleRobotCommentShort")));
            gridPendingOrders.Columns.Add(new FastColumn(blank.Property(p => p.PairOCO), "OCO"));
            gridPendingOrders.Columns.Add(new FastColumn(blank.Property(p => p.Status), Localizer.GetString("TitleStatus")));

            gridPendingOrders.SelectionChanged += GridPendingOrdersSelectionChanged;
            gridPendingOrders.DoubleClick += GridPendingOrdersDoubleClick;
            gridPendingOrders.ColorAltCellBackground = Color.FromArgb(230, 230, 230);
            gridPendingOrders.MinimumTableWidth = gridPendingOrders.Columns.Count * DefaultColumnWidth;

            gridPendingOrders.ColumnSettingsChanged += OnGridPendingOrdersColumnSettingsChange;
            // применить настройки из файла настроек
            var sets = GridSettings.EnsureSettings(GridSettings.ListAccountPendingOrders);
            sets.ApplyToGrid(gridPendingOrders);
        }

        #endregion

        #region IMdiNonChartWindow

        public NonChartWindowSettings.WindowCode WindowCode { get { return NonChartWindowSettings.WindowCode.Account; } }
        
        public int WindowInnerTabPageIndex
        {
            get
            {
                return (int)Invoke(new Func<object>(() => tabControl.SelectedIndex));
            }
            set
            {
                if (value < tabControl.TabPages.Count)
                    tabControl.SelectedIndex = value;
            }
        }

        private Action<Form> formMoved;
        public event Action<Form> FormMoved
        {
            add { formMoved += value; }
            remove { formMoved -= value; }
        }

        /// <summary>
        /// перемещение формы завершено - показать варианты Drop-a
        /// </summary>
        private Action<Form> resizeEnded;
        public event Action<Form> ResizeEnded
        {
            add { resizeEnded += value; }
            remove { resizeEnded -= value; }
        }

        #endregion

        #region Конструктор + события формы

        public AccountMonitorForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);

            fontBold = new Font(Font, FontStyle.Bold);
            SetupColorByLeverage();
            SetupGridSummaryMarketOrders();
            SetupGridMarketOrders();
            SetupGridPendingOrders();
            SetupAccountGrid();
            MarketOrdersStorage.Instance.MarketOrdersUpdated += OnMarketOrdersChanged;
            MarketOrdersStorage.Instance.PendingOrdersUpdated += OnPendingOrdersChanged;
            workerLoadHistory.DoWork += LoadHistoryOrdersInBackground;
            workerLoadHistory.RunWorkerCompleted += LoadHistoryOrdersCompleted;

            accountHistoryCtrl.historyGrid.ColumnSettingsChanged += OnHistoryGridColumnSettingsChange;
            accountHistoryCtrl.ConfigColumnsRequested += ConfigHistoryGridColumns;
            // применить настройки из файла настроек для таблицы с закрытыми ордерами
            var sets = GridSettings.EnsureSettings(GridSettings.ListAccountClosedOrders);
            sets.ApplyToGrid(accountHistoryCtrl.historyGrid);
        }

        private void SetupColorByLeverage()
        {
            dicLeverageLevel =
                new Dictionary<float, Color>
                    {
                        { UserSettings.Instance.RiskLeverWarning, Color.Aquamarine }, 
                        { UserSettings.Instance.RiskLeverCritical, Color.BurlyWood }, 
                        { 1000, Color.Coral }
                    };
        }

        private void AccountMonitorFormLoad(object sender, EventArgs e)
        {
            // информация по счету
            AccountStatusSummaryTable.UpdateAccountInfo(SetAccountStatusSafe);
            // показать открытые позы
            ShowOpenPositions();
            // показать ордера
            ShowPendingOrders();

            // запомнить окошко
            MainForm.Instance.AddNonChartWindowSets(new NonChartWindowSettings
            {
                Window = WindowCode,
                WindowPos = Location,
                WindowSize = Size,
                WindowState = WindowState.ToString()
            });
        }

        private void AccountMonitorFormFormClosing(object sender, FormClosingEventArgs e)
        {
            MarketOrdersStorage.Instance.MarketOrdersUpdated -= OnMarketOrdersChanged;
            MarketOrdersStorage.Instance.PendingOrdersUpdated -= OnPendingOrdersChanged;
            AccountStatus.Instance.Trades = 0;

            // убрать окошко из конфигурации
            if (e.CloseReason == CloseReason.UserClosing ||
                e.CloseReason == CloseReason.None)
                MainForm.Instance.RemoveNonChartWindowSets(WindowCode);
        }
        #endregion

        #region Клик в гридах

        private void GridOpenPosUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            if (rowIndex < 0 || rowIndex >= gridOpenPos.rows.Count)
                return;
            var order = gridOpenPos.rows[rowIndex].ValueObject as MarketOrder;
            if (order == null)
                return;
            
            // показать информацию по сигналу
            if (e.Button == MouseButtons.Left && col.PropertyName == order.Property(o => o.ExpertComment))
            {
                int signalCatId, orderId;
                if (!MarketOrder.GetTradeSignalFromDeal(order, out signalCatId, out orderId)) return;

                // получить счет перформера по кат. сигнала
                PerformerStat stat;
                try
                {
                    stat = TradeSharpAccountStatistics.Instance.proxy.GetPerformerStatBySignalCatId(signalCatId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Localizer.GetString("MesssageCanNotGetSubscribersInfo"));
                    Logger.Info("ShowSignallerStat", ex);
                    return;
                }

                if (stat == null) return;
                var form = new SubscriberStatisticsForm(stat);
                form.Show(this);

                return;
            }

            var selectedCount = gridOpenPos.rows.Count(r => r.Selected);
            CloseAllBtn.Text = "Закрыть все";

            if (selectedCount == 1)
            {
                var selRows = gridOpenPos.rows.Where(r => r.Selected).ToList();
                var deal = ((MarketOrder)selRows[0].ValueObject);
                if (deal.State != PositionState.Opened)
                {
                    EditOrderBtn.Enabled = false;
                    CloseAllBtn.Enabled = true;
                    ClosePositionBtn.Enabled = false;
                }
                else
                {
                    EditOrderBtn.Enabled = true;
                    ClosePositionBtn.Enabled = true;
                }
            }
            if (e.Button == MouseButtons.Right)
            {
                if (selectedCount == 0)
                {
                    EditOrderBtn.Enabled = false;
                    CloseAllBtn.Enabled = true;
                    ClosePositionBtn.Enabled = false;
                }
                return;
            }

            // редактировать ордер
            if (e.Clicks == 2)
            {
                EditOrderBtnClick(this, e);
                //return;
            }
        }

        private void GridPendingOrdersDoubleClick(object sender, EventArgs e)
        {
            BtnEditPendingOrderClick(sender, e);
            ShowPendingOrders();
        }

        // ReSharper disable InconsistentNaming
        private void DgSummaryUserHitCell(object sender, MouseEventArgs mouseEventArgs, int rowIndex, FastColumn col)
        // ReSharper restore InconsistentNaming
        {
            var deal = ((PositionSummary) dgSummary.rows[rowIndex].ValueObject);
            if (col.PropertyName == deal.Property(p => p.StopLoss) || col.PropertyName == deal.Property(p => p.TakeProfit))
            {
                EditOverallSymbol(deal.Symbol);
            }
            if (col.PropertyName == deal.Property(p => p.Command))
            {
                var positions = MarketOrdersStorage.Instance.MarketOrders;
                if (positions.Count == 0) return;
                if (MessageBox.Show(string.Format(
                    Localizer.GetString("MessageConfirmClosingAllOrdersFmt"), 
                    deal.Symbol),
                    Localizer.GetString("TitleWarning"), 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
                var account = AccountStatus.Instance.AccountData;
                if (MainForm.serverProxyTrade.proxy == null || account == null)
                    return;

                var ordersList =
                    (from poz in positions where poz.Symbol == deal.Symbol && poz.State == PositionState.Opened select poz.ID)
                        .ToList();
                if (ordersList.Count > 0)
                    ClosePositionsByTicker(deal.Symbol);
            }
        }

        #endregion

        #region Обновление таблиц - гридов
        private void UpdateDealsTreeSafe(List<MarketOrder> orders)
        {
            BeginInvoke(new UpdateDealsTreeDel(UpdateDealsTable), orders);
        }

        private void UpdateOrdersTreeSafe(List<PendingOrder> orders)
        {
            BeginInvoke(new UpdateOrdersTreeDel(UpdatePendingOrdersTree), orders);
        }

        private void UpdateHistoryGrid(List<MarketOrder> orders, List<BalanceChange> transfers)
        {
            if (AccountStatus.Instance.AccountData == null) return;
            var balance = AccountStatus.Instance.AccountData.Balance;
            var newHistOrderBySymbol = PositionSummary.GetPositionSummary(orders, AccountCurrency, (float)balance);
            if (historyOrders != null)
            {
                var histOrderBySymbol = PositionSummary.GetPositionSummary(historyOrders, AccountCurrency, (float)balance);
                // проверяем изменились ли ордера
                if (histOrderBySymbol.Sum(ps => ps.hashCode) == newHistOrderBySymbol.Sum(ps => ps.hashCode))
                    return;
            }
            historyOrders = orders;
            accountHistoryCtrl.BindOrdersAndTransfers(orders == null ? new List<MarketOrder>() : orders.ToList(), transfers);
        }

        private void UpdateDealsTable(List<MarketOrder> orders)
        {
            try
            {
                if (!AccountStatus.Instance.isAccountSelected)
                {
                    orders = new List<MarketOrder>();
                }
                if (orders == null)
                    return;
                if (AccountStatus.Instance.AccountData == null)
                    return;
                foreach (var order in orders.Where(o => o.State == PositionState.Opened))
                {
                    order.PriceEnter = (float)Math.Round(order.PriceEnter, DalSpot.Instance.GetPrecision(order.Symbol));
                    var quote = QuoteStorage.Instance.ReceiveValue(order.Symbol);
                    if (quote != null)
                    {
                        var currPrice = order.Side == 1 ? quote.bid : quote.ask;
                        order.ResultPoints = DalSpot.Instance.GetPointsValue(order.Symbol, order.Side == 1 ? (currPrice - order.PriceEnter) : order.PriceEnter - currPrice);
                    }
                    else
                        order.ResultPoints = 0;
                }

                // сгруппировать позы по валютам
                var newOrderBySymbol = PositionSummary.GetPositionSummary(orders.Where(o => o.State == PositionState.Opened).ToList(),
                    AccountCurrency, (float)AccountStatus.Instance.AccountData.Balance);

                RefreshOpenPositionsGrid(orders);
                RefreshOpenSummaryGrid(newOrderBySymbol);

                var scroll = dgSummary.Controls.OfType<HScrollBar>().FirstOrDefault();
                var hasScroll = scroll != null && scroll.Visible;
                var scrollHt = !hasScroll ? 0 : SystemInformation.HorizontalScrollBarHeight;
                panel2.Height = dgSummary.CaptionHeight + newOrderBySymbol.Count * dgSummary.CellHeight + scrollHt + 10;
            }
            catch (Exception ex)
            {
                Logger.Error("UpdateDealsTable", ex);
            }
        }

        private void RefreshOpenSummaryGrid(List<PositionSummary> summList)
        {
            // сравнить новые ордера со старыми
            if (dgSummary.rows.Count == 0 && summList.Count == 0) return;
            if (dgSummary.rows.Count > 0 && summList.Count == 0)
            {
                dgSummary.rows.Clear();
                dgSummary.Invalidate();
                return;
            }

            var selList = new List<int>();
            for (var i = 0; i < dgSummary.rows.Count; i++)
            {
                if (dgSummary.rows[i].Selected)
                    selList.Add(i);
            }
            dgSummary.DataBind(summList);
            dgSummary.CheckSize(true);

            foreach (var i in selList)
                dgSummary.rows[i].Selected = true;
        }

        private void RefreshOpenPositionsGrid(List<MarketOrder> orders)
        {
            // сравнить новые ордера со старыми
            if (gridOpenPos.rows.Count == 0 && orders.Count == 0) return;
            if (gridOpenPos.rows.Count > 0 && orders.Count == 0)
            {
                gridOpenPos.rows.Clear();
                gridOpenPos.Invalidate();
                return;
            }
            // заново байндить грид
            if (gridOpenPos.rows.Count != orders.Count)
            {
                gridOpenPos.rows.Clear();
                gridOpenPos.DataBind(orders, typeof(MarketOrder));
                gridOpenPos.CheckSize(true);
                gridOpenPos.Invalidate();
                return;
            }
            // обновить ордера
            var needInvalidate = false;
            var needRebind = false;
            for (var i = 0; i < gridOpenPos.rows.Count; i++)
            {
                var row = gridOpenPos.rows[i];
                var order = (MarketOrder)row.ValueObject;
                var newOrder = orders.FirstOrDefault(o => o.ID == order.ID);
                if (newOrder == null)
                {
                    needRebind = true;
                    break;
                }
                if (!newOrder.CompareOrders(order))
                {
                    needInvalidate = true;
                    gridOpenPos.UpdateRow(i, newOrder);
                }
            }

            // все же требуется заново байндить грид
            if (needRebind)
            {
                gridOpenPos.rows.Clear();
                gridOpenPos.DataBind(orders, typeof(MarketOrder));
                gridOpenPos.CheckSize(true);
                gridOpenPos.Invalidate();
                return;
            }

            if (needInvalidate)
                gridOpenPos.InvalidateColumns(6, 10);
        }

        private void UpdatePendingOrdersTree(List<PendingOrder> orders)
        {
            if (orders == null) return;
            try
            {
                if (!AccountStatus.Instance.isAuthorized)
                    orders = new List<PendingOrder>();
                if (gridPendingOrders.rows.Count != orders.Count)
                {
                    gridPendingOrders.DataBind(orders);
                    gridPendingOrders.CheckSize(true);
                    gridPendingOrders.Invalidate();
                    return;
                }
                if (orders.Count == 0) return;
                // обновить выборочно
                var orderById = orders.ToDictionary(o => o.ID, o => o);
                var oldOrders = gridPendingOrders.rows.Select(r => (PendingOrder)r.ValueObject).ToList();
                var needUpdate = false;
                foreach (var pendingOrder in oldOrders)
                {
                    PendingOrder newOrder;
                    orderById.TryGetValue(pendingOrder.ID, out newOrder);
                    if (newOrder == null)
                    {
                        needUpdate = true;
                        break;
                    }
                    if (newOrder.OrdersAreSame(pendingOrder)) continue;
                    needUpdate = true;
                    break;
                }
                if (needUpdate)
                {
                    gridPendingOrders.DataBind(orders);
                    gridPendingOrders.CheckSize(true);
                    gridPendingOrders.Invalidate();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка обновления отложенных ордеров", ex);
            }
        }

        #endregion

        public void OnPendingOrdersChanged()
        {
            // показать ордера
            ShowPendingOrders();
        }

        public void OnMarketOrdersChanged()
        {
            // показать открытые позы
            ShowOpenPositions();            
        }

        private static void ColorColumnSide(object cellValue, out Color? backColor, out Color? fontColor)
        {
            backColor = null;
            fontColor = null;
            if (cellValue == null) return;
            var sign = cellValue is float ? (float)cellValue >= 0
                : (int) cellValue >= 0;
            fontColor = sign ? colorTextProfit : colorTextLoss; 
        }

        private void OnDgSummaryColumnSettingsChange()
        {
            var sets = GridSettings.EnsureSettings(GridSettings.ListAccountSummaryOrders);
            sets.DeriveFromGrid(dgSummary);
        }

        #region MarketOrdersGrid
        private static string GetTrailingData(MarketOrder ord)
        {
            var res = new StringBuilder();
            for (var i = 0; i < ord.trailingLevels.Length; i++)
                if (ord.trailingLevels[i] != null && ord.trailingTargets[i] != null)
                    res.Append(string.Format("[{0}:{1}]", 
                        (ord.PriceEnter + DalSpot.Instance.GetAbsValue(ord.Symbol, ord.trailingLevels[i].Value * ord.Side)).ToStringUniformPriceFormat(), 
                        (int)(ord.trailingTargets[i].Value + 0.5f)));
            return res.ToString();
        }

        private static void SetFormattedStrings(ref string[] strings, List<FastColumn> columns, string columnPropertyName, string value)
        {
            var index = columns.FindIndex(column => column.PropertyName == columnPropertyName);
            if (index < 0 || index >= strings.Length)
                return;
            strings[index] = value;
        }

        private static string[] GetFormattedMarketOrder(MarketOrder ord, List<FastColumn> columns)
        {
            if (ord == null)
                return null;
            var result = new string[16];
            var formatString = "f" + (DalSpot.Instance.GetPrecision(ord.Symbol) + 1);
            SetFormattedStrings(ref result, columns, ord.Property(p => p.ID), ord.ID.ToString());
            SetFormattedStrings(ref result, columns, ord.Property(p => p.Volume), ord.Volume.ToStringUniformMoneyFormat());
            SetFormattedStrings(ref result, columns, ord.Property(p => p.Symbol), ord.Symbol);
            SetFormattedStrings(ref result, columns, ord.Property(p => p.Side), ord.Side > 0 ? "BUY" : "SELL");
            SetFormattedStrings(ref result, columns, ord.Property(p => p.PriceEnter), ord.PriceEnter.ToString(formatString));
            SetFormattedStrings(ref result, columns, ord.Property(p => p.TimeEnter), ord.TimeEnter.ToStringUniform());
            SetFormattedStrings(ref result, columns, ord.Property(p => p.ResultDepo),
                                ord.PriceExit.HasValue ? "" : ord.ResultDepo.ToStringUniformMoneyFormat());
            SetFormattedStrings(ref result, columns, ord.Property(p => p.ResultPoints), ord.PriceEnter == 0 ? "" : ord.ResultPoints.ToString("f0"));
            SetFormattedStrings(ref result, columns, ord.Property(p => p.StopLoss),
                                ord.StopLoss.HasValue ? ord.StopLoss.Value.ToString(formatString) : "");
            SetFormattedStrings(ref result, columns, ord.Property(p => p.TakeProfit),
                                ord.TakeProfit.HasValue ? ord.TakeProfit.Value.ToString(formatString) : "");
            SetFormattedStrings(ref result, columns, ord.Property(p => p.Trailing), GetTrailingData(ord));
            SetFormattedStrings(ref result, columns, ord.Property(p => p.Comment), ord.Comment);
            SetFormattedStrings(ref result, columns, ord.Property(p => p.ExpertComment), ord.ExpertComment);
            SetFormattedStrings(ref result, columns, ord.Property(p => p.Magic), ord.Magic.ToString());
            SetFormattedStrings(ref result, columns, ord.Property(p => p.State), EnumFriendlyName<PositionState>.GetString(ord.State));
            return result;
        }

        private void OnGridOpenPosColumnSettingsChange()
        {
            var sets = GridSettings.EnsureSettings(GridSettings.ListAccountOpenedOrders);
            sets.DeriveFromGrid(gridOpenPos);            
        }
        #endregion

        private void OnGridPendingOrdersColumnSettingsChange()
        {
            var sets = GridSettings.EnsureSettings(GridSettings.ListAccountPendingOrders);
            sets.DeriveFromGrid(gridPendingOrders);
        }

        private void OnHistoryGridColumnSettingsChange()
        {
            var sets = GridSettings.EnsureSettings(GridSettings.ListAccountClosedOrders);
            sets.DeriveFromGrid(accountHistoryCtrl.historyGrid);
        }

        private void UpdateTimerTick(object sender, EventArgs e)
        {
            if (AccountStatus.Instance.connectionStatus != AccountConnectionStatus.Connected) return;
            // информация по счету
            AccountStatusSummaryTable.UpdateAccountInfo(SetAccountStatusSafe);
            
            // показать открытые позы
            UpdateDealsTreeSafe(MarketOrdersStorage.Instance.MarketOrders);
            if (historyOrders == null || currAccountNo == 0 || currAccountNo != AccountStatus.Instance.accountID)
            {
                currAccountNo = AccountStatus.Instance.accountID;                
            }
        }
        
        private void ShowOpenPositions()
        {            
            UpdateDealsTreeSafe(MarketOrdersStorage.Instance.MarketOrders);
        }

        private void ShowPendingOrders()
        {
            UpdateOrdersTreeSafe(MarketOrdersStorage.Instance.PendingOrders);
        }

        private void SetAccountStatusSafe(List<StatItem> items)
        {
            BeginInvoke(new SetAccountStatusDel(SetAccountStatus), items);
        }
        
        private void SetAccountStatus(List<StatItem> items)
        {
            dgAccount.DataBind(items, typeof(StatItem));
            dgAccount.CheckSize();
        }
        
        private void EditOverallSymbol(string symbol)
        {
            var account = AccountStatus.Instance.AccountData;
            if (MainForm.serverProxyTrade.proxy == null || account == null)
                return;

            var positions = MarketOrdersStorage.Instance.MarketOrders;

            var ordersList = (from poz in positions where poz.Symbol == symbol && poz.State == PositionState.Opened select poz.ID).ToList();
            if (ordersList.Count == 0)
                return;
            new MultyOrdersEditForm(ordersList).ShowDialog();
            Refresh();
        }

        /// <summary>
        /// разом закрыть пачку сделок (отобразить процесс в диалоговом окошке)
        /// </summary>
        private static void ClosePositions(List<int> orderList)
        {
            if (orderList.Count == 0) return;
            
            if (orderList.Count == 1)
            {
                var accountId = AccountStatus.Instance.accountID;
                if (accountId == 0) return;
                MainForm.Instance.SendCloseRequestSafe(accountId, orderList[0], PositionExitReason.ClosedFromUI);
                return;
            }

            var dlg = new ClosePositionsForm(orderList);
            dlg.ShowDialog();
        }

        private static void ClosePositionsByTicker(string ticker)
        {
            var accountId = AccountStatus.Instance.accountID;
            if (accountId == 0) return;
            MainForm.Instance.SendCloseByTickerRequestSafe(accountId, ticker, PositionExitReason.ClosedFromUI);
        }

        private void ClosePosition()
        {
            var selRows = gridOpenPos.rows.Where(r => r.Selected).ToList();
            if (selRows.Count == 0) return;


            var deal = ((MarketOrder) selRows[0].ValueObject);
            if (deal.State == PositionState.StartClosed)
            {
                MessageBox.Show(string.Format(Localizer.GetString("MessagePositionIsAlreadyClosingFmt"), deal.ID), 
                    Localizer.GetString("TitleWarning"));
                return;
            }
            if (MessageBox.Show(string.Format(Localizer.GetString("MessageConfirmClosingOrderFmt"), deal.ID),
               Localizer.GetString("TitleWarning"), 
               MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            string errorStr;
            CloseDeal(deal.ID, out errorStr);
        }

        private static void RemovePendingOrders(List<int> orderList)
        {
            // закрыть...
            string errorStr;
            foreach (var order in orderList)
                DeletePendingOrder(order, out errorStr);
        }

        private void ClosePendingOrder()
        {
            var row = gridPendingOrders.rows.FirstOrDefault(r => r.Selected);
            if (row == null) return;
            var orderId = ((PendingOrder) row.ValueObject).ID;
            if (MessageBox.Show(
                string.Format(Localizer.GetString("MessageConfirmDeletePendingFmt"), orderId),
                Localizer.GetString("TitleWarning"), 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            string errorStr;
            DeletePendingOrder(orderId, out errorStr);
        }

        private static void CloseDeal(int orderId, out string errorStr)
        {
            errorStr = "";
            var account = AccountStatus.Instance.AccountData;
            if (MainForm.serverProxyTrade.proxy == null || account == null)
            {
                errorStr = EnumFriendlyName<AccountConnectionStatus>.GetString(AccountConnectionStatus.NotConnected);
                return;
            }

            MainForm.Instance.SendCloseRequestSafe(account.ID, orderId, PositionExitReason.ClosedFromUI);
        }

        private static void DeletePendingOrder(int orderId, out string errorStr)
        {
            errorStr = "";
            RequestStatus response;

            var account = AccountStatus.Instance.AccountData;
            if (MainForm.serverProxyTrade.proxy == null || account == null)
            {
                errorStr = EnumFriendlyName<AccountConnectionStatus>.GetString(AccountConnectionStatus.NotConnected);
                return;
            } 
            List<PendingOrder> orders;
            try
            {
                response = TradeSharpAccount.Instance.proxy.GetPendingOrders(AccountStatus.Instance.accountID, out orders);
            }
            catch (Exception)
            {
                return;
            }
            if (response != RequestStatus.OK) return;

            foreach (var order in orders.Where(order => order.ID == orderId))
            {
                MainForm.Instance.SendDeletePendingOrderRequestSafe(order, 
                    PendingOrderStatus.Отменен, null, EnumFriendlyName<PendingOrderStatus>.GetString(PendingOrderStatus.Отменен));
                return;
            }
        }
        
        private void GridPendingOrdersSelectionChanged(MouseEventArgs e, int rowIndex, FastColumn col)
        {
            var rows = gridPendingOrders.rows.Where(r => r.Selected).ToList();
            if (rows.Count == 0)
            {
                btnEditPendingOrder.Enabled = false;
                btnDeletePendingOrders.Text = "Удалить все ордера";
                btnDeletePendingOrders.Enabled = true;
                btnDeletePendingOrder.Enabled = false;
                return;
            }
            if (rows.Count > 0)
            {
                var symbol = ((PendingOrder) (rows[0].ValueObject)).Symbol;
                btnDeletePendingOrders.Text = string.Format("Удалить все ордера \"{0}\"", symbol);
                btnEditPendingOrder.Enabled = true;
                btnDeletePendingOrder.Enabled = true;
                btnDeletePendingOrders.Enabled = true;
                return;
            }            
        }
        
        /// <summary>
        /// на переключение вкладки - если выбрана вкладка истории позиций - обновить ее содержимое
        /// (асинхронно!)
        /// </summary>        
        private void TabControlSelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == pageHistoryAccount)
            {
                accountHistoryCtrl.Enabled = false;
                panelHistoryOrdersLoadingStatus.IsShown = true;
                panelHistoryOrdersLoadingStatus.Visible = true;
                // показать историю закрытых позиций
                workerLoadHistory.RunWorkerAsync();
            }
        }

        private void LoadHistoryOrdersInBackground(object o, DoWorkEventArgs e)
        {
            e.Result = null;
            if (!AccountStatus.Instance.isAccountSelected) return;
            
            RequestStatus status;
            List<MarketOrder> orders;
            List<BalanceChange> transfers = null;
            try
            {
                status = TradeSharpAccount.Instance.GetHistoryOrdersUncompressed(AccountStatus.Instance.accountID, null, out orders);
                if (status == RequestStatus.OK)
                    status = TradeSharpAccount.Instance.proxy.GetBalanceChanges(AccountStatus.Instance.accountID, null,
                                                                                 out transfers);
            }
            catch (Exception ex)
            {
                Logger.Error("UpdateAccountHistory - ошибка получения ордеров и трансферов", ex);                
                return;
            }
            if (status == RequestStatus.OK)                
                e.Result = new Cortege2<List<MarketOrder>, List<BalanceChange>>(orders, transfers);
        }

        private void LoadHistoryOrdersCompleted(object o, RunWorkerCompletedEventArgs e)
        {
            var resultTyped = e.Result == null
                ? new Cortege2<List<MarketOrder>, List<BalanceChange>>(new List<MarketOrder>(), new List<BalanceChange>())
                : (Cortege2<List<MarketOrder>, List<BalanceChange>>)e.Result;
            
            // убрать сообщение о загрузке
            panelHistoryOrdersLoadingStatus.IsShown = false;
            panelHistoryOrdersLoadingStatus.Visible = false;
            accountHistoryCtrl.Enabled = true;

            // показать историю
            UpdateHistoryGrid(resultTyped.a, resultTyped.b);
        }

        #region Button handlers

        private void CloseAllBtnClick(object sender, EventArgs e)
        {
            MenuitemCloseAllClick(sender, e);
        }

        private void EditOrderBtnClick(object sender, EventArgs e)
        {
            var id = -1;
            foreach (var row in gridOpenPos.rows)
            {
                if (!row.Selected) continue;
                var deal = (MarketOrder)row.ValueObject;
                if (deal.State != PositionState.Opened) return;
                id = deal.ID;
                break;
            }

            if (id < 0) return;
            var dlg = new OrderDlg(OrderDlg.OrderDialogMode.OrderEditMarket, id);
            dlg.ShowDialog();
            ShowOpenPositions();
            Refresh();
        }

        private void NewOrderBtnClick(object sender, EventArgs e)
        {
            if (!AccountStatus.Instance.isAuthorized)
            {
                MessageBox.Show(EnumFriendlyName<RequestStatus>.GetString(RequestStatus.Unauthorized),
                    Localizer.GetString("TitleError"));
                return;
            }
            var symbol = string.Empty;
            foreach (var row in gridOpenPos.rows)
            {
                if (!row.Selected) continue;
                var deal = (MarketOrder)row.ValueObject;
                symbol = deal.Symbol;
                break;
            }

            var dlg = !string.IsNullOrEmpty(symbol)
                ? new OrderDlg(symbol) : new OrderDlg();
            dlg.State = OrderDlg.OrderDialogMode.OrderNewMarket;

            dlg.ShowDialog();
        }

        // ReSharper disable MemberCanBeMadeStatic.Local
        private void BtnNewPendingOrderClick(object sender, EventArgs e)
        // ReSharper restore MemberCanBeMadeStatic.Local
        {
            if (!AccountStatus.Instance.isAuthorized)
            {
                MessageBox.Show(EnumFriendlyName<RequestStatus>.GetString(RequestStatus.Unauthorized),
                    Localizer.GetString("TitleError"));
                return;
            }
            
            var orderType = tabControl.SelectedTab == pageDelayedOrders
                                ? OrderDlg.OrderDialogMode.OrderNewPending
                                : OrderDlg.OrderDialogMode.OrderNewMarket;
            var dlg = new OrderDlg(orderType);
            dlg.ShowDialog();
        }

        private void BtnClosePositionClick(object sender, EventArgs e)
        {
            ClosePosition();
        }

        private void BtnEditPendingOrderClick(object sender, EventArgs e)
        {
            var row = gridPendingOrders.rows.FirstOrDefault(r => r.Selected);
            if (row == null) return;
            var orderId = ((PendingOrder)row.ValueObject).ID;

            var dlg = new OrderDlg(OrderDlg.OrderDialogMode.OrderEditPending, orderId);
            dlg.ShowDialog();
        }

        private void BtnDeletePendingOrdersClick(object sender, EventArgs e)
        {
            if (gridPendingOrders.rows.Count == 0) return;
            var orders = gridPendingOrders.rows.Where(r => r.Selected).Select(r => 
                (PendingOrder) r.ValueObject).ToList();
            
            if (orders.Count > 0)
            {
                var account = AccountStatus.Instance.AccountData;
                if (MainForm.serverProxyTrade.proxy == null || account == null)
                    return;
                var symbol = orders[0].Symbol;

                if (MessageBox.Show(
                    string.Format(Localizer.GetString("MessageConfirmCancelPendingOrders") + " ({0})?", symbol),
                    Localizer.GetString("TitleConfirmation"), 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var ordersToKill = gridPendingOrders.rows.Where(r => ((PendingOrder)r.ValueObject).Symbol ==
                        symbol).Select(r => ((PendingOrder)r.ValueObject).ID).ToList();
                    RemovePendingOrders(ordersToKill);                    
                }
                return;
            }

            if (MessageBox.Show(Localizer.GetString("MessageConfirmCancelPendingOrders") + "?",
                Localizer.GetString("TitleConfirmation"), 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            
            var pendingOrders = MarketOrdersStorage.Instance.PendingOrders;
            if (pendingOrders == null) return;
            var orderList = pendingOrders.Select(o => o.ID).ToList();
            RemovePendingOrders(orderList);
        }

        private void BtnDeletePendingOrderClick(object sender, EventArgs e)
        {
            ClosePendingOrder();
        }

        // ReSharper disable MemberCanBeMadeStatic.Local
        private void BtnStatClick(object sender, EventArgs e)
        // ReSharper restore MemberCanBeMadeStatic.Local
        {
            var dlg = new AccountTradeResultsForm { InstantCalculation = true, MdiParent = MainForm.Instance };
            dlg.Show();
        }

        private void BtnEstimateRiskClick(object sender, EventArgs e)
        {
            if (!AccountStatus.Instance.isAuthorized ||
                AccountStatus.Instance.AccountData == null)
            {
                MessageBox.Show(EnumFriendlyName<RequestStatus>.GetString(RequestStatus.Unauthorized));
                return;
            }

            var orders = MarketOrdersStorage.Instance.MarketOrders;
            if (orders.Count == 0)
            {
                MessageBox.Show(Localizer.GetString("MessageNoOpenPositions"));
                return;
            }

            var summary = PositionSummary.GetPositionSummary(orders, AccountCurrency, 
                (float)AccountStatus.Instance.AccountData.Balance);
            var portfolio = new List<PortfolioActive>();
            foreach (var pos in summary)
            {
                if (string.IsNullOrEmpty(pos.Symbol)) continue;
                var act = new PortfolioActive {Ticker = pos.Symbol, Side = pos.Side, Leverage = pos.Leverage, Price = pos.AveragePrice };
                portfolio.Add(act);
            }

            var dlg = new PortfolioRiskForm {portfolio = portfolio};
            dlg.ShowDialog();
        }

        #endregion

        #region Menu handlers

        private void DgSummaryContextMenuRequested(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            // почистить меню от временных записей
            for (var i = 0; i < contextMenuSummaryOrder.Items.Count; i++)
            {
                if (contextMenuSummaryOrder.Items[i].Tag != null)
                    if (contextMenuSummaryOrder.Items[i].Tag is string)
                    {
                        contextMenuSummaryOrder.Items.RemoveAt(i);
                        i--;
                    }
            }
            if (rowIndex < 0)
            {
                menuItemCloseAll1.Visible = false;
                menuItemConfigColumns1.Visible = true;
            }
            else
            {
                menuItemCloseAll1.Visible = true;
                menuItemConfigColumns1.Visible = false;
                var selRows = dgSummary.rows.Where(r => r.Selected).ToList();
                if (selRows.Count > 0)
                    for (var i = 0; i < dgSummary.Columns.Count; i++)
                    {
                        if (dgSummary.Columns[i].PropertyName != "Symbol") continue;
                        var symbol = selRows[0].cells[i].CellString;
                        
                        // закрыть все
                        var newItem = contextMenuSummaryOrder.Items.Add(string.Format(
                            Localizer.GetString("TitleCloseAll") + " {0}", symbol));
                        newItem.Tag = symbol;
                        newItem.Click += OnMenuCloseSymbol;
                        
                        newItem = contextMenuSummaryOrder.Items.Add(
                            string.Format(Localizer.GetString("TitleEditGroup") + " {0}", symbol));
                        newItem.Tag = symbol;
                        newItem.Click += OnMenuEditOverallSymbol;
                        break;
                    }
            }
            contextMenuSummaryOrder.Show(dgSummary, e.Location);
        }

        private void GridOpenPosContextMenuRequested(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            // почистить меню от временных записей
            for (var i = 0; i < contextMenuOrder.Items.Count; i++)
            {
                if (contextMenuOrder.Items[i].Tag != null)
                    if (contextMenuOrder.Items[i].Tag is string)
                    {
                        contextMenuOrder.Items.RemoveAt(i);
                        i--;
                    }
            }
            if (rowIndex < 0)
            {
                menuitemCloseOrder.Visible = false;
                menuitemModifyOrder.Visible = false;
                menuitemCloseAll.Visible = false;
                menuitemSetTrailing.Visible = false;
                menuItemConfigColumns.Visible = true;
            }
            else
            {
                menuitemCloseAll.Visible = true;
                menuitemSetTrailing.Visible = true;
                menuItemConfigColumns.Visible = false;
                var selRows = gridOpenPos.rows.Where(r => r.Selected).ToList();
                if (selRows.Count == 0)
                {
                    menuitemCloseOrder.Visible = false;
                    menuitemModifyOrder.Visible = false;
                }
                else
                {
                    var deal = ((MarketOrder) selRows[0].ValueObject);
                    menuitemCloseOrder.Visible = deal.State == PositionState.Opened;
                    menuitemModifyOrder.Visible = deal.State == PositionState.Opened;
                }
            }
            contextMenuOrder.Show(gridOpenPos, e.Location);
        }

        private void GridPendingOrdersContextMenuRequested(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            var pendingOrders = MarketOrdersStorage.Instance.PendingOrders;
            MenuDeleteOrder.Enabled = pendingOrders.Count != 0;
            MenuChangeOrder.Enabled = pendingOrders.Count != 0;
            MenuDeleteAllOrders.Enabled = pendingOrders.Count != 0;

            if (rowIndex < 0)
            {
                menuItemConfigColumns2.Visible = true;
                MenuDeleteAllOrders.Visible = false;
                MenuDeleteOrder.Visible = false;
                MenuChangeOrder.Visible = false;
            }
            else
            {
                var orders = gridPendingOrders.rows.Where(r => r.Selected).Select(r => (PendingOrder) r.ValueObject).ToList();
                menuItemConfigColumns2.Visible = false;
                if (orders.Count > 1)
                {
                    // если выделено несколько узлов
                    var symbol = orders[0].Symbol;
                    var newItem = contextMenuOrder.Items.Add(string.Format(
                        Localizer.GetString("TitleCloseAll") + " {0}", symbol));
                    newItem.Tag = symbol;
                    newItem.Click += OnMenuCloseSymbol;
                    MenuDeleteAllOrders.Visible = true;
                    MenuDeleteOrder.Visible = false;
                    MenuChangeOrder.Visible = false;
                    return;
                }
                else if (orders.Count == 1)
                {
                    // выделен один узел
                    menuitemCloseAll.Visible = true;
                    menuitemCloseOrder.Visible = true;
                    menuitemModifyOrder.Visible = true;
                }
                else
                {
                    // скрыть остальные опции
                    MenuDeleteAllOrders.Visible = false;
                    MenuDeleteOrder.Visible = false;
                    MenuChangeOrder.Visible = false;
                }
            }
            contextMenuPending.Show(gridPendingOrders, e.Location);
        }

        private void MenuitemModifyOrderClick(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == pageOpenOrders)
                EditOrderBtnClick(sender, e);
            else
                if (tabControl.SelectedTab == pageDelayedOrders)
                    BtnEditPendingOrderClick(sender, e);
        }

        /// <summary>
        /// закрыть все сделки по инструменту
        /// </summary>        
        private void OnMenuCloseSymbol(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == pageOpenOrders)
            {
                var account = AccountStatus.Instance.AccountData;
                if (MainForm.serverProxyTrade.proxy == null || account == null)
                    return;

                var positions = MarketOrdersStorage.Instance.MarketOrders;
                var symbol = ((ToolStripItem)sender).Tag.ToString();
                var ordersList = (from poz in positions where poz.Symbol == symbol && poz.State == PositionState.Opened select poz.ID).ToList();

                ClosePositions(ordersList);
            }
            else
                if (tabControl.SelectedTab == pageDelayedOrders)
                {
                    var ordersToClose = gridPendingOrders.rows.Where(r => r.Selected).Select(n => ((PendingOrder)n.ValueObject).ID).ToList();

                    if (ordersToClose.Count == 0) return;
                    var account = AccountStatus.Instance.AccountData;
                    if (MainForm.serverProxyTrade.proxy == null || account == null)
                        return;
                    RemovePendingOrders(ordersToClose);
                }
        }

        /// <summary>
        /// Выставить тейки на все сделки по инструменту
        /// </summary>
        private void OnMenuEditOverallSymbol(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab != pageOpenOrders) return;

            var symbol = ((ToolStripItem)sender).Tag.ToString();
            EditOverallSymbol(symbol);
        }

        /// <summary>
        /// закрыть одну сделку
        /// </summary>        
        private void MenuitemCloseOrderClick(object sender, EventArgs e)
        {
            ClosePosition();
        }

        // ReSharper disable MemberCanBeMadeStatic.Local
        private void MenuitemCloseAllClick(object sender, EventArgs e)
        // ReSharper restore MemberCanBeMadeStatic.Local
        {
            var account = AccountStatus.Instance.AccountData;
            if (MainForm.serverProxyTrade.proxy == null || account == null)
                return;

            var positions = MarketOrdersStorage.Instance.MarketOrders;
            var orderList = positions.Where(pos => pos.State == PositionState.Opened).Select(o => o.ID);
            if (orderList.ToList().Count == 0)
                return;
            if (MessageBox.Show(Localizer.GetString("MessageConfirmClosingAllOrders") + "?",
                Localizer.GetString("TitleConfirmation"), 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            ClosePositions(orderList.ToList());
        }

        private void MenuitemSetTrailingClick(object sender, EventArgs e)
        {
            var deal = gridOpenPos.rows.Where(row => row.Selected).Select(row => (MarketOrder)row.ValueObject).FirstOrDefault();

            if (deal == null || deal.State != PositionState.Opened) return;
            var dlg = new TrailingsFrom(deal);
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            //curr.ValueObject = dlg.deal;
            RefreshOpenPositionsGrid(MarketOrdersStorage.Instance.MarketOrders);
            gridOpenPos.Refresh();
        }

        private void MenuNewOrderClick(object sender, EventArgs e)
        {
            BtnNewPendingOrderClick(sender, e);
        }

        private void MenuDeleteAllOrdersClick(object sender, EventArgs e)
        {
            BtnDeletePendingOrdersClick(sender, e);
        }

        private void MenuDeleteOrderClick(object sender, EventArgs e)
        {
            ClosePendingOrder();
        }

        private void MenuChangeOrderClick(object sender, EventArgs e)
        {
            BtnEditPendingOrderClick(sender, e);
        }

        private void ConfigDgSummaryGridColumns(object sender, EventArgs e)
        {
            ConfigFastGridColumns(dgSummary, GridSettings.ListAccountSummaryOrders);
        }

        private void ConfigGridOpenPosColumns(object sender, EventArgs e)
        {
            ConfigFastGridColumns(gridOpenPos, GridSettings.ListAccountOpenedOrders);
        }

        private void ConfigGridPendingOrdersColumns(object sender, EventArgs e)
        {
            ConfigFastGridColumns(gridPendingOrders, GridSettings.ListAccountPendingOrders);
        }

        private void ConfigHistoryGridColumns(FastGrid.FastGrid grid)
        {
            ConfigFastGridColumns(grid, GridSettings.ListAccountClosedOrders);
        }

        private void ConfigFastGridColumns(FastGrid.FastGrid grid, string gridCode)
        {
            var columnConfigurator = new ColumnConfigurator(grid);
            if (columnConfigurator.ShowDialog(this) == DialogResult.Cancel)
                return;
            var sets = GridSettings.EnsureSettings(gridCode);
            sets.DeriveFromGrid(grid);
        }

        #endregion

        private void AccountMonitorFormMove(object sender, EventArgs e)
        {
            if (formMoved != null)
                formMoved(this);
        }

        private void AccountMonitorFormResizeEnd(object sender, EventArgs e)
        {
            if (resizeEnded != null)
                resizeEnded(this);
        }
    }
    // ReSharper restore LocalizableElement
    // ReSharper restore ParameterTypeCanBeEnumerable.Local
}

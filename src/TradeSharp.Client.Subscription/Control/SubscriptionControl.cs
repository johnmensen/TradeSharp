using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TradeSharp.Chat.Client.BL;
using TradeSharp.Client.Subscription.Dialog;
using TradeSharp.Client.Subscription.Model;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.SiteBridge.Lib.Finance;
using TradeSharp.Util;

namespace TradeSharp.Client.Subscription.Control
{
    public partial class SubscriptionControl : UserControl
    {
        struct PerformersRequest
        {
            public string function;

            public bool ascending;

            public int count;

            public float? marginValue;

            public int ServiceTypeMask;
        }

        enum PerformerSortOrder
        {
            Ascending, Descending
        }

        public enum ActivePage { Subscription = 0, Signals , Performers, Tops }

        public event ChatControlBackEnd.EnterRoomDel EnterRoomRequested;

        private Color[] refreshButtonColors;
        private int refreshButtonColorIndex;
        private Action closeHandler;
        private ChatControlBackEnd chat;
        private List<PerformerStatEx> performers;
        private readonly BackgroundWorker performersWorker = new BackgroundWorker();
        private readonly BackgroundWorker performerFilterWorker = new BackgroundWorker();
        private int extendedPanelHeight;
        private List<TopPortfolio> companyPortfolios;
        private int subscribedPortfolioId;
        private readonly BackgroundWorker portfoliosWorker = new BackgroundWorker();
        private readonly BackgroundWorker myPortfoliosWorker = new BackgroundWorker();

        // видимость панели фильтров
        // тут изменяется только внешний вид и поведение performerSplitContainer
        private bool isExtendedPanelVisible = true;
        private bool IsExtendedPanelVisible
        {
            get { return isExtendedPanelVisible; }
            set
            {
                if (value == isExtendedPanelVisible)
                    return;
                if (value)
                {
                    performerSplitContainer.Panel1MinSize = 50;
                    performerSplitContainer.SplitterDistance = extendedPanelHeight;
                    performerSplitContainer.IsSplitterFixed = false;
                }
                else
                {
                    extendedPanelHeight = performerSplitContainer.SplitterDistance;
                    performerSplitContainer.Panel1MinSize = 0;
                    performerSplitContainer.SplitterDistance = 0;
                    performerSplitContainer.IsSplitterFixed = true;
                }
                isExtendedPanelVisible = value;
            }
        }

        private int ParticipantCount
        {
            get
            {
                if (countComboBox.DropDownStyle == ComboBoxStyle.DropDownList)
                    return countComboBox.SelectedItem.ToString().ToInt(10);
                if (countComboBox.DropDownStyle == ComboBoxStyle.DropDown)
                    return countComboBox.Text.ToInt(10);
                return 10;
            }
        }

        private PerformerCriteriaFunction SelectedFunction
        {
            get { return topFilterControl.SelectedFunction; }
            set { topFilterControl.SelectedFunction = new PerformerCriteriaFunction(value); }
        }

        private SortOrder SelectedFunctionSortOrder
        {
            get { return isExtendedPanelVisible ? topFilterControl.SortOrder : SelectedFunction.PreferredSortOrder; }
        }

        public SubscriptionControl()
        {
            InitializeComponent();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ActivePage Page
        {
            get
            {
                return tabControl.SelectedTab == tabPagePerformers
                           ? ActivePage.Performers
                           : tabControl.SelectedTab == tabPageSignal ? ActivePage.Signals
                           : tabControl.SelectedTab == tabPageCompanyPortfolios ? ActivePage.Tops 
                           : ActivePage.Subscription;
            }
            set
            {
                if (value == ActivePage.Performers)
                    tabControl.SelectedTab = tabPagePerformers;
                else if (value == ActivePage.Signals)
                    tabControl.SelectedTab = tabPageSignal;
                else if (value == ActivePage.Tops)
                    tabControl.SelectedTab = tabPageCompanyPortfolios;
                else
                    tabControl.SelectedTab = tabPageSubscription;
            }
        }

        public void Initialize(Action closeHandler,
            TradeSharpServerTrade serverProxyAccount,
            Action<TradeSignalUpdate> signalUpdateSelected,
            Func<Account> getActualAccountData,
            Func<string> getUserLogin,
            Action<PerformerStat> investInPAMM,
            SavePerformersGridSelectedColumnsDel savePerformersGridSelectedColumns,
            LoadPerformersGridSelectedColumnsDel loadPerformersGridSelectedColumns,
            Func<ActionOnSignal> getActionOnSignal,
            Action<ActionOnSignal> setActionOnSignal,
            ChatControlBackEnd chat)
        {
            var gr = CreateGraphics();

            this.closeHandler = closeHandler;
            this.chat = chat;
            AccountModel.Instance.Initialize(getActualAccountData, getUserLogin, investInPAMM, serverProxyAccount.proxy, chat);
            SubscriptionModel.Instance.getActionOnSignal = getActionOnSignal;
            SubscriptionModel.Instance.setActionOnSignal = setActionOnSignal;
            subscriptionGrid.ShowTopPortfolio += portfolio =>
                {
                    LoadPortfolioSubscribers(portfolio);
                    Page = ActivePage.Performers;
                };

            // strategies
            var criterias = PerformerCriteriaFunctionCollection.Instance.criterias;
            var criteriaTitles = criterias.Select(c => c.Description).ToList();
            criteriaComboBox.Items.AddRange(criteriaTitles.Select(c => c as object).ToArray());
            criteriaComboBox.DropDownWidth = criteriaTitles.Max(c => gr.MeasureString(c, Font).ToSize().Width);
            criteriaComboBox.Width = criteriaComboBox.DropDownWidth + SystemInformation.VerticalScrollBarWidth;

            var indexInCombo = countComboBox.Items.Cast<string>().IndexOf("50");
            countComboBox.SelectedIndex = indexInCombo < 0 ? 0 : indexInCombo;

            sortOrderComboBox.Items.AddRange(EnumItem<PerformerSortOrder>.items.Cast<object>().ToArray());

            SetupSortCombos(gr);

            criteriaComboBox.SelectedIndex = criterias.IndexOf(PerformerCriteriaFunctionCollection.Instance.SelectedFunction);

            //topFilterControl.SortOrder = sortOrderComboBox.SelectedIndex == 0 ? SortOrder.Ascending : SortOrder.Descending;
            topFilterControl.PerformerCriteriaFunctionCollectionChanged += OnPerformerCriteriaFunctionCollectionChanged;
            topFilterControl.PerformerCriteriaFunctionChanged += OnPerformerCriteriaFunctionChanged;
            topFilterControl.CollapseButtonClicked += CollapseTopFilterControlButtonClick;
            topFilterControl.RefreshButtonClicked += RefreshButtonClick;
            topFilterControl.CreatePortfolioButtonClicked += CreatePortfolioButtonClick;

            IsExtendedPanelVisible = false;

            performersWorker.DoWork += GetPerformers;
            performersWorker.RunWorkerCompleted += GetPerformersCompleted;

            performerFilterWorker.DoWork += GetPerformersByFilters;
            performerFilterWorker.RunWorkerCompleted += GetPerformersByFiltersCompleted;

            // 4 refresh button
            refreshButtonColors = new[]
                {
                    refreshButton.BackColor,
                    HslColor.AdjuctBrightness(refreshButton.BackColor, 0.9f),
                    HslColor.AdjuctBrightness(refreshButton.BackColor, 0.85f),
                    HslColor.AdjuctBrightness(refreshButton.BackColor, 0.8f),
                    HslColor.AdjuctBrightness(refreshButton.BackColor, 0.85f),
                    HslColor.AdjuctBrightness(refreshButton.BackColor, 0.9f),
                    refreshButton.BackColor,
                    HslColor.AdjuctBrightness(refreshButton.BackColor, 1.1f),
                    HslColor.AdjuctBrightness(refreshButton.BackColor, 1.15f),
                    HslColor.AdjuctBrightness(refreshButton.BackColor, 1.2f),
                    HslColor.AdjuctBrightness(refreshButton.BackColor, 1.15f),
                    HslColor.AdjuctBrightness(refreshButton.BackColor, 1.1f)
                };
            topFilterControl.RefreshButtonEnabled = false;
            refreshButton.Enabled = false;

            performerGridCtrl.LoadPerformersGridSelectedColumns = loadPerformersGridSelectedColumns;
            performerGridCtrl.SavePerformersGridSelectedColumns = savePerformersGridSelectedColumns;
            performerGridCtrl.EnterRoomRequested += OnEnterRoomRequested;
            performerGridCtrl.PageTargeted += page => Page = page;
            performerGridCtrl.SetupGrid();

            signalFastGrid.SignalUpdateSelected += signalUpdateSelected;

            // company top portfolios
            portfoliosWorker.DoWork += GetPortfoloios;
            portfoliosWorker.RunWorkerCompleted += GetPortfoloiosCompleted;

            // my top
            myPortfoliosWorker.DoWork += GetMyPortfolio;
            myPortfoliosWorker.RunWorkerCompleted += GetMyPortfolioCompleted;

            signalsToolStripMenuItem.CheckedChanged += ParametersContextMenuStripItemClicked;
            pammsToolStripMenuItem.CheckedChanged += ParametersContextMenuStripItemClicked;
        }

        private void SetupSortCombos(Graphics gr)
        {
            sortComboBox.Items.AddRange(PerformerStatField.fields.Cast<object>().ToArray());
            sortComboBox.DropDownWidth = PerformerStatField.fields.Max(p => gr.MeasureString(p.Title, Font).ToSize().Width);
            sortComboBox.Width = sortComboBox.DropDownWidth + SystemInformation.VerticalScrollBarWidth;

            sortComboBox.SelectedIndex = 0;
            if (sortComboBox.Items.Count > 0)
                topFilterControl.SortFieldIndex = sortComboBox.SelectedIndex;
            sortComboBox.SelectedIndexChanged += SortComboBoxOnSelectedIndexChanged;

            sortOrderComboBox.SelectedIndex = 0;
            sortOrderComboBox.SelectedIndexChanged += (sender, args) =>
                {
                    topFilterControl.SortOrder = sortOrderComboBox.SelectedIndex == 0
                                                     ? SortOrder.Ascending
                                                     : SortOrder.Descending;
                    topFilterControl.RefreshButtonEnabled = true;
                };
        }

        private void SortComboBoxOnSelectedIndexChanged(object sender, EventArgs eventArgs)
        {
            var item = (PerformerStatField) sortComboBox.SelectedItem;
            if (item == null)
                return;
            topFilterControl.SortFieldIndex = sortComboBox.SelectedIndex;
            topFilterControl.RefreshButtonEnabled = true;
            if (item.DefaultSortOrder != System.Data.SqlClient.SortOrder.Unspecified)
                sortOrderComboBox.SelectedIndex = 
                    item.DefaultSortOrder == System.Data.SqlClient.SortOrder.Descending ? 1 : 0;
        }

        private void LoadPortfolioSubscribers(TopPortfolio portfolio)
        {
            var perfs = new List<PerformerStatEx>();
            var cats = SubscriptionModel.Instance.SubscribedCategories ?? new List<Contract.Entity.Subscription>();
            try
            {
                perfs.AddRange(portfolio.Managers.Select(performer => new PerformerStatEx(performer)
                {
                    IsSubscribed = cats.Any(c => c.Service == performer.Service),
                }));
            }
            catch (Exception ex)
            {
                Logger.Info("SubscriptionControl.LoadPortfolioSubscribers", ex);
            }
            performers = perfs;
            performerGridCtrl.DataBind(performers, chat);
            topFilterControl.RefreshButtonEnabled = true;
            refreshButton.Enabled = true;
        }

        private void OnEnterRoomRequested(string name, string password = "")
        {
            if(EnterRoomRequested != null)
                EnterRoomRequested(name, password);
        }

        private void CheckSubscribersLoaded()
        {
            if (performers == null)
                LoadSubscribers();
        }

        /// <summary>
        /// заполнить список сигнальщиков
        /// </summary>
        private void LoadSubscribers()
        {
            if (performersWorker.IsBusy)
                return;
            performerPanel.Visible = false;
            performersStandByControl.Visible = true;
            try
            {
                performersWorker.RunWorkerAsync(new PerformersRequest
                    {
                        function = SelectedFunction.Function,
                        ascending = SelectedFunctionSortOrder == SortOrder.Ascending,
                        count = ParticipantCount,
                        marginValue = SelectedFunction.MarginValue,
                        ServiceTypeMask = (signalsToolStripMenuItem.Checked ? 1 : 0) + (pammsToolStripMenuItem.Checked ? 2 : 0)
                    });
            }
            catch (Exception ex)
            {
                Logger.Info("SubscriptionControl.LoadSubscribers", ex);
            }
        }

        private void GetPerformers(object sender, DoWorkEventArgs e)
        {
            var perfs = new List<PerformerStatEx>();
            var cats = SubscriptionModel.Instance.SubscribedCategories ?? new List<Contract.Entity.Subscription>();
            try
            {
                var args = (PerformersRequest) e.Argument;
                var performersFromServer = TradeSharpAccountStatistics.Instance.proxy.GetAllPerformersWithCriteria(true, args.function,
                    args.count, args.ascending, args.marginValue, args.ServiceTypeMask);
                if (performersFromServer == null)
                    return;
                perfs.AddRange(performersFromServer.Select(performer => new PerformerStatEx(performer)
                    {
                        IsSubscribed = cats.Any(c => c.Service == performer.Service),
                    }));
            }
            catch (Exception ex)
            {
                Logger.Info("SubscriptionControl.GetPerformers", ex);
            }
            e.Result = perfs;
        }
        
        private void GetPerformersCompleted(object o, RunWorkerCompletedEventArgs args)
        {
            performers = (List<PerformerStatEx>) args.Result;
            if (performers == null)
            {
                MessageBox.Show(this,
                                "Невозможно получить информацию о подписчиках",
                                "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            performerGridCtrl.DataBind(performers, chat); // TODO: тормозные операции DataBind выполнять в отдельном потоке
            performersStandByControl.Visible = false;
            performerPanel.Visible = true;
        }

        private void GetCompanyTopPortfoloios()
        {
            if (portfolioContainer.AllPortfoliosLoaded)
                return;
            portfolioContainer.Visible = false;
            portfoliosStandByControl.IsShown = true;
            portfoliosStandByControl.Visible = true;
            portfoliosWorker.RunWorkerAsync();
        }

        private void GetPortfoloios(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            var portfolios = new List<TopPortfolio>();

            try
            {
                var portfolioIds = TradeSharpAccountStatistics.Instance.proxy.GetCompanyTopPortfolios();

                foreach (var portfolioId in portfolioIds)
                {
                    AccountEfficiency efficiency;
                    var portfolio = TradeSharpAccountStatistics.Instance.proxy.GetTopPortfolio(portfolioId, out efficiency);
                    portfolios.Add(portfolio);
                }
            }
            catch (TimeoutException ex)
            {
                Logger.Error("GetPortfoloios() - Возможно, отсутствует соединение с сетью интернет", ex);
            }
            catch (Exception ex)
            {
                Logger.Error("GetPortfoloios()", ex);
            }  
            var subscribedId = TradeSharpAccountStatistics.Instance.proxy.GetSubscribedTopPortfolioId(AccountModel.Instance.GetUserLogin());
            doWorkEventArgs.Result = new Cortege2<List<TopPortfolio>, int>(portfolios, subscribedId);
        }

        private void GetPortfoloiosCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            if (runWorkerCompletedEventArgs.Result == null)
            {
                MessageBox.Show(this,
                                "Невозможно получить информацию о портфелях",
                                "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            var result = (Cortege2<List<TopPortfolio>, int>) runWorkerCompletedEventArgs.Result;
            companyPortfolios = result.a;
            subscribedPortfolioId = result.b;
            portfolioContainer.SetPortfolios(companyPortfolios, subscribedPortfolioId);
            portfoliosStandByControl.IsShown = false;
            portfoliosStandByControl.Visible = false;
            portfolioContainer.Visible = true;
        }

        private void TabControlSelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == tabPagePerformers)
                CheckSubscribersLoaded();
            else if (tabControl.SelectedTab == tabPageSignal)
                LoadTradeSignals(SubscriptionModel.Instance.SubscribedCategories);
            else if (tabControl.SelectedTab == tabPageCompanyPortfolios)
                GetCompanyTopPortfoloios();
            else if (tabControl.SelectedTab == tabPageMyPortfolios)
                CreateMyPortfoliosTabPage(); // TODO: move to another thread
        }

        private void CreateMyPortfoliosTabPage()
        {
            if (tabPageMyPortfolios.Controls.Count == 2)
                tabPageMyPortfolios.Controls.RemoveAt(1);
            myPortfolioStandByControl.IsShown = true;
            myPortfolioStandByControl.Show();
            myPortfoliosWorker.RunWorkerAsync();
        }

        private void GetMyPortfolio(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            try
            {
                subscribedPortfolioId = TradeSharpAccountStatistics.Instance.proxy.GetSubscribedTopPortfolioId(AccountModel.Instance.GetUserLogin());
                if (subscribedPortfolioId == -1)
                {
                    doWorkEventArgs.Result = new Cortege2<TopPortfolio, AccountEfficiency>();
                    return;
                }
                AccountEfficiency efficiency;
                var portfolio = TradeSharpAccountStatistics.Instance.proxy.GetTopPortfolio(subscribedPortfolioId, out efficiency);
                if (portfolio == null)
                    return;

                // generating stat
                if (!portfolio.IsCompanyPortfolio)
                {
                    //portfolioControl.Efficiency = GeneratePortfolioAccountEfficiency(portfolio, proxy);
                    var stat = GeneratePortfolioAccountEfficiency(efficiency);
                    portfolio.Statistics = stat;
                }

                doWorkEventArgs.Result = new Cortege2<TopPortfolio, AccountEfficiency>(portfolio, efficiency);
            }
            catch (Exception ex)
            {
                Logger.Info("SubscriptionControl.CreateMyPortfoliosTabPage", ex);
            }
        }

        private void GetMyPortfolioCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            if (runWorkerCompletedEventArgs.Result == null)
            {
                MessageBox.Show(this,
                                "Невозможно получить информацию о портфелях",
                                "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                myPortfolioStandByControl.IsShown = false;
                myPortfolioStandByControl.Hide();
                return;
            }
            var result = (Cortege2<TopPortfolio, AccountEfficiency>) runWorkerCompletedEventArgs.Result;
            myPortfolioStandByControl.IsShown = false;
            myPortfolioStandByControl.Hide();
            if (result.a == null)
                return;
            var portfolioControl = new TopPortfolioControl
            {
                ShowFullInfo = true,
                HideStrategies = true,
                LoadAllData = true,
                IsSubsribed = true,
                Dock = DockStyle.Fill
            };
            // adding
            if (result.b != null)
                portfolioControl.Efficiency = result.b;
            portfolioControl.Portfolio = result.a;
            portfolioControl.SubscriptionChanged += (s, args) => CreateMyPortfoliosTabPage();
            tabPageMyPortfolios.Controls.Add(portfolioControl);
        }

        private static PerformerStat GeneratePortfolioAccountEfficiency(AccountEfficiency efficiency)
        {
            if (!AccountModel.Instance.AccountId.HasValue || efficiency.listProfit1000.Count == 0)
                return null;

            // pofit calc
            efficiency.listEquity = new List<EquityOnTime>();
            float equity = 10000;
            if (AccountModel.Instance.Account != null && AccountModel.Instance.Account != null)
                equity = (float) AccountModel.Instance.Account.Equity;
            // пусть performerStat.Equity соответствует последней точке на кривой доходности на 1000
            var profitFactor = equity / efficiency.listProfit1000.Last().equity;
            foreach (var equityOnTime in efficiency.listProfit1000)
            {
                efficiency.listEquity.Add(new EquityOnTime(equityOnTime.equity * profitFactor, equityOnTime.time));
            }

            // stats calc
            efficiency.listTransaction = new List<BalanceChange>();
            efficiency.InitialBalance = efficiency.listEquity[0].equity;
            efficiency.StartDate = efficiency.listEquity[0].time;
            new EfficiencyCalculator().CalculateProfitCoeffs(efficiency);

            var stat = efficiency.Statistics;
                //new PerformerStat
                //{
                //    Profit = efficiency.TWR,
                //    GreedyRatio = efficiency.GreedyRatio,
                //    MaxRelDrawDown = efficiency.MaxDrawdown,
                //    Sharp = efficiency.Sharp
                //};
            //stat.MaxLeverage = efficiency.MaxLeverage;
            return stat;
        }

        private static AccountEfficiency GeneratePortfolioAccountEfficiency(TopPortfolio portfolio, IAccountStatistics proxy)
        {
            var portfolioEfficiency = new AccountEfficiency();
            var efficiencies = new List<AccountEfficiency>();

            // detect beginning
            DateTime? beginDate = null;
            foreach (var performerStat in portfolio.Managers)
            {
                var efficiency = proxy.GetAccountEfficiencyShort(performerStat.Account, false, false);
                efficiencies.Add(efficiency);
                var firstDate = efficiency.listProfit1000.Min(e => e.time);
                if (!beginDate.HasValue)
                    beginDate = firstDate;
                else
                    if (firstDate < beginDate.Value)
                        beginDate = firstDate;
            }
            if (!beginDate.HasValue)
                return null;

            // pofit1000 calc
            portfolioEfficiency.listProfit1000 = new List<EquityOnTime>();
            portfolioEfficiency.listEquity = new List<EquityOnTime>();
            for (var date = beginDate.Value; date < DateTime.Now; date = date.AddDays(1))
            {
                float equity = 0;
                foreach (var efficiency in efficiencies)
                {
                    var equityOnTime = efficiency.listProfit1000.Find(e => e.time == date);
                    if (equityOnTime.time == default(DateTime))
                        continue;
                    equity += equityOnTime.equity;
                }
                equity /= efficiencies.Count;
                portfolioEfficiency.listProfit1000.Add(new EquityOnTime(equity, date));
            }
            if (!AccountModel.Instance.AccountId.HasValue || portfolioEfficiency.listProfit1000.Count == 0)
                return null;

            // pofit calc
            var stat = proxy.GetPerformerByAccountId(AccountModel.Instance.AccountId.Value);
            // пусть performerStat.Equity соответствует последней точке на кривой доходности на 1000
            var profitFactor = stat.Equity / portfolioEfficiency.listProfit1000.Last().equity;
            foreach (var equityOnTime in portfolioEfficiency.listProfit1000)
            {
                portfolioEfficiency.listEquity.Add(new EquityOnTime(equityOnTime.equity * profitFactor, equityOnTime.time));
            }

            // stats calc
            portfolioEfficiency.listTransaction = new List<BalanceChange>();
            portfolioEfficiency.InitialBalance = portfolioEfficiency.listEquity[0].equity;
            portfolioEfficiency.StartDate = portfolioEfficiency.listEquity[0].time;
            new EfficiencyCalculator().CalculateProfitCoeffs(portfolioEfficiency);

            return portfolioEfficiency;
        }

        private void LoadTradeSignals(List<Contract.Entity.Subscription> subscribedCategories)
        {
            if (subscribedCategories == null || subscribedCategories.Count == 0)
            {
                return;
            }

            // получить собственно сигналы (из файлового хранилища)
            var updates = TradeSignalFileStorage.Instance.GetAllTradeSignalUpdates();
            if (updates == null || updates.Count == 0)
            {
                return;
            }

            // поставить сигналам в соответствие имена
            foreach (var update in updates)
            {
                var updateCat = update.ServiceId;
                var cat = subscribedCategories.FirstOrDefault(c => c.Service == updateCat);
                if (cat != null) update.CategoryName = cat.Title;
            }

            var signals = updates.Where(u => !string.IsNullOrEmpty(u.CategoryName)).ToList();
            signalFastGrid.BindData(signals);
        }

        private void ExpandTopFilterControlButtonClick(object sender, EventArgs e)
        {
            refreshButton.Visible = false;
            IsExtendedPanelVisible = true;
            expandTopFilterControlButton.Visible = false;
            createPortfolioButton.Visible = false;
            sortLabel.Visible = true;
            sortComboBox.Visible = true;
            sortOrderComboBox.Visible = true;
        }

        private void CollapseTopFilterControlButtonClick(object sender, EventArgs e)
        {
            refreshButton.Visible = true;
            IsExtendedPanelVisible = false;
            expandTopFilterControlButton.Visible = true;
            createPortfolioButton.Visible = true;
            sortLabel.Visible = false;
            sortComboBox.Visible = false;
            sortOrderComboBox.Visible = false;
        }

        private void OnPerformerCriteriaFunctionCollectionChanged(object sender, EventArgs e)
        {
            criteriaComboBox.Items.Clear();
            var criterias = PerformerCriteriaFunctionCollection.Instance.criterias;
            criteriaComboBox.Items.AddRange(criterias.Select(c => c.Description as object).ToArray());
            criteriaComboBox.SelectedIndex = criterias.IndexOf(PerformerCriteriaFunctionCollection.Instance.SelectedFunction);
        }

        private void OnPerformerCriteriaFunctionChanged(object sender, EventArgs e)
        {
            sortComboBox.SelectedIndex = topFilterControl.SortFieldIndex;
            switch (SelectedFunctionSortOrder)
            {
                case SortOrder.None:
                    sortOrderComboBox.SelectedIndex = -1;
                    break;
                case SortOrder.Ascending:
                    sortOrderComboBox.SelectedIndex = 0;
                    break;
                case SortOrder.Descending:
                    sortOrderComboBox.SelectedIndex = 1;
                    break;
            }
            sortComboBox.Enabled = SelectedFunction.IsExpressionParsed;
            sortOrderComboBox.Enabled = SelectedFunction.IsExpressionParsed;
            topFilterControl.RefreshButtonEnabled = true;
            refreshButton.Enabled = true;
        }

        private void RefreshButtonClick(object sender, EventArgs e)
        {
            LoadSubscribers();
            topFilterControl.RefreshButtonEnabled = false;
            refreshButton.Enabled = false;
            // запомнить выбор
            if (!IsExtendedPanelVisible)
                PerformerCriteriaFunctionCollection.Instance.WriteToFile();
        }

        private void CriteriaComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (criteriaComboBox.SelectedIndex == -1)
                return;
            var function = PerformerCriteriaFunctionCollection.Instance.criterias[criteriaComboBox.SelectedIndex];
            PerformerCriteriaFunctionCollection.Instance.SelectedFunction = function;
            SelectedFunction = function;
            //UpdateServiceTypeInSelectedFunction();
            topFilterControl.SetExpression(SelectedFunction);
            OnPerformerCriteriaFunctionChanged(sender, e);
        }

        private void CreatePortfolioButtonClick(object sender, EventArgs e)
        {
            var portfolioName = "TOP " + AccountModel.Instance.GetUserLogin();
            if (portfolioName.Length > 50)
                portfolioName = portfolioName.Substring(0, 50);
            var portfolio = new TopPortfolio
                {
                    Name = portfolioName,
                    Criteria = SelectedFunction.Function,
                    DescendingOrder = SelectedFunction.PreferredSortOrder == SortOrder.Descending,
                    MarginValue = SelectedFunction.MarginValue,
                    ParticipantCount = ParticipantCount
                };

            var complete = new CompleteSubscribeOnPortfolioDlg(portfolio).ShowDialog() == DialogResult.OK;
            if (!complete) return;

            // открыть диалог настройки авто-торговли
            var dlg = new AutoTradeSettingsForm();
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;
            var tradeSettings = dlg.sets;
            
            RequestStatus status;
            try
            {
                status =
                    AccountModel.Instance.ServerProxy.SubscribeOnPortfolio(
                        CurrentProtectedContext.Instance.MakeProtectedContext(),
                        AccountModel.Instance.GetUserLogin(), portfolio, null, tradeSettings);
            }
            catch (Exception ex)
            {
                //4 debug
                MessageBox.Show(this, "Операция выполнена с ошибкой:" + Environment.NewLine + ex.Message, "Предупреждение",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Logger.Info("TopPortfolioControl.Subscribe: error calling SubscribeOnPortfolio/UnsubscribePortfolio", ex);
                return;
            }
            if (status == RequestStatus.OK)
                MessageBox.Show(this, "Операция выполнена успешно", "Информация", MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            else
                MessageBox.Show(this,
                                "Операция выполнена с ошибкой:" + Environment.NewLine +
                                EnumFriendlyName<RequestStatus>.GetString(status), "Предупреждение", MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
        }

        private void CountComboBoxTextChanged(object sender, EventArgs e)
        {
            int value;
            var valid = int.TryParse(countComboBox.Text, out value);
            countComboBox.ForeColor = valid ? Color.Black : Color.Red;
            if (!valid)
                return;
            topFilterControl.RefreshButtonEnabled = true; 
            refreshButton.Enabled = true;
        }

        /// <summary>
        /// анимировать элементы интерфейса
        /// </summary>
        private void TimerWhistlerFarterTick(object sender, EventArgs e)
        {
            if (refreshButtonColors == null)
                return;
            if (!refreshButton.Enabled)
            {
                refreshButton.BackColor = refreshButtonColors[0];
                return;
            }
            refreshButton.BackColor = refreshButtonColors[refreshButtonColorIndex++];
            if (refreshButtonColorIndex >= refreshButtonColors.Length)
                refreshButtonColorIndex = 0;
        }

        private void FindButtonClick(object sender, EventArgs e)
        {
            var form = new InputPerformerFiltersForm();
            if (form.ShowDialog(this) == DialogResult.Cancel)
                return;
            performerPanel.Visible = false;
            performersStandByControl.Visible = true;
            topFilterControl.RefreshButtonEnabled = true;
            refreshButton.Enabled = true;
            performerFilterWorker.RunWorkerAsync(new Cortege2<List<PerformerSearchCriteria>, int>(
                                                     form.GetFilters(), form.GetCount()));
        }

        // "Поиск..."
        private void GetPerformersByFilters(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            var args = (Cortege2<List<PerformerSearchCriteria>, int>) doWorkEventArgs.Argument;
            var perfs = new List<PerformerStatEx>();
            var cats = SubscriptionModel.Instance.SubscribedCategories ?? new List<Contract.Entity.Subscription>();
            try
            {
                var performersFromServer = TradeSharpAccountStatistics.Instance.proxy.GetPerformersByFilter(true, args.a, args.b);
                if (performersFromServer == null)
                    return;
                perfs.AddRange(performersFromServer.Select(performer => new PerformerStatEx(performer)
                {
                    IsSubscribed = cats.Any(c => c.Service == performer.Service),
                }));
            }
            catch (Exception ex)
            {
                Logger.Info("SubscriptionControl.GetPerformersByFilters", ex);
            }
            doWorkEventArgs.Result = perfs;
        }

        private void GetPerformersByFiltersCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            performers = (List<PerformerStatEx>) runWorkerCompletedEventArgs.Result;
            if (performers == null)
            {
                MessageBox.Show(this, "Невозможно получить информацию о подписчиках", "Предупреждение",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            performerGridCtrl.DataBind(performers, chat);
            performersStandByControl.Visible = false;
            performerPanel.Visible = true;
        }

        private void ParametersButtonClick(object sender, EventArgs e)
        {
            parametersContextMenuStrip.Show(parametersButton, new Point(0, parametersButton.Height), ToolStripDropDownDirection.Default);
        }

        private void ParametersContextMenuStripItemClicked(object sender, EventArgs eventArgs)
        {
            if (signalsToolStripMenuItem.Checked && pammsToolStripMenuItem.Checked)
            {
                signalsToolStripMenuItem.Checked = false;
                pammsToolStripMenuItem.Checked = false;
            }
            if (!signalsToolStripMenuItem.Checked && !pammsToolStripMenuItem.Checked)
                parametersButton.Text = Localizer.GetString("TitleAll");
            else if (signalsToolStripMenuItem.Checked && !pammsToolStripMenuItem.Checked)
                parametersButton.Text = signalsToolStripMenuItem.Text;
            else if (!signalsToolStripMenuItem.Checked && pammsToolStripMenuItem.Checked)
                parametersButton.Text = pammsToolStripMenuItem.Text;
            /*else
                parametersButton.Text = "Все";*/
            //UpdateServiceTypeInSelectedFunction();
            topFilterControl.RefreshButtonEnabled = true;
            refreshButton.Enabled = true;
        }

        private void UpdateServiceTypeInSelectedFunction()
        {
            var stat = new PerformerStat();
            var serviceTypeFiled = PerformerStatField.fields.FirstOrDefault(f => f.PropertyName == stat.Property(p => p.ServiceType));
            SelectedFunction.Filters.RemoveAll(f => f.a == serviceTypeFiled);
            if (signalsToolStripMenuItem.Checked || pammsToolStripMenuItem.Checked)
                SelectedFunction.Filters.Add(
                    new Cortege3<PerformerStatField, ExpressionOperator, double>(serviceTypeFiled, ExpressionOperator.Equal,
                                                                             ((signalsToolStripMenuItem.Checked ? 1 : 0) +
                                                                              (pammsToolStripMenuItem.Checked ? 2 : 0))));
            topFilterControl.SetExpression(SelectedFunction);
        }
    }
}

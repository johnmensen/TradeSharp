using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Client.Subscription.Control
{
    // отображение топов в TopPortfolioControl, выстроенных "плиткой" 2x2
    public partial class TopPortfolioListControl : UserControl
    {
        public bool AllPortfoliosLoaded;

        private int nextPortfolioIndexToLoad;

        private List<string> strategies;

        private string selectedStrategy;

        private List<TopPortfolio> portfolios;

        private int subscribedPortfolioId;

        private readonly List<TopPortfolio> portfoliosToLoad = new List<TopPortfolio>();

        private readonly List<TopPortfolioControl> controlsToLoadOpenedOrders = new List<TopPortfolioControl>();

        private readonly List<TopPortfolioControl> controlsToLoadClosedOrders = new List<TopPortfolioControl>();


        // SetBounds не желает устранавливать указанные в параметрах размеры
        // если он равен MinimumSize.? - устанавливает всегда больше на ~15%
        private const int MinControlWidth = 550;

        private const int MinControlHeight = 500;

        private const int Spacing = 10;

        public TopPortfolioListControl()
        {
            InitializeComponent();
        }

        // установить топы для отображения, начать загрузку данных для топов
        public void SetPortfolios(List<TopPortfolio> topPortfolios, int subscribedTopPortfolioId)
        {
            ResetControls();
            if (topPortfolios == null || topPortfolios.Count == 0)
                return;
            subscribedPortfolioId = subscribedTopPortfolioId;
            strategies = topPortfolios.Select(p => p.Name).Distinct().ToList();
            selectedStrategy = strategies[0];

            portfolios = topPortfolios;

            UpdatePortfoliosToLoad();
            LoadNextPortfolio();
        }

        // отобразить топы указанной стратегии (с указанным именем), начать загрузку данных для топов
        public void SetSelectedStrategy(string strategy)
        {
            ResetControls();
            selectedStrategy = strategy;
            UpdatePortfoliosToLoad();
            LoadNextPortfolio();
        }

        // прекратить загрузку всех контролов
        private void ResetControls()
        {
            foreach (var control in panel.Controls.OfType<TopPortfolioControl>())
            {
                control.PortfolioChanged = null;
                control.OpenedDealsLoaded = null;
                control.ClosedDealsLoaded = null;
                control.StrategyChanged = null;
            }
            panel.Controls.Clear();
            controlsToLoadOpenedOrders.Clear();
            controlsToLoadClosedOrders.Clear();
            portfoliosToLoad.Clear();
            AllPortfoliosLoaded = false;
            nextPortfolioIndexToLoad = 0;
        }

        private void UpdatePortfoliosToLoad()
        {
            var participantQuantities =
                portfolios.Where(p => p.Name == selectedStrategy).Select(p => p.ParticipantCount).Distinct().ToList();
            participantQuantities.Sort();
            foreach (var participantQuantity in participantQuantities)
            {
                var portfolio =
                    portfolios.FirstOrDefault(
                        p => p.Name == selectedStrategy && p.ParticipantCount == participantQuantity);
                if (portfolio == null)
                    continue;
                portfoliosToLoad.Add(portfolio);
            }
            AllPortfoliosLoaded = false;
        }

        private void LoadNextPortfolio()
        {
            if (nextPortfolioIndexToLoad < 0 || nextPortfolioIndexToLoad >= portfoliosToLoad.Count)
            {
                LoadNextOpenedDeals();
                return;
            }
            var portfolio = portfoliosToLoad[nextPortfolioIndexToLoad];
            nextPortfolioIndexToLoad++;

            var control = new TopPortfolioControl();
            control.SetStrategies(strategies);
            control.PortfolioChanged += OnPortfolioChanged;
            control.OpenedDealsLoaded += OnOpenedDealsLoaded;
            control.ClosedDealsLoaded += OnClosedDealsLoaded;
            control.StrategyChanged += (o , strategy) => SetSelectedStrategy(strategy);
            var controlWidth = Math.Max(MinControlWidth, (panel.Width - Spacing - 20) / 2);
            var x = ((panel.Controls.Count % 2 == 1) ? controlWidth + Spacing : 0) + panel.DisplayRectangle.X;
            var y = (MinControlHeight + Spacing) * (panel.Controls.Count / 2) + panel.DisplayRectangle.Y;
            panel.Controls.Add(control);
            control.SetBounds(x, y, controlWidth, MinControlHeight);
            //Console.WriteLine("LoadNextPortfolio {0}", portfolio.Id);
            control.Portfolio = portfolio;
            control.IsSubsribed = portfolio.Id == subscribedPortfolioId;
        }

        private void OnPortfolioChanged(object sender, EventArgs eventArgs)
        {
            controlsToLoadOpenedOrders.Add((TopPortfolioControl) sender);
            LoadNextPortfolio();
        }

        private void LoadNextOpenedDeals()
        {
            if (controlsToLoadOpenedOrders.Count == 0)
            {
                LoadNextClosedDeals();
                return;
            }
            var control = controlsToLoadOpenedOrders[0];
            controlsToLoadOpenedOrders.RemoveAt(0);
            //Console.WriteLine("LoadNextOpenedDeals {0}", control.Portfolio.Id);
            control.LoadDeals(true);
        }

        private void OnOpenedDealsLoaded(object sender, EventArgs eventArgs)
        {
            controlsToLoadClosedOrders.Add((TopPortfolioControl) sender);
            LoadNextOpenedDeals();
        }

        private void LoadNextClosedDeals()
        {
            if (controlsToLoadClosedOrders.Count == 0)
            {
                AllPortfoliosLoaded = true;
                return;
            }
            var control = controlsToLoadClosedOrders[0];
            controlsToLoadClosedOrders.RemoveAt(0);
            //Console.WriteLine("LoadNextClosedDeals {0}", control.Portfolio.Id);
            control.LoadDeals(false);
        }

        private void OnClosedDealsLoaded(object sender, EventArgs eventArgs)
        {
            LoadNextClosedDeals();
        }

        private void PanelResize(object sender, EventArgs e)
        {
            if (panel.Controls.Count == 0)
                return;
            for (var i = 0; i < panel.Controls.Count; i++)
            {
                System.Windows.Forms.Control control = panel.Controls[i];
                var controlWidth = Math.Max(MinControlWidth, (panel.Width - Spacing - 20) / 2);
                var x = ((i % 2 == 1) ? controlWidth  + Spacing : 0) + panel.DisplayRectangle.X;
                var y = (MinControlHeight + Spacing) * (i / 2) + panel.DisplayRectangle.Y;
                control.SetBounds(x, y, controlWidth, MinControlHeight);
            }
        }
    }
}

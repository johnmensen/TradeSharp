using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Candlechart;
using Candlechart.Chart;
using Candlechart.ChartMath;
using Candlechart.Core;
using Candlechart.Indicator;
using Entity;
using TradeSharp.Client.BL;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class ChartForm : Form
    {
        public long bookmarkId;
        
        public TabControl panesTabCtrl;

        private Point chartLocation;
        
        private Size chartSize;

        private FormWindowState chartState = FormWindowState.Normal;
        
        public string QuoteCachePath
        {
            get
            {
                return string.Format("{0}\\{1}.quote",
                                         ExecutablePath.ExecPath + TerminalEnvironment.QuoteCacheFolder,
                                         chart.chart.Symbol);
            }
        }

        private static readonly IQuoteStorage quoteStorage;
        private const int MinMinutesToFillGap = 2;
        public const int MinDaysToQuotesRequest = 25;
        public const int DaysToQuotesRequest = 90;
        public const int MaxDaysInQuotesRequest = 365;        
        private readonly  ManualResetEvent updateCompleted = new ManualResetEvent(true);
        private const int WaitFormUpdateInterval = 100;
        private const int UpdateQuoteTimeout = 300;
        private volatile bool startupFinished;
        /// <summary>
        /// если значение задано, будет осуществлена попытка подгрузить котировки за указанное количество дней
        /// </summary>
        public int? prefferedCountDaysInRequest;
        /// <summary>
        /// поддерево XML-документа, содержащее все объекты графика
        /// объекты загружаются после загрузки котировок и построения самого графика
        /// </summary>
        public XmlElement xmlRootObjects;

        /// <summary>
        /// масштаб графика - смещение начала и конца наблюдаемого интервала
        /// от первого и последнего индексов
        /// инициализируется при создании графика из сохраненных настроек
        /// </summary>
        private PointD? chartTimeExtends;

        private bool windowIsClosingByProgram;

        public string UniqueId
        {
            get { return chart.UniqueId; }
            set { chart.UniqueId = value; }
        }

        /// <summary>
        /// на перемещение формы - показать опции Drag & Drop
        /// </summary>
        public Action<Form> formMoved;

        /// <summary>
        /// перемещение формы завершено - показать варианты Drop-a
        /// </summary>
        public Action<Form> resizeEnded;

        static ChartForm()
        {
            try
            {
                quoteStorage = QuoteStorage.Instance.proxy;
            }
            catch (Exception)
            {
                Logger.Error("Связь с сервером (IQuoteStorageBinding) не установлена");
            }
        }

        public ChartForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
            CandleChartControl.ResetControlTags(this);
            chart.UniqueId = Guid.NewGuid().ToString();
            chart.updateTickersCacheForRobots = MainForm.Instance.UpdateTickersCacheForRobots;
            panesTabCtrl.Show();
        }

        public ChartForm(Form mdiParent, string symbol)
        {
            MdiParent = mdiParent;

            InitializeComponent();

            Localizer.LocalizeControl(this);
            CandleChartControl.ResetControlTags(this);
            Text = symbol;
            chart.Symbol = symbol;
            chart.updateTickersCacheForRobots = MainForm.Instance.UpdateTickersCacheForRobots;
            chart.UniqueId = string.Format("{0}_{1}", symbol, Guid.NewGuid().ToString().Substring(0, 10));
        }

        public ChartForm(Form mdiParent, ChartWindowSettings sets)
        {
            MdiParent = mdiParent;
            
            InitializeComponent();

            Localizer.LocalizeControl(this);
            CandleChartControl.ResetControlTags(this);
            Text = sets.Symbol;
            chart.updateTickersCacheForRobots = MainForm.Instance.UpdateTickersCacheForRobots;
            chart.Symbol = sets.Symbol;
            LoadChartSettings(sets);
        }

        private void SubscribeIndicatorsEvents()
        {
            // строим табы
            foreach (BaseChartIndicator indi in chart.indicators)
            {
                if (indi.CreateOwnPanel)
                {
                    // надо вкладку создать
                    var tp = new TabPage { Parent = panesTabCtrl, Text = indi.UniqueName, Tag = indi};
                    tp.Show();
                    if (!indi.IsPanelVisible)
                    {
                        foreach (Pane pane in chart.chart.Panes)
                        {
                            if (pane.Title != indi.ownPane.Title) continue;
                            chart.chart.Panes.Remove(pane);
                            break;
                        }
                    }
                }
            }
            if (panesTabCtrl.TabCount > 0)
                panesTabCtrl.Show();
            chart.IndiAddEvent += IndicatorAdded;
            chart.IndiRemoveEvent += IndicatorDeleted;
            chart.IndiEditEvent += IndicatorEdited;
        }

        private void MdiChildActivated(object sender, EventArgs e)
        {
            // !TAB
        } 

        /// <summary>
        /// асинхронно обновить график
        /// </summary>        
        public void UpdateQuoteAsynch(QuoteData[] quotes)
        {
            UpdateBaseNewsAsynch(null, quotes);
        }

        /// <summary>
        /// асинхронно обновить график
        /// </summary>        
        public void UpdateNewsAsynch(News[] news)
        {
            UpdateBaseNewsAsynch(news, null);
        }

        /// <summary>
        /// асинхронно обновить открытые позиции
        /// </summary>        
        public void UpdateOrdersListAsynch(MarketOrder[] posList)
        {
            if (!updateCompleted.WaitOne(WaitFormUpdateInterval)) return;

            var del = new UpdateOrdersListDel(UpdateOrdersList);
            try
            {
                Invoke(del, (object)posList);
            }
            catch (InvalidOperationException) { }
        }
        
        /// <summary>
        /// асинхронно обновить отложенные ордера
        /// </summary>        
        public void UpdatePendingOrdersListAsynch(PendingOrder[] ordList)
        {
            if (!updateCompleted.WaitOne(WaitFormUpdateInterval)) return;

            var del = new UpdatePendingOrdersListDel(UpdatePendingOrdersList);
            try
            {
                Invoke(del, (object)ordList);
            }
            catch (InvalidOperationException) { }
        }

        /// <summary>
        /// асинхронно обновить закрытые сделки
        /// </summary>        
        public void UpdateClosedOrdersListAsynch(List<MarketOrder> ordList)
        {
            if (!updateCompleted.WaitOne(WaitFormUpdateInterval)) return;

            var del = new UpdateClosedOrdersListDel(UpdateClosedOrdersList);
            try
            {
                Invoke(del, (object)ordList);
            }
            catch (InvalidOperationException) { }
        }

        /// <summary>
        /// асинхронно обновить график
        /// </summary>        
        public void UpdateBaseNewsAsynch(News[] news, QuoteData[] quotes)
        {
            if (!updateCompleted.WaitOne(WaitFormUpdateInterval)) return;
            
            var del = new UpdateQuoteDel(UpdateQuote);
            try
            {
                BeginInvoke(del, (object)news, (object)quotes);
            }
            catch (InvalidOperationException) { }
        }

        /// <summary>
        /// сохранить настройки, относящиеся к графику
        /// </summary>        
        public void InitChartSettings(ChartWindowSettings sets)
        {
            sets.Symbol = chart.Symbol;
            sets.UniqueId = chart.UniqueId;
            sets.Timeframe = chart.Timeframe.GetTag(string.Empty);
            sets.BarOffset = chart.chart.StockSeries.BarOffset;

            sets.Theme = chart.chart.visualSettings.Theme.ToString();
            sets.ColorShadowUp = chart.chart.visualSettings.StockSeriesUpLineColor;
            sets.ColorShadowDn = chart.chart.visualSettings.StockSeriesDownLineColor;
            sets.ColorBarUp = chart.chart.visualSettings.StockSeriesUpFillColor;
            sets.ColorBarDn = chart.chart.visualSettings.StockSeriesDownFillColor;
            sets.ColorBackground = chart.chart.visualSettings.PaneBackColor;
            sets.ShowLastQuote = chart.chart.StockSeries.ShowLastQuote;
            sets.AutoScroll = chart.chart.StockSeries.AutoScroll;
            sets.LastTemplateName = chart.CurrentTemplateName;
            sets.YAxisAlignment = chart.chart.YAxisAlignment;
            
            // сохранение временного масштаба            
            // смещение временного окна от начала и конца интервала
            int indexStart = 0, indexEnd = 0;
            double maxPos = 0, minPos = 0;
            chart.chart.GetScrollView(ref indexStart, ref indexEnd);
            chart.chart.StockSeries.GetXExtent(ref minPos, ref maxPos);
            sets.FirstCandleIndex = indexStart;// - minPos;
            sets.LastCandleIndex = indexEnd; // maxPos - indexEnd;
            sets.GraphMode = chart.chart.StockSeries.BarDrawMode;
            // позиции панелек
            foreach (Pane pane in chart.chart.Panes)
                sets.PaneLocations.Add(pane.PercentHeight);

            sets.SeriesSettings = chart.SaveSeriesSettings(sets.SeriesSettings);
        }

        /// <summary>
        /// загрузить настройки, относящиеся к графику
        /// </summary>        
        public void LoadChartSettings(ChartWindowSettings sets)
        {
            chart.Symbol = sets.Symbol;
            chart.Timeframe = new BarSettings(sets.Timeframe);
            chart.chart.StockSeries.BarDrawMode = sets.GraphMode;

            try
            {
                var theme = (ChartControl.Themes) Enum.Parse(typeof (ChartControl.Themes), sets.Theme);
                chart.chart.visualSettings.Theme = theme;
                chart.chart.visualSettings.ApplyTheme();
            }
            catch
            {                
            }
            chart.chart.visualSettings.StockSeriesUpLineColor = sets.ColorShadowUp;
            chart.chart.visualSettings.StockSeriesDownLineColor = sets.ColorShadowDn;
            chart.chart.visualSettings.StockSeriesUpFillColor = sets.ColorBarUp;
            chart.chart.visualSettings.StockSeriesDownFillColor = sets.ColorBarDn;
            chart.chart.visualSettings.PaneBackColor = sets.ColorBackground;
            chart.chart.StockSeries.ShowLastQuote = sets.ShowLastQuote;
            chart.chart.StockSeries.AutoScroll = sets.AutoScroll;
            chart.CurrentTemplateName = sets.LastTemplateName;
            chart.chart.YAxisAlignment = sets.YAxisAlignment;
            chart.chart.RightBars = sets.BarOffset;
            
            chartTimeExtends = new PointD(sets.FirstCandleIndex, sets.LastCandleIndex);
            chart.UniqueId = sets.UniqueId;
            chart.SetupSeries(sets.SeriesSettings);
        }

        public void LoadPaneLocations(ChartWindowSettings sets)
        {
            var index = 0;
            foreach (var location in sets.PaneLocations)
            {
                while (index < chart.chart.Panes.Count)
                {
                    var pane = chart.chart.Panes[index];
                    if (pane == chart.chart.StockPane) continue;
                    pane.PercentHeight = location;
                    index++;
                }
            }
        }

        /// <summary>
        /// создать в документе "objects.xml" поддерево для графика и заполнить его объектами
        /// </summary>
        public void SaveObjects(XmlElement xmlRoot)
        {
            // создать узел для окошка
            // ReSharper disable PossibleNullReferenceException
            var newNode = (XmlElement)xmlRoot.AppendChild(xmlRoot.OwnerDocument.CreateElement("chart"));
            var atrId = newNode.Attributes.Append(xmlRoot.OwnerDocument.CreateAttribute("Id"));
            // ReSharper restore PossibleNullReferenceException
            atrId.Value = chart.UniqueId;
            // сохранить сами объекты
            chart.SaveObjects(newNode);
        }

        public void LoadObjects()
        {
            // найти поддерево для чарта
            foreach (XmlElement node in xmlRootObjects.ChildNodes)
            {
                var atrId = node.Attributes["Id"];
                if (atrId != null)
                    if (atrId.Value == chart.UniqueId)
                        chart.LoadObjects(node);
            }
        }

        private delegate void UpdateQuoteDel(News[] news, QuoteData[] quotes);

        private void UpdateQuote(News[] news, QuoteData[] quotes)
        {
            if (!startupFinished) return;
            if (!updateCompleted.WaitOne(UpdateQuoteTimeout)) return;
            updateCompleted.Reset();
            try
            {
                // график обрабатывает новые котировки и перерисовывается
                var flagRedrawQuotes = false;
                var flagRedrawNews = false;
                if (quotes != null)
                    flagRedrawQuotes = chart.ProcessQuotes(quotes);
                if (news != null)
                    flagRedrawNews = chart.ProcessNews(news);
                if (flagRedrawNews || flagRedrawQuotes) // перерисовать
                {
                    chart.RedrawChartSafe();
                }
            }
            finally
            {
                updateCompleted.Set();
            }                        
        }

        private delegate void UpdateOrdersListDel(MarketOrder[] posList);
        private delegate void UpdateClosedOrdersListDel(List<MarketOrder> posList);
        private delegate void UpdatePendingOrdersListDel(PendingOrder[] ordList);
        
        private void UpdateOrdersList(MarketOrder[] posList)
        {
            if (!startupFinished) return;
            if (!updateCompleted.WaitOne()) return; //UpdateQuoteTimeout)) return;
            updateCompleted.Reset();
            try
            {
                // график обрабатывает список позиций и перерисовывается
                var flagRedrawOrders = false;
                if (posList != null)
                    flagRedrawOrders = chart.ProcessOrders(posList);

                if (flagRedrawOrders) // перерисовать
                    chart.RedrawChartSafe();
            }
            finally
            {
                updateCompleted.Set();
            }
        }

        private void UpdateClosedOrdersList(List<MarketOrder> posList)
        {
            if (!startupFinished) return;
            if (!updateCompleted.WaitOne()) return; //UpdateQuoteTimeout)) return;
            updateCompleted.Reset();
            try
            {
                // график обрабатывает список позиций и перерисовывается
                var flagRedrawOrders = false;
                if (posList != null)
                    flagRedrawOrders = chart.ProcessClosedOrders(posList);

                if (flagRedrawOrders) // перерисовать
                    chart.RedrawChartSafe();
            }
            finally
            {
                updateCompleted.Set();
            }
        }

        private void UpdatePendingOrdersList(PendingOrder[] ordList)
        {
            if (!startupFinished) return;
            if (!updateCompleted.WaitOne()) return; //UpdateQuoteTimeout)) return;
            updateCompleted.Reset();
            try
            {
                var flagRedrawOrders = false;
                if (ordList != null)
                    flagRedrawOrders = chart.ProcessPendingOrders(ordList);
                if (flagRedrawOrders) // перерисовать
                    chart.RedrawChartSafe();
                
            }
            finally
            {
                updateCompleted.Set();
            }
        }
        
        public void LoadQuotesAndBuildChart(bool loadQuotesFromServer)
        {
            LoadQuotesAndFillGap(loadQuotesFromServer);
            chart.BuildIndicators(loadQuotesFromServer);
            
            chart.chart.FitChart();
            if (chartTimeExtends.HasValue)
            {
                var leftBound = (int) (chartTimeExtends.Value.X);
                if (leftBound < 0) leftBound = 0;
                var rightBound = (int) (chartTimeExtends.Value.Y + 0.5);
                if (rightBound < 0)
                    rightBound = chart.chart.StockSeries.DataCount;
                if (rightBound > chart.chart.StockSeries.DataCount)
                    rightBound = chart.chart.StockSeries.DataCount;

                var delta = rightBound - leftBound;
                if (delta < 2)
                {
                    leftBound = 0;
                    rightBound = chart.chart.StockSeries.DataCount;
                }

                chart.chart.SetScrollView(leftBound, rightBound);
            }
            else
                chart.ShrinkDisplayedIntervalAuto();
            // загрузить объекты графика
            if (xmlRootObjects != null)
            {
                chart.LoadObjects(xmlRootObjects);
            }

            SubscribeIndicatorsEvents();
            chart.RedrawChartSafe();
            startupFinished = true;
            chart.SynchronizationEnabled = true;
            chart.UpdateWindowTitle();
        }

        private void LoadQuotesAndFillGap(bool actualizeQuotes = false)
        {
            // получить минутные котировки из хранилища
            var candles = AtomCandleStorage.Instance.GetAllMinuteCandles(chart.Symbol);

            // попытаться заглянуть в файл
            if (candles == null)
            {
                AtomCandleStorage.Instance.LoadFromFile(ExecutablePath.ExecPath + TerminalEnvironment.QuoteCacheFolder +
                                                        "\\" + chart.Symbol + ".quote", chart.Symbol);
                candles = AtomCandleStorage.Instance.GetAllMinuteCandles(chart.Symbol) ?? new List<CandleData>();
            }

            // актуализировать котировки
            if (actualizeQuotes)
            {
                var dateStart = prefferedCountDaysInRequest.HasValue
                                    ? DateTime.Now.Date.AddDays(-prefferedCountDaysInRequest.Value)
                                    : DateTime.Now.Date.AddDays(-DaysToQuotesRequest);
                MainForm.Instance.UpdateTickersCacheForRobots(new Dictionary<string, DateTime>
                                                                  {
                                                                      {chart.Symbol, dateStart}
                                                                  },  10);
                candles = AtomCandleStorage.Instance.GetAllMinuteCandles(chart.Symbol) ?? new List<CandleData>();
            }

            //// актуализировать котировки (прочитать с сервера)
            //var lastQuoteTime = candles.Count == 0
            //                        ? new DateTime()
            //                        : candles[candles.Count - 1].timeOpen;
            //var totalDays = candles.Count == 0
            //                    ? 0
            //                    : (DateTime.Now - candles[0].timeOpen).TotalDays;
            //var deltaMinutes = (DateTime.Now - lastQuoteTime).TotalMinutes;
            
            //// точно надо прочитать
            //if (deltaMinutes > MinMinutesToFillGap || 
            //    (prefferedCountDaysInRequest.HasValue && prefferedCountDaysInRequest.Value > totalDays))
            //{
            //    // если в истории слишком мало котировок - запросить за больший период
            //    var forgetLocalStoredQuotes = false;
                
            //    if (totalDays < MinDaysToQuotesRequest)
            //    {
            //        lastQuoteTime = DateTime.Now.Date.AddDays(-DaysToQuotesRequest);
            //        forgetLocalStoredQuotes = true;
            //    }

            //    // если пользователь хочет кусок истории потолще
            //    if (prefferedCountDaysInRequest.HasValue)
            //    {
            //        var daysInRequest = (DateTime.Now - lastQuoteTime).TotalDays;
            //        if (daysInRequest < prefferedCountDaysInRequest.Value)
            //        {
            //            lastQuoteTime = DateTime.Now.Date.AddDays(-prefferedCountDaysInRequest.Value);
            //            forgetLocalStoredQuotes = true;
            //        }
            //    }

            //    // таки запросить из истории БД
            //    PackedCandleStream candlesFromDbStream;
            //    try
            //    {
            //        candlesFromDbStream = quoteStorage.GetMinuteCandlesPackedFast(chart.Symbol, lastQuoteTime, DateTime.Now);                        
            //    }
            //    catch (Exception ex)
            //    {
            //        Logger.Error("QuoteStorageProxy:GetMinuteCandlesPackedFast - ошибка", ex);
            //        return;
            //    }
            //    if (candlesFromDbStream == null)
            //    {
            //        Logger.InfoFormat("QuoteStorageProxy:GetMinuteCandlesPackedFast({0} с {1}) - нет котировок (nil)",
            //            chart.Symbol, lastQuoteTime);
            //        return;
            //    }
            //    var candlesDenseFromDb = candlesFromDbStream.GetCandles();
            //    if (candlesDenseFromDb == null || candlesDenseFromDb.Count == 0)
            //    {
            //        Logger.InfoFormat("QuoteStorageProxy:GetMinuteCandlesPackedFast({0} с {1}) - нет котировок",
            //            chart.Symbol, lastQuoteTime);
            //        return;
            //    }
                
            //    // склеить котировки
            //    var pointCost = DalSpot.Instance.GetPrecision10(chart.Symbol);
            //    var candlesFromDb = candlesDenseFromDb.Select(c => new CandleData(c, pointCost)).ToList();

            //    var oldCandlesCount = candles.Count;
            //    if (forgetLocalStoredQuotes)
            //        candles = candlesFromDb;
            //    else
            //        CandleData.MergeCandles(ref candles, candlesFromDb, false);

            //    // обновить локальное хранилище
            //    if (candles.Count > oldCandlesCount)
            //    {
            //        AtomCandleStorage.Instance.RewriteCandles(chart.Symbol, candles);
            //        AtomCandleStorage.Instance.FlushInFile(ExecutablePath.ExecPath + TerminalEnvironment.QuoteCacheFolder,
            //            chart.Symbol);
            //    }
            //}

            //// обновить родной ТФ чарта
            //if (prefferedCountDaysInRequest.HasValue)
            //{
            //    // если запрошенный кусок данных короче того, что был закачан -
            //    // - отрезать котировки от начала списка
            //    var daysLoaded = candles.Count == 0 ? 0 : (DateTime.Now.Date - candles[0].timeOpen.Date).TotalDays;
            //    if (daysLoaded - prefferedCountDaysInRequest.Value > 1)
            //    {
            //        var startDate = DateTime.Now.Date.AddDays(-prefferedCountDaysInRequest.Value);
            //        var startIndex = candles.FindIndex(c => c.timeOpen >= startDate);
            //        if (startIndex > 0)
            //            candles = candles.GetRange(startIndex, candles.Count - startIndex);
            //    }
            //}

            chart.UpdateCandles(candles);
        }

        public static DateTime? GetQuoteRefreshingTimeBounds(
            DateTime? fileStart, DateTime? fileEnd, DateTime nowTime, string ticker)
        {
            // если файл пуст в принципе
            if (!fileStart.HasValue || !fileEnd.HasValue)
                return GetDefaultQuoteRefreshStartDate(nowTime);
            var lastQuoteTime = fileEnd.Value;

            // если в котировках есть дыра - корректировать lastQuoteTime
            /*var candles = AtomCandleStorage.Instance.GetAllMinuteCandles(ticker);
            var startCandleIndex = candles.Count - CandlesToFindGapBeforeLoading;
            if (startCandleIndex < 0) startCandleIndex = 0;
            var gaps = QuoteCacheManager.GetGaps(candles.GetRange(startCandleIndex, candles.Count - startCandleIndex));
            if (gaps.Count > 0)
                lastQuoteTime = gaps[0].a;*/
            
            var deltaMinutes = (nowTime - lastQuoteTime).TotalMinutes;
            var totalDays = (nowTime - fileStart.Value).TotalDays;
            
            // данные актуальные и полные
            if (deltaMinutes <= MinMinutesToFillGap
                && totalDays > MinDaysToQuotesRequest) return null;
            // скорректировать запрос - возможно, приходится на выходные
            var requestStartTime = GetDefaultQuoteRefreshStartDate(nowTime);
            if (totalDays < MinDaysToQuotesRequest && requestStartTime < fileStart.Value)
                return GetDefaultQuoteRefreshStartDate(nowTime);
            if (deltaMinutes > MinMinutesToFillGap) return lastQuoteTime;
            // начало запроса приходится на выходные - в остальном, недостающие данные уже подкачаны
            return null;
        }

        /// <summary>
        /// вернуть значение, отстоящее от указанного момента (в большинстве случаев - DateTime.Now)
        /// на DaysToQuotesRequest с поправкой на выходной день
        /// </summary>
        public static DateTime GetDefaultQuoteRefreshStartDate(DateTime nowTime)
        {
            var date = nowTime.AddDays(-DaysToQuotesRequest);
            if (WorkingDay.Instance.IsWorkingDay(date)) return date;
            // подвинуть дату на начало рабочей недели
            return WorkingDay.Instance.GetWeekStart(date);
        }

        #region работа с панелями
        private void TabCtrlMouseDoubleClick(object sender, EventArgs e)
        {
            //SelectChildrenForTab(tabCtrl.SelectedTab);
            var indi = (BaseChartIndicator)panesTabCtrl.SelectedTab.Tag;
            foreach (Pane pane in chart.chart.Panes)
            {
                if (pane.Title != indi.ownPane.Title) continue;
                chart.chart.Panes.Remove(pane);
                indi.IsPanelVisible = false;
                return;
            }
            //var indi = (IChartIndicator) panesTabCtrl.SelectedTab.Tag;
            chart.chart.Panes.Add(indi.ownPane, indi.ownPane.PercentHeight);
            indi.IsPanelVisible = true;
        }
        
        private void IndicatorAdded(object sender, CandleChartControl.IndiEventArgs ie)
        {

            if (ie.indi.CreateOwnPanel == false || ie.indi.ownPane == null) return;
            // надо вкладку создать
            var tp = new TabPage { Parent = panesTabCtrl, Text = ie.indi.UniqueName, Tag = ie.indi };
            tp.Show();
            panesTabCtrl.SelectedTab = tp;                       
        }

        void IndicatorDeleted(object sender, CandleChartControl.IndiEventArgs ie)
        {
            // надо вкладку удалить
            if (ie.indi.ownPane == null) return;

            foreach (TabPage page in panesTabCtrl.TabPages)
            {
                if (page.Text != ie.indi.UniqueName) continue;
                panesTabCtrl.TabPages.Remove(page);
                ie.indi.IsPanelVisible = false;
                break;
            }
        }

        void IndicatorEdited(object sender, CandleChartControl.IndiEventArgs ie)
        {
            // проверяем надо ли название панели поменять если изменили название индикатора
            var oldName = (string) sender;
            // имя индикатора не поменялось, проверяем надо ли удалять панель или добавить
            if (oldName != ie.indi.UniqueName)
            {
                foreach (TabPage page in panesTabCtrl.TabPages)
                {
                    if (page.Text != oldName) continue;
                    page.Text = ie.indi.UniqueName;
                    page.Tag = ie.indi;
                    break;
                }
            }
            
            // проверяем надо ли вкладку изменить
            if (!ie.indi.CreateOwnPanel && ie.indi.ownPane != null)
            {
                // возможно надо табу удалить, проверяем есть ли вообще она
                foreach (TabPage page in panesTabCtrl.TabPages)
                {
                    if (page.Text != ie.indi.UniqueName) continue;
                    panesTabCtrl.TabPages.Remove(page);
                    ie.indi.IsPanelVisible = false;
                    return;
                }
            }

            // проверяем случай когда включили свою панель на индикаторе
            if (ie.indi.CreateOwnPanel && ie.indi.ownPane != null)
            {
                // проверяем может такая таба уже открыта
                foreach (TabPage page in panesTabCtrl.TabPages)
                {
                    if (page.Text != ie.indi.UniqueName) continue;
                    // нашли табу - ничего не делаем
                    page.Tag = ie.indi;
                    return;
                }
                // табы нет, создаем ее
                chart.chart.Panes.Add(ie.indi.ownPane, ie.indi.ownPane.PercentHeight);
                var tp = new TabPage { Parent = panesTabCtrl, Text = ie.indi.UniqueName, Tag = ie.indi };
                tp.Show();
                panesTabCtrl.SelectedTab = tp;
                ie.indi.IsPanelVisible = true;
                foreach (var series in ie.indi.SeriesResult)
                {
                    if(!ie.indi.ownPane.Series.ContainsSeries(series))
                        ie.indi.ownPane.Series.Add(series);
                }
            }
        }
        #endregion

        /// <summary>
        /// в отличии от простого Close(), не приводит к появлению диалога - предложения
        /// таки не закрывать окно
        /// </summary>
        public void CloseByProgram()
        {
            windowIsClosingByProgram = true;
            Close();
        }

        public void SavePlacement()
        {
            if (WindowState == FormWindowState.Normal)
            {
                chartLocation = Location;
                chartSize = Size;
            }
            else
            {
                chartLocation = RestoreBounds.Location;
                chartSize = RestoreBounds.Size;
            }
            chartState = WindowState;
        }

        public void RestorePlacement()
        {
            Location = chartLocation;
            Size = chartSize;
            WindowState = chartState;
        }
        private void ChartFormResize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                return;
        }

        private void ChartFormMove(object sender, EventArgs e)
        {
            if (startupFinished && formMoved != null)
                formMoved(this);

            if (WindowState == FormWindowState.Minimized)
                return;
        }

        private void ChartFormResizeEnd(object sender, EventArgs e)
        {
            if (startupFinished && resizeEnded != null)
                resizeEnded(this);
        }

        private void ChartFormLoad(object sender, EventArgs e)
        {
            // подписаться на события графика
            chart.OnWindowTitleUpdated += (t => BeginInvoke(new Action<string>(text => Text = text), t));
        }

        // ReSharper disable MemberCanBeMadeStatic.Local
        private void ChartFormFormClosing(object sender, FormClosingEventArgs e)
        {
            // отменить закрытие?
            if (UserSettings.Instance.CloseChartPrompt)
                if (e.CloseReason == CloseReason.UserClosing && !windowIsClosingByProgram)
                {// если нажали на кнопку "Закрыть"
                    if (MessageBox.Show(Localizer.GetString("TitleCloseWindow") + "?", 
                        Localizer.GetString("TitleConfirmation"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
                    {
                        e.Cancel = true;
                        return;
                    }
                }

            // отписаться от событий графика
            chart.OnWindowTitleUpdated -= (t => BeginInvoke(new Action<string>(text => Text = text), t));

            // закрыть опустевшую закладку
            if (e.CloseReason == CloseReason.UserClosing && !windowIsClosingByProgram)
                MainForm.Instance.CheckEmptyTabOnChartClosing(bookmarkId, true);

            // отписаться от событий
            HistoryOrderStorage.Instance.OrdersUpdated -= UpdateClosedOrdersListAsynch;
        }

        // ReSharper restore MemberCanBeMadeStatic.Local
    }
}
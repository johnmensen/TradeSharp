using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using Candlechart.Chart;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Contract.Entity;
using System.Linq;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    [LocalizedDisplayName("TitleTradeOperations")]
    [LocalizedCategory("TitleTradingIndicatorsShort")]
    [TypeConverter(typeof(PropertySorter))]
    public partial class IndicatorOrders : BaseChartIndicator, IChartIndicator
    {
        public enum OrderType { Рыночный = 0, Отложенный = 1, ЗакрытыеСделки = 2}
        public enum OpenPositionType { Совокупная = 0, Все }
        public enum StartPoint { ОтВхода = 0, ВесьЭкран }

        [Browsable(false)]
        public override string Name { get { return Localizer.GetString("TitleTradeOperations"); } }

        [LocalizedDisplayName("TitleCreateDrawingPanel")]
        [LocalizedCategory("TitleMain")]
        [Description("Создавать свою панель отрисовки")]
        [Browsable(false)]
        public override bool CreateOwnPanel { get; set; }

        private OpenPositionType showType = OpenPositionType.Все;
        [LocalizedDisplayName("TitleShowType")]
        [Description("Режим отображения позиций (совокупная, отдельные)")]
        [LocalizedCategory("TitleMain")]
        public OpenPositionType ShowType
        {
            get { return showType; }
            set { showType = value; }
        }

        private bool showComments = true;
        [LocalizedDisplayName("TitleShowComments")]
        [Description("Показывать комментарии позиций на графике")]
        [LocalizedCategory("TitleMain")]
        public bool ShowComments
        {
            get { return showComments; }
            set { showComments = value; }
        }

        private bool showClosed = true;
        [LocalizedDisplayName("TitleShowDealsClosingShort")]
        [Description("Показывать закрытые сделки на графике")]
        [LocalizedCategory("TitleMain")]
        public bool ShowClosed
        {
            get { return showClosed; }
            set { showClosed = value; }
        }
        
        private bool showHistoryComments = true;
        [LocalizedDisplayName("TitleShowClosedDealsComments")]
        [Description("Показывать комментарии закрытых позиций на графике")]
        [LocalizedCategory("TitleMain")]
        public bool ShowHistoryComments
        {
            get { return showHistoryComments; }
            set { showHistoryComments = value; }
        }

        private bool showCurrent = true;
        [LocalizedDisplayName("TitleShowOpenedOrdersStatisticsOnCurrentBarShort")]
        [Description("Показывать на текущем баре статистику по открытым ордерам")]
        [LocalizedCategory("TitleMain")]
        public bool ShowCurrent
        {
            get { return showCurrent; }
            set { showCurrent = value; }
        }

        private Color colorBuy = Color.Blue;
        [LocalizedDisplayName("TitlePurchaseColor")]
        [Description("Цвет покупок")]
        [LocalizedCategory("TitleVisuals")]
        public Color ColorBuy
        {
            get { return colorBuy; }
            set { colorBuy = value; }
        }

        private Color colorSell = Color.Red;
        [LocalizedDisplayName("TitleSellingColor")]
        [Description("Цвет продаж")]
        [LocalizedCategory("TitleVisuals")]
        public Color ColorSell
        {
            get { return colorSell; }
            set { colorSell = value; }
        }

        private Color colorBuyClosed = Color.Navy;
        [LocalizedDisplayName("TitlePurchaseColorForClosedDealsShort")]
        [Description("Цвет покупок (закрытые сделки)")]
        [LocalizedCategory("TitleVisuals")]
        public Color ColorBuyClosed
        {
            get { return colorBuyClosed; }
            set { colorBuyClosed = value; }
        }

        private Color colorSellClosed = Color.DarkSalmon;
        [LocalizedDisplayName("TitleSellingColorForClosedDealsShort")]
        [Description("Цвет продаж (закрытые сделки)")]
        [LocalizedCategory("TitleVisuals")]
        public Color ColorSellClosed
        {
            get { return colorSellClosed; }
            set { colorSellClosed = value; }
        }

        private Color colorBuyQuit = Color.FromArgb(60, 60, 140);
        [LocalizedDisplayName("TitleExitColorForPurchasesShort")]
        [Description("Цвет выхода из сделки (покупки)")]
        [LocalizedCategory("TitleVisuals")]
        public Color ColorBuyQuit
        {
            get { return colorBuyQuit; }
            set { colorBuyQuit = value; }
        }

        private Color colorSellQuit = Color.FromArgb(140, 60, 60);
        [LocalizedDisplayName("TitleExitColorForSellingsShort")]
        [Description("Цвет выхода из сделки (продажи)")]
        [LocalizedCategory("TitleVisuals")]
        public Color ColorSellQuit
        {
            get { return colorSellQuit; }
            set { colorSellQuit = value; }
        }

        private int arrowSizeClosed = 6;
        [LocalizedDisplayName("TitleArrowSizesForClosedDealsShort")]
        [Description("Размер стрелок закрытых позиций")]
        [LocalizedCategory("TitleVisuals")]
        public int ArrowSizeClosed
        {
            get { return arrowSizeClosed; }
            set { arrowSizeClosed = value; }
        }

        private int arrowSizeOpened = 7;
        [LocalizedDisplayName("TitleArrowSizesForOpenedDealsShort")]
        [Description("Размер стрелок открытых позиций")]
        [LocalizedCategory("TitleVisuals")]
        public int ArrowSizeOpened
        {
            get { return arrowSizeOpened; }
            set { arrowSizeOpened = value; }
        }

        [LocalizedDisplayName("TitleStartPoint")]
        [Description("Рисовать с момента входа или на весь экран")]
        [LocalizedCategory("TitleVisuals")]
        public StartPoint StartPointType { get; set; }

        [LocalizedDisplayName("TitleColorizeChart")]
        [Description("Раскрасить свечи / бары в зависимости от направления входа")]
        [LocalizedCategory("TitleVisuals")]
        public bool PaintBars { get; set; }

        /// <summary>
        /// BUY, SELL, C, 98.51 и т.д.
        /// </summary>
        private readonly SeriesComment seriesComment = new SeriesComment("TradeOperations"); // { /* hideSpannedComments = true*/ };
        /// <summary>
        /// текущие совокупные показатели доходности на последнем баре
        /// </summary>
        private readonly SeriesComment seriesCommentLastBar = new SeriesComment("LastBarComments");
        /// <summary>
        /// стрелочки, показывают вход / выход
        /// </summary>
        private readonly SeriesAsteriks seriesAsteriks = new SeriesAsteriks("TradeMarkers");
        /// <summary>
        /// линии SL-TP, вход -> выход для выбранного ордера
        /// </summary>
        private readonly TrendLineSeries seriesSelectedLines = new TrendLineSeries("SelectedLines");
        /// <summary>
        /// комментарии по выбранному ордеру
        /// </summary>
        private readonly SeriesComment seriesCommentSelected = new SeriesComment("SelectedComments");
        /// <summary>
        /// открытые позы (рыночные ордера)
        /// </summary>
        private readonly List<MarketOrder> openPositions = new List<MarketOrder>();
        /// <summary>
        /// закрытые рыночные ордера
        /// </summary>
        private readonly List<MarketOrder> closedPositions = new List<MarketOrder>();
        /// <summary>
        /// отложенные ордера
        /// </summary>
        private readonly List<PendingOrder> pendingOrders = new List<PendingOrder>();

        private int currentHintIndex;
        /// <summary>
        /// в подсказке показываются не более N ордеров за раз (см currentHintIndex)
        /// </summary>
        private const int OrdersInHintMax = 2;

        public override BaseChartIndicator Copy()
        {
            var op = new IndicatorOrders();
            Copy(op);
            CopyParameters(op);
            return op;
        }

        private void CopyParameters(IndicatorOrders op)
        {
            op.ShowComments = ShowComments;
            op.ShowType = ShowType;
            op.StartPointType = StartPointType;
            op.ColorBuy = ColorBuy;
            op.ColorSell = ColorSell;
            op.ColorBuyClosed = ColorBuyClosed;
            op.ColorSellClosed = ColorSellClosed;
            op.ColorBuyQuit = ColorBuyQuit;
            op.ColorSellQuit = ColorSellQuit;
            op.StartPointType = StartPointType;
            op.ArrowSizeClosed = ArrowSizeClosed;
            op.ArrowSizeOpened = ArrowSizeOpened;
            op.ShowClosed = ShowClosed;
            op.ShowHistoryComments = ShowHistoryComments;
            op.showCurrent = ShowCurrent;
            op.PaintBars = PaintBars;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var op = (IndicatorOrders)indi;
            op.openPositions.Clear();
            op.openPositions.AddRange(openPositions);
            CopyParameters(op);
            CopyBaseSettings(op);
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            SeriesResult = new List<Series.Series> 
            { 
                seriesComment, seriesAsteriks, 
                seriesCommentLastBar, seriesSelectedLines, seriesCommentSelected
            };
            EntitleIndicator();
            owner.Owner.OnPositionsReceived += ProcessMarketOrders;
            owner.Owner.OnPendingOrdersReceived += ProcessPendingOrders;
            owner.Owner.OnClosedOrdersReceivedDel += ProcessClosedOrders;
        }

        public void Remove()
        {
            seriesComment.data.Clear();
            seriesAsteriks.data.Clear();
            seriesCommentLastBar.data.Clear();
            seriesSelectedLines.data.Clear();

            owner.Owner.OnPositionsReceived -= ProcessMarketOrders;
            owner.Owner.OnPendingOrdersReceived -= ProcessPendingOrders;
            owner.Owner.OnClosedOrdersReceivedDel -= ProcessClosedOrders;

            if (PaintBars)
            {
                var candles = owner.StockSeries.Data.Candles;
                if (candles.Count == 0) return;
                foreach (var candle in candles)
                    candle.customColor = null;
            }
        }

        public void AcceptSettings()
        {
            EntitleIndicator();
            GetChartOrders();
            
            if (!PaintBars)
            {
                var candles = owner.StockSeries.Data.Candles;
                if (candles.Count == 0) return;
                foreach (var candle in candles)
                    candle.customColor = null;
            }
        }

        public List<Cortege3<int, int, float>> GetEntryPoints()
        {
            // m => new Cortege3<int, int, float>((int)m.candleIndex, (int)m.Side, (float)m.Price)).ToList();
            var points = new List<Cortege3<int, int, float>>();

            foreach (var marker in seriesAsteriks.data)
            {
                var sign = 0;
                if (marker.Name == "c" || marker.Name == "o")
                    sign = marker.Shape == AsteriskTooltip.ShapeType.СтрелкаВверх ? 1 : -1;
                if (sign == 0) continue;

                points.Add(new Cortege3<int, int, float>(marker.CandleIndex, sign, marker.Price));
            }

            return points;
        }

        private void ProcessMarketOrders(MarketOrder[] marketOrders)
        {
            ProcessOrders(marketOrders.Select(o => o.MakeCopy()).ToList(), 
                OrderType.Рыночный);
        }

        private void ProcessPendingOrders(PendingOrder[] pndOrders)
        {
            //ProcessOrders(pndOrders.Select(o => new DisplayedOrder(o)).ToList(), 
            //    OrderType.Отложенный);
        }

        private void ProcessClosedOrders(List<MarketOrder> closedOrders)
        {
            ProcessOrders(closedOrders.Where(o => o.Symbol == owner.Symbol).Select(o => o.MakeCopy()).ToList(), 
                OrderType.ЗакрытыеСделки);
        }

        private void ProcessOrders(List<MarketOrder> newOrders, OrderType typeOrders)
        {
            if (typeOrders == OrderType.Отложенный) return;
            var targetList = typeOrders == OrderType.Рыночный ? openPositions : closedPositions;
            UpdateDisplayedOrders(newOrders, targetList);
            BuildSeries(owner);
            if (PaintBars)
                DoPaintBars();
        }

        private void UpdateDisplayedOrders(List<MarketOrder> newOrders, List<MarketOrder> targetList)
        {
            if (newOrders.Count == 0)
            {
                targetList.Clear();
                return;
            }
            if (newOrders.Count != targetList.Count)
            {
                targetList.Clear();
                targetList.AddRange(newOrders);
                return;
            }

            // сравнить новые ордера с обработанными
            newOrders = newOrders.OrderBy(o => o.ID).ToList();
            var same = true;
            for (var i = 0; i < newOrders.Count; i++)
            {
                if (DisplayedOrder.AreSame(newOrders[i], targetList[i])) continue;
                same = false;
                break;
            }
            if (same) return;

            // таки обновить
            targetList.Clear();
            targetList.AddRange(newOrders);
            BuildSeries(owner);
        }

        private void GetChartOrders()
        {
            var posList = owner.Owner.receiveMarketOrders();
            if (posList != null && posList.Count > 0)
            {
                var list = posList.Where(o => o.Symbol == owner.Symbol).Select(o =>
                    o.MakeCopy()).ToList();
                ProcessOrders(list, OrderType.Рыночный);
            }
            if (!ShowClosed) return;
            owner.Owner.enforceClosedOrdersUpdate();
        }

        public void BuildSeries(ChartControl chart)
        {
            seriesComment.data.Clear();
            seriesCommentLastBar.data.Clear();
            seriesAsteriks.data.Clear();
            var candles = chart.StockSeries.Data.Candles;
            if (candles.Count == 0) return;

            // открытые позиции и отложенные ордера
            var posList = openPositions;
            
            // посчитать совокупную позу
            if (ShowType == OpenPositionType.Совокупная)
            {
                var posSum = CalculateSummaryPosition(posList);
                posList = posSum.Volume == 0 ? new List<MarketOrder>() : new List<MarketOrder> { posSum };
            }

            foreach (var pos in posList)
            {
                var x = chart.StockSeries.GetDoubleIndexByTime(pos.TimeEnter, true);
                if (x < 0) continue;
                AddOpenPositionMarks(chart, pos, x);
            }

            // показывать статистику на текущем баре
            if (ShowCurrent && posList.Count > 0)
                AddLastBarMarks();
            
            UpdateProfit();
            
            // закрытые сделки
            if (!ShowClosed) return;
            foreach (var pos in closedPositions)            
                AddClosedOrderMarks(chart, pos);

            // опционально - раскрасить свечки
            if (PaintBars) DoPaintBars();
        }

        private void DoPaintBars()
        {
            var candles = owner.StockSeries.Data.Candles;
            if (candles.Count == 0) return;
            foreach (var candle in candles)
                candle.customColor = null;
            
            // полный список сделок (открытые и закрытые)
            var openedDeals = new List<MarketOrder>();
            var dueDeals = (openPositions ?? new List<MarketOrder>());
            dueDeals.AddRange(closedPositions ?? new List<MarketOrder>());
            
            foreach (var candle in candles)
            {
                var timeOpen = candle.timeOpen;
                var timeClose = candle.timeClose;

                for (var i = 0; i < dueDeals.Count; i++)
                {
                    if (dueDeals[i].TimeEnter <= timeOpen && (!dueDeals[i].TimeExit.HasValue ||
                        dueDeals[i].TimeExit.Value >= timeClose))
                    {
                        openedDeals.Add(dueDeals[i]);
                        dueDeals.RemoveAt(i);
                        i--;
                    }
                }

                var newSign = 0;
                for (var i = 0; i < openedDeals.Count; i++)
                {
                    if (openedDeals[i].TimeExit.HasValue && openedDeals[i].TimeExit.Value <= timeClose)
                    {
                        openedDeals.RemoveAt(i);
                        i--;
                    }
                    else
                        newSign += openedDeals[i].Side;
                }

                candle.customColor = newSign > 0 ? colorBuy : newSign < 0 ? colorSell : (Color?)null;
            }
        }

        private MarketOrder CalculateSummaryPosition(IEnumerable<MarketOrder> posList)
        {
            var posSum = new MarketOrder { Symbol = owner.Symbol };
            float sumBuys = 0, sumSell = 0;
            var exposition = 0;
            foreach (var pos in posList)
            {
                if (posSum.TimeEnter == default(DateTime) ||
                    posSum.TimeEnter > pos.TimeEnter)
                    posSum.TimeEnter = pos.TimeEnter;
                exposition += pos.Side * pos.Volume;
                if (pos.Side > 0)
                    sumBuys += pos.Volume * pos.PriceEnter;
                else
                    sumSell += pos.Volume * pos.PriceEnter;
            }

            posSum.Volume = Math.Abs(exposition);
            posSum.Side = Math.Sign(exposition);
            posSum.PriceEnter = (float)(exposition == 0 ? 0 : Math.Round((sumBuys - sumSell) / exposition, 
                                                                        DalSpot.Instance.GetPrecision(posSum.Symbol)));
            return posSum;
        }

        /// <summary>
        /// добавить метки справа от крайнего бара: SL, TP, профит и прочая хрень
        /// </summary>
        private void AddLastBarMarks()
        {
            var pivotIndex = owner.StockPane.StockSeries.Data.Count - 1;
            
            // прибыль/убыток по сделкам
            var comm = new ChartComment
                           {
                               PivotIndex = pivotIndex,
                               PivotPrice = owner.StockPane.StockSeries.Data[owner.StockPane.StockSeries.Data.Count - 1].close,
                               ArrowAngle = 0,
                               ArrowLength = 20,
                               Text = "",
                               Color = owner.StockSeries.BarNeutralColor,
                               ColorText = owner.StockSeries.BarNeutralColor,
                               HideArrow = true,
                               HideBox = true,
                               Name = "CurrProfit"
                           };
            seriesCommentLastBar.data.Add(comm);

            // стопы-тейки и прочая ерунда
            // раздельно по BUY и SELL
            float? buyStop = null, buyTake = null;//, buyTrail = null;
            float? sellStop = null, sellTake = null;//, sellTrail = null;
            int buyVolumeSl = 0, buyVolumeTake = 0;//, buyVolumeTrail = 0;
            int sellVolumeSl = 0, sellVolumeTake = 0;//, sellVolumeTrail = 0;

            foreach (var pos in openPositions)
            {
                if (pos.Side > 0)
                {
                    if (pos.StopLoss.HasValue)
                    {
                        buyStop = (buyStop ?? 0) + pos.StopLoss * pos.Volume;
                        buyVolumeSl += pos.Volume;
                    }
                    if (pos.TakeProfit.HasValue)
                    {
                        buyTake = (buyTake ?? 0) + pos.TakeProfit * pos.Volume;
                        buyVolumeTake += pos.Volume;
                    }
                    continue;
                }
                if (pos.StopLoss.HasValue)
                {
                    sellStop = (sellStop ?? 0) + pos.StopLoss * pos.Volume;
                    sellVolumeSl += pos.Volume;
                }
                if (pos.TakeProfit.HasValue)
                {
                    sellTake = (sellTake ?? 0) + pos.TakeProfit * pos.Volume;
                    sellVolumeTake += pos.Volume;
                }
            }

            var hasMarksBuy = buyStop.HasValue || buyTake.HasValue;
            var hasMarksSell = sellStop.HasValue || sellTake.HasValue;
            var colorMarkBuy = Color.Blue;
            var colorMarkSell = Color.Red;

            // добавить коменты
            if (buyStop.HasValue)
            {
                buyStop /= buyVolumeSl;
                AddTotalDealSlTpComment(pivotIndex, buyStop.Value, hasMarksSell
                                                                      ? "BUY SL"
                                                                      : "SL", colorMarkBuy);
            }
            if (buyTake.HasValue)
            {
                buyTake /= buyVolumeTake;
                AddTotalDealSlTpComment(pivotIndex, buyTake.Value, hasMarksSell
                                                                      ? "BUY TP"
                                                                      : "TP", colorMarkBuy);
            }
            if (sellStop.HasValue)
            {
                sellStop /= sellVolumeSl;
                AddTotalDealSlTpComment(pivotIndex, sellStop.Value, hasMarksBuy
                                                                      ? "SELL SL"
                                                                      : "SL", colorMarkSell);
            }
            if (sellTake.HasValue)
            {
                sellTake /= sellVolumeTake;
                AddTotalDealSlTpComment(pivotIndex, sellTake.Value, hasMarksBuy
                                                                      ? "SELL TP"
                                                                      : "TP", colorMarkSell);
            }
        }

        private void AddTotalDealSlTpComment(int pivotIndex, float price, 
            string comment, Color cl)
        {
            var text = comment + " " + DalSpot.Instance.FormatPrice(owner.Symbol,
                                                                    price);
            var comm = new ChartComment
            {
                PivotIndex = pivotIndex,
                PivotPrice = price,
                ArrowAngle = 0,
                ArrowLength = 20,
                Text = text,
                Color = cl,
                ColorText = cl,
                HideArrow = true,
                HideBox = true,
                Name = "drag"
            };
            seriesCommentLastBar.data.Add(comm);
        }

        private void AddOpenPositionMarks(ChartControl chart, 
            MarketOrder pos, double x)
        {
            var index = (int)x;
            if (index <= 0) return;

            var tip = new AsteriskTooltip("o", string.Empty)
                          {
                              Price = pos.PriceEnter,
                              Magic = pos.ID,
                              CandleIndex = index,
                              DateStart = pos.TimeEnter,
                              Sign = "",
                              Shape = pos.Side > 0
                                          ? AsteriskTooltip.ShapeType.СтрелкаВверх
                                          : AsteriskTooltip.ShapeType.СтрелкаВниз,
                              ColorFill = pos.Side > 0 ? ColorBuy : ColorSell,
                              ColorLine = Color.Black,
                              ColorText = Color.Black,
                              Radius = ArrowSizeOpened,
                          };
            seriesAsteriks.data.Add(tip);
            
            if (ShowComments)
            {
                var currPrice = chart.StockPane.StockSeries.Data[chart.StockPane.StockSeries.Data.Count - 1].close;
                var profit = Math.Round(DalSpot.Instance.GetPointsValue(pos.Symbol, pos.Side * (currPrice - pos.PriceEnter)));
                var profitText = MakeCommentProfit(profit);

                // измерить высоту текста (необходимо для позиционирования текстовой метки)
                SizeF szText;
                using (var g = owner.CreateGraphics())
                {
                    using (var font = new Font(owner.Font.FontFamily, 
                        SeriesComment.FontSize,
                        SeriesComment.FontBold ? FontStyle.Bold : FontStyle.Regular))
                    {
                        szText = g.MeasureString("BS", font);
                    }
                }
                var h = szText.Height / 2;
                const int l = 15;
                var arLen = (int) Math.Sqrt(h*h + l*l);
                var angle = 180 - Math.Atan2(h, l) * 180 / Math.PI;

                var dealText = (pos.Side == 1 ? "Buy " : "Sell ") + pos.PriceEnter.ToStringUniformPriceFormat();
                var commPos = new ChartComment
                                {
                                    PivotIndex = x,
                                    PivotPrice = pos.PriceEnter,
                                    ArrowAngle = -angle,
                                    ArrowLength = arLen,
                                    Text = dealText,
                                    ColorText = pos.Side > 0 ? ColorBuy : ColorSell,
                                    HideArrow = true,
                                    HideBox = true,
                                    // ReSharper disable SpecifyACultureInStringConversionExplicitly
                                    Name = pos.ID.ToString(),
                                    // ReSharper restore SpecifyACultureInStringConversionExplicitly
                                    Magic = pos.ID                                    
                                };
                seriesComment.data.Add(commPos);

                var commProfit = new ChartComment
                            {
                                PivotIndex = x,
                                PivotPrice = pos.PriceEnter,
                                ArrowAngle = angle,
                                ArrowLength = arLen,
                                Text = profitText,
                                ColorText = profit > 0 ? Color.Green : Color.Red,
                                HideArrow = true,
                                HideBox = true,
                                Name = pos.ID + "PL",
                                Magic = pos.ID,
                                TextCustom = "p"
                            };
                seriesComment.data.Add(commProfit);                
            }
        }

        private static string MakeCommentProfit(double profit)
        {
            return (profit > 0 ? "(+" : "(") + profit + "п.)";
        }

        private void AddClosedOrderMarks(ChartControl chart, MarketOrder pos)
        {
            // отметка входа
            var indexEnter = (int) chart.StockSeries.GetDoubleIndexByTime(pos.TimeEnter, true);
            if (indexEnter >= 0)
            {
                var tipEnter = new AsteriskTooltip("c", string.Empty)
                                   {
                                       Magic = pos.ID,
                                       Price = pos.PriceEnter,
                                       CandleIndex = indexEnter,
                                       DateStart = pos.TimeEnter,
                                       Sign = "",
                                       Shape = pos.Side > 0
                                                   ? AsteriskTooltip.ShapeType.СтрелкаВверх
                                                   : AsteriskTooltip.ShapeType.СтрелкаВниз,
                                       ColorFill = pos.Side > 0 ? ColorBuyClosed : ColorSellClosed,
                                       ColorLine = Color.Black,
                                       ColorText = Color.Black,
                                       Radius = ArrowSizeClosed
                                   };
                seriesAsteriks.data.Add(tipEnter);
            }
            if (pos.PriceExit == null || pos.TimeExit == null)
            {
                Logger.ErrorFormat("Индикатор ордеров: ошибка при отображении закрытой позиции №{0} - нет данных закрытия", pos.ID);
                return;
            }

            var indexExit = (int) chart.StockSeries.GetDoubleIndexByTime(pos.TimeExit.Value, true);
            if (indexExit >= 0)
            {
                var mark = pos.ExitReason == PositionExitReason.Closed
                               ? ""
                               : pos.ExitReason == PositionExitReason.ClosedFromUI
                                     ? "q"
                                     : pos.ExitReason == PositionExitReason.SL
                                           ? "SL"
                                           : pos.ExitReason == PositionExitReason.TP ? "TP"
                                           : pos.ExitReason == PositionExitReason.Stopout ? "STOP" : string.Empty;

                var tipExit = new AsteriskTooltip("q", mark)
                                  {
                                      Magic = pos.ID,
                                      Price = pos.PriceExit.Value,
                                      CandleIndex = indexExit,
                                      DateStart = pos.TimeExit,
                                      Sign = "",
                                      Shape = pos.Side > 0
                                                  ? AsteriskTooltip.ShapeType.КрестВниз
                                                  : AsteriskTooltip.ShapeType.КрестВверх,
                                      ColorFill = chart.StockSeries.BarNeutralColor,
                                      ColorLine = Color.Black,
                                      ColorText = Color.Black,
                                      Radius = ArrowSizeClosed
                                  };
                seriesAsteriks.data.Add(tipExit);
            }

            // комменты у стрелок входа / выхода
            if (ShowHistoryComments)
            {
                // коммент для входа
                if (indexEnter >= 0)
                {
                    var comm = new ChartComment
                                   {
                                       PivotIndex = indexEnter,
                                       PivotPrice = pos.PriceEnter,
                                       ArrowAngle = 180,
                                       ArrowLength = 3,
                                       Text = pos.Side == 1 ? "B" : "S",
                                       Color = chart.StockSeries.BarNeutralColor,
                                       ColorText = pos.Side > 0 ? ColorBuyClosed : ColorSellClosed,
                                       HideArrow = true,
                                       HideBox = true,
                                       Name = Localizer.GetString("TitlePositionNumber") + pos.ID,
                                       Magic = pos.ID
                                   };
                    seriesComment.data.Add(comm);
                }

                // коммент для выхода
                // разобраться - выход был по ордеру либо просто закрытие
                if (indexExit >= 0)
                {
                    var commentMark =
                        pos.ExitReason == PositionExitReason.SL
                            ? "SL"
                            : pos.ExitReason == PositionExitReason.TP
                                  ? "TP"
                                  : "C";
                    var comm = new ChartComment
                                   {
                                       PivotIndex = indexExit,
                                       PivotPrice = pos.PriceExit.Value,
                                       ArrowAngle = 180,
                                       ArrowLength = 3,
                                       Text = commentMark,
                                       Color = pos.Side > 0 ? ColorBuyQuit : ColorSellQuit,
                                       ColorText = chart.StockSeries.BarNeutralColor,
                                       HideArrow = true,
                                       HideBox = true,
                                       Name = Localizer.GetString("TitlePositionNumber") + pos.ID,
                                       Magic = pos.ID
                                   };
                    seriesComment.data.Add(comm);
                }
            }
        }

        private TrendLine MakeLineFromOpeningToClosing(MarketOrder order)
        {
            if (!order.TimeExit.HasValue) return null;
            var posLine = new TrendLine
            {
                LineColor = owner.StockSeries.BarNeutralColor,
                ShapeAlpha = 255,
                ShapeFillColor = owner.StockSeries.BarNeutralColor,
                LineStyle = TrendLine.TrendLineStyle.Отрезок,
                PenWidth = 0,
                PenDashStyle = DashStyle.Dash
            };
            posLine.AddPoint((int)owner.StockSeries.GetDoubleIndexByTime(order.TimeEnter), order.PriceEnter);
            // ReSharper disable PossibleInvalidOperationException
            posLine.AddPoint((int)owner.StockSeries.GetDoubleIndexByTime(order.TimeExit.Value), order.PriceExit.Value);
            // ReSharper restore PossibleInvalidOperationException
            return posLine;
        }
        
        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (/*DisplayedOrders == OrderType.Рыночный && */ShowComments || ShowCurrent)
                UpdateProfit();
        }

        private void UpdateProfit()
        {
            var currPrice = owner.StockPane.StockSeries.Data[owner.StockPane.StockSeries.Data.Count - 1].close;
            
            var posSum = new MarketOrder { Symbol = owner.Symbol };
            float sumBuys = 0, sumSell = 0;
            var exposition = 0;

            for (var i = 0; i < openPositions.Count(); i++)
            {
                var pos = openPositions[i];
                var profit = Math.Round(DalSpot.Instance.GetPointsValue(pos.Symbol, pos.Side * 
                    (currPrice - pos.PriceEnter)));
                if (ShowComments)
                {
                    var commentProfit = seriesComment.data.FirstOrDefault(c => c.Magic == pos.ID && c.TextCustom == "p");
                    if (commentProfit != null)
                    {
                        commentProfit.Text = MakeCommentProfit(profit);
                        commentProfit.ColorText = profit > 0 ? Color.Green : Color.Red;
                    }
                }
                // считаем общую статистику
                exposition += pos.Side * pos.Volume;
                if (pos.Side > 0)
                    sumBuys += pos.Volume * pos.PriceEnter;
                else
                    sumSell += pos.Volume * pos.PriceEnter;
            }
            posSum.Volume = Math.Abs(exposition);
            posSum.Side = Math.Sign(exposition);
            posSum.PriceEnter = (float)(exposition == 0 ? 0 : Math.Round((sumBuys - sumSell) / exposition, 
                DalSpot.Instance.GetPrecision(posSum.Symbol)));
            var sumProfit = Math.Round(DalSpot.Instance.GetPointsValue(posSum.Symbol, posSum.Side * 
                (currPrice - posSum.PriceEnter)));
            if (ShowComments)
            {
                owner.Owner.ExtraTitle = 
                    (openPositions.Count == 0 || posSum.Volume == 0) ? string.Empty 
                    : string.Format("          {0} {1} по цене {2}, {3}пп.",
                    posSum.Side == 1 ? "BUY" : "SELL", 
                    posSum.Volume.ToStringUniformMoneyFormat(), 
                    posSum.PriceEnter.ToStringUniformPriceFormat(), 
                    // ReSharper disable SpecifyACultureInStringConversionExplicitly
                    sumProfit > 0 ? "+" + sumProfit : sumProfit.ToString());
                    // ReSharper restore SpecifyACultureInStringConversionExplicitly
            }

            // показываем профит на текущем баре
            if (ShowCurrent)
            {
                var curr = seriesCommentLastBar.data.FirstOrDefault(c => c.Name == "CurrProfit");
                if (curr != null)
                {
                    curr.Text = (openPositions.Count == 0 || posSum.Volume == 0) 
                        ? string.Empty 
                        : (sumProfit > 0 ? "+" + sumProfit : sumProfit.ToStringUniformMoneyFormat()) + "п";
                    curr.PivotPrice = currPrice;
                    curr.ColorText = sumProfit > 0 ? Color.Green : Color.Red;
                }
            }
        }
    }
}

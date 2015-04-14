using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Candlechart.ChartMath;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    public partial class IndicatorOrders
    {
        /// <summary>
        /// выбранный пользователем ордер
        /// поордеру отображается самая подробная информация, включая уровни SL - TP
        /// </summary>
        private MarketOrder selectedOrder;

        /// <summary>
        /// тестовая меточка (SL, TP, E) цены выбранного ордера, перетаскиваемая пользователем
        /// </summary>
        private ChartComment draggedComment;

        private enum DraggedCommentSource
        {
            MarketOrder = 0, PendingOrder, SummaryPositionOrder
        }

        private DraggedCommentSource draggedCommentSource;

        private float startDragPrice;

        /// <summary>
        /// угол, на который повернуты стрелки комментариев по выбранному ордеру, смещается
        /// </summary>
        private int selectedDealCommentAngle = 45;

        private static readonly Regex regCommentPricePart = new Regex(@"[\d\.\,]+");

        public List<IChartInteractiveObject> GetObjectsUnderCursor(int screenX, int screenY, int tolerance)
        {
            return seriesAsteriks.data.Where(item => item.IsIn(screenX, screenY)).Cast<IChartInteractiveObject>().ToList();
        }

        public string GetHint(int x, int y, double index, double price, int tolerance)
        {
            var ptClient = new Point(x, y);
            var ptScreen = owner.StockPane.PointToScreen(ptClient);
            var hint = GetLastBarMarkersHint(ptScreen);
            if (!string.IsNullOrEmpty(hint)) return hint;

            // стрелочки сделок
            return GetOrdersHint(ptScreen);
        }

        private string GetLastBarMarkersHint(Point ptClient)
        {
            var comments = seriesCommentLastBar.GetObjectsUnderCursor(ptClient.X, ptClient.Y, SeriesComment.DefaultMouseTolerance);
            if (comments.Count == 0) return string.Empty;

            // по всем позам
            var sb = new StringBuilder();
            var counterCurx = DalSpot.Instance.GetActiveFromPair(owner.Symbol, false);

            foreach (ChartComment com in comments)
            {
                var isTake = com.Text.Contains("SL")
                                   ? false
                                   : com.Text.Contains("TP") ? true : (bool?)null;
                if (!isTake.HasValue) continue;

                // отбирать только покупки / продажи
                var side = com.Text.StartsWith("BUY")
                               ? 1 : com.Text.StartsWith("SELL") ? -1 : 0;
                float totalPoints = 0, totalCount = 0;

                foreach (var pos in openPositions)
                {
                    if (side != 0 && pos.Side != side) continue;
                    var target = isTake.Value ? pos.TakeProfit : pos.StopLoss;
                    if (!target.HasValue) continue;

                    var resultAbs = pos.Side * (target.Value - pos.PriceEnter);
                    var resulPoints = DalSpot.Instance.GetPointsValue(pos.Symbol, resultAbs);
                    var resultCount = resultAbs * pos.Volume;
                    totalPoints += resulPoints;
                    totalCount += resultCount;
                }
                sb.AppendLine(string.Format("{0}, результат: {1:f0} пп, {2} {3}",
                                            isTake.Value ? "TP" : "SL", totalPoints,
                                            totalCount.ToStringUniformMoneyFormat(false),
                                            counterCurx));
            }
            return sb.ToString();
        }

        private string GetOrdersHint(Point ptClient)
        {
            var asterixes = seriesAsteriks.GetObjectsUnderCursor(ptClient.X, ptClient.Y, SeriesComment.DefaultMouseTolerance);
            if (asterixes.Count == 0) return string.Empty;

            var selectedOrders = new List<Cortege2<string, string>>();
            foreach (var aster in asterixes)
            {
                var posId = aster.Magic;
                if (aster.Name == "p")
                {
                    var orderPend = pendingOrders.FirstOrDefault(p => p.ID == posId);
                    if (orderPend != null)
                        selectedOrders.Add(new Cortege2<string, string>("",
                                                                        DisplayedOrder.MakeOrderTitle(orderPend)));
                    continue;
                }

                var list = aster.Name == "o" ? openPositions : closedPositions;
                var pos = list.FirstOrDefault(p => p.ID == posId);
                var eventStr = aster.Name == "q" ? "выход: " : "";
                if (pos != null) selectedOrders.Add(
                    new Cortege2<string, string>(eventStr, DisplayedOrder.MakeOrderTitle(pos)));
            }

            if (selectedOrders.Count > OrdersInHintMax)
            {
                var lastIndex = selectedOrders.Count - OrdersInHintMax;
                currentHintIndex++;
                if (currentHintIndex > lastIndex)
                    currentHintIndex = 0;
                lastIndex = currentHintIndex + OrdersInHintMax;
                var trimmedList = new List<Cortege2<string, string>>();
                for (var i = currentHintIndex; i < lastIndex; i++)
                    trimmedList.Add(selectedOrders[i]);
                selectedOrders = trimmedList;
            }
            var sb = new StringBuilder();
            foreach (var order in selectedOrders)
            {
                sb.AppendLine(order.a + order.b);
            }

            return sb.ToString();
        }               

        /// <summary>
        /// опеределить попадание в ордер, дабы "подсветить" его и произвести определенные действия
        /// (показать линии SL - TP, редактировать ордера)
        /// </summary> 
        public override bool OnMouseButtonUp(MouseEventArgs e, Keys modifierKeys)
        {
            // закончить драг текстовой меточки
            if (draggedComment != null)
            {
                var newPrice = draggedComment.PivotPrice;
                var commentTitle = draggedComment.Text;
                // если цена сдвинута меньше, чем на 0.1 пп - ничего не предлагать
                var minDelta = DalSpot.Instance.GetAbsValue(owner.Symbol, 0.1f);

                // предложить пользователю изменить SL / TP
                if (commentTitle.Contains("SL") || commentTitle.Contains("TP"))
                {
                    var isSl = commentTitle.Contains("SL");
                    if (Math.Abs(newPrice - startDragPrice) > minDelta)
                    {
                        // меняем рыночный ордер?
                        if (draggedCommentSource == DraggedCommentSource.MarketOrder)
                            ModifyMarketOrderAfterDrag(newPrice, isSl, selectedOrder);
                        else if (draggedCommentSource == DraggedCommentSource.SummaryPositionOrder)
                            ModifySummaryOrderAfterDrag(newPrice, isSl, commentTitle);
                    }
                }

                // забыть о текстовой метке
                draggedComment.HideBox = true;
                draggedComment.DrawFrame = false;
                draggedComment = null;
            }
            
            if (e.Button != MouseButtons.Left) return false;

            var screenPoint = owner.PointToScreen(new Point(e.X, e.Y));
            var x = screenPoint.X;
            var y = screenPoint.Y;

            // попали в специальные отметки для выделенного ордера (например, предложение редактировать 
            // или закрыть ордер)?
            var selectedCommentsHit = seriesCommentSelected.GetObjectsUnderCursor(x, y, SeriesAsteriks.DefaultMouseTolerance);
            if (selectedCommentsHit.Count > 0)
            {
                if (selectedOrder.IsOpened)//selectedCommentsHit.Any(c => c.Name == "edit") && selectedOrder != null)
                {
                    // открыть окно редактирования ордера
                    owner.Owner.CallShowWindowEditMarketOrder(selectedOrder);
                }
                else
                    if (selectedCommentsHit.Any(c => c.Name == "info") && selectedOrder != null)
                    {
                        // показать информацию по закрытому ордеру
                        owner.Owner.CallShowWindowInfoOnClosedOrder(selectedOrder);
                    }

                return false;
            }

            // найти выделенные кликом коменты или астериксы
            var comments = seriesComment.GetObjectsUnderCursor(x, y, SeriesComment.DefaultMouseTolerance);
            var asters = seriesAsteriks.GetObjectsUnderCursor(x, y, SeriesAsteriks.DefaultMouseTolerance);
            var orders = comments.Where(p => p.Magic > 0).Select(p => p.Magic).ToList();
            orders.AddRange(asters.Where(a => a.Magic > 0).Select(a => a.Magic));
            
            // ордер не выбран и до того не был выбран
            if (orders.Count == 0 && selectedOrder == null) return false;

            // ордер не выбран, но был уже выбран ранее - снять выделение
            if (selectedOrder != null)
            {
                DeselectOrder();
                return true;
            }

            // найти ордер в списке открытых или закрытых позиций
            var order = openPositions.FirstOrDefault(p => orders.Contains(p.ID))
                ?? closedPositions.FirstOrDefault(p => orders.Contains(p.ID));
            if (order == null) return false;

            var ptPane = Conversion.ScreenToWorld(new PointD(e.X, e.Y), owner.StockPane.WorldRect,
                                                    owner.StockPane.CanvasRect);
            var pointTime = owner.StockSeries.GetCandleOpenTimeByIndex((int) Math.Round(ptPane.X));
            SelectOrder(order, pointTime);

            return true;
        }

        private void ModifyMarketOrderAfterDrag(double newPrice, bool isSl, MarketOrder order)
        {
            // подтвердить намерение пользователя
            var msg = string.Format(Localizer.GetString("MessageModifyMarketOrderByDraggingFmt"),
                                    isSl ? "SL" : "TP",
                                    newPrice.ToStringUniformPriceFormat(true),
                                    order.ID,
                                    order.Side > 0 ? "BUY" : "SELL",
                                    order.Volume.ToStringUniformMoneyFormat(),
                                    order.Symbol);
            msg += GetOrderFiredScenarioProfitLoss((float)newPrice, new List<MarketOrder> { order });
            if (MessageBox.Show(msg, 
                Localizer.GetString("TitleConfirmation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (isSl)
                    order.StopLoss = (float)newPrice;
                else
                    order.TakeProfit = (float)newPrice;

                // вызвать событие - на изменение ордера
                owner.Owner.CallEditMarketOrderRequest(order);
            }
            else
                draggedComment.PivotPrice = startDragPrice;
        }

        private void ModifySummaryOrderAfterDrag(double newPrice, bool isSl, string commentTitle)
        {
            // суммарный ордер - либо BUY SL (SELL SL, SELL TP, BUY TP)
            // либо SL - TP
            // предложить выставить ордер всем покупкам / продажам по паре
            var targetSide = commentTitle.StartsWith("SELL", StringComparison.OrdinalIgnoreCase)
                                 ? -1
                                 : commentTitle.StartsWith("BUY", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            // если не указано, установлен ли ордер для покупок или продаж,
            // определить самостоятельно
            if (targetSide == 0)
            {
                var sampleOrder = openPositions.FirstOrDefault(o =>
                    isSl ? o.StopLoss.HasValue : o.TakeProfit.HasValue);
                if (sampleOrder != null)
                    targetSide = sampleOrder.Side;
            }

            // учитываются ордера по данному направлению с установленным SL - TP ордером
            var orders = targetSide == 0
                             ? openPositions
                             : openPositions.Where(o => o.Side == targetSide 
                                 && (isSl ? o.StopLoss > 0 : o.TakeProfit > 0)).ToList();
            if (orders.Count == 0) return;
            if (orders.Count == 1)
            {
                ModifyMarketOrderAfterDrag(newPrice, isSl, orders[0]);
                return;
            }

            var ordersStr = targetSide != 0 ? " (" + (targetSide > 0 ? "BUY " : "SELL ") + owner.Symbol + ")" : "";
            var msg = string.Format(Localizer.GetString("MessageModifySummaryOrderByDraggingFmt"),
                                    isSl ? "SL" : "TP",
                                    newPrice.ToStringUniformPriceFormat(true),
                                    orders.Count,
                                    ordersStr);
            msg += GetOrderFiredScenarioProfitLoss((float) newPrice, orders);
            if (MessageBox.Show(msg, Localizer.GetString("TitleConfirmation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                foreach (var order in orders)
                {
                    if (isSl)
                        order.StopLoss = (float)newPrice;
                    else
                        order.TakeProfit = (float)newPrice;

                    // вызвать событие - на изменение ордера
                    owner.Owner.CallEditMarketOrderRequest(order);
                }
            }
            else
                draggedComment.PivotPrice = startDragPrice;
        }

        /// <summary>
        /// начать визуальное редактировние (перетаскивание) ордера
        /// </summary>
        public override bool OnMouseButtonDown(MouseEventArgs e, Keys modifierKeys)
        {
            if (e.Button != MouseButtons.Left) return false;
            
            draggedComment = null;
            var clientPoint = owner.PointToScreen(new Point(e.X, e.Y));
            var x = clientPoint.X;
            var y = clientPoint.Y;

            // перетаскиваем ордер суммарной позы?
            var selectedCommentsLastBar = seriesCommentLastBar.GetObjectsUnderCursor(x, y, 
                SeriesComment.DefaultMouseTolerance).Where(c => c.Name == "drag").ToList();
            if (selectedCommentsLastBar.Count > 0)
            {
                draggedComment = (ChartComment) selectedCommentsLastBar[0];
                draggedCommentSource = DraggedCommentSource.SummaryPositionOrder;
            }
            else
                if (selectedOrder != null)
                {
                    // перетаскиваем ордер открытой позы?
                    var selectedCommentsHit = seriesCommentSelected.GetObjectsUnderCursor(x, y, SeriesComment.DefaultMouseTolerance);
                    draggedComment = (ChartComment)selectedCommentsHit.FirstOrDefault(c => c.Name == "drag");
                    draggedCommentSource = DraggedCommentSource.MarketOrder;
                }

            if (draggedComment == null) return false;

            startDragPrice = (float)draggedComment.PivotPrice;
            draggedComment.DrawFrame = true;
            draggedComment.HideBox = false;
            
            return true;
        }

        public override bool OnMouseButtonMove(MouseEventArgs e, Keys modifierKeys)
        {
            if (draggedComment == null) return false;

            // перетащить цену вверх или вниз
            var clientPoint = owner.PointToScreen(new Point(e.X, e.Y));
            var ptClient = owner.Owner.PointToClient(clientPoint);
            var ptWorld = Conversion.ScreenToWorld(new PointD(ptClient.X, ptClient.Y),
                                                   owner.StockPane.WorldRect, owner.StockPane.CanvasRect);

            // перетащить комментарий с ценой
            var oldPrice = draggedComment.PivotPrice;
            var commentsToMove = seriesCommentSelected.data.Where(c => c.Name == "drag" && c.PivotPrice == oldPrice);
            foreach (var com in commentsToMove)
            {
                com.PivotPrice = ptWorld.Y;
            }
            draggedComment.PivotPrice = ptWorld.Y;
            
            // если текст комментария включает цену - обновить ее
            draggedComment.Text = regCommentPricePart.Replace(draggedComment.Text, 
                    ptWorld.Y.ToStringUniformPriceFormat());

            // перетащить линию цены
            var line = seriesSelectedLines.data.FirstOrDefault(l => l.Name == "drag" &&
                                                                    l.linePoints[0].Y == l.linePoints[1].Y &&
                                                                    l.linePoints[0].Y == oldPrice);
            if (line != null)
            {
                line.linePoints[0] = new PointD(line.linePoints[0].X, ptWorld.Y);
                line.linePoints[1] = new PointD(line.linePoints[1].X, ptWorld.Y);
            }

            return true;
        }

        private string GetOrderFiredScenarioProfitLoss(float orderPrice, List<MarketOrder> dealsWithOrder)
        {
            var symbol = owner.Symbol;
            float profitCounterSelected, totalProfit;
            
            try
            {
                // профит (убыток) по выбранным позициям
                profitCounterSelected = (dealsWithOrder == null || dealsWithOrder.Count == 0 || dealsWithOrder[0] == null) ? 0 
                    : dealsWithOrder.Sum(d => d.CalculateProfit(orderPrice));

                // профит (убыток) по прочим позициям
                totalProfit = profitCounterSelected;
                if (openPositions != null && openPositions.Count > 0)
                {
                    var curPrice = owner.StockSeries.Data.Candles[owner.StockSeries.DataCount - 1].close;
                    foreach (var pos in openPositions)
                    {
                        if (dealsWithOrder != null && dealsWithOrder.Contains(pos))
                            continue;
                        
                        // скорректировать цену выхода:
                        // поза может закрыться по стопу / тейку
                        var priceExit = orderPrice;
                        var exitSl = 0f;
                        if (pos.StopLoss.HasValue && pos.StopLoss.Value > 0)
                            if (pos.StopLoss.Value.Between(curPrice, orderPrice))
                                exitSl = pos.StopLoss.Value;
                        var exitTp = 0f;
                        if (pos.TakeProfit.HasValue && pos.TakeProfit.Value > 0)
                            if (pos.TakeProfit.Value.Between(curPrice, orderPrice))                        
                                exitTp = pos.TakeProfit.Value;
                        
                        // сработает тот, что ближе к текущей цене
                        if (exitSl > 0 && exitTp > 0)
                        {
                            priceExit = (Math.Abs(curPrice - exitSl) < Math.Abs(curPrice - exitTp)) ? exitSl : exitTp;
                        }
                        else
                        {
                            if (exitSl > 0) priceExit = exitSl;
                            else if (exitTp > 0) priceExit = exitTp;
                        }
                        
                        // таки посчитать профит по позе
                        totalProfit += pos.CalculateProfit(priceExit);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в GetOrderFiredScenarioProfitLoss", ex);
                throw;
            }

            // получить базовую и контрвалюту
            var counterCurx = DalSpot.Instance.GetActiveFromPair(symbol, false);
            var baseCurx = DalSpot.Instance.GetActiveFromPair(symbol, true);
            
            var resultCurrency = counterCurx;
            const string depoCurrency = "USD"; // !!
            
            // возможен пересчет в валюту депозита
            if (counterCurx == depoCurrency || baseCurx == depoCurrency)
            {
                resultCurrency = depoCurrency;
                var counterDepoRate = counterCurx == depoCurrency ? 1 : orderPrice;
                profitCounterSelected /= counterDepoRate;
                totalProfit /= counterDepoRate;
            }

            // текст сообщения
            var countRem = openPositions.Count - dealsWithOrder.Count;
            var remString = countRem == 0
                                ? ""
                                : "С учетом оставшихся " + countRem +
                                  " открытых ордеров финансовый результат при данной цене составит " +
                                  totalProfit.ToStringUniformMoneyFormat() + " " + resultCurrency;
            var msg = string.Format("{0}При исполнении ордера р-т составит {1} {2}.{0}{3}",
                Environment.NewLine,                         
                profitCounterSelected.ToStringUniformMoneyFormat(), resultCurrency,
                remString);
            return msg;
        }

        private void DeselectOrder()
        {
            // снять рамки с меток
            var orderComments = seriesComment.data.Where(p => p.Magic == selectedOrder.ID);
            foreach (var comment in orderComments)
            {
                comment.HideBox = true;
            }

            // удалить линии SL-TP, линии, связывающие вход и выход
            seriesSelectedLines.data.Clear();

            // и лишние текстовые метки
            seriesCommentSelected.data.Clear();

            selectedOrder = null;
        }

        /// <summary>
        /// отобразить уровни SL-TP, высветить ордер другим цветом 
        /// </summary>        
        private void SelectOrder(MarketOrder order, DateTime selectedDate)
        {
            // рамочками выделить текстовые метки
            var orderComments = seriesComment.data.Where(p => p.Magic == order.ID);
            foreach (var comment in orderComments)
            {
                comment.HideBox = false;
                // comment.FillTransparency = 0;
                comment.DrawFrame = false;
            }

            // добавить линию, связывающую вход и выход
            if (order.TimeExit.HasValue && order.PriceExit.HasValue)
                seriesSelectedLines.data.Add(MakeLineFromOpeningToClosing(order));
            
            // добавить линии SL - TP
            ShowSelectedDealSlTpLines(order);

            // добавить коммент на отметке входа либо выхода
            var showOnExitMark = false;
            if (order.TimeExit.HasValue)
                if (Math.Abs((order.TimeExit.Value - selectedDate).TotalMinutes) <
                    Math.Abs((order.TimeEnter - selectedDate).TotalMinutes))
                    showOnExitMark = true; // таки на отметке входа
            ShowSelectedDealComments(order, showOnExitMark);

            selectedOrder = order;
        }

        /// <summary>
        /// нарисовать "полочки" уровней SL - TP
        /// </summary>
        private void ShowSelectedDealSlTpLines(MarketOrder order)
        {
            if (order.StopLoss == null && order.TakeProfit == null) return;
            var indexStart = (int) owner.StockSeries.GetDoubleIndexByTime(order.TimeEnter);
            var indexEnd = order.TimeExit.HasValue
                               ? (int) owner.StockSeries.GetDoubleIndexByTime(order.TimeExit.Value)
                               : indexStart;
            // линия не должна быть слишком короткой
            var sizePixel = Conversion.WorldToScreen(new SizeD(indexEnd - indexStart, 0),
                                                        owner.StockPane.WorldRect, owner.StockPane.CanvasRect);
            const int targetWidth = 35;
            if (sizePixel.Width < targetWidth)
            {
                var newSize = Conversion.ScreenToWorld(new SizeD(targetWidth, 0),
                                                owner.StockPane.WorldRect, owner.StockPane.CanvasRect);
                var len = (int) Math.Round(newSize.Width);
                indexEnd += len;
            }

            var prices = new List<float>();
            var tags = new List<string>();

            if (order.StopLoss.HasValue)
            {
                prices.Add(order.StopLoss.Value);
                tags.Add("SL");
            }
            if (order.TakeProfit.HasValue)
            {
                prices.Add(order.TakeProfit.Value);
                tags.Add("TP");
            }

            // добавить линии
            var priceIndex = 0;
            foreach (var price in prices)
            {
                var posLine = new TrendLine
                                  {
                                      LineColor = owner.StockSeries.DownLineColor,
                                      ShapeAlpha = 255,
                                      ShapeFillColor = owner.StockSeries.DownLineColor,
                                      LineStyle = TrendLine.TrendLineStyle.Отрезок,
                                      PenWidth = 1,
                                      PenDashStyle = DashStyle.Dot,
                                      Name = "drag"
                                  };
                posLine.AddPoint(indexStart, price);
                posLine.AddPoint(indexEnd, price);
                seriesSelectedLines.data.Add(posLine);

                // добавить с каждой стороны масенький комент: SL или TP
                var comment = new ChartComment
                                  {
                                      Text = tags[priceIndex],
                                      PivotPrice = price,
                                      ArrowAngle = 180,
                                      ArrowLength = 2,
                                      PivotIndex = indexStart,
                                      ColorText = owner.visualSettings.SeriesForeColor,
                                      Color = owner.visualSettings.SeriesForeColor,
                                      DrawFrame = false,
                                      HideBox = true,
                                      HideArrow = true,
                                      Name = "drag"
                                  };
                seriesCommentSelected.data.Add(comment);
                comment = new ChartComment
                    {
                        Text = tags[priceIndex],
                        PivotPrice = price,
                        ArrowAngle = 0,
                        ArrowLength = 2,
                        PivotIndex = indexEnd,
                        ColorText = owner.visualSettings.SeriesForeColor,
                        Color = owner.visualSettings.SeriesForeColor,
                        DrawFrame = false,
                        HideBox = true,
                        HideArrow = true,
                        Name = "drag"
                    };
                seriesCommentSelected.data.Add(comment);
                priceIndex++;
            }
        }
    
        /// <summary>
        /// добавить подробные коменты по сделке
        /// </summary>
        private void ShowSelectedDealComments(MarketOrder order, bool selectedByExitMark)
        {
            var listComments = new List<string>();
            
            // данные по входу в рынок
            var commentMain = new StringBuilder
                              (((DealType) order.Side).ToString() + " "
                              + order.Volume.ToStringUniformMoneyFormat() + " "
                              + order.Symbol + Environment.NewLine +
                              order.TimeEnter.ToString("dd MMM HH:mm") + ", "
                              + order.PriceEnter.ToStringUniformPriceFormat(true));
            
            // SL, TP, результат выхода из рынка
            if (order.TimeExit.HasValue || order.StopLoss.HasValue || order.TakeProfit.HasValue)
            {
                commentMain.AppendLine();
                if (order.StopLoss.HasValue || order.TakeProfit.HasValue)
                {
                    if (order.StopLoss.HasValue)
                        commentMain.AppendFormat("SL: {0}", order.StopLoss.Value.ToStringUniformPriceFormat());
                    if (order.StopLoss.HasValue && order.TakeProfit.HasValue)
                        commentMain.Append(" ");
                    if (order.TakeProfit.HasValue)
                        commentMain.AppendFormat("TP: {0}", order.TakeProfit.Value.ToStringUniformPriceFormat());
                    if (order.TimeExit.HasValue)
                        commentMain.AppendLine();
                }
                if (order.TimeExit.HasValue)
                {
                    commentMain.AppendFormat("Закрыт {0} по {1}",
                        order.TimeExit.Value.ToString("dd MMM HH:mm"),
                        // ReSharper disable PossibleInvalidOperationException
                        order.PriceExit.Value.ToStringUniformPriceFormat());
                        // ReSharper restore PossibleInvalidOperationException
                    commentMain.AppendLine();
                    commentMain.AppendFormat("Результат: {0}",
                        order.ResultDepo.ToStringUniformMoneyFormat());
                }
            }

            listComments.Add(commentMain.ToString());

            // предложение закрыть или редактировать ордер
            //if (!order.TimeExit.HasValue)
            //    listComments.Add("Редактировать /" + Environment.NewLine + "Закрыть");

            // точка привязки (к маркеру входа или выхода)
            var pivot = selectedByExitMark
                // ReSharper disable PossibleInvalidOperationException
                            ? new PointD(owner.StockSeries.GetDoubleIndexByTime(order.TimeExit.Value, true),
                                         order.PriceExit.Value)
                // ReSharper restore PossibleInvalidOperationException
                            : new PointD(owner.StockSeries.GetDoubleIndexByTime(order.TimeEnter, true), 
                                order.PriceEnter);

            // по текущему результату определить, прибыльный ордер или убыточный
            var profitSide = Math.Sign(order.ResultDepo);
            if (order.IsOpened)
            {
                var curPrice = owner.StockSeries.Data.Candles[owner.StockSeries.DataCount - 1].close;
                profitSide = Math.Sign(order.Side*(curPrice - order.PriceEnter));
            }
            var colorFill = profitSide > 0
                                ? Color.LightGreen : profitSide < 0 ? Color.LightCoral : Color.BlanchedAlmond;

            // таки добавить коменты
            var angle = selectedDealCommentAngle;
            const int arrowLen = 90;            

            foreach (var text in listComments)
            {
                var x = pivot.X;
                var price = pivot.Y;
                var comment = new ChartComment
                                  {
                                      Text = text,
                                      ArrowLength = arrowLen,
                                      ArrowAngle = angle,
                                      HideBox = false,
                                      HideArrow = false,
                                      DrawFrame = true,
                                      FillTransparency = 200,
                                      ColorFill = colorFill,
                                      Color = owner.visualSettings.SeriesForeColor,
                                      ColorText = owner.visualSettings.SeriesForeColor,
                                      PivotIndex = x,
                                      PivotPrice = price
                                  };
                // развернуть комментарий так, чтобы он не накрывал ничего лишнего и не вылезал за пределы экрана
                AdjustDealCommentArrowAngle(comment, order);
                if (order.IsClosed) comment.Name = "info";
                seriesCommentSelected.data.Add(comment);

                angle += 120;
            }

            // пометить комментарий с предложением редактировать / закрыть ордер
            if (!order.TimeExit.HasValue)
                seriesCommentSelected.data[seriesCommentSelected.data.Count - 1].Name = "edit";
            
            // сместить угол стрелок
            selectedDealCommentAngle += 45;
            if (selectedDealCommentAngle > 360)
                selectedDealCommentAngle -= 360;
        }

        private void AdjustDealCommentArrowAngle(ChartComment comment, MarketOrder order)
        {
            if (!order.TimeExit.HasValue) return;
            // получить основное направление
            // (продолжение линии open - close)
            var ptE = Conversion.WorldToScreen(
                    new PointD(owner.StockSeries.GetDoubleIndexByTime(order.TimeEnter), order.PriceEnter),
                    owner.StockPane.WorldRect, owner.StockPane.CanvasRect);
            var ptQ = Conversion.WorldToScreen(
                    // ReSharper disable PossibleInvalidOperationException
                    new PointD(owner.StockSeries.GetDoubleIndexByTime(order.TimeExit.Value), order.PriceExit.Value),
                    // ReSharper restore PossibleInvalidOperationException
                    owner.StockPane.WorldRect, owner.StockPane.CanvasRect);
            var ptPivot = Conversion.WorldToScreen(new PointD(comment.PivotIndex, comment.PivotPrice),
                    owner.StockPane.WorldRect, owner.StockPane.CanvasRect);
            var ptStart = (Math.Abs(ptPivot.X - ptE.X) < Math.Abs(ptPivot.X - ptQ.X)) ? ptQ : ptE;
            var mainAngle = Math.Atan2(ptPivot.Y - ptStart.Y, ptPivot.X - ptStart.X) * 180 / Math.PI;
            
            // "округлить" угол
            mainAngle = Math.Round(mainAngle/15) * 15;

            // расставить направления по приоритету
            const int numAngles = 8;
            var angles = new double[numAngles];
            angles[0] = mainAngle;            
            for (var i = 0; i < numAngles / 2 - 1; i++)
            {
                var delta = (i + 1) * 360 / numAngles;
                angles[i * 2 + 1] = mainAngle + delta;
                angles[i * 2 + 2] = mainAngle - delta;
            }
            angles[numAngles - 1] = -mainAngle;

            // выбрать направление, для которого комментарий не вылезает за рамки экрана
            var foundAngle = false;
            var screenRect = seriesComment.Owner.CanvasRect;
            using (var gr = owner.CreateGraphics())
            foreach (var angle in angles)
            {
                comment.ArrowAngle = angle;
                var commentRect = comment.GetCommentRectangle(gr, 
                    owner.StockPane.WorldRect,
                    owner.StockPane.CanvasRect);
                // не вылез ли комментарий за пределы экрана?
                if (commentRect.Left < screenRect.Left || commentRect.Top < screenRect.Top ||
                    commentRect.Right > screenRect.Right || commentRect.Bottom > screenRect.Bottom)
                    continue;
                foundAngle = true;
                break;
            }
            
            // угол поворота по-умолчанию
            if (!foundAngle)
                comment.ArrowAngle = angles[0];
        }
    }
}

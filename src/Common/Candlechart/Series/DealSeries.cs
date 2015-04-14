using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using Candlechart.ChartMath;
using Candlechart.Core;
using Entity;
using TradeSharp.Contract.Entity;

namespace Candlechart.Series
{
    public class DealSeries : Series
    {
        public override int DataCount { get { return deals.Count; } }

        public enum DealResultFilter
        {
            All = 0, PlusOnly, MinusOnly
        }

        public DealResultFilter dealResultFilter = DealResultFilter.All;

        private enum DealEventSymbol
        {
            TagDown, TagUp, Circle, Diamond
        }

        [Flags]
        public enum EnabledVirtualDealSymbols
        {
            None = 0, Open = 1, Close = 2, Protect = 4, TP = 8, AP = 16, SL = 32, All = 255
        }

        public EnabledVirtualDealSymbols enabledVirtualDealSymbols = EnabledVirtualDealSymbols.All;

        private const int ItemHitRange = 5;

        public List<MarketOrder> deals = new List<MarketOrder>();

        public enum DrawMode
        {
            Orders,  /* по-умолчанию на графике доходности */
            Events,  /* ... на свечном графике */
            Auto     
        }

        public DrawMode seriesDrawMode = DrawMode.Auto;

        private bool symbolMode = true;
        /// <summary>
        /// true - отображать момент входа-выхода в виде символов
        /// false - рисовать отметки защиты/стопа
        /// </summary>
        public bool SymbolMode
        {
            get { return symbolMode; }
            set { symbolMode = value; }
        }
        
        /// <summary>
        /// Режим рисования - ордера (вертикальные линии с подпиьсю) или
        /// события (позиция закрыта, защищена)
        /// </summary>
        public DrawMode GetSeriesDrawMode(RectangleD worldRect)
        {
            if (seriesDrawMode == DrawMode.Auto)
            {
                if (worldRect == Chart.StockPane.WorldRect)
                    return DrawMode.Events;
                return DrawMode.Orders;
            }
            return seriesDrawMode;
        }                

        #region Drawing Settings
        public bool ShowSymbolEnter { get; set; }
        public bool ShowSymbolExit { get; set; }
        public bool ShowSymbolProtect { get; set; }
        public bool ShowSymbolAP { get; set; }
        public bool ShowSymbolTP { get; set; }
        public bool LinkLockingDeals { get; set; }

        private Color colorProfit = Color.ForestGreen;
        [Browsable(true)]
        [Category("Цветовая схема")]
        [DisplayName("Прибыльные сделки")]
        public Color ColorProfit
        {
            get { return colorProfit; }
            set { colorProfit = value; }
        }
        private Color colorLoss = Color.Maroon;
        [Browsable(true)]
        [Category("Цветовая схема")]
        [DisplayName("Убыточные сделки")]
        public Color ColorLoss
        {
            get { return colorLoss; }
            set { colorLoss = value; }
        }
        #endregion


        public DealSeries(string name)
            : base(name)
        {
        }

        public DealSeries CloneSeries(string name)
        {
            var vs = new DealSeries(name)
                         {
                             symbolMode = symbolMode,
                             ShowSymbolEnter = ShowSymbolEnter,
                             ShowSymbolExit = ShowSymbolExit,
                             ShowSymbolProtect = ShowSymbolProtect,
                             ShowSymbolAP = ShowSymbolAP,
                             ShowSymbolTP = ShowSymbolTP,
                             LinkLockingDeals = LinkLockingDeals
                         };

            foreach (var d in deals) vs.deals.Add(d);
            return vs;
        }

        public void CopyDealsInfo(DealSeries src)
        {
            deals.Clear();
            foreach (var d in src.deals) deals.Add(d);
        }

        public override bool GetXExtent(ref double left, ref double right)
        {            
            return false;
        }
        public override bool GetYExtent(double left, double right, ref double top, ref double bottom)
        {            
            return false;
        }
        public override void Draw(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {            
            base.Draw(g, worldRect, canvasRect);
            if (GetSeriesDrawMode(worldRect) == DrawMode.Orders)
                DrawOrders(g, worldRect, canvasRect);
            else
                DrawEvents(g, worldRect, canvasRect);
        }

        private void DrawOrders(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            using (var font = new Font(Chart.Font.FontFamily, 7.0F))
            {
                using (var penOpen = new Pen(Chart.ForeColor, LineWidth) { Alignment = PenAlignment.Center })
                {
                    using (var penClose = new Pen(Color.Maroon, LineWidth) { Alignment = PenAlignment.Center })
                    {
                        using (var penProtect = new Pen(Color.Green, LineWidth) { Alignment = PenAlignment.Center })
                        {
                            using (var foreBrush = new SolidBrush(Chart.ForeColor))
                            {
                                using (var backBrush = new SolidBrush(Color.White))
                                {
                                    using (var brushProfit = new SolidBrush(ColorProfit))
                                    {
                                        using (var brushLoss = new SolidBrush(ColorLoss))
                                        {
                                            foreach (var vd in deals)
                                            {
                                                if (IsDealFiltered(vd)) continue;
                                                DrawDealOrders(vd, g, worldRect, canvasRect, font, penOpen, penClose,
                                                               penProtect,
                                                               foreBrush, backBrush, brushProfit, brushLoss);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DrawDealOrders(MarketOrder d, Graphics g, RectangleD worldRect, Rectangle canvasRect,
            Font font, Pen penOp, Pen penClos, Pen penProtect, Brush foreBrush, Brush backBrush, Brush brushProfit, Brush brushLoss)
        {
            if ((enabledVirtualDealSymbols & EnabledVirtualDealSymbols.Open) != EnabledVirtualDealSymbols.None)
                DrawDealLine(d.TimeEnter, d.ID + " open", font, penOp, g, worldRect, canvasRect, foreBrush, backBrush, 1);
            
            /*if (d is BacktestPosition)
            {
                var backPos = (BacktestPosition) d;
                if ((enabledVirtualDealSymbols & EnabledVirtualDealSymbols.Protect) != EnabledVirtualDealSymbols.None)
                    if (backPos.TimeProtected.HasValue)
                        DrawDealLine(backPos.TimeProtected.Value, d.ID + " protect", font, penProtect, g, worldRect,
                                     canvasRect, foreBrush, backBrush, 1);
                if ((enabledVirtualDealSymbols & EnabledVirtualDealSymbols.SL) != EnabledVirtualDealSymbols.None)
                    if (backPos.TimeStoplossed.HasValue)
                        DrawDealLine(backPos.TimeStoplossed.Value, d.ID + " stop", font, penClos, g, worldRect, canvasRect,
                                     foreBrush, backBrush, -1);
            }*/
            if ((enabledVirtualDealSymbols & EnabledVirtualDealSymbols.Close) != EnabledVirtualDealSymbols.None)
                if (!d.IsOpened)
                {
                    var profit = (int) d.ResultPoints;
                    var brush = brushProfit;
                    if (profit < 0) brush = brushLoss;
                    DrawDealLine(d.TimeExit ?? DateTime.MaxValue, profit.ToString(), font, penClos, g, worldRect, canvasRect,
                                 brush, backBrush, 0);
                }
        }

        private void DrawDealLine(DateTime occur, string title, Font font, Pen pen, Graphics g,
            RectangleD worldRect, Rectangle canvasRect, Brush foreBrush, Brush backBrush, int alignTop)
        {
            double chartX = Chart.CandleRange.GetXCoord(occur);
            var x = (int)Conversion.WorldToScreen(new PointD(chartX, 0), worldRect, canvasRect).X;
            g.DrawLine(pen, x, canvasRect.Top, x, canvasRect.Bottom);
            SizeF sz = g.MeasureString(title, font);            
            StringAlignment lineAlignment = StringAlignment.Near;
            if (alignTop < 0)
                lineAlignment = StringAlignment.Far;
            if (alignTop == 0)
                lineAlignment = StringAlignment.Center;
            int textY = canvasRect.Top;
            if (alignTop < 0) textY = canvasRect.Bottom;
            if (alignTop == 0) textY = (canvasRect.Top + canvasRect.Bottom) / 2;
            if (alignTop > 0)
            {
                g.FillRectangle(backBrush, x - (int)sz.Height, textY, (int)sz.Height, (int)sz.Width);
                g.DrawRectangle(pen, x - (int)sz.Height, textY, (int)sz.Height, (int)sz.Width);
            }
            if (alignTop < 0)
            {
                g.FillRectangle(backBrush, x - (int)sz.Height, textY - (int)sz.Width, sz.Height, (int)sz.Width);
                g.DrawRectangle(pen, x - (int)sz.Height, textY - (int)sz.Width, sz.Height, (int)sz.Width);
            }
            if (alignTop == 0)
            {
                g.FillRectangle(backBrush, x - (int)sz.Height / 2, textY - (int)sz.Width / 2, (int)sz.Height, (int)sz.Width);
                g.DrawRectangle(pen, x - (int)sz.Height / 2, textY - (int)sz.Width / 2, (int)sz.Height, (int)sz.Width);
            }
            StringAlignment textAlignY = StringAlignment.Far;
            if (alignTop == 0)
                textAlignY = StringAlignment.Center;            
            g.DrawString(title, font, foreBrush, x, textY,
                new StringFormat(StringFormatFlags.DirectionVertical) { Alignment = lineAlignment, LineAlignment = textAlignY });            
        }

        private void DrawEvents(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            using (var font = new Font(Chart.Font.FontFamily, 7.0F))
            {
                using (var penBlack = new Pen(Color.Black, 1) { Alignment = PenAlignment.Center })
                {
                    using (var penClose = new Pen(Color.Maroon, LineWidth) {Alignment = PenAlignment.Center})
                    {
                        using (var penProtect = new Pen(Color.Green, LineWidth) {Alignment = PenAlignment.Center})
                        {
                            using (var foreBrush = new SolidBrush(Chart.ForeColor))
                            {
                                using (var backBrush = new SolidBrush(Color.White))
                                {
                                    using (var backBrushUp = new SolidBrush(Color.Blue))
                                    {
                                        using (var backBrushDown = new SolidBrush(Color.Red))
                                        {
                                            foreach (MarketOrder vd in deals)
                                            {
                                                if (IsDealFiltered(vd)) continue;
                                                DrawDealEvents(vd, g, worldRect, canvasRect, font, penClose, penProtect,
                                                               penBlack, foreBrush, backBrush, backBrushUp, backBrushDown);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //ConnectLockedDeals(g, worldRect, canvasRect);
        }

        private void DrawDealEvents(MarketOrder d, Graphics g, RectangleD worldRect, Rectangle canvasRect,
            Font font, Pen penClos, Pen penProtect, Pen penBlack,
            Brush foreBrush, Brush backBrush, Brush backBrushUp, Brush backBrushDown)
        {
            // вход
            if (ShowSymbolEnter)
                DrawDealEvent(d.TimeEnter, 
                    d.PriceEnter, 
                    d.Side == 1 ? "b" : "s", 
                    g, worldRect, canvasRect, font, penBlack,
                    d.Side == 1 ? backBrushUp : backBrushDown,
                    backBrush, 
                    d.Side == 1 ? DealEventSymbol.TagUp : DealEventSymbol.TagDown);
            // выход
            if (!d.IsOpened && ShowSymbolExit)
                DrawDealEvent(d.TimeExit ?? DateTime.MaxValue, d.PriceExit.Value, 
                    "c", g, worldRect, canvasRect, font, penClos,
                    foreBrush, backBrush,
                    d.Side == 1 ? DealEventSymbol.TagUp : DealEventSymbol.TagDown);
            
            // защита
            /*
            if (d is BacktestPosition)
            {
                var backPos = (BacktestPosition) d;
                if (backPos.TimeProtected.HasValue && ShowSymbolProtect)
                    DrawDealEvent(backPos.TimeProtected.Value, 
                        backPos.PriceProtected.Value, "p", g, worldRect, canvasRect, 
                        font, penProtect, foreBrush, backBrush, DealEventSymbol.Circle);

                // ТП
                if (backPos.TimeTakenprofit.HasValue && ShowSymbolTP)
                    DrawDealEvent(backPos.TimeTakenprofit.Value,
                        backPos.PriceTakenprofit.Value, "TP", g, worldRect, canvasRect, 
                        font, penClos, foreBrush, backBrush,
                        DealEventSymbol.Circle);
            }*/
        }

        ///// <summary>
        ///// связать пунктирными дугами локированные сделки
        ///// </summary>
        //private void ConnectLockedDeals(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        //{
        //    if (!LinkLockingDeals) return;
        //    var pens = new[]
        //                   {
        //                       new Pen(Color.Green) { DashStyle = DashStyle.Dot },
        //                       new Pen(Color.Red) { DashStyle = DashStyle.Dot },
        //                       new Pen(Color.Blue) { DashStyle = DashStyle.Dot },
        //                       new Pen(Color.Purple) { DashStyle = DashStyle.Dot },                               
        //                       new Pen(Color.Black) { DashStyle = DashStyle.Dot }                               
        //                   };                        
        //    // - 1 - разбить на подмножества по принадлежности к серии
        //    var lockLists = new Dictionary<int, List<Position>>();
        //    foreach (var deal in deals)
        //    {
        //        if (deal.LockingSeriesID == 0) continue;
        //        if (lockLists.ContainsKey(deal.LockingSeriesID))
        //            lockLists[deal.LockingSeriesID].Add(deal);
        //        else
        //            lockLists.Add(deal.LockingSeriesID, new List<IVirtualDeal> { deal });
        //    }
        //    // - 2 - упорядочить каждую серию по времени открытия
        //    foreach (var list in lockLists.Values)
        //    {
        //        list.Sort((a, b) => a.EntryTime > b.EntryTime ? 1 : a.EntryTime == b.EntryTime ? 0 : -1);
        //    }                
        //    // - 3 - соединить дугами
        //    var bendCurveUp = true;
        //    var penIndex = 0;
        //    foreach (var list in lockLists.Values)
        //    {
        //        for (var i = 1; i < list.Count; i++)
        //        {
        //            ConnectLockedPair(list[i], list[i - 1], g, pens[penIndex], worldRect, canvasRect, bendCurveUp);
        //            bendCurveUp = !bendCurveUp;
        //        }
        //        penIndex++;
        //        if (penIndex >= pens.Length) penIndex = 0;
        //    }            
        //}
        ///// <summary>
        ///// связать две локированные сделки
        ///// </summary>        
        //private void ConnectLockedPair(IVirtualDeal a, IVirtualDeal b, Graphics g, Pen p,
        //    RectangleD worldRect, Rectangle canvasRect, bool bendUp)
        //{
        //    double chartX = Chart.CandleRange.GetXCoord(a.EntryTime);
        //    var centerA = Conversion.WorldToScreen(new PointD(chartX, (double)a.EntryPrice), worldRect, canvasRect);
        //    chartX = Chart.CandleRange.GetXCoord(b.EntryTime);
        //    var centerB = Conversion.WorldToScreen(new PointD(chartX, (double)b.EntryPrice), worldRect, canvasRect);
        //    var cX = (int)(centerA.X + centerB.X)/2;
        //    var abLen = (centerA.X - centerB.X)*(centerA.X - centerB.X)
        //                + (centerA.Y - centerB.Y)*(centerA.Y - centerB.Y);
        //    abLen = Math.Sqrt(abLen);
        //    var cY = bendUp
        //                 ? (int) (Math.Min(centerA.Y, centerB.Y) + abLen/2)
        //                 : (int) (Math.Max(centerA.Y, centerB.Y) - abLen/2);
            
        //    var points = new []
        //                     {   new Point((int) centerA.X, (int) centerA.Y), 
        //                         new Point(cX, cY),
        //                         new Point((int) centerB.X, (int) centerB.Y) };
        //    g.DrawCurve(p, points);
        //}

        private void DrawDealEvent(DateTime time, float price, string title, Graphics g, 
            RectangleD worldRect, Rectangle canvasRect,
            Font font, Pen pen, Brush foreBrush, Brush backBrush, DealEventSymbol smb)
        {
            double chartX = Chart.CandleRange.GetXCoord(time);
            var center = Conversion.WorldToScreen(new PointD(chartX, price), worldRect, canvasRect);
            var sz = g.MeasureString(title, font);
            var diam = (int)Math.Max(sz.Width, sz.Height) + 1;
            DrawDealEventShape(title, g, font, pen, foreBrush, backBrush, center, diam, smb);
        }

        private static void DrawDealEventShape(string title, Graphics g,
            Font font, Pen pen, Brush foreBrush, Brush backBrush, PointD center, int diam, DealEventSymbol symb)
        {
            if (symb == DealEventSymbol.Circle)
            {                
                // усики
                g.DrawLine(pen,
                    (int)center.X - diam * 2, (int)center.Y,
                    (int)center.X - diam / 2, (int)center.Y);
                g.DrawLine(pen,
                    (int)center.X + diam / 2, (int)center.Y,
                    (int)center.X + diam * 2, (int)center.Y);
                // кружок с заливкой
                g.FillEllipse(backBrush, (int) center.X - diam/2, (int) center.Y - diam/2, diam, diam);
                g.DrawEllipse(pen, (int) center.X - diam/2, (int) center.Y - diam/2, diam, diam);
            }
            if (symb == DealEventSymbol.Diamond)
            {
                var points = new[] 
                { 
                    new Point((int)center.X - diam / 2, (int)center.Y),
                    new Point((int)center.X, (int)center.Y - diam / 2),
                    new Point((int)center.X + diam / 2, (int)center.Y),
                    new Point((int)center.X, (int)center.Y + diam / 2)
                };
                g.FillPolygon(backBrush, points);
                g.DrawPolygon(pen, points);
            }

            if (symb == DealEventSymbol.TagUp)
            {
                diam -= 2;
                var points = new[] 
                { 
                    new Point((int)center.X + diam / 2, (int)center.Y  - diam / 2),
                    new Point((int)center.X + diam / 2, (int)center.Y  + diam / 2),
                    new Point((int)center.X - diam / 2, (int)center.Y  + diam / 2),
                    new Point((int)center.X - diam / 2, (int)center.Y  - diam / 2),
                    new Point((int)center.X, (int)center.Y  - (int)(diam / 1.25))
                };
                g.FillPolygon(backBrush, points);
                g.DrawPolygon(pen, points);
            }

            if (symb == DealEventSymbol.TagDown)
            {
                diam -= 2;
                var points = new[] 
                { 
                    new Point((int)center.X - diam / 2, (int)center.Y  + diam / 2),
                    new Point((int)center.X - diam / 2, (int)center.Y  - diam / 2),
                    new Point((int)center.X + diam / 2, (int)center.Y  - diam / 2),
                    new Point((int)center.X + diam / 2, (int)center.Y  + diam / 2),
                    new Point((int)center.X, (int)center.Y  + (int)(diam / 1.25))
                };
                g.FillPolygon(backBrush, points);
                g.DrawPolygon(pen, points);
            }

            g.DrawString(title, font, foreBrush, (float)center.X, (float)center.Y,
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        public List<DealEvent> GetDealByCoordOnPane(PointD worldCoord, RectangleD worldRect, Rectangle canvasRect)
        {
            var events = new List<DealEvent>();
            foreach (var deal in deals)
            {
                if (ShowSymbolEnter)
                if (CursorHitsDealEvent(worldCoord, worldRect, canvasRect, deal.TimeEnter, 
                    deal.PriceEnter))                
                    events.Add(new DealEvent(deal, "вход"));
                if (!deal.IsOpened && ShowSymbolExit)
                {
                    if (CursorHitsDealEvent(worldCoord, worldRect, canvasRect, deal.TimeExit ?? DateTime.MaxValue, 
                        deal.PriceExit.Value))
                        events.Add(new DealEvent(deal, "выход"));
                }

                /*
                if (deal is BacktestPosition)
                {
                    var backPos = (BacktestPosition) deal;
                    if (backPos.TimeProtected.HasValue && ShowSymbolProtect)
                    {
                        if (CursorHitsDealEvent(worldCoord, worldRect, canvasRect, backPos.TimeProtected.Value,
                                                backPos.PriceProtected.Value))
                            events.Add(new DealEvent(deal, "защита"));
                    }
                    if (backPos.TimeTakenprofit.HasValue && ShowSymbolTP)
                    {
                        if (CursorHitsDealEvent(worldCoord, worldRect, canvasRect, backPos.TimeTakenprofit.Value,
                                                backPos.PriceTakenprofit.Value))
                            events.Add(new DealEvent(deal, "TP"));
                    }
                }*/
            }
            return events;
        }
        private bool CursorHitsDealEvent(PointD worldCoord, RectangleD worldRect,
            Rectangle canvasRect, DateTime time, float price)
        {
            var chartX = Chart.CandleRange.GetXCoord(time);
            var center = Conversion.WorldToScreen(new PointD(chartX, (double) price), worldRect, canvasRect);
            return (Math.Abs(worldCoord.X - center.X) < ItemHitRange &&
                    Math.Abs(worldCoord.Y - center.Y) < ItemHitRange);
        }
    
        private bool IsDealFiltered(MarketOrder vd)
        {
            if (!vd.IsOpened)
            {
                if (dealResultFilter == DealResultFilter.PlusOnly)
                {
                    if (Math.Sign(vd.PriceExit.Value - vd.PriceEnter) != vd.Side)
                        return true;
                }
                if (dealResultFilter == DealResultFilter.MinusOnly)
                {
                    if (Math.Sign(vd.PriceExit.Value - vd.PriceEnter) == vd.Side)
                        return true;
                }
            }
            return false;
        }
    }
    /// <summary>
    /// класс символов для
    /// отрисовки событий сделки на свечном графике
    /// </summary>
    static class DealEventSymbol
    {        
        public enum DealEventSymbolType
        {
            Open, Close, OpenLocking, Unlock, ClosePair       
        }
        public static void Draw(DealEventSymbolType evType, DealType side,
            double chartX, double chartY, RectangleD worldRect, Rectangle canvasRect,
            Graphics g, Pen pen, Brush foreBrush, Brush backBrush)
        {
            var center = Conversion.WorldToScreen(new 
                PointD(chartX, chartY), worldRect, canvasRect);
            Draw(evType, side, (int) center.X, (int) center.Y, g, pen, foreBrush, backBrush);
        }

        public static void Draw(DealEventSymbolType evType, DealType side,
            int x, int y,
            Graphics g, Pen pen, Brush foreBrush, Brush backBrush)
        {
            g.DrawLine(pen, x - 5, y, x + 5, y);
            switch (evType)
            {
                case DealEventSymbolType.Open:
                    DrawEnterSign(side, x, y, g, pen, foreBrush);
                    break;
                case DealEventSymbolType.Close:
                    DrawClose(x, y, g, pen);
                    break;
                case DealEventSymbolType.OpenLocking:
                    DrawEnterLock(side, x, y, g, pen, foreBrush);
                    break;
                case DealEventSymbolType.Unlock:
                    DrawUnlock(x, y, g, pen);
                    break;
            }
        }

        private static void DrawEnterSign(DealType side,
            int x, int y,
            Graphics g, Pen pen, Brush foreBrush)
        {
            if (side == DealType.Buy)
            {
                var points = new[] 
                { 
                    new Point(x, y),
                    new Point(x - 4, y + 6),
                    new Point(x + 4, y + 6)
                };
                g.FillPolygon(foreBrush, points);
                g.DrawPolygon(pen, points);
            }
            else
            {
                var points = new[] 
                { 
                    new Point(x, y),
                    new Point(x - 4, y - 6),
                    new Point(x + 4, y - 6)
                };
                g.FillPolygon(foreBrush, points);
                g.DrawPolygon(pen, points);
            }
        }
        
        private static void DrawEnterLock(DealType side,
            int x, int y,
            Graphics g, Pen pen, Brush foreBrush)
        {
            DrawEnterSign(side, x, y, g, pen, foreBrush);
            if (side == DealType.Buy)            
                g.DrawArc(pen, x - 4, y + 4, 8, 8, 0, 180);
            else
                g.DrawArc(pen, x - 4, y - 4 - 8, 8, 8, 180, 180);
        }    
    
        private static void DrawClose(int x, int y,
            Graphics g, Pen pen)
        {
            g.DrawEllipse(pen, x - 5, y + 1, 10, 10);
            g.DrawLine(pen, x - 3, y + 3, x + 3, y + 9);
            g.DrawLine(pen, x + 3, y + 3, x - 3, y + 9);
        }
    
        private static void DrawUnlock(int x, int y,
            Graphics g, Pen pen)
        {
            g.DrawArc(pen, x - 5, y + 1, 10, 10, 180, 180);
            g.DrawLine(pen, x - 5, y + 6, x + 5, y + 14);
            g.DrawLine(pen, x + 5, y + 6, x - 5, y + 14);
        }
    }
    /// <summary>
    /// Описание события, отмеченного на графике
    /// используется при формировании всплывающей подсказки
    /// </summary>
    public class DealEvent
    {
        public MarketOrder Deal { get; set; }
        public string EventName { get; set; }
        public DealEvent(MarketOrder deal, string eventName)
        {
            Deal = deal;
            EventName = eventName;
        }
    }    
}
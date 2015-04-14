using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml;
using Candlechart.Chart;
using Candlechart.ChartMath;
using Candlechart.Core;
using Candlechart.Interface;
using System.Linq;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Series
{
    [LocalizedSeriesToolButton(CandleChartControl.ChartTool.TurnBar, "TitleFibonacciBars", ToolButtonImageIndex.TurnBar)]
    public class TurnBarSeries : InteractiveObjectSeries
    {
        [PropertyXMLTag("ShowAckDegrees", "false")]
        [DisplayName("Показ степени подтвержд.")]
        [Category("Визуальные")]
        public static bool ShowAckDegrees { get; set; }

        /// <summary>
        /// не складывать "степени подтверждения"
        /// </summary>
        public bool DontSumDegree { get; set; }
        
        public int[] fibonacciSeries = new[] { 5, 8, 13, 21 };

        public int fibonacciTurnBarFilter = 2;

        private bool hideOldBars = true;
        public bool HideOldBars
        {
            get { return hideOldBars; }
            set { hideOldBars = value; }
        }

        /// <summary>
        /// очков за точной попадание, на соседний бар, на бар через свечу и т.д.
        /// </summary>
        public int[] fibonacciMarks = new[] { 2, 1 };

        public List<TurnBar> barsKey = new List<TurnBar>();
        public override int DataCount { get { return barsKey.Count; } }
        public List<TurnBar> barsTurn = new List<TurnBar>();


        public TurnBarSeries(string name)
            : base(name, CandleChartControl.ChartTool.TurnBar)
        {
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

            if ((Chart.CandleRange.End - Chart.CandleRange.Start).TotalMinutes == 0) return;
            if ((Chart.CandleRange.EndIndex - Chart.CandleRange.StartIndex) <= 0) return;

            using (var font = new Font(Chart.Font.FontFamily, 7.0F))
            {
                using (var pen = new Pen(Chart.ForeColor, LineWidth) { Alignment = PenAlignment.Center })
                {                    
                    using (var brushWhite = new SolidBrush(Color.White))
                    {
                        using (var brushRed = new SolidBrush(Color.Red))
                        {
                            using (var brushFont = new SolidBrush(Chart.ForeColor))
                            {
                                var allBars = barsKey.Union(barsTurn);
                                foreach (var d in allBars)
                                {
                                    DrawBar(d, g, font, pen, brushFont, brushWhite, brushRed, worldRect, canvasRect);
                                }
                            }
                        }
                    }                    
                }
            }
        }                
        private const int SignalMarkerR = 12;
        /// <summary>
        /// Отрисовать скобки вокруг разворотных баров
        /// </summary>        
        private void DrawBar(TurnBar b, Graphics g, Font font, Pen pen, Brush brushFont,
            Brush brushWhite, Brush brushRed, RectangleD worldRect, Rectangle canvasRect)
        {
            float maxPrice = float.MinValue, minPrice = float.MaxValue;
            DateTime minTime = DateTime.MaxValue, maxTime = DateTime.MinValue;
            var candles = Owner.StockSeries.Data.Candles;
            foreach (var bar in b.candleIndexes)
            {
                var candle = candles[bar];
                if (candle.open > maxPrice) maxPrice = candle.open;
                if (candle.close > maxPrice) maxPrice = candle.close;
                if (candle.open < minPrice) minPrice = candle.open;
                if (candle.close < minPrice) minPrice = candle.close;
                if (candle.timeOpen > maxTime) maxTime = candle.timeOpen;
                if (candle.timeOpen < minTime) minTime = candle.timeOpen;
            }
            double left = Chart.CandleRange.GetXCoord(minTime) - 0.5;
            double right = Chart.CandleRange.GetXCoord(maxTime) + 0.5;
            Point leftBotm =
                PointD.Round(Conversion.WorldToScreen(new PointD(left, (double) minPrice), worldRect, canvasRect));            
            Point rightTop =
                PointD.Round(Conversion.WorldToScreen(new PointD(right, (double)maxPrice), worldRect, canvasRect));
            leftBotm.Y += 3;
            rightTop.Y -= 3;            
            // bounds            
            if (b.candleIndexes.Count > 1)
            {
                g.DrawLine(pen, leftBotm.X, leftBotm.Y, rightTop.X, leftBotm.Y);
                if ((rightTop.X - leftBotm.X) > 10)
                {
                    g.DrawLine(pen, leftBotm.X, leftBotm.Y, leftBotm.X, leftBotm.Y - 4);
                    g.DrawLine(pen, rightTop.X, leftBotm.Y, rightTop.X, leftBotm.Y - 4);
                }
            }
            // marker
            if (!b.IsKeyBar)
            {
                var s = ShowAckDegrees ? b.AckDegree.ToString() : "!";
                var markerSz = g.MeasureString(s, font);
                g.FillRectangle(brushWhite, leftBotm.X - markerSz.Width, leftBotm.Y, markerSz.Width, markerSz.Height);
                g.DrawRectangle(pen, leftBotm.X - markerSz.Width, leftBotm.Y, markerSz.Width, markerSz.Height);
                g.DrawString(s, font, brushFont, leftBotm.X - markerSz.Width, leftBotm.Y);
            }
            else
            {
                const int markerX = (int)(SignalMarkerR * 0.7071);
                g.FillEllipse(brushRed, leftBotm.X - markerX, leftBotm.Y, markerX, markerX);
                g.DrawEllipse(pen, leftBotm.X - markerX, leftBotm.Y, markerX, markerX);
            }
        }

        public void CountTurnBars()
        {
            barsTurn.Clear();
            if (DontSumDegree)
            {
                CountTurnBarsPlain();
                return;
            }

            // массив попаданий
            var fibonacci = fibonacciSeries;
            var ackDegArray = new int[Owner.StockSeries.Data.Count + fibonacci[fibonacci.Length - 1]];
            
            foreach (var keyBar in barsKey)
            {
                var start = keyBar.candleIndexes[0];
                for (var i = 0; i < fibonacci.Length; i++)
                {
                    // ключевой бар имеет индекс [1]
                    var index = start + fibonacci[i] - 1; 
                    if (index >= ackDegArray.Length) break;
                    
                    // точное попадание - +N (2) очка
                    ackDegArray[index] += fibonacciMarks[0];
                    for (var j = 1; j < fibonacciMarks.Length; j++)
                    {
                        if ((index - j) >= 0)
                            ackDegArray[index - j] += fibonacciMarks[j];                        
                        if (index + j < Owner.StockSeries.Data.Count)
                            ackDegArray[index + j] += fibonacciMarks[j];
                    }
                }
            }
            // подсчитав попадания, сформировать массив баров подтверждения
            MakeTurnBars(ackDegArray);
            // отфильтровать бары разворота
            barsTurn = barsTurn.FindAll(b => b.AckDegree >= fibonacciTurnBarFilter || b.IsKeyBar);
        }

        private void CountTurnBarsPlain()
        {
            var candlesCount = Owner.StockSeries.Data.Count;

            for (var j = 0; j < barsKey.Count; j++)
            {
                var nextKeyIndex = -1;
                if (HideOldBars) nextKeyIndex = j == barsKey.Count - 1 ? -1 : 
                    barsKey[j + 1].candleIndexes[0] + fibonacciSeries[0];
                var start = barsKey[j].candleIndexes[0];
                for (var i = 0; i < fibonacciSeries.Length; i++)
                {
                    var candleIndex = start + fibonacciSeries[i] - 1;
                    if (candleIndex >= candlesCount) break;
                    if (nextKeyIndex >= 0 && candleIndex >= nextKeyIndex) break;
                    barsTurn.Add(new TurnBar(new List<int> { candleIndex }, false, Chart) { AckDegree = fibonacciSeries[i] });
                }
            }
        }

        private void MakeTurnBars(int[] ackDegArray)
        {
            TurnBar lastTurnBar = null;
            for (var i = 0; i < ackDegArray.Length; i++)
            {
                if (i >= Owner.StockSeries.Data.Count) break;
                var degree = ackDegArray[i];
                var nextDegree = (ackDegArray.Length - i > 1) ? ackDegArray[i + 1] : 0;
                var prevDegree = (i > 0) ? ackDegArray[i - 1] : 0;

                if (degree < prevDegree)
                {// заканчиваем крайний бар
                    if (lastTurnBar != null)
                        barsTurn.Add(lastTurnBar);
                    lastTurnBar = null;
                    continue;
                }
                if (degree < fibonacciTurnBarFilter)
                    continue;
                // если у соседнего бара степень подтверждения выше - берем соседний бар
                if (nextDegree > degree)
                {
                    lastTurnBar = null;
                    continue;
                }
                // если степень подтверждения равна предыдущему, бар еще не закончен
                if (prevDegree == degree && lastTurnBar != null)
                {
                    lastTurnBar.candleIndexes.Add(i);
                    continue;
                }
                // если степень подтверждения больше или равна предыдущей
                if (prevDegree <= degree)
                {
                    lastTurnBar = new TurnBar(new List<int> { i }, false, Chart) 
                        { AckDegree = (degree - 1) };
                    continue;
                }
            }
            if (lastTurnBar != null)
                barsTurn.Add(lastTurnBar);
            // проставить разворотным барам время закрытия
            foreach (var bar in barsTurn.Where(b => !b.IsKeyBar))
                bar.CloseTime = Owner.StockSeries.Data.Candles[
                    bar.candleIndexes[bar.candleIndexes.Count - 1]].timeClose;
        }

        public override void OnTimeframeChanged()
        {
            var newCandlesCount = Owner.StockSeries.Data.Count;
            // убрать ключевые бары, если их индексы вылезли за пределы диапазона свечек
            if (barsKey.RemoveAll(b => b.candleIndexes[b.candleIndexes.Count - 1] >= newCandlesCount) > 0)            
                CountTurnBars();
        }

        #region InteractiveObjectSeries

        protected override void OnMouseDown(List<SeriesEditParameter> parameters,
            MouseEventArgs e, Keys modifierKeys, out IChartInteractiveObject objectToEdit)
        {
            objectToEdit = null;
            if (e.Button != MouseButtons.Left) return;
            // поставить или снять ключевой бар
            
            // получить время (свечку)
            var clientPoint = Chart.PointToScreen(new Point(e.X, e.Y));
            clientPoint = Chart.StockPane.PointToClient(clientPoint);
            var pointD = Conversion.ScreenToWorld(new PointD(clientPoint.X, clientPoint.Y),
               Owner.WorldRect, Owner.CanvasRect);
            var index = (int)(pointD.X + 0.5);
            if (index >= Owner.StockSeries.Data.Count || index < 0) return;

            // найти ключевой бар на свечке
            var keyBar = barsKey.FirstOrDefault(b => b.candleIndexes.Contains(index));
            if (keyBar != null)
            {
                // удалить ключевой бар
                barsKey.Remove(keyBar);
                CountTurnBars();
                return;
            }
            // добавить новый бар
            var bar = new TurnBar(new List<int> {index}, true, Chart);            
            barsKey.Add(bar);
            if (Owner.Owner.Owner.AdjustObjectColorsOnCreation)
                bar.AjustColorScheme(Owner.Owner.Owner);
            CountTurnBars();
        }

        public override void AddObjectsInList(List<IChartInteractiveObject> interObjects)
        {
            interObjects.AddRange(barsKey.Where(b => b.IsKeyBar));
        }

        public override void RemoveObjectFromList(IChartInteractiveObject interObject)
        {
            var bar = interObject as TurnBar;
            if (bar == null) return;
            if (!bar.IsKeyBar) return;
            barsKey.Remove(bar);
            CountTurnBars();
        }

        public override void RemoveObjectByNum(int num)
        {
            barsKey.RemoveAt(num);
        }

        public override IChartInteractiveObject GetObjectByNum(int num)
        {
            return barsKey[num];
        }

        public override IChartInteractiveObject LoadObject(XmlElement objectNode, CandleChartControl owner, bool trimObjectsOutOfHistory = false)
        {
            var obj = new TurnBar(new List<int>(), true, Chart);
            obj.LoadFromXML(objectNode, owner);
            barsKey.Add(obj);
            return obj;
        }

        public override List<IChartInteractiveObject> GetObjectsUnderCursor(int screenX, int screenY, int tolerance)
        {
            // найти свечу
            var ptClient = Owner.PointToClient(new Point(screenX, screenY));
            var pointD = Conversion.ScreenToWorld(new PointD(ptClient.X, ptClient.Y),
               Owner.WorldRect, Owner.CanvasRect);
            var index = (int)(pointD.X + 0.5);
            if (index >= Owner.StockSeries.Data.Count) index = -1;
            var list = new List<IChartInteractiveObject>();
            if (index < 0) return list;

            if (index >= 0)
                list.AddRange(barsKey.Where(keyBar => keyBar.candleIndexes[0] == index));
            return list;
        }

        public override void ProcessLoadingCompleted(CandleChartControl owner)
        {
            // построить по ключевым барам разворотные бары
            CountTurnBars();
        }

        public override void AdjustColorScheme(CandleChartControl chart)
        {
            foreach (var obj in barsKey)
                obj.AjustColorScheme(chart);
        }

        public override IChartInteractiveObject FindObject(Func<IChartInteractiveObject, bool> predicate, out int objIndex)
        {
            objIndex = -1;
            for (var i = 0; i < barsKey.Count; i++)
            {
                if (predicate(barsKey[i]))
                {
                    objIndex = i;
                    return barsKey[i];
                }
            }
            return null;
        }
        #endregion
    }        
    
    public class TurnBar : IChartInteractiveObject
    {
        [Browsable(false)]
        public InteractiveObjectSeries Owner { get; set; }

        private ChartControl owner;
        
        [Browsable(false)]
        public string ClassName { get { return Localizer.GetString("TitleTurnBar"); } }

        /// <summary>
        /// для автоматического именования объектов
        /// </summary>
        private static int nextObjectNumber = 1;

        public List<int> candleIndexes
        {
            get; set;
        }
        /// <summary>
        /// время закрытия последней свечки
        /// </summary>
        public DateTime CloseTime { get; set; }
        public int AckDegree { get; set; }
        public bool IsKeyBar { get; set; }

        [DisplayName("Magic")]
        [LocalizedCategory("TitleMain")]
        [PropertyOrder(1, 1)]
        public int Magic { get; set; }

        public TurnBar(List<int> candles, bool isKeyBar, ChartControl owner)
        {
            this.owner = owner;
            candleIndexes = candles;
            IsKeyBar = isKeyBar;
            Name = string.Format("{0} {1}", ClassName, nextObjectNumber++);
        }

        public TurnBar(TurnBar bar)
        {
            owner = bar.owner;
            IsKeyBar = bar.IsKeyBar;
            Name = bar.Name;
            candleIndexes = new List<int>();
            candleIndexes.AddRange(bar.candleIndexes);
        }
        
        #region Implementation of IChartInteractiveObject

        public bool Selected { get; set; }

        [LocalizedDisplayName("TitleName")]
        [LocalizedCategory("TitleMain")]
        public string Name { get; set; }

        public DateTime? DateStart
        {
            get
            {
                if (candleIndexes.Count == 0) return null;
                var candles = owner.StockSeries.Data.Candles;
                return candleIndexes[0] < 0 || candleIndexes[0] >= candles.Count
                           ? (DateTime?)null : candles[candleIndexes[0]].timeOpen;
            }
            set { }
        }

        public int IndexStart
        {
            get { return -1; }
        }

        public void SaveInXML(XmlElement parentNode, CandleChartControl owner)
        {
            var node = parentNode.AppendChild(parentNode.OwnerDocument.CreateElement("TurnBar"));
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("magic")).Value = Magic.ToString();
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("KeyBar")).Value = IsKeyBar.ToString();
            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("Name")).Value = Name;
            foreach (var candle in candleIndexes)
            {
                node.AppendChild(parentNode.OwnerDocument.CreateElement("index")).Attributes.Append(
                    parentNode.OwnerDocument.CreateAttribute("value")).Value = candle.ToString();
            }
        }

        public void LoadFromXML(XmlElement itemNode, CandleChartControl owner)
        {
            IsKeyBar = bool.Parse(itemNode.Attributes["KeyBar"].Value);
            Name = itemNode.Attributes["Name"].Value;
            if (itemNode.Attributes["magic"] != null) Magic = itemNode.Attributes["magic"].Value.ToIntSafe() ?? 0;
            if (itemNode.Attributes["magic"] != null) Magic = itemNode.Attributes["magic"].Value.ToIntSafe() ?? 0;
            foreach (XmlElement node in itemNode.ChildNodes)
            {
                candleIndexes.Add(node.Attributes["value"].Value.ToInt());
            }            
        }

        public ChartObjectMarker IsInMarker(int screenX, int screenY, Keys modifierKeys)
        {
            return null;
        }

        public void OnMarkerMoved(ChartObjectMarker marker)
        {
            throw new NotImplementedException();
        }

        public void AjustColorScheme(CandleChartControl chart)
        {            
        }

        public Image CreateSample(Size sizeHint)
        {
            return null;
        }

        #endregion
    }
}

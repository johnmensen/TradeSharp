using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Entity;
using FastGrid;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace Candlechart.ChartIcon
{
    public partial class SummaryPositionDropWindow : UserControl
    {
        private readonly CandleChartControl chart;

        public Action closeControl;

        public bool isPinup;

        private ImageList imageListBound;

        private const int ImgBoundSize = 16;
        
        private const int BoundSelectedMagic = 50;

        public SummaryPositionDropWindow()
        {
            InitializeComponent();
        }

        public SummaryPositionDropWindow(CandleChartControl chart) : this()
        {
            this.chart = chart;
            SetupGrid();
        }

        private void SetupGrid()
        {
            var fontBold = new Font(Font, FontStyle.Bold);
            // создать список картинок для "связанных ордеров"
            MakeBoundImages();

            gridDeals.Columns.Add(new FastColumn("Magic", "*")
            {
                ImageList = imageListBound,
                ColumnWidth = 24,
                IsHyperlinkStyleColumn = true,
                HyperlinkActiveCursor = Cursors.Hand,
                SortOrder = FastColumnSort.Descending
            });
            gridDeals.Columns.Add(new FastColumn("Side", "Тип")
            {
                /*ColumnFont = fontBold,*/
                ColumnWidth = 40,
                formatter = value => (int) value == -1 ? "SELL" : "BUY",
                IsHyperlinkStyleColumn = true,
                HyperlinkActiveCursor = Cursors.Hand,
                ColorHyperlinkTextActive = Color.DodgerBlue,
                HyperlinkFontActive = fontBold
            });
            gridDeals.Columns.Add(new FastColumn("Volume", "Объем")
            {
                ColumnWidth = 74,
                formatter = value => ((int)value).ToStringUniformMoneyFormat()
            });
            gridDeals.Columns.Add(new FastColumn("PriceEnter", "Вход")
            {
                ColumnWidth = 66,
                formatter = value => ((float)value).ToStringUniformPriceFormat()
            });
            gridDeals.Columns.Add(new FastColumn("ResultDepo", "Прибыль")
            {
                ColumnWidth = 73,
                formatter = value => ((float)value).ToStringUniformMoneyFormat(),
                SortOrder = FastColumnSort.Ascending,
                colorColumnFormatter = (object value, out Color? color, out Color? fontColor) =>
                    {
                        color = null;
                        fontColor = null;
                        if ((float) value < 0) fontColor = Color.DarkRed;
                    }
            });
            gridDeals.Columns.Add(new FastColumn("ResultPoints", "Пункты")
            {
                ColumnWidth = 68,
                formatter = value => ((float)value).ToStringUniformMoneyFormat(),
                colorColumnFormatter = (object value, out Color? color, out Color? fontColor) =>
                {
                    color = null;
                    fontColor = null;
                    if ((float)value < 0) fontColor = Color.DarkRed;
                }
            });
            gridDeals.Columns.Add(new FastColumn("StopLoss", "SL")
            {
                ColumnWidth = 66,
                formatter = value => value == null ? "" : ((float)value).ToStringUniformPriceFormat()
            });
            gridDeals.Columns.Add(new FastColumn("TakeProfit", "TP")
            {
                ColumnWidth = 66,
                formatter = value => value == null ? "" : ((float)value).ToStringUniformPriceFormat()
            });

            gridDeals.ColorAnchorCellBackground = Color.FromArgb(220, 255, 200);
            gridDeals.StickLast = true;
            gridDeals.CalcSetTableMinWidth();
            gridDeals.UserHitCell += GridDealsOnUserHitCell;
        }

        private void MakeBoundImages()
        {
            imageListBound = new ImageList
            {
                ImageSize = new Size(ImgBoundSize, ImgBoundSize),
                ColorDepth = ColorDepth.Depth32Bit
            };

            var pointsFigure = new PointF[10];
            const float o = ImgBoundSize / 2f;
            for (var i = 0; i < 5; i++)
            {
                var angle = 2 * i * Math.PI / 5;
                pointsFigure[i * 2] = new PointF(o + (float)Math.Sin(angle) * o, o - (float)Math.Cos(angle) * o);
                angle += 2 * Math.PI / 5 / 2;
                pointsFigure[i * 2 + 1] = new PointF(o + (float)Math.Sin(angle) * o / 2, o - (float)Math.Cos(angle) * o / 2);
            }

            using (var font = new Font("Courier", (int)Math.Round(ImgBoundSize * 0.8), 
                FontStyle.Regular, GraphicsUnit.Pixel))
            for (var i = 1; i <= BoundSelectedMagic; i++)
            {
                var bmp = new Bitmap(imageListBound.ImageSize.Width, imageListBound.ImageSize.Height);
                using (var gr = Graphics.FromImage(bmp))
                using (var pen = new Pen(Color.FromArgb(168, 168, 140)))
                using (var brushPoly = new SolidBrush(Color.FromArgb(198, 198, 178)))
                using (var brushFont = new SolidBrush(Color.Black))
                {
                    gr.SmoothingMode = SmoothingMode.AntiAlias;
                    gr.FillPolygon(brushPoly, pointsFigure);
                    gr.DrawPolygon(pen, pointsFigure);
                    // в центре поместить текст - номер
                    // ReSharper disable SpecifyACultureInStringConversionExplicitly
                    gr.DrawString(i.ToString(), font, brushFont, o, o, new StringFormat
                    // ReSharper restore SpecifyACultureInStringConversionExplicitly
                        {
                            Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center
                        });
                }
                imageListBound.Images.Add(bmp);
            }

            // звездочка для выделяемого элемента
            var bmpSel = new Bitmap(imageListBound.ImageSize.Width, imageListBound.ImageSize.Height);
            using (var gr = Graphics.FromImage(bmpSel))
            using (var pen = new Pen(Color.FromArgb(255, 100, 100)))
            using (var brushPoly = new SolidBrush(Color.FromArgb(255, 180, 180)))
            {
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                gr.FillPolygon(brushPoly, pointsFigure);
                gr.DrawPolygon(pen, pointsFigure);
            }
            imageListBound.Images.Add(bmpSel);
        }

        private void GridDealsOnUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            var order = (MarketOrder) gridDeals.rows[rowIndex].ValueObject;
            
            if (e.Button == MouseButtons.Left)
            {
                if (col.PropertyName == "Side")
                {
                    // редактировать ордер
                    if (order.ID == 0) return;
                    chart.CallShowWindowEditMarketOrder(new MarketOrder { ID = order.ID });
                }
                
                if (col.PropertyName == "Magic")
                {
                    // если нажать Ctrl и кликнули в связанную группу ордеров -
                    // открыть диалог связывания
                    if ((ModifierKeys & Keys.Control) == Keys.Control)
                    {
                        if (order.Magic >= 0)
                        {
                            var orderIds = gridDeals.GetRowValues<MarketOrder>(false)
                                 .Where(o => o.Magic == order.Magic)
                                 .Select(o => o.ID)
                                 .ToList();

                            chart.CallEditMarketOrders(orderIds);
                            return;
                        }
                    }

                    // пометить ордер
                    if (order.ID == 0 || gridDeals.rows.Count < 2) return;
                    var isSelected = order.Magic == BoundSelectedMagic;
                    order.Magic = !isSelected ? BoundSelectedMagic : order.PendingOrderID;
                    gridDeals.UpdateRow(rowIndex, order);
                    gridDeals.InvalidateRow(rowIndex);
                    SetLinkButtonEnabled();
                }
            }
        }

        private void BtnCloseClick(object sender, EventArgs e)
        {
            // запинить
            if (!isPinup)
            {
                isPinup = true;
                btnClose.ImageIndex = 0;
                return;
            }

            // закрыть
            isPinup = false;
            if (closeControl != null)
                closeControl();
        }
    
        private void UpdateOrdersSafe()
        {
            Invoke(new Action(UpdateOrders));
        }

        private void UpdateOrders()
        {
            var marketOrders = chart.receiveMarketOrders().Where(o => o.Symbol == chart.Symbol).ToList();
            if (marketOrders.Count == 0)
            {
                 if (gridDeals.rows.Count != 0)
                 {
                     gridDeals.rows.Clear();
                     gridDeals.Invalidate();
                 }
                return;
            }

            // посчитать пункты
            var curPrice = chart.chart.StockPane.YAxis.CurrentPrice;
            marketOrders.ForEach(o => o.ResultPoints = curPrice.HasValue
                ? (int) Math.Round(DalSpot.Instance.GetPointsValue(
                    chart.Symbol,o.Side * (curPrice.Value - o.PriceEnter))) : 0);

            // посчитать суммарный ордер
            if (marketOrders.Count > 1)
            {
                var avgOrder = DalSpot.Instance.CalculateSummaryOrder(marketOrders, null, curPrice);
                if (avgOrder != null)
                    marketOrders.Add(avgOrder);
            }

            // по всем ордерам, кроме суммарного, найти "связанные" ордера
            // "Magic" используется как номер группы связанных ордеров
            SetOrdersGroupMagics(marketOrders);

            // прибайндить
            gridDeals.DataBind(marketOrders);
            SetLinkButtonEnabled();
        }

        private void SetOrdersGroupMagics(List<MarketOrder> marketOrders)
        {
            marketOrders.ForEach(o =>
                {
                    o.Magic = -1;
                    o.PendingOrderID = -1;
                });
            var nextGroup = 0;
            for (var i = 0; i < marketOrders.Count; i++)
            {
                if (marketOrders[i].ID == 0) continue;
                
                var orderA = marketOrders[i];
                if (orderA.Magic >= 0) continue;
                for (var j = i + 1; j < marketOrders.Count; j++)
                {
                    if (marketOrders[j].ID == 0) continue;
                    var orderB = marketOrders[j];

                    if ((orderA.StopLoss.HasValue && orderA.StopLoss == orderB.StopLoss) ||
                        (orderA.TakeProfit.HasValue && orderA.TakeProfit == orderB.TakeProfit) ||
                        (orderA.TakeProfit.HasValue && orderA.TakeProfit == orderB.StopLoss) ||
                        (orderA.StopLoss.HasValue && orderA.StopLoss == orderB.TakeProfit))
                    {
                        orderA.Magic = nextGroup;
                        orderA.PendingOrderID = nextGroup;
                        orderB.Magic = nextGroup;
                        orderB.PendingOrderID = nextGroup;
                    }
                }
                if (orderA.Magic >= 0) nextGroup++;
            }
        }

        private void SummaryPositionDropWindowLoad(object sender, EventArgs e)
        {
            UpdateOrdersSafe();
        }
    
        private void SetLinkButtonEnabled()
        {
            btnLinkOrders.Enabled =
                gridDeals.GetRowValues<MarketOrder>(false).Count(o => o.Magic == BoundSelectedMagic) > 1;
        }

        private void BtnLinkOrdersClick(object sender, EventArgs e)
        {
            var orderIds = gridDeals.GetRowValues<MarketOrder>(false)
                     .Where(o => o.Magic == BoundSelectedMagic)
                     .Select(o => o.ID)
                     .ToList();

            chart.CallEditMarketOrders(orderIds);
        }

        private void BtnCloseAllClick(object sender, EventArgs e)
        {
            var allOrders = gridDeals.GetRowValues<MarketOrder>(false).Where(r => r.ID > 0).ToList();

            if (allOrders.Any(o => o.Magic == BoundSelectedMagic))
                allOrders = allOrders.Where(o => o.Magic == BoundSelectedMagic).ToList();
            if (allOrders.Count == 0) return;

            // закрыть все ордера
            chart.CallCloseMarketOrders(allOrders.Select(o => o.ID).ToList());
        }
    }
}

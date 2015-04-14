using System.Windows.Forms;
using TradeSharp.Client.BL;

namespace TradeSharp.Client.Controls
{
    public class QuoteCellControl : Control
    {
        private QuoteCellShape cellShape;

        private int cellSize;
        public int CellSize
        {
            get { return cellSize; }
            set
            {
                if (cellSize == value) return;
                cellSize = value;

                Width = cellShape.Width;
                Height = cellShape.Height;
            }
        }

        private readonly int precision;
        public int Precision { get { return precision; } }

        public float Bid { get; set; }
        public float Ask { get; set; }

        public QuoteCellControl(QuoteTableCellSettings sets, int cellSize)
        {
            CellSize = cellSize;
            Text = sets.Ticker;
            precision = sets.Precision;
            Left = sets.X;
            Top = sets.Y;
            Cursor = Cursors.Hand;
        }

        public void SetPrice(float bid, float ask)
        {
            Bid = bid;
            Ask = ask;
            cellShape.DetermineSentimentColorIndex(bid);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            cellShape.Draw(e, Text, precision, Bid, Ask);
        }
    }   
}

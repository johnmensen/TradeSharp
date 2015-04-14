using Entity;

namespace TradeSharp.TradeSignal.BL
{
    public struct SignalStorageKey
    {
        public int categoryId;

        public string ticker;

        public BarSettings timeframe;

        public override string ToString()
        {
            return string.Format("{0}_{1}_{2}", categoryId, ticker, timeframe);
        }
    }
}

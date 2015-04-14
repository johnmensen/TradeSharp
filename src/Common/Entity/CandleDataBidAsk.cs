using TradeSharp.Contract.Entity;

namespace Entity
{
    public class CandleDataBidAsk : CandleData
    {
        public float openAsk, highAsk, lowAsk, closeAsk;

        public CandleDataBidAsk()
        {
        }

        public CandleDataBidAsk(CandleData candleBid, CandleData candleAsk) : base(candleBid)
        {
            openAsk = candleAsk.open;
            highAsk = candleAsk.high;
            lowAsk = candleAsk.low;
            closeAsk = candleAsk.close;
        }

        public CandleDataBidAsk(CandleData candle, float spread) : base (candle)
        {
            openAsk = open + spread;
            highAsk = high + spread;
            lowAsk = low + spread;
            closeAsk = close + spread;
        }

        public QuoteData GetCloseQuote()
        {
            return new QuoteData(close, closeAsk, timeClose);
        }
    }
}

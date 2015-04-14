using System;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class CandleDataPacked
    {
        public DateTime timeOpen;

        public float open;

        // ReSharper disable InconsistentNaming
        public int HLC;
        // ReSharper restore InconsistentNaming

        // для PackedCandleStream.GetCandles
        public float close;

        // не согласен!
        // ReSharper disable EmptyConstructor
        public CandleDataPacked()
        // ReSharper restore EmptyConstructor
        {
        }        
    }
}

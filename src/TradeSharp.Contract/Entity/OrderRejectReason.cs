namespace TradeSharp.Contract.Entity
{
    public enum OrderRejectReason
    {
        None = 0,
        // объявлены в FIX 4.4
        UnknownOrder,
        BrokerExchangeOption,
        OrderInPendingState,
        UnableMassCancel,
        OrigOrdModTimeMissmatch,
        DuplicateClOrdID
    }
}

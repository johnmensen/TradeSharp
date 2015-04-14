namespace TradeSharp.Contract.Entity
{
    public enum PositionExitReason
    {
        Closed = 0,
        SL = 1,
        TP = 2,
        Stopout = 3,
        ClosedByRobot = 4,
        ClosedFromUI = 5,
        ClosedFromScript = 6,
        ClosedBySignal = 7
    }
}

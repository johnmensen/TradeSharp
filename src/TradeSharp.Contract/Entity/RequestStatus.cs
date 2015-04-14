namespace TradeSharp.Contract.Entity
{
    public enum RequestStatus
    {
        OK = 0,
        NoConnection,
        CommonError,
        ServerError,
        BadRequest,
        IncorrectData,
        NotFound,
        NoPrice,
        SerializationError,
        Slippage,
        WrongSide,
        WrongStopOrder,
        WrongTakeOrder,
        GroupUnsupported,
        DealerError,
        MarginOrLeverageExceeded,
        WrongPrice,
        WrongTime,
        DoubledRequest,
        Unauthorized
    }
}
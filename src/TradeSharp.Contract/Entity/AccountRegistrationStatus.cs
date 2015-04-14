namespace TradeSharp.Contract.Entity
{
    public enum AccountRegistrationStatus
    {
        OK = 0,
        IncorrectLogin,
        IncorrectEmail,
        IncorrectPassword,
        DuplicateLogin,
        DuplicateEmail,
        IncorrectBalance,
        ServerError,
        WrongCurrency,
        EmailDeliveryError
    }
}

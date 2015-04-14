namespace TradeSharp.Contract.Entity
{
    public enum CreateReadonlyUserRequestStatus
    {
        Success = 0,
        UserNotFound,
        PasswordIsIncorrect,
        NotPermitted,
        CommonError
    }
}

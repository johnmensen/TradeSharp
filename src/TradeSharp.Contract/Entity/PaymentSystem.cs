namespace TradeSharp.Contract.Entity
{
    public enum PaymentSystem : byte
    {
        Unknown = 0,
        WebMoney,
        PayPal, 
        MoneyBookers, 
        Qiwi, 
        MoneyYandex, 
        EasyPay
    }
}

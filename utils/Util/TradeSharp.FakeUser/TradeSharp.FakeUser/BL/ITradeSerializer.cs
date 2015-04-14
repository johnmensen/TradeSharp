using TradeSharp.Contract.Entity;

namespace TradeSharp.FakeUser.BL
{
    interface ITradeSerializer
    {
        string SerializeOrder(MarketOrder deal);
        string SerializeTransaction(BalanceChange trans);
    }
}

using System;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;

namespace TradeSharp.Server.Repository
{
    public interface IOrderRepository
    {
        bool CloseOrder(MarketOrder order, decimal price, PositionExitReason exitReason);

        void MakeOrderClosedSignal(int accountId, int orderId, float priceExit);

        bool UpdateAccountBalance(TradeSharpConnection ctx,
                                  ACCOUNT account, decimal amount, BalanceChangeType changeType,
                                  string description, DateTime valueDate, int? positionId);
    }
}

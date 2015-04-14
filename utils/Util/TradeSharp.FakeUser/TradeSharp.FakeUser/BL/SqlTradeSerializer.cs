using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.FakeUser.BL
{
    class SqlTradeSerializer : ITradeSerializer
    {
        public string SerializeOrder(MarketOrder deal)
        {
            if (deal.IsClosed)
                return SerializeClosedDeal(deal);
            return SerializeOpenedDeal(deal);
        }

        private string SerializeOpenedDeal(MarketOrder deal)
        {
            return string.Format(
                "insert into POSITION (State, Symbol, Side, AccountID, Volume," +
                "Comment, ExpertComment, Stoploss, Takeprofit, Magic, PriceEnter, TimeEnter) VALUES " +
                "({0}, '{1}', {2}, {3}, {4}, " +
                "'{5}', '{6}', '{7}', '{8}', {9}, '{10}', '{11}')",
                (int) deal.State,
                deal.Symbol,
                deal.Side,
                deal.AccountID,
                deal.Volume,
                deal.Comment,
                deal.ExpertComment,
                deal.StopLoss.HasValue ? deal.StopLoss.Value.ToStringUniformPriceFormat() : "",
                deal.TakeProfit.HasValue ? deal.TakeProfit.Value.ToStringUniformPriceFormat() : "",
                deal.Magic,
                deal.PriceEnter.ToStringUniformPriceFormat(),
                deal.TimeEnter.ToString("yyyyMMdd HH:mm"));
        }

        private string SerializeClosedDeal(MarketOrder deal)
        {
            return string.Format(
                "insert into POSITION_CLOSED (ExitReason, Symbol, Side, AccountID, Volume," +
                "Comment, ExpertComment, Stoploss, Takeprofit, Magic, PriceEnter, TimeEnter, " +
                "PriceExit, TimeExit, Swap, ResultPoints, ResultBase, ResultDepo) VALUES " +
                "({0}, '{1}', {2}, {3}, {4}, " +
                "'{5}', '{6}', '{7}', '{8}', {9}, '{10}', '{11}', " +
                "'{12}', '{13}', '{14}', '{15}', '{16}', '{17}')",
                (int)deal.State,
                deal.Symbol,
                deal.Side,
                deal.AccountID,
                deal.Volume,
                deal.Comment,
                deal.ExpertComment,
                deal.StopLoss.HasValue ? deal.StopLoss.Value.ToStringUniformPriceFormat(true) : "",
                deal.TakeProfit.HasValue ? deal.TakeProfit.Value.ToStringUniformPriceFormat(true) : "",
                deal.Magic,
                deal.PriceEnter.ToStringUniformPriceFormat(),
                deal.TimeEnter.ToString("yyyyMMdd HH:mm"),
                deal.PriceExit.Value.ToStringUniformPriceFormat(),
                deal.TimeExit.Value.ToString("yyyyMMdd HH:mm"),
                0,
                deal.ResultPoints.ToStringUniformMoneyFormat(),
                deal.ResultBase.ToStringUniformMoneyFormat(),
                deal.ResultDepo.ToStringUniformMoneyFormat());
        }

        public string SerializeTransaction(BalanceChange trans)
        {
            return string.Format("INSERT INTO BALANCE_CHANGE (AccountID, ChangeType, Amount, ValueDate, Description)" +
                                 "VALUES ({0}, {1}, {2}, '{3}', '{4}')",
                trans.AccountID,
                (int) trans.ChangeType,
                trans.Amount,
                trans.ValueDate.ToString("yyyyMMdd HH:mm"),
                trans.Description);
        }
    }
}

using System;
using System.Collections.Generic;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Linq;

namespace TradeSharp.Test.Moq
{
    static class PositionMaker
    {
        public static List<POSITION> MakePositions(TradeSharpConnectionPersistent conn, int accountId)
        {
            var allPositions = new List<POSITION>();

            // создать сделки по счету
            var tickers = new[] { "EURUSD", "USDJPY", "AUDUSD", "EURCAD" };
            var timeNow = DateTime.Now;
            var times = new[] { timeNow.AddDays(-2), timeNow.AddDays(-1), timeNow.AddHours(-1) };
            var sides = new[] { -1, 1 };

            foreach (var ticker in tickers)
            foreach (var side in sides)
            foreach (var time in times)
            {
                var quote = QuoteStorage.Instance.ReceiveValue(ticker);
                var pos = new POSITION
                {
                    AccountID = accountId,
                    Symbol = ticker,
                    Side = side,
                    TimeEnter = time,
                    State = (int)PositionState.Opened,
                    PriceEnter = (decimal)(side == 1 ? quote.ask : quote.bid),
                    Volume = 100000
                };
                allPositions.Add(pos);
                conn.POSITION.Add(pos);
            }

            return allPositions;
        }
    }
}

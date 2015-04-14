using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Robot.BacktestServerProxy;
using TradeSharp.Util;

namespace TradeSharp.FakeUser.BL
{
    class JsonTrackMaker
    {
        public static int IncInt(int n)
        {
            return n + 1;
        }

        public static void BuildTrack(int accountId, string targetFilePath, int intervalMinutes)
        {
            var logger = new LimitedLogger();

            var positionsOpened = new List<MarketOrder>();
            var positionClosed = new List<MarketOrder>();
            List<BalanceChange> balances;
            string accountCurrency;

            using (var db = new TradeSharpConnection())
            {
                accountCurrency = db.ACCOUNT.First(a => a.ID == accountId).Currency;
                balances = db.BALANCE_CHANGE.Where(bc => bc.AccountID == accountId).OrderBy(b => b.ValueDate).ToList().Select(
                    LinqToEntity.DecorateBalanceChange).ToList();
                positionsOpened = db.POSITION.Where(bc => bc.AccountID == accountId).ToList().Select(LinqToEntity.DecorateOrder).ToList();
                positionClosed = db.POSITION_CLOSED.Where(bc => bc.AccountID == accountId).ToList().Select(LinqToEntity.DecorateOrder).ToList();
            }

            if (balances.Count < 2) return;

            var startTime = balances.Min(b => b.ValueDate);
            startTime = startTime.Date.AddMinutes(intervalMinutes *
                ((int) (startTime - startTime.Date).TotalMinutes) / intervalMinutes);
            var endTime = DateTime.Now;

            var dueDeals = positionsOpened.Union(positionClosed).OrderBy(p => p.TimeEnter).ToList();

            var cursor = new BacktestTickerCursor();
            var tickers = dueDeals.Select(p => p.Symbol).Distinct().ToList();
            var track = new Strategy(startTime, (int) balances.First().SignedAmountDepo);

            try
            {
                cursor.SetupCursor(ExecutablePath.ExecPath + "\\quotes", tickers, startTime);
                decimal balance = 0;

                for (var time = startTime; time <= endTime; time = time.AddMinutes(intervalMinutes))
                {
                    var candles = cursor.MoveToTime(time);
                    var quotes = candles.ToDictionary(
                        c => c.Key,
                        c => new QuoteData(c.Value.close, c.Value.close, c.Value.timeClose));

                    // учесть вводы - выводы
                    var deltaBalance = 0M;
                    var deltaDeposit = 0M;
                    var closedProfit = 0M;
                    for (var i = 0; i < balances.Count; i++)
                    {
                        if (balances[i].ValueDate > time) break;
                        deltaBalance += balances[i].SignedAmountDepo;
                        if (balances[i].ChangeType == BalanceChangeType.Profit ||
                            balances[i].ChangeType == BalanceChangeType.Loss)
                            closedProfit += balances[i].SignedAmountDepo;
                        else 
                            deltaDeposit += balances[i].SignedAmountDepo;
                        balances.RemoveAt(i);
                        i--;
                    }
                    balance += deltaBalance;

                    // открытый профит
                    var openProfit = 0M;
                    var volumesDepo = new Dictionary<string, decimal>();

                    for (var i = 0; i < dueDeals.Count; i++)
                    {
                        if (dueDeals[i].TimeEnter > time) break;
                        if (dueDeals[i].TimeExit.HasValue && dueDeals[i].TimeExit.Value <= time)
                        {
                            dueDeals.RemoveAt(i);
                            i--;
                            continue;
                        }
                        // профит по сделке на момент
                        var profit = DalSpot.Instance.CalculateProfitInDepoCurrency(dueDeals[i],
                            quotes, 
                            accountCurrency);
                        if (profit.HasValue)
                            openProfit += (decimal) profit.Value;
                        else
                            logger.Log(LogEntryType.Error, "CONV", 20, "Невозможно конвертировать профит сделки {0}",
                                dueDeals[i].ToStringShort() + " @" + dueDeals[i].TimeEnter.ToStringUniform());
                        // объем
                        string errStr;
                        var volumeDepo = DalSpot.Instance.ConvertToTargetCurrency(dueDeals[i].Symbol, true,
                            accountCurrency, dueDeals[i].Volume, quotes, out errStr, true) ?? 0;
                        if (!string.IsNullOrEmpty(errStr))
                            logger.Log(LogEntryType.Error, "CONV", 20, "Невозможно конвертировать объем сделки {0}: {1}",
                                dueDeals[i].ToStringShort() + " @" + dueDeals[i].TimeEnter.ToStringUniform(), errStr);

                        volumeDepo *= dueDeals[i].Side;

                        if (volumesDepo.ContainsKey(dueDeals[i].Symbol))
                            volumesDepo[dueDeals[i].Symbol] += volumeDepo;
                        else
                            volumesDepo.Add(dueDeals[i].Symbol, volumeDepo);
                    }

                    var sumVolumesDepo = Math.Abs(volumesDepo.Sum(v => v.Value));

                    track.AddRecord(time, balance, balance + openProfit, sumVolumesDepo, deltaDeposit, closedProfit);
                }
            }
            finally
            {
                cursor.Close();
            }

            // сохранить трек
            using (var sw = new StreamWriter(targetFilePath, false, new System.Text.UTF8Encoding(false)))
                track.Serialize(sw);
        }
    }
}

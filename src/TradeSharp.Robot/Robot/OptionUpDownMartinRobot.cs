using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Robot.BacktestServerProxy;
using TradeSharp.Util;

namespace TradeSharp.Robot.Robot
{
    // ReSharper disable LocalizableElement
    [DisplayName("Тест Б.О.")]
    public class OptionUpDownMartinRobot : BaseRobot
    {
        class Bet
        {
            public decimal Amount { get; set; }

            public decimal Price { get; set; }

            public DateTime Time { get; set; }

            public DateTime StrikeTime { get; set; }

            public string Symbol { get; set; }

            public int Side { get; set; }

            public decimal Payout { get; set; }

            public bool Successfull { get; set; }

            public int MartinNumber { get; set; }

            public Bet()
            {                
            }

            public Bet(decimal price, decimal amount, DateTime time, DateTime strikeTime, string symbol, int side)
            {
                Price = price;
                Amount = amount;
                Time = time;
                StrikeTime = strikeTime;
                Symbol = symbol;
                Side = side;
            }
        }

        #region Settings

        private decimal startDepo = 5000;
        [PropertyXMLTag("StartDepo")]
        [DisplayName("Старт. депозит")]
        [Category("Торговые")]
        [Description("Стартовый \"депозит\", USD")]
        public decimal StartDepo
        {
            get { return startDepo; }
            set { startDepo = value; }
        }

        private decimal betSize = 10;
        [PropertyXMLTag("BetSize")]
        [DisplayName("Ставка")]
        [Category("Торговые")]
        [Description("Ставка, USD")]
        public decimal BetSize
        {
            get { return betSize; }
            set { betSize = value; }
        }

        private decimal martinRate = 2;

        [PropertyXMLTag("MartinRate")]
        [DisplayName("К. Мартина")]
        [Category("Торговые")]
        [Description("Коэффициент Мартингейла")]
        public decimal MartinRate
        {
            get { return martinRate; }
            set { martinRate = value; }
        }

        private int percentWin = 70;
        [PropertyXMLTag("PercentWin")]
        [DisplayName("Ставка - выигрыш")]
        [Category("Торговые")]
        [Description("Процент выигрыша по сработавшему опциону")]
        public int PercentWin
        {
            get { return percentWin; }
            set { percentWin = value; }
        }
        
        private int percentLose = 5;
        [PropertyXMLTag("PercentLose")]
        [DisplayName("Ставка - возврат")]
        [Category("Торговые")]
        [Description("Процент возврата по не сработавшему опциону")]
        public int PercentLose
        {
            get { return percentLose; }
            set { percentLose = value; }
        }

        private int maxMartinSeries = 5;
        [PropertyXMLTag("MaxMartinSeries")]
        [DisplayName("Макс сделок")]
        [Category("Торговые")]
        [Description("Макс. сделок подряд")]
        public int MaxMartinSeries
        {
            get { return maxMartinSeries; }
            set { maxMartinSeries = value; }
        }

        private int candlesBeforeEnter = 4;
        [PropertyXMLTag("CandlesBeforeEnter")]
        [DisplayName("Шаблон, свечек")]
        [Category("Торговые")]
        [Description("Свечек подряд перед входом")]
        public int CandlesBeforeEnter
        {
            get { return candlesBeforeEnter; }
            set { candlesBeforeEnter = value; }
        }
        #endregion

        #region Variables

        private List<string> events;

        private Dictionary<string, CandlePacker> packers;

        private Dictionary<string, RestrictedQueue<CandleData>> lastCandles;

        private Dictionary<string, List<Bet>> betsBySymbol;

        private Dictionary<string, List<Bet>> betsHistory;

        private decimal balance;

        private int betNumberWhenDepoWasLost;

        private decimal maxBalance;

        private int betsCompletedOnBalancePeak;

        #endregion

        public override BaseRobot MakeCopy()
        {
            var bot = new OptionUpDownMartinRobot
            {
                StartDepo = StartDepo,
                BetSize = BetSize,
                MartinRate = MartinRate,
                PercentWin = PercentWin,
                PercentLose = PercentLose,
                MaxMartinSeries = MaxMartinSeries,
                CandlesBeforeEnter = CandlesBeforeEnter
            };
            CopyBaseSettings(bot);
            return bot;
        }

        public override void Initialize(RobotContext robotContext, CurrentProtectedContext protectedContext)
        {
            base.Initialize(robotContext, protectedContext);
            if (Graphics.Count == 0)
            {
                Logger.DebugFormat("OptionUpDownMartinRobot: настройки графиков не заданы");
                return;
            }
            packers = Graphics.ToDictionary(g => g.a, g => new CandlePacker(g.b));
            lastCandles = Graphics.ToDictionary(g => g.a, g => new RestrictedQueue<CandleData>(candlesBeforeEnter));
            betsBySymbol = Graphics.ToDictionary(g => g.a, g => new List<Bet>());
            betsHistory = Graphics.ToDictionary(g => g.a, g => new List<Bet>());
            balance = StartDepo;
            maxBalance = StartDepo;
            betNumberWhenDepoWasLost = 0;
        }

        public override void DeInitialize()
        {
            base.DeInitialize();
            // вывести в файл результаты
            var resultsStr = new StringBuilder();
            resultsStr.AppendLine("********************************************");
            resultsStr.AppendLine(string.Format("Start depo: {0}", StartDepo.ToStringUniformMoneyFormat()));
            resultsStr.AppendLine(string.Format("Bet: {0}, Martin: {1}, Win/Loss: {2}%/{3}%",
                BetSize, MartinRate, PercentWin, PercentLose));
            resultsStr.AppendLine(string.Format("Max series: {0}, Candles in pattern: {1}",
                MaxMartinSeries, CandlesBeforeEnter));
            resultsStr.AppendLine("********************************************");

            var allBets = betsHistory.SelectMany(g => g.Value).ToList();
            resultsStr.AppendLine(string.Format("Bets completed: {0}, Win percent: {1:f2}%",
                allBets.Count, 
                allBets.Count == 0 ? 0 : 100 * allBets.Count(b => b.Successfull) / allBets.Count));
            resultsStr.AppendLine(string.Format("Bets total: {0}, Payout total: {1}, Saldo: {2}",
                allBets.Sum(b => b.Amount).ToStringUniformMoneyFormat(),
                allBets.Sum(b => b.Payout).ToStringUniformMoneyFormat(),
                (allBets.Sum(b => b.Payout) - allBets.Sum(b => b.Amount)).ToStringUniformMoneyFormat()));
            resultsStr.AppendLine(string.Format("Avg chain: {0:f2}, Max chain: {1}",
                allBets.Count == 0 ? 0 : allBets.Average(b => b.MartinNumber),
                allBets.Count == 0 ? 0 : allBets.Max(b => b.MartinNumber)));
            resultsStr.AppendLine(string.Format("Avg bet: {0:f2}, Max bet: {1}",
                allBets.Count == 0 ? 0 : allBets.Average(b => b.Amount),
                allBets.Count == 0 ? 0 : allBets.Max(b => b.Amount)));
            resultsStr.AppendLine(string.Format("Final balance: {0}, lost deposit on: {1}",
                balance.ToStringUniformMoneyFormat(), 
                (betNumberWhenDepoWasLost > 0 ? betNumberWhenDepoWasLost + " bet" : "-")));
            resultsStr.AppendLine(string.Format("Max balance: {0} on {1} bets completed",
                maxBalance.ToStringUniformMoneyFormat(false), betsCompletedOnBalancePeak));
            resultsStr.AppendLine("[END]");

            // записать в файл
            var fileName = ExecutablePath.ExecPath + "\\option_martin.txt";
            using (var sw = new StreamWriter(fileName, true, Encoding.UTF8))
            {
                sw.Write(resultsStr);
            }
        }

        public override List<string> OnQuotesReceived(string[] names, CandleDataBidAsk[] quotes, bool isHistoryStartOff)
        {
            events = new List<string>();
            if (packers == null || packers.Count == 0) return events;

            for (var i = 0; i < quotes.Length; i++)
            {
                var quote = quotes[i];
                var name = names[i];
                CandlePacker packer;
                if (!packers.TryGetValue(name, out packer))
                    continue;
                var candle = packer.UpdateCandle(quote);
                if (candle == null) continue;
                lastCandles[name].Add(candle);

                // проверить ставки
                CheckBets(name, candle);
                if (balance > maxBalance)
                {
                    maxBalance = balance;
                    betsCompletedOnBalancePeak = betsHistory.Count;
                }
            }

            return events;
        }

        private void CheckBets(string symbol, CandleData candle)
        {
            var bets = betsBySymbol[symbol];
            var history = betsHistory[symbol];

            for (var i = 0; i < bets.Count; i++)
            {
                if (bets[i].StrikeTime > candle.timeClose) continue;
                // проверить контракт (win / loss)
                bets[i].Successfull = (bets[i].Side == 1 && candle.close > (float) bets[i].Price) ||
                          (bets[i].Side == -1 && candle.close < (float) bets[i].Price);
                var payout = bets[i].Successfull ? (100 + PercentWin)/100M : PercentLose/100M;
                payout *= bets[i].Amount;
                bets[i].Payout = payout;
                history.Add(bets[i]);
                bets.RemoveAt(i);
                i--;
                balance += payout;
            }

            // сделать новую ставку по мартину?
            var sign = 0;
            var rate = 1;
            var enterByMartin = false;
            var lastBet = history.LastOrDefault();
            
            if (lastBet != null)
            {
                sign = lastBet.Side;
                enterByMartin = lastBet.Successfull && lastBet.MartinNumber < MaxMartinSeries;                
            }
            rate = enterByMartin ? lastBet.MartinNumber + 1 : 0;

            // просто сделать новую ставку?
            if (!enterByMartin)
            {
                var candles = lastCandles[symbol];
                if (candles.Length < candles.MaxQueueLength) return;
                var firstCandle = candles.First;
                sign = Math.Sign(firstCandle.close - firstCandle.open);
                if (sign == 0) return;
                if (candles.Any(c => Math.Sign(c.close - c.open) != sign)) return;
                sign = -sign;
            }

            // таки сделать ставку
            var amount = Math.Round(BetSize * (decimal) Math.Pow((double) MartinRate, rate));
            var strikeTime = candle.timeClose.AddMinutes(packers[symbol].BarSettings.Intervals[0]);
            bets.Add(new Bet((decimal)candle.close, amount, candle.timeClose,
                strikeTime, symbol, sign)
            {
                MartinNumber = enterByMartin ? lastBet.MartinNumber + 1 : 1
            });
            balance -= amount;
            if (balance < 0 && betNumberWhenDepoWasLost == 0)
                betNumberWhenDepoWasLost = history.Count + bets.Count - 1;
        }        
    }
    // ReSharper restore LocalizableElement
}

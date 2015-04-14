using System;
using System.Collections.Generic;
using System.Globalization;

namespace TradeSharp.RobotFarm.Request
{
    public class Position
    {
        public int Id { get; set; }

        public int Mt4Order { get; set; }

        public int AccountId { get; set; }

        public string Symbol { get; set; }

        public int Side { get; set; }

        public int Volume { get; set; }

        public decimal PriceEnter { get; set; }

        public decimal PriceExit { get; set; }

        public DateTime TimeEnter { get; set; }

        public DateTime TimeExit { get; set; }

        public decimal Sl { get; set; }

        public decimal Tp { get; set; }

        public decimal Profit { get; set; }

        public decimal VolumeDepo { get; set; }

        public Position() { }

        public Position(int id, int side, string symbol, int volume, decimal priceEnter, DateTime timeEnter)
        {
            Id = id;
            Side = side;
            Symbol = symbol;
            Volume = volume;
            PriceEnter = priceEnter;
            TimeEnter = timeEnter;
        }

        public override string ToString()
        {
            var parts = new List<string>();
            parts.Add("[" + Id + "] ");
            parts.Add(Side > 0 ? "BUY" : Side < 0 ? "SELL" : "-");
            parts.Add(Volume.ToString());
            parts.Add(Symbol);
            parts.Add("at " + ToStringUniformPriceFormat(PriceEnter));
            parts.Add(TimeEnter.ToString("yyyy-MM-dd HH:mm:ss"));
            if (PriceExit > 0)
            {
                parts.Add("closed at " + ToStringUniformPriceFormat(PriceEnter));
                parts.Add(TimeExit.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            return string.Join(" ", parts);
        }

        private static string ToStringUniformPriceFormat(decimal price, bool extraDigit = false)
        {
            return price > 25
                ? price.ToString(extraDigit ? "f3" : "f2", CultureInfo.InvariantCulture)
                       : price > 7 ? price.ToString(extraDigit ? "f4" : "f3", CultureInfo.InvariantCulture)
                       : price.ToString(extraDigit ? "f5" : "f4", CultureInfo.InvariantCulture);
        }
    }
}

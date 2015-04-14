using System;
using System.Text;
using TradeSharp.Util;

namespace TradeSharp.NewsGrabber.Grabber.CME
{
    public class CMEBulletinTableRow
    {
        // строка вида
        // 1380      ----                       ----        ----        22.90N                   27.80+    49    .536   ----      6       157       UNCH   29.30B     13.60
        public decimal? Strike { get; set; }
        public decimal? OpenRange { get; set; }
        public decimal? High { get; set; }
        public decimal? Low { get; set; }
        public decimal? ClosingRange { get; set; }
        public decimal? SettPrice { get; set; }
        public decimal? PtChange { get; set; }
        public decimal? Delta { get; set; }
        public decimal? Excercises { get; set; }
        public decimal? Volume { get; set; }
        public decimal? OpenInterest { get; set; }
        public decimal? OpenInterestChange { get; set; }
        public decimal? ContractHigh { get; set; }
        public decimal? ContractLow { get; set; }

        public static CMEBulletinTableRow ParseLine(string line)
        {
            try
            {
                var parts = line.Split(new[] {' ', (char) 9, '+'}, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 14) return null;

                var row = new CMEBulletinTableRow {Strike = ParseValue(parts[0])};
                if (!row.Strike.HasValue) return null;

                row.OpenRange = ParseValue(parts[1]);
                row.High = ParseValue(parts[2]);
                row.Low = ParseValue(parts[3]);
                row.ClosingRange = ParseValue(parts[4]);
                row.SettPrice = ParseValue(parts[5]);
                row.PtChange = ParseValue(parts[6]);
                row.Delta = ParseValue(parts[7]);
                row.Excercises = ParseValue(parts[8]);
                row.Volume = ParseValue(parts[9]);
                row.OpenInterest = ParseValue(parts[10]);
                row.OpenInterestChange = ParseValue(parts[11]);
                row.ContractHigh = ParseValue(parts[12]);
                row.ContractLow = ParseValue(parts[13]);
                return row;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("CMEBulletinTableRow.ParseLine: \"{0}\", строка [{1}]", ex.Message, line);
                return null;
            }
        }

        private static decimal? ParseValue(string str)
        {
            if (string.IsNullOrEmpty(str)) return null;
            var strPure = new StringBuilder();
            foreach (var c in str)
            {
                if (char.IsDigit(c) || c == '.') strPure.Append(c);
            }
            return strPure.ToString().ToDecimalUniformSafe();
        }
    }
}

using System;
using System.Globalization;
using TradeSharp.Util;

namespace Entity
{
    [Serializable]
    public class FuturesVolume
    {
        public string Ticker { get; set; }
        public int VolumeRTH { get; set; }
        public int VolumeGlobex { get; set; }
        public int OpenInterest { get; set; }
        public DateTime Date { get; set; }
        private const string Separator = "#;";

        /// <summary>
        /// Формат строки "[#fmt]#;Ticker=EURUSD#;VolumeRTH=5412#;VolumeGlobex=25701#;OpenInterest=24790#;Date=10062011#;"
        /// </summary>        
        public static FuturesVolume Parse(string str)
        {
            if (string.IsNullOrEmpty(str)) return null;
            if (!str.StartsWith("[#fmt]#;")) return null;            
            var parts = str.Split(new [] { Separator }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 6) return null;

            var futVol = new FuturesVolume();

            for (var i = 1; i < parts.Length; i++)
            {
                var keyValue = parts[i].Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (keyValue.Length != 2) continue;
                if (keyValue[0] == "Ticker") futVol.Ticker = keyValue[1];
                else if (keyValue[0] == "VolumeRTH") futVol.VolumeRTH = ParseStringSafe(keyValue[1]);
                else if (keyValue[0] == "VolumeGlobex") futVol.VolumeGlobex = ParseStringSafe(keyValue[1]);
                else if (keyValue[0] == "OpenInterest") futVol.OpenInterest = ParseStringSafe(keyValue[1]);
                else if (keyValue[0] == "Date") futVol.Date = ParseDateSafe(keyValue[1]);
            }
            return futVol.IsCompleted ? futVol : null;
        }

        public override string ToString()
        {
            return string.Format(
                "[#fmt]{0}Ticker={1}{0}VolumeRTH={2}{0}VolumeGlobex={3}{0}OpenInterest={4}{0}Date={5:ddMMyyyy}{0}",
                Separator,
                Ticker,
                VolumeRTH,
                VolumeGlobex,
                OpenInterest,
                Date);
        }

        private static int ParseStringSafe(string str)
        {
            var rst = 0;
            int.TryParse(str, out rst);
            return rst;
        }

        private static DateTime ParseDateSafe(string str)
        {
            DateTime date;
            DateTime.TryParseExact(str, "ddMMyyyy", CultureProvider.Common, DateTimeStyles.None, out date);
            return date;
        }

        public bool IsCompleted
        {
            get { return !(string.IsNullOrEmpty(Ticker) || VolumeGlobex == 0 || Date == default(DateTime)); }
        }
    }
}
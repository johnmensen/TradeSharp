using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.OptionCalculator.BL
{
    public class Mt4CandleReader
    {
        private string[] tickers;

        private int[] timeframes;

        private readonly string mt4QuotePath = AppConfig.GetStringParam("Mt4.QuotePath",
            @"c:\MetaTrader4Server\history\");

        public void Setup(string[] tickers, int[] timeframes)
        {
            this.tickers = tickers;
            this.timeframes = timeframes;
        }

        public List<CandleByTicker> ReadCandles()
        {
            var allCandles = new List<CandleByTicker>();
            int fileTimeframe;

            foreach (var ticker in tickers)
                foreach (var timeframe in timeframes)
                {
                    var candles = new CandleByTicker(ticker, timeframe);
                    allCandles.Add(candles);
                    var path = Path.Combine(mt4QuotePath, ticker + timeframe + ".fxh");
                    if (!File.Exists(path))
                        continue;
                    ReadCandlesFromFile(candles, path, out fileTimeframe);
                }

            return allCandles;
        }

        public bool ReadCandlesFromFile(CandleByTicker candles, string path, out int timeframe)
        {
            timeframe = 0;
            using (var sr = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var header = sr.ReadStruct<DataBaseHeader>();
                if (!header.HasValue)
                    return false;
                var denominator = (float)Math.Pow(10, -header.Value.digits);
                var mt4StartDate = new DateTime(1970, 1, 1);
                timeframe = header.Value.period;

                while (true)
                {
                    var record = sr.ReadStruct<RateInfo>();
                    if (!record.HasValue) break;

                    var candle = new CandleData
                    {
                        timeOpen = mt4StartDate.AddSeconds(record.Value.ctm),
                        open = record.Value.open * denominator
                    };
                    candle.high = candle.open + record.Value.high * denominator;
                    candle.low = candle.open + record.Value.low * denominator;
                    candle.close = candle.open + record.Value.close * denominator;
                    candle.timeClose = candle.timeOpen.AddMinutes(candles.Timeframe);
                    candles.candles.Add(candle);
                }
            }
            if (candles.candles.Count > 0)
                candles.StartTime = candles.candles.Min(c => c.timeOpen);

            return true;
        }
    }

    public static class StreamExtensions
    {
        public static T? ReadStruct<T>(this Stream stream) where T : struct
        {
            var sz = Marshal.SizeOf(typeof(T));
            var buffer = new byte[sz];
            var countRead = stream.Read(buffer, 0, sz);
            if (countRead < sz)
                return null;
            var pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var structure = (T)Marshal.PtrToStructure(
                pinnedBuffer.AddrOfPinnedObject(), typeof(T));
            pinnedBuffer.Free();
            return structure;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DataBaseHeader // заголовок баз данных
    {
        public int version; // номер версии
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public char[] copyright; // строка копирайта
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public char[] name; // описание типа базы
        public int unused; // future use
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public double[] prices; // последние цены bid/ask
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public char[] symbol; // символ
        public int period; // период
        public UInt32 timesign; // временной отпечаток
        public int digits; // число знаков после запятой в инструменте
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public char[] reserved; // на всякий случай
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RateInfo
    {
        public UInt32 ctm; // текущее время в секундах (начиная с 1970 года)
        public int open; // цена открытия: 11987 => 119.87 (2 знака), 11987 => 1.1987 (4 знака)
        public int high, low, close;
        // смещение цен в пунктах: open=11987 => open равна 119.87; high=5 => high равна 119.92
        public double vol; // объем
    }
}

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Entity;

namespace TradeSharp.OptionCalculator.BL
{
    public class HstFileReader
    {
        public bool ReadCandlesFromFile(CandleByTicker candles, string path, out int timeframe)
        {
            timeframe = 0;
            using (var sr = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var header = sr.ReadStruct<HstFileHeader>();
                if (!header.HasValue)
                    return false;
                var mt4StartDate = new DateTime(1970, 1, 1);
                timeframe = header.Value.period;

                while (true)
                {
                    var record = sr.ReadStruct<HstFileRecord>();
                    if (!record.HasValue) break;

                    var candle = new CandleData
                    {
                        timeOpen = mt4StartDate.AddSeconds(record.Value.ctm),
                        open = (float)record.Value.open,
                        high = (float)record.Value.high,
                        low = (float)record.Value.low,
                        close = (float)record.Value.close
                    };
                    candle.timeClose = candle.timeOpen.AddMinutes(candles.Timeframe);
                    candles.candles.Add(candle);
                }
            }
            if (candles.candles.Count > 0)
                candles.StartTime = candles.candles.Min(c => c.timeOpen);

            return true;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct HstFileHeader
    {
        /// <summary>
        /// database version - 401	4 bytes
        /// </summary>
        public int	version;
        
        /// <summary>
        /// copyright info	64 bytes
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public char[]	copyright;
        
        /// <summary>
        /// symbol name	12 bytes
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public char[]	symbol;
        
        /// <summary>
        /// symbol timeframe 4 bytes
        /// </summary>
        public int	period;
        
        /// <summary>
        /// the amount of digits after decimal point	4 bytes
        /// </summary>
        public int	digits;

        /// <summary>
        /// timesign of the database creation	4 bytes
        /// </summary>
        public UInt32 timesign;
        
        /// <summary>
        /// the last synchronization time	4 bytes
        /// </summary>
        public UInt32 last_sync;

        /// <summary>
        /// to be used in future	52 bytes
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)] 
        public int[] unused;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct HstFileRecord
    {
        public UInt64 ctm;	// bar start time	8 bytes
        public double open;	// open price	8 bytes
        public double high;	// highest price	8 bytes
        public double low;	// lowest price	8 bytes
        public double close;	// close price	8 bytes
        public long volume;	// tick count	8 bytes
        public int spread;	// spread	4 bytes
        public long real_volume;	// real volume	8 bytes
    }
}

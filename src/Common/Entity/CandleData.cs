using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace Entity
{
    public class CandleData
    {
        private static readonly char[] fileLineSeparators = new [] { ' ' };
        
        public float open, high, low, close;
        
        public DateTime timeOpen, timeClose;

        #region Доп свойства
        public Color? customColor;
        #endregion

        public CandleData() {}
        
        public CandleData(CandleData spec)
        {
            open = spec.open;
            high = spec.high;
            close = spec.close;
            low = spec.low;
            timeOpen = spec.timeOpen;
            timeClose = spec.timeClose;
            customColor = spec.customColor;
        }

        public CandleData(CandleDataPacked spec, int pointCost)
        {
            open = spec.open;
            timeOpen = spec.timeOpen;
            timeClose = spec.timeOpen.AddMinutes(1);
            close = spec.close;
            MakeHlcFromOffset16(spec.HLC, pointCost);
        }
        
        public CandleData(float open, float high, float low, float close,
            DateTime timeOpen, DateTime timeClose)
        {
            this.open = open;
            this.high = high;
            this.close = close;
            this.low = low;
            this.timeOpen = timeOpen;
            this.timeClose = timeClose;
        }

        public float GetPrice(CandlePriceType priceType)
        {
            switch (priceType)
            {
                case CandlePriceType.Close : return close;
                case CandlePriceType.Open : return open;
                case CandlePriceType.High : return high;
                case CandlePriceType.Low : return low;
                case CandlePriceType.HighLowMid : return (high + low) * 0.5f;
                case CandlePriceType.OpenCloseMid : return (open + close) * 0.5f;
            }
            return close;
        }

        /// <summary>
        /// получить из H, L и С их дельты в пунктах от цены open
        /// и слепить их в одну переменную short
        /// </summary>
        /// <param name="versePointValue">10000 - EURUSD, 100 - USDJPY</param>
        /// <returns>[MSB]0xCCLLHH[LSB]</returns>
        public int GetHlcOffset(int versePointValue)
        {
            var h = (int) Math.Round(versePointValue * (high - open)) + 0x7F;
            if (h < 0)
                h = 0;
            else if (h > 0xFF)
                h = 0xFF;

            var l = (int) Math.Round(versePointValue * (low - open)) + 0x7F;
            if (l < 0)
                l = 0;
            else if (l > 0xFF)
                l = 0xFF;

            var c = (int) Math.Round(versePointValue * (close - open)) + 0x7F;
            if (c < 0)
                c = 0;
            else if (c > 0xFF)
                c = 0xFF;

            return h | (l << 8) | (c << 16);
        }

// ReSharper disable InconsistentNaming
        public string GetHlcOffsetHEX(int versePointValue)
// ReSharper restore InconsistentNaming
        {
            return GetHlcOffset(versePointValue).ToString("X");
        }

        public int GetHlcOffset16(int versePointValue)
        {
            var h = (int) Math.Round(versePointValue * 10 * (high - open)) + 0x7FFF;
            if (h < 0)
                h = 0;
            else if (h > 0xFFFF)
                h = 0xFFFF;

            var l = (int) Math.Round(versePointValue * 10 * (low - open)) + 0x7FFF;
            if (l < 0)
                l = 0;
            else if (l > 0xFFFF)
                l = 0xFFFF;

            return h | (l << 16);
        }

// ReSharper disable InconsistentNaming
        public string GetHlcOffsetHEX16(int versePointValue)
// ReSharper restore InconsistentNaming
        {
            return GetHlcOffset16(versePointValue).ToString("X");
        }

        /// <summary>
        /// обратна GetHlcOffset - восстанавливает цены high, low, close из целочисленной
        /// переменной, содержащей дельты цен (от open) побайтно
        /// </summary>
        /// <param name="offset">цены H, L и C, упакованные в байты (0xCCLLHH)</param>
        /// <param name="versePointValue">10000 - EURUSD, 100 - USDJPY</param>
        public void MakeHlcFromOffset(int offset, int versePointValue)
        {
            var h = (offset & 0xFF) - 0x7F;
            var l = ((offset & 0xFF00) >> 8) - 0x7F;
            var c = ((offset & 0xFF0000) >> 16) - 0x7F;

            high = open + (h / (float) versePointValue);
            low = open + (l / (float) versePointValue);
            close = open + (c / (float) versePointValue);
        }

        public void MakeHlcFromOffset16(int offset, int versePointValue)
        {
            var h = (offset & 0xFFFF) - 0x7FFF;
            var l = ((offset & 0xFFFF0000) >> 16) - 0x7FFF;

            high = open + (h / (float) versePointValue / 10);
            low = open + (l / (float) versePointValue / 10);
        }

        /// <summary>
        /// обратна GetHlcOffset - восстанавливает цены high, low, close из целочисленной
        /// переменной, содержащей дельты цен (от open) побайтно
        /// </summary>
        /// <param name="offsetStr">цены H, L и C, упакованные в байты (CCLLHH)</param>
        /// <param name="versePointValue">10000 - EURUSD, 100 - USDJPY</param>
// ReSharper disable InconsistentNaming
        public void MakeHlcFromOffsetHEX(string offsetStr, int versePointValue)
// ReSharper restore InconsistentNaming
        {
            MakeHlcFromOffset(int.Parse(offsetStr, NumberStyles.HexNumber), versePointValue);
        }

// ReSharper disable InconsistentNaming
        public void MakeHlcFromOffsetHEX16(string offsetStr, int versePointValue)
// ReSharper restore InconsistentNaming
        {
            MakeHlcFromOffset16(int.Parse(offsetStr, NumberStyles.HexNumber), versePointValue);
        }

        public override string ToString()
        {
            return string.Format("[{0}]: {1} {2} {3} {4}",
                                 timeOpen.ToStringUniform(),
                                 open.ToStringUniformPriceFormat(true),
                                 high.ToStringUniformPriceFormat(true),
                                 low.ToStringUniformPriceFormat(true),
                                 close.ToStringUniformPriceFormat(true));
        }

        #region Статические методы
        
        public static void SaveInFile(string fileName, string symbol, List<CandleData> candles)
        {
            var curDate = new DateTime(1780, 1, 1);
            var precision = DalSpot.Instance.GetPrecision(symbol);
            //var format = "f" + precision; // old format
            var newFormat = "f" + (precision + 1); // new format
            var precision10 = precision == 4 ? 10000 : precision == 2 ? 100 : (int) Math.Pow(10, precision);

            using (var sw = new StreamWriter(fileName, false, Encoding.ASCII))
            {
                foreach (var candle in candles)
                {
                    if (candle.timeOpen.Date != curDate)
                    {
                        curDate = candle.timeOpen.Date;
                        sw.WriteLine(string.Format("{0:ddMMyyyy}", curDate));                        
                    }
                    // old format
                    /*sw.WriteLine(string.Format("{0:HHmm} {1} {2} {3}/{4}/{5}",
                        candle.timeOpen, candle.open.ToString(format, CultureInfo.InvariantCulture),
                        candle.GetHlcOffsetHEX(precision10), candle.high, candle.low, candle.close));*/
                    // new format
                    sw.WriteLine(string.Format("{0:HHmm} {1} {2}",
                        candle.timeOpen, candle.open.ToString(newFormat, CultureInfo.InvariantCulture),
                        candle.GetHlcOffsetHEX16(precision10), candle.high, candle.low, candle.close));
                }
            }
        }

        public static List<CandleData> LoadFromFile(string filePath, string symbol)
        {
            var precision = DalSpot.Instance.GetPrecision10(symbol);
            var candles = new List<CandleData>();
            CandleData previousCandle = null;
            DateTime? date = null;

            var countErrorsLeft = 25;
            using (var sr = new StreamReader(filePath, Encoding.ASCII))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    var oldDate = date;
                    var candle = ParseLine(line, ref date, precision, ref previousCandle);
                    if (candle == null)
                    {
                        if (!oldDate.HasValue && date.HasValue)
                            continue;
                        if (oldDate.HasValue && date.HasValue && oldDate.Value != date.Value)
                            continue;
                        countErrorsLeft--;
                        if (countErrorsLeft <= 0)
                            break;
                    }
                    else // prevent adding nulls
                        candles.Add(candle);
                }
            }
            return candles;
        }

        /// <summary>
        /// "склеить" свечи (lstDest, candles) в единый список (lstDest)
        /// </summary>
        public static bool MergeCandles(ref List<CandleData> lstDest, List<CandleData> candles, bool checkOverlap)
        {
            var start = lstDest.Count == 0 ? new DateTime() : lstDest[0].timeOpen;
            var end = lstDest.Count == 0 ? new DateTime() : lstDest[lstDest.Count - 1].timeOpen;
                
            bool areUpdated;
            lstDest = MergeCandles(lstDest, candles, checkOverlap, ref start, ref end, out areUpdated);
            return areUpdated;
        }

        public static List<CandleData> MergeCandles(
            List<CandleData> srcCandles,
            List<CandleData> lstCandles, 
            bool checkOverlap,
            ref DateTime timeStart, 
            ref DateTime timeEnd,
            out bool areUpdated)
        {
            areUpdated = false;
            if (lstCandles.Count == 0) return srcCandles;
            if (srcCandles.Count == 0)
            {
                areUpdated = true;
                return lstCandles;
            }
            var s1 = srcCandles[0].timeOpen;
            var e1 = srcCandles[srcCandles.Count - 1].timeOpen;
            var s2 = lstCandles[0].timeOpen;
            var e2 = lstCandles[lstCandles.Count - 1].timeOpen;

            // интервалы дополняют один другой
            if (s2 > e1)
            {
                srcCandles.AddRange(lstCandles);
                timeEnd = e2;
                areUpdated = true;
                return srcCandles;
            }
            if (s1 > e2)
            {
                lstCandles.AddRange(srcCandles);
                srcCandles = lstCandles;
                timeStart = s1;
                areUpdated = true;
                return srcCandles;
            }

            // включение интервала
            if (s1 <= s2 && e1 >= e2)
            {
                //areUpdated = false;
                return srcCandles;
            }

            if (s2 <= s1 && e2 >= e1)
            {
                srcCandles = lstCandles;
                timeStart = s2;
                timeEnd = e2;
                areUpdated = true;
                return srcCandles;
            }
            if (!checkOverlap) // areUpdated = false;
                return srcCandles;

            // склеить новые и старые котировки, не допуская дублирования
            if (s2 > s1)
            {
                var start = 0;
                for (; start < lstCandles.Count; start++)
                    if (lstCandles[start].timeOpen > e1) break;
                for (var i = start; i < lstCandles.Count; i++)
                    srcCandles.Add(lstCandles[i]);
                timeEnd = e2;
                areUpdated = true;
                return srcCandles;
            }

            if (s2 < s1)
            {
                var start = 0;
                for (; start < srcCandles.Count; start++)
                    if (srcCandles[start].timeOpen > e2) break;
                for (; start < srcCandles.Count; start++)
                    lstCandles.Add(srcCandles[start]);
                srcCandles = lstCandles;
                timeStart = s2;
                areUpdated = true;
                return srcCandles;
            }
            //areUpdated = false;
            return srcCandles;
        }

        public static CandleData ParseLine(string line, ref DateTime? date, int precision, ref CandleData previousCandle)
        {
            if (string.IsNullOrEmpty(line))
                return null;

            if (line.Length == 8)
            {
                try
                {
                    date = DateTime.ParseExact(line, "ddMMyyyy", CultureInfo.InvariantCulture);
                }
                catch
                {
                }
                return null;
            }

            if (!date.HasValue)
                return null;
            var parts = line.Split(fileLineSeparators, StringSplitOptions.RemoveEmptyEntries);
            /*if (parts.Length != 3)
                return null;*/
            
            try
            {
                var hour = parts[0].Substring(0, 2).ToInt();
                var minute = parts[0].Substring(2, 2).ToInt();
                var time = date.Value.AddMinutes(minute + hour * 60);
                var open = parts[1].ToFloatUniform();
                var hlc = parts[2];
                var candle = new CandleData { open = open, timeOpen = time, timeClose = time.AddMinutes(1) };
                if (hlc.Length == 6)
                {
                    candle.MakeHlcFromOffsetHEX(hlc, precision);
                    previousCandle = null;
                }
                else if (hlc.Length == 8)
                {
                    candle.MakeHlcFromOffsetHEX16(hlc, precision);
                    candle.close = candle.open; // default value
                    if (previousCandle != null)
                    {
                        previousCandle.close = candle.open;
                        previousCandle.high = Math.Max(previousCandle.high, candle.open);
                        previousCandle.low = Math.Min(previousCandle.low, candle.open);
                    }
                    previousCandle = candle;
                }
                else // format error
                {
                    candle.high = candle.open;
                    candle.low = candle.open;
                    candle.close = candle.open;
                }
                return candle;
            }
            catch
            {
                return null;
            }                        
        }

        #endregion
    }

    public enum CandlePriceType
    {
        Close = 0,
        Open = 1,
        High = 2,
        Low = 3,
        HighLowMid = 4,
        OpenCloseMid = 5
    }
}

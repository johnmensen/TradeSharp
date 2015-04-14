using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using TradeSharp.Util;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class PackedCandleStream
    {
        private const int SizeofTime = 8;

        private const int SizeofPrice = 4;

        // ReSharper disable InconsistentNaming
        private const int SizeofHLC = 4;
        // ReSharper restore InconsistentNaming

        public enum PackedMethod { FastGZip = 0, DenseSharpZip = 1 }

        private PackedMethod method;

        private bool packed;

        private byte[] candleStream;

        private List<CandleDataPacked> unpacked;

        public int count;

        public PackedCandleStream(List<CandleDataPacked> src, bool shouldPack)
        {
            MakePackedStream(src, shouldPack);
        }

        public PackedCandleStream(List<CandleDataPacked> src, bool shouldPack, PackedMethod method)
        {
            this.method = method;
            MakePackedStream(src, shouldPack);
        }

        public PackedCandleStream(List<CandleDataPacked> src, int minCountToPack, int maxCountToPack)
        {
            var needPack = count >= minCountToPack && count <= maxCountToPack;
            MakePackedStream(src, needPack);
        }

        private void MakePackedStream(List<CandleDataPacked> src, bool shouldPack)
        {
            count = src.Count;
            if (count == 0) return;
            if (!shouldPack)
            {
                unpacked = src;
                return;
            }
            PackCandles(src);
            packed = true;
        }

        private void PackCandles(List<CandleDataPacked> src)
        {
            const int quoteSz = SizeofTime + SizeofPrice + SizeofPrice;
            var size = count * quoteSz;
            var srcStream = new byte[size];

            int i = 0;
            for (var offset = 0; offset < size; offset += quoteSz, i++)
            {
                var quote = src[i];
                var bytes = BitConverter.GetBytes(quote.timeOpen.Ticks);
                bytes.CopyTo(srcStream, offset);
                bytes = BitConverter.GetBytes(quote.open);
                bytes.CopyTo(srcStream, offset + SizeofTime);
                bytes = BitConverter.GetBytes(quote.HLC);
                bytes.CopyTo(srcStream, offset + SizeofTime + SizeofPrice);
            }

            candleStream = method == PackedMethod.FastGZip
                ? PackBytesGZip(srcStream)
                : CompressionHelper.CompressBytes(srcStream);
        }

        /// <summary>
        /// распаковать если необходимо
        /// </summary>
        public List<CandleDataPacked> GetCandles()
        {
            if (!packed) return unpacked;

            var result = new List<CandleDataPacked>(count);
            var unpackedStream = method == PackedMethod.FastGZip
                ? UnpackBytesGZip(candleStream)
                : CompressionHelper.DecompressBytes(candleStream);
            const int quoteSz = SizeofTime + SizeofPrice + SizeofHLC;
            var size = count * quoteSz;
            CandleDataPacked previousCandle = null;

            for (var offset = 0; offset < size; offset += quoteSz)
            {
                var q = new CandleDataPacked
                {
                    timeOpen = new DateTime(BitConverter.ToInt64(unpackedStream, offset)),
                    open = BitConverter.ToSingle(unpackedStream, offset + SizeofTime),
                    HLC = BitConverter.ToInt32(unpackedStream, offset + SizeofTime + SizeofPrice)
                };
                q.close = q.open; // default
                if (previousCandle != null)
                    previousCandle.close = q.open;
                previousCandle = q;
                result.Add(q);
            }

            return result;
        }

        private static byte[] PackBytesGZip(byte[] raw)
        {
            using (var memory = new MemoryStream())
            {
                using (var gzip = new GZipStream(memory, CompressionMode.Compress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }

        private static byte[] UnpackBytesGZip(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (var stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                var buffer = new byte[size];
                using (var memory = new MemoryStream())
                {
                    int count;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }
    }
}

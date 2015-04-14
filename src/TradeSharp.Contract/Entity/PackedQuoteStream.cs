using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using TradeSharp.Util;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class PackedQuoteStream
    {
        private const int SizeofTime = 8;
        private const int SizeofPrice = 4;

        public enum PackedMethod { FastGZip = 0, DenseSharpZip = 1 }

        public PackedMethod method;

        public bool packed;

        public byte[] quoteStream;
        
        public List<QuoteData> unpacked;

        public int count;

        public PackedQuoteStream(List<QuoteData> src, bool shouldPack)
        {
            MakePackedStream(src, shouldPack);
        }

        public PackedQuoteStream(List<QuoteData> src, bool shouldPack, PackedMethod method)
        {
            this.method = method;
            MakePackedStream(src, shouldPack);
        }

        public PackedQuoteStream(List<QuoteData> src, int minCountToPack, int maxCountToPack)
        {
            var needPack = count >= minCountToPack && count <= maxCountToPack;
            MakePackedStream(src, needPack);
        }

        private void MakePackedStream(List<QuoteData> src, bool shouldPack)
        {
            count = src.Count;
            if (count == 0) return;
            if (!shouldPack)
            {
                unpacked = src;
                return;
            }
            PackQuotes(src);
            packed = true;
        }

        private void PackQuotes(List<QuoteData> src)
        {
            const int quoteSz = SizeofTime + SizeofPrice + SizeofPrice;
            var size = count * quoteSz;
            var srcStream = new byte[size];

            int i = 0;
            for (var offset = 0; offset < size; offset += quoteSz, i++)
            {
                var quote = src[i];
                var bytes = BitConverter.GetBytes(quote.time.Ticks);
                bytes.CopyTo(srcStream, offset);
                bytes = BitConverter.GetBytes(quote.ask);
                bytes.CopyTo(srcStream, offset + SizeofTime);
                bytes = BitConverter.GetBytes(quote.bid);
                bytes.CopyTo(srcStream, offset + SizeofTime + SizeofPrice);
            }

            quoteStream = method == PackedMethod.FastGZip 
                ? PackBytesGZip(srcStream)
                : CompressionHelper.CompressBytes(srcStream);
            //Logger.InfoFormat("Packed to {0:f0}%", 100 * quoteStream.Length / size);
        }
    
        /// <summary>
        /// распаковать если необходимо
        /// </summary>
        public List<QuoteData> GetQuotes()
        {
            if (!packed) return unpacked;

            var result = new List<QuoteData>(count);
            var unpackedStream = method == PackedMethod.FastGZip 
                ? UnpackBytesGZip(quoteStream)
                : CompressionHelper.DecompressBytes(quoteStream);
            const int quoteSz = SizeofTime + SizeofPrice + SizeofPrice;
            var size = count * quoteSz;

            for (var offset = 0; offset < size; offset += quoteSz)
            {
                var q = new QuoteData
                            {
                                time = new DateTime(BitConverter.ToInt64(unpackedStream, offset)),
                                ask = BitConverter.ToSingle(unpackedStream, offset + SizeofTime),
                                bid = BitConverter.ToSingle(unpackedStream, offset + SizeofTime + SizeofPrice)
                            };
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
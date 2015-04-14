using System;
using System.Globalization;
using System.IO;
using FXI.Client.ServiceModel.Entities;

namespace IndexSpectrum.Index
{
    public class CurrencyStream
    {
        private StreamReader stream;
        private Quote? lastQuote, nextQuote;
        private readonly string filePath;


        public bool EndOfFile
        {
            get; private set;
        }

        public CurrencyStream(string curName, string directory)
        {
            EndOfFile = true;
            filePath = string.Format("{0}\\{1}.quote",
                                     directory.TrimEnd('\\'), curName);
            if (File.Exists(filePath))
            {
                EndOfFile = false;
                stream = new StreamReader(filePath);
            }
        }

        public CurrencyStream(StreamReader streamReader)
        {            
            stream = streamReader;
            EndOfFile = stream == null;
        }

        public void CloseStream()
        {
            EndOfFile = true;
            if (stream != null) stream.Close();
        }

        public decimal GetQuoteBid(DateTime time)
        {
            var quote = GetQuote(time);
            return quote.HasValue == false ? 0 : quote.Value.bid;
        }

        public decimal GetQuoteAsk(DateTime time)
        {
            var quote = GetQuote(time);
            return quote.HasValue == false ? 0 : quote.Value.ask;
        }

        /// <summary>
        /// вернуть котировку на момент до или точно time
        /// </summary>        
        public Quote? GetQuote(DateTime time)
        {
            if (stream == null) return null;
            if (nextQuote.HasValue)
            {
                if (nextQuote.Value.time > time) return lastQuote;
                // иначе следующая котировка становится последней
                lastQuote = nextQuote;
            }

            while (!stream.EndOfStream)
            {
                // прочитать котировку из потока
                var line = stream.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;
                var quote = ParseQuote(line);
                if (quote.HasValue == false) continue;

                lastQuote = nextQuote;
                nextQuote = quote;
                if (nextQuote.Value.time > time)
                {
                    return lastQuote;
                }
            }
            EndOfFile = true;

            return nextQuote;
        }

        /// <summary>
        /// вернуть дату первой котировки
        /// метод переоткрывает поток чтения файла
        /// </summary>
        /// <returns></returns>
        public DateTime? GetFirstQuoteDate()
        {
            if (stream == null) return null;
            try
            {
                while (!stream.EndOfStream)
                {
                    var line = stream.ReadLine();
                    if (string.IsNullOrEmpty(line)) continue;
                    // имеем 0.824700;0.825050;18.08.2009 14:43:35 ?                    
                    var quote = ParseQuote(line);
                    if (!quote.HasValue) continue;
                    return quote.Value.time;
                }
                return null;
            }
            finally
            {
                stream.Close();
                stream = new StreamReader(filePath);
            }
        }

        private static readonly CultureInfo quoteCulture = new CultureInfo("ru-RU", false);

        private static Quote? ParseQuote(string line)
        {
            if (string.IsNullOrEmpty(line)) return null;
            var parts = line.Split(';');
            if (parts.Length != 3) return null;
            var bid = decimal.Parse(parts[0], CultureInfo.InvariantCulture);
            var ask = decimal.Parse(parts[1], CultureInfo.InvariantCulture);
            var time = DateTime.Parse(parts[2], quoteCulture);
            return new Quote { bid = bid, ask = ask, time = time };
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ForexSiteDailyQuoteParser.CommonClass;
using ForexSiteDailyQuoteParser.Contract;
using TradeSharp.Util;

namespace ForexSiteDailyQuoteParser.Formatters
{
    public class TradeSharpParseFormat : BaseParseFormat, IQuoteParser
    {
        public TradeSharpParseFormat()
        {
            QuoteNameInFileName = true;
            DateTimeFormat = "ddMMyyyy";
            QuotesDate = new List<QuoteRecord>();
            FailRecords = new List<string>();
            QuoteRecordFieldCount = 2;
            Separator = new[] { ' ' };

            DisplayName = "TradeSharp";
        }

        public List<string> Parse(string quoteFileName)
        {
            if (!File.Exists(quoteFileName))
            {
                FailRecords.Add("Файл не найден");
                return null;
            }
            var simbol = quoteFileName.Split('\\').Last().Split('.').First();

            try
            {
                using (var sr = new StreamReader(quoteFileName, Encoding.UTF8))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (string.IsNullOrEmpty(line)) continue;

                        var parts = line.Trim().Split(Separator, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length != QuoteRecordFieldCount)
                        {
                            FailRecords.Add(string.Format("количество элементов в строке '{0}' меньше, чем ожидалось", line));
                            continue;
                        }

                        

                        DateTime date;
                        if (!DateTime.TryParseExact(parts[0], DateTimeFormat, CultureProvider.Common,DateTimeStyles.None, out date))
                        {
                            FailRecords.Add(string.Format("Не удалось распарсить дату в строке '{0}'", line));
                            continue;
                        }

                        var close = parts[1].ToFloatUniformSafe();
                        if (!close.HasValue)
                        {
                            FailRecords.Add(string.Format("Не удалось распарсить цену 'close' в строке '{0}'", line));
                            continue;
                        }

                        QuotesDate.Add(new QuoteRecord
                        {
                            Simbol = simbol,
                            Date = date,
                            Close = close.Value
                        });
                    }
                }
                return new List<string> { simbol };
            }
            catch (Exception ex)
            {
                FailRecords.Add(ex.Message);
                return null;
            }
        }

        public IEnumerable<string> QuoteListToString(List<QuoteRecord> quoteList)
        {
            return quoteList.Select(x => string.Format("{0:ddMMyyyy} {1}", x.Date, x.Close));
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}

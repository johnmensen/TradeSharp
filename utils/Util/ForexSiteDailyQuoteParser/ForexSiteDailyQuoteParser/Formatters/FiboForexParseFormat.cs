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
    public class FiboForexParseFormat : BaseParseFormat, IQuoteParser
    {
        public FiboForexParseFormat()
        {
            QuoteNameInFileName = true;
            DateTimeFormat = "yyyy.MM.dd";
            QuotesDate = new List<QuoteRecord>();
            FailRecords = new List<string>();
            QuoteRecordFieldCount = 7;
            Separator = new[] { ',' };

            DisplayName = "FiboForex";
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
                        if (!DateTime.TryParseExact(parts[0], DateTimeFormat, CultureProvider.Common, DateTimeStyles.None, out date))
                        {
                            FailRecords.Add(string.Format("Не удалось распарсить дату в строке '{0}'", line));
                            continue;
                        }

                        #region parse price
                        var open = parts[2].ToFloatUniformSafe();
                        if (!open.HasValue)
                        {
                            FailRecords.Add(string.Format("Не удалось распарсить цену 'open' в строке '{0}'", line));
                            continue;
                        }

                        var high = parts[3].ToFloatUniformSafe();
                        if (!high.HasValue)
                        {
                            FailRecords.Add(string.Format("Не удалось распарсить цену 'high' в строке '{0}'", line));
                            continue;
                        }

                        var low = parts[4].ToFloatUniformSafe();
                        if (!low.HasValue)
                        {
                            FailRecords.Add(string.Format("Не удалось распарсить цену 'low' в строке '{0}'", line));
                            continue;
                        }

                        var close = parts[5].ToFloatUniformSafe();
                        if (!close.HasValue)
                        {
                            FailRecords.Add(string.Format("Не удалось распарсить цену 'close' в строке '{0}'", line));
                            continue;
                        }
                        #endregion

                        QuotesDate.Add(new QuoteRecord
                        {
                            Simbol = simbol,
                            Date = date,
                            Open = open.Value,
                            High = high.Value,
                            Low = low.Value,
                            Close = close.Value
                        });
                    }
                }
                return QuotesDate.Select(x => x.Simbol).Distinct().ToList();
            }
            catch (Exception ex)
            {
                FailRecords.Add(ex.Message);
                return null;
            }
        }

        public IEnumerable<string> QuoteListToString(List<QuoteRecord> quoteList)
        {
            return quoteList.Select(x => string.Format("{0:yyyy.MM.dd},00:00,{1},{2},{3},{4},0", x.Date, x.Open, x.High, x.Low, x.Close));
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}

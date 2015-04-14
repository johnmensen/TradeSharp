using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TradeSharp.Util;

namespace TradeSharp.NewsGrabber.Grabber.CME
{
    public class CMEDocumentParser
    {
        public string SectionNum { get; set; }
        public bool Parsed { get; set; }
        public DateTime DocumentDate { get; set; }
        public readonly List<CMEBulletinTable> tables = new List<CMEBulletinTable>();
        private CMEFuturesSummary futuresInfo;

        public CMEDocumentParser(string fileName, CMETickerInfoCollection bulletinInfo, List<CMEFuturesSummary> futuresSummaries)
        {
            Parsed = ParseTxtDocument(fileName, bulletinInfo);
            if (futuresInfo != null)
                if (futuresInfo.IsValid)
                    futuresSummaries.Add(futuresInfo);
        }

        private bool ParseTxtDocument(string fileName, CMETickerInfoCollection bulletinInfo)
        {
            // получить секцию из имени файла
            int sectionNum;
            if (!GetSectionNumFromFileName(fileName, out sectionNum)) return false;
            SectionNum = sectionNum.ToString();

            // получить набор таблиц (экспирация - опцион - тип опциона)            
            CMEBulletinTable curTable = null;
            bool docDateParsed = false;

            CMETickerInfo curTicker = null;
            CMEBulletinReference curTickerRef = null;            
            
            using (var sr = new StreamReader(fileName, Encoding.Unicode))
            {
                while (!sr.EndOfStream)
                {
                    // парсинг даты
                    var line = sr.ReadLine();
                    try
                    {
                        if (!docDateParsed)
                        {
                            docDateParsed = IsDateRow(line);
                            continue;
                        }

                        // строка содержит месяц экспирации?
                        line = line.Trim(' ', (char) 9);
                        var parts = line.Split(new[] {' ', (char) 9}, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 0) continue;
                        var em = ExpirationMonth.Parse(parts[0]);
                        if (em != null)
                        {
                            if (curTicker != null && curTickerRef != null)
                            {
                                // создать таблицу
                                if (curTable != null) tables.Add(curTable);
                                curTable = new CMEBulletinTable
                                               {
                                                   ExpMonth = em,
                                                   OptStyle = curTickerRef.OptionStyle,
                                                   OptType = curTickerRef.OptionType,
                                                   SpotSymbol = curTicker.SpotSymbol,
                                                   StrikeToBaseRatio = curTicker.StrikeToBaseRatio,
                                                   PremiumToBaseRatio = curTicker.PremiumToBaseRatio,
                                                   InvertRate = curTicker.InvertRate
                                               };
                            }
                            continue;
                        }

                        // строка содержит заголовок?
                        // EURO FX C (EU)
                        // BRIT PND-P EU
                        CMETickerInfo tick;
                        CMEBulletinReference tickRef;
                        if (bulletinInfo.FindTicker(parts, out tick, out tickRef, SectionNum))
                        {
                            if (futuresInfo != null)
                                if (futuresInfo.Ticker == null) futuresInfo.Ticker = tick;
                            curTicker = tick;
                            curTickerRef = tickRef;
                            continue;
                        }

                        var row = CMEBulletinTableRow.ParseLine(line);
                        if (row != null && curTable != null) curTable.rows.Add(row);
                        if (row == null) // строка содержит данные по фьючам?
                        {
                            var summ = CheckFutureInfoLine(parts);
                            if (summ != null) futuresInfo = summ;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorFormat("CMEDocumentParser.ParseTxtDocument parse line: \"{0}\", line:[{1}]",
                            ex.Message, line);
                    }
                }
            }
            try
            {
                CMEBulletinTable.MergeSameTables(tables);
            }
            catch (Exception ex)
            {
                Logger.Error("CMEDocumentParser.ParseTxtDocument MergeSameTables", ex);
            }

            return true;
        }

        private CMEFuturesSummary CheckFutureInfoLine(string[] parts)
        {
            // TOTAL JAPAN YEN FUT 3237 127640 128916 - 3558
            // TOTAL BRIT PND FUT 2583 138407 135828 + 4590
            if (parts.Length < 9 || parts.Length > 10) return null;
            if (parts[0] != "TOTAL") return null;

            var lstNumbers = new List<int>();
            for (var i = parts.Length - 1; i > 3; i--)
            {
                int num;
                if (!int.TryParse(parts[i], out num)) continue;
                lstNumbers.Insert(0, num);
            }
            if (lstNumbers.Count != 4) return null;
            return new CMEFuturesSummary
                           {
                               VolumeRTH = lstNumbers[0],
                               VolumeGlobex = lstNumbers[1],
                               OI = lstNumbers[2],
                               DocumentDate = DocumentDate
                           };
        }

        private bool IsDateRow(string line)
        {            
            if (string.IsNullOrEmpty(line)) return false;
            // ожидается строка вида
            //PG39 BULLETIN # 41@        EURO FX & CME$INDEX OPTIONS     Wed, Mar 02, 2011   PG39
            //PG38 AUSTRALIAN DOLLAR AND NEW ZEALAND DOLLAR OPTIONS Thu, Aug 26, 2010 PG38
            line = line.Trim(' ', (char) 9);
            var preffix = string.Format("PG{0}", SectionNum);
            if (!line.StartsWith(preffix)) return false;
            if (!line.EndsWith(preffix)) return false;
            var parts = line.Split(new[] { ' ', (char)9 }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 9) return false;
            var month = ExpirationMonth.ParseMonth(parts[parts.Length - 4], true); // Mar
            if (month <= 0) return false;
            var dayStr = parts[parts.Length - 3].TrimEnd(','); // Mar
            int day, year;
            if (!int.TryParse(dayStr, out day)) return false;
            var yearStr = parts[parts.Length - 2];
            if (!int.TryParse(yearStr, out year)) return false;
            DocumentDate = new DateTime(year, month, day);
            return true;
        }

        public bool WriteTablesInExcelFormat(StreamWriter fs)
        {
            if (!Parsed) return false;
            try
            {
                fs.WriteLine(string.Format("BULLETIN{0}{1}{0}{2:dd.MM.yyyy}", (char) 9, SectionNum, DocumentDate));
                fs.WriteLine(
                    string.Format("LEGEND:strike{0}volume{0}OI{0}ClosRange{0}OpRange{0}High{0}Low{0}Delta{0}SettPrice",
                                  (char) 9));
                foreach (var tab in tables)
                {
                    // заголовок таблицы
                    fs.WriteLine(string.Format("TABLE{0}{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}", (char) 9,
                                               tab.SpotSymbol, tab.ExpMonth, tab.OptType, tab.OptStyle,
                                               tab.StrikeToBaseRatio, tab.PremiumToBaseRatio,
                                               tab.InvertRate ? 1 : 0));
                    foreach (var row in tab.rows)
                    {
                        var sb = new StringBuilder();
                        sb.AppendFormat("{0}{1}", row.Strike.ToStringUniform().Replace('.', ','), (char) 9);
                        sb.AppendFormat("{0}{1}", row.Volume.ToStringUniform().Replace('.', ','), (char) 9);
                        sb.AppendFormat("{0}{1}", row.OpenInterest.ToStringUniform().Replace('.', ','), (char) 9);
                        sb.AppendFormat("{0}{1}", row.ClosingRange.ToStringUniform().Replace('.', ','), (char) 9);
                        sb.AppendFormat("{0}{1}", row.OpenRange.ToStringUniform().Replace('.', ','), (char) 9);
                        sb.AppendFormat("{0}{1}", row.High.ToStringUniform().Replace('.', ','), (char) 9);
                        sb.AppendFormat("{0}{1}", row.Low.ToStringUniform().Replace('.', ','), (char) 9);
                        sb.AppendFormat("{0}{1}", row.Delta.ToStringUniform().Replace('.', ','), (char) 9);
                        sb.AppendFormat("{0}{1}", row.SettPrice.ToStringUniform().Replace('.', ','), (char) 9);
                        fs.WriteLine(sb.ToString());
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("WriteTablesInExcelFormat.CMEDocumentParser(Stream) error: \"{0}\"", ex);
                return false;
            }
        }

        public bool WriteTablesInExcelFormat(string fileName)
        {
            if (!Parsed) return false;
            try
            {
                using (var fs = new StreamWriter(fileName, false, Encoding.Unicode))
                {
                    return WriteTablesInExcelFormat(fs);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("WriteTablesInExcelFormat.CMEDocumentParser(string) error: \"{0}\"", ex);
                return false;
            }
        }

        public static bool WriteFutureSummary(string fileName, List<CMEFuturesSummary> futuresSummaries)
        {
            try
            {
                using (var fs = new StreamWriter(fileName, false))
                {
                    fs.WriteLine(string.Format("Ticker{0}VolumeRTH{0}VolumeGlobex{0}OI", (char)9));
                    foreach (var sum in futuresSummaries)
                    {
                        var line = string.Format("{1}{0}{2}{0}{3}{0}{4}", (char) 9,
                                                 sum.Ticker.SpotSymbol, sum.VolumeRTH, sum.VolumeGlobex, sum.OI);
                        fs.WriteLine(line);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в WriteFutureSummary", ex);
                return false;
            }            
        }

        [Obsolete("Session num might be a string")]
        private static bool GetSectionNumFromFileName(string fileName, out int sectionNum)
        {
            // Section39_Euro_FX_And_Cme$Index_Options_2011041.txt
            sectionNum = 0;
            var fName = Path.GetFileNameWithoutExtension(fileName);
            if (string.IsNullOrEmpty(fName)) return false;
            if (fName.Length < 15) return false;
            fName = fName.Substring(7);
            var indexSplit = fName.IndexOf('_');
            if (indexSplit < 0) return false;
            var numStr = fName.Substring(0, indexSplit);            
            if (!int.TryParse(numStr, out sectionNum)) return false;
            return true;
        }       
    }    
}

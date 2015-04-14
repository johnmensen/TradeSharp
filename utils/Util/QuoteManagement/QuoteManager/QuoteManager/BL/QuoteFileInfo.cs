using System;
using System.IO;
using System.Text;
using Entity;
using MTS.Live.Util;

namespace QuoteManager.BL
{
    public class QuoteFileInfo
    {
	    public DateTime EndDate { get; set; }

	    public string FullPath { get; set; }

	    public long Size { get; set; }

	    public DateTime StartDate { get; set; }

	    public string TickerName { get; set; }

        public static void GetFirstAndLastFileDates(string fileName,
                out DateTime? dateFirst, out DateTime? dateLast, out bool endsUpNewLine)
        {
            dateFirst = null;
            dateLast = null;
            endsUpNewLine = false;
            if (!File.Exists(fileName)) return;

            using (var sr = new StreamReaderLog(fileName))
            {
                // получить первую строку
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line)) continue;
                    try
                    {
                        if (!dateFirst.HasValue) dateFirst = DateTime.ParseExact(line, "ddMMyyyy", CultureProvider.Common);
                        else
                        {
                            var parts = line.Split(' ');
                            var nH = int.Parse(parts[0].Substring(0, 2));
                            var nM = int.Parse(parts[0].Substring(2));
                            dateFirst = dateFirst.Value.AddMinutes(nM + nH * 60);
                            break;
                        }
                    }
                    catch
                    {
                        return;
                    }
                }
                if (sr.EndOfStream) return;

                // получить последнюю строку                
                var strLast = ReadEndTokens(sr.BaseStream, 1500, Encoding.ASCII, "\n");
                if (!string.IsNullOrEmpty(strLast))
                    if (strLast[strLast.Length - 1] == '\n')
                        endsUpNewLine = true;
                var quoteLines = strLast.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                int? lastHour = null, lastMin = null;
                for (var i = quoteLines.Length - 1; i >= 0; i--)
                {
                    var quoteLineParts = quoteLines[i].Split(' ');
                    if (quoteLineParts.Length == 3 && lastHour.HasValue == false)
                    {
                        lastHour = int.Parse(quoteLineParts[0].Substring(0, 2));
                        lastMin = int.Parse(quoteLineParts[0].Substring(2));
                        continue;
                    }
                    if (quoteLineParts.Length == 1)
                    {
                        dateLast = DateTime.ParseExact(quoteLines[i], "ddMMyyyy", CultureProvider.Common);
                        if (lastHour.HasValue)
                            dateLast = dateLast.Value.AddMinutes(lastMin.Value + lastHour.Value * 60);
                        break;
                    }
                }
            }
        }

        private static string ReadEndTokens(Stream fs, Int64 numberOfTokens, Encoding encoding, string tokenSeparator)
        {
            var sizeOfChar = encoding.GetByteCount("\n");
            var buffer = encoding.GetBytes(tokenSeparator);

            var tokenCount = 0;
            var endPosition = fs.Length / sizeOfChar;
            for (var position = sizeOfChar; position < endPosition; position += sizeOfChar)
            {
                fs.Seek(-position, SeekOrigin.End);
                fs.Read(buffer, 0, buffer.Length);
                if (encoding.GetString(buffer) == tokenSeparator)
                {
                    tokenCount++;
                    if (tokenCount == numberOfTokens)
                    {
                        var returnBuffer = new byte[fs.Length - fs.Position];
                        fs.Read(returnBuffer, 0, returnBuffer.Length);
                        return encoding.GetString(returnBuffer);
                    }
                }
            }
            // handle case where number of tokens in file is less than numberOfTokens         
            fs.Seek(0, SeekOrigin.Begin);
            buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);
            return encoding.GetString(buffer);
        }

	    public static QuoteFileInfo ReadFile(string path)
	    {
		    DateTime? first;
		    DateTime? last;
		    bool endsNewLine;
		    var quoteFileInfo = new QuoteFileInfo {FullPath = path};
	    
            var qfi = quoteFileInfo;
		    var fi = new FileInfo(path);
		    qfi.Size = fi.Length;
		    qfi.TickerName = Path.GetFileNameWithoutExtension(path);
		
            GetFirstAndLastFileDates(path, out first, out last, out endsNewLine);
		    if (!first.HasValue || !last.HasValue) return null;

            quoteFileInfo.StartDate = first.Value;
            quoteFileInfo.EndDate = last.Value;
		    return qfi;
	    }	
    }
}

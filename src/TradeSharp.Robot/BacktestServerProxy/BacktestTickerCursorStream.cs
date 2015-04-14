using System;
using System.Collections.Generic;
using System.IO;
using Entity;

namespace TradeSharp.Robot.BacktestServerProxy
{
    /// <summary>
    /// в текущей реализации поток котировок "сидит" в ОП:
    /// хранится список котировок, по которому "указатель" (индекс)
    /// смещается вперед по мере теста
    /// </summary>
    public class BacktestTickerCursorStream : IDisposable
    {
        public string Ticker { get; private set; }

        public CandleData currentCandle, nextCandle;

        private DateTime startTime;
        
        private StreamReader stream;

        private DateTime? fileDate;

        private int precision = 0;

        private string fileName;

        public BacktestTickerCursorStream(string quoteFolder, string ticker)
        {
            Ticker = ticker;
            precision = DalSpot.Instance.GetPrecision10(Ticker);
            fileName = quoteFolder.TrimEnd('\\') + "\\" + Ticker + ".quote";
            if (string.IsNullOrEmpty(fileName))
                return;
            if (!File.Exists(fileName))
                return;

            stream = new StreamReader(fileName);
            Reset();
        }

        public void Close()
        {
            if (stream == null)
                return;
            stream.Close();
        }

        public bool MoveToTime(DateTime time, out CandleData candle)
        {
            candle = null;
            if (stream == null) return false;
            if (currentCandle == null) return false;
            if (time < currentCandle.timeClose)
            {
                stream.BaseStream.Position = 0;
                stream.DiscardBufferedData();  
                Reset();
                if (currentCandle.timeClose > time)
                    return false;
            }

            while (currentCandle != null)
            {
                if (nextCandle != null && nextCandle.timeClose > time)
                {
                    candle = currentCandle;
                    return true;
                }
                currentCandle = nextCandle;
                nextCandle = ReadCandle(nextCandle);
            }
            return false;
        }

        public CandleData GetCurQuote()
        {
            return currentCandle == null ? null : new CandleData(currentCandle);
        }

        public DateTime? GetNextTime()
        {
            return nextCandle == null ? (DateTime?)null : nextCandle.timeClose;
        }

        public void MoveNext()
        {
            if (stream == null || nextCandle == null)
                return;
            currentCandle = nextCandle;
            nextCandle = ReadCandle(nextCandle);
        }

        public void Dispose()
        {
            Close();
        }

        private void Reset()
        {
            fileDate = null;
            currentCandle = null;
            nextCandle = null;
            currentCandle = ReadCandle(null);
            
            if (currentCandle == null)
            {
                stream.Close();
                stream = null;
                currentCandle = null;
                fileName = null;
                return;
            }

            startTime = currentCandle.timeClose;
            nextCandle = ReadCandle(currentCandle);
        }

        private CandleData ReadCandle(CandleData candle)
        {
            while (true)
            {
                if (stream.EndOfStream)
                    return null;

                var fileLine = stream.ReadLine();
                if (string.IsNullOrEmpty(fileLine))
                    continue;
                candle = CandleData.ParseLine(fileLine, ref fileDate, precision, ref candle);
                if (candle != null)
                    break;
            }
            return candle;
        }
    }

    //class BacktestTickerCursorStreamOld
    //{
    //    public string Ticker { get; private set; }
    //    private StreamReader reader;
    //    private const int BufferSize = 50;
    //    private List<CandleData> buffer = new List<CandleData>();
    //    private int bufferPosition = -1;
    //    private DateTime? curDate;
    //    private bool readToEnd;
    //    private int precision;

    //    public BacktestTickerCursorStreamOld(string fileName, string ticker)
    //    {
    //        Ticker = ticker;
    //        precision = DalSpot.Instance.GetPrecision10(ticker);
    //        reader = new StreamReader(fileName);
    //    }

    //    public void Close()
    //    {
    //        if (reader != null) reader.Close();
    //    }

    //    public bool MoveToTime(DateTime time, out CandleData quote)
    //    {
    //        quote = null;
    //        if (reader == null) return false;
    //        // читать котировки из файла до даты "time"
    //        while (true)
    //        {
    //            var nextQuote = ReadNextQuoteFromStream();
    //            if (nextQuote == null)
    //            {// дошли до конца файла
    //                reader = null;
    //                return false;
    //            }

    //            if (nextQuote.timeOpen < time) continue;
    //            quote = nextQuote;
    //            buffer.Add(quote);
    //            bufferPosition = 0;

    //            // зачитать N строк в буфер
    //            FillBuffer();

    //            return true;
    //        }
    //    }

    //    public CandleData GetCurQuote()
    //    {
    //        if (bufferPosition < 0 || bufferPosition >= buffer.Count) return null;
    //        return buffer[bufferPosition];
    //    }

    //    public DateTime? GetNextTime()
    //    {
    //        if (bufferPosition < 0) return null;
    //        if (bufferPosition >= buffer.Count - 1) return null;
    //        var time = buffer[bufferPosition + 1].timeOpen;

    //        if (bufferPosition == buffer.Count - 2 && !readToEnd)
    //        {// перезаполнить буфер
    //            buffer = buffer.GetRange(buffer.Count - 2, 2);
    //            FillBuffer();
    //            bufferPosition = 0;
    //        }

    //        return time;
    //    }

    //    public void MoveNext()
    //    {
    //        bufferPosition++;
    //    }

    //    private void FillBuffer()
    //    {
    //        var quoteRead = false;
    //        while (buffer.Count < BufferSize)
    //        {
    //            var bufQuote = ReadNextQuoteFromStream();
    //            if (bufQuote == null)
    //            {
    //                reader = null;
    //                break;
    //            }
    //            quoteRead = true;
    //            buffer.Add(bufQuote);
    //        }
    //        if (!quoteRead && buffer.Count < BufferSize)
    //            readToEnd = true;
    //    }

    //    private CandleData ReadNextQuoteFromStream()
    //    {
    //        if (reader == null)
    //            return null;

    //        CandleData previousCandle = null;
    //        while (!reader.EndOfStream)
    //        {
    //            var line = reader.ReadLine();
    //            if (string.IsNullOrEmpty(line))
    //                continue;

    //            var candle = CandleData.ParseLine(line, ref curDate, precision, ref previousCandle);
    //            if (candle == null)
    //                continue;
                
    //            return candle;
    //        }
    //        return null;
    //    }
    //}
}

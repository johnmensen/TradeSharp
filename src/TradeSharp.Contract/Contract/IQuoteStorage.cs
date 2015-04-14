using System;
using System.Collections.Generic;
using System.ServiceModel;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Contract.Contract
{
    [ServiceContract]
    public interface IQuoteStorage
    {
        [OperationContract(IsOneWay = false)]
        PackedCandleStream GetMinuteCandlesPacked(string symbol, DateTime start, DateTime end);

        [OperationContract(IsOneWay = false)]
        Dictionary<string, DateSpan> GetTickersHistoryStarts();

        [OperationContract(IsOneWay = false)]
        Dictionary<string, QuoteData> GetQuoteData();

        [OperationContract(IsOneWay = false)]
        PackedCandleStream GetMinuteCandlesPackedFast(string symbol, List<Cortege2<DateTime, DateTime>> intervals);
    }

    [Serializable]
    public class DateSpan
    {
        public DateTime start;
        public DateTime end;

        public DateSpan() {}

        public DateSpan(DateTime start, DateTime end)
        {
            this.start = start;
            this.end = end;
        }

        public DateSpan(DateSpan src)
        {
            start = src.start;
            end = src.end;
        }

        public bool IsIn(DateTime time)
        {
            return start <= time && time <= end;
        }

        public bool AreEqual(DateSpan b)
        {
            return start == b.start && end == b.end;
        }

        public bool AreEqual(DateTime bStart, DateTime bEnd)
        {
            return start == bStart && end == bEnd;
        }

        public double TotalSeconds
        {
            get { return (end - start).TotalSeconds; }
        }

        public double TotalMinutes
        {
            get { return (end - start).TotalMinutes; }
        }

        public double TotalHours
        {
            get { return (end - start).TotalHours; }
        }

        public double TotalDays
        {
            get { return (end - start).TotalDays; }
        }

        public override string ToString()
        {
            return string.Format("{0:dd.MM.yyyy HH:mm}, {1:0.##} minutes",
                                 start, TotalMinutes);
        }
    }
}
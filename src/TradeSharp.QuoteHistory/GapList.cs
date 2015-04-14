using System.Collections.Generic;
using System.Linq;

namespace TradeSharp.QuoteHistory
{
    /// группа гэпов
    /// гэпы группируются для отправки одного запроса с указанием всей группы
    /// при этом должны выполняться требования:
    /// ограничение на запрос: MaxGapCount
    /// ограничение на ожидаемый ответ: MaxTotalLengthHours
    public class GapList
    {
        public readonly List<GapInfo> Gaps = new List<GapInfo>();

        private double totalLengthDays;

        private readonly int maxGapCount;

        private readonly int maxTotalLengthDays;

        /// группирование гэпов; применятся для работы с минутными свечками
        public static List<GapList> CreateGapList(List<GapInfo> gaps, int maxGapListLengthDays = 3, int maxGapCount = 50)
        {
            var result = new List<GapList>();
            foreach (var gap in gaps)
            {
                var requestStart = gap.start;
                while (requestStart < gap.end)
                {
                    if(result.Count == 0)
                        result.Add(new GapList(maxGapListLengthDays, maxGapCount));
                    var gapList = result.Last();
                    if (gapList.GetFreeLengthDays() < 0.01) // 0.01 принято за минимально допустимое GetFreeLengthDays
                    {
                        result.Add(new GapList(maxGapListLengthDays, maxGapCount));
                        gapList = result.Last();
                    }
                    var requestEnd = requestStart.AddDays(gapList.GetFreeLengthDays()).AddMinutes(-1);
                    if (requestEnd > gap.end)
                        requestEnd = gap.end;
                    gapList.AddGap(new GapInfo {start = requestStart, end = requestEnd});
                    requestStart = requestEnd.AddMinutes(1);
                }
            }
            return result;
        }

        public GapList(int maxTotalLengthDays = 3, int maxGapCount = 50)
        {
            this.maxTotalLengthDays = maxTotalLengthDays;
            this.maxGapCount = maxGapCount;
        }

        public bool CanAddGap(GapInfo gap)
        {
            if (Gaps.Count == 0)
                return true;
            if (Gaps.Count >= maxGapCount)
                return true;
            var newLength = totalLengthDays + (gap.end - gap.start).TotalDays;
            return newLength <= maxTotalLengthDays;
        }

        public double GetFreeLengthDays()
        {
            if (Gaps.Count == 0)
                return maxTotalLengthDays;
            return maxTotalLengthDays - totalLengthDays;
        }

        public void AddGap(GapInfo gap)
        {
            Gaps.Add(gap);
            totalLengthDays += (gap.end - gap.start).TotalDays;
        }

        public override string ToString()
        {
            var result = "{" + string.Format("{0:2f} minutes in {1} gap(s): ", totalLengthDays * 24, Gaps.Count);
            for (var i = 0; i < Gaps.Count; i++)
            {
                result += Gaps[i].ToString();
                if (i != Gaps.Count - 1)
                    result += ", ";
            }
            return result + "}";
        }
    }
}

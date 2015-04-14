using System;
using System.Collections.Generic;

namespace TradeSharp.QuoteHistory
{
    public struct GapInfo
    {
        public enum GapStatus
        {
            Gap = 0, Filled, PartiallyFilled, FailedToFill
        }

        public DateTime start, end;
        public GapStatus status;

        /// <summary>
        /// "склеивает" серию стоящих рядом гэпов, если расстояние между ними меньше
        /// maxIntervalBetweenMinutes минут
        /// </summary>
        public static void StickSmallGaps(ref List<GapInfo> gaps, int maxIntervalBetweenMinutes)
        {
            if (gaps.Count <= 1) return;

            var resulted = new List<GapInfo> {gaps[0]};

            for (var i = 1; i < gaps.Count; i++)
            {
                var delta = (gaps[i].start - resulted[resulted.Count - 1].end).TotalMinutes;
                if (delta <= maxIntervalBetweenMinutes)
                {
                    resulted[resulted.Count - 1] = new GapInfo
                        {
                            start = resulted[resulted.Count - 1].start,
                            end = gaps[i].end,
                            status = resulted[resulted.Count - 1].status
                        };
                    continue;
                }
                resulted.Add(gaps[i]);
            }
            gaps = resulted;
        }

        public override string ToString()
        {
            var result = string.Format("{0:dd.MM.yyyy HH:mm}, {1:0.##} minutes",
                                 start, (end - start).TotalMinutes);
            switch (status)
            {
                case GapStatus.Filled:
                    result += ", Filled";
                    break;
                case GapStatus.PartiallyFilled:
                    result += ", PartiallyFilled";
                    break;
                case GapStatus.FailedToFill:
                    result += ", FailedToFill";
                    break;
            }
            return result;
        }
    }
}

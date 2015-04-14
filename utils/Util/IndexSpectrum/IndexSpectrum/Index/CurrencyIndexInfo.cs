using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using FXI.Client.MtsBase.Common;
using CurrencyStream = IndexSpectrum.Index.CurrencyStream;

namespace IndexSpectrum.Index
{
    public class CurrencyIndexInfo
    {
        private int momentumPeriod = 1;
        public int MomentumPeriod
        {
            get { return momentumPeriod; }
            set
            {
                momentumPeriod = value;
                if (prevVector != null)
                    for (var i = 0; i < prevVector.Length; i++)
                        prevVector[i] = new RestrictedQueue<decimal>(momentumPeriod);
            }
        }

        public readonly string[] pairs;
        public readonly int[] signs;
        public readonly decimal[] weights;
        /// <summary>
        /// значения индекса, расчитанные до конца интервала
        /// </summary>
        public readonly List<decimal> forwardIndexArray = new List<decimal>();
        /// <summary>
        /// конец интервала, на котором имеются форвардные данные
        /// </summary>
        public DateTime? forwardArrayEndTime;

        private readonly RestrictedQueue<decimal>[] prevVector;

        /// <summary>
        /// пока более percentStreamToContinue потоков не в состоянии EoF, флаг EndOfStream не взводится
        /// </summary>
        public int percentStreamToContinue = 10;

        public bool EndOfStream { get; private set; }

        private bool sewDiffParts = false;
        private int? prevPairCount;

        /// <param name="str">строка вида "eur# gbp# chf jpy cad aud# nzd# /*коментарий*/"</param>        
        public CurrencyIndexInfo(string str)//, bool sewDiffParts)
        {
            // <param name="sewDiffParts">если true - границы индекса, при появлении новой пары "сшиваются"</param>
            //this.sewDiffParts = sewDiffParts;
            // убрать коментарии
            while (true)
            {
                var startIndex = str.IndexOf("/*");
                if (startIndex < 0) break;
                var endIndex = str.IndexOf("*/", startIndex);
                if (endIndex < 0) break;

                var firstPart = str.Substring(0, startIndex);
                firstPart = firstPart ?? "";
                var lastPart = str.Substring(endIndex + 2, str.Length - endIndex - 2);
                lastPart = lastPart ?? "";
                str = firstPart + lastPart;
            }

            // имя - знак - вес
            var data = new List<Cortege3<string, int, decimal>>();
            var parts = str.Split(' ', ',', ';', (char)9);
            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part)) continue;
                string symbol;
                int sign;
                decimal weight;
                SplitSymbolInfo(part, out symbol, out sign, out weight);
                data.Add(new Cortege3<string, int, decimal> { a = symbol, b = sign, c = weight });
            }
            pairs = data.Select(p => p.a).ToArray();
            signs = data.Select(p => p.b).ToArray();
            weights = data.Select(p => p.c).ToArray();

            prevVector = new RestrictedQueue<decimal>[pairs.Length];
            for (var i = 0; i < prevVector.Length; i++)
                prevVector[i] = new RestrictedQueue<decimal>(momentumPeriod);
        }

        private static readonly Regex regexWeight = new Regex("[.\\d]+");
        private static readonly Regex regexText = new Regex("\\D+");
        /// <param name="str">gbp или eur или chf0.618 или jpy0.52#</param>        
        /// <param name="smb">jpy</param>
        /// <param name="sign">-1</param>
        /// <param name="weight">0.52</param>
        private static void SplitSymbolInfo(string str, out string smb, out int sign, out decimal weight)
        {
            sign = 1;
            weight = 1;
            // решетка - инверсия?
            if (str[str.Length - 1] == '#')
            {
                str = str.Substring(0, str.Length - 1);
                sign = -1;
            }
            // вес
            var wm = regexWeight.Match(str);
            if (wm != null)
            {
                if (!string.IsNullOrEmpty(wm.Value))
                    weight = decimal.Parse(wm.Value, CultureInfo.InvariantCulture);
            }
            smb = regexText.Match(str).Value;
        }

        public CurrencyIndexInfo(string[] _pairs, int[] _signs)
        {
            pairs = _pairs;
            signs = _signs;

            prevVector = new RestrictedQueue<decimal>[pairs.Length];
            for (var i = 0; i < prevVector.Length; i++)
                prevVector[i] = new RestrictedQueue<decimal>(momentumPeriod);
            // веса по-умолчанию
            weights = new decimal[pairs.Length];
            for (var i = 0; i < weights.Length; i++) weights[i] = 1;
        }        

        public double CalculateIndexMultiplicative(DateTime time,
                                                   Dictionary<string, CurrencyStream> curStream)
        {
            double index = 1;
            int countOpenStream = 0;
            var multipliers = new double[pairs.Length];
            for (int i = 0; i < pairs.Length; i++)
            {
                if (!curStream[pairs[i]].EndOfFile) countOpenStream++;
                var curQuote = curStream[pairs[i]].GetQuoteBid(time);
                if (curQuote == 0) continue;
                multipliers[i] = (double) curQuote;
            }
            if (!EndOfStream)
            {
                int percentOpen = 100*countOpenStream/pairs.Length;
                if (percentOpen < percentStreamToContinue) EndOfStream = true;
            }

            // скорректировать веса            
            //double wSum = 0;
            //var quoteCount = 0;
            //for (int i = 0; i < pairs.Length; i++)
            //{
            //    if (multipliers[i] > 0)
            //    {
            //        quoteCount++;
            //        wSum += (double) weights[i];
            //    }
            //}
            //if (wSum == 0) return 1;
            for (int i = 0; i < pairs.Length; i++)
            {
                var weight = (double)weights[i]; // / wSum;
                if (multipliers[i] > 0) index = index * Math.Pow(multipliers[i], weight * signs[i]);
            }

            // скорректировать коэффициент
            /*if (!prevPairCount.HasValue)
                prevPairCount = quoteCount;
            else
            {
                
            }*/

            return index;
        }

        public void Clear()
        {
            foreach (var vect in prevVector)
                vect.Clear();
            forwardIndexArray.Clear();
            forwardArrayEndTime = null;
        }
    }           
}
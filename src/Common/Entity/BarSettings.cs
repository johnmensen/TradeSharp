using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml;
using TradeSharp.Util;

namespace Entity
{
    /// <summary>
    /// Класс описывает настройки свечей
    /// </summary>
    [Serializable]
    public class BarSettings
    {
        private static readonly char[] splitChars = new [] { ';', '#' };

        public string Title { get; set; }
        /// <summary>
        /// Начало отсчета, минут
        /// </summary>
        public int StartMinute { get; set; }
        
        private List<int> intervals;
        /// <summary>
        /// Интервалы свечек, минут
        /// </summary>
        public List<int> Intervals
        {
            get { return intervals; }
            set { intervals = value; }
        }

        /// <summary>
        /// true для свечи m1
        /// свечи меньшей продолжительности в терминале не предусмотрено
        /// </summary>
        public bool IsAtomic
        {
            get { return intervals.Count == 1 && intervals[0] == 1; }
        }

        /// <symmary>
        /// 
        /// </symmary>
        public string TimeDescription
        {
            get
            {
                var result = "+" + StartMinute + "; ";
                for (var i = 0; i < intervals.Count; i++)
                {
                    result += intervals[i];
                    if (i != intervals.Count - 1)
                        result += "; ";
                }
                return result;
            }
        }

        /// <summary>
        /// true - пользовательская настройка для свечек
        /// ее можно менять и удалять
        /// </summary>
        public bool IsUserDefined { get; set; }

        public int TotalMinutes
        {
            get { return intervals.Sum(); }
        }

        public BarSettings()
        {
            intervals = new List<int>();
            IsUserDefined = false;
        }
        public BarSettings(string tagStr)
            : this(tagStr, string.Empty)
        {
        }
        public BarSettings(string tagStr, string separator)
        {
            intervals = new List<int>();
            ReadFromTagString(tagStr, separator);
            IsUserDefined = false;
        }
        public BarSettings(BarSettings settings)
        {
            Title = settings.Title;
            intervals = new List<int>();
            StartMinute = settings.StartMinute;
            foreach (var span in settings.Intervals)
            {
                intervals.Add(span);
            }
            IsUserDefined = false;
        }

        public static BarSettings TryParseString(string str)
        {
            try
            {
                return new BarSettings(str);
            }
            catch
            {
                return null;               
            }
        }

        public void ReadFromTagString(string tagStr, string separator)
        {
            var parts = 
                !string.IsNullOrEmpty(separator) 
                    ? tagStr.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries)
                    : tagStr.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
            StartMinute = int.Parse(parts[0]);
            if (StartMinute < 0) throw new Exception("Bar start minute is <= 0");
            intervals.Clear();
            for (var i = 1; i < parts.Length; i++)
            {
                var interval = int.Parse(parts[i]); 
                if (interval <= 0) throw new Exception("Bar interval is <= 0");
                intervals.Add(interval);
            }
        }
        /// <summary>
        /// Вернуть строку, описывающую сущность
        /// (используется как tag в кеше)
        /// </summary>        
        public string GetTag(string separator)
        {
            if (string.IsNullOrEmpty(separator)) separator = ";#";
            var tag = new StringBuilder();
            tag.Append(StartMinute);
            tag.Append(separator);
            foreach (var inter in intervals)
            {
                tag.Append(inter);
                tag.Append(separator);
            }
            return tag.ToString();
        }

        public override string ToString()
        {
            return GetTag(string.Empty);
        }

        public string ToString(string separator)
        {
            return GetTag(separator);
        }

        public XmlNode ToXMLNode(XmlDocument owner)
        {
            var node = owner.CreateElement("BarSettings");
            var startTimeAttr = node.Attributes.Append(owner.CreateAttribute("startMinute"));
            startTimeAttr.Value = StartMinute.ToString();

            var intervalsStr = new StringBuilder();
            var first = true;
            foreach (var inter in intervals)
            {
                if (!first) intervalsStr.Append(" ");
                first = false;
                intervalsStr.Append(inter);
            }
            var intervalsAttr = node.Attributes.Append(owner.CreateAttribute("intervals"));
            intervalsAttr.Value = intervalsStr.ToString();

            return node;
        }

        public void FromXMLNode(XmlElement node)
        {
            StartMinute = int.Parse(node.Attributes["startMinute"].Value);
            intervals.Clear();
            var inters = node.Attributes["intervals"].Value;
            if (string.IsNullOrEmpty(inters)) return;
            var parts = inters.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var inter in parts)
            {
                intervals.Add(int.Parse(inter));
            }
        }

        public void FromDocumentNode(XmlDocument doc)
        {
            var settingsNodes = doc.SelectNodes("/*/BarSettings");
            if (settingsNodes == null) return;
            if (settingsNodes.Count == 0) return;
            FromXMLNode((XmlElement)settingsNodes[0]);
        }

        public override bool Equals(object obj)
        {
            if (obj is BarSettings == false) return false;
            return Equals((BarSettings) obj);
        }

        public static bool operator ==(BarSettings a, BarSettings b)
        {
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(BarSettings a, BarSettings b)
        {
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return true;
            return !a.Equals(b);
        }

        public bool Equals(BarSettings tf)
        {
            if (tf == null) return false;
            if (StartMinute != tf.StartMinute) return false;
            if (intervals.Count != tf.intervals.Count) return false;
            for (var i = 0; i < intervals.Count; i++)
                if (intervals[i] != tf.intervals[i]) return false;
            return true;
        }

        public override int GetHashCode()
        {
            var code = StartMinute;
            if (intervals.Count > 0) code *= intervals[0];
            return code;
        }

        /// <summary>
        /// Возвращает время свечи, отстоящей от времени timeFrom на количество свечек candleDistance в направлении dir
        /// </summary>
        /// <param name="candleDistance"></param>
        /// <param name="dir"></param>
        /// <param name="timeFrom"></param>
        /// <returns></returns>
        public DateTime GetDistanceTime(int candleDistance, int dir, DateTime timeFrom)
        {
            // определяем попало ли время отсчета на начало свечи, если нет то сдвигаемся на временную границу между свечами
            var currTime = GetQuoteTime(timeFrom, dir);
            
            var interval = 0;
            for (var i = 0; i < candleDistance; i++)
            {
                currTime = dir > 0 ? GetNextQuote(currTime, interval++) : GetPrevQuote(currTime, interval++);
                if (interval == Intervals.Count)
                    interval = 0;
            }
            return currTime;

        }

        /// <summary>
        /// Возвращает время свечи, на чье тело попадает время from
        /// </summary>
        /// <param name="from"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        private DateTime GetQuoteTime(DateTime from, int dir)
        {
            //var part = Math.Truncate((from.Minute + from.Hour * 60)/(double)Intervals[0]);
            var ret = from.Date;
            ret = ret.AddMinutes(StartMinute);
            var index = 0;
            while (ret.AddMinutes(intervals[index]) < from)
            {
                // листаем свечки пока не найдем тело свечи, в котором находится время from
                ret = ret.AddMinutes(intervals[index++]);
                if (index == Intervals.Count)
                    index = 0;
            }
            // еще сдвигаем если направление задано вперед
            if (dir > 0)
                ret = ret.AddMinutes(intervals[index]);

            while (!WorkingDay.Instance.IsWorkingDay(ret))
            {
                ret = ret.AddMinutes(dir > 0 ? Intervals[index++] : -Intervals[index++]);
                if (index == Intervals.Count)
                    index = 0;
            }
            return ret;
        }
        /// <summary>
        /// получить время следующей свечи, с учетом выходных. входное время должно быть началом текущей свечи
        /// </summary>
        /// <param name="from"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        private DateTime GetNextQuote(DateTime from, int interval)
        {
            var resDt = from.AddMinutes(Intervals[interval]);
            var index = interval;
            while (!WorkingDay.Instance.IsWorkingDay(resDt))
            {
                resDt = resDt.AddMinutes(Intervals[index++]);
                if (index == Intervals.Count)
                    index = 0;
            }
            return resDt;
        }

        /// <summary>
        /// получить время предыдущей свечи, с учетом выходных. входное время должно быть началом текущей свечи
        /// </summary>
        /// <param name="from"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        private DateTime GetPrevQuote(DateTime from, int interval)
        {
            var resDt = from.AddMinutes(-Intervals[interval]);
            var index = interval;
            while (!WorkingDay.Instance.IsWorkingDay(resDt))
            {
                resDt = resDt.AddMinutes(-Intervals[index++]);
                if (index == Intervals.Count)
                    index = 0;
            }
            return resDt;
        }
    }
}

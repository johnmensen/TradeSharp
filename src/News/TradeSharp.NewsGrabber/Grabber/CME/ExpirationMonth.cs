using System;
using TradeSharp.Util;

namespace TradeSharp.NewsGrabber.Grabber.CME
{
    public class ExpirationMonth
    {
        private int month;
        public int Month
        {
            get { return month; }
            set
            {
                if (value < 1 || value > 12)
                {
                    Logger.ErrorFormat("ExpirationMonth: month value is {0}", value);
                    throw new ArgumentOutOfRangeException("Month",
                                                          "ExpirationMonth: month whould be [1..12]");
                }
                month = value;
            }
        }

        public int Year { get; set; }

        private static readonly string[] monthCodes = 
            new []
                {
                    "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "DEC"
                };
        /// <summary>
        /// MAR11
        /// </summary>        
        public override string ToString()
        {
            var strYear = Year.ToString();
            strYear = strYear.Substring(strYear.Length - 2, 2);
            return string.Format("{0}{1}", monthCodes[month - 1], strYear);
        }
        public static ExpirationMonth Parse(string str)
        {
            if (string.IsNullOrEmpty(str)) return null;
            if (str.Length != 5) return null;
            var partMonth = str.Substring(0, 3);
            int month = ParseMonth(partMonth, false);
            if (month < 0) return null;
            var yearPart = str.Substring(3);
            int year;
            if (!int.TryParse(yearPart, out year)) return null;
            var yearFull = year > 50 ? 1900 + year : 2000 + year;
            return new ExpirationMonth { Month = month, Year = yearFull };
        }

        /// <param name="str">Mar | MAR</param>
        /// <param name="ignoreCase">MAR is ok if ignoreCase is off</param>
        /// <returns>1 - 12, -1 if not found</returns>
        public static int ParseMonth(string str, bool ignoreCase)
        {
            if (ignoreCase) str = str.ToUpper();
            int month = -1;
            for (var i = 0; i < monthCodes.Length; i++)
            {
                if (str == monthCodes[i])
                {
                    month = i + 1;
                    break;
                }
            }
            return month;
        }
        
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is ExpirationMonth == false) return false;
            var em = (ExpirationMonth) obj;
            return em.Month == Month && em.Year == Year;
        }

        public override int GetHashCode()
        {
            return (Month - 1) + Year * 12;
        }

        public static bool operator ==(ExpirationMonth a, ExpirationMonth b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b)) return true;

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) return false;

            return a.Month == b.Month && a.Year == b.Year;
        }

        public static bool operator !=(ExpirationMonth a, ExpirationMonth b)
        {
            return !(a == b);
        }
    }    
}
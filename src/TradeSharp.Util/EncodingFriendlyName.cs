using System;
using System.Text;

namespace TradeSharp.Util
{
    public static class EncodingFriendlyName
    {
        public static string[] GetFriendlyNames()
        {
            return new[]
                       {
                           "UTF8", "UTF7", "UTF32", "Unicode", "ASCII",
                           "BigEndianUnicode", "1251"
                       };
        }

        public static Encoding GetEncodingByName(string encString, Encoding defaultEncoding)
        {
            if (encString == "UTF8") return Encoding.UTF8;
            if (encString == "UTF7") return Encoding.UTF7;
            if (encString == "UTF32") return Encoding.UTF32;
            if (encString == "Unicode") return Encoding.Unicode;
            if (encString == "ASCII") return Encoding.ASCII;
            if (encString == "BigEndianUnicode") return Encoding.BigEndianUnicode;
            if (encString == "1251") return Encoding.GetEncoding(1251);
            try
            {
                return Encoding.GetEncoding(encString);
            }
            catch (ArgumentException)
            {
                return defaultEncoding;
            }
        }

        public static string GetEncodingName(Encoding encoding)
        {
            if (encoding == Encoding.UTF8) return "UTF8";
            if (encoding == Encoding.UTF7) return "UTF7";
            if (encoding == Encoding.UTF32) return "UTF32";
            if (encoding == Encoding.Unicode) return "Unicode";
            if (encoding == Encoding.ASCII) return "ASCII";
            if (encoding == Encoding.BigEndianUnicode) return "BigEndianUnicode";
            if (encoding.CodePage == 1251) return "1251";

            return encoding.EncodingName;
        }
    }
}

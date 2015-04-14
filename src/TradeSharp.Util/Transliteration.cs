using System.Collections.Generic;
using System.Linq;

namespace TradeSharp.Util
{
    public enum TransliterationType
    {
        Gost,
        ISO
    }
    public static class Transliteration
    {
        private static readonly Dictionary<string, string> gost = new Dictionary<string, string>(); //ГОСТ 16876-71
        private static readonly Dictionary<string, string> iso = new Dictionary<string, string>(); //ISO 9-95

        public static string Front(string text)
        {
            return Front(text, TransliterationType.ISO);
        }
        public static string Front(string text, TransliterationType type)
        {
            string output = text;
            Dictionary<string, string> tdict = GetDictionaryByType(type);

            return tdict.Aggregate(output, (current, key) => current.Replace(key.Key, key.Value));
        }
        public static string Back(string text)
        {
            return Back(text, TransliterationType.ISO);
        }
        public static string Back(string text, TransliterationType type)
        {
            var output = text;
            var tdict = GetDictionaryByType(type);

            return tdict.Aggregate(output, (current, key) => current.Replace(key.Value, key.Key));
        }

        private static Dictionary<string, string> GetDictionaryByType(TransliterationType type)
        {
            Dictionary<string, string> tdict = iso;
            if (type == TransliterationType.Gost) tdict = gost;
            return tdict;
        }

        static Transliteration()
        {
            gost.Add("Є", "Eh");
            gost.Add("І", "I");
            gost.Add("і", "i");
            gost.Add("№", "#");
            gost.Add("є", "eh");
            gost.Add("А", "A");
            gost.Add("Б", "B");
            gost.Add("В", "V");
            gost.Add("Г", "G");
            gost.Add("Д", "D");
            gost.Add("Е", "E");
            gost.Add("Ё", "Jo");
            gost.Add("Ж", "Zh");
            gost.Add("З", "Z");
            gost.Add("И", "I");
            gost.Add("Й", "Jj");
            gost.Add("К", "K");
            gost.Add("Л", "L");
            gost.Add("М", "M");
            gost.Add("Н", "N");
            gost.Add("О", "O");
            gost.Add("П", "P");
            gost.Add("Р", "R");
            gost.Add("С", "S");
            gost.Add("Т", "T");
            gost.Add("У", "U");
            gost.Add("Ф", "F");
            gost.Add("Х", "Kh");
            gost.Add("Ц", "C");
            gost.Add("Ч", "CH");
            gost.Add("Ш", "Sh");
            gost.Add("Щ", "Shh");
            gost.Add("Ъ", "'");
            gost.Add("Ы", "Y");
            gost.Add("Ь", "");
            gost.Add("Э", "Eh");
            gost.Add("Ю", "Yu");
            gost.Add("Я", "Ya");
            gost.Add("а", "a");
            gost.Add("б", "b");
            gost.Add("в", "v");
            gost.Add("г", "g");
            gost.Add("д", "d");
            gost.Add("е", "e");
            gost.Add("ё", "jo");
            gost.Add("ж", "zh");
            gost.Add("з", "z");
            gost.Add("и", "i");
            gost.Add("й", "jj");
            gost.Add("к", "k");
            gost.Add("л", "l");
            gost.Add("м", "m");
            gost.Add("н", "n");
            gost.Add("о", "o");
            gost.Add("п", "p");
            gost.Add("р", "r");
            gost.Add("с", "s");
            gost.Add("т", "t");
            gost.Add("у", "u");

            gost.Add("ф", "f");
            gost.Add("х", "kh");
            gost.Add("ц", "c");
            gost.Add("ч", "ch");
            gost.Add("ш", "sh");
            gost.Add("щ", "shh");
            gost.Add("ъ", "");
            gost.Add("ы", "y");
            gost.Add("ь", "");
            gost.Add("э", "eh");
            gost.Add("ю", "yu");
            gost.Add("я", "ya");
            gost.Add("«", "");
            gost.Add("»", "");
            gost.Add("—", "-");

            iso.Add("Є", "Ye");
            iso.Add("І", "I");
            iso.Add("Ѓ", "G");
            iso.Add("і", "i");
            iso.Add("№", "#");
            iso.Add("є", "ye");
            iso.Add("ѓ", "g");
            iso.Add("А", "A");
            iso.Add("Б", "B");
            iso.Add("В", "V");
            iso.Add("Г", "G");
            iso.Add("Д", "D");
            iso.Add("Е", "E");
            iso.Add("Ё", "Yo");
            iso.Add("Ж", "Zh");
            iso.Add("З", "Z");
            iso.Add("И", "I");
            iso.Add("Й", "J");
            iso.Add("К", "K");
            iso.Add("Л", "L");
            iso.Add("М", "M");
            iso.Add("Н", "N");
            iso.Add("О", "O");
            iso.Add("П", "P");
            iso.Add("Р", "R");
            iso.Add("С", "S");
            iso.Add("Т", "T");
            iso.Add("У", "U");
            iso.Add("Ф", "F");
            iso.Add("Х", "X");
            iso.Add("Ц", "C");
            iso.Add("Ч", "Ch");
            iso.Add("Ш", "Sh");
            iso.Add("Щ", "Shh");
            iso.Add("Ъ", "'");
            iso.Add("Ы", "Y");
            iso.Add("Ь", "");
            iso.Add("Э", "E");
            iso.Add("Ю", "Yu");
            iso.Add("Я", "Ya");
            iso.Add("а", "a");
            iso.Add("б", "b");
            iso.Add("в", "v");
            iso.Add("г", "g");
            iso.Add("д", "d");
            iso.Add("е", "e");
            iso.Add("ё", "yo");
            iso.Add("ж", "zh");
            iso.Add("з", "z");
            iso.Add("и", "i");
            iso.Add("й", "j");
            iso.Add("к", "k");
            iso.Add("л", "l");
            iso.Add("м", "m");
            iso.Add("н", "n");
            iso.Add("о", "o");
            iso.Add("п", "p");
            iso.Add("р", "r");
            iso.Add("с", "s");
            iso.Add("т", "t");
            iso.Add("у", "u");
            iso.Add("ф", "f");
            iso.Add("х", "x");
            iso.Add("ц", "c");
            iso.Add("ч", "ch");
            iso.Add("ш", "sh");
            iso.Add("щ", "shh");
            iso.Add("ъ", "");
            iso.Add("ы", "y");
            iso.Add("ь", "");
            iso.Add("э", "e");
            iso.Add("ю", "yu");
            iso.Add("я", "ya");
            iso.Add("«", "");
            iso.Add("»", "");
            iso.Add("—", "-");
        }
    }
}
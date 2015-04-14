using System;
using System.Linq;

namespace TradeSharp.Client.BL.PaymentSystem
{
    static class WebMoneyValidator
    {
        public static bool WmidIsValid(string wmid)
        {
            return !string.IsNullOrEmpty(wmid) && wmid.Length == 12 && wmid.All(Char.IsDigit);
        }

        public static bool PusrseIdIsValid(string purseId)
        {
            return WmidIsValid(purseId);
        }

        public static string GetCorrectWMIDSampleStringWithSpecs()
        {
            return "Корректный WMID содержит 12 цифр";
        }

        public static string GetCorrectPurseIdSampleStringWithSpecs()
        {
            return "Корректный ID кошелька содержит 12 цифр";
        }
    }
}

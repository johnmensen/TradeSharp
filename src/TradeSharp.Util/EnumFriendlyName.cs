using System;
using System.Collections.Generic;
using System.Linq;

namespace TradeSharp.Util
{
    /// <summary>
    /// переводчик Enum-ов
    /// </summary>
    public static class EnumFriendlyName<T> where T : struct
    {
        private static readonly Dictionary<T, string> statDic = new Dictionary<T, string>();

        static EnumFriendlyName()
        {
            if (!typeof (T).IsSubclassOf(typeof (Enum)))
                return;
            if (!Localizer.UseCache)
                return;
            // формируем кэш
            var values = Enum.GetValues(typeof (T)).Cast<T>();
            foreach (var value in values)
                statDic.Add(value, Localizer.GetString(GetResourceName(value)));
        }

        private static string GetResourceName(T val)
        {
            return "Enum" + typeof (T).Name + val;
        }

        public static bool HasKey(T val)
        {
            return Localizer.UseCache ? statDic.ContainsKey(val) : Localizer.HasKey(GetResourceName(val));
        }

        /// <summary>
        /// Пример: пусть T = DealSide
        /// </summary>
        /// <param name="val">DealSide.Buy</param>
        /// <returns>Buy или Покупка ...</returns>
        /// строка в ресурсах - EnumDealSideBuy
        public static string GetString(T val)
        {
            if (Localizer.UseCache)
                return statDic.ContainsKey(val) ? statDic[val] : val.ToString();
            return Localizer.GetString(GetResourceName(val));
        }
    }

    // упакованное значение Enum-а;
    // получившийся объект представляется локализованной строкой
    public struct EnumItem<T> where T : struct
    {
        public static readonly List<EnumItem<T>> items = new List<EnumItem<T>>();

        // локализованный Enum
// ReSharper disable StaticFieldInGenericType
        private static readonly bool isLocalized;
// ReSharper restore StaticFieldInGenericType

        static EnumItem()
        {
            if (!typeof (T).IsSubclassOf(typeof (Enum)))
                return;
            items.AddRange(Enum.GetValues(typeof (T)).Cast<T>().Select(v => new EnumItem<T> {Value = v}));
            // проверяем возможность локализации
            if (items.Count == 0)
                return;
            isLocalized = Localizer.HasKey("Enum" + typeof (T).Name + items[0].Value);
        }

        public T Value;

        public override string ToString()
        {
            return isLocalized ? EnumFriendlyName<T>.GetString(Value) : Value.ToString();
        }
    }
}

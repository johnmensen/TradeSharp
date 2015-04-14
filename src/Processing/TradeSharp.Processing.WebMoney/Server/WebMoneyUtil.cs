using System;
using System.Collections.Generic;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;
using WebMoney.BasicObjects;

namespace TradeSharp.Processing.WebMoney.Server
{
    public class WebMoneyUtil
    {
        public static ulong companyPurseId;

        private static WebMoneyUtil instance;
        public static WebMoneyUtil Instance
        {
            get { return instance ?? (instance = new WebMoneyUtil()); }
        }

        /// <summary>
        /// Читает учётные данные "корпоративного" Web Money кошелька из метаданных или их app.comfig
        /// </summary>
        public Dictionary<string, Object> GetWmAccountSettings()
        {
            var result = new Dictionary<string, Object>();
            try
            {
                var sysMeta = TradeSharpDictionary.Instance.proxy.GetMetadataByCategory("PaymentSystem.WM");

                if (sysMeta != null && sysMeta.ContainsKey("WmKeeperKeyValue")
                    && sysMeta.ContainsKey("WmTargetPurseNumber")
                    && sysMeta.ContainsKey("WmId")
                    && sysMeta.ContainsKey("WmPurseCurrency"))
                {
                    Logger.Debug("Ищем  учётные данные 'корпоративного' Web Money кошелька в метаданных");
                    #region Парсим данные из метаданных

                    if (!(sysMeta["WmTargetPurseNumber"] is ulong))
                    {
                        Logger.Error("GetWmAccountSettings() - не удалось распарсить параметр WmTargetPurseNumber из метаданных");
                        return null;
                    }

                    if (!(sysMeta["WmId"] is ulong))
                    {
                        Logger.Error("GetWmAccountSettings() - не удалось распарсить параметр WmId из метаданных");
                        return null;
                    }

                    result.Add("WmKeeperKeyValue", sysMeta["WmKeeperKeyValue"]);
                    result.Add("WmTargetPurseNumber", sysMeta["WmTargetPurseNumber"]);
                    result.Add("WmId", sysMeta["WmId"]);
                    result.Add("WmPurseCurrency", sysMeta["WmPurseCurrency"]);
                    #endregion

                    companyPurseId = (ulong)sysMeta["WmTargetPurseNumber"];
                }
                else
                {
                    Logger.Debug("Ищем  учётные данные 'корпоративного' Web Money кошелька в файле app.config");
                    #region Парсим данные из файла app.config
                    result.Add("WmKeeperKeyValue", AppConfig.GetStringParam("WmKeeperKeyValue", ""));
                    result.Add("WmTargetPurseNumber", AppConfig.GetULongParam("WmTargetPurseNumber", 0));
                    result.Add("WmId", AppConfig.GetULongParam("WmId", 0));
                    result.Add("WmPurseCurrency", AppConfig.GetStringParam("WmPurseCurrency", "None"));

                    if ((ulong)result["WmTargetPurseNumber"] == 0)
                    {
                        Logger.Error("GetWmAccountSettings() - не удалось распарсить параметр WmTargetPurseNumber из файла app.config");
                        return null;
                    }

                    if ((ulong)result["WmId"] == 0)
                    {
                        Logger.Error("GetWmAccountSettings() - не удалось распарсить параметр WmId из файла app.config");
                        return null;
                    }
                    #endregion                 

                    companyPurseId = (ulong)result["WmTargetPurseNumber"];
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetWmAccountSettings()", ex);
                return null;
            }

            Logger.Info("Прочитаны учётные данные 'корпоративного' Web Money кошелька");
            foreach (var o in result)
            {
                Logger.Info(o.Key + " - " + o.Value);
            }

            return result.Count == 4 ? result : null;
        }        
        
        public static string CurrencyToStr(WmCurrency wmCurrency)
        {
            switch (wmCurrency)
            {
                case WmCurrency.None:
                    return string.Empty;
                case WmCurrency.Z:
                    return "USD"; //TODO эти значения стоит ситать из БД, а не хардкодить
                case WmCurrency.E:
                    return "EUR";
                case WmCurrency.R:
                    return "RUB";
                default:
                    return string.Empty;
            }
        }

        public static WmCurrency StrToCurrency(string currency)
        {
            switch (currency)
            {
                case "Z":
                    return WmCurrency.Z;
                case "E":
                    return WmCurrency.E;
                case "R":
                    return WmCurrency.R;
                default:
                    return WmCurrency.None;
            }
        }
    }
}

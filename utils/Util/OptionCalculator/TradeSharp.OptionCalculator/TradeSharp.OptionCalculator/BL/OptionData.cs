using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using TradeSharp.Util;

namespace TradeSharp.OptionCalculator.BL
{
    partial class OptionData
    {
        #region Настройки
        public DateTime Expiration { get; set; }

        public DateTime ValueDate { get; set; }

        public string QuoteFilePath { get; set; }

        public decimal Strike { get; set; }

        public decimal VanillaPrice { get; set; }

        public OptionType OptionType { get; set; }

        public OptionSide Side { get; set; }

        public bool RemoveTrend { get; set; }
        #endregion

        public static Action<string> logMessage;

        private static readonly string setsFilePath = ExecutablePath.ExecPath + "\\option.txt";

        public void SaveSettings()
        {
            try
            {
                using (var sw = new StreamWriter(setsFilePath, false, Encoding.UTF8))
                {
                    sw.Write(JsonConvert.SerializeObject(this));
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка сохранения файла настроек опциона", ex);
                throw;
            }
        }

        public List<string> GetErrorsInSettings()
        {
            var errors = new List<string>();
            if (Strike <= 0) 
                errors.Add(string.Format("Цена страйк должна быть строго больше 0"));
            if (Expiration < ValueDate)
                errors.Add("Время экспирации не может быть меньше времени сделки");
            if (string.IsNullOrEmpty(QuoteFilePath))
                errors.Add("Файл котировки не указан");
            else if (!File.Exists(QuoteFilePath))
                errors.Add("Указанный файл котировки не найден");
            return errors;
        }

        public static OptionData LoadLastSettings()
        {
            if (!File.Exists(setsFilePath))
                return MakeDefault();
            try
            {
                using (var sr = new StreamReader(setsFilePath, Encoding.UTF8))
                {
                    var str = sr.ReadToEnd();
                    return JsonConvert.DeserializeObject<OptionData>(str) ?? MakeDefault();
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("Ошибка загрузки настроек опциона", ex);
                return MakeDefault();
            }
        }

        private static OptionData MakeDefault()
        {
            return new OptionData
            {
                Expiration = DateTime.Now.Date,
                ValueDate = DateTime.Now.Date,
                OptionType = OptionType.European,
                Side = OptionSide.Call
            };
        }

        private void LogMessage(string format, params object[] values)
        {
            logMessage(string.Format(format, values));
        }
    }

    enum OptionType
    {
        European = 0,
        American
    }

    enum OptionSide
    {
        Call = 0,
        Put
    }
}

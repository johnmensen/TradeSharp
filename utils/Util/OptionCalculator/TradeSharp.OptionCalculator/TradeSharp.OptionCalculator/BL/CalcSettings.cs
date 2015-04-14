using System;
using System.IO;
using System.Text;
using AutoMapper;
using Newtonsoft.Json;
using TradeSharp.Util;

namespace TradeSharp.OptionCalculator.BL
{
    class CalcSettings
    {
        private static readonly Lazy<CalcSettings> instance = new Lazy<CalcSettings>(() => new CalcSettings());

        public static CalcSettings Instance
        {
            get { return instance.Value; }
        }

        private readonly string setsFilePath = ExecutablePath.ExecPath + "\\settings.txt";

        public int QuoteTimeOffset { get; set; }

        private string quoteFolder;

        public string QuoteFolder
        {
            get { return quoteFolder; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    value = value.TrimEnd('\\');
                quoteFolder = value;
            }
        }

        private decimal highPercent = 1.5M;

        public decimal HighPercent
        {
            get { return highPercent; }
            set { highPercent = value; }
        }

        private int iterationsCount = 50000;

        public int IterationsCount
        {
            get { return iterationsCount; }
            set { iterationsCount = value; }
        }

        static CalcSettings()
        {
            Mapper.CreateMap<CalcSettings, CalcSettings>();
        }

        private CalcSettings()
        {
            if (!File.Exists(setsFilePath))
            {
                InitDefault();
                return;
            }
            try
            {
                using (var sr = new StreamReader(setsFilePath, Encoding.UTF8))
                {
                    var str = sr.ReadToEnd();
                    var sets = JsonConvert.DeserializeObject<CalcSettings>(str);
                    Mapper.Map(sets, this);
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("Ошибка загрузки настроек", ex);
                InitDefault();
            }
        }

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
                Logger.Error("Ошибка сохранения файла настроек", ex);
                throw;
            }
        }

        private void InitDefault()
        {
            QuoteFolder = ExecutablePath.ExecPath + "\\history";
        }
    }
}

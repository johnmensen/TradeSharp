using System.Collections.Generic;
using ForexSiteDailyQuoteParser.CommonClass;

namespace ForexSiteDailyQuoteParser.Formatters
{
    public abstract class BaseParseFormat
    {
        /// <summary>
        /// Имя котировки зашито в имени файла 
        /// </summary>
        protected bool QuoteNameInFileName { get; set; }

        /// <summary>
        /// Формат, в котором записана дата
        /// </summary>
        protected string DateTimeFormat { get; set; }

        /// <summary>
        /// Символ(ы) для деления данных в одной строке
        /// </summary>
        protected char[] Separator { get; set; }

        /// <summary>
        /// Количество полей в одной записи (для проверки)
        /// </summary>
        protected int QuoteRecordFieldCount { get; set; }

        /// <summary>
        /// Список всех записей в распарсеном виде
        /// </summary>
        public List<QuoteRecord> QuotesDate { get; protected set; }

        /// <summary>
        /// записи, которые не удалось прочитать из файла
        /// </summary>
        public List<string> FailRecords { get; protected set; }

        public string DisplayName { get; protected set; }
    }
}

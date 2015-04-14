using System.Collections.Generic;
using ForexSiteDailyQuoteParser.CommonClass;

namespace ForexSiteDailyQuoteParser.Contract
{
    public interface IQuoteParser
    {
        /// <summary>
        ///  Распарсить содержимое файла(ов) в соответствии с настройками конкретного ParseFormat-а
        /// </summary>
        /// <param name="quoteFileName"></param>
        /// <returns>возвращает список тех валютных пар, которые присутствуют в файле</returns>
        List<string> Parse(string quoteFileName);

        /// <summary>
        /// Преобразует указанный список котировок в список строк, отформатированный в соответствии с текущим типом
        /// </summary>
        /// <param name="quoteList">список котировок для форматирования в список строк</param>
        IEnumerable<string> QuoteListToString(List<QuoteRecord> quoteList);
    }
}
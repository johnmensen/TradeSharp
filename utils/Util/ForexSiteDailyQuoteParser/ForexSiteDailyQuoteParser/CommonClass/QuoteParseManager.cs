using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ForexSiteDailyQuoteParser.Contract;
using ForexSiteDailyQuoteParser.Formatters;

namespace ForexSiteDailyQuoteParser.CommonClass
{
    public class QuoteParseManager
    {
        private IQuoteParser Resourse { get; set; }
        private IQuoteParser Output { get; set; }

        public QuoteParseManager(IQuoteParser resourse, IQuoteParser output)
        {
            Resourse = resourse;
            Output = output;
        }

        public List<string> Parse(string resourseFileName, string outputFileName, out List<string> resourseSumbol, out List<string> outputSumbol)
        {
            resourseSumbol = Resourse.Parse(resourseFileName);
            outputSumbol = Output.Parse(outputFileName);

            return ((BaseParseFormat)Resourse).FailRecords.Union(((BaseParseFormat)Output).FailRecords).ToList();
        }

        /// <summary>
        /// Объединение списков в указанном порядке
        /// </summary>
        /// <param name="simbolsToMarge"></param>
        /// <param name="mergeMode"></param>
        /// <returns></returns>
        public List<QuoteRecord> Merge(List<string> simbolsToMarge, MergeMode mergeMode = MergeMode.OutputMain)
        {
            List<QuoteRecord> result;

            var resourseDate = ((BaseParseFormat) Resourse).QuotesDate.Where(x => simbolsToMarge.Contains(x.Simbol)).ToList();
            var outputDate = ((BaseParseFormat)Output).QuotesDate.Where(x => simbolsToMarge.Contains(x.Simbol)).ToList();

            switch (mergeMode)
            {
                case MergeMode.ResourseMain:
                    result = resourseDate;
                    var exOutput = outputDate.Except(resourseDate, new QouteRecordDateComparer());
                    result.AddRange(exOutput);
                    break;
                case MergeMode.OutputMain:
                    result = outputDate;
                    var exResourse = resourseDate.Except(outputDate, new QouteRecordDateComparer());
                    result.AddRange(exResourse);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("mergeMode");
            }

            return result.OrderBy(x => x.Simbol).ThenBy(x => x.Date).ToList();
        }

        public void SaveNewFile(string newFileFullName, string[] dataToSave)
        {
            if (File.Exists(newFileFullName)) File.Delete(newFileFullName);
            File.WriteAllLines(newFileFullName, dataToSave);
        }
    }
}
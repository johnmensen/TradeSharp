using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace OnixAnalyzer.BL
{
    class Statistics
    {
        /// <summary>
        /// посчитать для сделок депо на момент открытия-закрытия
        /// </summary>        
        //public static void PreProcessDeals(AccountInfo ac)
        //{
        //    foreach (var deal in ac.Deals)
        //    {
        //        var eventTimeOpen = deal.OpenTime;
        //        var dealsProfit = ac.Deals.Sum(d => d.CloseTime <= eventTimeOpen ? d.Profit : 0);
        //        var balances = ac.Balances.Sum(b => b.Date <= eventTimeOpen ? b.Amount : 0);                
        //        deal.OpenDeposit = dealsProfit + balances;

        //        var eventTimeClose = deal.CloseTime;
        //        dealsProfit = ac.Deals.Sum(d => d.CloseTime <= eventTimeClose ? d.Profit : 0);
        //        balances = ac.Balances.Sum(b => b.Date <= eventTimeClose ? b.Amount : 0);
        //        deal.CloseDeposit = dealsProfit + balances;
        //    }            
        //}
    
        public static string BuildStatistics(int minDeals, int percentHistory, string pathToAccountFiles)
        {            
            var resultFile = new StringBuilder();
            
            var accounts = ReadAccounts(pathToAccountFiles);
            var totalAccountDeals = accounts.Sum(a => a.Deals.Count);
            var accountDealsInStat = accounts.Sum(a => a.Deals.Count >= minDeals ? a.Deals.Count : 0);

            if (accountDealsInStat == 0) return "Недостаточно сделок по статистике";

            foreach (var ac in accounts)
            {
                if (ac.Deals.Count < minDeals) continue;
                var dealsOrderedClose = ac.Deals.OrderBy(d => d.CloseTime);
                var nHist = percentHistory * ac.Deals.Count / 100;
                // разбить список сделок
                List<DealInfo> dealsHist = new List<DealInfo>(), dealsNow = new List<DealInfo>();
                var dealIndex = 0;
                foreach (var deal in dealsOrderedClose)
                {
                    if (dealIndex++ < nHist) dealsHist.Add(deal);
                    else dealsNow.Add(deal);
                }

                var histF = dealsHist.Sum(d => (d.Close - d.Open) * d.DealType * 100 / d.Open) / dealsHist.Count;
                var nowF = dealsNow.Sum(d => (d.Close - d.Open) * d.DealType * 100 / d.Open) / dealsNow.Count;
                
                // сформировать строку файла отчета (csv)
                var strCSV = string.Format(CultureInfo.InvariantCulture,
                    "{0},{1},{2}", ac.Id, histF, nowF);
                resultFile.AppendLine(strCSV);
            }
            // сохранить отчет
            var csvFileName = string.Format("{0}\\report_{1}.csv", 
                pathToAccountFiles, DateTime.Now.ToString("ddMMyyyy_HHmmss"));
            using (var sw = new StreamWriter(csvFileName, false))
            {
                sw.Write(resultFile.ToString());
            }
            var resultStr = string.Format("Всего сделок по счетам {0}, из них учтены {1}. В среднем {2} сделок на счет.",
                                          totalAccountDeals, accountDealsInStat,
                                          accountDealsInStat / accounts.Count(a => a.Deals.Count >= minDeals));
            return resultStr;
        }

        private static List<AccountInfo> ReadAccounts(string pathToAccountFiles)
        {
            var acList = new List<AccountInfo>();
            foreach (var fileInfo in new DirectoryInfo(pathToAccountFiles).GetFiles("*.xml"))
            {
                var ac = new AccountInfo();
                ac.LoadFromFile(fileInfo.FullName);
                acList.Add(ac);
            }
            return acList;
        }
    }

    struct ValueOnDate
    {
        public decimal val;
        public DateTime date;
        public ValueOnDate(decimal _val, DateTime _date)
        {
            val = _val;
            date = _date;
        }
    }
}

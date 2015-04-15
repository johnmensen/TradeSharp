using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Reports.Lib.Report
{
    /// <summary>
    /// на входе:
    /// 
    /// доходность по месяцам { 0.011, -0.002, ... }
    /// (VAMI[i] - VAMI[i-1])/VAMI[i]
    /// </summary>
    class MonthlyReportStatistics
    {
        #region HistoricalData
        public float cumulativeReturn;
        public float cumulativeVAMI;
        public float meanReturn;
        public float compoundRoRmonth;
        public float largestMonthGain;
        public float largestMonthLoss;
        public float percentPositiveMonths;
        #endregion

        #region Risk (фуфло это, а не мера риска)
        public float meanDeviation;
        public float sharpRatio;
        public float downsideDeviation8pc;
        public float sortinoRatio8pc;
        public float maxRelDrawdown;
        public float drawDownMonths;
        #endregion

        public MonthlyReportStatistics(List<EquityOnTime> listProfit)
        {
            var prevDepo = listProfit[0].equity;
            var prevDate = listProfit[0].time;
            var profitMonthly = new List<float>();
            foreach (var pt in listProfit)
            {
                if (pt.time.Month == prevDate.Month) continue;
                var profit = (pt.equity - prevDepo) / prevDepo;
                prevDepo = pt.equity;
                prevDate = pt.time;
                profitMonthly.Add(profit);
            }
            if (profitMonthly.Count < 2) return;
            CalculateHistoricalData(listProfit, profitMonthly);
            CalculateRisk(listProfit, profitMonthly);
        }

        public MonthlyReportStatistics(List<EquityOnTime> listProfit,
            List<float> profitMonthly)
        {
            CalculateHistoricalData(listProfit, profitMonthly);
            CalculateRisk(listProfit, profitMonthly);
        }

        private void CalculateHistoricalData(List<EquityOnTime> listProfit, List<float> lstProfit)
        {
            cumulativeReturn = 100 * (listProfit[listProfit.Count - 1].equity - listProfit[0].equity) / listProfit[0].equity;
            cumulativeVAMI = listProfit[listProfit.Count - 1].equity;
            meanReturn = lstProfit.Average() * 100;
            compoundRoRmonth = lstProfit.Product(p => 1 + p);
            compoundRoRmonth = 100 * ((float)Math.Pow(compoundRoRmonth, 1.0f / lstProfit.Count) - 1);
            largestMonthGain = 100 * lstProfit.Max();
            largestMonthLoss = 100 * lstProfit.Min();
            if (largestMonthLoss > 0) largestMonthLoss = 0;
            percentPositiveMonths = 100 * lstProfit.Count(p => p > 0) / (float)lstProfit.Count;
        }

        private void CalculateRisk(List<EquityOnTime> listProfit, List<float> lstProfit)
        {
            var mean = meanReturn/100;
            meanDeviation = 100 * (float)Math.Sqrt(lstProfit.Sum(p => (p - mean) * (p - mean)) / lstProfit.Count);
            sharpRatio = meanDeviation == 0 ? 0 : 100 * meanReturn / meanDeviation;
            downsideDeviation8pc = 100 * (float)Math.Sqrt((double)lstProfit.Sum(p => 
                p >= 0.08f ? 0 : (p - mean) * (p - mean)) / lstProfit.Count);
            sortinoRatio8pc = downsideDeviation8pc == 0 ? 0 : 100 * compoundRoRmonth / downsideDeviation8pc;
            // расчет просадки, времени от начала проседания
            maxRelDrawdown = 0f;
            DateTime drawStart = default(DateTime), drawEnd = default(DateTime);
            DateTime tempDrawStart = default(DateTime);

            for (var i = 0; i < listProfit.Count - 1; i++)
            {
                var tempDrawDown = 0f;
                var startBalance = listProfit[i].equity;
                var j = i + 1;
                tempDrawStart = listProfit[i].time;
                for (; j < listProfit.Count; j++)
                {
                    var curBal = listProfit[j].equity;
                    if (curBal >= startBalance) break;
                    var curDd = startBalance - curBal;
                    if (curDd > tempDrawDown) tempDrawDown = curDd;
                }
                i = j;
                if (startBalance > 0) tempDrawDown /= startBalance;
                if (tempDrawDown > maxRelDrawdown)
                {
                    maxRelDrawdown = tempDrawDown;
                    drawStart = tempDrawStart;
                    drawEnd = j < listProfit.Count ? listProfit[j].time : listProfit[listProfit.Count - 1].time;
                }
            }
            maxRelDrawdown *= 100;
            drawDownMonths = drawStart == default(DateTime) ? 0 : (float)drawEnd.MonthDifference(drawStart);
        }        
    }
}

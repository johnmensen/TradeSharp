using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TradeSharp.Client.Forms;

namespace TradeSharp.Client
{
    public partial class MainForm
    {
        /// <summary>
        /// этот метод дергает ферма роботов при запуске
        /// актуализируется кеш котировок
        /// если по какой-либо паре в кеше нет данных либо недостаточно
        /// продолжительная история - делается запрос из БД
        /// 
        /// на входе словарь: тикер - старт загрузки истории (предполагается время окончания - DateTime.Now)
        /// </summary>
        public void UpdateTickersCacheForRobots(
            Dictionary<string, DateTime> tickersToUpload, 
            int minMinutesToUpdateCache)
        {
            if (tickersToUpload == null)
                throw new Exception("UpdateTickersCacheForRobots: неправильные входные данные tickersToUpload == null");

            var dlg = new DownloadQuotesForm(tickersToUpload, minMinutesToUpdateCache);
            dlg.ShowDialog();
        }      
  
        /// <summary>
        /// заполняет котировки "a-la torrent", предлагает указать интервалы
        /// </summary>
        public void UpdateTickersCacheForRobotsChooseIntervals(
            Dictionary<string, DateTime> tickersToUpload, 
            bool showTickerPeriodForm,
            bool rebuildCharts)
        {
            // определить интервал синхронизации
            if (showTickerPeriodForm)
            {
                var dlg = new TimeIntervalsDlg(tickersToUpload);
                if (dlg.ShowDialog() == DialogResult.Cancel)
                    return;
                tickersToUpload = dlg.TimeIntervals;
            }
            // таки синхронизировать
            UpdateTickersCacheForRobots(tickersToUpload, 5);
            if (rebuildCharts)
                foreach (var ticker in tickersToUpload.Keys)
                    ReopenChartsSafe(ticker);
        }
    }
}
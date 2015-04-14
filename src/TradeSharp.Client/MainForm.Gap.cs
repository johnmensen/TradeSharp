using System;
using System.Linq;
using System.Windows.Forms;
using Entity;
using TradeSharp.Client.BL;
using TradeSharp.Client.Forms;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util.NotificationControl;

namespace TradeSharp.Client
{
    /*
     Заполнение гэпов (дырок) в котировках
     */
    partial class MainForm
    {
        private const int MinutesOfGapInQuoteStream = 5;
        private const string LinkTargetFillGaps = "DoFillGaps";

        /// <summary>
        /// сообщить о возникшем гэпе - предложить закачать котировки или же
        /// закачать их автоматом
        /// </summary>
        private void ReportOnGapFound(DateTime startOfGap)
        {
            // определить, не пришелся ли "гэп" на выходные
            var gapInterval = new DateSpan(startOfGap, DateTime.Now);
            var miniHoles = DaysOff.Instance.GetIntersected(gapInterval);
            var gaps = QuoteCacheManager.SubtractPeriods(gapInterval, miniHoles);
            if (gaps == null || gaps.Count == 0) return;
            var sumMinutes = gaps.Sum(g => (g.end - g.start).TotalMinutes);
            if (sumMinutes < MinutesOfGapInQuoteStream) return;

            // вывести уведомление
            var msg = gaps.Count == 1
                          ? string.Format("заполнить гэп ({0} минут)", (int) sumMinutes)
                          : string.Format("заполнить гэпы ({0} минут суммарно)", (int) sumMinutes);
            AddUrlToStatusPanelSafe(DateTime.Now, msg, LinkTargetFillGaps);
            var action = UserSettings.Instance.GetAccountEventAction(AccountEventCode.GapFound);
            if (action == AccountEventAction.StatusPanelOnly || action == AccountEventAction.DoNothing)
                return;
            
            // показать желтое окошко
            var repeatNotification = false;
            var shouldFill = !UserSettings.Instance.ConfirmGapFilling || 
                (NotificationBox.Show(msg + Environment.NewLine + "Заполнить сейчас?", "Обнаружен гэп",
                    MessageBoxButtons.YesNo, out repeatNotification) == DialogResult.Yes);

            if (UserSettings.Instance.ConfirmGapFilling != repeatNotification)
            {
                UserSettings.Instance.ConfirmGapFilling = repeatNotification;
                UserSettings.Instance.SaveSettings();
            }
            if (!shouldFill) return;
            Invoke(new Action<string>(FillGapAfterReport), LinkTargetFillGaps);
        }

        private void FillGapAfterReport(string linkTarget)
        {
            ProcessUserClickOnFillGapLink(linkTarget);
        }

        /// <summary>
        /// пользователь кликнул ссылку в окне сообщений - заполнить все гэпы
        /// (по всем графикам)
        /// </summary>
        private bool ProcessUserClickOnFillGapLink(string linkTarget)
        {
            if (linkTarget != LinkTargetFillGaps) return false;
            // заполнить дырки
            var dlg =
                new DownloadQuotesForm(
                    Charts.Select(c => c.chart.Symbol)
                          .Distinct()
                          .ToDictionary(s => s, s =>
                              {
                                  var interval = AtomCandleStorage.Instance.GetDataRange(s);
                                  return interval.HasValue
                                             ? interval.Value.b
                                             : DateTime.Now.AddDays(-1);
                              }), 0);
            dlg.LoadCompleted += delegate
                {
                    // обновить графики
                    AddMessageToStatusPanelSafe(DateTime.Now, "Обновление графиков...");
                    foreach (var ticker in dlg.TickersToUpload)
                        ReopenChartsSafe(ticker);

                    // вывести сообщение - готово
                    AddMessageToStatusPanelSafe(DateTime.Now, dlg.TickersToUpload.Count == 1
                                                                  ? "График " + dlg.TickersToUpload[0] + " обновлен"
                                                                  : "Обновлено " + dlg.TickersToUpload.Count +
                                                                    " графиков");
                };
            dlg.ShowDialog(this);
            return true;
        }
    }
}

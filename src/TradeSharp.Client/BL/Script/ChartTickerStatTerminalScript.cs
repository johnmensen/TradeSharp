using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Candlechart;
using System.Linq;
using Candlechart.ChartMath;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.BL.Script
{
    [DisplayName("Статистика по графику")]
    public class ChartTickerStatTerminalScript : TerminalScript
    {
        [DisplayName("Сохранять свечи")]
        [Category("Основные")]
        [Description("Предложить экспортировать свечи")]
        public bool PromptExportCandles { get; set; }

        public override bool CanBeTriggered
        {
            get { return false; }
        }

        public ChartTickerStatTerminalScript()
        {
            ScriptTarget = TerminalScriptTarget.График;
            ScriptName = "Статистика по графику";
        }

        public override string ActivateScript(string ticker)
        {
            throw new Exception("Неверный тип вызова скрипта \"ChartTickerStatTerminalScript\"");
        }

        public override string ActivateScript(bool byTrigger)
        {
            throw new Exception("Неверный тип вызова скрипта \"ChartTickerStatTerminalScript\"");
        }

        public override string ActivateScript(CandleChartControl chart, PointD worldCoords)
        {
            new PriceDistributionForm(chart.Symbol).ShowDialog();

            var candles = chart.chart.StockSeries.Data.Candles;
            if (candles == null || candles.Count == 0) return "нет котировок";

            var ticker = chart.Symbol;
            var midOC = 
                DalSpot.Instance.GetPointsValue(ticker, candles.Average(c => Math.Abs(c.close - c.open)));
            var midHL =
                DalSpot.Instance.GetPointsValue(ticker, candles.Average(c => c.high - c.low));
            var low = candles.Min(c => c.low);
            var high = candles.Max(c => c.high);
            var digits = DalSpot.Instance.GetPrecision(chart.Symbol);

            MessageBox.Show(string.Format("{0} свеч, диапазон: {1}-{2}{3}" +
                                          "сред. OC: {4:f1} пп, сред. HL: {5:f1} пп",
                                          candles.Count, low.ToStringUniform(digits), high.ToStringUniform(digits),
                                          Environment.NewLine,
                                          midOC, midHL));

            if (!PromptExportCandles || candles.Count == 0) return "исполнен";
            
            // экспортировать свечи
            DialogResult rst;
            var countStr = Dialogs.ShowInputDialog("Экспорт свечей", "Макс кол-во", true, "200", out rst);
            if (rst != DialogResult.OK) return "исполнен";

            // таки экспортировать
            var count = countStr.ToIntSafe() ?? 100;
            var start = candles.Count - count;
            if (start < 0) start = 0;
            var sb = new StringBuilder();
            sb.AppendLine("index\ttime op\ttime cl\to\th\tl\tc\t");

            for (var i = start; i < candles.Count; i++)
            {
                var c = candles[i];
                sb.AppendLine(string.Format("{0}\t{1:dd/MM/yyy HH:mm:ss}\t{2:dd/MM/yyy HH:mm:ss}\t{3}\t{4}\t{5}\t{6}", 
                    i, c.timeOpen, c.timeClose, 
                    c.open.ToStringUniformPriceFormat(true),
                    c.high.ToStringUniformPriceFormat(true),
                    c.low.ToStringUniformPriceFormat(true),
                    c.close.ToStringUniformPriceFormat(true)));
            }

            var dlg = new SaveFileDialog
            {
                Title = "Сохранить свечи",
                DefaultExt = "xls",
                Filter = "XLS (*.xls)|*.xls|CSV (*.csv)|*.csv|Text (*.txt)|txt|Все файлы|*.*",
                FilterIndex = 0,
                FileName = string.Format("свечи_{0}_{1}.xls", 
                    chart.Symbol, BarSettingsStorage.Instance.GetBarSettingsFriendlyName(chart.Timeframe))
            };
            if (dlg.ShowDialog() != DialogResult.OK) return "исполнен";

            using (var sw = new StreamWriter(dlg.FileName, false, Encoding.ASCII))
            {
                sw.Write(sb.ToString());
            }
            
            return "исполнен";
        }        
    }
}

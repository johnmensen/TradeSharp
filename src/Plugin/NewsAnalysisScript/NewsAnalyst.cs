using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Candlechart;
using Candlechart.ChartMath;
using TradeSharp.Client;
using TradeSharp.Client.BL.Script;
using TradeSharp.Util;

namespace NewsAnalysisScript
{
    public class RobotNewsStat
    {
        public string CountryCode { get; set; }
        public string Title { get; set; }
        
        public int FollowCountNumber
        {
            get
            {
                return IndexFollowByNewsFlags.Count == 0
                           ? 0
                           : IndexFollowByNewsFlags.Count(flag => flag) * 100 / IndexFollowByNewsFlags.Count;
            }
        }

        public string FollowCount
        {
            get
            {
                return string.Format("{0} %", FollowCountNumber);
            }
        }
        public int Count { get { return DeltaIndexes.Count; } }
        public string Average
        {
            get
            {
                double followAvg = 0, unfollowAvg = 0;
                double followCount = IndexFollowByNewsFlags.Count(flag => flag);
                for (int i = 0; i < DeltaIndexes.Count; i++)
                    if(IndexFollowByNewsFlags[i])
                        followAvg += Math.Abs(DeltaIndexes[i]) / followCount;
                double unfollowCount = IndexFollowByNewsFlags.Count(flag => !flag);
                for (int i = 0; i < DeltaIndexes.Count; i++)
                    if (!IndexFollowByNewsFlags[i])
                        unfollowAvg += Math.Abs(DeltaIndexes[i]) / unfollowCount;
                return string.Format("{0} / {1}", followAvg.ToString("N4"), unfollowAvg.ToString("N4"));
            }
        }
        public List<double> DeltaIndexes = new List<double>();
        public List<bool> IndexFollowByNewsFlags = new List<bool>();
        public List<DateTime> Times = new List<DateTime>();

        public bool MatchCriteria { get; set; }
    }

    [DisplayName("Аналитик новостей")]
    public class NewsAnalyst : TerminalScript
    {
        public NewsAnalyst()
        {
            ScriptTarget = TerminalScriptTarget.Терминал;
            ScriptName = "Аналитик новостей";
        }
        
        public override string ActivateScript(CandleChartControl chart, PointD worldCoords)
        {
            throw new Exception("Неверный тип вызова скрипта " + ScriptName);
        }

        public override string ActivateScript(string ticker)
        {
            throw new Exception("Неверный тип вызова скрипта " + ScriptName);
        }

        public override string ActivateScript(bool byTrigger)
        {
            var newsRobotFileName = ExecutablePath.ExecPath + "\\plugin\\NewsRobot.dll.xml";
            string error;
            var settings = NewsRobot.CurrencySettings.LoadCurrencySettings(newsRobotFileName, out error);
            if (error != null)
                return "error: " + error;
            var inputDataForm = new InputDataForm();
            var currencies = settings.Select(s => "[" + s.CountryCode + "] " + s.CurrencyCode);
            inputDataForm.SetCurrencies(currencies.ToList());
            if (inputDataForm.ShowDialog(MainForm.Instance) == DialogResult.Cancel)
                return "cancelled";

            var progressForm = new ProgressForm();
            progressForm.SetNewsFileName(inputDataForm.GetNewsFileName());
            progressForm.SetQuotesFileNames(inputDataForm.GetQuotesFileNames());
            progressForm.Start(inputDataForm.GetStartTime(), inputDataForm.GetEndTime(),
                               inputDataForm.GetSelectedCurrencies(), inputDataForm.GetOnlyValuableNewsFlag());
            progressForm.ShowDialog(MainForm.Instance);

            var resultsForm = new ResultsForm();
            resultsForm.SetNews(progressForm.GetRobotNews());
            resultsForm.SetStats(progressForm.GetRobotNewsStats());
            resultsForm.ShowDialog(MainForm.Instance);
            return "done";
        }
    }
}

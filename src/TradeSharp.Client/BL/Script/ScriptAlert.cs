using System;
using System.ComponentModel;
using Candlechart;
using Candlechart.ChartMath;
using Entity;
using TradeSharp.Client.BL.Sound;
using TradeSharp.Util;

namespace TradeSharp.Client.BL.Script
{
    [DisplayName("Сигнал")]
    public class ScriptAlert : TerminalScript
    {
        #region Параметры

        private int freezeSeconds = 60;
        [PropertyXMLTag("FreezeSeconds")]
        [LocalizedDisplayName("TitleIntervalBetweenCalls")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageIntervalBetweenCalls")]
        public int FreezeSeconds
        {
            get { return freezeSeconds; }
            set { freezeSeconds = value; }
        }

        private int repeatTimes = 5;
        [LocalizedDisplayName("TitleRepeatTimes")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageRepeatTimes")]
        [PropertyXMLTag("RepeatTimes")]
        public int RepeatTimes
        {
            get { return repeatTimes; }
            set { repeatTimes = value; }
        }

        private VocalizedEvent eventSound = VocalizedEvent.TradeSignal;
        [LocalizedDisplayName("TitleSound")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageAlertSound")]
        [PropertyXMLTag("EventSound")]
        public VocalizedEvent EventSound
        {
            get { return eventSound; }
            set { eventSound = value; }
        }

        private string scriptMessage = Localizer.GetString("TitleSignal");
        
        [LocalizedDisplayName("TitleText")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("TitleMessageText")]
        [PropertyXMLTag("ScriptMessage")]
        public string ScriptMessage
        {
            get { return scriptMessage; }
            set { scriptMessage = value; }
        }

        #endregion

        private readonly ThreadSafeTimeStamp timeActivated = new ThreadSafeTimeStamp();

        public ScriptAlert()
        {
            ScriptTarget = TerminalScriptTarget.Терминал;
            ScriptName = "Сигнал";
        }

        public override string ActivateScript(CandleChartControl chart, PointD worldCoords)
        {
            return ActivateScript(false);
        }

        public override string ActivateScript(string ticker)
        {
            return ActivateScript(false);
        }

        public override string ActivateScript(bool byTrigger)
        {
            // проверить количество повторений и интервал ожидания
            if (repeatTimes == 0) return string.Empty;

            var timeAct = timeActivated.GetLastHitIfHitted();
            if (timeAct.HasValue)
                if ((DateTime.Now - timeAct.Value).TotalSeconds < freezeSeconds)
                    return string.Empty;
            
            // таки проиграть сигнал
            EventSoundPlayer.Instance.PlayEvent(eventSound);

            if (repeatTimes > 0)
                repeatTimes--;
            timeActivated.Touch();

            MainForm.Instance.AddMessageToStatusPanelSafe(DateTime.Now, scriptMessage);
            return scriptMessage;
        }
    }
}

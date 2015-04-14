using System;

namespace TradeSharp.Util
{
    public class HelpManager
    {
        public const string KeySignalAutoTrade = "20";
        public const string KeyChatClient = "30";
        public const string KeyTradeSettings = "40";
        public const string KeyFastButtons = "50";
        public const string IndicatorWindow = "60";
        public const string ToolPanel = "70";

        private Action<object, string> onHelpRequested;

        private static HelpManager instance;

        public static HelpManager Instance
        {
            get { return instance ?? (instance = new HelpManager()); }
        }

        private HelpManager()
        {            
        }

        public void Initialize(Action<object, string> onHelpRequested)
        {
            this.onHelpRequested = onHelpRequested;
        }

        public void ShowHelp(object sender, string topicId = null)
        {
            if (onHelpRequested != null)
                onHelpRequested(sender, topicId);
        }
    }
}

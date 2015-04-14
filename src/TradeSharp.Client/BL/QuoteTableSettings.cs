using System;
using System.Collections.Generic;
using System.Linq;

namespace TradeSharp.Client.BL
{
    /// <summary>
    /// хранит настройки таблицы котировок
    /// актуализирует UserSettings, вызывает обновление контролов, 
    /// </summary>
    class QuoteTableSettings
    {
        private static QuoteTableSettings instance;

        public static QuoteTableSettings Instance
        {
            get { return instance ?? (instance = new QuoteTableSettings()); }
        }

        private List<QuoteTableCellSettings> settingsList;

        private List<QuoteTableCellSettings> SettingsList
        {
            get { return (settingsList ?? (settingsList = UserSettings.Instance.TickerCellList)).ToList(); }
        }

        private Action<List<QuoteTableCellSettings>, object> settingsAreUpdated;

        public event Action<List<QuoteTableCellSettings>, object> SettingsAreUpdated
        {
            add { settingsAreUpdated += value; }
            remove { settingsAreUpdated -= value; }
        }

        public List<QuoteTableCellSettings> GetSettings()
        {
            return SettingsList;
        }

        public void ExcludeTicker(string ticker, object sender)
        {
            var sets = settingsList ?? (settingsList = UserSettings.Instance.TickerCellList);
            var cellIndex = sets.FindIndex(c => c.Ticker == ticker);
            if (cellIndex < 0) return;
            sets.RemoveAt(cellIndex);
            UpdateSettings(sets, sender);
        }

        public void UpdateSettings(List<QuoteTableCellSettings> newSets, object sender)
        {
            settingsList = newSets;
            UserSettings.Instance.TickerCellList = newSets;
            UserSettings.Instance.SaveSettings();
            if (settingsAreUpdated != null)
                settingsAreUpdated(newSets, sender);
        }
            
    }
}

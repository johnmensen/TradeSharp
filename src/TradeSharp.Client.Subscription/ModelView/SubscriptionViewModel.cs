using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using TradeSharp.Client.Subscription.Model;
using TradeSharp.Util;

namespace TradeSharp.Client.Subscription.ModelView
{
    class SubscriptionViewModel : INotifyPropertyChanged
    {
        private static SubscriptionViewModel instance;

        public static SubscriptionViewModel Instance
        {
            get { return instance; }
        }

        private List<TradeSignalSubscriptionSettings> originalSettings;

        public List<TradeSignalSubscriptionSettings> CurrentSettings { get; private set; }

        private readonly Action<IList<TradeSignalSubscriptionSettings>> saveSubscriptionSettings;

        private PropertyChangedEventHandler propertyChanged;

        public event PropertyChangedEventHandler PropertyChanged
        {
            add { propertyChanged += value; }
            remove { propertyChanged -= value; }
        }

        public Func<List<int>> getMagicsUsed; 

        public static void Initialize(List<TradeSignalSubscriptionSettings> sets,
                               Action<IList<TradeSignalSubscriptionSettings>> saveSubscriptionSettings,
                               Func<List<int>> getMagicsUsed)
        {
            instance = new SubscriptionViewModel(sets, saveSubscriptionSettings, getMagicsUsed);
        }

        private SubscriptionViewModel(List<TradeSignalSubscriptionSettings> sets,
            Action<IList<TradeSignalSubscriptionSettings>> saveSubscriptionSettings,
            Func<List<int>> getMagicsUsed)
        {
            originalSettings = sets;
            this.getMagicsUsed = getMagicsUsed;
            CurrentSettings = sets.Select(s => new TradeSignalSubscriptionSettings(s)).ToList();
            this.saveSubscriptionSettings = saveSubscriptionSettings;
        }

        public void Unsubscribe(TradeSignalSubscriptionSettings sub, bool showDialog = true)
        {
            if (MessageBox.Show(string.Format("Отписаться от сигналов \"{0}\"?", sub.Title),
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;
            CurrentSettings.RemoveByPredicate(settings => settings.Id == sub.Id);
            propertyChanged(this, new PropertyChangedEventArgs("CurrentSettings"));
        }

        public void Unsubscribe(int tradeSignalCatId)
        {
            var sets = originalSettings.FirstOrDefault(s => s.Id == tradeSignalCatId);
            if (sets != null)
                Unsubscribe(sets, false);
        }

        public void Subscribe(TradeSignalSubscriptionSettings sub)
        {
            CurrentSettings.Add(sub);
            propertyChanged(this, new PropertyChangedEventArgs("CurrentSettings"));
        }

        public void SaveChanges()
        {
            saveSubscriptionSettings(CurrentSettings);
        }

        public void UpdateSettings(TradeSignalSubscriptionSettings newItem)
        {
            var index = CurrentSettings.FindIndex(item => item.Id == newItem.Id);
            CurrentSettings[index] = newItem;
        }

        public void GetUpdates(
            out List<TradeSignalSubscriptionSettings> removedSubscriptions,
            out List<TradeSignalSubscriptionSettings> addedSubscriptions,
            out List<TradeSignalSubscriptionSettings> changedTradeStatusSubscriptions)
        {
            removedSubscriptions =
                (from set in originalSettings
                 let setId = set.Id 
                 where CurrentSettings.All(s => s.Id != setId) select set).ToList();
            addedSubscriptions = 
                (from set in CurrentSettings let setId = set.Id
                 where originalSettings.All(s => s.Id != setId)
                 select set).ToList();

            changedTradeStatusSubscriptions = (from set in originalSettings
                                               let setId = set.Id
                                               let newSet = CurrentSettings.FirstOrDefault(s => s.Id == setId)
                                               where newSet != null && !(newSet.AreEqual(set))
                                               select newSet).ToList();
        }

        /// <summary>
        /// вызывается после принятия изменений (сохранения на клиенте и на сервере)
        /// </summary>
        public void ApplyCurrentSettings()
        {
            originalSettings = CurrentSettings.Select(s => new TradeSignalSubscriptionSettings(s)).ToList();
        }
    }
}

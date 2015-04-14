using System;
using System.Collections.Generic;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.Subscription.Model
{
    /// <summary>
    /// управляет подписками на торговый сигнал
    /// </summary>
    class AccountSubscription
    {
        private static AccountSubscription instance;

        public static AccountSubscription Instance
        {
            get { return instance ?? (instance = new AccountSubscription()); }
        }

        private Action<int, TradeSignalCategory, bool> updateSubscription;
        
        public event Action<int, TradeSignalCategory, bool> UpdateSubscription
        {
            add { updateSubscription += value; }
            remove { updateSubscription -= value; }
        }

        public Func<ActionOnSignal> getActionOnSignal;

        public Action<ActionOnSignal> setActionOnSignal;

        #region Актуализируемая информация о подписках на сигналы

        private readonly ActualizableSafeList<TradeSignalCategory> categories = 
            new ActualizableSafeList<TradeSignalCategory>(1000, 5 * 60 * 1000);

        #endregion

        private AccountSubscription()
        {
        }

        public void ForceRenewSubscriptions()
        {
            categories.MarkInactual();
        }

        public void Subscribe(TradeSignalCategory cat)
        {
            var accountId = AccountModel.Instance.AccountId;
            if (!accountId.HasValue) return;
            updateSubscription(accountId.Value, cat, true);
        }

        public void Unsubscribe(TradeSignalCategory cat)
        {
            var accountId = AccountModel.Instance.AccountId;
            if (!accountId.HasValue) return;
            updateSubscription(accountId.Value, cat, false);
        }

        /// <summary>
        /// вернуть список категорий, на которые подписан клиент
        /// возможно, список должен быть предварительно обновлен
        /// </summary>
        public List<TradeSignalCategory> GetAccountSubscriptions()
        {
            if (categories.IsActual())
                return categories.GetValues();

            var accountId = AccountModel.Instance.AccountId;
            if (accountId == null) return new List<TradeSignalCategory>();
            try
            {
                var signals = AccountModel.Instance.ServerProxy.proxy.GetTradeSignalsSubscribed(accountId.Value);
                categories.Update(signals);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка в GetTradeSignalsSubscribed({0}) : {1}",
                    accountId.Value, ex);
            }

            return categories.GetValues();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace TradeSharp.Client.Util
{
    public enum ActionOnSignal
    {
        ПредложитьОткрытьГрафик = 0, Сообщение, ПоказатьНаГрафике, ОткрытьГрафик
    }

    public class SubscriptionModel
    {
        #region Singletone

        private static readonly Lazy<SubscriptionModel> instance =
            new Lazy<SubscriptionModel>(() => new SubscriptionModel());

        public static SubscriptionModel Instance
        {
            get { return instance.Value; }
        }

        #endregion

        private List<Subscription> categories = new List<Subscription>();
        public List<Subscription> SubscribedCategories
        {
            get
            {
                if (!categoriesLoaded)
                    LoadSubscribedCategories();
                return categories;
            }
        }

        // изменение состава подписки
        private Action<List<Subscription>> modelIsLoaded;
        public event Action<List<Subscription>> ModelIsLoaded
        {
            add { modelIsLoaded += value; }
            remove { modelIsLoaded -= value; }
        }

        private Action<bool> modelStateChanged;
        public event Action<bool> ModelStateChanged
        {
            add { modelStateChanged += value; }
            remove { modelStateChanged -= value; }
        }

        private bool wasModified;
        public bool WasModified
        {
            get { return wasModified; }
            set
            {
                if (value == wasModified)
                    return;
                wasModified = value;
                if (modelStateChanged != null)
                    modelStateChanged(wasModified);
            }
        }

        public ITradeSharpServerTrade ServerProxy;

        public Func<ActionOnSignal> getActionOnSignal;

        public Action<ActionOnSignal> setActionOnSignal;

        private bool categoriesLoaded;

        private SubscriptionModel()
        {
        }

        // TODO: load in another thread
        /// <summary>
        /// обновить подписки на категории (получить от сервера)
        /// </summary>
        public void LoadSubscribedCategories()
        {
            categories = GetSubscriptionsFromServer() ?? new List<Subscription>();
            categoriesLoaded = true;
            WasModified = false;
            if (modelIsLoaded != null)
                modelIsLoaded(categories);
        }

        /// <summary>
        /// сохранить настройки подписки на сервере
        /// </summary>
        public void SaveSubscribedCategories()
        {
            var serverCats = GetSubscriptionsFromServer();

            // сравнить текущие подписки и серверные
            if (categories.Count == serverCats.Count)
                if (categories.All(c => serverCats.Any(s => s.AreSame(c))))
                {
                    WasModified = false;
                    return;
                }

            // отписаться
            var catsToUnsubscribe = serverCats.Where(c => categories.All(s => s.Service != c.Service)).ToList();
            foreach (var cat in catsToUnsubscribe)
            {
                WalletError error;
                var login = AccountStatus.Instance.Login;
                if (ServerProxy == null || !ServerProxy.SubscribeOnService(
                    CurrentProtectedContext.Instance.MakeProtectedContext(),
                    login, cat.Service, false, true, new AutoTradeSettings(), out error))
                {
                    Logger.ErrorFormat("SubscriptionModel.SaveSubscribedCategories: попытка отписаться от сигнала #{0} безуспешна", cat);
                }
            }

            // подписаться / модифицировать
            var catsToSubscribe = categories.Where(c => serverCats.All(o => o.Service != c.Service)).ToList();
            catsToSubscribe.AddRange(categories.Where(c => serverCats.Any(o => o.Service == c.Service && !o.AreSame(c))));
            foreach (var cat in catsToSubscribe)
            {
                WalletError error;
                if (ServerProxy == null ||
                    !ServerProxy.SubscribeOnService(CurrentProtectedContext.Instance.MakeProtectedContext(),
                                                   AccountStatus.Instance.Login, cat.Service, true, false,
                                                   cat.AutoTradeSettings, out error))
                {
                    Logger.ErrorFormat("SubscriptionModel.SaveSubscribedCategories: попытка подписаться на сигнал #{0} безуспешна", cat);
                }
            }

            // обновляем кэш; вызывающий код должен удостовериться в корректности результата
            LoadSubscribedCategories();
        }

        public void AddSubscription(Subscription cat)
        {
            var existCat = categories.FirstOrDefault(c => c.Service == cat.Service);
            if (existCat != null) return;
            categories.Add(cat);
            WasModified = true;
            if (modelIsLoaded != null)
                modelIsLoaded(categories);
        }

        public void RemoveSubscription(Subscription cat)
        {
            var existCat = categories.FirstOrDefault(c => c.Service == cat.Service);
            if (existCat == null) return;
            categories.Remove(existCat);
            WasModified = true;
            if (modelIsLoaded != null)
                modelIsLoaded(categories);
        }

        public void ModifySubscription(Subscription cat)
        {
            var existCatIndex = categories.FindIndex(c => c.Service == cat.Service);
            if (existCatIndex < 0) return;
            categories[existCatIndex] = cat;
            WasModified = true;
            if (modelIsLoaded != null)
                modelIsLoaded(categories);
        }

        public List<Subscription> GetSubscriptionsFromServer()
        {
            var login = AccountStatus.Instance.Login;
            try
            {
                var signals = TradeSharpAccount.Instance.proxy.GetSubscriptions(login);
                return signals;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка в SubscriptionModel.GetSubscriptionsFromServer({0}) : {1}",
                    login, ex);
                return null;
            }
        }

        public Account[] GetUserOwnedAccounts()
        {
            List<int> accountIds;
            List<AccountRights> accountRights;
            try
            {
                ServerProxy.GetUserAccounts(AccountStatus.Instance.Login,
                                        CurrentProtectedContext.Instance.MakeProtectedContext(),
                                        out accountIds, out accountRights);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка в GetUserOwnedAccounts: {0}", ex);
                return new Account[0];
            }

            if (accountIds == null || accountIds.Count == 0) return new Account[0];
            var ownedAccounts = accountIds.Where((v, i) => accountRights[i] == AccountRights.Управление).ToArray();
            // получить более подробную инф. по счетам
            try
            {
                return ownedAccounts.Select(a =>
                    {
                        Account act;
                        TradeSharpAccount.Instance.proxy.GetAccountInfo(a, false, out act);
                        return act;
                    }).Where(a => a != null).ToArray();
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка в GetUserOwnedAccounts - получение расширенной информации: {0}", ex);
                return ownedAccounts.Select(a => new Account {ID = a}).ToArray();
            }
        }
    }
}

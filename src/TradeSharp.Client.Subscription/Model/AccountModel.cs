using System;
using TradeSharp.Chat.Client.BL;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.Subscription.Model
{
    class AccountModel
    {
        private static readonly Lazy<AccountModel> instance = new Lazy<AccountModel>(() => new AccountModel());

        public static AccountModel Instance
        {
            get { return instance.Value; }
        }

        private Func<Account> getActualAccountData;

        private Func<string> getUserLogin;

        private Action<PerformerStat> investInPAMM;

        public ITradeSharpServerTrade ServerProxy { get; private set; }

        public Account Account
        {
            get { return getActualAccountData(); }
        }

        public int? AccountId
        {
            get
            {
                try
                {
                    var account = Account;
                    return account == null ? (int?)null : account.ID;
                }
                catch (Exception ex)
                {
                    Logger.Error("AccountModel - ошибка получения счета", ex);
                    return null;
                }
            }
        }

        public ChatControlBackEnd Chat { get; private set; }

        private AccountModel()
        {
        }

        public void Initialize(
            Func<Account> getActualAccountData, Func<string> getUserLogin, 
            Action<PerformerStat> investInPAMM,
            ITradeSharpServerTrade serverProxy, ChatControlBackEnd chat)
        {
            this.getActualAccountData = getActualAccountData;
            this.getUserLogin = getUserLogin;
            this.investInPAMM = investInPAMM;
            ServerProxy = serverProxy;
            Chat = chat;
        }

        public void InvestInPAMM(PerformerStat performer)
        {
            if (investInPAMM != null)
                investInPAMM(performer);
        }

        public string GetUserLogin()
        {
            return getUserLogin == null ? "" : getUserLogin();
        }
    }
}

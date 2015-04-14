using System;
using System.Threading;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.Util
{
    /// <summary>
    /// Отображает состояние подключения и информацию по счету
    /// Потокобезопасный
    /// </summary>
    public class AccountStatus
    {
        public delegate void AccountInfoUpdateDel(Account account);
        
        private static AccountStatus instance;
        public static AccountStatus Instance
        {
            get { return instance ?? (instance = new AccountStatus()); }
        }

        private AccountStatus() {}

        private AccountInfoUpdateDel onAccountInfoUpdated;
        public event AccountInfoUpdateDel OnAccountInfoUpdated
        {
            add { onAccountInfoUpdated += value; }
            remove { onAccountInfoUpdated -= value; }
        }

        public volatile AccountConnectionStatus connectionStatus;

        public volatile bool isAuthorized;

        public volatile bool isAccountSelected;

        private readonly ReaderWriterLock accountDataLocker = new ReaderWriterLock();

        public volatile int accountID;

        private volatile float leverage;
        public float Leverage
        {
            get { return leverage; }
            set { leverage = value; }
        }

        private volatile int trades;
        public int Trades
        {
            get { return trades; }
            set { trades = value; }
        }

        private volatile float points;
        public float Points
        {
            get { return points; }
            set { points = value; }
        }

        private volatile float profitInPercents;
        public float ProfitInPercents
        {
            get { return profitInPercents; }
            set { profitInPercents = value; }
        }

        private volatile int lastAccountId;

        private readonly ReaderWriterLock loginLocker = new ReaderWriterLock();

        private const int LoginLockerTimeout = 1000;

        private string login;
        public string Login
        {
            get
            {
                try
                {
                    loginLocker.AcquireReaderLock(LoginLockerTimeout);
                }
                catch (ApplicationException)
                {
                    return string.Empty;
                }
                try
                {
                    return login;
                }
                finally 
                {
                    loginLocker.ReleaseReaderLock();
                }
            }
            
            set
            {
                try
                {
                    loginLocker.AcquireWriterLock(LoginLockerTimeout);
                }
                catch (ApplicationException)
                {
                    return;
                }
                try
                {
                    login = value;
                }
                finally 
                {
                    loginLocker.ReleaseWriterLock();
                }
            }
        }

        private volatile int serverConnectionStatus;
        public ServerConnectionStatus ServerConnectionStatus
        {
            get { return (ServerConnectionStatus) serverConnectionStatus; }
            set
            {
                serverConnectionStatus = (int) value;
                if (connectionStatusIsUpdated != null)
                    connectionStatusIsUpdated(value);
            }
        }

        public delegate void ConnectionAbortedDel();

        private ConnectionAbortedDel onConnectionAborted;
        public event ConnectionAbortedDel OnConnectionAborted
        {
            add { onConnectionAborted += value; }
            remove { onConnectionAborted -= value; }
        }
        
        public delegate void AuthenticationFailedDel();
        
        private AuthenticationFailedDel onAuthenticationFailed;
        public event AuthenticationFailedDel OnAuthenticationFailed
        {
            add { onAuthenticationFailed += value; }
            remove { onAuthenticationFailed -= value; }
        }

        private Action onAccountDataFirstReceived;

        public event Action OnAccountDataFirstReceived
        {
            add { onAccountDataFirstReceived += value; }
            remove { onAccountDataFirstReceived -= value; }
        }
        
        private Account accountData;
        public Account AccountData
        {
            get
            {
                try
                {
                    accountDataLocker.AcquireReaderLock(1000);
                }
                catch (ApplicationException)
                {
                    Logger.Error("AccountStatus.AccountData get timeout");
                    return null;
                }
                try
                {
                    return accountData == null ? null :
                        new Account
                               {
                                   Balance = accountData.Balance,
                                   Currency = accountData.Currency,
                                   Equity = accountData.Equity,
                                   Group = accountData.Group,
                                   ID = accountData.ID,                                   
                                   UsedMargin = accountData.UsedMargin
                               };
                }
                finally
                {
                    accountDataLocker.ReleaseReaderLock();
                }
            }
            set
            {
                try
                {
                    accountDataLocker.AcquireWriterLock(1000);
                }
                catch (ApplicationException)
                {
                    Logger.Error("AccountStatus.AccountData set timeout");
                    isAuthorized = false;
                    return;
                }
                try
                {
                    if (value != null && accountData != null)
                    {
                        if (accountData.Equity != 0 && value.Equity == 0)
                            value.Equity = accountData.Equity;
                        accountData = value;

                        if (onAccountInfoUpdated != null)
                            onAccountInfoUpdated(accountData);
                    }
                    else
                    {
                        if (onAccountInfoUpdated != null)
                            onAccountInfoUpdated(null);
                    }
                    accountData = value;
                }
                finally
                {
                    accountDataLocker.ReleaseWriterLock();
                }                
            }
        }

        private Action<ServerConnectionStatus> connectionStatusIsUpdated;

        public event Action<ServerConnectionStatus> ConnectionStatusIsUpdated
        {
            add { connectionStatusIsUpdated += value; }
            remove { connectionStatusIsUpdated -= value; }
        }

        // not used
        public string GetStatusString()
        {
            if (connectionStatus == AccountConnectionStatus.Connected)
            {
                if (!isAuthorized)
                    return "подключен, нет авторизации";
                if (!isAccountSelected)
                    return "авторизован, не выбран счет";
                var acData = AccountData;
                return acData == null ? "подключение..." 
                    : string.Format("Счет №{7} Баланс {0} {1}\nЭкьюти: {2} {1} P/L: {3:f2} {1}{4} {5} {6}",
                    acData.Balance.ToStringUniformMoneyFormat(), 
                    acData.Currency,
                    acData.Equity.ToStringUniformMoneyFormat(), 
                    acData.Equity - acData.Balance, 
                    Trades == 0 ? "" : string.Format("({0:f2}% {1:f0}п.)", ProfitInPercents, Points),
                    Trades == 0 ? "" : string.Format("Плечо:{0:f2}", Leverage), 
                    Trades == 0 ? "" : string.Format("Позиций:{0}", Trades), acData.ID);
            }

            return connectionStatus == AccountConnectionStatus.NotConnected ? "не подключен" : "ошибка подключения";                
        }

        public void OnAccountUpdated(Account account)
        {
            connectionStatus = account == null ?
                AccountConnectionStatus.NotConnected : AccountConnectionStatus.Connected;

            if (connectionStatus == AccountConnectionStatus.NotConnected)
                if (onConnectionAborted != null)
                    onConnectionAborted();
            
            AccountData = account;

            if (accountID > 0 && accountID != lastAccountId)
            {
                // вызвать событие - получена информация по счету
                if (onAccountDataFirstReceived != null)
                    onAccountDataFirstReceived();
                lastAccountId = accountID;
            }
        }
    }

    public enum ServerConnectionStatus
    {
        Connecting = 0,
        Connected,
        NotConnected,
        ConnectionAborted
    }

    public enum AccountConnectionStatus
    {
        NotConnected = 0,
        ConnectionError,
        Connected
    }
}

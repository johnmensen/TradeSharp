using System;
using System.Threading;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.BL
{
    /// <summary>
    /// хранит данные, из которых формируется заголовок главного окна
    /// </summary>
    class MainWindowTitle
    {
        #region Singleton

        private static readonly Lazy<MainWindowTitle> instance = new Lazy<MainWindowTitle>(() => new MainWindowTitle());

        public static MainWindowTitle Instance
        {
            get { return instance.Value; }
        }

        private MainWindowTitle()
        {
        }

        #endregion

        #region Событие - смена заголовка
        private Action<string> titleUpdated;
        #endregion

        #region Данные, из которых формируется заголовок

        private string versionInfo;

        private string userTitle;

        public string UserTitle
        {
            set
            {
                if (!locker.TryEnterWriteLock(LockTimeout)) return;
                userTitle = value;
                locker.ExitWriteLock();
                // обновить заголовок
                //MakeTitle();
            }
        }

        private Account account;

        public Account Account
        {
            set
            {
                if (!locker.TryEnterWriteLock(LockTimeout)) return;
                account = value;
                locker.ExitWriteLock();
                // обновить заголовок
                MakeTitle();
            }
        }

        public void Initialize(string versionInfo, Action<string> titleUpdated)
        {
            this.versionInfo = versionInfo;
            this.titleUpdated = titleUpdated;
            titleUpdated(versionInfo);
        }

        #endregion

        #region Формирование заголовка
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        private const int LockTimeout = 1000;

        private void MakeTitle()
        {
            string title;
            if (!locker.TryEnterReadLock(LockTimeout)) return;
            try
            {
                if (account == null)
                    title = versionInfo;
                else
                    title = versionInfo + " №" + account.ID +
                        " (" + account.Equity.ToStringUniformMoneyFormat() + " " + account.Currency + ") / "
                        + userTitle;
            }
            finally
            {
                locker.ExitReadLock();
            }
            titleUpdated(title);
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;

namespace TradeSharp.Contract.Entity
{
    /// <summary>
    /// информация по счетам, которыми управляет, либо имеет в них долю
    /// трейдер
    /// только для реальных счетов
    /// </summary>
    [Serializable]
    public class AccountShared
    {
        public class ComparerOnId : IEqualityComparer<AccountShared>
        {
            private static readonly Lazy<ComparerOnId> instance = new Lazy<ComparerOnId>(() => new ComparerOnId());

            public static ComparerOnId Instance
            {
                get { return instance.Value; }
            }

            public bool Equals(AccountShared x, AccountShared y)
            {
                return x.AccountId.Equals(y.AccountId);
            }

            public int GetHashCode(AccountShared obj)
            {
                return obj.AccountId;
            }
        }

        public Account Account { get; set; }

        public bool IsOwnAccount { get; set; }

        public decimal SharePercent { get; set; }

        public decimal ShareMoney { get; set; }
        
        public decimal ShareMoneyWallet { get; set; }

        #region Поля счета
        public int AccountId { get { return Account.ID; } }

        public string Group { get { return Account.Group; } }

        public string Currency { get { return Account.Currency; } }
        #endregion
    
        public AccountShared()
        {
        }

        public AccountShared(Account account, bool isOwn)
        {
            Account = account;
            IsOwnAccount = isOwn;
        }
    }
}

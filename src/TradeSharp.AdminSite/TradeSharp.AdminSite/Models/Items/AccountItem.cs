using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.BL.Localisation;

namespace TradeSharp.SiteAdmin.Models.Items
{
    public class AccountItem : Account
    {
        /// <summary>
        /// Количество открытых позиций
        /// </summary>
        [LocalizedDisplayName("TitleOpenOrdersCount")]
        public int CountOpenPosition { get; set; }

        /// <summary>
        /// Экспозиция
        /// </summary>
        [LocalizedDisplayName("TitleExposure")]
        public decimal Exposure { get; set; }

        public AccountItem()
        {
        }

        public AccountItem(ACCOUNT account)
        {
            ID = account.ID;
            Currency = account.Currency;
            Group = account.AccountGroup;
            MaxLeverage = (float) account.MaxLeverage;
            Balance = account.Balance;
            TimeCreated = account.TimeCreated;
            TimeBlocked = account.TimeBlocked;
            Status = (AccountStatus) account.Status;
        }
    }
}
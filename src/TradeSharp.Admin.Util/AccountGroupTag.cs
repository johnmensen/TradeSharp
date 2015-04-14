using System.ComponentModel;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Admin.Util
{
    public class AccountGroupTag
    {
        public AccountGroup group;

        [DisplayName("Код")]
        public string Code { get { return group.Code; } }
        
        [DisplayName("Название")]
        public string Name { get { return group.Name; } }

        [DisplayName("Реальный")]
        public string IsReal { get { return group.IsReal ? "реал" : "демо"; } }

        [DisplayName("Стопаут%")]
        public string Stopout { get { return string.Format("{0:f0}%", group.StopoutPercentLevel); } }

        [DisplayName("Дилер")]
        public string DealerCode { get { return group.Dealer == null ? string.Empty : group.Dealer.Code; } }

        [DisplayName("Сессия")]
        public string ProviderSession { get { return group.SessionName; } }

        [DisplayName("Счет брокера")]
        public string HedgingAccount { get; set; }
        
        public AccountGroupTag() {}

        public AccountGroupTag(AccountGroup group)
        {
            this.group = group;            
        }

        public override string ToString()
        {
            return group.Name;
        }
    }
}

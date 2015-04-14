using System.ComponentModel;
using TradeSharp.Linq;

namespace TradeSharp.Admin.Util
{
    public class DealerTag
    {
        public DEALER dealer;

        [DisplayName("Код")]
        public string Code
        {
            get
            {
                return dealer == null ? "Нет" : dealer.Code;
            }
        }

        public DealerTag() {}

        public DealerTag(DEALER dealer)
        {
            this.dealer = dealer;
        }

        public override string ToString()
        {
            return Code;
        }
    }
}

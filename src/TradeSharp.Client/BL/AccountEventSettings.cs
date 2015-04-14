using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.BL
{
    public class AccountEventSettings
    {
        [PropertyXMLTag("EventCode")]
        public AccountEventCode EventCode { get; set; }

        [PropertyXMLTag("EventAction")]
        public AccountEventAction EventAction { get; set; }

        public string CodeName
        {
            get { return EnumFriendlyName<AccountEventCode>.GetString(EventCode); }
        }

        public string ActionName
        {
            get { return EnumFriendlyName<AccountEventAction>.GetString(EventAction); }
        }

        public AccountEventSettings()
        {
        }

        public AccountEventSettings(AccountEventCode eventCode,
            AccountEventAction eventAction)
        {
            EventCode = eventCode;
            EventAction = eventAction;
        }
    }
}

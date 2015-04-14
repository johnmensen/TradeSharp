namespace TradeSharp.Contract.Entity
{
    public class AccountEvent
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public AccountEventCode AccountEventCode { get; set; }

        public AccountEvent(string title, string body, AccountEventCode accountEventCode)
        {
            Title = title;
            Body = body;
            AccountEventCode = accountEventCode;
        }
    }
}
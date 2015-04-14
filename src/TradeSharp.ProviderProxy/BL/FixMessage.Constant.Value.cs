namespace TradeSharp.ProviderProxy.BL
{
    // ReSharper disable InconsistentNaming
    public partial class FixMessage
    {
        #region TAG VALUES
        public const string VALUE_SIDE_BUY = "1";
        public const string VALUE_SIDE_SELL = "2";
        public const string VALUE_ORDTYPE_MARKET = "1";
        public const string VALUE_ORDTYPE_PREVQUOTED = "D";
        public const string VALUE_ORDTYPE_LIMIT = "2";
        public const string VALUE_ORDTYPE_STOP = "3";
        public const string VALUE_PRODUCT_CURRENCY = "4";
        public const string VALUE_HANDLE_INST_AUTO_NOINTERVENT = "1";
        public const string VALUE_TIME_IN_FORCE_DAY = "0";
        public const string VALUE_TIME_IN_FORCE_GTC = "1";
        public const string VALUE_TIME_IN_FORCE_FILL_OR_KILL = "4";

        public const string VALUE_ORDER_NEW = "0";
        public const string VALUE_ORDER_PARTIALLY_FILLED = "1";
        public const string VALUE_ORDER_FILLED = "2";
        public const string VALUE_ORDER_STOPPED = "7";
        public const string VALUE_ORDER_CALCULATED = "B";
        public const string VALUE_ORDER_REJECTED = "8";
        public const string VALUE_ORDER_CANCELED = "4";
        public const string VALUE_ORDER_REPLACED = "5";
        public const string VALUE_ORDER_EXPIRED = "C";

        public const string VALUE_MD_ENTRY_TYPE_BID = "0";
        public const string VALUE_MD_ENTRY_TYPE_OFFER = "1";
        public const string VALUE_MD_ENTRY_TYPE_TRADE = "2";
        public const string VALUE_MD_ENTRY_TYPE_INDEX = "3";
        public const string VALUE_MD_ENTRY_TYPE_OPENING = "4";
        public const string VALUE_MD_ENTRY_TYPE_CLOSING = "5";
        public const string VALUE_MD_ENTRY_TYPE_SETTLEMENT = "6";

        public const string VALUE_ORDER_REJECT_BROKER_EXCHANGE_OPT = "0";

        public const string VALUE_ORDER_REJECT_UNKNOWN_SYMBOL = "1";
        public const string VALUE_ORDER_REJECT_EXCHANGE_CLOSED = "2";
        public const string VALUE_ORDER_REJECT_EXCEEDS_LIMIT = "3";
        public const string VALUE_ORDER_REJECT_TOO_LATE_ENTER = "4";
        public const string VALUE_ORDER_REJECT_UNKNOWN_ORDER = "5";
        public const string VALUE_ORDER_REJECT_DUPLICATE_ORDER = "6";
        public const string VALUE_ORDER_REJECT_DUPLICATE_VERBAL_ORDER = "7";
        public const string VALUE_ORDER_REJECT_STALE_ORDER = "8";
        public const string VALUE_ORDER_REJECT_TRADE_ALONG_REQ = "9";
        public const string VALUE_ORDER_REJECT_INVALID_INVESTOR_ID = "0";
        #endregion
    }
    // ReSharper restore InconsistentNaming
}

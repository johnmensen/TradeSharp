namespace TradeSharp.ProviderProxy.BL
{
    static class FixMessageConstant
    {
        // ReSharper disable InconsistentNaming
        #region TAGS
        public const string TAG_SENDER_ID = "49";
        public const string TAG_SENDER_SUB_ID = "50";
        public const string TAG_TARGET_ID = "56";
        public const string TAG_NO_RELATED_SUM = "146";
        public const string TAG_PRODUCT = "460";
        public const string TAG_SYMBOL = "55";
        public const string TAG_QUOTE_REQUEST_MODE = "303";
        public const string TAG_ORDER_QTY = "38";
        public const string TAG_QUOTE_REQUEST_ID = "131";
        public const string TAG_ACCOUNT = "1";
        public const string TAG_SIDE = "54";
        public const string TAG_TRANSACT_TIME = "60";
        public const string TAG_ORDER_TYPE = "40";
        public const string TAG_PRICE = "44";
        public const string TAG_QUOTE_ID = "117";
        public const string TAG_CLIENT_ORDER_ID = "11";
        public const string TAG_ON_BEHALF_ID = "115";
        public const string TAG_FUT_SET_DATE = "64";
        public const string TAG_HANDLE_INST = "21";
        public const string TAG_STOP_PX = "99";
        public const string TAG_TIME_IN_FORCE = "59";
        public const string TAG_EXEC_INST = "18";
        public const string TAG_ORIG_CL_ORDER_ID = "41";
        public const string TAG_ORDER_ID = "37";
        public const string TAG_MD_REQUEST_ID = "262";
        public const string TAG_SUBSCRIPT_REQUEST_TYPE = "263";
        public const string TAG_MARKET_DEPTH = "264";
        public const string TAG_MD_UPDATE_TYPE = "265";
        public const string TAG_MD_NUM_ENTRIES = "267";
        public const string TAG_MD_ENTRY_TYPE = "269";
        public const string TAG_MD_ENTRY_PX = "270";
        public const string TAG_POS_REQ_ID = "710";
        public const string TAG_POS_REQ_TYPE = "724";
        public const string TAG_ACCOUNT_TYPE = "581";
        public const string TAG_CUM_QTY = "14";
        public const string TAG_SENDING_TIME = "52";
        #endregion

        #region DeutscheBankTags
        public const string TAG_FXCM_POS_ID = "9041";
        public const string TAG_FXCM_INTEREST_FEE = "9040";
        public const string TAG_FXCM_COMMISSION = "9053";
        public const string TAG_FXCM_PROFIT = "9052";
        #endregion

        #region TAG VALUES
        public const string VALUE_SIDE_BUY = "1";
        public const string VALUE_SIDE_SELL = "2";
        public const string VALUE_ORDTYPE_MARKET = "1";
        public const string VALUE_ORDTYPE_PREVQUOTED = "D";
        public const string VALUE_ORDTYPE_LIMIT = "2";
        public const string VALUE_ORDTYPE_STOP = "3";
        public const string VALUE_PRODUCT_AGENCY = "1";
        public const string VALUE_PRODUCT_COMMODITY = "2";
        public const string VALUE_PRODUCT_CORPORATE = "3";
        public const string VALUE_PRODUCT_CURRENCY = "4";
        public const string VALUE_PRODUCT_EQUITY = "5";
        public const string VALUE_PRODUCT_GOVERNMENT = "6";
        public const string VALUE_PRODUCT_INDEX = "7";
        public const string VALUE_PRODUCT_LOAN = "8";
        public const string VALUE_PRODUCT_MONEYMARKET = "9";
        public const string VALUE_PRODUCT_MORTGAGE = "10";
        public const string VALUE_PRODUCT_MUNICIPAL = "11";
        public const string VALUE_PRODUCT_OTHER = "12";
        public const string VALUE_PRODUCT_FINANCING = "13";
        public const string VALUE_HANDLE_INST_AUTO_NOINTERVENT = "1";
        public const string VALUE_TIME_IN_FORCE_DAY = "0";
        public const string VALUE_TIME_IN_FORCE_GTC = "1";
        public const string VALUE_TIME_IN_FORCE_FILL_OR_KILL = "4";

        public const string VALUE_ORDER_PARTIALLY_FILLED = "1";
        public const string VALUE_ORDER_FILLED = "2";
        public const string VALUE_ORDER_STOPPED = "7";
        public const string VALUE_ORDER_CALCULATED = "B";
        public const string VALUE_ORDER_REJECTED = "8";
        public const string VALUE_ORDER_NEW = "0";
        public const string VALUE_ORDER_CANCELED = "4";
        public const string VALUE_ORDER_REPLACED = "5";
        public const string VALUE_ORDER_EXPIRED = "C";
        #endregion

        #region MSG TYPES
        public const string MSG_TYPE_QUOTE_REQUEST = "R";
        public const string MSG_TYPE_NEW_ORDER = "D";
        public const string MSG_TYPE_ORDER_CANCEL_REPLACE = "G";
        public const string MSG_TYPE_ORDER_CANCEL = "F";
        public const string MSG_TYPE_MARKET_DATA_REQUEST = "V";
        public const string MSG_TYPE_MARKET_DATA = "W";
        public const string MSG_TYPE_REQUEST_POSITIONS = "AN";
        public const string MSG_TYPE_POSITION_REPORT = "AP";
        #endregion
        // ReSharper restore InconsistentNaming
    }
}

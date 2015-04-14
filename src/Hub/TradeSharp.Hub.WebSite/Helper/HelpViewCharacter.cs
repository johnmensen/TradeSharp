namespace TradeSharp.Hub.WebSite.Helper
{
    /// <summary>
    /// Класс содержит вспомогательные символы, например разделители для dll
    /// </summary>
    public static class HelpViewCharacter
    {
        private const string DropDownPrefixChar = "ddl-";
        public static string DropDownPrefix
        {
            get { return DropDownPrefixChar; }
        }

        private const string TextPrefixChar = "txt-";
        public static string TextPrefix
        {
            get { return TextPrefixChar; }
        }

        private const string DividerChar = "-";
        public static string Divider
        {
            get { return DividerChar; }
        }

        private const string SuccessOperationMessage = "Операция выполнена успешно.";
        public static string SuccessOperation
        {
            get { return SuccessOperationMessage; }
        }

        private const string FailOperationMessage = "При выполнении операции произошла ошибка. Подробности см. в логах.";
        public static string FailOperation
        {
            get { return FailOperationMessage; }
        }
    }
}

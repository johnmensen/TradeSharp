using TradeSharp.Chat.Client.BL;

namespace TradeSharp.Chat.Client.Control
{
    /// <summary>
    /// содержит делегаты, заглушки для методов, которые вызывает
    /// терминал
    /// </summary>
    public partial class ChatControl
    {
        /// <summary>
        /// флаг взведен изначально, если чат "встроен" в окно терминала
        /// </summary>
        public static bool IsEmbedded { get; set; }

        /// <summary>
        /// вызывается главным окном единожды
        /// </summary>
        public static void SetupChatEnvironment(GetAllPlatformUsersDel getPlatformUsers, bool isEmbeded)
        {
            IsEmbedded = isEmbeded;
            AllUsers.GetAllPlatformUsers = getPlatformUsers;
        }
    }
}

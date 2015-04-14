namespace TradeSharp.Server.BL
{
    interface IUserSettingsStorage
    {
        void SaveUserSettings(int userId, string settingsString);
        string LoadUserSettings(int userId);
    }
}

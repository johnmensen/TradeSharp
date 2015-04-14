using TradeSharp.Util;

namespace TradeSharp.Client.BL
{
    public class TerminalEnvironment
    {
        /// <summary>
        /// в режиме offline нет лишних запросов к серверу
        /// </summary>
        public static bool workOffline = AppConfig.GetBooleanParam("Offline", false);

        /// <summary>
        /// путь к каталогу котировок (кеш) относительно пути к *.exe
        /// </summary>
        public const string QuoteCacheFolder = "\\quotes";
        public const string NewsCacheFolder = "\\news";
        public const string RobotCacheFolder = "\\robots";
        public const string FileCacheFolder = "\\files";
        public const string ChatHistoryFolder = "\\chat";
        
        /// <summary>
        /// путь к файлу пользовательских настроек
        /// файл может быть переписан программой
        /// </summary>
        public const string UserSettingsFileName = "\\settings.xml";

        /// <summary>
        /// путь к портфелю роботов, торгующих в реальном времени
        /// </summary>
        public const string LiveRobotsPortfolioPath = "\\robots.pxml";

        /// <summary>
        /// путь к файлу справки
        /// </summary>
        public const string HelpFile = "\\terminal.chm";
    
        /// <summary>
        /// получение абсолютного пути каталога
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public static string GetFullPath(string folderName)
        {
            return ExecutablePath.ExecPath + folderName;
        }
    }
}

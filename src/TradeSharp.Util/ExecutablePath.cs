using System.IO;
using System.Reflection;

namespace TradeSharp.Util
{
    public static class ExecutablePath
    {
        private static string execFileName;

        private static string execPath;
        /// <summary>
        /// путь к исполняемому файлу без завершающего слэш
        /// </summary>
        public static string ExecPath
        {
            get
            {
                if (string.IsNullOrEmpty(execPath)) SetExecAppProperty();
                return execPath;
            }
        }

        /// <summary>
        /// имя исполняемого файла
        /// </summary>
        public static string ExecFileName
        {
            get
            {
                if (string.IsNullOrEmpty(execFileName)) SetExecAppProperty();
                return execFileName;
            }
        }

        private static void SetExecAppProperty()
        {
            var sm = Assembly.GetEntryAssembly() ??
                             Assembly.GetExecutingAssembly();
            execPath = Path.GetDirectoryName(sm.Location);
            execFileName = Path.GetFileName(sm.Location);
        }

        public static void InitializeFake(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
                execPath = directoryName != null ? directoryName.Replace("file:\\", "") : string.Empty;
            }
            else
                execPath = path;
        }

        public static void Unitialize()
        {
            execPath = string.Empty;
        }
    }
}

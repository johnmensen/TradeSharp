using System;
using System.IO;
using System.Text;

namespace TradeSharp.Util
{
    public static class HiddenModes
    {
        private static bool? managerMode;
        /// <summary>
        /// режим менеджера, доступны определенные опции 
        /// (например, выкладка прогнозов на сайт)
        /// </summary>
        public static bool ManagerMode
        {
            get
            {
                if (managerMode.HasValue) return managerMode.Value;
                try
                {
                    var fileName = ExecutablePath.ExecPath +
                                   "\\managerkey.txt";
                    if (!File.Exists(fileName))
                        managerMode = false;
                    else
                    {
                        using (var sr = new StreamReader(fileName, Encoding.ASCII))
                        {
                            managerMode = sr.ReadToEnd() == "adminmodeison";
                        }
                    }
                }
                catch (Exception)
                {
                    managerMode = false;
                }
                return managerMode.Value;
            }
        }
    }
}

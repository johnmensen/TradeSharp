using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TradeSharp.Util;

namespace TradeSharp.DealerInterface
{
    /// <summary>
    /// читает настройки дилера из файла конфигурации
    /// </summary>
    public class DealerConfigParser
    {
        private readonly Type callerAsmType;
        private readonly Dictionary<string, string> keyValues = new Dictionary<string, string>();
        private string dealerFileName;

        public DealerConfigParser(Type callerAsmType, string dealerFileName)
        {
            this.callerAsmType = callerAsmType;
            this.dealerFileName = dealerFileName;
            var fileName = MakeConfFileName(callerAsmType, dealerFileName);
            if (!File.Exists(fileName)) return;
            
            try
            {
                using (var sr = new StreamReader(fileName, Encoding.UTF8))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (string.IsNullOrEmpty(line)) continue;
                        var tab = line.IndexOf((char) 9);
                        if (tab <= 0) continue;
                        var key = line.Substring(0, tab).Trim();
                        var value = line.Substring(tab + 1);
                        if (!keyValues.ContainsKey(key))
                            keyValues.Add(key, value);
                        else
                            Logger.DebugFormat("Файл конфигурации \"{0}\": повторяющийся ключ \"{1}\"", 
                                fileName, key);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка чтения файла конфигурации \"{0}\": {1}", fileName, ex);
            }
        }

        private static string MakeConfFileName(Type callerAsmType, string dealerFileName)
        {
            var fullPath = System.Reflection.Assembly.GetAssembly(callerAsmType).Location;
            // заменить имя файла
            var folder = Path.GetDirectoryName(fullPath);
            fullPath = string.Format("{0}\\{1}", folder, dealerFileName);
            Logger.InfoFormat("cfg dll is {0}", fullPath);
            return fullPath.Replace(".dll", ".txt");
        }        
    
        private void SaveConfig()
        {
            var fileName = MakeConfFileName(callerAsmType, dealerFileName);
            try
            {
                using (var sw = new StreamWriter(fileName, false, Encoding.UTF8))
                {
                    foreach (var keyValue in keyValues)
                    {
                        sw.WriteLine(string.Format("{0}{1}{2}",
                            keyValue.Key, (char)9, keyValue.Value));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка записи файла конфигурации \"{0}\": {1}", fileName, ex);
            }
        }

        #region Get accessors
        public string GetString(string key, string defaultValue)
        {
            string val;
            if (!keyValues.TryGetValue(key, out val)) return defaultValue;
            return val;
        }
        
        public int GetInt(string key, int defaultValue)
        {
            string val;
            if (!keyValues.TryGetValue(key, out val)) return defaultValue;
            return val.ToIntSafe() ?? defaultValue;
        }

        public bool GetBool(string key, bool defaultValue)
        {
            string val;
            if (!keyValues.TryGetValue(key, out val)) return defaultValue;
            return val.ToBoolSafe() ?? defaultValue;
        }

        public decimal GetDecimal(string key, decimal defaultValue)
        {
            string val;
            if (!keyValues.TryGetValue(key, out val)) return defaultValue;
            return val.ToDecimalUniformSafe() ?? defaultValue;
        }

        public DateTime GetDateTime(string key, DateTime defaultValue)
        {
            string val;
            if (!keyValues.TryGetValue(key, out val)) return defaultValue;
            return val.ToDateTimeUniformSafe() ?? defaultValue;
        }
        #endregion

        #region Set accessors
        public void SetString(string key, string value)
        {
            if (keyValues.ContainsKey(key)) keyValues[key] = value;
            else keyValues.Add(key, value);
            SaveConfig();
        }
        
        public void SetInt(string key, int value)
        {
            SetString(key, value.ToString());
        }

        public void SetBool(string key, bool value)
        {
            SetString(key, value.ToString());
        }

        public void SetDecimal(string key, decimal value)
        {
            SetString(key, value.ToStringUniform());
        }

        public void SetDateTime(string key, DateTime value)
        {
            SetString(key, value.ToStringUniform());
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.ProviderProxy.BL
{
    public class SessionSettingsParser
    {
        public const string SessionSettingsFileName = "SessionSettings.txt";

        private int sessionCounter = 1;
        /// <summary>
        /// key: session name (DEFAULT, 1, 2, ...)
        /// value: key-value collection
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, string>> rawParams = new Dictionary<string, Dictionary<string, string>>();

        private readonly Dictionary<SessionSettingsKey, Dictionary<string, string>> sessionParams = new Dictionary<SessionSettingsKey, Dictionary<string, string>>();

        private static SessionSettingsParser instance;
        public static SessionSettingsParser Instance
        {
            get
            {
                if (instance == null)
                    throw new Exception(
                        "SessionSettingsParser не инициализирован (SessionSettingsParser.Init)");
                return instance;
            }
        }

        /// <returns>sender ID - target ID - param value</returns>
        public List<Cortege3<string, string, string>> GetParamInAllSections(string paramName, string defaultValue)
        {
            var ptrs = new List<Cortege3<string, string, string>>();
            foreach (var key in sessionParams.Keys)
            {
                var dic = sessionParams[key];
                var val = dic.ContainsKey(paramName) ? dic[paramName] : defaultValue;
                ptrs.Add(new Cortege3<string, string, string>(key.senderCompID, key.targetCompID, val));
            }
            return ptrs;
        }

        public string GetSessionParam(string targetID, string senderID, string paramName,
            string defaultValue)
        {
            var defaultSets = sessionParams[new SessionSettingsKey()]; // default params
            Dictionary<string, string> sets = null;
            var fullKey = new SessionSettingsKey { senderCompID = senderID, targetCompID = targetID };
            var senderKey = new SessionSettingsKey { senderCompID = senderID };
            var targetKey = new SessionSettingsKey { targetCompID = targetID };

            if (sessionParams.ContainsKey(fullKey))
                sets = sessionParams[fullKey];
            else
                if (sessionParams.ContainsKey(senderKey))
                    sets = sessionParams[senderKey];
                else
                    if (sessionParams.ContainsKey(targetKey))
                        sets = sessionParams[targetKey];
            if (sets != null)
                if (sets.ContainsKey(paramName)) return sets[paramName];
            if (defaultSets.ContainsKey(paramName)) return defaultSets[paramName];
            return defaultValue;
        }

        private SessionSettingsParser(string fileName)
        {
            if (!File.Exists(fileName))
                throw new Exception(string.Format("Файл {0} не найден", fileName));
            string curSession = "";
            using (var sr = new StreamReader(fileName))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line)) continue;
                    ParseFileLine(line, ref curSession);
                }
            }
            MakeSessionFromRawParams();
        }

        private void MakeSessionFromRawParams()
        {
            foreach (var sessionName in rawParams.Keys)
            {
                var sessSetts = rawParams[sessionName];
                var sessKey = new SessionSettingsKey();
                if (sessSetts.ContainsKey("TargetCompID"))
                    sessKey.targetCompID = sessSetts["TargetCompID"];
                if (sessSetts.ContainsKey("SenderCompID"))
                    sessKey.senderCompID = sessSetts["SenderCompID"];
                sessionParams.Add(sessionName == "DEFAULT"
                    ? new SessionSettingsKey() : sessKey, sessSetts);
            }
        }

        private void ParseFileLine(string line, ref string curSession)
        {
            line = line.Trim();
            if (string.IsNullOrEmpty(line)) return;
            if (line.StartsWith("#")) return;
            if (line.StartsWith("["))
            {
                curSession = line.Substring(1, line.Length - 2);
                if (curSession == "SESSION")
                    curSession = string.Format("{0}", sessionCounter++);
                return;
            }
            var parts = line.Split('=');
            if (!rawParams.ContainsKey(curSession))
                rawParams.Add(curSession, new Dictionary<string, string>());
            var sesPtrs = rawParams[curSession];
            sesPtrs.Add(parts[0], parts[1]);
        }

        public static void Init(string fileName)
        {
            instance = new SessionSettingsParser(fileName);
        }
    }

    public struct SessionSettingsKey
    {
        public string targetCompID;
        public string senderCompID;
    }
}

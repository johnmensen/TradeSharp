using System;
using System.Collections.Generic;
using System.Text;
using TradeSharp.Util;

namespace TradeSharp.ProviderProxy.BL
{
    /// <summary>
    /// класс описывает команду FIX
    /// типизированное представление FIX-сообщения
    /// </summary>
    public partial class FixMessage
    {
        private static FixVersion fixVersion = FixVersion.Undefined;
        public static FixVersion FixVersion
        {
            get
            {
                if (fixVersion == FixVersion.Undefined)
                {
                    fixVersion = (FixVersion)Enum.Parse(typeof(FixVersion),
                        AppConfig.GetStringParam("FIX.Version", "Fix43"));
                }
                return fixVersion;
            }
        }

        private FixMessageType messageType = FixMessageType.Undefined;
        /// <summary>
        /// Тип сообщения (enum)
        /// </summary>
        public FixMessageType MessageType
        {
            get { return messageType; }
        }

        private string messageTypeString;
        /// <summary>
        /// Тип сообщения - строка (tag 35)
        /// </summary>
        public string MessageTypeString
        {
            get { return messageTypeString; }
            set
            {
                messageTypeString = value;
                var tp = FixMessageTypeFormatter.GetMessageType(messageTypeString);
                if (tp.HasValue) messageType = tp.Value;
            }
        }

        public string SessionId { get; set; }
        
        /// <summary>
        /// tag = value
        /// </summary>
        public Dictionary<string, string> fieldValues = new Dictionary<string, string>();
        
        /// <summary>
        /// доступ к полям по имени тега (строка)
        /// </summary>
        /// <param name="key">имя тега</param>
        /// <returns>значение тега</returns>
        public string this[string key]
        {
            get
            {
                return fieldValues[key];
            }
            set
            {
                if (fieldValues.ContainsKey(key))
                    fieldValues[key] = value;
                else
                    fieldValues.Add(key, value);
            }
        }

        public FixMessage(string msgText, string sessionId)
        {
            SessionId = sessionId;
            // распарсить
            var parts = msgText.Split(new[] {(char) 1}, StringSplitOptions.RemoveEmptyEntries);            
            foreach (var part in parts)
            {
                var keyVal = part.Split('=');
                if (keyVal.Length != 2) continue;
                if (!fieldValues.ContainsKey(keyVal[0])) fieldValues.Add(keyVal[0], keyVal[1]);                    
            }            
            // установить тип сообщения
            if (fieldValues.ContainsKey(TAG_MSG_TYPE)) 
                MessageTypeString = fieldValues[TAG_MSG_TYPE];
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var tag in fieldValues.Keys)
            {
                sb.AppendFormat("{0}={1}#", tag, fieldValues[tag]);
            }
            return sb.ToString();
        }

        public string GetValueString(string key)
        {
            string val;
            if (!fieldValues.TryGetValue(key, out val)) return null;
            return val;
        }

        public int? GetValueInt(string key)
        {
            string val;
            if (!fieldValues.TryGetValue(key, out val)) return null;
            int result;
            if (int.TryParse(val, out result)) return result;
            return null;
        }

        public decimal? GetValueDecimal(string key)
        {
            string val;
            if (!fieldValues.TryGetValue(key, out val)) return null;
            return val.ToDecimalUniformSafe();
        }

        public float? GetValueFloat(string key)
        {
            string val;
            if (!fieldValues.TryGetValue(key, out val)) return null;
            return val.ToFloatUniformSafe();
        }
    }    
}

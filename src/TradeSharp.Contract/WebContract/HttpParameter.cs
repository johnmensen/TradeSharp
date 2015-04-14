using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using TradeSharp.Util;

namespace TradeSharp.Contract.WebContract
{
    /// <summary>
    /// базовый класс для запроса к сервису, передаваемому в
    /// HTTP POST и сериализуемому в JSON
    /// </summary>
    [DataContract]
    [KnownType(typeof(RegisterAccountQuery))]
    [KnownType(typeof(TradeSharpServiceStartStop))]
    [KnownType(typeof(TradeSharpServiceProcess))]
    [KnownType(typeof(ExecutionReport))]
    [KnownType(typeof(TerminalUser))]
    [KnownType(typeof(ChangeAccountBalanceQuery))]
    public abstract class HttpParameter
    {
        private static readonly Encoding encoding = Encoding.UTF8;

        public static string SerializeInJSon(IEnumerable<HttpParameter> httpParams)
        {
            var ser = new DataContractJsonSerializer(typeof(List<HttpParameter>));
            using (var ms = new MemoryStream())
            {                
                ser.WriteObject(ms, httpParams);
                return encoding.GetString(ms.GetBuffer(), 0, (int)ms.Length);
            }
        }

        public static List<HttpParameter> DeserializeFromJSon(string strJson)
        {
            var values = new List<HttpParameter>();
            if (string.IsNullOrEmpty(strJson)) return values;

            try
            {
                var ser = new DataContractJsonSerializer(typeof(List<HttpParameter>));
                using (var ms = new MemoryStream(encoding.GetBytes(strJson)))
                {
                    var obj = ser.ReadObject(ms);
                    if (obj == null) return values;
                    if (obj is List<HttpParameter>) return (List<HttpParameter>)obj;
                    if (obj is HttpParameter)
                    {
                        values.Add((HttpParameter)obj);
                        return values;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка в DeserializeFromJSon({0}): {1}",
                    strJson.Length > 255 ? strJson.Substring(0, 255) + "..." : strJson,
                    ex);
            }
            
            return values;
        }

        public static List<HttpParameter> DeserializeServerResponse(string url, List<HttpParameter> inputParams,
            out string rawData,
            string userName = "", string password = "")
        {
            rawData = string.Empty;
            var req = WebRequest.Create(url);
            if (!string.IsNullOrEmpty(userName))
                req.Credentials = new NetworkCredential(userName, password);

            if (inputParams != null && inputParams.Count > 0)
            {
                req.Method = "POST";
                using (var streamWr = req.GetRequestStream())
                using (var writer = new StreamWriter(streamWr, encoding))
                {
                    writer.Write(SerializeInJSon(inputParams));
                }
            }
            
            string strData;
            
            var resp = req.GetResponse();
            using (var stream = resp.GetResponseStream())
            {
                if (stream == null) return new List<HttpParameter>();
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    strData = reader.ReadToEnd();
                    if (string.IsNullOrEmpty(strData)) return new List<HttpParameter>();
                }
            }

            try
            {
                return DeserializeFromJSon(strData);                        
            }
            catch
            {
                rawData = strData;
                return new List<HttpParameter>();
            }
        }
    }
}

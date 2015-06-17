using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TradeSharp.RobotFarm.Request
{
    public class JsonRequest
    {
        [JsonProperty(PropertyName = "RequestId")]
        public long RequestId { get; set; }

        [JsonProperty(PropertyName = "RequestType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public JsonRequestType RequestType { get; set; }

        private static readonly Dictionary<string, Type> commandByName =
            new Dictionary<string, Type>
            {
                {JsonRequestType.RequestAccount.ToString(), typeof (RequestAccounts)},
                {JsonRequestType.LastOrders.ToString(), typeof (RequestLastOrders)},
                {JsonRequestType.PositionsCLosing.ToString(), typeof (RequestPositionsClosing)},
                {JsonRequestType.ActualizeAccounts.ToString(), typeof (RequestAccountActualizing)}
            };

        private static readonly Dictionary<Type, Type> responseByRequest = new Dictionary<Type, Type>();
        
        private static readonly Regex commandNameRegex = new Regex("(?<=\"RequestType\":\")[a-z,A-Z,_,0-9]+");

        static JsonRequest()
        {
            var asm = typeof(JsonRequest).Assembly;
            foreach (var type in asm.GetTypes())
            {
                var attr = type.GetCustomAttributes(typeof(JsonResponseTypeAttribute), true).FirstOrDefault()
                    as JsonResponseTypeAttribute;
                if (attr == null)
                    continue;
                responseByRequest.Add(type, attr.ResponseType);
            }
        }

        public static JsonRequest ParseCommand(string commandJson)
        {
            if (string.IsNullOrEmpty(commandJson))
                return null;

            var nameMatch = commandNameRegex.Match(commandJson);
            if (string.IsNullOrEmpty(nameMatch.Value))
                return null;

            var cmdName = nameMatch.Value;
            Type commandType;
            if (!commandByName.TryGetValue(cmdName, out commandType))
                return null; // команда не распознана

            try
            {
                var cmd = (JsonRequest)JsonConvert.DeserializeObject(commandJson, commandType);
                return cmd;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Type GetResponseType()
        {
            var thisType = GetType();
            Type t;
            if (responseByRequest.TryGetValue(thisType, out t))
                return t;
            return null;
        }
    }

    public enum JsonRequestType
    {
        RequestAccount = 0,
        LastOrders = 1,
        PositionsCLosing = 2,
        ActualizeAccounts = 3
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class JsonResponseTypeAttribute : Attribute
    {
        public Type ResponseType { get; set; }

        public JsonResponseTypeAttribute(Type responseType)
        {
            ResponseType = responseType;
        }
    }    
}

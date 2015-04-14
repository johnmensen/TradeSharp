using System;
using System.Collections.Generic;
using System.Globalization;

namespace TestMTTradeServer.BL
{
    public  static class DealerLogic
    {
        public enum ResponseStatus
        { 
            RequestCompleted = 10, 
            RequestFailed = 20, 
            RequestWrongParams = 30, 
            RequestWrongPrice = 40,
            RequestToClose = 50,
            OrderSLExecuted = 60,
            OrderTPExecuted = 70
        };

        /// <summary>
        /// requestId=3;group=FXI_hedged;time=65704142;login=1000002;type=CLOSE;side=1;order=0;orderby=0;price=1.455;symbol=EURUSD;volume=20;tp=0.;sl=0.;slippage=0<
        /// </summary>        
        public static string ResponseOnMessage(string msg, int shiftPoints)
        {
            var provider = CultureInfo.InvariantCulture;

            var values = ParseStr(msg);

            var id = values.ContainsKey("requestId") ? int.Parse(values["requestId"]) : 0;
            var login = values.ContainsKey("login") ? values["login"] : "undefined";
            var price = values.ContainsKey("price") ? double.Parse(values["price"], provider) : 0D;
            var tp = values.ContainsKey("tp") ? double.Parse(values["tp"], provider) : 0D;
            var sl = values.ContainsKey("sl") ? double.Parse(values["sl"], provider) : 0D;
            var symbol = values.ContainsKey("symbol") ? values["symbol"] : "";
            var cmdtype = values.ContainsKey("type") ? values["type"] : "";
            var side = values.ContainsKey("side") ? int.Parse(values["side"]) : 0;
            var order = values.ContainsKey("order") ? int.Parse(values["order"]) : 0;

            if (cmdtype != "OPEN" && cmdtype != "CLOSE" && cmdtype != "SL" && cmdtype != "TP") return "";

            var responseType = cmdtype == "SL"
                                   ? ResponseStatus.OrderSLExecuted
                                   : cmdtype == "TP" ? ResponseStatus.OrderTPExecuted : ResponseStatus.RequestCompleted;
            price += side * shiftPoints * 0.0001;

            var requestId = cmdtype == "CLOSE" || cmdtype == "SL" || cmdtype == "TP" ? order : id;
            return string.Format("requestId={0};price={1};status={5};sl={2};tp={3};comment={4};",
                requestId, price.ToString(provider), sl, tp, 
                DateTime.Now.ToString("ddHHmmss"), (int)responseType);
        }

        private static Dictionary<string, string> ParseStr(string val)
        {
            var dict = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(val)) return dict;
            var parts = val.Split(';');
            foreach (var part in parts)
            {
                var valParts = part.Split('=');
                if (valParts.Length != 2) continue;
                dict.Add(valParts[0], valParts[1]);
            }
            return dict;
        }
    }
}

using System;
using Entity;

namespace TradeSharp.SiteBridge.Lib.Distribution
{
    public enum ClientRequestType
    {
        Auth,
        Profit_1000,
        Effectiveness,
        Positions,
        OpenPositions,
        Portfolio
    }
    
    public abstract class ClientRequest
    {
        [PropertyXMLTag("Id", "-1")]
        public int RequestID { get; set; }

        public ClientRequestType requestType { get; protected set; }
    }

    public abstract class ClientRequestAccount : ClientRequest
    {
        [PropertyXMLTag("account")]
        public string AccountID { get; set; }
    }

    public class ClientRequestAuth : ClientRequest
    {
        [PropertyXMLTag("login")]
        public string Login { get; set; }

        [PropertyXMLTag("password")]
        public string Password { get; set; }

        public ClientRequestAuth()
        {
            requestType = ClientRequestType.Auth;
        }
    }

    public class ClientRequestProfit1000 : ClientRequestAccount
    {
        public ClientRequestProfit1000()
        {
            requestType = ClientRequestType.Profit_1000;
        }
    }

    public class ClientRequestEffectiveness : ClientRequestAccount
    {
        public ClientRequestEffectiveness()
        {
            requestType = ClientRequestType.Effectiveness;
        }
    }

    public class ClientRequestClosedPositions : ClientRequestAccount
    {
        [PropertyXMLTag("date_from", FormatString = "dd.MM.yyyy HH:mm")]
        public DateTime DateFrom { get; set; }
        
        [PropertyXMLTag("date_to", FormatString = "dd.MM.yyyy HH:mm")]
        public DateTime DateTo { get; set; }

        public ClientRequestClosedPositions()
        {
            requestType = ClientRequestType.Positions;
        }
    }

    public class ClientRequestOpenPositions : ClientRequestAccount
    {
        [PropertyXMLTag("pair")]
        public string pair { get; set; }

        public ClientRequestOpenPositions()
        {
            requestType = ClientRequestType.OpenPositions;
        }
    }

    public class ClientRequestPortfolio : ClientRequestAccount
    {
        public ClientRequestPortfolio()
        {
            requestType = ClientRequestType.Portfolio;
        }
    }
}

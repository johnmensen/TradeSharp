namespace TradeSharp.RobotFarm.Request
{
    [JsonResponseType(typeof(JsonResponseAccounts))]
    public class RequestAccounts : JsonRequest
    {
        public RequestAccounts()
        {
            RequestType = JsonRequestType.RequestAccount;
        }

        public RequestAccounts(long requestId)
            : this()
        {
            RequestId = requestId;
        }
    }
}

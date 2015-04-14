namespace TradeSharp.RobotFarm.Request
{
    [JsonResponseType(typeof(JsonResponseLastOrders))]
    public class RequestLastOrders : JsonRequest
    {
        public RequestLastOrders()
        {
            RequestType = JsonRequestType.LastOrders;
        }

        public RequestLastOrders(long requestId)
            : this()
        {
            RequestId = requestId;
        }
    }
}

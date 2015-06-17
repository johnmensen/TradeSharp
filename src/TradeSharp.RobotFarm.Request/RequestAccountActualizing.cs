namespace TradeSharp.RobotFarm.Request
{
    [JsonResponseType(typeof(JsonResponse))]
    public class RequestAccountActualizing : JsonRequest
    {
        public RequestAccountActualizing()
        {
            RequestType = JsonRequestType.ActualizeAccounts;
        }

        public RequestAccountActualizing(long requestId)
            : this()
        {
            RequestId = requestId;
        }
    }
}

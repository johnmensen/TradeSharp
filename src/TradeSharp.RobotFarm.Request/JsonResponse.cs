namespace TradeSharp.RobotFarm.Request
{
    public class JsonResponse
    {
        public bool Success { get; set; }

        public string ErrorString { get; set; }

        public long RequestId { get; set; }

        public JsonResponse()
        {            
        }

        public JsonResponse(bool success)
        {
            Success = success;
        }

        public JsonResponse(bool success, string errorString)
        {
            Success = success;
            ErrorString = errorString;
        }
    }
}

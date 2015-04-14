using System.Collections.Generic;

namespace TradeSharp.RobotFarm.Request
{
    [JsonResponseType(typeof(JsonResponsePositionsClosing))]
    public class RequestPositionsClosing : JsonRequest
    {
        public List<Position> positions = new List<Position>();

        public RequestPositionsClosing()
        {
            RequestType = JsonRequestType.RequestAccount;
        }

        public RequestPositionsClosing(long requestId)
            : this()
        {
            RequestId = requestId;
        }
    }
}

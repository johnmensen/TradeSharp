using System.Collections.Generic;

namespace TradeSharp.RobotFarm.Request
{
    public class JsonResponseLastOrders : JsonResponse
    {
        public Dictionary<int, List<Position>> AccountPositions { get; set; }
    }
}

namespace TradeSharp.RobotFarm.Request
{
    public class JsonResponsePositionsClosing : JsonResponse
    {
        public int CountClosed { get; set; }

        public int CountFail { get; set; }
    }
}

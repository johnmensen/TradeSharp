namespace TradeSharp.RobotFarm.BL
{
    public class FarmAccountArg
    {
        public int CurrentAccountId { get; set; }

        public int AccountId { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public bool TradeEnabled { get; set; }
    }
}

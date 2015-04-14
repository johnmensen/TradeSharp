using System.Collections.Generic;

namespace TradeSharp.RobotFarm.Request
{
    public class JsonResponseAccounts : JsonResponse
    {
        public class AccountRobot
        {
            public string Name { get; set; }

            public string TickerTimeframe { get; set; }
        }

        public class Account
        {
            public int Id { get; set; }

            public string Login { get; set; }

            public string Password { get; set; }

            public decimal Balance { get; set; }

            public decimal Equity { get; set; }

            public List<AccountRobot> Robots { get; set; } 
        }

        public List<Account> Accounts { get; set; }

        public FarmState FarmState { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TradeSharp.Client.BL
{
    class ForexInvestResponse
    {

        public string error { get; set; }
        public ForexInvestResponseResult result { get; set; }
    }

    class ForexInvestResponseResult
    {
        public string token { get; set; }
    }
}

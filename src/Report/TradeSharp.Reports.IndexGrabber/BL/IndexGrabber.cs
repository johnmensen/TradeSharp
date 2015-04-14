using System;
using System.Collections.Generic;
using System.ServiceModel;
using Entity;
using TradeSharp.Reports.Lib.Contract;
using TradeSharp.Reports.Lib.IndexGrabber;
using TradeSharp.Util;

namespace TradeSharp.Reports.IndexGrabber.BL
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class IndexGrabber : IIndexGrabber
    {
        private static IndexGrabber instance;
        public static IndexGrabber Instance
        {
            get { return instance ?? (instance = new IndexGrabber()); }
        }

        public IndexStorage indexStorage;

        private IndexGrabber()
        {
        }

        public Dictionary<string, List<Cortege2<DateTime, float>>> GetIndexData()
        {
            return indexStorage.GetTickerData();
        }
    }
}

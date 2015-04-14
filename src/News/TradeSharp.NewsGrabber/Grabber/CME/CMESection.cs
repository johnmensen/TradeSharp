using System.Collections.Generic;

namespace TradeSharp.NewsGrabber.Grabber.CME
{
    public class CMEFutureInfo
    {
        /// <summary>
        /// EURUSD
        /// </summary>
        public string SpotSymbol { get; set; }

        /// <summary>
        /// EURO FX
        /// </summary>
        public string BulletinReference { get; set; }

        /// <summary>
        /// 1000 (1455 = 1.455)
        /// </summary>
        public decimal StrikeToBaseRatio { get; set; }

        public CMEFutureInfo(string spotSymbol, string bulletinReference, decimal strikeToBase)
        {
            SpotSymbol = spotSymbol;
            BulletinReference = bulletinReference;
            StrikeToBaseRatio = strikeToBase;
        }
    }

    public class CMESectionInfo
    {
        public static readonly Dictionary<int, List<CMEFutureInfo>> sections =
            new Dictionary<int, List<CMEFutureInfo>>
                {
                    { 39, new List<CMEFutureInfo> { new CMEFutureInfo("EURUSD", "EURO FX", 1000)} },
                    { 38, new List<CMEFutureInfo> { new CMEFutureInfo("AUDUSD", "AUST DLR", 1000),
                                                    new CMEFutureInfo("NZDUSD", "NEW ZEALND", 1000)} }
                };
    }
}

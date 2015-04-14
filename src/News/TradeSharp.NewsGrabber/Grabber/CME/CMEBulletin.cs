using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.NewsGrabber.Grabber.CME
{
    public class CMETickerInfo
    {
        /// <summary>
        /// EURUSD
        /// </summary>
        public string SpotSymbol { get; set; }

        /// <summary>
        /// EURO FX
        /// </summary>
        //public string BulletinReference { get; set; }

        /// <summary>
        /// 1000 (1455 = 1.455)
        /// </summary>
        public decimal StrikeToBaseRatio { get; set; }

        /// <summary>
        /// 20.1 = 0.0201
        /// </summary>
        public decimal PremiumToBaseRatio { get; set; }

        /// <summary>
        /// если true - цена "страйк" б.а = StrikeToBaseRatio / contract price
        /// </summary>
        public bool InvertRate { get; set; }

        public List<CMEBulletinReference> bulletinReferences = new List<CMEBulletinReference>();

        public CMETickerInfo(string spotSymbol, decimal strikeToBase, bool invert)
        {
            SpotSymbol = spotSymbol;            
            StrikeToBaseRatio = strikeToBase;
            InvertRate = invert;
        }
    }

    /// <summary>
    /// <BulletinRef Type="PUT" Style="European" Title="AUST DLR P EU" Section="38" />
    /// </summary>
    public class CMEBulletinReference
    {
        public OptionType OptionType { get; set; }
        public OptionStyle OptionStyle { get; set; }
        public string Title { get; set; }
        public string Section { get; set; }
    }

    public class CMETickerInfoCollection
    {
        public readonly List<CMETickerInfo> tickers = new List<CMETickerInfo>();
        public readonly List<string> sectionsToParse = new List<string>();

        public CMETickerInfoCollection(string xmlFile)
        {
            //<Ticker BaseActive="EURUSD" StrikeToBase="1000">
            //    <BulletinRef Type="CALL" Style="American" Title="EURO FX CALL" Section="39" />
            //    <BulletinRef Type="CALL" Style="European" Title="EURO FX C EU" Section="39" />
            //    <BulletinRef Type="PUT" Style="American" Title="EURO FX PUT" Section="39" />
            //    <BulletinRef Type="PUT" Style="European" Title="EURO FX P EU" Section="39" />
            //</Ticker>

            if (!File.Exists(xmlFile)) throw new FileNotFoundException(xmlFile);
            var doc = new XmlDocument();
            doc.Load(xmlFile);
            if (doc.DocumentElement == null) 
                throw new Exception("Xml doc: document element is missing");
            
            // номера бюллетеней и инструменты
            var nodes = doc.DocumentElement.SelectNodes("Ticker");
            if (nodes == null) throw new Exception("No Ticker entry in XML root");
            if (nodes.Count == 0) throw new Exception("No Ticker entry in XML root");

            
            foreach (XmlElement node in nodes)
            {
                var ticker = new CMETickerInfo(node.Attributes["BaseActive"].Value,
                                               node.Attributes["StrikeToBase"].Value.ToDecimalUniform(),
                                               Convert.ToBoolean(node.Attributes["Invert"].Value));
                ticker.PremiumToBaseRatio = node.Attributes["PremiumToBase"] != null
                                                ? node.Attributes["PremiumToBase"].Value.ToInt()
                                                : ticker.StrikeToBaseRatio;

                foreach (XmlElement nodeRef in node.ChildNodes)
                {
                    var bulRef = new CMEBulletinReference
                                     {
                                         OptionStyle = (OptionStyle) Enum.Parse(typeof (OptionStyle),
                                                                                nodeRef.Attributes["Style"].Value),
                                         OptionType = (OptionType) Enum.Parse(typeof (OptionType),
                                                                              nodeRef.Attributes["Type"].Value),
                                         Section = nodeRef.Attributes["Section"].Value,
                                         Title = nodeRef.Attributes["Title"].Value
                                     };
                    ticker.bulletinReferences.Add(bulRef);
                    if (!sectionsToParse.Contains(bulRef.Section))
                        sectionsToParse.Add(bulRef.Section);
                }
                tickers.Add(ticker);
            }            
        }
    
        public bool FindTicker(string[] lineParts, out CMETickerInfo ticker, 
            out CMEBulletinReference reference, string section)
        {
            ticker = null;
            reference = null;

            var title = string.Join(" ", lineParts);
            foreach (var tick in tickers)
            {
                foreach (var tickRef in tick.bulletinReferences)
                {
                    if (tickRef.Section == section && tickRef.Title == title)
                    {
                        ticker = tick;
                        reference = tickRef;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

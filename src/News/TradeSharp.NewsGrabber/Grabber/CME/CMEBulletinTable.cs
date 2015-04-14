using System.Collections.Generic;
using Entity;

namespace TradeSharp.NewsGrabber.Grabber.CME
{
    public class CMEBulletinTable
    {
        public string SpotSymbol { get; set; }
        public OptionType OptType { get; set; }
        public OptionStyle OptStyle { get; set; }
        public ExpirationMonth ExpMonth { get; set; }
        public decimal StrikeToBaseRatio { get; set; }
        public decimal PremiumToBaseRatio { get; set; }
        public bool InvertRate { get; set; }
        public readonly List<CMEBulletinTableRow> rows = new List<CMEBulletinTableRow>();

        public static void MergeSameTables(List<CMEBulletinTable> tables)
        {
            for (var i = 0; i < tables.Count; i++)
            {
                var tabA = tables[i];
                for (var j = i + 1; j < tables.Count; j++)
                {
                    var tabB = tables[j];
                    if (tabA.SpotSymbol == tabB.SpotSymbol && tabA.ExpMonth == tabB.ExpMonth
                        && tabA.OptType == tabB.OptType && tabA.OptStyle == tabB.OptStyle && tabA.InvertRate == tabB.InvertRate)
                    {
                        tabA.rows.AddRange(tabB.rows);
                        tables.RemoveAt(j);
                        j--;
                    }
                }
            }
        }
    }    
}
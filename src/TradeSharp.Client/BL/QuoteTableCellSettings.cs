using Entity;

namespace TradeSharp.Client.BL
{
    public class QuoteTableCellSettings
    {
        private string ticker;
        [PropertyXMLTag("Ticker.Ticker")]
        public string Ticker
        {
            get { return ticker; } 
            set
            {
                ticker = value;
                UserSettings.Instance.lastTimeModified.Touch();
            }
        }

        private int precision;
        [PropertyXMLTag("Ticker.Precision")]
        public int Precision
        {
            get { return precision; }
            set { precision = value; UserSettings.Instance.lastTimeModified.Touch(); }
        }

        private int x;
        [PropertyXMLTag("Ticker.X")]
        public int X
        {
            get { return x; }
            set
            {
                x = value;
                UserSettings.Instance.lastTimeModified.Touch();
            }
        }

        private int y;
        [PropertyXMLTag("Ticker.Y")]
        public int Y
        {
            get { return y; }
            set
            {
                y = value;
                UserSettings.Instance.lastTimeModified.Touch();
            }
        }
    }
}

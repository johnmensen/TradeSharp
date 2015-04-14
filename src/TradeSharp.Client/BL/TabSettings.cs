using System;
using System.Collections.Generic;
using System.Xml;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.BL
{
    public class TabSettings
    {
        private List<ChartWindowSettings> chartSetsList = new List<ChartWindowSettings>();
        [PropertyXMLTag("Chart")]
        public List<ChartWindowSettings> ChartSetsList
        {
            get { return chartSetsList; }
            set { chartSetsList = value; }
        }

        public void GetTabSettings(XmlElement tabNode)
        {
            try
            {
                PropertyXMLTagAttribute.SaveObjectProperties(this, tabNode);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка сохранения конфигурации вкладки", ex);
            }
        }

        public void LoadProperties(XmlElement tabNode)
        {
            PropertyXMLTagAttribute.InitObjectProperties(this, tabNode);
        }
    }    
}

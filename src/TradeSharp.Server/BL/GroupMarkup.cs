using System.Collections.Generic;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Server.BL
{
    /// <summary>
    /// используется для расчета комиссии ДЦ
    /// содержит "расширение спреда" по группам, по каждому тикеру в отдельности
    /// </summary>
    public class GroupMarkup
    {
        public string GroupCode { get; set; }

        public Dictionary<string, float> spreadByTicker;

        public float DefaultSpread { get; set; }

        public AccountGroup.MarkupType MarkupType { get; set; }

        public float GetDeltaPriceAbs(string ticker)
        {
            float delta;
            return spreadByTicker.TryGetValue(ticker, out delta) ? delta : DefaultSpread;
        }

        public GroupMarkup()
        {            
        }

        public GroupMarkup(string groupCode)
        {
            GroupCode = groupCode;
        }
    }
}

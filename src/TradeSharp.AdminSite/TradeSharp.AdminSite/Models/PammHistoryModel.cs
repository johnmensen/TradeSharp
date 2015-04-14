using System;
using System.Linq;
using TradeSharp.Linq;

namespace TradeSharp.SiteAdmin.Models
{
    /// <summary>
    /// Модель для истории ПАММ-счетов
    /// </summary>
    public class PammHistoryModel
    {
        public decimal MaxShareY { get; private set; }
        public decimal MinShareY { get; private set; }

        public decimal MaxHtmY { get; private set; }
        public decimal MinHtmY { get; private set; }

        /// <summary>
        /// Не пересекает ли график Share ось oX
        /// 
        /// </summary>
        public bool OneCharacterShare { get; private set; }

        /// <summary>
        /// Не пересекает ли график htm ось oX
        /// </summary>
        public bool OneCharacterHtm { get; private set; }

        public ACCOUNT_SHARE_HISTORY[] CurrentItems { get; private set; }


        public PammHistoryModel(ACCOUNT_SHARE_HISTORY[] data)
        {
            CurrentItems = data;

            MaxShareY = CurrentItems.Max(x => x.NewShare);
            MinShareY = CurrentItems.Min(x => x.NewShare);

            OneCharacterShare = (Math.Sign(MaxShareY) == Math.Sign(MinShareY));

            MaxHtmY = CurrentItems.Max(x => x.NewHWM);
            MinHtmY = CurrentItems.Min(x => x.NewHWM);

            OneCharacterHtm = (Math.Sign(MaxHtmY) == Math.Sign(MinHtmY));
        }
    }
}
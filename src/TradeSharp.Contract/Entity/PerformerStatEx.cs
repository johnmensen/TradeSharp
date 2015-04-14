using System.ComponentModel;
using TradeSharp.Util;

namespace TradeSharp.Contract.Entity
{
    public class PerformerStatEx : PerformerStat
    {
        public static readonly PerformerStatEx speciman = new PerformerStatEx();

        public PerformerStatEx()
        {
        }

        public PerformerStatEx(PerformerStat performer)
            : base(performer)
        {
        }

        public PerformerStatEx(PerformerStatEx performer)
            : base(performer)
        {
            IsSubscribed = performer.IsSubscribed;
            ChartIndex = performer.ChartIndex;
            AvatarSmallIndex = performer.AvatarSmallIndex;
            Rooms = performer.Rooms;
        }

        [DisplayName("Подписан")]
        [LocalizedDisplayName("TitleSubscribed")]
        [Description("Трейдер управляет Вашим счетом")]
        public bool IsSubscribed { get; set; }

        [DisplayName("График")]
        [LocalizedDisplayName("TitleChart")]
        public int ChartIndex { get; set; }

        [DisplayName("Фото")]
        [LocalizedDisplayName("TitlePhoto")]
        public int AvatarSmallIndex { get; set; }

        [DisplayName("Комнаты")]
        [LocalizedDisplayName("TitleRooms")]
        [Description("Комнаты чата трейдера")]
        public string Rooms { get; set; }
    }
}

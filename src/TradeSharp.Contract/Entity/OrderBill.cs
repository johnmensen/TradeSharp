using System;

namespace TradeSharp.Contract.Entity
{
    /// <summary>
    /// запись, сопровождающая ордер
    /// содержит данные о разного рода комиссии, профите в валюте
    /// брокера
    /// </summary>
    [Serializable]
    public class OrderBill
    {
        /// <summary>
        /// ID позиции (открытой либо закрытой)
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// тип маркапа
        /// </summary>
        public AccountGroup.MarkupType MarkupType { get; set; }

        /// <summary>
        /// комиссия на вход, в абс. ед. 
        /// </summary>
        public float MarkupEnter { get; set; }

        /// <summary>
        /// комиссия на выход, в абс. ед. 
        /// </summary>
        public float MarkupExit { get; set; }

        /// <summary>
        /// маркап полный, в валюте брокера
        /// заполняется на момент выхода из сделки
        /// </summary>
        public float MarkupBroker { get; set; }

        /// <summary>
        /// профит (лосс) в валюте брокера
        /// заполняется на момент выхода из сделки
        /// </summary>
        public float ProfitBroker { get; set; }

        public OrderBill() {}

        public OrderBill(int orderId, AccountGroup.MarkupType markupType, float markupEnter)
        {
            Position = orderId;
            MarkupType = markupType;
            MarkupEnter = markupEnter;
        }
    }
}

using System;

namespace TradeSharp.Delivery.Contract
{
    /// <summary>
    /// Електронное письмо
    /// </summary>
    [Serializable]
    public class EmailMessage
    {
        /// <summary>
        /// Заголовок сообщения
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Тело сообщения
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Флаг
        /// </summary>
        public bool HTMLFormat { get; set; }

        /// <summary>
        /// Список получателей сообщения типа ivanov@mail.com
        /// </summary>
        public string[] Receivers { get; set; }

        /// <summary>
        /// Важность сообщения - влияет на место в очереди отправки
        /// </summary>
        public UrgencyFlag Urgency { get; set; }
    }
}

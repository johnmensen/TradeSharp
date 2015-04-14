using System;
using TradeSharp.Delivery.Contract;

namespace TradeSharp.Delivery.Service.WebServer
{
    /// <summary>
    /// Описание ошибок, происходящих на сервере
    /// </summary>
    public class DeliveryServerError
    {
        /// <summary>
        /// Если ошибка произошла в результате исключения (что бывает не всегда), это ссылка на него
        /// </summary>
        public Exception ExceptionLink { get; set; }

        /// <summary>
        /// Причина ошибки - выводится в лог-файл и в web-интерфейс 
        /// </summary>
        public string ReasonMessage { get; set; }

        /// <summary>
        /// Время отправки сообщения перед возникновением ошибки
        /// </summary>
        public double TimeSpan { get; set; }

        /// <summary>
        /// Время и дата, когда произошла ошибка
        /// </summary>
        public DateTime DateException { get; set; }

        /// <summary>
        /// Получатель, на котором произошла ошибка
        /// </summary>
        public string Receiver { get; set; }

        /// <summary>
        /// Важность сообщения, при отправке которого произошла ошибка
        /// </summary>
        public UrgencyFlag Urgency { get; set; }
    }
}

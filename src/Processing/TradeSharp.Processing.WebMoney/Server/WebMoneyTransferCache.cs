using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using TradeSharp.Util;
using WebMoney.XmlInterfaces.Responses;

namespace TradeSharp.Processing.WebMoney.Server
{
    public class WebMoneyTransferCache
    {
        /// <summary>
        /// Количество итераций бесконечного цикла между перезапросами к серверам WebMoney
        /// Поснее этот параметр нужно сделать "volatile" свойством, что бы настраивать его динамически с сайта администратора
        /// </summary>
        public int countIterationToRequest = 10;

        /// <summary>
        /// Максимальное количество дней, за которые можно получить историю (чуть меньше трёх месяцев)
        /// </summary>
        private const int MaxHistoryDays = 85;

        /// <summary>
        /// На скользо приостанавливается поток в главном цикле опроса. Задаётся в конструкторе
        /// </summary>
        private readonly int threadSleep;

        /// <summary>
        /// Экземпляр класса, обеспечивающий запросы к серверам WebMoney
        /// </summary>
        private readonly IPaymentAccessor webMoneyAccessor;
        

        /// <summary>
        /// Формат записи времени последнего запроса к серверам WebMoney (для проверки не пополнился ли счёт)
        /// </summary>
        private readonly DateTimeFormatInfo dateTimeFormatProvider = new DateTimeFormatInfo
        {
            DateSeparator = ".",
            ShortDatePattern = "dd.MM.yyyy",
            TimeSeparator = ":",
            ShortTimePattern = "h:mm:ss"
        };

        #region Переменные файла настроек
        /// <summary>
        /// Локальная копия времени последнего обращения к серверам WebMoney
        /// </summary>
        private DateTime? lastRequestDate; 

        /// <summary>
        /// Время последнего сохранённого обращения к серверам WebMoney
        /// </summary>
        public DateTime? LastRequestDate
        {
            get
            {
                // Читаем эту переменную из файла, только есть локальная переменная не содержит данных
                if (lastRequestDate.HasValue) return lastRequestDate;

                var strLastUpdateDate = WebMoneySettings.Instance.LastUpdateDate;
                if (string.IsNullOrEmpty(strLastUpdateDate)) return null;

                lastRequestDate = strLastUpdateDate.ToDateTimeUniformSafe(dateTimeFormatProvider);
                return lastRequestDate;
            }
            set
            {
                if (!value.HasValue) return;
                lastRequestDate = value.Value;
                WebMoneySettings.Instance.LastUpdateDate = value.Value.ToString(dateTimeFormatProvider);
                WebMoneySettings.Instance.SaveSettings();
            }
        }

        /// <summary>
        /// Локальная копия идентификатора последней транзакции WebMoney
        /// </summary>
        private uint? lastRequestTransferId;

        /// <summary>
        /// Уникальный идентификатор, прочитанный из файла настроек, последней проведённой транзакции 
        /// </summary>
        public uint? LastRequestTransferId
        {
            get
            {
                if (lastRequestTransferId.HasValue) return lastRequestTransferId;

                var strLastTransferId = WebMoneySettings.Instance.LastTransferId;
                if (string.IsNullOrEmpty(strLastTransferId)) return null;

                uint lastTransferId;
                if (uint.TryParse(strLastTransferId, out lastTransferId))
                    lastRequestTransferId = lastTransferId;
                return lastRequestTransferId;
            }
            set
            {
                if (!value.HasValue) return;
                lastRequestTransferId = value;
// ReSharper disable SpecifyACultureInStringConversionExplicitly
                WebMoneySettings.Instance.LastTransferId = value.Value.ToString();
// ReSharper restore SpecifyACultureInStringConversionExplicitly
                WebMoneySettings.Instance.SaveSettings();
            }
        }
        #endregion

        public WebMoneyTransferCache(int threadSleepMilliseconds)
            : this(threadSleepMilliseconds, null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="threadSleepMilliseconds"></param>
        /// <param name="accessor">для теста</param>
        public WebMoneyTransferCache(int threadSleepMilliseconds, IPaymentAccessor accessor)
        {
            threadSleep = threadSleepMilliseconds;
            webMoneyAccessor = accessor ?? new WebMoneyAccessor();

            if (!webMoneyAccessor.CheckInitial())
                throw new SettingsPropertyNotFoundException("WebMoneyAccessor() - не удалось прочитать необходимые для инициализировать WebMoney параметры (wmId, purseNumber и др.)");
        }

        /// <summary>
        /// возвращает транзакции с сервера WebMoney
        /// </summary>
        /// <returns>Транзакции зачисления</returns>
        public List<Transfer> GetActualTransaction()
        {
            // Время, которое будет finishTime в текущем запросе (и которое запишется в файл настроек)
            var timeCurrentRequest = DateTime.Now;
            var oldestRequestTime = timeCurrentRequest.AddDays(-MaxHistoryDays);

            var startDate = LastRequestDate.HasValue
                                ? LastRequestDate.Value.AddMilliseconds(-threadSleep*countIterationToRequest*50)
                                : oldestRequestTime;

            if (startDate < oldestRequestTime) startDate = oldestRequestTime;
            var transfers = GetTransaction(startDate, timeCurrentRequest);
            if (transfers == null) return null;         

            try
            {
                var lastReqId = LastRequestTransferId;
                #region перехватить ошибку - пропущены трансферы
                    if (!CheckIntersectionTransfer(transfers, lastReqId) && startDate != oldestRequestTime)
                    {
                        #region Серъёзная потеря данных платежей WebMoney
                        var errorMessage =
                            string.Format(
                                "Серъёзная потеря данных платежей WebMoney: присутствует Id последнего перевода {0}, но этого Id нет среди операций за последние {1} дней! \r\n" +
                                "Вероятно, служба TradeSharp.WebMoney была выключена всё это время и не могла обновлять базу \r\n" +
                                "данных TradeSharp при поступлении новых перевов на WebMoney кошелёк {2}. \r\n" +
                                "Служба TradeSharp.WebMoney обработает операции за последние {1} дней. Но более ранние недостающие транзакции \r\n" +
                                "администратору базы данных TradeSharp придётся восстановить вручную. Для получения более обширной истории операций \r\n" +
                                "с кошельком {2} рекомендуется использовать штатные инструменты, предоставляемые WebMoney (например, WebMoney Keeper Classic).",
                                lastReqId,
                                MaxHistoryDays,
                                WebMoneyUtil.companyPurseId);

                        #endregion
                        Logger.Error(errorMessage); // TODO нужно оповестить администратора базы данных TradeSharp
                    }
                #endregion

                if (transfers.Count > 0 && lastReqId.HasValue)
                {
                    var previousTransfer = transfers.SingleOrDefault(x => x.Id == lastReqId.Value);
                    
                    if (previousTransfer != null)
                        transfers = transfers.Where(x =>
                                x.CreateTime.GetServerTime() >=  previousTransfer.CreateTime.GetServerTime() &&
                                x.Id != lastReqId.Value).ToList();
                }
            }
            finally
            {
                if (transfers.Count > 0)
                {
                    LastRequestDate = timeCurrentRequest;
                    LastRequestTransferId = transfers[transfers.IndexOfMax(t =>  t.CreateTime.GetServerTime())].Id;
                }
            }            
            return transfers;
        }

        /// <summary>
        /// Проверка, пересекаются ли Id полученных транзакций с тем Id, что лежит в файле
        /// true - дырок нет
        /// </summary>
        private bool CheckIntersectionTransfer(List<Transfer> transfers, uint? lastReqId)
        {
            if (!lastReqId.HasValue) return true;
            if (transfers.Count == 0)
                return true;
            var transId = transfers[transfers.IndexOfMin(t => t.CreateTime.GetServerTime())].Id;
            return transId <= lastReqId.Value;
        }

        /// <summary>
        /// Вытаскиваем все сделки, которые относились к "зачислению" в кошелёк
        /// </summary>
        /// <param name="сurrentDate">время текущего запроса</param>
        /// <param name="startDate">время предыдущего запроса</param>
        /// <returns></returns>
        private List<Transfer> GetTransaction(DateTime startDate, DateTime сurrentDate)
        {
            
            var transfers = webMoneyAccessor.GetTransfers(startDate, сurrentDate);
            return transfers == null ? null :
                transfers.Where(x => x.TargetPurse.Number == WebMoneyUtil.companyPurseId).ToList();

            //TODO тут может быть ошибка - вытаскиваются те сделки, в которых "корпоративный" кошелёк был "целевым". 
            //TODO Это будет работать, если на "корпоративный" кошелёк средства будут только поступать, но не сниматься
        }
    }
}
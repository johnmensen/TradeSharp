using System;
using System.Threading;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.ProviderProxy.Quote;
using TradeSharp.ProviderProxyContract.Entity;
using TradeSharp.Util;
using QuickFix;
using Message = QuickFix.Message;

namespace TradeSharp.ProviderProxy.BL
{
    /// <summary>
    /// первичная обработка сообщений, полученных от брокера
    /// </summary>
    public static class MessageDispatcher
    {
        class MessagePair
        {
            public Message msgOrig;
            public FixMessage msg;

            public MessagePair(Message msgOrig, FixMessage msg)
            {
                this.msgOrig = msgOrig;
                this.msg = msg;
            }
        }

        private static readonly FloodSafeLogger loggerNoFlood = new FloodSafeLogger(1000 * 60);
        private const int LogMagicIncorrectId = 1;
        private const int LogMagicIncorrectStatus = 2;
        private const int LogMagicNoQuoteInPositiveReport = 3;
        private const int LogMagicCommonReportParseError = 4;
        private const int LogMagicSendReportError = 5;        
        private const int LogMagicSymbolNotFound = 9;
        

        private static bool? shouldStoreQuote;
        private static bool ShouldStoreQuote
        {
            get
            {
                return shouldStoreQuote.HasValue
                           ? shouldStoreQuote.Value
                           : (shouldStoreQuote = AppConfig.GetBooleanParam("Quote.ShouldStore", true)).Value;
            }
        }
       
        public static void ProcessMessageFromBroker(Message msgOrig, FixMessage msg)
        {
            //ThreadPool.QueueUserWorkItem(ProcessMessageAsynch, new MessagePair(msgOrig, msg));
            ProcessMessageAsynch(new MessagePair(msgOrig, msg));
        }

        private static void ProcessMessageAsynch(object messagePair)
        {
            var pair = (MessagePair) messagePair;
            switch (pair.msg.MessageType)
            {
                case FixMessageType.Quote:
                    if (ShouldStoreQuote)
                        MarketDataProcessor.ProcessMsgQuote(pair.msg);
                    break;
                case FixMessageType.MarketDataIncrementalRefresh:
                    if (ShouldStoreQuote)
                        MarketDataProcessor.ProcessMsgMarketDataIncrRefresh(pair.msgOrig, pair.msg);
                    break;
                case FixMessageType.MarketDataSnapshotFullRefresh:
                    if (ShouldStoreQuote)
                        MarketDataProcessor.ProcessMsgMarketDataFullRefresh(pair.msgOrig, pair.msg);
                    break;
                case FixMessageType.ExecutionReport:
                    ProcessExecutionReport(pair.msg);
                    break;
            }
        }

        private static void ProcessExecutionReport(FixMessage msg)
        {
            ExecutionReport execReport;
            try
            {
                // отчет об исполнении может относиться не к клиентскому ордеру,
                // а к инициированной брокером процедуре (в т.ч. - стопауту)
                if (!msg.fieldValues.ContainsKey(FixMessage.TAG_CLIENT_ORDER_ID))
                    ProcessBrokerInitiatedExecutionReport(msg);

                // промежуточный отчет по исполнению ордера, не учитывается
                var ordStatusStr = msg[FixMessage.TAG_ORDER_STATUS];
                if (ordStatusStr == FixMessage.VALUE_ORDER_NEW ||
                    ordStatusStr == FixMessage.VALUE_ORDER_PARTIALLY_FILLED ||
                    ordStatusStr == FixMessage.VALUE_ORDER_CALCULATED)
                    return;

                // получить Id запроса
                var clientIdStr = msg[FixMessage.TAG_CLIENT_ORDER_ID];
                int clientId;
                if (!int.TryParse(clientIdStr, out clientId))
                {
                    loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Debug,
                        LogMagicIncorrectId, 1000 * 60 * 15, "Отчет брокера: поле ClientOrderId не представлено целым [{0}]",
                        clientIdStr);
                    return;
                }

                execReport = new ExecutionReport { brokerResponse = { RequestId = clientId, ValueDate = DateTime.Now } };                
                if (ordStatusStr == FixMessage.VALUE_ORDER_FILLED)
                    execReport.brokerResponse.Status = OrderStatus.Исполнен;
                else
                    if (ordStatusStr == FixMessage.VALUE_ORDER_REJECTED)
                        execReport.brokerResponse.Status = OrderStatus.Отклонен;
                    else
                    {
                        // ордер может быть либо исполнен, либо не исполнен, третьего не дано
                        loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Debug,
                        LogMagicIncorrectStatus, 1000 * 60 * 15, "Отчет брокера по запросу [{0}]: статус ({1}) не поддерживается",
                        clientId,
                        ordStatusStr);
                        return;
                    }

                var price = (decimal?)null;
                if (msg.fieldValues.ContainsKey(FixMessage.TAG_ARG_PX))
                    price = msg.GetValueDecimal(FixMessage.TAG_ARG_PX);
                else
                    if (msg.fieldValues.ContainsKey(FixMessage.TAG_PRICE))
                        price = msg.GetValueDecimal(FixMessage.TAG_PRICE);
                // нет цены в положительной квитанции
                if (!price.HasValue && execReport.brokerResponse.Status == OrderStatus.Исполнен)
                {
                    loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Debug,
                        LogMagicNoQuoteInPositiveReport, 1000 * 60 * 15,
                        "Отчет брокера по запросу [{0}]: статус \"Исполнен\", нет цены",
                        clientId);
                    return;
                }
                execReport.brokerResponse.Price = price ?? 0;
                // сообщение об ошибке
                execReport.brokerResponse.RejectReason = OrderRejectReason.None;
                if (execReport.brokerResponse.Status != OrderStatus.Исполнен)
                {
                    if (msg.fieldValues.ContainsKey(FixMessage.TAG_ORD_REJECT_REASON))
                    {
                        var rejectReasonStr = msg[FixMessage.TAG_ORD_REJECT_REASON];
                        execReport.brokerResponse.RejectReason =
                            rejectReasonStr == FixMessage.VALUE_ORDER_REJECT_BROKER_EXCHANGE_OPT
                                ? OrderRejectReason.BrokerExchangeOption
                                : rejectReasonStr == FixMessage.VALUE_ORDER_REJECT_DUPLICATE_ORDER
                                      ? OrderRejectReason.DuplicateClOrdID
                                      : rejectReasonStr == FixMessage.VALUE_ORDER_REJECT_UNKNOWN_ORDER
                                      ? OrderRejectReason.UnknownOrder : OrderRejectReason.None;
                        execReport.brokerResponse.RejectReasonString = rejectReasonStr;
                    }
                }
            }
            catch (Exception ex)
            {
                loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Debug,
                                                         LogMagicCommonReportParseError, 1000*60*15,
                                                         "Общая ошибка обработки ответа провайдера: {0}",
                                                         ex);
                return;
            }
            // отправить отчет FIX-дилеру
            try
            {
                execReport.SendToQueue(false);
            }
            catch (Exception ex)
            {
                loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Debug,
                                                         LogMagicSendReportError, 1000 * 60 * 15,
                                                         "Ошибка отправки сообщения в очередь: {0}",
                                                         ex);
            }                        

        }

        [Obsolete("Надо реализовать")]
        private static void ProcessBrokerInitiatedExecutionReport(FixMessage msg)
        {
            loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Debug,
                                                         101, 1000 * 60 * 15,
                                                         "ProcessBrokerInitiatedExecutionReport: [{0}]",
                                                         msg);
        }        
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TradeSharp.Util;
using QuickFix;

namespace TradeSharp.ProviderProxy.BL
{
    class QuoteRequest
    {
        private static readonly bool quoteSubscriptionIncrementalUpdateType = AppConfig.GetBooleanParam(
            "QuoteRequest.Incremental", false);
        private static readonly bool subscribeBySecurityId = AppConfig.GetBooleanParam(
            "QuoteRequest.SubscribeBySecurityId", false);
        private static readonly bool useTickerCsvDictionary = AppConfig.GetBooleanParam(
            "QuoteRequest.UseCSVDictionary", false);
        private static readonly string quoteSessionTargetId = AppConfig.GetStringParam(
            "QuoteRequest.TargetCompId", "");
        private static readonly int marketDepth = AppConfig.GetIntParam(
            "QuoteRequest.MarketDepth", 0);
        private static readonly bool distinctQueriesForBidAsk = AppConfig.GetBooleanParam(
            "QuoteRequest.DistinctBidAsk", false);
                       
        public QuoteRequest()
        {
        }

        public void RequestMarketData(SessionID sessionId)
        {
            if (string.IsNullOrEmpty(quoteSessionTargetId)) return;
            var sessionTarget = sessionId.getTargetCompID();
            if (sessionTarget != quoteSessionTargetId)
            {
                Logger.InfoFormat("RequestMarketData skipped (session target is '{0}', target is '{1}')",
                    sessionTarget, quoteSessionTargetId);
                return;
            }

            if (useTickerCsvDictionary)
                RequestTickersFromCsvDic(sessionId);
            else
                RequestTickersFromTextFile(sessionId);
        }

        private void RequestTickersFromCsvDic(SessionID sessionId)
        {
            Logger.InfoFormat("Запрос {0} тикеров из словаря CSV", TickerCodeDictionary.Instance.tickerCode.Keys.Count);
            foreach (var ticker in TickerCodeDictionary.Instance.tickerCode.Keys)
                RequestTicker(ticker, sessionId);
        }

        private void RequestTickersFromTextFile(SessionID sessionId)
        {
            var setsFileName = string.Format("{0}\\{1}",
                                             ExecutablePath.ExecPath, "tickers.txt");
            string [] tickers = null;

            if (File.Exists(setsFileName));
            {
                using (var sr = new StreamReader(setsFileName, Encoding.ASCII))
                {
                    var line = sr.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                        tickers = line.Split(new[] {' ', (char) 9}, StringSplitOptions.RemoveEmptyEntries);
                }
            }

            if (tickers == null || tickers.Length == 0)
            {
                Logger.Error("Словарь тикеров (CSV) пуст");
                return;
            }

            Logger.InfoFormat("Запрос {0} тикеров из словаря CSV", tickers.Length);
            foreach (var ticker in tickers)
                RequestTicker(ticker, sessionId);
        }

        private static void RequestTicker(string ticker, SessionID sessionId)
        {
            if (sessionId == null) return;
            Message msg;
            try
            {
                msg = MakeQuoteRequest(ticker, sessionId);
            }
            catch (Exception ex)
            {
                Logger.Error("QuoteRequest.RequestTicker: ошибка форматирования сообщения", ex);
                return;
            }
            if (msg == null)
            {
                Logger.ErrorFormat("Ошибка сборки сообщения (запрос котировки) для {0}, {1}", ticker, sessionId);
                return;
            }
            try
            {
                Logger.InfoFormat("Отправка запроса \"{0}\": {1}", ticker, msg);
                FixApplication.SendMessage(msg);
            }
            catch (Exception ex)
            {
                Logger.Error("QuoteRequest.RequestTicker: ошибка отправки сообщения", ex);
            }
        }

        public static Message MakeQuoteRequest(string ticker, SessionID sessionInfo)
        {
            var senderId = sessionInfo.getSenderCompID();
            if (string.IsNullOrEmpty(senderId))
            {
                Logger.ErrorFormat("MakeMessage: код отправителя для [{0}] не найден", sessionInfo);
                return null;
            }
            var msg =
                FixMessage.FixVersion == FixVersion.Fix42 ?
                    (Message)new QuickFix42.MarketDataRequest(
                                new MDReqID(string.Format("{0}-{1:dd-HH-mm}", ticker, DateTime.Now)),
                                    new SubscriptionRequestType((char)1), // subscribe
                                    new MarketDepth(marketDepth))
                : FixMessage.FixVersion == FixVersion.Fix43 ?
                    (Message)new QuickFix43.MarketDataRequest(
                                new MDReqID(string.Format("{0}-{1:dd-HH-mm}", ticker, DateTime.Now)),
                                    new SubscriptionRequestType((char)1), // subscribe
                                    new MarketDepth(marketDepth))
                : new QuickFix44.MarketDataRequest(
                                new MDReqID(string.Format("{0}-{1:dd-HH-mm}", ticker, DateTime.Now)), // 262=EURAUD-02-17-07
                                    new SubscriptionRequestType(SubscriptionRequestType.SNAPSHOT_PLUS_UPDATES), // subscribe
                                    new MarketDepth(marketDepth));

            msg.setField(new MDUpdateType(
                quoteSubscriptionIncrementalUpdateType
                ? MDUpdateType.INCREMENTAL_REFRESH
                : MDUpdateType.FULL_REFRESH));

            if (!AddBidAskQueryClause(msg, ticker))
                return null;

            msg.getHeader().setField(new SenderCompID(senderId));
            msg.getHeader().setField(new TargetCompID(sessionInfo.getTargetCompID()));

            return msg;
        }

        private static bool AddBidAskQueryClause(Message msg, string ticker)
        {
            var simGroup = new QuickFix44.MarketDataRequest.NoRelatedSym(); // 146=1

            // подписаться на символ по его коду?
            if (subscribeBySecurityId)
            {
                int tickerCode;
                if (!TickerCodeDictionary.Instance.tickerCode.TryGetValue(ticker, out tickerCode))
                {
                    Logger.ErrorFormat("Запрос котировки {0} - нет кода", ticker);
                    return false;
                }

                simGroup.setField(new SecurityID(tickerCode.ToString())); // 48=4006 symbol
                simGroup.setField(new SecurityIDSource("8")); // 22=8 symbol
            }
            else
            // подписаться по названию
            {
                simGroup.setField(new Symbol(TickerCodeDictionary.Instance.GetTickerNameFormatted(ticker)));
            }

            // два отдельных запроса на bid-ask?
            if (distinctQueriesForBidAsk)
            {
                var group = new QuickFix44.MarketDataRequest.NoMDEntryTypes(); // 267=2
                group.setField(new MDEntryType('0')); // 269=0 (bid)
                simGroup.addGroup(group);

                group = new QuickFix44.MarketDataRequest.NoMDEntryTypes();
                group.setField(new MDEntryType('1')); // 269=1 (offer)
                simGroup.addGroup(group);
            }
            else
            // один запрос
            {
                var group = new QuickFix44.MarketDataRequest.NoMDEntryTypes(); // 267=2
                group.setField(new MDEntryType('2')); // 269=0 (bid + offer)
                simGroup.addGroup(group);
            }
            msg.addGroup(simGroup);
            
            msg.setField(new NoRelatedSym(1)); // 1 subscription
            msg.setField(new Symbol(ticker));

            return true;
        }
    }
}

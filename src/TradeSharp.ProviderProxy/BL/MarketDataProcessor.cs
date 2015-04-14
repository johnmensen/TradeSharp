using System;
using System.Text.RegularExpressions;
using Entity;
using QuickFix;
using TradeSharp.Contract.Entity;
using TradeSharp.ProviderProxy.Quote;
using TradeSharp.Util;
using Group = QuickFix.Group;

namespace TradeSharp.ProviderProxy.BL
{
    class MarketDataProcessor
    {
        private static readonly FloodSafeLogger loggerNoFlood = new FloodSafeLogger(1000 * 60);
        private const int LogMagicMarketData = 1;
        private const int LogMagicQuote = 6;
        private const int LogMagicParseMarketDataError = 7;
        private const int LogMagicParseMarketDataSuccess = 8;
        private const int LogMagicLackPrice = 10;
        private const int LogMagicSymbolNotFound = 11;

        private static readonly Regex tickerFromRequestRegex = new Regex("^[A-Z0-1]{0,8}", RegexOptions.None);

        /// <summary>
        /// сообщение содержит котировку: одну цену либо бид и аск сразу
        /// </summary>        
        public static void ProcessMsgQuote(FixMessage msg)
        {
            loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info,
                LogMagicMarketData, 1000 * 60 * 60 * 2, "Quote: {0}", msg.ToString());

            string symbol = msg.GetValueString(FixMessage.TAG_SYMBOL);
            if (string.IsNullOrEmpty(symbol))
            {
                Logger.Debug("Quote msg: no symbol tag");
                return;
            }

            if (!ConvertFromProviderSymbolNaming(ref symbol)) return;

            var bid = msg.GetValueFloat(FixMessage.TAG_BID_PRICE);
            var ask = msg.GetValueFloat(FixMessage.TAG_OFFER_PRICE);
            if (!bid.HasValue && ask.HasValue == false)
            {
                Logger.Debug("Quote msg: no bid - ask");
                return;
            }
            QuoteDistributor.Instance.UpdateQuote(symbol, bid, ask);
        }

        public static void ProcessMsgMarketDataIncrRefresh(Message msgOrig, FixMessage msg)
        {
            loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info,
                LogMagicMarketData, 1000 * 60 * 60 * 2, "Quote incr: {0}", msg.ToString());

            try
            {
                // информация по тикерам разбита на группы, каждому тикеру - по группе
                var groupHeader = new NoMDEntries();
                msgOrig.getField(groupHeader);
                int numGroups = groupHeader.getValue();

                var group = FixMessage.FixVersion == FixVersion.Fix42
                                ? new QuickFix42.MarketDataIncrementalRefresh.NoMDEntries()
                                : FixMessage.FixVersion == FixVersion.Fix43
                                      ? (Group)new QuickFix43.MarketDataIncrementalRefresh.NoMDEntries()
                                      : new QuickFix44.MarketDataIncrementalRefresh.NoMDEntries();

                var lastQuotes = new System.Collections.Generic.Dictionary<string, QuoteData>();

                for (var i = 1; i <= numGroups; i++)
                {
                    var groupTicker = msgOrig.getGroup((uint)i, group);
                    var symbol = groupTicker.getField(new Symbol()).getValue();

                    if (!ConvertFromProviderSymbolNaming(ref symbol)) return;

                    var entType = new MDEntryType();
                    var entTypeChar = groupTicker.getField(entType).getValue();
                    var priceType = entTypeChar == FixMessage.VALUE_MD_ENTRY_TYPE_BID[0]
                                        ? Contract.Entity.QuoteType.Bid
                                        : entTypeChar == FixMessage.VALUE_MD_ENTRY_TYPE_OFFER[0]
                                              ? Contract.Entity.QuoteType.Ask
                                              : Contract.Entity.QuoteType.NonSpecified;
                    if (priceType == Contract.Entity.QuoteType.NonSpecified) continue;
                    var price = msg.GetValueFloat(FixMessage.TAG_MD_ENTRY_PX) ?? 0;
                    if (price == 0) continue;

                    if (lastQuotes.ContainsKey(symbol))
                    {
                        var quote = lastQuotes[symbol];
                        if (priceType == Contract.Entity.QuoteType.Bid)
                            quote.bid = price;
                        else
                            quote.ask = price;
                    }
                    else
                    {
                        lastQuotes.Add(symbol, new QuoteData(
                            priceType == Contract.Entity.QuoteType.Bid ? price : 0,
                            priceType == Contract.Entity.QuoteType.Ask ? price : 0, DateTime.Now));
                    }
                }

                foreach (var quote in lastQuotes)
                {
                    loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info,
                                                             LogMagicQuote, 1000 * 60 * 15,
                                                             "Котировка {0}: {1} - {2}",
                                                             quote.Key, quote.Value.bid, quote.Value.ask);

                    QuoteDistributor.Instance.UpdateQuote(quote.Key,
                        quote.Value.bid == 0 ? (float?)null : quote.Value.bid,
                        quote.Value.ask == 0 ? (float?)null : quote.Value.ask);
                }

                loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info,
                                                         LogMagicParseMarketDataSuccess,
                                                         1000 * 60 * 10, "Обработан пакет MarketDataIncrRefresh");
            }
            catch (Exception ex)
            {
                loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info,
                                                         LogMagicParseMarketDataError,
                                                         1000 * 60 * 10, "Ошибка разбора MarketDataIncrRefresh: {0}", ex);
            }
        }

        public static void ProcessMsgMarketDataFullRefresh(Message msgOrig, FixMessage msg)
        {
            loggerNoFlood.LogMessageFormatCheckFlood(
                LogEntryType.Info,
                LogMagicMarketData, 
                1000 * 60 * 60 * 2, "Quote full: {0}", msg.ToString());

            // 8=FIX.4.4#9=128#35=W#34=2#49=XTB#52=20141125-16:12:33.269#56=PARTNER1#55=USDJPY..#262=USDJPY-25-16-12#268=2#269=0#270=117.906#10=097#

            try
            {
                var reqIdField = msgOrig.getField(new MDReqID());
                if (reqIdField == null)
                    throw new Exception("Поле MDReqID не прочитано");
                // инструмент указан в самом запросе
                var requestedSymbol = msg.GetValueString(FixMessage.TAG_SYMBOL);
                //requestedSymbol = TickerCodeDictionary.Instance.GetClearTickerName(requestedSymbol);
                // котировка будет исправлена в дальнейшем

                var groupHeader = new NoMDEntries();
                msgOrig.getField(groupHeader);
                var numGroups = groupHeader.getValue();

                if (numGroups == 0)
                    throw new Exception("Поле NoMDEntries не прочитано");

                var group = FixMessage.FixVersion == FixVersion.Fix42
                                ? new QuickFix42.MarketDataIncrementalRefresh.NoMDEntries()
                                : FixMessage.FixVersion == FixVersion.Fix43
                                      ? (Group)new QuickFix43.MarketDataIncrementalRefresh.NoMDEntries()
                                      : new QuickFix44.MarketDataIncrementalRefresh.NoMDEntries();

                float bid = 0, ask = 0;
                for (var i = 1; i <= numGroups; i++)
                {
                    var groupTicker = msgOrig.getGroup((uint)i, group);
                    // тип (бид или аск)
                    var isBid = true;
                    var typeField = groupTicker.getField(new MDEntryType());
                    if (typeField == null)
                        throw new Exception("Поле MDEntryType не прочитано");
                    isBid = typeField.getValue() == '0';
                    // цена
                    var priceField = group.getField(new MDEntryPx());
                    if (priceField == null)
                        throw new Exception("Поле MDEntryPx не прочитано");
                    var price = priceField.getValue();
                    if (isBid)
                        bid = (float)price;
                    else
                        ask = (float)price;
                }
                // цены прочитаны?
                if (bid == 0 || ask == 0)
                {
                    loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info,
                                                             LogMagicLackPrice,
                                                             1000 * 60 * 10, "MarketDataFullRefresh - нет цены (" +
                                                             bid.ToStringUniformPriceFormat() + ", " + ask.ToStringUniformPriceFormat() + ")");
                    return;
                }

                // обновить котировку
                QuoteDistributor.Instance.UpdateQuote(requestedSymbol, bid, ask);

                loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info,
                                                         LogMagicParseMarketDataSuccess,
                                                         1000 * 60 * 10, 
                                                         "Обработан пакет MarketDataFullRefresh - {0}: {1}/{2}",
                                                         requestedSymbol, bid, ask);
            }
            catch (Exception ex)
            {
                loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info,
                                                         LogMagicParseMarketDataError,
                                                         1000 * 60 * 10, "Ошибка разбора MarketDataFullRefresh: {0}", ex);
            }
        }

        private static bool ConvertFromProviderSymbolNaming(ref string symbol)
        {
            if (FixApplication.ProviderNaming != TickerNamingStyle.Системный)
            {
                string ownSymbol;
                if (!DalSpot.Instance.ConvertTickerNaming(symbol, out ownSymbol,
                    FixApplication.ProviderNaming, TickerNamingStyle.Системный))
                {
                    loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Debug,
                        LogMagicSymbolNotFound, 1000 * 60 * 20,
                        "Не найден символ \"{0}\" (стиль именования провайдера - {1})",
                        symbol, FixApplication.ProviderNaming);
                    return false;
                }
                symbol = ownSymbol;
            }
            return true;
        }

        private static string GetRequestedTickerFromRequestId(string reqId)
        {
            if (string.IsNullOrEmpty(reqId))
                return reqId;
            var match = tickerFromRequestRegex.Match(reqId);
            return match.Value ?? "";
        }
    }
}

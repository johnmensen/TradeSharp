using System;
using System.Collections.Generic;
using Castle.Components.DictionaryAdapter;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.Test.Moq
{
    static class MoqTradeSharpDictionary
    {
        public delegate List<TradeTicker> GetQuotesDel(out long hash);

        private static ITradeSharpDictionary mock;

        public static ITradeSharpDictionary Mock
        {
            get
            {
                if (mock != null) return mock;
                mock = ProxyBuilder.Instance.MakeImplementer<ITradeSharpDictionary>(true);
                // "замочить" нужные методы
                long hsh;
                ((IMockableProxy)mock).MockMethods.Add(
                    StronglyName.GetMethodName<ITradeSharpDictionary>(m => m.GetTickers(out hsh)),
                    // название метода - SendNewOrderRequest
                    // тело мок-метода
                    new GetQuotesDel((out long hash) =>
                        {
                            hash = 0;
                            return new List<TradeTicker>
                                {
                                    #region Tickers
                                    new TradeTicker
                                        {
                                            Title = "EURUSD",
                                            ActiveBase = "EUR",
                                            ActiveCounter = "USD",
                                            CodeFXI = 1,
                                            Precision = 4,
                                            Instrument = Instrument.Spot
                                        },
                                    new TradeTicker
                                        {
                                            Title = "EURGBP",
                                            ActiveBase = "EUR",
                                            ActiveCounter = "GBP",
                                            CodeFXI = 9,
                                            Precision = 4,
                                            Instrument = Instrument.Spot
                                        },
                                    new TradeTicker
                                        {
                                            Title = "EURJPY",
                                            ActiveBase = "EUR",
                                            ActiveCounter = "JPY",
                                            CodeFXI = 6,
                                            Precision = 2,
                                            Instrument = Instrument.Spot
                                        },
                                    new TradeTicker
                                        {
                                            Title = "GBPUSD",
                                            ActiveBase = "GBP",
                                            ActiveCounter = "USD",
                                            CodeFXI = 2,
                                            Precision = 4,
                                            Instrument = Instrument.Spot
                                        },
                                    new TradeTicker
                                        {
                                            Title = "GBPJPY",
                                            ActiveBase = "GBP",
                                            ActiveCounter = "JPY",
                                            CodeFXI = 15,
                                            Precision = 2,
                                            Instrument = Instrument.Spot
                                        },
                                    new TradeTicker
                                        {
                                            Title = "USDJPY",
                                            ActiveBase = "USD",
                                            ActiveCounter = "JPY",
                                            CodeFXI = 3,
                                            Precision = 2,
                                            Instrument = Instrument.Spot
                                        },
                                    new TradeTicker
                                        {
                                            Title = "USDCHF",
                                            ActiveBase = "USD",
                                            ActiveCounter = "CHF",
                                            CodeFXI = 4,
                                            Precision = 4,
                                            Instrument = Instrument.Spot
                                        },
                                    new TradeTicker
                                        {
                                            Title = "NZDUSD",
                                            ActiveBase = "NZD",
                                            ActiveCounter = "USD",
                                            CodeFXI = 8,
                                            Precision = 4,
                                            Instrument = Instrument.Spot
                                        },
                                    new TradeTicker
                                        {
                                            Title = "AUDUSD",
                                            ActiveBase = "AUD",
                                            ActiveCounter = "USD",
                                            CodeFXI = 7,
                                            Precision = 4,
                                            Instrument = Instrument.Spot
                                        },
                                    new TradeTicker
                                        {
                                            Title = "USDRUB",
                                            ActiveBase = "USD",
                                            ActiveCounter = "RUB",
                                            CodeFXI = 35,
                                            Precision = 2,
                                            Instrument = Instrument.Spot
                                        },
                                    new TradeTicker
                                        {
                                            Title = "USDCAD",
                                            ActiveBase = "USD",
                                            ActiveCounter = "CAD",
                                            CodeFXI = 5,
                                            Precision = 4,
                                            Instrument = Instrument.Spot
                                        },
                                    new TradeTicker
                                        {
                                            Title = "INDUSD",
                                            ActiveBase = "IND",
                                            ActiveCounter = "USD",
                                            CodeFXI = 1000,
                                            Precision = 4,
                                            Instrument = Instrument.Spot
                                        },
                                    new TradeTicker
                                        {
                                            Title = "EURCAD",
                                            ActiveBase = "EUR",
                                            ActiveCounter = "CAD",
                                            CodeFXI = 13,
                                            Precision = 4,
                                            Instrument = Instrument.Spot
                                        }
                                    #endregion
                                };
                        }));

                ((IMockableProxy) mock).MockMethods.Add(
                    StronglyName.GetMethodName<ITradeSharpDictionary>(m => m.GetMetadataByCategory("")),
                    new Func<string, Dictionary<string, object>>(s =>
                        {
                            if (s == "DayOff")
                                return new Dictionary<string, object>
                                    {
                                        {"New Year's Day", "D:01/01/-;0-24"},
                                        {"New Year holiday", "D:02/01/-;0-96"},
                                        {"Christmas", "D:07/01/-;0-24"},
                                        {"Defender of the Fatherland Day", "D:23/02/-;0-24"},
                                        {"Internation Women's Day", "D:08/03/-;0-24"},
                                        {"Spring and Labour Day", "D:01/05/-;0-24"},
                                        {"Victory Day", "D:09/05/-;0-24"},
                                        {"Russia Day", "D:12/06/-;0-24"},
                                        {"Unity Day", "D:04/11/-;0-24"},
                                        {"WeekEnd", "WD:6;1-51"}
                                    };
                            return new Dictionary<string, object>();
                        }));

                ((IMockableProxy)mock).MockMethods.Add(
                    StronglyName.GetMethodName<ITradeSharpDictionary>(m => m.GetAccountGroupsWithSessionInfo()),
                    new Func<List<AccountGroup>>(() => new EditableList<AccountGroup>
                        {
                            new AccountGroup
                                {
                                    Code = "Demo",
                                    BrokerLeverage = 100,
                                    Name = "Demo",
                                    StopoutPercentLevel = 90,
                                    MarginCallPercentLevel = 50
                                },
                            new AccountGroup
                                {
                                    Code = "Real",
                                    BrokerLeverage = 50,
                                    Name = "Real",
                                    IsReal = true
                                }
                        }));
                return mock;
            }
        }
    }
}

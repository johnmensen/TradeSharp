using System;
using System.Collections.Generic;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.Server.Contract
{
    class TradingContractDictionary
    {
        #region Singletone

        private static readonly Lazy<TradingContractDictionary> instance =
            new Lazy<TradingContractDictionary>(() => new TradingContractDictionary());

        public static TradingContractDictionary Instance
        {
            get { return instance.Value; }
        }

        private TradingContractDictionary()
        {
            tickersList = new ThreadSafeUpdatingList<TradeTicker>(lockIntervalMils, updateIntervalMils, TickersUpdateRoutine);
        }

        #endregion

        private static readonly int updateIntervalMils = AppConfig.GetIntParam("Dictionary.UpdateIntervalMils", 1000 * 15);

        private static readonly int lockIntervalMils = AppConfig.GetIntParam("Dictionary.LockIntervalMils", 1000);

        private readonly LotByGroupDictionary lotByGroup = new LotByGroupDictionary
            {
                dictionary = new Dictionary<string, Dictionary<string, Cortege2<int, int>>>()
            };
        
        private readonly ThreadSafeTimeStamp lastTimeLotByGroupUpdated = new ThreadSafeTimeStamp();

        /// <summary>
        /// авто-обновляемый список тикеров
        /// </summary>
        private readonly ThreadSafeUpdatingList<TradeTicker> tickersList;


        public List<TradeTicker> GetTickers(out long lotByGroupHash)
        {
            try
            {
                UpdateLotByGroup();
                lotByGroupHash = lotByGroup.calculatedHashCode;
                return tickersList.GetItems();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в TradingContractDictionary.GetTickers()", ex);
                lotByGroupHash = 0;
                return new List<TradeTicker>();
            }
        }

        public LotByGroupDictionary GetLotByGroup()
        {
            UpdateLotByGroup();
            return lotByGroup;
        }

        private static List<TradeTicker> TickersUpdateRoutine()
        {
            using (var ctx = DatabaseContext.Instance.Make())
            {
                try
                {
                    var list = new List<TradeTicker>();
                    foreach (var c in ctx.SPOT)
                    {
                        var ticker = new TradeTicker
                        {
                            Title = c.Title,
                            ActiveBase = c.ComBase,
                            ActiveCounter = c.ComCounter,
                            Description = c.Description,
                            Precision = c.Precise,
                            SwapBuy = c.SwapBuy,
                            SwapSell = c.SwapSell,
                            CodeFXI = c.CodeFXI
                        };
                        list.Add(ticker);
                    }

                    return list;
                }
                catch (Exception ex)
                {
                    Logger.Error("DictionaryManager - ошибка в TickersUpdateRoutine()", ex);
                    return new List<TradeTicker>();
                }
            }
        }
    
        private void UpdateLotByGroup()
        {
            var timUpdated = lastTimeLotByGroupUpdated.GetLastHitIfHitted();
            if (timUpdated.HasValue)
            {
                var deltaMils = (DateTime.Now - timUpdated.Value).TotalMilliseconds;
                if (deltaMils <= updateIntervalMils) return;
            }

            lotByGroup.Clear();

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    foreach (var lotGroup in ctx.LOT_BY_GROUP)
                    {
                        Dictionary<string, Cortege2<int, int>> dic;
                        if (!lotByGroup.dictionary.TryGetValue(lotGroup.Group, out dic))
                        {
                            dic = new Dictionary<string, Cortege2<int, int>>();
                            lotByGroup.dictionary.Add(lotGroup.Group, dic);
                        }
                        dic.Add(lotGroup.Spot,
                                new Cortege2<int, int>((int) lotGroup.MinVolume, (int) lotGroup.MinStepVolume));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в UpdateLotByGroup()", ex);
                throw;
            }
            finally
            {
                lotByGroup.GetHashCodeForDic();
                lastTimeLotByGroupUpdated.Touch();
            }

            lotByGroup.GetHashCodeForDic();
        }
    }
}

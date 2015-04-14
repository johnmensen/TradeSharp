using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TradeSharp.Util;

namespace TradeSharp.Contract.Entity
{
    /// <summary>
    /// словарь группа - инструмент - торгуемый объем (мин. и шаг объема)
    /// </summary>
    [Serializable]
    public class LotByGroupDictionary
    {
        /// <summary>
        /// группа - (тикер)
        /// </summary>
        public Dictionary<string, Dictionary<string, Cortege2<int, int>>> dictionary;

        public long calculatedHashCode;

        /// <summary>
        /// получить аккуратный hash - код
        /// </summary>
        /// <returns></returns>
        public long GetHashCodeForDic()
        {
            long? hash = null;
            foreach (var pair in dictionary)
            {
                var pairHash = MakeHashSum(pair.Key.GetHashCode(), MakeHashFromDic(pair.Value));
                hash = hash.HasValue ? MakeHashSum(hash.Value, pairHash) : pairHash;
            }
            return calculatedHashCode = (hash ?? 0);
        }

        public static LotByGroupDictionary LoadFromFile(string path)
        {
            var dic = new LotByGroupDictionary
                {
                    dictionary = new Dictionary<string, Dictionary<string, Cortege2<int, int>>>()
                };

            if (!File.Exists(path)) return dic;
            try
            {
                using (var sr = new StreamReader(path, Encoding.UTF8))
                {
                    Dictionary<string, Cortege2<int, int>> curGroup = null;
                    string curTicker = string.Empty;

                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (string.IsNullOrEmpty(line)) continue;
                        line = line.Trim();

                        // строка вида [DemoGroup] или GBPUSD 10000 5000
                        if (line.StartsWith("["))
                        {
                            curTicker = line.Substring(1, line.Length - 2);
                            curGroup = new Dictionary<string, Cortege2<int, int>>();
                            dic.dictionary.Add(curTicker, curGroup);

                            continue;
                        }
                        if (curGroup == null) continue;

                        var parts = line.Split(new [] {' '}, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length != 3) continue;
                        var minVolm = parts[1].ToIntSafe();
                        if (!minVolm.HasValue) continue;
                        var stepVolm = parts[2].ToIntSafe();
                        if (!stepVolm.HasValue) continue;

                        curGroup.Add(parts[0], new Cortege2<int, int>(minVolm.Value, stepVolm.Value));                        
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в LotByGroupDictionary.LoadFromFile()", ex);                
            }

            // посчитать хеш код
            dic.GetHashCodeForDic();
            return dic;
        }
        
        public void SaveInFile(string filePath)
        {
            try
            {
                using (var sw = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    foreach (var pair in dictionary)
                    {
                        sw.WriteLine("[" + pair.Key + "]");
                        foreach (var lot in pair.Value)
                        {
                            sw.WriteLine(lot.Key + " " + lot.Value.a + " " + lot.Value.b);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в LotByGroupDictionary.SaveInFile()", ex);
            }
        }

        public void Clear()
        {
            dictionary.Clear();
            calculatedHashCode = 0;
        }

        public Cortege2<int, int>? GetMinStepLot(string group, string ticker)
        {
            Dictionary<string, Cortege2<int, int>> dic;
            if (!dictionary.TryGetValue(group, out dic)) return null;

            Cortege2<int, int> tickerLot;
            if (!dic.TryGetValue(ticker, out tickerLot)) return null;
            return tickerLot;
        }

        private static long MakeHashFromDic(Dictionary<string, Cortege2<int, int>> dic)
        {
            long? hash = null;
            foreach (var pair in dic)
            {
                var keyHash = pair.Key.GetHashCode();
                var valHash = pair.Value.a*31 + pair.Value.b;
                var pairHash = MakeHashSum(keyHash, valHash);

                hash = hash.HasValue ? MakeHashSum(hash.Value, pairHash) : pairHash;
            }
            return hash ?? 0;
        }

        private static long MakeHashSum(long hash1, long hash2)
        {
            var finalHash = hash1 ^ hash2;
            return finalHash != 0 ? finalHash : hash1 != 0 ? hash1 : hash2;
        }
    }
}

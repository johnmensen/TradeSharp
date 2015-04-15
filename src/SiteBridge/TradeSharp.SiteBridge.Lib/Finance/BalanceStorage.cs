using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace TradeSharp.SiteBridge.Lib.Finance
{
    /// <summary>
    /// зачитывает информацию о вводе-выводе средств на депозит
    /// фейковая часть (файл) и данные со счета
    /// </summary>
    public class BalanceStorage
    {
        private static BalanceStorage instance;
        public static BalanceStorage Instance
        {
            get { return instance ?? (instance = new BalanceStorage()); }
        }


        private BalanceStorage()
        {
        }

        /// <summary>
        /// вернуть изменения баланса с указанной даты по настоящий момент
        /// берутся из БД
        /// </summary>                
        public List<BalanceChange> GetBalanceChanges(int accountId)
        {
            // фейковая часть
            var changes = ReadFakePart();
            // данные со счета
            try
            {
                List<BalanceChange> realInOuts;
                TradeSharpAccount.Instance.proxy.GetBalanceChanges(accountId, null, out realInOuts);
                if (realInOuts != null && realInOuts.Count > 0)
                    changes.AddRange(realInOuts.OrderBy(d => d.ValueDate));
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("BalanceStorage: ошибка получения баланса по счету {0}: {1}",
                    accountId, ex);
            }
            
            return changes;
        }

        private List<BalanceChange> ReadFakePart()
        {
            var changes = new List<BalanceChange>();
            var fileName = ExecutablePath.ExecPath + "\\deposit_inout.xls";
            if (!File.Exists(fileName)) return changes;
            
            using (var sr = new StreamReaderLog(fileName))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line)) continue;
                    // 14.06.2008 00:00 -200000 1
                    var parts = line.Split(new[] { (char)9 }, StringSplitOptions.None);
                    if (parts.Length != 3) return changes;
                    var date = parts[0].ToDateTimeUniformSafe();
                    var amount = parts[1].ToIntSafe();
                    var changeType = parts[2] == "1"
                                         ? BalanceChangeType.Deposit
                                         : BalanceChangeType.Withdrawal;
                    if (!date.HasValue || !amount.HasValue) continue;
                    changes.Add(new BalanceChange { ValueDate = date.Value,
                                                    Amount = amount.Value,
                                                    CurrencyToDepoRate = 1,
                                                    ChangeType = changeType });
                }
            }
            return changes;
        }
    }        
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using TradeSharp.Contract.Entity;
using TradeSharp.DealerInterface;
using TradeSharp.Linq;
using TradeSharp.Util;
using System.Linq;

namespace TradeSharp.Server.Contract
{
    public partial class ManagerTrade
    {
        private readonly FloodSafeLogger loggerNoFlood = new FloodSafeLogger(2000);
        private const int LogMagicNoDealer = 1;
        private const int LogMagicDealerExecutionError = 2;

        /// <summary>
        /// код группы - обслуживающий дилер
        /// </summary>
        private readonly Dictionary<string, IDealer> dealers = new Dictionary<string, IDealer>();

        public void SaveProviderMessage(BrokerOrder msg)
        {
            using (var ctx = DatabaseContext.Instance.Make())
            {
                var brokOrder = LinqToEntity.UndecorateBrokerOrder(msg);                
                ctx.BROKER_ORDER.Add(brokOrder);
                ctx.SaveChanges();
            }
        }

        /// <summary>
        /// возвращает словарь response - request
        /// значения м.б. пустыми
        /// </summary>        
        /// <returns>response - request</returns>
        public Dictionary<BrokerResponse, BrokerOrder>
            FindRequestsForExecutionReports(List<BrokerResponse> reports)
        {
            var reqDic = new Dictionary<BrokerResponse, BrokerOrder>();
            if (reports.Count == 0) return reqDic;
            var requestIds = reports.Select(r => r.RequestId);
            using (var ctx = DatabaseContext.Instance.Make())
            {
                var query = from ord in ctx.BROKER_ORDER
                            where requestIds.Contains(ord.RequestID)
                            select new BrokerOrder
                                       {
                                           Id = ord.ID,
                                           RequestId = ord.RequestID,
                                           Ticker = ord.Ticker,
                                           Instrument = (Instrument)ord.Instrument,
                                           Volume = ord.Volume,
                                           Side = ord.Side,
                                           OrderPricing = (OrderPricing)ord.OrderPricing,
                                           RequestedPrice = ord.RequestedPrice,
                                           Slippage = ord.Slippage,
                                           DealerCode = ord.Dealer,
                                           AccountID = ord.AccountID,
                                           ClosingPositionID = ord.ClosingPositionID,
                                           TimeCreated = ord.TimeCreated,
                                           MarkupAbs = (float)ord.Markup
                                       };
                foreach (var ord in query)
                {
                    var ordId = ord.RequestId;
                    var report = reports.Find(r => r.RequestId == ordId);
                    if (reqDic.ContainsKey(report))
                    {
                        Logger.DebugFormat("FindRequestsForExecutionReports: множественные ответы для ордера [{0}]",
                            ordId);
                        continue;
                    }
                    reqDic.Add(report, ord);
                }
                Logger.DebugFormat("Found {0} RequestsFor {1} ExecutionReports", reqDic.Count, reports.Count);
                return reqDic;
            }
        }

        public string GetDealersErrorString()
        {
            var sb = new StringBuilder();
            foreach (var d in dealers)
            {
                var str = d.Value.GetErrorString();
                if (string.IsNullOrEmpty(str)) continue;
                sb.AppendLine(string.Format("[{0}] {1}", d.Key, str));
            }
            return sb.ToString();
        }

        public void ClearDealersError()
        {
            foreach (var dealer in dealers.Values)
                dealer.ClearError();
        }

        private void InitializeDealers()
        {
            // код дилера - код группы
            var groupsWithDealers = brokerRepository.GetAccountGroupsWithSessionInfo();
            // группы, которые обслуживает дилер
            var groupsByDealer = new Dictionary<string, List<string>>();
            Logger.Debug(string.Concat(groupsWithDealers.Select(gd => string.Format("[Группа:{0}, Дилер:{1}]",
                            gd.Code, gd.Dealer == null ? "-" : gd.Dealer.Code)).ToArray()));

            // детализовать информацию по дилерам
            List<DealerDescription> dealerList;
            using (var ctx = DatabaseContext.Instance.Make())
            {
                var dealersQuery = from dl in ctx.DEALER
                                   select new DealerDescription
                                              {
                                                  Code = dl.Code,
                                                  DealerEnabled = dl.DealerEnabled,
                                                  FileName = dl.FileName
                                              };
                dealerList = dealersQuery.ToList();
            }
            foreach (var group in groupsWithDealers)
            {
                if (group.Dealer == null) continue;
                if (!groupsByDealer.ContainsKey(group.Dealer.Code))
                    groupsByDealer.Add(group.Dealer.Code, new List<string> { group.Code });
                else
                    groupsByDealer[group.Dealer.Code].Add(group.Code);
            }

            var dealerInstances = new List<IDealer>();
            // создать экземпляры классов дилеров, подгрузив их из сборок
            foreach (var dealerDesc in dealerList)
            {
                // не загружать дилеров, которые не обслуживают группы
                if (!groupsByDealer.ContainsKey(dealerDesc.Code))
                {
                    Logger.DebugFormat("Дилер [{0}] не связан ни с одной группой и не загружен",
                        dealerDesc.Code);                    
                    continue;
                }
                // загрузить из файла
                var path = GetFullDealerLibPath(dealerDesc.FileName);
                if (!File.Exists(path))
                    throw new FileNotFoundException(string.Format("Не найдена библиотека дилера {0}", dealerDesc.Code),
                        path);
                Assembly dealerAsm;
                try
                {
                    dealerAsm = Assembly.LoadFrom(path);
                }
                catch (Exception ex)
                {                    
                    Logger.ErrorFormat("Ошибка при загрузке библиотеки дилера {0}: {1}", dealerDesc.Code, ex);
                    throw new Exception(string.Format("Ошибка при загрузке библиотеки дилера {0}", dealerDesc.Code));
                }
                
                foreach (var dealerType in dealerAsm.GetTypes())
                {
                    var dealerInterface = dealerType.GetInterface("IDealer");
                    if (dealerInterface == null)
                    {
                        Logger.DebugFormat("[{0}] экспортирует вспомогательный тип [{1}]",
                            dealerDesc.FileName, dealerType);
                        continue;
                    }
                    Logger.DebugFormat("[{0}] экспортирует тип [{1}] (IDealer)",
                            dealerDesc.FileName, dealerType);
                    // обязательные параметры - List<string> groupCodes                    
                    // (группы, которые обслуживает дилер)
                    try
                    {
                        var dealer = (IDealer)Activator.CreateInstance(dealerType, dealerDesc,
                            groupsByDealer[dealerDesc.Code]);
                        dealer.ServerInterface = this;
                        dealer.Initialize();
                        dealerInstances.Add(dealer);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorFormat("Ошибка загрузки IDealer из {0}: {1}", dealerDesc.FileName, ex);
                        throw;
                    }                    
                }
            }

            // заполнить словарь - группа-дилер
            foreach (var d in dealerInstances)
            {
                foreach (var groupCode in d.GroupCodes)
                {
                    if (dealers.ContainsKey(groupCode))
                    {
                        var msg = string.Format("С группой [{0}] связано более одного дилера", groupCode);
                        Logger.Error(msg);
                        throw new Exception(msg);
                    }
                    dealers.Add(groupCode, d);
                }
            }
        }

        private static string GetFullDealerLibPath(string fileName)
        {
            return string.Format("{0}\\dealers\\{1}", ExecutablePath.ExecPath, fileName);
        }

        private IDealer GetDealerByAccount(int accountId, out Account accountTyped)
        {
            accountTyped = accountRepository.GetAccount(accountId);
            if (accountTyped == null) return null;
            var dealer = GetDealerByGroup(accountTyped.Group);
            return dealer;
        }

        private IDealer GetDealerByGroup(string group)
        {
            if (!dealers.ContainsKey(group))
                Logger.DebugFormat("Нет дилера для группы [{0}]. Дилеры по группам: {1}",
                    group,
                    string.Concat(dealers.Keys.Select(k => string.Format("[{0}]", k))));
            return dealers.ContainsKey(group) ? dealers[group] : null;
        }

        /// <summary>
        /// обработать ответ от провайдера: раздать дилерам Execution reports по своим ордерам
        /// </summary>
        public void OnProviderReport(Dictionary<BrokerResponse, BrokerOrder> reports)
        {
            // сохранить отчеты об исполнении
            using (var ctx = DatabaseContext.Instance.Make())
            {
                try
                {
                    foreach (var report in reports.Keys)
                    {
                        var repoDbItem = LinqToEntity.UndecorateBrokerResponse(report);
                        Logger.DebugFormat("report [{0}, {1:f5}, {2:ddMMyyyy HH:mm}, {3}", 
                            repoDbItem.ID, repoDbItem.Price ?? 0, 
                            repoDbItem.ValueDate, repoDbItem.Status);
                        ctx.BROKER_RESPONSE.Add(repoDbItem);                        
                    }
                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка сохранения отчетов брокеров в БД", ex);                    
                }                
            }

            // обработать сообщения
            foreach (var reqPair in reports)
            {
                var dealerCode = reqPair.Value.DealerCode;
                var dealer = dealers.Values.FirstOrDefault(d => d.DealerCode == dealerCode);
                if (dealer == null)
                {
                    loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Debug,
                                                             LogMagicNoDealer, 1000*60*5,
                                                             "OnProviderReport: дилер [{0}] не найден", dealerCode);
                    continue;
                }
                var dealerFix = dealer as IFixDealer;
                if (dealerFix == null)
                {
                    loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Debug,
                                                             LogMagicNoDealer, 1000 * 60 * 5,
                                                             "OnProviderReport: сообщение для дилера \"{0}\" ({1}), " + 
                                                             "не являющегося FIX-дилером",
                                                             dealerCode, dealer.GetType());
                    continue;
                }                
                try
                {
                    if (reqPair.Value == null)
                        Logger.ErrorFormat("OnProviderReport - для ответа сервера Id={0} (цена {1:f4}) не найден запрос",
                            reqPair.Key.Id, reqPair.Key.Price);
                    else
                        dealerFix.ProcessExecutionReport(reqPair.Key, reqPair.Value);
                }
                catch (Exception ex)
                {
                    loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Debug,
                                                             LogMagicDealerExecutionError, 1000 * 60 * 5,
                                                             "OnProviderReport: ошибка обработки ответа брокера дилером [{0}]: {1}", 
                                                             dealerCode, ex);                    
                }                
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Linq;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace TradeSharp.SiteBridge.Lib.Finance
{
    public class DealStorage
    {
        private static DealStorage instance;
        public static DealStorage Instance
        {
            get { return instance ?? (instance = new DealStorage()); }
        }
        
        private DealStorage()
        {
        }

        /// <summary>
        /// вернуть сделки FXI, отфильтрованные на предмет засадных
        /// берутся из БД для набора счетов
        /// </summary>                
        public List<MarketOrder> GetDeals(int accountId)
        {
            // ордера фейковые, из файла
            var allOrders = ReadFakeDeals(accountId);
            // риал-тайм ордера из БД

            List<MarketOrder> histOrders, openOrders;
            try
            {
                TradeSharpAccount.Instance.proxy.GetHistoryOrders(accountId, null, out histOrders);
                TradeSharpAccount.Instance.proxy.GetMarketOrders(accountId, out openOrders);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("DealStorage: GetDeals({0}) error: {1}", accountId, ex);
                throw;
            }

            if (histOrders != null) allOrders.AddRange(histOrders);
            if (openOrders != null) allOrders.AddRange(openOrders);

            return allOrders.OrderBy(d => d.TimeEnter).ToList();
        }

        public List<MarketOrder> GetOpenedDeals(int accountId)
        {
            List<MarketOrder> openOrders;
            try
            {
                TradeSharpAccount.Instance.proxy.GetMarketOrders(accountId, out openOrders);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("DealStorage: GetOpenedDeals({0}) error: {1}", accountId, ex);
                throw;
            }
            return openOrders;
        }

        public List<MarketOrder> GetClosedDeals(int accountId)
        {
            List<MarketOrder> histOrders;
            try
            {
                TradeSharpAccount.Instance.proxy.GetHistoryOrders(accountId, null, out histOrders);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("DealStorage: GetOpenedDeals({0}) error: {1}", accountId, ex);
                throw;
            }
            return histOrders;
        }

        private static List<MarketOrder> ReadFakeDeals(int accountId)//, DateTime startTime)
        {
            var orders = new List<MarketOrder>();
            var fileName = ExecutablePath.ExecPath + "\\olddeals.xls";
            if (!File.Exists(fileName)) return orders;
            using (var sr = new StreamReaderLog(fileName))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line)) continue;
                    // Buy	EURUSD	1.5408	14.06.2008 00:00	1.5925	11.07.2008 00:00    20000
                    var parts = line.Split(new[] { (char)9 }, StringSplitOptions.None);
                    if (parts.Length != 7) continue;
                    var side = parts[0] == "Buy" ? 1 : -1;
                    var symbol = parts[1];
                    var priceEnter = parts[2].ToFloatUniform();
                    DateTime timeEnter;
                    try
                    {
                        timeEnter = parts[3].ToDateTimeUniform();
                    }
                    catch
                    {
                        Logger.ErrorFormat("Error parsing time ({0}) from str {1}", parts[3], line);
                        throw;
                    }
                    var priceExit = parts[4].ToFloatUniformSafe();
                    var timeExit = parts[5].ToDateTimeUniformSafe();
                    var volume = parts[6].ToInt();

                    orders.Add(new MarketOrder
                    {
                        AccountID = accountId,
                        PriceEnter = priceEnter,
                        TimeEnter = timeEnter,
                        PriceExit = priceExit,
                        TimeExit = timeExit,
                        Volume = volume,
                        Side = side,
                        Symbol = symbol
                    });
                }
            }
            return orders;
        }
    }


    /// <summary>
    /// 
    /// [J]ID
    /// 
    /// [K]=ЕСЛИ(A2="EURUSD";1;ЕСЛИ(A2="GBPUSD";2;ЕСЛИ(A2="USDJPY";3;ЕСЛИ(A2="EURGBP";9;4))))
    /// 
    /// [L]=ЕСЛИ(A1="USDJPY";30000;50000)
    /// 
    /// INSERT INTO SUB_DEAL(reserve_risk, reserve_amount, reserve_mo, deal, uni_trd_account, 
    /// sub_deal, trd_currency, deal_type, deal_status, open_date, create_date, close_date, entry_cost, exit_cost, base_amount) VALUES (1000, 1000, 1000, 17090, 163, 
    /// 
    /// =СЦЕПИТЬ(O2;J2;",";K2;",";C2;",6,'";ТЕКСТ(F2;"ггггММдд чч:мм:сс");"','";
    /// ТЕКСТ(F2;"ггггММдд чч:мм:сс");"','";ТЕКСТ(G2;"ггггММдд чч:мм:сс");"',";D2;",";E2;",";L2;")")
    /// 
    /// INSERT INTO SUB_DEAL(reserve_risk, reserve_amount, reserve_mo, deal, uni_trd_account, sub_deal, trd_currency, deal_type, deal_status, open_date, create_date, close_date, entry_cost, exit_cost, base_amount) VALUES (1000, 1000, 1000, 17090, 163, 21232,3,-1,6,'20080708 13:16', '20080708 13:16','20080709 00:04',106.74,107,50000)
    /// </summary>
    public class DealStorageOld
    {
        private static DealStorageOld instance;
        public static DealStorageOld Instance
        {
            get
            {
                if (instance == null) instance = new DealStorageOld();
                return instance;
            }
        }

        private const int FXIDealStatusOpened = 3, FXIDealStatusClosed = 6;
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["FXI"].ConnectionString;


        /// <summary>
        /// вернуть сделки FXI, отфильтрованные на предмет засадных
        /// берутся из БД для набора счетов
        /// </summary>                
        public List<MarketOrder> GetFXIDeals(List<int> accountIds, DateTime startTime, bool filterDeals)
        {
            var deals = new List<MarketOrder>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var command = BuildSelectCommand(connection, accountIds, startTime);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader == null)
                        {
                            Logger.DebugFormat("GetDeals: ExecuteReader = null");
                            return deals;
                        }
                        while (reader.Read())
                        {
                            var order = ParseOrder(reader);
                            if (order != null) deals.Add(order);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("DealStorage.GetDeals error", ex);
            }

            return filterDeals ? FilterBadDeals(deals) : deals;
        }

        private List<MarketOrder> FilterBadDeals(List<MarketOrder> deals)
        {
            var filtered = new List<MarketOrder>();
            const int WorstAllowedPoints = -100;
            foreach (var deal in deals)
            {
                if (deal.IsOpened) continue; // открытые априорно убыточны
                // посчитать доходность в пунктах и время жизни сделки
                var deltaAbs = (deal.PriceExit.Value - deal.PriceEnter) * deal.Side;
                var deltaPP = DalSpot.Instance.GetPointsValue(deal.Symbol, deltaAbs);
                if (deltaPP < WorstAllowedPoints) continue;
                filtered.Add(deal);
            }
            return filtered;
        }

        private static SqlCommand BuildSelectCommand(SqlConnection connection,
           List<int> accountIds, DateTime start)
        {
            var accountCompareSubstr = new StringBuilder();
            for (var i = 0; i < accountIds.Count; i++)
            {
                if (i > 0) accountCompareSubstr.Append(" OR ");
                accountCompareSubstr.AppendFormat("uni_trd_account = @account{0}", i);
            }

            var queryStr = string.Format("SELECT d.sub_deal, d.trd_currency, d.deal_type, d.deal_status, d.open_date, " +
                                         "d.close_date, d.entry_cost, d.exit_cost, d.base_amount " +
                                         "FROM SUB_DEAL d " +
                                         "WHERE (d.close_date IS NULL OR d.close_date >= @start) AND ({2}) " +
                                         "AND d.deal_status IN ({0}, {1}) ORDER BY d.open_date",
                                         FXIDealStatusOpened,
                                         FXIDealStatusClosed,
                                         accountCompareSubstr);

            var command = new SqlCommand(queryStr, connection);
            command.Parameters.Add(new SqlParameter("@start", start));

            for (var i = 0; i < accountIds.Count; i++)
                command.Parameters.Add(new SqlParameter(string.Format("@account{0}", i), accountIds[i]));

            return command;
        }

        private static MarketOrder ParseOrder(SqlDataReader r)
        {
            try
            {
                var order = new MarketOrder { ID = ((int)r[0]) };
                var tickerFXI = (int)r[1];
                order.Symbol = DalSpot.Instance.GetSymbolByFXICode(tickerFXI);
                order.Side = (int)r[2];
                order.State = (int)r[3] == FXIDealStatusClosed ? PositionState.Closed : PositionState.Opened;
                order.TimeEnter = (DateTime)r[4];
                order.TimeExit = r[5] == DBNull.Value ? (DateTime?)null : (DateTime)r[5];
                order.PriceEnter = (float)(decimal)r[6];
                order.PriceExit = (float?) (r[7] == DBNull.Value ? (decimal?)null : (decimal)r[7]);
                order.Volume = (int)((decimal)r[8]);
                return order;
            }
            catch (Exception ex)
            {
                Logger.Error("DealStorage.ParseOrder error: ", ex);
                return null;
            }
        }
    }
}

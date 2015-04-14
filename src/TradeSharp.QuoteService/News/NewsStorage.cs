using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.ServiceModel;
using System.Linq;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace TradeSharp.QuoteService.News
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class NewsStorage : INewsStorage
    {
        private static NewsStorage instance;
        public static NewsStorage Instance
        {
            get { return instance ?? (instance = new NewsStorage()); }
        }

        private readonly TradeSharpServerTrade proxyAccount;
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["QuoteBase"].ConnectionString;        
        
        private NewsStorage()
        {
            try
            {
                proxyAccount = new TradeSharpServerTrade(EmptyTradeSharpCallbackReceiver.Instance);
            }
            catch (Exception ex)
            {
                Logger.Error("NewsStorage ctor: ошибка создания proxyAccount", ex);
            }
        }
        
        public List<Contract.Entity.News> GetNews(int account, DateTime date, int[] newsChannelIds)
        {
            if (proxyAccount == null)
            {
                Logger.Debug("GetNews: proxyAccount is null");
                return new List<Contract.Entity.News>();
            }
            List<int> channels;
            try
            {
                channels = TradeSharpAccount.Instance.proxy.GetAccountChannelIDs(account);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("NewsStorage.GetNews: ошибка в GetAccountChannelIDs({0}): {1}",
                    account, ex);
                return new List<Contract.Entity.News>();
            }
            if (channels.Count == 0)
            {
                Logger.DebugFormat("GetNews: no channels for account {0}", account);
                return new List<Contract.Entity.News>();
            }
            channels = channels.Where(newsChannelIds.Contains).ToList();
            if (channels.Count == 0) return new List<Contract.Entity.News>();

            return GetNewsFromDb(channels, date);
        }

        public NewsMap GetNewsMap(int accountId)
        {
            var channels = GetChannelsByAccount(accountId);
            if (channels == null)
                return null;

            var map = new NewsMap
                {
                    channelIds = channels.ToArray()
                };
            // сформировать записи - дата - количество новостей по указанным каналам
            // запрос вида
            // select DateNews, COUNT(*) as Count from NEWS where Channel = 1 group by DateNews order by DateNews
            try
            {
                using (var cn = new SqlConnection(connectionString))
                {
                    cn.Open();
                    DbCommand cmd = cn.CreateCommand();
                    cmd.Connection = cn;
                    cmd.CommandText = string.Format("select DateNews, COUNT(*) as Count" +
                        " from NEWS where Channel in ({0}) group by DateNews order by DateNews",
                        string.Join(",", channels));
                    
                    using (var dr = cmd.ExecuteReader())
                    {
                        var recordsList = new List<NewsMapRecord>();
                        while (dr.Read())
                        {
                            recordsList.Add(new NewsMapRecord((DateTime)dr["DateNews"], (int)dr["Count"]));
                        }
                        map.records = recordsList.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("DbNewsStorage.GetNewsMap: возникла ошибка ", ex);
                return null;
            }
            
            return map;
        }

        /// <summary>
        /// получаем из БД список новостей по указанным каналам
        /// </summary>
        private List<Contract.Entity.News> GetNewsFromDb(List<int> channels, DateTime date)
        {
            try
            {
                using (var cn = new SqlConnection(connectionString))
                {
                    cn.Open();
                    DbCommand cmd = cn.CreateCommand();
                    cmd.Connection = cn;
                    cmd.CommandText = string.Format("select * from NEWS where DateNews=@dateNews and Channel in ({0}) order by Minutes",
                        string.Join(",", channels));
                    cmd.Parameters.Add(new SqlParameter("@dateNews", date));
                    
                    using (var dr = cmd.ExecuteReader())
                    {
                        var resList = new List<Contract.Entity.News>();
                        while (dr.Read())
                        {
                            resList.Add(new Contract.Entity.News(
                                (int)dr["Channel"],
                                ((DateTime)dr["DateNews"]).AddMinutes((Int16)dr["Minutes"]),
                                (string)dr["Title"],
                                dr["Body"] == DBNull.Value ? "" : (string)dr["Body"]));
                        }
                        return resList;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("DbNewsStorage.GetNewsFromDb: возникла ошибка ", ex);
            }
            return new List<Contract.Entity.News>();
        }

        private List<int> GetChannelsByAccount(int accountId)
        {
            if (proxyAccount == null)
            {
                Logger.Debug("GetChannelsByAccount: proxyAccount is null");
                return null;
            }
            List<int> channels;
            try
            {
                channels = TradeSharpAccount.Instance.proxy.GetAccountChannelIDs(accountId);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("GetChannelsByAccount: ошибка в GetAccountChannelIDs({0}): {1}",
                    accountId, ex);
                return null;
            }
            if (channels.Count == 0)
            {
                Logger.DebugFormat("GetChannelsByAccount: no channels for account {0}", accountId);
                return null;
            }

            return channels;
        }
    }
}


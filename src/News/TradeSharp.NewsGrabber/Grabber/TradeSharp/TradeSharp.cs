using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.NewsGrabber.Grabber.TradeSharp
{
    class TradeSharpGrabber : BaseNewsGrabber
    {
        private const string SettingsFileName = "tradesharp.txt";

        private readonly int newsChannelId;

        private readonly string newsTitle;

        private readonly string[] tikersToStore;

        private const string GetVolumesSql = "SELECT SUM(CASE WHEN Side > 0 THEN Volume ELSE 0 END) AS Buy, " +
                              "SUM(CASE WHEN Side < 0 THEN Volume ELSE 0 END) AS Sell, Symbol " +
                              "FROM POSITION group by Symbol";

        public TradeSharpGrabber()
        {
            var setsPath = string.Format("{0}\\{1}", 
                ExecutablePath.ExecPath, SettingsFileName);
            if (!File.Exists(setsPath)) return;

            var newsSets = new IniFile(setsPath).ReadSection("news");
            if (newsSets.ContainsKey("channel"))
                newsChannelId = newsSets["channel"].ToIntSafe() ?? 0;

            if (newsChannelId == 0) return;
            newsSets.TryGetValue("title", out newsTitle);
            if (string.IsNullOrEmpty(newsTitle))
                newsTitle = "TRADE# Volumes";

            string tikersToStoreStr;
            newsSets.TryGetValue("tikers", out tikersToStoreStr);
            if (string.IsNullOrEmpty(tikersToStoreStr))
                tikersToStoreStr = "EURUSD;GBPUSD;USDCAD;AUDUSD;USDJPY;USDCHF";
            tikersToStore = tikersToStoreStr.Trim().Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);

            Logger.InfoFormat("Новости TRADE#({0}), канал {1}, {2} тикеров",
                newsTitle, newsChannelId, tikersToStore.Length);
        }

        public override void GetNews()
        {
            if (newsChannelId == 0) return;
            if (tikersToStore.Length == 0) return;

            // объем на покупку / продажу по каждой валютной паре
            var volumes = GetPairVolumes();
            if (volumes.Count == 0) return;

            Logger.InfoFormat("{0} тикеров прочитано (TRADE#)", volumes.Count);

            // сформировать и сохранить на сервере список новостей
            var newsBody = "[#fmt]#&newstype=TradeSharpVolumes#&" + 
                string.Join(";", volumes.Select(v => string.Format("{0}:{1}:{2}", v.Key, v.Value.a, v.Value.b)));
            var news = new News(newsChannelId, DateTime.Now, newsTitle, newsBody);
            PutNewsOnServer(new [] {news});
            Logger.Info("Новость (объемы T#) отправлена на сервер");
        }

        private Dictionary<string, Cortege2<long, long>> GetPairVolumes()
        {
            var result = new Dictionary<string, Cortege2<long, long>>();

            try
            {
                using (var conn = new SqlConnection(
                    ConfigurationManager.ConnectionStrings["TradeSharp"].ConnectionString))
                {
                    conn.Open();
                    DbCommand cmd = conn.CreateCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = GetVolumesSql;

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            result.Add((string)dr["Symbol"],
                                new Cortege2<long, long>((int)dr["Buy"], (int)dr["Sell"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в TradeSharpGrabber.GetPairVolumes()", ex);
            }

            return result;
        }
    }
}

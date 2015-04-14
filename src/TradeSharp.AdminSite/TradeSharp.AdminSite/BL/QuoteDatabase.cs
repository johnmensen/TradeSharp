using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.BL
{
    /// <summary>
    /// обращается непосредственно к БД котировок
    /// </summary>
    public static class QuoteDatabase
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["QuoteBase"].ConnectionString;

        // private const string TableNameQuote = "QUOTE";
        
        /// <summary>
        /// получает котировки (свечи m1) по всем торгуемым активам на момент
        /// времени t меньше либо равный time
        /// </summary>
        public static Dictionary<string, CandleData> GetCandlesOnTime(DateTime? time)
        {
            var candles = new Dictionary<string, CandleData>();
            var cmdText = 
                time.HasValue ?
                "WITH e AS ( select ticker, MAX([date]) as 'mxdate' from QUOTE where [date] <= '"
                + time.Value.ToString("yyyyMMdd HH:mm") +
                "' group by ticker ) select q.* " +
                "FROM e join QUOTE q on q.ticker = e.ticker and q.date = e.mxdate"
                :
                "WITH e AS ( select ticker, MAX([date]) as 'mxdate' from QUOTE group by ticker ) select q.* " +
                "FROM e join QUOTE q on q.ticker = e.ticker and q.date = e.mxdate";
            
            var cmd = new SqlCommand(cmdText);
            var values = new object[4];

            try
            {            
                using (var conn = MakeSqlConnection())
                {
                    cmd.Connection = conn;
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader.GetValues(values) != values.Length) continue;
                            var ticker = (Int16) values[0];
                            var symbol = DalSpot.Instance.GetSymbolByFXICode(ticker);
                            if (string.IsNullOrEmpty(symbol)) continue;

                            var pointValue = DalSpot.Instance.GetPrecision10(symbol);

                            var candle = new CandleData
                            {
                                timeOpen = (DateTime)values[1],
                                open = (float)(double)values[2],                           
                            };
                            var hlc = (int)values[3];
                            candle.MakeHlcFromOffset(hlc, pointValue);
                            candles.Add(symbol, candle);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetCandlesOnTime() - ошибка доступа к БД котировок", ex);
            }

            return candles;
        }

        private static SqlConnection MakeSqlConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Linq
{
    /// <summary>
    /// Класс используется в сайте администратора
    /// </summary>
    public class AdoHalper
    {
        /// <summary>
        /// Вызывает хранимую процедуру 'GetLastQuote' или 'FindQuote'
        /// </summary>
        /// <param name="ticker"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public QuoteData GetQuoteStoredProc(string ticker, DateTime? start = null)
        {
            if (string.IsNullOrEmpty(ticker)) return null;

            SqlConnection conn = null;
            SqlDataReader rdr = null;
            var tickerId = DalSpot.Instance.GetFXICodeBySymbol(ticker);
            var date = default(DateTime);
            var open = -1f;
            var lhc = -1;

            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["QuoteBase"].ConnectionString;
                conn = new SqlConnection(connectionString);
                conn.Open();

                var procName = start.HasValue ? "FindQuote" : "GetLastQuote";

                var cmd = new SqlCommand(procName, conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add(new SqlParameter("@ticker", tickerId));
                if (start.HasValue) cmd.Parameters.Add(new SqlParameter("@date", start.Value));

                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    if (!int.TryParse(rdr["ticker"].ToString(), out tickerId)) return null;
                    if (!DateTime.TryParse(rdr["date"].ToString(), out date)) return null;
                    if (!float.TryParse(rdr["open"].ToString(), out open)) return null;
                    if (!int.TryParse(rdr["HLC"].ToString(), out lhc)) return null;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в методе GetQuoteStoredProc", ex);
                return null;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
                if (rdr != null)
                {
                    rdr.Close();
                }
            }

            var candelDataPacked = new CandleDataPacked
            {
                HLC = lhc,
                open = open,
                timeOpen = date
            };
            var candleData = new CandleData(candelDataPacked, DalSpot.Instance.GetPrecision10(ticker));
            var useOpen = start.HasValue && date == start.Value;
            return GetQuoteWithDefaultSpread(candleData, ticker, useOpen);
        }

        private static QuoteData GetQuoteWithDefaultSpread(CandleData candle, string ticker, bool useCloseTime)
        {
            return useCloseTime 
                ? new QuoteData(candle.close, DalSpot.Instance.GetAskPriceWithDefaultSpread(ticker, candle.close), candle.timeClose)
                : new QuoteData(candle.open, DalSpot.Instance.GetAskPriceWithDefaultSpread(ticker, candle.open), candle.timeOpen);
        }
    }
}

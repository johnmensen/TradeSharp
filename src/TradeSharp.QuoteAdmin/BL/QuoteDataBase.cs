using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.QuoteAdmin.BL
{
    public class QuoteDataBase
    {
        #region Данные
        private static QuoteDataBase instance;

        public static QuoteDataBase Instance
        {
            get { return instance ?? (instance = new QuoteDataBase()); }
        }

        private const string TableNameQuote = "QUOTE";
        
        private const string TableNameQuoteTemp = "#QUOTE_TEMP";

        private const string ProcUpsertQuoteRecord = "[QUOTE$InsertOrUpdate]";
        
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["QuoteBase"].ConnectionString;

        private static Dictionary<int, int> pointCostByTicker;
        #endregion

        private QuoteDataBase()
        {
            pointCostByTicker = new Dictionary<int, int>();

            foreach (var symbol in DalSpot.Instance.GetTickerNames())
            {
                var ticker = DalSpot.Instance.GetFXICodeBySymbol(symbol);
                var precison = DalSpot.Instance.GetPrecision10(symbol);
                if (!pointCostByTicker.ContainsKey(ticker))
                    pointCostByTicker.Add(ticker, precison);
            }
        }

        #region Открытые методы
        
        public SqlConnection MakeSqlConnection()
        {
            return new SqlConnection(connectionString);
        }

        public static List<CandleData> ReadTopNumCandles(string symbol,
            int count, DateTime dateStart, DateTime dateEnd)
        {
            var candles = new List<CandleData>();
            var ticker = DalSpot.Instance.GetFXICodeBySymbol(symbol);
            if (ticker == 0) return candles;
            var pointValue = DalSpot.Instance.GetPrecision10(symbol);

            var cmdText = string.Format("select top({0}) date, [open], HLC from QUOTE where ticker={1} and " +
                                        "date between '{2:yyyyMMdd HH:mm}' and '{3:yyyyMMdd HH:mm}'",
                                        count, ticker, dateStart, dateEnd);
            var cmd = new SqlCommand(cmdText);
            var values = new object[3];

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    cmd.Connection = connection;
                    connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        CandleData previousCandle = null;
                        while (reader.Read())
                        {
                            if (reader.GetValues(values) != values.Length) continue;
                            var candle = new CandleData
                                {
                                    timeOpen = (DateTime) values[0],
                                    open = (float) (double) values[1],
                                };
                            var hlc = (int) values[2];
                            candle.MakeHlcFromOffset16(hlc, pointValue);
                            if (previousCandle != null)
                            {
                                previousCandle.close = candle.open;
                                previousCandle.high = Math.Max(candle.open, previousCandle.high);
                                previousCandle.low = Math.Min(candle.open, previousCandle.low);
                            }
                            previousCandle = candle;
                            candles.Add(candle);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ReadTopNumCandles() error", ex);
            }
            return candles;
        }
        
        public static Cortege2<DateTime, DateTime>? GetFirstAndLastDateByTicker(int code)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var command = BuildSelectTimesCommand(connection, code);
                connection.Open();

                var timeValues = new object[2];
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var count = reader.GetValues(timeValues);
                        if (count == 2)
                            return new Cortege2<DateTime, DateTime>((DateTime)timeValues[0], (DateTime)timeValues[1]);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// формирует пачку многострочных SQL вида
        /// exec QUOTE$InsertOrUpdate @ticker=..
        /// exec QUOTE$InsertOrUpdate ...
        /// </summary>
        public void StoreQuotesByStoredProc(List<CandleData> candles, int ticker)
        {
            var curCommands = new List<string>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                foreach (var candle in candles)
                {
                    var cmd = MakeInsertCommand(candle, ticker);
                    curCommands.Add(cmd);            
                }
                 
                if (curCommands.Count > 0)
                    ExecuteSqlNonQuerySafe(string.Join(Environment.NewLine, curCommands), connection);
            }
        }
        
        /// <summary>
        /// пачкой удаляет котировки на интервале
        /// потом пачкой вставляет новые котировки (просто inset)
        /// </summary>        
        public void StoreQuotesByDeleteInsert(List<CandleData> candles, int ticker, bool rewriteQuotes)
        {
            const int maxCandlesInPack = 80;
            int pointCost;
            if (!pointCostByTicker.TryGetValue(ticker, out pointCost)) return;

            var cmdDeleteSql = string.Format("delete from {0} where ticker=@Tick and date between " +
                                                     "@DateStart and @DateEnd", TableNameQuote);
            var cmdDelete = new SqlCommand(cmdDeleteSql);

            using (var connection = new SqlConnection(connectionString))
            {
                cmdDelete.Connection = connection;
                connection.Open();

                for (var i = 0; i < candles.Count; i += maxCandlesInPack)
                {
                    var endIndex = i + maxCandlesInPack - 1;
                    if (endIndex >= candles.Count)
                        endIndex = candles.Count - 1;

                    if (rewriteQuotes)
                    {
                        // удалить maxCandlesInPack записей таблице на временном интервале, который
                        // займут новые свечки
                        var dateStart = candles[i].timeOpen;
                        var dateEnd = candles[endIndex].timeOpen;
                        cmdDelete.Parameters.Clear();
                        cmdDelete.Parameters.AddWithValue("@Tick", ticker);
                        cmdDelete.Parameters.AddWithValue("@DateStart", dateStart);
                        cmdDelete.Parameters.AddWithValue("@DateEnd", dateEnd);
                        try
                        {
                            cmdDelete.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("StoreQuotesByDeleteInsert() - error deleting rows", ex);
                            break;
                        }
                    }

                    // слепить SQL для добавления новых свечек
                    var sqlStr = new StringBuilder();
                    for (var j = i; j <= endIndex; j++)
                    {
                        sqlStr.AppendLine(string.Format("insert into {0} (ticker, date, [open], HLC) values (" +
                                                        "{1}, '{2:yyyyMMdd HH:mm}', {3}, {4})",
                                                        TableNameQuote, ticker, candles[j].timeOpen,
                                                        candles[j].open.ToStringUniformPriceFormat(true),
                                                        candles[j].GetHlcOffset16(pointCost)));
                    }
                    var cmdInsert = new SqlCommand(sqlStr.ToString()) { Connection = connection };
                    try
                    {
                        cmdInsert.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("StoreQuotesByDeleteInsert() - error inserting rows", ex);
                        break;
                    }
                }
            }
        }


        public void StoreQuotesPack(List<CandleData> candles, 
            int startIndex, int endIndex,
            int ticker, bool rewriteQuotes, SqlConnection connection)
        {
            if (candles.Count == 0) return;
            int pointCost;
            if (!pointCostByTicker.TryGetValue(ticker, out pointCost)) return;
            
            // удалить maxCandlesInPack записей таблице на временном интервале, который
            // займут новые свечки
            if (rewriteQuotes)
            {
                var cmdDeleteSql = string.Format("delete from {0} where ticker=@Tick and date between " +
                                                     "@DateStart and @DateEnd", TableNameQuote);
                var cmdDelete = new SqlCommand(cmdDeleteSql) {Connection = connection};


                var dateStart = candles[startIndex].timeOpen;
                var dateEnd = candles[endIndex].timeOpen;
                cmdDelete.Parameters.Clear();
                cmdDelete.Parameters.AddWithValue("@Tick", ticker);
                cmdDelete.Parameters.AddWithValue("@DateStart", dateStart);
                cmdDelete.Parameters.AddWithValue("@DateEnd", dateEnd);
                try
                {
                    cmdDelete.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Logger.Error("StoreQuotesByDeleteInsert() - error deleting rows", ex);
                    return;
                }
            }    
        
            // сформировать запрос на добавление
            var sqlStr = new StringBuilder();
            for (var j = startIndex; j <= endIndex; j++)
            {
                sqlStr.AppendLine(string.Format("insert into {0} (ticker, date, [open], HLC) values (" +
                                        "{1}, '{2:yyyyMMdd HH:mm}', {3}, {4})",
                                        TableNameQuote, ticker, candles[j].timeOpen,
                                        candles[j].open.ToStringUniformPriceFormat(true),
                                        candles[j].GetHlcOffset16(pointCost)));
            }
            // ... и исполнить его
            var cmdInsert = new SqlCommand(sqlStr.ToString()) { Connection = connection };
            try
            {
                cmdInsert.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.Error("StoreQuotesByDeleteInsert() - error inserting rows", ex);
            }
        }

        /// <summary>
        /// создает временную таблицу, заполянет ее котировками (bulkcopy) и MERGE-ит ее
        /// на таблицу QUOTE
        /// </summary>        
        public bool StoreQuotesByMerge(List<CandleData> candles, int ticker)
        {
            int pointCost;
            if (!pointCostByTicker.TryGetValue(ticker, out pointCost)) return false;

            // SQL - создать или очистить временную таблицу
            const string cmdTempTable =
                "if not exists (SELECT * FROM tempdb.sys.tables WHERE [name] like '" +
                TableNameQuoteTemp + "%') begin" +
                " create table #QUOTE_TEMP" +
                "( date smalldatetime not null, ticker smallint not null, [open] float not null, HLC int not null) end " +
                "else begin delete from " + TableNameQuoteTemp + " end";
            var cmdMakeTemp = new SqlCommand(cmdTempTable);

            const string cmdMergeSql = "merge into " + TableNameQuote + " as Target " +
            "using " + TableNameQuoteTemp + " as Source " +
            "on Target.date = Source.date and Target.ticker = Source.ticker " +
            "when matched then " +
            "update set Target.[open] = Source.[open], Target.HLC = Source.HLC " +
            "when not matched then " +
            "insert (date, ticker, [open], HLC) values (Source.date, Source.ticker, Source.[open], Source.HLC);";
            var cmdMerge = new SqlCommand(cmdMergeSql);

            // подготовить DataTable на основании параметра candles
            var table = new DataTable();
            table.Columns.Add(new DataColumn("date", typeof(DateTime)));
            table.Columns.Add(new DataColumn("ticker", typeof(short)));
            table.Columns.Add(new DataColumn("open", typeof(float)));
            table.Columns.Add(new DataColumn("HLC", typeof(int)));
            foreach (var candle in candles)
            {
                table.Rows.Add(new object[] { candle.timeOpen, ticker, candle.open, candle.GetHlcOffset16(pointCost)});
            }

            using (var connection = new SqlConnection(connectionString))
            {
                cmdMakeTemp.Connection = connection;
                cmdMerge.Connection = connection;
                connection.Open();

                // таки создать временную таблицу
                try
                {
                    cmdMakeTemp.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Logger.Error("StoreQuotesByMerge() - error create temp table", ex);
                    return false;
                }

                using (new TimeLogger("insert into temp table"))
                {
                    try
                    {
                        using (var cpy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, null)
                                      {
                                          DestinationTableName = TableNameQuoteTemp
                                      })
                            cpy.WriteToServer(table);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("StoreQuotesByMerge() - error in bulk copy", ex);
                    }                    
                }

                using (new TimeLogger("merge"))
                // выполнить MERGE на основную таблицу котировок                
                try
                {
                    cmdMerge.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Logger.Error("StoreQuotesByMerge() - error while merging", ex);
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region Закрытые методы
        private static bool ExecuteSqlNonQuerySafe(string sql, SqlConnection conn)
        {
            try
            {
                using (var cmd = new SqlCommand(sql) { Connection = conn })
                {
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Error in ExecuteSqlNonQuerySafe()", ex);
                return false;
            }
        }

        private string MakeInsertCommand(CandleData candle, int ticker)
        {
            int pointCost;
            if (!pointCostByTicker.TryGetValue(ticker, out pointCost))
                return string.Empty;

            return string.Format("exec {0} @ticker={1}, @date='{2:yyyyMMdd HH:mm}', " +
                                 "@open={3}, @HLC={4}",
                                 ProcUpsertQuoteRecord,
                                 ticker, candle.timeOpen, candle.open.ToStringUniform(), candle.GetHlcOffset16(pointCost));
            //return string.Format("if exists (select * from QUOTE where ticker={0} and date='{1:yyyyMMdd HH:mm}') begin " +
            //                     " update QUOTE set [open]={2}, HLC={3} end else begin " +
            //                     "insert into QUOTE (ticker, date, [open], HLC) values (" +
            //                     "{0}, '{1:yyyyMMdd HH:mm}', {2}, {3}) end",
            //                     ticker,
            //                     candle.timeOpen, candle.open.ToStringUniform(), candle.GetHlcOffset(pointCost));
        }

        private static SqlCommand BuildSelectTimesCommand(SqlConnection connection,
            int codeFxi)
        {
            var command = new SqlCommand("SELECT min(date), max(date) FROM QUOTE WHERE ticker = @currency", connection);
            command.Parameters.Add(new SqlParameter("@currency", codeFxi));            
            return command;
        }
        #endregion
    }
}

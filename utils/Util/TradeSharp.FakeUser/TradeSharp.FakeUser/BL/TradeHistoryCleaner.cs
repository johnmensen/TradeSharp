using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using TradeSharp.Linq;

namespace TradeSharp.FakeUser.BL
{
    static class TradeHistoryCleaner
    {
        public static int ClearAllRecordsByAccounts(int[] accountIds)
        {
            var inClause = string.Join(",", accountIds);
            var connStr = ConfigurationManager.ConnectionStrings["ClearSharpConnection"];
            var recordsDeleted = accountIds.Length;

            using (var conn = new SqlConnection(connStr.ConnectionString))
            {
                conn.Open();

                var userIds = new List<int>();
                using (var cmd = new SqlCommand("select PlatformUser from PLATFORM_USER_ACCOUNT where Account in (" +
                                                inClause + ")") { Connection = conn })
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        userIds.Add((int) reader[0]);
                    }
                }
                recordsDeleted += userIds.Count;

                using (var cmd = new SqlCommand("delete from POSITION where AccountID in (" +
                                                inClause + ")") {Connection = conn})
                {
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new SqlCommand("delete from POSITION_CLOSED where AccountID in (" +
                                                inClause + ")") { Connection = conn })
                {
                    cmd.ExecuteNonQuery();
                }

                var userInClause = string.Join(",", userIds);
                using (var cmd = new SqlCommand("delete from TRANSFER where [User] in (" +
                                                userInClause + ")") { Connection = conn })
                {
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new SqlCommand("delete from TRANSFER where exists " + 
                        "(select ID from BALANCE_CHANGE where ID = TRANSFER.BalanceChange and AccountID in (" +
                                                inClause + "))") { Connection = conn })
                {
                    cmd.ExecuteNonQuery();
                }  

                using (var cmd = new SqlCommand("delete from BALANCE_CHANGE where AccountID in (" +
                                                inClause + ")") { Connection = conn })
                {
                    cmd.ExecuteNonQuery();
                }                              
            }

            return recordsDeleted;
        }

        public static int ClearAllRecordsByAccountsEF(int[] accountIds)
        {
            var totalDeleted = 0;
            using (var conn = DatabaseContext.Instance.Make())
            {
                foreach (var acId in accountIds)
                {
                    foreach (var pos in conn.POSITION.Where(p => p.AccountID == acId).ToList())
                    {
                        conn.POSITION.Remove(pos);
                        totalDeleted++;
                    }
                    foreach (var pos in conn.POSITION_CLOSED.Where(p => p.AccountID == acId).ToList())
                    {
                        conn.POSITION_CLOSED.Remove(pos);
                        totalDeleted++;
                    }
                    foreach (var bc in conn.BALANCE_CHANGE.Where(p => p.AccountID == acId).ToList())
                    {
                        conn.BALANCE_CHANGE.Remove(bc);
                        totalDeleted++;
                    }
                    var userAccount = conn.PLATFORM_USER_ACCOUNT.FirstOrDefault(a => a.Account == acId);
                    if (userAccount != null)
                        foreach (var trans in conn.TRANSFER.Where(p => p.User == userAccount.PlatformUser).ToList())
                        {
                            conn.TRANSFER.Remove(trans);
                            totalDeleted++;
                        } 
                }                
            }
            return totalDeleted;
        }
    }
}

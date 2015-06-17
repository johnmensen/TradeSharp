using System;
using TradeSharp.Util;

namespace TradeSharp.Contract.Entity
{
    /// <summary>
    /// передается от клиента к сервису в каждой операции,
    /// требующей авторизации
    /// </summary>
    [Serializable]
    public class ProtectedOperationContext
    {
        public long terminalId;
        public long clientLocalTime;
        public int sessionTag;
        public string hash;
        public string userMachineName;

        public static ProtectedOperationContext MakeServerSideContext()
        {
            return new ProtectedOperationContext
                       {
                           userMachineName = "server#",
                           terminalId = long.MaxValue
                       };
        }

        public static bool IsServerContext(ProtectedOperationContext ctx)
        {
            return ctx.userMachineName == "server#" && ctx.terminalId == long.MaxValue;
        }

        public static string GetUserHash(string login, string pwrd, long magic)
        {
            return CredentialsHash.MakeCredentialsHash(login, pwrd, magic);
        }

        public override string ToString()
        {
            return string.Format("terminal: {0}, time: {1}, session: {2}, hash: {3}, machine: {4}",
                terminalId, clientLocalTime, sessionTag, userMachineName, userMachineName);
        }
    }
}

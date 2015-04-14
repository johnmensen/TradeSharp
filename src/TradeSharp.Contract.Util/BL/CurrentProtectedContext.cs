using System;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Contract.Util.BL
{
    /// <summary>
    /// класс хранит Id терминала и номер сессии
    /// номер обновляется после успешного вызова Authenticate
    /// </summary>
    public class CurrentProtectedContext
    {
        private static CurrentProtectedContext instance;
        public static CurrentProtectedContext Instance
        {
            get { return instance ?? (instance = new CurrentProtectedContext()); }
        }

        private long terminalId;
        private volatile int sessionTag;

        private static string shortMachineName;
        
        public CurrentProtectedContext()
        {
            try
            {
                shortMachineName = Environment.MachineName;
            }
            catch (Exception ex)
            {
                Logger.Error("TerminalEnvironment - get ShortMachineName error", ex);
            }
            if (string.IsNullOrEmpty(shortMachineName))
                shortMachineName = "unspec";
            else
                if (shortMachineName.Length > 15) shortMachineName = shortMachineName.Substring(15);
        }

        public void Initialize(long terminalId)
        {
            this.terminalId = terminalId;
        }

        public void OnAuthenticated(int sessionTag)
        {
            this.sessionTag = sessionTag;
        }

        public void OnAuthenticateFaulted()
        {
            sessionTag = 0;
        }

        public ProtectedOperationContext MakeProtectedContext()
        {
            if (sessionTag == 0) return null;
            var context = new ProtectedOperationContext
                              {
                                  sessionTag = sessionTag,
                                  terminalId = terminalId,
                                  clientLocalTime = DateTime.Now.Ticks,
                                  userMachineName = shortMachineName
                              };
            context.hash = CredentialsHash.MakeOperationParamsHash(context.clientLocalTime, sessionTag, terminalId);
            return context;
        }
    }
}

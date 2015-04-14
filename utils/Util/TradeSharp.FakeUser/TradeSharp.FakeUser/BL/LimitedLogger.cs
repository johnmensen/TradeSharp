using System.Collections.Generic;
using TradeSharp.Util;

namespace TradeSharp.FakeUser.BL
{
    class LimitedLogger
    {
        private readonly Dictionary<string, int> countByCat = new Dictionary<string, int>();

        public void Log(LogEntryType logType,
            string preffix, int counter, string message, params object[] ptrs)
        {
            int countLeft;
            if (countByCat.TryGetValue(preffix, out countLeft))
            {
                if (countLeft <= 0) return;
                countByCat[preffix] = countByCat[preffix] - 1;
            }
            else
                countByCat.Add(preffix, counter);

            Logger.Log(logType, message, ptrs);
        }
    }
}

using System;

namespace TradeSharp.UpdateContract.Entity
{
    public enum SystemName
    {
        Terminal = 0,
        AdminApp = 1,
        ManagerApp = 2
    }

    public static class SystemNameParser
    {
        public static SystemName? ParseSystemName(string nameStr)
        {
            try
            {
                return (SystemName) Enum.Parse(typeof(SystemName), nameStr, true);
            }
            catch
            {
                return null;
            }
        }
    }
}

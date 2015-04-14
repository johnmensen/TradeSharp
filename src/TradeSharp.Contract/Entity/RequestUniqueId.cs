using System;

namespace TradeSharp.Contract.Entity
{
    public static class RequestUniqueId
    {
        private static readonly Random rnd = new Random();

        public static int Next()
        {
            return rnd.Next();
        }
    }
}

using System;
using System.Diagnostics;

namespace TradeSharp.Util
{
    public static class SharpCodeContract
    {
        public static void Requires<TException>(bool predicate) where TException : Exception, new()
        {
            Requires<TException>(predicate, "");
        }

        public static void Requires<TException>(bool predicate, string message) where TException : Exception, new()
        {
            if (!predicate)
            {
                Debug.WriteLine(message);

                TException exp;

                var ctor = typeof (TException).GetConstructor(new [] {typeof (string)});
                if (ctor != null)
                    exp = (TException)ctor.Invoke(new object[] {message});
                else
                    exp = new TException();
                throw exp;
            }
        }
    }
}

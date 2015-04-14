using System;

namespace TradeSharp.Util
{
    public interface IParsable
    {
    }

    public static class ObjectParser
    {
        public static T ParseObject<T>(string ptrsA)
        {
            return Converter.GetObjectFromString<T>(ptrsA);
        }
    }

    /// <summary>
    /// Структура из двух произвольных полей
    /// </summary>    
    [Serializable]
    public struct Cortege2<A, B> : IParsable
    {
        public A a { get; set; }
        public B b { get; set; }
        public static Cortege2<A, B> Parse(params string[] vals)
        {
            return new Cortege2<A, B>
            {
                a = ObjectParser.ParseObject<A>(vals[0]),
                b = ObjectParser.ParseObject<B>(vals[1])
            };
        }
        public Cortege2(A _a, B _b)
            : this()
        {
            a = _a;
            b = _b;
        }
        public bool IsDefault()
        {
            return Equals(default(Cortege2<A, B>));
        }
        public override string ToString()
        {
            return string.Format("{0};{1}", a, b);
        }
    }

    [Serializable]
    public struct Cortege3<A, B, C> : IParsable
    {
        public A a { get; set; }
        public B b { get; set; }
        public C c { get; set; }
        public Cortege3(A _a, B _b, C _c)
            : this()
        {
            a = _a;
            b = _b;
            c = _c;
        }
        public static Cortege3<A, B, C> Parse(params string[] vals)
        {
            return new Cortege3<A, B, C>
            {
                a = ObjectParser.ParseObject<A>(vals[0]),
                b = ObjectParser.ParseObject<B>(vals[1]),
                c = ObjectParser.ParseObject<C>(vals[2])
            };
        }
        public override string ToString()
        {
            return string.Format("{0};{1};{2}", a, b, c);
        }
    }    
}

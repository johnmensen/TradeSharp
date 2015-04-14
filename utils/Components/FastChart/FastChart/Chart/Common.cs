using System;

namespace FastChart.Chart
{
    [Serializable]
    public struct Cortege2<A, B>
    {
        public A a { get; set; }
        public B b { get; set; }
        public Cortege2 (A _a, B _b) : this()
        {
            a = _a;
            b = _b;
        }
        public bool IsDefault()
        {
            return Equals(default(Cortege2<A, B>));
        }
    }
}

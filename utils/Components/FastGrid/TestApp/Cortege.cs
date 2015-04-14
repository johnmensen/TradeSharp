namespace Util
{
    public struct Cortege2<A, B>
    {
        public A a { get; set; }
        public B b { get; set; }
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

    public struct Cortege3<A, B, C>
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
        public override string ToString()
        {
            return string.Format("{0};{1};{2}", a, b, c);
        }
    }    
}

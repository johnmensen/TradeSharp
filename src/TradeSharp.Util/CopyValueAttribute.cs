using System;
using System.Linq;

namespace TradeSharp.Util
{
    public class CopyValueAttribute : Attribute
    {
        public bool DeepCopy { get; set; }

        public CopyValueAttribute()
        {            
        }

        public CopyValueAttribute(bool deepCopy)
        {
            DeepCopy = deepCopy;
        }

        public static Func<object, object> MakeCopyValuesRoutine(Type t)
        {
            var properties =
                t.GetProperties().Where(p => p.GetCustomAttributes(typeof (CopyValueAttribute), true).Length == 1);
            var ctor = t.GetConstructor(new Type[0]);
            return o =>
                {
                    var c = ctor.Invoke(new object[0]);
                    foreach (var propertyInfo in properties)
                    {
                        var val = propertyInfo.GetValue(o, new object[0]);
                        propertyInfo.SetValue(c, val, new object[0]);
                    }
                    return c;
                };
        }
    }
}

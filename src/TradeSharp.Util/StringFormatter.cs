using System;
using System.Drawing;

namespace TradeSharp.Util
{
    public static class StringFormatter
    {
        public static string ObjectToString(object val)
        {
            if (val == null) 
                return string.Empty;
            var valType = val.GetType();
            if (valType == typeof(string)) return (string)val;
            if (valType == typeof(bool) || valType == typeof(bool) ||
                valType == typeof(bool) || valType.IsSubclassOf(typeof(Enum)))
                    return val.ToString();
            if (valType == typeof(decimal)) return ((decimal)val).ToStringUniform();
            if (valType == typeof(float)) return ((float)val).ToStringUniform();
            if (valType == typeof(double)) return ((double)val).ToStringUniform();
            if (valType == typeof(DateTime)) return ((DateTime)val).ToStringUniform();
            if (valType == typeof(Point)) return string.Format("{0};{1}", ((Point)val).X, ((Point)val).Y);
            if (valType == typeof(Size)) return string.Format("{0};{1}", ((Size)val).Width, ((Size)val).Height);
            if (valType == typeof(Color)) return ((Color)val).ToArgb().ToString();
            return val.ToString();
        }

        public static object StringToObject(string str, Type objType)
        {
            if (string.IsNullOrEmpty(str)) return null;
            if (objType == typeof(string)) return str;
            if (objType == typeof(bool)) return str.ToBool();
            if (objType == typeof(int)) return str.ToInt();
            if (objType == typeof(long)) return long.Parse(str);
            if (objType == typeof(decimal)) return str.ToDecimalUniform();
            if (objType == typeof(float)) return str.ToFloatUniform();
            if (objType == typeof(double)) return str.ToDoubleUniform();
            if (objType == typeof(DateTime)) return str.ToDateTimeUniform();
            if (objType == typeof(Point))
            {
                var parts = str.Split(';');
                return new Point(parts[0].ToInt(), parts[1].ToInt());
            }
            if (objType == typeof(Size))
            {
                var parts = str.Split(';');
                return new Size(parts[0].ToInt(), parts[1].ToInt());
            }
            if (objType == typeof(Color)) return Color.FromArgb(str.ToInt());
            if (objType.IsSubclassOf(typeof(Enum)))
            {
                return Enum.Parse(objType, str);
            }
            return null;
        }

        public static string ObjectToStringWithType(object val)
        {
            var valType = val.GetType();
            if (valType == typeof(string)) return "str:" + (string)val;
            if (valType == typeof(bool)) return "bol:" + val;
            if (valType == typeof(int)) return "int:" + val;
            if (valType == typeof(long)) return "lng:" + val;
            if (valType == typeof(decimal)) return "dec:" + ((decimal) val).ToStringUniform();
            if (valType == typeof(float)) return "flo:" + ((float) val).ToStringUniform();
            if (valType == typeof(double)) return "dob:" + ((double) val).ToStringUniform();
            if (valType == typeof(DateTime)) return "dat:" + ((DateTime) val).ToStringUniform();
            if (valType == typeof(Point)) return "pnt:" + string.Format("{0};{1}", ((Point)val).X, ((Point)val).Y);
            if (valType == typeof(Size)) return "siz:" + string.Format("{0};{1}", ((Size)val).Width, ((Size)val).Height);
            if (valType == typeof(Color)) return "col:" + ((Color)val).ToArgb();
            if (valType.IsSubclassOf(typeof(Enum))) return "enm:" + valType.FullName + ";" + val;
            return "unk:";
        }

        public static object TypedStringToObject(string str)
        {
            if (string.IsNullOrEmpty(str)) return null;
            if (str.Length < 5) return null;
            var tag = str.Substring(0, 3);
            str = str.Substring(4);
            if (tag == "str") return str;
            if (tag == "bol") return str.ToBool();
            if (tag == "int") return str.ToInt();
            if (tag == "lng") return long.Parse(str);
            if (tag == "dec") return str.ToDecimalUniform();
            if (tag == "flo") return str.ToFloatUniform();
            if (tag == "dob") return str.ToDoubleUniform();
            if (tag == "dat") return str.ToDateTimeUniform();
            if (tag == "pnt")
            {
                var parts = str.Split(';');
                return new Point(parts[0].ToInt(), parts[1].ToInt());
            }
            if (tag == "siz")
            {
                var parts = str.Split(';');
                return new Size(parts[0].ToInt(), parts[1].ToInt());
            }
            if (tag == "col") return Color.FromArgb(str.ToInt());
            if (tag == "enm")
            {
                var parts = str.Split(';');
                return Enum.Parse(Type.GetType(parts[0]), parts[1]);
            }
            return null;
        }
    }
}

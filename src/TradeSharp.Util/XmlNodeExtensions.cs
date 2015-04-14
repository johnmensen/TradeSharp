using System.Drawing;
using System.Xml;

namespace TradeSharp.Util
{
    public static class XmlNodeExtensions
    {
        public static string GetAttributeString(this XmlNode node, string attrName, string defaultValue = "")
        {
            if (node.Attributes[attrName] == null) return defaultValue;
            return node.Attributes[attrName].Value;
        }

        public static bool? GetAttributeBool(this XmlNode node, string attrName)
        {
            if (node.Attributes[attrName] == null) return null;
            var valStr = node.Attributes[attrName].Value;
            return string.IsNullOrEmpty(valStr) ? null : valStr.ToBoolSafe();
        }

        public static int? GetAttributeInt(this XmlNode node, string attrName)
        {
            if (node.Attributes[attrName] == null) return null;
            var valStr = node.Attributes[attrName].Value;
            return string.IsNullOrEmpty(valStr) ? null : valStr.ToIntSafe();
        }

        public static float? GetAttributeFloat(this XmlNode node, string attrName)
        {
            if (node.Attributes[attrName] == null) return null;
            var valStr = node.Attributes[attrName].Value;
            return string.IsNullOrEmpty(valStr) ? null : valStr.ToFloatUniformSafe();
        }

        public static double? GetAttributeDouble(this XmlNode node, string attrName)
        {
            if (node.Attributes[attrName] == null) return null;
            var valStr = node.Attributes[attrName].Value;
            return string.IsNullOrEmpty(valStr) ? null : valStr.ToDoubleUniformSafe();
        }

        public static Size? GetAttributeSize(this XmlNode node, string attrName, Size? defaultValue = null)
        {
            if (node.Attributes[attrName] == null) return defaultValue;
            var val = node.Attributes[attrName].Value;
            if (string.IsNullOrEmpty(val)) return defaultValue;
            return val.ToSizeSafe();
        }

        public static SizeF? GetAttributeSizeF(this XmlNode node, string attrName, SizeF? defaultValue = null)
        {
            if (node.Attributes[attrName] == null) return defaultValue;
            var val = node.Attributes[attrName].Value;
            if (string.IsNullOrEmpty(val)) return defaultValue;
            return val.ToSizeFSafe();
        }
    }
}

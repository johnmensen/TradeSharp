using System;
using System.Xml;

namespace Candlechart.Core
{
    /// <summary>
    /// если класс сериализуется, например, кодом MakeIndicatorXMLNode()
    /// поля, помеченные этим атрибутом, будут сохранены в узел XML
    /// вызовом перегруженного метода 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class CustomXmlSerializationAttribute : Attribute
    {
        public abstract void SerializeProperty(object proValue, XmlNode parent);
        public abstract object DeserializeProperty(XmlNode parent);
    }
}

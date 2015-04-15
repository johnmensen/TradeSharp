using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Xml;
using Entity;
using TradeSharp.Util;
using System.Linq;

namespace TradeSharp.Reports.Lib.IndexGrabber
{
    public abstract class IndexDataGrabber
    {
        public string IndexName { get; set; }

        public abstract List<Cortege2<DateTime, decimal>> GrabIndexData();

        protected string QueryPage(string queryString)
        {
            var req = WebRequest.Create(queryString);            
            WebResponse responce;
            try
            {
                responce = req.GetResponse();
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка запроса к URL({0}): {1}", req.RequestUri, ex);
                return string.Empty;
            }

            if (responce == null)
            {
                Logger.ErrorFormat("Нет ответа от {0}", queryString);
                return string.Empty;
            }
            var stream = responce.GetResponseStream();
            if (stream == null)
            {
                Logger.ErrorFormat("Нет ответа от {0}", queryString);
                return string.Empty;
            }
            var sr = new StreamReader(stream);
            return sr.ReadToEnd();
        }
    
        public static List<IndexDataGrabber> ReadGrabbers(string fileName)
        {
            // получить всех наследников IndexDataGrabber
            var asm = Assembly.GetEntryAssembly();
            var grabberTypes = asm.GetTypes().Where(t => t.IsSubclassOf(typeof (IndexDataGrabber))).ToList();
            if (grabberTypes.Count == 0) return new List<IndexDataGrabber>();
            
            // заполнить словарь - константа - конструктор        
            var dicCtorByTagName = new Dictionary<string, ConstructorInfo>();
            foreach (var typ in grabberTypes)
            {
                var fieldTypeName = typ.GetField("GrabberTag");
                dicCtorByTagName.Add((string) fieldTypeName.GetRawConstantValue(),
                                     typ.GetConstructor(new[] {typeof (XmlElement)}));
            }

            // открыть документ и прочитать все дочерние узлы
            var doc = new XmlDocument();
            try
            {
                doc.Load(fileName);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка чтения документа парсеров новостей \"{0}\": {1}", fileName, ex);
                return new List<IndexDataGrabber>();
            }
            if (doc.DocumentElement == null) return new List<IndexDataGrabber>();

            var grabbers = new List<IndexDataGrabber>();
            foreach (XmlElement child in doc.DocumentElement.ChildNodes)
            {
                ConstructorInfo ctor;
                dicCtorByTagName.TryGetValue(child.Name, out ctor);
                if (ctor == null)
                {
                    Logger.ErrorFormat("Настройки парсеров: \"{0}\" не найден в словаре", child.Name);
                    continue;
                }
                var grabber = (IndexDataGrabber)ctor.Invoke(new object[] { child });
                grabber.IndexName = child.Attributes["IndexName"].Value;
                grabbers.Add(grabber);
            }
            return grabbers;
        }
    }
}

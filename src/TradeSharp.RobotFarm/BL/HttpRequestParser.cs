using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace TradeSharp.RobotFarm.BL
{
    // ReSharper disable StringIndexOfIsCultureSpecific.1
    // ReSharper disable StringIndexOfIsCultureSpecific.2
    public class HttpRequestParser
    {
        public class ContentRecord
        {
            public string Name { get; set; }

            /// <summary>
            /// Тип передаваемого файла
            /// </summary>
            public string AttachedFileType { get; set; }

            /// <summary>
            /// Имя передаваемого файла
            /// </summary>
            public string FileName { get; set; }

            /// <summary>
            /// Содержание передаваемого файла
            /// </summary>
            public string FileData { get; set; }

            public XmlDocument GetXml()
            {
                var clearStr = RemoveUtf8ByteOrderMark(FileData).Trim('\r', '\n');
                if (string.IsNullOrEmpty(clearStr)) return null;
                var doc = new XmlDocument();
                doc.LoadXml(clearStr);
                return doc;
            }

            /// <summary>
            /// вспомогательный метод, очистки строки с содержанием xml файла от битов типа 0x00, для корректного сохранения.
            /// </summary>
            private static string RemoveUtf8ByteOrderMark(string xml)
            {
                var byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
                if (xml.StartsWith(byteOrderMarkUtf8))
                {
                    xml = xml.Remove(0, byteOrderMarkUtf8.Length);
                }
                return xml;
            }

            public override string ToString()
            {
                return FileData;
            }
        }

        private const string Boundary = "--";
        private const string NewLineHeader = "\r\n";
        
        public List<ContentRecord> records = new List<ContentRecord>();

        public HttpRequestParser(string sourseString)
        {
            Parse(sourseString);
        }     

        /// <summary>
        /// Парсим текст запроса и получаем имя файла, его расширение и содержание
        /// </summary>
        private void Parse(string sourceString)
        {
            if (string.IsNullOrEmpty(sourceString)) return;

            // найти строку - разделитель вида ------WebKitFormBoundaryrPWDJjvroexatXyd
            var boundLine = FindBoundLine(sourceString);
            if (string.IsNullOrEmpty(boundLine)) return;

            // считать все секции
            var pos = 0;
            while (true)
            {
                pos = ParseSection(pos, sourceString, boundLine);
                if (pos < 0) break;
            }
        }

        private string FindBoundLine(string sourceString)
        {
            var boundIndex = sourceString.IndexOf(Boundary);
            if (boundIndex < 0) return string.Empty;
            var boundEndIndex = sourceString.IndexOf("\r", boundIndex);
            if (boundEndIndex < 0) return string.Empty;
            return sourceString.Substring(boundIndex, boundEndIndex - boundIndex);            
        }

        private int ParseSection(int pos, string sourceString, string boundLine)
        {
            var boundIndex = sourceString.IndexOf(boundLine, pos);
            if (boundIndex < 0) return -1;

            var descrIndex = sourceString.IndexOf("Content-Disposition: form-data;", boundIndex);
            if (descrIndex < 0) return -1;

            var ctxHeaderEnd = sourceString.IndexOf(NewLineHeader, descrIndex);
            if (ctxHeaderEnd < 0) return -1;

            var headerStr = sourceString.Substring(descrIndex + "Content-Disposition: form-data;".Length,
                                                   ctxHeaderEnd - descrIndex - "Content-Disposition: form-data;".Length);
            
            // строка вида Content-Disposition: form-data; name="uploadedFile"; filename="settings_news.xml"
            // или Content-Disposition: form-data; name="accountId"
            var dicPtrVal = new Dictionary<string, string>();
            var attrParts = headerStr.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var attrPart in attrParts)
            {
                var nameValParts = attrPart.Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries);
                if (nameValParts.Length != 2) continue;

                dicPtrVal.Add(nameValParts[0].Trim(), nameValParts[1].Trim(' ', '"'));
            }

            // имеем словарь вида { name, uploadedFile }, { filename, settings_news.xml }
            string fieldName, fileName;
            if (!dicPtrVal.TryGetValue("name", out fieldName)) return ctxHeaderEnd;
            if (!dicPtrVal.TryGetValue("filename", out fileName)) fileName = string.Empty;

            // след строка мб пустая либо содержит Content-Type: text/xml
            var ctxTypeStart = ctxHeaderEnd + NewLineHeader.Length;

            // индекс первого символа данных
            var bodyStart = ctxTypeStart + NewLineHeader.Length;
            
            // распарсить тип данных, если он указан
            var fileType = string.Empty;
            if (sourceString.Substring(ctxTypeStart, "Content-Type: ".Length) ==
                "Content-Type: ")
            {
                var lineEndIndex = sourceString.IndexOf(NewLineHeader, ctxTypeStart);
                if (lineEndIndex < 0) return ctxHeaderEnd;
                var strCtxType = sourceString.Substring(ctxTypeStart + "Content-Type: ".Length,
                                                        lineEndIndex - ctxTypeStart - "Content-Type: ".Length);
                fileType = strCtxType;
                bodyStart = lineEndIndex + NewLineHeader.Length;
            }

            // получить тельце
            var bodyEnd = sourceString.IndexOf(NewLineHeader + boundLine, bodyStart);
            if (bodyEnd < 0) return -1;
            var strBody = sourceString.Substring(bodyStart, bodyEnd - bodyStart);

            // создать запись
            var rec = new ContentRecord
                {
                    AttachedFileType = fileType,
                    FileName = fileName,
                    Name = fieldName,
                    FileData = strBody
                };
            records.Add(rec);
            return bodyEnd;
        }
    }
    // ReSharper restore StringIndexOfIsCultureSpecific.2
    // ReSharper restore StringIndexOfIsCultureSpecific.1
}
using System.IO;
using System.Text;

namespace TradeSharp.RobotFarm.BL
{
    public class HttpMultipartParser
    {
        public string fileName;

        public HttpMultipartParser(Stream stream, Encoding encoding)
        {
            using (var sr = new StreamReader(stream, encoding))
            {
                var boundarySmb = string.Empty;
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line)) continue;

                    if (!string.IsNullOrEmpty(boundarySmb))
                    {
                        if (line == boundarySmb)
                        {
                            var lineCtxTitle = sr.ReadLine();
                            var lineCtxType = sr.ReadLine();
                            GetFileNameFromFormTitle(lineCtxTitle);
                        }
                    }

                    if (!line.StartsWith("boundary=")) continue;
                    boundarySmb = line.Substring("boundary=".Length);


                }
                if (string.IsNullOrEmpty(boundarySmb)) return;
            }
        }

        private void GetFileNameFromFormTitle(string title)
        {
            // name="filename";
            if (string.IsNullOrEmpty(title)) return;

            var start = title.IndexOf("name=\"");
            if (start < 0) return;
            var end = title.IndexOf("\"", start + "name=\"".Length);
            if (end < 0) return;
            fileName = title.Substring(start + "name=\"".Length, end - (start + "name=\"".Length) + 1);
        }
    }
}

using System.IO;
using System.Text;
using PDFLibNet;

namespace TradeSharp.NewsGrabber.Grabber.CME
{
    public class PdfConverter
    {
        public static void PdfFileToText(string srcFileName, string dstFileName)
        {
            var sb = new StringBuilder();
            using (var doc = new PDFWrapper())
            {
                doc.LoadPDF(srcFileName);

                PDFPage pg;
                for (int j = 1; j <= doc.PageCount; j++)
                {
                    pg = doc.Pages[j];
                    sb.Append(pg.Text);
                }
            }

            using (var sw = new StreamWriter(dstFileName, false, Encoding.Unicode))
            {
                sw.Write(sb.ToString());
            }
        }
    }
}

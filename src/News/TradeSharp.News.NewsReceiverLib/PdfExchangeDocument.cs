using System;
using System.Linq;
using System.IO;
using System.Text;
using PDFLibNet;


namespace TradeSharp.News.NewsReceiverLib
{
    public class PdfExchangeDocument : BaseExchangeDocument
    {
        public delegate void ParsePdfFinished();
        public ParsePdfFinished PdfParsed;

        /// <summary>
        /// запоминаем текущий инструмент
        /// </summary>
        PDFWrapper _pdfDoc = null;

        public StringBuilder ResText = new StringBuilder();
        /// <summary>
        /// запоминаем страницу для рендеринга
        /// </summary>
        //PDFPage pg = null;
        //int currentpdf = 0;
        //int currentpage = 0;
        public override void ReceiveDoc()
        {

            var my_session = new ExchangeSession();
            my_session.FileDownloaded = ExtractTextFromPDFBytes;
            my_session.ExecuteOptionsSession();
            
        }

        public override void ParseDoc()
        {
            throw new NotImplementedException();
        }

        public override void UpdateDB()
        {
            throw new NotImplementedException();
        }

        #region ExtractTextFromPDFBytes
        /// <summary>
        /// This method processes an uncompressed Adobe (text) object 
        /// and extracts text.
        /// </summary>
        /// <param name="input">uncompressed</param>
        /// <returns></returns>
        private void ExtractTextFromPDFBytes(byte[] input)
        {
            var f = new FileInfo("c:\\MyTest.pdf");
            FileStream fs = f.Create();
            fs.Write(input, 0, input.Count());
            fs.Close();

            _pdfDoc.LoadPDF("c:\\MyTest.pdf");

            f.Delete();
            OutTexts();

            PdfParsed();
        }
        #endregion

        /// <summary>
        /// Вывод текста
        /// </summary>
        private void OutTexts()
        {
            if (_pdfDoc == null) 
                return;
            PDFPage pg;
            for (int j = 1; j <= _pdfDoc.PageCount; j++)
            {
                pg = _pdfDoc.Pages[j];
                //pg.Text текст одной страницы;
                ResText.Append(pg.Text);
            }
        }

    
    }
       
}

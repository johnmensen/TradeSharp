using System;
using System.Diagnostics;
using System.Security;
using TradeSharp.Util;

namespace TradeSharp.Reports.Lib
{
    /// <summary>
    /// на входе - well-formed XHTML
    /// может содержать картинки с прописанными абсолютными путями
    /// на выходе - pdf-файл
    /// запускает стороннее приложение
    /// </summary>
    public class PdfFromXHtml
    {
        private static PdfFromXHtml instance;
        public static PdfFromXHtml Instance
        {
            get { return instance ?? (instance = new PdfFromXHtml()); }
        }

        private readonly string pdfMakerPath;
        private readonly string exeStartUser, exeStartPassword, exeStartDomain;

        private PdfFromXHtml()
        {
            //D:\Program Files\wkhtmltopdf\
            pdfMakerPath = AppConfig.GetStringParam("PDF.MakerPath", string.Empty);
            if (string.IsNullOrEmpty(pdfMakerPath))
                throw new Exception("Путь к файлу wkhtmltopdf.exe не прописан в .config (PDF.MakerPath)");
            exeStartUser = AppConfig.GetStringParam("PDF.StartUser", string.Empty);
            exeStartPassword = AppConfig.GetStringParam("PDF.StartPassword", string.Empty);
            exeStartDomain = AppConfig.GetStringParam("PDF.StartDomain", string.Empty);
        }

        public void MakePdf(string pathSrc, string pathDest)
        {
            var exePtrs = string.Format("\"{0}\" \"{1}\"",
                                        pathSrc, pathDest);
            try
            {
                if (!string.IsNullOrEmpty(exeStartUser))
                {
                    var pwrdSec = new SecureString();
                    foreach (var c in exeStartPassword)
                        pwrdSec.AppendChar(c);
                    Process.Start(pdfMakerPath, exePtrs, exeStartUser, pwrdSec, exeStartDomain);
                }
                else Process.Start(pdfMakerPath, exePtrs);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка запуска мастера PDF (\"{0}\", \"{1}\"): {2}",
                    pathSrc, pathDest, ex);
            }
        }        
    }
}

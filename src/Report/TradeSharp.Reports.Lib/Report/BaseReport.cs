namespace TradeSharp.Reports.Lib.Report
{
    public abstract class BaseReport
    {
        public string TemplateFileName { get; set; }
        public abstract string MakePdf(int accountId,
            string templateFolder, string destFolder, string tempFolder);
    }
}

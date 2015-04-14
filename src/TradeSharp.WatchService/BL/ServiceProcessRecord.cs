namespace TradeSharp.WatchService.BL
{
    /// <summary>
    /// имя службы Windows, ее удобочитаемое название + имя запускаемого файла
    /// </summary>
    class ServiceProcessRecord
    {
        public string Name { get; set; }
        
        public string Title { get; set; }

        public string FileName { get; set; }
    }
}

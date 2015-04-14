namespace TradeSharp.SiteAdmin.Models
{
    public abstract class ListModel
    {
        /// <summary>
        /// Номер текущей страници
        /// </summary>
        public int PageNomber { get; set; }

        /// <summary>
        /// Текущий размер страници
        /// </summary>
        public int CurrentPageSize { get; set; }

        private readonly int[] pageSizeItems = new[] { 10, 15, 25, 50, 100 };

        /// <summary>
        /// возможные значения размеров страници
        /// </summary>
        public int[] PageSizeItems
        {
            get { return pageSizeItems; }
        }
    }
}
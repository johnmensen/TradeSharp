using System.Drawing;
using TradeSharp.Util;

namespace Candlechart.ChartIcon
{
    public class ChartIconDealTableRow : IChartIconDropDownRow
    {
        public int Id { get; set; }

        public int Side { get; set; }

        public int Volume { get; set; }

        public float Price { get; set; }

        public float Profit { get; set; }

        public string DepoCurrency { get; set; }

        public int ProfitPoints { get; set; }

        public ChartIconDropDownCell[] GetCells()
        {
            return new[]
                {
                    new ChartIconDropDownCell
                        {
                            CellValue = Side,
                            CellString = Side > 0 ? "BUY" : "SELL",
                            FontStyle = Id > 0 ? (FontStyle?)null : FontStyle.Bold
                        },
                    new ChartIconDropDownCell
                        {
                            CellValue = Volume,
                            CellString = Volume.ToStringUniformMoneyFormat(),
                            ColorFont = Profit >= 0 ? Color.DarkGreen : Color.DarkRed,
                            FontStyle = Id > 0 ? (FontStyle?)null : FontStyle.Bold
                        },
                    new ChartIconDropDownCell
                        {
                            CellValue = Price,
                            CellString = Price.ToStringUniformPriceFormat()
                        },
                    new ChartIconDropDownCell
                        {
                            CellValue = Profit,
                            CellString = Profit.ToStringUniformMoneyFormat() + " " + DepoCurrency,
                            ColorFont = Profit >= 0 ? Color.DarkGreen : Color.DarkRed
                        },
                    new ChartIconDropDownCell
                        {
                            CellValue = ProfitPoints,
                            CellString = ProfitPoints.ToStringUniformMoneyFormat() + " п"
                        }
                };
        }
    }
}

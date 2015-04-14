using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TradeSharp.Hub.BL.Model
{
    public class Currency
    {
        [Key]
        [MinLength(3)]
        [MaxLength(3)]
        [DisplayName("Валюта")]
        public string Code { get; set; }

        [Required]
        [Range(1, 100000)]
        [DisplayName("Цифровой код")]
        public int CurrencyIndex { get; set; }

        public override string ToString()
        {
            return Code;
        }
    }
}

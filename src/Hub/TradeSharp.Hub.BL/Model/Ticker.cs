using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TradeSharp.Hub.BL.Model
{
    public class Ticker
    {
        [Key]
        [StringLength(8, ErrorMessage = "Длина поля - Название - должна быть в пределах от 2 до 8 символов", MinimumLength = 2)]
        [DisplayName("Название")]
        public string Name { get; set; }

        [Required]
        [DisplayName("Базовая валюта")]
        public virtual Currency BaseCurrency { get; set; }
    }
}
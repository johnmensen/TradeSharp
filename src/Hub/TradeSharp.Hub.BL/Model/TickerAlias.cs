using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeSharp.Hub.BL.Model
{
    public class TickerAlias
    {
        [Key, Column(Order = 0), DisplayName("Сервер")]
        public string Server { get; set; }

        [Key, Column(Order = 1), DisplayName("Тикер")]
        public string Ticker { get; set; }

        [Required]
        [StringLength(12, ErrorMessage = "Длина поля - Название - должна быть в пределах от 2 до 8 символов", MinimumLength = 2)]
        [DisplayName("Псевдоним")]
        public string Alias { get; set; }
    }
}

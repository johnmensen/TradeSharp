using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TradeSharp.Hub.BL.Model
{
    /// <summary>
    /// описывает сервер Trade#, установленный, например,
    /// в SaxoBank
    /// </summary>
    public class ServerInstance
    {
        [Key]
        [MinLength(2)]
        [MaxLength(5)]
        [DisplayName("Код")]
        public string Code { get; set; }

        [Required]
        [MaxLength(20)]
        [DisplayName("Страна")]
        public string Country { get; set; }

        [Required]
        [MaxLength(64)]
        [DisplayName("Название")]
        public string Title { get; set; }

        [Required]
        [MinLength(7)]
        [MaxLength(15)]
        [DisplayName("Основной IP")]
        public string PrimaryIP { get; set; }

        [Required]
        [MinLength(7)]
        [MaxLength(15)]
        [DisplayName("Пароль")]
        public string Password { get; set; }

        [Required]
        [DisplayName("Разрешен")]
        public bool Allowed { get; set; }

        [Required]
        [DisplayName("Валюта расчета")]
        public virtual Currency Currency { get; set; }

        [Required]
        [DisplayName("Маркап трейдеров")]
        [Range(0.0, 1000.0)]
        public decimal MarkupPerMillionTrader { get; set; }

        [Required]
        [DisplayName("Маркап инвесторов")]
        [Range(0.0, 1000.0)]
        public decimal MarkupPerMillionInvestor { get; set; }

        [DisplayName("Среднее время по Гринвичу")]
        public short GMT { get; set; }
    }    
}

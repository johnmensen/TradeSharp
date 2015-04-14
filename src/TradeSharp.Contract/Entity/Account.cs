using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TradeSharp.Util;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class Account
    {
        public enum AccountStatus { Created = 0, Blocked = 1 }

        [Browsable(false)]
        [LocalizedDisplayName("TitleAccount")]
        public int ID { get; set; }

        [LocalizedDisplayName("TitleAccountGroup")]
        [Category("Основные")]
        [Description("Принадлежность счета к группе")]
        public string Group { get; set; }

        [LocalizedDisplayName("TitleBalance")]
        [Category("Основные")]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Значение должно быть числом большим 0.01")]
        public decimal Balance { get; set; }
        
        [Browsable(false)]
        public decimal Equity { get; set; } 
        
        [Browsable(false)]
        public decimal UsedMargin { get; set; }

        [LocalizedDisplayName("TitleCurrency")]
        [Category("Основные")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public string Currency { get; set; }
        /// <summary>
        /// макс. плечо, при котором разрешено открывать сделку,
        /// увеличивающую экспозицию
        /// </summary>
        [LocalizedDisplayName("TitleMaxLeverage")]
        [Category("Основные")]
        [Description("Максимальное разрешенное плечо для счета")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [Range(0, float.MaxValue, ErrorMessage = "Значение должно быть числом не меньшим 0")]
        public float MaxLeverage { get; set; }

        [LocalizedDisplayName("TitleAccountStatus")]
        [Category("Основные")]
        [Description("Создан либо заблокирован")]
        public AccountStatus Status { get; set; }

        [LocalizedDisplayName("TitleTimeCreated")]
        [Category("Основные")]
        [Description("Время создания счета")]
        public DateTime TimeCreated { get; set; }

        [LocalizedDisplayName("TitleTimeBlocked")]
        [Category("Основные")]
        [Description("Время блокирования счета")]
        public DateTime? TimeBlocked { get; set; }

        [LocalizedDisplayName("TitlePasswordView")]
        [Category("Основные")]
        [Description("Пароль для просмотра, торговля запрещена")]
        public string ReadonlyPassword { get; set; }

        public const int MinimumStartDepo = 1000;
        public const int MaximumStartDepo = 5000000;
        public const int MaximumDepoLeverage = 500;

        public Account()
        {
            TimeCreated = DateTime.Now;
        }
        public Account(Account acc)
        {
            ID = acc.ID;            
            Group = acc.Group;
            Balance = acc.Balance;
            Equity = acc.Equity;
            UsedMargin = acc.UsedMargin;
            Currency = acc.Currency;
            MaxLeverage = acc.MaxLeverage;
            TimeCreated = acc.TimeCreated;
            TimeBlocked = acc.TimeBlocked;
            Status = acc.Status;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1} {2}, {3} {4}", ID, Localizer.GetString("TitleAccountGroupShort"), Group,
                                 Localizer.GetString("TitleBalance"), Balance.ToStringUniformMoneyFormat());
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.BL.Localisation;

namespace TradeSharp.SiteAdmin.Models
{
   public class AccountUserModel
    {
        [LocalizedDisplayName("TitleUserId")]
        public int UserId { get; set; }

        [LocalizedDisplayName("Login")]
        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        [StringLength(50, MinimumLength = 7, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageStringLength7_50")]
        public string UserLogin { get; set; }

        [LocalizedDisplayName("Password")]
        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        [StringLength(50, MinimumLength = 7, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageStringLength7_50")]
        [DataType(DataType.Password)]
        public string UserPassword { get; set; }

        [LocalizedDisplayName("TitleUserName")]
        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        [StringLength(50, MinimumLength = 2, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageStringLength2_50")]
        public string UserName { get; set; }

        [LocalizedDisplayName("TitlesSurname")]
        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        [StringLength(50, MinimumLength = 2, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageStringLength2_50")]
        public string UserSurname { get; set; }

        [LocalizedDisplayName("TitlePatronymic")]
        [StringLength(50, MinimumLength = 2, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageStringLength2_50")]
        public string UserPatronym { get; set; }

        [LocalizedDisplayName("TitleUserFullName")]
        public string Title { get; set; }

        [LocalizedDisplayName("TitleAdditionalDescription")]
        public string UserDescription { get; set; }

        [LocalizedDisplayName("TitleEmailAddress")]
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageInvalidEmail")]
        public string UserEmail { get; set; }

        [LocalizedDisplayName("TitleUserPhone1")]
        [RegularExpression(@"^((8|\+7)[\- ]?)?(\(?\d{3}\)?[\- ]?)?[\d\- ]{6,10}$", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageInvalidUserPhone")]
        public string UserPhone1 { get; set; }

        [LocalizedDisplayName("TitleUserPhone2")]
        [RegularExpression(@"^((8|\+7)[\- ]?)?(\(?\d{3}\)?[\- ]?)?[\d\- ]{6,10}$", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageInvalidUserPhone")]
        public string UserPhone2 { get; set; }

        /// <summary>
        /// Соответствие "уникальный идентификатор счёта / права, которе имеет пользователь на этот счёт"
        /// </summary>
        [LocalizedDisplayName("TitleRights")]
        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        public Dictionary<int, UserAccountRights> UserRightsMask { get; set; }

        [LocalizedDisplayName("TitleRole")]
        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        public UserRole UserRoleMask { get; set; }

        [LocalizedDisplayName("TitleDateRegistration")]
        [Required(ErrorMessageResourceName = "ErrorMessageFieldRequired", ErrorMessageResourceType = typeof(Resource))]
        public DateTime UserRegistrationDate { get; set; }

        /// <summary>
        /// Список служб, которыми управляет пользователь
        /// </summary>
        [LocalizedDisplayName("TitleServices")]
        public ICollection<SERVICE> UserService { get; set; }

        /// <summary>
        /// Вспомогательное поле "количество сигналов пользователя", 
        /// для того, что бы в представлении UserList указать есть ли у пользователя собственные сигналы и если есть, то сколько
        /// </summary>
        public int CountOwnerSignal { get; set; }

        /// <summary>
        /// Вспомогательное поле "количество подписок пользователя", 
        /// для того, что бы в представлении UserList указать есть ли у пользователя подписки и если есть, то сколько
        /// </summary>
        public int CountSubscription { get; set; }

        /// <summary>
        /// Валюта кошелька T#, пренадлежащего данному пользователю 
        /// </summary>
        [LocalizedDisplayName("TitleCurrencyPurse")]
        public string WalletCurrency { get; set; }

        /// <summary>
        /// Средства на кошельке T#, пренадлежащего данному пользователю 
        /// </summary>
        [LocalizedDisplayName("TitleWalletBalance")]
        public decimal WalletBalance { get; set; }


        /// <summary>
        /// Создаёт неполную копию объекта. Копирует только нестатические поля
        /// </summary>
        public AccountUserModel DeepCopy()
        {
            var other = (AccountUserModel)MemberwiseClone();
            return other;
        }
    }
}
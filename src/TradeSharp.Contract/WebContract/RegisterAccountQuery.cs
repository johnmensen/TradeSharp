using System.Runtime.Serialization;
using TradeSharp.Contract.Entity;


namespace TradeSharp.Contract.WebContract
{
    [DataContract]
    public class RegisterAccountQuery : HttpParameter
    {
        #region поля для пользователя
        [DataMember]
        public string UserLogin { get; set; }

        [DataMember]
        public string UserPassword { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string UserSurname { get; set; }

        [DataMember]
        public string UserPatronym { get; set; }

        [DataMember]
        public string UserDescription { get; set; }

        [DataMember]
        public string UserEmail { get; set; }

        [DataMember]
        public string UserPhone1 { get; set; }

        [DataMember]
        public string UserPhone2 { get; set; }

        [DataMember]
        public UserAccountRights UserRightsMask { get; set; }

        [DataMember]
        public UserRole UserRoleMask { get; set; }
        #endregion

        #region поля для счёта
        [DataMember]
        public float Balance { get; set; }

        [DataMember]
        public string Group { get; set; }

        [DataMember]
        public string Currency { get; set; }

        [DataMember]
        public float MaxLeverage { get; set; }
        #endregion     
    }
}

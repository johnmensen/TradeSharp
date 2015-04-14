namespace TradeSharp.Contract.Entity
{
    /// <summary>
    /// Запись таблици сопоставления "Кошелёк платёжной системы" - "Кошелёк T#"
    /// По этой таблице ищется, какой намер кошелька в какой то платёжной системе   какому T# кошелёку  соответствует
    /// </summary>
    public class UserPaymentSystem
    {
        /// <summary>
        /// В системе T# на один кошелёк может быть зарегистрировано несколько платёжных систем - "UserId + SystemPayment + RootId + PurseId" дают уникальное сочетание.
        /// Следовательно, что бы определить, какой Id "кошелька", какой либо платёжной системы, нужно указать это сочетание
        /// 
        /// Вообще "UserId" уникально само по себе и "SystemPayment + RootId + PurseId" уникально само по себе
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Уникальный идентификатор пользователя в T#. Технически - это уникальный идентификатор и кошелька пользователя потому, что 
        /// в T# всегда одному пользователю соответствует один кошелёк.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Приведённый к Byte экземпляр репечисления PaymentSystem.
        /// </summary>
        public PaymentSystem SystemPayment { get; set; }

        /// <summary>
        /// Уникальный идентификатор пользователя в какой либо платёжной системе (например, WmId = "128586585" в WebMoney).
        /// </summary>
        public string RootId { get; set; }

        /// <summary>
        /// Уникальный идентификатор "кошелька", какой либо платёжной системы. Например, в системе WebMoney это строка типа R111111111
        /// в какой то другой системе это может быть вообще совокупность элементов (она будет представлена как строка JSON). 
        /// </summary>
        public string PurseId { get; set; }

        /// <summary>
        /// Подтверждён ли этот кошелёк
        /// </summary>
        public bool PurseConfirm { get; set; }

        /// <summary>
        /// Имя пользователся. В таких системах как PayPal это обязательное поле
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Фамилия пользователся. В таких системах как PayPal это обязательное поле
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// электронный адрес для этого кошелька этой платёжной системы. По умолчанию устанавливается электронный адрес указанный
        /// пользователем при регистрации в терминале T#
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// проверяет по совпадению фактического
        /// (составного) ключа
        /// </summary>
        public bool AreSame(UserPaymentSystem syst)
        {
            return SystemPayment == syst.SystemPayment && RootId == syst.RootId &&
                   PurseId == syst.PurseId;
        }
    }
}

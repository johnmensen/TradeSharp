using System;
using System.Text.RegularExpressions;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class PlatformUser
    {
        private static readonly Regex regLogin = new Regex(@"[\-0-9a-zA-Z]+");

        public int ID { get; set; }
        private string title;
        public string Title
        {
            get { return title; }
            set { title = string.IsNullOrEmpty(value) ? string.Empty : value.Length <= 50 ? value : value.Substring(0, 50); }
        }
        private string name;
        public string Name
        {
            get { return name; }
            set { name = string.IsNullOrEmpty(value) ? string.Empty : value.Length <= 50 ? value : value.Substring(0, 50); }
        }
        private string surname;
        public string Surname
        {
            get { return surname; }
            set { surname = string.IsNullOrEmpty(value) ? string.Empty : value.Length <= 50 ? value : value.Substring(0, 50); }
        }
        private string patronym;
        public string Patronym
        {
            get { return patronym; }
            set { patronym = string.IsNullOrEmpty(value) ? string.Empty : value.Length <= 50 ? value : value.Substring(0, 50); }
        }
        private string description;
        public string Description
        {
            get { return description; }
            set { description = string.IsNullOrEmpty(value) ? string.Empty : value.Length <= 50 ? value : value.Substring(0, 50); }
        }
        private string email;
        public string Email
        {
            get { return email; }
            set { email = string.IsNullOrEmpty(value) ? string.Empty : (value.Length <= 50 ? value : value.Substring(0, 50)).ToLower(); }
        }
        private string phone1;
        public string Phone1
        {
            get { return phone1; }
            set { phone1 = string.IsNullOrEmpty(value) ? string.Empty : value.Length <= 25 ? value : value.Substring(0, 25); }
        }
        private string phone2;
        public string Phone2
        {
            get { return phone2; }
            set { phone2 = string.IsNullOrEmpty(value) ? string.Empty : value.Length <= 25 ? value : value.Substring(0, 25); }
        }
        private string login;
        public string Login
        {
            get { return login; }
            set { login = string.IsNullOrEmpty(value) ? string.Empty : value.Length <= 25 ? value : value.Substring(0, 25); }
        }
        private string password;
        public string Password
        {
            get { return password; }
            set { password = string.IsNullOrEmpty(value) ? string.Empty : value.Length <= 25 ? value : value.Substring(0, 25); }
        }
        public UserRole RoleMask { get; set; }
        public DateTime RegistrationDate { get; set; }
        /// <summary>
        /// права пользователя на акаунт
        /// </summary>
        public UserAccountRights? RightsMask { get; set; }

        #region Проверки
        public static bool CheckLoginSpelling(string login)
        {
            return regLogin.IsMatch(login);
        }

        public const int LoginLenMin = 7, LoginLenMax = 25;
        public const int EmailLenMin = 6, EmailLenMax = 50;
        public const int PasswordLenMin = 6, PasswordLenMax = 25;

        #endregion

        public string MakeFullName()
        {
            return string.IsNullOrEmpty(Name)
                       ? Surname ?? Login
                       : string.Format("{0} {1}{2}", Surname, Name,
                                       string.IsNullOrEmpty(Patronym) ? "" : " " + Patronym);
        }

        public string MakeNameWithInitials()
        {
            if (string.IsNullOrEmpty(Surname)) return string.IsNullOrEmpty(Name) ? Login : Name; ;
            if (string.IsNullOrEmpty(Name)) return string.IsNullOrEmpty(Surname) ? Login : Surname;
            return string.IsNullOrEmpty(Patronym)
                       ? string.Format("{0} {1}.", Surname, Name[0])
                       : string.Format("{0} {1}. {2}.", Surname, Name[0], Patronym[0]);
        }

        public string FullName
        {
            get { return MakeFullName(); }
        }

        public string NameWithInitials
        {
            get { return MakeNameWithInitials(); }
        }
    }
}

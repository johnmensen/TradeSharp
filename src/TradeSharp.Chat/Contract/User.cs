using TradeSharp.Contract.Entity;

namespace TradeSharp.Chat.Contract
{
    public enum UserActionType
    {
        Enter,
        Exit,
        StatusChange,
        RoomEnter,
        RoomLeave
    };

    public class User : PlatformUser
    {
        public User()
        {
        }

        public User(PlatformUser user)
        {
            ID = user.ID;
            Title = user.Title;
            Name = user.Name;
            Surname = user.Surname;
            Patronym = user.Patronym;
            Description = user.Description;
            Login = user.Login;
            RoleMask = user.RoleMask;
        }

        public User(User user) : this((PlatformUser)user)
        {
        }

        public string NickName
        {
            get
            {
                string result;
                if (!string.IsNullOrEmpty(Name))
                {
                    result = Name;
                    if (!string.IsNullOrEmpty(Surname))
                    {
                        result = Surname + " " + result;
                        if (!string.IsNullOrEmpty(Patronym))
                            result += " " + Patronym;
                    }
                }
                else if (!string.IsNullOrEmpty(Title))
                    result = Title;
                else if (!string.IsNullOrEmpty(Login))
                    result = Login;
                else
                    result = "ID: " + ID.ToString();
                return result;
            }
            //set { Login = value; } // 4 debug
        }

        public override string ToString()
        {
            return NickName;
            //return NickName + " ID: " + ID.ToString();
            //return "ID: " + ID.ToString() + " Login: " + Login + " Name: " + Name + " Surname: " + Surname + " Patronym: " + Patronym;
        }
    }
}

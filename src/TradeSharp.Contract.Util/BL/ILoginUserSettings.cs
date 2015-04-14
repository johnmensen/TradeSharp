using System.Collections.Generic;
using TradeSharp.Contract.Contract;

namespace TradeSharp.Contract.Util.BL
{
    public delegate bool AuthenticateDel(string login, string password,
                                      out AuthenticationResponse response, out string authStatusString);
    
    public interface ILoginUserSettings
    {
        string Login { get; set; }

        List<string> LastLogins { get; }

        string GetPasswordForLogin(string login);

        void StoreLogin(string login, string pwrd);
        
        void SaveSettings();
    }
}

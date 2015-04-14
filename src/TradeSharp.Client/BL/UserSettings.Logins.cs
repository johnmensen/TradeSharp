using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Entity;

namespace TradeSharp.Client.BL
{
    public partial class UserSettings
    {
        private readonly Dictionary<string, string> loginPassword = new Dictionary<string, string>();

        #region ILoginUserSettings

        [PropertyXMLTag("Login")]
        public string Login { get; set; }

        public void StoreLogin(string login, string pwrd)
        {
            Login = login;

            if (loginPassword.ContainsKey(login))
                loginPassword[login] = pwrd;
            else 
                loginPassword.Add(login, pwrd);

            lastTimeModified.Touch();
        }

        public string GetPasswordForLogin(string login)
        {
            string pwrd;
            loginPassword.TryGetValue(login, out pwrd);
            return pwrd;
        }

        public List<string> LastLogins
        {
            get { return loginPassword.Keys.ToList(); }
        }
        
        #endregion

        public void SaveLoginPassword(XmlDocument doc)
        {
            var nodeAccounts = doc.DocumentElement.AppendChild(doc.CreateElement("accounts"));
            foreach (var pair in loginPassword)
            {
                // закодировать пароль
                string pwrdCiphered;
                cipher.EncryptSafe(pair.Value, out pwrdCiphered);

                // добавить в XML
                var account = nodeAccounts.AppendChild(doc.CreateElement("account"));
                account.Attributes.Append(doc.CreateAttribute("login")).Value = pair.Key;
                account.Attributes.Append(doc.CreateAttribute("pwrd")).Value = pwrdCiphered;
            }
        }

        public void LoadLoginPassword(XmlDocument doc)
        {
            if (doc.DocumentElement == null) return;
            var accountsNode = doc.DocumentElement.SelectSingleNode("accounts");
            if (accountsNode == null) return;
            
            loginPassword.Clear();

            foreach (XmlElement node in accountsNode)
            {
                var atrLogin = node.Attributes["login"];
                if (atrLogin == null) continue;
                var login = atrLogin.Value;
                if (string.IsNullOrEmpty(login)) continue;

                var atrPwrd = node.Attributes["pwrd"];
                var pwrd = atrPwrd == null ? string.Empty : atrPwrd.Value;
                var pwrdClear = string.Empty;
                if (!string.IsNullOrEmpty(pwrd))
                    cipher.DecryptSafe(pwrd, out pwrdClear);

                if (loginPassword.ContainsKey(login)) continue;
                loginPassword.Add(login, pwrdClear);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using TradeSharp.Chat.Client.BL;
using TradeSharp.Chat.Contract;

namespace TradeSharp.Chat.Client.Form
{
    public partial class LoginForm : System.Windows.Forms.Form
    {
        private List<User> users = new List<User>();

        public LoginForm()
        {
            InitializeComponent();
            users = AllUsers.Instance.GetAllUsers();
            foreach (var user in users)
                loginComboBox.Items.Add("[" + user.ID + "] " + user.FullName);
        }

        public int GetId()
        {
            if (loginComboBox.SelectedIndex == -1)
                return 0;
            return users[loginComboBox.SelectedIndex].ID;
        }

        public User GetUser()
        {
            if (loginComboBox.SelectedIndex == -1)
                return null;
            return users[loginComboBox.SelectedIndex];
        }

        private void LoginComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            enterButton.Enabled = loginComboBox.SelectedIndex != -1;
        }
    }
}

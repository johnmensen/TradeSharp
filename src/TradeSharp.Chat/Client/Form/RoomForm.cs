using System.Linq;
using System.Windows.Forms;
using TradeSharp.Chat.Client.BL;
using TradeSharp.Chat.Contract;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Chat.Client.Form
{
    public partial class RoomForm : System.Windows.Forms.Form
    {
        private int roomId;
        private readonly User currentUser;
        
        public RoomForm(int userId)
        {
            InitializeComponent();
            ownerComboBox.Items.Add(new User {Name = "(нет)", ID = 0});
            foreach (var user in AllUsers.Instance.GetAllUsers())
                ownerComboBox.Items.Add(user);
            currentUser = AllUsers.Instance.GetAllUsers().FirstOrDefault(u => u.ID == userId);
            if (currentUser == null)
                return;
            ownerComboBox.SelectedItem = currentUser;
            var admin = ((int) currentUser.RoleMask & (int) UserRole.Administrator) != 0;
            if (admin)
            {
                ownerComboBox.Enabled = true;
                isBoundCheckBox.Enabled = true;
            }
        }

        public Room GetRoom()
        {
            var result = new Room
                {
                    Id = roomId,
                    Name = nameTextBox.Text,
                    Owner = ownerComboBox.SelectedItem != null ? ((User)ownerComboBox.SelectedItem).ID : 0,
                    Description = descriptionRichTextBox.Text,
                    Greeting = greetingRichTextBox.Text,
                    Password = passwordTextBox.Text,
                    IsBound = isBoundCheckBox.Checked
                };
            return result;
        }

        public void SetRoom(Room room)
        {
            roomId = room.Id;
            nameTextBox.Text = room.Name;
            var owner = AllUsers.Instance.GetAllUsers().FirstOrDefault(u => u.ID == room.Owner);
            if (owner != null)
                ownerComboBox.SelectedItem = owner;
            else if (room.Owner == 0)
                ownerComboBox.SelectedIndex = 0;
            else
            {
                MessageBox.Show(this,
                                "Пользователь-владелец комнаты с идентификатором " + room.Owner + " отсутствует в БД",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            descriptionRichTextBox.Text = room.Description;
            greetingRichTextBox.Text = room.Greeting;
            // TODO: аккуратнее с паролем - с сервера приходят звездочки, вместо подлинного
            // TODO: за этим нужно проследить во время измненения комнаты
            passwordTextBox.Text = room.Password;
            passwordConfirmationTextBox.Text = room.Password;
            isBoundCheckBox.Checked = room.IsBound;
            if (!room.IsBound && room.ExpireTime.HasValue)
            {
                expireLabel.Visible = true;
                expireTimeLabel.Visible = true;
                expireTimeLabel.Text = room.ExpireTime.Value.ToString();
            }
            nameTextBox.ReadOnly = true;
            var admin = ((int)currentUser.RoleMask & (int)UserRole.Administrator) != 0;
            if (!admin && owner != currentUser)
            {
                //nameTextBox.ReadOnly = true;
                descriptionRichTextBox.ReadOnly = true;
                greetingRichTextBox.ReadOnly = true;
                passwordTextBox.ReadOnly = true;
                passwordConfirmationTextBox.ReadOnly = true;
            }
        }

        private void CheckOkButton()
        {
            var enabled = !string.IsNullOrEmpty(nameTextBox.Text);
            enabled &= passwordTextBox.Text == passwordConfirmationTextBox.Text;
            okButton.Enabled = enabled;
        }

        private void NameTextBoxTextChanged(object sender, System.EventArgs e)
        {
            CheckOkButton();
        }

        private void PasswordTextBoxTextChanged(object sender, System.EventArgs e)
        {
            CheckOkButton();
        }

        private void PasswordConfirmationTextBoxTextChanged(object sender, System.EventArgs e)
        {
            CheckOkButton();
        }
    }
}

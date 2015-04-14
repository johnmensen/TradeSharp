using System.Windows.Forms;
using TradeSharp.Chat.Client.BL;

namespace TradeSharp.Chat.Client.Form
{
    public partial class SettingsForm : System.Windows.Forms.Form
    {
        public SettingsForm()
        {
            InitializeComponent();

            ownLabel.Font = ChatSettings.Instance.OwnFont;
            ownLabel.Text = ownLabel.Font.FontFamily.Name;
            ownLabel.ForeColor = ChatSettings.Instance.OwnColor;

            ownerLabel.Font = ChatSettings.Instance.OwnerFont;
            ownerLabel.Text = ownerLabel.Font.FontFamily.Name;
            ownerLabel.ForeColor = ChatSettings.Instance.OwnerColor;

            othersLabel.Font = ChatSettings.Instance.OthersFont;
            othersLabel.Text = othersLabel.Font.FontFamily.Name;
            othersLabel.ForeColor = ChatSettings.Instance.OthersColor;

            dateTimeFormatTextBox.Text = ChatSettings.Instance.DateTimeFormat;
            showNotificationsCheckBox.Checked = ChatSettings.Instance.ShowNotifications;
            showLogCheckBox.Checked = ChatSettings.Instance.ShowLog;
            autoLoginCheckBox.Checked = ChatSettings.Instance.AutoLogin;
        }

        private void OkButtonClick(object sender, System.EventArgs e)
        {
            ChatSettings.Instance.OwnFont = ownLabel.Font;
            ChatSettings.Instance.OwnColor = ownLabel.ForeColor;
            ChatSettings.Instance.OwnerFont = ownerLabel.Font;
            ChatSettings.Instance.OwnerColor = ownerLabel.ForeColor;
            ChatSettings.Instance.OthersFont = othersLabel.Font;
            ChatSettings.Instance.OthersColor = othersLabel.ForeColor;
            ChatSettings.Instance.DateTimeFormat = dateTimeFormatTextBox.Text;
            ChatSettings.Instance.ShowNotifications = showNotificationsCheckBox.Checked;
            ChatSettings.Instance.ShowLog = showLogCheckBox.Checked;
            ChatSettings.Instance.AutoLogin = autoLoginCheckBox.Checked;
        }

        private void ChangeFont(Label label)
        {
            var dialog = new FontDialog();
            dialog.Font = label.Font;
            if (dialog.ShowDialog(this) == DialogResult.Cancel)
                return;
            label.Font = dialog.Font;
            label.Text = label.Font.FontFamily.Name;
        }

        private void ChangeColor(Label label)
        {
            var dialog = new ColorDialog();
            dialog.Color = label.ForeColor;
            dialog.AllowFullOpen = true;
            if (dialog.ShowDialog(this) == DialogResult.Cancel)
                return;
            label.ForeColor = dialog.Color;
        }

        private void OwnFontButtonClick(object sender, System.EventArgs e)
        {
            ChangeFont(ownLabel);
        }

        private void OwnColorButtonClick(object sender, System.EventArgs e)
        {
            ChangeColor(ownLabel);
        }

        private void OwnerFontButtonClick(object sender, System.EventArgs e)
        {
            ChangeFont(ownerLabel);
        }

        private void OwnerColorButtonClick(object sender, System.EventArgs e)
        {
            ChangeColor(ownerLabel);
        }

        private void OthersFontButtonClick(object sender, System.EventArgs e)
        {
            ChangeFont(othersLabel);
        }

        private void OthersColorButtonClick(object sender, System.EventArgs e)
        {
            ChangeColor(othersLabel);
        }
    }
}

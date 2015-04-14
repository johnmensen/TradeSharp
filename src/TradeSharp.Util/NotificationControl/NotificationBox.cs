using System.Windows.Forms;
using TradeSharp.Util.Forms;

namespace TradeSharp.Util.NotificationControl
{
    public static class NotificationBox
    {
        public static DialogResult Show(string text, string caption, out bool repeatNotification)
        {
            var form = new NotificationForm(text, caption, MessageBoxButtons.OK, MessageBoxIcon.None);
            return ShowMessage(form, out repeatNotification);
        }

        public static DialogResult Show(string text, string caption, MessageBoxIcon icon, out bool repeatNotification)
        {
            var form = new NotificationForm(text, caption, MessageBoxButtons.OK, icon);
            return ShowMessage(form, out repeatNotification);
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, out bool repeatNotification)
        {
            var form = new NotificationForm(text, caption, buttons, MessageBoxIcon.None);
            return ShowMessage(form, out repeatNotification);
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, out bool repeatNotification)
        {
            var form = new NotificationForm(text, caption, buttons, icon);
            return ShowMessage(form, out repeatNotification);
        }

        private static DialogResult ShowMessage(NotificationForm notificationForm, out bool repeatNotification)
        {
            var dialogResult = notificationForm.ShowDialog();
            repeatNotification = notificationForm.RepeatNotification;
            return dialogResult;
        }
    }
}

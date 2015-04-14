using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TradeSharp.Util.Forms
{
    public partial class NotificationForm : Form
    {
        private readonly Dictionary<MessageBoxButtons, List<Button>> allBtnSets;

        public NotificationForm()
            : this("Sample text", "Sample caption", MessageBoxButtons.OK, MessageBoxIcon.None)
        {
        }
        
        public NotificationForm(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            InitializeComponent();

            allBtnSets = new Dictionary<MessageBoxButtons, List<Button>>
                {
                    {MessageBoxButtons.OK, new List<Button> {btnOk}},
                    {MessageBoxButtons.OKCancel, new List<Button> {btnOk, btnCancel}},
                    {MessageBoxButtons.AbortRetryIgnore, new List<Button> {btnAbort, btnRetry, btnIgnore}},
                    {MessageBoxButtons.RetryCancel, new List<Button> {btnRetry, btnCancel}},
                    {MessageBoxButtons.YesNo, new List<Button> {btnYes, btnNo}},
                    {MessageBoxButtons.YesNoCancel, new List<Button> {btnYes, btnNo, btnCancel}}
                };
            
            InitButtonSet(buttons);
            InitText(text, caption);
            Localizer.LocalizeControl(this);

            switch (icon)
            {
                case MessageBoxIcon.Error:
                    pictureBox.Image = SystemIcons.Error.ToBitmap();
                    break;
                case MessageBoxIcon.Question:
                    pictureBox.Image = SystemIcons.Question.ToBitmap();
                    break;
                case MessageBoxIcon.Exclamation:
                    pictureBox.Image = SystemIcons.Exclamation.ToBitmap();
                    break;
                case MessageBoxIcon.Asterisk:
                    pictureBox.Image = SystemIcons.Asterisk.ToBitmap();
                    break;
            }
        }

        private void InitText(string text, string caption)
        {
            title.Text = caption;
            title.Left = (Width - title.Width) / 2;
            mainText.Text = text;
            mainText.SelectionAlignment = HorizontalAlignment.Center;            
        }

        private void InitButtonSet(MessageBoxButtons buttons)
        {
            const int margin = 5;
            var left = 0;

            List<Button> btnSet;
            try
            {
                btnSet = allBtnSets[buttons];
            }
            catch (Exception ex)
            {
                Logger.Error("InitButtonSet (" + buttons + ")", ex);
                throw;
            }
            
            foreach (var button in btnSet)
            {
                if (button == null)
                    throw new Exception("button is null in " + buttons);

                button.Visible = true;
                button.Left = left;
                left = button.Left + button.Width + margin;
            }
        }

        public bool RepeatNotification
        {
            get { return repeatNotification.Checked; }
            set { repeatNotification.Checked = value; }
        }
    }
}

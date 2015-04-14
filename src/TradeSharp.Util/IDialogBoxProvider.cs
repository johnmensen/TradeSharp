using System.Collections.Generic;
using System.Windows.Forms;

namespace TradeSharp.Util
{
    public interface IDialogBoxProvider
    {
        DialogResult ShowMessageBox(string text);
        DialogResult ShowMessageBox(string text, string caption);
        DialogResult ShowMessageBox(string text, string caption, MessageBoxButtons buttons);
        DialogResult ShowMessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon);

        string ShowInputDialog(string caption, string label, bool enableCancel, out DialogResult rst);
        string ShowInputDialog(string caption, string label, bool enableCancel, string inputText, out DialogResult rst);
        string ShowInputDialog(string caption, string label, out DialogResult rst);
        bool ShowComboDialog(string caption, List<object> items, out object selected, out string inputText,
                             FormStartPosition startPosition, bool listStyle);
        bool ShowComboDialog(string caption, List<object> items, out object selected, out string inputText);
        bool ShowComboDialog(string caption, List<object> items, out object selected, out string inputText, bool listStyle);
    }

    public class DialogBoxProvider : IDialogBoxProvider
    {
        public DialogResult ShowMessageBox(string text)
        {
            return MessageBox.Show(text);
        }

        public DialogResult ShowMessageBox(string text, string caption)
        {
            return MessageBox.Show(text, caption);
        }

        public DialogResult ShowMessageBox(string text, string caption, MessageBoxButtons buttons)
        {
            return MessageBox.Show(text, caption, buttons);
        }

        public DialogResult ShowMessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return MessageBox.Show(text, caption, buttons, icon);
        }

        public string ShowInputDialog(string caption, string label, bool enableCancel, out DialogResult rst)
        {
            return Dialogs.ShowInputDialog(caption, label, enableCancel, out rst);
        }

        public string ShowInputDialog(string caption, string label, bool enableCancel, string inputText, out DialogResult rst)
        {
            return Dialogs.ShowInputDialog(caption, label, enableCancel, inputText, out rst);
        }

        public string ShowInputDialog(string caption, string label, out DialogResult rst)
        {
            return Dialogs.ShowInputDialog(caption, label, out rst);
        }

        public bool ShowComboDialog(string caption, List<object> items, out object selected, out string inputText,
                                    FormStartPosition startPosition, bool listStyle)
        {
            return Dialogs.ShowComboDialog(caption, items, out selected, out inputText,
                                           startPosition, listStyle);
        }

        public bool ShowComboDialog(string caption, List<object> items, out object selected, out string inputText)
        {
            return Dialogs.ShowComboDialog(caption, items, out selected, out inputText);
        }

        public bool ShowComboDialog(string caption, List<object> items, out object selected, out string inputText, bool listStyle)
        {
            return Dialogs.ShowComboDialog(caption, items, out selected, out inputText, listStyle);
        }
    }
}

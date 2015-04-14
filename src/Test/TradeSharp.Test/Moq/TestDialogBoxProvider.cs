using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TradeSharp.Util;

namespace TradeSharp.Test.Moq
{
    class TestDialogBoxProvider : IDialogBoxProvider
    {
        private DialogResult expectedResult = DialogResult.OK;

        public DialogResult ExpectedResult
        {
            get { return expectedResult; }
            set { expectedResult = value; }
        }

        public DialogResult ShowMessageBox(string text)
        {
            return expectedResult;
        }

        public DialogResult ShowMessageBox(string text, string caption)
        {
            return expectedResult;
        }

        public DialogResult ShowMessageBox(string text, string caption, MessageBoxButtons buttons)
        {
            return expectedResult;
        }

        public DialogResult ShowMessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return expectedResult;
        }

        public string ShowInputDialog(string caption, string label, bool enableCancel, out DialogResult rst)
        {
            throw new NotImplementedException();
        }

        public string ShowInputDialog(string caption, string label, bool enableCancel, string inputText, out DialogResult rst)
        {
            throw new NotImplementedException();
        }

        public string ShowInputDialog(string caption, string label, out DialogResult rst)
        {
            throw new NotImplementedException();
        }

        public bool ShowComboDialog(string caption, List<object> items, out object selected, out string inputText,
                                    FormStartPosition startPosition, bool listStyle)
        {
            selected = items.FirstOrDefault();
            inputText = selected == null ? string.Empty : selected.ToString();
            return true;
        }

        public bool ShowComboDialog(string caption, List<object> items, out object selected, out string inputText)
        {
            selected = items.FirstOrDefault();
            inputText = selected == null ? string.Empty : selected.ToString();
            return true;
        }

        public bool ShowComboDialog(string caption, List<object> items, out object selected, out string inputText, bool listStyle)
        {
            selected = items.FirstOrDefault();
            inputText = selected == null ? string.Empty : selected.ToString();
            return true;
        }
    }
}

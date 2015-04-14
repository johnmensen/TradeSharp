using System.Collections.Generic;
using System.Windows.Forms;
using TradeSharp.Util.Forms;

namespace TradeSharp.Util
{
    public static class Dialogs
    {
        /// <summary>
        /// Ввод значения из модального диалога
        /// </summary>
        /// <param name="caption">заголовок окна диалога</param>
        /// <param name="label">надпись в окне слева от поля ввода</param>
        /// <param name="enableCancel">видимость кнопки "Отмена"</param>
        /// <param name="rst">DialogResult.OK - нажата кнопка "Принять", DialogResult.Cancel - нажата кнопка "Отмена"</param>
        /// <returns></returns>
        public static string ShowInputDialog(string caption, string label, 
            bool enableCancel, out DialogResult rst)
        {
            var dlg = new SimpleDialog(caption, label, enableCancel);
            rst = dlg.ShowDialog();
            return dlg.InputValue;
        }

        /// <summary>
        /// Ввод значения из модального диалога
        /// </summary>
        /// <param name="caption">заголовок окна диалога</param>
        /// <param name="label">надпись в окне слева от поля ввода</param>
        /// <param name="enableCancel">видимость кнопки "Отмена"</param>
        /// <param name="inputText">начальное значение</param>
        /// <param name="rst">DialogResult.OK - нажата кнопка "Принять", DialogResult.Cancel - нажата кнопка "Отмена"</param>
        /// <returns></returns>
        public static string ShowInputDialog(string caption, string label,
            bool enableCancel, string inputText, out DialogResult rst)
        {
            var dlg = new SimpleDialog(caption, label, enableCancel, inputText);
            rst = dlg.ShowDialog();
            return dlg.InputValue;
        }

        /// <summary>
        /// Ввод значения из модального диалога
        /// </summary>
        /// <param name="caption">заголовок окна диалога</param>
        /// <param name="label">надпись в окне слева от поля ввода</param>
        /// <param name="rst">DialogResult.OK - нажата кнопка "Принять", DialogResult.Cancel - нажата кнопка "Отмена"</param>
        /// <returns></returns>
        public static string ShowInputDialog(string caption, string label, out DialogResult rst)
        {
            return ShowInputDialog(caption, label, true, out rst);
        }

        public static bool ShowComboDialog(string caption, List<object> items, out object selected, out string inputText, 
            FormStartPosition startPosition, bool listStyle)
        {
            selected = null;
            inputText = null;
            var ddd = new DropDownDialog(caption, items, listStyle) { StartPosition = startPosition };
            if (ddd.ShowDialog() == DialogResult.OK)
            {
                selected = ddd.SelectedItem;
                inputText = ddd.SelectedText;
                return true;
            }
            return false;
        }

        public static bool ShowComboDialog(string caption, List<object> items, out object selected, out string inputText)
        {
            return ShowComboDialog(caption, items, out selected, out inputText, FormStartPosition.WindowsDefaultLocation, false);
        }

        public static bool ShowComboDialog(string caption, List<object> items, out object selected, out string inputText, bool listStyle)
        {
            return ShowComboDialog(caption, items, out selected, out inputText, FormStartPosition.WindowsDefaultLocation, listStyle);
        }
    }
}

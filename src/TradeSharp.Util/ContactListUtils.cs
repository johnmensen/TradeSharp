using System.Collections.Generic;
using System.Windows.Forms;
using TradeSharp.Util.Controls;

namespace TradeSharp.Util
{
    public static class ContactListUtils
    {
        public static string PackContacts(ListView listView)
        {
            var result = new List<string>();
            foreach (ListViewItem item in listView.Items)
            {
                result.Add(string.Format("[{0}]{1}", item.ImageKey, item.Text));
            }
            return string.Join("\n", result);
        }

        public static string PackContacts(ImagedLabelList contactList)
        {
            return string.Join("\n", contactList.Labels);
        }

        public static void UnpackContacts(string contacts, ListView listView)
        {
            listView.Items.Clear();
            if (string.IsNullOrEmpty(contacts))
                return;
            var stringList = contacts.Split(new[] { '\n' });
            foreach (var str in stringList)
            {
                var text = str;
                string key = null;
                try
                {
                    var keyBeginPos = str.IndexOf('[');
                    if (keyBeginPos != -1)
                    {
                        var keyEndPos = str.IndexOf(']', keyBeginPos);
                        if (keyEndPos != -1)
                            key = str.Substring(keyBeginPos + 1, keyEndPos - keyBeginPos - 1);
                        text = str.Substring(keyEndPos + 1);
                    }
                    listView.Items.Add(string.IsNullOrEmpty(key) ? new ListViewItem(text) : new ListViewItem(text, key));
                }
                catch
                {
                }
            }
        }

        public static void UnpackContacts(string contacts, ImagedLabelList contactList)
        {
            contactList.Labels.Clear();
            if (string.IsNullOrEmpty(contacts))
                return;
            contactList.Labels = new List<string>(contacts.Split(new[] { '\n' }));
        }
    }
}

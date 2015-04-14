using System;
using System.Windows.Forms;

namespace TradeSharp.Util
{
    // подстановщик строк (переводчик)
    public interface IResourceResolver
    {
        string TryGetResourceValue(string resxKey);
    }

    // обобщение для всех классов-переводчиков
    public class Localizer
    {
        public static IResourceResolver ResourceResolver;

        // для использования в EnumFriendlyName<T>
        // но для удобства установки перенесен сюда
        public static bool UseCache = true;

        public static bool ThrowResourceNotFoundException = true;

        public static bool HasKey(string key)
        {
            if (ResourceResolver == null)
                return false;
            return ResourceResolver.TryGetResourceValue(key) != null;
        }

        public static string GetString(string key)
        {
            if (ResourceResolver == null)
                return null;
            var result = ResourceResolver.TryGetResourceValue(key);
            if (result != null)
                return result;
            if (ThrowResourceNotFoundException)
                throw new Exception(String.Format("Localizer.GetString: resource not found: {0}", key));
            return key;
        }

        // локализация контролов по якорю в Control.Tag
        public static void LocalizeControl(Control control)
        {
            if (ResourceResolver == null)
                return;
            if (control.Tag != null)
                control.Text = ResourceResolver.TryGetResourceValue(control.Tag.ToString());
            foreach (Control childControl in control.Controls)
            {
                LocalizeControl(childControl);

                if (childControl.ContextMenuStrip != null)
                {
                    foreach (ToolStripItem menuItem in childControl.ContextMenuStrip.Items)
                        LocalizeToolStripItem(menuItem);
                }
                var menu = childControl as ToolStrip;
                if (menu == null)
                    continue;
                foreach (ToolStripItem menuItem in menu.Items)
                    LocalizeToolStripItem(menuItem);
            }
        }

        public static void LocalizeToolStripItem(ToolStripItem item)
        {
            if (ResourceResolver == null)
                return;
            if (item.Tag != null)
                item.Text = ResourceResolver.TryGetResourceValue(item.Tag.ToString());
            var dropDownItem = item as ToolStripDropDownItem;
            if (dropDownItem == null)
                return;
            foreach (ToolStripItem childItem in dropDownItem.DropDownItems)
                LocalizeToolStripItem(childItem);
        }
    }
}
